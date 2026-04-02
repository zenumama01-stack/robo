    default: { hostname: () => 'test-host' },
    hostname: () => 'test-host'
    UserInfo: class UserInfo { ID = 'user-1' },
            ScheduledJobID: '',
            ExecutedByUserID: '',
            StartedAt: null,
            QueuedAt: null,
            ID: 'run-123'
    IsVerboseLoggingEnabled: vi.fn(() => false)
    MJScheduledJobEntity: class {
        CronExpression = '';
        Timezone = 'UTC';
        NextRunAt: Date | null = null;
        StartAt: Date | null = null;
        EndAt: Date | null = null;
        LockToken: string | null = null;
        LockedAt: Date | null = null;
        LockedByInstance: string | null = null;
        ExpectedCompletionAt: Date | null = null;
        ConcurrencyMode = 'Skip';
        RunCount = 0;
        SuccessCount = 0;
        FailureCount = 0;
        LastRunAt: Date | null = null;
        JobTypeID = '';
        NotifyOnSuccess = false;
        NotifyOnFailure = false;
        NotifyUserID: string | null = null;
        OwnerUserID: string | null = null;
        NotifyViaEmail = false;
        NotifyViaInApp = false;
        Configuration = '';
    MJScheduledJobRunEntity: class {
        ID = 'run-123';
        ScheduledJobID = '';
        ExecutedByUserID = '';
        CompletedAt: Date | null = null;
        QueuedAt: Date | null = null;
        Details: string | null = null;
vi.mock('@memberjunction/scheduling-engine-base', () => ({
    SchedulingEngineBase: class {
        private static _instances = new Map<Function, unknown>();
            const ctor = this as unknown as new () => T;
            if (!this._instances.has(ctor)) {
                this._instances.set(ctor, new ctor());
            return this._instances.get(ctor) as T;
        ScheduledJobs: Array<Record<string, unknown>> = [];
        ScheduledJobTypes: Array<Record<string, unknown>> = [];
        ActivePollingInterval: number | null = 60000;
        Config = vi.fn().mockResolvedValue(undefined);
        UpdatePollingInterval = vi.fn();
vi.mock('../CronExpressionHelper', () => ({
    CronExpressionHelper: {
        GetNextRunTime: vi.fn().mockReturnValue(new Date('2025-06-01T00:00:00Z')),
        IsExpressionDue: vi.fn().mockReturnValue(false)
vi.mock('../NotificationManager', () => ({
    NotificationManager: {
        SendScheduledJobNotification: vi.fn().mockResolvedValue(undefined)
import { SchedulingEngine } from '../ScheduledJobEngine';
describe('SchedulingEngine', () => {
    let engine: SchedulingEngine;
        // Access the singleton and reset its state
        engine = SchedulingEngine.Instance;
        // Stop any existing polling
        engine.StopPolling();
            const instance1 = SchedulingEngine.Instance;
            const instance2 = SchedulingEngine.Instance;
        it('should be an instance of SchedulingEngine', () => {
            expect(SchedulingEngine.Instance).toBeInstanceOf(SchedulingEngine);
    describe('IsPolling', () => {
            expect(engine.IsPolling).toBe(false);
    describe('StopPolling', () => {
        it('should do nothing when not polling', () => {
        it('should set IsPolling to false when called', () => {
            // Start polling first
            const mockUser = { ID: 'user-1' } as Parameters<typeof engine.StartPolling>[0];
            engine.StartPolling(mockUser);
            expect(engine.IsPolling).toBe(true);
    describe('StartPolling', () => {
        it('should set IsPolling to true', () => {
        it('should not start double polling if already polling', () => {
            const firstPollingState = engine.IsPolling;
            // Calling again should be a no-op
            expect(engine.IsPolling).toBe(firstPollingState);
    describe('ExecuteScheduledJobs', () => {
        it('should return an empty array when no jobs are due', async () => {
            const mockUser = { ID: 'user-1' } as Parameters<typeof engine.ExecuteScheduledJobs>[0];
            engine.ScheduledJobs = [];
            const runs = await engine.ExecuteScheduledJobs(mockUser);
            expect(runs).toEqual([]);
        it('should call Config before executing', async () => {
            await engine.ExecuteScheduledJobs(mockUser);
            expect(engine.Config).toHaveBeenCalledWith(false, mockUser);
        it('should update polling interval after execution', async () => {
            expect(engine.UpdatePollingInterval).toHaveBeenCalled();
        it('should accept a custom evalTime', async () => {
            const evalTime = new Date('2025-06-15T12:00:00Z');
            const runs = await engine.ExecuteScheduledJobs(mockUser, evalTime);
    describe('ExecuteScheduledJob', () => {
        it('should throw when job is not found', async () => {
            const mockUser = { ID: 'user-1' } as Parameters<typeof engine.ExecuteScheduledJob>[1];
                engine.ExecuteScheduledJob('nonexistent-id', mockUser)
            ).rejects.toThrow('not found or not active');
        it('should throw with the job ID in the error message', async () => {
                engine.ExecuteScheduledJob('abc-123', mockUser)
            ).rejects.toThrow('abc-123');
    describe('OnJobChanged', () => {
        it('should reload configuration', async () => {
            const mockUser = { ID: 'user-1' } as Parameters<typeof engine.OnJobChanged>[0];
            await engine.OnJobChanged(mockUser);
            expect(engine.Config).toHaveBeenCalledWith(true, mockUser);
        it('should recalculate polling interval', async () => {
