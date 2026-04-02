// Mock the IMetadataProvider
    { ID: 'e-1', Name: 'Users', SchemaName: 'admin', BaseTable: 'User', Fields: [] },
    { ID: 'e-2', Name: 'Roles', SchemaName: 'admin', BaseTable: 'Role', Fields: [] }
    Entities: mockEntities,
    CurrentUser: { ID: 'u-1', Name: 'TestUser' },
    Applications: [{ ID: 'app-1', Name: 'Explorer' }],
    Roles: [{ ID: 'r-1', Name: 'Admin' }],
    Authorizations: [],
    RowLevelSecurityFilters: [],
    AuditLogTypes: [],
    Queries: [],
    QueryCategories: [],
    QueryFields: [],
    QueryPermissions: [],
    Libraries: [],
    ExplorerNavigationItems: [],
    LatestRemoteMetadata: null,
    LocalMetadataStore: null,
    Refresh: vi.fn().mockResolvedValue(true),
    GetEntityObject: vi.fn().mockResolvedValue({}),
    GetEntityObjectByID: vi.fn().mockResolvedValue({})
describe('Metadata', () => {
    let globalStore: Record<string, unknown>;
        globalStore = {};
        vi.spyOn(MJGlobal.Instance, 'GetGlobalObjectStore').mockReturnValue(globalStore);
    describe('Provider static property', () => {
        it('should get and set the static provider', () => {
            Metadata.Provider = mockProvider as never;
            expect(Metadata.Provider).toBe(mockProvider);
        it('should throw when global store is unavailable (get)', () => {
            vi.spyOn(MJGlobal.Instance, 'GetGlobalObjectStore').mockReturnValue(null as never);
            expect(() => Metadata.Provider).toThrow();
        it('should create a Metadata instance', () => {
    describe('entity access', () => {
        it('should access Entities from the provider', () => {
            expect(md.Entities).toEqual(mockEntities);
        it('should access CurrentUser from the provider', () => {
            expect(md.CurrentUser.ID).toBe('u-1');
        it('should access Applications from the provider', () => {
            expect(md.Applications).toHaveLength(1);
        it('should access Roles from the provider', () => {
            expect(md.Roles).toHaveLength(1);
    describe('GetEntityObject', () => {
        it('should delegate to provider', async () => {
            await md.GetEntityObject('MJ: Users');
            // GetEntityObject always passes 3 args: (entityName, loadKey, contextUser)
            expect(mockProvider.GetEntityObject).toHaveBeenCalledWith('MJ: Users', undefined, undefined);
        it('should pass contextUser to provider', async () => {
            // contextUser must have the shape: ID, Name, Email, UserRoles
            const contextUser = { ID: 'u-1', Name: 'TestUser', Email: 'test@test.com', UserRoles: [] } as never;
            await md.GetEntityObject('MJ: Users', contextUser);
            // When called with (entityName, contextUser), the overload resolves to
            // provider.GetEntityObject(entityName, undefined, wrappedContextUser)
            // contextUser gets wrapped into a UserInfo instance, so check positionally
            expect(mockProvider.GetEntityObject).toHaveBeenCalledTimes(1);
            const callArgs = mockProvider.GetEntityObject.mock.calls[0];
            expect(callArgs[0]).toBe('MJ: Users');
            expect(callArgs[1]).toBeUndefined();
            // Third arg is a UserInfo wrapping the contextUser object
            expect(callArgs[2]).toBeDefined();
            expect(callArgs[2].ID).toBe('u-1');
        it('should delegate to provider Refresh', async () => {
            expect(mockProvider.Refresh).toHaveBeenCalled();
