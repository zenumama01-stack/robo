import { Command, Flags, Args } from '@oclif/core';
export default class AgentRun extends Command {
  static description = 'Audit and analyze AI agent execution runs for debugging and performance analysis';
    // Summary mode
    '<%= config.bin %> <%= command.id %> abc-123-def-456',
    '<%= config.bin %> <%= command.id %> abc-123-def-456 --output=markdown',
    // Step detail mode
    '<%= config.bin %> <%= command.id %> abc-123-def-456 --step 3',
    '<%= config.bin %> <%= command.id %> abc-123-def-456 --step 3 --detail full',
    '<%= config.bin %> <%= command.id %> abc-123-def-456 --errors',
    // List recent runs
    '<%= config.bin %> <%= command.id %> --list --agent "Skip: Requirements Expert"',
    '<%= config.bin %> <%= command.id %> --list --status failed --days 7',
    // Export mode
    '<%= config.bin %> <%= command.id %> abc-123-def-456 --export full --file report.json',
    // Output format
      options: ['compact', 'json', 'table', 'markdown'],
    step: Flags.integer({
      description: 'Show details for specific step number (1-based index)',
      exclusive: ['list', 'errors'],
    detail: Flags.string({
      char: 'd',
      description: 'Detail level for step output (default: standard)',
      options: ['minimal', 'standard', 'detailed', 'full'],
      dependsOn: ['step'],
    // Error analysis mode
    errors: Flags.boolean({
      char: 'e',
      description: 'Show only error details and context',
      exclusive: ['step', 'list'],
    // List mode
    list: Flags.boolean({
      char: 'l',
      description: 'List recent agent runs',
      exclusive: ['step', 'errors'],
      description: 'Filter by agent name',
      dependsOn: ['list'],
      description: 'Filter by run status',
      options: ['success', 'failed', 'running', 'all'],
      default: 'all',
    days: Flags.integer({
      description: 'Number of days to look back',
      default: 7,
    limit: Flags.integer({
      description: 'Maximum number of runs to return',
      default: 50,
    export: Flags.string({
      description: 'Export full data to file',
      options: ['full', 'summary', 'steps'],
      dependsOn: ['file'],
    file: Flags.string({
      char: 'f',
      description: 'Output file path for export',
    // Truncation control
      description: 'Maximum tokens per field (0 = no limit)',
      default: 5000,
    // Verbose mode
      description: 'Show detailed diagnostic information',
  static args = {
    runId: Args.string({
      description: 'AI Agent Run ID to audit (UUID)',
      required: false, // Optional because --list mode doesn't need it
    const { AgentAuditService } = await import('@memberjunction/ai-cli');
    const { args, flags } = await this.parse(AgentRun);
      const service = new AgentAuditService();
      // MODE 1: List recent runs
      if (flags.list) {
        spinner.start('Loading recent agent runs...');
        const runs = await service.listRecentRuns({
          agentName: flags.agent,
          status: flags.status as 'success' | 'failed' | 'running' | 'all',
          days: flags.days,
          limit: flags.limit,
        this.log(service.formatRunList(runs, flags.output as any));
      // Validate runId for non-list modes
      if (!args.runId) {
        this.error('Run ID is required (use --list to see recent runs)');
      // MODE 2: Step detail mode
      if (flags.step != null) {
        spinner.start(`Loading step ${flags.step} details...`);
        const stepDetail = await service.getStepDetail(
          args.runId,
          flags.step,
            detailLevel: (flags.detail || 'standard') as 'minimal' | 'standard' | 'detailed' | 'full',
        this.log(service.formatStepDetail(stepDetail, flags.output as any));
      // MODE 3: Error analysis mode
      if (flags.errors) {
        spinner.start('Analyzing errors...');
        const errorAnalysis = await service.analyzeErrors(args.runId);
        this.log(service.formatErrorAnalysis(errorAnalysis, flags.output as any));
      // MODE 4: Export mode
      if (flags.export && flags.file) {
        spinner.start('Exporting full audit data...');
        const exportData = await service.exportRun(args.runId, flags.export as 'full' | 'summary' | 'steps');
        await fs.writeFile(flags.file, JSON.stringify(exportData, null, 2), 'utf8');
        this.log(chalk.green(`✓ Full audit data exported to ${flags.file}`));
        this.log(chalk.dim(`  File size: ${(JSON.stringify(exportData).length / 1024).toFixed(1)} KB`));
      // MODE 5: Summary mode (default)
      spinner.start('Loading agent run summary...');
      const summary = await service.getRunSummary(args.runId, {
        includeStepList: true,
      this.log(service.formatRunSummary(summary, flags.output as any));
      if (flags.verbose) {
        this.log(chalk.dim('\n💡 Tip: Use --step <N> to see details for a specific step'));
        this.log(chalk.dim('💡 Tip: Use --errors to see only error information'));
      spinner.fail('Audit failed');
      if (flags.verbose && error instanceof Error) {
        this.log(chalk.red('\nError Details:'));
        this.log(error.stack || error.message);
