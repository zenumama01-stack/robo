 * Test Mock for ProviderBase
 * Provides minimal implementation for testing metadata refresh logic
 * Does NOT require database connection
import { ProviderBase } from '../../generic/providerBase';
    ProviderType,
    PotentialDuplicateRequest,
    PotentialDuplicateResponse,
    DatasetResultType,
    DatasetStatusResultType,
    DatasetItemFilterType,
    ILocalStorageProvider,
    EntityMergeOptions,
    PotentialDuplicateResult
} from '../../generic/interfaces';
import { UserInfo } from '../../generic/securityInfo';
import { RecordDependency, RecordMergeRequest, RecordMergeResult } from '../../generic/entityInfo';
import { CompositeKey } from '../../generic/compositeKey';
import { TransactionGroupBase } from '../../generic/transactionGroup';
export class TestMetadataProvider extends ProviderBase {
    private _allowRefresh = true;
    private _mockMetadata: any = null;
    private _getMetadataCallCount = 0;
    private _mockDelay = 100; // milliseconds
        return this._allowRefresh;
    public setAllowRefresh(value: boolean): void {
        this._allowRefresh = value;
        return 'Database';
        return {}; // Mock connection
    public getCallCount(): number {
        return this._getMetadataCallCount;
    public resetCallCount(): void {
        this._getMetadataCallCount = 0;
    public setMockDelay(ms: number): void {
        this._mockDelay = ms;
    public setMockMetadata(metadata: any): void {
        this._mockMetadata = metadata;
    // Don't override GetAllMetadata() - let the base class handle it
    // It will call our GetDatasetByName() which properly formats the data
    // Stub implementations for other provider methods
    public async GetEntityRecordName(): Promise<string> {
        return 'Test Record';
    public async GetEntityRecordNames(): Promise<any[]> {
    public async GetRecordFavoriteStatus(): Promise<boolean> {
    public async SetRecordFavoriteStatus(): Promise<void> {
        // No-op
    protected async InternalRunView(): Promise<any> {
        return { Success: true, Results: [], RowCount: 0, TotalRowCount: 0, ExecutionTime: 0, ErrorMessage: '', UserViewRunID: '' };
    protected async InternalRunViews(): Promise<any[]> {
    protected async InternalRunQuery(): Promise<any> {
        return { Success: true, Results: [], RowCount: 0, TotalRowCount: 0, ExecutionTime: 0, ErrorMessage: '', QueryID: '', QueryName: '' };
    protected async InternalRunQueries(): Promise<any[]> {
    // Required abstract methods (minimal stubs for testing - no database needed)
        // Return a minimal UserInfo object that satisfies TypeScript
        user.ID = 'test-user-id';
        user.Name = 'Test User';
        user.Email = 'test@example.com';
        user.FirstName = 'Test';
        user.LastName = 'User';
    public async GetRecordDependencies(entityName: string, compositeKey: CompositeKey, contextUser?: UserInfo): Promise<RecordDependency[]> {
    public async GetRecordDuplicates(params: PotentialDuplicateRequest, contextUser?: UserInfo): Promise<PotentialDuplicateResponse> {
        const response = new PotentialDuplicateResponse();
        response.PotentialDuplicateResult = [] as PotentialDuplicateResult[];
        const result = new RecordMergeResult();
        result.OverallStatus = 'Success';
        result.RecordStatus = [];
        result.RecordMergeLogID = null;
        result.Request = request;
    public async GetDatasetByName(datasetName: string, itemFilters?: DatasetItemFilterType[], contextUser?: UserInfo, providerToUse?: IMetadataProvider): Promise<DatasetResultType> {
        // Simulate the MJ_Metadata dataset structure that GetAllMetadata expects
        if (datasetName === 'MJ_Metadata') {
            // Track calls and simulate delay
            this._getMetadataCallCount++;
            await new Promise(resolve => setTimeout(resolve, this._mockDelay));
            // Use mock data if set, otherwise use default
            const baseData = this._mockMetadata || {
                        ID: `entity-${this._getMetadataCallCount}`,
                        Name: `Test Entity ${this._getMetadataCallCount}`,
                        BaseView: `vwTestEntity${this._getMetadataCallCount}`,
                        BaseTable: `MJTestEntity${this._getMetadataCallCount}`,
                            { ID: `f${this._getMetadataCallCount}-1`, EntityID: `entity-${this._getMetadataCallCount}`, Name: 'ID', Type: 'uniqueidentifier', IsPrimaryKey: true, Sequence: 1 },
                            { ID: `f${this._getMetadataCallCount}-2`, EntityID: `entity-${this._getMetadataCallCount}`, Name: 'Name', Type: 'nvarchar', IsPrimaryKey: false, Sequence: 2 },
                EntityPermissions: [],
                ApplicationEntities: [],
                ApplicationSettings: [],
                Roles: [],
                QueryEntities: [],
                QueryParameters: [],
                EntityDocumentTypes: [],
                DatasetID: 'mock-dataset-id',
                DatasetName: 'MJ_Metadata',
                Status: 'Success',
                LatestUpdateDate: new Date(),
                    { Code: 'Applications', EntityName: 'MJ: Applications', EntityID: '1', Results: baseData.Applications || [] },
                    { Code: 'Entities', EntityName: 'MJ: Entities', EntityID: '2', Results: baseData.Entities || [] },
                    { Code: 'EntityFields', EntityName: 'MJ: Entity Fields', EntityID: '3', Results: baseData.EntityFields || [] },
                    { Code: 'EntityFieldValues', EntityName: 'MJ: Entity Field Values', EntityID: '4', Results: baseData.EntityFieldValues || [] },
                    { Code: 'EntityPermissions', EntityName: 'MJ: Entity Permissions', EntityID: '5', Results: baseData.EntityPermissions || [] },
                    { Code: 'EntityRelationships', EntityName: 'MJ: Entity Relationships', EntityID: '6', Results: baseData.EntityRelationships || [] },
                    { Code: 'EntitySettings', EntityName: 'MJ: Entity Settings', EntityID: '7', Results: baseData.EntitySettings || [] },
                    { Code: 'ApplicationEntities', EntityName: 'MJ: Application Entities', EntityID: '8', Results: baseData.ApplicationEntities || [] },
                    { Code: 'ApplicationSettings', EntityName: 'MJ: Application Settings', EntityID: '9', Results: baseData.ApplicationSettings || [] },
                    { Code: 'Roles', EntityName: 'MJ: Roles', EntityID: '10', Results: baseData.Roles || [] },
                    { Code: 'RowLevelSecurityFilters', EntityName: 'MJ: Row Level Security Filters', EntityID: '11', Results: baseData.RowLevelSecurityFilters || [] },
                    { Code: 'AuditLogTypes', EntityName: 'MJ: Audit Log Types', EntityID: '12', Results: baseData.AuditLogTypes || [] },
                    { Code: 'Authorizations', EntityName: 'MJ: Authorizations', EntityID: '13', Results: baseData.Authorizations || [] },
                    { Code: 'QueryCategories', EntityName: 'MJ: Query Categories', EntityID: '14', Results: baseData.QueryCategories || [] },
                    { Code: 'Queries', EntityName: 'MJ: Queries', EntityID: '15', Results: baseData.Queries || [] },
                    { Code: 'QueryFields', EntityName: 'MJ: Query Fields', EntityID: '16', Results: baseData.QueryFields || [] },
                    { Code: 'QueryPermissions', EntityName: 'MJ: Query Permissions', EntityID: '17', Results: baseData.QueryPermissions || [] },
                    { Code: 'QueryEntities', EntityName: 'MJ: Query Entities', EntityID: '18', Results: baseData.QueryEntities || [] },
                    { Code: 'QueryParameters', EntityName: 'Query Parameters', EntityID: '19', Results: baseData.QueryParameters || [] },
                    { Code: 'EntityDocumentTypes', EntityName: 'MJ: Entity Document Types', EntityID: '20', Results: baseData.EntityDocumentTypes || [] },
                    { Code: 'Libraries', EntityName: 'MJ: Libraries', EntityID: '21', Results: baseData.Libraries || [] },
                    { Code: 'ExplorerNavigationItems', EntityName: 'MJ: Explorer Navigation Items', EntityID: '22', Results: baseData.ExplorerNavigationItems || [] },
    public async GetDatasetStatusByName(datasetName: string, itemFilters?: DatasetItemFilterType[], contextUser?: UserInfo, providerToUse?: IMetadataProvider): Promise<DatasetStatusResultType> {
            Status: 'Ready',
            EntityUpdateDates: []
        return 'mock://test-connection';
        throw new Error('TransactionGroup not implemented in test mock - not needed for metadata tests');
    public get LocalStorageProvider(): ILocalStorageProvider {
            GetItem: async (key: string) => null,
            SetItem: async (key: string, value: string) => {},
            Remove: async (key: string) => {}
        } as ILocalStorageProvider;
