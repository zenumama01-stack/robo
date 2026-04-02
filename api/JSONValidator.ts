 * @fileoverview JSON validation utility with support for flexible validation rules.
 * This module provides a lightweight JSON validator that supports special field
 * suffixes and validation rules for validating objects against example templates.
 * It's designed to be simpler and more flexible than formal JSON Schema validation.
 * ## Validation Syntax
 * Field names can include validation rules using these patterns:
 * - `fieldName?` - Optional field
 * - `fieldName*` - Required field with any content (wildcard)
 * - `fieldName:rule` - Field with validation rule
 * - `fieldName:rule1:rule2` - Field with multiple validation rules
 * - `fieldName:rule?` - Optional field with validation rule
 * ## Supported Validation Rules
 * **Array Length:**
 * - `[N+]` - Array with at least N elements
 * - `[N-M]` - Array with between N and M elements
 * - `[=N]` - Array with exactly N elements
 * **Type Checking:**
 * - `string` - Must be a string
 * - `number` - Must be a number
 * - `boolean` - Must be a boolean
 * - `object` - Must be an object (not array)
 * - `array` - Must be an array
 * **Value Constraints:**
 * - `!empty` - Non-empty string, array, or object
import { ValidationResult, ValidationErrorInfo, ValidationErrorType } from './ValidationTypes';
 * Parsed validation rule information
