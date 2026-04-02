export default class Validate extends Command {
  static description = 'Validate existing query templates';
    `<%= config.bin %> <%= command.id %> --path ./metadata/queries`,
      description: 'Path to queries metadata file or directory',
    const { validateCommand } = await import('@memberjunction/query-gen');
    const { flags } = await this.parse(Validate);
        path: flags.path,
      // Call QueryGen validate command
      await validateCommand(options);
  static description = 'Validate metadata files';
    `<%= config.bin %> <%= command.id %> --save-report`,
    dir: Flags.string({ description: 'Specific entity directory to validate' }),
    verbose: Flags.boolean({ char: 'v', description: 'Show detailed validation output' }),
    'save-report': Flags.boolean({ description: 'Save validation report as markdown file' }),
    'output': Flags.string({ description: 'Output file path for validation report (default: validation-report.md)' }),
      ValidationService, FormattingService, loadMJConfig, initializeProvider,
      // Initialize sync engine (needed for metadata access)
      await getSyncEngine(getSystemUser());
      // Create validation service
      // Determine directory to validate
      spinner.start(`Validating metadata in ${path.relative(process.cwd(), targetDir)}...`);
      // Run validation
      // Format and display results
      const formattedResult = formatter.formatValidationResult(validationResult, flags.verbose);
      this.log('\n' + formattedResult);
      // Save report if requested
      if (flags['save-report']) {
        const reportPath = flags.output || 'validation-report.md';
        const fullReportPath = path.resolve(reportPath);
        // Generate markdown report
        const report = formatter.formatValidationResultAsMarkdown(validationResult);
        await fs.writeFile(fullReportPath, report, 'utf8');
        this.log(chalk.green(`\n✅ Validation report saved to: ${path.relative(process.cwd(), fullReportPath)}`));
      const totalIssues = validationResult.errors.length + validationResult.warnings.length;
      if (validationResult.isValid && validationResult.warnings.length === 0) {
        this.log(chalk.green('\n✅ All metadata files are valid!'));
      } else if (validationResult.isValid && validationResult.warnings.length > 0) {
        this.log(chalk.yellow(`\n⚠️  Validation passed with ${validationResult.warnings.length} warning(s)`));
        this.log(chalk.red(`\n❌ Validation failed with ${validationResult.errors.length} error(s) and ${validationResult.warnings.length} warning(s)`));
      // Exit with error code if validation failed
        this.error('Validation failed');
      spinner.fail('Validation failed');
      this.log('\n=== Validation Error Details ===');
export default class TestValidate extends Command {
  static description = 'Validate test definitions without executing';
    '<%= config.bin %> <%= command.id %> --all',
    '<%= config.bin %> <%= command.id %> --all --save-report',
    '<%= config.bin %> <%= command.id %> --all --output=validation-report.md',
      description: 'Test ID to validate',
    all: Flags.boolean({
      description: 'Validate all tests',
      description: 'Validate tests by type',
    'save-report': Flags.boolean({
      description: 'Save validation report to file',
    const { ValidateCommand } = await import('@memberjunction/testing-cli');
    const { args, flags } = await this.parse(TestValidate);
      // Create ValidateCommand instance and execute
      const validateCommand = new ValidateCommand();
      await validateCommand.execute(args.testId, {
        all: flags.all,
        saveReport: flags['save-report'],
 * Validate command - Validate existing query templates
 * Tests existing query metadata files to ensure they are valid:
 * - SQL syntax validation
 * - Parameter validation
 * - Output field validation
 * - Execution testing (optional)
import { Metadata, DatabaseProviderBase } from '@memberjunction/core';
import { GeneratedQuery, QueryMetadataRecord } from '../../data/schema';
 * Execute the validate command
 * Loads query metadata files and validates each query template.
 * Reports success/failure statistics.
export async function validateCommand(options: Record<string, unknown>): Promise<void> {
  const spinner = ora('Initializing validation...').start();
    const queryPath = String(options.path || './metadata/queries');
    // 3. Load query metadata files
    spinner.start(`Loading query files from ${queryPath}...`);
    const queryFiles = await loadQueryFiles(queryPath);
    spinner.succeed(chalk.green(`Found ${queryFiles.length} query files`));
    // 4. Validate each query
    let passCount = 0;
    const errors: Array<{ file: string; error: string }> = [];
    for (let i = 0; i < queryFiles.length; i++) {
      const { file, queries } = queryFiles[i];
      const filePrefix = chalk.cyan(`[${i + 1}/${queryFiles.length}]`);
      spinner.start(`${filePrefix} Validating ${chalk.dim(file)}...`);
      for (const queryRecord of queries) {
          const query = convertMetadataToGeneratedQuery(queryRecord);
          // Create a minimal business question and entity metadata for testing
          const dummyQuestion = {
            userQuestion: queryRecord.fields.UserQuestion || 'Test query',
            description: queryRecord.fields.Description || '',
            technicalDescription: queryRecord.fields.TechnicalDescription || '',
            complexity: 'medium' as const,
            requiresAggregation: false,
            requiresJoins: false,
            entities: []
          const tester = new QueryTester(dataProvider, [], dummyQuestion, contextUser, config);
          // Test query execution
          const testResult = await tester.testQuery(query, 1);
            passCount++;
              spinner.info(`${filePrefix} ${chalk.green('✓')} ${queryRecord.fields.Name}`);
            const errorMsg = testResult.error || 'Unknown error';
            errors.push({ file, error: `${queryRecord.fields.Name}: ${errorMsg}` });
              spinner.warn(`${filePrefix} ${chalk.red('✗')} ${queryRecord.fields.Name}: ${errorMsg}`);
          const errorMsg = extractErrorMessage(error, 'Query Validation');
      spinner.succeed(`${filePrefix} ${chalk.dim(file)} complete`);
    // 5. Summary
    if (failCount === 0) {
      spinner.succeed(chalk.green.bold(`✓ All ${passCount} queries validated successfully!`));
      console.log('\n' + chalk.green.bold('✓ Validation complete!\n'));
      console.log(`  Total Queries: ${chalk.cyan(passCount.toString())}`);
      console.log(`  Passed: ${chalk.green(passCount.toString())}`);
      console.log(`  Failed: ${chalk.green('0')}`);
      spinner.fail(chalk.yellow(`Validation completed with ${failCount} errors`));
      console.log('\n' + chalk.yellow.bold('⚠ Validation completed with errors\n'));
      console.log(`  Total Queries: ${chalk.cyan((passCount + failCount).toString())}`);
      console.log(`  Failed: ${chalk.red(failCount.toString())}`);
        for (const { file, error } of errors) {
          console.log(chalk.red(`  ${file}: ${error}`));
    spinner.fail(chalk.red('Validation failed'));
    console.error(chalk.red(extractErrorMessage(error, 'Query Validation')));
 * Load all query metadata files from the specified directory
async function loadQueryFiles(queryPath: string): Promise<Array<{ file: string; queries: QueryMetadataRecord[] }>> {
  const files: Array<{ file: string; queries: QueryMetadataRecord[] }> = [];
  if (!fs.existsSync(queryPath)) {
    throw new Error(`Query path not found: ${queryPath}`);
  const entries = fs.readdirSync(queryPath);
    const fullPath = path.join(queryPath, entry);
    const stat = fs.statSync(fullPath);
    if (stat.isFile() && entry.endsWith('.json')) {
      const content = fs.readFileSync(fullPath, 'utf-8');
      // Handle both single query and array formats
      const queries = Array.isArray(data) ? data : [data];
      files.push({ file: entry, queries });
 * Convert metadata record to GeneratedQuery format for testing
 * Note: QueryFields and QueryParameters are auto-extracted by MJQueryEntity.server.ts,
 * so we only validate the SQL itself. The validate command focuses on SQL syntax
 * and execution, not on field/parameter metadata which is managed by MJ.
function convertMetadataToGeneratedQuery(record: QueryMetadataRecord): GeneratedQuery {
    queryName: record.fields.Name,
    sql: record.fields.SQL,
    parameters: [],    // Not needed for validation - MJQueryEntity will extract
 * @fileoverview Validate command implementation
import { ValidateFlags } from '../types';
 * Validation result for a single test
interface ValidationResult {
 * Validate command - Validate test definitions without executing
export class ValidateCommand {
     * @param testId - Optional test ID to validate
    async execute(testId: string | undefined, flags: ValidateFlags, contextUser?: UserInfo): Promise<void> {
            let testsToValidate: MJTestEntity[];
                // Validate specific test
                const test = engine.GetTestByID(testId);
                testsToValidate = [test];
                // Validate all tests
                testsToValidate = engine.Tests;
            } else if (flags.type) {
                // Validate tests by type
                    console.error(OutputFormatter.formatError(`Test type not found: ${flags.type}`));
                testsToValidate = engine.Tests.filter(t => t.TypeID === type.ID);
                console.error(OutputFormatter.formatError('Must specify test ID, --all, or --type'));
            // Validate tests
            this.spinner.start(`Validating ${testsToValidate.length} test(s)...`);
            const results = testsToValidate.map(test => this.validateTest(test, engine));
            this.displayResults(results);
            if (flags.saveReport || flags.output) {
                this.saveReport(results, reportPath);
            const hasErrors = results.some(r => !r.valid);
            process.exit(hasErrors ? 1 : 0);
            console.error(OutputFormatter.formatError('Failed to validate tests', error as Error));
     * Validate a single test
    private validateTest(test: MJTestEntity, engine: TestEngine): ValidationResult {
            testId: test.ID,
        // Check test type exists
        const testType = engine.GetTestTypeByID(test.TypeID);
            result.errors.push(`Test type not found: ${test.TypeID}`);
            result.valid = false;
        if (test.Status !== 'Active') {
            result.warnings.push(`Test is not active (status: ${test.Status})`);
        // Validate InputDefinition
        if (test.InputDefinition) {
                JSON.parse(test.InputDefinition);
                result.errors.push('InputDefinition is not valid JSON');
            result.warnings.push('No InputDefinition provided');
        // Validate ExpectedOutcomes
        if (test.ExpectedOutcomes) {
                JSON.parse(test.ExpectedOutcomes);
                result.errors.push('ExpectedOutcomes is not valid JSON');
            result.warnings.push('No ExpectedOutcomes defined');
        // Validate Configuration (OracleConfiguration and TestConfiguration combined)
        if (test.Configuration) {
                JSON.parse(test.Configuration);
                result.errors.push('Configuration is not valid JSON');
            result.warnings.push('No Configuration defined');
     * Display validation results
    private displayResults(results: ValidationResult[]): void {
        console.log(chalk.bold('\nValidating Tests...\n'));
            const symbol = result.valid ? chalk.green('✓') : chalk.red('✗');
            console.log(`${symbol} ${result.testName}`);
            if (result.valid && result.warnings.length === 0) {
                console.log(chalk.gray('  - No issues found'));
                console.log(chalk.red(`  ✗ Error: ${error}`));
                console.log(chalk.yellow(`  ⚠ Warning: ${warning}`));
        const validCount = results.filter(r => r.valid).length;
        const warningCount = results.filter(r => r.warnings.length > 0).length;
        console.log(chalk.bold('[SUMMARY]'));
        if (validCount === results.length && warningCount === 0) {
            console.log(chalk.green(`All ${results.length} test(s) are valid with no warnings`));
        } else if (validCount === results.length) {
            console.log(chalk.yellow(`${validCount}/${results.length} test(s) valid, ${warningCount} with warnings`));
            console.log(chalk.red(`${validCount}/${results.length} test(s) valid, ${results.length - validCount} with errors`));
     * Save validation report to file
    private saveReport(results: ValidationResult[], filePath: string): void {
        lines.push('# Test Validation Report');
        lines.push(`Generated: ${new Date().toISOString()}\n`);
        lines.push('## Summary\n');
        lines.push(`- **Total Tests:** ${results.length}`);
        lines.push(`- **Valid:** ${validCount}`);
        lines.push(`- **Invalid:** ${results.length - validCount}`);
        lines.push(`- **With Warnings:** ${warningCount}\n`);
        lines.push('## Test Results\n');
            const status = result.valid ? '✓ Valid' : '✗ Invalid';
            lines.push(`### ${result.testName}`);
            lines.push(`**Status:** ${status}`);
            lines.push(`**ID:** ${result.testId}\n`);
                lines.push('**Errors:**');
                lines.push('**Warnings:**');
                lines.push('No issues found.\n');
        const report = lines.join('\n');
            fs.writeFileSync(filePath, report, 'utf-8');
            console.log(OutputFormatter.formatSuccess(`Validation report saved to ${filePath}`));
            console.error(OutputFormatter.formatError(`Failed to save report to ${filePath}`, error as Error));
