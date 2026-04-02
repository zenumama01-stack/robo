import { Arg, Ctx, Field, InputType, Int, ObjectType, PubSubEngine, Query, Resolver } from 'type-graphql';
import { ResolverBase } from './ResolverBase.js';
import { LogError, LogStatus, EntityInfo, RunViewWithCacheCheckResult, RunViewsWithCacheCheckResponse, RunViewWithCacheCheckParams, AggregateResult } from '@memberjunction/core';
import { RequireSystemUser } from '../directives/RequireSystemUser.js';
import { GetReadOnlyProvider } from '../util.js';
import { KeyValuePairOutputType } from './KeyInputOutputTypes.js';
 * The PURPOSE of this resolver is to provide a generic way to run a view and return the results.
 * The best practice is to use the strongly typed sub-class of this resolver for each entity.
 * that way you get back strongly typed results. If you need a generic way to call a view and get
 * back the results, and have your own type checking in place, this resolver can be used.
// INPUT/OUTPUT TYPE for Aggregates
 * Input type for a single aggregate expression
export class AggregateExpressionInput {
  @Field(() => String, {
    description: 'SQL expression for the aggregate (e.g., "SUM(OrderTotal)", "COUNT(*)", "AVG(Price)")'
    nullable: true,
    description: 'Optional alias for the result (used in error messages and debugging)'
 * Output type for a single aggregate result
export class AggregateResultOutput {
  @Field(() => String, { description: 'The expression that was calculated' })
  @Field(() => String, { description: 'The alias (or expression if no alias provided)' })
    description: 'The calculated value as a JSON string (preserves type information)'
    description: 'Error message if calculation failed'
// INPUT TYPE for Running Views
export class RunViewByIDInput {
      'Optional, pass in a valid condition to append to the view WHERE clause. For example, UpdatedAt >= Some Date - if not provided, no filter is applied',
  ExtraFilter: string;
      'Optional, pass in a valid order by clause sort the results on the server. For example, CreatedAt DESC to order by row creation date in reverse order. Any Valid SQL Order By clause is okay - if not provided, no server-side sorting is applied',
  OrderBy: string;
  @Field(() => [String], {
      'Optional, array of entity field names, if not provided, ID and all other columns used in the view columns are returned. If provided, only the fields in the array are returned.',
  UserSearchString: string;
  @Field(() => String, { nullable: true, description: 'Pass in a UserViewRun ID value to exclude all records from that run from results' })
      'Pass in a valid condition to append to the view WHERE clause to override the Exclude List. For example, UpdatedAt >= Some Date',
  @Field(() => Boolean, {
      'If set to True, the results of this view are saved into a new UserViewRun record and the UserViewRun.ID is passed back in the results.',
      'if set to true, the resulting data will filter out ANY records that were ever returned by this view, when the SaveViewResults property was set to true. This is useful if you want to run a particular view over time and make sure the results returned each time are new to the view.',
      'if set to true, if there IS any UserViewMaxRows property set for the entity in question, it will be IGNORED. This is useful in scenarios where you want to programmatically run a view and get ALL the data back, regardless of the MaxRows setting on the entity.',
  @Field(() => Int, {
      'if a value > 0 is provided, and IgnoreMaxRows is set to false, this value is used for the max rows to be returned by the view.',
      'If set to true, an Audit Log record will be created for the view run, regardless of the property settings in the entity for auditing view runs',
      "if provided and either ForceAuditLog is set, or the entity's property settings for logging view runs are set to true, this will be used as the Audit Log Description.",
      'Optional, pass in entity_object, simple, or count_only as options to specify the type of result you want back. Defaults to simple if not provided',
    description: 'If a value > 0 is provided, this value will be used to offset the rows returned.',
  @Field(() => [AggregateExpressionInput], {
    description: 'Optional aggregate expressions to calculate on the full result set (e.g., SUM, COUNT, AVG). Results are returned in AggregateResults.',
  Aggregates?: AggregateExpressionInput[];
export class RunViewByNameInput {
      'Optional, array of entity field names, if not provided, ID and all other columns used in the view are returned. If provided, only the fields in the array are returned.',
export class RunDynamicViewInput {
      'Optional, pass in a valid condition to use as the view WHERE clause. For example, UpdatedAt >= Some Date - if not provided, no filter is applied',
      'Optional, array of entity field names, if not provided, all columns are returned. If provided, only the fields in the array are returned.',
export class RunViewGenericInput {
// INPUT/OUTPUT TYPES for RunViewsWithCacheCheck
export class RunViewCacheStatusInput {
  @Field(() => String, { description: 'The maximum __mj_UpdatedAt value from cached results' })
  @Field(() => Int, { description: 'The number of rows in cached results' })
export class RunViewWithCacheCheckInput {
  @Field(() => RunDynamicViewInput, { description: 'The RunView parameters' })
  params: RunDynamicViewInput;
  @Field(() => RunViewCacheStatusInput, {
    description: 'Optional cache status - if provided, server will check if cache is current'
  cacheStatus?: RunViewCacheStatusInput;
export class DifferentialDataOutput {
  @Field(() => [RunViewGenericResultRow], {
    description: 'Records that have been created or updated since the client\'s maxUpdatedAt'
  updatedRows: RunViewGenericResultRow[];
    description: 'Primary key values (as concatenated strings) of records that have been deleted'
export class RunViewWithCacheCheckResultOutput {
  @Field(() => Int, { description: 'The index of this view in the batch request' })
  @Field(() => String, { description: "'current', 'differential', 'stale', or 'error'" })
    description: 'Fresh results - only populated when status is stale (full refresh)'
  Results?: RunViewGenericResultRow[];
  @Field(() => DifferentialDataOutput, {
    description: 'Differential update data - only populated when status is differential'
  differentialData?: DifferentialDataOutput;
  @Field(() => String, { nullable: true, description: 'Max __mj_UpdatedAt from results when stale or differential' })
  @Field(() => Int, { nullable: true, description: 'Row count of results when stale or differential (total after applying delta)' })
  @Field(() => String, { nullable: true, description: 'Error message if status is error' })
export class RunViewsWithCacheCheckOutput {
  @Field(() => Boolean, { description: 'Whether the overall operation succeeded' })
  @Field(() => [RunViewWithCacheCheckResultOutput], { description: 'Results for each view in the batch' })
  results: RunViewWithCacheCheckResultOutput[];
  @Field(() => String, { nullable: true, description: 'Overall error message if success is false' })
// OUTPUT TYPES for RunView Results
export class RunViewResultRow {
  @Field(() => [KeyValuePairOutputType], {
    description: 'Primary key values for the record'
  PrimaryKey: KeyValuePairOutputType[];
export class RunViewGenericResultRow {
export class RunViewResult {
  @Field(() => [RunViewResultRow])
  Results: RunViewResultRow[];
  @Field(() => Boolean, { nullable: false })
  @Field(() => [AggregateResultOutput], {
    description: 'Results of aggregate calculations, in same order as input Aggregates array. Only present if Aggregates were requested.'
  AggregateResults?: AggregateResultOutput[];
    description: 'Execution time for aggregate query specifically (in milliseconds). Only present if Aggregates were requested.'
export class RunViewGenericResult {
  @Field(() => [RunViewGenericResultRow])
  Results: RunViewGenericResultRow[];
@Resolver(RunViewResultRow)
export class RunViewResolver extends ResolverBase {
  @Query(() => RunViewResult)
  async RunViewByName(
    @Arg('input', () => RunViewByNameInput) input: RunViewByNameInput,
      const rawData = await super.RunViewByNameGeneric(input, provider, userPayload, pubSub);
      if (rawData === null) 
      const viewInfo = super.safeFirstArrayElement<UserViewEntityExtended>(await super.findBy<UserViewEntityExtended>(provider, "MJ: User Views", { Name: input.ViewName }, userPayload.userRecord));
      const entity = provider.Entities.find((e) => e.ID === viewInfo.EntityID);
      const returnData = this.processRawData(rawData.Results, viewInfo.EntityID, entity);
        Results: returnData,
        UserViewRunID: rawData?.UserViewRunID,
        RowCount: rawData?.RowCount,
        TotalRowCount: rawData?.TotalRowCount,
        ExecutionTime: rawData?.ExecutionTime,
        Success: rawData?.Success,
        AggregateResults: this.processAggregateResults(rawData?.AggregateResults),
        AggregateExecutionTime: rawData?.AggregateExecutionTime,
  async RunViewByID(
    @Arg('input', () => RunViewByIDInput) input: RunViewByIDInput,
      const rawData = await super.RunViewByIDGeneric(input, provider, userPayload, pubSub);
      const viewInfo = super.safeFirstArrayElement<UserViewEntityExtended>(await super.findBy<UserViewEntityExtended>(provider, "MJ: User Views", { ID: input.ViewID }, userPayload.userRecord));
  async RunDynamicView(
    @Arg('input', () => RunDynamicViewInput) input: RunDynamicViewInput,
      const rawData = await super.RunDynamicViewGeneric(input, provider, userPayload, pubSub);
      if (rawData === null) return null;
      const entity = provider.Entities.find((e) => e.Name === input.EntityName);
      const returnData = this.processRawData(rawData.Results, entity.ID, entity);
  @Query(() => [RunViewGenericResult])
  async RunViews(
    @Arg('input', () => [RunViewGenericInput]) input: (RunViewByNameInput & RunViewByIDInput & RunDynamicViewInput)[],
      // Note: RunViewsGeneric returns the core RunViewResult type, not the GraphQL type
      const rawData = await super.RunViewsGeneric(input, provider, userPayload);
      let results: RunViewGenericResult[] = [];
      for (const [index, data] of rawData.entries()) {
        const entity = provider.Entities.find((e) => e.Name === input[index].EntityName);
        const returnData: any[] = this.processRawData(data.Results, entity.ID, entity);
          UserViewRunID: data?.UserViewRunID,
          RowCount: data?.RowCount,
          TotalRowCount: data?.TotalRowCount,
          ExecutionTime: data?.ExecutionTime,
          Success: data?.Success,
          AggregateResults: this.processAggregateResults(data?.AggregateResults),
          AggregateExecutionTime: data?.AggregateExecutionTime,
  @RequireSystemUser()
  async RunViewByNameSystemUser(
      if (rawData === null) {
          ErrorMessage: `Failed to execute view: ${input.ViewName}`,
      const entity = provider.Entities.find((e) => e.Name === input.ViewName);
      const entityId = entity ? entity.ID : null;
      const returnData = this.processRawData(rawData.Results, entityId, entity);
        ErrorMessage: rawData?.ErrorMessage,
      const errorMessage = err instanceof Error ? err.message : 'Unknown error occurred';
        ErrorMessage: errorMessage,
  async RunViewByIDSystemUser(
          ErrorMessage: `Failed to execute view with ID: ${input.ViewID}`,
  async RunDynamicViewSystemUser(
          ErrorMessage: 'Failed to execute dynamic view',
        const errorMsg = `Entity ${input.EntityName} not found in metadata`;
        LogError(new Error(errorMsg));
  async RunViewsSystemUser(
          LogError(new Error(`Entity with name ${input[index].EntityName} not found`));
          ErrorMessage: data?.ErrorMessage,
  @Query(() => RunViewsWithCacheCheckOutput)
  async RunViewsWithCacheCheck(
    @Arg('input', () => [RunViewWithCacheCheckInput]) input: RunViewWithCacheCheckInput[],
    @Ctx() { providers, userPayload }: AppContext
  ): Promise<RunViewsWithCacheCheckOutput> {
      // Cast provider to SQLServerDataProvider to access RunViewsWithCacheCheck method
      const sqlProvider = provider as unknown as SQLServerDataProvider;
      if (!sqlProvider.RunViewsWithCacheCheck) {
        throw new Error('Provider does not support RunViewsWithCacheCheck');
      // Convert GraphQL input types to core types
      const coreParams: RunViewWithCacheCheckParams[] = input.map(item => ({
          EntityName: item.params.EntityName,
          ExtraFilter: item.params.ExtraFilter,
          OrderBy: item.params.OrderBy,
          UserSearchString: item.params.UserSearchString,
          ExcludeUserViewRunID: item.params.ExcludeUserViewRunID,
          OverrideExcludeFilter: item.params.OverrideExcludeFilter,
          IgnoreMaxRows: item.params.IgnoreMaxRows,
          ForceAuditLog: item.params.ForceAuditLog,
          AuditLogDescription: item.params.AuditLogDescription,
          ResultType: (item.params.ResultType || 'simple') as 'simple' | 'entity_object' | 'count_only',
      const response = await sqlProvider.RunViewsWithCacheCheck(coreParams, userPayload.userRecord);
      // Transform results to include processed data rows
      const transformedResults: RunViewWithCacheCheckResultOutput[] = response.results.map((result, index) => {
        const inputItem = input[index];
        const entity = provider.Entities.find(e => e.Name === inputItem.params.EntityName);
        // If we have differential data but no entity, that's a configuration error
        if (result.status === 'differential' && result.differentialData && !entity) {
            `Entity '${inputItem.params.EntityName}' not found in provider metadata but server returned differential data. ` +
            `This may indicate a metadata sync issue.`
        if (result.status === 'differential' && result.differentialData && entity) {
          // Process differential data into GraphQL-compatible format
          const processedUpdatedRows = this.processRawData(
            result.differentialData.updatedRows as Record<string, unknown>[],
            entity.ID,
            Results: undefined,
              updatedRows: processedUpdatedRows,
        if (result.status === 'stale' && result.results && entity) {
          // Process raw data into GraphQL-compatible format (full refresh)
          const processedRows = this.processRawData(result.results as Record<string, unknown>[], entity.ID, entity);
            Results: processedRows,
            differentialData: undefined,
  protected processRawData(rawData: any[], entityId: string, entityInfo: EntityInfo): RunViewResultRow[] {
    const returnResult = [];
    for (let i = 0; i < rawData.length; i++) {
      const row = rawData[i];
      // Build the primary key array from the entity's primary key fields
      const primaryKey: KeyValuePairOutputType[] = entityInfo.PrimaryKeys.map(pk => ({
        Value: row[pk.Name]?.toString() || ''
      returnResult.push({
        Data: JSON.stringify(row),
   * Transform core AggregateResult[] to GraphQL AggregateResultOutput[].
   * Converts the value to a JSON string to preserve type information across GraphQL.
  protected processAggregateResults(aggregateResults: AggregateResult[] | undefined): AggregateResultOutput[] | undefined {
    if (!aggregateResults || aggregateResults.length === 0) {
    return aggregateResults.map(result => ({
      expression: result.expression,
      alias: result.alias,
      value: result.value !== null && result.value !== undefined
        ? JSON.stringify(result.value)
