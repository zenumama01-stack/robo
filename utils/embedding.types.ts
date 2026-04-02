 * For reference only - the actual types should be imported from @memberjunction/ai
 * Parameters for creating embeddings
export interface EmbeddingParams {
     * Embedding model to use
     * Input text(s) to generate embeddings for
    input: string[];
     * Embedding dimensions (optional)
    dimensions?: number;
 * Result of creating embeddings
export interface EmbeddingResult {
     * Whether the operation was successful
     * Status text (e.g., "OK", "Error")
     * Operation start time
     * Operation end time
     * Time elapsed in milliseconds
    timeElapsed: number;
     * Embedding vectors
    data: number[][];
     * Model used for embeddings
    tokenUsage: {
        prompt: number;
     * Exception object if an error occurred
    exception: any;
 * These are just placeholder interfaces for reference
 * In the actual implementation, these should be imported from @memberjunction/ai
export abstract class BaseEmbeddingModel {
     * Create embeddings for the given input
    public abstract CreateEmbeddings(params: EmbeddingParams): Promise<EmbeddingResult>;
