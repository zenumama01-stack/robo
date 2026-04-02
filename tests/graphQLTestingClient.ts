 * Parameters for running a test
export interface RunTestParams {
    environment?: string;
     * Variable values to use for this test run.
     * Key is the variable name, value is the resolved value.
    onProgress?: (progress: TestExecutionProgress) => void;
 * Result from running a test
export interface RunTestResult {
    result: any; // Parsed TestRunResult from engine
 * Parameters for running a test suite
export interface RunTestSuiteParams {
     * Variable values to apply to all tests in this suite.
     * Run only specific tests by their IDs.
     * If provided, only tests with matching IDs will be executed.
    selectedTestIds?: string[];
     * Start execution from this sequence number (inclusive).
     * Tests with sequence numbers less than this value will be skipped.
    sequenceStart?: number;
     * Stop execution at this sequence number (inclusive).
     * Tests with sequence numbers greater than this value will be skipped.
    sequenceEnd?: number;
 * Result from running a test suite
export interface RunTestSuiteResult {
    result: any; // Parsed TestSuiteRunResult from engine
 * Test execution progress update
    oracleEvaluation?: string;
 * Client for executing tests through GraphQL.
 * This class provides an easy way to run tests and test suites from a client application.
 * const testingClient = new GraphQLTestingClient(graphQLProvider);
 * // Run a test
 * const result = await testingClient.RunTest({
 *   testId: "test-uuid",
 *   verbose: true,
 *   environment: "dev"
 * // Run a test suite
 * const suiteResult = await testingClient.RunTestSuite({
 *   suiteId: "suite-uuid",
 *   parallel: true
export class GraphQLTestingClient {
     * Creates a new GraphQLTestingClient instance.
     * Run a single test with the specified parameters.
     * This method invokes a test on the server through GraphQL and returns the result.
     * @param params The parameters for running the test
     * @returns A Promise that resolves to a RunTestResult object
     *   environment: "staging",
     *     console.log(`${progress.currentStep}: ${progress.message} (${progress.percentage}%)`);
     *   console.log('Test passed!', result.result);
     *   console.error('Test failed:', result.errorMessage);
    public async RunTest(params: RunTestParams): Promise<RunTestResult> {
                            // Filter for TestExecutionProgress messages from RunTestResolver
                            if (parsed.resolver === 'RunTestResolver' &&
                                parsed.type === 'TestExecutionProgress' &&
                                // Forward progress to callback
                                params.onProgress!(parsed.data.progress);
                            console.error('[GraphQLTestingClient] Failed to parse progress message:', e);
                mutation RunTest(
                    $testId: String!,
                    $verbose: Boolean,
                    $environment: String,
                    $tags: String,
                    $variables: String
                    RunTest(
                        testId: $testId,
                        verbose: $verbose,
                        environment: $environment,
                        tags: $tags,
                        variables: $variables
            // Serialize tags array to JSON string for GraphQL
            const tagsJson = params.tags && params.tags.length > 0 ? JSON.stringify(params.tags) : undefined;
            // Serialize variables object to JSON string for GraphQL
            const variablesJson = params.variables ? JSON.stringify(params.variables) : undefined;
                testId: params.testId,
                environment: params.environment,
                tags: tagsJson,
                variables: variablesJson
            return this.processTestResult(result.RunTest);
            return this.handleError(e, 'RunTest');
     * Run a test suite with the specified parameters.
     * @param params The parameters for running the test suite
     * @returns A Promise that resolves to a RunTestSuiteResult object
     * const result = await testingClient.RunTestSuite({
     *   parallel: true,
     *   verbose: false,
     * console.log(`Suite: ${result.result.totalTests} tests run`);
     * console.log(`Passed: ${result.result.passedTests}`);
    public async RunTestSuite(params: RunTestSuiteParams): Promise<RunTestSuiteResult> {
                mutation RunTestSuite(
                    $suiteId: String!,
                    $parallel: Boolean,
                    $variables: String,
                    $selectedTestIds: String,
                    $sequenceStart: Int,
                    $sequenceEnd: Int
                    RunTestSuite(
                        suiteId: $suiteId,
                        parallel: $parallel,
                        variables: $variables,
                        selectedTestIds: $selectedTestIds,
                        sequenceStart: $sequenceStart,
                        sequenceEnd: $sequenceEnd
            // Serialize selectedTestIds array to JSON string for GraphQL
            const selectedTestIdsJson = params.selectedTestIds && params.selectedTestIds.length > 0
                ? JSON.stringify(params.selectedTestIds)
                suiteId: params.suiteId,
                parallel: params.parallel,
                variables: variablesJson,
                selectedTestIds: selectedTestIdsJson,
                sequenceStart: params.sequenceStart,
                sequenceEnd: params.sequenceEnd
            return this.processSuiteResult(result.RunTestSuite);
            return this.handleError(e, 'RunTestSuite');
     * Check if a test is currently running
     * @param testId The test ID to check
     * @returns True if the test is running, false otherwise
    public async IsTestRunning(testId: string): Promise<boolean> {
                query IsTestRunning($testId: String!) {
                    IsTestRunning(testId: $testId)
            const result = await this._dataProvider.ExecuteGQL(query, { testId });
            return result.IsTestRunning;
            LogError(`Error checking test running status: ${(e as Error).message}`);
    // ===== Helper Methods =====
    private processTestResult(result: any): RunTestResult {
            parsedResult = SafeJSONParse(result.result);
            executionTimeMs: result.executionTimeMs,
            result: parsedResult
    private processSuiteResult(result: any): RunTestSuiteResult {
    private handleError(error: any, operation: string): any {
        const errorMsg = (error as Error).message;
        LogError(`${operation} failed: ${errorMsg}`);
            errorMessage: errorMsg,
