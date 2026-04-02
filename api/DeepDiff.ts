 * @fileoverview Deep difference comparison utility for JavaScript objects
 * This module provides comprehensive utilities to generate detailed diffs between
 * any two JavaScript objects, arrays, or primitive values. It recursively traverses
 * nested structures and produces both machine-readable change objects and human-readable
 * formatted output.
 * - Deep recursive comparison of objects and arrays
 * - Configurable depth limits and output formatting
 * - Path tracking for nested changes
 * - Summary statistics for change types
 * - Human-readable formatted output
 * - Type-safe change tracking
 * @module @memberjunction/global
 * @since 2.63.0
 * const differ = new DeepDiffer();
 * const diff = differ.diff(
 *   { name: 'John', age: 30, hobbies: ['reading'] },
 *   { name: 'John', age: 31, hobbies: ['reading', 'gaming'] }
 * console.log(diff.formatted);
 * // Output:
 * // Modified: age
 * //   Changed from 30 to 31
 * // Modified: hobbies
 * //   Array length changed from 1 to 2
 * // Added: hobbies[1]
 * //   Added "gaming"
 * // With treatNullAsUndefined option
 * const differ = new DeepDiffer({ treatNullAsUndefined: true });
 *   { name: null, status: 'active', oldProp: 'value' },
 *   { name: 'John', status: null, newProp: 'value' }
 * // name: shows as Added (not Modified)
 * // status: shows as Removed (not Modified)
 * // oldProp: shows as Removed
 * // newProp: shows as Added
 * Types of changes that can occur in a deep diff operation
export enum DiffChangeType {
    /** A new property or value was added */
    Added = 'added',
    /** An existing property or value was removed */
    Removed = 'removed',
    /** An existing value was changed to a different value */
    Modified = 'modified',
    /** No change detected (only included when includeUnchanged is true) */
    Unchanged = 'unchanged'
 * Represents a single change detected during diff operation
export interface DiffChange {
    /** The path to the changed value (e.g., "user.profile.name" or "items[2].id") */
    /** The type of change that occurred */
    type: DiffChangeType;
    /** The original value (undefined for Added changes) */
    oldValue?: any;
    /** The new value (undefined for Removed changes) */
    newValue?: any;
    /** Human-readable description of the change */
 * Complete result of a deep diff operation
export interface DeepDiffResult {
    /** Array of all detected changes */
    changes: DiffChange[];
    /** Summary statistics about the diff */
        /** Number of properties/values that were added */
        /** Number of properties/values that were removed */
        /** Number of properties/values that were modified */
        /** Number of properties/values that remained unchanged (if tracked) */
        /** Total number of paths examined */
        totalPaths: number;
    /** Human-readable formatted diff output suitable for display or logging */
    formatted: string;
 * Configuration options for deep diff generation
export interface DeepDiffConfig {
     * Whether to include unchanged paths in the diff results.
     * Useful for seeing the complete structure comparison.
    includeUnchanged: boolean;
     * Maximum depth to traverse in nested objects.
     * Prevents infinite recursion and controls performance.
     * @default 10
     * Maximum string length before truncation in formatted output.
     * Helps keep the output readable for large text values.
    maxStringLength: number;
     * Whether to include array indices in paths (e.g., "items[0]" vs "items").
     * Provides more precise change tracking for arrays.
    includeArrayIndices: boolean;
     * Whether to treat null values as equivalent to undefined.
     * When true, transitions between null and undefined are not considered changes,
     * and null values in the old object are treated as "not present" for new values.
     * Useful for APIs where null and undefined are used interchangeably.
    treatNullAsUndefined: boolean;
     * Custom value formatter for the formatted output.
     * Allows customization of how values are displayed.
     * @param value - The value to format
     * @param type - The type of the value
     * @returns Formatted string representation
    valueFormatter?: (value: any, type: string) => string;
 * Deep difference generator for comparing JavaScript objects, arrays, and primitives.
 * This class provides comprehensive comparison capabilities with configurable
 * output formatting and depth control.
export class DeepDiffer {
    private config: DeepDiffConfig;
     * Creates a new DeepDiffer instance
    constructor(config?: Partial<DeepDiffConfig>) {
            maxDepth: 10,
            maxStringLength: 100,
            includeArrayIndices: true,
            treatNullAsUndefined: false,
     * Generate a deep diff between two values
     * @param oldValue - The original value
     * @param newValue - The new value to compare against
     * @returns Complete diff results including changes, summary, and formatted output
     * const differ = new DeepDiffer({ includeUnchanged: true });
     * const result = differ.diff(
     *   { users: [{ id: 1, name: 'Alice' }] },
     *   { users: [{ id: 1, name: 'Alice Cooper' }] }
    public diff<T = any>(oldValue: T, newValue: T): DeepDiffResult {
        const changes: DiffChange[] = [];
        // Generate the diff recursively
        this.generateDiff(
            changes,
        // Sort changes by path for consistency
        changes.sort((a, b) => a.path.localeCompare(b.path));
            added: changes.filter(c => c.type === DiffChangeType.Added).length,
            removed: changes.filter(c => c.type === DiffChangeType.Removed).length,
            modified: changes.filter(c => c.type === DiffChangeType.Modified).length,
            unchanged: changes.filter(c => c.type === DiffChangeType.Unchanged).length,
            totalPaths: changes.length
        // Generate formatted output
        const formatted = this.formatDiff(changes, summary);
            formatted
     * Update configuration options
     * @param config - Partial configuration to merge with existing config
    public updateConfig(config: Partial<DeepDiffConfig>): void {
     * Recursively generate diff between two values
    private generateDiff(
        oldValue: any,
        newValue: any,
        changes: DiffChange[],
        depth: number
        // Check depth limit
        if (depth > this.config.maxDepth) {
        // Helper to check if a value is effectively undefined (includes null if treatNullAsUndefined is true)
        const isEffectivelyUndefined = (value: any): boolean => {
            return value === undefined || (this.config.treatNullAsUndefined && value === null);
        // Helper to check if values are effectively equal
        const areEffectivelyEqual = (val1: any, val2: any): boolean => {
            if (val1 === val2) return true;
            if (this.config.treatNullAsUndefined && isEffectivelyUndefined(val1) && isEffectivelyUndefined(val2)) {
        // Handle different cases
        if (areEffectivelyEqual(oldValue, newValue)) {
            if (this.config.includeUnchanged) {
                    path: pathStr || 'root',
                    type: DiffChangeType.Unchanged,
                    description: 'No change'
        } else if (isEffectivelyUndefined(oldValue) && !isEffectivelyUndefined(newValue)) {
                type: DiffChangeType.Added,
                description: this.describeValue(newValue, 'Added')
        } else if (!isEffectivelyUndefined(oldValue) && isEffectivelyUndefined(newValue)) {
                type: DiffChangeType.Removed,
                description: this.describeValue(oldValue, 'Removed')
        } else if (Array.isArray(oldValue) && Array.isArray(newValue)) {
            this.diffArrays(oldValue, newValue, path, changes, depth);
        } else if (_.isObject(oldValue) && _.isObject(newValue)) {
            this.diffObjects(oldValue, newValue, path, changes, depth);
            // Values are different types or primitives
                type: DiffChangeType.Modified,
                description: `Changed from ${this.describeValue(oldValue)} to ${this.describeValue(newValue)}`
     * Compare two arrays and generate diff
    private diffArrays(
        oldArray: any[],
        newArray: any[],
        // Check if array lengths are different
        if (oldArray.length !== newArray.length) {
                oldValue: `Array[${oldArray.length}]`,
                newValue: `Array[${newArray.length}]`,
                description: `Array length changed from ${oldArray.length} to ${newArray.length}`
        // Compare array elements
        const maxLength = Math.max(oldArray.length, newArray.length);
        for (let i = 0; i < maxLength; i++) {
            const elementPath = this.config.includeArrayIndices 
                ? [...path, `[${i}]`]
                : [...path, `[]`];
            if (i < oldArray.length && i < newArray.length) {
                // Element exists in both arrays
                this.generateDiff(oldArray[i], newArray[i], elementPath, changes, depth + 1);
            } else if (i < oldArray.length) {
                // Element was removed
                    path: elementPath.join('.'),
                    oldValue: oldArray[i],
                    description: this.describeValue(oldArray[i], 'Removed')
                // Element was added
                    newValue: newArray[i],
                    description: this.describeValue(newArray[i], 'Added')
     * Compare two objects and generate diff
    private diffObjects(
        oldObj: any,
        newObj: any,
        // Get all unique keys from both objects
        const allKeys = new Set([
            ...Object.keys(oldObj),
            ...Object.keys(newObj)
        // Compare each key
            const keyPath = [...path, key];
                oldObj[key],
                newObj[key],
                depth + 1
     * Create a human-readable description of a value
    private describeValue(value: any, prefix?: string): string {
        if (this.config.valueFormatter) {
            const type = Array.isArray(value) ? 'array' : typeof value;
            return this.config.valueFormatter(value, type);
        const description = this.getValueDescription(value);
        return prefix ? `${prefix} ${description}` : description;
     * Get a description of a value for display
    private getValueDescription(value: any): string {
        const type = typeof value;
        if (type === 'string') {
            const str = value.length > this.config.maxStringLength
                ? `"${value.substring(0, this.config.maxStringLength)}..."`
                : `"${value}"`;
        if (type === 'number' || type === 'boolean') {
            return `Array[${value.length}]`;
        if (_.isObject(value)) {
            return `Object{${keys.length} ${keys.length === 1 ? 'key' : 'keys'}}`;
     * Format the diff results as a human-readable string
    private formatDiff(changes: DiffChange[], summary: DeepDiffResult['summary']): string {
        // Add summary header
        lines.push('=== Deep Diff Summary ===');
        lines.push(`Total changes: ${summary.added + summary.removed + summary.modified}`);
        if (summary.added > 0) lines.push(`  Added: ${summary.added}`);
        if (summary.removed > 0) lines.push(`  Removed: ${summary.removed}`);
        if (summary.modified > 0) lines.push(`  Modified: ${summary.modified}`);
        if (this.config.includeUnchanged && summary.unchanged > 0) {
            lines.push(`  Unchanged: ${summary.unchanged}`);
        // Add changes
        if (changes.length > 0) {
            lines.push('=== Changes ===');
            // Group changes by type for better readability
            const changesByType = _.groupBy(changes, 'type');
            for (const type of [DiffChangeType.Added, DiffChangeType.Removed, DiffChangeType.Modified]) {
                const typeChanges = changesByType[type];
                if (typeChanges && typeChanges.length > 0) {
                    lines.push(`\n${type.charAt(0).toUpperCase() + type.slice(1)}:`);
                    for (const change of typeChanges) {
                        lines.push(`  ${change.path}: ${change.description}`);
            if (this.config.includeUnchanged && changesByType[DiffChangeType.Unchanged]) {
                lines.push(`\nUnchanged (${changesByType[DiffChangeType.Unchanged].length} items)`);
