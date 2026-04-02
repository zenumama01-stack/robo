 * Usage Logger for API Key Authorization
 * Records detailed audit trail of API key usage and authorization decisions
import { MJAPIKeyUsageLogEntity } from '@memberjunction/core-entities';
import { UsageLogEntry, EvaluatedRule } from './interfaces';
 * Logs API key usage with detailed authorization evaluation information
export class UsageLogger {
     * Log a usage entry to the database
     * @param entry - The usage log entry to record
     * @returns The created log entity ID, or null if logging failed
    public async Log(entry: UsageLogEntry, contextUser: UserInfo): Promise<string | null> {
            const logEntity = await md.GetEntityObject<MJAPIKeyUsageLogEntity>(
                'MJ: API Key Usage Logs',
            logEntity.APIKeyID = entry.APIKeyId;
            logEntity.ApplicationID = entry.ApplicationId;
            logEntity.Endpoint = entry.Endpoint;
            logEntity.Operation = entry.Operation;
            logEntity.Method = entry.Method;
            logEntity.StatusCode = entry.StatusCode;
            logEntity.ResponseTimeMs = entry.ResponseTimeMs;
            logEntity.IPAddress = entry.IPAddress;
            logEntity.UserAgent = entry.UserAgent;
            logEntity.RequestedResource = entry.RequestedResource;
            logEntity.ScopesEvaluated = this.serializeEvaluatedRules(entry.ScopesEvaluated);
            logEntity.AuthorizationResult = entry.AuthorizationResult;
            logEntity.DeniedReason = entry.DeniedReason;
            return saved ? logEntity.ID : null;
            console.error('Failed to log API key usage:', error);
     * Log a successful request
    public async LogSuccess(
        applicationId: string | null,
        statusCode: number,
        responseTimeMs: number | null,
        requestedResource: string | null,
        evaluatedRules: EvaluatedRule[],
        return this.Log({
            APIKeyId: apiKeyId,
            ApplicationId: applicationId,
            Endpoint: endpoint,
            Method: method,
            StatusCode: statusCode,
            ResponseTimeMs: responseTimeMs,
            IPAddress: ipAddress,
            UserAgent: userAgent,
            RequestedResource: requestedResource,
            ScopesEvaluated: evaluatedRules,
            AuthorizationResult: 'Allowed',
            DeniedReason: null
     * Log a denied request
    public async LogDenied(
        deniedReason: string,
            AuthorizationResult: 'Denied',
            DeniedReason: deniedReason
     * Log a request that didn't require scope checking
    public async LogNoScopesRequired(
            RequestedResource: null,
            ScopesEvaluated: [],
            AuthorizationResult: 'NoScopesRequired',
     * Serialize evaluated rules to JSON for storage
    private serializeEvaluatedRules(rules: EvaluatedRule[]): string {
        return JSON.stringify(rules.map(r => ({
            level: r.Level,
            ruleId: r.Rule.Id,
            scopePath: r.Rule.ScopePath,
            pattern: r.Rule.Pattern,
            patternType: r.Rule.PatternType,
            isDeny: r.Rule.IsDeny,
            priority: r.Rule.Priority,
            matched: r.Matched,
            patternMatched: r.PatternMatched,
            result: r.Result
     * Parse evaluated rules from JSON storage
    public static ParseEvaluatedRules(json: string | null): EvaluatedRule[] {
        if (!json) return [];
            const parsed = JSON.parse(json);
            return parsed.map((r: Record<string, unknown>) => ({
                Level: r.level as 'application' | 'key',
                Rule: {
                    Id: r.ruleId as string,
                    ScopeId: '',  // Not stored in compact format
                    ScopePath: r.scopePath as string,
                    Pattern: r.pattern as string | null,
                    PatternType: r.patternType as 'Include' | 'Exclude',
                    IsDeny: r.isDeny as boolean,
                    Priority: r.priority as number
                Matched: r.matched as boolean,
                PatternMatched: r.patternMatched as string | null,
                Result: r.result as 'Allowed' | 'Denied' | 'NoMatch'
