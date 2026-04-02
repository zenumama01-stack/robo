import { google, drive_v3 } from 'googleapis';
 * Configuration interface for Google Drive storage provider.
interface GoogleDriveConfig extends StorageProviderConfig {
  /** OAuth2 Refresh Token (never expires) */
 * Interface for Google Service Account credentials JSON structure.
interface ServiceAccountCredentials {
  project_id: string;
  private_key_id: string;
  private_key: string;
  client_email: string;
  auth_uri: string;
  token_uri: string;
  auth_provider_x509_cert_url: string;
  client_x509_cert_url: string;
 * Google Drive implementation of the FileStorageBase interface.
 * This class provides methods for interacting with Google Drive as a file storage provider.
 * It implements most of the abstract methods defined in FileStorageBase and handles
 * Google Drive-specific authentication, authorization, and file operations.
 * Unlike other storage providers like S3 or Azure, Google Drive has native concepts of
 * folders and files with hierarchical paths, which makes some operations more natural
 * while others (like pre-authenticated upload URLs) are not directly supported.
 * It requires one of the following environment variables to be set:
 * - STORAGE_GDRIVE_KEY_FILE: Path to a service account key file with Drive permissions
 * - STORAGE_GDRIVE_CREDENTIALS_JSON: A JSON object containing service account credentials
 * Optionally, you can set:
 * - STORAGE_GDRIVE_ROOT_FOLDER_ID: ID of a folder to use as the root (for isolation)
 * // Create an instance of GoogleDriveFileStorage
 * const driveStorage = new GoogleDriveFileStorage();
 * const downloadUrl = await driveStorage.CreatePreAuthDownloadUrl('documents/report.pdf');
 * const files = await driveStorage.ListObjects('documents/');
 * // Upload a file directly
 * const uploaded = await driveStorage.PutObject('documents/report.pdf', fileData);
@RegisterClass(FileStorageBase, 'Google Drive Storage')
export class GoogleDriveFileStorage extends FileStorageBase {
  protected readonly providerName = 'Google Drive';
  /** The Google Drive API client */
  private _drive: drive_v3.Drive;
  /** Optional root folder ID to restrict operations to a specific folder */
  private _rootFolderId?: string;
  /** OAuth2 credentials for configuration checking */
  private _clientID?: string;
  private _clientSecret?: string;
  private _refreshToken?: string;
   * Creates a new instance of GoogleDriveFileStorage.
   * Initializes the connection to Google Drive using either a service account
   * key file or credentials provided directly in environment variables.
   * Throws an error if neither authentication method is properly configured.
    const config = getProviderConfig('googleDrive');
    // Get credentials from config or environment
    const keyFile = config?.keyFile || env.get('STORAGE_GDRIVE_KEY_FILE').asString();
    const credentials = config?.credentialsJSON || env.get('STORAGE_GDRIVE_CREDENTIALS_JSON').asJsonObject();
    this._clientID = config?.clientID || env.get('STORAGE_GDRIVE_CLIENT_ID').asString();
    this._clientSecret = config?.clientSecret || env.get('STORAGE_GDRIVE_CLIENT_SECRET').asString();
    this._refreshToken = config?.refreshToken || env.get('STORAGE_GDRIVE_REFRESH_TOKEN').asString();
    const redirectURI = config?.redirectURI || env.get('STORAGE_GDRIVE_REDIRECT_URI').asString();
    // Initialize the Google Drive client - support THREE auth methods
    // Note: If no credentials are found here, initialize() will be called later with database config
    if (keyFile) {
      // Method 1: Using key file (service account)
      const auth = new google.auth.GoogleAuth({
        keyFile,
        scopes: ['https://www.googleapis.com/auth/drive'],
      this._drive = google.drive({ version: 'v3', auth });
    } else if (credentials) {
      // Method 2: Using credentials directly (service account)
      const creds: ServiceAccountCredentials = typeof credentials === 'string' ? JSON.parse(credentials) : credentials;
      const auth = new google.auth.JWT({
        email: creds.client_email,
        key: creds.private_key,
    } else if (this._clientID && this._clientSecret && this._refreshToken) {
      // Method 3: Using OAuth2 with refresh token
      const auth = new google.auth.OAuth2(this._clientID, this._clientSecret, redirectURI);
      auth.setCredentials({ refresh_token: this._refreshToken });
    // If no credentials found, _drive will be undefined and initialize() must be called
    // Optionally set a root folder ID to restrict operations
    this._rootFolderId = config?.rootFolderID || env.get('STORAGE_GDRIVE_ROOT_FOLDER_ID').asString();
   * Checks if Google Drive provider is properly configured.
   * Returns true if all required OAuth credentials are present.
    const hasClientID = !!this._clientID;
    const hasClientSecret = !!this._clientSecret;
    const hasRefreshToken = !!this._refreshToken;
    const isConfigured = hasClientID && hasClientSecret && hasRefreshToken;
      if (!hasClientID) missing.push('Client ID');
      if (!hasClientSecret) missing.push('Client Secret');
      if (!hasRefreshToken) missing.push('Refresh Token');
        `❌ Google Drive provider not configured. Missing: ${missing.join(', ')}\n\n` +
        `  export STORAGE_GDRIVE_CLIENT_ID="..."\n` +
        `  export STORAGE_GDRIVE_CLIENT_SECRET="..."\n` +
        `  export STORAGE_GDRIVE_REFRESH_TOKEN="..."\n` +
        `  const storage = new GoogleDriveFileStorage();\n` +
        `    refreshToken: "..."\n` +
   * Initialize Google Drive storage provider.
   * const storage = new GoogleDriveFileStorage(); // Constructor loads env vars
   * const storage = new GoogleDriveFileStorage();
   *   accountName: 'Google Drive Account',
   *   rootFolderID: 'optional-folder-id'
  public async initialize(config?: GoogleDriveConfig): Promise<void> {
    // Update OAuth2 credentials
    this._clientID = config.clientID || this._clientID;
    this._clientSecret = config.clientSecret || this._clientSecret;
    this._refreshToken = config.refreshToken || this._refreshToken;
    // Update root folder ID if provided
    if (config.rootFolderID) {
      this._rootFolderId = config.rootFolderID;
    // Reinitialize the Google Drive client with new OAuth2 credentials
    if (this._clientID && this._clientSecret && this._refreshToken) {
      const redirectURI = 'urn:ietf:wg:oauth:2.0:oob';
      throw new Error('Google Drive storage requires clientID, clientSecret, and refreshToken to be set');
   * Finds a file or folder by path.
   * This helper method navigates the Google Drive folder structure to find
   * a file or folder at the specified path. It starts from the root (or the
   * configured root folder) and traverses the path components one by one.
   * @param path - The path to the file or folder to find
   * @returns A Promise resolving to the Google Drive file object
   * @throws Error if the path cannot be found
  private async _getItemByPath(path: string): Promise<drive_v3.Schema$File> {
    console.log('[GoogleDriveFileStorage] _getItemByPath called with:', JSON.stringify(path));
    // Normalize path: remove leading/trailing slashes and collapse multiple slashes
    const normalizedPath = path ? path.replace(/^\/+|\/+$/g, '').replace(/\/+/g, '/') : '';
    console.log('[GoogleDriveFileStorage] Normalized path:', JSON.stringify(normalizedPath));
    if (!normalizedPath) {
      // Return the root folder or the specified root folder
        id: this._rootFolderId || 'root',
        name: 'Root',
        mimeType: 'application/vnd.google-apps.folder',
    // Split path into parts
    const pathParts = normalizedPath.split('/').filter((p) => p);
    console.log('[GoogleDriveFileStorage] Path parts:', pathParts);
    // Start with root folder or the specified root folder
    let currentParentId = this._rootFolderId || 'root';
    let currentItem: drive_v3.Schema$File | null = null;
    // Navigate through path parts
      const isLastPart = i === pathParts.length - 1;
      // Escape single quotes in the part name for Google Drive query syntax
      const escapedPart = part.replace(/'/g, "\\'");
      // Query for the item
      const query = isLastPart
        ? `name = '${escapedPart}' and '${currentParentId}' in parents and trashed = false`
        : `name = '${escapedPart}' and '${currentParentId}' in parents and mimeType = 'application/vnd.google-apps.folder' and trashed = false`;
      console.log('[GoogleDriveFileStorage] Querying for part:', part, 'Query:', query);
      const response = await this._drive.files.list({
        fields: 'files(id, name, mimeType, size, modifiedTime, parents)',
        spaces: 'drive',
      console.log('[GoogleDriveFileStorage] Query result:', {
        part,
        filesFound: response.data.files?.length || 0,
        files: response.data.files?.map((f) => ({ id: f.id, name: f.name })),
      if (!response.data.files || response.data.files.length === 0) {
        throw new Error(`Path not found: ${path} (at part: ${part})`);
      currentItem = response.data.files[0];
      currentParentId = currentItem.id!;
    if (!currentItem) {
      throw new Error(`Path not found: ${path}`);
    console.log('[GoogleDriveFileStorage] Found item:', { id: currentItem.id, name: currentItem.name });
    return currentItem;
   * Finds a parent folder by path and creates it if it doesn't exist.
   * This helper method is used to ensure a folder path exists before
   * creating or moving files. It navigates through each path component,
   * creating folders as needed if they don't exist yet.
   * @param path - The path to the folder to find or create
   * @returns A Promise resolving to the ID of the folder
  private async _getOrCreateParentFolder(path: string): Promise<string> {
      return this._rootFolderId || 'root';
    const pathParts = path.split('/').filter((p) => p);
    // Navigate through path parts, creating folders as needed
      // Check if folder exists
      const query = `name = '${part}' and '${currentParentId}' in parents and mimeType = 'application/vnd.google-apps.folder' and trashed = false`;
        fields: 'files(id)',
      if (response.data.files && response.data.files.length > 0) {
        // Folder exists, use it
        currentParentId = response.data.files[0].id!;
        // Create the folder
        const folderMetadata = {
          name: part,
          parents: [currentParentId],
        const folder = await this._drive.files.create({
          requestBody: folderMetadata,
          fields: 'id',
        currentParentId = folder.data.id!;
    return currentParentId;
   * Helper method to convert Google Drive file objects to StorageObjectMetadata.
   * This method transforms the Google Drive API's file representation into
   * the standardized StorageObjectMetadata format used by the FileStorageBase
   * interface. It handles special properties like folder detection and paths.
   * @param file - The Google Drive file object to convert
   * @param parentPath - The parent path to use for constructing the full path
   * @returns A StorageObjectMetadata representation of the file
  private _fileToMetadata(file: drive_v3.Schema$File, parentPath: string = ''): StorageObjectMetadata {
    const isDirectory = file.mimeType === 'application/vnd.google-apps.folder';
    // Normalize parent path: remove trailing slash to avoid double slashes, handle root properly
    const normalizedParent = parentPath && parentPath !== '/' ? parentPath.replace(/\/+$/, '') : '';
    const fullPath = normalizedParent ? `${normalizedParent}/${file.name}` : file.name!;
      name: file.name!,
      path: parentPath,
      size: parseInt(file.size || '0'),
      contentType: file.mimeType || mime.lookup(file.name!) || 'application/octet-stream',
      lastModified: new Date(file.modifiedTime || Date.now()),
      etag: file.md5Checksum || undefined,
        fileId: file.id!,
   * Creates a pre-authenticated upload URL for an object in Google Drive.
   * Google Drive doesn't directly support pre-signed upload URLs in the same
   * way as other storage providers like S3 or Azure. Instead, uploads
   * should be performed using the PutObject method.
   * @param objectName - The name of the object to upload
   * @throws UnsupportedOperationError as this operation is not supported
    // Google Drive doesn't support pre-signed upload URLs in the same way as S3
    // Instead, use PutObject method for uploads
    this.throwUnsupportedOperationError('CreatePreAuthUploadUrl');
   * Creates a pre-authenticated download URL for an object in Google Drive.
   * This method creates a temporary, public sharing link for a file that allows
   * anyone with the link to access the file for a limited time (10 minutes).
   * It uses Google Drive's permissions system to create a temporary reader
   * permission for 'anyone' with an expiration time.
   * @param objectName - The path to the file to download
   * @throws Error if the file cannot be found or the URL creation fails
   * Map of Google Workspace MIME types to their export formats.
   * Google Workspace files must be exported to these formats for download.
  private static readonly GOOGLE_WORKSPACE_EXPORT_MAP: Record<string, { mimeType: string; format: string; urlBase: string }> = {
    'application/vnd.google-apps.document': {
      mimeType: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      format: 'docx',
      urlBase: 'https://docs.google.com/document/d',
    'application/vnd.google-apps.spreadsheet': {
      mimeType: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      format: 'xlsx',
      urlBase: 'https://docs.google.com/spreadsheets/d',
    'application/vnd.google-apps.presentation': {
      mimeType: 'application/vnd.openxmlformats-officedocument.presentationml.presentation',
      format: 'pptx',
      urlBase: 'https://docs.google.com/presentation/d',
    'application/vnd.google-apps.drawing': {
      format: 'png',
      urlBase: 'https://docs.google.com/drawings/d',
      // Get the file by path
      const file = await this._getItemByPath(objectName);
      if (!file.id) {
        throw new Error(`File not found: ${objectName}`);
      console.log(`[GoogleDrive] CreatePreAuthDownloadUrl for: ${objectName}, mimeType: ${file.mimeType}`);
      // Check if this is a Google Workspace file that needs export
      const exportInfo = GoogleDriveFileStorage.GOOGLE_WORKSPACE_EXPORT_MAP[file.mimeType!];
      if (exportInfo) {
        // Google Workspace file - return export URL
        // These URLs work without authentication if the file is shared
        const exportUrl = `${exportInfo.urlBase}/${file.id}/export?format=${exportInfo.format}`;
        console.log(`[GoogleDrive] Google Workspace file, using export URL: ${exportUrl}`);
        return exportUrl;
      // Check for other Google Workspace types that can't be exported
      if (file.mimeType?.startsWith('application/vnd.google-apps.')) {
        // For unsupported types (Forms, Sites, Maps, etc.), return the web view link
        console.log(`[GoogleDrive] Unsupported Google Workspace type: ${file.mimeType}, returning webViewLink`);
        const fileInfo = await this._drive.files.get({
          fileId: file.id,
          fields: 'webViewLink',
        if (fileInfo.data.webViewLink) {
          return fileInfo.data.webViewLink;
        throw new Error(`Cannot download Google Workspace file of type: ${file.mimeType}`);
      // Regular file - try to get direct download link
        fields: 'webViewLink, webContentLink',
      // webContentLink is the direct download link (only available for non-Google files)
      if (fileInfo.data.webContentLink) {
        console.log(`[GoogleDrive] Using webContentLink for direct download`);
        return fileInfo.data.webContentLink;
      // Try to create a public sharing permission to enable download
        await this._drive.permissions.create({
            role: 'reader',
            type: 'anyone',
        // Fetch updated file info
        const updatedFileInfo = await this._drive.files.get({
          fields: 'webContentLink',
        if (updatedFileInfo.data.webContentLink) {
          return updatedFileInfo.data.webContentLink;
      } catch (permError) {
        console.warn(`[GoogleDrive] Could not create public permission:`, permError);
      // Fallback to web view link
        console.log(`[GoogleDrive] Falling back to webViewLink`);
      throw new Error(`No download link available for: ${objectName}`);
      console.error('Error creating pre-auth download URL', { objectName, error: errorMessage });
      throw new Error(`Failed to create download URL for: ${objectName}. ${errorMessage}`);
   * Moves an object from one location to another within Google Drive.
   * This method first locates the file to be moved, then gets or creates the
   * destination folder, and finally updates the file's name and parent folder.
   * Google Drive has native support for moving files between folders.
   * @param oldObjectName - The current path of the object
   * @param newObjectName - The new path for the object
   * const success = await driveStorage.MoveObject(
      // Get the file to move
      const file = await this._getItemByPath(oldObjectName);
        throw new Error(`File not found: ${oldObjectName}`);
      // Parse new path to get parent folder and new name
      const newPathParts = newObjectName.split('/');
      const newName = newPathParts.pop() || '';
      const newParentPath = newPathParts.join('/');
      // Get or create the destination folder
      const newParentId = await this._getOrCreateParentFolder(newParentPath);
      // Move the file by updating parent and name
      await this._drive.files.update({
          name: newName,
      // Update parents (remove old parents and add new parent)
        removeParents: file.parents?.join(','),
        addParents: newParentId,
        fields: 'id, parents',
   * Deletes an object from Google Drive.
   * This method locates the specified file by path and deletes it from Google Drive.
   * By default, this moves the file to the trash rather than permanently deleting it,
   * unless your Drive settings are configured for immediate permanent deletion.
   * @param objectName - The path to the file to delete
   * const deleted = await driveStorage.DeleteObject('temp/report-draft.pdf');
      // Get the file to delete
        // If file doesn't exist, consider it success for idempotency
      // Delete the file (move to trash)
      await this._drive.files.delete({
      // If file not found, consider it success
      if (error.code === 404) {
   * Lists objects in a directory in Google Drive.
   * This method retrieves all files and folders directly inside the specified
   * folder path. It handles Google Drive's native folder structure and converts
   * the Drive API responses to the standardized StorageListResult format.
   * @param prefix - The path to the directory to list
   * @param delimiter - Delimiter character (unused in Google Drive implementation)
   * const result = await driveStorage.ListObjects('documents/');
      console.log('[GoogleDriveFileStorage] ListObjects called with prefix:', prefix);
      // Check if drive client is initialized
      if (!this._drive) {
        console.error('[GoogleDriveFileStorage] Drive client not initialized!');
        throw new Error('Google Drive client not initialized. Call initialize() first.');
      // Get the folder
      const folder = await this._getItemByPath(prefix);
      console.log('[GoogleDriveFileStorage] Got folder:', { id: folder.id, name: folder.name });
      if (!folder.id) {
        throw new Error(`Folder not found: ${prefix}`);
      // List files in the folder
        q: `'${folder.id}' in parents and trashed = false`,
      console.log('[GoogleDriveFileStorage] Got files response:', {
        fileCount: response.data.files?.length || 0,
        files: response.data.files?.map((f) => ({ name: f.name, mimeType: f.mimeType })),
        for (const file of response.data.files) {
          // Add to objects list
          objects.push(this._fileToMetadata(file, prefix));
          if (file.mimeType === 'application/vnd.google-apps.folder') {
            const folderPath = prefix ? (prefix.endsWith('/') ? `${prefix}${file.name}` : `${prefix}/${file.name}`) : file.name!;
      console.log('[GoogleDriveFileStorage] Returning:', { objectCount: objects.length, prefixCount: prefixes.length });
      console.error('[GoogleDriveFileStorage] Error listing objects', { prefix, error });
   * Creates a directory in Google Drive.
   * This method creates a folder at the specified path, creating parent
   * folders as needed if they don't exist. Google Drive natively supports
   * folders as a special file type with the 'application/vnd.google-apps.folder'
   * MIME type.
   * const created = await driveStorage.CreateDirectory('documents/reports/annual/');
      // Parse path
      const pathParts = normalizedPath.split('/');
      const folderName = pathParts.pop() || '';
      const parentPath = pathParts.join('/');
      // Get parent folder
      const parentId = await this._getOrCreateParentFolder(parentPath);
        parents: [parentId],
      await this._drive.files.create({
   * Deletes a directory and optionally its contents from Google Drive.
   * This method deletes a folder at the specified path. If recursive is false,
   * it will fail if the folder has any contents. If recursive is true, it
   * deletes the folder and all its contents.
   * const deleted = await driveStorage.DeleteDirectory('documents/temp/');
   * const recursivelyDeleted = await driveStorage.DeleteDirectory('documents/old_projects/', true);
      const folder = await this._getItemByPath(normalizedPath);
        // Check if folder is empty
      // Delete the folder
        fileId: folder.id,
   * Retrieves metadata for a specific object in Google Drive.
   * This method fetches the file information without downloading its content,
   * which is more efficient for checking file attributes like size, type,
   * @throws Error if the file doesn't exist or cannot be accessed
   *   // Fast path: Use objectId (Google Drive file ID)
   *   const metadata = await driveStorage.GetObjectMetadata({ objectId: '1a2b3c4d5e' });
   *   const metadata2 = await driveStorage.GetObjectMetadata({ fullPath: 'documents/report.pdf' });
      let file: drive_v3.Schema$File;
        const response = await this._drive.files.get({
          fileId: params.objectId,
          fields: 'id, name, mimeType, size, modifiedTime, createdTime, parents',
        file = response.data;
        // Slow path: Resolve path to file
        file = await this._getItemByPath(params.fullPath!);
          throw new Error(`File not found: ${params.fullPath}`);
        pathParts.pop(); // Remove filename
      return this._fileToMetadata(file, parentPath);
   * Downloads an object's content from Google Drive.
   * This method retrieves the full content of a file and returns it
   * as a Buffer for processing in memory.
   * @returns A Promise resolving to a Buffer containing the file's data
   *   const content = await driveStorage.GetObject({ objectId: '1a2b3c4d5e' });
   *   const content2 = await driveStorage.GetObject({ fullPath: 'documents/config.json' });
      let mimeType: string | undefined;
        // Need to get the mimeType to check if it's a Google Workspace file
          fields: 'mimeType',
        mimeType = fileInfo.data.mimeType || undefined;
        const file = await this._getItemByPath(params.fullPath!);
        fileId = file.id;
        mimeType = file.mimeType || undefined;
      const exportInfo = mimeType ? GoogleDriveFileStorage.GOOGLE_WORKSPACE_EXPORT_MAP[mimeType] : undefined;
        // Google Workspace file - use export instead of direct download
        console.log(`[GoogleDrive] Exporting Google Workspace file (${mimeType}) as ${exportInfo.format}`);
        const response = await this._drive.files.export(
            mimeType: exportInfo.mimeType,
        return Buffer.from(response.data as ArrayBuffer);
      if (mimeType?.startsWith('application/vnd.google-apps.')) {
        throw new Error(`Cannot download Google Workspace file of type: ${mimeType}. This file type does not support export.`);
      // Regular file - download directly
      const response = await this._drive.files.get(
          alt: 'media',
   * Uploads data to an object in Google Drive.
   * This method directly uploads a Buffer of data to a file with the specified path.
   * It will create any necessary parent folders if they don't exist, and will update
   * the file if it already exists or create a new one if it doesn't.
   * @param objectName - The path to the file to upload
   * @param contentType - Optional MIME type for the file (inferred from name if not provided)
   * @param metadata - Optional key-value pairs of custom metadata (not supported in current implementation)
   * const uploaded = await driveStorage.PutObject(
      const fileName = pathParts.pop() || '';
      // Get or create parent folder
      // Determine content type
      // Check if file already exists to decide whether to create or update
      let existingFileId: string | null = null;
        const existingFile = await this._getItemByPath(objectName);
        existingFileId = existingFile.id || null;
        // File doesn't exist, will create new
      if (existingFileId) {
        // Update existing file
          fileId: existingFileId,
            body: data,
            mimeType: effectiveContentType,
        // Create new file
   * Copies an object within Google Drive.
   * This method creates a copy of a file at a new location without removing the original.
   * It uses the Google Drive API's native file copying capabilities.
   * @param sourceObjectName - The path to the file to copy
   * @param destinationObjectName - The path where the copy should be created
   * const copied = await driveStorage.CopyObject(
      // Get source file
      const sourceFile = await this._getItemByPath(sourceObjectName);
      if (!sourceFile.id) {
        throw new Error(`Source file not found: ${sourceObjectName}`);
      // Parse destination path
      const destPathParts = destinationObjectName.split('/');
      const destFileName = destPathParts.pop() || '';
      const destParentPath = destPathParts.join('/');
      // Get or create destination parent folder
      const destParentId = await this._getOrCreateParentFolder(destParentPath);
      // Copy the file
      await this._drive.files.copy({
        fileId: sourceFile.id,
          name: destFileName,
          parents: [destParentId],
   * Checks if an object exists in Google Drive.
   * This method verifies the existence of a file at the specified path
   * without downloading its content.
   * @param objectName - The path to the file to check
   * @returns A Promise resolving to a boolean indicating if the file exists
   * const exists = await driveStorage.ObjectExists('documents/report.pdf');
   *   const content = await driveStorage.GetObject('documents/report.pdf');
      await this._getItemByPath(objectName);
   * Checks if a directory exists in Google Drive.
   * This method verifies the existence of a folder at the specified path.
   * It also checks that the item is actually a folder (has the correct MIME type),
   * not a file with the same name.
   * const exists = await driveStorage.DirectoryExists('documents/reports/');
   *   await driveStorage.CreateDirectory('documents/reports/');
   * await driveStorage.PutObject('documents/reports/new-report.pdf', fileData);
      const item = await this._getItemByPath(normalizedPath);
      return item.mimeType === 'application/vnd.google-apps.folder';
   * Searches for files in Google Drive using the Drive API search capabilities.
   * Google Drive search syntax supports:
   * - Simple terms: "report" matches files containing "report"
   * - Exact phrases: "quarterly report" matches that exact phrase
   * - Boolean OR: "budget OR forecast"
   * - Exclusion: "report -draft" excludes files with "draft"
   * - Wildcards: Not supported in Drive API
   * Content search is always enabled for supported file types (Docs, Sheets, PDFs, etc.)
   * when searchContent option is true.
   * @param query - Search query using Google Drive search syntax
   * @param options - Search options
   * @returns Promise resolving to search results
   * // Simple name search
   * // Content search
   *   pathPrefix: 'documents/research/'
    // Build the Google Drive query string
    const queryParts: string[] = [];
    // Add the user's search query (searches name and/or content)
    if (options?.searchContent) {
      queryParts.push(`fullText contains '${this._escapeQuery(query)}'`);
      queryParts.push(`name contains '${this._escapeQuery(query)}'`);
    // Add file type filter
      const mimeTypes = options.fileTypes.map((ft) => {
        // Convert extensions to MIME types if needed
        return ft.includes('/') ? ft : mime.lookup(ft) || ft;
      const mimeQuery = mimeTypes.map((mt) => `mimeType='${mt}'`).join(' or ');
      queryParts.push(`(${mimeQuery})`);
    if (options?.modifiedAfter) {
      queryParts.push(`modifiedTime > '${options.modifiedAfter.toISOString()}'`);
    if (options?.modifiedBefore) {
      queryParts.push(`modifiedTime < '${options.modifiedBefore.toISOString()}'`);
    // Add path prefix filter (parent folder)
        const folder = await this._getItemByPath(options.pathPrefix);
        parentFolderId = folder.id || undefined;
        // If path doesn't exist, throw error so caller knows search failed
        throw new Error(`Google Drive search failed - invalid pathPrefix: ${error instanceof Error ? error.message : String(error)}`);
    if (parentFolderId) {
      queryParts.push(`'${parentFolderId}' in parents`);
    // Exclude trashed files
    queryParts.push('trashed=false');
    // Add provider-specific options
    if (options?.providerSpecific) {
      for (const [key, value] of Object.entries(options.providerSpecific)) {
          queryParts.push(`${key}=${value}`);
          queryParts.push(`${key}='${value}'`);
    const finalQuery = queryParts.join(' and ');
        q: finalQuery,
        pageSize: maxResults,
        fields: 'nextPageToken, files(id, name, mimeType, size, modifiedTime, parents, properties)',
        orderBy: 'modifiedTime desc',
      const files = response.data.files || [];
        const path = await this._getFilePathFromId(file.id!);
        const pathParts = path.split('/');
        const fileName = pathParts.pop() || file.name!;
          contentType: file.mimeType!,
          lastModified: new Date(file.modifiedTime!),
          objectId: file.id || '', // Google Drive file ID for direct access
          matchInFilename: file.name!.toLowerCase().includes(query.toLowerCase()),
          customMetadata: file.properties as Record<string, string>,
          providerData: { driveFileId: file.id },
        totalMatches: undefined, // Drive API doesn't provide total count
        hasMore: !!response.data.nextPageToken,
        nextPageToken: response.data.nextPageToken || undefined,
      console.error('Error searching files in Google Drive', { query, options, error });
      throw new Error(`Google Drive search failed: ${error.message || 'Unknown error'}`);
   * Escapes special characters in search queries for Google Drive.
  private _escapeQuery(query: string): string {
    return query.replace(/'/g, "\\'").replace(/\\/g, '\\\\');
   * Gets the full path of a file from its ID by traversing up the parent chain.
  private async _getFilePathFromId(fileId: string): Promise<string> {
    let currentId: string | undefined = fileId;
    while (currentId && currentId !== 'root' && currentId !== this._rootFolderId) {
      const file = await this._drive.files.get({
        fileId: currentId,
        fields: 'id, name, parents',
      if (file.data.name) {
        pathParts.unshift(file.data.name);
      currentId = file.data.parents?.[0];
      // Stop if we've reached root or the configured root folder
      if (currentId === 'root' || currentId === this._rootFolderId) {
