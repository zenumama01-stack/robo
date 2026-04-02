 * Unit tests for OutputFormatter
// Mock chalk to return plain text
vi.mock('chalk', () => {
    const identity = (s: string) => s;
    const handler: ProxyHandler<Record<string, unknown>> = {
        get: (_target, prop) => {
            if (prop === 'default' || prop === '__esModule') return _target;
            // Support chained calls like chalk.bold('text')
    return { default: new Proxy({}, handler) };
// Mock the table function
vi.mock('table', () => ({
    table: (data: string[][]) => data.map(row => row.join(' | ')).join('\n'),
// Mock TextFormatter
vi.mock('../lib/text-formatter', () => ({
    TextFormatter: {
        formatText: (text: string) => text,
        formatJSON: (obj: unknown) => JSON.stringify(obj, null, 2),
import { OutputFormatter, OutputFormat, AgentInfo, ActionInfo, ExecutionResult } from '../lib/output-formatter';
describe('OutputFormatter', () => {
    describe('formatAgentList()', () => {
        it('should return message when no agents found', () => {
            const formatter = new OutputFormatter('compact');
            const result = formatter.formatAgentList([]);
            expect(result).toContain('No agents found');
        it('should format agents in compact mode', () => {
            const agents: AgentInfo[] = [
                { name: 'TestAgent', description: 'A test agent', status: 'available' },
                { name: 'DisabledAgent', status: 'disabled' },
            const result = formatter.formatAgentList(agents);
            expect(result).toContain('TestAgent');
            expect(result).toContain('A test agent');
            expect(result).toContain('DisabledAgent');
        it('should format agents as JSON', () => {
            const formatter = new OutputFormatter('json');
                { name: 'TestAgent', status: 'available' },
            const parsed = JSON.parse(result);
            expect(parsed).toHaveLength(1);
            expect(parsed[0].name).toBe('TestAgent');
        it('should format agents as table', () => {
            const formatter = new OutputFormatter('table');
    describe('formatActionList()', () => {
        it('should return message when no actions found', () => {
            expect(formatter.formatActionList([])).toContain('No actions found');
        it('should format actions with parameters', () => {
            const actions: ActionInfo[] = [
                    name: 'SendEmail',
                    description: 'Send an email',
                    status: 'available',
                    parameters: [
                        { name: 'to', type: 'string', required: true },
                        { name: 'subject', type: 'string', required: true },
                        { name: 'body', type: 'string', required: false },
            const result = formatter.formatActionList(actions);
            expect(result).toContain('SendEmail');
            expect(result).toContain('Send an email');
        it('should format actions as JSON', () => {
                { name: 'TestAction', status: 'available' },
            expect(JSON.parse(result)[0].name).toBe('TestAction');
    describe('formatAgentResult()', () => {
        it('should format successful result in compact mode', () => {
            const result: ExecutionResult = {
                entityName: 'TestAgent',
                duration: 1500,
                steps: 3,
                result: 'Agent completed successfully',
            const output = formatter.formatAgentResult(result);
            expect(output).toContain('TestAgent');
            expect(output).toContain('1500ms');
        it('should format failed result in compact mode', () => {
                duration: 500,
                error: 'Connection timeout',
            expect(output).toContain('failed');
            expect(output).toContain('Connection timeout');
        it('should format result as JSON', () => {
                duration: 100,
            expect(JSON.parse(output).success).toBe(true);
    describe('formatActionResult()', () => {
        it('should format action result', () => {
                entityName: 'SendEmail',
                duration: 200,
            const output = formatter.formatActionResult(result);
            expect(output).toContain('SendEmail');
    describe('formatPromptResult()', () => {
        it('should format prompt result with response text', () => {
                entityName: 'SummarizePrompt',
                duration: 800,
                result: 'This is the summarized text response.',
            const output = formatter.formatPromptResult(result);
            expect(output).toContain('successfully');
        it('should format prompt result with model info', () => {
                entityName: 'TestPrompt',
                duration: 1000,
                    response: 'The answer is 42',
                    modelSelection: {
                        modelUsed: 'GPT-4',
                        vendorUsed: 'OpenAI',
                    usage: {
                        promptTokens: 100,
                        completionTokens: 50,
                        totalTokens: 150,
            expect(output).toContain('The answer is 42');
        it('should format failed prompt', () => {
                error: 'Model unavailable',
            expect(output).toContain('Model unavailable');
// Mock chalk to pass through strings for testable output
  const chainable = (fn: (s: string) => string): Record<string, unknown> => {
    const handler: ProxyHandler<typeof fn> = {
        if (prop === 'bold') return chainable(fn);
        if (typeof prop === 'symbol') return undefined;
        return chainable(fn);
      apply(_target, _thisArg, args) {
        return fn(args[0]);
    return new Proxy(fn, handler) as unknown as Record<string, unknown>;
      bold: chainable(identity),
      gray: chainable(identity),
      cyan: chainable(identity),
      green: chainable(identity),
      red: chainable(identity),
      yellow: chainable(identity),
      blue: chainable(identity),
vi.mock('@memberjunction/testing-engine', () => ({
  TestRunResult: class {},
  TestSuiteRunResult: class {},
import { OutputFormatter } from '../utils/output-formatter';
// Helper type to match the expected structure
type TestRunResult = {
  oracleResults: Array<{ passed: boolean; oracleType: string; message: string; score?: number }>;
  targetLogId: string;
  passedChecks: number;
  totalChecks: number;
type TestSuiteRunResult = {
  testResults: TestRunResult[];
  const mockTestResult: TestRunResult = {
    testName: 'Test Auth Flow',
    status: 'Passed',
    durationMs: 5000,
    totalCost: 0.0523,
    oracleResults: [
      { passed: true, oracleType: 'exactMatch', message: 'Output matches expected', score: 1.0 },
    targetType: 'agent',
    targetLogId: 'log-001',
    passedChecks: 5,
    totalChecks: 5,
  const mockFailedResult: TestRunResult = {
    testName: 'Test Failing',
    status: 'Failed',
    score: 0.2,
    durationMs: 3000,
    totalCost: 0.03,
      { passed: false, oracleType: 'semanticMatch', message: 'Output does not match', score: 0.2 },
    errorMessage: 'Assertion failed',
    targetLogId: 'log-002',
    passedChecks: 1,
  describe('formatTestResult', () => {
    it('should format test result as JSON', () => {
      const output = OutputFormatter.formatTestResult(mockTestResult as never, 'json');
      const parsed = JSON.parse(output);
      expect(parsed.testName).toBe('Test Auth Flow');
      expect(parsed.score).toBe(0.95);
    it('should format test result as markdown', () => {
      const output = OutputFormatter.formatTestResult(mockTestResult as never, 'markdown');
      expect(output).toContain('# Test Run: Test Auth Flow');
      expect(output).toContain('**Status:** PASSED');
      expect(output).toContain('95.0%');
    it('should format test result for console', () => {
      const output = OutputFormatter.formatTestResult(mockTestResult as never, 'console');
      expect(output).toContain('Test Auth Flow');
      expect(output).toContain('[SCORE]');
    it('should include error information in markdown for failed results', () => {
      const output = OutputFormatter.formatTestResult(mockFailedResult as never, 'markdown');
      expect(output).toContain('FAILED');
      expect(output).toContain('Assertion failed');
  describe('formatSuiteResult', () => {
    const suiteResult: TestSuiteRunResult = {
      suiteName: 'Auth Suite',
      totalTests: 2,
      passedTests: 1,
      failedTests: 1,
      durationMs: 8000,
      totalCost: 0.0823,
      testResults: [mockTestResult, mockFailedResult],
    it('should format suite result as JSON', () => {
      const output = OutputFormatter.formatSuiteResult(suiteResult as never, 'json');
      expect(parsed.suiteName).toBe('Auth Suite');
      expect(parsed.totalTests).toBe(2);
    it('should format suite result as markdown', () => {
      const output = OutputFormatter.formatSuiteResult(suiteResult as never, 'markdown');
      expect(output).toContain('# Test Suite: Auth Suite');
      expect(output).toContain('**Passed:** 1');
      expect(output).toContain('**Failed:** 1');
    it('should format suite result for console', () => {
      const output = OutputFormatter.formatSuiteResult(suiteResult as never, 'console');
      expect(output).toContain('Auth Suite');
      expect(output).toContain('1/2 passed');
  describe('writeToFile', () => {
    it('should not write when no file path given', () => {
      OutputFormatter.writeToFile('content');
      expect(fs.writeFileSync).not.toHaveBeenCalled();
    it('should write to file when path is provided', () => {
      OutputFormatter.writeToFile('content', '/tmp/output.txt');
      expect(fs.writeFileSync).toHaveBeenCalledWith('/tmp/output.txt', 'content', 'utf-8');
  describe('formatError', () => {
    it('should format basic error message', () => {
      const output = OutputFormatter.formatError('Something went wrong');
      expect(output).toContain('Something went wrong');
    it('should include error details when Error object provided', () => {
      const error = new Error('Detailed error');
      const output = OutputFormatter.formatError('Operation failed', error);
      expect(output).toContain('Operation failed');
      expect(output).toContain('Detailed error');
  describe('formatSuccess', () => {
    it('should format success message', () => {
      const output = OutputFormatter.formatSuccess('All tests passed');
      expect(output).toContain('All tests passed');
  describe('formatWarning', () => {
    it('should format warning message', () => {
      const output = OutputFormatter.formatWarning('Slow test detected');
      expect(output).toContain('Slow test detected');
  describe('formatInfo', () => {
    it('should format info message', () => {
      const output = OutputFormatter.formatInfo('Running 5 tests');
      expect(output).toContain('Running 5 tests');
