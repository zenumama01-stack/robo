    Int
} from 'type-graphql';
import { TestEngine } from '@memberjunction/testing-engine';
import { TestRunVariables } from '@memberjunction/testing-engine-base';
// ===== GraphQL Types =====
export class TestRunResult {
    result: string; // JSON serialized TestRunResult
export class TestSuiteRunResult {
    result: string; // JSON serialized TestSuiteRunResult
export class TestExecutionProgress {
export class TestExecutionStreamMessage {
    type: 'progress' | 'oracle_eval' | 'complete' | 'error';
    progress?: TestExecutionProgress;
    // Not a GraphQL field - used internally
    testRun?: any;
// ===== Resolver =====
export class RunTestResolver extends ResolverBase {
     * Execute a single test
    @Mutation(() => TestRunResult)
    async RunTest(
        @Arg('testId') testId: string,
        @Arg('verbose', { nullable: true }) verbose: boolean = true,
        @Arg('environment', { nullable: true }) environment?: string,
        @Arg('tags', { nullable: true }) tags?: string,
        @Arg('variables', { nullable: true }) variables?: string,
        @PubSub() pubSub?: PubSubEngine,
        @Ctx() { userPayload }: AppContext = {} as AppContext
    ): Promise<TestRunResult> {
                throw new Error('User context required');
            LogStatus(`[RunTestResolver] Starting test execution: ${testId}`);
            // Get singleton instance
            const engine = TestEngine.Instance;
            // Configure engine (loads driver and oracle registries)
            await engine.Config(verbose, user);
            // Create progress callback if we have pubSub
            const progressCallback = pubSub ?
                this.createProgressCallback(pubSub, userPayload, testId) :
            // Parse variables from JSON string if provided
            let parsedVariables: TestRunVariables | undefined;
                    parsedVariables = JSON.parse(variables);
                    LogError(`[RunTestResolver] Failed to parse variables: ${variables}`);
            // Run the test
                environment,
                tags,
                variables: parsedVariables,
                progressCallback
            const result = await engine.RunTest(testId, options, user);
            // Handle both single result and array of results (RepeatCount > 1)
            let finalResult;
                // Multiple iterations - check if all passed
                allPassed = result.every(r => r.status === 'Passed');
                // For GraphQL, return summary information
                // The full array is serialized in the result field
                finalResult = {
                    testRunId: result[0]?.testRunId || '',
                    status: allPassed ? 'Passed' as const : 'Failed' as const
                // Publish completion for each iteration
                if (pubSub) {
                    for (const iterationResult of result) {
                        this.publishComplete(pubSub, userPayload, iterationResult);
                LogStatus(`[RunTestResolver] Test completed: ${result.length} iterations, ${allPassed ? 'all passed' : 'some failed'} in ${Date.now() - startTime}ms`);
                // Single result
                finalResult = result;
                allPassed = result.status === 'Passed';
                // Publish completion
                if (pubSub && result.testRunId) {
                    this.publishComplete(pubSub, userPayload, result);
                LogStatus(`[RunTestResolver] Test completed: ${result.status} in ${Date.now() - startTime}ms`);
                success: allPassed,
                result: JSON.stringify(result), // Full result (single or array)
            LogError(`[RunTestResolver] Test execution failed: ${errorMsg}`);
            // Publish error
                this.publishError(pubSub, userPayload, testId, errorMsg);
                result: JSON.stringify({}),
     * Execute a test suite
    @Mutation(() => TestSuiteRunResult)
    async RunTestSuite(
        @Arg('suiteId') suiteId: string,
        @Arg('parallel', { nullable: true }) parallel: boolean = false,
        @Arg('selectedTestIds', { nullable: true }) selectedTestIds?: string,
        @Arg('sequenceStart', () => Int, { nullable: true }) sequenceStart?: number,
        @Arg('sequenceEnd', () => Int, { nullable: true }) sequenceEnd?: number,
    ): Promise<TestSuiteRunResult> {
            LogStatus(`[RunTestResolver] Starting suite execution: ${suiteId}`);
            // Create progress callback
                this.createProgressCallback(pubSub, userPayload, suiteId) :
            // Parse selectedTestIds from JSON string if provided
            let parsedSelectedTestIds: string[] | undefined;
            if (selectedTestIds) {
                    parsedSelectedTestIds = JSON.parse(selectedTestIds);
                    LogError(`[RunTestResolver] Failed to parse selectedTestIds: ${selectedTestIds}`);
                parallel,
                selectedTestIds: parsedSelectedTestIds,
                sequenceStart,
                sequenceEnd,
            const result = await engine.RunSuite(suiteId, options, user);
            LogStatus(`[RunTestResolver] Suite completed: ${result.totalTests} tests in ${executionTime}ms`);
                success: result.status === 'Completed',
                result: JSON.stringify(result),
            LogError(`[RunTestResolver] Suite execution failed: ${errorMsg}`);
     * Query to check if a test is currently running
    @Query(() => Boolean)
    async IsTestRunning(
        // TODO: Implement running test tracking
        // For now, return false
    // ===== Progress Callbacks =====
     * Create progress callback for test execution
        testId: string
        return (progress: {
            metadata?: any;
            LogStatus(`[RunTestResolver] Progress: ${progress.step} - ${progress.percentage}%`);
            // Get test run from metadata
            const testRun = progress.metadata?.testRun;
            const progressMsg: TestExecutionStreamMessage = {
                sessionId: userPayload.sessionId || '',
                testRunId: testRun?.ID || testId,
                testRun: testRun ? testRun.GetAll() : undefined,
                    testName: progress.metadata?.testName,
                    driverType: progress.metadata?.driverType,
                    oracleEvaluation: progress.metadata?.oracleType
            this.publishProgress(pubSub, progressMsg, userPayload);
    private publishProgress(pubSub: PubSubEngine, data: TestExecutionStreamMessage, userPayload: UserPayload) {
                resolver: 'RunTestResolver',
                type: 'TestExecutionProgress',
    private publishComplete(pubSub: PubSubEngine, userPayload: UserPayload, result: any) {
                type: 'TestExecutionComplete',
                    testRunId: result.testRunId,
    private publishError(pubSub: PubSubEngine, userPayload: UserPayload, testId: string, errorMsg: string) {
                type: 'TestExecutionError',
                status: 'error',
                    testRunId: testId,
