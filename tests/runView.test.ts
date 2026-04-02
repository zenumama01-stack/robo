import { RunView, RunViewParams } from '../views/runView';
// Mock the IRunViewProvider
const mockRunViewFn = vi.fn().mockResolvedValue({
    Results: [{ ID: '1', Name: 'Test' }],
    TotalRowCount: 1,
    RowCount: 1
const mockRunViewsFn = vi.fn().mockResolvedValue([{
    Results: [{ ID: '1' }],
    RunView: mockRunViewFn,
    RunViews: mockRunViewsFn,
describe('RunViewParams', () => {
        it('should return true for same reference', () => {
            const params = new RunViewParams();
            params.EntityName = 'MJ: Users';
            expect(RunViewParams.Equals(params, params)).toBe(true);
        it('should return true for both null', () => {
            expect(RunViewParams.Equals(null, null)).toBe(true);
        it('should return true for both undefined', () => {
            expect(RunViewParams.Equals(undefined, undefined)).toBe(true);
        it('should return false when one is null', () => {
            expect(RunViewParams.Equals(params, null)).toBe(false);
            expect(RunViewParams.Equals(null, params)).toBe(false);
        it('should return true for equivalent params', () => {
            const a: RunViewParams = {
                ExtraFilter: "IsActive=1",
            const b: RunViewParams = {
            expect(RunViewParams.Equals(a, b)).toBe(true);
        it('should return false for different EntityName', () => {
            const a: RunViewParams = { EntityName: 'MJ: Users' };
            const b: RunViewParams = { EntityName: 'MJ: Roles' };
            expect(RunViewParams.Equals(a, b)).toBe(false);
        it('should return false for different ExtraFilter', () => {
            const a: RunViewParams = { EntityName: 'MJ: Users', ExtraFilter: 'A=1' };
            const b: RunViewParams = { EntityName: 'MJ: Users', ExtraFilter: 'B=2' };
        it('should return false for different MaxRows', () => {
            const a: RunViewParams = { EntityName: 'MJ: Users', MaxRows: 10 };
            const b: RunViewParams = { EntityName: 'MJ: Users', MaxRows: 20 };
        it('should return false for different ResultType', () => {
            const a: RunViewParams = { EntityName: 'MJ: Users', ResultType: 'simple' };
            const b: RunViewParams = { EntityName: 'MJ: Users', ResultType: 'entity_object' };
        it('should compare Fields arrays', () => {
            const a: RunViewParams = { EntityName: 'MJ: Users', Fields: ['ID', 'Name'] };
            const b: RunViewParams = { EntityName: 'MJ: Users', Fields: ['ID', 'Name'] };
            const c: RunViewParams = { EntityName: 'MJ: Users', Fields: ['ID'] };
            expect(RunViewParams.Equals(a, c)).toBe(false);
        it('should compare CacheLocal and CacheLocalTTL', () => {
            const a: RunViewParams = { EntityName: 'MJ: Users', CacheLocal: true, CacheLocalTTL: 5000 };
            const b: RunViewParams = { EntityName: 'MJ: Users', CacheLocal: true, CacheLocalTTL: 5000 };
            const c: RunViewParams = { EntityName: 'MJ: Users', CacheLocal: false };
describe('RunView', () => {
        RunView.Provider = mockProvider as never;
        // Use mockReset + re-set resolved values so the implementation is restored
        mockRunViewFn.mockReset();
        mockRunViewFn.mockResolvedValue({
        mockRunViewsFn.mockReset();
        mockRunViewsFn.mockResolvedValue([{
        it('should use the static provider by default', () => {
            expect(rv.ProviderToUse).toBe(mockProvider);
        it('should use an instance-specific provider when provided', () => {
            const customProvider = {
                RunViews: vi.fn()
            const rv = new RunView(customProvider as never);
            expect(rv.ProviderToUse).toBe(customProvider);
        it('should delegate to the provider', async () => {
            expect(mockRunViewFn).toHaveBeenCalledWith(params, undefined);
            expect(result.Results).toHaveLength(1);
            const params: RunViewParams = { EntityName: 'MJ: Users' };
            const contextUser = { ID: 'u-1' } as never;
            await rv.RunView(params, contextUser);
            expect(mockRunViewFn).toHaveBeenCalledWith(params, contextUser);
    describe('RunViews', () => {
            const params: RunViewParams[] = [
                { EntityName: 'MJ: Users' },
                { EntityName: 'MJ: Roles' }
            await rv.RunViews(params);
            expect(mockRunViewsFn).toHaveBeenCalledWith(params, undefined);
            const newProvider = { RunView: vi.fn(), RunViews: vi.fn() };
            RunView.Provider = newProvider as never;
            expect(RunView.Provider).toBe(newProvider);
            expect(() => RunView.Provider).toThrow('No global object store');
        it('should throw when global store is unavailable (set)', () => {
            expect(() => { RunView.Provider = mockProvider as never; }).toThrow('No global object store');
