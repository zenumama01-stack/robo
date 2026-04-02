import { QueryInfo, QueryParameterInfo, RunQuerySQLFilterManager } from '@memberjunction/core';
 * Result of parameter validation
export interface ParameterValidationResult {
     * Whether all parameters passed validation
     * Error messages for any validation failures
     * The validated and type-converted parameters
    validatedParameters: Record<string, any>;
 * Result of processing a query template
export interface QueryProcessingResult {
     * Whether template processing was successful
     * The processed SQL query with parameters substituted
    processedSQL: string;
     * Error message if processing failed
     * The final parameters that were applied, including defaults
    appliedParameters: Record<string, any>;
 * Handles parameter validation and query template processing for parameterized queries.
 * Provides type conversion, validation, and secure template processing using Nunjucks.
export class QueryParameterProcessor {
    private static _nunjucksEnv: nunjucks.Environment;
     * Gets or creates the Nunjucks environment with custom SQL-safe filters
    private static get nunjucksEnv(): nunjucks.Environment {
        if (!this._nunjucksEnv) {
            this._nunjucksEnv = new nunjucks.Environment(null, {
                lstripBlocks: true
            // Add custom SQL-safe filters from the RunQuerySQLFilterManager
                    this._nunjucksEnv.addFilter(filter.name, filter.implementation);
        return this._nunjucksEnv;
     * Validates parameters against their definitions
    public static validateParameters(
        parameters: Record<string, any> | undefined,
        parameterDefinitions: QueryParameterInfo[]
    ): ParameterValidationResult {
        const validatedParams: Record<string, any> = {};
        // Process each defined parameter
        for (const paramDef of parameterDefinitions) {
            const value = parameters?.[paramDef.Name];
            if (paramDef.IsRequired && (value === undefined || value === null || value === '')) {
                errors.push(`Required parameter '${paramDef.Name}' is missing`);
            // Use default value if not provided
            let finalValue = value;
            if ((finalValue === undefined || finalValue === null) && paramDef.DefaultValue !== null) {
                    // Parse default value based on type
                    switch (paramDef.Type) {
                            finalValue = Number(paramDef.DefaultValue);
                            finalValue = paramDef.DefaultValue.toLowerCase() === 'true';
                            finalValue = new Date(paramDef.DefaultValue);
                            finalValue = JSON.parse(paramDef.DefaultValue);
                            finalValue = paramDef.DefaultValue;
                    errors.push(`Failed to parse default value for parameter '${paramDef.Name}': ${e.message}`);
            // Type conversion and validation
            if (finalValue !== undefined && finalValue !== null) {
                            validatedParams[paramDef.Name] = String(finalValue);
                            const num = Number(finalValue);
                                errors.push(`Parameter '${paramDef.Name}' must be a number`);
                            validatedParams[paramDef.Name] = num;
                            const date = finalValue instanceof Date ? finalValue : new Date(finalValue);
                                errors.push(`Parameter '${paramDef.Name}' must be a valid date`);
                            // Store as ISO string for SQL compatibility - Date.toString() produces
                            // format like "Mon Jan 26 2026..." which SQL Server cannot parse
                            validatedParams[paramDef.Name] = date.toISOString();
                            // Convert to 0/1 for SQL Server bit fields
                            // This ensures proper SQL syntax: WHERE BitColumn = 1 (not WHERE BitColumn = true)
                            if (typeof finalValue === 'boolean') {
                                validatedParams[paramDef.Name] = finalValue ? 1 : 0;
                                validatedParams[paramDef.Name] = String(finalValue).toLowerCase() === 'true' ? 1 : 0;
                            if (Array.isArray(finalValue)) {
                                validatedParams[paramDef.Name] = finalValue;
                            } else if (typeof finalValue === 'string') {
                                    validatedParams[paramDef.Name] = JSON.parse(finalValue);
                                    errors.push(`Parameter '${paramDef.Name}' must be a valid JSON array`);
                                errors.push(`Parameter '${paramDef.Name}' must be an array`);
                    // Apply validation filters if any
                    if (paramDef.ValidationFilters) {
                        const filters = paramDef.ParsedFilters;
                            // This is where custom validation logic would go
                            // For now, we'll just log that we would apply filters
                            // In a real implementation, you'd apply the filter rules
                    errors.push(`Error processing parameter '${paramDef.Name}': ${e.message}`);
            const definedParamNames = new Set(parameterDefinitions.map(p => p.Name));
            for (const key of Object.keys(parameters)) {
                if (!definedParamNames.has(key)) {
                    errors.push(`Unknown parameter: '${key}'`);
            validatedParameters: validatedParams
     * Processes a query template with the provided parameters
    public static processQueryTemplate(
        parameters: Record<string, any> | undefined
    ): QueryProcessingResult {
            // If query doesn't use templates, return the SQL as-is
            if (!query.UsesTemplate) {
                    processedSQL: query.SQL,
                    appliedParameters: {}
            const validation = this.validateParameters(parameters, query.Parameters);
            if (!validation.success) {
                    processedSQL: '',
                    error: `Parameter validation failed: ${validation.errors.join('; ')}`,
            // Process the template
                const processedSQL = this.nunjucksEnv.renderString(
                    query.SQL,
                    validation.validatedParameters
                    processedSQL,
                    appliedParameters: validation.validatedParameters
                    error: `Template processing failed: ${e.message}`,
                error: `Unexpected error during query processing: ${e.message}`,
