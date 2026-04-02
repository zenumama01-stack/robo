// Mocks - must be declared before imports that use them
        get ActionID() { return ''; },
        set ActionID(_v: string) {},
        get StartedAt() { return null; },
        set StartedAt(_v: Date | null) {},
        get EndedAt() { return null; },
        set EndedAt(_v: Date | null) {},
        get UserID() { return ''; },
        set UserID(_v: string) {},
        get Params() { return ''; },
        set Params(_v: string) {},
        get ResultCode() { return ''; },
        set ResultCode(_v: string | undefined) {},
        get Message() { return ''; },
        set Message(_v: string | undefined) {},
        get LatestResult() { return null; },
        Metadata: vi.fn(function() {
            return { GetEntityObject: vi.fn().mockResolvedValue(mockLogEntity) };
        RunView: vi.fn(),
        BaseEngine: class MockBaseEngine<T> {
            protected static getInstance<U>(_key?: string): U {
                return {} as U;
            protected ContextUser = { ID: 'test-user-id', Name: 'Test User' };
            protected Loaded = true;
            protected async Load() {}
            protected async AdditionalLoading() {}
            protected HandleSingleViewResult() {}
            protected RunViewProviderToUse = undefined;
        BaseEntity: class {},
// Mock @memberjunction/core-entities
    MJActionExecutionLogEntity: class {},
    MJActionFilterEntity: class {},
    MJActionParamEntity: class {},
    MJActionResultCodeEntity: class {},
    MJActionCategoryEntity: class {},
    MJActionEntity: class {},
    MJActionLibraryEntity: class {},
    MJEntityActionParamEntity: class {},
    MJEntityActionFilterEntity: class {},
    MJEntityActionInvocationEntity: class {},
    MJEntityActionInvocationTypeEntity: class {},
    MJEntityActionEntity: class {},
    MJCompanyIntegrationEntity: class {},
    MJIntegrationEntity: class {},
const { mockClassFactory } = vi.hoisted(() => ({
    mockClassFactory: {
        CreateInstance: vi.fn(),
        GetAllRegistrations: vi.fn().mockReturnValue([]),
            ClassFactory: mockClassFactory,
    SafeJSONParse: vi.fn((str: string) => {
            return JSON.parse(str);
// Mock @memberjunction/actions-base
    class MockActionEngineBase {
        private _Actions: Array<Record<string, unknown>> = [];
        private _ActionCategories: Array<Record<string, unknown>> = [];
        private _Filters: Array<Record<string, unknown>> = [];
        private _Params: Array<Record<string, unknown>> = [];
        private _ActionResultCodes: Array<Record<string, unknown>> = [];
        private _ActionLibraries: Array<Record<string, unknown>> = [];
            return new MockActionEngineBase() as unknown as T;
        get Actions() { return this._Actions; }
        get ActionCategories() { return this._ActionCategories; }
        get ActionParams() { return this._Params; }
        get ActionFilters() { return this._Filters; }
        get ActionResultCodes() { return this._ActionResultCodes; }
        get ActionLibraries() { return this._ActionLibraries; }
        async Config() {}
        protected async LoadMultipleEntityConfigs() {}
    class MockEntityActionEngineBase {
        static get Instance() { return new MockEntityActionEngineBase(); }
        static getInstance<T>(): T { return new MockEntityActionEngineBase() as unknown as T; }
        ActionEngineBase: MockActionEngineBase,
        ActionEntityExtended: class {
            Params: Array<Record<string, unknown>> = [];
            Name = '';
            ID = '';
            DriverClass = '';
        ActionParam: class { Name = ''; Value: unknown = null; Type: string = 'Input'; },
        ActionResult: class {},
        ActionResultSimple: class { Success = false; ResultCode = ''; Message = ''; },
        RunActionParams: class {
            Action: Record<string, unknown> = {};
            ContextUser: Record<string, unknown> = {};
            Filters: Array<Record<string, unknown>> = [];
            SkipActionLog = false;
        EntityActionEngineBase: MockEntityActionEngineBase,
        EntityActionInvocationParams: class {},
        EntityActionResult: class {},
// Now import the modules under test
import { ActionEngineServer } from '../generic/ActionEngine';
import { BaseAction } from '../generic/BaseAction';
import { Metadata, LogError } from '@memberjunction/core';
// Create a concrete subclass of BaseAction for testing
class TestAction extends BaseAction {
    public mockResult = { Success: true, ResultCode: 'SUCCESS', Message: 'Test passed' };
    protected async InternalRunAction(): Promise<{ Success: boolean; ResultCode: string; Message: string }> {
        return this.mockResult;
class FailingAction extends BaseAction {
    protected async InternalRunAction(): Promise<never> {
        throw new Error('Action blew up');
describe('ActionEngineServer', () => {
    let engine: ActionEngineServer;
        // Create a fresh instance by calling the constructor directly
        engine = new ActionEngineServer();
        // Set up internal state
        (engine as Record<string, unknown>)['ContextUser'] = { ID: 'test-user-id', Name: 'Test User' };
    describe('RunAction', () => {
        it('should return failure when ValidateInputs returns false', async () => {
            const validateSpy = vi.spyOn(engine as never, 'ValidateInputs' as never).mockResolvedValue(false as never);
                Action: { ID: 'action-1', Name: 'Test Action', DriverClass: 'TestDriver' },
                ContextUser: { ID: 'user-1', Name: 'Test' },
                Params: [{ Name: 'input1', Value: 'val', Type: 'Input' }],
                SkipActionLog: true,
            const result = await engine.RunAction(params as unknown as Record<string, Function>);
            expect(result.Message).toContain('Input validation failed');
            expect(validateSpy).toHaveBeenCalledWith(params);
        it('should log when SkipActionLog is false and validation fails', async () => {
            vi.spyOn(engine as never, 'ValidateInputs' as never).mockResolvedValue(false as never);
            const startAndEndSpy = vi.spyOn(engine as never, 'StartAndEndActionLog' as never).mockResolvedValue({} as unknown as Record<string, Function>);
            expect(startAndEndSpy).toHaveBeenCalled();
        it('should NOT log when SkipActionLog is true and validation fails', async () => {
            const startAndEndSpy = vi.spyOn(engine as never, 'StartAndEndActionLog' as never);
            await engine.RunAction(params as unknown as Record<string, Function>);
            expect(startAndEndSpy).not.toHaveBeenCalled();
        it('should call InternalRunAction when inputs valid and filters pass', async () => {
            vi.spyOn(engine as never, 'ValidateInputs' as never).mockResolvedValue(true as never);
            vi.spyOn(engine as never, 'RunFilters' as never).mockResolvedValue(true as never);
            const internalSpy = vi.spyOn(engine as never, 'InternalRunAction' as never).mockResolvedValue({
                Message: 'Done',
                RunParams: {},
            } as unknown as Record<string, Function>);
            expect(internalSpy).toHaveBeenCalledWith(params);
            const result = await (engine as unknown as Record<string, Function>)['ValidateInputs']({} as unknown as Record<string, Function>);
    describe('RunFilters', () => {
        it('should return true when no filters are provided', async () => {
            const params = { Filters: undefined };
            const result = await (engine as unknown as Record<string, Function>)['RunFilters'](params as unknown as Record<string, Function>);
        it('should return true when filters array is empty', async () => {
            const params = { Filters: [] };
        it('should return true when all filters pass', async () => {
            vi.spyOn(engine as never, 'RunSingleFilter' as never).mockResolvedValue(true as never);
            const params = { Filters: [{ ID: 'f1' }, { ID: 'f2' }] };
        it('should return false when any filter fails', async () => {
            vi.spyOn(engine as never, 'RunSingleFilter' as never)
                .mockResolvedValueOnce(true as never)
                .mockResolvedValueOnce(false as never);
        it('should short-circuit on first failing filter', async () => {
            const singleFilterSpy = vi.spyOn(engine as never, 'RunSingleFilter' as never)
            const params = { Filters: [{ ID: 'f1' }, { ID: 'f2' }, { ID: 'f3' }] };
            await (engine as unknown as Record<string, Function>)['RunFilters'](params as unknown as Record<string, Function>);
            expect(singleFilterSpy).toHaveBeenCalledTimes(1);
    describe('RunSingleFilter', () => {
        it('should return true (stub implementation)', async () => {
            const result = await (engine as unknown as Record<string, Function>)['RunSingleFilter']({} as never, {} as unknown as Record<string, Function>);
    describe('GetActionParamsForAction', () => {
        it('should map Scalar ValueType to default value directly', () => {
            const action = {
                Name: 'TestAction',
                Params: [{ Name: 'param1', ValueType: 'Scalar', DefaultValue: 'hello', Type: 'Input' }],
            const result = (engine as unknown as Record<string, Function>)['GetActionParamsForAction'](action as unknown as Record<string, Function>);
            expect(result).toEqual([{ Name: 'param1', Value: 'hello', Type: 'Input' }]);
        it('should parse JSON for Simple Object ValueType', () => {
                Params: [{ Name: 'param1', ValueType: 'Simple Object', DefaultValue: '{"key":"val"}', Type: 'Input' }],
            expect(result).toEqual([{ Name: 'param1', Value: { key: 'val' }, Type: 'Input' }]);
        it('should use raw string for Simple Object when JSON parse fails', () => {
                Params: [{ Name: 'param1', ValueType: 'Simple Object', DefaultValue: 'not-json', Type: 'Input' }],
            expect(result).toEqual([{ Name: 'param1', Value: 'not-json', Type: 'Input' }]);
        it('should pass through BaseEntity Sub-Class ValueType', () => {
                Params: [{ Name: 'param1', ValueType: 'BaseEntity Sub-Class', DefaultValue: 'entity-ref', Type: 'Input' }],
            expect(result).toEqual([{ Name: 'param1', Value: 'entity-ref', Type: 'Input' }]);
        it('should pass through Other ValueType', () => {
                Params: [{ Name: 'param1', ValueType: 'Other', DefaultValue: 'other-val', Type: 'Both' }],
            expect(result).toEqual([{ Name: 'param1', Value: 'other-val', Type: 'Both' }]);
        it('should log error and use default for unknown ValueType', () => {
                Params: [{ Name: 'param1', ValueType: 'UnknownType', DefaultValue: 'fallback', Type: 'Output' }],
            expect(result).toEqual([{ Name: 'param1', Value: 'fallback', Type: 'Output' }]);
            expect(LogError).toHaveBeenCalledWith(expect.stringContaining('Unknown ValueType'));
        it('should handle multiple params', () => {
                    { Name: 'p1', ValueType: 'Scalar', DefaultValue: 'a', Type: 'Input' },
                    { Name: 'p2', ValueType: 'Scalar', DefaultValue: 'b', Type: 'Output' },
            expect(result[0].Name).toBe('p1');
            expect(result[1].Name).toBe('p2');
        it('should handle empty params array', () => {
            const action = { Name: 'TestAction', Params: [] };
        it('should create action via ClassFactory and run it', async () => {
            const testAction = new TestAction();
            mockClassFactory.CreateInstance.mockReturnValue(testAction);
            // Set up the engine's ActionResultCodes
            (engine as Record<string, unknown>)['_ActionResultCodes'] = [
                { ActionID: 'action-1', ResultCode: 'SUCCESS' },
                Action: { ID: 'action-1', Name: 'Test Action', DriverClass: 'TestAction' },
            const result = await (engine as unknown as Record<string, Function>)['InternalRunAction'](params as unknown as Record<string, Function>);
            expect(mockClassFactory.CreateInstance).toHaveBeenCalledWith(
                BaseAction, 'TestAction', { ID: 'user-1', Name: 'Test' }
        it('should throw when ClassFactory returns null', async () => {
            mockClassFactory.CreateInstance.mockReturnValue(null);
            expect(result.Message).toContain('Could not find a class for action');
        it('should throw when ClassFactory returns base BaseAction', async () => {
            // Create a plain BaseAction-like object whose constructor === BaseAction
            const baseAction = Object.create(BaseAction.prototype);
            Object.defineProperty(baseAction, 'constructor', { value: BaseAction });
            mockClassFactory.CreateInstance.mockReturnValue(baseAction);
            expect(result.Message).toContain('Could not find a class');
        it('should catch errors from action execution', async () => {
            const failAction = new FailingAction();
            mockClassFactory.CreateInstance.mockReturnValue(failAction);
                Action: { ID: 'action-1', Name: 'Failing Action', DriverClass: 'FailAction' },
            expect(result.Message).toContain('Action blew up');
            expect(LogError).toHaveBeenCalled();
        it('should use Action.Name when DriverClass is not provided', async () => {
            (engine as Record<string, unknown>)['_ActionResultCodes'] = [];
                Action: { ID: 'action-1', Name: 'FallbackName', DriverClass: '' },
            await (engine as unknown as Record<string, Function>)['InternalRunAction'](params as unknown as Record<string, Function>);
                BaseAction, 'FallbackName', expect.anything()
        it('should match result codes case-insensitively', async () => {
            testAction.mockResult = { Success: true, ResultCode: 'success', Message: 'ok' };
                { ActionID: 'action-1', ResultCode: '  SUCCESS  ' },
                Action: { ID: 'action-1', Name: 'Test', DriverClass: 'TestAction' },
            expect(result.Result).toBeDefined();
            expect(result.Result.ResultCode).toBe('  SUCCESS  ');
        it('should create and end action log when SkipActionLog is false', async () => {
            const startSpy = vi.spyOn(engine as never, 'StartActionLog' as never).mockResolvedValue({
            const endSpy = vi.spyOn(engine as never, 'EndActionLog' as never).mockResolvedValue(undefined as never);
            expect(startSpy).toHaveBeenCalled();
            expect(endSpy).toHaveBeenCalled();
    describe('StartActionLog', () => {
        it('should create and save a log entity', async () => {
                set StartedAt(_v: Date) {},
            (Metadata as unknown as ReturnType<typeof vi.fn>).mockImplementation(function() {
                return { GetEntityObject: vi.fn().mockResolvedValue(mockEntity) };
                Action: { ID: 'action-1', Name: 'Test' },
                Params: [{ Name: 'p1', Value: 'v1' }],
            const logEntry = await (engine as unknown as Record<string, Function>)['StartActionLog'](params as never, true);
            expect(mockEntity.NewRecord).toHaveBeenCalled();
            expect(mockEntity.Save).toHaveBeenCalled();
            expect(logEntry).toBe(mockEntity);
        it('should not save when saveRecord is false', async () => {
            await (engine as unknown as Record<string, Function>)['StartActionLog'](params as never, false);
            expect(mockEntity.Save).not.toHaveBeenCalled();
        it('should log error when save fails', async () => {
                Save: vi.fn().mockResolvedValue(false),
                LatestResult: { Message: 'DB error' },
            const params = { Action: { ID: 'a1', Name: 'Test' }, Params: [] };
            await (engine as unknown as Record<string, Function>)['StartActionLog'](params as never, true);
    describe('EndActionLog', () => {
        it('should set end time and save', async () => {
            const logEntity = {
                set EndedAt(_v: Date) {},
            const params = { Params: [{ Name: 'p1', Value: 'v1' }] };
            const result = { Result: { ResultCode: 'OK' }, Message: 'Done' };
            await (engine as unknown as Record<string, Function>)['EndActionLog'](logEntity as never, params as never, result as unknown as Record<string, Function>);
            expect(logEntity.Save).toHaveBeenCalled();
                LatestResult: { Message: 'save error' },
            const params = { Action: { Name: 'Test' }, Params: [] };
            const result = { Result: undefined, Message: 'fail' };
    describe('StartAndEndActionLog', () => {
        it('should call StartActionLog with saveRecord=false then EndActionLog', async () => {
            const mockLogEntity = { Save: vi.fn().mockResolvedValue(true) };
            const startSpy = vi.spyOn(engine as never, 'StartActionLog' as never).mockResolvedValue(mockLogEntity as unknown as Record<string, Function>);
            const result = { Message: 'ok' };
            const logEntry = await (engine as unknown as Record<string, Function>)['StartAndEndActionLog'](params as never, result as unknown as Record<string, Function>);
            expect(startSpy).toHaveBeenCalledWith(params, false);
            expect(endSpy).toHaveBeenCalledWith(mockLogEntity, params, result);
            expect(logEntry).toBe(mockLogEntity);
        it('should return an instance from static getter', () => {
            const instance = ActionEngineServer.Instance;
            expect(instance).toBeDefined();
