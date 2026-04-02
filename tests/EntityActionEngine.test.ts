// Mock MJGlobal
        try { return JSON.parse(str); } catch { return null; }
    BaseEngine: class {
        static getInstance() { return new this(); }
        protected ContextUser = { ID: 'test-user' };
    CodeNameFromString: (s: string) => s.replace(/\s/g, '_'),
    MJActionParamEntity: class { ID = ''; Name = ''; ValueType = ''; Value = ''; Type = ''; ActionID = ''; },
    MJEntityActionParamEntity: class { ActionParamID = ''; ValueType = ''; Value = ''; EntityActionID = ''; },
    ActionEngineBase: class {
        static get Instance() { return new this(); }
        get Actions() { return []; }
        get ActionParams() { return []; }
        get ActionResultCodes() { return []; }
        get ActionFilters() { return []; }
    EntityActionEngineBase: class {
        get EntityActions() { return []; }
        get Filters() { return []; }
        get Invocations() { return []; }
        get Params() { return []; }
    ActionEntityExtended: class { ID = ''; Name = ''; DriverClass = ''; Params = []; },
    EntityActionEntityExtended: class { ID = ''; ActionID = ''; Filters = []; Params = []; },
    ActionParam: class { Name = ''; Value: unknown = null; Type = 'Input'; },
// Mock ActionEngineServer (from own package)
vi.mock('../generic/ActionEngine', () => ({
    ActionEngineServer: {
            ActionFilters: [],
            RunAction: vi.fn().mockResolvedValue({
                Message: 'OK',
                LogEntry: null,
import { EntityActionEngineServer } from '../entity-actions/EntityActionEngine';
    EntityActionInvocationBase,
    EntityActionInvocationSingleRecord,
    EntityActionInvocationMultipleRecords,
    EntityActionInvocationValidate,
} from '../entity-actions/EntityActionInvocationTypes';
describe('EntityActionEngineServer', () => {
    describe('RunEntityAction', () => {
        it('should throw when EntityAction is not provided', async () => {
            const engine = new EntityActionEngineServer();
            const params = { EntityAction: null, InvocationType: { Name: 'Read' } };
            await expect(engine.RunEntityAction(params as unknown as Record<string, Function>)).rejects.toThrow('EntityAction is required');
        it('should throw when InvocationType is not provided', async () => {
            const params = { EntityAction: { ID: 'ea-1' }, InvocationType: null };
            await expect(engine.RunEntityAction(params as unknown as Record<string, Function>)).rejects.toThrow('Invalid invocation type');
        it('should throw when ClassFactory fails to create invocation instance', async () => {
                EntityAction: { ID: 'ea-1' },
                InvocationType: { Name: 'Read' },
            await expect(engine.RunEntityAction(params as unknown as Record<string, Function>)).rejects.toThrow('Error creating instance');
        it('should invoke action on the created instance', async () => {
            const mockInvocation = {
                InvokeAction: vi.fn().mockResolvedValue({ Success: true, Message: 'OK' }),
            mockClassFactory.CreateInstance.mockReturnValue(mockInvocation);
                InvocationType: { Name: 'SingleRecord' },
            const result = await engine.RunEntityAction(params as unknown as Record<string, Function>);
            expect(mockInvocation.InvokeAction).toHaveBeenCalledWith(params);
describe('EntityActionInvocationBase', () => {
    describe('FindActionParam', () => {
        it('should find param by valueType (case insensitive)', () => {
            // We need a concrete class that doesn't use the abstract method
            const invocation = new EntityActionInvocationSingleRecord();
            const params = [
                { ValueType: ' Scalar ', Name: 'p1', ID: '1' },
                { ValueType: 'Simple Object', Name: 'p2', ID: '2' },
                { ValueType: 'BaseEntity Sub-Class', Name: 'p3', ID: '3' },
            const result = invocation.FindActionParam(params as never, 'Scalar');
            expect((result as Record<string, unknown>).Name).toBe('p1');
        it('should return undefined when no match found', () => {
            const params = [{ ValueType: 'Scalar', Name: 'p1', ID: '1' }];
            const result = invocation.FindActionParam(params as never, 'Other');
    describe('MapActionResultToEntityActionResult', () => {
        it('should map fields correctly', () => {
            const actionResult = {
                RunParams: { Action: { Name: 'Test' } },
                LogEntry: { ID: 'log-1' },
            const result = invocation.MapActionResultToEntityActionResult(actionResult as unknown as Record<string, Function>);
            expect(result.RunParams).toBe(actionResult.RunParams);
            expect(result.LogEntry).toBe(actionResult.LogEntry);
    describe('MapParams', () => {
        it('should handle Static valueType with JSON', async () => {
            const params = [{ ID: 'p1', Name: 'param1', Type: 'Input' }];
            const entityActionParams = [
                { ActionParamID: 'p1', ValueType: 'Static', Value: '{"key":"val"}' },
            const result = await invocation.MapParams(params as never, entityActionParams as never, {} as unknown as Record<string, Function>);
            expect(result[0].Name).toBe('param1');
            expect(result[0].Value).toEqual({ key: 'val' });
        it('should handle Static valueType with non-JSON string', async () => {
                { ActionParamID: 'p1', ValueType: 'Static', Value: 'plain-string' },
            expect(result[0].Value).toBe('plain-string');
        it('should handle Entity Object valueType', async () => {
            const entityObject = { ID: 'entity-1', Name: 'MJTestEntity' };
            const params = [{ ID: 'p1', Name: 'entity', Type: 'Input' }];
                { ActionParamID: 'p1', ValueType: 'Entity Object', Value: '' },
            const result = await invocation.MapParams(params as never, entityActionParams as never, entityObject as unknown as Record<string, Function>);
            expect(result[0].Value).toBe(entityObject);
        it('should handle Entity Field valueType', async () => {
            const entityObject = { ID: 'entity-1', Name: 'MJTestEntity', Status: 'Active' };
            const params = [{ ID: 'p1', Name: 'status', Type: 'Input' }];
                { ActionParamID: 'p1', ValueType: 'Entity Field', Value: 'Status' },
            expect(result[0].Value).toBe('Active');
        it('should handle Script valueType', async () => {
            // Override SafeEvalScript to avoid actual eval
            vi.spyOn(invocation, 'SafeEvalScript').mockResolvedValue('script-result');
            const params = [{ ID: 'p1', Name: 'computed', Type: 'Input' }];
                { ActionParamID: 'p1', ValueType: 'Script', Value: 'return 42', ID: 'eap-1' },
            expect(result[0].Value).toBe('script-result');
    describe('SafeEvalScript', () => {
        it('should execute simple scripts and return result', async () => {
            const result = await invocation.SafeEvalScript(
                'test-1',
                'EntityActionContext.result = 42;',
                {} as never
            expect(result).toBe(42);
        it('should return null on script error', async () => {
                'test-err',
                'throw new Error("boom");',
        it('should cache compiled scripts by EntityActionID', async () => {
            await invocation.SafeEvalScript('cache-test', 'EntityActionContext.result = 1;', {} as unknown as Record<string, Function>);
            await invocation.SafeEvalScript('cache-test', 'EntityActionContext.result = 2;', {} as unknown as Record<string, Function>);
            // Second call should use cached version (same ID), so result stays 1
            const result = await invocation.SafeEvalScript('cache-test', 'EntityActionContext.result = 3;', {} as unknown as Record<string, Function>);
            expect(result).toBe(1); // The cached first script
        it('should pass entity object via EntityActionContext', async () => {
            const entity = { Name: 'MJTestEntity' };
                'entity-test',
                'EntityActionContext.result = EntityActionContext.entityObject.Name;',
                entity as never
            expect(result).toBe('MJTestEntity');
describe('EntityActionInvocationSingleRecord', () => {
    describe('ValidateParams', () => {
        it('should throw when EntityObject is null', async () => {
            const params = { EntityObject: null };
            await expect(invocation.ValidateParams(params as unknown as Record<string, Function>)).rejects.toThrow(
                'EntityObject is required'
        it('should return true when EntityObject is present', async () => {
            const params = { EntityObject: { ID: 'e-1' } };
            const result = await invocation.ValidateParams(params as unknown as Record<string, Function>);
describe('EntityActionInvocationMultipleRecords', () => {
        it('should throw when InvocationType=List and no ListID', async () => {
            const invocation = new EntityActionInvocationMultipleRecords();
                InvocationType: { Name: 'List' },
                ListID: null,
                'ListID is required'
        it('should throw when InvocationType=View and no ViewID', async () => {
                InvocationType: { Name: 'View' },
                ViewID: null,
                'ViewID is required'
        it('should throw for unsupported invocation types', async () => {
                InvocationType: { Name: 'Unknown' },
                'only supports invocation types of List or View'
        it('should return true for List with valid ListID', async () => {
        it('should return true for View with valid ViewID', async () => {
                ViewID: 'view-1',
        it('should be case insensitive for invocation type name', async () => {
                InvocationType: { Name: '  LIST  ' },
    describe('GetRecordList', () => {
        it('should return empty array by default', async () => {
            const result = await (invocation as unknown as Record<string, Function>)['GetRecordList']();
describe('EntityActionInvocationValidate', () => {
    it('should extend EntityActionInvocationSingleRecord', () => {
        const invocation = new EntityActionInvocationValidate();
        expect(invocation).toBeInstanceOf(EntityActionInvocationSingleRecord);
