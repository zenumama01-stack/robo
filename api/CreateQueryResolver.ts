import { Arg, Ctx, Field, InputType, Mutation, ObjectType, registerEnumType, Resolver, PubSub, PubSubEngine } from 'type-graphql';
import { LogError, RunView, UserInfo, CompositeKey, DatabaseProviderBase, LogStatus } from '@memberjunction/core';
import { MJQueryCategoryEntity, MJQueryPermissionEntity } from '@memberjunction/core-entities';
import { MJQueryResolver } from '../generated/generated.js';
import { GetReadOnlyProvider, GetReadWriteProvider } from '../util.js';
import { DeleteOptionsInput } from '../generic/DeleteOptionsInput.js';
import { QueryEntityExtended } from '@memberjunction/core-entities-server';
 * Query status enumeration for GraphQL
export enum QueryStatus {
    Pending = "Pending",
    Approved = "Approved", 
    Rejected = "Rejected",
    Expired = "Expired"
registerEnumType(QueryStatus, {
    name: "QueryStatus",
    description: "Status of a query: Pending, Approved, Rejected, or Expired"
export class QueryPermissionInputType {
    RoleID!: string;
export class CreateQuerySystemUserInput {
    @Field(() => QueryStatus, { nullable: true, defaultValue: QueryStatus.Pending })
    Status?: QueryStatus;
    @Field(() => Number, { nullable: true })
    @Field(() => [QueryPermissionInputType], { nullable: true })
    Permissions?: QueryPermissionInputType[];
export class UpdateQuerySystemUserInput {
    @Field(() => QueryStatus, { nullable: true })
export class QueryFieldType {
    QueryID!: string;
    @Field(() => Number)
    Sequence!: number;
    IsComputed!: boolean;
export class QueryParameterType {
    IsRequired!: boolean;
export class MJQueryEntityType {
    EntityID!: string;
export class QueryPermissionType {
export class CreateQueryResultType {
    Success!: boolean;
    // Core query properties
    // Related collections
    @Field(() => [QueryFieldType], { nullable: true })
    Fields?: QueryFieldType[];
    @Field(() => [QueryParameterType], { nullable: true })
    Parameters?: QueryParameterType[];
    @Field(() => [MJQueryEntityType], { nullable: true })
    Entities?: MJQueryEntityType[];
    @Field(() => [QueryPermissionType], { nullable: true })
    Permissions?: QueryPermissionType[];
export class UpdateQueryResultType {
export class DeleteQueryResultType {
    // Core query properties of deleted query
export class MJQueryResolverExtended extends MJQueryResolver {
     * Creates a new query with the provided attributes. This mutation is restricted to system users only.
     * @param input - CreateQuerySystemUserInput containing all the query attributes
     * @param context - Application context containing user information
     * @returns CreateQueryResultType with success status and query data
    @Mutation(() => CreateQueryResultType)
    async CreateQuerySystemUser(
        @Arg('input', () => CreateQuerySystemUserInput) input: CreateQuerySystemUserInput,
        @Ctx() context: AppContext,
    ): Promise<CreateQueryResultType> {
            // Handle CategoryPath if provided
            let finalCategoryID = input.CategoryID;
            const provider = GetReadWriteProvider(context.providers);
            if (input.CategoryPath) {
                finalCategoryID = await this.findOrCreateCategoryPath(input.CategoryPath, provider, context.userPayload.userRecord);
            // Check for existing query with same name in the same category
            const existingQuery = await this.findExistingQuery(provider, input.Name, finalCategoryID, context.userPayload.userRecord);
            if (existingQuery) {
                const categoryInfo = input.CategoryPath ? `category path '${input.CategoryPath}'` : `category ID '${finalCategoryID}'`;
                    ErrorMessage: `Query with name '${input.Name}' already exists in ${categoryInfo}`
            // Use QueryEntityExtended which handles AI processing
            const record = await provider.GetEntityObject<QueryEntityExtended>("MJ: Queries", context.userPayload.userRecord);
            // Set the fields from input, handling CategoryPath resolution
            const fieldsToSet = {
                ...input,
                CategoryID: finalCategoryID || input.CategoryID,
                Status: input.Status || 'Approved',
                QualityRank: input.QualityRank || 0,
                UsesTemplate: input.UsesTemplate || false,
                AuditQueryRuns: input.AuditQueryRuns || false,
                CacheEnabled: input.CacheEnabled || false,
                CacheTTLMinutes: input.CacheTTLMinutes || null,
                CacheMaxSize: input.CacheMaxSize || null
            // Remove non-database fields that we handle separately or are input-only
            delete (fieldsToSet as any).Permissions;    // Handled separately via createPermissions
            delete (fieldsToSet as any).CategoryPath;   // Input-only field, resolved to CategoryID
            record.SetMany(fieldsToSet, true);
            this.ListenForEntityMessages(record, pubSub, context.userPayload.userRecord);
            // Attempt to save the query
                // Save succeeded - fire the AfterCreate event and return all the data
                const queryID = record.ID;
                if (input.Permissions && input.Permissions.length > 0) {
                    await this.createPermissions(provider, input.Permissions, queryID, context.userPayload.userRecord);
                    await record.RefreshRelatedMetadata(true); // force DB update since we just created new permissions
                // Refresh metadata cache to include the newly created query
                // This ensures subsequent operations can find the query without additional DB calls
                    ID: record.ID,
                    Name: record.Name,
                    Description: record.Description,
                    CategoryID: record.CategoryID,
                    Category: record.Category,
                    SQL: record.SQL,
                    Status: record.Status,
                    QualityRank: record.QualityRank,
                    EmbeddingVector: record.EmbeddingVector,
                    EmbeddingModelID: record.EmbeddingModelID,
                    EmbeddingModelName: record.EmbeddingModel,
                    Fields: record.QueryFields.map(f => ({
                    Parameters: record.QueryParameters.map(p => ({
                    Entities: record.QueryEntities.map(e => ({
                    Permissions: record.QueryPermissions.map(p => ({
                        Role: p.Role
                // Save failed - check if another request created the same query (race condition)
                // Always recheck regardless of error type to handle all duplicate scenarios
                    // Found the query that was created by another request
                    // Return it as if we created it (it has the same name/category)
                    LogStatus(`[CreateQuery] Unique constraint detected for query '${input.Name}'. Using existing query (ID: ${existingQuery.ID}) created by concurrent request.`);
                        ID: existingQuery.ID,
                        Name: existingQuery.Name,
                        Description: existingQuery.Description,
                        CategoryID: existingQuery.CategoryID,
                        Category: existingQuery.Category,
                        SQL: existingQuery.SQL,
                        Status: existingQuery.Status,
                        QualityRank: existingQuery.QualityRank,
                        EmbeddingVector: existingQuery.EmbeddingVector,
                        EmbeddingModelID: existingQuery.EmbeddingModelID,
                        EmbeddingModelName: existingQuery.EmbeddingModel,
                        Fields: existingQuery.Fields?.map((f: any) => ({
                        Parameters: existingQuery.Parameters?.map((p: any) => ({
                        Entities: existingQuery.Entities?.map((e: any) => ({
                        Permissions: existingQuery.Permissions?.map((p: any) => ({
                // Genuine failure - couldn't find an existing query with the same name
                const errorMessage = record.LatestResult?.Message || '';
                    ErrorMessage: `Failed to create query: ${errorMessage || 'Unknown error'}`
                ErrorMessage: `MJQueryResolverExtended::CreateQuerySystemUser --- Error creating query: ${err instanceof Error ? err.message : String(err)}`
    protected async createPermissions(p: DatabaseProviderBase, permissions: QueryPermissionInputType[], queryID: string, contextUser: UserInfo): Promise<QueryPermissionType[]> {
        // Create permissions if provided
        const createdPermissions: QueryPermissionType[] = [];
        if (permissions && permissions.length > 0) {
                const permissionEntity = await p.GetEntityObject<MJQueryPermissionEntity>('MJ: Query Permissions', contextUser);
                if (permissionEntity) {
                    permissionEntity.QueryID = queryID;
                    permissionEntity.RoleID = perm.RoleID;
                    const saveResult = await permissionEntity.Save();
                        createdPermissions.push({
                            ID: permissionEntity.ID,
                            QueryID: permissionEntity.QueryID,
                            RoleID: permissionEntity.RoleID,
                            Role: permissionEntity.Role
        return createdPermissions;
     * Updates an existing query with the provided attributes. This mutation is restricted to system users only.
     * @param input - UpdateQuerySystemUserInput containing the query ID and fields to update
     * @returns UpdateQueryResultType with success status and updated query data including related entities
    @Mutation(() => UpdateQueryResultType)
    async UpdateQuerySystemUser(
        @Arg('input', () => UpdateQuerySystemUserInput) input: UpdateQuerySystemUserInput,
    ): Promise<UpdateQueryResultType> {
            // Load the existing query using QueryEntityExtended
            const queryEntity = await provider.GetEntityObject<QueryEntityExtended>('MJ: Queries', context.userPayload.userRecord);
            if (!queryEntity || !await queryEntity.Load(input.ID)) {
                    ErrorMessage: `Query with ID ${input.ID} not found`
            // now make sure there is NO existing query by the same name in the specified category
            const existingQueryResult = await provider.RunView({
                ExtraFilter: `Name='${input.Name}' AND CategoryID='${finalCategoryID}'` 
            }, context.userPayload.userRecord);
            if (existingQueryResult.Success && existingQueryResult.Results?.length > 0) {
                // we have a match! Let's return an error
                    ErrorMessage: `Query with name '${input.Name}' already exists in the specified ${input.CategoryID ? 'category' : 'categoryPath'}`
            // Update fields that were provided
            const updateFields: Record<string, any> = {};
            if (input.Name !== undefined) updateFields.Name = input.Name;
            if (finalCategoryID !== undefined) updateFields.CategoryID = finalCategoryID;
            if (input.UserQuestion !== undefined) updateFields.UserQuestion = input.UserQuestion;
            if (input.Description !== undefined) updateFields.Description = input.Description;
            if (input.SQL !== undefined) updateFields.SQL = input.SQL;
            if (input.TechnicalDescription !== undefined) updateFields.TechnicalDescription = input.TechnicalDescription;
            if (input.OriginalSQL !== undefined) updateFields.OriginalSQL = input.OriginalSQL;
            if (input.Feedback !== undefined) updateFields.Feedback = input.Feedback;
            if (input.Status !== undefined) updateFields.Status = input.Status;
            if (input.QualityRank !== undefined) updateFields.QualityRank = input.QualityRank;
            if (input.ExecutionCostRank !== undefined) updateFields.ExecutionCostRank = input.ExecutionCostRank;
            if (input.UsesTemplate !== undefined) updateFields.UsesTemplate = input.UsesTemplate;
            if (input.AuditQueryRuns !== undefined) updateFields.AuditQueryRuns = input.AuditQueryRuns;
            if (input.CacheEnabled !== undefined) updateFields.CacheEnabled = input.CacheEnabled;
            if (input.CacheTTLMinutes !== undefined) updateFields.CacheTTLMinutes = input.CacheTTLMinutes;
            if (input.CacheMaxSize !== undefined) updateFields.CacheMaxSize = input.CacheMaxSize;
            // Use SetMany to update all fields at once
            queryEntity.SetMany(updateFields);
            // Save the updated query
            const saveResult = await queryEntity.Save();
                    ErrorMessage: `Failed to update query: ${queryEntity.LatestResult?.Message || 'Unknown error'}`
            const queryID = queryEntity.ID;
            // Handle permissions update if provided
            if (input.Permissions !== undefined) {
                // Delete existing permissions
                const existingPermissions = await rv.RunView<MJQueryPermissionEntity>({
                    ExtraFilter: `QueryID='${queryID}'`,
                if (existingPermissions.Success && existingPermissions.Results) {
                    for (const perm of existingPermissions.Results) {
                        await perm.Delete();
                // Create new permissions
                // Refresh the metadata to get updated permissions
                await queryEntity.RefreshRelatedMetadata(true);
                ID: queryEntity.ID,
                Name: queryEntity.Name,
                Description: queryEntity.Description,
                CategoryID: queryEntity.CategoryID,
                Category: queryEntity.Category,
                SQL: queryEntity.SQL,
                Status: queryEntity.Status,
                QualityRank: queryEntity.QualityRank,
                EmbeddingVector: queryEntity.EmbeddingVector,
                EmbeddingModelID: queryEntity.EmbeddingModelID,
                EmbeddingModelName: queryEntity.EmbeddingModel,
                Fields: queryEntity.QueryFields.map(f => ({
                Parameters: queryEntity.QueryParameters.map(p => ({
                Entities: queryEntity.QueryEntities.map(e => ({
                Permissions: queryEntity.QueryPermissions.map(p => ({
                ErrorMessage: `MJQueryResolverExtended::UpdateQuerySystemUser --- Error updating query: ${err instanceof Error ? err.message : String(err)}`
     * Deletes a query by ID. This mutation is restricted to system users only.
     * @param options - Delete options controlling action execution
     * @returns DeleteQueryResultType with success status and deleted query data
    @Mutation(() => DeleteQueryResultType)
    async DeleteQuerySystemResolver(
        @Arg('ID', () => String) ID: string,
        @Arg('options', () => DeleteOptionsInput, { nullable: true }) options: DeleteOptionsInput | null,
    ): Promise<DeleteQueryResultType> {
                    ErrorMessage: 'MJQueryResolverExtended::DeleteQuerySystemResolver --- Invalid query ID: ID cannot be null or empty'
            // Provide default options if none provided
            const deleteOptions = options || {
                SkipEntityAIActions: false,
                SkipEntityActions: false,
                ReplayOnly: false,
                IsParentEntityDelete: false
            // Use inherited DeleteRecord method from ResolverBase
            const deletedQuery = await this.DeleteRecord('MJ: Queries', key, deleteOptions, provider, context.userPayload, pubSub);
            if (deletedQuery) {
                    ID: deletedQuery.ID,
                    Name: deletedQuery.Name,
                    Description: deletedQuery.Description,
                    CategoryID: deletedQuery.CategoryID,
                    SQL: deletedQuery.SQL,
                    Status: deletedQuery.Status
                    ErrorMessage: 'Failed to delete query using DeleteRecord method'
                ErrorMessage: `MJQueryResolverExtended::DeleteQuerySystemResolver --- Error deleting query: ${err instanceof Error ? err.message : String(err)}`
     * Finds or creates a category hierarchy based on the provided path.
     * Path format: "Parent/Child/Grandchild" - case insensitive lookup and creation.
     * @param categoryPath - Slash-separated category path
     * @param md - Metadata instance
     * @returns The ID of the final category in the path
    private async findOrCreateCategoryPath(categoryPath: string, p: DatabaseProviderBase, contextUser: UserInfo): Promise<string> {
        if (!categoryPath || categoryPath.trim() === '') {
            throw new Error('CategoryPath cannot be empty');
        const pathParts = categoryPath.split('/').map(part => part.trim()).filter(part => part.length > 0);
        if (pathParts.length === 0) {
            throw new Error('CategoryPath must contain at least one valid category name');
        let currentParentID: string | null = null;
        let currentCategoryID: string | null = null;
        for (let i = 0; i < pathParts.length; i++) {
            const categoryName = pathParts[i];
            // Look for existing category at this level
            const existingCategory = await this.findCategoryByNameAndParent(p, categoryName, currentParentID, contextUser);
                currentCategoryID = existingCategory.ID;
                currentParentID = existingCategory.ID;
                    const newCategory = await p.GetEntityObject<MJQueryCategoryEntity>("MJ: Query Categories", contextUser);
                    if (!newCategory) {
                        throw new Error(`Failed to create entity object for Query Categories`);
                    newCategory.Name = categoryName;
                    newCategory.ParentID = currentParentID;
                    newCategory.UserID = contextUser.ID;
                    newCategory.Description = `Auto-created category from path: ${categoryPath}`;
                    const saveResult = await newCategory.Save();
                        // Save failed - always recheck if another request created the same category
                        const recheckExisting = await this.findCategoryByNameAndParent(p, categoryName, currentParentID, contextUser);
                        if (recheckExisting) {
                            // Another request created it - use that one
                            LogStatus(`[CreateQuery] Unique constraint detected for category '${categoryName}'. Using existing category (ID: ${recheckExisting.ID}) created by concurrent request.`);
                            currentCategoryID = recheckExisting.ID;
                            currentParentID = recheckExisting.ID;
                            // Genuine failure (not a duplicate)
                            const errorMessage = newCategory.LatestResult?.Message || '';
                            throw new Error(`Failed to create category '${categoryName}': ${errorMessage || 'Unknown error'}`);
                        currentCategoryID = newCategory.ID;
                        currentParentID = newCategory.ID;
                    // On error, double-check if category exists (race condition handling)
                        // Category exists, another request created it
                        LogStatus(`[CreateQuery] Exception during category creation for '${categoryName}'. Using existing category (ID: ${recheckExisting.ID}) created by concurrent request.`);
                        throw new Error(`Failed to create category '${categoryName}': ${error instanceof Error ? error.message : String(error)}`);
        if (!currentCategoryID) {
            throw new Error('Failed to determine final category ID');
        return currentCategoryID;
     * Finds an existing query by name and category ID using RunView.
     * Bypasses metadata cache to ensure we get the latest data from database.
     * @param provider - Database provider
     * @param queryName - Name of the query to find
     * @param categoryID - Category ID (can be null)
     * @returns The matching query info or null if not found
    private async findExistingQuery(
        categoryID: string | null,
    ): Promise<any | null> {
            // Query database directly to avoid cache staleness issues
            const categoryFilter = categoryID ? `CategoryID='${categoryID}'` : 'CategoryID IS NULL';
            const nameFilter = `LOWER(Name) = LOWER('${queryName.replace(/'/g, "''")}')`;
            const result = await provider.RunView({
                ExtraFilter: `${nameFilter} AND ${categoryFilter}`
            // If query fails, return null (query doesn't exist)
     * Finds a category by name and parent ID using RunView.
     * @param categoryName - Name of the category to find
     * @param parentID - Parent category ID (null for root level)
     * @returns The matching category entity or null if not found
    private async findCategoryByNameAndParent(provider: DatabaseProviderBase, categoryName: string, parentID: string | null, contextUser: UserInfo): Promise<MJQueryCategoryEntity | null> {
            const parentFilter = parentID ? `ParentID='${parentID}'` : 'ParentID IS NULL';
            const nameFilter = `LOWER(Name) = LOWER('${categoryName.replace(/'/g, "''")}')`; // Escape single quotes
            const result = await provider.RunView<MJQueryCategoryEntity>({
                ExtraFilter: `${nameFilter} AND ${parentFilter}`,
            // If query fails, return null
