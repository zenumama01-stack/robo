 * @fileoverview Scope Evaluator for OAuth Authorization
 * Provides helper methods for tools to evaluate scopes from JWT claims.
 * This module enables scope-based access control for MCP tools.
 * @module @memberjunction/ai-mcp-server/auth/ScopeEvaluator
 * Scope evaluator for checking granted permissions.
 * Used by tools to verify the authenticated user has required scopes.
export class ScopeEvaluator {
  private readonly scopes: Set<string>;
   * Creates a new ScopeEvaluator with the given granted scopes.
   * @param grantedScopes - Array of scope names granted to the user
  constructor(grantedScopes: string[]) {
    this.scopes = new Set(grantedScopes);
   * Checks if a specific scope is granted.
   * @param scope - The scope name to check (e.g., "entity:read")
   * @returns true if the scope is granted
   * const evaluator = new ScopeEvaluator(['entity:read', 'entity:write']);
   * evaluator.hasScope('entity:read');  // true
   * evaluator.hasScope('action:execute');  // false
  hasScope(scope: string): boolean {
    return this.scopes.has(scope);
   * Checks if any of the specified scopes is granted.
   * @param scopes - Array of scope names to check
   * @returns true if at least one scope is granted
   * const evaluator = new ScopeEvaluator(['entity:read']);
   * evaluator.hasAnyScope(['entity:read', 'entity:write']);  // true
   * evaluator.hasAnyScope(['action:execute', 'agent:run']);  // false
  hasAnyScope(scopes: string[]): boolean {
    return scopes.some((scope) => this.scopes.has(scope));
   * Checks if all specified scopes are granted.
   * @returns true if all scopes are granted
   * evaluator.hasAllScopes(['entity:read', 'entity:write']);  // true
   * evaluator.hasAllScopes(['entity:read', 'action:execute']);  // false
  hasAllScopes(scopes: string[]): boolean {
    return scopes.every((scope) => this.scopes.has(scope));
   * Gets all granted scopes.
   * @returns Array of granted scope names
  getScopes(): string[] {
    return Array.from(this.scopes);
   * Gets scopes matching a pattern.
   * Supports glob-style wildcards: `*` matches any characters.
   * @param pattern - Scope pattern with optional wildcards (e.g., "entity:*")
   * @returns Array of matching scope names
   * const evaluator = new ScopeEvaluator(['entity:read', 'entity:write', 'action:execute']);
   * evaluator.getScopesMatching('entity:*');  // ['entity:read', 'entity:write']
   * evaluator.getScopesMatching('*:read');    // ['entity:read']
  getScopesMatching(pattern: string): string[] {
    // Convert glob pattern to regex
      .replace(/[.+^${}()|[\]\\]/g, '\\$&')  // Escape regex special chars
      .replace(/\*/g, '.*');                  // Convert * to .*
    const regex = new RegExp(`^${regexPattern}$`);
    return this.getScopes().filter((scope) => regex.test(scope));
   * Checks if the evaluator has no scopes (empty).
   * @returns true if no scopes are granted
  isEmpty(): boolean {
    return this.scopes.size === 0;
   * Gets the count of granted scopes.
   * @returns Number of granted scopes
  get count(): number {
    return this.scopes.size;
 * Creates a ScopeEvaluator from a JWT claims object.
 * @param claims - JWT claims containing a 'scopes' array
 * @returns ScopeEvaluator instance
 * const jwt = { scopes: ['entity:read', 'action:execute'], ... };
 * const evaluator = createScopeEvaluator(jwt);
 * if (!evaluator.hasScope('entity:write')) {
 *   throw new Error('Permission denied: entity:write scope required');
export function createScopeEvaluator(claims: { scopes?: string[] }): ScopeEvaluator {
  return new ScopeEvaluator(claims.scopes ?? []);
 * Checks if a specific scope is present in a claims object.
 * Convenience function for simple scope checks without creating an evaluator.
 * @param scope - The scope name to check
export function checkScope(claims: { scopes?: string[] }, scope: string): boolean {
  return claims.scopes?.includes(scope) ?? false;
 * Checks if any of the specified scopes are present in a claims object.
 * @param scopes - The scope names to check
 * @returns true if any scope is granted
export function checkAnyScope(claims: { scopes?: string[] }, scopes: string[]): boolean {
  if (!claims.scopes) return false;
  return scopes.some((scope) => claims.scopes!.includes(scope));
 * Checks if all specified scopes are present in a claims object.
export function checkAllScopes(claims: { scopes?: string[] }, scopes: string[]): boolean {
  return scopes.every((scope) => claims.scopes!.includes(scope));
 * Scope Evaluator for API Key Authorization
 * Implements two-level evaluation: Application Ceiling -> Key Scopes
 * Uses APIKeysEngineBase for cached metadata access.
    MJAPIKeyScopeEntity
    EvaluatedRule,
    ScopeRule,
    ScopeRuleMatch
 * Internal result from evaluating a single level (app or key)
interface LevelEvaluationResult {
    matchedRule: ScopeRuleMatch | undefined;
    evaluatedRules: EvaluatedRule[];
 * Evaluates API key authorization using two-level scope evaluation:
 * 1. Application Ceiling (APIApplicationScope) - what the app allows
 * 2. Key Scopes (APIKeyScope) - what the key grants
 * Both levels support pattern matching with Include/Exclude and Deny rules.
 * Deny rules always trump Allow rules at the same priority level.
 * This class now uses APIKeysEngineBase for cached metadata access,
 * eliminating redundant per-request database queries.
    private _defaultBehaviorNoScopes: 'allow' | 'deny';
    constructor(defaultBehaviorNoScopes: 'allow' | 'deny' = 'allow') {
        this._defaultBehaviorNoScopes = defaultBehaviorNoScopes;
     * Access to the cached metadata from APIKeysEngineBase
     * Evaluate authorization for a request.
     * Uses cached metadata from APIKeysEngineBase.
     * @param request - The authorization request
     * @param _contextUser - The user context (kept for API compatibility)
     * @returns Authorization result with detailed evaluation info
    public async EvaluateAccess(
        request: AuthorizationRequest,
    ): Promise<AuthorizationResult> {
        const evaluatedRules: EvaluatedRule[] = [];
        // 1. Check if API key is bound to specific applications (from Base cache)
        const keyApps = this.Base.GetKeyApplicationsByKeyId(request.APIKeyId);
            const boundToThisApp = keyApps.some(ka => ka.ApplicationID === request.ApplicationId);
                    Reason: 'API key not authorized for this application',
        // If keyApps is empty, key works with all applications
        // 2. Evaluate application scope ceiling
        const appResult = this.evaluateApplicationCeiling(
            request.ApplicationId,
            request.ScopePath,
            request.Resource
        evaluatedRules.push(...appResult.evaluatedRules);
        if (!appResult.allowed) {
                Reason: appResult.reason,
                MatchedAppRule: appResult.matchedRule,
                EvaluatedRules: evaluatedRules
        // 3. Evaluate API key scope rules
        const keyResult = this.evaluateKeyScopes(
            request.APIKeyId,
        evaluatedRules.push(...keyResult.evaluatedRules);
            Allowed: keyResult.allowed,
            Reason: keyResult.reason,
            MatchedKeyRule: keyResult.matchedRule,
     * Clear cache - delegates to Base engine
     * @deprecated Use APIKeysEngineBase.Instance.Config(true) to force refresh instead
        // The Base engine manages its own cache now.
        // To force a refresh, call APIKeysEngineBase.Instance.Config(true, contextUser)
     * Evaluate application-level scope ceiling
     * Uses cached data from APIKeysEngineBase
    private evaluateApplicationCeiling(
        applicationId: string,
    ): LevelEvaluationResult {
        const rules = this.getApplicationScopeRules(applicationId, scopePath);
        return this.evaluateRules(rules, resource, 'application');
     * Evaluate key-level scope rules.
     * If the key has no scope rules defined, applies the defaultBehaviorNoScopes setting.
    private evaluateKeyScopes(
        const rules = this.getKeyScopeRules(apiKeyId, scopePath);
        // If key has no scope rules for this scope, apply default behavior
        if (rules.length === 0) {
            if (this._defaultBehaviorNoScopes === 'allow') {
                    allowed: true,
                    reason: 'Key has no scope restrictions (default: allow)',
                    matchedRule: undefined,
                    evaluatedRules: []
            // Default deny behavior falls through to evaluateRules which will return denied
        return this.evaluateRules(rules, resource, 'key');
     * Evaluate a set of rules against a resource
     * Rules are sorted by Priority DESC, IsDeny DESC
    private evaluateRules(
        rules: ScopeRule[],
        level: 'application' | 'key'
        // Sort: Priority DESC, then IsDeny DESC (deny rules first at same priority)
        const sortedRules = [...rules].sort((a, b) => {
            if (b.Priority !== a.Priority) {
                return b.Priority - a.Priority;
            return (b.IsDeny ? 1 : 0) - (a.IsDeny ? 1 : 0);
        for (const rule of sortedRules) {
            const evalResult = this.evaluateSingleRule(rule, resource, level);
            evaluatedRules.push(evalResult);
            if (evalResult.Matched) {
                if (rule.IsDeny) {
                        reason: `${level === 'application' ? 'Application' : 'Key'} denies access via rule ${rule.ID}`,
                        matchedRule: this.toRuleMatch(rule),
                        evaluatedRules
                // First matching allow rule wins
                    reason: `${level === 'application' ? 'Application' : 'Key'} allows access`,
        // No matching rules
        const noMatchReason = level === 'application'
            ? 'Application does not allow this scope/resource combination'
            : 'No matching key scope rules';
            reason: noMatchReason,
     * Evaluate a single rule against a resource
    private evaluateSingleRule(
        rule: ScopeRule,
    ): EvaluatedRule {
        const matchResult = PatternMatcher.match(resource, rule.ResourcePattern);
        let matched: boolean;
        let result: 'Allowed' | 'Denied' | 'NoMatch';
        if (rule.PatternType === 'Include') {
            // Include: grant if pattern matches
            matched = matchResult.matched;
            // Exclude: grant if pattern does NOT match
            matched = !matchResult.matched;
        if (!matched) {
            result = 'NoMatch';
        } else if (rule.IsDeny) {
            result = 'Denied';
            result = 'Allowed';
            Level: level,
            Rule: this.toRuleMatch(rule),
            Matched: matched,
            PatternMatched: matchResult.matchedPattern,
            Result: result
     * Convert a ScopeRule to ScopeRuleMatch
    private toRuleMatch(rule: ScopeRule): ScopeRuleMatch {
            Id: rule.ID,
            ScopeId: rule.ScopeID,
            ScopePath: rule.FullPath,
            Pattern: rule.ResourcePattern,
            PatternType: rule.PatternType,
            IsDeny: rule.IsDeny,
            Priority: rule.Priority
     * Get applications bound to a key.
    public async GetKeyApplications(
    ): Promise<MJAPIKeyApplicationEntity[]> {
        return this.Base.GetKeyApplicationsByKeyId(apiKeyId);
     * Get application scope rules for a scope path.
    private getApplicationScopeRules(
    ): ScopeRule[] {
        // Get the scope by path from cache
        const scope = this.Base.GetScopeByPath(scopePath);
        if (!scope) {
        // Get application scope rules from cache
        const appScopes = this.Base.GetApplicationScopeRules(applicationId, scope.ID);
        return this.toScopeRules(appScopes, scope.FullPath);
     * Get key scope rules for a scope path.
    private getKeyScopeRules(
        // Get key scope rules from cache
        const keyScopes = this.Base.GetKeyScopeRules(apiKeyId, scope.ID);
        return this.toScopeRulesFromKey(keyScopes, scope.FullPath);
     * Convert MJAPIApplicationScopeEntity to ScopeRule
    private toScopeRules(
        entities: MJAPIApplicationScopeEntity[],
        fullPath?: string
        return entities.map(e => ({
            ID: e.ID,
            ScopeID: e.ScopeID,
            FullPath: fullPath || '',
            ResourcePattern: e.ResourcePattern,
            PatternType: e.PatternType as 'Include' | 'Exclude',
            IsDeny: e.IsDeny,
            Priority: e.Priority
     * Convert MJAPIKeyScopeEntity to ScopeRule
    private toScopeRulesFromKey(
        entities: MJAPIKeyScopeEntity[],
