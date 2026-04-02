 * QueryRefiner - Iteratively improves queries based on evaluation feedback
 * Uses evaluation and refinement AI prompts to assess if queries answer
 * the business question correctly and improve them through iterations.
  RefinedQuery,
  QueryEvaluation,
  QueryTestResult,
import { QueryTester } from './QueryTester';
import { PROMPT_QUERY_EVALUATOR, PROMPT_QUERY_REFINER } from '../prompts/PromptNames';
 * QueryRefiner class
 * Iteratively refines queries based on evaluation feedback
export class QueryRefiner {
  private entityMetadata: EntityMetadataForPrompt[] = [];
    private tester: QueryTester,
   * Refine a query through evaluation and improvement iterations
   * Loops up to maxRefinements times, testing and evaluating each iteration.
   * Returns when query passes evaluation or max refinements reached.
   * @param query - Initial generated query
   * @param businessQuestion - Original business question
   * @param entityMetadata - Entity metadata for refinement
   * @param maxRefinements - Maximum refinement iterations (default: 3)
   * @returns Refined query with test results and evaluation
  async refineQuery(
    businessQuestion: BusinessQuestion,
    maxRefinements: number = 3
  ): Promise<RefinedQuery> {
    // Store entity metadata for use in evaluation
    this.entityMetadata = entityMetadata;
    let currentQuery = query;
    let refinementCount = 0;
    // Track the last successfully tested query and its results
    let lastWorkingQuery = query;
    let lastWorkingTestResult: QueryTestResult | null = null;
    let lastWorkingEvaluation: QueryEvaluation | null = null;
    await this.configureAIEngine();
    while (refinementCount < maxRefinements) {
      // 1. Test the current query
      let testResult: QueryTestResult;
        testResult = await this.testCurrentQuery(currentQuery);
        // Success! Save this as our last working version
        lastWorkingQuery = currentQuery;
        lastWorkingTestResult = testResult;
        // Query broke during refinement - revert to last working version
          LogStatus(`Refinement produced broken query: ${extractErrorMessage(error, 'Refinement Test')}. Reverting to last working version.`);
        // If we have a previous working version, use that
        if (lastWorkingTestResult && lastWorkingEvaluation) {
          return this.buildSuccessResult(
            lastWorkingQuery,
            lastWorkingTestResult,
            lastWorkingEvaluation,
            refinementCount
        // No previous working version - throw the error
      // 2. Evaluate if it answers the question
      const evaluation = await this.evaluateQuery(
        currentQuery,
        testResult
      lastWorkingEvaluation = evaluation;
      // 3. If evaluation passes, we're done!
      if (this.shouldStopRefining(evaluation)) {
          testResult,
          evaluation,
      // 4. Refine the query based on suggestions
      refinementCount++;
        LogStatus(`Refinement iteration ${refinementCount}/${maxRefinements}`);
      currentQuery = await this.performRefinement(
        entityMetadata
    // Reached max refinements - return best attempt
    // Use the last successfully tested query
        query: lastWorkingQuery,
        testResult: lastWorkingTestResult,
        evaluation: lastWorkingEvaluation,
        refinementCount,
        reachedMaxRefinements: true,
    // Fallback: try to build final result with current query
    return await this.buildFinalResult(
   * Configure AIEngine for prompt execution
   * Ensures engine is ready before running prompts
  private async configureAIEngine(): Promise<void> {
      throw new Error(extractErrorMessage(error, 'AIEngine Configuration'));
   * Test current query using QueryTester
   * Throws if query testing fails
  private async testCurrentQuery(query: GeneratedQuery): Promise<QueryTestResult> {
    const testResult = await this.tester.testQuery(query);
        `Query testing failed after ${testResult.attempts} attempts: ${testResult.error}`
    return testResult;
   * Determine if refinement should stop based on evaluation
   * Stops if query answers question and doesn't need refinement
  private shouldStopRefining(evaluation: QueryEvaluation): boolean {
    return evaluation.answersQuestion && !evaluation.needsRefinement;
   * Build success result when refinement loop completes successfully
  private buildSuccessResult(
    testResult: QueryTestResult,
    evaluation: QueryEvaluation,
    refinementCount: number
  ): RefinedQuery {
   * Build final result when max refinements reached
   * Re-tests and re-evaluates final query
  private async buildFinalResult(
   * Evaluate if query answers the business question correctly
   * Uses Query Result Evaluator AI prompt
   * @param query - Query to evaluate
   * @param testResult - Test execution results with sample data
   * @returns Evaluation with confidence and suggestions
  private async evaluateQuery(
    testResult: QueryTestResult
  ): Promise<QueryEvaluation> {
      const prompt = this.findPromptByName(aiEngine, PROMPT_QUERY_EVALUATOR);
      // Limit sample results to first 10 rows for efficiency
      const sampleResults = testResult.sampleRows?.slice(0, 10) || [];
        technicalDescription: businessQuestion.technicalDescription,
        entityMetadata: this.entityMetadata,
        generatedSQL: query.sql,
        sampleResults,
      const evaluation = await this.executePrompt<QueryEvaluation>(
        promptData
      this.logEvaluation(evaluation);
      return evaluation;
      throw new Error(extractErrorMessage(error, 'QueryRefiner.evaluateQuery'));
   * Refine query based on evaluation feedback
   * Uses Query Refiner AI prompt
   * @param query - Current query to refine
   * @param evaluation - Evaluation feedback
   * @returns Refined query with improvements
  private async performRefinement(
    entityMetadata: EntityMetadataForPrompt[]
      const prompt = this.findPromptByName(aiEngine, PROMPT_QUERY_REFINER);
        currentSQL: query.sql,
        evaluationFeedback: evaluation,
      const refinedQuery = await this.executePrompt<
        GeneratedQuery & { improvementsSummary: string }
      >(prompt, promptData);
        LogStatus(`Refinements applied: ${refinedQuery.improvementsSummary}`);
        queryName: refinedQuery.queryName,
        sql: refinedQuery.sql,
        parameters: refinedQuery.parameters,
        extractErrorMessage(error, 'QueryRefiner.performRefinement')
   * Execute AI prompt and parse result
   * Generic method for any prompt type
    promptData: Record<string, unknown>
    const result = await executePromptWithOverrides<T>(
      promptData,
      this.contextUser,
   * Log evaluation results for debugging
  private logEvaluation(evaluation: QueryEvaluation): void {
    if (!this.config.verbose) return;
      `Evaluation: answersQuestion=${evaluation.answersQuestion}, ` +
        `confidence=${evaluation.confidence}, ` +
        `needsRefinement=${evaluation.needsRefinement}`
    if (evaluation.reasoning) {
      LogStatus(`Reasoning: ${evaluation.reasoning}`);
    if (evaluation.suggestions.length > 0) {
      LogStatus(`Suggestions: ${evaluation.suggestions.join('; ')}`);
