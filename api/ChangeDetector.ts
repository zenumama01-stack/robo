import { BaseEngine, BaseEnginePropertyConfig, BaseEntity, CompositeKey, ConsoleColor, EntityField, EntityFieldTSType, EntityInfo, IMetadataProvider, KeyValuePair, LogError, LogStatus, Metadata, RunView, UpdateCurrentConsoleLine, UpdateCurrentConsoleProgress, UserInfo } from "@memberjunction/core";
import { MJRecordChangeEntity, MJRecordChangeReplayRunEntity } from "@memberjunction/core-entities";
 * Represents a change to a single field in a record
export class FieldChange {
    public FieldName: string;
    public OldValue: any;
    public NewValue: any;
 * Represents a change to a single record in an entity
export class ChangeDetectionItem {
    public Entity: EntityInfo;
    public PrimaryKey: CompositeKey;
    public Type: 'Create' | 'Update' | 'Delete';
    public ChangedAt: Date;
    public Changes: FieldChange[];
     * Populated for Create and Update types only. This is the latest version of the record from the organic database table
    public LatestRecord?: BaseEntity;
    public LegacyKey?: boolean = false; // if true, this means that the key was a single value and not a concatenated key
    public LegacyKeyValue?: string; // if LegacyKey is true, this will be the single value of the key
 * Result type for a change detection operation
export class ChangeDetectionResult {
    public ErrorMessage?: string;
    public Changes: ChangeDetectionItem[];
 * Engine to handle detection of external changes and "replay" them in MemberJunction to invoke all standard functionality
 * such as BaseEntity sub-classes as well as any custom logic that are tied to the system via Actions/AI Actions/etc...
 * This is a server only package
export class ExternalChangeDetectorEngine extends BaseEngine<ExternalChangeDetectorEngine> {
        const p = <SQLServerDataProvider> ( provider || Metadata.Provider );
        const c: Array<Partial<BaseEnginePropertyConfig>> = [
                EntityName: "MJ: Entities",
                PropertyName: "_EligibleEntities",
                Filter: `ID IN (SELECT ID FROM ${p.MJCoreSchemaName}.vwEntitiesWithExternalChangeTracking)`, // limit to entities that are in this view. This view has the logic which basically is TrackRecordChanges=1 and also has an UpdatedAt field
        await this.Load(c, provider, forceRefresh, contextUser);
    public static get Instance(): ExternalChangeDetectorEngine {
        return super.getInstance<ExternalChangeDetectorEngine>();
    private _IneligibleEntities: string[] = [];// ['Entities', 'MJ: Entity Fields', 'MJ: Entity Field Values', 'MJ: Entity Relationships', 'MJ: Record Changes']; // default ineligible entities --- turned off for now
     * A list of entities that will automatically be excluded from all calls to this class. This array is used as a "safety"
     * mechanism to prevent the system from trying to replay changes to these entities which wouldn't negatively affect system integrity
     * but could cause performance issues. If you want to add/remove entities to this list, you can do so by manipulating this array.
     * If you want to run Change Detection and/or replay changes for entities in this array, you will need to remove them
     * in your code before calling the methods in this class. While executing methods on this class with these default 
     * ineligible entities will not cause any issues, they may take a long time to run and utilize a significant amount
     * of resources. 
    public get IneligibleEntities(): string[] {
        return this._IneligibleEntities;
    private _EligibleEntities: EntityInfo[];
     * A list of the entities that are eligible for external change detection. This is determined by using the underlying 
     * database view vwEntitiesWithExternalChangeTracking which is a view that is maintained by the MJ system and is used to
     * find a list of entities that have the required characteristics that support external change detection. These characteristics
     * include:
     *  * The entity has the TrackRecordChanges property set to 1
     *  * The entity has the special UpdatedAt/CreatedAt fields (which are called __mj_UpdatedAt and __mj_CreatedAt in the database). These fields are AUTOMATICALLY added to an entity that has TrackRecordChanges set to 1 by the MJ CodeGen tool.
     *  * The entity is not in the IneligibleEntities list. See info on the IneligibleEntities property for more information on excluded entities.
    public get EligibleEntities(): EntityInfo[] {
        const netEligible = this._EligibleEntities.filter(e => !this.IneligibleEntities.map(i => i.toLowerCase().trim()).includes(e.Name.toLowerCase().trim()));
        return netEligible;
     * Detects external changes for a single entity
    public async DetectChangesForEntity(entity: EntityInfo): Promise<ChangeDetectionResult> {
            // check to make sure that the entity is in the eligible list
                throw new Error("entity parameter is required");
            else if (!this.EligibleEntities.find(e => e.ID === entity.ID)) {
                throw new Error(`Entity ${entity.Name} is not eligible for external change detection. Refer to the documentation on the EligibleEntities and IneligibleEntities properties for more information.`);
            const sqlCreates = this.generateDetectCreationsQuery(entity);
            const sqlUpdates = this.generateDetectUpdatesQuery(entity);
            const sqlDeletes = this.generateDetectDeletionsQuery(entity);
            const provider = Metadata.Provider as SQLServerDataProvider;
            const createResult = await provider.ExecuteSQL(sqlCreates);
            const updateResult = await provider.ExecuteSQL(sqlUpdates);
            const deleteResult = await provider.ExecuteSQL(sqlDeletes);
            // we have the results for all of the queries, now we need to convert them into ChangeDetectionItems
            const changes: ChangeDetectionItem[] = [];
            if (createResult && createResult.length > 0) {
                for (const row of createResult) {
                    const item = new ChangeDetectionItem();
                    item.Entity = entity;
                    item.PrimaryKey = new CompositeKey(entity.PrimaryKeys.map(pk => { 
                            Value: row[pk.Name]
                    item.Type = 'Create';
                    item.ChangedAt = row.CreatedAt >= row.UpdatedAt ? row.CreatedAt : row.UpdatedAt;
                    item.Changes = []; // not relevant because the row is new 
                    changes.push(item);
            if (updateResult && updateResult.length > 0) {
                for (const row of updateResult) {
                    item.Type = 'Update';
                    item.ChangedAt = row.UpdatedAt;
                    // push the item but first make sure it is NOT already in the changes from the
                    // create detection, if it is, we do not push it into changes
                    if (!changes.find(c => c.PrimaryKey.Equals(item.PrimaryKey))) {
            if (deleteResult && deleteResult.length > 0) {
                deleteResult.forEach(row => {
                    const ck = new CompositeKey();
                    // row.RecordID should have a format of Field1|Value1||Field2|Value2, however in some cases there is legacy
                    // data in the RecordChange table that just has a single value in it and in that case assuming that the entity
                    // in question has a single-valued primary key, we can just use that value as the key, so we need to test for that
                    // first and if we find that the RecordID is just a single value, we can use that as the key
                    if (row.RecordID.indexOf(CompositeKey.DefaultValueDelimiter) === -1) {
                        // there is no field delimiter, so we can assume this is a single value
                        ck.LoadFromSingleKeyValuePair(entity.PrimaryKeys[0].Name, row.RecordID); // this is a string like 'Field1Value' (no quotes
                        item.LegacyKey = true;
                        item.LegacyKeyValue = row.RecordID;
                        ck.LoadFromConcatenatedString(row.RecordID); // this is a string like 'Field1Value|Field2Value' (no quotes)
                    item.PrimaryKey = ck;
                    item.Type = 'Delete';
                    item.ChangedAt = row.ChangedAt;
                    item.Changes = []; // not relevant because the row is now deleted
            await this.GetLatestDatabaseRecords(md, changes); // load everything from the database in one step
            // now we have latest records, go back through and update the Changes field for the UPDATE types
            for (const c of changes) {
                if (c.Type === 'Update') {
                    const changesResult = await this.DetermineRecordChanges(md, c);
                    c.Changes = changesResult.changes;
                Changes: changes 
                ErrorMessage: e.message,
                Changes: [] 
     * This method compares a version of the record in question from the database with the last version we had in RecordChange table
     * @param change 
    public async DetermineRecordChanges(md: Metadata, change: ChangeDetectionItem): Promise<{changes: FieldChange[], latestRecord: BaseEntity}> {
            // Step 1 - load the current record if needed, sometimes already loaded by here
            const record = change.LatestRecord ? change.LatestRecord : await this.GetLatestDatabaseRecord(md, change);
                // now we have the version from the database that has been updated from an external source
                // then we need to get the latest version from the vwRecordChanges table that matches this entity and RecordID
                const result = await this.GetLatestRecordChangesDataForEntityRecord(change);
                if (result && result.FullRecordJSON && result.FullRecordJSON.length > 0) {
                    // we have our row, so get the JSON, parse it and we'll have the differences
                    const json = JSON.parse(result.FullRecordJSON);
                    // now go through each field in the record object and compare it with the json
                    const changes: FieldChange[] = [];
                    for (const field of record.Fields) {
                        if (!field.IsPrimaryKey) {
                            const differResult = this.DoValuesDiffer(field, field.Value, json[field.Name])
                            if (differResult.differ) {
                                    NewValue: differResult.castValue1, // use the typecast values so they're the right types
                                    OldValue: differResult.castValue2  // use the typecast values so they're the right types
                    return {changes, latestRecord: record};
                    LogStatus(`      WARNING: No record found, or no FullRecordJSON found, in vwRecordChanges for ${change.Entity.Name}: ${change.PrimaryKey.ToConcatenatedString()}`);
                    return {changes: [], latestRecord: record};
                throw new Error(`Failed to load record: ${change.Entity.Name}: ${change.PrimaryKey}`);        
            return {changes: [], latestRecord: null};
    protected DoValuesDiffer(field: EntityField, value1: any, value2: any): {differ: boolean, castValue1: any, castValue2: any} {
        // type specific comparisons
        // for each scenario, make sure both value1 and value 2 are of the type we care about, if they're not, create new variables 
        // of those types and then do type specific comparisons for equality
        switch (field.EntityFieldInfo.TSType) {
                // check to see if value1 and value2 are both boolean, if not, convert them to boolean
                const v1 = typeof value1 === 'boolean' ? value1 : value1 === 'true' ? true : false;
                const v2 = typeof value2 === 'boolean' ? value2 : value2 === 'true' ? true : false;
                return {differ: v1 !== v2, castValue1: v1, castValue2: v2};
                // check to see if value1 and value2 are both dates, if not, convert them to dates
                const d1 = value1 instanceof Date ? value1 : new Date(value1);
                const d2 = value2 instanceof Date ? value2 : new Date(value2);
                return {differ: d1.getTime() !== d2.getTime(), castValue1: d1, castValue2: d2};
                // check to see if value1 and value2 are both numbers, if not, convert them to numbers
                const n1 = value1 ? typeof value1 === 'number' ? value1 : parseFloat(value1) : value1;
                const n2 = value2 ? typeof value2 === 'number' ? value2 : parseFloat(value2) : value2;
                return {differ: n1 !== n2, castValue1: n1, castValue2: n2};
                // check to see if value1 and value2 are both strings, if not, convert them to strings
                const s1 = typeof value1 === 'string' || !value1 ? value1 : value1.toString();
                const s2 = typeof value2 === 'string' || !value2 ? value2 : value2.toString();
                return {differ: s1 !== s2, castValue1: s1, castValue2: s2};
                // don't know the type, shouldn't get here, just do a basic TypeScript equality check
                // with a single != instead of !== because we don't want to check type just value
                return {differ: value1 != value2, castValue1: value1, castValue2: value2};
    protected async GetLatestRecordChangesDataForEntityRecord(change: ChangeDetectionItem) {
        const Provider = <SQLServerDataProvider>this.ProviderToUse;
                        TOP 1 FullRecordJSON 
                        ${Provider.MJCoreSchemaName}.vwRecordChanges 
                        RecordID = '${change.PrimaryKey.ToConcatenatedString()}' 
                        AND EntityID = '${change.Entity.ID}' 
                        AND Status <> 'Pending'
                        ChangedAt DESC`;                
        const result = await Provider.ExecuteSQL(sql);
    protected async GetLatestDatabaseRecord(md: Metadata, change: ChangeDetectionItem): Promise<BaseEntity> {
        const record = await md.GetEntityObject(change.Entity.Name, this.ContextUser);
        if (await record.InnerLoad(change.PrimaryKey) ) {
     * Get all of the latest database records together in grouped queries for each entity that has records we need instead 
     * of one at a time like GetLatestDatabaseRecord does. This method will return true/false and will place the LatestRecord
     * into each item in the changes array for you.
     * @param changes 
    protected async GetLatestDatabaseRecords(md: Metadata, changes: ChangeDetectionItem[]): Promise<boolean> {
            // Step 1 - group by entity and get a complete list of entities from the changes
            const entities: {entity: EntityInfo, keys: CompositeKey[]}[] = [];
            const provider = <SQLServerDataProvider>this.ProviderToUse;
                let e= entities.find(e => e.entity.ID === c.Entity.ID)
                    e = {
                        entity: c.Entity,
                        keys: [c.PrimaryKey]
                    entities.push(e);
                    e.keys.push(c.PrimaryKey);
            // now we have a distinct list of entities and all of the pkeys for each one, so we can run a single
            // select statement for each entity
                const sql = `SELECT * FROM [${e.entity.SchemaName}].[${e.entity.BaseView}]
                            WHERE ${e.keys.map(k => `(${k.KeyValuePairs.map(kvp => {
                                    const f = e.entity.Fields.find(f => kvp.FieldName.trim().toLowerCase() === f.Name);
                                    const quotes = f?.NeedsQuotes ? "'" : "";
                                    return `[${kvp.FieldName}]=${quotes}${kvp.Value}${quotes}`
                                }).join(' AND ')})`).join(' OR ')} `
                const result = await provider.ExecuteSQL(sql);
                    // we have the rows from the result, now go back through each of the changes we have in the changes array
                    // and associate the data with each one 
                    for (const r of result) {
                        const kvp: KeyValuePair[] = e.entity.PrimaryKeys.map(pk => {
                                Value: r[pk.Name]
                        const changeItem = changes.find(ci => ci.Entity === e.entity && ci.PrimaryKey.EqualsKey(kvp))
                        if (changeItem) {
                            // found the match, update latest Record
                            const record = await md.GetEntityObject(changeItem.Entity.Name, this.ContextUser);
                            await record.LoadFromData(r);
                            changeItem.LatestRecord = record;
    protected getPrimaryKeyString(entity: EntityInfo, tablePrefix: string): string {
        return entity.PrimaryKeys.map(pk => `'${pk.Name}${CompositeKey.DefaultValueDelimiter}' + CAST(${tablePrefix}.[${pk.Name}] AS NVARCHAR(MAX))`).join(` + '${CompositeKey.DefaultFieldDelimiter}' + `);
    protected generateDetectUpdatesQuery(entity: EntityInfo): string {
        const primaryKeyString = this.getPrimaryKeyString(entity, 'ot')
                ${entity.PrimaryKeys.map(pk => `ot.[${pk.Name}]` ).join(', ')}, ot.${EntityInfo.UpdatedAtFieldName} AS UpdatedAt, rc.last_change_time LatestRecordChangeAt
                [${entity.SchemaName}].[${entity.BaseView}] ot
            INNER JOIN (
                    RecordID, MAX(ChangedAt) AS last_change_time
                    __mj.vwRecordChanges
                    Type IN ('Update', 'Create') AND EntityID = '${entity.ID}'
                    RecordID
            ) rc ON ${primaryKeyString} = rc.RecordID
                FORMAT(ot.${EntityInfo.UpdatedAtFieldName}, 'yyyy-MM-dd HH:mm:ss.fff') > COALESCE(FORMAT(rc.last_change_time, 'yyyy-MM-dd HH:mm:ss.fff'), '1900-01-01 00:00:00.000');`;
                // use up to 3 digits of precision because when we get the values back into JavaScript objects, Date objects only have 3 digits of precision
    protected generateDetectCreationsQuery(entity: EntityInfo): string {
        const primaryKeyString = this.getPrimaryKeyString(entity, 'ot');
                ${entity.PrimaryKeys.map(pk => `ot.[${pk.Name}]`).join(', ')}, ot.${EntityInfo.CreatedAtFieldName} AS CreatedAt, ot.${EntityInfo.UpdatedAtFieldName} AS UpdatedAt
            LEFT JOIN 
                __mj.vwRecordChanges rc 
                (${primaryKeyString} = rc.RecordID) AND 
                rc.Type = 'Create' AND 
                rc.EntityID = '${entity.ID}' 
                rc.RecordID IS NULL;
    protected generateDetectDeletionsQuery(entity: EntityInfo): string {
                rc.RecordID, MAX(rc.ChangedAt) ChangedAt
                rc.RecordID = ${primaryKeyString}
                ${entity.PrimaryKeys.map(pk => `ot.[${pk.Name}] IS NULL`).join(' AND ')}
                    NOT EXISTS 
                        SELECT rc2.ID FROM __mj.vwRecordChanges rc2 WHERE 
                        rc2.RecordID = rc.RecordID AND rc2.EntityID=rc.EntityID AND rc2.Type='Delete'
                rc.RecordID
        `; // last part of above query makes sure we don't include records already deleted in Record Changes
     * Detects changes across all of the entities specified
    public async DetectChangesForEntities(entities: EntityInfo[]): Promise<ChangeDetectionResult>  {
            if (!entities)
                throw new Error("entities parameter is required");
            else if (entities.length === 0)
                throw new Error("entities parameter must have at least one entity in it");
            const result: ChangeDetectionResult = new ChangeDetectionResult();
            result.Changes = [];
            const promises = [];
            // fire them all off in parallel
            LogStatus(`Detecting changes for ${entities.length} entities`)
            let numFinished: number = 0;
            entities.forEach((e) => {
                UpdateCurrentConsoleLine(`   Starting change detection changes for ${e.Name}`, ConsoleColor.gray);
                const p = this.DetectChangesForEntity(e); // no await
                promises.push(p);
                p.then(entityResult => {
                    UpdateCurrentConsoleProgress(`   Finished change detection changes for ${e.Name}`, ++numFinished, entities.length, entityResult.Success ? ConsoleColor.cyan : ConsoleColor.crimson);
                    result.Changes = result.Changes.concat(entityResult.Changes);
                    result.Success = result.Success && entityResult.Success;    
            // now wait for all of the promises to finish
            UpdateCurrentConsoleProgress(`   Finished detecting changes for ${entities.length} entities`, entities.length, entities.length, result.Success ? ConsoleColor.green : ConsoleColor.red);
     * This method will detect changes for all eligible entities as defined by the EligibleEntities property
    public async DetectChangesForAllEligibleEntities(): Promise<ChangeDetectionResult> {
        return await this.DetectChangesForEntities(this.EligibleEntities);
     * This method will replay all of the items in the changes array
     * @param changes Array of changes to replay.
     * @param batchSize Optional, defines the # of concurrent changes to replay at once. If you want to replay changes serially, set this to 1
     * @returns {Promise<boolean>} - Returns true if all changes are successfully replayed, otherwise false.
    public async ReplayChanges(changes: ChangeDetectionItem[], batchSize: number = 20): Promise<boolean> {
        let run; // delcare outside of try block so we have access to it in the catch block
            if (changes && changes.length > 0) {
                run = await this.StartRun();
                LogStatus(`Replaying ${changes.length} changes`);
                let numProcessed = 0;
                for (let i = 0; i < changes.length; i += batchSize) {
                    const batch = changes.slice(i, i + batchSize);
                    // Process the current batch
                    const batchPromises = batch.map(async (change) => {
                        UpdateCurrentConsoleProgress(
                            `   Replaying ${change.Entity.Name} ${change.Type} ${change.PrimaryKey.ToConcatenatedString()}`,
                            ++numProcessed,
                            changes.length,
                            ConsoleColor.gray
                        const result = await this.ReplaySingleChange(md, run, change);
                        return { Success: result, change: change };
                    // Wait for all promises in the current batch to complete
                // for (const change of changes) {
                //     UpdateCurrentConsoleProgress(`   Replaying ${change.Entity.Name} ${change.Type} ${change.PrimaryKey.ToConcatenatedString()}`, changes.indexOf(change), changes.length, ConsoleColor.gray);
                //     const result = await this.ReplaySingleChange(md, run, change);
                //     results.push({Success: result, change: change});
                run.Status = results.every(r => r.Success) ? 'Complete' : 'Error';
                    return results.every(r => r.Success);
                    throw new Error("Failed to save run");
                return true; // no changes to process
            if (run) {
                // attempt to mark the run as error
                    run.Status = 'Error';
                    await run.Save(); // dont' care about return value here, we've already tried to save it and the return value will be false anyway and message is the original root cause of the first exception
                catch (innerError) {
                    LogError('Attempted to mark run as error failed, make sure you update the database for future runs to be allowed.');
                    LogError(innerError);
                    return false; // couldn't get it done
     * Method creates a new Record Change Replay Run and returns the object for the run
    protected async StartRun(): Promise<MJRecordChangeReplayRunEntity> {
        // first make sure an existing run isn't in progress
        const existingRun = await rv.RunView({
            EntityName: "MJ: Record Change Replay Runs",
            ExtraFilter: "Status NOT IN('Complete', 'Error')"
        if (existingRun && existingRun.Success) {
            if (existingRun.Results.length > 0) 
                throw new Error(`Existing Record Change Replay Run ${existingRun.Results[0].ID} is not complete or marked as error, cannot start a new run.`);
                const run = await md.GetEntityObject<MJRecordChangeReplayRunEntity>("MJ: Record Change Replay Runs", this.ContextUser)
                run.Status = 'In Progress';
                    throw new Error("Failed to start run");
            // failed to run the view
            throw new Error("Failed to check for existing run: " + existingRun.ErrorMessage);
     * Replays a single change item within a given run. This method will:
     *  1) Create a new Record Change record for the replay item
     *  2) Load a BaseEntity derived class for the entity in question and call either Save() or Delete() and pass in the Replay Only option
     *  3) Record the result in the Record Change record, updating status and if appropriate, the error message
    protected async ReplaySingleChange(md: Metadata, run: MJRecordChangeReplayRunEntity, change: ChangeDetectionItem): Promise<boolean> {
            const rc = await this.CreateRecordChangeRecord(md, run, change);
            if (rc) {
                // step 2 - get the base entity for the change
                let entityObject = change.LatestRecord; // for updates and creates we already have this
                if (!entityObject) {
                    // for deletes we don't have this yet - it is not the normal load from DB since it wont be in the database at all
                    entityObject = await md.GetEntityObject(change.Entity.Name, this.ContextUser);
                    const latestRCData = await this.GetLatestRecordChangesDataForEntityRecord(change);
                    if (latestRCData && latestRCData.FullRecordJSON?.length > 0) {
                        const obj = JSON.parse(latestRCData.FullRecordJSON);
                        await entityObject.LoadFromData(obj); // loaded up from the latest data in the Record Change table
                        // we have an issue becuase we don't have data in the RecordChange table so we can't replay the DELETE, so mark that in the error for the RecordChange
                        return this.FinishRecordChangeRecord(rc, 'error', 'No prior record data is available in the Record Changes entity, so it is not possible to replay the Delete');
                // if we get here, entityObject is now all set to go for replay!
                if (change.Type === 'Delete') {
                    const result = await entityObject.Delete({
                        ReplayOnly: true
                    return this.FinishRecordChangeRecord(rc, result ? 'success' : 'error', entityObject.LatestResult?.CompleteMessage);
                    // for updates and creates we just call Save() with the ReplayOnly option
                    const result = await entityObject.Save({
                        ReplayOnly: true,
                        IgnoreDirtyState: false //not relevant for replay
                throw new Error("Failed to create Record Change record");
     * This method will attempt to update the RecordChange record and return true if succesful, false otherwise.
    protected async FinishRecordChangeRecord(rc: MJRecordChangeEntity, code: 'error' | 'success', errorMessage: string): Promise<boolean> {
            rc.Status = code === 'error' ? 'Error' : 'Complete';
            rc.ErrorLog = errorMessage;
            if(await rc.Save())
     * Creates a new record change record for the start of the replay process.
    protected async CreateRecordChangeRecord(md: Metadata, run: MJRecordChangeReplayRunEntity, change: ChangeDetectionItem): Promise<MJRecordChangeEntity> {
            const rc = await md.GetEntityObject<MJRecordChangeEntity>("MJ: Record Changes", this.ContextUser);
            rc.EntityID = change.Entity.ID;
            if (change.LegacyKey)
                rc.RecordID = change.LegacyKeyValue; // need to match legacy key otherwise the other RC records will keep coming back up in detect changes runs in the future
                rc.RecordID = change.PrimaryKey.ToConcatenatedString();
            rc.Source = 'External';
            rc.Type = change.Type;
            rc.Status = 'Pending';
            if (change.ChangedAt)
                rc.ChangedAt = change.ChangedAt;
                // we don't have a ChangedAt from the database, so we need to use the current date
                // however, we want UTC date/time for now as that is what we use in the DB for all of
                // these fields.
                const d = new Date();
                rc.ChangedAt = new Date(d.toISOString());
            const changesObject = {};
            for (const field of change.Changes) {
                changesObject[field.FieldName] = {
                    field: field.FieldName,
                    oldValue: field.OldValue,
                    newValue: field.NewValue
            if (change.Type === 'Update') 
                rc.ChangesJSON = JSON.stringify(changesObject);
                rc.ChangesJSON = ''; // not null
            if (change.Type !== 'Delete') {
                const obj = change.LatestRecord?.GetAll();
                    rc.FullRecordJSON = JSON.stringify(obj);    
                    rc.FullRecordJSON = ''; // null not allowed
                rc.ChangesDescription = provider.CreateUserDescriptionOfChanges(changesObject);
            else if (change.Type === 'Create')
                rc.ChangesDescription = 'New Record';
                rc.ChangesDescription = 'Record Deleted';
            rc.ReplayRunID = run.ID;
            rc.UserID = this.ContextUser.ID;
            if (await rc.Save()) {
                throw new Error("Failed to save Record Change record: " + rc.LatestResult?.CompleteMessage);
