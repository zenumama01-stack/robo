 * Unit tests for APIKeyEngine
 * Tests the main orchestrator for API key operations and authorization
import { vi } from 'vitest';
import { APIKeyEngine, GetAPIKeyEngine, ResetAPIKeyEngine } from './APIKeyEngine';
import { UserInfo, setMockRunViewResult, clearMockRunViewResults, setMockEntity, clearMockEntities } from './__mocks__/core';
import { MJAPIKeyEntity, MJAPIApplicationEntity, MJUserEntity } from './__mocks__/core-entities';
// Note: Mocking is handled by resolve.alias in vitest.config.ts
// @memberjunction/core -> ./__mocks__/core.ts
// @memberjunction/core-entities -> ./__mocks__/core-entities.ts
// Helper to cast mock UserInfo to the expected type (using any to avoid circular dependency)
const asUserInfo = (user: UserInfo): any => user;
describe('APIKeyEngine', () => {
    let engine: APIKeyEngine;
    let contextUser: UserInfo;
        ResetAPIKeyEngine();
        engine = GetAPIKeyEngine();
        contextUser = new UserInfo({ ID: 'test-user', Name: 'Test User', Email: 'test@example.com' });
        clearMockRunViewResults();
        clearMockEntities();
    describe('GenerateAPIKey()', () => {
        it('should generate a key with correct format', () => {
            const { Raw, Hash } = engine.GenerateAPIKey();
            expect(Raw).toMatch(/^mj_sk_[a-f0-9]{64}$/);
            expect(Hash).toHaveLength(64); // SHA-256 hex
        it('should generate unique keys', () => {
            const key1 = engine.GenerateAPIKey();
            const key2 = engine.GenerateAPIKey();
            expect(key1.Raw).not.toBe(key2.Raw);
            expect(key1.Hash).not.toBe(key2.Hash);
    describe('HashAPIKey()', () => {
        it('should produce consistent hashes', () => {
            const key = 'mj_sk_' + 'a'.repeat(64);
            const hash1 = engine.HashAPIKey(key);
            const hash2 = engine.HashAPIKey(key);
            expect(hash1).toBe(hash2);
        it('should produce 64-character hex hash', () => {
            const key = 'mj_sk_' + 'b'.repeat(64);
            const hash = engine.HashAPIKey(key);
            expect(hash).toHaveLength(64);
            expect(hash).toMatch(/^[a-f0-9]+$/);
        it('should produce different hashes for different keys', () => {
            const key1 = 'mj_sk_' + 'a'.repeat(64);
            const key2 = 'mj_sk_' + 'b'.repeat(64);
            expect(engine.HashAPIKey(key1)).not.toBe(engine.HashAPIKey(key2));
    describe('IsValidAPIKeyFormat()', () => {
        it('should accept valid format', () => {
            const validKey = 'mj_sk_' + 'a'.repeat(64);
            expect(engine.IsValidAPIKeyFormat(validKey)).toBe(true);
        it('should reject wrong prefix', () => {
            const invalidKey = 'wrong_' + 'a'.repeat(64);
            expect(engine.IsValidAPIKeyFormat(invalidKey)).toBe(false);
        it('should reject wrong length', () => {
            const shortKey = 'mj_sk_' + 'a'.repeat(32);
            const longKey = 'mj_sk_' + 'a'.repeat(128);
            expect(engine.IsValidAPIKeyFormat(shortKey)).toBe(false);
            expect(engine.IsValidAPIKeyFormat(longKey)).toBe(false);
        it('should reject non-hex characters', () => {
            const invalidKey = 'mj_sk_' + 'g'.repeat(64); // 'g' is not hex
        it('should accept mixed case hex (lowercase)', () => {
            const lowerKey = 'mj_sk_' + 'abcdef0123456789'.repeat(4);
            expect(engine.IsValidAPIKeyFormat(lowerKey)).toBe(true);
    describe('ValidateAPIKey()', () => {
        const validRawKey = 'mj_sk_' + 'a'.repeat(64);
            // Mock the API key entity
            const mockApiKey = new MJAPIKeyEntity({
                ID: 'key-id',
                Hash: engine.HashAPIKey(validRawKey),
                UserID: 'user-id',
                ExpiresAt: null
            setMockRunViewResult('MJ: API Keys', {
                Results: [mockApiKey]
            // Mock the user entity
            const mockUser = new MJUserEntity({
                Email: 'user@example.com',
                IsActive: true
            setMockRunViewResult('Users', {
                Results: [mockUser]
        it('should reject invalid format', async () => {
            const result = await engine.ValidateAPIKey(
                { RawKey: 'invalid-key' },
                asUserInfo(contextUser)
            expect(result.IsValid).toBe(false);
            expect(result.Error).toContain('Invalid API key format');
        it('should validate correct key', async () => {
                { RawKey: validRawKey },
            expect(result.IsValid).toBe(true);
            expect(result.User).toBeDefined();
            expect(result.APIKeyId).toBe('key-id');
            expect(result.APIKeyHash).toBeDefined();
        it('should reject revoked key', async () => {
            const revokedKey = new MJAPIKeyEntity({
                ID: 'revoked-key-id',
                Status: 'Revoked'
                Results: [revokedKey]
            expect(result.Error).toContain('revoked');
        it('should reject expired key', async () => {
            const expiredKey = new MJAPIKeyEntity({
                ID: 'expired-key-id',
                ExpiresAt: new Date('2020-01-01')  // Past date
                Results: [expiredKey]
            expect(result.Error).toContain('expired');
        it('should reject key not found', async () => {
                Results: []
            expect(result.Error).toContain('not found');
        it('should reject inactive user', async () => {
            const inactiveUser = new MJUserEntity({
                Name: 'Inactive User',
                IsActive: false
                Results: [inactiveUser]
            expect(result.Error).toContain('inactive');
        describe('application binding', () => {
                // Mock application
                const mockApp = new MJAPIApplicationEntity({
                    ID: 'app-id',
                    Name: 'MCPServer',
                setMockRunViewResult('MJ: API Applications', {
                    Results: [mockApp]
            it('should allow global key for any application', async () => {
                // Key has no bindings
                setMockRunViewResult('MJ: API Key Applications', {
                    { RawKey: validRawKey, ApplicationName: 'MCPServer' },
            it('should allow key bound to requested application', async () => {
                    Results: [{ APIKeyID: 'key-id', ApplicationID: 'app-id' }]
            it('should reject key bound to different application', async () => {
                    Results: [{ APIKeyID: 'key-id', ApplicationID: 'other-app-id' }]
                expect(result.Error).toContain('not authorized for this application');
            it('should reject unknown application', async () => {
                    { RawKey: validRawKey, ApplicationName: 'UnknownApp' },
                expect(result.Error).toContain('Unknown application');
        it('should return API key hash for subsequent Authorize calls', async () => {
            expect(result.APIKeyHash).toBe(engine.HashAPIKey(validRawKey));
    describe('Authorize()', () => {
        const validHash = 'a'.repeat(64);
            // Mock valid API key
                Hash: validHash,
            // Mock global key (no bindings)
            // Mock scope
            setMockRunViewResult('MJ: API Scopes', {
                Results: [{ ID: 'scope-id', FullPath: 'entity:read', IsActive: true }]
        it('should reject invalid API key', async () => {
            const result = await engine.Authorize(
                'invalid-hash',
                'entity:read',
                'Users',
            expect(result.Allowed).toBe(false);
            expect(result.Reason).toContain('not found');
                validHash,
                'UnknownApp',
            expect(result.Reason).toContain('Unknown application');
        it('should reject inactive application', async () => {
            const inactiveApp = new MJAPIApplicationEntity({
                Results: [inactiveApp]
            expect(result.Reason).toContain('not active');
        it('should allow when enforcement is disabled', async () => {
            const disabledEngine = new APIKeyEngine({ enforcementEnabled: false });
            const result = await disabledEngine.Authorize(
            expect(result.Allowed).toBe(true);
            expect(result.Reason).toContain('Enforcement disabled');
        it('should evaluate scope rules and allow', async () => {
            // App ceiling allows entity:read
            setMockRunViewResult('MJ: API Application Scopes', {
                Results: [{
                    ID: 'app-scope-id',
                    ScopePath: 'entity:read',
                    ResourcePattern: '*',
                    PatternType: 'Include',
                    IsDeny: false,
                    Priority: 0
            // API key has entity:read scope assigned (required with default deny behavior)
            setMockRunViewResult('MJ: API Key Scopes', {
                    ID: 'key-scope-id',
                    APIKeyID: 'key-id',
                    ScopeID: 'scope-id',
                    Scope: 'entity:read',
        it('should include log ID in result', async () => {
            // Mock usage log entity
            const mockLogEntity = {
                ID: 'log-id',
                Save: vi.fn().mockResolvedValue(true)
            setMockEntity('MJ: API Key Usage Logs', mockLogEntity);
                asUserInfo(contextUser),
                { endpoint: '/api', method: 'POST' }
            // LogId may be undefined if logging fails silently, but shouldn't throw
    describe('GetApplicationByName()', () => {
        it('should return application by name', async () => {
            const app = await engine.GetApplicationByName('MCPServer', asUserInfo(contextUser));
            expect(app).not.toBeNull();
            expect(app?.Name).toBe('MCPServer');
        it('should return null for unknown application', async () => {
            const app = await engine.GetApplicationByName('Unknown', asUserInfo(contextUser));
            expect(app).toBeNull();
        it('should cache application lookups', async () => {
            // First call
            await engine.GetApplicationByName('MCPServer', asUserInfo(contextUser));
            // Clear mock results
            // Second call should use cache
    describe('ClearCache()', () => {
        it('should clear all caches', async () => {
            // Populate cache
            engine.ClearCache();
            // Should hit database again (no cache)
    describe('GetAPIKeyEngine() singleton', () => {
            const engine1 = GetAPIKeyEngine();
            const engine2 = GetAPIKeyEngine();
            expect(engine1).toBe(engine2);
        it('should reset with ResetAPIKeyEngine()', () => {
            expect(engine1).not.toBe(engine2);
