 * Worker Pool Manager
 * Manages a pool of worker processes for executing code with fault isolation.
 * Each worker runs in a separate OS process, so a catastrophic failure in one
 * worker (e.g., V8 OOM crash) doesn't affect the main application or other workers.
 * - Process-level isolation for fault tolerance
 * - Automatic worker restart on crashes
 * - Request queuing when all workers are busy
 * - Health monitoring and circuit breaker pattern
import { fork, ChildProcess } from 'child_process';
const __dirname = path.dirname(__filename);
interface PendingRequest {
    params: CodeExecutionParams;
    resolve: (result: CodeExecutionResult) => void;
    timeout: NodeJS.Timeout;
interface Worker {
    process: ChildProcess;
    id: number;
    busy: boolean;
    currentRequest: PendingRequest | null;
    crashCount: number;
    lastCrashTime: number;
 * Configuration options for worker pool
export interface WorkerPoolOptions {
    /** Number of worker processes to maintain (default: 2) */
    poolSize?: number;
    /** Maximum crashes per worker before marking unhealthy (default: 3) */
    maxCrashesPerWorker?: number;
    /** Time window in ms for crash counting (default: 60000 = 1 minute) */
    crashTimeWindow?: number;
    /** Maximum queue size before rejecting requests (default: 100) */
    maxQueueSize?: number;
 * Manages a pool of worker processes for code execution
export class WorkerPool {
    private workers: Worker[] = [];
    private requestQueue: PendingRequest[] = [];
    private nextRequestId = 0;
    private poolSize: number;
    private maxCrashesPerWorker: number;
    private crashTimeWindow: number;
    private maxQueueSize: number;
    private isShuttingDown = false;
        this.poolSize = options.poolSize || 2;
        this.maxCrashesPerWorker = options.maxCrashesPerWorker || 3;
        this.crashTimeWindow = options.crashTimeWindow || 60000; // 1 minute
        this.maxQueueSize = options.maxQueueSize || 100;
     * Initialize the worker pool
        LogStatus('Initializing code execution worker pool with ' + this.poolSize + ' workers');
        for (let i = 0; i < this.poolSize; i++) {
            await this.createWorker(i);
     * Create a new worker process
    private async createWorker(id: number): Promise<void> {
        const workerPath = path.join(__dirname, 'worker.js'); // Use .js because worker.ts is compiled
        const childProcess = fork(workerPath, [], {
            stdio: ['ignore', 'inherit', 'inherit', 'ipc'] // Inherit stdout/stderr for debugging
        const worker: Worker = {
            process: childProcess,
            busy: false,
            currentRequest: null,
            crashCount: 0,
            lastCrashTime: 0
        this.workers[id] = worker;
        // Wait for ready message
        await new Promise<void>((resolve, reject) => {
            const timeout = setTimeout(() => {
                reject(new Error(`Worker ${id} failed to start within 5 seconds`));
            }, 5000);
            childProcess.once('message', (msg: any) => {
                if (msg.type === 'ready') {
                    clearTimeout(timeout);
            childProcess.once('error', (error) => {
        // Set up message handler for responses
        childProcess.on('message', (message: any) => {
            this.handleWorkerMessage(worker, message);
        // Set up crash handler
        childProcess.on('exit', (code, signal) => {
            this.handleWorkerCrash(worker, code, signal);
        LogStatus(`Worker ${id} initialized and ready`);
     * Handle messages from worker process
    private handleWorkerMessage(worker: Worker, message: any): void {
        if (message.type === 'ready') {
            // Worker is ready (initial startup or after restart)
        if (!worker.currentRequest) {
            LogError(new Error(`Worker ${worker.id} sent message but has no current request`));
        const request = worker.currentRequest;
        clearTimeout(request.timeout);
        if (message.type === 'result') {
            request.resolve(message.result);
        } else if (message.type === 'error') {
            request.reject(new Error(message.error));
        // Mark worker as available and process queue
        worker.busy = false;
        worker.currentRequest = null;
     * Handle worker process crash
    private handleWorkerCrash(worker: Worker, code: number | null, signal: string | null): void {
        // Update crash tracking
        if (now - worker.lastCrashTime > this.crashTimeWindow) {
            // Outside time window, reset count
            worker.crashCount = 1;
            worker.crashCount++;
        worker.lastCrashTime = now;
        LogError(new Error(
            `Worker ${worker.id} crashed (code: ${code}, signal: ${signal}). ` +
            `Crash count: ${worker.crashCount}/${this.maxCrashesPerWorker}`
        // Reject current request if any
        if (worker.currentRequest) {
            clearTimeout(worker.currentRequest.timeout);
            worker.currentRequest.reject(new Error(
                'Worker process crashed during code execution. This may indicate a severe error in the code.'
        // Check if worker should be restarted
        if (worker.crashCount >= this.maxCrashesPerWorker) {
                `Worker ${worker.id} has crashed ${worker.crashCount} times within ${this.crashTimeWindow}ms. ` +
                'Not restarting to prevent crash loop. Code execution capacity reduced.'
            // Remove from pool
            this.workers[worker.id] = null as any;
        // Restart worker if not shutting down
        if (!this.isShuttingDown) {
            LogStatus(`Restarting worker ${worker.id}...`);
            this.createWorker(worker.id).catch(error => {
                LogError(new Error(`Failed to restart worker ${worker.id}: ${error.message}`));
     * Execute code in an available worker
        if (this.isShuttingDown) {
                error: 'Worker pool is shutting down',
        // Check if queue is full
        if (this.requestQueue.length >= this.maxQueueSize) {
                error: `Request queue is full (${this.maxQueueSize} pending requests). Please try again later.`,
        const requestId = `req-${this.nextRequestId++}`;
        return new Promise<CodeExecutionResult>((resolve, reject) => {
            // Create timeout for entire request (includes queue time + execution)
            const totalTimeout = ((params.timeoutSeconds || 30) + 5) * 1000; // Add 5s buffer for overhead
                // Remove from queue if still there
                const queueIndex = this.requestQueue.findIndex(r => r.requestId === requestId);
                if (queueIndex >= 0) {
                    this.requestQueue.splice(queueIndex, 1);
                    error: 'Request timed out waiting for worker availability',
                    errorType: 'TIMEOUT'
            }, totalTimeout);
            const request: PendingRequest = {
                resolve,
                timeout
            // Add to queue
            this.requestQueue.push(request);
            // Try to process immediately
     * Process queued requests by assigning them to available workers
        if (this.requestQueue.length === 0) {
        // Find an available worker
        const worker = this.workers.find(w => w && !w.busy);
        if (!worker) {
            return; // All workers busy, wait for one to finish
        // Get next request from queue
        const request = this.requestQueue.shift();
        // Assign request to worker
        worker.busy = true;
        worker.currentRequest = request;
        // Send execution request to worker
        worker.process.send({
            type: 'execute',
            requestId: request.requestId,
            params: request.params
        // Try to process more if queue has items
        if (this.requestQueue.length > 0) {
            setImmediate(() => this.processQueue());
     * Get pool statistics
    getStats(): {
        totalWorkers: number;
        activeWorkers: number;
        busyWorkers: number;
        const activeWorkers = this.workers.filter(w => w !== null).length;
        const busyWorkers = this.workers.filter(w => w && w.busy).length;
            totalWorkers: this.poolSize,
            activeWorkers,
            busyWorkers,
            queueLength: this.requestQueue.length
     * Shutdown the worker pool gracefully
        this.isShuttingDown = true;
        LogStatus('Shutting down code execution worker pool...');
        for (const request of this.requestQueue) {
            request.resolve({
        this.requestQueue = [];
        // Kill all workers
        const killPromises = this.workers
            .filter(w => w !== null)
            .map(worker => {
                return new Promise<void>((resolve) => {
                    if (worker.process.killed) {
                    worker.process.once('exit', () => resolve());
                    worker.process.kill('SIGTERM');
                    // Force kill after 5 seconds
                        if (!worker.process.killed) {
                            worker.process.kill('SIGKILL');
        await Promise.all(killPromises);
        LogStatus('Worker pool shutdown complete');
