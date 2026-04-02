 * Unit tests for APIKeys/Engine interface and type definitions
 * Verifies that all exported interfaces conform to expected shapes.
    APIKeyValidationResult,
    KeyScopeRule,
} from '../interfaces';
describe('GeneratedAPIKey interface', () => {
    it('should accept raw and hash values', () => {
        const key: GeneratedAPIKey = {
            Raw: 'mj_sk_abc123',
            Hash: 'sha256hashvalue',
        expect(key.Raw).toBe('mj_sk_abc123');
        expect(key.Hash).toBe('sha256hashvalue');
describe('CreateAPIKeyParams interface', () => {
    it('should accept minimal params', () => {
        const params: CreateAPIKeyParams = {
            Label: 'My Test Key',
        expect(params.UserId).toBe('user-1');
        expect(params.Label).toBe('My Test Key');
        expect(params.Description).toBeUndefined();
        expect(params.ExpiresAt).toBeUndefined();
    it('should accept full params', () => {
        const expiry = new Date('2026-12-31');
            Label: 'Production Key',
            Description: 'Used for CI/CD',
            ExpiresAt: expiry,
        expect(params.Description).toBe('Used for CI/CD');
        expect(params.ExpiresAt).toBe(expiry);
describe('CreateAPIKeyResult interface', () => {
    it('should represent success', () => {
        const result: CreateAPIKeyResult = {
            RawKey: 'mj_sk_abc',
        expect(result.RawKey).toBeDefined();
    it('should represent failure', () => {
            Error: 'Duplicate label',
        expect(result.Error).toBe('Duplicate label');
describe('APIKeyValidationOptions interface', () => {
    it('should accept minimal options', () => {
        const options: APIKeyValidationOptions = {
        expect(options.RawKey).toBe('mj_sk_abc');
    it('should accept full options', () => {
            ApplicationName: 'TestApp',
            Endpoint: '/api/v1/users',
            Method: 'GET',
            Operation: 'listUsers',
            ResponseTimeMs: 150,
            IPAddress: '192.168.1.1',
            UserAgent: 'Mozilla/5.0',
        expect(options.ApplicationId).toBe('app-1');
        expect(options.StatusCode).toBe(200);
describe('APIKeyValidationResult interface', () => {
    it('should represent valid key', () => {
        const result: APIKeyValidationResult = {
            APIKeyHash: 'hash-value',
    it('should represent invalid key', () => {
            IsValid: false,
            Error: 'Key expired',
        expect(result.Error).toBe('Key expired');
describe('AuthorizationRequest interface', () => {
    it('should accept all required fields', () => {
            ScopePath: 'view:run',
        expect(request.ScopePath).toBe('view:run');
        expect(request.Resource).toBe('Users');
    it('should accept optional context', () => {
            Resource: 'SkipAgent',
            Context: { environment: 'production' },
        expect(request.Context).toEqual({ environment: 'production' });
describe('AuthorizationResult interface', () => {
    it('should represent allowed result', () => {
        const result: AuthorizationResult = {
            Reason: 'Key allows access',
            EvaluatedRules: [],
    it('should represent denied result with matched rules', () => {
        const match: ScopeRuleMatch = {
            Id: 'rule-1',
            ScopeId: 'scope-1',
            Pattern: 'AuditLogs*',
            PatternType: 'Exclude',
            Priority: 100,
            Reason: 'Application denies access via rule rule-1',
            MatchedAppRule: match,
        expect(result.MatchedAppRule?.IsDeny).toBe(true);
describe('ScopeRule interface', () => {
    it('should accept an application scope rule', () => {
        const rule: ApplicationScopeRule = {
            ID: 'rule-1',
            ScopeID: 'scope-1',
            FullPath: 'view:run',
            Priority: 10,
            ApplicationID: 'app-1',
        expect(rule.ApplicationID).toBe('app-1');
    it('should accept a key scope rule', () => {
        const rule: KeyScopeRule = {
            ID: 'rule-2',
            FullPath: 'agent:execute',
            Priority: 5,
            APIKeyID: 'key-1',
        expect(rule.APIKeyID).toBe('key-1');
describe('EvaluatedRule interface', () => {
    it('should represent a matched allow rule', () => {
        const evalRule: EvaluatedRule = {
            Level: 'application',
                Pattern: '*',
            Matched: true,
            PatternMatched: '*',
            Result: 'Allowed',
        expect(evalRule.Level).toBe('application');
        expect(evalRule.Result).toBe('Allowed');
    it('should represent a non-matching rule', () => {
            Level: 'key',
                Id: 'rule-2',
                ScopeId: 'scope-2',
            Matched: false,
            PatternMatched: null,
            Result: 'NoMatch',
        expect(evalRule.Result).toBe('NoMatch');
describe('UsageLogEntry interface', () => {
    it('should accept full log entry', () => {
        const entry: UsageLogEntry = {
            UserAgent: 'curl/7.68',
            RequestedResource: 'Users',
        expect(entry.AuthorizationResult).toBe('Allowed');
        expect(entry.DeniedReason).toBeNull();
    it('should accept denied entry', () => {
            ApplicationId: null,
            Endpoint: '/api/v1/audit',
            Method: 'POST',
            StatusCode: 403,
            RequestedResource: 'AuditLogs',
            DeniedReason: 'Scope not granted',
        expect(entry.AuthorizationResult).toBe('Denied');
describe('APIKeyEngineConfig interface', () => {
    it('should accept empty config', () => {
        const config: APIKeyEngineConfig = {};
        expect(config.enforcementEnabled).toBeUndefined();
    it('should accept full config', () => {
        const config: APIKeyEngineConfig = {
            enforcementEnabled: true,
            loggingEnabled: true,
            scopeCacheTTLMs: 120000,
        expect(config.enforcementEnabled).toBe(true);
        expect(config.defaultBehaviorNoScopes).toBe('allow');
    it('should accept deny as default behavior', () => {
            defaultBehaviorNoScopes: 'deny',
        expect(config.defaultBehaviorNoScopes).toBe('deny');
    EnableFieldEncryptionResult,
    EnableFieldEncryptionParams
 * These tests verify the shape of exported interfaces by creating conforming objects.
 * Since interfaces are erased at runtime, we validate them through structural conformance.
describe('Encryption Interfaces', () => {
    describe('EncryptionKeySourceConfig', () => {
        it('should allow creating a minimal config', () => {
            const config: EncryptionKeySourceConfig = {};
            expect(config).toBeDefined();
        it('should allow creating a config with lookupValue', () => {
            const config: EncryptionKeySourceConfig = {
                lookupValue: 'MJ_ENCRYPTION_KEY_PII'
            expect(config.lookupValue).toBe('MJ_ENCRYPTION_KEY_PII');
        it('should allow creating a config with additionalConfig', () => {
                additionalConfig: { vaultUrl: 'https://vault.example.com', timeout: 5000 }
            expect(config.additionalConfig).toBeDefined();
            expect(config.additionalConfig!.vaultUrl).toBe('https://vault.example.com');
    describe('EncryptedValueParts', () => {
        it('should represent a GCM encrypted value with all parts', () => {
            const parts: EncryptedValueParts = {
                keyId: '550e8400-e29b-41d4-a716-446655440000',
                algorithm: 'AES-256-GCM',
                iv: 'base64iv',
                ciphertext: 'base64ciphertext',
                authTag: 'base64authtag'
            expect(parts.authTag).toBeDefined();
        it('should represent a CBC encrypted value without authTag', () => {
                algorithm: 'AES-256-CBC',
                ciphertext: 'base64ciphertext'
    describe('KeyConfiguration', () => {
        it('should represent a complete key config', () => {
            const config: KeyConfiguration = {
                    lookupValue: 'MJ_ENCRYPTION_KEY'
            expect(config.algorithm.keyLengthBits).toBe(256);
            expect(config.algorithm.isAEAD).toBe(true);
            expect(config.source.driverClass).toBe('EnvVarKeySource');
    describe('RotateKeyResult', () => {
        it('should represent a successful rotation', () => {
            const result: RotateKeyResult = {
                recordsProcessed: 150,
                fieldsProcessed: ['Users.SSN', 'Contacts.Email']
            expect(result.fieldsProcessed).toHaveLength(2);
        it('should represent a failed rotation', () => {
                recordsProcessed: 50,
                fieldsProcessed: ['Users.SSN'],
                error: 'Key material not found'
            expect(result.error).toBe('Key material not found');
    describe('RotateKeyParams', () => {
        it('should represent rotation params with defaults', () => {
            const params: RotateKeyParams = {
                encryptionKeyId: '550e8400-e29b-41d4-a716-446655440000',
                newKeyLookupValue: 'MJ_ENCRYPTION_KEY_V2'
            expect(params.batchSize).toBeUndefined();
        it('should represent rotation params with custom batch size', () => {
                newKeyLookupValue: 'MJ_ENCRYPTION_KEY_V2',
                batchSize: 500
            expect(params.batchSize).toBe(500);
    describe('EnableFieldEncryptionResult', () => {
        it('should represent a successful encryption result', () => {
            const result: EnableFieldEncryptionResult = {
                recordsEncrypted: 100,
                recordsSkipped: 5
            expect(result.recordsEncrypted).toBe(100);
        it('should represent a failed encryption result', () => {
                recordsEncrypted: 0,
                recordsSkipped: 0,
                error: 'Key not found'
            expect(result.error).toBe('Key not found');
    describe('EnableFieldEncryptionParams', () => {
        it('should represent params with required fields', () => {
            const params: EnableFieldEncryptionParams = {
                entityFieldId: '550e8400-e29b-41d4-a716-446655440000'
            expect(params.entityFieldId).toBeDefined();
        it('should represent params with optional batch size', () => {
                entityFieldId: '550e8400-e29b-41d4-a716-446655440000',
                batchSize: 200
            expect(params.batchSize).toBe(200);
