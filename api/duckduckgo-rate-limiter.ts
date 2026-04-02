import { Observable, Subject, from, of, throwError, timer, EMPTY } from 'rxjs';
import { concatMap, delay, retry, catchError, tap, timeout, mergeMap } from 'rxjs/operators';
    queueTimeout: number;
    retryDelay: number;
    resetQueueAfterMs: number;
export interface SearchRequest {
    options: RequestInit;
    disableQueueing?: boolean;
export interface SearchResponse {
    response: Response;
 * Adaptive rate limiter for DuckDuckGo searches
 * - Starts with immediate execution (no queue)
 * - Activates queue on rate limit errors (202, 403, 429)
 * - Automatically deactivates queue after success period or when empty
export class DuckDuckGoRateLimiter {
    private requestQueue$ = new Subject<{
        request: SearchRequest;
        resolver: (value: SearchResponse) => void;
        rejecter: (error: any) => void;
    }>();
    private isQueueActive = false;
    private lastRateLimitTime = 0;
    private resetQueueTimer?: NodeJS.Timeout;
    private queuedRequests = 0;
    private requestCounter = 0;
    private readonly config: RateLimitConfig = {
        maxConcurrent: 1, // When queue is active, serialize requests
        queueTimeout: 30000, // 30 seconds timeout for queued requests
        retryDelay: 2000, // 2 seconds between retries
        maxRetries: 4,
        resetQueueAfterMs: 5000 // Reset queue after 5 seconds of success
    private readonly rateLimitStatusCodes = [202, 403, 429];
    constructor(config?: Partial<RateLimitConfig>) {
        this.config = { ...this.config, ...config };
        this.setupQueueProcessor();
     * Process requests through the queue when active
    private setupQueueProcessor(): void {
        // Only process queued requests when queue is active
            concatMap(({ request, resolver, rejecter, requestId }) => {
                console.log(`        ⚙️  Processing queued request ${requestId} - Active: ${this.activeRequests}, Remaining in queue: ${this.queuedRequests - 1}`);
                return this.executeRequestWithRetries(request, requestId).pipe(
                    tap({
                        next: (result) => {
                            this.queuedRequests--;
                            console.log(`        ✅ Queued request ${requestId} completed - Remaining in queue: ${this.queuedRequests}`);
                            resolver(result);
                            this.checkQueueEmpty();
                        error: (error) => {
                            console.log(`        ❌ Queued request ${requestId} failed: ${error.message} - Remaining in queue: ${this.queuedRequests}`);
                            rejecter(error);
                    catchError(() => EMPTY) // Prevent stream termination
     * Check if queue is empty and deactivate if so
    private checkQueueEmpty(): void {
        if (this.isQueueActive && this.queuedRequests === 0 && this.activeRequests === 0) {
            console.log(`\n        🟢🟢🟢 QUEUE DEACTIVATED 🟢🟢🟢`);
            console.log(`        Queue is empty - returning to immediate execution mode`);
            console.log(`        🟢🟢🟢🟢🟢🟢🟢🟢🟢🟢🟢🟢🟢\n`);
            this.isQueueActive = false;
            if (this.resetQueueTimer) {
                clearTimeout(this.resetQueueTimer);
                this.resetQueueTimer = undefined;
     * Execute a single request attempt
    private async executeSingleRequest(request: SearchRequest, requestId: string): Promise<SearchResponse> {
            const response = await fetch(request.url, request.options);
            const text = await response.text();
            // Check for rate limit status codes
            if (this.rateLimitStatusCodes.includes(response.status)) {
                console.log(`\n        🚨🚨🚨 RATE LIMIT DETECTED 🚨🚨🚨`);
                console.log(`        Request ${requestId} got ${response.status} status`);
                console.log(`        URL: ${request.url.substring(0, 100)}...`);
                console.log(`        Duration: ${duration}ms`);
                console.log(`        🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨🚨\n`);
                throw new Error(`Rate limited: ${response.status}`);
            return { response, text };
     * Execute request with retry logic
    private executeRequestWithRetries(request: SearchRequest, requestId: string): Observable<SearchResponse> {
        return from(this.executeSingleRequest(request, requestId)).pipe(
            timeout(this.config.queueTimeout),
            retry({
                count: this.config.maxRetries,
                delay: (error, retryCount) => {
                    attemptCount = retryCount;
                    // Check if it's a rate limit error
                    const isRateLimit = error.message?.includes('Rate limited');
                    if (isRateLimit) {
                        const delayMs = this.config.retryDelay * Math.pow(2, retryCount - 1);
                        console.log(`        ⏳ Rate limit retry ${retryCount}/${this.config.maxRetries} - waiting ${delayMs}ms before retry`);
                        return timer(delayMs);
                    // For other errors, retry immediately
                    return timer(0);
                next: () => {
                    if (attemptCount > 0) {
                        console.log(`        ✅ Request ${requestId} succeeded after ${attemptCount} retries`);
                    this.scheduleQueueReset();
            catchError((error) => {
                if (error.message?.includes('Rate limited')) {
                    console.log(`        ❌ Request ${requestId} failed after all retries - still rate limited`);
                return throwError(() => error);
     * Activate the request queue due to rate limiting
    private activateQueue(): void {
        if (!this.isQueueActive) {
            console.log(`\n        🔴🔴🔴 QUEUE ACTIVATED 🔴🔴🔴`);
            console.log(`        Rate limiting detected - all future requests will be queued`);
            console.log(`        Queue will deactivate when empty or after ${this.config.resetQueueAfterMs}ms of success`);
            console.log(`        🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴🔴\n`);
            this.isQueueActive = true;
            this.lastRateLimitTime = Date.now();
        // Cancel any pending queue reset
     * Schedule queue deactivation after successful requests
    private scheduleQueueReset(): void {
        if (!this.isQueueActive) return;
        // Clear existing timer
        // Schedule queue reset
        this.resetQueueTimer = setTimeout(() => {
            if (Date.now() - this.lastRateLimitTime >= this.config.resetQueueAfterMs) {
                console.log(`\n        🟢🟢🟢 QUEUE DEACTIVATED (TIMEOUT) 🟢🟢🟢`);
                console.log(`        No rate limits for ${this.config.resetQueueAfterMs}ms - returning to immediate execution`);
                console.log(`        🟢🟢🟢🟢🟢🟢🟢🟢🟢🟢🟢🟢🟢🟢🟢🟢\n`);
        }, this.config.resetQueueAfterMs);
     * Execute a search request through the rate limiter
    async search(request: SearchRequest): Promise<SearchResponse> {
        const requestId = `REQ-${++this.requestCounter}`;
            // If queueing is disabled and queue is active, fail immediately
            if (request.disableQueueing && this.isQueueActive) {
                console.log(`        🚫 Request ${requestId} rejected - rate limiting active and queueing disabled`);
                reject(new Error('Rate limiting active and queueing is disabled'));
            // If queue is not active, execute immediately
                this.executeRequestWithRetries(request, requestId)
                    .subscribe({
                            // Check if we got rate limited
                                console.log(`        🔄 Request ${requestId} was rate limited, activating queue for future requests`);
                                this.activateQueue();
                // Queue is active, add to queue
                console.log(`        📥 Request ${requestId} added to queue (position: ${this.queuedRequests + 1})`);
                this.queuedRequests++;
                    resolver: resolve,
                    rejecter: reject,
                    requestId
     * Get current queue status
        isQueueActive: boolean; 
        activeRequests: number;
        queuedRequests: number;
        totalProcessed: number;
        lastRateLimitTime: number;
            isQueueActive: this.isQueueActive,
            activeRequests: this.activeRequests,
            queuedRequests: this.queuedRequests,
            totalProcessed: this.requestCounter,
            lastRateLimitTime: this.lastRateLimitTime
     * Force queue activation (for testing)
    forceActivateQueue(): void {
     * Force queue deactivation (for testing)
    forceDeactivateQueue(): void {
// Singleton instance
let rateLimiterInstance: DuckDuckGoRateLimiter | null = null;
 * Get the singleton rate limiter instance
export function getDuckDuckGoRateLimiter(config?: Partial<RateLimitConfig>): DuckDuckGoRateLimiter {
    if (!rateLimiterInstance) {
        rateLimiterInstance = new DuckDuckGoRateLimiter(config);
    return rateLimiterInstance;
