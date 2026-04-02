 * @fileoverview LLM-based reranker implementation using AI Prompts system.
 * Provides semantic document reranking using large language models through
 * the MemberJunction AI Prompts infrastructure.
 * @module @memberjunction/ai-reranker
import { AIPromptParams, AIPromptEntityExtended } from '@memberjunction/ai-core-plus';
 * Result item from LLM reranking response
interface LLMRerankItem {
    score: number;
 * LLM-based implementation of the BaseReranker class.
 * Uses AI Prompts to perform semantic reranking via large language models.
 * This reranker is useful when:
 * - You want to use your existing LLM infrastructure
 * - You need more control over the reranking logic via custom prompts
 * - Cohere or other dedicated rerankers are not available
 * Configuration:
 * - Requires a rerank prompt ID to be provided
 * - The prompt should accept 'query', 'documents', and 'topK' template variables
 * - The prompt should return JSON array: [{ index: number, score: number }, ...]
 *     'LLMReranker',
 *     '', // No API key needed, uses AI Prompts system
 *     '', // No model name, uses prompt configuration
 *     promptID,
@RegisterClass(BaseReranker, 'LLMReranker')
export class LLMReranker extends BaseReranker {
    private _promptID: string;
    private _contextUser: UserInfo;
    private _promptRunner: AIPromptRunner;
    private _cachedPrompt: AIPromptEntityExtended | null = null;
     * Create a new LLMReranker instance.
     * @param apiKey - Not used for LLM reranker, pass empty string
     * @param modelName - Not used for LLM reranker, pass empty string
     * @param promptID - ID of the AI Prompt to use for reranking
     * @param contextUser - User context for executing the prompt
    constructor(apiKey: string, modelName: string, promptID: string, contextUser: UserInfo) {
        super(apiKey, modelName || 'LLM');
        this._promptID = promptID;
        this._promptRunner = new AIPromptRunner();
     * Get the prompt ID used for reranking
    public get PromptID(): string {
        return this._promptID;
     * Load the rerank prompt from AIEngine cache.
     * Caches the prompt for subsequent calls.
    private async loadPrompt(): Promise<AIPromptEntityExtended | null> {
        if (this._cachedPrompt) {
            return this._cachedPrompt;
        const prompts = AIEngine.Instance.Prompts;
        const prompt = prompts.find(p => p.ID === this._promptID);
            LogError(`LLMReranker: Prompt not found with ID: ${this._promptID}`);
        this._cachedPrompt = prompt;
        return prompt;
     * Format documents for the rerank prompt template.
     * Creates a numbered list of documents with their text.
    private formatDocumentsForPrompt(documents: RerankParams['documents']): string {
        return documents.map((doc, idx) =>
            `[${idx}] ${doc.text}`
        ).join('\n\n');
     * Parse the LLM response to extract ranking scores.
     * Expected format: [{ index: 0, score: 0.95 }, { index: 1, score: 0.82 }, ...]
    private parseRankingResponse(
        rawOutput: unknown,
        documents: RerankParams['documents']
    ): RerankResult[] {
        let rankings: LLMRerankItem[];
        // Handle both string and already-parsed JSON
        if (typeof rawOutput === 'string') {
                rankings = JSON.parse(rawOutput);
                LogError(`LLMReranker: Failed to parse response as JSON: ${rawOutput}`);
        } else if (Array.isArray(rawOutput)) {
            rankings = rawOutput as LLMRerankItem[];
            LogError(`LLMReranker: Unexpected response type: ${typeof rawOutput}`);
        if (!Array.isArray(rankings)) {
            LogError(`LLMReranker: Response is not an array: ${JSON.stringify(rankings)}`);
        // Map rankings to RerankResult format
        const results: RerankResult[] = [];
        for (const item of rankings) {
            const index = item.index;
            const score = item.score;
            if (typeof index !== 'number' || index < 0 || index >= documents.length) {
                LogError(`LLMReranker: Invalid index ${index} for documents array of length ${documents.length}`);
            if (typeof score !== 'number' || score < 0 || score > 1) {
                LogError(`LLMReranker: Invalid score ${score}, expected 0-1 range`);
            const doc = documents[index];
                relevanceScore: score,
                rank: results.length // Will be updated by BaseReranker
        // Sort by relevance score descending
        return this.sortByRelevance(results);
     * Rerank documents using an LLM via the AI Prompts system.
     * 1. Loads the configured rerank prompt
     * 2. Formats documents for the prompt template
     * 3. Executes the prompt with query and documents
     * 4. Parses the JSON response to extract rankings
        // Load the rerank prompt
        const prompt = await this.loadPrompt();
            throw new Error(`LLMReranker: Rerank prompt not found with ID: ${this._promptID}`);
        promptParams.contextUser = this._contextUser;
        promptParams.attemptJSONRepair = true;
        // Set template data for the rerank prompt
            query: params.query,
            documents: this.formatDocumentsForPrompt(params.documents),
            topK: params.topK || params.documents.length
        LogStatus(`LLMReranker: Executing rerank prompt for ${params.documents.length} documents`);
        LogStatus(`LLMReranker: Query: "${params.query}"`);
        LogStatus(`LLMReranker: Documents:\n${this.formatDocumentsForPrompt(params.documents)}`);
        const result = await this._promptRunner.ExecutePrompt(promptParams);
            throw new Error(`LLMReranker: Prompt execution failed: ${result.errorMessage || 'Unknown error'}`);
        // Parse and return results
        // Use result.result which may be already parsed JSON, or rawResult for string
        const output = result.result || result.rawResult;
        LogStatus(`LLMReranker: Raw LLM response: ${JSON.stringify(output)}`);
        const parsed = this.parseRankingResponse(output, params.documents);
        LogStatus(`LLMReranker: Parsed ${parsed.length} results with scores: ${parsed.map(r => r.relevanceScore.toFixed(2)).join(', ')}`);
        return parsed;
 * Factory function to create an LLMReranker.
export function createLLMReranker(promptID: string, contextUser: UserInfo): LLMReranker {
    return new LLMReranker('', '', promptID, contextUser);
