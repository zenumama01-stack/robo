import { BoxDeveloperTokenAuth, BoxClient } from 'box-node-sdk';
import { Readable } from 'stream';
interface BoxTokenResponse {
  expires_in: number;
 * Box issues a new refresh token with every token refresh, invalidating the old one.
type TokenRefreshCallback = (newRefreshToken: string, newAccessToken?: string) => Promise<void>;
 * Configuration interface for Box storage provider.
 * Supports OAuth2 authentication with refresh token.
interface BoxConfig extends StorageProviderConfig {
  /** OAuth2 Client ID */
  clientID?: string;
  /** OAuth2 Client Secret */
  /** OAuth2 Access Token (short-lived) */
  /** Box Enterprise ID for JWT auth */
  enterpriseID?: string;
   * CRITICAL: Box issues new refresh tokens with each refresh, invalidating the old one.
   * This callback must be used to persist the new token.
 * FileStorageBase implementation for Box.com cloud storage
 * This provider allows working with files stored in Box.com. It supports
 * authentication via access token, refresh token, or client credentials (JWT).
 * This implementation requires at least one of the following authentication methods:
 * 1. Access Token:
 *    - STORAGE_BOX_ACCESS_TOKEN - A valid Box API access token
 * 2. Refresh Token:
 *    - STORAGE_BOX_REFRESH_TOKEN - A valid Box API refresh token
 *    - STORAGE_BOX_CLIENT_ID - Your Box application client ID
 *    - STORAGE_BOX_CLIENT_SECRET - Your Box application client secret
 * 3. Client Credentials (JWT):
 *    - STORAGE_BOX_ENTERPRISE_ID - Your Box enterprise ID
 * Optional configuration:
 * - STORAGE_BOX_ROOT_FOLDER_ID - ID of a Box folder to use as the root (defaults to '0' which is the root)
 * // Set required environment variables for JWT auth
 * process.env.STORAGE_BOX_CLIENT_ID = 'your-client-id';
 * process.env.STORAGE_BOX_CLIENT_SECRET = 'your-client-secret';
 * process.env.STORAGE_BOX_ENTERPRISE_ID = 'your-enterprise-id';
 * // Create the provider
 * const storage = new BoxFileStorage();
 * await storage.initialize(); // Required for JWT auth
 * // Upload a file
 * const fileContent = Buffer.from('Hello, Box!');
 * await storage.PutObject('documents/hello.txt', fileContent, 'text/plain');
 * // Download a file
 * const downloadedContent = await storage.GetObject('documents/hello.txt');
 * // Get a temporary download URL
 * const downloadUrl = await storage.CreatePreAuthDownloadUrl('documents/hello.txt');
@RegisterClass(FileStorageBase, 'Box.com Storage')
export class BoxFileStorage extends FileStorageBase {
   * The name of this storage provider
  protected readonly providerName = 'Box';
   * Box API access token
  private _accessToken: string;
   * Box API refresh token
  private _refreshToken: string;
   * Box application client ID
  private _clientId: string;
   * Box application client secret
  private _clientSecret: string;
   * Timestamp when current access token expires
  private _tokenExpiresAt: number = 0;
   * Base URL for Box API
  private _baseApiUrl: string = 'https://api.box.com/2.0';
   * Base URL for Box Upload API
  private _uploadApiUrl: string = 'https://upload.box.com/api/2.0';
   * ID of the Box folder to use as root
  private _rootFolderId: string;
   * Box enterprise ID for JWT auth
  private _enterpriseId: string;
   * Box SDK client for making API calls
  private _client: BoxClient;
   * Callback to persist new refresh tokens when they are issued.
   * Box issues new refresh tokens with each token refresh.
  private _onTokenRefresh?: TokenRefreshCallback;
   * Creates a new BoxFileStorage instance
   * This constructor reads the required Box authentication configuration
   * from environment variables.
   * @throws Error if refresh token is provided without client ID and secret
    const config = getProviderConfig('box');
    // Box auth can be via access token or refresh token
    this._accessToken = config?.accessToken || env.get('STORAGE_BOX_ACCESS_TOKEN').asString();
    this._refreshToken = config?.refreshToken || env.get('STORAGE_BOX_REFRESH_TOKEN').asString();
    this._clientId = config?.clientID || env.get('STORAGE_BOX_CLIENT_ID').asString();
    this._clientSecret = config?.clientSecret || env.get('STORAGE_BOX_CLIENT_SECRET').asString();
    this._enterpriseId = config?.enterpriseID || env.get('STORAGE_BOX_ENTERPRISE_ID').asString();
    if (this._refreshToken && (!this._clientId || !this._clientSecret)) {
      throw new Error('Box storage with refresh token requires STORAGE_BOX_CLIENT_ID and STORAGE_BOX_CLIENT_SECRET');
    // Root folder ID, optional (defaults to '0' which is root)
    this._rootFolderId = config?.rootFolderID || env.get('STORAGE_BOX_ROOT_FOLDER_ID').default('0').asString();
   * Checks if Box provider is properly configured.
   * Returns true if client credentials or access token are present.
    const hasClientCredentials = !!(this._clientId && this._clientSecret);
    const hasAccessToken = !!this._accessToken;
    const isConfigured = hasClientCredentials || hasAccessToken;
        `❌ Box provider not configured. Missing: Client ID & Client Secret (or Access Token)\n\n` +
        `Option 1: Environment Variables (JWT Authentication)\n` +
        `  export STORAGE_BOX_CLIENT_ID="..."\n` +
        `  export STORAGE_BOX_CLIENT_SECRET="..."\n` +
        `  export STORAGE_BOX_ENTERPRISE_ID="..."\n` +
        `  export STORAGE_BOX_JWT_KEY_ID="..."\n` +
        `  export STORAGE_BOX_PRIVATE_KEY="..." # base64 encoded\n` +
        `  const storage = new BoxFileStorage();\n` +
        `    clientID: "...",\n` +
        `    clientSecret: "...",\n` +
        `    enterpriseID: "..."\n` +
   * Initialize Box storage provider.
   * const storage = new BoxFileStorage(); // Constructor loads env vars
   *   accountName: 'Box Account',
   *   clientID: '...',
   *   clientSecret: '...',
   *   refreshToken: '...',
   *   rootFolderID: '0'
  public async initialize(config?: BoxConfig): Promise<void> {
    // If config is provided (from database OAuth), use those credentials
      if (config.clientID) this._clientId = config.clientID;
      if (config.clientSecret) this._clientSecret = config.clientSecret;
      if (config.refreshToken) this._refreshToken = config.refreshToken;
      if (config.accessToken) this._accessToken = config.accessToken;
      if (config.enterpriseID) this._enterpriseId = config.enterpriseID;
      if (config.rootFolderID) this._rootFolderId = config.rootFolderID;
      // Store the token refresh callback - CRITICAL for Box since it issues new refresh tokens
      if (config.onTokenRefresh) this._onTokenRefresh = config.onTokenRefresh;
    // Always get a fresh access token using refresh token if available.
    // Box access tokens are short-lived (~60 min), so stored tokens are likely expired.
    // We should always refresh rather than trusting a stored access token.
    if (this._refreshToken && this._clientId && this._clientSecret) {
      // Use refresh token to get a fresh access token (OAuth flow)
      console.log('[Box] Refreshing access token during initialization...');
      const tokenData = await this._refreshAccessToken();
      this._accessToken = tokenData.access_token;
    } else if (!this._accessToken) {
      // No refresh token and no access token - try other auth methods
      if (this._clientId && this._clientSecret && this._enterpriseId) {
        // Use client credentials to get token (JWT flow)
        this._accessToken = await this._getAccessToken();
        throw new Error('Box storage requires either access token, refresh token, or client credentials');
    // If we only have an access token (no refresh token), use it as-is and hope it's still valid
    // Initialize Box client with developer token auth
    const auth = new BoxDeveloperTokenAuth({ token: this._accessToken });
    this._client = new BoxClient({ auth });
   * This method requests a new access token using the Box client credentials
   * flow (JWT) with the enterprise as the subject.
   * @returns A Promise that resolves with the access token
   * @throws Error if token acquisition fails
  private async _getAccessToken(): Promise<string> {
      const response = await fetch('https://api.box.com/oauth2/token', {
        body: new URLSearchParams({
          client_id: this._clientId,
          client_secret: this._clientSecret,
          box_subject_type: 'enterprise',
          box_subject_id: this._enterpriseId,
        throw new Error(`Failed to get access token: ${response.status} ${response.statusText}`);
      const tokenData = (await response.json()) as BoxTokenResponse;
      this._tokenExpiresAt = Date.now() + tokenData.expires_in * 1000 - 60000;
      return tokenData.access_token;
      console.error('Error getting Box access token', error);
      throw new Error('Failed to authenticate with Box: ' + (error instanceof Error ? error.message : String(error)));
   * @returns A Promise that resolves with the token data
  private async _refreshAccessToken(): Promise<BoxTokenResponse> {
          refresh_token: this._refreshToken,
        throw new Error(`Failed to refresh token: ${response.status} ${response.statusText}`);
      const data = (await response.json()) as BoxTokenResponse;
      this._accessToken = data.access_token;
      this._tokenExpiresAt = Date.now() + data.expires_in * 1000 - 60000;
      // CRITICAL: Box issues a NEW refresh token with every token refresh.
      // The old refresh token is immediately invalidated.
      // We MUST persist the new refresh token to the database.
      if (data.refresh_token) {
        this._refreshToken = data.refresh_token;
        // Call the callback to persist the new tokens to the database
        if (this._onTokenRefresh) {
          console.log('[Box] New refresh token received, persisting to database...');
            await this._onTokenRefresh(data.refresh_token, data.access_token);
            console.log('[Box] New tokens persisted successfully');
            console.error('[Box] Failed to persist new tokens:', callbackError);
            // Don't throw here - we still have the tokens in memory and can continue
            // But the next time the server restarts, authentication will fail
          console.warn('[Box] New refresh token received but no callback registered to persist it!');
          console.warn('[Box] This will cause authentication to fail after server restart.');
      console.error('Error refreshing Box access token', error);
      throw new Error('Failed to refresh token: ' + (error instanceof Error ? error.message : String(error)));
   * Returns the current Box API access token
   * This method ensures a valid token is available before returning it,
   * refreshing or generating a new token if necessary.
   * @returns A Promise that resolves to a valid access token string
   * // Get a valid Box access token
   * const token = await storage.AccessToken();
   * console.log(`Using access token: ${token}`);
  public async AccessToken(): Promise<string> {
    if (this._accessToken && Date.now() < this._tokenExpiresAt) {
      return this._accessToken;
    // Otherwise refresh using SDK's built-in token management
      this._tokenExpiresAt = Date.now() + tokenData.expires_in * 1000;
    } else if (this._clientId && this._clientSecret && this._enterpriseId) {
   * Parses a path string into Box API components
   * This helper method converts a standard path string (e.g., 'documents/reports/file.txt')
   * into components used by the Box API (folder ID, name, parent path).
   * @param path - The path to parse
   * @returns An object containing the parsed components: id, name, and parent
  private _parsePath(path: string): { id: string; name: string; parent: string } {
    // Default to root folder
    if (!path || path === '/' || path === '') {
      return { id: this._rootFolderId, name: '', parent: '' };
    // Remove leading and trailing slashes
    let normalizedPath = path;
    if (normalizedPath.startsWith('/')) {
      normalizedPath = normalizedPath.substring(1);
    if (normalizedPath.endsWith('/')) {
      normalizedPath = normalizedPath.substring(0, normalizedPath.length - 1);
    // Parse the name from the path
    const parts = normalizedPath.split('/');
    const name = parts[parts.length - 1];
    // Parse the parent path
    const parentParts = parts.slice(0, parts.length - 1);
    const parent = parentParts.join('/');
    return { id: '', name, parent };
   * Resolves a path string to a Box item ID
   * This helper method navigates the Box folder hierarchy to find
   * the item at the specified path, returning its Box ID.
   * @param path - The path to resolve
   * @returns A Promise that resolves to the Box item ID
   * @throws Error if the item does not exist
  private async _getIdFromPath(path: string): Promise<string> {
    const itemInfo = await this._getItemInfoFromPath(path);
    return itemInfo?.id || null;
   * Gets both ID and type information for an item at the given path
   * @param path - Path to the item
   * @returns Object with id and type, or null if not found
  private async _getItemInfoFromPath(path: string): Promise<{ id: string; type: string } | null> {
      // Parse the path
      const parsedPath = this._parsePath(path);
      // If the id is already in the path, we need to determine type separately
      if (parsedPath.id) {
        // Try as file first, then folder
          const fileInfo = await this._client.files.getFileById(parsedPath.id, {
            queryParams: { fields: ['id', 'type'] },
          return { id: fileInfo.id, type: fileInfo.type };
            const folderInfo = await this._client.folders.getFolderById(parsedPath.id, {
            return { id: folderInfo.id, type: folderInfo.type };
      // If it's root, return root folder id
      if (!parsedPath.name) {
        return { id: this._rootFolderId, type: 'folder' };
      // First, find the parent folder ID
      let parentFolderId = this._rootFolderId;
      if (parsedPath.parent) {
        parentFolderId = await this._findFolderIdByPath(parsedPath.parent);
      // Search for the item with pagination support
      let hasMoreItems = true;
      const LIMIT = 1000;
      while (hasMoreItems) {
        const folderItems = await this._client.folders.getFolderItems(parentFolderId, {
          queryParams: {
            fields: ['name', 'type', 'id'],
            limit: LIMIT,
            offset: offset,
        // Look for the item by name
        const item = folderItems.entries?.find((i) => i.name === parsedPath.name);
        if (item && item.id && item.type) {
          return { id: item.id, type: item.type };
        // Update pagination variables
        offset += folderItems.entries?.length || 0;
        // Check if we've processed all items
        hasMoreItems = (folderItems.entries?.length || 0) === LIMIT && offset < (folderItems.totalCount || 0);
      // If we get here, the item was not found
      console.log(`Item not found: ${parsedPath.name}`);
      console.error('Error in _getItemInfoFromPath', { path, error });
   * Converts a Box API item to StorageObjectMetadata
   * This helper method transforms a Box API item representation into
   * the standard StorageObjectMetadata format used by FileStorageBase.
   * @param item - The Box API item object
   * @param parentPath - The parent path string
   * @returns A StorageObjectMetadata object
  private _convertToMetadata(item: any, parentPath: string = ''): StorageObjectMetadata {
    const isDirectory = item.type === 'folder';
    const name = item.name;
    // Construct full path
    const path = parentPath || '';
    const fullPath = path ? `${path}/${name}` : name;
      size: isDirectory ? 0 : item.size || 0,
      contentType: isDirectory ? 'application/x-directory' : item.content_type || mime.lookup(name) || 'application/octet-stream',
      lastModified: new Date(item.modified_at || item.created_at || Date.now()),
      isDirectory,
      etag: item.etag,
      customMetadata: {
        id: item.id,
        sequence_id: item.sequence_id,
   * Creates a pre-authenticated upload URL for a file
   * This method creates a Box upload session and returns a URL that can be used
   * to upload file content directly to Box without requiring authentication.
   * @param objectName - Path where the file should be uploaded (e.g., 'documents/report.pdf')
   * @returns A Promise that resolves to an object containing the upload URL and provider key
   * @throws Error if the URL creation fails
   * - The parent folder structure will be created automatically if it doesn't exist
   * - The returned provider key contains the session ID needed to complete the upload
   * - Box upload sessions expire after a certain period (typically 1 hour)
   *   // Generate a pre-authenticated upload URL
   *   const uploadInfo = await storage.CreatePreAuthUploadUrl('presentations/quarterly-results.pptx');
   *   // The URL can be used to upload content directly
   *   console.log(`Upload URL: ${uploadInfo.UploadUrl}`);
   *   // Make sure to save the provider key, as it's needed to reference the upload
   *   console.log(`Provider Key: ${uploadInfo.ProviderKey}`);
   *   // You can use fetch or another HTTP client to upload to this URL
   *   await fetch(uploadInfo.UploadUrl, {
   *     method: 'PUT',
   *     headers: { 'Content-Type': 'application/octet-stream' },
   *     body: fileContent
   *   console.error('Error creating upload URL:', error.message);
      // Get the parent folder ID and file name
      const parsedPath = this._parsePath(objectName);
      let parentId = this._rootFolderId;
          parentId = await this._getIdFromPath(parsedPath.parent);
          // If parent folder doesn't exist, create it recursively
            await this.CreateDirectory(parsedPath.parent);
      // Box SDK v10 doesn't have a simple createUploadSession on files manager
      // We need to use chunkedUploads manager instead
      // For now, return a simplified URL structure
      // TODO: Implement proper chunked upload session support
      throw new Error('Pre-authenticated upload URLs are not currently supported with Box SDK v10. Use PutObject instead.');
      console.error('Error creating pre-auth upload URL', { objectName, error });
      throw new Error(`Failed to create upload URL for: ${objectName}`);
   * Creates a pre-authenticated download URL for a file
   * This method generates a time-limited URL that can be used to download
   * a file without authentication. The URL typically expires after 60 minutes.
   * @param objectName - Path to the file to download (e.g., 'documents/report.pdf')
   * @returns A Promise that resolves to the download URL string
   * @throws Error if the file doesn't exist or URL creation fails
   * - Cannot be used with upload sessions that haven't been completed
   * - Box download URLs typically expire after 60 minutes
   * - Generated URLs can be shared with users who don't have Box access
   *   // Generate a pre-authenticated download URL
   *   const downloadUrl = await storage.CreatePreAuthDownloadUrl('documents/financial-report.pdf');
   *   console.log(`Download the file using this URL: ${downloadUrl}`);
   *   // The URL can be shared or used in a browser to download the file
   *   // without requiring Box authentication
   *   console.error('Error creating download URL:', error.message);
      // Check if this is a session upload that hasn't been completed
      if (objectName.startsWith('session:')) {
        throw new Error('Cannot create download URL for incomplete upload session');
      // Get the file ID
      const fileId = await this._getIdFromPath(objectName);
      // Get download URL using SDK
      const downloadUrl = await this._client.downloads.getDownloadFileUrl(fileId);
        throw new Error(`No download URL available for: ${objectName}`);
      console.error('Error creating pre-auth download URL', { objectName, error });
      throw new Error(`Failed to create download URL for: ${objectName}`);
   * Moves a file or folder from one location to another
   * This method moves a file or folder to a new location in Box storage.
   * It handles both renaming and changing the parent folder.
   * @param oldObjectName - Current path of the object (e.g., 'old-folder/document.docx')
   * @param newObjectName - New path for the object (e.g., 'new-folder/renamed-document.docx')
   * @returns A Promise that resolves to true if successful, false otherwise
   * - Parent folders will be created automatically if they don't exist
   * - Works with both files and folders
   * - For folders, all contents will move with the folder
   * // Move a file to a different folder and rename it
   * const moveResult = await storage.MoveObject(
   *   'documents/old-report.pdf',
   *   'archive/2023/annual-report.pdf'
   * if (moveResult) {
   *   console.log('File moved successfully');
   *   console.error('Failed to move file');
      // Get source info
      const sourceInfo = await this._getItemInfoFromPath(oldObjectName);
      if (!sourceInfo) {
        console.log(`Item not found: ${oldObjectName}`);
      // Get destination info
      const destPath = this._parsePath(newObjectName);
      let destParentId = this._rootFolderId;
      if (destPath.parent) {
          destParentId = await this._getIdFromPath(destPath.parent);
          // Create parent folder if it doesn't exist
          await this.CreateDirectory(destPath.parent);
      // Move the item using SDK
      if (sourceInfo.type === 'folder') {
        await this._client.folders.updateFolderById(sourceInfo.id, {
            parent: { id: destParentId },
            name: destPath.name,
        await this._client.files.updateFileById(sourceInfo.id, {
      console.error('Error moving object', { oldObjectName, newObjectName, error });
   * Deletes a file or folder from Box storage
   * This method permanently deletes a file or folder. It can also
   * handle special cases like incomplete upload sessions.
   * @param objectName - Path to the object to delete (e.g., 'documents/old-report.docx')
   * @returns A Promise that resolves to true if successful, false if an error occurs
   * - Returns true if the object doesn't exist (for idempotency)
   * - Can handle special provider keys like upload sessions
   * - Box puts deleted items in the trash, where they can be recovered for a limited time
   * - To permanently delete folder contents, use DeleteDirectory with recursive=true
   * // Delete a file
   * const deleteResult = await storage.DeleteObject('temp/draft-document.docx');
   * if (deleteResult) {
   *   console.log('File deleted successfully or already didn\'t exist');
   *   console.error('Failed to delete file');
   * // Delete an upload session
   * await storage.DeleteObject('session:1234567890:documents/large-file.zip');
      // Handle session objects specially
        // Session support not implemented in SDK v10 migration
        // Just return true for now
      const itemInfo = await this._getItemInfoFromPath(objectName);
      if (!itemInfo) {
        console.log(`Item not found: ${objectName}`);
        return true; // Already deleted/doesn't exist
      // Delete the item using SDK
      if (itemInfo.type === 'folder') {
        await this._client.folders.deleteFolderById(itemInfo.id);
        await this._client.files.deleteFileById(itemInfo.id);
      // If the error is a 404, consider it already deleted
      if (error.message && error.message.includes('404')) {
      console.error('Error deleting object', { objectName, error });
   * Lists files and folders in a given directory
   * This method retrieves all files and subfolders in the specified directory.
   * It returns both a list of object metadata and a list of directory prefixes.
   * @param prefix - Path to the directory to list (e.g., 'documents/reports')
   * @param delimiter - Optional delimiter character (default: '/')
   * @returns A Promise that resolves to a StorageListResult containing objects and prefixes
   * - The `objects` array includes both files and folders
   * - The `prefixes` array includes only folder paths (with trailing slashes)
   * - Returns empty arrays if the directory doesn't exist
   * - The delimiter parameter is included for interface compatibility but not used internally
   * // List all files and folders in the 'documents' directory
   * const result = await storage.ListObjects('documents');
   * // Process files and folders
   * console.log(`Found ${result.objects.length} items:`);
   * for (const obj of result.objects) {
   *   console.log(`- ${obj.name} (${obj.isDirectory ? 'Folder' : 'File'}, ${obj.size} bytes)`);
   * // List subfolders only
   * console.log(`Found ${result.prefixes.length} subfolders:`);
   * for (const prefix of result.prefixes) {
   *   console.log(`- ${prefix}`);
      // Ensure we have a valid token before making API calls
      await this._ensureValidToken();
      console.log(`[Box ListObjects] Called with prefix: "${prefix}", delimiter: "${delimiter}"`);
      // Normalize the prefix - remove leading/trailing slashes for consistent handling
      let normalizedPrefix = prefix;
      if (normalizedPrefix && normalizedPrefix !== '/') {
        // Remove leading slash
        if (normalizedPrefix.startsWith('/')) {
          normalizedPrefix = normalizedPrefix.substring(1);
        // Remove trailing slash
        if (normalizedPrefix.endsWith('/')) {
          normalizedPrefix = normalizedPrefix.substring(0, normalizedPrefix.length - 1);
        // Empty string or "/" means root
        normalizedPrefix = '';
      console.log(`[Box ListObjects] Normalized prefix: "${normalizedPrefix}"`);
      let folderId: string;
      // Handle root folder case
      if (!normalizedPrefix || normalizedPrefix === '') {
        folderId = this._rootFolderId;
        console.log(`[Box ListObjects] Using root folder ID: ${folderId}`);
          const itemInfo = await this._getItemInfoFromPath(normalizedPrefix);
            console.log(`[Box ListObjects] Folder not found for prefix: "${normalizedPrefix}"`);
          if (itemInfo.type !== 'folder') {
            console.log(`[Box ListObjects] Path "${normalizedPrefix}" is not a folder (type: ${itemInfo.type})`);
          folderId = itemInfo.id;
          // If folder doesn't exist, return empty result
          console.log(`[Box ListObjects] Error resolving path "${normalizedPrefix}": ${errorMsg}`);
      console.log(`[Box ListObjects] Listing folder ID: ${folderId} for prefix: "${normalizedPrefix}"`);
      // Get folder contents using SDK
      const result = await this._client.folders.getFolderItems(folderId, {
          fields: ['id', 'name', 'type', 'size', 'content_type', 'modified_at', 'created_at', 'etag', 'sequence_id'],
      // Process entries - use normalizedPrefix for consistent path handling
      for (const entry of result.entries) {
        objects.push(this._convertToMetadata(entry, normalizedPrefix));
        // If it's a folder, add to prefixes
        if (entry.type === 'folder') {
          const folderPath = normalizedPrefix ? `${normalizedPrefix}/${entry.name}` : entry.name;
          prefixes.push(`${folderPath}/`);
      console.log(`[Box ListObjects] Found ${objects.length} objects and ${prefixes.length} prefixes`);
      console.error('[Box ListObjects] Error:', { prefix, error: error instanceof Error ? error.message : error });
      // Re-throw with more context so the error is visible to the user
      throw new Error(`Failed to list Box folder "${prefix}": ${error instanceof Error ? error.message : 'Unknown error'}`);
   * Ensures we have a valid access token, refreshing if necessary.
   * Also reinitializes the Box client with the new token.
  private async _ensureValidToken(): Promise<void> {
    // Check if token is expired or about to expire (within 5 minutes)
    const tokenExpiresSoon = this._tokenExpiresAt && Date.now() > this._tokenExpiresAt - 300000;
    if (!this._accessToken || tokenExpiresSoon) {
      console.log('[Box] Token expired or expiring soon, refreshing...');
        // Reinitialize the Box client with the new token
        console.log('[Box] Token refreshed successfully');
        throw new Error('Cannot refresh Box token: missing credentials');
   * Creates a new directory (folder) in Box storage
   * This method creates a folder at the specified path, automatically
   * creating any parent folders that don't exist.
   * @param directoryPath - Path where the directory should be created (e.g., 'documents/reports/2023')
   * - Creates parent directories recursively if they don't exist
   * - Returns true if the directory already exists (idempotent operation)
   * - Trailing slashes in the path are automatically removed
   * // Create a nested directory structure
   * const createResult = await storage.CreateDirectory('documents/reports/2023/Q1');
   * if (createResult) {
   *   // Now we can put files in this directory
   *   await storage.PutObject(
   *     'documents/reports/2023/Q1/financial-summary.xlsx',
   *     fileContent,
   *     'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
   *   console.error('Failed to create directory');
      // Root directory always exists
      if (!directoryPath || directoryPath === '/' || directoryPath === '') {
      const normalizedPath = directoryPath.endsWith('/') ? directoryPath.substring(0, directoryPath.length - 1) : directoryPath;
      // First check if directory already exists
        if (await this.DirectoryExists(normalizedPath)) {
        // Ignore error, we'll try to create it anyway
      // Parse the path to get parent folder and name
      const lastSlashIndex = normalizedPath.lastIndexOf('/');
      const parentPath = lastSlashIndex > 0 ? normalizedPath.substring(0, lastSlashIndex) : '';
      const folderName = lastSlashIndex > 0 ? normalizedPath.substring(lastSlashIndex + 1) : normalizedPath;
      // Make sure parent folder exists
      let parentFolderId = '0'; // Default to root
      if (parentPath) {
          // Recursive call to ensure parent directory exists
          const parentDirExists = await this.CreateDirectory(parentPath);
          if (!parentDirExists) {
            console.log(`Failed to create parent directory: ${parentPath}`);
          // Get the parent folder ID
          parentFolderId = await this._findFolderIdByPath(parentPath);
          console.error(`Error ensuring parent folder exists: ${error.message}`);
      // Create the folder using SDK
        await this._client.folders.createFolder({
          parent: { id: parentFolderId },
        console.log(`✅ Folder created successfully: ${normalizedPath}`);
        // Handle conflicts - if the folder already exists, that's a success
        if (error.statusCode === 409 || (error.message && error.message.includes('item_name_in_use'))) {
          console.log(`Folder already exists (conflict): ${normalizedPath}`);
        console.error(`Error creating folder: ${error.message || JSON.stringify(error)}`);
      console.error('Error creating directory', { directoryPath, error });
   * Gets file representation information for a Box file
   * This method retrieves information about available representations
   * (such as thumbnails, previews, or other formats) for a specific file.
   * @param fileId - The Box file ID to get representations for
   * @param repHints - The representation hints string (format and options)
   * @returns A Promise that resolves to a JSON object containing representations data
   * @throws Error if the request fails
   * - Requires a valid file ID (not a path)
   * - The repHints parameter controls what type of representations are returned
   * - Common representation types include thumbnails, preview images, and text extractions
   *   // Get a high-resolution PNG representation of a file
   *   const fileId = '12345';
   *   const representations = await storage.GetFileRepresentations(
   *     fileId,
   *     'png?dimensions=2048x2048'
   *   // Process the representation information
   *   console.log('Available representations:', representations);
   *   console.error('Error getting representations:', error.message);
  public async GetFileRepresentations(fileId: string, repHints: string = 'png?dimensions=2048x2048'): Promise<JSON> {
      // Get file with representations field - SDK handles auth automatically
      const file = await this._client.files.getFileById(fileId, {
          fields: ['representations'],
          xRepHints: repHints,
      // Convert to plain JSON object
      return JSON.parse(JSON.stringify(file));
      console.error('Error getting file representations:', error);
   * Deletes a directory from Box storage
   * This method deletes a folder and optionally its contents. By default,
   * it will only delete empty folders unless recursive is set to true.
   * @param directoryPath - Path to the directory to delete (e.g., 'documents/old-reports')
   * @param recursive - If true, delete the directory and all its contents; if false, only delete if empty
   * - Returns true if the directory doesn't exist (idempotent operation)
   * - If recursive=false and the directory contains files, the operation will fail
   * - Box puts deleted folders in the trash, where they can be recovered for a limited time
   * // Try to delete an empty folder
   * const deleteResult = await storage.DeleteDirectory('temp/empty-folder');
   * // Delete a folder and all its contents
   * const recursiveDeleteResult = await storage.DeleteDirectory('archive/old-data', true);
   * if (recursiveDeleteResult) {
   *   console.log('Folder and all its contents deleted successfully');
   *   console.error('Failed to delete folder');
      // Get folder ID
      let folderId;
        folderId = await this._getIdFromPath(normalizedPath);
        // If folder doesn't exist, consider it success
      // Check if folder is empty if not recursive
        const contents = await this._client.folders.getFolderItems(folderId, {
            limit: 1,
        if (contents.entries.length > 0) {
          throw new Error('Directory is not empty');
      // Delete the folder using SDK
      if (recursive) {
        await this._client.folders.deleteFolderById(folderId, {
            recursive: true,
        await this._client.folders.deleteFolderById(folderId);
      console.error('Error deleting directory', { directoryPath, recursive, error });
   * Gets metadata for a file or folder
   * This method retrieves metadata about a file or folder in Box storage,
   * such as size, type, and modification date.
   * @param objectName - Path to the object to get metadata for (e.g., 'documents/report.pdf')
   * @returns A Promise that resolves to a StorageObjectMetadata object
   *   // Get metadata for a file
   *   const metadata = await storage.GetObjectMetadata('presentations/quarterly-update.pptx');
   *   console.log(`Name: ${metadata.name}`);
   *   console.log(`Path: ${metadata.path}`);
   *   console.log(`Content Type: ${metadata.contentType}`);
   *   console.log(`Last Modified: ${metadata.lastModified}`);
   *   console.log(`Is Directory: ${metadata.isDirectory}`);
   *   // Box-specific metadata is available in customMetadata
   *   console.log(`Box ID: ${metadata.customMetadata.id}`);
   *   console.error('Error getting metadata:', error.message);
      let itemInfo: { id: string; type: string } | null;
      let parentPath = '';
      // Fast path: Use objectId if provided
      if (params.objectId) {
        console.log(`⚡ Fast path: Using Object ID directly: ${params.objectId}`);
          const fileInfo = await this._client.files.getFileById(params.objectId, {
          itemInfo = { id: fileInfo.id, type: fileInfo.type };
          const folderInfo = await this._client.folders.getFolderById(params.objectId, {
          itemInfo = { id: folderInfo.id, type: folderInfo.type };
        // Slow path: Resolve path to ID
        console.log(`🐌 Slow path: Resolving path "${params.fullPath}" to ID`);
        itemInfo = await this._getItemInfoFromPath(params.fullPath!);
        const parsedPath = this._parsePath(params.fullPath!);
        parentPath = parsedPath.parent;
      // Get full metadata using the SDK based on type
      const metadata =
        itemInfo.type === 'folder'
          ? await this._client.folders.getFolderById(itemInfo.id, options)
          : await this._client.files.getFileById(itemInfo.id, options);
      return this._convertToMetadata(metadata, parentPath);
      console.error('Error getting object metadata', { params, error });
   * Downloads a file's contents
   * This method retrieves the raw content of a file as a Buffer.
   * @returns A Promise that resolves to a Buffer containing the file's contents
   * @throws Error if the file doesn't exist or cannot be downloaded
   * - This method will throw an error if the object is a folder
   * - For large files, consider using CreatePreAuthDownloadUrl instead
   * - For upload sessions that haven't been completed, this method will fail
   *   // Download a text file
   *   const fileContent = await storage.GetObject('documents/notes.txt');
   *   // Convert Buffer to string for text files
   *   const textContent = fileContent.toString('utf8');
   *   console.log('File content:', textContent);
   *   // For binary files, you can write the buffer to disk
   *   // or process it as needed
   *   console.error('Error downloading file:', error.message);
      let fileId: string;
        fileId = params.objectId;
        console.log(`⚡ Fast path: Using Object ID directly: ${fileId}`);
        fileId = await this._getIdFromPath(params.fullPath!);
        console.log(`🐌 Slow path: Resolved path "${params.fullPath}" to ID: ${fileId}`);
      if (!fileId) {
        throw new Error(`File not found: ${params.objectId || params.fullPath}`);
      // Use SDK to download file content as a stream
      const stream = await this._client.downloads.downloadFile(fileId);
        throw new Error(`Failed to download file: ${params.objectId || params.fullPath}`);
      // Convert stream to buffer
        stream.on('data', (chunk: Buffer) => chunks.push(chunk));
        stream.on('error', reject);
        stream.on('end', () => resolve(Buffer.concat(chunks as unknown as Uint8Array[])));
      console.error('Error getting object', { params, error });
   * Uploads a file to Box storage
   * This method uploads a file to the specified path in Box storage. It automatically
   * determines whether to use a simple upload or chunked upload based on file size.
   * @param data - Buffer containing the file content
   * @param contentType - Optional MIME type of the file (if not provided, it will be guessed from the filename)
   * @param metadata - Optional metadata to associate with the file (not used in Box implementation)
   * - Automatically creates parent directories if they don't exist
   * - Files smaller than 50MB use a simple upload
   * - Files 50MB or larger use a chunked upload process
   * - If a file with the same name exists, it will be replaced
   * // Create a simple text file
   * const textContent = Buffer.from('This is a sample document', 'utf8');
   * const uploadResult = await storage.PutObject(
   *   'documents/sample.txt',
   *   textContent,
   *   'text/plain'
   * // Upload a large file using chunked upload
   * const largeFileBuffer = fs.readFileSync('/path/to/large-presentation.pptx');
   * const largeUploadResult = await storage.PutObject(
   *   'presentations/quarterly-results.pptx',
   *   largeFileBuffer,
   *   'application/vnd.openxmlformats-officedocument.presentationml.presentation'
   * if (largeUploadResult) {
   *   console.log('Large file uploaded successfully');
   *   console.error('Failed to upload large file');
      console.log(`[Box PutObject] Starting upload for: ${objectName}, size: ${data.length} bytes`);
      console.log(`[Box PutObject] Account: ${this.AccountName}, Root folder ID: ${this._rootFolderId}`);
      console.log(`[Box PutObject] Parsed path:`, { name: parsedPath.name, parent: parsedPath.parent });
          // First ensure the parent directory exists (create it if needed)
          const parentDirExists = await this.CreateDirectory(parsedPath.parent);
            console.error(`Failed to ensure parent directory exists: ${parsedPath.parent}`);
          // Find folder ID using path resolution
          parentId = await this._findFolderIdByPath(parsedPath.parent);
          console.error(`Error resolving parent folder: ${error.message}`);
      // Check if file already exists
        fileId = await this._getIdFromPath(objectName);
        // File doesn't exist, we'll upload as new
        // Convert Buffer to Readable stream for the SDK
        const fileStream = Readable.from(data);
        console.log(`[Box PutObject] Uploading to parent folder ID: ${parentId}, filename: ${parsedPath.name}`);
        if (fileId) {
          // Update existing file (upload new version)
          console.log(`[Box PutObject] File exists (ID: ${fileId}), uploading new version...`);
          await this._client.uploads.uploadFileVersion(fileId, {
            attributes: {
              name: parsedPath.name,
            file: fileStream,
          console.log(`✅ File updated successfully: ${objectName}`);
          // Upload new file
          console.log(`[Box PutObject] File does not exist, creating new file...`);
          await this._client.uploads.uploadFile({
              parent: { id: parentId },
          console.log(`✅ File uploaded successfully: ${objectName}`);
      } catch (uploadError: unknown) {
        const error = uploadError as { statusCode?: number; message?: string; code?: string; context_info?: unknown };
        console.error(`[Box PutObject] Error uploading file:`, {
          statusCode: error.statusCode,
          code: error.code,
          contextInfo: error.context_info,
          fullError: uploadError,
        if (error.statusCode === 409) {
          console.log(`[Box PutObject] File already exists (conflict): ${objectName}`);
      console.error('Error putting object', { objectName, error });
   * Copies a file from one location to another
   * This method creates a copy of a file at a new location. The original file
   * remains unchanged.
   * @param sourceObjectName - Path to the source file (e.g., 'templates/report-template.docx')
   * @param destinationObjectName - Path where the copy should be created (e.g., 'documents/new-report.docx')
   * - Only files can be copied; folders cannot be copied with this method
   * - Parent directories in the destination path will be created automatically if they don't exist
   * - If a file with the same name exists at the destination, it will be replaced
   * // Copy a template file to a new location with a different name
   * const copyResult = await storage.CopyObject(
   *   'templates/financial-report.xlsx',
   *   'reports/2023/q1-financial-report.xlsx'
   * if (copyResult) {
   *   console.error('Failed to copy file');
      const sourceInfo = await this._getItemInfoFromPath(sourceObjectName);
        console.log(`Source item not found: ${sourceObjectName}`);
      if (sourceInfo.type !== 'file') {
        throw new Error('Only files can be copied with CopyObject');
      const destPath = this._parsePath(destinationObjectName);
      // Copy the file using SDK
      await this._client.files.copyFile(sourceInfo.id, {
      console.error('Error copying object', { sourceObjectName, destinationObjectName, error });
   * Checks if a file or folder exists
   * This method verifies whether an object (file or folder) exists at the specified path.
   * @param objectName - Path to check (e.g., 'documents/report.pdf')
   * @returns A Promise that resolves to true if the object exists, false otherwise
   * // Check if a file exists before attempting to download it
   * const exists = await storage.ObjectExists('presentations/quarterly-update.pptx');
   *   // File exists, proceed with download
   *   const fileContent = await storage.GetObject('presentations/quarterly-update.pptx');
   *   // Process the file...
      const id = await this._getIdFromPath(objectName);
      return id !== null;
   * Checks if a directory exists
   * This method verifies whether a folder exists at the specified path.
   * Unlike ObjectExists, this method also checks that the item is a folder.
   * @param directoryPath - Path to check (e.g., 'documents/reports')
   * @returns A Promise that resolves to true if the directory exists, false otherwise
   * - Returns false if the path exists but points to a file instead of a folder
   * // Check if a directory exists before creating a file in it
   * const dirExists = await storage.DirectoryExists('documents/reports');
   * if (!dirExists) {
   *   // Create the directory first
   *   await storage.CreateDirectory('documents/reports');
   * // Now we can safely put a file in this directory
   * await storage.PutObject('documents/reports/annual-summary.pdf', fileContent, 'application/pdf');
        const folderId = await this._findFolderIdByPath(normalizedPath);
        console.log(`✅ Directory ${normalizedPath} exists with ID: ${folderId}`);
        // Make a direct call to verify it's a folder using SDK
          const folderInfo = await this._client.folders.getFolderById(folderId, {
              fields: ['type'],
          return folderInfo.type === 'folder';
          // If we can't get the folder info, it's not a valid folder
          console.log(`Item with ID ${folderId} exists but is not a folder`);
        console.log(`❌ Directory ${normalizedPath} does not exist`);
      console.error('Error checking directory exists', { directoryPath, error });
   * Finds a Box folder ID by traversing a path string
   * This helper method navigates through the Box folder hierarchy,
   * following each segment of the path to find the ID of the target folder.
   * It uses pagination to handle large folders efficiently.
   * @param path - The path string to resolve (e.g., 'documents/reports/2023')
   * @returns A Promise that resolves to the Box folder ID
   * @throws Error if any segment of the path cannot be found
  private async _findFolderIdByPath(path: string): Promise<string> {
      // Split the path into segments
      const pathSegments = path.split('/').filter((segment) => segment.length > 0);
      // Handle "All Files" special case - it's not an actual folder name in the API
      if (pathSegments.length > 0 && pathSegments[0] === 'All Files') {
        pathSegments.shift(); // Remove "All Files" from the path
      let currentFolderId = this._rootFolderId; // Start from root
      if (pathSegments.length === 0) {
        return currentFolderId; // Return root folder ID if path is empty
      // Traverse the path
      for (const segment of pathSegments) {
        // Use pagination to handle large folders
        let folder = null;
        while (hasMoreItems && !folder) {
          const items = await this._client.folders.getFolderItems(currentFolderId, {
          // Filter to only folders
          const folders = items.entries?.filter((item) => item.type === 'folder') || [];
          // Look for the target folder
          folder = folders.find((item) => item.name === segment);
          // If folder is found, break out of pagination loop
          if (folder) {
          offset += items.entries?.length || 0;
          hasMoreItems = (items.entries?.length || 0) === LIMIT && offset < (items.totalCount || 0);
        if (!folder || !folder.id) {
          throw new Error(`Folder not found: ${segment}`);
        currentFolderId = folder.id;
      return currentFolderId;
   * Search files in Box using Box Search API.
   * This method provides full-text search capabilities across file names and content
   * using Box's native search functionality. It supports filtering by file type,
   * date ranges, and path prefixes.
   * @param query - The search query string. Supports boolean operators (AND, OR, NOT) and exact phrases in quotes
   * @param options - Optional search options to filter and customize results
   * @param options.maxResults - Maximum number of results to return (default: 100, max: 200)
   * @param options.fileTypes - Array of file types to filter by (e.g., ['pdf', 'docx'] or ['application/pdf'])
   * @param options.modifiedAfter - Only return files modified after this date
   * @param options.modifiedBefore - Only return files modified before this date
   * @param options.pathPrefix - Restrict search to files within this path (e.g., 'documents/reports/')
   * @param options.searchContent - Whether to search file contents (Box searches content by default)
   * @returns A Promise resolving to a FileSearchResultSet containing matching files
   * - Box searches both file names and content by default
   * - The searchContent option is provided for interface compatibility but doesn't restrict Box's behavior
   * - File types can be specified as extensions or MIME types
   * - Date filters use Box's created_at_range parameter
   * - Path prefix is implemented by filtering results to ancestor folder IDs
   * - Results are sorted by relevance when available
   * // Search for PDF files containing "quarterly report"
   * const results = await storage.SearchFiles('quarterly report', {
   *   fileTypes: ['pdf'],
   *   pathPrefix: 'documents/reports',
   *   modifiedAfter: new Date('2023-01-01'),
   *   maxResults: 50
   * console.log(`Found ${results.results.length} matching files`);
   * for (const file of results.results) {
   *   console.log(`- ${file.path} (${file.size} bytes, score: ${file.relevance})`);
   *   if (file.excerpt) {
   *     console.log(`  Excerpt: ${file.excerpt}`);
      // Default options
      const maxResults = Math.min(options?.maxResults || 100, 200); // Box API limit is 200
        limit: maxResults,
        fields: ['id', 'name', 'type', 'size', 'content_type', 'modified_at', 'created_at', 'etag', 'path_collection', 'parent'],
        type: 'file', // Only search for files, not folders
      // Build file extensions filter from fileTypes option
      if (options?.fileTypes && options.fileTypes.length > 0) {
        const extensions = this._buildFileExtensionsFilter(options.fileTypes);
          searchParams.fileExtensions = extensions;
      // Build date range filter (Box uses updatedAtRange for date filtering)
      if (options?.modifiedAfter || options?.modifiedBefore) {
        const dateRanges: string[] = [];
        if (options.modifiedAfter) {
          dateRanges.push(options.modifiedAfter.toISOString());
          // Box requires both start and end dates, use epoch if not specified
          dateRanges.push(new Date(0).toISOString());
        if (options.modifiedBefore) {
          dateRanges.push(options.modifiedBefore.toISOString());
        // If modifiedBefore is not specified, Box uses current date
        searchParams.updatedAtRange = dateRanges;
      // Handle pathPrefix by converting to ancestor folder ID
      if (options?.pathPrefix) {
          const folderId = await this._getIdFromPath(options.pathPrefix);
          if (folderId) {
            searchParams.ancestorFolderIds = [folderId];
          console.warn(`Could not resolve pathPrefix "${options.pathPrefix}", searching all folders`);
      } else if (this._rootFolderId && this._rootFolderId !== '0') {
        // If we have a configured root folder (not account root), scope search to it
        // First verify the folder is accessible to avoid 404 errors with enterprise auth
          await this._client.folders.getFolderById(this._rootFolderId, {
            queryParams: { fields: ['id'] },
          console.log(`✅ Scoping search to configured root folder: ${this._rootFolderId}`);
          searchParams.ancestorFolderIds = [this._rootFolderId];
          console.warn(`⚠️  Configured root folder ${this._rootFolderId} not accessible to current user. Searching entire account instead.`);
          // Don't set ancestorFolderIds - search entire accessible account
      // Perform the search using Box SDK
      const searchResults = await this._client.search.searchForContent(searchParams);
      // Transform Box search results to FileSearchResult format
      const results: FileSearchResult[] = [];
      // Type guard to check if result is SearchResults (not SearchResultsWithSharedLinks)
      const isSearchResults = (result: any): result is { entries?: readonly any[] } => {
        return 'entries' in result;
      if (isSearchResults(searchResults) && searchResults.entries) {
        for (const item of searchResults.entries) {
          // Skip folders and web links, only include files
          if (!item || item.type !== 'file') {
          // Type assertion for file item
          const fileItem = item as any;
          // Reconstruct the full path from pathCollection
          const path = this._reconstructPath(fileItem);
          // Parse lastModified date safely
          let lastModified: Date;
          const dateValue = fileItem.modifiedAt || fileItem.createdAt;
          if (dateValue) {
            const parsedDate = new Date(dateValue);
            lastModified = isNaN(parsedDate.getTime()) ? new Date() : parsedDate;
            lastModified = new Date();
            name: fileItem.name || '',
            size: fileItem.size || 0,
            contentType: mime.lookup(fileItem.name || '') || 'application/octet-stream',
            lastModified,
            objectId: fileItem.id || '', // Box file ID for direct access (bypasses path resolution)
            // Box search doesn't provide relevance scores in the standard API
            relevance: undefined,
            // Box search doesn't provide excerpts in the standard API
            excerpt: undefined,
            // Cannot determine if match was in filename vs content from Box API
            matchInFilename: undefined,
              id: fileItem.id || '',
              etag: fileItem.etag || '',
            providerData: {
              boxItemId: fileItem.id,
              boxItemType: fileItem.type,
      // Determine if there are more results
      const totalMatches = isSearchResults(searchResults) ? searchResults.totalCount : undefined;
      const hasMore = totalMatches ? totalMatches > results.length : false;
        totalMatches,
        hasMore,
        // Box SDK v10 search doesn't provide pagination tokens in the standard response
        nextPageToken: undefined,
      console.error('Error searching files in Box', { query, options, error });
      // Throw error so caller knows search failed
      throw new Error(`Box search failed: ${error instanceof Error ? error.message : String(error)}`);
   * Builds a list of file extensions from file type specifications
   * This helper method converts generic file type specifications (extensions or MIME types)
   * into Box API fileExtensions parameter values.
   * @param fileTypes - Array of file extensions (e.g., 'pdf', 'docx') or MIME types
   * @returns Array of file extension strings (without dots)
  private _buildFileExtensionsFilter(fileTypes: string[]): string[] {
    const extensions: string[] = [];
    for (const fileType of fileTypes) {
      // If it looks like a MIME type (contains '/'), look up extensions
      if (fileType.includes('/')) {
        const extension = mime.extension(fileType);
          extensions.push(extension);
        // Otherwise treat as extension directly (remove leading dot if present)
        const cleanExt = fileType.startsWith('.') ? fileType.substring(1) : fileType;
        extensions.push(cleanExt);
    return extensions;
   * Reconstructs the full path to a Box item from its pathCollection
   * This helper method builds the complete path by traversing the parent
   * folder hierarchy stored in the item's pathCollection.
   * @param item - The Box item with pathCollection data
   * @returns The full path string (e.g., 'documents/reports/file.pdf')
  private _reconstructPath(item: any): string {
    const pathParts: string[] = [];
    // Add parent folder names from pathCollection
    // pathCollection.entries contains the full ancestor chain from root to immediate parent
    if (item.pathCollection?.entries) {
      for (const entry of item.pathCollection.entries) {
        // Skip the system root (id '0' or 'All Files')
        if (entry.id === '0' || entry.name === 'All Files') {
        // Skip the configured root folder (if we have a custom root)
        // This makes paths relative to the configured root, not the account root
        if (entry.id === this._rootFolderId) {
        pathParts.push(entry.name);
    // Add the item name itself
    if (item.name) {
      pathParts.push(item.name);
    return pathParts.join('/');
   * Checks if a string is a Box Object ID (numeric) vs a path (contains /)
   * @param identifier - String to check
   * @returns True if it's a Box file/folder ID, false if it's a path
  private _isObjectId(identifier: string): boolean {
    // Box IDs are purely numeric strings
    return /^\d+$/.test(identifier);
