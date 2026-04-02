 * Generate Queries command - Generate sample SQL queries from existing state
import { AutoDocConnectionConfig } from '../types/driver.js';
import { PromptEngine } from '../prompts/PromptEngine.js';
import { SampleQueryGenerator } from '../generators/SampleQueryGenerator.js';
import { SampleQueryGenerationConfig } from '../types/sample-queries.js';
export default class GenerateQueries extends Command {
  static description = 'Generate sample SQL queries from existing analysis state';
    '$ db-auto-doc generate-queries --from-state ./output/run-1/state.json',
    '$ db-auto-doc generate-queries --from-state ./output/run-1/state.json --queries-per-table 10',
    '$ db-auto-doc generate-queries --from-state ./output/run-1/state.json --max-execution-time 60000'
      description: 'Path to existing state.json file from previous analysis',
      description: 'Path to config file (for database connection and AI settings)',
    'queries-per-table': Flags.integer({
      description: 'Number of queries to generate per table',
    'max-execution-time': Flags.integer({
      description: 'Maximum execution time for query validation (ms)',
    const { flags } = await this.parse(GenerateQueries);
      // Load configuration for database connection and AI settings
      // Load existing state
      spinner.start('Loading existing analysis state');
      const stateJson = await fs.readFile(flags['from-state'], 'utf-8');
      const state = JSON.parse(stateJson) as DatabaseDocumentation;
      spinner.succeed(`State loaded: ${state.schemas.length} schemas, ${state.schemas.reduce((sum, s) => sum + s.tables.length, 0)} tables`);
      // Connect to database
      spinner.start('Connecting to database');
      const driverConfig: AutoDocConnectionConfig = {
        trustServerCertificate: config.database.trustServerCertificate,
        connectionTimeout: config.database.connectionTimeout,
        requestTimeout: config.database.requestTimeout,
        maxConnections: config.database.maxConnections,
        minConnections: config.database.minConnections,
        idleTimeoutMillis: config.database.idleTimeoutMillis
      const db = new DatabaseConnection(driverConfig);
      const testResult = await db.test();
      if (!testResult.success) {
        throw new Error(`Database connection failed: ${testResult.message}`);
      spinner.succeed('Connected to database');
      // Initialize prompt engine
      spinner.start('Initializing AI prompt engine');
      const promptsDir = path.join(__dirname, '../../prompts');
      const promptEngine = new PromptEngine(config.ai, promptsDir);
      await promptEngine.initialize();
      spinner.succeed('Prompt engine ready');
      // Build query generation config
      const queryConfig: SampleQueryGenerationConfig = {
        queriesPerTable: flags['queries-per-table'] || config.analysis.sampleQueryGeneration?.queriesPerTable || 5,
        maxExecutionTime: flags['max-execution-time'] || config.analysis.sampleQueryGeneration?.maxExecutionTime || 30000,
        includeMultiQueryPatterns: config.analysis.sampleQueryGeneration?.includeMultiQueryPatterns !== false,
        validateAlignment: config.analysis.sampleQueryGeneration?.validateAlignment !== false,
        tokenBudget: config.analysis.sampleQueryGeneration?.tokenBudget || 100000,
        maxRowsInSample: config.analysis.sampleQueryGeneration?.maxRowsInSample || 10,
        maxTables: config.analysis.sampleQueryGeneration?.maxTables  // Optional, defaults to 10 in generator
      // Use AI config from main settings
      const model = config.ai.model;
      const effortLevel = config.ai.effortLevel || 75;
      const maxTokens = config.ai.maxTokens || 16000;  // Use config value or default
      // Create StateManager for state file updates
      const stateManager = new StateManager(flags['from-state']);
      // Create generator
      spinner.start('Generating sample queries');
      const generator = new SampleQueryGenerator(
        queryConfig,
        promptEngine,
        db.getDriver(),
        effortLevel,
        maxTokens
      // Generate queries (will update state file incrementally)
      const result = await generator.generateQueries(state.schemas);
        spinner.succeed('Sample queries generated!');
        this.log(chalk.green('\n✓ Query generation complete!'));
        this.log(`  Total queries: ${result.summary.totalQueriesGenerated}`);
        this.log(`  Validated: ${result.summary.queriesValidated}`);
        this.log(`  Failed validation: ${result.summary.queriesFailed}`);
        this.log(`  Tokens used: ${result.summary.tokensUsed.toLocaleString()}`);
        this.log(`  Estimated cost: $${result.summary.estimatedCost.toFixed(2)}`);
        this.log(`  Average confidence: ${(result.summary.averageConfidence * 100).toFixed(1)}%`);
        this.log(`  Execution time: ${(result.summary.totalExecutionTime / 1000).toFixed(1)}s`);
        this.log(`\n  Queries saved to:`);
        this.log(`    - ${flags['from-state']} (state.sampleQueries)`);
        this.log(chalk.blue('\n  Query breakdown:'));
        this.log(`    By type:`);
        Object.entries(result.summary.queriesByType).forEach(([type, count]) => {
          this.log(`      ${type}: ${count}`);
        this.log(`    By complexity:`);
        Object.entries(result.summary.queriesByComplexity).forEach(([complexity, count]) => {
          this.log(`      ${complexity}: ${count}`);
        spinner.fail('Query generation failed');
        this.error(result.errorMessage || 'Unknown error');
      spinner.fail('Command failed');
export default class DbDocGenerateQueries extends Command {
  static description = 'Generate sample SQL queries from existing analysis state (delegates to db-auto-doc generate-queries)';
    '<%= config.bin %> <%= command.id %> --from-state ./output/run-1/state.json',
    '<%= config.bin %> <%= command.id %> --from-state ./output/run-1/state.json --output-dir ./queries',
    '<%= config.bin %> <%= command.id %> --from-state ./output/run-1/state.json --queries-per-table 10',
      description: 'Output directory for generated queries',
    const { flags } = await this.parse(DbDocGenerateQueries);
    const { default: GenerateQueriesCommand } = await import('@memberjunction/db-auto-doc/dist/commands/generate-queries');
    if (flags['from-state']) {
      args.push('--from-state', flags['from-state']);
    if (flags['output-dir']) {
      args.push('--output-dir', flags['output-dir']);
    if (flags['queries-per-table']) {
      args.push('--queries-per-table', flags['queries-per-table'].toString());
    if (flags['max-execution-time']) {
      args.push('--max-execution-time', flags['max-execution-time'].toString());
    // Execute the DBAutoDoc generate-queries command
    await GenerateQueriesCommand.run(args);
