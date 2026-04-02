import { GetSignedUrlConfig, Storage } from '@google-cloud/storage';
 * Google Cloud Storage implementation of the FileStorageBase interface.
 * This class provides methods for interacting with Google Cloud Storage as a file storage provider.
 * It implements all the abstract methods defined in FileStorageBase and handles Google-specific
 * - STORAGE_GOOGLE_KEY_JSON: A JSON object containing Google Cloud service account credentials
 * - STORAGE_GOOGLE_BUCKET_NAME: The GCS bucket name
 * // Create an instance of GoogleFileStorage
 * const gcsStorage = new GoogleFileStorage();
 * const { UploadUrl } = await gcsStorage.CreatePreAuthUploadUrl('documents/report.pdf');
 * const downloadUrl = await gcsStorage.CreatePreAuthDownloadUrl('documents/report.pdf');
 * const files = await gcsStorage.ListObjects('documents/');
@RegisterClass(FileStorageBase, 'Google Cloud Storage')
export class GoogleFileStorage extends FileStorageBase {
  protected readonly providerName = 'Google Cloud Storage';
  /** The GCS bucket name */
  /** The Google Cloud Storage client instance */
  private _client: Storage;
   * Creates a new instance of GoogleFileStorage.
   * Initializes the connection to Google Cloud Storage using environment variables.
    const config = getProviderConfig('googleCloud');
    // Handle credentials from config or env vars
    let credentials;
    if (config?.keyJSON) {
      // If keyJSON is a string, parse it
      credentials = typeof config.keyJSON === 'string' ? JSON.parse(config.keyJSON) : config.keyJSON;
    } else if (config?.keyFilename) {
      // If keyFilename is provided, use it
      this._client = new Storage({ keyFilename: config.keyFilename });
      this._bucket = config?.defaultBucket || env.get('STORAGE_GOOGLE_BUCKET_NAME').required().asString();
      // Fall back to env vars
      credentials = env.get('STORAGE_GOOGLE_KEY_JSON').required().asJsonObject();
    // Initialize with credentials
    const storageOptions: { credentials?: object; projectId?: string } = { credentials };
    if (config?.projectID) {
      storageOptions.projectId = config.projectID;
    this._client = new Storage(storageOptions);
   * Initialize Google Cloud Storage provider.
   * const storage = new GoogleFileStorage(); // Constructor loads env vars
   * const storage = new GoogleFileStorage();
   *   accountName: 'GCS Account',
   *   keyJSON: '{"type":"service_account",...}',
   *   projectID: 'my-project',
   *   defaultBucket: 'my-bucket'
    const gcsConfig = config as any;
    if (gcsConfig.keyJSON) {
      credentials = typeof gcsConfig.keyJSON === 'string' ? JSON.parse(gcsConfig.keyJSON) : gcsConfig.keyJSON;
    } else if (gcsConfig.keyFilename) {
      this._client = new Storage({ keyFilename: gcsConfig.keyFilename });
      if (gcsConfig.defaultBucket) {
        this._bucket = gcsConfig.defaultBucket;
    if (credentials) {
      if (gcsConfig.projectID) {
        storageOptions.projectId = gcsConfig.projectID;
   * Checks if Google Cloud Storage provider is properly configured.
   * Returns true if service account credentials and bucket name are present.
    const hasClient = !!this._client;
    const hasBucket = !!this._bucket;
    const isConfigured = hasClient && hasBucket;
      if (!hasClient) missing.push('Service Account Credentials');
        `❌ Google Cloud Storage provider not configured. Missing: ${missing.join(', ')}\n\n` +
        `  export STORAGE_GOOGLE_KEY_JSON='{"type":"service_account",...}'\n` +
        `  export STORAGE_GOOGLE_BUCKET_NAME="my-bucket"\n` +
        `  const storage = new GoogleFileStorage();\n` +
        `    keyJSON: '{"type":"service_account",...}',\n` +
        `    projectID: "my-project",\n` +
   * Normalizes directory paths to ensure they end with a slash.
   * directory path representation. Google Cloud Storage doesn't have actual
   * Converts metadata to a string map for consistent handling.
   * This is a helper method used internally to ensure that all metadata
   * values are strings, as required by the StorageObjectMetadata type.
   * @param metadata - The metadata object to convert
   * @returns A record with string keys and string values
  private _convertMetadataToStringMap(metadata: any): Record<string, string> {
    if (!metadata) return {};
    const result: Record<string, string> = {};
    for (const key in metadata) {
      result[key] = String(metadata[key]);
   * Creates a pre-authenticated upload URL for an object in Google Cloud Storage.
   * This method generates a signed URL that allows for uploading an object
   * to GCS without needing Google Cloud credentials. The URL is valid for
   * 10 minutes and includes the content type based on the file extension.
    const file = this._client.bucket(this._bucket).file(objectName);
      version: 'v4',
      action: 'write',
      expires: Date.now() + 10 * 60 * 1000, // 10 mins
      contentType: mime.lookup(objectName) || 'application/octet-stream',
    const [UploadUrl] = await file.getSignedUrl(options);
    return { UploadUrl };
   * Creates a pre-authenticated download URL for an object in Google Cloud Storage.
   * This method generates a signed URL that allows for downloading an object
   * from GCS without needing Google Cloud credentials. The URL is valid for
   * 10 minutes and can be shared with clients.
      action: 'read',
    } as GetSignedUrlConfig;
    const [url] = await file.getSignedUrl(options);
   * Moves an object from one location to another within Google Cloud Storage.
   * Unlike some other storage providers, GCS has a native rename operation
   * that can be used to efficiently move objects without needing to copy
   * and delete. This method leverages that capability.
   * const success = await gcsStorage.MoveObject(
      const response = await this._client.bucket(this._bucket).file(oldObjectName).rename(newObjectName);
      console.error('Error renaming file in Google storage', { oldObjectName, newObjectName, bucket: this._bucket });
   * Deletes an object from Google Cloud Storage.
   * This method attempts to delete the specified object. It uses the
   * ignoreNotFound option to ensure it returns true even if the object
   * didn't exist.
   * const deleted = await gcsStorage.DeleteObject('temp/report-draft.pdf');
      await this._client.bucket(this._bucket).file(objectName).delete({ ignoreNotFound: true });
      return true; // ignoreNotFound ensures this returns true even if the file doesn't exist
      console.error('Error deleting object from Google storage', { file: objectName, bucket: this._bucket });
   * Lists objects with the specified prefix in Google Cloud Storage.
   * This method returns a list of objects (files) and prefixes (directories)
   * under the specified path prefix. It uses the GCS getFiles API which
   * supports delimiter-based hierarchy simulation.
   * Note: This implementation fetches metadata for each file, which can be
   * inefficient for directories with many files. In a production environment,
   * you might want to optimize this for large directories.
   * const result = await gcsStorage.ListObjects('documents/');
      const [files, , apiResponse] = await this._client.bucket(this._bucket).getFiles(options);
      let prefixes: string[] = [];
      // Process files (objects)
        const [metadata] = await file.getMetadata();
        // Skip directory placeholders when listing their contents
        if (file.name === prefix) continue;
        const pathParts = file.name.split('/');
          fullPath: file.name,
          size: parseInt(String(metadata.size)) || 0,
          contentType: metadata.contentType || mime.lookup(file.name) || 'application/octet-stream',
          lastModified: new Date(metadata.updated),
          isDirectory: file.name.endsWith('/'),
          etag: String(metadata.etag),
          cacheControl: String(metadata.cacheControl || ''),
          customMetadata: this._convertMetadataToStringMap(metadata.metadata),
      // Extract directory prefixes
      if (apiResponse && (apiResponse as any).prefixes) {
        prefixes = (apiResponse as any).prefixes;
      console.error('Error listing objects in Google storage', { prefix, bucket: this._bucket });
   * Creates a directory (virtual) in Google Cloud Storage.
   * Since GCS doesn't have a native directory concept, this method creates
   * const created = await gcsStorage.CreateDirectory('documents/reports/annual/');
      // GCS doesn't have real directories, so we create a zero-byte file with the directory name
      const file = this._client.bucket(this._bucket).file(directoryPath);
      await file.save('', {
          contentType: 'application/x-directory',
      console.error('Error creating directory in Google storage', { directoryPath, bucket: this._bucket });
   * Deletes a directory (virtual) and optionally its contents from Google Cloud Storage.
   * the directory path as prefix and deletes them in parallel.
   * const deleted = await gcsStorage.DeleteDirectory('documents/temp/');
   * const recursivelyDeleted = await gcsStorage.DeleteDirectory('documents/old_projects/', true);
      // For recursive delete, list all files under this prefix and delete them
      const [files] = await this._client.bucket(this._bucket).getFiles(options);
      // Delete all files concurrently
      const deletePromises = files.map((file) => file.delete());
      console.error('Error deleting directory from Google storage', { directoryPath, recursive, bucket: this._bucket });
   * Retrieves metadata for a specific object in Google Cloud Storage.
   * This method fetches the properties of an object without downloading its content,
   * @param params - Object identifier (objectId and fullPath are equivalent for GCS)
   *   // For GCS, objectId and fullPath are the same (both are the object name)
   *   const metadata = await gcsStorage.GetObjectMetadata({ fullPath: 'documents/report.pdf' });
   *   const metadata2 = await gcsStorage.GetObjectMetadata({ objectId: 'documents/report.pdf' });
    // For GCS, objectId and fullPath are the same (both are the object name/path)
        contentType: metadata.contentType || mime.lookup(objectName) || 'application/octet-stream',
        etag: String(metadata.etag || ''),
      console.error('Error getting object metadata from Google storage', { objectName, bucket: this._bucket });
   * Downloads an object's content from Google Cloud Storage.
   * This method retrieves the full content of an object and returns it
   *   const content = await gcsStorage.GetObject({ fullPath: 'documents/config.json' });
   *   const content2 = await gcsStorage.GetObject({ objectId: 'documents/config.json' });
      const [bufferContent] = await file.download();
      return bufferContent;
      console.error('Error getting object from Google storage', { objectName, bucket: this._bucket });
   * Uploads data to an object in Google Cloud Storage.
   * const uploaded = await gcsStorage.PutObject(
      await file.save(data, {
          contentType: effectiveContentType,
      console.error('Error putting object to Google storage', { objectName, bucket: this._bucket });
   * Copies an object within Google Cloud Storage.
   * It uses the GCS copy API, which allows for efficient copying within the same bucket.
   * const copied = await gcsStorage.CopyObject(
      const sourceFile = this._client.bucket(this._bucket).file(sourceObjectName);
      const destinationFile = this._client.bucket(this._bucket).file(destinationObjectName);
      await sourceFile.copy(destinationFile);
      console.error('Error copying object in Google storage', {
   * Checks if an object exists in Google Cloud Storage.
   * This method verifies the existence of an object without downloading
   * its content. This is efficient for validation purposes.
   * const exists = await gcsStorage.ObjectExists('documents/report.pdf');
   *   const content = await gcsStorage.GetObject('documents/report.pdf');
      const [exists] = await file.exists();
      console.error('Error checking if object exists in Google storage', { objectName, bucket: this._bucket });
   * Checks if a directory (virtual) exists in Google Cloud Storage.
   * Since GCS doesn't have a native directory concept, this method checks for either:
   * const exists = await gcsStorage.DirectoryExists('documents/reports/');
   *   await gcsStorage.CreateDirectory('documents/reports/');
   * await gcsStorage.PutObject('documents/reports/new-report.pdf', fileData);
      // Method 1: Check if directory placeholder exists
      // Method 2: Check if any objects with this prefix exist
        maxResults: 1,
      return files.length > 0;
      console.error('Error checking if directory exists in Google storage', { directoryPath, bucket: this._bucket });
   * Search is not supported by Google Cloud Storage.
   * GCS is an object storage service without built-in search capabilities.
   * To search GCS objects, consider:
   * - Using BigQuery to query GCS data
   * - Maintaining a separate search index (Elasticsearch, etc.)
   * - Using object metadata for filtering with ListObjects
   * - Using Cloud Data Loss Prevention API for content discovery
