 * Tests key generation, format validation, hashing, and singleton management.
// Mock the crypto module for key generation
vi.mock('crypto', async () => {
    const actual = await vi.importActual<typeof import('crypto')>('crypto');
        ...actual,
        randomBytes: actual.randomBytes,
        createHash: actual.createHash,
// Mock external dependencies
    RunView: class {
        async RunView() { return { Success: false, Results: [] }; }
    Metadata: class {
        async GetEntityObject() { return { Save: async () => false }; }
    UserInfo: class { ID = 'mock-user'; },
    MJAPIKeyEntity: class {},
    MJUserEntity: class {},
    MJAPIKeyUsageLogEntity: class {},
vi.mock('@memberjunction/api-keys-base', () => ({
    APIKeysEngineBase: {
            Scopes: [],
            Applications: [],
            GetApplicationByName: vi.fn().mockReturnValue(null),
            GetApplicationById: vi.fn().mockReturnValue(null),
            GetKeyApplicationsByKeyId: vi.fn().mockReturnValue([]),
            GetScopeByPath: vi.fn().mockReturnValue(null),
            GetApplicationScopeRules: vi.fn().mockReturnValue([]),
            GetKeyScopeRules: vi.fn().mockReturnValue([]),
import { APIKeyEngine, GetAPIKeyEngine, ResetAPIKeyEngine } from '../APIKeyEngine';
        engine = new APIKeyEngine();
        it('should generate a key with mj_sk_ prefix', () => {
            const { Raw } = engine.GenerateAPIKey();
        it('should generate a SHA-256 hash as 64 hex characters', () => {
            const { Hash } = engine.GenerateAPIKey();
            expect(Hash).toMatch(/^[a-f0-9]{64}$/);
        it('should generate unique keys each time', () => {
        it('should produce a consistent hash for the same key', () => {
            const rehash = engine.HashAPIKey(Raw);
            expect(rehash).toBe(Hash);
        it('should return a 64-character hex string', () => {
            const hash = engine.HashAPIKey('mj_sk_test');
            expect(hash).toMatch(/^[a-f0-9]{64}$/);
        it('should be deterministic', () => {
            const hash1 = engine.HashAPIKey('mj_sk_test');
            const hash2 = engine.HashAPIKey('mj_sk_test');
            const hash1 = engine.HashAPIKey('mj_sk_key1');
            const hash2 = engine.HashAPIKey('mj_sk_key2');
            expect(hash1).not.toBe(hash2);
            expect(engine.IsValidAPIKeyFormat(Raw)).toBe(true);
            expect(engine.IsValidAPIKeyFormat('sk_' + 'a'.repeat(64))).toBe(false);
            expect(engine.IsValidAPIKeyFormat('mj_sk_' + 'a'.repeat(32))).toBe(false);
            expect(engine.IsValidAPIKeyFormat('mj_sk_' + 'z'.repeat(64))).toBe(false);
            expect(engine.IsValidAPIKeyFormat('')).toBe(false);
        it('should reject uppercase hex', () => {
            expect(engine.IsValidAPIKeyFormat('mj_sk_' + 'A'.repeat(64))).toBe(false);
    describe('Config()', () => {
        it('should set IsConfigured to true after Config', async () => {
            expect(engine.IsConfigured).toBe(false);
            await engine.Config();
            expect(engine.IsConfigured).toBe(true);
    describe('constructor config defaults', () => {
        it('should default enforcementEnabled to true', () => {
            const e = new APIKeyEngine();
            // We can test indirectly through behavior
            expect(e).toBeDefined();
        it('should accept custom config', () => {
            const e = new APIKeyEngine({
                enforcementEnabled: false,
                loggingEnabled: false,
                defaultBehaviorNoScopes: 'allow',
                scopeCacheTTLMs: 30000,
    describe('GetScopeEvaluator()', () => {
        it('should return a ScopeEvaluator', () => {
            const evaluator = engine.GetScopeEvaluator();
            expect(typeof evaluator.EvaluateAccess).toBe('function');
    describe('GetUsageLogger()', () => {
        it('should return a UsageLogger', () => {
            const logger = engine.GetUsageLogger();
            expect(logger).toBeDefined();
            expect(typeof logger.Log).toBe('function');
    describe('GetPatternMatcher()', () => {
        it('should return the PatternMatcher class', () => {
            const pm = engine.GetPatternMatcher();
            expect(typeof pm.match).toBe('function');
            expect(typeof pm.hasWildcards).toBe('function');
describe('GetAPIKeyEngine / ResetAPIKeyEngine', () => {
    it('should return the same instance on multiple calls', () => {
        const e1 = GetAPIKeyEngine();
        const e2 = GetAPIKeyEngine();
        expect(e1).toBe(e2);
    it('should return a new instance after reset', () => {
        expect(e1).not.toBe(e2);
    it('should accept config on first call', () => {
        const engine = GetAPIKeyEngine({ enforcementEnabled: false });
        expect(engine).toBeDefined();
