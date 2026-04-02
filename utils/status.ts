 * Status command - Show current analysis status
export default class Status extends Command {
  static description = 'Show current analysis status';
  static examples = ['$ db-auto-doc status'];
        this.log(chalk.yellow('No analysis has been run yet.'));
      this.log(chalk.blue(`\nDatabase Documentation Status\n`));
      this.log(`Database: ${state.database.name}`);
      this.log(`Server: ${state.database.server}`);
      this.log(`Last Modified: ${state.summary.lastModified}\n`);
      // Schemas
      this.log(`Schemas: ${state.schemas.length}`);
      const tableCount = state.schemas.reduce((sum, s) => sum + s.tables.length, 0);
      this.log(`Tables: ${tableCount}\n`);
      // Latest run
      if (state.phases.descriptionGeneration.length > 0) {
        const lastRun = state.phases.descriptionGeneration[state.phases.descriptionGeneration.length - 1];
        this.log(chalk.blue('Latest Analysis Run:'));
        this.log(`  Status: ${lastRun.status}`);
        this.log(`  Iterations: ${lastRun.iterationsPerformed}`);
        this.log(`  Tokens Used: ${lastRun.totalTokensUsed.toLocaleString()}`);
        this.log(`  Estimated Cost: $${lastRun.estimatedCost.toFixed(2)}`);
        if (lastRun.converged) {
          this.log(chalk.green(`  Converged: ${lastRun.convergenceReason}`));
          this.log(chalk.yellow('  Not yet converged'));
      // Low confidence tables
      const lowConfidence = stateManager.getLowConfidenceTables(state, 0.7);
      if (lowConfidence.length > 0) {
        this.log(chalk.yellow(`\nLow Confidence Tables (< 0.7): ${lowConfidence.length}`));
      // Unprocessed tables
      const unprocessed = stateManager.getUnprocessedTables(state);
      if (unprocessed.length > 0) {
        this.log(chalk.yellow(`Unprocessed Tables: ${unprocessed.length}`));
export default class DbDocStatus extends Command {
  static description = 'Show analysis status and progress (delegates to db-auto-doc status)';
    '<%= config.bin %> <%= command.id %> --state-file ./custom-state.json',
    const { flags } = await this.parse(DbDocStatus);
    const { default: StatusCommand } = await import('@memberjunction/db-auto-doc/dist/commands/status');
      args.push('--state-file', flags['state-file']);
    // Execute the DBAutoDoc status command
    await StatusCommand.run(args);
  static description = 'Show status of local files vs database';
    dir: Flags.string({ description: 'Specific entity directory to check status' }),
    verbose: Flags.boolean({ char: 'v', description: 'Show detailed field-level differences' }),
      StatusService, loadMJConfig, initializeProvider,
    const { flags } = await this.parse(Status);
      // Create status service and execute
      const statusService = new StatusService(syncEngine);
      spinner.start('Checking status...');
      const result = await statusService.checkStatus({
      // Display results
      const summary = result.summary;
      const totalFiles = summary.new + summary.modified + summary.deleted + summary.unchanged;
      if (totalFiles === 0) {
        this.log(chalk.yellow('\nNo metadata files found in the specified directory.'));
        this.log(chalk.bold('\nSummary:'));
        if (summary.new > 0) {
          this.log(chalk.green(`  ${summary.new} new file(s) to push`));
        if (summary.modified > 0) {
          this.log(chalk.yellow(`  ${summary.modified} modified file(s) to push`));
        if (summary.deleted > 0) {
          this.log(chalk.red(`  ${summary.deleted} deleted file(s) to remove`));
        if (summary.unchanged > 0) {
          this.log(chalk.gray(`  ${summary.unchanged} unchanged file(s)`));
        // Show details by entity if verbose
        if (flags.verbose && result.details.length > 0) {
          this.log(chalk.bold('\nDetails by entity:'));
          for (const detail of result.details) {
            this.log(`\n  ${detail.entityName} (${detail.directory}):`);
            this.log(`    New: ${detail.new}, Modified: ${detail.modified}, Deleted: ${detail.deleted}, Unchanged: ${detail.unchanged}`);
      spinner.fail('Status check failed');
      this.log('\n=== Status Error Details ===');
