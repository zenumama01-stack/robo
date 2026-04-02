 * Pattern Matcher for API Key Scope Authorization
 * Supports glob-style patterns with wildcards
 * Result of a pattern match operation
export interface PatternMatchResult {
    /** Whether any pattern matched */
    matched: boolean;
    /** The specific pattern that matched (null if no match) */
    matchedPattern: string | null;
 * Pattern matcher supporting glob-style wildcards
 * Patterns can be:
 * - Exact match: "Users"
 * - Wildcard: "*" (matches anything)
 * - Prefix: "Skip*" (matches "SkipAnalysis", "SkipReport", etc.)
 * - Suffix: "*Agent" (matches "SkipAgent", "DataAgent", etc.)
 * - Contains: "*Report*" (matches "DailyReportSummary")
 * - Single char: "User?" (matches "Users", "User1", etc.)
 * - Comma-separated list: "Users,Accounts,Products"
export class PatternMatcher {
     * Check if a value matches a pattern or comma-separated list of patterns
     * @param value - The value to test
     * @param pattern - The pattern or comma-separated patterns to match against
     * @returns PatternMatchResult with match status and the specific pattern that matched
    public static match(value: string, pattern: string | null): PatternMatchResult {
        // NULL pattern = wildcard (match all)
        if (pattern === null || pattern === undefined) {
            return { matched: true, matchedPattern: '*' };
        // Empty pattern = no match
        if (pattern.trim() === '') {
            return { matched: false, matchedPattern: null };
        // Handle comma-separated list
        const patterns = pattern.split(',').map(p => p.trim()).filter(p => p.length > 0);
        for (const p of patterns) {
            if (this.matchSinglePattern(value, p)) {
                return { matched: true, matchedPattern: p };
     * Check if a value matches a single pattern (not comma-separated)
     * @param pattern - The single pattern to match against
     * @returns true if matched, false otherwise
    private static matchSinglePattern(value: string, pattern: string): boolean {
        // Universal wildcard
        const regexPattern = this.globToRegex(pattern);
        const regex = new RegExp(`^${regexPattern}$`, 'i'); // Case-insensitive
        return regex.test(value);
     * Convert a glob pattern to a regex pattern string
     * @param glob - The glob pattern
     * @returns Regex pattern string (without anchors)
    private static globToRegex(glob: string): string {
        // Escape regex special characters except * and ?
        let result = glob.replace(/[.+^${}()|[\]\\]/g, '\\$&');
        // Convert glob wildcards to regex
        result = result.replace(/\*/g, '.*');  // * -> .*
        result = result.replace(/\?/g, '.');   // ? -> .
     * Check if a pattern contains wildcards
     * @param pattern - The pattern to check
     * @returns true if pattern contains wildcards
    public static hasWildcards(pattern: string | null): boolean {
            return true; // NULL is treated as wildcard
        return pattern.includes('*') || pattern.includes('?');
     * Get all patterns from a comma-separated string
     * @param pattern - Comma-separated pattern string
     * @returns Array of individual patterns
    public static parsePatterns(pattern: string | null): string[] {
            return ['*'];
        return pattern.split(',').map(p => p.trim()).filter(p => p.length > 0);
     * Validate that a pattern is well-formed
     * @param pattern - The pattern to validate
    public static isValidPattern(pattern: string | null): boolean {
            return true; // NULL is valid (wildcard)
        // Check for empty string
        // Check each pattern in comma-separated list
        const patterns = pattern.split(',').map(p => p.trim());
            // Pattern must have at least one non-whitespace character
            if (p.length === 0) {
            // Avoid regex injection - only allow alphanumeric, *, ?, :, -, _, and space
            if (!/^[a-zA-Z0-9*?:\-_ ]+$/.test(p)) {
