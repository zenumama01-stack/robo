 * Client for file storage operations through GraphQL.
 * This class provides an easy way to interact with file storage accounts from a client application.
 * All operations use accountId (FileStorageAccount ID) as the primary identifier,
 * supporting the enterprise model where storage accounts are organizational resources.
 * const storageClient = new GraphQLFileStorageClient(graphQLProvider);
 * // List objects in a directory
 * const objects = await storageClient.ListObjects(accountId, 'documents/');
 * // Create a pre-authenticated upload URL
 * const uploadResult = await storageClient.CreatePreAuthUploadUrl(
 *   accountId,
 *   'documents/report.pdf',
 *   'application/pdf'
export class GraphQLFileStorageClient {
     * Creates a new GraphQLFileStorageClient instance.
    // Navigation / Directory Operations
     * List objects in a storage account at the specified path.
     * @param accountId The ID of the FileStorageAccount
     * @param prefix The path prefix to list objects from (e.g., 'documents/')
     * @param delimiter Optional delimiter for grouping results (default: '/')
     * @returns A Promise that resolves to a StorageListResult
     * const result = await storageClient.ListObjects(accountId, 'documents/', '/');
     * console.log('Files:', result.objects.filter(o => !o.isDirectory));
     * console.log('Folders:', result.prefixes);
    public async ListObjects(
        accountId: string,
        prefix: string = '',
        delimiter?: string
    ): Promise<StorageListResult> {
                query ListStorageObjects($input: ListStorageObjectsInput!) {
                    ListStorageObjects(input: $input) {
                        objects {
                            path
                            fullPath
                            size
                            contentType
                            lastModified
                            isDirectory
                            etag
                            cacheControl
                        prefixes
                    AccountID: accountId,
                    Prefix: prefix,
                    Delimiter: delimiter || '/'
            if (!result?.ListStorageObjects) {
                objects: result.ListStorageObjects.objects.map((obj: StorageObjectMetadataResponse) => ({
                    ...obj,
                    lastModified: new Date(obj.lastModified)
                prefixes: result.ListStorageObjects.prefixes
            LogError(`Error listing storage objects: ${error}`);
     * Check if a directory exists in the storage account.
     * @param path The directory path to check
     * @returns A Promise that resolves to true if the directory exists
    public async DirectoryExists(
        path: string
            // Check by listing with the path as prefix and looking for any results
            const result = await this.ListObjects(accountId, path.endsWith('/') ? path : `${path}/`, '/');
            return result.objects.length > 0 || result.prefixes.length > 0;
            LogError(`Error checking directory existence: ${error}`);
     * Create a directory in the storage account.
     * @param path The directory path to create
     * @returns A Promise that resolves to true if the directory was created successfully
    public async CreateDirectory(
                mutation CreateDirectory($input: CreateDirectoryInput!) {
                    CreateDirectory(input: $input)
                    Path: path
            return result?.CreateDirectory ?? false;
            LogError(`Error creating directory: ${error}`);
    // File Operations
     * Check if an object exists in the storage account.
     * @param objectName The name/path of the object to check
     * @returns A Promise that resolves to true if the object exists
    public async ObjectExists(
        objectName: string
            // Try to list the exact object
            const parentPath = objectName.substring(0, objectName.lastIndexOf('/') + 1);
            const fileName = objectName.substring(objectName.lastIndexOf('/') + 1);
            const result = await this.ListObjects(accountId, parentPath, '/');
            return result.objects.some(obj => obj.name === fileName || obj.fullPath === objectName);
            LogError(`Error checking object existence: ${error}`);
     * Create a pre-authenticated URL for uploading a file.
     * @param objectName The name/path of the object to upload
     * @param contentType Optional content type for the file
     * @returns A Promise that resolves to the upload URL and provider key
     * const result = await storageClient.CreatePreAuthUploadUrl(
     * // Use the upload URL to upload the file
     * await fetch(result.uploadUrl, {
     *   method: 'PUT',
     *   body: fileContent,
     *   headers: { 'Content-Type': 'application/pdf' }
    public async CreatePreAuthUploadUrl(
        contentType?: string
    ): Promise<CreatePreAuthUploadUrlResult> {
                mutation CreatePreAuthUploadUrl($input: CreatePreAuthUploadUrlInput!) {
                    CreatePreAuthUploadUrl(input: $input) {
                    ObjectName: objectName,
                    ContentType: contentType
            if (!result?.CreatePreAuthUploadUrl) {
                uploadUrl: result.CreatePreAuthUploadUrl.UploadUrl,
                providerKey: result.CreatePreAuthUploadUrl.ProviderKey
            LogError(`Error creating pre-auth upload URL: ${error}`);
     * Create a pre-authenticated URL for downloading a file.
     * @param objectName The name/path of the object to download
     * @returns A Promise that resolves to the download URL
     * const downloadUrl = await storageClient.CreatePreAuthDownloadUrl(
     *   'documents/report.pdf'
     * // Use the download URL
     * window.open(downloadUrl, '_blank');
    public async CreatePreAuthDownloadUrl(
                query CreatePreAuthDownloadUrl($input: CreatePreAuthDownloadUrlInput!) {
                    CreatePreAuthDownloadUrl(input: $input)
                    ObjectName: objectName
            if (result?.CreatePreAuthDownloadUrl === undefined) {
            return result.CreatePreAuthDownloadUrl;
            LogError(`Error creating pre-auth download URL: ${error}`);
     * Delete an object from the storage account.
     * @param objectName The name/path of the object to delete
     * @returns A Promise that resolves to true if the object was deleted successfully
    public async DeleteObject(
                mutation DeleteStorageObject($input: DeleteStorageObjectInput!) {
                    DeleteStorageObject(input: $input)
            return result?.DeleteStorageObject ?? false;
            LogError(`Error deleting storage object: ${error}`);
     * Move/rename an object within the storage account.
     * @param oldName The current name/path of the object
     * @param newName The new name/path for the object
     * @returns A Promise that resolves to true if the object was moved successfully
    public async MoveObject(
        oldName: string,
        newName: string
                mutation MoveStorageObject($input: MoveStorageObjectInput!) {
                    MoveStorageObject(input: $input)
                    OldName: oldName,
                    NewName: newName
            return result?.MoveStorageObject ?? false;
            LogError(`Error moving storage object: ${error}`);
     * Copy an object within the storage account.
     * @param sourceName The source name/path of the object
     * @param destinationName The destination name/path for the copy
     * @returns A Promise that resolves to true if the object was copied successfully
    public async CopyObject(
        sourceName: string,
        destinationName: string
                mutation CopyStorageObject($input: CopyStorageObjectInput!) {
                    CopyStorageObject(input: $input)
                    SourceName: sourceName,
                    DestinationName: destinationName
            return result?.CopyStorageObject ?? false;
            LogError(`Error copying storage object: ${error}`);
     * Copy an object between two different storage accounts.
     * @param sourceAccountId The ID of the source FileStorageAccount
     * @param destinationAccountId The ID of the destination FileStorageAccount
     * @param sourcePath The source path of the object
     * @param destinationPath The destination path for the copy
     * @returns A Promise that resolves to the copy result
    public async CopyObjectBetweenAccounts(
        sourceAccountId: string,
        destinationAccountId: string,
        sourcePath: string,
        destinationPath: string
    ): Promise<CopyBetweenAccountsResult> {
                mutation CopyObjectBetweenAccounts($input: CopyObjectBetweenAccountsInput!) {
                    CopyObjectBetweenAccounts(input: $input) {
                        bytesTransferred
                        sourceAccount
                        destinationAccount
                        sourcePath
                        destinationPath
                    SourceAccountID: sourceAccountId,
                    DestinationAccountID: destinationAccountId,
                    SourcePath: sourcePath,
                    DestinationPath: destinationPath
            if (!result?.CopyObjectBetweenAccounts) {
                success: result.CopyObjectBetweenAccounts.success,
                message: result.CopyObjectBetweenAccounts.message,
                bytesTransferred: result.CopyObjectBetweenAccounts.bytesTransferred,
                sourceAccount: result.CopyObjectBetweenAccounts.sourceAccount,
                destinationAccount: result.CopyObjectBetweenAccounts.destinationAccount,
                sourcePath: result.CopyObjectBetweenAccounts.sourcePath,
                destinationPath: result.CopyObjectBetweenAccounts.destinationPath
            LogError(`Error copying object between accounts: ${error}`);
                message: error.message,
                sourceAccount: '',
                destinationAccount: '',
    // Search Operations
     * Search for files across one or more storage accounts.
     * @param accountIds Array of FileStorageAccount IDs to search
     * @param query The search query
     * @param options Optional search options
     * @returns A Promise that resolves to the search results
     * const results = await storageClient.SearchFiles(
     *   [accountId1, accountId2],
     *   'quarterly report',
     *     maxResultsPerAccount: 10,
     *     fileTypes: ['pdf', 'docx'],
     *     searchContent: true
     * for (const accountResult of results.accountResults) {
     *   console.log(`Results from ${accountResult.accountName}:`);
     *   for (const file of accountResult.results) {
     *     console.log(`  - ${file.name} (${file.relevance})`);
    public async SearchFiles(
        accountIds: string[],
        searchQuery: string,
        options?: FileSearchOptions
    ): Promise<SearchAcrossAccountsResult> {
            const gqlQuery = gql`
                query SearchAcrossAccounts($input: SearchAcrossAccountsInput!) {
                    SearchAcrossAccounts(input: $input) {
                        accountResults {
                            accountID
                            accountName
                                relevance
                                excerpt
                                matchInFilename
                                objectId
                            totalMatches
                            hasMore
                            nextPageToken
                        totalResultsReturned
                        successfulAccounts
                        failedAccounts
                    AccountIDs: accountIds,
                    Query: searchQuery,
                    MaxResultsPerAccount: options?.maxResultsPerAccount,
                    FileTypes: options?.fileTypes,
                    SearchContent: options?.searchContent
            const result = await this._dataProvider.ExecuteGQL(gqlQuery, variables);
            if (!result?.SearchAcrossAccounts) {
            const searchResult = result.SearchAcrossAccounts;
                accountResults: searchResult.accountResults.map((ar: AccountSearchResultResponse) => ({
                    accountId: ar.accountID,
                    results: ar.results.map((r: FileSearchResultResponse) => ({
                        lastModified: new Date(r.lastModified),
            LogError(`Error searching across accounts: ${error}`);
                accountResults: [],
                totalResultsReturned: 0,
                successfulAccounts: 0,
                failedAccounts: accountIds.length
// Type Definitions
 * Metadata for a storage object
export interface StorageObjectMetadata {
    /** The name of the object (filename) */
    /** The path to the object (directory) */
    /** The full path including name */
    /** Size in bytes */
    /** MIME content type */
    /** Last modification date */
    /** Whether this is a directory */
    isDirectory: boolean;
    /** ETag for caching */
    /** Cache control header */
    cacheControl?: string;
 * Response type for storage object metadata (with string date)
interface StorageObjectMetadataResponse {
 * Result from listing storage objects
export interface StorageListResult {
    /** Array of objects in the directory */
    objects: StorageObjectMetadata[];
    /** Array of subdirectory prefixes */
    prefixes: string[];
 * Result from creating a pre-authenticated upload URL
export interface CreatePreAuthUploadUrlResult {
    /** The URL to use for uploading */
    uploadUrl: string;
    /** The provider-specific key for the object */
    providerKey?: string;
 * Result from copying an object between accounts
export interface CopyBetweenAccountsResult {
    /** Whether the copy was successful */
    /** Human-readable message */
    /** Number of bytes transferred */
    bytesTransferred?: number;
    /** Name of the source account */
    sourceAccount: string;
    /** Name of the destination account */
    destinationAccount: string;
    /** Source path */
    sourcePath: string;
    /** Destination path */
    destinationPath: string;
 * Options for file search operations
export interface FileSearchOptions {
    /** Maximum results per account */
    maxResultsPerAccount?: number;
    /** Filter by file types (extensions) */
    fileTypes?: string[];
    /** Whether to search file content (not just names) */
    searchContent?: boolean;
 * A single file search result
export interface FileSearchResult {
    /** Path to the file */
    /** File name */
    /** Relevance score (0-1) */
    /** Text excerpt showing the match */
    /** Whether the match was in the filename */
    /** Provider-specific object ID */
 * Response type for file search result (with string date)
interface FileSearchResultResponse {
 * Search results for a single account
    /** The account ID */
    /** The account name */
    /** Whether the search was successful */
    /** Error message if search failed */
    /** Search results */
    results: FileSearchResult[];
    /** Total matches found */
    /** Whether there are more results */
    /** Token for pagination */
 * Response type for account search result (with string dates in results)
interface AccountSearchResultResponse {
    results: FileSearchResultResponse[];
 * Result from searching across multiple accounts
export interface SearchAcrossAccountsResult {
    /** Results grouped by account */
    /** Total results returned across all accounts */
    /** Number of accounts that were searched successfully */
    /** Number of accounts that failed to search */
