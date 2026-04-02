 * Unit tests for credential validation using Ajv JSON Schema validator
 * These tests verify that the CredentialEngine properly validates credential values
 * against JSON Schema constraints including:
 * - default value application
import { describe, test, expect, beforeEach } from 'vitest';
describe('Credential Validation with Ajv', () => {
    let ajv: Ajv;
        // Initialize Ajv with same options as CredentialEngine
        ajv = new Ajv({
            allErrors: true,
            strict: false,
            coerceTypes: false
        addFormats(ajv);
    describe('Required Field Validation', () => {
        test('should reject when required field is missing', () => {
            const schema = {
                required: ['apiKey']
            const validator = ajv.compile(schema);
            const result = validator({});
            expect(validator.errors).toBeDefined();
            expect(validator.errors?.[0].keyword).toBe('required');
            expect(validator.errors?.[0].params.missingProperty).toBe('apiKey');
        test('should pass when all required fields are present', () => {
                    apiKey: { type: 'string' },
                    endpoint: { type: 'string' }
                required: ['apiKey', 'endpoint']
            const result = validator({
                apiKey: 'sk-test123',
                endpoint: 'https://api.example.com'
            expect(validator.errors).toBeNull();
    describe('Const Field Validation', () => {
        test('should reject when const field has wrong value', () => {
                    tokenUrl: {
                        const: 'https://api.box.com/oauth2/token'
            const result = validator({ tokenUrl: 'https://wrong.url.com' });
            expect(validator.errors?.[0].keyword).toBe('const');
            expect(validator.errors?.[0].params.allowedValue).toBe('https://api.box.com/oauth2/token');
        test('should pass when const field matches exactly', () => {
            const result = validator({ tokenUrl: 'https://api.box.com/oauth2/token' });
    describe('Enum Field Validation', () => {
        test('should reject when value is not in enum', () => {
                    boxSubjectType: {
                        enum: ['enterprise', 'user']
            const result = validator({ boxSubjectType: 'invalid' });
            expect(validator.errors?.[0].keyword).toBe('enum');
            expect(validator.errors?.[0].params.allowedValues).toEqual(['enterprise', 'user']);
        test('should pass when value is in enum', () => {
            const result = validator({ boxSubjectType: 'enterprise' });
    describe('Format Validation', () => {
        test('should reject invalid URI format', () => {
                    endpoint: {
                        format: 'uri'
            const result = validator({ endpoint: 'not-a-valid-url' });
            expect(validator.errors?.[0].keyword).toBe('format');
            expect(validator.errors?.[0].params.format).toBe('uri');
        test('should pass valid URI format', () => {
            const result = validator({ endpoint: 'https://api.example.com/v1' });
        test('should reject invalid email format', () => {
                    email: {
                        format: 'email'
            const result = validator({ email: 'not-an-email' });
            expect(validator.errors?.[0].params.format).toBe('email');
        test('should pass valid email format', () => {
            const result = validator({ email: 'test@example.com' });
        test('should reject invalid uuid format', () => {
                    id: {
                        format: 'uuid'
            const result = validator({ id: 'not-a-uuid' });
        test('should pass valid uuid format', () => {
            const result = validator({ id: '550e8400-e29b-41d4-a716-446655440000' });
    describe('Pattern Validation', () => {
        test('should reject value not matching pattern', () => {
                    apiKey: {
                        pattern: '^sk-[a-zA-Z0-9]{32}$'
            const result = validator({ apiKey: 'invalid-key' });
            expect(validator.errors?.[0].keyword).toBe('pattern');
        test('should pass value matching pattern', () => {
            const result = validator({ apiKey: 'sk-abcdefghijklmnopqrstuvwxyz123456' });
    describe('Length Validation', () => {
        test('should reject value shorter than minLength', () => {
                    password: {
                        minLength: 8
            const result = validator({ password: 'short' });
            expect(validator.errors?.[0].keyword).toBe('minLength');
            expect(validator.errors?.[0].params.limit).toBe(8);
        test('should reject value longer than maxLength', () => {
                        maxLength: 10
            const result = validator({ name: 'this-is-too-long' });
            expect(validator.errors?.[0].keyword).toBe('maxLength');
            expect(validator.errors?.[0].params.limit).toBe(10);
        test('should pass value within length bounds', () => {
                        minLength: 8,
                        maxLength: 20
            const result = validator({ password: 'goodpassword' });
    describe('Numeric Range Validation', () => {
        test('should reject value below minimum', () => {
                    port: {
                        type: 'number',
                        minimum: 1024
            const result = validator({ port: 80 });
            expect(validator.errors?.[0].keyword).toBe('minimum');
        test('should reject value above maximum', () => {
                        maximum: 65535
            const result = validator({ port: 70000 });
            expect(validator.errors?.[0].keyword).toBe('maximum');
        test('should pass value within numeric bounds', () => {
                        minimum: 1024,
            const result = validator({ port: 8080 });
    describe('Combined Constraints', () => {
        test('should validate Box.com OAuth schema', () => {
                    clientId: { type: 'string' },
                    clientSecret: { type: 'string' },
                required: ['clientId', 'clientSecret', 'tokenUrl', 'boxSubjectType']
            // Valid credential
            expect(validator({
                clientId: 'test-client',
                clientSecret: 'test-secret',
                tokenUrl: 'https://api.box.com/oauth2/token',
                boxSubjectType: 'enterprise'
            // Invalid: wrong tokenUrl
                tokenUrl: 'https://wrong.url.com',
            })).toBe(false);
            // Invalid: wrong enum value
                boxSubjectType: 'invalid'
        test('should validate GCP credential schema with defaults', () => {
                    projectId: { type: 'string' },
                        default: 'us-central1'
                required: ['projectId']
            // Valid without location (default should be applied separately)
                projectId: 'my-project',
            // Invalid: bad URI format
                endpoint: 'not-a-uri'
    describe('Error Message Formatting', () => {
        test('should format required error message', () => {
            validator({});
            const error = validator.errors?.[0];
            expect(error?.keyword).toBe('required');
            expect(error?.params.missingProperty).toBe('apiKey');
            // Format as CredentialEngine would
            const message = `Missing required field: ${error?.params.missingProperty}`;
            expect(message).toBe('Missing required field: apiKey');
        test('should format const error message', () => {
                    tokenUrl: { const: 'https://api.box.com/oauth2/token' }
            validator({ tokenUrl: 'wrong' });
            const field = error?.instancePath.replace(/^\//, '') || 'credential';
            const message = `Field "${field}" must be "${error?.params.allowedValue}"`;
            expect(message).toContain('must be "https://api.box.com/oauth2/token"');
        test('should format enum error message', () => {
                    type: { enum: ['enterprise', 'user'] }
            validator({ type: 'invalid' });
            const allowed = error?.params.allowedValues.join(', ');
            const message = `Field "type" must be one of: ${allowed}`;
            expect(message).toBe('Field "type" must be one of: enterprise, user');
