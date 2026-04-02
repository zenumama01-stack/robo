 * @fileoverview Schema validation oracle implementation
 * Schema Validator Oracle.
 * Validates that actual output conforms to an expected JSON schema.
 * Uses JSON Schema draft-07 specification for validation.
 * - schema: JSON Schema object defining expected structure
 * - strict: Whether to fail on additional properties (default: false)
 * const oracle = new SchemaValidatorOracle();
 *     actualOutput: { name: 'John', age: 30 },
 *         responseSchema: {
 *             type: 'object',
 *             required: ['name', 'age'],
 *             properties: {
 *                 name: { type: 'string' },
 *                 age: { type: 'number' }
export class SchemaValidatorOracle implements IOracle {
    readonly type = 'schema-validate';
     * Evaluate actual output against JSON schema.
     * @param input - Oracle input with expected schema and actual output
     * @returns Oracle result with pass/fail and validation details
            // Extract schema from expected outcomes
            const schema = (input.expectedOutput as any)?.responseSchema;
                    message: 'No responseSchema provided in ExpectedOutcomes'
            // Validate actual output against schema
            const validationErrors = this.validateAgainstSchema(
                config.strict as boolean
                    message: 'Output matches expected schema'
                    message: `Schema validation failed: ${validationErrors.join(', ')}`,
                    details: { validationErrors }
                message: `Schema validation error: ${(error as Error).message}`
     * Validate data against JSON schema.
    private validateAgainstSchema(
        schema: Record<string, unknown>,
        strict: boolean = false
        // Simple JSON Schema validation implementation
        // For production, consider using a library like ajv
        this.validateValue(data, schema, 'root', errors, strict);
     * Validate a single value against schema.
    private validateValue(
        errors: string[],
        strict: boolean
        // Check type
        const expectedType = schema.type as string;
        const actualType = this.getType(value);
        if (expectedType && actualType !== expectedType) {
            errors.push(`${path}: Expected type ${expectedType}, got ${actualType}`);
        // Check required properties for objects
        if (actualType === 'object' && value !== null) {
            const obj = value as Record<string, unknown>;
            const required = schema.required as string[] || [];
            const properties = schema.properties as Record<string, Record<string, unknown>> || {};
            for (const requiredProp of required) {
                if (!(requiredProp in obj)) {
                    errors.push(`${path}: Missing required property '${requiredProp}'`);
            // Validate properties
            for (const [key, val] of Object.entries(obj)) {
                if (properties[key]) {
                    this.validateValue(val, properties[key], `${path}.${key}`, errors, strict);
                } else if (strict) {
                    errors.push(`${path}: Unexpected property '${key}'`);
        // Check array items
        if (actualType === 'array' && schema.items) {
            const arr = value as unknown[];
            const itemSchema = schema.items as Record<string, unknown>;
            for (let i = 0; i < arr.length; i++) {
                this.validateValue(arr[i], itemSchema, `${path}[${i}]`, errors, strict);
        // Check enum values
        if (schema.enum) {
            const enumValues = schema.enum as unknown[];
            if (!enumValues.includes(value)) {
                errors.push(
                    `${path}: Value must be one of [${enumValues.join(', ')}], got '${value}'`
        // Check string patterns
        if (actualType === 'string' && schema.pattern) {
            const pattern = new RegExp(schema.pattern as string);
            if (!pattern.test(value as string)) {
                errors.push(`${path}: String does not match pattern ${schema.pattern}`);
        // Check numeric constraints
        if (actualType === 'number') {
            const num = value as number;
            if (schema.minimum != null && num < (schema.minimum as number)) {
                errors.push(`${path}: Value ${num} is less than minimum ${schema.minimum}`);
            if (schema.maximum != null && num > (schema.maximum as number)) {
                errors.push(`${path}: Value ${num} is greater than maximum ${schema.maximum}`);
     * Get JSON Schema type for a value.
    private getType(value: unknown): string {
            return 'array';
        const jsType = typeof value;
        if (jsType === 'boolean') {
        if (jsType === 'number') {
            return Number.isInteger(value) ? 'integer' : 'number';
        if (jsType === 'string') {
        if (jsType === 'object') {
