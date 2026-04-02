import { BaseEntity, LogError } from "@memberjunction/core";
import { SQLServerDataProvider } from "@memberjunction/sqlserver-dataprovider";
 * Action that executes read-only SQL SELECT queries for research purposes with
 * security validation.
 * Security Features:
 * - SELECT-only enforcement (rejects INSERT, UPDATE, DELETE, DROP, etc.)
 * - Dangerous operation detection (EXEC, xp_, sp_, dynamic SQL, etc.)
 * - Query timeout protection
 * - Audit logging of all queries
 * - Result size limiting
 * Performance Features:
 * - Configurable row limits to prevent overwhelming results
 * - Execution time tracking
 * - Validation warnings for potentially slow queries
 * Note: SQL syntax validation is handled by SQL Server during execution.
 * This provides more accurate error messages than a JavaScript parser.
 * // Simple SELECT query
 *   ActionName: 'Execute Research Query',
 *     Name: 'Query',
 *     Value: 'SELECT TOP 100 * FROM Customers WHERE Country = ''USA'''
 * // Query with timeout
 *     Value: 'SELECT COUNT(*) FROM Orders GROUP BY CustomerID'
 *     Name: 'Timeout',
 *     Value: 60
@RegisterClass(BaseAction, "Execute Research Query")
export class ExecuteResearchQueryAction extends BaseAction {
     * List of dangerous SQL keywords and patterns that should be blocked
    private readonly DANGEROUS_PATTERNS = [
        /\bEXEC\b/i,
        /\bEXECUTE\b/i,
        /\bsp_/i,
        /\bxp_/i,
        /\bOPENROWSET\b/i,
        /\bOPENQUERY\b/i,
        /\bOPENDATASOURCE\b/i,
        /\bINSERT\b/i,
        /\bUPDATE\b/i,
        /\bDELETE\b/i,
        /\bDROP\b/i,
        /\bCREATE\b/i,
        /\bALTER\b/i,
        /\bTRUNCATE\b/i,
        /\bGRANT\b/i,
        /\bREVOKE\b/i,
        /\bDENY\b/i,
        /\bBACKUP\b/i,
        /\bRESTORE\b/i,
        /\bSHUTDOWN\b/i,
        /\bDBCC\b/i,
        /--\+/,  // SQL hints
        /\/\*\+/  // Oracle-style hints
            const query = this.getStringParam(params, "query");
            if (!query) {
                    ResultCode: "MISSING_QUERY",
                    Message: "Query parameter is required"
                } as ActionResultSimple;
            const maxRows = this.getNumericParam(params, "maxrows", 1000);
            const dataFormat = this.getStringParam(params, "dataformat") || 'csv';
            const analysisRequest = this.getStringParam(params, "analysisrequest");
            const returnType = this.getStringParam(params, "returntype") ||
                (analysisRequest ? 'data and analysis' : 'data only');
            const columnMaxLength = this.getNumericParam(params, "columnmaxlength", 50); // Default: 50 chars, 0 = no limit
            // Validate query security
            const securityValidation = this.validateQuerySecurity(query);
            if (!securityValidation.isValid) {
                    ResultCode: securityValidation.resultCode!,
                    Message: securityValidation.message!
            // Ensure query returns limited results
            const limitedQuery = this.ensureRowLimit(query, maxRows);
            const dataProvider = BaseEntity.Provider as SQLServerDataProvider;
                // Execute the query with timeout
                const queryStartTime = Date.now();
                const results = await Promise.race([
                    dataProvider.ExecuteSQL(limitedQuery, null, {
                        description: 'Execute Research Query',
                        ignoreLogging: false,
                        isMutation: false
                    }, params.ContextUser),
                    new Promise((_, reject) =>
                        setTimeout(() => reject(new Error('Query timeout exceeded')), timeout * 1000)
                ]) as any;
                const executionTimeMs = Date.now() - queryStartTime;
                // Get column metadata
                const columns = results && results.columns
                    ? Object.entries(results.columns).map(([name, col]: [string, any]) => ({
                        ColumnName: name,
                        DataType: col.type?.name || 'unknown',
                        IsNullable: col.nullable !== false
                const wasTruncated = results.length >= maxRows;
                // Generate validation warnings
                const warnings = this.generateValidationWarnings(query, results.length, executionTimeMs);
                // Format data based on requested format
                let formattedData: string | undefined;
                if (dataFormat === 'csv') {
                    formattedData = this.formatAsCSV(results, columnMaxLength);
                } else if (dataFormat === 'json') {
                    const trimmedResults = columnMaxLength > 0
                        ? this.trimResultColumns(results, columnMaxLength)
                        : results;
                    formattedData = JSON.stringify(trimmedResults, null, 2);
                // Perform analysis if requested
                let analysis: string | undefined;
                if (analysisRequest && (returnType === 'analysis only' || returnType === 'data and analysis')) {
                        // Don't call LLM for empty results - generate immediate response
                        analysis = 'Query returned no results. No data available to analyze.';
                        const analysisResult = await this.analyzeQueryData(
                            columns,
                            analysisRequest,
                            columnMaxLength
                        if (analysisResult.success) {
                            analysis = analysisResult.analysis;
                            LogError(`Failed to analyze query data: ${analysisResult.error}`);
                const totalExecutionTime = Date.now() - startTime;
                // Build detailed message based on return type
                const message = this.buildDetailedMessage(
                    executionTimeMs,
                    totalExecutionTime,
                    wasTruncated,
                    returnType,
                    formattedData,
                    analysis
                // Build result object based on return type
                const resultData = {
                    Columns: columns,
                    RowCount: results.length,
                    ExecutionTimeMs: executionTimeMs,
                    TotalTimeMs: totalExecutionTime,
                    WasTruncated: wasTruncated,
                    ValidationWarnings: warnings,
                    Query: limitedQuery
                // Add data and/or analysis to results based on returnType
                if (returnType === 'data only' || returnType === 'data and analysis') {
                    (resultData as any).Results = formattedData || results;
                if (returnType === 'analysis only' || returnType === 'data and analysis') {
                    (resultData as any).Analysis = analysis;
                return resultData;
            } catch (queryError: any) {
                // Handle query timeout
                if (queryError.message && queryError.message.includes('timeout')) {
                        ResultCode: "QUERY_TIMEOUT",
                        Message: `Query execution exceeded ${timeout} second timeout. Consider optimizing query or increasing timeout parameter.`
                // Handle permission errors
                if (queryError.message &&
                    (queryError.message.toLowerCase().includes('permission') ||
                     queryError.message.toLowerCase().includes('denied'))) {
                        ResultCode: "PERMISSION_DENIED",
                        Message: `Insufficient permissions to execute query: ${queryError.message}`
                // Handle other database errors
                    ResultCode: "DATABASE_ERROR",
                    Message: `Database error occurred: ${queryError.message || String(queryError)}`
                ResultCode: "QUERY_EXECUTION_FAILED",
                Message: `Query execution failed: ${errorMessage}`
     * Validates query for security concerns
    private validateQuerySecurity(query: string): { isValid: boolean; message?: string; resultCode?: string } {
        // Check for dangerous patterns
        for (const pattern of this.DANGEROUS_PATTERNS) {
            if (pattern.test(query)) {
                    message: `Query contains potentially dangerous operation: ${pattern.source}. Only SELECT queries are allowed.`,
                    resultCode: 'DANGEROUS_QUERY'
        // Check if query starts with SELECT (allowing for whitespace and comments)
        const trimmedQuery = query.trim();
        const cleanQuery = trimmedQuery.replace(/^\/\*[\s\S]*?\*\//, '').replace(/^--.*$/gm, '').trim();
        if (!cleanQuery.toUpperCase().startsWith('SELECT') &&
            !cleanQuery.toUpperCase().startsWith('WITH')) {  // Allow CTEs
                message: 'Only SELECT queries are allowed. Query must start with SELECT or WITH (for Common Table Expressions).',
                resultCode: 'NOT_SELECT_STATEMENT'
     * Ensures query has a row limit to prevent overwhelming results
    private ensureRowLimit(query: string, maxRows: number): string {
        // Check if query already has TOP clause
        const hasTop = /SELECT\s+TOP\s+\d+/i.test(query);
        if (hasTop) {
        // Check if query has OFFSET-FETCH (SQL Server 2012+)
        const hasOffsetFetch = /OFFSET\s+\d+\s+ROWS\s+FETCH/i.test(query);
        if (hasOffsetFetch) {
        // Add TOP clause
        return query.replace(/^(\s*SELECT\s+)/i, `$1TOP ${maxRows} `);
     * Generates validation warnings for potentially problematic queries
    private generateValidationWarnings(query: string, rowCount: number, executionTimeMs: number): string[] {
        // Warn about slow queries
        if (executionTimeMs > 5000) {
            warnings.push(`Query took ${executionTimeMs}ms to execute. Consider adding indexes or optimizing query.`);
        // Warn about SELECT *
        if (/SELECT\s+\*/i.test(query)) {
            warnings.push('Query uses SELECT *. Specifying explicit columns improves performance and clarity.');
        // Warn about missing WHERE clause on large result sets
        if (rowCount > 100 && !/WHERE/i.test(query)) {
            warnings.push('Query returned many rows without WHERE clause. Consider adding filters for better performance.');
        // Warn about potential Cartesian products
        const fromCount = (query.match(/FROM/gi) || []).length;
        const joinCount = (query.match(/JOIN/gi) || []).length;
        const whereCount = (query.match(/WHERE/gi) || []).length;
        if (fromCount > 1 && joinCount === 0 && whereCount === 0) {
            warnings.push('Query may contain Cartesian product (multiple tables without JOIN or WHERE). This can be very slow.');
        return warnings;
     * Formats results as CSV string with proper escaping
     * @param results Array of result objects
     * @param columnMaxLength Optional maximum length for column values (0 = no limit)
    private formatAsCSV(results: any[], columnMaxLength: number = 0): string {
        if (results.length === 0) return '';
        const headers = Object.keys(results[0]);
        const csvRows = [this.formatCSVRow(headers)];
        for (const row of results) {
            const values = headers.map(header => {
                let value = row[header];
                // Apply column length limit if specified
                if (columnMaxLength > 0 && value != null) {
                    const stringValue = String(value);
                    if (stringValue.length > columnMaxLength) {
                        value = stringValue.substring(0, columnMaxLength) + '...';
                return this.formatCSVValue(value);
            csvRows.push(values.join(','));
        return csvRows.join('\n');
     * Formats a single CSV row (for headers)
    private formatCSVRow(values: string[]): string {
        return values.map(value => this.formatCSVValue(value)).join(',');
     * Formats a single CSV value with proper escaping
     * - Null/undefined values become empty strings
     * - All string values are quoted and escaped
     * - Numbers and booleans are converted to strings and quoted
    private formatCSVValue(value: any): string {
            return '""';
        // Always quote and escape for maximum compatibility
        // Escape existing double quotes by doubling them
        const escaped = stringValue.replace(/"/g, '""');
        return `"${escaped}"`;
     * Trims columns in result set to maximum length
     * Used for JSON format results to prevent verbose fields from overwhelming context
     * @param maxLength Maximum length for string values
     * @returns New array with trimmed values
    private trimResultColumns(results: any[], maxLength: number): any[] {
        return results.map(row => {
            const trimmedRow: any = {};
            for (const [key, value] of Object.entries(row)) {
                if (value != null && typeof value === 'string' && value.length > maxLength) {
                    trimmedRow[key] = value.substring(0, maxLength) + '...';
                    trimmedRow[key] = value;
            return trimmedRow;
     * Analyze query data using AI prompt
    private async analyzeQueryData(
        columns: Array<{ ColumnName: string; DataType: string; IsNullable: boolean }>,
        analysisRequest: string,
        columnMaxLength: number = 0
    ): Promise<{ success: boolean; analysis?: string; error?: string }> {
            // Get the analysis prompt from AIEngine
            const prompt = this.getPromptByNameAndCategory('Analyze Query Data', 'MJ: System');
                    error: "Prompt 'Analyze Query Data' not found. Ensure metadata has been synced."
            // Format data as CSV for more efficient token usage
            // Apply column max length to trim verbose fields
            const dataCSV = this.formatAsCSV(results, columnMaxLength);
                data: dataCSV,
                columns: columns,
                rowCount: results.length,
                analysisRequest: analysisRequest
            const result = await runner.ExecutePrompt<{ analysis: string }>(promptParams);
                analysis: result.result?.analysis || String(result.result)
        return AIEngine.Instance.Prompts.find(p =>
            p.Name.trim().toLowerCase() === name.trim().toLowerCase() &&
            p.Category?.trim().toLowerCase() === category?.trim().toLowerCase()
     * Helper to get string parameter value
     * Helper to get numeric parameter value
     * Build detailed message with query results for agent consumption
    private buildDetailedMessage(
        executionTimeMs: number,
        totalTimeMs: number,
        wasTruncated: boolean,
        returnType: string,
        formattedData?: string,
        analysis?: string
        lines.push(`# Query Results`);
        lines.push(`\n**Rows Returned:** ${results.length.toLocaleString()}`);
        lines.push(`**Execution Time:** ${executionTimeMs}ms`);
        lines.push(`**Total Time:** ${totalTimeMs}ms`);
        if (wasTruncated) {
            lines.push(`**Note:** Results were truncated to maximum row limit`);
        lines.push(`\n---\n`);
        // Columns
        if (columns.length > 0) {
            lines.push(`## Columns (${columns.length})\n`);
            for (const col of columns) {
                const nullable = col.IsNullable ? 'NULL' : 'NOT NULL';
                lines.push(`- **${col.ColumnName}** \`${col.DataType}\` [${nullable}]`);
        // Warnings
        if (warnings.length > 0) {
            lines.push(`## Warnings\n`);
            for (const warning of warnings) {
                lines.push(`⚠️ ${warning}`);
        // Analysis section (if applicable)
        if (analysis && (returnType === 'analysis only' || returnType === 'data and analysis')) {
            lines.push(`## Analysis\n`);
            lines.push(analysis);
        // Results Data (if applicable)
            if (results.length > 0) {
                lines.push(`## Data (${results.length} row${results.length !== 1 ? 's' : ''})\n`);
                if (formattedData) {
                    lines.push('```');
                    lines.push(formattedData);
                    lines.push('```json');
                    lines.push(JSON.stringify(results, null, 2));
                lines.push(`## Data\n*No rows returned*`);
        lines.push(`\n---`);
        lines.push(`\n**The full result set is available in the Results output parameter for further processing.**`);
