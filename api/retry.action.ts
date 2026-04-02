 * Action that retries failed actions with exponential backoff
 * // Basic retry with defaults
 *   ActionName: 'Retry',
 *       ActionName: 'HTTP Request',
 *       Params: { URL: 'https://api.example.com/data' }
 * // Advanced retry with custom configuration
 *       ActionName: 'Database Query',
 *       Params: { Query: 'SELECT * FROM orders' }
 *     Name: 'MaxRetries',
 *     Name: 'RetryDelay',
 *     Value: 2000
 *     Name: 'BackoffMultiplier',
 *     Value: 2
 *     Name: 'RetryOn',
 *     Value: ['TIMEOUT', 'CONNECTION_ERROR']
 *     Name: 'GiveUpOn',
 *     Value: ['INVALID_CREDENTIALS', 'NOT_FOUND']
@RegisterClass(BaseAction, "Retry")
export class RetryAction extends BaseAction {
     * Retries failed actions with exponential backoff
     *   - Action: Action configuration to retry (required)
     *   - MaxRetries: Maximum number of retry attempts (default: 3)
     *   - RetryDelay: Initial delay in milliseconds (default: 1000)
     *   - BackoffMultiplier: Multiplier for exponential backoff (default: 2)
     *   - MaxDelay: Maximum delay between retries in ms (default: 30000)
     *   - UseJitter: Add random jitter to delays (default: true)
     *   - RetryOn: Array of error codes to retry on (optional - retries all by default)
     *   - GiveUpOn: Array of error codes to not retry (optional)
     * @returns Result of the successful action or final failure
            const action = this.getParamValue(params, 'action');
            const retryDelay = this.getNumericParam(params, 'retrydelay', 1000);
            const backoffMultiplier = this.getNumericParam(params, 'backoffmultiplier', 2);
            const maxDelay = this.getNumericParam(params, 'maxdelay', 30000);
            const useJitter = this.getBooleanParam(params, 'usejitter', true);
            const retryOn = this.getParamValue(params, 'retryon');
            const giveUpOn = this.getParamValue(params, 'giveupon');
            if (!action.ActionName) {
            // Validate numeric parameters
            if (maxRetries < 0) {
                    Message: "MaxRetries must be non-negative",
                    ResultCode: "INVALID_MAX_RETRIES"
            if (retryDelay < 0) {
                    Message: "RetryDelay must be non-negative",
                    ResultCode: "INVALID_RETRY_DELAY"
            if (backoffMultiplier < 1) {
                    Message: "BackoffMultiplier must be at least 1",
                    ResultCode: "INVALID_BACKOFF_MULTIPLIER"
            // Prepare action configuration
            const preparedAction = {
                ...action,
                Params: action.Params || []
            // Create action engine
            // Track attempts
            const attempts: any[] = [];
            let lastResult: ActionResultSimple;
            // Execute with retries
            let currentDelay = retryDelay;
                const attemptStart = Date.now();
                    const result = await engine.RunAction(preparedAction);
                    const attemptDuration = Date.now() - attemptStart;
                    const simpleResult: ActionResultSimple = {
                    // Record attempt
                    attempts.push({
                        success: simpleResult.Success,
                        resultCode: simpleResult.ResultCode,
                        delay: attempt > 0 ? currentDelay : 0
                    if (simpleResult.Success) {
                        // Success! Return the result
                            Name: 'Attempts',
                            Value: attempts
                            Name: 'TotalAttempts',
                            Value: attempt + 1
                            Name: 'ActionResult',
                            Value: simpleResult
                            ResultCode: "SUCCESS_WITH_RETRIES",
                                message: `Action succeeded after ${attempt + 1} attempt(s)`,
                                attempts: attempts,
                                finalResult: simpleResult
                    // Check if we should retry this error
                    lastResult = simpleResult;
                    const shouldRetry = this.shouldRetry(simpleResult, retryOn, giveUpOn);
                    if (!shouldRetry || attempt === maxRetries) {
                        // Don't retry or final attempt
                            Name: 'FinalError',
                            ResultCode: shouldRetry ? "MAX_RETRIES_EXCEEDED" : "NON_RETRYABLE_ERROR",
                                message: shouldRetry 
                                    ? `Action failed after ${attempt + 1} attempt(s)` 
                                    : `Action failed with non-retryable error`,
                                finalError: simpleResult
                    // Wait before retry
                        await this.delay(currentDelay, useJitter);
                        currentDelay = Math.min(currentDelay * backoffMultiplier, maxDelay);
                    // Create error result
                    const errorResult: ActionResultSimple = {
                        ResultCode: "EXECUTION_ERROR",
                        resultCode: errorResult.ResultCode,
                    lastResult = errorResult;
                    const shouldRetry = this.shouldRetry(errorResult, retryOn, giveUpOn);
                            Value: error instanceof Error ? error.message : String(error)
                                finalError: error instanceof Error ? error.message : String(error)
                Message: "Retry logic completed unexpectedly"
                Message: `Retry action failed: ${error instanceof Error ? error.message : String(error)}`,
                ResultCode: "RETRY_FAILED"
     * Check if error should be retried
    private shouldRetry(result: ActionResultSimple, retryOn?: string[], giveUpOn?: string[]): boolean {
        // If giveUpOn is specified and matches, don't retry
        if (giveUpOn && Array.isArray(giveUpOn) && giveUpOn.length > 0) {
            if (giveUpOn.includes(result.ResultCode || '')) {
            // Also check if error message contains any giveUpOn values
            if (result.Message) {
                for (const giveUp of giveUpOn) {
                    if (result.Message.toLowerCase().includes(giveUp.toLowerCase())) {
        // If retryOn is not specified, retry all errors
        if (!retryOn || !Array.isArray(retryOn) || retryOn.length === 0) {
        // Check if error code matches retryOn list
        if (retryOn.includes(result.ResultCode || '')) {
        // Check if error message contains any retryOn values
            for (const retry of retryOn) {
                if (result.Message.toLowerCase().includes(retry.toLowerCase())) {
     * Delay with optional jitter
    private delay(milliseconds: number, useJitter: boolean): Promise<void> {
        const actualDelay = useJitter 
            ? milliseconds * (0.5 + Math.random() * 0.5) // 50% to 100% of delay
            : milliseconds;
        return new Promise(resolve => setTimeout(resolve, actualDelay));
