 * Action that performs AI-powered web search using Perplexity's Search API
 * Returns comprehensive search results with citations and related questions
 * // Basic search with Perplexity
 *   ActionName: 'Perplexity Search',
 *     Value: 'latest developments in quantum computing'
 * // Search with specific model and parameters
 *     Value: 'climate change research papers 2024'
 *     Value: 'llama-3.1-sonar-large-128k-online'
 *     Name: 'ReturnRelatedQuestions',
 *     Name: 'ReturnImages',
 * // Search with domain filtering
 *     Value: 'machine learning tutorials'
 *     Name: 'SearchDomainFilter',
 *     Value: ['github.com', 'arxiv.org', 'medium.com']
@RegisterClass(BaseAction, "Perplexity Search")
export class PerplexitySearchAction extends BaseAction {
     * Executes the Perplexity AI search
     *   - Query: Search query text (required)
     *   - Model: Perplexity model to use (default: 'llama-3.1-sonar-small-128k-online')
     *     Options: llama-3.1-sonar-small-128k-online, llama-3.1-sonar-large-128k-online,
     *              llama-3.1-sonar-huge-128k-online
     *   - MaxTokens: Maximum tokens in response (default: 1000)
     *   - Temperature: Sampling temperature 0-2 (default: 0.2)
     *   - TopP: Nucleus sampling threshold (default: 0.9)
     *   - ReturnImages: Include images in results (default: false)
     *   - ReturnRelatedQuestions: Include related questions (default: false)
     *   - SearchDomainFilter: Array of domains to limit/exclude search (use '-' prefix to exclude)
     *   - SearchRecencyFilter: Filter by recency - 'day', 'week', 'month', 'year' (optional)
     * @returns Search results with content, citations, and related questions
            const query = this.getStringParam(params, 'query');
            // Get API key from config (which checks mj.config.cjs then falls back to environment variable)
            const apiKey = apiConfig.perplexityApiKey;
                    "Perplexity API key not found. Set perplexityApiKey in mj.config.cjs or PERPLEXITY_API_KEY environment variable",
            const model = this.getStringParam(params, 'model') || 'llama-3.1-sonar-small-128k-online';
            const maxTokens = this.getNumericParam(params, 'maxtokens', 1000);
            const temperature = this.getNumericParam(params, 'temperature', 0.2);
            const topP = this.getNumericParam(params, 'topp', 0.9);
            const returnImages = this.getBooleanParam(params, 'returnimages', false);
            const returnRelatedQuestions = this.getBooleanParam(params, 'returnrelatedquestions', false);
            const searchRecencyFilter = this.getStringParam(params, 'searchrecencyfilter');
            // Get domain filter (can be array or comma-separated string)
            const domainFilterParam = this.getParamValue(params, 'searchdomainfilter');
            let searchDomainFilter: string[] | undefined;
            if (domainFilterParam) {
                if (Array.isArray(domainFilterParam)) {
                    searchDomainFilter = domainFilterParam;
                } else if (typeof domainFilterParam === 'string') {
                    searchDomainFilter = domainFilterParam.split(',').map(d => d.trim());
                        content: query
                top_p: topP,
                return_images: returnImages,
                return_related_questions: returnRelatedQuestions,
                stream: false
            if (searchDomainFilter && searchDomainFilter.length > 0) {
                requestBody.search_domain_filter = searchDomainFilter;
            if (searchRecencyFilter) {
                requestBody.search_recency_filter = searchRecencyFilter;
            // Make API request
                'https://api.perplexity.ai/chat/completions',
                        'Authorization': `Bearer ${apiKey}`,
                return this.createErrorResult("Empty response from Perplexity API", "EMPTY_RESPONSE");
            // Extract response data
            const result = response.data;
            const choice = result.choices?.[0];
            const message = choice?.message;
            const content = message?.content || '';
            const citations = result.citations || [];
            const images = result.images || [];
            const relatedQuestions = result.related_questions || [];
            // Build output data
                citations,
                relatedQuestions,
                usage: result.usage || {},
                finishReason: choice?.finish_reason
            this.addOutputParam(params, 'Content', content);
            this.addOutputParam(params, 'Citations', citations);
            this.addOutputParam(params, 'CitationCount', citations.length);
            if (returnImages && images.length > 0) {
                this.addOutputParam(params, 'Images', images);
                this.addOutputParam(params, 'ImageCount', images.length);
            if (returnRelatedQuestions && relatedQuestions.length > 0) {
                this.addOutputParam(params, 'RelatedQuestions', relatedQuestions);
                this.addOutputParam(params, 'RelatedQuestionCount', relatedQuestions.length);
            this.addOutputParam(params, 'Usage', result.usage);
            this.addOutputParam(params, 'SearchResultDetails', outputData);
                        "Invalid Perplexity API key",
                        "Perplexity API rate limit exceeded",
                    `Perplexity API error: ${errorData?.error?.message || error.message}`,
                `Failed to perform Perplexity search: ${error instanceof Error ? error.message : String(error)}`,
     * Get parameter value (any type)
    private getParamValue(params: RunActionParams, paramName: string): unknown {
    private getNumericParam(params: RunActionParams, paramName: string, defaultValue: number = 0): number {
