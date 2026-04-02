 * @fileoverview SQL validation oracle implementation
import { Metadata, DatabaseProviderBase, UserInfo } from '@memberjunction/core';
 * SQL Validator Oracle.
 * Validates database state by executing SQL queries and comparing results.
 * Useful for testing that agent actions had the expected database effects.
 * - queries: Array of SQL validation queries
 * - requireAll: Whether all queries must pass (default: true)
 * Each query object contains:
 * - sql: The SQL query to execute
 * - expectedResult: Expected result (can be value, row count, or boolean)
 * - description: Human-readable description of what the query validates
 * const oracle = new SQLValidatorOracle();
 *         sqlValidations: [
 *             {
 *                 sql: "SELECT COUNT(*) FROM Reports WHERE Name LIKE '%Sales%'",
 *                 expectedResult: { count: 1 },
 *                 description: "Sales report was created"
 *             },
 *                 sql: "SELECT Status FROM Reports WHERE ID = @ReportID",
 *                 expectedResult: { status: 'Published' },
 *                 description: "Report status is Published"
 *     actualOutput: {
 *         reportId: 'abc-123'
 * }, {});
export class SQLValidatorOracle implements IOracle {
    readonly type = 'sql-validate';
     * Evaluate database state using SQL queries.
     * @param input - Oracle input with SQL validations and actual output
     * @returns Oracle result with query validation details
            // Get SQL validations from expected outcomes
            const validations = ((input.expectedOutput as any)?.sqlValidations as Array<{
            }>) || [];
            if (validations.length === 0) {
                    message: 'No SQL validations provided in ExpectedOutcomes.sqlValidations'
            const requireAll = config.requireAll !== false; // Default true
                expected: unknown;
                actual: unknown;
            // Execute each validation query
            for (const validation of validations) {
                const queryResult = await this.executeValidation(
                    validation,
                    input.actualOutput,
                    input.contextUser
            // Calculate score and determine pass/fail
            const passedCount = results.filter(r => r.passed).length;
            const totalCount = results.length;
            const score = totalCount > 0 ? passedCount / totalCount : 0;
            const passed = requireAll ? passedCount === totalCount : passedCount > 0;
                message: requireAll
                    ? `${passedCount}/${totalCount} validation(s) passed`
                    : `At least one validation passed (${passedCount}/${totalCount})`,
                details: { validations: results }
                message: `SQL validation error: ${(error as Error).message}`
     * Execute a single validation query.
    private async executeValidation(
        validation: { sql: string; expectedResult: unknown; description?: string },
        actualOutput: unknown,
            // Replace parameters in SQL with values from actualOutput
            const sql = this.replaceParameters(validation.sql, actualOutput);
            // Get database provider from Metadata.Provider
            const dbProvider = Metadata.Provider as DatabaseProviderBase;
            // Execute the SQL query
            const queryResults = await dbProvider.ExecuteSQL<Record<string, unknown>>(
                undefined,  // No parameters (already substituted in SQL string)
                { description: validation.description || 'SQL Validation' },
            // Extract result (handle single value, single row, or multiple rows)
            const actualResult = this.extractResult(queryResults);
            // Compare with expected result
            const passed = this.compareResults(validation.expectedResult, actualResult);
                description: validation.description || 'SQL validation',
                sql: validation.sql,
                expected: validation.expectedResult,
                actual: actualResult
                actual: null,
     * Replace parameters in SQL query with actual values.
    private replaceParameters(sql: string, actualOutput: unknown): string {
        if (!actualOutput || typeof actualOutput !== 'object') {
        let result = sql;
        const outputObj = actualOutput as Record<string, unknown>;
        // Replace @ParameterName with values from actualOutput
        const paramMatches = sql.matchAll(/@(\w+)/g);
        for (const match of paramMatches) {
            const camelCaseParam = paramName.charAt(0).toLowerCase() + paramName.slice(1);
            // Try both original and camelCase versions
            const value = outputObj[paramName] || outputObj[camelCaseParam];
                // Properly escape and quote the value
                const escapedValue = typeof value === 'string'
                    ? `'${value.replace(/'/g, "''")}'`
                result = result.replace(new RegExp(`@${paramName}`, 'g'), escapedValue);
     * Extract result from query result set.
    private extractResult(results: unknown[]): unknown {
        // If single row, single column, return the value
            const row = results[0] as Record<string, unknown>;
            const keys = Object.keys(row);
            if (keys.length === 1) {
                return row[keys[0]];
        // Multiple rows
     * Compare expected and actual results.
    private compareResults(expected: unknown, actual: unknown): boolean {
        if (expected === null || expected === undefined) {
            return actual === null || actual === undefined;
        // Handle boolean
        if (typeof expected === 'boolean') {
            return Boolean(actual) === expected;
        // Handle number
        if (typeof expected === 'number') {
            return Number(actual) === expected;
        // Handle string
            return String(actual) === expected;
        // Handle object/array
        if (typeof expected === 'object') {
            return JSON.stringify(expected) === JSON.stringify(actual);
