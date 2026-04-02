import { Dropbox, DropboxOptions, files } from 'dropbox';
 * Configuration interface for Dropbox file storage.
 * Supports both standard OAuth naming (clientID/clientSecret) and Dropbox naming (appKey/appSecret).
interface DropboxConfig extends StorageProviderConfig {
  /** OAuth client ID (standard naming) */
  /** OAuth client secret (standard naming) */
  /** Dropbox app key (alternative to clientID) */
  appKey?: string;
  /** Dropbox app secret (alternative to clientSecret) */
  appSecret?: string;
  selectUser?: string;
  rootPath?: string;
 * Extended type for filesDownload response that includes the fileBinary property
 * The Dropbox SDK TypeScript definitions don't include this property, but it's present in the actual response
interface DropboxDownloadResponse extends files.FileMetadata {
  fileBinary: ArrayBuffer;
 * FileStorageBase implementation for Dropbox cloud storage
 * This provider allows working with files stored in Dropbox. It supports
 * authentication via access token or refresh token with app credentials.
 * This implementation requires one of the following authentication methods:
 *    - STORAGE_DROPBOX_ACCESS_TOKEN - A valid Dropbox API access token
 *    - STORAGE_DROPBOX_REFRESH_TOKEN - A valid Dropbox API refresh token
 *    - STORAGE_DROPBOX_APP_KEY - Your Dropbox application key (client ID)
 *    - STORAGE_DROPBOX_APP_SECRET - Your Dropbox application secret
 * - STORAGE_DROPBOX_ROOT_PATH - Path within Dropbox to use as the root (defaults to empty which is the root)
 * // Set required environment variables
 * process.env.STORAGE_DROPBOX_ACCESS_TOKEN = 'your-access-token';
 * const storage = new DropboxFileStorage();
 * const fileContent = Buffer.from('Hello, Dropbox!');
@RegisterClass(FileStorageBase, 'Dropbox Storage')
export class DropboxFileStorage extends FileStorageBase {
  protected readonly providerName = 'Dropbox';
   * Dropbox API client instance
  private _client: Dropbox;
   * Access token for Dropbox authentication
  private _accessToken: string | undefined;
   * Root path within Dropbox to use as the storage root
  private _rootPath: string;
   * Creates a new DropboxFileStorage instance
   * This constructor initializes the Dropbox client using the provided credentials
   * @throws Error if neither access token nor refresh token with app credentials are provided
    const config = getProviderConfig('dropbox');
    // Dropbox auth can be via access token or refresh token
    const accessToken = config?.accessToken || env.get('STORAGE_DROPBOX_ACCESS_TOKEN').asString();
    const refreshToken = config?.refreshToken || env.get('STORAGE_DROPBOX_REFRESH_TOKEN').asString();
    const appKey = config?.clientID || env.get('STORAGE_DROPBOX_APP_KEY').asString();
    const appSecret = config?.clientSecret || env.get('STORAGE_DROPBOX_APP_SECRET').asString();
    if (accessToken) {
      // Use access token directly
      this._accessToken = accessToken;
      const dropboxConfig: DropboxOptions = { accessToken };
      // For Dropbox Business/Team accounts, specify the team member
      if (config && 'selectUser' in config && config.selectUser) {
        dropboxConfig.selectUser = config.selectUser as string;
      this._client = new Dropbox(dropboxConfig);
    } else if (refreshToken && appKey && appSecret) {
      // Use refresh token with app credentials
      const dropboxConfig: DropboxOptions = {
        refreshToken,
        clientId: appKey,
        clientSecret: appSecret,
    // Note: If no credentials are available, client will be initialized in initialize() method
    // This allows for database-driven configuration to be passed after construction
    // Root path, optional (defaults to empty which is root)
    this._rootPath = config?.rootPath || env.get('STORAGE_DROPBOX_ROOT_PATH').default('').asString();
    // Ensure root path starts with / if not empty
    if (this._rootPath && !this._rootPath.startsWith('/')) {
      this._rootPath = '/' + this._rootPath;
   * Initialize Dropbox storage provider.
   * const storage = new DropboxFileStorage(); // Constructor loads env vars
   *   accountName: 'Dropbox Account',
   *   rootPath: '/optional-root-path'
  public async initialize(config?: DropboxConfig): Promise<void> {
    // If config is provided, reinitialize the client with it
    const accessToken = config.accessToken;
    const refreshToken = config.refreshToken;
    // Support both naming conventions: clientID/clientSecret (standard) and appKey/appSecret (Dropbox terminology)
    const appKey = config.clientID || config.appKey;
    const appSecret = config.clientSecret || config.appSecret;
    // Prefer refresh token over access token when both are available
    // Access tokens expire (4 hours), refresh tokens are long-lived and auto-refresh
    if (refreshToken && appKey && appSecret) {
      // Use refresh token with app credentials - this is the preferred method
      if (config.selectUser) {
        dropboxConfig.selectUser = config.selectUser;
      // Set a placeholder for IsConfigured check - the SDK will get a real token on first API call
      this._accessToken = 'refresh-token-mode';
    } else if (accessToken) {
      // Fall back to access token if no refresh token available
      // Note: This will fail when the access token expires (typically 4 hours)
    // Update root path if provided
    if (config.rootPath) {
      this._rootPath = config.rootPath;
   * Checks if Dropbox provider is properly configured.
   * Returns true if access token is present.
    if (!hasAccessToken) {
        `❌ Dropbox provider not configured. Missing: Access Token\n\n` +
        `Option 1: Environment Variables (Access Token)\n` +
        `  export STORAGE_DROPBOX_ACCESS_TOKEN="..."\n` +
        `  const storage = new DropboxFileStorage();\n` +
        `Option 2: Environment Variables (Refresh Token)\n` +
        `  export STORAGE_DROPBOX_REFRESH_TOKEN="..."\n` +
        `  export STORAGE_DROPBOX_APP_KEY="..."\n` +
        `  export STORAGE_DROPBOX_APP_SECRET="..."\n` +
        `Option 3: Database Credentials (Multi-Tenant)\n` +
        `    accessToken: "..."\n` +
    return hasAccessToken;
   * Normalizes a path to be compatible with Dropbox API
   * This helper method ensures paths are formatted correctly for the Dropbox API,
   * including proper handling of the root path prefix.
   * @param path - The path to normalize
   * @returns A normalized path string suitable for Dropbox API calls
  private _normalizePath(path: string): string {
    console.log('[DropboxFileStorage._normalizePath] Input:', {
      rootPath: this._rootPath,
    // Combine root path with the given path
    let fullPath = path;
    if (this._rootPath) {
      fullPath = path ? (path.startsWith('/') ? this._rootPath + path : this._rootPath + '/' + path) : this._rootPath;
    } else if (!fullPath.startsWith('/') && fullPath !== '') {
      fullPath = '/' + fullPath;
    console.log('[DropboxFileStorage._normalizePath] After combining root path:', { fullPath });
    // For root, Dropbox uses empty string instead of "/"
    if (fullPath === '/') {
      console.log('[DropboxFileStorage._normalizePath] Converted root "/" to empty string');
    console.log('[DropboxFileStorage._normalizePath] Final result:', { fullPath });
   * Gets metadata for a file or folder from Dropbox
   * This helper method retrieves metadata for a file or folder using the Dropbox API.
   * @param path - The path to get metadata for
   * @returns A Promise that resolves to the Dropbox metadata object
   * @throws Error if the item doesn't exist or cannot be accessed
  private async _getMetadata(path: string): Promise<any> {
    const normalizedPath = this._normalizePath(path);
      const response = await this._client.filesGetMetadata({
        path: normalizedPath,
        include_media_info: false,
      return response.result;
      throw new Error(`Item not found: ${path} (${error.message})`);
   * Converts a Dropbox file/folder to StorageObjectMetadata
   * This helper method transforms Dropbox-specific metadata into the
   * standard StorageObjectMetadata format used by FileStorageBase.
   * @param item - The Dropbox item metadata
   * @param parentPath - Optional parent path string
  private _convertToMetadata(item: files.FileMetadataReference | files.FolderMetadataReference, parentPath: string = ''): StorageObjectMetadata {
    const isDirectory = item['.tag'] === 'folder';
    console.log('[DropboxFileStorage._convertToMetadata] Processing item:', {
      path_display: item.path_display,
    // Extract path from item.path_display
    let path = '';
    if (item.path_display) {
      // Remove file name from path
      const pathParts = item.path_display.split('/');
      pathParts.pop();
      path = pathParts.join('/');
      console.log('[DropboxFileStorage._convertToMetadata] After extracting directory:', { path });
      // Remove root path if present
      if (this._rootPath && path.startsWith(this._rootPath)) {
        const oldPath = path;
        path = path.substring(this._rootPath.length);
        console.log('[DropboxFileStorage._convertToMetadata] Removed root path:', {
          newPath: path,
      if (path.startsWith('/')) {
        path = path.substring(1);
        console.log('[DropboxFileStorage._convertToMetadata] Removed leading slash:', { path });
    // Use parentPath if provided
      console.log('[DropboxFileStorage._convertToMetadata] Using parentPath:', { parentPath });
      path = parentPath;
    // Construct full path - ensure no double slashes
    let fullPath = name;
      // Remove trailing slash from path and leading slash from name
      const cleanPath = path.endsWith('/') ? path.slice(0, -1) : path;
      const cleanName = name.startsWith('/') ? name.slice(1) : name;
      fullPath = `${cleanPath}/${cleanName}`;
    console.log('[DropboxFileStorage._convertToMetadata] Final result:', {
      contentType: isDirectory ? 'application/x-directory' : mime.lookup(name) || 'application/octet-stream',
      lastModified: isDirectory ? new Date() : new Date(item.server_modified || Date.now()),
        rev: isDirectory ? undefined : (item as files.FileMetadataReference).rev,
   * Creates a pre-authenticated upload URL (not supported in Dropbox)
   * This method is not supported for Dropbox storage as Dropbox doesn't provide
   * a way to generate pre-authenticated upload URLs like object storage services.
   * Instead, use the PutObject method for file uploads.
   * @param objectName - The object name (path) to create a pre-auth URL for
   * @throws UnsupportedOperationError always, as this operation is not supported
   * // This will throw an UnsupportedOperationError
   *   await storage.CreatePreAuthUploadUrl('documents/report.docx');
   *   if (error instanceof UnsupportedOperationError) {
   *     console.log('Pre-authenticated upload URLs are not supported in Dropbox.');
   *     // Use PutObject instead
   *     await storage.PutObject('documents/report.docx', fileContent);
   * This method generates a time-limited URL that can be used to upload
   * a file directly to Dropbox without additional authentication.
   * @throws Error if URL creation fails
   * - Dropbox temporary upload links typically expire after 4 hours
   * - Maximum file size for upload via temporary link is 150MB
   * - The upload must use Content-Type: application/octet-stream
   * - The URL is for one-time use only
   *   const uploadPayload = await storage.CreatePreAuthUploadUrl('documents/financial-report.pdf');
   *   console.log(`Upload the file to this URL: ${uploadPayload.UploadUrl}`);
   *   // Use the URL to upload file directly from client
   *   // POST request with Content-Type: application/octet-stream
      console.log('[DropboxFileStorage.CreatePreAuthUploadUrl] Input:', {
      const normalizedPath = this._normalizePath(objectName);
      console.log('[DropboxFileStorage.CreatePreAuthUploadUrl] After normalization:', {
        normalizedPath,
      // Create a temporary upload link
      // Note: commit_info is optional, defaults to overwrite mode
      const response = await this._client.filesGetTemporaryUploadLink({
        commit_info: {
          mode: { '.tag': 'overwrite' },
          autorename: false,
          mute: true,
      console.log('[DropboxFileStorage.CreatePreAuthUploadUrl] Success:', {
        link: response.result.link,
        UploadUrl: response.result.link,
        ProviderKey: normalizedPath,
      console.error('[DropboxFileStorage.CreatePreAuthUploadUrl] Error:', {
        error: error.message || error,
        errorDetails: error.error || error,
        errorStatus: error.status,
        fullError: JSON.stringify(error, null, 2),
      const errorMsg = error.error?.error_summary || error.message || JSON.stringify(error);
      throw new Error(`Failed to create upload URL for: ${objectName} - ${errorMsg}`);
   * a file without authentication.
   * - Dropbox temporary download links typically expire after 4 hours
   * - Generated URLs can be shared with users who don't have Dropbox access
   *   // without requiring Dropbox authentication
      console.log('[DropboxFileStorage.CreatePreAuthDownloadUrl] Input:', {
      console.log('[DropboxFileStorage.CreatePreAuthDownloadUrl] After normalization:', {
      // Create a temporary download link
      const response = await this._client.filesGetTemporaryLink({
      console.log('[DropboxFileStorage.CreatePreAuthDownloadUrl] Success:', {
      return response.result.link;
      console.error('[DropboxFileStorage.CreatePreAuthDownloadUrl] Error:', {
   * This method moves a file or folder to a new location in Dropbox.
   * - If the destination already exists, the operation will fail
      const fromPath = this._normalizePath(oldObjectName);
      const toPath = this._normalizePath(newObjectName);
      await this._client.filesMoveV2({
        from_path: fromPath,
        to_path: toPath,
   * Deletes a file or folder from Dropbox
   * This method permanently deletes a file or folder from Dropbox storage.
   * - Dropbox puts deleted items in the trash, where they can be recovered for a limited time
   * - For deleting folders with contents, use DeleteDirectory with recursive=true
      // Remove trailing slash if present (Dropbox doesn't accept trailing slashes for folder deletion)
      const normalizedPath = objectName.endsWith('/') ? this._normalizePath(objectName.substring(0, objectName.length - 1)) : this._normalizePath(objectName);
      console.log('[DropboxFileStorage] DeleteObject called:', {
        hadTrailingSlash: objectName.endsWith('/'),
      const result = await this._client.filesDeleteV2({
      console.log('[DropboxFileStorage] filesDeleteV2 result:', result);
      // If the path doesn't exist, consider it success for idempotency
      if (error.status === 409 && error.error?.error?.['.tag'] === 'path_lookup') {
        console.log('[DropboxFileStorage] Path not found (already deleted):', objectName);
      console.error('[DropboxFileStorage] Error deleting object', {
        normalizedPath: objectName.endsWith('/') ? this._normalizePath(objectName.substring(0, objectName.length - 1)) : this._normalizePath(objectName),
   * - Returns empty arrays if the directory doesn't exist or an error occurs
      const normalizedPath = this._normalizePath(prefix);
      console.log('[DropboxFileStorage] ListObjects called:', {
        hasClient: !!this._client,
        isConfigured: this.IsConfigured,
      // Debug: Try to get current account info to understand access type
        const accountInfo = await this._client.usersGetCurrentAccount();
        console.log('[DropboxFileStorage] Account info:', {
          accountId: accountInfo.result.account_id,
          email: accountInfo.result.email,
          name: accountInfo.result.name.display_name,
        console.log('[DropboxFileStorage] Could not get account info:', error?.message);
      const response = await this._client.filesListFolder({
        recursive: false,
        include_deleted: false,
        include_has_explicit_shared_members: false,
      console.log('[DropboxFileStorage] filesListFolder response:', {
        entriesCount: response.result.entries.length,
        entries: response.result.entries.map((e) => ({ name: e.name, tag: e['.tag'] })),
        has_more: response.result.has_more,
        cursor: response.result.cursor,
      // Check if we're in an app folder scenario
      if (response.result.entries.length === 0 && normalizedPath === '') {
        console.log('[DropboxFileStorage] Empty root - this might be an app-folder-only token');
        console.log('[DropboxFileStorage] Note: If using app folder access, files are in /Apps/[YourAppName]/');
        console.log('[DropboxFileStorage] You can set rootPath in configuration to point to your app folder');
        // Try to list a few common app folder paths to help diagnose
        const testPaths = ['/Apps', '/Apps/MJ-Files-Test', '/MJ-FileTest'];
        for (const testPath of testPaths) {
            console.log(`[DropboxFileStorage] Testing path: ${testPath}`);
            const testResponse = await this._client.filesListFolder({ path: testPath, recursive: false });
              `[DropboxFileStorage] Found ${testResponse.result.entries.length} items at ${testPath}:`,
              testResponse.result.entries.map((e) => ({ name: e.name, tag: e['.tag'] })),
          } catch (testError) {
            const errorMsg = testError.error?.error_summary || testError.message;
            console.log(`[DropboxFileStorage] Cannot access ${testPath}: ${errorMsg}`);
      // Process entries
      for (const entry of response.result.entries) {
        // Skip deleted entries
        if (entry['.tag'] === 'deleted') {
        objects.push(this._convertToMetadata(entry, prefix));
        if (entry['.tag'] === 'folder') {
          const folderPath = prefix ? (prefix.endsWith('/') ? `${prefix}${entry.name}` : `${prefix}/${entry.name}`) : entry.name;
          console.log('[DropboxFileStorage] Adding folder prefix:', folderPath);
      console.log('[DropboxFileStorage] Final result:', {
        objectsCount: objects.length,
        prefixesCount: prefixes.length,
        prefixes,
      console.error('[DropboxFileStorage] Error listing objects:', { prefix, error, errorMessage: error?.message, errorStatus: error?.status });
   * Creates a new directory (folder) in Dropbox
   * This method creates a folder at the specified path.
   * - Parent directories must already exist; this method doesn't create them recursively
   * // Create a new folder
   * const createResult = await storage.CreateDirectory('documents/reports/2023');
   *     'documents/reports/2023/annual-summary.xlsx',
      const normalizedPath = directoryPath.endsWith('/')
        ? this._normalizePath(directoryPath.substring(0, directoryPath.length - 1))
        : this._normalizePath(directoryPath);
      await this._client.filesCreateFolderV2({
      // If folder already exists, consider it success
      if (error.status === 409 && error.error?.error?.['.tag'] === 'path') {
   * Deletes a directory from Dropbox
   * This method deletes a folder and optionally ensures it's empty first.
   * Note that Dropbox API always deletes folders recursively, so we perform
   * an additional check when recursive=false to protect against accidental deletion.
   * - Dropbox puts deleted folders in the trash, where they can be recovered for a limited time
   * // Attempt to delete an empty folder
   * const deleteResult = await storage.DeleteDirectory('temp/empty-folder', false);
  public async DeleteDirectory(directoryPath: string, recursive = true): Promise<boolean> {
      // Note: Dropbox API always deletes directories recursively
      // If we want to prevent deleting non-empty dirs, we'd need to check contents first
        // Check if directory is empty first
        const listing = await this._client.filesListFolder({
        if (listing.result.entries.length > 0) {
      await this._client.filesDeleteV2({
   * This method retrieves metadata information about a file or folder, such as
   * its name, size, content type, and last modified date.
   * @param params - Object identifier (prefer objectId for performance, fallback to fullPath)
   *   // Fast path: Use objectId (Dropbox file ID)
   *   const metadata = await storage.GetObjectMetadata({ objectId: 'id:a4ayc_80_OEAAAAAAAAAXw' });
   *   // Slow path: Use path
   *   const metadata2 = await storage.GetObjectMetadata({ fullPath: 'presentations/quarterly-update.pptx' });
   *   // Dropbox-specific metadata is available in customMetadata
   *   console.log(`Dropbox ID: ${metadata.customMetadata.id}`);
   *   console.log(`Revision: ${metadata.customMetadata.rev}`);
      let path: string;
        // Dropbox IDs must be prefixed with "id:"
        path = params.objectId.startsWith('id:') ? params.objectId : `id:${params.objectId}`;
        console.log(`⚡ Fast path: Using Object ID directly: ${path}`);
        // Slow path: Use normalized path
        path = this._normalizePath(params.fullPath!);
        console.log(`🐌 Slow path: Using path: ${path}`);
        // Parse path to get parent path
        const pathParts = params.fullPath!.split('/');
        pathParts.pop(); // Remove filename/foldername
        parentPath = pathParts.join('/');
      // Check if the result is a deleted entry
      if (response.result['.tag'] === 'deleted') {
        throw new Error(`Object not found (deleted): ${params.objectId || params.fullPath}`);
      return this._convertToMetadata(response.result, parentPath);
   *   const fileContent = await storage.GetObject({ objectId: 'id:a4ayc_80_OEAAAAAAAAAXw' });
   *   const fileContent2 = await storage.GetObject({ fullPath: 'documents/notes.txt' });
      const response = await this._client.filesDownload({
      // Extract file content as Buffer
      // Note: In Dropbox SDK, the file content is in response.result.fileBinary
      // The TypeScript definitions don't include fileBinary, but it's present in the actual response
      return Buffer.from((response.result as DropboxDownloadResponse).fileBinary);
   * Uploads a file to Dropbox
   * This method uploads a file to the specified path in Dropbox. It automatically
   * @param contentType - Optional MIME type of the file (not used in Dropbox implementation)
   * @param metadata - Optional metadata to associate with the file (not used in Dropbox implementation)
   * - Files smaller than 150MB use a simple upload
   * - Files 150MB or larger use a chunked upload process
   * - If a file with the same name exists, it will be overwritten
   * - Parent folders must exist before uploading files to them
   * // Upload a simple text file
   * const uploadResult = await storage.PutObject('documents/sample.txt', textContent);
   *   largeFileBuffer
      // For smaller files (<150MB), use simple upload
      if (data.length < 150 * 1024 * 1024) {
        await this._client.filesUpload({
          contents: data,
        // For larger files, use session upload
        const CHUNK_SIZE = 8 * 1024 * 1024; // 8MB chunks
        // Start upload session
        const sessionStart = await this._client.filesUploadSessionStart({
          close: false,
          contents: data.slice(0, CHUNK_SIZE),
        const sessionId = sessionStart.result.session_id;
        let offset = CHUNK_SIZE;
        // Upload the remaining chunks
        while (offset < data.length) {
          const chunk = data.slice(offset, Math.min(offset + CHUNK_SIZE, data.length));
          const isLastChunk = offset + chunk.length >= data.length;
          if (isLastChunk) {
            // Finish the session with the last chunk
            await this._client.filesUploadSessionFinish({
              cursor: {
                session_id: sessionId,
              commit: {
              contents: chunk,
            // Append chunk to session
            await this._client.filesUploadSessionAppendV2({
          offset += chunk.length;
   * Copies a file or folder from one location to another
   * This method creates a copy of a file or folder at a new location.
   * The original file or folder remains unchanged.
   * @param sourceObjectName - Path to the source object (e.g., 'templates/report-template.docx')
   * - Parent directories must exist in the destination path
      const fromPath = this._normalizePath(sourceObjectName);
      const toPath = this._normalizePath(destinationObjectName);
      await this._client.filesCopyV2({
      await this._getMetadata(objectName);
   * await storage.PutObject('documents/reports/annual-summary.pdf', fileContent);
      const item = await this._getMetadata(normalizedPath);
      return item['.tag'] === 'folder';
   * Search files in Dropbox using Dropbox Search API v2.
   * Dropbox provides full-text search capabilities across file names and content.
   * The search API supports natural language queries and can search both filenames
   * and file content based on the searchContent option.
   * @param query - The search query string (supports natural language and quoted phrases)
   * @param options - Search options for filtering and limiting results
   * @returns A Promise resolving to search results
   * - Content search (searchContent: true) searches both filename and file content
   * - Filename search (searchContent: false, default) searches only filenames
   * - File type filtering converts extensions to Dropbox file categories
   * - Date filters use server_modified timestamp
   * - Path prefix restricts search to a specific folder and its subfolders
   * // Simple filename search
   * const results = await storage.SearchFiles('quarterly report');
   * // Search with file type filter
   * const pdfResults = await storage.SearchFiles('budget', {
   *   modifiedAfter: new Date('2024-01-01')
   * // Content search within a specific folder
   * const contentResults = await storage.SearchFiles('machine learning', {
   *   searchContent: true,
   *   pathPrefix: 'documents/research',
      const maxResults = options?.maxResults || 100;
      // Build Dropbox search options
      const searchOptions: files.SearchV2Arg = {
          max_results: Math.min(maxResults, 1000), // Dropbox max is 1000
          path: options?.pathPrefix ? this._normalizePath(options.pathPrefix) : undefined,
          file_status: { '.tag': 'active' }, // Exclude deleted files
          filename_only: !options?.searchContent, // Search filename only or filename + content
      // Add file extension filter if fileTypes provided
        const extensions = this._extractFileExtensions(options.fileTypes);
          searchOptions.options.file_extensions = extensions;
      // Execute search using Dropbox Search API v2
      const response = await this._client.filesSearchV2(searchOptions);
      // Process search results
      for (const match of response.result.matches || []) {
        // The metadata field is MetadataV2, which can be MetadataV2Metadata or MetadataV2Other
        // We only want MetadataV2Metadata which has the actual file/folder metadata
        if (match.metadata['.tag'] !== 'metadata') {
        const metadataV2 = match.metadata as files.MetadataV2Metadata;
        if (metadataV2.metadata['.tag'] === 'deleted') {
        const metadata = metadataV2.metadata;
        // Skip if not a file (could be folder or other type)
        if (metadata['.tag'] !== 'file') {
        // Apply date filters client-side (Dropbox search doesn't support date filters directly)
          const modifiedDate = new Date(metadata.server_modified);
          if (options.modifiedAfter && modifiedDate < options.modifiedAfter) {
          if (options.modifiedBefore && modifiedDate > options.modifiedBefore) {
        // Extract path information
        const fullPath = this._extractRelativePath(metadata.path_display || metadata.path_lower);
        const pathParts = fullPath.split('/');
        const fileName = pathParts.pop() || metadata.name;
          path: fullPath,
          name: fileName,
          size: metadata.size || 0,
          contentType: mime.lookup(fileName) || 'application/octet-stream',
          lastModified: new Date(metadata.server_modified),
          objectId: metadata.id || '', // Dropbox file ID for direct access
          matchInFilename: this._checkFilenameMatch(fileName, query),
            id: metadata.id,
            rev: metadata.rev,
            dropboxId: metadata.id,
            pathLower: metadata.path_lower,
      // Check if there are more results available
      const hasMore = response.result.has_more || false;
        totalMatches: undefined, // Dropbox doesn't provide total count
        nextPageToken: hasMore ? 'continue' : undefined, // Dropbox uses continue endpoint for pagination
      console.error('Error searching files in Dropbox', { query, options, error });
      throw new Error(`Dropbox search failed: ${error.message || 'Unknown error'}`);
   * Extracts file extensions from fileTypes array.
   * Converts MIME types to extensions and removes duplicates.
   * @param fileTypes - Array of file types (extensions or MIME types)
   * @returns Array of file extensions without leading dots
  private _extractFileExtensions(fileTypes: string[]): string[] {
    const extensions = new Set<string>();
        // It's a MIME type, convert to extension
        const ext = mime.extension(fileType);
        if (ext) {
          extensions.add(ext);
        // It's already an extension, remove leading dot if present
        extensions.add(fileType.startsWith('.') ? fileType.substring(1) : fileType);
    return Array.from(extensions);
   * Extracts the relative path from a Dropbox absolute path.
   * Removes the root path prefix if configured.
   * @param dropboxPath - The absolute Dropbox path
   * @returns The relative path without root prefix
  private _extractRelativePath(dropboxPath: string): string {
    let relativePath = dropboxPath;
    if (this._rootPath && relativePath.startsWith(this._rootPath)) {
      relativePath = relativePath.substring(this._rootPath.length);
    if (relativePath.startsWith('/')) {
      relativePath = relativePath.substring(1);
   * Checks if a filename contains the search query.
   * Performs case-insensitive matching.
   * @param filename - The filename to check
   * @param query - The search query
   * @returns True if the filename contains the query
  private _checkFilenameMatch(filename: string, query: string): boolean {
    return filename.toLowerCase().includes(query.toLowerCase());
