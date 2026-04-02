interface GoogleSearchItem {
    displayLink?: string;
    snippet?: string;
    htmlSnippet?: string;
    formattedUrl?: string;
    mime?: string;
    fileFormat?: string;
    pagemap?: Record<string, unknown>;
interface GoogleSearchResponse {
    items?: GoogleSearchItem[];
    searchInformation?: {
        searchTime?: number;
        formattedSearchTime?: string;
        totalResults?: string;
        formattedTotalResults?: string;
    queries?: Record<string, unknown>;
    context?: Record<string, unknown>;
 * Action that performs web search using the Google Custom Search JSON API
 * and returns structured results.
 * // Basic Google search
 *   ActionName: 'Google Custom Search',
 *     Value: 'latest trends in renewable energy'
 * // Search with additional options
 *     Value: 'AI regulation whitepapers'
 *     Name: 'SafeSearch',
 *     Value: 'high'
 *     Name: 'SiteSearch',
 *     Value: 'europa.eu'
 *     Name: 'Verbosity',
 *     Value: 'minimal'  // 'minimal' | 'standard' (default) | 'detailed'
@RegisterClass(BaseAction, "Google Custom Search")
export class GoogleCustomSearchAction extends BaseAction {
        const query = this.getStringParam(params, "query") || this.getStringParam(params, "searchterms");
        const apiKey = apiConfig.google?.customSearch?.apiKey;
        const cx = apiConfig.google?.customSearch?.cx;
                "Google Custom Search API key not found. Set google.customSearch.apiKey in mj.config.cjs or GOOGLE_CUSTOM_SEARCH_API_KEY environment variable",
        if (!cx) {
                "Google Custom Search engine identifier (CX) not found. Set google.customSearch.cx in mj.config.cjs or GOOGLE_CUSTOM_SEARCH_CX environment variable",
                "MISSING_SEARCH_ENGINE"
        const maxResults = this.clamp(this.getNumericParam(params, "maxresults", 10), 1, 10);
        let startIndex = this.getNumericParam(params, "startindex", 1);
        if (isNaN(startIndex) || startIndex < 1) {
            startIndex = 1;
        // Google Custom Search limits the index to the first 100 results
        const maxStartIndex = Math.max(1, 101 - maxResults);
        if (startIndex > maxStartIndex) {
            startIndex = maxStartIndex;
        const safeSearch = this.normalizeSafeSearch(this.getStringParam(params, "safesearch"));
        const siteSearch = this.getStringParam(params, "sitesearch");
        const siteSearchFilter = this.normalizeSiteSearchFilter(this.getStringParam(params, "sitesearchfilter"));
        const languageRestriction = this.normalizeLanguageRestriction(this.getStringParam(params, "languagerestriction") || this.getStringParam(params, "language"));
        const exactTerms = this.getStringParam(params, "exactterms");
        const excludeTerms = this.getStringParam(params, "excludeterms");
        const dateRestrict = this.getStringParam(params, "daterestrict");
        const searchType = this.normalizeSearchType(this.getStringParam(params, "searchtype"));
        const fileType = this.getStringParam(params, "filetype");
        const region = this.getStringParam(params, "region") || this.getStringParam(params, "gl");
        const verbosity = this.normalizeVerbosity(this.getStringParam(params, "verbosity"));
        const requestParams: Record<string, string | number> = {
            key: apiKey,
            cx,
            q: query,
            num: maxResults,
            start: startIndex
        if (safeSearch) {
            requestParams.safe = safeSearch;
        if (siteSearch) {
            requestParams.siteSearch = siteSearch;
        if (siteSearchFilter) {
            requestParams.siteSearchFilter = siteSearchFilter;
        if (languageRestriction) {
            requestParams.lr = languageRestriction;
        if (exactTerms) {
            requestParams.exactTerms = exactTerms;
        if (excludeTerms) {
            requestParams.excludeTerms = excludeTerms;
        if (dateRestrict) {
            requestParams.dateRestrict = dateRestrict;
        if (searchType) {
            requestParams.searchType = searchType;
        if (fileType) {
            requestParams.fileType = fileType;
        if (region) {
            requestParams.gl = region.toLowerCase();
            const response = await axios.get<GoogleSearchResponse>(
                "https://www.googleapis.com/customsearch/v1",
                    params: requestParams,
                    timeout: 15000
            if (!response.data) {
                return this.createErrorResult("Empty response from Google Custom Search API", "EMPTY_RESPONSE");
            const items = (data.items || []).map(item => this.transformItemByVerbosity(item, verbosity));
            const totalResults = Number(data.searchInformation?.totalResults || "0");
            const resultData = this.buildResultData(data, items, verbosity, {
                startIndex,
                totalResults
            this.addOutputParam(params, "SearchResultsDetails", resultData);
            this.addOutputParam(params, "Items", items);
            this.addOutputParam(params, "ResultCount", items.length);
            this.addOutputParam(params, "TotalResults", totalResults);
                const errorMessage = error.response?.data?.error?.message;
                if (status === 403) {
                        `Google Custom Search quota exceeded or invalid credentials: ${errorMessage || "Forbidden"}`,
                        "FORBIDDEN"
                        `Google Custom Search request error: ${errorMessage || "Bad Request"}`,
                        "BAD_REQUEST"
                    `Google Custom Search API error: ${errorMessage || error.message}`,
                `Failed to perform Google Custom Search: ${error instanceof Error ? error.message : String(error)}`,
    private transformItemByVerbosity(item: GoogleSearchItem, verbosity: 'minimal' | 'standard' | 'detailed') {
        const pagemap = item.pagemap as Record<string, unknown> | undefined;
        const cseImage = this.extractFirstString(pagemap, "cse_image", "src");
        const cseThumbnail = this.extractFirstString(pagemap, "cse_thumbnail", "src");
        // Minimal: Just the essentials for AI agents needing quick results
        if (verbosity === 'minimal') {
                title: item.title,
                link: item.link,
                snippet: item.snippet
        // Standard: Balanced result set with most commonly needed fields
        if (verbosity === 'standard') {
                snippet: item.snippet,
                image: cseImage,
                thumbnail: cseThumbnail
        // Detailed: Everything including metadata and pagemap
            displayLink: item.displayLink,
            htmlSnippet: item.htmlSnippet,
            formattedUrl: item.formattedUrl,
            mime: item.mime,
            fileFormat: item.fileFormat,
            thumbnail: cseThumbnail,
            pagemap
    private buildResultData(
        data: GoogleSearchResponse,
        items: unknown[],
        verbosity: 'minimal' | 'standard' | 'detailed',
        metadata: { query: string; maxResults: number; startIndex: number; totalResults: number }
        const baseResult = {
            query: metadata.query,
            maxResults: metadata.maxResults,
            startIndex: metadata.startIndex,
            totalResults: metadata.totalResults,
            items
            return baseResult;
                ...baseResult,
                searchTime: data.searchInformation?.searchTime
        // Detailed: Include everything
            searchTime: data.searchInformation?.searchTime,
            searchInformation: data.searchInformation,
            queries: data.queries,
            context: data.context
    private extractFirstString(pagemap: Record<string, unknown> | undefined, key: string, field: string): string | undefined {
        if (!pagemap) {
        const value = pagemap[key];
        if (!Array.isArray(value)) {
        for (const entry of value) {
            if (entry && typeof entry === "object" && field in entry) {
                const fieldValue = (entry as Record<string, unknown>)[field];
                if (typeof fieldValue === "string" && fieldValue.trim().length > 0) {
                    return fieldValue;
    private normalizeSafeSearch(value?: string): string | undefined {
        if (!value) {
        switch (value.trim().toLowerCase()) {
            case "off":
            case "none":
            case "disabled":
                return "off";
            case "medium":
            case "moderate":
                return "medium";
            case "high":
            case "strict":
            case "active":
                return "high";
    private normalizeSiteSearchFilter(value?: string): "i" | "e" | undefined {
        const normalized = value.trim().toLowerCase();
        if (normalized === "include" || normalized === "i") {
            return "i";
        if (normalized === "exclude" || normalized === "e") {
            return "e";
    private normalizeLanguageRestriction(value?: string): string | undefined {
        const trimmed = value.trim();
        if (trimmed.toLowerCase().startsWith("lang_")) {
            return trimmed;
        if (/^[a-zA-Z]{2}$/.test(trimmed)) {
            return `lang_${trimmed.toLowerCase()}`;
    private normalizeSearchType(value?: string): "image" | undefined {
        if (normalized === "image" || normalized === "images") {
            return "image";
    private normalizeVerbosity(value?: string): 'minimal' | 'standard' | 'detailed' {
            return 'standard'; // Default
        switch (normalized) {
            case 'simple':
                return 'minimal';
            case 'detail':
            case 'verbose':
                return 'detailed';
            case 'normal':
            case 'default':
                return 'standard';
    private clamp(value: number, min: number, max: number): number {
            return min;
        return Math.min(Math.max(value, min), max);
