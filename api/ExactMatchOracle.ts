 * @fileoverview Exact match oracle implementation
import { IOracle } from './IOracle';
import { OracleInput, OracleConfig, OracleResult } from '../types';
 * Exact Match Oracle.
 * Performs deterministic comparison between expected and actual output.
 * Supports various comparison modes (exact, contains, regex, deep equality).
 * - mode: Comparison mode ('exact' | 'contains' | 'regex' | 'deep' | 'partial')
 * - caseSensitive: Whether string comparisons are case-sensitive (default: true)
 * - ignoreWhitespace: Whether to normalize whitespace before comparison (default: false)
 * - fields: For 'partial' mode, which fields to compare (array of paths)
 * const oracle = new ExactMatchOracle();
 * // Exact match
 * const result1 = await oracle.evaluate({
 *     expectedOutput: { status: 'success', count: 5 },
 *     actualOutput: { status: 'success', count: 5 }
 * }, { mode: 'exact' });
 * // Contains mode (actual must contain all expected fields)
 * const result2 = await oracle.evaluate({
 *     expectedOutput: { status: 'success' },
 *     actualOutput: { status: 'success', count: 5, extra: 'data' }
 * }, { mode: 'contains' });
 * // Regex match
 * const result3 = await oracle.evaluate({
 *     expectedOutput: { pattern: 'sales.*region' },
 *     actualOutput: { response: 'Sales by region report' }
 * }, { mode: 'regex' });
export class ExactMatchOracle implements IOracle {
    readonly type = 'exact-match';
     * Evaluate exact match between expected and actual output.
     * @param input - Oracle input with expected and actual output
     * @param config - Oracle configuration
     * @returns Oracle result with match details
    async evaluate(input: OracleInput, config: OracleConfig): Promise<OracleResult> {
            const mode = (config.mode as string) || 'exact';
            const caseSensitive = config.caseSensitive !== false; // Default true
            const ignoreWhitespace = config.ignoreWhitespace === true; // Default false
            const expected = input.expectedOutput;
            const actual = input.actualOutput;
            if (!expected) {
                    oracleType: this.type,
                    message: 'No expected output provided'
            if (!actual) {
                    message: 'No actual output provided'
            // Perform comparison based on mode
            let result: { passed: boolean; score: number; message: string; details?: unknown };
                    result = this.exactMatch(expected, actual);
                    result = this.containsMatch(expected, actual);
                case 'regex':
                    result = this.regexMatch(expected, actual, caseSensitive);
                case 'deep':
                    result = this.deepEqual(expected, actual);
                case 'partial':
                    result = this.partialMatch(
                        actual,
                        config.fields as string[]
                        message: `Unknown comparison mode: ${mode}`
                message: `Exact match error: ${(error as Error).message}`
     * Exact JSON string match.
    private exactMatch(
        expected: unknown,
        actual: unknown
    ): { passed: boolean; score: number; message: string; details?: unknown } {
        if (expectedStr === actualStr) {
                score: 1.0,
                message: 'Output exactly matches expected'
                message: 'Output does not match expected',
                    expected: expectedStr.substring(0, 200),
                    actual: actualStr.substring(0, 200)
     * Check if actual contains all expected fields.
    private containsMatch(
        const missingFields = this.findMissingFields(expected, actual, '');
        if (missingFields.length === 0) {
                message: 'Output contains all expected fields'
                message: `Missing ${missingFields.length} expected field(s)`,
                details: { missingFields }
     * Find missing fields in actual compared to expected.
    private findMissingFields(
        actual: unknown,
        if (typeof expected === 'object' && expected !== null) {
            if (typeof actual !== 'object' || actual === null) {
                return [path || 'root'];
            const expectedObj = expected as Record<string, unknown>;
            const actualObj = actual as Record<string, unknown>;
            for (const key in expectedObj) {
                const fieldPath = path ? `${path}.${key}` : key;
                if (!(key in actualObj)) {
                    missing.push(fieldPath);
                    const nested = this.findMissingFields(
                        expectedObj[key],
                        actualObj[key],
                    missing.push(...nested);
     * Regex pattern matching.
    private regexMatch(
        caseSensitive: boolean
        // Convert expected to regex patterns
        const patterns = this.extractPatterns(expected);
        const failed: string[] = [];
            const flags = caseSensitive ? '' : 'i';
            const regex = new RegExp(pattern, flags);
            if (!regex.test(actualStr)) {
                failed.push(pattern);
        if (failed.length === 0) {
                message: `All ${patterns.length} pattern(s) matched`
                score: 1 - (failed.length / patterns.length),
                message: `${failed.length} of ${patterns.length} pattern(s) failed`,
                details: { failedPatterns: failed }
     * Extract regex patterns from expected output.
    private extractPatterns(expected: unknown): string[] {
        if (typeof expected === 'string') {
            patterns.push(expected);
        } else if (Array.isArray(expected)) {
            patterns.push(...expected.filter(p => typeof p === 'string'));
        } else if (typeof expected === 'object' && expected !== null) {
            const obj = expected as Record<string, unknown>;
            if (obj.responsePatterns && Array.isArray(obj.responsePatterns)) {
                patterns.push(...obj.responsePatterns.filter(p => typeof p === 'string'));
     * Deep equality check.
    private deepEqual(
    ): { passed: boolean; score: number; message: string } {
        const isEqual = this.deepEquality(expected, actual);
            passed: isEqual,
            score: isEqual ? 1.0 : 0,
            message: isEqual ? 'Output deeply equals expected' : 'Output does not deeply equal expected'
     * Recursive deep equality check.
    private deepEquality(a: unknown, b: unknown): boolean {
        if (a === b) {
        if (a === null || b === null || a === undefined || b === undefined) {
            return a === b;
        if (typeof a !== typeof b) {
        if (Array.isArray(a) && Array.isArray(b)) {
            return a.every((item, index) => this.deepEquality(item, b[index]));
        if (typeof a === 'object' && typeof b === 'object') {
            const aKeys = Object.keys(a as object);
            const bKeys = Object.keys(b as object);
            if (aKeys.length !== bKeys.length) {
            const aObj = a as Record<string, unknown>;
            const bObj = b as Record<string, unknown>;
            return aKeys.every(key =>
                bKeys.includes(key) && this.deepEquality(aObj[key], bObj[key])
     * Partial match on specific fields.
    private partialMatch(
        fields?: string[]
        if (!fields || fields.length === 0) {
                message: 'No fields specified for partial match'
            const expectedValue = this.getFieldValue(expected, field);
            const actualValue = this.getFieldValue(actual, field);
            if (!this.deepEquality(expectedValue, actualValue)) {
                failed.push(field);
                message: `All ${fields.length} field(s) matched`
                score: 1 - (failed.length / fields.length),
                message: `${failed.length} of ${fields.length} field(s) failed`,
                details: { failedFields: failed }
     * Get field value by path (e.g., 'user.name' or 'items[0].id').
    private getFieldValue(obj: unknown, path: string): unknown {
        let current: unknown = obj;
            if (!current || typeof current !== 'object') {
            // Handle array indices
            const arrayMatch = part.match(/^(.+)\[(\d+)\]$/);
                const key = arrayMatch[1];
                const index = parseInt(arrayMatch[2]);
                current = (current as Record<string, unknown>)[key];
                if (!Array.isArray(current)) {
                current = (current as Record<string, unknown>)[part];
