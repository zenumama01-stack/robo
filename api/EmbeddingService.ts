 * EmbeddingService - Generates embeddings for queries and golden queries
 * Wraps AIEngine embedding functionality for QueryGen use cases.
 * Generates embeddings for multiple fields (name, userQuestion, description, technicalDescription).
import { QueryEmbeddings, GoldenQuery, EmbeddedGoldenQuery } from '../data/schema';
 * Service for generating embeddings using AI Engine
export class EmbeddingService {
  private readonly modelName: string;
   * Create an EmbeddingService with the specified embedding model
   * @param modelName - Name of the embedding model to use (default: 'text-embedding-3-small')
  constructor(modelName: string = 'text-embedding-3-small') {
    this.modelName = modelName;
   * Embed a single query by generating embeddings for all its fields
   * Generates separate embeddings for:
   * - userQuestion: Natural language question
   * - description: High-level description
   * - technicalDescription: Technical implementation details
   * @param query - Query to embed (partial structure with text fields)
   * @returns Embeddings for all fields
  async embedQuery(query: {
  }): Promise<QueryEmbeddings> {
    // Generate embeddings for each field in parallel
    const [userQuestionResult, descResult, techDescResult] = await Promise.all([
      aiEngine.EmbedTextLocal(query.userQuestion),
      aiEngine.EmbedTextLocal(query.description),
      aiEngine.EmbedTextLocal(query.technicalDescription),
      userQuestion: userQuestionResult?.result.vector || [],
      description: descResult?.result.vector || [],
      technicalDescription: techDescResult?.result.vector || [],
   * Embed all golden queries for few-shot learning
   * Golden queries are example queries that serve as few-shot learning examples.
   * This method embeds all fields of all golden queries for similarity search.
   * Note: name field is excluded from embeddings as it's not available during query generation.
   * @param goldenQueries - Array of golden queries to embed
   * @returns Array of golden queries with their embeddings
  async embedGoldenQueries(goldenQueries: GoldenQuery[]): Promise<EmbeddedGoldenQuery[]> {
    const embedded: EmbeddedGoldenQuery[] = [];
      const embeddings = await this.embedQuery({
        userQuestion: query.userQuestion,
        description: query.description,
        technicalDescription: query.technicalDescription,
      embedded.push({
    return embedded;
   * Embed multiple queries in batch
   * @param queries - Array of queries to embed
   * @returns Array of embeddings corresponding to input queries
  async embedQueries(
  ): Promise<QueryEmbeddings[]> {
    const embeddings: QueryEmbeddings[] = [];
      const embedding = await this.embedQuery(query);
      embeddings.push(embedding);
    return embeddings;
