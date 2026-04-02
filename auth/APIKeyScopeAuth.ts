 * API Key Scope Authorization Utilities
 * Provides utilities for checking API key scopes in resolvers
 * @module @memberjunction/server
import { AuthorizationError } from 'type-graphql';
import { GetAPIKeyEngine, AuthorizationResult, AuthorizationRequest } from '@memberjunction/api-keys';
import { MJAPIKeyEntity, MJAPIApplicationEntity } from '@memberjunction/core-entities';
 * Application names used by the API Key authorization system
export type ApplicationName = 'MJAPI' | 'MCPServer' | 'A2AServer' | string;
 * Options for scope authorization
export interface ScopeAuthOptions {
    /** The application making the request (default: 'MJAPI') */
    applicationName?: ApplicationName;
    /** Resource being accessed (e.g., entity name, action name) */
    resource?: string;
    /** Whether to throw an error on denied access (default: true) */
    throwOnDenied?: boolean;
 * Result of scope authorization check
export interface ScopeAuthResult {
    /** Whether scope checking was performed (false if no API key or enforcement disabled) */
    Checked: boolean;
    EvaluatedRules?: AuthorizationResult['EvaluatedRules'];
 * Check if an API key has the required scope for an operation.
 * This function implements the three-tier permission model:
 * @param apiKeyId - The API key ID from context.userPayload.apiKeyId
 * @param scopePath - The scope path required (e.g., 'view:run', 'agent:execute')
 * @param contextUser - The authenticated user from context.userPayload.userRecord
 * @param options - Additional options for scope checking
 * @returns ScopeAuthResult with authorization details
 * @throws AuthorizationError if access is denied and throwOnDenied is true
 * // In a resolver
 * async runView(@Ctx() ctx: AppContext): Promise<ViewResult> {
 *   await CheckAPIKeyScope(
 *     ctx.userPayload.apiKeyId,
 *     'view:run',
 *     ctx.userPayload.userRecord,
 *     { resource: 'User' }
 *   );
 *   // ... proceed with operation
export async function CheckAPIKeyScope(
    apiKeyId: string | undefined,
    options: ScopeAuthOptions = {}
): Promise<ScopeAuthResult> {
        applicationName = 'MJAPI',
        resource = '*',
        throwOnDenied = true
    // If no API key ID, not authenticated via API key - skip scope check
    if (!apiKeyId) {
            Checked: false,
            Reason: 'Not authenticated via API key'
    const engine = GetAPIKeyEngine();
    // Get the API key to find the user ID
    const keyResult = await rv.RunView<MJAPIKeyEntity>({
        ExtraFilter: `ID='${apiKeyId}'`,
        const result: ScopeAuthResult = {
            Checked: true,
            Reason: 'API key not found'
        if (throwOnDenied) {
            throw new AuthorizationError(result.Reason);
    const apiKey = keyResult.Results[0];
    // Get the application by name
    const appResult = await rv.RunView<MJAPIApplicationEntity>({
        ExtraFilter: `Name='${applicationName}'`,
    if (!appResult.Success || appResult.Results.length === 0) {
            Reason: `Unknown application: ${applicationName}`
    const app = appResult.Results[0];
            Reason: `Application is not active: ${applicationName}`
    // Build the authorization request
        UserId: apiKey.UserID,
    // Use the scope evaluator directly (since we already have the key ID)
    const scopeEvaluator = engine.GetScopeEvaluator();
    const authResult = await scopeEvaluator.EvaluateAccess(request, contextUser);
    if (!authResult.Allowed && throwOnDenied) {
        const scopeDisplay = resource !== '*' ? `${scopePath} (${resource})` : scopePath;
        throw new AuthorizationError(
            `API key does not have permission for scope: ${scopeDisplay}. ${authResult.Reason || ''}`
        Allowed: authResult.Allowed,
        Reason: authResult.Reason,
        EvaluatedRules: authResult.EvaluatedRules
 * Check if an API key has the required scope and log usage.
 * Same as CheckAPIKeyScope but also logs the authorization attempt.
 * Use this for operations where you want detailed audit trails.
 * @param scopePath - The scope path required
 * @param contextUser - The authenticated user
 * @param usageDetails - Details about the request for logging
 * @returns ScopeAuthResult with authorization details and optional log ID
export async function CheckAPIKeyScopeAndLog(
    usageDetails: {
        responseTimeMs?: number;
): Promise<ScopeAuthResult & { LogId?: string }> {
    // Get the API key
        const result: ScopeAuthResult & { LogId?: string } = {
    // Get the application
    // Evaluate access
    // Log the usage
    const usageLogger = engine.GetUsageLogger();
    const statusCode = usageDetails.statusCode ?? (authResult.Allowed ? 200 : 403);
    if (authResult.Allowed) {
        logId = (await usageLogger.LogSuccess(
            apiKeyId,
            usageDetails.endpoint,
            usageDetails.operationName || null,
            usageDetails.method,
            usageDetails.responseTimeMs || null,
            authResult.EvaluatedRules,
            usageDetails.ipAddress || null,
            usageDetails.userAgent || null,
        logId = (await usageLogger.LogDenied(
            authResult.Reason,
        EvaluatedRules: authResult.EvaluatedRules,
        LogId: logId
 * Decorator-style function for common scope checks.
 * Returns a function that can be used in resolvers.
 * @returns A function that performs the scope check
 * const requireViewRun = RequireScope('view:run');
 * // In resolver
 *   await requireViewRun(ctx);
 *   // ... proceed
export function RequireScope(scopePath: string, options: Omit<ScopeAuthOptions, 'resource'> = {}) {
    return async (ctx: { userPayload: { apiKeyId?: string; userRecord: UserInfo } }, resource?: string) => {
        await CheckAPIKeyScope(
            ctx.userPayload.apiKeyId,
            ctx.userPayload.userRecord,
            { ...options, resource }
// Pre-built scope checkers for common operations
export const RequireViewRun = RequireScope('view:run');
export const RequireQueryRun = RequireScope('query:run');
export const RequireAgentExecute = RequireScope('agent:execute');
