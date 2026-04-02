export default class TestCompare extends Command {
  static description = 'Compare test runs for regression detection';
    '<%= config.bin %> <%= command.id %> <run-id-1> <run-id-2>',
    '<%= config.bin %> <%= command.id %> --baseline=<run-id> --current=<run-id>',
    '<%= config.bin %> <%= command.id %> --suite=<suite-id> --since="2024-01-01"',
    '<%= config.bin %> <%= command.id %> <run-id-1> <run-id-2> --format=json',
    runId1: Args.string({
      description: 'First test run ID to compare',
    runId2: Args.string({
      description: 'Second test run ID to compare',
    version: Flags.string({
      description: 'Compare runs by version',
    commit: Flags.string({
      description: 'Compare runs by git commit',
    'diff-only': Flags.boolean({
      description: 'Show only differences',
    format: Flags.string({
      options: ['console', 'json', 'markdown'],
      default: 'console',
      description: 'Output file path',
      description: 'Show detailed information',
    const { CompareCommand } = await import('@memberjunction/testing-cli');
    const { args, flags } = await this.parse(TestCompare);
      // Create CompareCommand instance and execute
      // Context user will be fetched internally after MJ provider initialization
      const compareCommand = new CompareCommand();
      await compareCommand.execute(args.runId1, args.runId2, {
        version: flags.version,
        commit: flags.commit,
        diffOnly: flags['diff-only'],
        format: flags.format as 'console' | 'json' | 'markdown',
 * @fileoverview Compare command implementation
import { CompareFlags } from '../types';
 * Compare command - Compare test runs to detect regressions
 * Note: This is a placeholder implementation. Full comparison requires
 * tracking test runs with version/commit metadata.
export class CompareCommand {
     * Execute the compare command
     * @param runId1 - First run ID (optional)
     * @param runId2 - Second run ID (optional)
     * @param flags - Command flags
     * @param contextUser - Optional user context (will be fetched if not provided)
    async execute(
        runId1: string | undefined,
        runId2: string | undefined,
        flags: CompareFlags,
            console.log(OutputFormatter.formatInfo('Compare command not yet implemented'));
            console.log('\nPlanned features:');
            console.log('  - Compare two specific test runs by ID');
            console.log('  - Compare runs by version or git commit');
            console.log('  - Detect regressions in scores or pass rates');
            console.log('  - Show performance and cost differences');
            console.log('  - Filter with --diff-only to show only changes');
            console.log('\nRequires:');
            console.log('  - Test Run Results with version/commit metadata');
            console.log('  - Comparison algorithms for detecting regressions');
            // TODO: Implement full comparison
            // - Load two test runs from database
            // - Compare scores, pass/fail status, duration, cost
            // - Identify regressions (score decreased, new failures)
            // - Display side-by-side comparison
            // - Exit with non-zero if regressions detected
            console.error(OutputFormatter.formatError('Failed to compare test runs', error as Error));
