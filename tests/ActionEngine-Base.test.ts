// Mock external dependencies before imports
            protected static getInstance<T>(key?: string): T {
                return {} as T;
            protected async Load(configs: unknown[], provider: unknown, forceRefresh: boolean, contextUser?: unknown): Promise<void> {}
            protected HandleSingleViewResult(config: unknown, result: unknown): void {}
            protected get RunViewProviderToUse(): unknown { return null; }
            RunView: vi.fn().mockResolvedValue({ Success: true, Results: [] }),
            RunViews: vi.fn().mockResolvedValue([])
        CodeNameFromString: (name: string) => name.replace(/[^a-zA-Z0-9]/g, '_'),
            Config(_contextUser: unknown): Promise<void> { return Promise.resolve(); }
    MJActionCategoryEntity: class MJActionCategoryEntity {
        ParentID: string | null = null;
    MJActionEntity: class MJActionEntity {
        CategoryID: string = '';
    MJActionExecutionLogEntity: class MJActionExecutionLogEntity {},
    MJActionFilterEntity: class MJActionFilterEntity {},
    MJActionLibraryEntity: class MJActionLibraryEntity {
        ActionID: string = '';
    MJActionParamEntity: class MJActionParamEntity {
    MJActionResultCodeEntity: class MJActionResultCodeEntity {
    MJEntityActionEntity: class MJEntityActionEntity {
        EntityID: string = '';
        Entity: string = '';
        Status: string = '';
    MJEntityActionFilterEntity: class MJEntityActionFilterEntity {
        EntityActionID: string = '';
    MJEntityActionInvocationEntity: class MJEntityActionInvocationEntity {
        InvocationType: string = '';
    MJEntityActionInvocationTypeEntity: class MJEntityActionInvocationTypeEntity {},
    MJEntityActionParamEntity: class MJEntityActionParamEntity {
    ActionEngineBase,
    ActionResult,
    ActionResultSimple,
    ActionParam,
    ActionLibrary,
    GeneratedCode,
    RunActionParams,
    EntityActionEngineBase,
    EntityActionInvocationParams,
    EntityActionResult,
    ActionEntityExtended,
    EntityActionEntityExtended
} from '../index';
describe('ActionParam', () => {
    it('should create an ActionParam with correct properties', () => {
        const param = new ActionParam();
        param.Name = 'TestParam';
        param.Value = 42;
        expect(param.Name).toBe('TestParam');
        expect(param.Value).toBe(42);
        expect(param.Type).toBe('Input');
    it('should support Input, Output, and Both types', () => {
        const inputParam = new ActionParam();
        inputParam.Type = 'Input';
        expect(inputParam.Type).toBe('Input');
        const outputParam = new ActionParam();
        outputParam.Type = 'Output';
        expect(outputParam.Type).toBe('Output');
        const bothParam = new ActionParam();
        bothParam.Type = 'Both';
        expect(bothParam.Type).toBe('Both');
describe('ActionResultSimple', () => {
    it('should hold success result data', () => {
        const result = new ActionResultSimple();
        result.Success = true;
        result.ResultCode = 'SUCCESS';
        result.Message = 'Operation completed';
        expect(result.ResultCode).toBe('SUCCESS');
        expect(result.Message).toBe('Operation completed');
    it('should hold failure result data', () => {
        result.Success = false;
        result.ResultCode = 'FAILED';
        result.Params = [{ Name: 'error', Value: 'something went wrong', Type: 'Output' }];
        expect(result.ResultCode).toBe('FAILED');
        expect(result.Params).toHaveLength(1);
        expect(result.Params![0].Name).toBe('error');
describe('ActionResult', () => {
    it('should hold complete action result data', () => {
        const result = new ActionResult();
        result.Message = 'Done';
        expect(result.Message).toBe('Done');
describe('GeneratedCode', () => {
    it('should store code generation results', () => {
        const gc = new GeneratedCode();
        gc.Success = true;
        gc.Code = 'console.log("hello")';
        gc.Comments = 'A simple log';
        gc.LibrariesUsed = [];
        expect(gc.Success).toBe(true);
        expect(gc.Code).toBe('console.log("hello")');
        expect(gc.Comments).toBe('A simple log');
        expect(gc.LibrariesUsed).toEqual([]);
    it('should store error information on failure', () => {
        gc.Success = false;
        gc.ErrorMessage = 'Code generation failed';
        gc.Code = '';
        gc.Comments = '';
        expect(gc.Success).toBe(false);
        expect(gc.ErrorMessage).toBe('Code generation failed');
describe('ActionLibrary', () => {
    it('should store library name and items', () => {
        const lib = new ActionLibrary();
        lib.LibraryName = 'MyLib';
        lib.ItemsUsed = ['item1', 'item2'];
        expect(lib.LibraryName).toBe('MyLib');
        expect(lib.ItemsUsed).toEqual(['item1', 'item2']);
describe('RunActionParams', () => {
    it('should create params with default values', () => {
        const params = new RunActionParams();
        expect(params.SkipActionLog).toBeUndefined();
        expect(params.Context).toBeUndefined();
    it('should support generic context typing', () => {
        interface MyContext {
        const params = new RunActionParams<MyContext>();
        params.Context = { apiKey: 'test-key' };
        params.Params = [];
        params.Filters = [];
        expect(params.Context.apiKey).toBe('test-key');
        expect(params.Params).toEqual([]);
        expect(params.Filters).toEqual([]);
describe('ActionEngineBase', () => {
    let engine: ActionEngineBase;
        // Access the engine via its public singleton pattern
        // but we need to create a testable instance
        engine = new (ActionEngineBase as unknown as new () => ActionEngineBase)();
        // Manually set internal state for testing
        (engine as unknown as Record<string, unknown>)['_Actions'] = [];
        (engine as unknown as Record<string, unknown>)['_ActionCategories'] = [];
        (engine as unknown as Record<string, unknown>)['_Filters'] = [];
        (engine as unknown as Record<string, unknown>)['_Params'] = [];
        (engine as unknown as Record<string, unknown>)['_ActionResultCodes'] = [];
        (engine as unknown as Record<string, unknown>)['_ActionLibraries'] = [];
    describe('CoreActionsRootCategoryID', () => {
        it('should return the hardcoded root category ID', () => {
            expect(engine.CoreActionsRootCategoryID).toBe('15E03732-607E-4125-86F4-8C846EE88749');
    describe('GetActionByName', () => {
        it('should throw when name is null or empty', () => {
            expect(() => engine.GetActionByName('')).toThrow('Action name cannot be null or empty.');
            expect(() => engine.GetActionByName('  ')).toThrow('Action name cannot be null or empty.');
        it('should return undefined when no matching action exists', () => {
            const result = engine.GetActionByName('NonExistent');
        it('should find action case-insensitively', () => {
            const mockAction = { Name: 'Test Action', ID: '123', CategoryID: 'cat1' };
            (engine as unknown as Record<string, unknown>)['_Actions'] = [mockAction];
            const result = engine.GetActionByName('test action');
            expect(result).toBe(mockAction);
        it('should trim whitespace in search', () => {
            const result = engine.GetActionByName('  Test Action  ');
    describe('IsChildCategoryOf', () => {
        it('should return false for null/empty IDs', () => {
            expect(engine.IsChildCategoryOf('', 'parent')).toBe(false);
            expect(engine.IsChildCategoryOf('child', '')).toBe(false);
            expect(engine.IsChildCategoryOf('', '')).toBe(false);
        it('should return true when IDs match (same category)', () => {
            expect(engine.IsChildCategoryOf('abc', 'ABC')).toBe(true);
        it('should return true when category is direct child of parent', () => {
            (engine as unknown as Record<string, unknown>)['_ActionCategories'] = [
                { ID: 'child1', ParentID: 'parent1' }
            expect(engine.IsChildCategoryOf('child1', 'parent1')).toBe(true);
        it('should return true for nested child through recursion', () => {
                { ID: 'grandchild', ParentID: 'child1' },
                { ID: 'child1', ParentID: 'root' }
            expect(engine.IsChildCategoryOf('grandchild', 'root')).toBe(true);
        it('should return false when category is not a child', () => {
                { ID: 'child1', ParentID: 'parent1' },
                { ID: 'child2', ParentID: 'parent2' }
            expect(engine.IsChildCategoryOf('child1', 'parent2')).toBe(false);
    describe('IsCoreAction', () => {
        it('should return false for null action', () => {
            expect(engine.IsCoreAction(null as unknown as ActionEntityExtended)).toBe(false);
    describe('IsCoreActionCategory', () => {
        it('should return false for null/empty category ID', () => {
            expect(engine.IsCoreActionCategory('')).toBe(false);
    describe('property accessors', () => {
        it('should return arrays from each accessor', () => {
            expect(engine.Actions).toEqual([]);
            expect(engine.ActionCategories).toEqual([]);
            expect(engine.ActionParams).toEqual([]);
            expect(engine.ActionFilters).toEqual([]);
            expect(engine.ActionResultCodes).toEqual([]);
            expect(engine.ActionLibraries).toEqual([]);
        it('should return filtered CoreActions and NonCoreActions', () => {
            expect(engine.CoreActions).toEqual([]);
            expect(engine.NonCoreActions).toEqual([]);
    describe('ValidateInputs', () => {
        it('should return true by default', async () => {
            const result = await (engine as unknown as Record<string, (...args: unknown[]) => Promise<boolean>>)['ValidateInputs'](params);
describe('EntityActionEngineBase', () => {
    let engine: EntityActionEngineBase;
        engine = new (EntityActionEngineBase as unknown as new () => EntityActionEngineBase)();
        (engine as unknown as Record<string, unknown>)['_EntityActions'] = [];
        (engine as unknown as Record<string, unknown>)['_EntityActionParams'] = [];
        (engine as unknown as Record<string, unknown>)['_EntityActionInvocationTypes'] = [];
        (engine as unknown as Record<string, unknown>)['_EntityActionFilters'] = [];
        (engine as unknown as Record<string, unknown>)['_EntityActionInvocations'] = [];
    describe('GetActionsByEntityName', () => {
        it('should return empty array when no matching actions', () => {
            const result = engine.GetActionsByEntityName('SomeEntity');
            expect(result).toEqual([]);
        it('should find actions case-insensitively', () => {
            const mockAction = { Entity: 'Customers', EntityID: 'e1', ID: 'ea1', Status: 'Active' };
            (engine as unknown as Record<string, unknown>)['_EntityActions'] = [mockAction];
            const result = engine.GetActionsByEntityName('customers');
        it('should filter by status when provided', () => {
            const activeAction = { Entity: 'Customers', EntityID: 'e1', ID: 'ea1', Status: 'Active' };
            const disabledAction = { Entity: 'Customers', EntityID: 'e1', ID: 'ea2', Status: 'Disabled' };
            (engine as unknown as Record<string, unknown>)['_EntityActions'] = [activeAction, disabledAction];
            const result = engine.GetActionsByEntityName('Customers', 'Active');
            expect(result[0]).toBe(activeAction);
    describe('GetActionsByEntityID', () => {
        it('should return empty array when no matching entity ID', () => {
            const result = engine.GetActionsByEntityID('non-existent');
        it('should return matching entity actions by ID', () => {
            const result = engine.GetActionsByEntityID('e1');
            expect(engine.InvocationTypes).toEqual([]);
            expect(engine.Filters).toEqual([]);
            expect(engine.Invocations).toEqual([]);
            expect(engine.EntityActions).toEqual([]);
            expect(engine.Params).toEqual([]);
describe('EntityActionInvocationParams', () => {
    it('should hold invocation params', () => {
        const params = new EntityActionInvocationParams();
        params.ViewID = 'view-1';
        params.ListID = 'list-1';
        expect(params.ViewID).toBe('view-1');
        expect(params.ListID).toBe('list-1');
describe('EntityActionResult', () => {
    it('should hold result data', () => {
        const result = new EntityActionResult();
        result.Message = 'Completed';
        expect(result.Message).toBe('Completed');
