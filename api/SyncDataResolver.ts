import { Arg, Ctx, Field, InputType, Mutation, ObjectType, registerEnumType } from 'type-graphql';
import { BaseEntity, CompositeKey, EntityDeleteOptions, EntitySaveOptions, LogError, Metadata, RunView, UserInfo } from '@memberjunction/core';
export enum SyncDataActionType {
  registerEnumType(SyncDataActionType, {
    name: "SyncDataActionType", // GraphQL Enum Name
    description: "Specifies the type of action to be taken in syncing, Create, Update, CreateOrUpdate, Delete" // Description,
  export class ActionItemInputType {
      @Field(() => CompositeKeyInputType, {nullable: true})
      PrimaryKey?: CompositeKeyInputType;
      AlternateKey?: CompositeKeyInputType;
      @Field(() => SyncDataActionType) 
      Type!: SyncDataActionType;
export class ActionItemOutputType {
    @Field(() => CompositeKeyOutputType, {nullable: true})
    PrimaryKey?: CompositeKeyOutputType;
    AlternateKey?: CompositeKeyOutputType;
export class SyncDataResultType {
  @Field(() => [ActionItemOutputType])
  Results: ActionItemOutputType[] = [];
const __metadata_DatasetItems: string[] = [];
export class SyncDataResolver {
     * This mutation will sync the specified items with the existing system. Items will be processed in order and the results of each operation will be returned in the Results array within the return value.
    @Mutation(() => SyncDataResultType)
    async SyncData(
    @Arg('items', () => [ActionItemInputType] ) items: ActionItemInputType[],
            // iterate through the items 
            const results: ActionItemOutputType[] = [];
                results.push(await this.SyncSingleItem(item, context, md, context.userPayload)); 
            if (await this.DoSyncItemsAffectMetadata(context.userPayload.userRecord, items)) {
                await md.Refresh(); // force refesh the metadata which will cause a reload from the DB
            const overallSuccess = !results.some((r) => !r.Success); // if any element in the array of results has a Success value of false, then the overall success is false
            return { Success: overallSuccess, Results: results };
            throw new Error('SyncDataResolver::SyncData --- Error Syncing Data\n\n' + err);
    protected async GetLowercaseMetadataEntitiesList(user: UserInfo, forceRefresh: boolean = false): Promise<string[]> {
        if (forceRefresh || __metadata_DatasetItems.length === 0) {
            const rv = new RunView(); // cache this, veyr simple - should use an engine for this stuff later
            const result = await rv.RunView<MJDatasetItemEntity>({
                EntityName: "MJ: Dataset Items",
                ExtraFilter: "Dataset = 'MJ_Metadata'",
            }, user)
                __metadata_DatasetItems.length = 0;
                __metadata_DatasetItems.push(...result.Results.map((r) => {
                    return r.Entity.trim().toLowerCase();
        // now return the list of entities
        return __metadata_DatasetItems;
    protected async DoSyncItemsAffectMetadata(user: UserInfo, items: ActionItemInputType[]): Promise<boolean> {
        // check to see if any of the items affect any of these entities:
        const entitiesToCheck = await this.GetLowercaseMetadataEntitiesList(user, false);
            if (entitiesToCheck.find(e => e === item.EntityName.trim().toLowerCase()) ) {
        return false; // didn't find any
    protected async SyncSingleItem(item: ActionItemInputType, context: AppContext, md: Metadata, userPayload: UserPayload): Promise<ActionItemOutputType> {
        const result = new ActionItemOutputType();
        result.AlternateKey = item.AlternateKey;
        result.PrimaryKey = item.PrimaryKey;
        result.DeleteFilter = item.DeleteFilter;
        result.EntityName = item.EntityName;
        result.RecordJSON = item.RecordJSON;
        result.Type = item.Type;
        result.ErrorMessage = '';
            const e = md.Entities.find((e) => e.Name === item.EntityName);
                const pk = item.PrimaryKey ? new CompositeKey(item.PrimaryKey.KeyValuePairs) : null;
                const ak = item.AlternateKey ? new CompositeKey(item.AlternateKey.KeyValuePairs) : null;
                const entityObject = item.Type === SyncDataActionType.DeleteWithFilter ? null : await md.GetEntityObject(e.Name, context.userPayload.userRecord);
                const fieldValues = item.RecordJSON ? JSON.parse(item.RecordJSON) : {};
                    case SyncDataActionType.Create:
                        await this.SyncSingleItemCreate(entityObject, fieldValues, result, userPayload);
                    case SyncDataActionType.Update:
                        await this.SyncSingleItemUpdate(entityObject, pk, ak, fieldValues, result, userPayload);
                    case SyncDataActionType.CreateOrUpdate:
                        // in this case we attempt to load the item first, if it is not possible to load the item, then we create it
                        await this.SyncSingleItemCreateOrUpdate(entityObject, pk, ak, fieldValues, result, userPayload);
                    case SyncDataActionType.Delete:
                        await this.SyncSingleItemDelete(entityObject, pk, ak, result, userPayload);
                    case SyncDataActionType.DeleteWithFilter:
                        await this.SyncSingleItemDeleteWithFilter(item.EntityName, item.DeleteFilter, result, context.userPayload.userRecord, userPayload);
                        throw new Error('Invalid SyncDataActionType');
                throw new Error('Entity not found');
            result.ErrorMessage = typeof err === 'string' ? err : (err as any).message;
    protected async SyncSingleItemDeleteWithFilter(entityName: string, filter: string, result: ActionItemOutputType, user: UserInfo, userPayload: UserPayload) {
            // here we will iterate through the result of a RunView on the entityname/filter and delete each matching record
            let overallSuccess: boolean = true;
            let combinedErrorMessage: string = "";
            const data = await rv.RunView<BaseEntity>({
                for (const entityObject of data.Results) {
                    if (!await entityObject.Delete()) {
                        combinedErrorMessage += 'Failed to delete the item :' + entityObject.LatestResult.CompleteMessage + '\n';
                result.Success = overallSuccess
                if (!overallSuccess) {
                    result.ErrorMessage = combinedErrorMessage
                result.ErrorMessage = 'Failed to run the view to get the list of items to delete for entity: ' + entityName + ' with filter: ' + filter + '\n';
            result.ErrorMessage = typeof e === 'string' ? e : (e as any).message;
    protected async LoadFromAlternateKey(entityName: string, alternateKey: CompositeKey, user: UserInfo): Promise<BaseEntity> {
            // no primary key provided, attempt to look up the primary key based on the 
            const entity = md.EntityByName(entityName);
            const r = await rv.RunView<BaseEntity>({
                ExtraFilter: alternateKey.KeyValuePairs.map((kvp) => {
                    const fieldInfo = entity.Fields.find((f) => f.Name === kvp.FieldName);
                    const quotes = fieldInfo.NeedsQuotes ? "'" : '';
                    return `${kvp.FieldName} = ${quotes}${kvp.Value}${quotes}`;
                }).join(' AND '),
            if (r && r.Success && r.Results.length === 1) {
                return r.Results[0];
                //LogError (`Failed to load the item with alternate key: ${alternateKey.KeyValuePairs.map((kvp) => `${kvp.FieldName} = ${kvp.Value}`).join(' AND ')}. Result: ${r.Success} and ${r.Results?.length} items returned`);
    protected async SyncSingleItemCreateOrUpdate(entityObject: BaseEntity, pk: CompositeKey, ak: CompositeKey, fieldValues: any, result: ActionItemOutputType, userPayload: UserPayload) {
        if (!pk || pk.KeyValuePairs.length === 0) {
            // no primary key try to load from alt key
            const altKeyResult = await this.LoadFromAlternateKey(entityObject.EntityInfo.Name, ak, entityObject.ContextCurrentUser);
            if (!altKeyResult) {
                // no record found, create a new one
                await this.InnerSyncSingleItemUpdate(altKeyResult, fieldValues, result, userPayload);
            // have a primary key do the usual load
            if (await entityObject.InnerLoad(pk)) {
                await this.InnerSyncSingleItemUpdate(entityObject, fieldValues, result, userPayload);
    protected async SyncSingleItemDelete(entityObject: BaseEntity, pk: CompositeKey, ak: CompositeKey, result: ActionItemOutputType, userPayload: UserPayload) {
                result.ErrorMessage = 'Failed to load the item, it is possible the record with the specified primary key does not exist';
                // pass back the full record as it was JUST BEFORE the delete, often quite useful on the other end
                result.RecordJSON = await altKeyResult.GetDataObjectJSON({
                    oldValues: false
                if (await altKeyResult.Delete()) {
                    result.ErrorMessage = 'Failed to delete the item :' + entityObject.LatestResult.CompleteMessage;
        else if (await entityObject.InnerLoad(pk)) {
            result.RecordJSON = await entityObject.GetDataObjectJSON({
            if (await entityObject.Delete()) {
    protected async SyncSingleItemCreate(entityObject: BaseEntity, fieldValues: any, result: ActionItemOutputType, userPayload: UserPayload) {
        // make sure we strip out the primary key from fieldValues before we pass it in because otherwise it will appear to be an existing record to the BaseEntity
        const noPKValues = {...fieldValues};
        entityObject.EntityInfo.PrimaryKeys.forEach((pk) => {
            delete noPKValues[pk.Name];
        entityObject.SetMany(noPKValues);
            result.PrimaryKey = new CompositeKey(entityObject.PrimaryKeys.map((pk) => ({FieldName: pk.Name, Value: pk.Value})));
            // pass back the full record AFTER the sync, that's often quite useful on the other end
            result.ErrorMessage = 'Failed to create the item :' + entityObject.LatestResult.CompleteMessage;
    protected async SyncSingleItemUpdate(entityObject: BaseEntity, pk: CompositeKey, ak: CompositeKey, fieldValues: any, result: ActionItemOutputType, userPayload: UserPayload) {
            // no pk, attempt to load by alt key
                result.ErrorMessage = 'Failed to load the item, it is possible the record with the specified alternate key does not exist';
            // failed to load the item
    protected async InnerSyncSingleItemUpdate(entityObject: BaseEntity, fieldValues: any, result: ActionItemOutputType, userPayload: UserPayload) {
        entityObject.SetMany(fieldValues);
            if (!result.PrimaryKey || result.PrimaryKey.KeyValuePairs.length === 0) {
            result.ErrorMessage = 'Failed to update the item :' + entityObject.LatestResult.CompleteMessage;
