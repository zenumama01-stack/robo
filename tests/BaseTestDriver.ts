 * @fileoverview Base class for all test driver implementations
    MJTestRunEntity
    TestLogMessage
 * Default timeout for test execution in milliseconds (5 minutes)
export const DEFAULT_TEST_TIMEOUT_MS = 300000;
 * Abstract base class for test driver implementations.
 * Each TestType in the database has a corresponding DriverClass that extends this base.
 * The driver is responsible for:
 * - Parsing test-specific configuration from Configuration JSON
 * - Executing the test with appropriate logic
 * - Running oracles to evaluate results
 * - Calculating scores and determining pass/fail status
 * - Returning structured results
 * BaseTestDriver handles common functionality:
 * - Configuration parsing
 * - Score calculation
 * - Status determination
 * - Logging
 * Follows pattern from BaseScheduledJob and BaseAgent.
 * @RegisterClass(BaseTestDriver, 'AgentEvalDriver')
 * export class AgentEvalDriver extends BaseTestDriver {
 *     async Execute(context: DriverExecutionContext): Promise<DriverExecutionResult> {
 *         const config = this.parseConfig<AgentEvalConfig>(context.test);
 *         // Execute test logic
 *         return result;
export abstract class BaseTestDriver {
    protected _metadata: Metadata = new Metadata();
     * Execute the test.
     * This is the main entry point for test execution. The driver should:
     * 1. Parse Configuration, InputDefinition, ExpectedOutcomes from test entity
     * 2. Perform test-specific execution (e.g., run agent, execute workflow)
     * 3. Run oracles to evaluate results
     * 4. Calculate score and determine status
     * 5. Return structured DriverExecutionResult
     * The base engine will handle:
     * - Creating/updating TestRun entity
     * - Logging to database
     * - Timing and cost tracking
     * @param context - Execution context including test, run, user, options
    abstract Execute(context: DriverExecutionContext): Promise<DriverExecutionResult>;
     * Validate test configuration.
     * Called when creating or updating a test to ensure the configuration is valid
     * for this test type. Override to add type-specific validation.
     * @param test - The test being validated
     * @returns Validation result with errors and warnings
    public async Validate(test: MJTestEntity): Promise<ValidationResult> {
        const errors: ValidationError[] = [];
        const warnings: ValidationWarning[] = [];
        // Basic validation that all drivers need
        if (!test.InputDefinition || test.InputDefinition.trim() === '') {
                message: 'InputDefinition is required',
                suggestion: 'Provide test input definition in JSON format'
                    message: `InputDefinition is not valid JSON: ${(error as Error).message}`,
                    suggestion: 'Fix JSON syntax errors'
        if (!test.ExpectedOutcomes || test.ExpectedOutcomes.trim() === '') {
                message: 'ExpectedOutcomes is recommended for validation',
                recommendation: 'Define expected outcomes to enable automated validation'
                    category: 'expected-outcome',
                    message: `ExpectedOutcomes is not valid JSON: ${(error as Error).message}`,
                    field: 'ExpectedOutcomes',
        if (!test.Configuration || test.Configuration.trim() === '') {
                message: 'Configuration is recommended',
                recommendation: 'Define test configuration including oracles and weights'
                    message: `Configuration is not valid JSON: ${(error as Error).message}`,
     * Calculate overall score from oracle results.
     * If weights are provided, calculates weighted average.
     * Otherwise, calculates simple average.
     * @param oracleResults - Results from oracle evaluations
     * @param weights - Optional scoring weights by oracle type
     * @returns Overall score from 0.0 to 1.0
    protected calculateScore(
        oracleResults: OracleResult[],
        weights?: ScoringWeights
        if (oracleResults.length === 0) {
        if (!weights) {
            // Simple average
            const sum = oracleResults.reduce((acc, r) => acc + r.score, 0);
            return sum / oracleResults.length;
        // Weighted average
        let weightedSum = 0;
        let totalWeight = 0;
        for (const result of oracleResults) {
            const weight = weights[result.oracleType] || 0;
            weightedSum += result.score * weight;
            totalWeight += weight;
        return totalWeight > 0 ? weightedSum / totalWeight : 0;
     * Determine overall test status from oracle results.
     * Test passes only if ALL oracles pass.
     * @returns 'Passed' if all oracles passed, 'Failed' otherwise
    protected determineStatus(oracleResults: OracleResult[]): 'Passed' | 'Failed' {
        return oracleResults.every(r => r.passed) ? 'Passed' : 'Failed';
     * Parse and validate Configuration JSON.
     * Helper method for drivers to parse their configuration with type safety.
     * @param test - The test containing the configuration
    protected parseConfig<T>(test: MJTestEntity): T {
        if (!test.Configuration) {
            throw new Error('Configuration is required for test execution');
            return JSON.parse(test.Configuration) as T;
     * Parse and validate InputDefinition JSON.
     * @template T - The input definition type
     * @param test - The test containing the input definition
     * @returns Parsed input definition
     * @throws Error if input definition is missing or invalid
    protected parseInputDefinition<T>(test: MJTestEntity): T {
        if (!test.InputDefinition) {
            throw new Error('InputDefinition is required for test execution');
            return JSON.parse(test.InputDefinition) as T;
            throw new Error(`Invalid InputDefinition JSON: ${errorMessage}`);
     * Parse and validate ExpectedOutcomes JSON.
     * @template T - The expected outcomes type
     * @param test - The test containing the expected outcomes
     * @returns Parsed expected outcomes
     * @throws Error if expected outcomes is missing or invalid
    protected parseExpectedOutcomes<T>(test: MJTestEntity): T {
        if (!test.ExpectedOutcomes) {
            throw new Error('ExpectedOutcomes is required for test execution');
            return JSON.parse(test.ExpectedOutcomes) as T;
            throw new Error(`Invalid ExpectedOutcomes JSON: ${errorMessage}`);
     * Log execution progress.
     * @param verboseOnly - Whether to only log in verbose mode (default: false)
     * Log errors.
    protected logError(message: string, error?: Error): void {
     * Whether this driver supports cancellation via AbortSignal.
     * Drivers should override this to return true if they properly handle
     * cancellation tokens. When a driver doesn't support cancellation,
     * timeout will still mark the test as failed but the underlying
     * execution may continue in the background.
     * @returns true if driver supports cancellation, false otherwise
    public supportsCancellation(): boolean {
     * Get the effective timeout for a test.
     * 1. Configuration JSON maxExecutionTime field (backward compatibility)
     * 2. Test.MaxExecutionTimeMS column
     * 3. DEFAULT_TEST_TIMEOUT_MS constant (5 minutes)
     * @param test - The test entity
     * @param config - Parsed configuration object (optional)
     * @returns Timeout in milliseconds
    protected getEffectiveTimeout(test: MJTestEntity, config?: { maxExecutionTime?: number }): number {
        // Priority 1: JSON config maxExecutionTime (backward compatibility)
        if (config?.maxExecutionTime != null && config.maxExecutionTime > 0) {
            return config.maxExecutionTime;
        // Priority 2: Entity field MaxExecutionTimeMS
        if (test.MaxExecutionTimeMS != null && test.MaxExecutionTimeMS > 0) {
            return test.MaxExecutionTimeMS;
        // Priority 3: Default timeout
        return DEFAULT_TEST_TIMEOUT_MS;
     * Create a log message for the test execution log.
     * @param level - Log level
     * @param metadata - Optional metadata
     * @returns TestLogMessage object
    protected createLogMessage(
        level: 'info' | 'warn' | 'error' | 'debug',
        metadata?: Record<string, unknown>
    ): TestLogMessage {
     * Log a message to both the console (if verbose) and accumulate for test run log.
     * @param context - Driver execution context
    protected logToTestRun(
        // Log to console based on level and verbosity
        const verboseOnly = level === 'debug';
        // Send to log callback if provided
        if (context.options.logCallback) {
            context.options.logCallback(this.createLogMessage(level, message, metadata));
