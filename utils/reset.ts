 * Reset command - Clear state and start fresh
export default class Reset extends Command {
  static description = 'Reset state and start fresh analysis';
  static examples = ['$ db-auto-doc reset'];
    force: Flags.boolean({ char: 'f', description: 'Skip confirmation prompt' })
    const { flags } = await this.parse(Reset);
      const config = await ConfigLoader.load('./config.json');
      const stateManager = new StateManager(config.output.stateFile);
      // Confirm deletion unless --force
      if (!flags.force) {
        const answer = await inquirer.prompt([
            name: 'confirm',
            message: chalk.yellow('This will delete all analysis state. Continue?'),
        if (!answer.confirm) {
          this.log('Reset cancelled');
      // Delete state file
      await stateManager.delete();
      this.log(chalk.green('✓ State reset complete!'));
      this.log('Run: db-auto-doc analyze');
export default class DbDocReset extends Command {
  static description = 'Reset analysis state (delegates to db-auto-doc reset)';
    '<%= config.bin %> <%= command.id %> --force',
    force: Flags.boolean({
      description: 'Force reset without confirmation',
    const { flags } = await this.parse(DbDocReset);
    const { default: ResetCommand } = await import('@memberjunction/db-auto-doc/dist/commands/reset');
    if (flags.force) {
      args.push('--force');
    // Execute the DBAutoDoc reset command
    await ResetCommand.run(args);
