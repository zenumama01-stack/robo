 * Basic Usage Example for QueryGen
 * This example demonstrates the fundamental workflow:
 * 1. Load entities
 * 2. Create entity groups
 * 3. Generate business questions
 * 4. Generate SQL queries
 * 5. Test queries
 * 6. Export results
  ValidatedQuery
 * Main function demonstrating basic QueryGen usage
  console.log('QueryGen Basic Usage Example\n');
  // Step 1: Setup
  // Create context user for server-side operations
  // Ensure metadata provider is configured
    console.error('Please configure database connection before running this example');
  // Step 2: Load Entities
  console.log('Step 1: Loading entities...');
  // Filter to specific entity for this example
  const targetEntityName = 'Customers'; // Change this to test with different entities
  const entity = md.Entities.find(e => e.Name === targetEntityName);
    console.error(`ERROR: Entity '${targetEntityName}' not found`);
  console.log(`✓ Found entity: ${entity.Name}\n`);
  // Step 3: Create Entity Groups
  console.log('Step 2: Creating entity groups...');
  // Create groups of 1-2 entities
  const groups = await grouper.generateEntityGroups([entity], 1, 2);
  console.log(`✓ Created ${groups.length} entity group(s)\n`);
  // Show first group
  console.log('Entity group:', {
    entities: entityGroup.entities.map(e => e.Name),
    relationshipType: entityGroup.relationshipType,
    primaryEntity: entityGroup.primaryEntity.Name
  // Step 4: Generate Business Questions
  console.log('Step 3: Generating business questions...');
  let questions;
    questions = await questionGen.generateQuestions(entityGroup);
    console.log(`✓ Generated ${questions.length} business question(s)\n`);
    // Show questions
    questions.forEach((q, i) => {
      console.log(`Question ${i + 1}:`);
      console.log(`  User Question: ${q.userQuestion}`);
      console.log(`  Description: ${q.description}`);
      console.log(`  Complexity: ${q.complexity}`);
      console.log(`  Requires Aggregation: ${q.requiresAggregation}`);
      console.log(`  Requires Joins: ${q.requiresJoins}`);
    console.error('ERROR generating questions:', extractErrorMessage(error, 'Question Generation'));
  // Step 5: Generate SQL Query
  console.log('Step 4: Generating SQL query...');
  // Format entity metadata for prompt
  // Use first question for this example
  const businessQuestion = questions[0];
  // For this basic example, we'll skip few-shot learning (no golden queries)
  const fewShotExamples: any[] = [];
  let generatedQuery;
    generatedQuery = await queryWriter.generateQuery(
      businessQuestion,
    console.log('✓ Generated SQL query\n');
    console.log('SQL Template:');
    console.log(generatedQuery.sql);
    console.log('Parameters:', generatedQuery.parameters.length);
    generatedQuery.parameters.forEach(param => {
      console.log(`  - ${param.name} (${param.type}): ${param.description}`);
    console.log('Output Fields:', generatedQuery.selectClause.length);
    generatedQuery.selectClause.forEach(field => {
      console.log(`  - ${field.name} (${field.type}): ${field.description}`);
    console.error('ERROR generating query:', extractErrorMessage(error, 'Query Generation'));
  // Step 6: Test Query
  console.log('Step 5: Testing query...');
  let testResult;
    // Test with up to 5 error-fixing attempts
    testResult = await tester.testQuery(generatedQuery, 5);
    if (testResult.success) {
      console.log(`✓ Query test passed in ${testResult.attempts} attempt(s)`);
      // Show sample results (first 3 rows)
      if (testResult.sampleRows && testResult.sampleRows.length > 0) {
        console.log('Sample Results (first 3 rows):');
        testResult.sampleRows.slice(0, 3).forEach((row, i) => {
          console.log(`Row ${i + 1}:`, JSON.stringify(row, null, 2));
      console.error(`✗ Query test failed after ${testResult.attempts} attempt(s)`);
      console.error('  Error:', testResult.error);
      console.log('Note: This example will continue with export despite test failure');
    console.error('ERROR testing query:', extractErrorMessage(error, 'Query Testing'));
    console.log('Note: This example will continue with export despite test error');
  // Step 7: Export Results
  console.log('Step 6: Exporting query...');
  // Create ValidatedQuery object
  const validatedQuery: ValidatedQuery = {
    query: generatedQuery,
    testResult: testResult || {
      error: 'Test was skipped or failed',
      attempts: 0
    evaluation: {
      answersQuestion: true,
      confidence: 0.8,
      reasoning: 'Basic example - no formal evaluation performed',
      needsRefinement: false
      [validatedQuery],
    console.log(`✓ Query exported successfully`);
    console.error('ERROR exporting query:', extractErrorMessage(error, 'Query Export'));
  console.log('═'.repeat(80));
  console.log('Basic QueryGen Workflow Complete!');
  console.log(`  Entity: ${entity.Name}`);
  console.log(`  Questions Generated: ${questions.length}`);
  console.log(`  Query Generated: Yes`);
  console.log(`  Query Tested: ${testResult?.success ? 'Passed' : 'Failed/Skipped'}`);
  console.log(`  Query Exported: Yes`);
  console.log('Next Steps:');
  console.log('  1. Review the exported query metadata file');
  console.log('  2. Test the query with different parameter values');
  console.log('  3. Run the advanced-usage.ts example for refinement workflow');
    console.error('✗ Example failed:', extractErrorMessage(error, 'Basic Usage Example'));
