 * @fileoverview Safe expression evaluator for conditional logic in MemberJunction.
 * This module provides a secure way to evaluate boolean expressions against
 * context objects without allowing arbitrary code execution. It supports
 * dot notation for nested property access and common comparison operations.
 * Result of expression evaluation including success status and diagnostics
export interface ExpressionEvaluationResult {
    value?: boolean;
    diagnostics?: {
        context: Record<string, any>;
        evaluationTime: number;
 * Safe expression evaluator that prevents arbitrary code execution while
 * supporting common boolean expressions and property access patterns.
 * Supported operations:
 * - Comparison: ==, ===, !=, !==, <, >, <=, >=
 * - Logical: &&, ||, !
 * - Property access: dot notation (e.g., payload.customer.name)
 * - Array access: bracket notation (e.g., items[0])
 * - Safe methods: .length, .includes(), .startsWith(), .endsWith()
 * - Array methods: .some(), .every(), .find(), .filter()
 * - Type checking: typeof, instanceof (limited to safe types)
 * @class SafeExpressionEvaluator
 * const evaluator = new SafeExpressionEvaluator();
 * // Simple comparison
 * const result1 = evaluator.evaluate(
 *   "status == 'active'",
 *   { status: 'active' }
 * // Nested property access
 * const result2 = evaluator.evaluate(
 *   "payload.customer.tier == 'premium' && payload.order.total > 1000",
 *   { payload: { customer: { tier: 'premium' }, order: { total: 1500 } } }
 * // Array methods
 * const result3 = evaluator.evaluate(
 *   "items.some(item => item.price > 100)",
 *   { items: [{ price: 50 }, { price: 150 }] }
export class SafeExpressionEvaluator {
     * Patterns that indicate potentially dangerous code
    private static readonly DANGEROUS_PATTERNS = [
        /\beval\s*\(/i,
        /\bnew\s+Function/i,
        /\bFunction\s*\(/i,
        /\bimport\s+/i,
        /\brequire\s*\(/i,
        /\bprocess\./i,
        /\bglobal\./i,
        /\bwindow\./i,
        /\bdocument\./i,
        /\b__proto__\b/i,
        /\bconstructor\b/i,
        /\bprototype\b/i,
        /\bthis\b/,
        /\bawait\b/i,
        /\basync\b/i,
        /\bclass\b/i,
        /\bextends\b/i,
        /\bthrow\b/i,
        /\btry\b/i,
        /\bcatch\b/i,
        /\bfinally\b/i,
        /;/, // No semicolons to prevent multiple statements
        /{/, // No curly braces to prevent code blocks
        /}/, // No curly braces
        /`/, // No template literals
        /\$\{/, // No template expressions
     * Safe methods that can be called on objects
    private static readonly SAFE_METHODS = [
        'length',
        'includes',
        'startsWith', 
        'endsWith',
        'indexOf',
        'lastIndexOf',
        'toLowerCase',
        'toUpperCase',
        'trim',
        'trimStart',
        'trimEnd',
        'toString',
        'valueOf',
        'some',
        'every',
        'find',
        'filter',
        'map',
        'reduce'
     * Evaluates a boolean expression against a context object
     * @param {string} expression - The boolean expression to evaluate
     * @param {Record<string, any>} context - The context object containing variables
     * @param {boolean} [enableDiagnostics=false] - Whether to include diagnostic information
     * @returns {ExpressionEvaluationResult} The evaluation result
    public evaluate(
        expression: string, 
        enableDiagnostics: boolean = false
    ): ExpressionEvaluationResult {
            // Validate expression safety
            const validationError = this.validateExpression(expression);
            if (validationError) {
                    error: validationError,
                    diagnostics: enableDiagnostics ? {
                        expression,
                        evaluationTime: Date.now() - startTime
            // Prepare safe context
            const safeContext = this.createSafeContext(context);
            // Create evaluation function
            // Build function body with strict mode
            const functionBody = `
                    return Boolean(${expression});
                    throw new Error('Expression evaluation failed: ' + e.message);
            // Create and execute function
                value: Boolean(result),
     * Validates an expression for safety
     * @param {string} expression - The expression to validate
     * @returns {string | null} Error message if invalid, null if valid
    private validateExpression(expression: string): string | null {
            return 'Expression must be a non-empty string';
        if (expression.length > 1000) {
            return 'Expression exceeds maximum length of 1000 characters';
        for (const pattern of SafeExpressionEvaluator.DANGEROUS_PATTERNS) {
            if (pattern.test(expression)) {
                return `Expression contains forbidden construct: ${pattern.source}`;
        // Basic syntax validation - ensure balanced parentheses
        let parenCount = 0;
        for (const char of expression) {
            if (char === '(') parenCount++;
            if (char === ')') parenCount--;
            if (parenCount < 0) {
                return 'Unbalanced parentheses in expression';
        if (parenCount !== 0) {
     * Creates a safe context object with only allowed properties
     * @param {Record<string, any>} context - The original context
     * @returns {Record<string, any>} The safe context
    private createSafeContext(context: Record<string, any>): Record<string, any> {
        // Deep clone to prevent modifications to original
        const safeContext: Record<string, any> = {};
        for (const [key, value] of Object.entries(context)) {
            // Skip dangerous property names
            if (this.isDangerousPropertyName(key)) {
            // Clone value safely
            safeContext[key] = this.cloneValue(value);
        return safeContext;
     * Checks if a property name is potentially dangerous
     * @param {string} name - The property name
     * @returns {boolean} True if dangerous
    private isDangerousPropertyName(name: string): boolean {
        const dangerous = [
            '__proto__',
            'constructor',
            'prototype',
            'eval',
            'Function'
        return dangerous.includes(name);
     * Safely clones a value for use in evaluation context
     * @param {any} value - The value to clone
     * @returns {any} The cloned value
    private cloneValue(value: any): any {
        // Primitives are safe
        if (type === 'string' || type === 'number' || type === 'boolean') {
        // Arrays
            return value.map(item => this.cloneValue(item));
        // Plain objects - clone by copying enumerable own properties
        if (type === 'object' && value.constructor === Object) {
            const cloned: Record<string, any> = {};
                if (!this.isDangerousPropertyName(key)) {
                    cloned[key] = this.cloneValue(val);
        // Class instances - shallow clone enumerable properties without recursion
        // to avoid issues with circular references or complex nested objects
        if (type === 'object' && typeof value.constructor === 'function') {
                    // Don't recursively clone class instance properties
                    // Just copy them directly for safe shallow access
                    cloned[key] = val;
        // For other types, try safe string conversion
            // If String() fails, return a placeholder
            return '[Object]';
     * Evaluates multiple expressions and returns all results
     * @param {Array<{expression: string, name?: string}>} expressions - Array of expressions to evaluate
     * @param {Record<string, any>} context - The context object
     * @returns {Record<string, ExpressionEvaluationResult>} Map of results by name or index
    public evaluateMultiple(
        expressions: Array<{expression: string, name?: string}>,
        context: Record<string, any>
    ): Record<string, ExpressionEvaluationResult> {
        const results: Record<string, ExpressionEvaluationResult> = {};
        expressions.forEach((expr, index) => {
            const key = expr.name || `expression_${index}`;
            results[key] = this.evaluate(expr.expression, context);
 * Default instance for convenience
export const defaultExpressionEvaluator = new SafeExpressionEvaluator();