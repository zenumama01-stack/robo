import { TextFormatter } from './text-formatter';
export type OutputFormat = 'compact' | 'json' | 'table';
export interface AgentInfo {
  description?: string;
  status: 'available' | 'disabled';
  lastUsed?: string;
export interface ActionInfo {
  parameters?: Array<{
    required: boolean;
  }>;
export interface ExecutionResult {
  entityName: string;
  prompt?: string;
  error?: string;
  duration: number;
  steps?: number;
  executionId?: string;
  logFilePath?: string;
export class OutputFormatter {
  constructor(private format: OutputFormat) {}
  public formatAgentList(agents: AgentInfo[]): string {
    if (agents.length === 0) {
      return chalk.yellow('No agents found.');
    switch (this.format) {
      case 'json':
        return JSON.stringify(agents, null, 2);
      case 'table':
        return this.formatAgentTable(agents);
      case 'compact':
        return this.formatAgentCompact(agents);
  public formatActionList(actions: ActionInfo[]): string {
    if (actions.length === 0) {
      return chalk.yellow('No actions found.');
        return JSON.stringify(actions, null, 2);
        return this.formatActionTable(actions);
        return this.formatActionCompact(actions);
  public formatAgentResult(result: ExecutionResult): string {
        return JSON.stringify(result, null, 2);
        return this.formatResultTable(result, 'Agent');
        return this.formatResultCompact(result, 'Agent');
  public formatActionResult(result: ExecutionResult): string {
        return this.formatResultTable(result, 'Action');
        return this.formatResultCompact(result, 'Action');
  public formatPromptResult(result: ExecutionResult): string {
        return this.formatResultTable(result, 'Prompt');
        return this.formatPromptResultCompact(result);
  private formatAgentTable(agents: AgentInfo[]): string {
    const tableData = [
      [chalk.bold('Name'), chalk.bold('Status'), chalk.bold('Description'), chalk.bold('Last Used')]
    agents.forEach(agent => {
      const status = agent.status === 'available' 
        ? chalk.green('available') 
        : chalk.red('disabled');
      tableData.push([
        agent.name,
        agent.description || '',
        agent.lastUsed || 'Never'
    return table(tableData, {
      border: {
        topBody: '─',
        topJoin: '┬',
        topLeft: '┌',
        topRight: '┐',
        bottomBody: '─',
        bottomJoin: '┴',
        bottomLeft: '└',
        bottomRight: '┘',
        bodyLeft: '│',
        bodyRight: '│',
        bodyJoin: '│',
        joinBody: '─',
        joinLeft: '├',
        joinRight: '┤',
        joinJoin: '┼'
  private formatActionTable(actions: ActionInfo[]): string {
      [chalk.bold('Name'), chalk.bold('Status'), chalk.bold('Parameters'), chalk.bold('Description')]
    actions.forEach(action => {
      const status = action.status === 'available' 
      const params = action.parameters 
        ? action.parameters.map(p => `${p.name}${p.required ? '*' : ''}`).join(', ')
        : 'None';
        action.name,
        params,
        action.description || ''
  private formatAgentCompact(agents: AgentInfo[]): string {
    let output = chalk.bold(`Found ${agents.length} agent(s):\n\n`);
        ? chalk.green('✓') 
        : chalk.red('✗');
      output += `${status} ${chalk.cyan(agent.name)}`;
      if (agent.description) {
        output += ` - ${agent.description}`;
  private formatActionCompact(actions: ActionInfo[]): string {
    let output = chalk.bold(`Found ${actions.length} action(s):\n\n`);
      output += `${status} ${chalk.cyan(action.name)}`;
      if (action.description) {
        output += ` - ${action.description}`;
      if (action.parameters && action.parameters.length > 0) {
          .map(p => `${p.name}${p.required ? '*' : ''}`)
        output += chalk.dim(` (${params})`);
  private formatResultTable(result: ExecutionResult, type: string): string {
      [chalk.bold('Property'), chalk.bold('Value')]
    tableData.push(['Status', result.success ? chalk.green('Success') : chalk.red('Failed')]);
    tableData.push([type, result.entityName]);
    if (result.prompt) {
      tableData.push(['Prompt', result.prompt.length > 50 ? result.prompt.substring(0, 50) + '...' : result.prompt]);
    tableData.push(['Duration', `${result.duration}ms`]);
    if (result.steps) {
      tableData.push(['Steps', result.steps.toString()]);
    if (result.executionId) {
      tableData.push(['Execution ID', result.executionId]);
    if (result.logFilePath) {
      tableData.push(['Log File', result.logFilePath]);
    if (result.error) {
      tableData.push(['Error', chalk.red(result.error)]);
    return table(tableData);
  private formatResultCompact(result: ExecutionResult, type: string): string {
    if (result.success) {
      output += chalk.green(`✓ ${type} execution completed successfully\n`);
      output += chalk.bold(`${type}:`) + ` ${result.entityName}\n`;
        output += chalk.bold('Prompt:') + ` ${result.prompt}\n`;
      output += chalk.bold('Duration:') + ` ${result.duration}ms\n`;
        output += chalk.bold('Steps:') + ` ${result.steps}\n`;
      if (result.result) {
        output += chalk.bold('Result:') + '\n';
        if (typeof result.result === 'string') {
          const formatted = TextFormatter.formatText(result.result, {
            maxWidth: 80,
            indent: 2,
            preserveParagraphs: true
          output += formatted + '\n';
          output += TextFormatter.formatJSON(result.result, 2) + '\n';
        output += chalk.dim(`\nDetailed logs: ${result.logFilePath}\n`);
      output += chalk.red(`✗ ${type} execution failed\n`);
        output += chalk.bold('Error:') + ` ${chalk.red(result.error)}\n`;
        output += chalk.dim(`\nError logs: ${result.logFilePath}\n`);
  private formatPromptResultCompact(result: ExecutionResult): string {
      output += chalk.green('✓ Prompt executed successfully\n\n');
      // If result has structure with model selection info
      if (result.result && typeof result.result === 'object' && 'response' in result.result) {
        // Show the response
        output += chalk.bold('Response:\n');
        output += result.result.response + '\n';
        // Show model selection info if available
        if (result.result.modelSelection) {
          output += '\n' + chalk.bold('Model Information:\n');
          const ms = result.result.modelSelection;
          output += chalk.gray(`• Model: ${ms.modelUsed || 'Default'}\n`);
          output += chalk.gray(`• Vendor: ${ms.vendorUsed || 'Default'}\n`);
          if (ms.configurationUsed) {
            output += chalk.gray(`• Configuration: ${ms.configurationUsed}\n`);
          if (ms.selectionStrategy) {
            output += chalk.gray(`• Selection Strategy: ${ms.selectionStrategy}\n`);
          if (ms.modelsConsidered) {
            output += chalk.gray(`• Models Considered: ${ms.modelsConsidered}\n`);
        // Show usage info if available
        if (result.result.usage) {
          output += '\n' + chalk.bold('Token Usage:\n');
          const usage = result.result.usage;
          if (usage.promptTokens) output += chalk.gray(`• Prompt Tokens: ${usage.promptTokens}\n`);
          if (usage.completionTokens) output += chalk.gray(`• Completion Tokens: ${usage.completionTokens}\n`);
          if (usage.totalTokens) output += chalk.gray(`• Total Tokens: ${usage.totalTokens}\n`);
        // Simple string response
        output += (typeof result.result === 'string' ? result.result : JSON.stringify(result.result, null, 2)) + '\n';
      output += '\n' + chalk.gray(`Duration: ${result.duration}ms`);
        output += chalk.dim(`\nDetailed logs: ${result.logFilePath}`);
      output += chalk.red('✗ Prompt execution failed\n\n');
      output += chalk.gray(`Duration: ${result.duration}ms`);
        output += chalk.dim(`\nError logs: ${result.logFilePath}`);
 * @fileoverview Output formatting utilities for CLI
import { TestRunResult, TestSuiteRunResult } from '@memberjunction/testing-engine';
import { OutputFormat } from '../types';
 * Format test result for display
     * Format test run result based on output format
    static formatTestResult(result: TestRunResult, format: OutputFormat): string {
                return this.formatJSON(result);
                return this.formatMarkdown(result);
            case 'console':
                return this.formatConsole(result);
     * Format suite result based on output format
    static formatSuiteResult(result: TestSuiteRunResult, format: OutputFormat): string {
                return this.formatSuiteJSON(result);
                return this.formatSuiteMarkdown(result);
                return this.formatSuiteConsole(result);
     * Format test result as JSON
    private static formatJSON(result: TestRunResult): string {
     * Format test result as Markdown
    private static formatMarkdown(result: TestRunResult): string {
        const passed = result.status === 'Passed';
        const status = passed ? 'PASSED ✓' : 'FAILED ✗';
        const scorePercent = (result.score * 100).toFixed(1);
        let md = `# Test Run: ${result.testName}\n`;
        md += `**Status:** ${status}\n`;
        md += `**Score:** ${result.score.toFixed(4)} (${scorePercent}%)\n`;
        md += `**Duration:** ${(result.durationMs / 1000).toFixed(1)}s\n`;
        md += `**Cost:** $${result.totalCost.toFixed(4)}\n`;
        md += '\n## Oracle Results\n';
        for (const oracle of result.oracleResults) {
            const symbol = oracle.passed ? '✓' : '✗';
            md += `- ${symbol} ${oracle.oracleType}: ${oracle.message}\n`;
            md += '\n## Error\n';
            md += `\`\`\`\n${result.errorMessage}\n\`\`\`\n`;
     * Format test result for console output
    private static formatConsole(result: TestRunResult): string {
        lines.push(chalk.bold(`\n[TEST_START] ${result.testName}`));
        lines.push(chalk.gray(`[TARGET] ${result.targetType}`));
        if (result.targetLogId) {
            lines.push(chalk.cyan(`[TARGET_ID] ${result.targetLogId}`));
        // Oracle results
        if (result.oracleResults.length > 0) {
            lines.push(chalk.bold(`[ORACLES] Running ${result.oracleResults.length} validation(s)`));
                const symbol = oracle.passed ? chalk.green('✓') : chalk.red('✗');
                const scoreStr = oracle.score != null ? ` (score: ${oracle.score.toFixed(4)})` : '';
                lines.push(`${symbol} ${oracle.oracleType}${scoreStr}: ${oracle.message}`);
        // Score
        lines.push(chalk.bold('[SCORE]'));
        lines.push(`  Overall: ${result.score.toFixed(4)} (${scorePercent}%)`);
        lines.push(`  Checks: ${result.passedChecks}/${result.totalChecks} passed`);
        lines.push(`  Duration: ${(result.durationMs / 1000).toFixed(1)}s`);
        lines.push(`  Cost: $${result.totalCost.toFixed(4)}`);
            lines.push(chalk.green.bold(`[TEST_PASS] ${result.testName}`));
            lines.push(chalk.red.bold(`[TEST_FAIL] ${result.testName}`));
            lines.push(chalk.red.bold('[ERROR]'));
            lines.push(chalk.red(result.errorMessage));
     * Format suite result as JSON
    private static formatSuiteJSON(result: TestSuiteRunResult): string {
     * Format suite result as Markdown
    private static formatSuiteMarkdown(result: TestSuiteRunResult): string {
        const passRate = result.totalTests > 0 ? (result.passedTests / result.totalTests * 100).toFixed(1) : '0.0';
        let md = `# Test Suite: ${result.suiteName}\n`;
        md += `**Total Tests:** ${result.totalTests}\n`;
        md += `**Passed:** ${result.passedTests}\n`;
        md += `**Failed:** ${result.failedTests}\n`;
        md += `**Pass Rate:** ${passRate}%\n`;
        md += `**Total Cost:** $${result.totalCost.toFixed(4)}\n`;
        md += '\n## Test Results\n';
        md += '| Test | Status | Score | Duration | Cost |\n';
        md += '|------|--------|-------|----------|------|\n';
        for (const testResult of result.testResults) {
            const passed = testResult.status === 'Passed';
            const status = passed ? '✓ PASS' : '✗ FAIL';
            const cost = `$${testResult.totalCost.toFixed(4)}`;
            md += `| ${testResult.testName} | ${status} | ${testResult.score.toFixed(4)} | ${(testResult.durationMs / 1000).toFixed(1)}s | ${cost} |\n`;
        const failedTests = result.testResults.filter(t => t.status !== 'Passed');
        if (failedTests.length > 0) {
            md += '\n## Failures\n';
            for (const testResult of failedTests) {
                md += `\n### ${testResult.testName}\n`;
                md += `- **Score:** ${testResult.score.toFixed(4)}\n`;
                for (const oracle of testResult.oracleResults) {
                    if (!oracle.passed) {
                        md += `- **Failed Oracle:** ${oracle.oracleType} - ${oracle.message}\n`;
                if (testResult.errorMessage) {
                    md += `- **Error:** ${testResult.errorMessage}\n`;
     * Format suite result for console output
    private static formatSuiteConsole(result: TestSuiteRunResult): string {
        lines.push(chalk.bold(`\n[SUITE_START] ${result.suiteName}`));
        lines.push(chalk.gray(`[TESTS] ${result.totalTests} tests queued`));
        // Individual test results
        for (let i = 0; i < result.testResults.length; i++) {
            const testResult = result.testResults[i];
            const number = chalk.gray(`[${i + 1}/${result.totalTests}]`);
            const symbol = passed ? chalk.green('✓') : chalk.red('✗');
            const status = passed ? chalk.green('PASSED') : chalk.red('FAILED');
            lines.push(`${number} ${testResult.testName}`);
            lines.push(`${symbol} ${status} (${(testResult.durationMs / 1000).toFixed(1)}s, score: ${testResult.score.toFixed(4)}, cost: ${cost})`);
            if (!passed) {
                        lines.push(chalk.red(`  - Oracle '${oracle.oracleType}' failed: ${oracle.message}`));
                    lines.push(chalk.red(`  - Error: ${testResult.errorMessage}`));
        lines.push(chalk.bold('[SUITE_COMPLETE] ' + result.suiteName));
        lines.push(chalk.bold(`[SUMMARY] ${result.passedTests}/${result.totalTests} passed (${passRate}%)`));
        lines.push(chalk.bold(`[DURATION] ${(result.durationMs / 1000).toFixed(1)}s`));
        lines.push(chalk.bold(`[COST] $${result.totalCost.toFixed(4)}`));
        if (result.failedTests > 0) {
            lines.push(chalk.red.bold(`[FAILURES] ${result.failedTests} test(s) failed - see details above`));
     * Write output to file if specified
    static writeToFile(content: string, filePath?: string): void {
        if (!filePath) {
            fs.writeFileSync(filePath, content, 'utf-8');
            console.log(chalk.green(`✓ Output saved to ${filePath}`));
            console.error(chalk.red(`✗ Failed to write to ${filePath}: ${(error as Error).message}`));
     * Format error message
    static formatError(message: string, error?: Error): string {
        lines.push(chalk.red.bold('✗ Error: ' + message));
            lines.push(chalk.red(error.message));
                lines.push(chalk.gray(error.stack));
     * Format success message
    static formatSuccess(message: string): string {
        return chalk.green.bold('✓ ' + message);
     * Format warning message
    static formatWarning(message: string): string {
        return chalk.yellow.bold('⚠ ' + message);
     * Format info message
    static formatInfo(message: string): string {
        return chalk.blue.bold('ℹ ' + message);
