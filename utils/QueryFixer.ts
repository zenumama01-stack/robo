 * QueryFixer - Fixes SQL queries that fail execution
 * Uses the SQL Query Fixer AI prompt to analyze errors and generate
 * corrected SQL queries with updated metadata.
import { UserInfo, LogStatus } from '@memberjunction/core';
  GeneratedQuery,
  EntityMetadataForPrompt,
  BusinessQuestion,
} from '../data/schema';
import { PROMPT_SQL_QUERY_FIXER } from '../prompts/PromptNames';
 * QueryFixer class
 * Fixes SQL queries that fail to execute
export class QueryFixer {
    private config: QueryGenConfig
   * Fix a SQL query that failed to execute
   * Uses AI to analyze the error and generate a corrected query
   * @param query - The query that failed
   * @param errorMessage - The error message from execution
   * @param entityMetadata - Entity metadata for context
   * @param businessQuestion - Original business question for context
   * @returns Corrected SQL query with updated metadata
  async fixQuery(
    query: GeneratedQuery,
    entityMetadata: EntityMetadataForPrompt[],
    businessQuestion: BusinessQuestion
  ): Promise<GeneratedQuery> {
      // Find the SQL Query Fixer prompt
      const prompt = this.findPromptByName(aiEngine, PROMPT_SQL_QUERY_FIXER);
        queryName: query.queryName,
        originalSQL: query.sql,
        userQuestion: businessQuestion.userQuestion,
        description: businessQuestion.description,
      // Execute AI prompt to fix the query
      const fixedQuery = await this.executePrompt(prompt, promptData);
      // Validate the fixed query structure
      this.validateFixedQuery(fixedQuery);
      return fixedQuery;
      throw new Error(extractErrorMessage(error, 'QueryFixer.fixQuery'));
   * Find prompt by name in AIEngine cache
   * Throws if prompt not found
  private findPromptByName(
    aiEngine: AIEngine,
    promptName: string
  ): AIPromptEntityExtended {
    const prompt = aiEngine.Prompts.find((p) => p.Name === promptName);
      throw new Error(`Prompt '${promptName}' not found in AIEngine cache`);
   * Execute the SQL Query Fixer AI prompt
   * Parses JSON response and validates structure
  private async executePrompt(
    promptData: {
      originalSQL: string;
      parameters: GeneratedQuery['parameters'];
      entityMetadata: EntityMetadataForPrompt[];
      userQuestion: string;
    // The SQL Query Fixer template returns { newSQL, reasoning }
    const result = await executePromptWithOverrides<{
      newSQL: string;
    }>(prompt, promptData, this.contextUser, this.config);
        `AI prompt execution failed: ${result?.errorMessage || 'Unknown error'}`
      throw new Error('AI prompt returned no result');
    // Log the reasoning
    if (this.config.verbose && result.result.reasoning) {
      LogStatus(`Query fix reasoning: ${result.result.reasoning}`);
    // Return GeneratedQuery format, preserving original parameters and queryName
      queryName: promptData.queryName,
      sql: result.result.newSQL,
      parameters: promptData.parameters,
   * Validate fixed query structure
   * Ensures query has proper metadata
   * @param query - Fixed query to validate
   * @throws Error if query structure is invalid
  private validateFixedQuery(query: GeneratedQuery): void {
    // Validate SQL is present
    if (!query.sql || query.sql.trim().length === 0) {
      throw new Error('Fixed query has empty SQL');
    // Validate parameters array
    if (!Array.isArray(query.parameters)) {
      throw new Error('Fixed query parameters must be an array');
