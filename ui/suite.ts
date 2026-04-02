export default class TestSuite extends Command {
  static description = 'Execute a test suite';
    '<%= config.bin %> <%= command.id %> <suite-id>',
    '<%= config.bin %> <%= command.id %> --name="Agent Quality Suite"',
    '<%= config.bin %> <%= command.id %> <suite-id> --format=json',
    '<%= config.bin %> <%= command.id %> <suite-id> --output=suite-results.json',
    suiteId: Args.string({
      description: 'Test suite ID to execute',
      description: 'Test suite name to execute',
    const { SuiteCommand } = await import('@memberjunction/testing-cli');
    const { args, flags } = await this.parse(TestSuite);
      // Create SuiteCommand instance and execute
      const suiteCommand = new SuiteCommand();
      await suiteCommand.execute(args.suiteId, {
 * @fileoverview Suite command implementation
import { SuiteFlags } from '../types';
import { parseVariableFlags } from '../utils/variable-parser';
 * Suite command - Execute a test suite
export class SuiteCommand {
     * Execute the suite command
     * @param suiteId - Test suite ID to run
    async execute(suiteId: string | undefined, flags: SuiteFlags, contextUser?: UserInfo): Promise<void> {
            console.log('Initializing MJ provider...');
            console.log('MJ provider initialized successfully');
            console.log('Getting TestEngine instance...');
            console.log('Configuring TestEngine...');
            console.log(`TestEngine configured. Test Types loaded: ${engine.TestTypes?.length || 0}`);
            console.log(`Test Suites loaded: ${engine.TestSuites?.length || 0}`);
            console.log(`Tests loaded: ${engine.Tests?.length || 0}`);
            let suite;
            if (suiteId) {
                // Run specific suite by ID
                console.log(`Looking for suite by ID: ${suiteId}`);
                suite = engine.GetTestSuiteByID(suiteId);
                if (!suite) {
                    console.error(OutputFormatter.formatError(`Test suite not found: ${suiteId}`));
                    console.error(`Available suites: ${engine.TestSuites?.map(s => s.Name).join(', ') || 'none'}`);
                // Run suite by name
                console.log(`Looking for suite by name: ${flags.name}`);
                suite = engine.GetTestSuiteByName(flags.name);
                    console.error(OutputFormatter.formatError(`Test suite not found: ${flags.name}`));
                console.error(OutputFormatter.formatError('Must specify suite ID or --name'));
            // Note: Suite variables apply to all tests - type conversion happens per-test
            const variables = parseVariableFlags(flags.var);
            // Execute suite
            this.spinner.start(`Running test suite: ${suite.Name}...`);
            // Note: parallel and failFast are handled by RunSuite internally
            // We only pass the standard TestRunOptions
            const result = await engine.RunSuite(suite.ID, {
            // Format and display result
            const output = OutputFormatter.formatSuiteResult(result, format);
            // Exit with appropriate code (non-zero if any test failed)
            process.exit(result.failedTests === 0 ? 0 : 1);
            console.error(OutputFormatter.formatError('Failed to run test suite', error as Error));
