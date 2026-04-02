export default class Generate extends Command {
  static description = 'Generate SQL query templates for entities using AI';
    `<%= config.bin %> <%= command.id %> --entities "Customers,Orders"`,
    `<%= config.bin %> <%= command.id %> --max-entities 5 --verbose`,
    `<%= config.bin %> <%= command.id %> --mode database`,
    entities: Flags.string({
      description: 'Specific entities to generate queries for (comma-separated)',
      multiple: false
    'exclude-entities': Flags.string({
      char: 'x',
      description: 'Entities to exclude (comma-separated)',
    'exclude-schemas': Flags.string({
      description: 'Schemas to exclude (comma-separated)',
    'max-entities': Flags.integer({
      description: 'Max entities per group',
      default: 3
    'target-groups': Flags.integer({
      description: 'Target number of entity groups to generate',
      default: 75
    'max-refinements': Flags.integer({
      description: 'Max refinement iterations',
    'max-fixes': Flags.integer({
      description: 'Max error-fixing attempts',
      default: 5
    model: Flags.string({ description: 'Preferred AI model' }),
    vendor: Flags.string({ description: 'Preferred AI vendor' }),
      description: 'Output directory'
    mode: Flags.string({
      description: 'Output mode: metadata|database|both',
      options: ['metadata', 'database', 'both']
    const { generateCommand } = await import('@memberjunction/query-gen');
    const { loadMJConfig, initializeProvider, getSystemUser } = await import('@memberjunction/metadata-sync');
    const { flags } = await this.parse(Generate);
      // Get system user and initialize AI Engine
      const systemUser = getSystemUser();
        entities: flags.entities,
        excludeEntities: flags['exclude-entities'],
        excludeSchemas: flags['exclude-schemas'],
        maxEntities: flags['max-entities'],
        targetGroupCount: flags['target-groups'],
        maxRefinements: flags['max-refinements'],
        maxFixes: flags['max-fixes'],
        vendor: flags.vendor,
        mode: flags.mode,
      // Call QueryGen generate command
      await generateCommand(options);
 * Generate command - Main query generation workflow
 * Orchestrates the full query generation pipeline:
 * 1. Load entity groups
 * 2. Generate business questions
 * 3. Generate SQL queries
 * 4. Test and fix queries
 * 5. Refine queries
import { Metadata, DatabaseProviderBase, EntityInfo, LogStatus } from '@memberjunction/core';
import { loadConfig } from '../config';
import { EntityGrouper } from '../../core/EntityGrouper';
import { QuestionGenerator } from '../../core/QuestionGenerator';
import { QueryWriter } from '../../core/QueryWriter';
import { QueryTester } from '../../core/QueryTester';
import { QueryRefiner } from '../../core/QueryRefiner';
import { MetadataExporter } from '../../core/MetadataExporter';
import { QueryDatabaseWriter } from '../../core/QueryDatabaseWriter';
import { EmbeddingService } from '../../vectors/EmbeddingService';
import { SimilaritySearch } from '../../vectors/SimilaritySearch';
import { formatEntityMetadataForPrompt } from '../../utils/entity-helpers';
import { buildQueryCategory, extractUniqueCategories } from '../../utils/category-builder';
import { ValidatedQuery, GoldenQuery, EntityGroup, QueryCategoryInfo } from '../../data/schema';
import { readFileSync } from 'fs';
 * Execute the generate command
 * Full orchestration of query generation workflow with progress reporting.
 * Uses ora for spinners and chalk for colored output.
export async function generateCommand(options: Record<string, unknown>): Promise<void> {
  const spinner = ora('Initializing query generation...').start();
    // 1. Load configuration
    spinner.text = 'Loading configuration...';
    const config = loadConfig(options);
    // Show model/vendor overrides if configured
    if (config.modelOverride || config.vendorOverride) {
      const overrideMsg = [];
      if (config.modelOverride) overrideMsg.push(`Model: ${config.modelOverride}`);
      if (config.vendorOverride) overrideMsg.push(`Vendor: ${config.vendorOverride}`);
      spinner.info(chalk.cyan(`Using overrides - ${overrideMsg.join(', ')}`));
      spinner.info(chalk.dim('Configuration loaded'));
      console.log(chalk.dim(JSON.stringify(config, null, 2)));
    // 2. Get system user from UserCache (populated by provider initialization)
    // 3. Verify database connection and metadata
    // Assume provider and AIEngine are already configured by the calling application (MJCLI)
    // 4. Filter and build entity groups
    spinner.start('Filtering entities...');
    // DIAGNOSTIC: Log entity filtering context
      LogStatus(`\n=== Entity Filtering Diagnostics ===`);
      LogStatus(`Total entities in metadata: ${md.Entities.length}`);
      LogStatus(`Excluded schemas: ${config.excludeSchemas.join(', ')}`);
      // Show schema distribution BEFORE filtering
      const schemasBefore = new Map<string, number>();
      md.Entities.forEach(e => {
        const schema = e.SchemaName || 'null';
        schemasBefore.set(schema, (schemasBefore.get(schema) || 0) + 1);
      LogStatus(`Schema distribution (before filtering):`);
      Array.from(schemasBefore.entries())
        .forEach(([schema, count]) => {
          const excluded = config.excludeSchemas.includes(schema) ? ' [EXCLUDED]' : '';
          LogStatus(`  ${schema}: ${count} entities${excluded}`);
    // Apply entity filtering (includeEntities takes precedence over excludeEntities)
    let filteredEntities = md.Entities.filter(
      e => !config.excludeSchemas.includes(e.SchemaName || '')
      LogStatus(`\nAfter schema filtering: ${filteredEntities.length} entities remaining`);
      if (filteredEntities.length > 0) {
        LogStatus(`Sample entity names: ${filteredEntities.slice(0, 5).map(e => e.Name).join(', ')}`);
        const schemasAfter = new Set(filteredEntities.map(e => e.SchemaName || 'null'));
        LogStatus(`Remaining schemas: ${Array.from(schemasAfter).join(', ')}`);
    if (config.includeEntities.length > 0) {
      // Allowlist: only include specified entities
      filteredEntities = filteredEntities.filter(e => config.includeEntities.includes(e.Name));
      spinner.info(chalk.dim(`Including only ${config.includeEntities.length} specified entities`));
    } else if (config.excludeEntities.length > 0) {
      // Denylist: exclude specified entities
      filteredEntities = filteredEntities.filter(e => !config.excludeEntities.includes(e.Name));
      spinner.info(chalk.dim(`Excluded ${config.excludeEntities.length} entities`));
      LogStatus(`Final filtered entities: ${filteredEntities.length}`);
      LogStatus('====================================\n');
    // 4. Group entities by schema and generate entity groups
    spinner.text = 'Analyzing entity relationships...';
    // Count entities per schema for informational logging
    const schemaCount = new Set(filteredEntities.map(e => e.SchemaName)).size;
    if (config.verbose && schemaCount > 1) {
      spinner.info(chalk.dim(`Processing ${schemaCount} schemas separately`));
    const grouper = new EntityGrouper(config);
    const entityGroups = await grouper.generateEntityGroups(filteredEntities, contextUser);
    spinner.succeed(chalk.green(`Found ${entityGroups.length} entity groups across ${schemaCount} ${schemaCount === 1 ? 'schema' : 'schemas'}`));
    // 5. Initialize vector similarity search
    spinner.start('Embedding golden queries...');
    const embeddingService = new EmbeddingService(config.embeddingModel);
    const goldenQueries = await loadGoldenQueries(config);
    spinner.succeed(chalk.green(`Embedded ${goldenQueries.length} golden queries`));
    // 5b. Build category structure for all entity groups upfront
    spinner.start('Building category structure...');
    const categoryMap = new Map<string, QueryCategoryInfo>();
    for (const group of entityGroups) {
      const category = buildQueryCategory(config, group);
      // Use primary entity name as key for lookup during query generation
      categoryMap.set(group.primaryEntity.Name, category);
    const uniqueCategories = extractUniqueCategories(Array.from(categoryMap.values()));
    spinner.succeed(chalk.green(`Created ${uniqueCategories.length} ${uniqueCategories.length === 1 ? 'category' : 'categories'}`));
    // 6. Generate queries for each entity group
    const totalGroups = entityGroups.length;
    let processedGroups = 0;
      processedGroups++;
      const groupPrefix = chalk.cyan(`[${processedGroups}/${totalGroups}]`);
      spinner.start(`${groupPrefix} Processing ${chalk.bold(group.primaryEntity.Name)}...`);
      let queriesCreatedForGroup = 0;
        // 6a. Generate business questions for this entity group
        const questionGenerator = new QuestionGenerator(contextUser, config);
        const questions = await questionGenerator.generateQuestions(group);
          spinner.info(`${groupPrefix} Generated ${questions.length} questions`);
        // 6b. For each question, generate and validate query
        for (const question of questions) {
          spinner.text = `${groupPrefix} Generating query: ${chalk.italic(question.userQuestion)}`;
          // Embed question for similarity search
          const questionEmbedding = await embeddingService.embedQuery({
            technicalDescription: question.technicalDescription,
          // Find similar golden queries
            questionEmbedding,
            config.topSimilarQueries
          const fewShotExamples = fewShotResults.map(s => s.query);
          // Generate SQL query
          const queryWriter = new QueryWriter(contextUser, config);
            group.entities.map((e: EntityInfo) => formatEntityMetadataForPrompt(e, group.entities)),
          // Test and fix query
          // Access the database provider through Metadata.Provider
          const dataProvider = Metadata.Provider as DatabaseProviderBase;
          const entityMetadata = group.entities.map((e: EntityInfo) => formatEntityMetadataForPrompt(e, group.entities));
          const queryTester = new QueryTester(
          const testResult = await queryTester.testQuery(
            config.maxFixingIterations
            spinner.warn(
              chalk.yellow(`${groupPrefix} Query failed after ${config.maxFixingIterations} attempts: ${question.userQuestion}`)
          // Refine query
          const queryRefiner = new QueryRefiner(queryTester, contextUser, config);
          const refinedResult = await queryRefiner.refineQuery(
            config.maxRefinementIterations
          // Get pre-built category from map
          const category = categoryMap.get(group.primaryEntity.Name);
            throw new Error(`Category not found for entity group: ${group.primaryEntity.Name}`);
          allValidatedQueries.push({
            query: refinedResult.query,
            testResult: refinedResult.testResult,
            evaluation: refinedResult.evaluation,
            entityGroup: group,
          queriesCreatedForGroup++;
            spinner.info(`${groupPrefix} ${chalk.green('✓')} ${question.userQuestion}`);
        // Format entity list with primary entity in bold
        const entityDisplay = group.entities
          .map(e => e.Name === group.primaryEntity.Name ? chalk.bold(e.Name) : e.Name)
        spinner.succeed(
          `${groupPrefix} ${entityDisplay} complete (${chalk.green(queriesCreatedForGroup + ' queries')})`
          chalk.yellow(
            `${groupPrefix} Error processing ${entityDisplay}: ${extractErrorMessage(error, 'Query Generation')}`
    // 7. Export results
    spinner.start(`Exporting ${allValidatedQueries.length} queries...`);
    if (config.outputMode === 'metadata' || config.outputMode === 'both') {
        uniqueCategories,
      spinner.succeed(chalk.green(`Exported to ${exportResult.outputPath}`));
    if (config.outputMode === 'database' || config.outputMode === 'both') {
      spinner.succeed(chalk.green(`Wrote ${allValidatedQueries.length} queries to database`));
    // 8. Summary
    console.log('\n' + chalk.green.bold('✓ Query generation complete!\n'));
    console.log(`  Entity Groups Processed: ${chalk.cyan(processedGroups.toString())}`);
    console.log(`  Queries Generated: ${chalk.green(allValidatedQueries.length.toString())}`);
    console.log(`  Output Location: ${chalk.dim(config.outputDirectory)}`);
    spinner.fail(chalk.red('Query generation failed'));
    console.error(chalk.red(extractErrorMessage(error, 'Query Generation')));
 * Load golden queries from compile-time imported JSON data
 * Golden queries are example queries used for few-shot learning.
 * They are imported at compile time from data/golden-queries.json.
 * This approach is more efficient than runtime file I/O and provides
 * type safety through TypeScript's resolveJsonModule feature.
 * @param config - QueryGen configuration with verbose flag
 * @returns Array of golden queries
async function loadGoldenQueries(config: { verbose: boolean }): Promise<GoldenQuery[]> {
    // Load golden queries from JSON file using ES module compatible path resolution
    const goldenQueriesPath = join(__dirname, '../../data/golden-queries.json');
    const goldenQueries = goldenQueriesData as GoldenQuery[];
    // Validate that it's an array
        LogStatus('[Warning] Golden queries data is not an array');
      LogStatus(`[Info] Loaded ${goldenQueries.length} golden queries for few-shot learning`);
      LogStatus(`[Warning] Failed to load golden queries: ${extractErrorMessage(error, 'loadGoldenQueries')}`);
