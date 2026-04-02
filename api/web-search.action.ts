import { getDuckDuckGoRateLimiter, SearchRequest } from "./duckduckgo-rate-limiter";
 * Action that performs web search using DuckDuckGo's Instant Answer API
 * Returns search results with titles, URLs, and snippets
 * // Basic web search
 *   ActionName: 'Web Search',
 *     Name: 'SearchTerms',
 *     Value: 'TypeScript programming'
 * // Search with custom result count
 *     Value: 15
@RegisterClass(BaseAction, "__WebSearch")
export class WebSearchAction extends BaseAction {
     * Executes the web search using DuckDuckGo's API
     *   - SearchTerms: Keywords to search for (required)
     *   - MaxResults: Maximum number of results to return (default: 10, max: 30)
     *   - Region: Regional search preference (default: 'us-en')
     *   - SafeSearch: Safe search level - 'strict', 'moderate', 'off' (default: 'moderate')
     *   - DisableQueueing: If true, fails immediately when rate limited instead of queueing (default: false)
     * @returns Web search results with titles, URLs, and snippets
            const searchTermsParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'searchterms');
            const maxResultsParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'maxresults');
            const regionParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'region');
            const safeSearchParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'safesearch');
            const disableQueueingParam = params.Params.find(p => p.Name.trim().toLowerCase() === 'disablequeueing');
            if (!searchTermsParam || !searchTermsParam.Value) {
                    Message: "SearchTerms parameter is required",
            const searchTerms = searchTermsParam.Value.toString().trim();
            const maxResults = Math.min(parseInt(maxResultsParam?.Value?.toString() || '10'), 30);
            const region = regionParam?.Value?.toString() || 'us-en';
            const safeSearch = safeSearchParam?.Value?.toString() || 'moderate';
            const disableQueueing = disableQueueingParam?.Value?.toString()?.toLowerCase() === 'true';
            if (searchTerms.length === 0) {
                    Message: "SearchTerms cannot be empty",
            // Get the rate limiter instance
            const rateLimiter = getDuckDuckGoRateLimiter();
            // Try DuckDuckGo Instant Answer API first (JSON response)
            let response: Response;
            let html: string = '';
            let useInstantAPI = true;
                // Try the Instant Answer API first (more reliable)
                const instantUrl = `https://api.duckduckgo.com/?q=${encodeURIComponent(searchTerms)}&format=json&no_html=1&skip_disambig=1`;
                const searchRequest: SearchRequest = {
                    url: instantUrl,
                            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
                    disableQueueing
                const result = await rateLimiter.search(searchRequest);
                response = result.response;
                if (response.ok) {
                    const jsonData = result.text;
                    const instantResults = this.parseInstantResults(jsonData, maxResults);
                    if (instantResults.length > 0) {
                            searchTerms,
                            totalResults: instantResults.length,
                            region,
                            results: instantResults,
                            searchUrl: `https://duckduckgo.com/?q=${encodeURIComponent(searchTerms)}`,
                            method: 'instant-api'
                // Instant API failed, fall back to HTML scraping
                useInstantAPI = false;
            // Fallback to HTML search with more realistic headers
            const searchUrl = `https://duckduckgo.com/html/?q=${encodeURIComponent(searchTerms)}&kl=${region}`;
            const htmlSearchRequest: SearchRequest = {
                url: searchUrl,
                        'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
                        'Accept-Language': 'en-US,en;q=0.5',
            const htmlResult = await rateLimiter.search(htmlSearchRequest);
            response = htmlResult.response;
            html = htmlResult.text;
                    Message: `DuckDuckGo search failed: ${response.status} ${response.statusText}`,
                    ResultCode: "SEARCH_FAILED"
            // Check if we got an empty or invalid response
            if (!html || html.length < 1000 || !html.includes('result')) {
                    Message: "DuckDuckGo returned empty or invalid response. This may be due to rate limiting or anti-bot measures.",
            const results = this.parseSearchResults(html, maxResults);
                    Message: "No search results found in response",
                    ResultCode: "NO_RESULTS"
                totalResults: results.length,
                method: 'html-scraping'
            // add output props as well to make it easier to use in flows
                Name: 'SearchResultsDetails',
                Value: resultData,
                Name: 'SearchUrl',
                Value: resultData.searchUrl,
                Value: results,
                Name: 'SearchResultsCount',
                Value: results.length,
            // and finally share the first result as well
                    Name: 'FirstSearchResult',
                    Value: results[0],
            // Check if it's a rate limit error with queueing disabled
            if (errorMessage.includes('Rate limiting active and queueing is disabled') || 
                errorMessage.includes('Rate limited and queueing is disabled')) {
                    Message: "DuckDuckGo rate limit reached. Queueing is disabled for this request.",
                Message: `Failed to perform web search: ${errorMessage}`,
     * Parses DuckDuckGo Instant Answer API JSON response
    private parseInstantResults(jsonData: string, maxResults: number): any[] {
            const data = JSON.parse(jsonData);
            // Handle abstract (summary) if available
            if (data.Abstract && data.AbstractText) {
                    title: data.Heading || 'Summary',
                    url: data.AbstractURL || '',
                    displayUrl: data.AbstractSource || '',
                    snippet: data.AbstractText,
                    rank: 1,
                    type: 'abstract'
            // Handle related topics
            if (data.RelatedTopics && Array.isArray(data.RelatedTopics)) {
                let rank = results.length + 1;
                for (const topic of data.RelatedTopics.slice(0, maxResults - results.length)) {
                    if (topic.FirstURL && topic.Text) {
                            title: topic.Text.split(' - ')[0] || 'Related Topic',
                            url: topic.FirstURL,
                            displayUrl: this.extractDomain(topic.FirstURL),
                            snippet: topic.Text,
                            rank: rank++,
                            type: 'related'
            // Handle definition if available
            if (data.Definition && data.DefinitionURL) {
                    title: `Definition: ${data.Heading || 'Term'}`,
                    url: data.DefinitionURL,
                    displayUrl: data.DefinitionSource || '',
                    snippet: data.Definition,
                    rank: results.length + 1,
                    type: 'definition'
            // Handle answer if available
            if (data.Answer) {
                    title: 'Direct Answer',
                    url: '',
                    displayUrl: 'DuckDuckGo',
                    snippet: data.Answer,
                    type: 'answer'
            console.error('Error parsing instant results:', error);
     * Extracts domain from URL
    private extractDomain(url: string): string {
            return new URL(url).hostname;
     * Parses DuckDuckGo HTML search results
    private parseSearchResults(html: string, maxResults: number): any[] {
            // Improved regex patterns for DuckDuckGo HTML structure
            // DuckDuckGo uses different class names, try multiple patterns
            const resultPatterns = [
                // Standard result pattern
                /<div class="[^"]*result[^"]*"[^>]*>(.*?)<\/div>/gs,
                // Alternative result pattern
                /<div[^>]+data-testid="result"[^>]*>(.*?)<\/div>/gs,
                // Links result pattern  
                /<div class="[^"]*links_main[^"]*"[^>]*>(.*?)<\/div>/gs
            const linkPatterns = [
                /<a[^>]+href="([^"]+)"[^>]*class="[^"]*result__a[^"]*"[^>]*>(.*?)<\/a>/s,
                /<a[^>]+href="([^"]+)"[^>]*>(.*?)<\/a>/s
            const snippetPatterns = [
                /<a[^>]+class="[^"]*result__snippet[^"]*"[^>]*>(.*?)<\/a>/s,
                /<span class="[^"]*result__snippet[^"]*"[^>]*>(.*?)<\/span>/s,
                /<div class="[^"]*snippet[^"]*"[^>]*>(.*?)<\/div>/s
            const urlDisplayPatterns = [
                /<span class="[^"]*result__url[^"]*"[^>]*>(.*?)<\/span>/s,
                /<cite[^>]*>(.*?)<\/cite>/s
            let match: RegExpExecArray | null;
            let count = 0;
            // Try each result pattern until we find matches
            for (const resultPattern of resultPatterns) {
                if (count >= maxResults) break;
                const regex = new RegExp(resultPattern.source, resultPattern.flags);
                while ((match = regex.exec(html)) !== null && count < maxResults) {
                    const resultHtml = match[1];
                    // Try each link pattern
                    let linkMatch = null;
                    for (const linkPattern of linkPatterns) {
                        linkMatch = linkPattern.exec(resultHtml);
                        if (linkMatch) break;
                    if (linkMatch) {
                        const url = this.decodeUrl(linkMatch[1]);
                        const title = this.stripHtml(linkMatch[2]);
                        // Try each snippet pattern
                        let snippetMatch = null;
                        for (const snippetPattern of snippetPatterns) {
                            snippetMatch = snippetPattern.exec(resultHtml);
                            if (snippetMatch) break;
                        const snippet = snippetMatch ? this.stripHtml(snippetMatch[1]) : '';
                        // Try each URL display pattern
                        let urlDisplayMatch = null;
                        for (const urlDisplayPattern of urlDisplayPatterns) {
                            urlDisplayMatch = urlDisplayPattern.exec(resultHtml);
                            if (urlDisplayMatch) break;
                        const displayUrl = urlDisplayMatch ? this.stripHtml(urlDisplayMatch[1]) : this.extractDomain(url);
                        if (url && title && (url.startsWith('http') || url.startsWith('//'))) {
                            // Clean up URL
                            const cleanUrl = url.startsWith('//') ? 'https:' + url : url;
                                title: title.trim(),
                                url: cleanUrl.trim(),
                                displayUrl: displayUrl.trim(),
                                snippet: snippet.trim(),
                                rank: count + 1,
                                type: 'web'
                // If we found results with this pattern, don't try others
                if (results.length > 0) break;
            console.error('Error parsing search results:', error);
     * Decodes DuckDuckGo URL redirects
    private decodeUrl(encodedUrl: string): string {
            if (encodedUrl.includes('uddg=')) {
                const urlMatch = encodedUrl.match(/uddg=([^&]+)/);
                if (urlMatch) {
                    return decodeURIComponent(urlMatch[1]);
            return encodedUrl;
     * Strips HTML tags and decodes entities
    private stripHtml(html: string): string {
        return html
            .replace(/\s+/g, ' ') // Normalize whitespace
