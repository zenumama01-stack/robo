 * QueryTester - Tests and validates SQL queries
 * Renders Nunjucks templates with sample parameter values and executes
 * queries against the database. Handles error fixing with retry loop.
  RunQuerySQLFilterManager,
import { QueryFixer } from './QueryFixer';
 * QueryTester class
 * Tests SQL queries by rendering templates and executing against database
export class QueryTester {
    private dataProvider: DatabaseProviderBase,
    private entityMetadata: EntityMetadataForPrompt[],
    private businessQuestion: BusinessQuestion,
    // Initialize Nunjucks environment with SQL-safe filters
    this.nunjucksEnv = new nunjucks.Environment(null, {
      throwOnUndefined: true,
      trimBlocks: true,
      lstripBlocks: true,
    // Add custom SQL-safe filters from RunQuerySQLFilterManager
    const filterManager = RunQuerySQLFilterManager.Instance;
    const filters = filterManager.getAllFilters();
    for (const filter of filters) {
        this.nunjucksEnv.addFilter(filter.name, filter.implementation);
   * Test a query by rendering template with sample values and executing it
   * Retries up to maxAttempts times, calling QueryFixer on failures
   * @param query - Generated query to test
   * @param maxAttempts - Maximum number of retry attempts (default: 5)
   * @returns Test result with success status, SQL, and sample data
  async testQuery(
    maxAttempts: number = 5
  ): Promise<QueryTestResult> {
    let attempt = 0;
    let lastError: string | undefined;
    while (attempt < maxAttempts) {
        // 1. Render template with sample parameter values
        const renderedSQL = this.renderQueryTemplate(currentQuery);
        // 2. Execute SQL on database
        const results = await this.executeSQLQuery(renderedSQL);
        // 3. Success! (Empty results are valid - query executed without errors)
        // Note: We don't validate rowCount because:
        // - Empty results may indicate no data in database (not a query error)
        // - Query structure can be correct even with zero rows returned
        // - Testing should focus on SQL syntax/execution, not data presence
          renderedSQL,
          sampleRows: results.slice(0, 10), // Return first 10 rows
          attempts: attempt,
        lastError = extractErrorMessage(error, 'Query Testing');
          LogError(`Attempt ${attempt}/${maxAttempts} failed: ${lastError}`);
        // 5. If not last attempt, try to fix the query
        if (attempt < maxAttempts) {
          currentQuery = await this.fixQuery(currentQuery, lastError);
    // Failed after max attempts
      attempts: maxAttempts,
   * Render query template with sample parameter values
   * Uses QueryParameterProcessor for proper type handling
   * @param query - Generated query with parameters
   * @returns Rendered SQL string ready for execution
  private renderQueryTemplate(query: GeneratedQuery): string {
    const paramValues: Record<string, unknown> = {};
    for (const param of query.parameters) {
      const rawValue = param.sampleValue;
      if (rawValue !== undefined && rawValue !== null) {
        paramValues[param.name] = this.processParameterValue(rawValue, param.type);
      // Render template using Nunjucks with SQL-safe filters
      const renderedSQL = this.nunjucksEnv.renderString(query.sql, paramValues);
      return renderedSQL;
        `Template rendering failed: ${extractErrorMessage(error, 'Nunjucks')}`
   * Processes a raw parameter value based on its type, handling special cases like arrays.
   * Follows Skip-Brain pattern for parameter processing.
   * For array types, this function will:
   * - Parse JSON arrays if the value is a JSON string
   * - Split comma-separated strings as a fallback
   * - Return as-is for sqlIn filter to handle
   * @param rawValue - The raw parameter value (from sampleValue)
   * @param paramType - The parameter type ('string', 'number', 'date', 'boolean', 'array')
   * @returns Processed value ready for use in Nunjucks template
  private processParameterValue(rawValue: unknown, paramType: string): unknown {
    if (rawValue === undefined || rawValue === null) {
      return rawValue;
    // For array type parameters, ensure value is compatible with sqlIn filter
    if (paramType === 'array' && typeof rawValue === 'string') {
        // Try to parse as JSON array
        const parsed = JSON.parse(rawValue);
        // Not valid JSON - return as-is for sqlIn filter to handle comma-separated strings
      // Return comma-separated string as-is - sqlIn filter handles this
    // For non-array types, convert as needed
    switch (paramType) {
        if (typeof rawValue === 'string') {
          const num = Number(rawValue);
            throw new Error(`Invalid number sample value: ${rawValue}`);
          const lower = rawValue.toLowerCase();
          if (lower !== 'true' && lower !== 'false') {
            throw new Error(`Invalid boolean sample value: ${rawValue}`);
          return lower === 'true';
          const date = new Date(rawValue);
            throw new Error(`Invalid date sample value: ${rawValue}`);
   * Execute SQL query against database
   * Uses DataProvider to run query with contextUser
   * @param sql - Rendered SQL query
   * @returns Array of result rows
  private async executeSQLQuery(sql: string): Promise<unknown[]> {
      const result = await this.dataProvider.ExecuteSQL(
        sql,
      if (!result || !Array.isArray(result)) {
        throw new Error('ExecuteSQL returned invalid result format');
        `SQL execution failed: ${extractErrorMessage(error, 'Database')}`
   * Fix a query that failed to execute
   * Uses QueryFixer with AI to analyze error and generate correction
   * @param query - Query that failed
   * @param errorMessage - Error message from execution
   * @returns Corrected query
  private async fixQuery(
    const fixer = new QueryFixer(this.contextUser, this.config);
    return await fixer.fixQuery(
      this.entityMetadata,
      this.businessQuestion
