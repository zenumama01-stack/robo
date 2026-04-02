import { IRunQueryProvider, RunQueryResult } from './interfaces';
 * Parameters for running a query, must provide either QueryID or QueryName. If both are provided QueryName is ignored.
 * QueryName and CategoryPath together uniquely identify a Query, just as QueryID does.
 * CategoryPath supports hierarchical paths (e.g., "/MJ/AI/Agents/") for navigation through nested categories.
export type RunQueryParams = {
     * Provide either QueryID or QueryName. If both are provided QueryName is ignored
    QueryID?: string
    QueryName?: string
     * Optional, if provided, the query to be run will be selected to match the specified Category by hierarchical path
     * (e.g., "/MJ/AI/Agents/") or simple category name for backward compatibility
    CategoryPath?: string
     * Optional, if provided, the query to be run will be selected to match the specified CategoryID 
    CategoryID?: string
     * Optional parameters to pass to parameterized queries that use Nunjucks templates.
     * Key-value pairs where keys match parameter names defined in QueryParameter metadata.
     * Values will be validated and type-converted based on parameter definitions.
    Parameters?: Record<string, any>
     * Optional maximum number of rows to return from the query.
     * If not provided, all rows will be returned.
    MaxRows?: number
     * Optional - if provided, this value will be used to offset the rows returned.
     * Used for pagination in conjunction with MaxRows.
    StartRow?: number
     * optional - if set to true, the query run will ALWAYS be logged to the Audit Log, 
     * regardless of the query's property settings for logging query runs.
    ForceAuditLog?: boolean
     * optional - if provided and either ForceAuditLog is set, or the query's property
     * settings for logging query runs are set to true, this will be used as the Audit Log Description.
    AuditLogDescription?: string
     * When set to true, the RunQuery will first check the LocalCacheManager for cached results.
     * If cached results exist and are still valid (based on smart cache fingerprinting), they will
     * be returned immediately without hitting the server. This is useful for frequently-accessed,
     * relatively-static query results.
     * Note: The LocalCacheManager must be initialized before this can work.
     * For queries with CacheValidationSQL configured, the server will validate cache freshness
     * using fingerprint comparison (maxUpdatedAt + rowCount) before returning fresh data.
    CacheLocal?: boolean
     * Optional TTL (time-to-live) in milliseconds for cached results when CacheLocal is true.
     * If not specified:
     * 1. The Query's CacheTTLMinutes property will be used if configured
     * 2. Otherwise the LocalCacheManager's default TTL will be used (typically 5 minutes)
    CacheLocalTTL?: number
 * Class used to run a query and return the results.
 * Provides an abstraction layer for executing saved queries with proper security checks.
 * Supports both instance-based and static provider patterns.
export class RunQuery  {
    private _provider: IRunQueryProvider | null;
     * Optionally, you can pass in a provider to use for running the query. If not provided, the static provider will be used.
    constructor(provider: IRunQueryProvider | null = null) {
     * Gets the query provider to use for execution.
     * Returns the instance provider if set, otherwise falls back to the static provider.
     * @returns The query provider instance
    public get ProviderToUse(): IRunQueryProvider {
        return this._provider || RunQuery.Provider;
     * Executes a saved query and returns the results.
     * The query must exist in the system and the user must have permission to execute it.
     * Queries can be identified by QueryID or by QueryName + CategoryPath combination.
     * CategoryPath supports hierarchical navigation (e.g., "/MJ/AI/Agents/") and falls back to simple name matching.
     * @param params - Parameters including query ID or name with optional CategoryPath for disambiguation
     * @param contextUser - Optional user context for permissions (mainly used server-side)
     * @returns Query results including data rows and execution metadata
        // Simple proxy to the provider - telemetry is handled by ProviderBase Pre/Post hooks
        return this.ProviderToUse.RunQuery(params, contextUser);
     * More efficient than calling RunQuery multiple times as it reduces network overhead
     * and allows the database to execute queries in parallel.
     * @param params - Array of query parameters, each specifying a query to execute
     * @returns Array of query results in the same order as the input params
        return this.ProviderToUse.RunQueries(params, contextUser);
    private static _globalProviderKey: string = 'MJ_RunQueryProvider';
     * Gets the static query provider instance.
     * Used when no instance-specific provider is configured.
     * @returns The global query provider
     * @throws Error if global object store is not available
    public static get Provider(): IRunQueryProvider {
            return g[RunQuery._globalProviderKey];
     * Sets the static query provider instance.
     * This provider will be used by all RunQuery instances that don't have their own provider.
     * @param value - The query provider to set globally
    public static set Provider(value: IRunQueryProvider) {
            g[RunQuery._globalProviderKey] = value;
