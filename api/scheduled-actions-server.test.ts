// ---- hoisted mocks ----
vi.mock('mssql', () => {
    class MockConnectionPool {
        connect = vi.fn().mockResolvedValue(undefined);
    return { ConnectionPool: MockConnectionPool };
vi.mock('../db', () => ({
    default: { connect: vi.fn().mockResolvedValue(undefined) },
// Mock config with non-required env vars
    mjCoreSchema: '__mj',
    currentUserEmail: 'test@test.com',
    serverPort: 8000,
    autoRefreshInterval: 3600000,
    SQLServerProviderConfigData: vi.fn(),
    setupSQLServerClient: vi.fn().mockResolvedValue(undefined),
vi.mock('@memberjunction/scheduled-actions', () => ({
    ScheduledActionEngine: {
            ExecuteScheduledActions: vi.fn().mockResolvedValue([]),
            ExecuteScheduledAction: vi.fn().mockResolvedValue({ Success: true }),
// ---- import after mocks ----
import { timeout } from '../util';
describe('timeout utility', () => {
    it('should reject after the specified milliseconds', async () => {
        const promise = timeout(1000);
        vi.advanceTimersByTime(1000);
        await expect(promise).rejects.toThrow('Batch operation timed out');
    it('should not resolve before timeout', async () => {
        let rejected = false;
        const promise = timeout(5000).catch(() => { rejected = true; });
        vi.advanceTimersByTime(4999);
        // Give microtask queue a tick
        await Promise.resolve();
        expect(rejected).toBe(false);
        vi.advanceTimersByTime(1);
        await promise;
        expect(rejected).toBe(true);
