import { WorkerPool, WorkerPoolOptions } from '../WorkerPool';
import { CodeExecutionParams } from '../types';
import { EventEmitter } from 'events';
// Mock @memberjunction/core
  LogStatus: vi.fn()
// Create a mock ChildProcess class that extends EventEmitter
class MockChildProcess extends EventEmitter {
  killed = false;
  pid = Math.floor(Math.random() * 10000);
  send = vi.fn().mockReturnValue(true);
  kill = vi.fn().mockImplementation((signal?: string) => {
    this.killed = true;
    // Simulate async exit
    setImmediate(() => this.emit('exit', signal === 'SIGKILL' ? 1 : 0, signal || 'SIGTERM'));
  // Simulate sending ready message after creation
  simulateReady() {
    setImmediate(() => this.emit('message', { type: 'ready' }));
  // Simulate sending a result message
  simulateResult(result: Record<string, unknown>) {
    this.emit('message', { type: 'result', result });
  // Simulate sending an error message
  simulateError(error: string) {
    this.emit('message', { type: 'error', error });
  // Simulate crash (exit with non-zero)
  simulateCrash(code = 1, signal: string | null = null) {
    this.emit('exit', code, signal);
// Mock child_process.fork
vi.mock('child_process', () => ({
  fork: vi.fn()
import { fork } from 'child_process';
const mockedFork = vi.mocked(fork);
describe('WorkerPool', () => {
  let pool: WorkerPool;
  let mockProcesses: MockChildProcess[];
    mockProcesses = [];
    // Set up fork to create mock child processes that auto-send ready
    mockedFork.mockImplementation(() => {
      const proc = new MockChildProcess();
      mockProcesses.push(proc);
      proc.simulateReady();
      return proc as unknown as ReturnType<typeof fork>;
  afterEach(async () => {
    // Clean up any active pools
    if (pool) {
        await pool.shutdown();
        // Ignore shutdown errors in cleanup
    it('should accept default options', () => {
      pool = new WorkerPool();
      expect(pool).toBeDefined();
    it('should accept custom pool size', () => {
      pool = new WorkerPool({ poolSize: 4 });
    it('should accept all configuration options', () => {
      const options: WorkerPoolOptions = {
        poolSize: 3,
        maxCrashesPerWorker: 5,
        crashTimeWindow: 120000,
        maxQueueSize: 200
      pool = new WorkerPool(options);
    it('should create the expected number of worker processes', async () => {
      pool = new WorkerPool({ poolSize: 3 });
      await pool.initialize();
      expect(mockedFork).toHaveBeenCalledTimes(3);
    it('should create 2 workers by default', async () => {
      expect(mockedFork).toHaveBeenCalledTimes(2);
    it('should fork worker.js file', async () => {
      pool = new WorkerPool({ poolSize: 1 });
      expect(mockedFork).toHaveBeenCalledWith(
        expect.stringContaining('worker.js'),
          stdio: ['ignore', 'inherit', 'inherit', 'ipc']
    it('should wait for ready message from each worker', async () => {
      pool = new WorkerPool({ poolSize: 2 });
      // If we got here without hanging, workers sent ready messages
      expect(mockProcesses).toHaveLength(2);
    it('should send execute message to an available worker', async () => {
      // Set up to resolve when the worker sends back a result
      const executePromise = pool.execute(params);
      // Find the worker that received the message
      const calledProc = mockProcesses.find(p => p.send.mock.calls.length > 0);
      expect(calledProc).toBeDefined();
      // Verify the sent message
      const sentMessage = calledProc!.send.mock.calls[0][0] as Record<string, unknown>;
      expect(sentMessage.type).toBe('execute');
      expect(sentMessage.params).toEqual(params);
      // Simulate the result coming back
      calledProc!.simulateResult({ success: true, output: 42 });
      const result = await executePromise;
      expect(result).toEqual({ success: true, output: 42 });
    it('should return error when pool is shutting down', async () => {
      // Trigger shutdown
      const shutdownPromise = pool.shutdown();
      const result = await pool.execute(params);
      expect(result.error).toContain('shutting down');
      await shutdownPromise;
    it('should return error when queue is full', async () => {
      pool = new WorkerPool({ poolSize: 1, maxQueueSize: 1 });
      // Make the single worker busy
      const params1: CodeExecutionParams = { code: 'output = 1;', language: 'javascript' };
      pool.execute(params1).catch(() => { /* will be rejected on shutdown */ }); // First request takes the worker
      // Second request should be queued
      const params2: CodeExecutionParams = { code: 'output = 2;', language: 'javascript' };
      pool.execute(params2).catch(() => { /* will be resolved/rejected on shutdown */ }); // Fills the queue
      // Third request should fail because queue is full
      const params3: CodeExecutionParams = { code: 'output = 3;', language: 'javascript' };
      const result = await pool.execute(params3);
      expect(result.error).toContain('queue is full');
    it('should use default timeout of 30 seconds with 5s buffer', async () => {
        // No timeoutSeconds specified -> defaults to 30
      // Resolve immediately to avoid waiting for actual timeout
      calledProc!.simulateResult({ success: true, output: 1 });
    it('should pass custom timeout to request', async () => {
        timeoutSeconds: 10
    it('should report correct stats before initialization', () => {
      const stats = pool.getStats();
      expect(stats.totalWorkers).toBe(3);
      expect(stats.activeWorkers).toBe(0);
    it('should report correct stats after initialization', async () => {
    it('should report busy workers correctly', async () => {
      // Start an execution without resolving it
      pool.execute(params).catch(() => { /* will be rejected on shutdown */ }); // Don't await - leave worker busy
      expect(stats.busyWorkers).toBe(1);
    it('should reject queued requests on shutdown', async () => {
      // Make worker busy
      pool.execute(params1).catch(() => { /* will be rejected when worker crashes during shutdown */ });
      // Queue another request
      const queuedPromise = pool.execute(params2);
      // Shutdown
      // The queued request should have been resolved with error
      const result = await queuedPromise;
    it('should kill all worker processes', async () => {
      for (const proc of mockProcesses) {
        expect(proc.kill).toHaveBeenCalled();
    it('should handle workers that are already killed', async () => {
      // Pre-kill the worker
      mockProcesses[0].killed = true;
      // Should not throw
      await expect(pool.shutdown()).resolves.not.toThrow();
  describe('worker crash handling', () => {
    it('should handle worker crash during execution', async () => {
      pool = new WorkerPool({ poolSize: 1, maxCrashesPerWorker: 3 });
        code: 'while(true) {}',
      // Simulate the worker crashing
      mockProcesses[0].simulateCrash(1, 'SIGSEGV');
      // The promise should reject because the worker crashed
      await expect(executePromise).rejects.toThrow('Worker process crashed');
    it('should attempt to restart crashed workers', async () => {
      pool = new WorkerPool({ poolSize: 1, maxCrashesPerWorker: 5 });
      const initialForkCount = mockedFork.mock.calls.length;
      // Simulate crash (not during execution)
      mockProcesses[0].simulateCrash(1, null);
      // Wait for restart attempt
      await new Promise(resolve => setTimeout(resolve, 50));
      // Should have tried to create a new worker
      expect(mockedFork.mock.calls.length).toBeGreaterThan(initialForkCount);
