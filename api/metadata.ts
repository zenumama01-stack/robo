import { DatasetItemFilterType, DatasetResultType, DatasetStatusResultType, EntityRecordNameInput, EntityRecordNameResult, EntityMergeOptions, ILocalStorageProvider, IMetadataProvider, PotentialDuplicateRequest, PotentialDuplicateResponse, ProviderConfigDataBase, ProviderType } from "./interfaces";
import { EntityDependency, EntityInfo, RecordDependency, RecordMergeRequest, RecordMergeResult } from "./entityInfo"
import { ApplicationInfo } from "./applicationInfo"
import { AuditLogTypeInfo, AuthorizationInfo, RoleInfo, UserInfo } from "./securityInfo";
import { QueryCategoryInfo, QueryFieldInfo, QueryInfo, QueryPermissionInfo } from "./queryInfo";
import { RunView } from "../views/runView";
 * Class used to access a wide array of MemberJunction metadata, to instantiate derived classes of BaseEntity for record access and manipulation and more. This class uses a provider model where different providers transparently plug-in to implement the functionality needed based on where the code is running. The provider in use is generally not of any importance to users of the class and code can be written indepdenent of tier/provider.
    private static _globalProviderKey: string = 'MJ_MetadataProvider';
     * When an application initializes, the Provider package that is being used for that application will handle setting the provider globally via this static property. 
     * This is done so that the provider can be accessed from anywhere in the application without having to pass it around. This pattern is used sparingly in MJ.
    public static get Provider(): IMetadataProvider {
            return g[Metadata._globalProviderKey];
    public static set Provider(value: IMetadataProvider) {
            g[Metadata._globalProviderKey] = value;
     * Forces a refresh of all cached metadata.
    public async Refresh(providerToUse?: IMetadataProvider): Promise<boolean> {
        return await Metadata.Provider.Refresh(providerToUse);
        return Metadata.Provider.ProviderType;
    public get Applications(): ApplicationInfo[] {
        return Metadata.Provider.Applications;
    public get Entities(): EntityInfo[] {
        return Metadata.Provider.Entities;
     * Helper method to find an entity by name in a case insensitive manner.  
    public EntityByName(entityName: string): EntityInfo {
        if (!entityName || typeof entityName !== 'string' || entityName.trim().length === 0) {
            throw new Error('EntityByName: entityName must be a non-empty string');
        return this.Entities.find(e => e.Name.toLowerCase().trim() === entityName.toLowerCase().trim());
     * Helper method to find an entity by ID
    public EntityByID(entityID: string): EntityInfo {
        return this.Entities.find(e => e.ID === entityID);
    public get Queries(): QueryInfo[] {
        return Metadata.Provider.Queries;
    public get QueryFields(): QueryFieldInfo[] {
        return Metadata.Provider.QueryFields;
    public get QueryCategories(): QueryCategoryInfo[] {
        return Metadata.Provider.QueryCategories;
    public get QueryPermissions(): QueryPermissionInfo[] {
        return Metadata.Provider.QueryPermissions;
     * Returns the current user, if known. In some execution environments, mainly on server tiers like in a node.js environment, there won't be a "current user" known to Metadata since the Metadata instance is shared across all requests. In this situation you should determine the current user from the server context where you get the user payload and find the user from the UserCache.
    public get CurrentUser(): UserInfo {
        return Metadata.Provider.CurrentUser;
    public get Roles(): RoleInfo[] {
        return Metadata.Provider.Roles;
    public get AuditLogTypes(): AuditLogTypeInfo[] {
        return Metadata.Provider.AuditLogTypes;
    public get Authorizations(): AuthorizationInfo[] {
        return Metadata.Provider.Authorizations;
    public get Libraries(): LibraryInfo[] {
        return Metadata.Provider.Libraries;
     * Returns all of the ExplorerNavigationItems that are visible to the user, sorted by Sequence. Filtered by the IsActive bit.
    public get VisibleExplorerNavigationItems(): ExplorerNavigationItem[] {
        return Metadata.Provider.VisibleExplorerNavigationItems;
     * Returns all of the ExplorerNavigationItems, including those that are not visible. This is useful for admin tools and other places where you need to see all of the navigation items, not just the ones that are visible to the user.
    public get AllExplorerNavigationItems(): ExplorerNavigationItem[] {
        return Metadata.Provider.AllExplorerNavigationItems;
     * Helper function to return an Entity Name from a given Entity ID.
    public EntityIDFromName(entityName: string): string {
        let entity = this.Entities.find(e => e.Name == entityName);
        if (entity != null)
            return entity.ID;
            throw new Error(`Entity ${entityName} not found`);
     * Helper function to return an Entity Name from an Entity ID
    public EntityNameFromID(entityID: string): string {
        let entity = this.Entities.find(e => e.ID == entityID);
        if(entity){
            return entity.Name;
            LogError(`Entity ID: ${entityID} not found`);
     * Helper function to return an EntityInfo from an Entity ID
    public EntityFromEntityID(entityID: string): EntityInfo | null {
     * Returns true if the combination of userId/entityName/KeyValuePairs has a favorite status on (meaning the user has marked the record as a "favorite" for easy access)
     * @param userId 
     * @param primaryKey 
    public async GetRecordFavoriteStatus(userId: string, entityName: string, primaryKey: CompositeKey, contextUser?: UserInfo): Promise<boolean> {
        return await Metadata.Provider.GetRecordFavoriteStatus(userId, entityName, primaryKey, contextUser);
     * Returns an array of records representing the list of changes made to a record. This functionality only works
     * if an entity has TrackRecordChanges = 1, which is the default for most entities. If TrackRecordChanges = 0, this method will return an empty array.
     * This method is defined in the @memberjunction/core package, which is lower level in the dependency 
     * hierarchy than the @memberjunction/core-entities package where the MJRecordChangeEntity class is defined.
     * For this reason, we are not using the MJRecordChangeEntity class here, but rather returning a generic type T.
     * When you call this method, you can specify the type T to be the MJRecordChangeEntity class or any other class that matches the structure of the record changes.
     * const md = new Metadata();
     * const changes: MJRecordChangeEntity[] = await md.GetRecordChanges<MJRecordChangeEntity>('MyEntity', myPrimaryKey);
    public async GetRecordChanges<T>(entityName: string, primaryKey: CompositeKey, contextUser?: UserInfo): Promise<Array<T>> {
            const e = this.EntityByName(entityName);
            if (!e.TrackRecordChanges) {
                LogStatus(`GetRecordChanges called for entity ${entityName} with primary key ${primaryKey.ToConcatenatedString()} but TrackRecordChanges is not enabled, returning empty array.`);
                return []; // Return empty array if TrackRecordChanges is not enabled
            const result = await rv.RunView<T>({
                EntityName: "MJ: Record Changes",
                ExtraFilter: `Entity='${entityName}' AND RecordID='${primaryKey.ToConcatenatedString()}'`,
                OrderBy: "ChangedAt DESC"
            LogError(`GetRecordChanges failed for entityName: ${entityName} and primaryKey: ${primaryKey}. Error: ${e.message}`);
     * Sets the favorite status for a given user for a specific entityName/KeyValuePairs
     * @param isFavorite 
    public async SetRecordFavoriteStatus(userId: string, entityName: string, primaryKey: CompositeKey, isFavorite: boolean, contextUser: UserInfo = null) {
        await Metadata.Provider.SetRecordFavoriteStatus(userId, entityName, primaryKey, isFavorite, contextUser);
     * Returns a list of dependencies - records that are linked to the specified Entity/Primary Key Value combination. A dependency is as defined by the relationships in the database. The MemberJunction metadata that is used
     * @param primaryKey the primary key value to check
        return await Metadata.Provider.GetRecordDependencies(entityName, primaryKey);
     * @param params object containing many properties used in fetching records and determining which ones to return
        return await Metadata.Provider.GetRecordDuplicates(params, contextUser);
    public async GetEntityDependencies(entityName: string): Promise<EntityDependency[]> {
        return await Metadata.Provider.GetEntityDependencies(entityName);
     * 1. The surviving record is loaded and fields are updated from the field map, if provided, and the record is saved. If a FieldMap not provided within the request object, this step is skipped.
     * 2. For each of the records that will be merged INTO the surviving record, we call the GetEntityDependencies() method and get a list of all other records in the database are linked to the record to be deleted. We then go through each of those dependencies and update the link to point to the SurvivingRecordKeyValuePair and save the record.
     * 3. The record to be deleted is then deleted.
     * IMPORTANT NOTE: This functionality ASSUMES that you are calling BEGIN TRANS and COMMIT TRANS/ROLLBACK TRANS outside of the work being done inside if you are on the database server side (not on the client side). The reason is that many API servers that use this object infrastructure have transaction wrappers for each individual API request
     * so we are not doing BEGIN/COMMIT/ROLLBACK within this functionality. If you are using this on the client side, you don't need to do anything extra, the server side, however, must wrap this with begin/commit/rollback statements to the database server.
     * If you're using MJAPI/MJServer this is done for you automatically.
        const e = this.EntityByName(request.EntityName);
        if (e.AllowRecordMerge)
            return await Metadata.Provider.MergeRecords(request, contextUser, options);
     * Creates a new instance of a BaseEntity subclass for the specified entity and automatically calls NewRecord() to initialize it.
     * This method uses the MJGlobal ClassFactory to instantiate the correct subclass based on registered entity types.
     * For entities with non-auto-increment uniqueidentifier primary keys, a UUID will be automatically generated.
     * @param entityName - The name of the entity to create (e.g., "Users", "Customers", "Orders")
     * @param contextUser - Optional user context for server-side operations. Client-side code can typically omit this.
     *                      Can be a UserInfo instance or an object with matching shape (ID, Name, Email, UserRoles)
     * @returns Promise resolving to a strongly-typed entity instance ready for data entry
     * const customer = await metadata.GetEntityObject<CustomerEntity>('Customers');
     * customer.Name = 'Acme Corp';
     * await customer.Save();
     * // Server-side with context user
     * const order = await metadata.GetEntityObject<OrderEntity>('Orders', contextUser);
     * order.CustomerID = customerId;
     * await order.Save();
    public async GetEntityObject<T extends BaseEntity>(entityName: string, contextUser?: UserInfo): Promise<T>;
     * Creates a new instance of a BaseEntity subclass and loads an existing record using the provided composite key.
     * This overload combines entity instantiation with record loading in a single call for convenience.
     * @param loadKey - CompositeKey containing the primary key value(s) to load. Use static helper methods:
     *                  - CompositeKey.FromID(id) for single "ID" primary keys
     *                  - CompositeKey.FromKeyValuePair(field, value) for single named primary keys
     *                  - CompositeKey.FromKeyValuePairs([...]) for composite primary keys
     * @throws Error if the entity name is invalid or the record cannot be found
     * // Load by ID (most common case)
     * const customer = await metadata.GetEntityObject<CustomerEntity>('Customers', CompositeKey.FromID(customerId));
     * // Load by named field
     * const user = await metadata.GetEntityObject<MJUserEntity>('Users', CompositeKey.FromKeyValuePair('Email', 'user@example.com'));
     * // Load with composite key
     * const orderItem = await metadata.GetEntityObject<OrderItemEntity>('OrderItems', 
     *   CompositeKey.FromKeyValuePairs([
     *     { FieldName: 'OrderID', Value: orderId },
     *     { FieldName: 'ProductID', Value: productId }
     *   ])
    public async GetEntityObject<T extends BaseEntity>(entityName: string, loadKey: CompositeKey, contextUser?: UserInfo): Promise<T>;
    public async GetEntityObject<T extends BaseEntity>(
        loadKeyOrContextUser?: CompositeKey | UserInfo,
        // validate that entityName is not null, undefined and IS a string > 0 length
            throw new Error('GetEntityObject: entityName must be a non-empty string');
        // Determine which overload was called
        let actualLoadKey: CompositeKey | undefined;
        let actualContextUser: UserInfo | undefined;
        if (loadKeyOrContextUser instanceof CompositeKey) {
            // Second overload: entityName, loadKey, contextUser
            actualLoadKey = loadKeyOrContextUser;
            actualContextUser = contextUser;
        } else if (contextUser !== undefined) {
            // Second overload with null/undefined loadKey: entityName, null/undefined, contextUser
            actualLoadKey = undefined;
            // First overload: entityName, contextUser
            actualContextUser = loadKeyOrContextUser as UserInfo;
        // validate that contextUser is either null/undefined or a UserInfo object
        if (actualContextUser) {
            // contextUser has been specified. We need to make sure the shape of the object
            // is correct to allow objects that are not true instances of UserInfo but have the
            // same shape - e.g. duck typing
            if (!(actualContextUser instanceof UserInfo)) {
                const u = actualContextUser as any;
                if (u && u.ID && u.Name && u.Email && u.UserRoles) {
                    // we have a UserInfo-like object, so we can use it
                    actualContextUser = new UserInfo(Metadata.Provider, u);
                    throw new Error('GetEntityObject: contextUser must be null/undefined, a UserInfo instance, or an object that has the same shape as UserInfo, notably having the following properties: ID, Name, Email, and UserRoles');
        return await Metadata.Provider.GetEntityObject(entityName, actualLoadKey, actualContextUser);
     * Returns the Name of the specific KeyValuePairs for a given entityName. This is done by 
    public async GetEntityRecordName(entityName: string, primaryKey: CompositeKey, contextUser?: UserInfo, forceRefresh: boolean = false): Promise<string> {
        let result = primaryKey.Validate();
        if(!result.IsValid){
            throw new Error(result.ErrorMessage);
        return await Metadata.Provider.GetEntityRecordName(entityName, primaryKey, contextUser, forceRefresh);
    public async GetEntityRecordNames(info: EntityRecordNameInput[], contextUser?: UserInfo, forceRefresh: boolean = false): Promise<EntityRecordNameResult[]> {
        // valiate to make sure we don't have any null primary keys being sent in
        for (let i = 0; i < info.length; i++) {
            if (!info[i].CompositeKey.KeyValuePairs || info[i].CompositeKey.KeyValuePairs.length == 0) {
                throw new Error('GetEntityRecordNames: KeyValuePairs cannot be null or empty. It is for item ' + i.toString() + ' in the input array.');
                // check each primary key value to make sure it's not null
                for (let j = 0; j < info[i].CompositeKey.KeyValuePairs.length; j++) {
                    if (!info[i].CompositeKey.KeyValuePairs[j] || !info[i].CompositeKey.KeyValuePairs[j].Value) {
                        throw new Error('GetEntityRecordNames: KeyValuePairs cannot contain null values. FieldName: ' + info[i].CompositeKey.KeyValuePairs[j]?.FieldName);
        return await Metadata.Provider.GetEntityRecordNames(info, contextUser, forceRefresh);
     * Creates a new TransactionGroup which can be used to bundle multiple database changes for BaseEntity derived classes to be processed as a single database transaction
        return await Metadata.Provider.CreateTransactionGroup();
     * Saves all the in-memory metadata to be updated in the local persistent storage method (which varies by provider). This generally shouldn't need to be called externally but is available to force an update to local storage as desired.
    public async SaveLocalMetadataToStorage() {
        await Metadata.Provider.SaveLocalMetadataToStorage();
     * Removes all the metadata from the local persistent storage method (which varies by provider). This generally shouldn't need to be called externally but is available to force an complete removal of local metadata in storage.
     * NOTE: this does not remove Datasets, for removing datasets, use ClearDatasetCache()
    public async RemoveLocalMetadataFromStorage() {
        await Metadata.Provider.RemoveLocalMetadataFromStorage();
     * Returns the local storage provider. This is used to store metadata locally on the client.
     * @returns - the local storage provider
     * @remarks - Use this for storing any type of data on the client. The Provider implements the storage mechanism which is persistent whenever possible, but in some cases purely in memory if local persistence is not available. Keep in mind that you must ensure that keys are unique so prefix all of your keys with something unique to avoid collisions.
        return Metadata.Provider.LocalStorageProvider;
        return Metadata.Provider.GetDatasetStatusByName(datasetName, itemFilters, contextUser, providerToUse);
        return Metadata.Provider.GetDatasetByName(datasetName, itemFilters, contextUser, providerToUse);
     * Gets a dataset by name, if required, and caches it in a format available to the client (e.g. IndexedDB, LocalStorage, File, etc). The cache method is Provider specific
    public async GetAndCacheDatasetByName(datasetName: string, itemFilters?: DatasetItemFilterType[], contextUser?: UserInfo): Promise<DatasetResultType>  {
        return Metadata.Provider.GetAndCacheDatasetByName(datasetName, itemFilters, contextUser);
    public async IsDatasetCacheUpToDate(datasetName: string, itemFilters?: DatasetItemFilterType[]): Promise<boolean> {
        return Metadata.Provider.IsDatasetCacheUpToDate(datasetName, itemFilters);
    public async GetCachedDataset(datasetName: string, itemFilters?: DatasetItemFilterType[]): Promise<DatasetResultType> {
        return Metadata.Provider.GetCachedDataset(datasetName, itemFilters);
    public async CacheDataset(datasetName: string, itemFilters: DatasetItemFilterType[], dataset: DatasetResultType): Promise<void> {
        return Metadata.Provider.CacheDataset(datasetName, itemFilters, dataset);
    public async IsDatasetCached(datasetName: string, itemFilters?: DatasetItemFilterType[]): Promise<boolean> {
        return Metadata.Provider.IsDatasetCached(datasetName, itemFilters);
    public GetDatasetCacheKey(datasetName: string, itemFilters?: DatasetItemFilterType[]): string {
        return Metadata.Provider.GetDatasetCacheKey(datasetName, itemFilters);
    public async ClearDatasetCache(datasetName: string, itemFilters?: DatasetItemFilterType[]): Promise<void> {
        return Metadata.Provider.ClearDatasetCache(datasetName, itemFilters);
    get ConfigData(): ProviderConfigDataBase {
        return Metadata.Provider.ConfigData;
import { Configuration } from "../configuration"
export interface Metadata {
   * The application name.
  readonly name?: string
   * The application description.
  readonly description?: string
   * The url to the project [homepage](https://docs.npmjs.com/files/package.json#homepage) (NuGet Package `projectUrl` (optional) or Linux Package URL (required)).
   * If not specified and your project repository is public on GitHub, it will be `https://github.com/${user}/${project}` by default.
  readonly homepage?: string | null
   * *linux-only.* The [license](https://docs.npmjs.com/files/package.json#license) name.
  readonly license?: string | null
  readonly author?: AuthorMetadata | null
   * The [repository](https://docs.npmjs.com/files/package.json#repository).
  readonly repository?: string | RepositoryInfo | null
   * The electron-builder configuration.
  readonly build?: Configuration
  /** @private */
  readonly dependencies?: Record<string, string>
  readonly version?: string
  readonly type?: string
  readonly shortVersion?: string | null
  readonly shortVersionWindows?: string | null
  readonly productName?: string | null
  readonly main?: string | null
export interface AuthorMetadata {
  readonly email?: string
export interface RepositoryInfo {
  readonly url: string
