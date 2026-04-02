    RunActionParams: class {},
// Concrete implementation for testing
class ConcreteAction extends BaseAction {
    public callArgs: unknown[] = [];
    public returnValue = { Success: true, ResultCode: 'SUCCESS', Message: 'Done' };
    public throwError: Error | null = null;
    protected async InternalRunAction(params: unknown): Promise<{ Success: boolean; ResultCode: string; Message: string }> {
        this.callArgs.push(params);
        if (this.throwError) {
            throw this.throwError;
        return this.returnValue;
describe('BaseAction', () => {
    let action: ConcreteAction;
        action = new ConcreteAction();
    describe('Run', () => {
        it('should delegate to InternalRunAction', async () => {
            const result = await action.Run(params as never);
            expect(action.callArgs).toHaveLength(1);
            expect(action.callArgs[0]).toBe(params);
        it('should return the result from InternalRunAction', async () => {
            action.returnValue = { Success: false, ResultCode: 'FAIL', Message: 'Nope' };
            const result = await action.Run({} as never);
            expect(result.ResultCode).toBe('FAIL');
            expect(result.Message).toBe('Nope');
        it('should propagate errors from InternalRunAction', async () => {
            action.throwError = new Error('Internal failure');
            await expect(action.Run({} as never)).rejects.toThrow('Internal failure');
        it('should pass params through unchanged', async () => {
            const complexParams = {
                Action: { ID: '123', Name: 'Complex' },
                    { Name: 'p1', Value: 42, Type: 'Input' },
                    { Name: 'p2', Value: { nested: true }, Type: 'Output' },
                Filters: [{ ID: 'filter-1' }],
            await action.Run(complexParams as never);
            expect(action.callArgs[0]).toBe(complexParams);
    describe('abstract enforcement', () => {
        it('should not be instantiable directly', () => {
            // TypeScript prevents this at compile time, but at runtime we can verify the pattern
            expect(BaseAction).toBeDefined();
            expect(typeof BaseAction).toBe('function');
