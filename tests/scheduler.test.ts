        protected static getInstance<T>(_key?: string): T { return {} as T; }
        protected async Load(_configs: unknown[], _provider: unknown, _forceRefresh: unknown, _contextUser?: unknown): Promise<void> {}
    Metadata: { Provider: null },
    RunView: vi.fn()
    ScheduledActionEntityExtended: class ScheduledActionEntityExtended {
        CronExpression: string = '';
    MJScheduledActionParamEntity: class MJScheduledActionParamEntity {
        ScheduledActionID: string = '';
        ActionParamID: string = '';
        Value: string = '';
        ValueType: string = 'Static';
    ActionEntityExtended: class ActionEntityExtended {
    ActionResult: class ActionResult {
    RunActionParams: class RunActionParams {}
vi.mock('cron-parser', () => ({
        parseExpression: vi.fn()
    SafeJSONParse: (val: string) => {
        try { return JSON.parse(val); } catch { return null; }
vi.mock('@memberjunction/sqlserver-dataprovider', () => ({
    SQLServerDataProvider: class SQLServerDataProvider {
        async ExecuteSQL(_sql: string): Promise<unknown> { return null; }
            ActionParams: [],
            async RunAction(_params: unknown): Promise<unknown> { return { Success: true }; }
import { ScheduledActionEngine } from '../scheduler';
describe('ScheduledActionEngine', () => {
    describe('IsActionDue', () => {
        it('should return true when action is due based on cron expression', () => {
            const mockScheduledAction = {
                ID: '1',
                Name: 'Test Action',
                ActionID: 'a1',
                CronExpression: '* * * * *', // every minute
                Params: []
            // Mock cron-parser to return a next date that is in the past
            const mockDate = new Date();
            const pastDate = new Date(mockDate.getTime() - 60000); // 1 min ago
            vi.mocked(cronParser.parseExpression).mockReturnValue({
                next: () => ({ toDate: () => pastDate }),
            } as unknown as ReturnType<typeof cronParser.parseExpression>);
            const result = ScheduledActionEngine.IsActionDue(mockScheduledAction as unknown as InstanceType<typeof import('@memberjunction/core-entities').ScheduledActionEntityExtended>, mockDate);
        it('should return false when action is not due', () => {
                CronExpression: '0 0 * * *', // midnight daily
            const futureDate = new Date(mockDate.getTime() + 3600000); // 1 hour ahead
                next: () => ({ toDate: () => futureDate }),
        it('should return false when cron expression is invalid', () => {
                CronExpression: 'invalid-cron',
            vi.mocked(cronParser.parseExpression).mockImplementation(() => {
                throw new Error('Invalid cron expression');
            const result = ScheduledActionEngine.IsActionDue(mockScheduledAction as unknown as InstanceType<typeof import('@memberjunction/core-entities').ScheduledActionEntityExtended>, new Date());
    describe('MapScheduledActionParamsToActionParams', () => {
        it('should map Static value type params correctly', async () => {
            const engine = new (ScheduledActionEngine as unknown as new () => ScheduledActionEngine)();
            // Mock ActionEngineServer.Instance.ActionParams
            const { ActionEngineServer } = await import('@memberjunction/actions');
            (ActionEngineServer.Instance as unknown as Record<string, unknown>).ActionParams = [
                { ID: 'p1', Name: 'TestParam', Type: 'Input' }
            const scheduledAction = {
                ID: 'sa1',
                CronExpression: '* * * * *',
                    { ScheduledActionID: 'sa1', ActionParamID: 'p1', Value: 'hello', ValueType: 'Static' }
            const result = await (engine as unknown as Record<string, (sa: unknown) => Promise<Array<{ Name: string; Value: unknown; Type: string }>>>).MapScheduledActionParamsToActionParams(scheduledAction);
            expect(result[0].Name).toBe('TestParam');
            expect(result[0].Value).toBe('hello');
            expect(result[0].Type).toBe('Input');
        it('should parse JSON values for Static type', async () => {
                { ID: 'p1', Name: 'JsonParam', Type: 'Input' }
                    { ScheduledActionID: 'sa1', ActionParamID: 'p1', Value: '{"key":"value"}', ValueType: 'Static' }
            const result = await (engine as unknown as Record<string, (sa: unknown) => Promise<Array<{ Name: string; Value: unknown }>>>).MapScheduledActionParamsToActionParams(scheduledAction);
            expect(result[0].Value).toEqual({ key: 'value' });
        it('should be accessible as a static property', () => {
            // This tests the static instance getter pattern
            expect(ScheduledActionEngine).toBeDefined();
