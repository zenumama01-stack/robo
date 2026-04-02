import { AuthenticationProvider } from '@microsoft/microsoft-graph-client';
// Define types for Microsoft OAuth token response
interface MicrosoftTokenResponse {
  ext_expires_in?: number;
 * Callback type for persisting refreshed tokens to the database.
 * This is called when a new refresh token is obtained (some providers issue new refresh tokens on each refresh).
 * Configuration interface for SharePoint storage provider with OAuth2 refresh token flow.
 * Used when initializing the provider with user-specific OAuth credentials.
interface SharePointOAuthConfig extends StorageProviderConfig {
  /** OAuth2 Client ID (from Azure AD app registration) */
  /** OAuth2 Client Secret (from Azure AD app registration) */
  /** OAuth2 Refresh Token (obtained from OAuth flow, used to get new access tokens) */
  /** Azure AD Tenant ID (use 'common' for multi-tenant or 'consumers' for personal accounts) */
  tenantID?: string;
  /** SharePoint Site ID (optional - can be determined from user's OneDrive if not specified) */
  siteID?: string;
  /** Drive ID (document library ID - optional, defaults to user's OneDrive) */
  driveID?: string;
  /** Callback to persist new tokens when they are refreshed */
 * Implementation of the Microsoft Graph API AuthenticationProvider interface
 * that uses the OAuth2 client credentials flow for authentication.
 * This provider handles token acquisition, caching, and automatic token refresh
 * when tokens expire, providing seamless authentication for SharePoint operations.
 * This class is designed for server-to-server authentication scenarios where
 * user interaction isn't possible. It requires an Azure AD application with
 * appropriate permissions to access SharePoint/OneDrive resources.
class ClientCredentialsAuthProvider implements AuthenticationProvider {
   * Azure AD application (client) ID
  private clientId: string;
   * Azure AD application client secret
  private clientSecret: string;
   * Azure AD tenant ID
  private tenantId: string;
   * OAuth2 token endpoint URL
  private tokenEndpoint: string;
   * Cached access token
   * Expiration timestamp for the cached token
  private tokenExpiration: Date | null = null;
   * Creates a new ClientCredentialsAuthProvider instance
   * @param clientId - The Azure AD application (client) ID
   * @param clientSecret - The Azure AD application client secret
   * @param tenantId - The Azure AD tenant ID
  constructor(clientId: string, clientSecret: string, tenantId: string) {
    this.clientId = clientId;
    this.clientSecret = clientSecret;
    this.tenantId = tenantId;
    this.tokenEndpoint = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/token`;
   * Gets an access token for Microsoft Graph API
   * This method implements the AuthenticationProvider interface required by the
   * Microsoft Graph client. It acquires a new token or returns a cached token
   * if it's still valid.
   * @returns A Promise that resolves to the access token string
    if (this.accessToken && this.tokenExpiration && this.tokenExpiration > new Date()) {
    const data = new URLSearchParams({
      client_id: this.clientId,
      scope: 'https://graph.microsoft.com/.default',
      client_secret: this.clientSecret,
    const response = await fetch(this.tokenEndpoint, {
      throw new Error(`Failed to get access token: ${response.statusText}`);
    const json = (await response.json()) as MicrosoftTokenResponse;
    this.accessToken = json.access_token;
    // Set token expiration time (subtract 5 minutes as a buffer)
    const expiresIn = json.expires_in || 3600;
    this.tokenExpiration = new Date(Date.now() + (expiresIn - 300) * 1000);
 * that uses the OAuth2 refresh token flow for per-user authentication.
 * This provider handles token acquisition using a refresh token, enabling
 * users to access their own OneDrive/SharePoint files rather than a shared
 * service account.
 * This class is designed for scenarios where each user authenticates with
 * their own Microsoft account via OAuth. The refresh token is obtained
 * through the OAuth authorization code flow and stored per-user.
class RefreshTokenAuthProvider implements AuthenticationProvider {
   * OAuth2 refresh token for obtaining new access tokens
  private refreshToken: string;
   * Callback to persist new tokens when they are refreshed
  private onTokenRefresh?: TokenRefreshCallback;
   * Creates a new RefreshTokenAuthProvider instance
   * @param refreshToken - The OAuth2 refresh token
   * @param tenantId - The Azure AD tenant ID (use 'common' for multi-tenant)
   * @param onTokenRefresh - Optional callback to persist new tokens
  constructor(clientId: string, clientSecret: string, refreshToken: string, tenantId: string = 'common', onTokenRefresh?: TokenRefreshCallback) {
    this.onTokenRefresh = onTokenRefresh;
   * Gets an access token for Microsoft Graph API using the refresh token.
   * Microsoft Graph client. It uses the refresh token to obtain a new access token
   * when needed, and caches the token until it expires.
    console.log('[SharePoint RefreshTokenAuth] Refreshing access token...');
      scope: 'https://graph.microsoft.com/Files.ReadWrite.All offline_access',
      console.error('[SharePoint RefreshTokenAuth] Token refresh failed:', errorText);
      throw new Error(`Failed to refresh access token: ${response.statusText}`);
    console.log('[SharePoint RefreshTokenAuth] Token refreshed successfully, expires:', this.tokenExpiration);
    // Microsoft may return a new refresh token - if so, persist it
    if (json.refresh_token && json.refresh_token !== this.refreshToken) {
      console.log('[SharePoint RefreshTokenAuth] New refresh token received, persisting...');
      this.refreshToken = json.refresh_token;
      if (this.onTokenRefresh) {
          await this.onTokenRefresh(json.refresh_token, json.access_token);
          console.log('[SharePoint RefreshTokenAuth] New tokens persisted successfully');
          console.error('[SharePoint RefreshTokenAuth] Failed to persist new tokens:', callbackError);
 * FileStorageBase implementation for Microsoft SharePoint using the Microsoft Graph API
 * This provider allows working with files stored in SharePoint document libraries.
 * It uses the Microsoft Graph API and client credentials authentication flow to
 * securely access and manipulate SharePoint files and folders.
 * This implementation requires the following environment variables:
 * - STORAGE_SHAREPOINT_CLIENT_ID - Azure AD application (client) ID
 * - STORAGE_SHAREPOINT_CLIENT_SECRET - Azure AD application client secret
 * - STORAGE_SHAREPOINT_TENANT_ID - Azure AD tenant ID
 * - STORAGE_SHAREPOINT_SITE_ID - The SharePoint site ID
 * - STORAGE_SHAREPOINT_DRIVE_ID - The ID of the document library (drive)
 * - STORAGE_SHAREPOINT_ROOT_FOLDER_ID (optional) - ID of a subfolder to use as the root
 * To use this provider, you need to:
 * 1. Register an Azure AD application with appropriate Microsoft Graph API permissions
 *    (typically Files.ReadWrite.All and Sites.ReadWrite.All)
 * 2. Create a client secret for the application
 * 3. Grant admin consent for the permissions
 * 4. Find your SharePoint site ID and document library (drive) ID using the Microsoft Graph Explorer
 * // Set required environment variables before creating the provider
 * process.env.STORAGE_SHAREPOINT_CLIENT_ID = 'your-client-id';
 * process.env.STORAGE_SHAREPOINT_CLIENT_SECRET = 'your-client-secret';
 * process.env.STORAGE_SHAREPOINT_TENANT_ID = 'your-tenant-id';
 * process.env.STORAGE_SHAREPOINT_SITE_ID = 'your-site-id';
 * process.env.STORAGE_SHAREPOINT_DRIVE_ID = 'your-drive-id';
 * const storage = new SharePointFileStorage();
 * const fileContent = Buffer.from('Hello, SharePoint!');
@RegisterClass(FileStorageBase, 'SharePoint Storage')
export class SharePointFileStorage extends FileStorageBase {
  protected readonly providerName = 'SharePoint';
   * Microsoft Graph API client
  private _client!: Client;
   * The ID of the SharePoint document library (drive)
  private _driveId!: string;
   * The ID of the SharePoint site
  private _siteId!: string;
   * Optional ID of a subfolder to use as the root folder (if specified)
   * OAuth2 Client ID (for per-user OAuth flow)
   * OAuth2 Client Secret (for per-user OAuth flow)
   * OAuth2 Refresh Token (for per-user OAuth flow)
   * Azure AD Tenant ID
  private _tenantID?: string;
   * Callback for persisting refreshed tokens
   * Creates a new SharePointFileStorage instance
   * This constructor reads configuration from environment variables if available.
   * If no environment variables are set, the provider can be initialized later
   * via the initialize() method with OAuth credentials from the database.
    const config = getProviderConfig('sharePoint');
    // Extract values from config, fall back to env vars (don't require them - initialize() may be called later)
    const clientId = config?.clientID || env.get('STORAGE_SHAREPOINT_CLIENT_ID').asString();
    const clientSecret = config?.clientSecret || env.get('STORAGE_SHAREPOINT_CLIENT_SECRET').asString();
    const tenantId = config?.tenantID || env.get('STORAGE_SHAREPOINT_TENANT_ID').asString();
    const siteId = config?.siteID || env.get('STORAGE_SHAREPOINT_SITE_ID').asString();
    const driveId = config?.driveID || env.get('STORAGE_SHAREPOINT_DRIVE_ID').asString();
    // Store OAuth credentials for IsConfigured check
    this._clientID = clientId;
    this._clientSecret = clientSecret;
    this._tenantID = tenantId;
    // Only initialize if we have all required credentials (env/config-based setup)
    if (clientId && clientSecret && tenantId && siteId && driveId) {
      this._siteId = siteId;
      this._driveId = driveId;
      // Optionally set a root folder within the SharePoint drive
      this._rootFolderId = config?.rootFolderID || env.get('STORAGE_SHAREPOINT_ROOT_FOLDER_ID').asString();
      // Initialize Graph client with client credentials auth provider (service account)
      const authProvider = new ClientCredentialsAuthProvider(clientId, clientSecret, tenantId);
      this._client = Client.initWithMiddleware({
        authProvider: authProvider,
    // If credentials not available, initialize() must be called with OAuth config
   * Checks if SharePoint provider is properly configured.
   * Returns true if the Graph client is initialized and has required IDs.
    const hasDriveId = !!this._driveId;
    const isConfigured = hasClient && hasDriveId;
      if (!hasClient) missing.push('Graph Client (credentials)');
      if (!hasDriveId) missing.push('Drive ID');
        `❌ SharePoint provider not configured. Missing: ${missing.join(', ')}\n\n` +
        `  export STORAGE_SHAREPOINT_TENANT_ID="..."\n` +
        `  export STORAGE_SHAREPOINT_CLIENT_ID="..."\n` +
        `  export STORAGE_SHAREPOINT_CLIENT_SECRET="..."\n` +
        `  export STORAGE_SHAREPOINT_SITE_URL="https://tenant.sharepoint.com/sites/sitename"\n` +
        `  const storage = new SharePointFileStorage();\n` +
        `    tenantID: "...",\n` +
        `    siteUrl: "https://tenant.sharepoint.com/sites/sitename"\n` +
   * Initialize SharePoint storage provider with optional configuration.
   * ## Standard Usage Pattern
   * **ALWAYS call this method** after creating a provider instance.
   * ### Simple Deployment (Environment Variables)
   * Constructor loads credentials from environment variables, then call
   * initialize() with no config to complete setup:
   * const storage = new SharePointFileStorage(); // Constructor loads env vars
   * await storage.ListObjects('/'); // Ready to use
   * ### Multi-Tenant Enterprise (Database)
   * Use infrastructure utility which handles credential decryption automatically:
   * const storage = await initializeDriverWithAccountCredentials({
   *   accountEntity: accountWithProvider.account,
   *   providerEntity: accountWithProvider.provider,
   * await storage.ListObjects('/'); // Credentials already decrypted and initialized
   * @param config - Configuration object containing OAuth2 credentials from database
  public async initialize(config?: SharePointOAuthConfig): Promise<void> {
    console.log('[SharePoint] Initializing with OAuth config...');
    this._tenantID = config.tenantID || this._tenantID || 'common';
    this._onTokenRefresh = config.onTokenRefresh;
    // Update site/drive IDs if provided
    if (config.siteID) {
      this._siteId = config.siteID;
    if (config.driveID) {
      this._driveId = config.driveID;
    // Validate we have required OAuth credentials
    if (!this._clientID || !this._clientSecret || !this._refreshToken) {
      throw new Error('SharePoint OAuth requires clientID, clientSecret, and refreshToken');
    // Initialize the Graph client with refresh token auth provider (per-user OAuth)
    const authProvider = new RefreshTokenAuthProvider(this._clientID, this._clientSecret, this._refreshToken, this._tenantID, this._onTokenRefresh);
    // If no driveID provided, get the user's default OneDrive
    if (!this._driveId) {
      console.log("[SharePoint] No driveID provided, getting user's OneDrive...");
        const driveResponse = await this._client.api('/me/drive').get();
        this._driveId = driveResponse.id;
        console.log("[SharePoint] Using user's OneDrive:", this._driveId);
        console.error("[SharePoint] Failed to get user's OneDrive:", error);
        throw new Error("Failed to get user's OneDrive. Ensure the refresh token has Files.ReadWrite.All scope.");
    console.log('[SharePoint] Initialized successfully with driveId:', this._driveId);
   * Gets the SharePoint item ID for a folder at the specified path
   * This helper method navigates the folder hierarchy in SharePoint to find
   * the folder specified by the path, returning its item ID.
   * @param path - The path to get the parent folder for (e.g., 'documents/reports')
   * @returns A Promise that resolves to the parent folder ID
   * @throws Error if any folder in the path doesn't exist
  private async _getParentFolderIdByPath(path: string): Promise<string> {
    let currentFolderId = this._rootFolderId || 'root';
      const folderName = pathParts[i];
      const result = await this._client
        .api(`/drives/${this._driveId}/items/${currentFolderId}/children`)
        .filter(`name eq '${folderName}' and folder ne null`)
      if (!result.value || result.value.length === 0) {
        throw new Error(`Folder not found: ${folderName}`);
      currentFolderId = result.value[0].id;
   * Gets a SharePoint item by its path
   * This helper method retrieves a SharePoint item (file or folder) using
   * its path. It handles path normalization and root folder redirection.
   * @param path - The path of the item to retrieve (e.g., 'documents/reports/report.docx')
   * @returns A Promise that resolves to the SharePoint item
  private async _getItemByPath(path: string): Promise<any> {
      const itemId = this._rootFolderId || 'root';
      return this._client.api(`/drives/${this._driveId}/items/${itemId}`).get();
    // Normalize path
    const normalizedPath = path.startsWith('/') ? path.substring(1) : path;
      // Try to get the item directly by path
      const driveRoot = this._rootFolderId ? `/drives/${this._driveId}/items/${this._rootFolderId}` : `/drives/${this._driveId}/root`;
      return await this._client.api(`${driveRoot}:/${normalizedPath}`).get();
      console.error('Error getting item by path', { path, error });
   * Converts a SharePoint item to a StorageObjectMetadata object
   * This helper method transforms the Microsoft Graph API item representation
   * into the standard StorageObjectMetadata format used by the FileStorageBase interface.
   * @param item - The SharePoint item from the Microsoft Graph API
   * @returns A StorageObjectMetadata object representing the item
  private _itemToMetadata(item: any): StorageObjectMetadata {
    const isDirectory = !!item.folder;
    const parentPath = item.parentReference?.path?.split(':').pop() || '';
    // Remove any root folder prefix from the parent path if present
    let path = parentPath;
    if (this._rootFolderId && path.startsWith(`/drives/${this._driveId}/items/${this._rootFolderId}`)) {
      path = path.replace(`/drives/${this._driveId}/items/${this._rootFolderId}`, '');
    // Ensure path starts with / and remove leading slash for storage format
    path = path.startsWith('/') ? path.substring(1) : path;
    // For full paths, combine parent path with name
    const fullPath = path ? `${path}/${item.name}` : item.name;
      size: item.size || 0,
      contentType: item.file?.mimeType || mime.lookup(item.name) || 'application/octet-stream',
      lastModified: new Date(item.lastModifiedDateTime),
      etag: item.eTag,
      customMetadata: {},
   * Creates a pre-authenticated upload URL (not supported in SharePoint)
   * This method is not supported for SharePoint storage as SharePoint doesn't provide
   *     console.log('Pre-authenticated upload URLs are not supported in SharePoint.');
   *     await storage.PutObject('documents/report.docx', fileContent, 'application/vnd.openxmlformats-officedocument.wordprocessingml.document');
    // SharePoint doesn't provide a way to get pre-authenticated upload URLs like S3
    // Instead, we'll use the PutObject method for actual uploads
   * Creates a pre-authenticated download URL for an object
   * This method generates a time-limited, publicly accessible URL that can be used
   * to download a file without authentication. The URL expires after 10 minutes.
   * @param objectName - Path to the object to create a download URL for (e.g., 'documents/report.pdf')
   * @returns A Promise that resolves to the pre-authenticated download URL
   * @throws Error if the object doesn't exist or the URL creation fails
   * // Generate a pre-authenticated download URL that will work for 10 minutes
   * const downloadUrl = await storage.CreatePreAuthDownloadUrl('presentations/quarterly-update.pptx');
   * console.log(`Download the file using this URL: ${downloadUrl}`);
   * // You can share this URL with users who don't have SharePoint access
   * // The URL will expire after 10 minutes
      const item = await this._getItemByPath(objectName);
      // Request a download URL - this is a time-limited URL
      const downloadUrl = await this._client.api(`/drives/${this._driveId}/items/${item.id}/createLink`).post({
        type: 'view',
        scope: 'anonymous',
        expirationDateTime: new Date(Date.now() + 10 * 60 * 1000).toISOString(), // 10 minutes
      return downloadUrl.link.webUrl;
   * Moves an object from one location to another
   * This method moves a file or folder from one location in SharePoint to another.
   * // Move a file to a different folder
      // Get the old item
      const item = await this._getItemByPath(oldObjectName);
      // Parse the new path to get the new parent folder and name
      // Get the new parent folder ID
      const parentFolderId = await this._getParentFolderIdByPath(newParentPath);
      // Move the item
      await this._client.api(`/drives/${this._driveId}/items/${item.id}`).update({
        parentReference: {
          id: parentFolderId,
   * Deletes an object (file) from SharePoint
   * This method permanently deletes a file from SharePoint storage.
   * Note that deleted files may be recoverable from the SharePoint recycle bin
   * depending on your SharePoint configuration.
   * - Handles 404 errors by returning true since the end result is the same
      // Delete the item
      await this._client.api(`/drives/${this._driveId}/items/${item.id}`).delete();
      // Check if it's a "not found" error, which we'll consider a success for idempotency
      if (error.statusCode === 404) {
   * Lists objects in a given directory (folder)
   * @param delimiter - Optional delimiter character (not used in this implementation)
   * - The `objects` array in the result includes both files and folders
   *   console.log(`Name: ${obj.name}, Size: ${obj.size}, Type: ${obj.isDirectory ? 'Folder' : 'File'}`);
   * // Process subfolders
   *   console.log(`Subfolder: ${prefix}`);
      // Get the folder ID
      // List children
      const children = await this._client.api(`/drives/${this._driveId}/items/${folder.id}/children`).get();
      for (const item of children.value) {
        if (item.folder) {
          // This is a folder/directory
          const folderPath = prefix ? (prefix.endsWith('/') ? `${prefix}${item.name}` : `${prefix}/${item.name}`) : item.name;
        // Add all items as objects (including folders)
        objects.push(this._itemToMetadata(item));
      console.error('Error listing objects', { prefix, error });
   * Creates a directory (folder) in SharePoint
   * This method creates a new folder at the specified path. The parent directory
   * must already exist.
   * @param directoryPath - Path where the directory should be created (e.g., 'documents/new-folder')
   * - If a folder with the same name already exists, the operation will fail
   * - The parent directory must exist for the operation to succeed
   * const createResult = await storage.CreateDirectory('documents/2024-reports');
   *   console.log('Folder created successfully');
   *   // Now we can put files in this folder
   *     'documents/2024-reports/q1-results.xlsx',
   *   console.error('Failed to create folder');
      // Parse the path to get the parent folder and new folder name
      const parentFolderId = await this._getParentFolderIdByPath(parentPath);
      await this._client.api(`/drives/${this._driveId}/items/${parentFolderId}/children`).post({
        folder: {},
        '@microsoft.graph.conflictBehavior': 'fail',
   * Deletes a directory (folder) and optionally its contents
   * This method deletes a folder from SharePoint. By default, it will only delete
   * empty folders unless the recursive parameter is set to true.
   * @param directoryPath - Path to the directory to delete (e.g., 'archive/old-reports')
   * - SharePoint deleted items may be recoverable from the recycle bin depending on site settings
        if (children.value && children.value.length > 0) {
      // Delete the folder (SharePoint will delete recursively by default)
      await this._client.api(`/drives/${this._driveId}/items/${folder.id}`).delete();
   *   // Fast path: Use objectId (SharePoint item ID)
   *   const metadata = await storage.GetObjectMetadata({ objectId: '01BYE5RZ6QN3VYRVNHHFDK2QJODWDDFR4E' });
      let item: any;
        item = await this._client.api(`/drives/${this._driveId}/items/${params.objectId}`).get();
        // Slow path: Resolve path to item
        item = await this._getItemByPath(params.fullPath!);
      return this._itemToMetadata(item);
   * - This method uses the Graph API's download URL to retrieve the file contents
   * - The method will throw an error if the object is a folder
   *   const fileContent = await storage.GetObject({ objectId: '01BYE5RZ6QN3VYRVNHHFDK2QJODWDDFR4E' });
   *   // For binary files, you can write the buffer to a local file
      // Get the content
      const response = await fetch(item['@microsoft.graph.downloadUrl']);
        throw new Error(`Failed to download item: ${response.statusText}`);
      // Convert response to buffer
      return Buffer.from(arrayBuffer);
   * Uploads a file to SharePoint
   * This method uploads a file to SharePoint at the specified path. It automatically
   * determines whether to use a simple upload or a chunked upload based on file size.
   * @param metadata - Optional metadata to associate with the file (not used in SharePoint implementation)
   * - Files smaller than 4MB use a simple upload
   * - Files 4MB or larger use a chunked upload session for better reliability
   * - Automatically creates the parent folder structure if it doesn't exist
   * // Create a text file
      // Parse the path to get the parent folder and filename
      if (data.length < 4 * 1024 * 1024) {
        // For small files (< 4MB), use simple upload
        await this._client.api(`/drives/${this._driveId}/items/${parentFolderId}:/${fileName}:/content`).put(data);
        // For larger files, use upload session
        // Create upload session
        const uploadSession = await this._client.api(`/drives/${this._driveId}/items/${parentFolderId}:/${fileName}:/createUploadSession`).post({
          item: {
            '@microsoft.graph.conflictBehavior': 'replace',
        // Upload the file in chunks (could be improved with parallel uploads)
        const maxChunkSize = 60 * 1024 * 1024; // 60 MB chunks
        for (let i = 0; i < data.length; i += maxChunkSize) {
          const chunk = data.slice(i, Math.min(i + maxChunkSize, data.length));
          const contentRange = `bytes ${i}-${i + chunk.length - 1}/${data.length}`;
          await fetch(uploadSession.uploadUrl, {
              'Content-Length': chunk.length.toString(),
              'Content-Range': contentRange,
            body: chunk as BodyInit,
   * - The parent folder of the destination must exist
   * - Both files and folders can be copied
   * - The operation is asynchronous in SharePoint and may not complete immediately
   * // Copy a file to a new location with a different name
   *   'reports/2024/q1-financial-report.xlsx'
      // Get source item
      const sourceItem = await this._getItemByPath(sourceObjectName);
      const destName = destPathParts.pop() || '';
      // Get destination parent folder ID
      const destParentId = await this._getParentFolderIdByPath(destParentPath);
      // Create a copy
      await this._client.api(`/drives/${this._driveId}/items/${sourceItem.id}/copy`).post({
          id: destParentId,
        name: destName,
      console.error('Error checking if object exists', { objectName, error });
      return !!item.folder;
      console.error('Error checking if directory exists', { directoryPath, error });
   * Search files in SharePoint using Microsoft Graph Search API.
   * This method provides powerful search capabilities using KQL (Keyword Query Language),
   * SharePoint's native query language. The search can target file names, metadata, and
   * optionally file contents.
   * @param query - The search query string. Can be plain text or use KQL syntax for advanced queries.
   * @param options - Optional search configuration including filters, limits, and content search
   * @returns A Promise resolving to FileSearchResultSet with matched files and pagination info
   * **KQL Query Syntax Examples:**
   * - Simple text: `"quarterly report"` - searches for files containing these terms
   * - Boolean operators: `"budget AND 2024"`, `"draft OR final"`, `"report NOT internal"`
   * - Wildcards: `"proj*"` matches "project", "projection", etc.
   * - Property filters: `"FileType:pdf"`, `"Author:John Smith"`, `"Size>1000000"`
   * - Date filters: `"Created>=2024-01-01"`, `"LastModifiedTime<2024-12-31"`
   * - Proximity: `"project NEAR report"` - finds terms near each other
   * - Exact phrases: `"\"annual budget report\""` - exact phrase match
   * **Additional Filtering:**
   * The method automatically adds KQL filters based on the provided options:
   * - `fileTypes`: Adds FileType filters (e.g., `FileType:pdf OR FileType:docx`)
   * - `modifiedAfter`/`modifiedBefore`: Adds LastModifiedTime filters
   * - `pathPrefix`: Adds Path filter to restrict search to a directory
   * - `searchContent`: When false, restricts search to filename only
   * // Simple text search in filenames
   *   maxResults: 20
   * // Search for PDFs only
   * // Search with date range
   * const recentResults = await storage.SearchFiles('meeting notes', {
   *   modifiedAfter: new Date('2024-01-01'),
   *   modifiedBefore: new Date('2024-12-31'),
   *   searchContent: true
   * // Search within specific directory
   * const folderResults = await storage.SearchFiles('presentation', {
   *   fileTypes: ['pptx', 'pdf']
   * // Advanced KQL query
   * const advancedResults = await storage.SearchFiles(
   *   'FileType:xlsx AND Created>=2024-01-01 AND Author:"John Smith"',
   *   { maxResults: 100 }
      // Build KQL query from options
      const kqlQuery = this.buildKQLQuery(query, options);
      // Prepare the search request payload
      const searchRequest = {
        requests: [
            entityTypes: ['driveItem'],
            query: {
              queryString: kqlQuery,
            from: 0,
            size: maxResults,
            fields: ['name', 'path', 'size', 'lastModifiedDateTime', 'fileSystemInfo', 'webUrl', 'id', 'contentType'],
      // Execute the search
      const response = await this._client.api('/search/query').post(searchRequest);
      // Transform results
      return this.transformSearchResults(response, maxResults);
      console.error('Error searching files in SharePoint', { query, options, error });
      throw new Error(`SharePoint search failed: ${error instanceof Error ? error.message : String(error)}`);
   * Builds a KQL (Keyword Query Language) query string from the base query and search options.
   * This helper method constructs a properly formatted KQL query by combining the user's
   * search query with filters derived from FileSearchOptions. It handles file type filters,
   * date range filters, path restrictions, and content search options.
   * @param baseQuery - The user's search query (plain text or KQL)
   * @param options - Optional search options to convert into KQL filters
   * @returns A complete KQL query string
  private buildKQLQuery(baseQuery: string, options?: FileSearchOptions): string {
    // Add base query if provided
    if (baseQuery && baseQuery.trim()) {
      queryParts.push(`(${baseQuery})`);
    // Restrict to this specific drive
    queryParts.push(`(Path:"${this._siteId}/${this._driveId}")`);
    // Add path prefix filter if specified
      const normalizedPrefix = options.pathPrefix.startsWith('/') ? options.pathPrefix.substring(1) : options.pathPrefix;
      queryParts.push(`(Path:"${this._siteId}/${this._driveId}/${normalizedPrefix}*")`);
    // Add file type filters if specified
      const fileTypeFilters = options.fileTypes
        .map((fileType) => {
          // Handle both extensions (pdf) and MIME types (application/pdf)
            // It's a MIME type
            return `ContentType:"${fileType}"`;
            // It's a file extension
            const extension = fileType.startsWith('.') ? fileType.substring(1) : fileType;
            return `FileType:${extension}`;
      queryParts.push(`(${fileTypeFilters})`);
      const dateString = options.modifiedAfter.toISOString().split('T')[0];
      queryParts.push(`(LastModifiedTime>=${dateString})`);
      const dateString = options.modifiedBefore.toISOString().split('T')[0];
      queryParts.push(`(LastModifiedTime<=${dateString})`);
    // Restrict to filename search if content search is disabled
    if (options?.searchContent === false && baseQuery && baseQuery.trim()) {
      // Remove the base query and add it as a filename-only search
      queryParts.shift(); // Remove the base query we added earlier
      queryParts.unshift(`(filename:${baseQuery})`);
    // Combine all parts with AND
    return queryParts.join(' AND ');
   * Transforms Microsoft Graph Search API response into FileSearchResultSet format.
   * This helper method processes the raw search response from the Graph API,
   * extracting relevant file information and converting it to the standard
   * FileSearchResult format. It handles pagination info and calculates relevance scores.
   * @param response - The raw response from Microsoft Graph Search API
   * @param maxResults - The maximum number of results requested
   * @returns A FileSearchResultSet with transformed results
  private transformSearchResults(response: any, maxResults: number): FileSearchResultSet {
    let totalMatches = 0;
    let hasMore = false;
    // Navigate the response structure
    if (response.value && response.value.length > 0) {
      const searchResponse = response.value[0];
      if (searchResponse.hitsContainers && searchResponse.hitsContainers.length > 0) {
        const hitsContainer = searchResponse.hitsContainers[0];
        totalMatches = hitsContainer.total || 0;
        hasMore = hitsContainer.moreResultsAvailable || false;
        if (hitsContainer.hits && hitsContainer.hits.length > 0) {
          for (const hit of hitsContainer.hits) {
            const resource = hit.resource;
            // Skip if not a file (e.g., folders)
            if (!resource || resource.folder) {
            // Extract file information
            const result: FileSearchResult = {
              path: this.extractPathFromResource(resource),
              name: resource.name || '',
              size: resource.size || 0,
              contentType: resource.file?.mimeType || mime.lookup(resource.name) || 'application/octet-stream',
              lastModified: resource.lastModifiedDateTime ? new Date(resource.lastModifiedDateTime) : new Date(),
              objectId: resource.id || '', // SharePoint item ID for direct access
              relevance: hit.rank ? hit.rank / 100.0 : undefined,
              excerpt: hit.summary || undefined,
              matchInFilename: this.determineMatchLocation(hit),
                id: resource.id,
                webUrl: resource.webUrl,
                driveId: this._driveId,
                siteId: this._siteId,
   * Extracts the file path from a SharePoint resource object.
   * This helper method processes the path information from a Graph API resource,
   * removing the drive and site prefixes to return just the file path relative
   * to the configured root folder.
   * @param resource - The resource object from Graph API search results
   * @returns The relative file path
  private extractPathFromResource(resource: any): string {
    // Try to get path from parentReference
    if (resource.parentReference?.path) {
      let path = resource.parentReference.path;
      // Remove the drive/site prefix
      const pathParts = path.split(':');
      if (pathParts.length > 1) {
        path = pathParts[1];
      // Remove root folder prefix if configured
      if (this._rootFolderId && path.startsWith(this._rootFolderId)) {
        path = path.substring(this._rootFolderId.length);
      // Combine with filename
      return path ? `${path}/${resource.name}` : resource.name;
    // Fallback to just the filename
    return resource.name || '';
   * Determines whether the search match was in the filename or content.
   * This helper method analyzes the hit metadata to determine if the search
   * term was found in the filename versus the file content.
   * @param hit - The search hit object from Graph API
   * @returns True if match was in filename, false if in content, undefined if unknown
  private determineMatchLocation(hit: any): boolean | undefined {
    // Check if summary exists (indicates content match)
    if (hit.summary && hit.summary.length > 0) {
      return false; // Match in content
    // If no summary but we have a hit, likely a filename match
    if (hit.resource?.name) {
      return true; // Match in filename
