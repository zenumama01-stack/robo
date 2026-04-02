import { BaseEmbeddings, Embeddings, EmbedTextParams, EmbedTextResult, EmbedTextsParams, EmbedTextsResult, BaseResult, ErrorAnalyzer } from '@memberjunction/ai';
 * Implementation of Azure AI Embedding Model
 * @class AzureEmbedding
 * @extends BaseEmbeddings
@RegisterClass(BaseEmbeddings, "AzureEmbedding")
export class AzureEmbedding extends BaseEmbeddings {
     * Create a new AzureEmbedding instance with an API key
     * @param apiKey API key string
        // Call the base class implementation to store settings
     * Create embeddings for a single text using Azure AI
     * @param params Embedding parameters
     * @returns Embedding result
            // Call Azure AI embeddings API
            const response = await this.Client.path("/embeddings").post({
                    input: [params.text], // API requires array
                    model: params.model || "text-embedding-ada-002", // Default model
                    dimensions: 1536 // Default dimensions
            const responseBody = response.body as any;
            // Check for errors in the response
            if (!responseBody.data || !Array.isArray(responseBody.data) || responseBody.data.length === 0) {
                throw new Error('Invalid embedding response format');
            // Extract embedding from response
            const embedding = responseBody.data[0].embedding;
            // Return embedding result
            const result: EmbedTextResult = {
                object: 'object',
                model: responseBody.model || params.model || "unknown",
                vector: embedding,
                ModelUsage: {
                    promptTokens: responseBody.usage?.prompt_tokens || 0,
                    get totalTokens() { return this.promptTokens + this.completionTokens; }
            // Log error details for debugging
            const errorInfo = ErrorAnalyzer.analyzeError(error, 'Azure');
            console.error('Azure embedding error:', errorInfo);
            // Return error result
                model: params.model || "unknown",
                vector: [],
     * Create embeddings for multiple texts using Azure AI
            // Ensure we have text input
            if (!params.texts || params.texts.length === 0) {
                throw new Error('Input texts are required for embedding');
                    input: params.texts,
            if (!responseBody.data || !Array.isArray(responseBody.data)) {
            // Extract embeddings from response
            const embeddings = responseBody.data.map((item: any) => item.embedding);
            const result: EmbedTextsResult = {
                object: 'list',
                vectors: embeddings,
                vectors: [],
     * Get available embedding models
     * @returns List of available embedding models
    public async GetEmbeddingModels(): Promise<any> {
                id: "text-embedding-ada-002",
                name: "Azure Embedding - text-embedding-ada-002",
                contextLength: 8191,
                dimensions: 1536
