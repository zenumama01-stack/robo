    FileSearchOptions,
    FileSearchResult,
    UnsupportedOperationError
} from "@memberjunction/storage";
 * Action that searches for files across configured storage providers
 * using each provider's native search capabilities.
 * This action leverages MJStorage's SearchFiles method which uses:
 * - Google Drive: Drive API search with content indexing
 * - SharePoint: Microsoft Graph Search with KQL queries
 * - Dropbox: Search API v2 with filename and content search
 * - Box: Box Search API with Boolean operators
 * Providers without native search (AWS S3, Azure Blob, GCS) will return errors.
 * // Basic file search
 *   ActionName: 'Search Storage Files',
 *     Value: 'quarterly report'
 *     Value: 'Google Drive - Marketing'
 * // Advanced search with filters
 *     Value: 'budget 2024'
 *     Value: 'SharePoint - Finance'
 *     Name: 'FileTypes',
 *     Value: 'pdf,xlsx'
 *     Name: 'PathPrefix',
 *     Value: 'documents/finance'
 *     Name: 'SearchContent',
 *     Value: 50
@RegisterClass(BaseAction, "Search Storage Files")
export class SearchStorageFilesAction extends BaseFileStorageAction {
        // Required parameters
            return this.createErrorResult("Query parameter is required", "MISSING_QUERY");
        // Get storage driver (uses StorageAccount parameter via base class)
        // Optional parameters
        const maxResults = this.getNumericParam(params, "maxresults", 100);
        const pathPrefix = this.getStringParam(params, "pathprefix");
        const searchContent = this.getBooleanParam(params, "searchcontent", false);
        const modifiedAfterStr = this.getStringParam(params, "modifiedafter");
        const modifiedBeforeStr = this.getStringParam(params, "modifiedbefore");
        // Parse file types (comma-separated)
        const fileTypesParam = this.getStringParam(params, "filetypes");
        const fileTypes = fileTypesParam
            ? fileTypesParam.split(',').map(ft => ft.trim()).filter(ft => ft.length > 0)
        // Parse dates
        let modifiedAfter: Date | undefined;
        let modifiedBefore: Date | undefined;
        if (modifiedAfterStr) {
            const date = new Date(modifiedAfterStr);
                modifiedAfter = date;
                    `Invalid ModifiedAfter date format: ${modifiedAfterStr}. Use ISO 8601 format (e.g., '2024-01-01')`,
                    "INVALID_DATE_FORMAT"
        if (modifiedBeforeStr) {
            const date = new Date(modifiedBeforeStr);
                modifiedBefore = date;
                    `Invalid ModifiedBefore date format: ${modifiedBeforeStr}. Use ISO 8601 format (e.g., '2024-12-31')`,
            // Build search options
            const searchOptions: FileSearchOptions = {
                searchContent
            if (fileTypes) {
                searchOptions.fileTypes = fileTypes;
            if (pathPrefix) {
                searchOptions.pathPrefix = pathPrefix;
            if (modifiedAfter) {
                searchOptions.modifiedAfter = modifiedAfter;
            if (modifiedBefore) {
                searchOptions.modifiedBefore = modifiedBefore;
            const searchResults = await driver!.SearchFiles(query, searchOptions);
            // Format results for output
            const formattedResults = searchResults.results.map((file: FileSearchResult) => ({
                Path: file.path,
                Name: file.name,
                Size: file.size,
                ContentType: file.contentType,
                LastModified: file.lastModified.toISOString(),
                ObjectID: file.objectId,  // Provider-specific ID for fast direct access
                Relevance: file.relevance,
                Excerpt: file.excerpt,
                MatchInFilename: file.matchInFilename,
                CustomMetadata: file.customMetadata,
                ProviderData: file.providerData
            // Build output parameters array per ActionResultSimple spec
                    Name: 'SearchResults',
                    Value: formattedResults,
                    Name: 'ResultCount',
                    Value: searchResults.results.length,
                    Name: 'TotalMatches',
                    Value: searchResults.totalMatches,
                    Value: searchResults.hasMore,
                    Value: searchResults.nextPageToken,
                Message: `Found ${searchResults.results.length} file(s) matching query '${query}'`,
            // Handle specific error types
            if (error instanceof UnsupportedOperationError) {
                    `Storage provider does not support file search. Providers with search support: Google Drive, SharePoint, Dropbox, Box. Consider using a different provider or List Objects action with filtering.`,
                    "SEARCH_NOT_SUPPORTED"
            // Handle other errors
                `Search failed: ${errorMessage}`,
                "SEARCH_FAILED"
