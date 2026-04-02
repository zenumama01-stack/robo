import { BaseEmbeddings, EmbedTextParams, EmbedTextResult, EmbedTextsParams, EmbedTextsResult, ModelUsage } from '@memberjunction/ai';
import { Ollama, EmbeddingsRequest, EmbeddingsResponse } from 'ollama';
 * Ollama implementation of the BaseEmbeddings class for local embedding generation
 * Supports various embedding models like nomic-embed-text, mxbai-embed-large, etc.
@RegisterClass(BaseEmbeddings, "OllamaEmbedding")
export class OllamaEmbedding extends BaseEmbeddings {
    private _client: Ollama;
    private _baseUrl: string = 'http://localhost:11434';
    private _keepAlive: string | number = '5m'; // Default keep model loaded for 5 minutes
        super(apiKey || ''); // Ollama doesn't require API key for local usage
        this._client = new Ollama({ host: this._baseUrl });
     * Read only getter method to get the Ollama client instance
    public get OllamaClient(): Ollama {
    public get client(): Ollama {
        return this.OllamaClient;
     * Override SetAdditionalSettings to handle Ollama specific settings
        // Handle Ollama-specific settings
        if (settings.baseUrl || settings.host) {
            this._baseUrl = settings.baseUrl || settings.host;
        if (settings.keepAlive !== undefined) {
            this._keepAlive = settings.keepAlive;
     * Embed a single text string using Ollama
            throw new Error('Model name is required for Ollama embedding provider');
            // Ensure the model is available
            await this.ensureModelAvailable(params.model);
            // Create embeddings request
            const embeddingsRequest: EmbeddingsRequest = {
                prompt: params.text,
                keep_alive: this._keepAlive
            // Additional options can be passed through additionalParams if needed
            if ((params as any).additionalParams) {
                Object.assign(embeddingsRequest, (params as any).additionalParams);
            // Make the embeddings request
            const response: EmbeddingsResponse = await this.client.embeddings(embeddingsRequest);
            // Return the embedding result
                ModelUsage: new ModelUsage(
                    (response as any).prompt_eval_count || 0,
                    0
                vector: response.embedding
            // On error, return a minimal valid result structure
            // The BaseEmbeddings class expects a specific format
     * Embed multiple texts in batch using Ollama
     * Note: Ollama doesn't have native batch support, so we process sequentially
     * For better performance, consider running multiple Ollama instances or using async processing
            // Process each text sequentially
            // Note: Ollama doesn't support true batch processing, but we can optimize by keeping the model loaded
                    prompt: text,
                embeddings.push(response.embedding);
                totalPromptTokens += (response as any).prompt_eval_count || 0;
            // Return the batch embedding result
                ModelUsage: new ModelUsage(totalPromptTokens, 0),
            // On error, throw to let caller handle
     * Ensure a model is available locally, pulling it if necessary
    private async ensureModelAvailable(modelName: string): Promise<void> {
            // Check if model is available
            const models = await this.client.list();
            const isAvailable = models.models.some((m: any) => 
                m.name === modelName || m.name.startsWith(modelName + ':')
            if (!isAvailable) {
                console.log(`Model ${modelName} not found locally. Attempting to pull...`);
                await this.client.pull({ model: modelName, stream: false });
                console.log(`Model ${modelName} pulled successfully.`);
            // If we can't check or pull, continue anyway - the embeddings call will fail with a clear error
            console.warn(`Could not verify model availability: ${error}`);
     * Required by BaseEmbeddings abstract class
            // Filter for common embedding models
            const embeddingKeywords = ['embed', 'e5', 'bge', 'gte', 'nomic', 'mxbai', 'all-minilm'];
            const embeddingModels = models.models
                .filter((m: any) => 
                    embeddingKeywords.some(keyword => m.name.toLowerCase().includes(keyword))
                .map((m: any) => ({
                    name: m.name,
                    size: m.size,
                    modified: m.modified_at
            return embeddingModels;
            console.error('Failed to get embedding models:', error);
     * List available embedding models in Ollama
    public async listEmbeddingModels(): Promise<string[]> {
            // Filter for common embedding models (this is a heuristic as Ollama doesn't strictly categorize)
            return models.models
                .map((m: any) => m.name)
                .filter((name: string) => 
                    embeddingKeywords.some(keyword => name.toLowerCase().includes(keyword))
            console.error('Failed to list embedding models:', error);
     * Get information about a specific embedding model
    public async getModelInfo(modelName: string): Promise<any> {
            const response = await this.client.show({ model: modelName });
            console.error(`Failed to get info for model ${modelName}:`, error);
     * Get the dimension size for a specific embedding model
     * This is useful for setting up vector databases
    public async getEmbeddingDimension(modelName: string): Promise<number | null> {
            // Generate a sample embedding to get dimensions
            const response = await this.client.embeddings({
                prompt: "test",
                keep_alive: 0 // Don't keep model loaded for this test
            return response.embedding.length;
            console.error(`Failed to get embedding dimension for ${modelName}:`, error);
