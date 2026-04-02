import { Arg, Ctx, Field, ObjectType, Query, Resolver } from "type-graphql";
import { GetReadOnlyDataSource, GetReadOnlyProvider } from "../util.js";
import { MJDataContextItemEntity } from "@memberjunction/core-entities";
export class GetDataContextItemDataOutputType {
     * If not successful, this will be the error message.
     * If successful, this will be the JSON for the data context item's data.
export class GetDataContextDataOutputType {
    @Field(() => [String], { nullable: 'itemsAndList' }) // Allow nulls inside array & entire field nullable
     * Each data context item's results will be converted to JSON and returned as a string
export class GetDataContextDataResolver extends ResolverBase {
     * Returns data for a given data context item.
     * @param DataContextItemID
    @Query(() => GetDataContextItemDataOutputType)
    async GetDataContextItemData(
        @Arg('DataContextItemID', () => String) DataContextItemID: string,
        @Ctx() appCtx: AppContext
        // Check API key scope authorization for data context read
        await this.CheckAPIKeyScopeAuthorization('datacontext:read', DataContextItemID, appCtx.userPayload);
            const ds = GetReadOnlyDataSource(appCtx.dataSources, {
                allowFallbackToReadWrite: true,
            const md = GetReadOnlyProvider(appCtx.providers, {allowFallbackToReadWrite: true});
            const dciData = await md.GetEntityObject<MJDataContextItemEntity>("MJ: Data Context Items", appCtx.userPayload.userRecord);
            if (await dciData.Load(DataContextItemID)) {
                const dci = DataContext.CreateDataContextItem(); // use class factory to get whatever lowest level sub-class is registered
                await dci.LoadMetadataFromEntityRecord(dciData, Metadata.Provider, appCtx.userPayload.userRecord);
                // now the metadata is loaded so we can call the regular load function
                if (await dci.LoadData(ds)) {
                        Result: JSON.stringify(dci.Data),
                        ErrorMessage: 'Error loading data context item data',
                    ErrorMessage: 'Error loading data context item metadata',
                ErrorMessage: e,
     * Returns data for a given data context.
     * @param DataContextID
    @Query(() => GetDataContextDataOutputType)
    async GetDataContextData(
        @Arg('DataContextID', () => String) DataContextID: string,
        await this.CheckAPIKeyScopeAuthorization('datacontext:read', DataContextID, appCtx.userPayload);
            // our job here is to load the entire data context, so we do that with the Data Context object
            const dc = new DataContext();
            const success = await dc.Load(DataContextID, ds, true, false, 0, appCtx.userPayload.userRecord);
                const retVal =   {
                    ErrorMessages: null,
                    Results: dc.Items.map((item) => {
                        return JSON.stringify(item.Data);
                    ErrorMessages: ['Error loading data context'],
                ErrorMessages: [e],
