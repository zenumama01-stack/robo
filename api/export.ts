 * Export command - Generate SQL and Markdown documentation
import { ReportGenerator } from '../generators/ReportGenerator.js';
import { HTMLGenerator } from '../generators/HTMLGenerator.js';
import { CSVGenerator } from '../generators/CSVGenerator.js';
import { MermaidGenerator } from '../generators/MermaidGenerator.js';
import { DatabaseConnection } from '../database/Database.js';
export default class Export extends Command {
  static description = 'Export documentation in multiple formats (SQL, Markdown, HTML, CSV, Mermaid)';
    '$ db-auto-doc export --state-file=./db-doc-state.json',
    '$ db-auto-doc export --sql',
    '$ db-auto-doc export --markdown',
    '$ db-auto-doc export --html',
    '$ db-auto-doc export --csv',
    '$ db-auto-doc export --mermaid',
    '$ db-auto-doc export --sql --markdown --html --csv --mermaid --apply'
    'state-file': Flags.string({ description: 'Path to state JSON file', char: 's' }),
    'output-dir': Flags.string({ description: 'Output directory for generated files', char: 'o' }),
    sql: Flags.boolean({ description: 'Generate SQL script' }),
    markdown: Flags.boolean({ description: 'Generate Markdown documentation' }),
    html: Flags.boolean({ description: 'Generate interactive HTML documentation' }),
    csv: Flags.boolean({ description: 'Generate CSV exports (tables and columns)' }),
    mermaid: Flags.boolean({ description: 'Generate Mermaid ERD diagram files' }),
    report: Flags.boolean({ description: 'Generate analysis report' }),
    apply: Flags.boolean({ description: 'Apply SQL to database', default: false }),
    'approved-only': Flags.boolean({ description: 'Only export approved items', default: false }),
    'confidence-threshold': Flags.string({ description: 'Minimum confidence threshold', default: '0' })
    const { flags } = await this.parse(Export);
      // Determine state file path
      let stateFilePath: string;
      let outputDir: string;
      let config: any = null;
      if (flags['state-file']) {
        // Direct state file mode - no config needed
        stateFilePath = path.resolve(flags['state-file']);
        outputDir = flags['output-dir']
          ? path.resolve(flags['output-dir'])
          : path.dirname(stateFilePath);
        // Config-based mode (original behavior)
        config = await ConfigLoader.load('./config.json');
        stateFilePath = config.output.stateFile;
        outputDir = path.dirname(config.output.sqlFile);
      spinner.start('Loading state');
      const stateManager = new StateManager(stateFilePath);
        throw new Error(`No state file found at ${stateFilePath}. Run "db-auto-doc analyze" first.`);
      spinner.succeed('State loaded');
      // Ensure output directory exists
      // Default to SQL + Markdown if no specific format flags provided
      const anyFormatSpecified = flags.sql || flags.markdown || flags.html || flags.csv || flags.mermaid || flags.report;
      const generateSQL = flags.sql || !anyFormatSpecified;
      const generateMarkdown = flags.markdown || !anyFormatSpecified;
      const generateHTML = flags.html;
      const generateCSV = flags.csv;
      const generateMermaid = flags.mermaid;
      // Prepare generator options
      const generatorOptions = {
        approvedOnly: flags['approved-only'],
        confidenceThreshold: parseFloat(flags['confidence-threshold'])
        spinner.start('Generating SQL script');
        const sql = sqlGen.generate(state, generatorOptions);
        spinner.succeed(`SQL script saved to ${sqlPath}`);
        // Apply to database if requested
        if (flags.apply) {
            this.warn('--apply requires a config file. Skipping database application.');
            spinner.start('Applying SQL to database');
            const dbConfig = {
              provider: (config.database.provider as 'sqlserver' | 'mysql' | 'postgresql' | 'oracle') || 'sqlserver',
              host: config.database.server,
              port: config.database.port,
              database: config.database.database,
              user: config.database.user,
              password: config.database.password,
              encrypt: config.database.encrypt,
              trustServerCertificate: config.database.trustServerCertificate
            const db = new DatabaseConnection(dbConfig);
            await db.connect();
            const result = await db.query(sql);
            await db.close();
              spinner.succeed('SQL applied successfully');
              spinner.fail(`SQL application failed: ${result.errorMessage}`);
        spinner.start('Generating Markdown documentation');
        spinner.succeed(`Markdown documentation saved to ${mdPath}`);
      // Generate HTML
      if (generateHTML) {
        spinner.start('Generating HTML documentation');
        const htmlGen = new HTMLGenerator();
        const html = htmlGen.generate(state, generatorOptions);
        const htmlPath = path.join(outputDir, 'documentation.html');
        await fs.writeFile(htmlPath, html, 'utf-8');
        spinner.succeed(`HTML documentation saved to ${htmlPath}`);
      // Generate CSV
      if (generateCSV) {
        spinner.start('Generating CSV exports');
        const csvGen = new CSVGenerator();
        const csvExport = csvGen.generate(state, generatorOptions);
        const tablesPath = path.join(outputDir, 'tables.csv');
        const columnsPath = path.join(outputDir, 'columns.csv');
        await fs.writeFile(tablesPath, csvExport.tables, 'utf-8');
        await fs.writeFile(columnsPath, csvExport.columns, 'utf-8');
        spinner.succeed(`CSV exports saved to ${tablesPath} and ${columnsPath}`);
      // Generate Mermaid
      if (generateMermaid) {
        spinner.start('Generating Mermaid diagram');
        const mermaidGen = new MermaidGenerator();
        const mermaidDiagram = mermaidGen.generate(state, generatorOptions);
        const mermaidHtml = mermaidGen.generateHtml(state, generatorOptions);
        const mermaidPath = path.join(outputDir, 'erd.mmd');
        const mermaidHtmlPath = path.join(outputDir, 'erd.html');
        await fs.writeFile(mermaidPath, mermaidDiagram, 'utf-8');
        await fs.writeFile(mermaidHtmlPath, mermaidHtml, 'utf-8');
        spinner.succeed(`Mermaid diagram saved to ${mermaidPath} and ${mermaidHtmlPath}`);
      // Generate Report
      if (flags.report) {
        spinner.start('Generating analysis report');
        const reportGen = new ReportGenerator(stateManager);
        const report = reportGen.generate(state);
        const reportPath = path.join(outputDir, 'analysis-report.md');
        await fs.writeFile(reportPath, report, 'utf-8');
        spinner.succeed(`Analysis report saved to ${reportPath}`);
export default class DbDocExport extends Command {
  static description = 'Export documentation in multiple formats (delegates to db-auto-doc export)';
    '<%= config.bin %> <%= command.id %> --state-file=./db-doc-state.json',
    '<%= config.bin %> <%= command.id %> --sql',
    '<%= config.bin %> <%= command.id %> --markdown',
    '<%= config.bin %> <%= command.id %> --html',
    '<%= config.bin %> <%= command.id %> --csv',
    '<%= config.bin %> <%= command.id %> --mermaid',
    '<%= config.bin %> <%= command.id %> --sql --markdown --html --csv --mermaid --apply',
    'state-file': Flags.string({
      description: 'Path to state JSON file',
      char: 's'
    'output-dir': Flags.string({
      description: 'Output directory for generated files',
      char: 'o'
    sql: Flags.boolean({
      description: 'Generate SQL script'
    markdown: Flags.boolean({
      description: 'Generate Markdown documentation'
    html: Flags.boolean({
      description: 'Generate interactive HTML documentation'
    csv: Flags.boolean({
      description: 'Generate CSV exports (tables and columns)'
    mermaid: Flags.boolean({
      description: 'Generate Mermaid ERD diagram files'
    report: Flags.boolean({
      description: 'Generate analysis report'
    apply: Flags.boolean({
      description: 'Apply SQL to database',
    'approved-only': Flags.boolean({
      description: 'Only export approved items',
    'confidence-threshold': Flags.string({
      description: 'Minimum confidence threshold',
    const { flags } = await this.parse(DbDocExport);
    const { default: ExportCommand } = await import('@memberjunction/db-auto-doc/dist/commands/export');
    if (flags['state-file']) args.push('--state-file', flags['state-file']);
    if (flags['output-dir']) args.push('--output-dir', flags['output-dir']);
    if (flags.sql) args.push('--sql');
    if (flags.markdown) args.push('--markdown');
    if (flags.html) args.push('--html');
    if (flags.csv) args.push('--csv');
    if (flags.mermaid) args.push('--mermaid');
    if (flags.report) args.push('--report');
    if (flags.apply) args.push('--apply');
    if (flags['approved-only']) args.push('--approved-only');
    if (flags['confidence-threshold']) args.push('--confidence-threshold', flags['confidence-threshold']);
    // Execute the DBAutoDoc export command
    await ExportCommand.run(args);
  static description = 'Export queries from database to metadata files';
    `<%= config.bin %> <%= command.id %>`,
    `<%= config.bin %> <%= command.id %> --output ./metadata/queries`,
    `<%= config.bin %> <%= command.id %> --verbose`,
      description: 'Output directory',
      default: './metadata/queries'
    verbose: Flags.boolean({ char: 'v', description: 'Verbose output' }),
    const { exportCommand } = await import('@memberjunction/query-gen');
    const { loadMJConfig, initializeProvider } = await import('@memberjunction/metadata-sync');
      // Load MJ configuration and initialize provider
      const mjConfig = loadMJConfig();
      if (!mjConfig) {
        this.error('No mj.config.cjs found in current directory or parent directories');
      await initializeProvider(mjConfig);
      // Convert flags to options object for QueryGen
      const options: Record<string, unknown> = {
        output: flags.output,
        verbose: flags.verbose
      // Call QueryGen export command
      await exportCommand(options);
      // QueryGen commands call process.exit(), so this may not be reached
      // But we handle it just in case
 * Export command - Export queries from database to metadata files
 * Reads existing queries from the database and exports them to metadata format:
 * - Loads all queries from Queries table
 * - Includes related Query Fields and Query Params
 * - Exports to JSON files in MJ metadata format
import { getSystemUser } from '../../utils/user-helpers';
import { extractErrorMessage } from '../../utils/error-handlers';
import { QueryMetadataRecord } from '../../data/schema';
 * Execute the export command
 * Loads queries from database and exports them to metadata files.
export async function exportCommand(options: Record<string, unknown>): Promise<void> {
  const spinner = ora('Initializing export...').start();
    const outputPath = String(options.output || './metadata/queries');
    const verbose = Boolean(options.verbose);
    // 1. Get system user from UserCache (populated by provider initialization)
    const contextUser = getSystemUser();
    // 2. Verify database connection and load metadata
    spinner.text = 'Loading metadata...';
    // Assume provider is already configured by the calling application
      throw new Error('Metadata provider not configured. Please ensure database connection is set up before running CLI.');
    spinner.succeed('Metadata loaded');
    // 3. Load queries from database
    spinner.start('Loading queries from database...');
    const queries = await loadQueriesFromDatabase(contextUser);
    spinner.succeed(chalk.green(`Found ${queries.length} queries`));
    // 4. Create output directory if it doesn't exist
    if (!fs.existsSync(outputPath)) {
      fs.mkdirSync(outputPath, { recursive: true });
    // 5. Export each query to metadata format
    let exportCount = 0;
    const errors: Array<{ query: string; error: string }> = [];
    for (let i = 0; i < queries.length; i++) {
      const query = queries[i];
      const queryPrefix = chalk.cyan(`[${i + 1}/${queries.length}]`);
      spinner.start(`${queryPrefix} Exporting ${chalk.bold(query.Name)}...`);
        const metadataRecord = await convertQueryToMetadata(query, contextUser);
        const filename = sanitizeFilename(query.Name) + '.json';
        const fullPath = path.join(outputPath, filename);
        fs.writeFileSync(fullPath, JSON.stringify(metadataRecord, null, 2), 'utf-8');
        exportCount++;
          spinner.info(`${queryPrefix} ${chalk.green('✓')} Exported ${query.Name}`);
        const errorMsg = extractErrorMessage(error, 'Query Export');
        errors.push({ query: query.Name, error: errorMsg });
          spinner.warn(`${queryPrefix} ${chalk.red('✗')} ${query.Name}: ${errorMsg}`);
    // 6. Summary
      spinner.succeed(chalk.green.bold(`✓ All ${exportCount} queries exported successfully!`));
      console.log('\n' + chalk.green.bold('✓ Export complete!\n'));
      console.log(chalk.bold('Summary:'));
      console.log(`  Total Queries: ${chalk.cyan(exportCount.toString())}`);
      console.log(`  Exported: ${chalk.green(exportCount.toString())}`);
      console.log(`  Output Location: ${chalk.dim(outputPath)}`);
      spinner.fail(chalk.yellow(`Export completed with ${errors.length} errors`));
      console.log('\n' + chalk.yellow.bold('⚠ Export completed with errors\n'));
      console.log(`  Total Queries: ${chalk.cyan(queries.length.toString())}`);
      console.log(`  Failed: ${chalk.red(errors.length.toString())}`);
        console.log('\n' + chalk.bold('Errors:'));
        for (const { query, error } of errors) {
          console.log(chalk.red(`  ${query}: ${error}`));
    spinner.fail(chalk.red('Export failed'));
    console.error(chalk.red(extractErrorMessage(error, 'Query Export')));
 * Load all queries from the database
async function loadQueriesFromDatabase(contextUser: UserInfo): Promise<MJQueryEntity[]> {
  const result = await rv.RunView<MJQueryEntity>(
    throw new Error(`Failed to load queries: ${result.ErrorMessage}`);
 * Convert MJQueryEntity to metadata record format
 * Note: For the export command, we only export the Query entity itself.
 * QueryFields and QueryParameters are managed by MJQueryEntity.server.ts and
 * will be automatically extracted when the query is imported/saved.
async function convertQueryToMetadata(
  query: MJQueryEntity,
      Name: query.Name,
      CategoryID: query.CategoryID || 'unknown',
      UserQuestion: query.UserQuestion || '',
      Description: query.Description || '',
      TechnicalDescription: query.TechnicalDescription || '',
      SQL: query.SQL || '',
      UsesTemplate: query.UsesTemplate || false,
      Status: query.Status || 'Active',
 * Sanitize filename by removing invalid characters
function sanitizeFilename(name: string): string {
    .replace(/[^a-zA-Z0-9-_\s]/g, '')
    .replace(/\s+/g, '_')