interface ParsedFieldInfo {
    /** Clean field name without validation suffixes */
    /** Whether the field is optional (? suffix) */
    isOptional: boolean;
    /** Whether the field is a wildcard (* suffix) */
    isWildcard: boolean;
    /** Array of validation rules to apply */
    validationRules: string[];
 * Array length validation rule
interface ArrayLengthRule {
    type: 'min' | 'range' | 'exact';
    exact?: number;
 * Lightweight JSON validator with flexible validation rules.
 * const validator = new JSONValidator();
 * const template = {
 *   "name": "John Doe",              // Required field
 *   "email?": "user@example.com",    // Optional field
 *   "settings*": {},                 // Required, any content
 *   "tags:[1+]": ["tag1"],          // Array with 1+ items
 *   "age:number": 25                 // Must be number
 * const data = {
 *   name: "Jane Smith",
 *   tags: ["work", "urgent"],
 *   age: 30
 * const result = validator.validate(data, template);
 *   console.log('Validation passed!');
export class JSONValidator {
     * Validates an object against a template with validation rules.
     * @param data - The data object to validate
     * @param template - The template object with validation rules
     * @param path - The current path in the object hierarchy (used internally)
     * @returns ValidationResult with Success flag and any validation errors
    public validate(
        template: unknown,
        path: string = ''
    ): ValidationResult {
        const errors = this.validateObject(data, template, path);
     * Validates an object against a template recursively.
    private validateObject(
    ): ValidationErrorInfo[] {
        const errors: ValidationErrorInfo[] = [];
        // If template is not an object, we don't validate structure
        if (typeof template !== 'object' || template === null || Array.isArray(template)) {
        // Data must be an object if template is an object
        if (typeof data !== 'object' || data === null || Array.isArray(data)) {
            errors.push(new ValidationErrorInfo(
                path || 'root',
                `Expected object but got ${Array.isArray(data) ? 'array' : typeof data}`,
        const dataObj = data as Record<string, unknown>;
        const templateObj = template as Record<string, unknown>;
        // Check each field in the template
        for (const [templateKey, templateValue] of Object.entries(templateObj)) {
            const parsed = this.parseFieldKey(templateKey);
            const fieldPath = path ? `${path}.${parsed.fieldName}` : parsed.fieldName;
            // Check if required field exists
            if (!parsed.isOptional && !parsed.isWildcard && !(parsed.fieldName in dataObj)) {
                    fieldPath,
                    `Required field '${parsed.fieldName}' is missing`,
            // If field exists, validate it
            if (parsed.fieldName in dataObj) {
                const fieldValue = dataObj[parsed.fieldName];
                // Skip validation for wildcard fields
                if (parsed.isWildcard) {
                // Apply validation rules
                if (parsed.validationRules.length > 0) {
                    const ruleErrors = this.applyValidationRules(
                        fieldValue,
                        parsed.validationRules,
                        fieldPath
                    errors.push(...ruleErrors);
                // Recursively validate nested objects (unless validation failed)
                if (errors.filter(e => e.Source === fieldPath).length === 0) {
                    const nestedErrors = this.validateObject(
                        templateValue,
                    errors.push(...nestedErrors);
     * Parses a field key to extract the field name and validation rules.
    private parseFieldKey(key: string): ParsedFieldInfo {
        let workingKey = key.trim();
        let isOptional = false;
        let isWildcard = false;
        const validationRules: string[] = [];
        // Check for optional suffix (?)
        if (workingKey.endsWith('?')) {
            isOptional = true;
            workingKey = workingKey.slice(0, -1);
        // Check for wildcard suffix (*)
        else if (workingKey.endsWith('*')) {
        // Parse validation rules (everything after first :)
        const colonIndex = workingKey.indexOf(':');
        let fieldName = workingKey;
        if (colonIndex !== -1) {
            fieldName = workingKey.substring(0, colonIndex);
            const rulesString = workingKey.substring(colonIndex + 1);
            // Split by : but handle array syntax [N+] specially
            const rules = this.parseValidationRules(rulesString);
            validationRules.push(...rules);
            isOptional,
            isWildcard,
            validationRules
     * Parses validation rules from a string, handling special cases like array syntax.
    private parseValidationRules(rulesString: string): string[] {
        const rules: string[] = [];
        let currentRule = '';
        let inBrackets = false;
        for (const char of rulesString) {
                inBrackets = true;
                currentRule += char;
                inBrackets = false;
            } else if (char === ':' && !inBrackets) {
                if (currentRule) {
                    rules.push(currentRule);
                    currentRule = '';
     * Applies validation rules to a field value.
    private applyValidationRules(
        value: unknown,
        rules: string[],
        fieldPath: string
            // Array length validation
            if (rule.startsWith('[') && rule.endsWith(']')) {
                const lengthErrors = this.validateArrayLength(value, rule, fieldPath);
                errors.push(...lengthErrors);
            else if (['string', 'number', 'boolean', 'object', 'array'].includes(rule)) {
                const typeError = this.validateType(value, rule, fieldPath);
                if (typeError) {
                    errors.push(typeError);
            // Non-empty validation
            else if (rule === '!empty') {
                const emptyError = this.validateNonEmpty(value, fieldPath);
                if (emptyError) {
                    errors.push(emptyError);
     * Validates array length constraints.
    private validateArrayLength(
        rule: string,
                `Array length validation requires an array, but got ${typeof value}`,
        const arrayLength = value.length;
        const constraint = rule.slice(1, -1); // Remove [ and ]
        // Parse the constraint
        if (constraint.endsWith('+')) {
            // Minimum length: [N+]
            const min = parseInt(constraint.slice(0, -1), 10);
            if (arrayLength < min) {
                    `Array must have at least ${min} element${min > 1 ? 's' : ''}, but has ${arrayLength}`,
        } else if (constraint.includes('-')) {
            // Range: [N-M]
            const [minStr, maxStr] = constraint.split('-');
            const min = parseInt(minStr, 10);
            const max = parseInt(maxStr, 10);
            if (arrayLength < min || arrayLength > max) {
                    `Array must have between ${min} and ${max} elements, but has ${arrayLength}`,
        } else if (constraint.startsWith('=')) {
            // Exact: [=N]
            const exact = parseInt(constraint.slice(1), 10);
            if (arrayLength !== exact) {
                    `Array must have exactly ${exact} element${exact > 1 ? 's' : ''}, but has ${arrayLength}`,
     * Validates type constraints.
    private validateType(
        expectedType: string,
    ): ValidationErrorInfo | null {
        const actualType = Array.isArray(value) ? 'array' : typeof value;
        let isValid = false;
        switch (expectedType) {
                isValid = typeof value === 'string';
                isValid = typeof value === 'number' && !isNaN(value as number);
                isValid = typeof value === 'boolean';
                isValid = typeof value === 'object' && value !== null && !Array.isArray(value);
                isValid = Array.isArray(value);
            return new ValidationErrorInfo(
                `Expected ${expectedType} but got ${actualType}`,
     * Validates non-empty constraint.
    private validateNonEmpty(
        let isEmpty = false;
            isEmpty = value.trim().length === 0;
            isEmpty = value.length === 0;
            isEmpty = Object.keys(value).length === 0;
        if (isEmpty) {
                `Field must not be empty`,
     * Validates an object against a JSON schema string.
     * Convenience method that parses the schema and validates.
     * @param data - The data to validate
     * @param schemaJson - JSON string containing the validation schema
    public validateAgainstSchema(
        schemaJson: string
            return this.validate(data, schema);
            const errorMessage = parseError instanceof Error ? parseError.message : String(parseError);
                Errors: [
                    new ValidationErrorInfo(
                        'schema',
                        `Invalid JSON schema: ${errorMessage}`,
                        schemaJson,
     * Cleans validation syntax from JSON object keys.
     * This method recursively processes a JSON object and removes validation
     * syntax markers (?, *, :rules) from all object keys. This is useful when
     * AI models mistakenly include our validation syntax in their responses.
     * interface MyData {
     *   name: string;
     *   items: string[];
     *   config: { enabled: boolean };
     * const dirtyJson = {
     *   "name?": "John",
     *   "items:[1+]": ["a", "b"],
     *   "config*": { "enabled?": true }
     * const cleanJson = validator.cleanValidationSyntax<MyData>(dirtyJson);
     * // Result is typed as MyData
     * @template T - The expected type of the cleaned data
     * @param data - The JSON data to clean
     * @returns A new object with cleaned keys, typed as T
    public cleanValidationSyntax<T>(data: unknown): T {
        // Handle non-objects
        if (data === null || data === undefined) {
            // Recursively clean array elements
            return data.map(item => this.cleanValidationSyntax(item)) as T;
        if (typeof data !== 'object') {
            // Primitive values pass through unchanged
        // Process object
        const cleaned: Record<string, unknown> = {};
        for (const [key, value] of Object.entries(dataObj)) {
            // Parse the key to extract the clean field name
            const parsed = this.parseFieldKey(key);
            // Recursively clean the value
            const cleanedValue = this.cleanValidationSyntax(value);
            // Use the clean field name as the key
            cleaned[parsed.fieldName] = cleanedValue;
        return cleaned as T;
