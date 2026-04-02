import { BaseReranker, RerankParams, RerankResult } from '@memberjunction/ai';
import { CohereClient } from 'cohere-ai';
 * Cohere implementation of the BaseReranker class.
 * Uses Cohere's Rerank API for semantic document reranking.
 *     'CohereReranker',
@RegisterClass(BaseReranker, 'CohereLLM')
export class CohereReranker extends BaseReranker {
    private _client: CohereClient;
     * Create a new CohereReranker instance.
     * @param apiKey - Cohere API key
     * @param modelName - Model to use (default: 'rerank-v3.5')
        super(apiKey, modelName || 'rerank-v3.5');
        // Use inherited protected apiKey getter from BaseModel
        this._client = new CohereClient({ token: this.apiKey });
     * Rerank documents using Cohere's Rerank API.
     * The Cohere API returns results sorted by relevance with scores from 0-1.
     * We map the response to our RerankResult format, preserving document metadata.
        // Build enhanced query with context about what makes memory notes relevant
        const enhancedQuery = `You are evaluating agent memory notes for relevance to a user message.
Key principles for scoring relevance:
- User identity notes (name, preferences) are almost ALWAYS relevant, especially for greetings
- Memory notes provide context that COULD be useful, not just direct matches
- Preferences should be applied whenever the topic might come up
- Context notes help maintain continuity across conversations
The user's current message is: "${params.query}"
Find memory notes that would help respond appropriately to this message.`;
        // DEBUG: Log request with ALL documents
        console.log('CohereReranker REQUEST:', JSON.stringify({
            model: this._modelName,
            originalQuery: params.query,
            enhancedQuery: enhancedQuery.substring(0, 200) + '...',
            documentCount: params.documents.length,
            topK: params.topK,
            allDocs: params.documents.map((d, i) => `[${i}] ${d.text.substring(0, 80)}...`)
        }, null, 2));
        // Call Cohere's rerank endpoint with enhanced query
        const response = await this._client.rerank({
            query: enhancedQuery,
            documents: params.documents.map(d => d.text),
            topN: params.topK || params.documents.length,
            returnDocuments: false // We track documents ourselves to preserve metadata
        // DEBUG: Log response
        console.log('CohereReranker RESPONSE:', JSON.stringify({
            resultCount: response.results.length,
            results: response.results.slice(0, 5).map(r => ({
                index: r.index,
                relevanceScore: r.relevanceScore
        // Map Cohere results to our format
        // Cohere returns results already sorted by relevance
        const results: RerankResult[] = response.results.map((result, idx) => {
            const originalDoc = params.documents[result.index];
                id: originalDoc.id,
                relevanceScore: result.relevanceScore,
                document: originalDoc,
 * Factory function to create a CohereReranker with default model.
 * Convenience function for simple usage.
export function createCohereReranker(apiKey: string, modelName?: string): CohereReranker {
    return new CohereReranker(apiKey, modelName);
