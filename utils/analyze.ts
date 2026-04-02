 * Analyze command - Thin CLI wrapper around AnalysisOrchestrator
export default class Analyze extends Command {
  static description = 'Analyze database and generate documentation';
    '$ db-auto-doc analyze',
    '$ db-auto-doc analyze --resume ./output/run-6/state.json',
    '$ db-auto-doc analyze --config ./my-config.json'
    resume: Flags.string({
      char: 'r',
      description: 'Resume from an existing state file',
    config: Flags.string({
      description: 'Path to config file',
      default: './config.json'
  static args = {};
    const { flags } = await this.parse(Analyze);
      // Load configuration
      spinner.start('Loading configuration');
      const config = await ConfigLoader.load(flags.config);
      spinner.succeed('Configuration loaded');
        resumeFromState: flags.resume,
        onProgress: (message, data) => {
            spinner.succeed(`${message}: ${JSON.stringify(data)}`);
            spinner.text = message;
      spinner.start('Starting analysis');
        spinner.succeed('Analysis complete!');
        this.log(chalk.green('\n✓ Analysis complete!'));
        this.log(`  Iterations: ${result.run.iterationsPerformed}`);
        this.log(`  Tokens used: ${result.run.totalTokensUsed?.toLocaleString() || 0}`);
        this.log(`  Estimated cost: $${result.run.estimatedCost?.toFixed(2) || '0.00'}`);
        this.log(`  Output folder: ${result.outputFolder}`);
        this.log(`  Files:`);
        this.log(`    - state.json`);
        this.log(`    - extended-props.sql`);
        this.log(`    - summary.md`);
        if (flags.resume) {
          this.log(chalk.blue(`\n  Resumed from: ${flags.resume}`));
        spinner.fail('Analysis failed');
        this.error(result.message || 'Unknown error');
      this.error((error as Error).message);
export default class DbDocAnalyze extends Command {
  static description = 'Analyze database and generate documentation (delegates to db-auto-doc analyze)';
    '<%= config.bin %> <%= command.id %> --resume ./output/run-6/state.json',
    '<%= config.bin %> <%= command.id %> --config ./my-config.json',
    const { flags } = await this.parse(DbDocAnalyze);
    // Load DBAutoDoc command dynamically
    const { default: AnalyzeCommand } = await import('@memberjunction/db-auto-doc/dist/commands/analyze');
    // Build args array for DBAutoDoc command
    const args: string[] = [];
      args.push('--resume', flags.resume);
    if (flags.config) {
      args.push('--config', flags.config);
    // Execute the DBAutoDoc analyze command
    await AnalyzeCommand.run(args);
