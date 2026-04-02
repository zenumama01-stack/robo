import { BaseActionFilter } from '../generic/BaseActionFilter';
// Concrete implementations for testing
class PassingFilter extends BaseActionFilter {
    protected async InternalRun(): Promise<boolean> {
class FailingFilter extends BaseActionFilter {
class ErrorFilter extends BaseActionFilter {
    protected async InternalRun(): Promise<never> {
        throw new Error('Filter error');
class SpyFilter extends BaseActionFilter {
    public receivedParams: unknown = null;
    public receivedFilter: unknown = null;
    protected async InternalRun(params: unknown, filter: unknown): Promise<boolean> {
        this.receivedParams = params;
        this.receivedFilter = filter;
describe('BaseActionFilter', () => {
        it('should delegate to InternalRun and return true when filter passes', async () => {
            const filter = new PassingFilter();
            const result = await filter.Run({} as never, {} as never);
        it('should delegate to InternalRun and return false when filter fails', async () => {
            const filter = new FailingFilter();
        it('should propagate errors from InternalRun', async () => {
            const filter = new ErrorFilter();
            await expect(filter.Run({} as never, {} as never)).rejects.toThrow('Filter error');
        it('should pass params and filter to InternalRun', async () => {
            const filter = new SpyFilter();
            const params = { Action: { Name: 'Test' } };
            const filterEntity = { ID: 'filter-1', Name: 'TestFilter' };
            await filter.Run(params as never, filterEntity as never);
            expect(filter.receivedParams).toBe(params);
            expect(filter.receivedFilter).toBe(filterEntity);
        it('should be defined as an abstract class', () => {
            expect(BaseActionFilter).toBeDefined();
            expect(typeof BaseActionFilter).toBe('function');
