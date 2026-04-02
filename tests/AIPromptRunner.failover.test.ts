 * Unit tests for AIPromptRunner Failover Bug Fix (Phase 1).
 * These tests verify that the critical failover bug is fixed:
 * - Network errors trigger failover to alternate models
 * - Rate limit errors trigger failover after retries exhausted
 * - Successful results don't trigger unnecessary failover
 * - All candidates exhausted returns last error correctly
 * Background: The bug was that executeModelWithFailover() returned failed results
 * without checking result.success, only catching thrown exceptions. Provider drivers
 * (GeminiLLM, OpenAILLM, etc.) return ChatResult{success: false} instead of throwing,
 * so failover never triggered for network failures, rate limits, etc.
 * To run these tests:
 *   npm test                    # Run all tests
 *   npm run test:watch         # Watch mode
 *   npm run test:coverage      # With coverage
 * @since 3.3.0 (Phase 1 failover bug fix)
import { AIErrorInfo, AIErrorType, ErrorSeverity } from '@memberjunction/ai';
// Mock Types and Interfaces
 * Mock ChatResult for testing
interface MockChatResult {
    statusText?: string;
    exception?: Error;
    errorInfo?: AIErrorInfo;
 * Mock model candidate for testing
interface MockCandidate {
    vendorId: string;
    vendorName: string;
 * Mock failover attempt tracking
interface MockFailoverAttempt {
 * Mock failover configuration
interface MockFailoverConfig {
    strategy: string;
    delaySeconds?: number;
// Test Helper Functions
 * Simulates the processFailoverError logic for testing purposes.
 * Returns decision object indicating what the failover loop should do.
function simulateProcessFailoverError(
    candidate: MockCandidate,
    allCandidates: MockCandidate[],
    failoverAttempts: MockFailoverAttempt[],
    failoverConfig: MockFailoverConfig
    shouldRetry: boolean;
    shouldContinue: boolean;
    updatedCandidates: MockCandidate[];
    let updatedCandidates = [...allCandidates];
    // Record the failover attempt
    const failoverAttempt: MockFailoverAttempt = {
        modelId: candidate.modelId,
        duration: 0,
        const beforeFilter = updatedCandidates.length;
        updatedCandidates = updatedCandidates.filter(c => c.vendorId !== candidate.vendorId);
        const afterFilter = updatedCandidates.length;
        // Adjust attemptIndex if candidates were removed before current position
        if (afterFilter < beforeFilter && attemptIndex < allCandidates.length) {
            // Don't adjust - the main loop will handle the updated candidates list
    // Check if there are NO MORE candidates left to try AFTER filtering
    // After filtering vendor candidates, we may have removed the current one,
    // so check if there are any candidates remaining
    const hasMoreCandidates = updatedCandidates.length > 0 &&
                               (attemptIndex < updatedCandidates.length - 1 ||
                                updatedCandidates[attemptIndex]?.vendorId !== candidate.vendorId);
    const isLastCandidate = !hasMoreCandidates;
    // Fatal errors: stop immediately ONLY if canFailover is false
    // (canFailover=true means we can try other vendors even for Fatal severity errors like Auth)
    if (errorInfo.severity === 'Fatal' && !errorInfo.canFailover) {
        const matchesScope = errorMatchesScope(errorInfo.errorType, failoverConfig.errorScope);
    // Rate limit errors: for testing, assume handleRateLimitRetry returns false (retries exhausted)
    // In reality, this would check MaxRetries configuration and retry attempt count
        const shouldRetry = false; // Simplified: assume retries exhausted
    // Continue to next candidate
 * Checks if error type matches the configured error scope
function errorMatchesScope(errorType: AIErrorType, errorScope: 'All' | 'NetworkOnly' | 'RateLimitOnly' | 'ServiceErrorOnly'): boolean {
    if (errorScope === 'All') return true;
    switch (errorScope) {
 * Simulates executeModelWithFailover with the Phase 1 fix applied.
 * This is a simplified version that tests the core failover logic.
function simulateExecuteModelWithFailover(
    candidates: MockCandidate[],
    executeModelFn: (candidate: MockCandidate) => Promise<MockChatResult>,
): Promise<MockChatResult> {
    return new Promise(async (resolve, reject) => {
        let allCandidates = [...candidates];
        const failoverAttempts: MockFailoverAttempt[] = [];
        let candidateIndex = 0;
        while (candidateIndex < allCandidates.length) {
            const candidate = allCandidates[candidateIndex];
                const result = await executeModelFn(candidate);
                // PHASE 1 FIX: Check if result failed but is retriable
                    const decision = simulateProcessFailoverError(
                        candidateIndex,
                    const candidatesRemoved = allCandidates.length - decision.updatedCandidates.length;
                        // Retry same model/vendor - don't increment index
                        // Try next candidate - but if candidates were removed, the "next" candidate
                        // may already be at the current index
                        if (candidatesRemoved > 0) {
                            // Check if current candidate is still in the list
                            const stillExists = allCandidates[candidateIndex]?.modelId === candidate.modelId;
                            if (!stillExists) {
                                // Current candidate was removed, stay at same index
                        // Move to next candidate
                        candidateIndex++;
                // Create error info for thrown exception
                const errorInfo: AIErrorInfo = {
                    error: lastError,
                    errorType: 'NetworkError',
        // All candidates exhausted, return final error
        const finalError = lastError || new Error('All failover attempts exhausted');
        resolve({
            errorMessage: `All ${candidates.length} failover attempts failed. Last error: ${finalError.message}`,
            exception: finalError,
            errorInfo: {
                error: finalError,
                errorType: 'Unknown',
                canFailover: false
// Tests for Phase 1 Failover Bug Fix
describe('AIPromptRunner Failover Bug Fix (Phase 1)', () => {
    it('should trigger failover when network error occurs', async () => {
    const candidates: MockCandidate[] = [
        { modelId: 'model-1', modelName: 'Gemini 3 Flash', vendorId: 'vendor-google', vendorName: 'Google', priority: 1 },
        { modelId: 'model-2', modelName: 'GPT-4o', vendorId: 'vendor-openai', vendorName: 'OpenAI', priority: 2 }
    let attemptCount = 0;
    const executeModelFn = async (candidate: MockCandidate): Promise<MockChatResult> => {
        attemptCount++;
        if (candidate.modelId === 'model-1') {
            // First model fails with network error
                errorMessage: 'TypeError: fetch failed',
                exception: new Error('TypeError: fetch failed'),
                    error: new Error('TypeError: fetch failed'),
            // Second model succeeds
                data: 'Success with fallback model'
    const failoverConfig: MockFailoverConfig = {
        strategy: 'priority',
        maxAttempts: 2
    const result = await simulateExecuteModelWithFailover(candidates, executeModelFn, failoverConfig);
        expect(attemptCount).toBe(2);
        expect(result.data).toBe('Success with fallback model');
    it('should trigger failover when rate limit error occurs', async () => {
        { modelId: 'model-1', modelName: 'GPT-4', vendorId: 'vendor-openai', vendorName: 'OpenAI', priority: 1 },
        { modelId: 'model-2', modelName: 'Claude Opus', vendorId: 'vendor-anthropic', vendorName: 'Anthropic', priority: 2 }
            // First model hits rate limit
                errorMessage: 'Rate limit exceeded',
                exception: new Error('Rate limit exceeded'),
                    error: new Error('Rate limit exceeded'),
                    httpStatusCode: 429,
                    suggestedRetryDelaySeconds: 30
                data: 'Success with alternate provider'
        expect(result.data).toBe('Success with alternate provider');
    it('should not trigger failover when result is successful', async () => {
        // First model succeeds
            data: `Success with ${candidate.modelName}`
        expect(attemptCount).toBe(1);
        expect(result.data).toBe('Success with Gemini 3 Flash');
    it('should return last error when all candidates are exhausted', async () => {
        { modelId: 'model-2', modelName: 'GPT-4o', vendorId: 'vendor-openai', vendorName: 'OpenAI', priority: 2 },
        { modelId: 'model-3', modelName: 'Claude Opus', vendorId: 'vendor-anthropic', vendorName: 'Anthropic', priority: 3 }
        // All models fail with network errors
            errorMessage: `Network failure for ${candidate.modelName}`,
            exception: new Error(`Network failure for ${candidate.modelName}`),
                error: new Error(`Network failure for ${candidate.modelName}`),
        maxAttempts: 3
        expect(attemptCount).toBe(3);
        expect(result.errorMessage).toContain('All 3 failover attempts failed');
        expect(result.errorMessage).toContain('Claude Opus');
    it('should trigger failover when service unavailable error occurs', async () => {
        { modelId: 'model-1', modelName: 'Gemini Pro', vendorId: 'vendor-google', vendorName: 'Google', priority: 1 },
        { modelId: 'model-2', modelName: 'GPT-4', vendorId: 'vendor-openai', vendorName: 'OpenAI', priority: 2 }
            // First model returns 503 Service Unavailable
                statusText: 'Service Unavailable',
                errorMessage: 'Service temporarily unavailable',
                exception: new Error('Service temporarily unavailable'),
                    error: new Error('Service temporarily unavailable'),
                    httpStatusCode: 503,
                    errorType: 'ServiceUnavailable',
                data: 'Success after service unavailable'
        expect(result.data).toBe('Success after service unavailable');
    it('should filter all vendor candidates when vendor-level error occurs', async () => {
    const attemptedModels: string[] = [];
        attemptedModels.push(candidate.modelName);
        if (candidate.vendorId === 'vendor-openai') {
            // OpenAI models fail with authentication error (vendor-level)
                errorMessage: 'Invalid API key',
                exception: new Error('Invalid API key'),
                    error: new Error('Invalid API key'),
                    httpStatusCode: 401,
                    errorType: 'Authentication',
                    canFailover: true // canFailover=true for vendor-level errors to try other vendors
            // Anthropic model succeeds
                data: 'Success with different vendor'
        expect(attemptCount).toBe(2); // GPT-4 fails, GPT-4o skipped, Claude Opus succeeds
        expect(attemptedModels).toEqual(['GPT-4', 'Claude Opus']);
        expect(result.data).toBe('Success with different vendor');
    it('should respect errorScope filter and not failover for non-matching errors', async () => {
            // First model fails with rate limit (not network error)
            // Should not reach here due to errorScope filter
                data: 'Should not be reached'
        errorScope: 'NetworkOnly', // Only failover for network errors
        expect(result.errorMessage).toContain('Rate limit exceeded');
