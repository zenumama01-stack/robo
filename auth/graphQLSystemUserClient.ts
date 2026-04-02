import { CompositeKey, LogError, KeyValuePair, IsVerboseLoggingEnabled } from '@memberjunction/core';
import { ActionItemInput, RolesAndUsersInput, SyncDataResult, SyncRolesAndUsersResult } from './rolesAndUsersType';
    RunAIPromptParams, 
    RunAIPromptResult, 
    ExecuteSimplePromptParams,
    SimplePromptResult,
    EmbedTextResult
} from './graphQLAIClient';
import { ExecuteAgentParams, ExecuteAgentResult } from '@memberjunction/ai-core-plus';
 * Specialized client that is designed to be used exclusively on the server side
 * by the System User using the MJ API KEY. This is designed to allow server side
 * code such as within the context of an MJAPI server to talk to another MAJPI server
 * but as the client.
 * It is possible for a server to use the regular @GraphQLDataProvider client to talk
 * to another MJAPI server, but that would require the server to have a user account
 * and the server would have to be able to log in as that user via a JWT token. This
 * is not always possible or convenient in the flow. 
 * Also the standard client configuration process has a certain amount over overhead
 * in always loading up certain metadata that is not necessary for many system user
 * operations.
 * Finaly, this client can be used in parallel with the regular client to allow a server
 * to mix and match the two as needed.
