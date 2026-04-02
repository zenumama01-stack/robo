 * @fileoverview LLM Judge oracle implementation
 * LLM Judge Oracle.
 * Uses an LLM to evaluate output quality based on custom criteria.
 * Provides semantic evaluation beyond deterministic checks.
 * - criteria: Array of validation criteria (required)
 * - model: Model to use for judging (default: from prompt or default model)
 * - temperature: Temperature for LLM (default: 0.1 for consistency)
 * - promptTemplate: Custom prompt template (optional, uses default if not provided)
 * - strictMode: Require all criteria to pass (default: false, uses weighted scoring)
 * const oracle = new LLMJudgeOracle();
 * const result = await oracle.evaluate({
 *     actualOutput: { response: 'Sales by region report created successfully' },
 *     expectedOutput: {
 *         judgeValidationCriteria: [
 *             'Response accurately answers the user\'s question',
 *             'Response includes actionable information',
 *             'Response is professional and clear'
 *     model: 'claude-sonnet-4',
 *     temperature: 0.1
export class LLMJudgeOracle implements IOracle {
    readonly type = 'llm-judge';
     * Default prompt template for LLM judge.
    private readonly defaultPromptTemplate = `You are an expert AI evaluator. Your task is to judge whether an AI agent's output meets the specified validation criteria.
**Input:**
{{input}}
**Expected Output Requirements:**
{{expected}}
**Actual Output:**
{{actual}}
**Validation Criteria:**
{{criteria}}
**Instructions:**
For each criterion, evaluate whether the actual output satisfies it. Provide:
1. A score from 0.0 to 1.0 for each criterion
2. A brief explanation for each score
3. An overall assessment
Respond in JSON format:
  "criteriaScores": [
    { "criterion": "...", "score": 0.0-1.0, "explanation": "..." }
  "overallScore": 0.0-1.0,
  "overallAssessment": "...",
  "passed": true/false
     * Evaluate output using LLM judge.
     * @param input - Oracle input with expected criteria and actual output
     * @returns Oracle result with LLM judgment
            await AIEngine.Instance.Config(false, input.contextUser);
            // Get criteria from expected outcomes or config
            const criteria = ((input.expectedOutput as any)?.judgeValidationCriteria as string[]) ||
                           (config.criteria as string[]);
            if (!criteria || criteria.length === 0) {
                    message: 'No validation criteria provided'
            // Find the LLM Judge prompt from AIEngine
            const judgePrompt = AIEngine.Instance.Prompts.find(p =>
                p.Name === 'Test LLM Judge' || p.Name.toLowerCase().includes('llm judge')
                    message: 'LLM Judge prompt not found in AIEngine.Instance.Prompts. Please create a prompt named "Test LLM Judge".'
            // Prepare data for prompt template
            const inputDefinition = input.test.InputDefinition ?
                (typeof input.test.InputDefinition === 'string' ?
                    JSON.parse(input.test.InputDefinition) :
                    input.test.InputDefinition) :
                {};
                input: JSON.stringify(inputDefinition, null, 2),
                expected: JSON.stringify(input.expectedOutput, null, 2),
                actual: JSON.stringify(input.actualOutput, null, 2),
                criteria: criteria.map((c, i) => `${i + 1}. ${c}`).join('\n')
            // Execute LLM judgment
            promptParams.prompt = judgePrompt;
            promptParams.data = promptData;
            promptParams.contextUser = input.contextUser;
                    message: `LLM judgment failed: ${result.errorMessage}`
            const judgment = this.parseJudgment(result.result as string);
            // Determine pass/fail
            const strictMode = config.strictMode as boolean;
            const passed = strictMode
                ? judgment.criteriaScores.every(s => s.score >= 0.8)
                : judgment.overallScore >= 0.7;
                score: judgment.overallScore,
                message: judgment.overallAssessment,
                    criteriaScores: judgment.criteriaScores,
                    llmModel: config.model || 'default',
                    llmCost: result.cost
                message: `LLM judge error: ${(error as Error).message}`
     * Build prompt for LLM judge.
    private buildPrompt(
        input: OracleInput,
        criteria: string[],
        customTemplate?: string
        const template = customTemplate || this.defaultPromptTemplate;
        // Format input data
        const inputStr = JSON.stringify(input.test.InputDefinition, null, 2);
        const expectedStr = JSON.stringify(input.expectedOutput, null, 2);
        const actualStr = JSON.stringify(input.actualOutput, null, 2);
        const criteriaStr = criteria.map((c, i) => `${i + 1}. ${c}`).join('\n');
        // Replace placeholders
        return template
            .replace('{{input}}', inputStr)
            .replace('{{expected}}', expectedStr)
            .replace('{{actual}}', actualStr)
            .replace('{{criteria}}', criteriaStr);
     * Parse LLM judgment response.
    private parseJudgment(response: string): {
        criteriaScores: Array<{ criterion: string; score: number; explanation: string }>;
        overallScore: number;
            // Try to extract JSON from response
            const jsonMatch = response.match(/\{[\s\S]*\}/);
            if (!jsonMatch) {
                throw new Error('No JSON found in LLM response');
                criteriaScores: parsed.criteriaScores || [],
                overallScore: parsed.overallScore || 0,
                overallAssessment: parsed.overallAssessment || 'No assessment provided'
            // Fallback to simple parsing
                criteriaScores: [],
                overallScore: 0,
                overallAssessment: `Failed to parse LLM response: ${response.substring(0, 200)}`
