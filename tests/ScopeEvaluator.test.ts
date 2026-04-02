 * Unit tests for MCP Server ScopeEvaluator
    ScopeEvaluator,
    createScopeEvaluator,
    checkScope,
    checkAnyScope,
    checkAllScopes,
} from '../auth/ScopeEvaluator';
describe('ScopeEvaluator', () => {
        it('should create an evaluator with given scopes', () => {
            const evaluator = new ScopeEvaluator(['entity:read', 'entity:write']);
            expect(evaluator.count).toBe(2);
        it('should create an empty evaluator', () => {
            const evaluator = new ScopeEvaluator([]);
            expect(evaluator.isEmpty()).toBe(true);
            expect(evaluator.count).toBe(0);
        it('should deduplicate scopes', () => {
            const evaluator = new ScopeEvaluator(['entity:read', 'entity:read']);
            expect(evaluator.count).toBe(1);
    describe('hasScope()', () => {
        it('should return true for granted scope', () => {
            expect(evaluator.hasScope('entity:read')).toBe(true);
        it('should return false for missing scope', () => {
            const evaluator = new ScopeEvaluator(['entity:read']);
            expect(evaluator.hasScope('entity:write')).toBe(false);
        it('should be exact match (no wildcard)', () => {
            expect(evaluator.hasScope('entity:*')).toBe(false);
    describe('hasAnyScope()', () => {
        it('should return true if at least one scope matches', () => {
            expect(evaluator.hasAnyScope(['entity:read', 'entity:write'])).toBe(true);
        it('should return false if no scopes match', () => {
            expect(evaluator.hasAnyScope(['action:execute', 'agent:run'])).toBe(false);
        it('should return false for empty input', () => {
            expect(evaluator.hasAnyScope([])).toBe(false);
    describe('hasAllScopes()', () => {
        it('should return true when all scopes match', () => {
            const evaluator = new ScopeEvaluator(['entity:read', 'entity:write', 'action:execute']);
            expect(evaluator.hasAllScopes(['entity:read', 'entity:write'])).toBe(true);
        it('should return false when not all scopes match', () => {
            expect(evaluator.hasAllScopes(['entity:read', 'entity:write'])).toBe(false);
        it('should return true for empty input', () => {
            expect(evaluator.hasAllScopes([])).toBe(true);
    describe('getScopes()', () => {
        it('should return all granted scopes', () => {
            const scopes = evaluator.getScopes();
            expect(scopes).toContain('entity:read');
            expect(scopes).toContain('entity:write');
            expect(scopes).toHaveLength(2);
        it('should return empty array when no scopes', () => {
            expect(evaluator.getScopes()).toEqual([]);
    describe('getScopesMatching()', () => {
        it('should match prefix pattern', () => {
            const matched = evaluator.getScopesMatching('entity:*');
            expect(matched).toContain('entity:read');
            expect(matched).toContain('entity:write');
            expect(matched).not.toContain('action:execute');
        it('should match suffix pattern', () => {
            const evaluator = new ScopeEvaluator(['entity:read', 'action:read', 'agent:run']);
            const matched = evaluator.getScopesMatching('*:read');
            expect(matched).toContain('action:read');
            expect(matched).not.toContain('agent:run');
        it('should match all with *', () => {
            const evaluator = new ScopeEvaluator(['a', 'b', 'c']);
            expect(evaluator.getScopesMatching('*')).toHaveLength(3);
        it('should match exact', () => {
            expect(evaluator.getScopesMatching('entity:read')).toEqual(['entity:read']);
        it('should return empty for no matches', () => {
            expect(evaluator.getScopesMatching('action:*')).toEqual([]);
    describe('isEmpty()', () => {
        it('should return true for empty evaluator', () => {
            expect(new ScopeEvaluator([]).isEmpty()).toBe(true);
        it('should return false for non-empty evaluator', () => {
            expect(new ScopeEvaluator(['a']).isEmpty()).toBe(false);
    describe('count getter', () => {
        it('should return the number of scopes', () => {
            expect(new ScopeEvaluator([]).count).toBe(0);
            expect(new ScopeEvaluator(['a']).count).toBe(1);
            expect(new ScopeEvaluator(['a', 'b', 'c']).count).toBe(3);
describe('createScopeEvaluator()', () => {
    it('should create evaluator from claims with scopes', () => {
        const evaluator = createScopeEvaluator({ scopes: ['entity:read'] });
    it('should create empty evaluator from claims without scopes', () => {
        const evaluator = createScopeEvaluator({});
    it('should create empty evaluator from claims with undefined scopes', () => {
        const evaluator = createScopeEvaluator({ scopes: undefined });
describe('checkScope()', () => {
    it('should return true when scope is present', () => {
        expect(checkScope({ scopes: ['entity:read'] }, 'entity:read')).toBe(true);
    it('should return false when scope is missing', () => {
        expect(checkScope({ scopes: ['entity:read'] }, 'entity:write')).toBe(false);
    it('should return false when scopes is undefined', () => {
        expect(checkScope({}, 'entity:read')).toBe(false);
describe('checkAnyScope()', () => {
    it('should return true when any scope matches', () => {
        expect(checkAnyScope({ scopes: ['entity:read'] }, ['entity:read', 'entity:write'])).toBe(true);
    it('should return false when no scope matches', () => {
        expect(checkAnyScope({ scopes: ['entity:read'] }, ['action:execute'])).toBe(false);
        expect(checkAnyScope({}, ['entity:read'])).toBe(false);
describe('checkAllScopes()', () => {
    it('should return true when all scopes present', () => {
        expect(checkAllScopes({ scopes: ['a', 'b'] }, ['a', 'b'])).toBe(true);
    it('should return false when not all present', () => {
        expect(checkAllScopes({ scopes: ['a'] }, ['a', 'b'])).toBe(false);
        expect(checkAllScopes({}, ['a'])).toBe(false);
 * Uses alias-based mock for APIKeysEngineBase cached metadata
import { describe, it, expect, beforeEach } from 'vitest';
import { ScopeEvaluator } from '../ScopeEvaluator';
import { UserInfo } from '../__mocks__/core';
} from '../__mocks__/core-entities';
    setMockBaseScopes,
    setMockBaseApplicationScopes,
    setMockBaseKeyApplications,
    setMockBaseKeyScopes,
    clearMockBaseState,
} from '../__mocks__/api-keys-base';
import type { AuthorizationRequest } from '../interfaces';
        APIKeyId: 'key-1',
        UserId: 'user-1',
        ApplicationId: 'app-1',
        Resource: 'Users',
        clearMockBaseState();
        evaluator = new ScopeEvaluator('deny');
        contextUser = new UserInfo({ ID: 'test-user' });
        // Default: single active scope
        setMockBaseScopes([
            new MJAPIScopeEntity({ ID: 'scope-1', FullPath: 'entity:read', IsActive: true }),
    // CONSTRUCTOR
        it('should default to allow when no argument given', () => {
            const allowEval = new ScopeEvaluator();
            expect(allowEval).toBeDefined();
        it('should accept deny default', () => {
            const denyEval = new ScopeEvaluator('deny');
            expect(denyEval).toBeDefined();
    // APPLICATION BINDING
    describe('EvaluateAccess() - application binding', () => {
        it('should deny if key is bound to a different application', async () => {
            setMockBaseKeyApplications([
                new MJAPIKeyApplicationEntity({ APIKeyID: 'key-1', ApplicationID: 'other-app' }),
            const result = await evaluator.EvaluateAccess(baseRequest, contextUser as never);
        it('should proceed if key is bound to the requested application', async () => {
                new MJAPIKeyApplicationEntity({ APIKeyID: 'key-1', ApplicationID: 'app-1' }),
            setMockBaseApplicationScopes([
                new MJAPIApplicationScopeEntity({
                    ID: 'as-1', ApplicationID: 'app-1', ScopeID: 'scope-1',
                    ResourcePattern: '*', PatternType: 'Include', IsDeny: false, Priority: 0,
            setMockBaseKeyScopes([
                new MJAPIKeyScopeEntity({
                    ID: 'ks-1', APIKeyID: 'key-1', ScopeID: 'scope-1',
            setMockBaseKeyApplications([]);
    // APPLICATION CEILING
    describe('EvaluateAccess() - application ceiling', () => {
        it('should deny if scope path is not found', async () => {
            setMockBaseScopes([]);
        it('should deny if application has no scope rules for the requested scope', async () => {
            setMockBaseApplicationScopes([]);
        it('should allow if app ceiling includes the exact resource', async () => {
                    ResourcePattern: 'Users', PatternType: 'Include', IsDeny: false, Priority: 0,
        it('should deny if app ceiling has a deny rule', async () => {
                    ResourcePattern: '*', PatternType: 'Include', IsDeny: true, Priority: 0,
        it('should support wildcard patterns in app ceiling', async () => {
                    ResourcePattern: 'User*', PatternType: 'Include', IsDeny: false, Priority: 0,
        it('should deny if wildcard does not match resource', async () => {
                    ResourcePattern: 'Admin*', PatternType: 'Include', IsDeny: false, Priority: 0,
    // KEY SCOPES
    describe('EvaluateAccess() - key scopes', () => {
        it('should deny with no key scope rules (default: deny)', async () => {
            setMockBaseKeyScopes([]);
        it('should allow with no key scope rules (default: allow)', async () => {
            const allowEval = new ScopeEvaluator('allow');
            const result = await allowEval.EvaluateAccess(baseRequest, contextUser as never);
                    ResourcePattern: 'Users', PatternType: 'Include', IsDeny: true, Priority: 0,
        it('should support Exclude pattern type (grant when NOT matching)', async () => {
                    ResourcePattern: 'AdminData', PatternType: 'Exclude', IsDeny: false, Priority: 0,
        it('should deny with Exclude pattern type when resource matches', async () => {
                    ResourcePattern: 'Users', PatternType: 'Exclude', IsDeny: false, Priority: 0,
    // PRIORITY ORDERING
    describe('EvaluateAccess() - priority ordering', () => {
                    ID: 'ks-deny', APIKeyID: 'key-1', ScopeID: 'scope-1',
                    ID: 'ks-allow', APIKeyID: 'key-1', ScopeID: 'scope-1',
                    ResourcePattern: 'Users', PatternType: 'Include', IsDeny: false, Priority: 10,
        it('should prefer deny at same priority', async () => {
        it('should prioritize app ceiling rules too', async () => {
                    ID: 'as-deny', ApplicationID: 'app-1', ScopeID: 'scope-1',
                    ID: 'as-allow', ApplicationID: 'app-1', ScopeID: 'scope-1',
    // EVALUATED RULES TRACKING
    describe('EvaluateAccess() - evaluated rules', () => {
        it('should include evaluated rules in result', async () => {
            expect(result.EvaluatedRules.length).toBeGreaterThanOrEqual(2);
            const appRules = result.EvaluatedRules.filter(r => r.Level === 'application');
            const keyRules = result.EvaluatedRules.filter(r => r.Level === 'key');
            expect(appRules.length).toBeGreaterThanOrEqual(1);
            expect(keyRules.length).toBeGreaterThanOrEqual(1);
    // MULTIPLE SCOPES
    describe('EvaluateAccess() - agent scope', () => {
        it('should evaluate agent:execute scope', async () => {
            const agentScope = new MJAPIScopeEntity({ ID: 'scope-agent', FullPath: 'agent:execute', IsActive: true });
            setMockBaseScopes([agentScope]);
                    ID: 'as-1', ApplicationID: 'app-1', ScopeID: 'scope-agent',
                    ResourcePattern: 'Skip*', PatternType: 'Include', IsDeny: false, Priority: 0,
                    ID: 'ks-1', APIKeyID: 'key-1', ScopeID: 'scope-agent',
                Resource: 'SkipAnalysisAgent',
            const result = await evaluator.EvaluateAccess(request, contextUser as never);
    // UTILITY
        it('should return key applications from Base', async () => {
                new MJAPIKeyApplicationEntity({ APIKeyID: 'key-1', ApplicationID: 'app-2' }),
            const apps = await evaluator.GetKeyApplications('key-1', contextUser as never);
        it('should return empty for global keys', async () => {