export class GraphQLSystemUserClient {
     * Returns the underlying GraphQL client which is an instance of the GraphQLClient object
     * from the graphql-request package. This is useful if you want to perform any operation
     * that is not directly supported by this class via specialized methods.
    public get Client(): GraphQLClient {
     * @param url MJAPI server URL
     * @param token Optional, JWT token that is used for a normal user authentication flow. This is required if mjAPIKey is not provided.
     * @param sessionId Optional, UUID that is used to track a session. This can be any string.
     * @param mjAPIKey Shared Secret key that is provided for system user authentication. This is required if token is not provided.
    constructor (url: string, token: string, sessionId: string, mjAPIKey: string) {
        this._sessionId = sessionId;
        this._client = new GraphQLClient(url, {
     * Calls the GetData() query on the server to execute any number of provided SQL queries in parallel and returns the results.
     * The queries MUST BE read only and not perform any DML operations. The remote server will execute the queries using a special
     * lower-privilege user that is not allowed to perform any form of write operations.
     * @param queries an array of SQL queries to execute on the remote server
     * @param accessToken the short-lived access token that is required to perform this operation. This is different from the MJAPI key and is used for a second factor and is a short-lived token. You will receive this token 
     * when an MJAPI calls your server to request something along with the URL to call back.
    public async GetData(queries: string[], accessToken: string): Promise<GetDataOutput> {
            const query = `query GetData($input: GetDataInputType!) {
                GetData(input: $input) {
            const result = await this.Client.request(query, {input: {Queries: queries, Token: accessToken}}) as {GetData: GetDataOutput};
            if (result && result.GetData) {
                // for each succesful item, we will parse and return the array of objects instead of the string
                    Success: result.GetData.Success,
                    Results: result.GetData.Results.map(r => r ? SafeJSONParse(r) : null),
                    ErrorMessages: result.GetData.ErrorMessages,
                    Queries: result.GetData.Queries 
                    ErrorMessages: result.GetData?.ErrorMessages ?? ['Unknown error'],
                    Queries: result.GetData?.Queries ?? queries
            // Extract clean error message - the GraphQL error response contains the actual SQL error
            let cleanError = e instanceof Error ? e.message : String(e);
            // Try to extract just the SQL error from GraphQL response
            // Look for the actual error message before the JSON payload
            const match = cleanError.match(/Error: ([^:]+)\./);
                cleanError = match[1] + '.';
            // Only log verbose details if in verbose mode
                const verboseError = `GraphQLSystemUserClient::GetData - Error getting data - ${e}`;
                LogError(verboseError);
                ErrorMessages: [cleanError],
                Queries: queries
     * This method will return a list of all entities that are available on the remote server. This is a lightweight call that only returns the basic metadata for each entity and does not include all of the attributes at 
     * either the entity or the field level. This is useful for getting a list of entities that are available on the remote server and knowing their IDs and Entity Field IDs on the remote server. For core MemberJunction 
     * entities and entity fields, the ID values are globally unique and set by the MemberJunction distribution, however, for other entities that are generated on each target system they can vary so it is best to use
     * lookups name or a combination of SchemaName and BaseTable to uniquely identify an entity.
     * @param client 
    public async GetAllRemoteEntities(): Promise<SimpleRemoteEntityOutput> {
            const query = `query GetAllEntities {
                GetAllEntities {
                        SchemaName
                        BaseView
                        BaseTable
                        CodeName
                        ClassName
                        Fields {
                            AllowsNull
                            MaxLength
            const result = (await this.Client.request(query)) as {GetAllEntities: SimpleRemoteEntityOutput};
            if (result && result.GetAllEntities) {
                return result.GetAllEntities;
                    ErrorMessage: result.GetAllEntities?.ErrorMessage ?? 'Unknown error'
            LogError(`GraphQLSystemUserClient::GetAllRemoteEntities - Error getting remote entities - ${e}`);
                ErrorMessage: e
     * This method is used to invoke the data synchronization mutation on the remote server. This method is used to create, update, or delete records in the remote server. The method takes an array of ActionItemInput objects
     * Each of the ActionItemInput objects represents a single action to take on a single entity. The action can be Create, Update, CreateOrUpdate, Delete, or DeleteWithFilter. The RecordJSON field is used for Create, CreateOrUpdate and Update whereas
     * the DeleteFilter field is used for DeleteWithFilter. The PrimaryKey and AlternateKey fields are used to identify the record to be acted upon. 
     * Use of the AlternateKey is important for situations where you might have divergent primary keys across systems. For example for user entities that are individually generated on each system by CodeGen, the primary key will
     * be different per system, so you would use the combination of the SchemaName and BaseTable to identify the entity and then use the AlternateKey to provide the combination of these fields to uniquely identify the record for updates.
     * @param items 
    public async SyncData(items: ActionItemInput[]): Promise<SyncDataResult> {
            // call the resolver to sync the roles and users
            const query = `mutation SyncData($items: [ActionItemInputType!]!) {
                SyncData(items: $items) {
                        AlternateKey {
                        DeleteFilter
                        RecordJSON
            const d = <{SyncData: SyncDataResult}>await this.Client.request(query, {items});
            if (d && d.SyncData) {
                return d.SyncData;
            LogError(`GraphQLSystemUserClient::SyncData - Error syncing data - ${e}`);
     * This method will sync the roles and users on the remote server. This method is used to create, update, or delete roles and users on the remote server. 
     * The method takes a RolesAndUsersInput object that contains an array of RoleInput objects. Note that this will not result in the removal of roles on the 
     * remote server that are deemed to be built-in MemberJunction roles such as Developer, Integration and UI.
    public async SyncRolesAndUsers(data: RolesAndUsersInput): Promise<SyncRolesAndUsersResult> {
            const query = `mutation SyncRolesAndUsers($data: RolesAndUsersInputType!) {
                SyncRolesAndUsers(data: $data) {
            const d = await this.Client.request(query, {data}) as {SyncRolesAndUsers: SyncRolesAndUsersResult};
            if (d && d.SyncRolesAndUsers) {
                return d.SyncRolesAndUsers;
            LogError(`GraphQLSystemUserClient::SyncRolesAndUsers - Error syncing roles and users - ${e}`);
     * Runs a view by name using the RunViewByNameSystemUser resolver.
     * @param input - View input parameters for running by name
     * @returns Promise containing the view execution results
    public async RunViewByName(input: RunViewByNameSystemUserInput): Promise<RunViewSystemUserResult> {
            const query = `query RunViewByNameSystemUser($input: RunViewByNameInput!) {
                RunViewByNameSystemUser(input: $input) {
            const result = await this.Client.request(query, { input }) as { RunViewByNameSystemUser: RunViewSystemUserResult };
            if (result && result.RunViewByNameSystemUser) {
                return result.RunViewByNameSystemUser;
                    ErrorMessage: 'Failed to execute view by name'
            LogError(`GraphQLSystemUserClient::RunViewByNameSystemUser - Error running view by name - ${e}`);
                ErrorMessage: e.toString()
     * Runs a view by ID using the RunViewByIDSystemUser resolver.
     * @param input - View input parameters for running by ID
    public async RunViewByID(input: RunViewByIDSystemUserInput): Promise<RunViewSystemUserResult> {
            const query = `query RunViewByIDSystemUser($input: RunViewByIDInput!) {
                RunViewByIDSystemUser(input: $input) {
            const result = await this.Client.request(query, { input }) as { RunViewByIDSystemUser: RunViewSystemUserResult };
            if (result && result.RunViewByIDSystemUser) {
                return result.RunViewByIDSystemUser;
                    ErrorMessage: 'Failed to execute view by ID'
            LogError(`GraphQLSystemUserClient::RunViewByIDSystemUser - Error running view by ID - ${e}`);
     * Runs a dynamic view using the RunDynamicViewSystemUser resolver.
     * @param input - View input parameters for dynamic view execution
    public async RunDynamicView(input: RunDynamicViewSystemUserInput): Promise<RunViewSystemUserResult> {
            const query = `query RunDynamicViewSystemUser($input: RunDynamicViewInput!) {
                RunDynamicViewSystemUser(input: $input) {
            const result = await this.Client.request(query, { input }) as { RunDynamicViewSystemUser: RunViewSystemUserResult };
            if (result && result.RunDynamicViewSystemUser) {
                return result.RunDynamicViewSystemUser;
                    ErrorMessage: 'Failed to execute dynamic view'
            LogError(`GraphQLSystemUserClient::RunDynamicViewSystemUser - Error running dynamic view - ${e}`);
     * Runs multiple views using the RunViewsSystemUser resolver. This method allows system users
     * to execute view queries with the same functionality as regular users but with system-level privileges.
     * @param input - Array of view input parameters
     * @returns Promise containing the results from all view executions
    public async RunViews(input: RunViewSystemUserInput[]): Promise<RunViewSystemUserResult[]> {
            const query = `query RunViewsSystemUser($input: [RunViewGenericInput!]!) {
                RunViewsSystemUser(input: $input) {
            const result = await this.Client.request(query, { input }) as { RunViewsSystemUser: RunViewSystemUserResult[] };
            if (result && result.RunViewsSystemUser) {
                return result.RunViewsSystemUser;
            LogError(`GraphQLSystemUserClient::RunViewsSystemUser - Error running views - ${e}`);
     * Executes a stored query by ID using the GetQueryDataSystemUser resolver.
     * @param input - Query input parameters for execution by ID
     * @returns Promise containing the query execution results
    public async GetQueryData(input: GetQueryDataSystemUserInput): Promise<RunQuerySystemUserResult> {
            // Validate that Parameters is a JSON object, not an array
            if (input.Parameters !== undefined && Array.isArray(input.Parameters)) {
                throw new Error('Parameters must be a JSON object, not an array. Use {} for empty parameters instead of [].');
            const query = `query GetQueryDataSystemUser($QueryID: String!, $CategoryID: String, $CategoryPath: String, $Parameters: JSONObject, $MaxRows: Int, $StartRow: Int) {
                GetQueryDataSystemUser(QueryID: $QueryID, CategoryID: $CategoryID, CategoryPath: $CategoryPath, Parameters: $Parameters, MaxRows: $MaxRows, StartRow: $StartRow) {
            const variables: any = { QueryID: input.QueryID };
            if (input.CategoryID !== undefined) variables.CategoryID = input.CategoryID;
            if (input.CategoryPath !== undefined) variables.CategoryPath = input.CategoryPath;
            if (input.Parameters !== undefined) variables.Parameters = input.Parameters;
            if (input.MaxRows !== undefined) variables.MaxRows = input.MaxRows;
            if (input.StartRow !== undefined) variables.StartRow = input.StartRow;
            const result = await this.Client.request(query, variables) as { GetQueryDataSystemUser: RunQuerySystemUserResult };
            if (result && result.GetQueryDataSystemUser) {
                // Parse the JSON results for easier consumption
                    ...result.GetQueryDataSystemUser,
                    Results: result.GetQueryDataSystemUser.Results ? SafeJSONParse(result.GetQueryDataSystemUser.Results) : null
                    QueryID: input.QueryID,
                    QueryName: '',
                    Results: null,
                    ExecutionTime: 0,
                    ErrorMessage: 'Query execution failed'
            LogError(`GraphQLSystemUserClient::GetQueryDataSystemUser - Error executing query - ${e}`);
     * Executes a stored query by name using the GetQueryDataByNameSystemUser resolver.
     * @param input - Query input parameters for execution by name
    public async GetQueryDataByName(input: GetQueryDataByNameSystemUserInput): Promise<RunQuerySystemUserResult> {
            const query = `query GetQueryDataByNameSystemUser($QueryName: String!, $CategoryID: String, $CategoryPath: String, $Parameters: JSONObject, $MaxRows: Int, $StartRow: Int) {
                GetQueryDataByNameSystemUser(QueryName: $QueryName, CategoryID: $CategoryID, CategoryPath: $CategoryPath, Parameters: $Parameters, MaxRows: $MaxRows, StartRow: $StartRow) {
            const variables: any = { QueryName: input.QueryName };
            const result = await this.Client.request(query, variables) as { GetQueryDataByNameSystemUser: RunQuerySystemUserResult };
            if (result && result.GetQueryDataByNameSystemUser) {
                    ...result.GetQueryDataByNameSystemUser,
                    Results: result.GetQueryDataByNameSystemUser.Results ? SafeJSONParse(result.GetQueryDataByNameSystemUser.Results) : null
                    QueryID: '',
                    QueryName: input.QueryName,
            LogError(`GraphQLSystemUserClient::GetQueryDataByNameSystemUser - Error executing query - ${e}`);
     * Creates a new query using the CreateQuerySystemUser mutation. This method is restricted to system users only.
     * @param input - CreateQuerySystemUserInput containing all the query attributes including optional CategoryPath
     * @returns Promise containing the result of the query creation
    public async CreateQuery(input: CreateQueryInput): Promise<CreateQueryResult> {
            const query = `mutation CreateQuerySystemUser($input: CreateQuerySystemUserInput!) {
                CreateQuerySystemUser(input: $input) {
                    QualityRank
                    EmbeddingVector
                    EmbeddingModelID
                    EmbeddingModelName
                        Sequence
                        SQLBaseType
                        SQLFullType
                        SourceEntityID
                        SourceEntity
                        SourceFieldName
                        IsComputed
                        ComputationDescription
                        IsSummary
                        SummaryDescription
                    Parameters {
                        IsRequired
                        SampleValue
                        ValidationFilters
                    Entities {
                        Entity
                    Permissions {
                        RoleID
                        Role
            const result = await this.Client.request(query, { input }) as { CreateQuerySystemUser: CreateQueryResult };
            if (result && result.CreateQuerySystemUser) {
                return result.CreateQuerySystemUser;
                    ErrorMessage: 'Failed to create query'
            LogError(`GraphQLSystemUserClient::CreateQuery - Error creating query - ${e}`);
     * Updates an existing query with the provided attributes. This method is restricted to system users only.
     * @param input - UpdateQueryInput containing the query ID and fields to update
     * @returns Promise containing the result of the query update including updated fields, parameters, entities, and permissions
    public async UpdateQuery(input: UpdateQueryInput): Promise<UpdateQueryResult> {
            const query = `mutation UpdateQuerySystemUser($input: UpdateQuerySystemUserInput!) {
                UpdateQuerySystemUser(input: $input) {
            const result = await this.Client.request(query, { input }) as { UpdateQuerySystemUser: UpdateQueryResult };
            if (result && result.UpdateQuerySystemUser) {
                return result.UpdateQuerySystemUser;
                    ErrorMessage: 'Failed to update query'
            LogError(`GraphQLSystemUserClient::UpdateQuery - Error updating query - ${e}`);
     * Deletes a query by ID using the DeleteQuerySystemResolver mutation. This method is restricted to system users only.
     * @param ID - The ID of the query to delete
     * @param options - Optional delete options controlling action execution
     * @returns Promise containing the result of the query deletion
    public async DeleteQuery(ID: string, options?: DeleteQueryOptionsInput): Promise<DeleteQueryResult> {
            // Validate ID is not null/undefined/empty
            if (!ID || ID.trim() === '') {
                LogError('GraphQLSystemUserClient::DeleteQuery - Invalid query ID: ID cannot be null or empty');
                    ErrorMessage: 'Invalid query ID: ID cannot be null or empty'
            const query = `mutation DeleteQuerySystemResolver($ID: String!, $options: DeleteOptionsInput) {
                DeleteQuerySystemResolver(ID: $ID, options: $options) {
            const variables: Record<string, unknown> = { ID: ID };
            if (options !== undefined) {
                // Apply defaults for all required fields in DeleteOptionsInput
                // The server requires all fields to be present
                variables.options = {
                    SkipEntityAIActions: options.SkipEntityAIActions ?? false,
                    SkipEntityActions: options.SkipEntityActions ?? false,
                    ReplayOnly: options.ReplayOnly ?? false,
                    IsParentEntityDelete: options.IsParentEntityDelete ?? false
            const result = await this.Client.request(query, variables) as { DeleteQuerySystemResolver: DeleteQueryResult };
            if (result && result.DeleteQuerySystemResolver) {
                return result.DeleteQuerySystemResolver;
                    ErrorMessage: 'Failed to delete query'
        catch (e: unknown) {
            // Extract detailed error information for debugging
            let errorDetails = '';
            if (e instanceof Error) {
                errorDetails = e.message;
                // Check for cause (common in fetch errors)
                if ('cause' in e && e.cause) {
                    const cause = e.cause as Error;
                    errorDetails += ` | Cause: ${cause.message || cause}`;
                    if ('code' in cause) {
                        errorDetails += ` | Code: ${(cause as NodeJS.ErrnoException).code}`;
                // Check for response details (GraphQL client errors)
                if ('response' in e) {
                    const response = (e as { response?: { status?: number; errors?: unknown[] } }).response;
                    if (response?.status) {
                        errorDetails += ` | HTTP Status: ${response.status}`;
                    if (response?.errors) {
                        errorDetails += ` | GraphQL Errors: ${JSON.stringify(response.errors)}`;
                // Include stack trace for debugging
                if (e.stack) {
                    console.error('DeleteQuery stack trace:', e.stack);
                errorDetails = String(e);
            LogError(`GraphQLSystemUserClient::DeleteQuery - Error deleting query - ${errorDetails}`);
                ErrorMessage: errorDetails
     * Run an AI prompt with system user privileges.
     * This method allows system-level execution of AI prompts without user authentication.
     * const result = await systemClient.RunAIPrompt({
     *   data: { systemData: "value" },
     *   skipValidation: true
            // Build the query for system user
                query RunAIPromptSystemUser(
                    RunAIPromptSystemUser(
            const result = await this.Client.request(query, variables) as { RunAIPromptSystemUser: any };
            if (result && result.RunAIPromptSystemUser) {
                return this.processPromptResult(result.RunAIPromptSystemUser);
                    error: 'Failed to execute AI prompt as system user'
            LogError(`GraphQLSystemUserClient::RunAIPrompt - Error running AI prompt - ${e}`);
                error: e.toString()
     * Run an AI agent with system user privileges.
     * This method allows system-level execution of AI agents without user authentication.
     * const result = await systemClient.RunAIAgent({
     *   messages: [{ role: "system", content: "Process data" }],
     *   sessionId: "system-session"
    public async RunAIAgent(params: ExecuteAgentParams): Promise<ExecuteAgentResult> {
                query RunAIAgentSystemUser(
                    $configurationId: String
                    RunAIAgentSystemUser(
                        configurationId: $configurationId
            const variables = this.prepareAgentVariables(params);
            const result = await this.Client.request(query, variables) as { RunAIAgentSystemUser: any };
            if (result && result.RunAIAgentSystemUser) {
                return this.processAgentResult(result.RunAIAgentSystemUser.result);
            LogError(`GraphQLSystemUserClient::RunAIAgent - Error running AI agent - ${e}`);
     * Helper method to prepare prompt variables for GraphQL
     * Helper method to prepare agent variables for GraphQL
    private prepareAgentVariables(params: ExecuteAgentParams): Record<string, any> {
            sessionId: this._sessionId
     * Helper method to process prompt results
    private processPromptResult(promptResult: any): RunAIPromptResult {
     * Helper method to process agent results
    private processAgentResult(agentResult: any): ExecuteAgentResult {
        return SafeJSONParse(agentResult) as ExecuteAgentResult;
     * This method allows system-level execution of simple prompts.
     * const result = await systemClient.ExecuteSimplePrompt({
     *   systemPrompt: "You are a data analyst",
                query ExecuteSimplePromptSystemUser(
                    ExecuteSimplePromptSystemUser(
            const result = await this.Client.request(query, variables) as { ExecuteSimplePromptSystemUser: any };
            if (!result?.ExecuteSimplePromptSystemUser) {
                    error: 'Failed to execute simple prompt as system user'
            const promptResult = result.ExecuteSimplePromptSystemUser;
            LogError(`GraphQLSystemUserClient::ExecuteSimplePrompt - Error executing simple prompt - ${e}`);
     * Generate embeddings using local embedding models.
     * This method allows system-level generation of text embeddings.
     * const result = await systemClient.EmbedText({
     *   textToEmbed: ["System data", "Configuration"],
                query EmbedTextSystemUser(
                    EmbedTextSystemUser(
            const result = await this.Client.request(query, variables) as { EmbedTextSystemUser: any };
            if (!result?.EmbedTextSystemUser) {
                    error: 'Failed to generate embeddings as system user'
            const embedResult = result.EmbedTextSystemUser;
            LogError(`GraphQLSystemUserClient::EmbedText - Error generating embeddings - ${e}`);
 * Output type for GetData calls - contains results from executing multiple SQL queries
export class GetDataOutput {
     * Indicates if the operation was successful overall. If any individual query failed, this will be false. However, any successful queries will still be returned in the Results array.
     * The original input of Queries that were run - same order as provided in the request
    Queries: string[];
     * An ordered array of error messages for each query that was run. This array will always have the same # of entries as Queries. If a query was successful, the corresponding entry will be null.
    ErrorMessages: (string | null)[];
     * An ordered array of results for each query that was run. This array will always have the same # of entries as Queries. If a query failed, the corresponding entry will be null. Successful results are JSON strings containing the query data.
    Results: (string | null)[];
 * Return type for calls to the GetAllRemoteEntities query - provides lightweight entity metadata
export class SimpleRemoteEntityOutput {
     * Indicates whether the remote entity retrieval was successful
     * Error message if the operation failed, undefined if successful
     * An array of simple entity types that are returned from the remote server - contains basic metadata for each entity
    Results: SimpleRemoteEntity[];
 * Represents a simple entity type that is used for lightweight retrieval of partial remote entity metadata 
export class SimpleRemoteEntity {
     * Unique identifier of the entity on the remote server
     * Display name of the entity (e.g., "Users", "Companies")
     * Optional description explaining the entity's purpose
     * Database schema name where the entity resides (e.g., "dbo", "custom")
     * Name of the database view used for reading this entity
    BaseView: string;
     * Name of the database table used for storing this entity
     * Optional code-friendly name for the entity (typically PascalCase)
    CodeName?: string;
     * Optional TypeScript/JavaScript class name for the entity
     * Array of field definitions for this entity
    Fields: SimpleRemoteEntityField[];
 * Represents a field within a remote entity - contains basic field metadata
export class SimpleRemoteEntityField {
     * Unique identifier of the entity field on the remote server
     * Field name (e.g., "FirstName", "Email", "CreatedAt")
     * Optional description explaining the field's purpose
     * Data type of the field (e.g., "nvarchar", "int", "datetime", "bit")
     * Whether the field can contain null values
     * Maximum length for string fields, -1 for unlimited, 0 for non-string types
    MaxLength: number;
 * Input type for RunViewByNameSystemUser method calls - executes a saved view by name
export interface RunViewByNameSystemUserInput {
     * Name of the saved view to execute
     * Additional WHERE clause conditions to apply (optional)
     * ORDER BY clause for sorting results (optional)
     * Specific fields to return, if not specified returns all fields (optional)
     * Search string to filter results across searchable fields (optional)
    UserSearchString?: string;
     * ID of a previous view run to exclude results from (optional)
    ExcludeUserViewRunID?: string;
     * Override the exclude filter with custom logic (optional)
    OverrideExcludeFilter?: string;
     * Whether to save the view execution results for future reference (optional)
    SaveViewResults?: boolean;
     * Whether to exclude data from all prior view runs (optional)
    ExcludeDataFromAllPriorViewRuns?: boolean;
     * Whether to ignore the view's MaxRows setting and return all results (optional)
    IgnoreMaxRows?: boolean;
     * Maximum number of rows to return, overrides view setting if specified (optional)
     * Whether to force audit logging for this view execution (optional)
    ForceAuditLog?: boolean;
     * Description for the audit log entry if ForceAuditLog is true (optional)
    AuditLogDescription?: string;
     * Type of result format: "simple", "entity_object", etc. (optional)
     * Starting row number for pagination (optional, 0-based)
    StartRow?: number;
 * Input type for RunViewByIDSystemUser method calls - executes a saved view by its unique ID
export interface RunViewByIDSystemUserInput {
     * Unique identifier of the saved view to execute
    ViewID: string;
 * Input type for RunDynamicViewSystemUser method calls - creates and executes a view dynamically based on entity
export interface RunDynamicViewSystemUserInput {
     * Name of the entity to query (e.g., "Users", "Companies")
     * Whether to ignore MaxRows limits and return all results (optional)
     * Maximum number of rows to return (optional)
 * Input type for RunViewsSystemUser method calls - executes multiple views in parallel
export interface RunViewSystemUserInput {
 * Result row type for view execution results - represents a single data row
export interface RunViewSystemUserResultRow {
     * Primary key fields and values for the record
    PrimaryKey: KeyValuePair[];
     * ID of the entity type this record belongs to
     * JSON string containing the actual record data
    Data: string;
 * Result type for RunViewsSystemUser method calls - contains execution results and metadata
export interface RunViewSystemUserResult {
     * Array of result rows containing the actual data
    Results: RunViewSystemUserResultRow[];
     * Unique identifier for this view execution run (optional)
     * Number of rows returned in this result set (optional)
     * Total number of rows available (before pagination) (optional)
    TotalRowCount?: number;
     * Time taken to execute the view in milliseconds (optional)
    ExecutionTime?: number;
     * Error message if the execution failed (optional)
     * Whether the view execution was successful
 * Result type for query execution methods - contains query results and execution metadata
export interface RunQuerySystemUserResult {
     * Unique identifier of the executed query
     * Display name of the executed query
     * Whether the query execution was successful
     * Query results data (parsed from JSON)
    Results: any;
     * Number of rows returned by the query
     * Total number of rows available (before pagination)
     * Time taken to execute the query in milliseconds
     * Error message if the query execution failed
     * JSON string containing the applied parameters (optional)
    AppliedParameters?: string;
 * Input type for GetQueryDataSystemUser method calls - executes a stored query by ID
export interface GetQueryDataSystemUserInput {
     * The ID of the query to execute
     * Optional category ID filter
    CategoryID?: string;
     * Optional category path filter (hierarchical path like "/MJ/AI/Agents/" or simple name)
    CategoryPath?: string;
     * Optional parameters for templated queries
    Parameters?: Record<string, any>;
     * Optional maximum number of rows to return
     * Optional starting row number for pagination
 * Input type for GetQueryDataByNameSystemUser method calls - executes a stored query by name
export interface GetQueryDataByNameSystemUserInput {
     * The name of the query to execute
 * Input type for query permissions to be created with a new query
export interface QueryPermissionInput {
     * Role ID to grant access to
    RoleID: string;
 * Input type for CreateQuery mutation calls - creates a new query with optional hierarchical category path
export interface CreateQueryInput {
     * Required name for the query (must be unique within category)
     * Optional existing category ID to assign the query to
     * Optional category path for automatic hierarchy creation (e.g., "Reports/Sales/Monthly") - takes precedence over CategoryID
     * Optional natural language question this query answers
    UserQuestion?: string;
     * Optional general description of what the query does
     * Optional SQL query text to execute (can contain Nunjucks template syntax)
    SQL?: string;
     * Optional technical documentation for developers
    TechnicalDescription?: string;
     * Optional original SQL before optimization or modification
    OriginalSQL?: string;
     * Optional user feedback about the query
    Feedback?: string;
     * Optional query approval status (defaults to 'Pending')
    Status?: 'Pending' | 'Approved' | 'Rejected' | 'Expired';
     * Optional quality indicator (higher = better quality, defaults to 0)
    QualityRank?: number;
     * Optional execution cost indicator (higher = more expensive to run)
    ExecutionCostRank?: number;
     * Optional flag indicating if the query uses Nunjucks template syntax (auto-detected if not specified)
    UsesTemplate?: boolean;
     * Optional array of permissions to create for the query
    Permissions?: QueryPermissionInput[];
 * Type for query field information
export interface QueryField {
    SQLBaseType?: string;
    SQLFullType?: string;
    SourceEntityID?: string;
    SourceEntity?: string;
    SourceFieldName?: string;
    IsComputed: boolean;
    ComputationDescription?: string;
    IsSummary?: boolean;
    SummaryDescription?: string;
 * Type for query parameter information
    IsRequired: boolean;
    DefaultValue?: string;
    SampleValue?: string;
    ValidationFilters?: string;
 * Type for query entity information
export interface MJQueryEntity {
 * Type for query permission information
export interface QueryPermission {
    Role?: string;
 * Result type for CreateQuery mutation calls - contains creation success status and query data
export interface CreateQueryResult {
     * Whether the query creation was successful
     * Error message if the creation failed (optional)
     * Unique identifier of the created query (optional)
     * Display name of the created query (optional)
    Name?: string;
     * Description of the created query (optional)
     * Category ID the query belongs to (optional)
     * Category name the query belongs to (optional)
     * SQL query text (optional)
     * Query status: Pending, Approved, Rejected, or Expired (optional)
     * Quality rank indicator (optional)
     * Embedding vector for semantic search (optional)
    EmbeddingVector?: string;
     * ID of the embedding model used (optional)
    EmbeddingModelID?: string;
     * Name of the embedding model used (optional)
    EmbeddingModelName?: string;
     * Array of fields discovered in the query (optional)
    Fields?: QueryField[];
     * Array of parameters found in the query template (optional)
    Parameters?: QueryParameter[];
     * Array of entities referenced by the query (optional)
    Entities?: MJQueryEntity[];
     * Array of permissions created for the query (optional)
    Permissions?: QueryPermission[];
 * Input type for UpdateQuery mutation calls - updates an existing query
export interface UpdateQueryInput {
     * Required ID of the query to update
     * Optional name for the query (must be unique within category)
     * Optional category ID to move the query to
     * Optional query approval status
     * Optional quality indicator (higher = better quality)
     * Optional flag indicating if the query uses Nunjucks template syntax
     * Optional array of permissions to update for the query (replaces existing permissions)
 * Result type for UpdateQuery mutation calls - contains update success status and query data
export interface UpdateQueryResult {
     * Whether the query update was successful
     * Error message if the update failed (optional)
     * Unique identifier of the updated query (optional)
     * Display name of the updated query (optional)
     * Description of the updated query (optional)
     * Array of permissions for the query (optional)
 * Delete options input type for controlling delete behavior.
 * All fields are optional - defaults will be applied if not provided.
export interface DeleteQueryOptionsInput {
     * Whether to skip AI actions during deletion.
    SkipEntityAIActions?: boolean;
     * Whether to skip regular entity actions during deletion.
    SkipEntityActions?: boolean;
     * When true, bypasses Validate() and actual database deletion but still
     * invokes associated actions (AI Actions, Entity Actions, etc.).
     * Used for replaying/simulating delete operations.
    ReplayOnly?: boolean;
     * When true, indicates this entity is being deleted as part of an IS-A parent chain
     * initiated by a child entity.
    IsParentEntityDelete?: boolean;
 * Result type for DeleteQuery mutation calls - contains deletion success status and deleted query data
export interface DeleteQueryResult {
     * Whether the query deletion was successful
     * Error message if the deletion failed (optional)
     * Unique identifier of the deleted query (optional)
     * Display name of the deleted query (optional)
