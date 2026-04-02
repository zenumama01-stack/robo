 * QueryWriter - Generates SQL query templates using AI with few-shot learning
 * Uses the SQL Query Writer AI prompt to generate Nunjucks SQL templates
 * based on business questions and similar golden query examples.
import { BusinessQuestion, GeneratedQuery, EntityMetadataForPrompt, GoldenQuery } from '../data/schema';
import { PROMPT_SQL_QUERY_WRITER } from '../prompts/PromptNames';
 * QueryWriter class
 * Generates Nunjucks SQL query templates using AI with few-shot learning
export class QueryWriter {
   * Generate SQL query template for a business question
   * Uses few-shot learning with similar golden query examples
   * @param businessQuestion - Business question to answer with SQL
   * @param entityMetadata - Available entity metadata for query
   * @param fewShotExamples - Similar golden queries for few-shot learning
   * @returns Generated SQL query with parameters and output schema
  async generateQuery(
    fewShotExamples: GoldenQuery[]
      // Find the SQL Query Writer prompt
      const prompt = this.findPromptByName(aiEngine, PROMPT_SQL_QUERY_WRITER);
        fewShotExamples,
      // Execute AI prompt
      const generatedQuery = await this.executePrompt(prompt, promptData);
      // Validate the generated query structure
      this.validateGeneratedQuery(generatedQuery);
      return generatedQuery;
        extractErrorMessage(error, 'QueryWriter.generateQuery')
   * Execute the SQL Query Writer AI prompt with retry logic for validation failures
   * Parses JSON response and validates structure, retrying with feedback if validation fails
      technicalDescription: string;
      fewShotExamples: GoldenQuery[];
    let lastResult: GeneratedQuery | null = null;
        const result = await executePromptWithOverrides<GeneratedQuery>(
        lastResult = result.result;
        // This will throw if validation fails
        this.validateGeneratedQuery(lastResult);
        // Validation passed, return the query
        return lastResult;
        // If this is the last attempt, throw the error
            `Query generation failed after ${maxRetries + 1} attempts: ${lastError.message}`
        // Log retry attempt
          LogStatus(`⚠️ Query validation failed on attempt ${attempt + 1}/${maxRetries + 1}: ${lastError.message}`);
          LogStatus(`   Retrying with validation feedback...`);
        // Add validation feedback to the prompt data for next attempt
        // This helps the LLM correct its mistakes
        promptData = {
          ...promptData,
          // Add feedback about what went wrong
          validationFeedback: `Previous attempt failed validation: ${lastError.message}. Please correct this issue.`,
        } as typeof promptData & { validationFeedback: string };
    // Should never reach here due to throw in loop, but TypeScript needs this
    throw lastError || new Error('Query generation failed');
   * Validate generated query structure
   * Ensures query has proper SQL template syntax and valid metadata
   * @param query - Generated query to validate
  private validateGeneratedQuery(query: GeneratedQuery): void {
      throw new Error('Generated query has empty SQL');
    // Validate SQL contains base view references (not raw tables)
    if (!this.usesBaseViews(query.sql)) {
      throw new Error('Generated SQL must use base views (vw*), not raw tables');
      throw new Error('Generated query parameters must be an array');
    // Validate each parameter
      this.validateParameter(param);
   * Check if SQL uses base views (vw* pattern)
   * Basic heuristic: looks for view names in FROM and JOIN clauses
  private usesBaseViews(sql: string): boolean {
    // Look for view patterns like [schema].[vw...] or FROM vw...
    const viewPattern = /\b(FROM|JOIN)\s+(\[\w+\]\.)?\[?vw\w+\]?/i;
    return viewPattern.test(sql);
   * Validate a single query parameter
   * Ensures all required fields are present and valid
  private validateParameter(param: unknown): void {
    if (!param || typeof param !== 'object') {
      throw new Error('Query parameter must be an object');
    const p = param as Record<string, unknown>;
    if (!p.name || typeof p.name !== 'string') {
      throw new Error('Query parameter must have a valid name');
    if (!p.type || typeof p.type !== 'string') {
      throw new Error(`Query parameter '${p.name}' must have a valid type`);
    if (typeof p.isRequired !== 'boolean') {
      throw new Error(`Query parameter '${p.name}' must have isRequired boolean`);
    if (!p.description || typeof p.description !== 'string') {
      throw new Error(`Query parameter '${p.name}' must have a description`);
    if (!Array.isArray(p.usage)) {
      throw new Error(`Query parameter '${p.name}' must have usage array`);
    // sampleValue is required and can be string, number, boolean, or array depending on parameter type
    if (p.sampleValue === undefined || p.sampleValue === null || p.sampleValue === '') {
      throw new Error(`Query parameter '${p.name}' must have a sampleValue`);
