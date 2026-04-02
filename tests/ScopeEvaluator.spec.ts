 * Unit tests for ScopeEvaluator
 * Tests two-level scope evaluation: Application Ceiling -> Key Scopes
import { UserInfo, setMockRunViewResult, clearMockRunViewResults } from './__mocks__/core';
import { AuthorizationRequest } from './interfaces';
    let evaluator: ScopeEvaluator;
        evaluator = new ScopeEvaluator(60000, 'allow');
        contextUser = new UserInfo({ ID: 'test-user', Name: 'Test User' });
        evaluator.ClearCache();
        it('should accept cache TTL parameter', () => {
            const evaluator = new ScopeEvaluator(30000);
            expect(evaluator).toBeDefined();
        it('should accept defaultBehaviorNoScopes parameter', () => {
            const allowEvaluator = new ScopeEvaluator(60000, 'allow');
            const denyEvaluator = new ScopeEvaluator(60000, 'deny');
            expect(allowEvaluator).toBeDefined();
            expect(denyEvaluator).toBeDefined();
    describe('EvaluateAccess()', () => {
        const baseRequest: AuthorizationRequest = {
            APIKeyId: 'test-key-id',
            UserId: 'test-user-id',
            ApplicationId: 'test-app-id',
            Resource: 'Users'
        describe('application binding check', () => {
            it('should deny if key is bound to different application', async () => {
                // Key is bound to a different app
                    Results: [{ APIKeyID: 'test-key-id', ApplicationID: 'other-app-id' }]
                const result = await evaluator.EvaluateAccess(baseRequest, asUserInfo(contextUser));
                expect(result.Reason).toContain('not authorized for this application');
            it('should allow if key is bound to requested application', async () => {
                // Key is bound to the requested app
                    Results: [{ APIKeyID: 'test-key-id', ApplicationID: 'test-app-id' }]
                // Mock scope to pass app ceiling
                // Mock app ceiling - allow entity:read for Users
                        ApplicationID: 'test-app-id',
            it('should allow global keys (no application bindings)', async () => {
                // Key has no bindings (global)
                // Mock app ceiling
        describe('application ceiling evaluation', () => {
                // Global key
                setMockRunViewResult('MJ: API Key Applications', { Success: true, Results: [] });
            it('should deny if application has no scope rules for requested scope', async () => {
                // Scope exists
                // No app scope rules
                expect(result.Reason).toContain('Application does not allow');
            it('should allow if application ceiling includes the resource', async () => {
                        ResourcePattern: 'Users',
                expect(result.MatchedAppRule).toBeDefined();
                expect(result.MatchedAppRule?.Pattern).toBe('Users');
            it('should deny if application has deny rule', async () => {
                        IsDeny: true,
                expect(result.Reason).toContain('denies access');
        describe('key scope evaluation', () => {
                // App ceiling allows
            it('should allow if key has no scope rules (default: allow)', async () => {
                expect(result.Reason).toContain('no scope restrictions');
            it('should deny if key has no scope rules and default is deny', async () => {
                const result = await denyEvaluator.EvaluateAccess(baseRequest, asUserInfo(contextUser));
            it('should allow if key scope includes the resource', async () => {
                        APIKeyID: 'test-key-id',
                expect(result.MatchedKeyRule).toBeDefined();
            it('should deny if key scope has deny rule', async () => {
                expect(result.Reason).toContain('Key denies access');
        describe('pattern matching', () => {
                    Results: [{ ID: 'scope-id', FullPath: 'agent:execute', IsActive: true }]
            it('should match wildcard patterns', async () => {
                        ResourcePattern: 'Skip*',
                    ...baseRequest,
                    ScopePath: 'agent:execute',
                    Resource: 'SkipAnalysisAgent'
                const result = await evaluator.EvaluateAccess(request, asUserInfo(contextUser));
            it('should not match non-matching wildcards', async () => {
                    Resource: 'DataAnalysisAgent'
                const result = await denyEvaluator.EvaluateAccess(request, asUserInfo(contextUser));
        describe('priority ordering', () => {
            it('should respect priority (higher priority wins)', async () => {
                    Results: [
                            ID: 'key-scope-1',
                            Priority: 0  // Lower priority
                            ID: 'key-scope-2',
                            Priority: 10  // Higher priority - should win
            it('should prefer deny rules at same priority', async () => {
                            Priority: 0  // Same priority, deny should win
    describe('GetKeyApplications()', () => {
        it('should return key applications', async () => {
                    { APIKeyID: 'key-1', ApplicationID: 'app-2' }
            const apps = await evaluator.GetKeyApplications('key-1', asUserInfo(contextUser));
            expect(apps).toHaveLength(2);
        it('should return empty array for global keys', async () => {
            const apps = await evaluator.GetKeyApplications('global-key', asUserInfo(contextUser));
            expect(apps).toHaveLength(0);
        it('should clear all caches', () => {
            // This test just verifies the method doesn't throw
            expect(() => evaluator.ClearCache()).not.toThrow();
