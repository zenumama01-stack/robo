 * Action that adds delays to workflows
 * // Fixed delay
 *   ActionName: 'Delay',
 *     Name: 'DelayMs',
 *     Value: 5000
 * // Random delay between min and max
 *     Name: 'DelayType',
 *     Value: 'random'
 *     Name: 'MinDelayMs',
 *     Name: 'MaxDelayMs',
 * // Delay with message
 *     Value: 3000
 *     Value: 'Waiting for external system to process...'
@RegisterClass(BaseAction, "Delay")
export class DelayAction extends BaseAction {
     * Adds a delay to the workflow
     *   - DelayMs: Delay in milliseconds (required for fixed delay)
     *   - DelayType: "fixed" | "random" (default: "fixed")
     *   - MinDelayMs: Minimum delay for random delays (required for random)
     *   - MaxDelayMs: Maximum delay for random delays (required for random)
     *   - Message: Optional message to include in result
     * @returns Success with actual delay applied
            const delayType = (this.getParamValue(params, 'delaytype') || 'fixed').toLowerCase();
            let actualDelay: number;
            if (delayType === 'random') {
                // Random delay
                const minDelay = this.getNumericParam(params, 'mindelayms', 0);
                const maxDelay = this.getNumericParam(params, 'maxdelayms', 0);
                if (minDelay < 0 || maxDelay < 0) {
                        Message: "Delay values must be non-negative",
                        ResultCode: "INVALID_DELAY"
                if (minDelay > maxDelay) {
                        Message: "MinDelayMs must be less than or equal to MaxDelayMs",
                        ResultCode: "INVALID_DELAY_RANGE"
                // Calculate random delay
                actualDelay = Math.floor(minDelay + Math.random() * (maxDelay - minDelay + 1));
                // Fixed delay
                actualDelay = this.getNumericParam(params, 'delayms', 0);
                if (actualDelay < 0) {
                        Message: "DelayMs must be non-negative",
            // Record start time
            // Perform the delay
            await this.delay(actualDelay);
            // Calculate actual elapsed time
            const elapsedTime = Date.now() - startTime;
                Name: 'ActualDelayMs',
                Value: actualDelay
                Name: 'ElapsedTimeMs',
                Value: elapsedTime
                Name: 'DelayType',
                Value: delayType
            // Build result message
                message: message || `Delay completed`,
                delayType: delayType,
                requestedDelayMs: actualDelay,
                actualElapsedMs: elapsedTime,
                timeDifference: elapsedTime - actualDelay
                resultData.minDelayMs = this.getNumericParam(params, 'mindelayms', 0);
                resultData.maxDelayMs = this.getNumericParam(params, 'maxdelayms', 0);
                ResultCode: "DELAY_COMPLETED",
                Message: `Delay action failed: ${error instanceof Error ? error.message : String(error)}`,
                ResultCode: "DELAY_FAILED"
     * Perform the actual delay
