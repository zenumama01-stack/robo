export default class Watch extends Command {
  static description = 'Watch for file changes and sync automatically';
    `<%= config.bin %> <%= command.id %> --debounce=1000`,
    `<%= config.bin %> <%= command.id %> --no-validate`,
    dir: Flags.string({ description: 'Specific entity directory to watch' }),
    debounce: Flags.integer({ description: 'Debounce delay in milliseconds (default: 500)' }),
    'no-validate': Flags.boolean({ description: 'Skip validation before sync' }),
  private watchController?: any;
      WatchService, loadMJConfig, loadSyncConfig, initializeProvider,
    const { BaseEntity } = await import('@memberjunction/core');
    const { flags } = await this.parse(Watch);
      // Load sync config
      const syncConfig = await loadSyncConfig(syncConfigDir);
      // Determine watch directory
      const watchDir = flags.dir 
      // Create watch service
      const watchService = new WatchService(syncEngine);
      // Setup graceful shutdown
      process.on('SIGINT', async () => {
        this.log(chalk.yellow('\n\n⏹️  Stopping file watcher...'));
        if (this.watchController) {
          await this.watchController.stop();
      process.on('SIGTERM', async () => {
      // Start watching
      this.log(chalk.bold(`\n👀 Watching for changes in: ${path.relative(process.cwd(), watchDir)}`));
      this.log(chalk.gray('Press Ctrl+C to stop watching\n'));
      this.watchController = await watchService.watch({
        debounceMs: flags.debounce || 500
        onFileAdd: (filePath: string, entityDir: string, entityConfig: any) => {
          const relativePath = path.relative(process.cwd(), filePath);
          this.log(chalk.gray(`➕ added: ${relativePath}`));
        onFileChange: (filePath: string, entityDir: string, entityConfig: any) => {
          this.log(chalk.gray(`📝 changed: ${relativePath}`));
        onFileDelete: (filePath: string, entityDir: string, entityConfig: any) => {
          this.log(chalk.gray(`❌ deleted: ${relativePath}`));
        onRecordSaved: (entity: InstanceType<typeof BaseEntity>, isNew: boolean, entityConfig: Record<string, unknown>) => {
          const action = isNew ? 'created' : 'updated';
          this.log(chalk.green(`✅ Record ${action} for ${entityConfig.entity}`));
        onError: (error: Error) => {
          spinner.fail('Watch error');
          this.error(error);
      // Keep process alive
      await new Promise(() => {
        // This promise never resolves, keeping the process running
        // until interrupted by SIGINT/SIGTERM
      spinner.fail('Watch failed');
      this.log('\n=== Watch Error Details ===');
