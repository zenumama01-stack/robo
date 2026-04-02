import { confirm } from '@inquirer/prompts';
export default class Push extends Command {
  static description = 'Push local file changes to the database';
    `<%= config.bin %> <%= command.id %> --dry-run`,
    `<%= config.bin %> <%= command.id %> --ci`,
    dir: Flags.string({ description: 'Specific entity directory to push' }),
    'dry-run': Flags.boolean({ description: 'Show what would be pushed without actually pushing' }),
    ci: Flags.boolean({ description: 'CI mode - no prompts, fail on issues' }),
    verbose: Flags.boolean({ char: 'v', description: 'Show detailed field-level output' }),
    'no-validate': Flags.boolean({ description: 'Skip validation before push' }),
    'delete-db-only': Flags.boolean({
      description: 'Delete database-only records that reference records being deleted (prevents FK errors)',
    'parallel-batch-size': Flags.integer({
      description: 'Number of records to process in parallel (default: 10)',
      min: 1,
      max: 50
    include: Flags.string({
      description: 'Only process these directories (comma-separated, supports patterns)',
    exclude: Flags.string({
      description: 'Skip these directories (comma-separated, supports patterns)',
      PushService, ValidationService, FormattingService,
      loadMJConfig, loadSyncConfig, initializeProvider,
    const { flags } = await this.parse(Push);
      // Load configurations
      // Load sync config (currently only used internally by PushService)
      const syncConfigDir = flags.dir ? path.resolve(configManager.getOriginalCwd(), flags.dir) : configManager.getOriginalCwd();
      await loadSyncConfig(syncConfigDir);
      // Parse include/exclude filters
      const includeFilter = flags.include ? flags.include.split(',').map(s => s.trim()) : undefined;
      const excludeFilter = flags.exclude ? flags.exclude.split(',').map(s => s.trim()) : undefined;
      // Run validation unless disabled
        spinner.start('Validating metadata...');
        const validator = new ValidationService({
          include: includeFilter,
          exclude: excludeFilter
        const targetDir = flags.dir ? path.resolve(configManager.getOriginalCwd(), flags.dir) : configManager.getOriginalCwd();
            // In CI mode, fail immediately
            if (flags.ci) {
              this.error('Validation failed. Cannot proceed with push.');
            // Otherwise, ask for confirmation
              this.log(chalk.yellow('\n⚠️  Push cancelled due to validation errors.'));
              // Exit cleanly without throwing an error
      // Create push service and execute
      const pushService = new PushService(syncEngine, getSystemUser());
      const result = await pushService.push(
          dir: flags.dir,
          noValidate: flags['no-validate'],
          deleteDbOnly: flags['delete-db-only'],
          parallelBatchSize: flags['parallel-batch-size'],
          exclude: excludeFilter,
            // For rollback messages, just log them - don't throw
            if (message.includes('Rolling back database') || message.includes('rolled back successfully')) {
              // For actual errors, throw to stop execution
              this.error(message);
            // Check if this is a user-friendly warning that doesn't need a stack trace
            const userFriendlyPatterns = [
              'Record not found:',
              'To auto-create missing records',
              'Circular dependencies detected',
              'Skipping',
              'File backups rolled back',
              'Failed to rollback file backups',
              'SQL logging requested but provider does not support it',
              'Failed to close SQL logging session',
              'WARNING: alwaysPush is enabled',
              'WARNING: autoCreateMissingRecords is enabled'
            const isUserFriendly = userFriendlyPatterns.some(pattern =>
              message.includes(pattern)
            if (isUserFriendly) {
              // Log as a styled warning without stack trace
              this.log(chalk.yellow(message));
              // Use standard warn for unexpected warnings that need debugging info
          onConfirm: async (message) => {
              return true; // Always confirm in CI mode
            return await confirm({ message });
      const summary = formatter.formatSyncSummary('push', {
        updated: result.updated,
        unchanged: result.unchanged,
        deleted: result.deleted || 0,
        skipped: result.skipped || 0,
        deferred: result.deferred || 0,
        errors: result.errors,
        duration: endTime - startTime,
      this.log('\n' + summary);
      if (result.errors > 0) {
          this.error('Push failed with errors in CI mode');
          message: 'Push completed with errors. Do you want to commit the successful changes?',
          throw new Error('Push cancelled due to errors');
      // Show warnings summary
      if (result.warnings.length > 0) {
        this.log(chalk.yellow(`\n⚠️  ${result.warnings.length} warning${result.warnings.length > 1 ? 's' : ''} during push`));
          result.warnings.forEach((warning) => this.log(`   - ${warning}`));
      // Show final status
        if (result.errors === 0) {
          this.log(chalk.green('\n✅ Push completed successfully'));
          this.log(chalk.yellow('\n⚠️  Push completed with errors'));
        if (result.sqlLogPath) {
          this.log(`\n📄 SQL log saved to: ${path.relative(process.cwd(), result.sqlLogPath)}`);
      spinner.fail('Push failed');
      // Show concise error message without duplicate stack traces
      this.log(chalk.red(`\n❌ Error: ${errorMessage}\n`));
      // Exit with error code but don't show stack trace again (already logged by handlers)
