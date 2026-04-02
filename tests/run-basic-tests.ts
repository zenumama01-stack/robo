 * Linter Test Suite Runner
 * Runs all linter validation tests with database connection
 *   npx ts-node run-linter-tests.ts
import { describe, it, expect, runTests, SuiteResult } from './infrastructure/test-runner';
import { initializeDatabase, getContextUser, cleanupDatabase } from './infrastructure/database-setup';
// Store context user for tests
// Base spec for query-based components (cast to any to avoid strict type checking for test spec)
const baseQuerySpec = {
  type: 'chart' as const,
  title: 'Test Component',
  description: 'Test component for linter validation',
  location: 'embedded' as const,
  functionalRequirements: 'Test requirements',
  technicalDesign: 'Test design',
  dataRequirements: {
    mode: 'queries' as const,
    queries: [{
      name: 'TestQuery',
      categoryPath: 'Test',
      entityNames: []
// Define tests
describe('OptionalMemberExpression - Basic Invalid Properties', () => {
  it('should detect result?.records (lowercase) - invalid property', async () => {
    const code = `
      function TestComponent({ utilities }) {
        const [data, setData] = React.useState([]);
            QueryName: 'TestQuery'
          // ❌ WRONG - using result?.records
          const rows = result?.records ?? [];
          setData(rows);
        return React.createElement('div', null, data.length + ' items');
    const lintResult = await ComponentLinter.lintComponent(
      'TestComponent',
      baseQuerySpec,
    const recordsViolation = lintResult.violations.find((v: any) =>
      v.message.includes('records') &&
      v.message.includes('Results')
    expect(recordsViolation).toBeDefined();
    expect(recordsViolation?.severity).toBe('critical');
  it('should detect result?.Rows (capitalized) - invalid property', async () => {
          // ❌ WRONG - using result?.Rows
          return result?.Rows ?? [];
        return React.createElement('div', null, 'Test');
    const rowsViolation = lintResult.violations.find((v: any) =>
      v.message.includes('Rows') &&
    expect(rowsViolation).toBeDefined();
    expect(rowsViolation?.severity).toBe('critical');
  it('should NOT flag result?.Results - correct property', async () => {
          // ✅ CORRECT - using result?.Results
          const data = result?.Results ?? [];
    const invalidViolations = lintResult.violations.filter((v: any) =>
      v.message.includes('.Results') &&
      v.message.includes("don't have")
    expect(invalidViolations).toHaveLength(0);
describe('Weak Fallback Chain Detection', () => {
  it('should detect result?.records ?? result?.Rows ?? [] - EXACT BUG PATTERN', async () => {
          // ❌ WRONG - This is the EXACT bug pattern
          const rows = result?.records ?? result?.Rows ?? [];
    // Should detect either individual invalid properties OR weak fallback pattern
    const relevantViolations = lintResult.violations.filter((v: any) =>
      (v.message.includes('records') || v.message.includes('Rows')) &&
      (v.message.includes('Results') || v.message.toLowerCase().includes('fallback'))
    expect(relevantViolations.length).toBeGreaterThan(0);
describe('Regression Tests - Existing Patterns Should Still Work', () => {
  it('should still catch regular member access (no optional chaining)', async () => {
          // ❌ WRONG - regular member access (existing test case)
          const data = result.records || [];
// Main execution
  console.log('║                    Linter Test Suite Runner                                  ║');
    console.log('🔄 Initializing MemberJunction...');
    // Context user is optional - only needed for library-dependent components
    // For basic property validation tests, we can skip it
    console.log('ℹ️  Running without context user (tests use simple specs without libraries)');
    contextUser = null as any; // Tests will pass undefined to lintComponent
    console.log('\n' + '='.repeat(80));
    console.log('Running Tests...');
    const results = await runTests();
    // Print summary
    console.log('📊 TEST SUMMARY');
    console.log('═'.repeat(80) + '\n');
    let totalPassed = 0;
    let totalFailed = 0;
    results.forEach((suite: SuiteResult) => {
      totalPassed += suite.passed;
      totalFailed += suite.failed;
      totalDuration += suite.duration;
    console.log(`  Total Suites: ${results.length}`);
    console.log(`  Total Tests:  ${totalPassed + totalFailed}`);
    console.log(`  Passed:       ${totalPassed} ✅`);
    console.log(`  Failed:       ${totalFailed} ${totalFailed > 0 ? '❌' : ''}`);
    console.log(`  Duration:     ${totalDuration}ms\n`);
    if (totalFailed === 0) {
      console.log('  🎉 All tests passed!\n');
      console.log('  ❌ Some tests failed\n');
