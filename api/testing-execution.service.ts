export interface TestExecutionOptions {
  parallel?: boolean;
   * Variable values to use for this test run
  variables?: Record<string, unknown>;
export interface TestExecutionProgress {
  currentStep: string;
export interface TestExecutionResult {
    totalCost?: number;
  executionTimeMs: number;
export class TestingExecutionService {
  private testingClient: GraphQLTestingClient;
  ExecuteTest(
    testId: string,
    options: { verbose?: boolean; variables?: Record<string, unknown> } = {}
  ): Observable<{ result: TestExecutionResult; progress: TestExecutionProgress }> {
    const progress$ = new Subject<{ result: TestExecutionResult; progress: TestExecutionProgress }>();
    this.testingClient.RunTest({
      verbose: options.verbose ?? true,
      variables: options.variables,
      onProgress: (progressUpdate) => {
        progress$.next({
          result: null as any,
            currentStep: progressUpdate.currentStep,
            percentage: progressUpdate.percentage,
            message: progressUpdate.message
    }).then((result) => {
        result: result as TestExecutionResult,
          currentStep: 'complete',
          percentage: 100,
          message: 'Test execution complete'
      progress$.complete();
    }).catch((error) => {
      progress$.error(error);
    return progress$.asObservable();
  ExecuteSuite(
    suiteId: string,
    options: { verbose?: boolean; parallel?: boolean; variables?: Record<string, unknown> } = {}
    this.testingClient.RunTestSuite({
      parallel: options.parallel ?? false,
          message: 'Suite execution complete'
  async RerunTest(testRunId: string): Promise<TestExecutionResult> {
    // TODO: Implement re-run logic by fetching original test config and re-executing
    throw new Error('Re-run functionality not yet implemented');
