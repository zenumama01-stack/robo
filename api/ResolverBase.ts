  AggregateExpression,
  BaseEntityEvent,
  DatabaseProviderBase,
  EntityPermissionType,
  EntitySaveOptions,
  IRunViewProvider,
  LogDebug,
  RunViewResult,
import { MJAuditLogEntity, MJErrorLogEntity, UserViewEntityExtended } from '@memberjunction/core-entities';
import { SQLServerDataProvider, UserCache } from '@memberjunction/sqlserver-dataprovider';
import { PubSubEngine, AuthorizationError } from 'type-graphql';
import { httpTransport, CloudEvent, emitterFor } from 'cloudevents';
import { RunViewGenericParams, UserPayload } from '../types.js';
import { RunDynamicViewInput, RunViewByIDInput, RunViewByNameInput } from './RunViewResolver.js';
import { DeleteOptionsInput } from './DeleteOptionsInput.js';
import { MJEvent, MJEventType, MJGlobal, ENCRYPTED_SENTINEL, IsValueEncrypted, IsOnlyTimezoneShift } from '@memberjunction/global';
import { EncryptionEngine } from '@memberjunction/encryption';
import { PUSH_STATUS_UPDATES_TOPIC } from './PushStatusResolver.js';
import { FieldMapper } from '@memberjunction/graphql-dataprovider';
export class ResolverBase {
  private static _emit = process.env.CLOUDEVENTS_HTTP_TRANSPORT ? emitterFor(httpTransport(process.env.CLOUDEVENTS_HTTP_TRANSPORT)) : null;
  private static _cloudeventsHeaders = process.env.CLOUDEVENTS_HTTP_HEADERS ? JSON.parse(process.env.CLOUDEVENTS_HTTP_HEADERS) : {};
  private static _eventSubscriptionKey: string = '___MJServer___ResolverBase___EventSubscriptions';
  private get EventSubscriptions(): Map<string, Subscription> {
    // here we use the global object store instead of a static member becuase in some cases based on import code paths/bundling/etc, the static member
    // could actually be duplicated and we'd end up with multiple instances of the same map, which would be bad.
    if (!g[ResolverBase._eventSubscriptionKey]) {
      LogDebug(`>>>>> MJServer.ResolverBase.EventSubscriptions: Creating new Map - this should only happen once per server instance <<<<<<`);
      g[ResolverBase._eventSubscriptionKey] = new Map<string, Subscription>();
    return g[ResolverBase._eventSubscriptionKey];
   * Maps field names to their GraphQL-safe CodeNames and handles encryption for API responses.
   * For encrypted fields coming from raw SQL queries (not entity objects):
   * - AllowDecryptInAPI=true: Decrypt the value before sending to client
   * - AllowDecryptInAPI=false + SendEncryptedValue=true: Keep encrypted ciphertext
   * - AllowDecryptInAPI=false + SendEncryptedValue=false: Replace with sentinel
   * @param entityName - The entity name
   * @param dataObject - The data object with field values
   * @param contextUser - Optional user context for decryption (required for encrypted fields)
   * @returns The processed data object
  protected async MapFieldNamesToCodeNames(entityName: string, dataObject: any, contextUser?: UserInfo): Promise<any> {
    // for the given entity name provided, check to see if there are any fields
    // where the code name is different from the field name, and for just those
    // fields, iterate through the dataObject and REPLACE the property that has the field name
    // with the CodeName, because we can't transfer those via GraphQL as they are not
    // valid property names in GraphQL
    if (dataObject) {
      const entityInfo = md.Entities.find((e) => e.Name === entityName);
      if (!entityInfo) throw new Error(`Entity ${entityName} not found in metadata`);
      // const fields = entityInfo.Fields.filter((f) => f.Name !== f.CodeName || f.Name.startsWith('__mj_'));
      entityInfo.Fields.forEach((f) => {
        if (dataObject.hasOwnProperty(f.Name)) {
          // GraphQL doesn't allow us to pass back fields with __ so we are mapping our special field cases that start with __mj_ to _mj__ for transport - they are converted back on the other side automatically
          const mappedFieldName = mapper.MapFieldName(f.CodeName);
          if (mappedFieldName !== f.Name) {
            dataObject[mappedFieldName] = dataObject[f.Name];
            delete dataObject[f.Name];
      // Handle encrypted fields - data from raw SQL queries is still encrypted
      const encryptedFields = entityInfo.EncryptedFields;
        for (const field of encryptedFields) {
          const fieldName = field.CodeName;
          const value = dataObject[fieldName];
          // Skip null/undefined values
          if (value === null || value === undefined) continue;
          // Check if value is encrypted (raw SQL returns encrypted values)
          const keyMarker = field.EncryptionKeyID ? engine.GetKeyByID(field.EncryptionKeyID)?.Marker : undefined;
          if (typeof value === 'string' && IsValueEncrypted(value, keyMarker)) {
            if (field.AllowDecryptInAPI) {
              // Decrypt the value for the client
                  const decryptedValue = await engine.Decrypt(value, contextUser);
                  dataObject[fieldName] = decryptedValue;
                  LogError(`Failed to decrypt field ${fieldName} for API response: ${err}`);
                  // On decryption failure, use sentinel for safety
                  dataObject[fieldName] = ENCRYPTED_SENTINEL;
                // No context user, can't decrypt - use sentinel
            } else if (!field.SendEncryptedValue) {
              // AllowDecryptInAPI=false and SendEncryptedValue=false - use sentinel
            // else: AllowDecryptInAPI=false and SendEncryptedValue=true - keep encrypted value as-is
    return dataObject;
  protected async ArrayMapFieldNamesToCodeNames(entityName: string, dataObjectArray: any[], contextUser?: UserInfo): Promise<any[]> {
    // iterate through the array and call MapFieldNamesToCodeNames for each element
    if (dataObjectArray && dataObjectArray.length > 0) {
      for (const element of dataObjectArray) {
        await this.MapFieldNamesToCodeNames(entityName, element, contextUser);
    return dataObjectArray;
   * Filters encrypted field values before sending to the API client.
   * For each encrypted field in the entity:
   * - If AllowDecryptInAPI is true: value passes through unchanged (already decrypted by data provider)
   * - If AllowDecryptInAPI is false and SendEncryptedValue is true: re-encrypt and send ciphertext
   * - If AllowDecryptInAPI is false and SendEncryptedValue is false: replace with sentinel value
   * @param entityName - Name of the entity
   * @param dataObject - The data object containing field values
   * @param encryptionEngine - Optional encryption engine for re-encryption (lazy loaded if needed)
   * @param contextUser - User context for encryption operations
   * @returns The filtered data object
  protected async FilterEncryptedFieldsForAPI(
    dataObject: Record<string, unknown>,
    if (!dataObject) return dataObject;
    if (!entityInfo) return dataObject;
    // Find all encrypted fields that need filtering
    if (encryptedFields.length === 0) return dataObject;
    // Process each encrypted field
      // If AllowDecryptInAPI is true, the decrypted value passes through
      if (field.AllowDecryptInAPI) continue;
      // AllowDecryptInAPI is false - we need to filter the value
      if (field.SendEncryptedValue) {
        // Re-encrypt the value before sending
        // Only re-encrypt if it's not already encrypted (data provider decrypted it)
        if (typeof value === 'string' && !IsValueEncrypted(value, keyMarker)) {
              field.EncryptionKeyID as string,
            dataObject[fieldName] = encryptedValue;
            // If re-encryption fails, use sentinel for safety
            LogError(`Failed to re-encrypt field ${fieldName} for API response: ${err}`);
        // If already encrypted (shouldn't happen normally), keep as-is
        // SendEncryptedValue is false - replace with sentinel
   * Filters encrypted fields for an array of data objects
  protected async ArrayFilterEncryptedFieldsForAPI(
    dataObjectArray: Record<string, unknown>[],
  ): Promise<Record<string, unknown>[]> {
    if (!dataObjectArray || dataObjectArray.length === 0) return dataObjectArray;
    // Check if entity has any encrypted fields first to avoid unnecessary processing
    if (!entityInfo) return dataObjectArray;
    const encryptedFields = entityInfo.Fields.filter(f => f.Encrypt && !f.AllowDecryptInAPI);
    if (encryptedFields.length === 0) return dataObjectArray;
    // Process each element
      await this.FilterEncryptedFieldsForAPI(entityName, element, contextUser);
  protected async findBy<T = any>(provider: DatabaseProviderBase, entity: string, params: any, contextUser: UserInfo): Promise<Array<T>> {
    // build the SQL query based on the params passed in
    const rv = provider as any as IRunViewProvider;
    const e = provider.Entities.find((e) => e.Name === entity);
    if (!e) throw new Error(`Entity ${entity} not found in metadata`);
    // now build a SQL string using the entityInfo and using the properties in the params object
    let extraFilter = "";
    const keys = Object.keys(params);
    keys.forEach((k, i) => {
      if (i > 0) extraFilter += ' AND ';
      // look up the field in the entityInfo to see if it needs quotes
      const field = e.Fields.find((f) => f.Name === k);
      if (!field) throw new Error(`Field ${k} not found in entity ${entity}`);
      const quotes = field.NeedsQuotes ? "'" : '';
      extraFilter += `${k} = ${quotes}${params[k]}${quotes}`;
    // ok, now we have a SQL string, run it and return the results
    // use the SQLServerDataProvider
      EntityName: entity,
  async RunViewByNameGeneric(viewInput: RunViewByNameInput, provider: DatabaseProviderBase, userPayload: UserPayload, pubSub: PubSubEngine) {
      // Log aggregate input for debugging
      if (viewInput.Aggregates?.length) {
        LogStatus(`[ResolverBase] RunViewByNameGeneric received aggregates: viewName=${viewInput.ViewName}, aggregateCount=${viewInput.Aggregates.length}, aggregates=${JSON.stringify(viewInput.Aggregates.map(a => ({ expression: a.expression, alias: a.alias })))}`);
        ExtraFilter: "Name='" + viewInput.ViewName + "'",
      }, userPayload.userRecord);
        const viewInfo = result.Results[0];
        return this.RunViewGenericInternal(
          viewInfo,
          viewInput.ExtraFilter,
          viewInput.OrderBy,
          viewInput.UserSearchString,
          viewInput.ExcludeUserViewRunID,
          viewInput.OverrideExcludeFilter,
          viewInput.SaveViewResults,
          viewInput.Fields,
          viewInput.IgnoreMaxRows,
          viewInput.ExcludeDataFromAllPriorViewRuns,
          viewInput.ForceAuditLog,
          viewInput.AuditLogDescription,
          viewInput.ResultType,
          userPayload,
          viewInput.MaxRows,
          viewInput.StartRow,
          viewInput.Aggregates
        LogError(`RunViewByNameGeneric: View ${viewInput.ViewName} not found or no results returned`);
      console.log(err);
  async RunViewByIDGeneric(viewInput: RunViewByIDInput, provider: DatabaseProviderBase, userPayload: UserPayload, pubSub: PubSubEngine) {
        LogStatus(`[ResolverBase] RunViewByIDGeneric received aggregates: viewID=${viewInput.ViewID}, aggregateCount=${viewInput.Aggregates.length}, aggregates=${JSON.stringify(viewInput.Aggregates.map(a => ({ expression: a.expression, alias: a.alias })))}`);
      const contextUser = this.GetUserFromPayload(userPayload);
      const viewInfo = await provider.GetEntityObject<UserViewEntityExtended>('MJ: User Views', contextUser);
      await viewInfo.Load(viewInput.ViewID);
  async RunDynamicViewGeneric(viewInput: RunDynamicViewInput, provider: DatabaseProviderBase, userPayload: UserPayload, pubSub: PubSubEngine) {
        LogStatus(`[ResolverBase] RunDynamicViewGeneric received aggregates: entityName=${viewInput.EntityName}, aggregateCount=${viewInput.Aggregates.length}, aggregates=${JSON.stringify(viewInput.Aggregates.map(a => ({ expression: a.expression, alias: a.alias })))}`);
      const md = provider;
      const entity = md.Entities.find((e) => e.Name === viewInput.EntityName);
      if (!entity) throw new Error(`Entity ${viewInput.EntityName} not found in metadata`);
      const viewInfo: UserViewEntityExtended = {
        Entity: viewInput.EntityName,
        EntityID: entity.ID,
        EntityBaseView: entity.BaseView as string,
      } as UserViewEntityExtended; // only providing a few bits of data here, but it's enough to get the view to run
  async RunViewsGeneric(
    viewInputs: (RunViewByNameInput & RunViewByIDInput & RunDynamicViewInput)[],
    provider: DatabaseProviderBase,
    userPayload: UserPayload
    let params: RunViewGenericParams[] = [];
    for (const viewInput of viewInputs) {
        let viewInfo: UserViewEntityExtended | null = null;
        if (viewInput.ViewName) {
          viewInfo = this.safeFirstArrayElement(await this.findBy(provider, 'MJ: User Views', { Name: viewInput.ViewName }, userPayload.userRecord));
        } else if (viewInput.ViewID) {
          viewInfo = await provider.GetEntityObject<UserViewEntityExtended>('MJ: User Views', contextUser);
        } else if (viewInput.EntityName) {
            throw new Error(`Entity ${viewInput.EntityName} not found in metadata`);
          // only providing a few bits of data here, but it's enough to get the view to run
          viewInfo = {
            EntityBaseView: entity.BaseView,
          } as UserViewEntityExtended;
          throw new Error('Unable to determine input type');
        params.push({
          extraFilter: viewInput.ExtraFilter,
          orderBy: viewInput.OrderBy,
          userSearchString: viewInput.UserSearchString,
          excludeUserViewRunID: viewInput.ExcludeUserViewRunID,
          overrideExcludeFilter: viewInput.OverrideExcludeFilter,
          saveViewResults: viewInput.EntityName ? false : viewInput.SaveViewResults,
          fields: viewInput.Fields,
          ignoreMaxRows: viewInput.IgnoreMaxRows,
          maxRows: viewInput.MaxRows,
          startRow: viewInput.StartRow,
          excludeDataFromAllPriorViewRuns: viewInput.EntityName ? false : viewInput.ExcludeDataFromAllPriorViewRuns,
          forceAuditLog: viewInput.ForceAuditLog,
          auditLogDescription: viewInput.AuditLogDescription,
          resultType: viewInput.ResultType,
          aggregates: viewInput.Aggregates,
    let results: RunViewResult[] = await this.RunViewsGenericInternal(params);
  private static _priorEmittedData: {Entity: string, PKey: CompositeKey}[] = [];
  protected async EmitCloudEvent({ component, event, eventCode, args }: MJEvent) {
    if (ResolverBase._emit && event === MJEventType.ComponentEvent && eventCode === BaseEntity.BaseEventCode) {
      const extendedType = args instanceof BaseEntityEvent ? `.${args.type}` : '';
      const type = `MemberJunction.${event}${extendedType}`;
      const source = `${process.env.CLOUDEVENTS_SOURCE ?? 'MemberJunction'}`;
      const subject = args instanceof BaseEntityEvent ? args.baseEntity.EntityInfo.CodeName : undefined;
      const data = args?.baseEntity?.GetAll() ?? {};
      const cloudEvent = new CloudEvent({ type, source, subject, data });
        // check to see if the combination of Entity and pkey was already emitted, if so, Log that condtion next
        const pkey = args.baseEntity.PrimaryKeys as CompositeKey;
        const emittedData = { Entity: args.baseEntity.EntityInfo.Name, PKey: pkey };
        if (ResolverBase._priorEmittedData.find((e) => {
          if (e.Entity !== emittedData.Entity) return false;
          // if we get here compare the pkeys
          const pkey2 = e.PKey as CompositeKey;
          if (pkey.KeyValuePairs.length !== pkey2.KeyValuePairs.length) 
          for (const kv of pkey.KeyValuePairs) {
            // find the match by field name
            const kv2 = pkey2.KeyValuePairs.find((k) => k.FieldName === kv.FieldName);
            if (!kv2 || kv2.Value !== kv.Value) 
          return true; // if we get here, all the keys matched
        })) {
          console.log(`IMPORTANT: CloudEvent already emitted for ${JSON.stringify(emittedData)}`);
        LogStatus(`Emitting CloudEvent: ${JSON.stringify(cloudEvent)}`);
        const cloudeventTransportResponse = await ResolverBase._emit(cloudEvent, { headers: ResolverBase._cloudeventsHeaders });
        const cloudeventResponse = JSON.stringify(cloudeventTransportResponse);
        if (/error/i.test(cloudeventResponse)) {
          console.error('CloudEvent ERROR', cloudeventResponse);
        console.error('CloudEvent ERROR', JSON.stringify(e));
  protected CheckUserReadPermissions(entityName: string, userPayload: UserPayload | null) {
      throw new Error(`userPayload is null`);
    // first check permissions, the logged in user must have read permissions on the entity to run the view
      const userInfo = UserCache.Users.find((u) => u.Email.toLowerCase().trim() === userPayload.email.toLowerCase().trim()); // get the user record from MD so we have ROLES attached, don't use the one from payload directly
      if (!userInfo) {
        throw new Error(`User ${userPayload.email} not found in metadata`);
      const userPermissions = entityInfo.GetUserPermisions(userInfo);
      if (!userPermissions.CanRead) {
        throw new Error(`User ${userPayload.email} does not have read permissions on ${entityInfo.Name}`);
      throw new Error(`Entity not found in metadata`);
   * Checks API key scope authorization. Only performs check if request
   * was authenticated via API key (apiKeyHash present in userPayload).
   * For OAuth/JWT auth, this is a no-op.
   * @param scopePath - The scope path (e.g., 'entity:read', 'agent:execute')
   * @param resource - The resource name (e.g., entity name, agent name)
   * @param userPayload - The user payload from context
   * @throws AuthorizationError if API key lacks required scope
  protected async CheckAPIKeyScopeAuthorization(
    // Skip scope check for OAuth/JWT auth (no API key)
    if (!userPayload.apiKeyHash) {
    // Get system user for authorization call
    // NOTE: We use system user here because Authorize() needs to run internal
    // database queries (loading scope rules, logging decisions). The system user
    // ensures these queries work regardless of what permissions the API key's
    // user has. The API key's associated user (in userPayload.userRecord) is
    // used later when the actual operation executes - their permissions are
    // the ultimate ceiling that scopes can only narrow, never expand.
    const systemUser = UserCache.Instance.Users.find(u => u.Type === 'System');
      throw new Error('System user not found');
    // Check for full_access scope first (god power - bypasses all other checks)
    const fullAccessResult = await apiKeyEngine.Authorize(
      userPayload.apiKeyHash,
      'MJAPI',
      'full_access',
      '*',
      { endpoint: '/graphql', method: 'POST' }
    if (fullAccessResult.Allowed) {
      // full_access granted - skip specific scope check
    // Check specific scope
        endpoint: '/graphql',
        method: 'POST'
      // Provide specific, actionable error message
        `Access denied. This API key requires the '${scopePath}' scope ` +
        `for resource '${resource}' to perform this operation. ` +
        `Please update the API key's scopes or use an API key with appropriate permissions. ` +
        `Denial reason: ${result.Reason}`
   * Optimized RunViewGenericInternal implementation with:
   * - Field filtering at source (Fix #7)
   * - Improved error handling (Fix #9)
  protected async RunViewGenericInternal(
    viewInfo: UserViewEntityExtended,
    extraFilter: string,
    orderBy: string,
    userSearchString: string,
    excludeUserViewRunID: string | undefined,
    overrideExcludeFilter: string | undefined,
    saveViewResults: boolean | undefined,
    fields: string[] | undefined,
    ignoreMaxRows: boolean | undefined,
    excludeDataFromAllPriorViewRuns: boolean | undefined,
    forceAuditLog: boolean | undefined,
    auditLogDescription: string | undefined,
    resultType: string | undefined,
    userPayload: UserPayload | null,
    maxRows: number | undefined,
    startRow: number | undefined,
    aggregates?: AggregateExpression[]
      if (!viewInfo || !userPayload) return null;
      // Check API key scope authorization for view operations
      await this.CheckAPIKeyScopeAuthorization('view:run', viewInfo.Entity, userPayload);
      const md = provider
      const user = UserCache.Users.find((u) => u.Email.toLowerCase().trim() === userPayload?.email.toLowerCase().trim());
      if (!user) throw new Error(`User ${userPayload?.email} not found in metadata`);
      const entityInfo = md.Entities.find((e) => e.Name === viewInfo.Entity);
      if (!entityInfo) throw new Error(`Entity ${viewInfo.Entity} not found in metadata`);
      const rv = md as any as IRunViewProvider;
      // Determine result type
      let rt: 'simple' | 'entity_object' | 'count_only' = 'simple';
      if (resultType?.trim().toLowerCase() === 'count_only') {
        rt = 'count_only';
      // Fix #7: Implement field filtering - preprocess fields for more efficient field selection
      // This is passed to RunView() which uses it to optimize the SQL query
      let optimizedFields = fields;
      if (fields?.length) {
        // Always ensure primary keys are included for proper record handling
        const primaryKeys = entityInfo.PrimaryKeys.map(pk => pk.Name);
        const missingPrimaryKeys = primaryKeys.filter(pk => 
          !fields.find(f => f.toLowerCase() === pk.toLowerCase())
        if (missingPrimaryKeys.length) {
          optimizedFields = [...fields, ...missingPrimaryKeys];
      if (aggregates?.length) {
        LogStatus(`[ResolverBase] RunViewGenericInternal with aggregates: entityName=${viewInfo.Entity}, viewName=${viewInfo.Name}, aggregateCount=${aggregates.length}, aggregates=${JSON.stringify(aggregates.map(a => ({ expression: a.expression, alias: a.alias })))}`);
          ViewID: viewInfo.ID,
          ViewName: viewInfo.Name,
          EntityName: viewInfo.Entity,
          Fields: optimizedFields,  // Use optimized fields list
          UserSearchString: userSearchString,
          ExcludeUserViewRunID: excludeUserViewRunID,
          OverrideExcludeFilter: overrideExcludeFilter,
          SaveViewResults: saveViewResults,
          ExcludeDataFromAllPriorViewRuns: excludeDataFromAllPriorViewRuns,
          IgnoreMaxRows: ignoreMaxRows,
          ForceAuditLog: forceAuditLog,
          AuditLogDescription: auditLogDescription,
          ResultType: rt,
          Aggregates: aggregates,
        user
      // Log aggregate results for debugging
        LogStatus(`[ResolverBase] RunView result aggregate info: entityName=${viewInfo.Entity}, hasAggregateResults=${!!result?.AggregateResults}, aggregateResultCount=${result?.AggregateResults?.length || 0}, aggregateExecutionTime=${result?.AggregateExecutionTime}, aggregateResults=${JSON.stringify(result?.AggregateResults)}`);
      // Process results for GraphQL transport
      if (result?.Success && result.Results?.length) {
        for (const r of result.Results) {
          mapper.MapFields(r);
        // Filter encrypted fields before sending to API client
        await this.ArrayFilterEncryptedFieldsForAPI(
          viewInfo.Entity,
          result.Results as Record<string, unknown>[],
      // Fix #9: Improved error handling with structured logging
      const error = err as Error;
      LogError({
        service: 'RunView',
        operation: 'RunViewGenericInternal',
        entityName: viewInfo?.Entity,
        errorType: error.constructor.name,
        // Only include stack trace for non-validation errors
        stack: error.message?.includes('not found in metadata') ? undefined : error.stack
   * Optimized implementation that:
   * 1. Fetches user info only once (fixes N+1 query)
   * 2. Processes views in parallel for independent operations
   * 3. Implements structured error logging
  protected async RunViewsGenericInternal(params: RunViewGenericParams[]): Promise<RunViewResult[]> {
      // Skip processing if no params
      if (!params.length) return [];
      let md: Metadata | null = null;
      const rv = params[0].provider as any as IRunViewProvider;
      let runViewParams: RunViewParams[] = [];
      // Fix #1: Get user info only once for all queries
      if (params[0]?.userPayload?.email) {
        const userEmail = params[0].userPayload.email.toLowerCase().trim();
        const user = UserCache.Users.find(u => u.Email.toLowerCase().trim() === userEmail);
          throw new Error(`User ${userEmail} not found in metadata`);
        contextUser = user;
      // Create a map of entities to validate only once per entity
      const validatedEntities = new Set<string>();
      md = new Metadata();
      // Transform parameters
        if (param.viewInfo) {
          // Validate entity only once per entity type
          const entityName = param.viewInfo.Entity;
          if (!validatedEntities.has(entityName)) {
            validatedEntities.add(entityName);
        if (param.resultType?.trim().toLowerCase() === 'count_only') {
        // Build parameters
        runViewParams.push({
          ViewID: param.viewInfo.ID,
          ViewName: param.viewInfo.Name,
          EntityName: param.viewInfo.Entity,
          ExtraFilter: param.extraFilter,
          OrderBy: param.orderBy,
          Fields: param.fields,
          UserSearchString: param.userSearchString,
          ExcludeUserViewRunID: param.excludeUserViewRunID,
          OverrideExcludeFilter: param.overrideExcludeFilter,
          SaveViewResults: param.saveViewResults,
          ExcludeDataFromAllPriorViewRuns: param.excludeDataFromAllPriorViewRuns,
          IgnoreMaxRows: param.ignoreMaxRows,
          MaxRows: param.maxRows,
          StartRow: param.startRow,
          ForceAuditLog: param.forceAuditLog,
          AuditLogDescription: param.auditLogDescription,
          Aggregates: param.aggregates,
      // Fix #4: Run views in a single batch through RunViews
      const runViewResults: RunViewResult[] = await rv.RunViews(runViewParams, contextUser);
      for (let i = 0; i < runViewResults.length; i++) {
        const runViewResult = runViewResults[i];
        if (runViewResult?.Success && runViewResult.Results?.length) {
          for (const result of runViewResult.Results) {
            mapper.MapFields(result);
          // Use the corresponding param's entity name
          const entityName = params[i]?.viewInfo?.Entity;
          if (entityName && contextUser) {
              runViewResult.Results as Record<string, unknown>[],
      return runViewResults;
      // Fix #9: Structured error logging with less verbosity
  protected async createRecordAccessAuditLogRecord(provider: DatabaseProviderBase, userPayload: UserPayload, entityName: string, recordId: any): Promise<any> {
      const entityInfo = md.Entities.find((e) => e.Name.trim().toLowerCase() === entityName.trim().toLowerCase());
      if (entityInfo.AuditRecordAccess) {
        const userInfo = UserCache.Users.find((u) => u.Email.toLowerCase().trim() === userPayload?.email.toLowerCase().trim());
        const auditLogTypeName = 'Record Accessed';
        const auditLogType = md.AuditLogTypes.find((a) => a.Name.trim().toLowerCase() === auditLogTypeName.trim().toLowerCase());
        if (!userInfo) throw new Error(`User ${userPayload?.email} not found in metadata`);
        if (!auditLogType) throw new Error(`Audit Log Type ${auditLogTypeName} not found in metadata`);
        return await this.createAuditLogRecord(provider, userPayload, null, auditLogTypeName, 'Success', null, entityInfo.ID, recordId);
  protected getRowLevelSecurityWhereClause(provider: DatabaseProviderBase, entityName: string, userPayload: UserPayload, type: EntityPermissionType, returnPrefix: string) {
    return entityInfo.GetUserRowLevelSecurityWhereClause(user, type, returnPrefix);
  protected async createAuditLogRecord(
    userPayload: UserPayload,
    authorizationName: string | null,
    status: string,
    details: string | null,
    recordId: any | null
      const authorization = authorizationName
        ? md.Authorizations.find((a) => a.Name.trim().toLowerCase() === authorizationName.trim().toLowerCase())
      const auditLog = await md.GetEntityObject<MJAuditLogEntity>('MJ: Audit Logs', userInfo); // must pass user context on back end as we're not authenticated the same way as the front end
      auditLog.UserID = userInfo.ID;
      auditLog.AuditLogTypeID = auditLogType.ID;
      if (authorization) {
        auditLog.AuthorizationID = authorization.ID;
      if (status?.trim().toLowerCase() === 'success') auditLog.Status = 'Success';
      else auditLog.Status = 'Failed';
      if (details) auditLog.Details = details;
      if (recordId) auditLog.RecordID = recordId;
      if (await auditLog.Save()) 
        return auditLog;
        throw new Error(`Error saving audit log record`);
  protected safeFirstArrayElement<T = any>(arr: T[]) {
    if (arr && arr.length > 0) {
      return arr[0];
  protected packageSPParam(paramValue: any, quoteString: string) {
    return paramValue === null || paramValue === undefined ? null : quoteString + paramValue + quoteString;
  protected GetUserFromEmail(email: string): UserInfo | undefined {
    return UserCache.Users.find((u) => u.Email.toLowerCase().trim() === email.toLowerCase().trim());
  protected GetUserFromPayload(userPayload: UserPayload): UserInfo | undefined {
    if (!userPayload) 
    if (userPayload.userRecord)
      return userPayload.userRecord; // if we have a user record, use that directly
    if (!userPayload.email) 
    return UserCache.Users.find((u) => u.Email.toLowerCase().trim() === userPayload.email.toLowerCase().trim());
  public get MJCoreSchema(): string {
    return Metadata.Provider.ConfigData.MJCoreSchemaName;
  protected ListenForEntityMessages(entityObject: BaseEntity, pubSub: PubSubEngine, userPayload: UserPayload) {
    // The unique key is set up for each entity object via it's primary key to ensure that we only have one listener at most for each unique
    // entity in the system. This is important because we don't want to have multiple listeners for the same entity as it could
    // cause issues with multiple messages for the same event.
    const uniqueKey = entityObject.EntityInfo.Name;
    if (!this.EventSubscriptions.has(uniqueKey)) {
      // listen for events from the entityObject in case it is a long running task and we can push messages back to the client via pubSub
      LogDebug(`ResolverBase.ListenForEntityMessages: About to call MJGlobal.Instance.GetEventListener() to get the event listener subscription for ${uniqueKey}`);
      const theSub = MJGlobal.Instance.GetEventListener(false).subscribe(async (event: MJEvent) => {
          const baseEntity = <BaseEntity>event.args?.baseEntity;
          // Only process events for the entity type this subscription was created for
          // This prevents duplicate CloudEvents when multiple entity types have active subscriptions
          if (baseEntity?.EntityInfo?.Name !== uniqueKey) {
          const baseEntityValues = baseEntity ? baseEntity.GetAll() : null;
          const eventToLog = { entityName: entityObject.EntityInfo.Name, baseEntity: baseEntityValues, event: event.event, eventCode: event.eventCode };
          LogDebug(`ResolverBase.ListenForEntityMessages: Received Event from within MJGlobal.Instance.GetEventListener() callback. Will call EmitCloudEvent() next\nEvent data:\n${JSON.stringify(eventToLog)}`);
          await this.EmitCloudEvent(event);
          LogDebug(`ResolverBase.ListenForEntityMessages: EmitCloudEvent() completed successfully`);  
          if (event.args && event.args instanceof BaseEntityEvent) {
            const baseEntityEvent = event.args as BaseEntityEvent;
            // message from our entity object, relay it to the client
            LogDebug('ResolverBase.ListenForEntityMessages: About to publish PUSH_STATUS_UPDATES_TOPIC');
            pubSub.publish(PUSH_STATUS_UPDATES_TOPIC, {
              message: JSON.stringify({
                status: 'OK',
                type: 'EntityObjectStatusMessage',
                entityName: baseEntityEvent.baseEntity.EntityInfo.Name,
                primaryKey: baseEntityEvent.baseEntity.PrimaryKey,
                message: event.args.payload,
              sessionId: userPayload.sessionId,
      this.EventSubscriptions.set(uniqueKey, theSub);
  protected async CreateRecord(entityName: string, input: any, provider: DatabaseProviderBase, userPayload: UserPayload, pubSub: PubSubEngine) {
    // Check API key scope authorization for entity create operations
    await this.CheckAPIKeyScopeAuthorization('entity:create', entityName, userPayload);
    if (await this.BeforeCreate(provider, input)) {
      // fire event and proceed if it wasn't cancelled
      const entityObject = await provider.GetEntityObject(entityName, this.GetUserFromPayload(userPayload));
      entityObject.NewRecord();
      entityObject.SetMany(input);
      this.ListenForEntityMessages(entityObject, pubSub, userPayload);
      // Pass the transactionScopeId from the user payload to the save operation
      if (await entityObject.Save()) {
        // save worked, fire the AfterCreate event and then return all the data
        await this.AfterCreate(provider, input); // fire event
        // MapFieldNamesToCodeNames now handles encryption filtering as well
        return await this.MapFieldNamesToCodeNames(entityName, entityObject.GetAll(), contextUser);
      // save failed, throw error with message
        throw new GraphQLError(entityObject.LatestResult?.CompleteMessage ?? 'Unknown error creating record', {
          extensions: { code: 'CREATE_ENTITY_ERROR', entityName },
  // Before/After CREATE Event Hooks for Sub-Classes to Override
  protected async BeforeCreate(provider: DatabaseProviderBase, input: any): Promise<boolean> {
  protected async AfterCreate(provider: DatabaseProviderBase, input: any) {}
  protected async UpdateRecord(entityName: string, input: any, provider: DatabaseProviderBase, userPayload: UserPayload, pubSub: PubSubEngine) {
    // Check API key scope authorization for entity update operations
    await this.CheckAPIKeyScopeAuthorization('entity:update', entityName, userPayload);
    if (await this.BeforeUpdate(provider, input)) {
      const userInfo = this.GetUserFromPayload(userPayload);
      const entityObject = await provider.GetEntityObject(entityName, userInfo);
      const entityInfo = entityObject.EntityInfo;
      const clientNewValues = {};
        if (key !== 'OldValues___') {
          clientNewValues[key] = input[key];
      }); // grab all the props except for the OldValues property
      if (entityInfo.TrackRecordChanges || !input.OldValues___) {
        // We get here because EITHER the entity tracks record changes OR the client did not provide OldValues, so we need to load the old values from the DB
        const cKey = new CompositeKey(
          entityInfo.PrimaryKeys.map((pk) => {
              Value: input[pk.CodeName],
        if (await entityObject.InnerLoad(cKey)) {
          // load worked, now, only IF we have OldValues, we need to check them against the values in the DB we just loaded.
          if (input.OldValues___) {
            // we DO have OldValues, so we need to do a more in depth analysis
            await this.TestAndSetClientOldValuesToDBValues(input, clientNewValues, entityObject, userInfo);
            // no OldValues, so we can just set the new values from input
          // save failed, return null
          throw new GraphQLError(`Record not found for ${entityName} with key ${JSON.stringify(cKey)}`, {
            extensions: { code: 'LOAD_ENTITY_ERROR', entityName },
        // we get here if we are NOT tracking changes and we DO have OldValues, so we can load from them
        const oldValues = {};
        // for each item in the oldValues array, add it to the oldValues object
        input.OldValues___?.forEach((item) => (oldValues[item.Key] = item.Value));
        // 1) load the old values, this will be the initial state of the object
        await entityObject.LoadFromData(oldValues);
        // 2) set the new values from the input, not including the OldValues property
        entityObject.SetMany(clientNewValues);
        // save worked, fire afterevent and return all the data
        await this.AfterUpdate(provider, input); // fire event
        return await this.MapFieldNamesToCodeNames(entityName, entityObject.GetAll(), userInfo);
        throw new GraphQLError(entityObject.LatestResult?.Message ?? 'Unknown error', {
          extensions: { code: 'SAVE_ENTITY_ERROR', entityName },
      throw new GraphQLError('Save Canceled by BeforeSave() handler in ResolverBase', {
   * This routine compares the OldValues property in the input object to the values in the DB that we just loaded. If there are differences, we need to check to see if the client
   * is trying to update any of those fields (e.g. overlap). If there is overlap, we throw an error. If there is no overlap, we can proceed with the update even if the DB Values
   * and the ClientOldValues are not 100% the same, so long as there is no overlap in the specific FIELDS that are different.
   * ASSUMES: input object has an OldValues___ property that is an array of Key/Value pairs that represent the old values of the record that the client is trying to update.
  protected async TestAndSetClientOldValuesToDBValues(input: any, clientNewValues: any, entityObject: BaseEntity, contextUser: UserInfo) {
    // we have OldValues, so we need to compare them to the values we just loaded from the DB
    const clientOldValues = {};
    // for each item in the oldValues array, add it to the clientOldValues object
    input.OldValues___.forEach((item) => {
      // we need to do a quick transform on the values to make sure they match the TS Type for the given field because item.Value will always be a string
      const field = entityObject.EntityInfo.Fields.find((f) => f.CodeName === item.Key);
      let val = item.Value;
      if ((val === null || val === undefined) && field.DefaultValue !== null && field.DefaultValue !== undefined && !field.AllowsNull)
        val = field.DefaultValue; // set default value as the field was never set and it does NOT allow nulls
            if (val == null && val == undefined) {
              val = null;
              let typeLowered = (field.Type as string).toLowerCase();
              switch (typeLowered) {
                  val = parseInt(val);
                  val = parseFloat(val);
            val = val === null || val === undefined || val === 'false' || val === '0' || parseInt(val) === 0 ? false : true;
            // first, if val is a string and it is actually a number (milliseconds since epoch), convert it to a number.
            if (val !== null && val !== undefined && val.toString().trim() !== '' && !isNaN(val)) val = parseInt(val);
            val = val !== null && val !== undefined ? new Date(val) : null;
            break; // already a string
      clientOldValues[item.Key] = val;
    // clientOldValues now has all of the oldValues the CLIENT passed us. Now we need to build the same kind of object
    // with the DB values
    const dbValues = entityObject.GetAll();
    // now we need to compare clientOldValues and dbValues and have a new array that has entries for any differences and have FieldName, clientOldValue and dbValue as properties
    const dbDifferences = [];
    Object.keys(clientOldValues).forEach((key) => {
      const f = entityObject.EntityInfo.Fields.find((f) => f.CodeName === key);
      let different = false;
      switch (typeof clientOldValues[key]) {
          different = clientOldValues[key] !== dbValues[key];
          if (clientOldValues[key] instanceof Date) {
            const clientDate = clientOldValues[key] as Date;
            const dbDate = dbValues[key];
            if (dbDate == null || !(dbDate instanceof Date)) {
              different = true;
            } else if (clientDate.getTime() !== dbDate.getTime()) {
              // Check if this is a datetime/datetime2 field with only a timezone hour shift
              const sqlType = f?.SQLFullType?.trim().toLowerCase() || '';
              if (sqlType !== 'datetimeoffset' && IsOnlyTimezoneShift(clientDate, dbDate)) {
                  `Timezone hour shift detected on field "${key}" (${sqlType || 'datetime'}). ` +
                  `Client: ${clientDate.toISOString()}, DB: ${dbDate.toISOString()}. ` +
                  `Consider using datetimeoffset to avoid timezone ambiguity.`
                different = false; // Allow timezone shifts through for non-datetimeoffset fields
          } else if (clientOldValues[key] === null) {
            different = dbValues[key] !== null;
      if (different && f && !f.ReadOnly) {
        // only include updateable fields
        dbDifferences.push({
          FieldName: key,
          ClientOldValue: clientOldValues[key],
          DBValue: dbValues[key],
    if (dbDifferences.length > 0) {
      // now we have an array of any dbDifferences with length > 0, between the clientOldValues and the dbValues, we need to check to see if any of the differences are on fields that the client is trying to update
      // first step is to get clientNewValues into an object that is like clientOldValues, get the diff and then compare that diff to the differences array that shows diff between DB and ClientOld
      const clientDifferences = [];
        if (clientOldValues[key] !== clientNewValues[key] && f && f.AllowUpdateAPI && !f.IsPrimaryKey) {
          clientDifferences.push({
            ClientNewValue: clientNewValues[key],
      // now we have clientDifferences which shows what the client thinks they are changing. And, we have the dbDifferences array that shows changes between the clientOldValues and the dbValues
      // if there is ANY overlap in the FIELDS that appear in both arrays, we need to log a warning but allow the save to continue
      const overlap = clientDifferences.filter((cd) => dbDifferences.find((dd) => dd.FieldName === cd.FieldName));
      if (overlap.length > 0) {
        const msg = {
          Message:
            'Inconsistency between old values provided for changed fields, and the values of one or more of those fields in the database. Save operation continued with warning.',
          ClientDifferences: clientDifferences,
          DBDifferences: dbDifferences,
          Overlap: overlap,
        // Log as warning to console and ErrorLog table instead of throwing error
        console.warn('Entity save inconsistency detected but allowing save to continue:', JSON.stringify(msg));
          service: 'ResolverBase',
          operation: 'TestAndSetClientOldValuesToDBValues',
          error: `Entity save inconsistency detected: ${JSON.stringify(msg)}`,
            entityName: entityObject.EntityInfo.Name,
            clientDifferences: clientDifferences,
            dbDifferences: dbDifferences,
            overlap: overlap
        // Create ErrorLog record in the database
          const errorLogEntity = await md.GetEntityObject<MJErrorLogEntity>('MJ: Error Logs', contextUser);
          errorLogEntity.Code = 'ENTITY_SAVE_INCONSISTENCY';
          errorLogEntity.Message = `Entity save inconsistency detected for ${entityObject.EntityInfo.Name}: ${JSON.stringify(msg)}`;
          errorLogEntity.Status = 'Warning';
          errorLogEntity.Category = 'Entity Save';
          errorLogEntity.CreatedBy = contextUser.Email || contextUser.Name;
          const saveResult = await errorLogEntity.Save();
            console.error('Failed to save ErrorLog record');
        } catch (errorLogError) {
          console.error('Error creating ErrorLog record:', errorLogError);
    // If we get here that means we've not thrown an exception, so there is
    // NO OVERLAP, so we can set the new values from the data provided from the client now...
  protected async DeleteRecord(
    key: CompositeKey,
    options: DeleteOptionsInput,
    pubSub: PubSubEngine
    // Check API key scope authorization for entity delete operations
    await this.CheckAPIKeyScopeAuthorization('entity:delete', entityName, userPayload);
    if (await this.BeforeDelete(provider, key)) {
      await entityObject.InnerLoad(key);
      const returnValue = entityObject.GetAll(); // grab the values before we delete so we can return last state before delete if we are successful.
      if (await entityObject.Delete(options)) {
        await this.AfterDelete(provider, key); // fire event
          extensions: { code: 'DELETE_ENTITY_ERROR', entityName },
      throw new GraphQLError('Delete operation canceled by BeforeDelete() handler in ResolverBase', {
  // Before/After DELETE Event Hooks for Sub-Classes to Override
  protected async BeforeDelete(provider: DatabaseProviderBase, key: CompositeKey): Promise<boolean> {
  protected async AfterDelete(provider: DatabaseProviderBase, key: CompositeKey) {}
  // Before/After UPDATE Event Hooks for Sub-Classes to Override
  protected async BeforeUpdate(provider: DatabaseProviderBase, input: any): Promise<boolean> {
  protected async AfterUpdate(provider: DatabaseProviderBase, input: any) {}
