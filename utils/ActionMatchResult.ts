 * Metadata stored with each action's vector embedding.
 * Used for filtering and result formatting.
export interface ActionEmbeddingMetadata {
     * Unique identifier for the action
     * Action name
     * Action description (used in embedding generation)
     * Category name for grouping/filtering
    categoryName: string | null;
     * Action status (Active, Disabled, etc.)
     * Driver class name for the action
 * Result from semantic action search.
 * Returned by FindSimilarActions with similarity scores.
export interface ActionMatchResult {
    actionId: string;
    actionName: string;
     * Action description
     * Cosine similarity score (0-1)
     * Higher scores indicate better matches
    similarityScore: number;
     * Category name for the action
    categoryName?: string | null;
     * Action status
    status?: string;
     * Driver class name
    driverClass?: string;
