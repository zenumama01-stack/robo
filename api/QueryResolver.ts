import { Arg, Ctx, ObjectType, Query, Resolver, Field, Int, InputType } from 'type-graphql';
import { RunQuery, QueryInfo, IRunQueryProvider, IMetadataProvider, RunQueryParams, LogError, RunQueryWithCacheCheckParams, RunQueriesWithCacheCheckResponse, RunQueryWithCacheCheckResult } from '@memberjunction/core';
 * Input type for batch query execution - allows running multiple queries in a single network call
export class RunQueryInput {
  @Field(() => String, { nullable: true, description: 'Query ID to run - either QueryID or QueryName must be provided' })
  @Field(() => String, { nullable: true, description: 'Query Name to run - either QueryID or QueryName must be provided' })
  @Field(() => String, { nullable: true, description: 'Category ID to help disambiguate queries with the same name' })
  @Field(() => String, { nullable: true, description: 'Category path to help disambiguate queries with the same name' })
  @Field(() => GraphQLJSONObject, { nullable: true, description: 'Parameters to pass to the query' })
  Parameters?: Record<string, unknown>;
  @Field(() => Int, { nullable: true, description: 'Maximum number of rows to return' })
  @Field(() => Int, { nullable: true, description: 'Starting row offset for pagination' })
  @Field(() => Boolean, { nullable: true, description: 'Force audit logging regardless of query settings' })
  @Field(() => String, { nullable: true, description: 'Description to use in audit log' })
export class RunQueryResultType {
// INPUT/OUTPUT TYPES for RunQueriesWithCacheCheck
export class RunQueryCacheStatusInput {
export class RunQueryWithCacheCheckInput {
  @Field(() => RunQueryInput, { description: 'The RunQuery parameters' })
  params: RunQueryInput;
  @Field(() => RunQueryCacheStatusInput, {
  cacheStatus?: RunQueryCacheStatusInput;
export class RunQueryWithCacheCheckResultOutput {
  @Field(() => Int, { description: 'The index of this query in the batch request' })
  @Field(() => String, { description: 'The query ID' })
  @Field(() => String, { description: "'current', 'stale', 'no_validation', or 'error'" })
    description: 'JSON-stringified results - only populated when status is stale or no_validation'
  @Field(() => String, { nullable: true, description: 'Max __mj_UpdatedAt from results when stale' })
  @Field(() => Int, { nullable: true, description: 'Row count of results when stale' })
export class RunQueriesWithCacheCheckOutput {
  @Field(() => [RunQueryWithCacheCheckResultOutput], { description: 'Results for each query in the batch' })
  results: RunQueryWithCacheCheckResultOutput[];
export class RunQueryResolver extends ResolverBase {
  private async findQuery(md: IMetadataProvider, QueryID: string, QueryName?: string, CategoryID?: string, CategoryPath?: string, refreshMetadataIfNotFound: boolean = false): Promise<QueryInfo | null> {
    // Filter queries based on provided criteria
    const queries = md.Queries.filter(q => {
      if (QueryID) {
        return q.ID.trim().toLowerCase() === QueryID.trim().toLowerCase();
      } else if (QueryName) {
        let matches = q.Name.trim().toLowerCase() === QueryName.trim().toLowerCase();
        if (CategoryID) {
          matches = matches && q.CategoryID?.trim().toLowerCase() === CategoryID.trim().toLowerCase();
        if (CategoryPath) {
          matches = matches && q.Category?.trim().toLowerCase() === CategoryPath.trim().toLowerCase();
    if (queries.length === 0) {
      if (refreshMetadataIfNotFound) {
        // If we didn't find the query, refresh metadata and try again
        return this.findQuery(md, QueryID, QueryName, CategoryID, CategoryPath, false); // change the refresh flag to false so we don't loop infinitely
        return null; // No query found and not refreshing metadata
      return queries[0];
  @Query(() => RunQueryResultType)
  async GetQueryData(@Arg('QueryID', () => String) QueryID: string,
                     @Arg('CategoryID', () => String, {nullable: true}) CategoryID?: string,
                     @Arg('CategoryPath', () => String, {nullable: true}) CategoryPath?: string,
                     @Arg('Parameters', () => GraphQLJSONObject, {nullable: true}) Parameters?: Record<string, any>,
                     @Arg('MaxRows', () => Int, {nullable: true}) MaxRows?: number,
                     @Arg('StartRow', () => Int, {nullable: true}) StartRow?: number,
                     @Arg('ForceAuditLog', () => Boolean, {nullable: true}) ForceAuditLog?: boolean,
                     @Arg('AuditLogDescription', () => String, {nullable: true}) AuditLogDescription?: string): Promise<RunQueryResultType> {
    // Check API key scope authorization for query execution
    await this.CheckAPIKeyScopeAuthorization('query:run', QueryID, context.userPayload);
    const provider = GetReadOnlyProvider(context.providers, {allowFallbackToReadWrite: true});
    const md = provider as unknown as IMetadataProvider;
    const rq = new RunQuery(provider as unknown as IRunQueryProvider);
    console.log('GetQueryData called with:', { QueryID, Parameters, MaxRows, StartRow, ForceAuditLog, AuditLogDescription });
    const result = await rq.RunQuery(
        QueryID: QueryID,
        CategoryID: CategoryID,
        CategoryPath: CategoryPath,
        Parameters: Parameters,
        MaxRows: MaxRows,
        StartRow: StartRow,
        ForceAuditLog: ForceAuditLog,
        AuditLogDescription: AuditLogDescription
      context.userPayload.userRecord);
    console.log('RunQuery result:', { 
      AppliedParameters: result.AppliedParameters 
    // If QueryName is not populated by the provider, use efficient lookup
    let queryName = result.QueryName;
    if (!queryName) {
        const queryInfo = await this.findQuery(md, QueryID, undefined, CategoryID, CategoryPath, true);
          queryName = queryInfo.Name;
        console.error('Error finding query to get name:', error);
      QueryName: queryName || 'Unknown Query',
      Success: result.Success ?? false,
      Results: JSON.stringify(result.Results ?? null),
      RowCount: result.RowCount ?? 0,
      TotalRowCount: result.TotalRowCount ?? 0,
      ExecutionTime: result.ExecutionTime ?? 0,
      ErrorMessage: result.ErrorMessage || '',
      AppliedParameters: result.AppliedParameters ? JSON.stringify(result.AppliedParameters) : undefined,
      CacheHit: (result as any).CacheHit,
      CacheTTLRemaining: (result as any).CacheTTLRemaining
  async GetQueryDataByName(@Arg('QueryName', () => String) QueryName: string,
    await this.CheckAPIKeyScopeAuthorization('query:run', QueryName, context.userPayload);
        QueryName: QueryName, 
      QueryID: result.QueryID || '',
  async GetQueryDataSystemUser(@Arg('QueryID', () => String) QueryID: string, 
  async GetQueryDataByNameSystemUser(@Arg('QueryName', () => String) QueryName: string, 
   * Batch query execution - runs multiple queries in a single network call
   * This is more efficient than making multiple individual query calls
  @Query(() => [RunQueryResultType])
  async RunQueries(
    @Arg('input', () => [RunQueryInput]) input: RunQueryInput[],
  ): Promise<RunQueryResultType[]> {
    // Check API key scope authorization for batch query execution
    // We check against '*' since this runs multiple queries
    await this.CheckAPIKeyScopeAuthorization('query:run', '*', context.userPayload);
    // Convert input to RunQueryParams array
    const params: RunQueryParams[] = input.map(i => ({
      QueryID: i.QueryID,
      QueryName: i.QueryName,
      CategoryID: i.CategoryID,
      CategoryPath: i.CategoryPath,
      Parameters: i.Parameters,
      MaxRows: i.MaxRows,
      StartRow: i.StartRow,
      ForceAuditLog: i.ForceAuditLog,
      AuditLogDescription: i.AuditLogDescription
    // Execute all queries in parallel using the batch method
    const results = await rq.RunQueries(params, context.userPayload.userRecord);
    // Map results to output format
    return results.map((result, index) => {
        QueryID: result.QueryID || inputItem.QueryID || '',
        QueryName: result.QueryName || inputItem.QueryName || 'Unknown Query',
        CacheHit: (result as Record<string, unknown>).CacheHit as boolean | undefined,
        CacheTTLRemaining: (result as Record<string, unknown>).CacheTTLRemaining as number | undefined
   * Batch query execution with system user privileges
  async RunQueriesSystemUser(
   * If the Query doesn't have CacheValidationSQL configured, returns 'no_validation' with data.
  @Query(() => RunQueriesWithCacheCheckOutput)
  async RunQueriesWithCacheCheck(
    @Arg('input', () => [RunQueryWithCacheCheckInput]) input: RunQueryWithCacheCheckInput[],
  ): Promise<RunQueriesWithCacheCheckOutput> {
      // Cast provider to SQLServerDataProvider to access RunQueriesWithCacheCheck method
      if (!sqlProvider.RunQueriesWithCacheCheck) {
        throw new Error('Provider does not support RunQueriesWithCacheCheck');
      const coreParams: RunQueryWithCacheCheckParams[] = input.map(item => ({
          QueryID: item.params.QueryID,
          QueryName: item.params.QueryName,
          CategoryID: item.params.CategoryID,
          CategoryPath: item.params.CategoryPath,
          Parameters: item.params.Parameters,
      const response = await sqlProvider.RunQueriesWithCacheCheck(coreParams, context.userPayload.userRecord);
      // Transform results to GraphQL output format
      const transformedResults: RunQueryWithCacheCheckResultOutput[] = response.results.map(result => ({
        Results: result.results ? JSON.stringify(result.results) : undefined,
   * RunQueriesWithCacheCheck with system user privileges
  async RunQueriesWithCacheCheckSystemUser(
    // Same implementation as regular version - RequireSystemUser handles auth
    return this.RunQueriesWithCacheCheck(input, context);
