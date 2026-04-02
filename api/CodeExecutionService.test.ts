import { CodeExecutionService } from '../CodeExecutionService';
import { CodeExecutionParams, CodeExecutionResult } from '../types';
import { WorkerPool } from '../WorkerPool';
// Mock the WorkerPool to avoid actually forking child processes
vi.mock('../WorkerPool', () => {
  const MockWorkerPool = vi.fn().mockImplementation(function () {
      initialize: vi.fn().mockResolvedValue(undefined),
      execute: vi.fn().mockResolvedValue({
        output: 'test output',
        executionTimeMs: 10
      getStats: vi.fn().mockReturnValue({
        totalWorkers: 2,
        activeWorkers: 2,
        busyWorkers: 0,
        queueLength: 0
      shutdown: vi.fn().mockResolvedValue(undefined)
    WorkerPool: MockWorkerPool
describe('CodeExecutionService', () => {
  let service: CodeExecutionService;
  let mockPoolInstance: ReturnType<typeof getMockPool>;
  function getMockPool() {
    // Access the mocked pool instance through the service
    const pool = (service as Record<string, unknown>)['workerPool'] as {
      initialize: ReturnType<typeof vi.fn>;
      execute: ReturnType<typeof vi.fn>;
      getStats: ReturnType<typeof vi.fn>;
      shutdown: ReturnType<typeof vi.fn>;
    return pool;
    service = new CodeExecutionService();
    mockPoolInstance = getMockPool();
    it('should create a service with default options', () => {
      const svc = new CodeExecutionService();
      expect(svc).toBeDefined();
      expect(WorkerPool).toHaveBeenCalledWith({});
    it('should pass options to WorkerPool', () => {
      const options = { poolSize: 4, maxQueueSize: 50 };
      const svc = new CodeExecutionService(options);
      expect(WorkerPool).toHaveBeenCalledWith(options);
    it('should not be initialized on construction', () => {
      const initialized = (svc as Record<string, unknown>)['initialized'];
      expect(initialized).toBe(false);
  describe('initialize', () => {
    it('should initialize the worker pool', async () => {
      expect(mockPoolInstance.initialize).toHaveBeenCalledOnce();
    it('should set initialized flag to true', async () => {
      const initialized = (service as Record<string, unknown>)['initialized'];
      expect(initialized).toBe(true);
    it('should not initialize twice on repeated calls', async () => {
  describe('execute', () => {
    it('should auto-initialize if not already initialized', async () => {
      const params: CodeExecutionParams = {
        code: 'output = 42;',
      await service.execute(params);
    it('should not re-initialize on second execute call', async () => {
    it('should return error for empty code', async () => {
        code: '',
      const result = await service.execute(params);
      expect(result.error).toContain('Code parameter is required');
      expect(result.errorType).toBe('RUNTIME_ERROR');
    it('should return error for non-string code', async () => {
        code: null,
      } as unknown as CodeExecutionParams;
    it('should return error for unsupported language', async () => {
        code: 'print("hello")',
        language: 'python'
      expect(result.error).toContain('Unsupported language');
      expect(result.error).toContain('python');
    it('should delegate valid requests to worker pool', async () => {
      expect(mockPoolInstance.execute).toHaveBeenCalledWith(params);
      expect(result.output).toBe('test output');
    it('should pass input data to worker pool', async () => {
        code: 'output = input.name;',
        inputData: { name: 'test' }
    it('should pass timeout and memory limit to worker pool', async () => {
        code: 'output = 1;',
        timeoutSeconds: 10,
        memoryLimitMB: 64
    it('should not call execute on worker pool for invalid params', async () => {
      expect(mockPoolInstance.execute).not.toHaveBeenCalled();
  describe('getStats', () => {
    it('should delegate to worker pool getStats', () => {
      const stats = service.getStats();
      expect(mockPoolInstance.getStats).toHaveBeenCalledOnce();
      expect(stats.totalWorkers).toBe(2);
      expect(stats.activeWorkers).toBe(2);
      expect(stats.busyWorkers).toBe(0);
      expect(stats.queueLength).toBe(0);
  describe('shutdown', () => {
    it('should shut down the worker pool when initialized', async () => {
      expect(mockPoolInstance.shutdown).toHaveBeenCalledOnce();
    it('should set initialized to false after shutdown', async () => {
    it('should not shut down if never initialized', async () => {
      expect(mockPoolInstance.shutdown).not.toHaveBeenCalled();
    it('should allow re-initialization after shutdown', async () => {
      expect(mockPoolInstance.initialize).toHaveBeenCalledTimes(2);
