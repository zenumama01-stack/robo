async function testWhitelistValidation() {
  console.log('🔍 Testing WHITELIST approach for RunQuery/RunView validation...\n');
  // Test various incorrect property accesses that should be caught
  const testCases = [
      name: '.Data (uppercase)',
      code: 'result.Data',
      expected: true
      name: '.data (lowercase)',
      code: 'result.data',
      name: '.rows',
      code: 'result.rows',
      name: '.Rows',
      code: 'result.Rows',
      name: '.records',
      code: 'result.records',
      name: '.items',
      code: 'result.items',
      name: '.dataset',
      code: 'result.dataset',
      name: '.response',
      code: 'result.response',
      name: '.count (not a valid property)',
      code: 'result.count',
      name: '.length (array property on result object)',
      code: 'result.length',
      name: '.Results (correct)',
      code: 'result.Results',
      expected: false
      name: '.Success (correct)',
      code: 'result.Success',
      name: '.RowCount (correct)',
      code: 'result.RowCount',
      name: '.ErrorMessage (correct)',
      code: 'result.ErrorMessage',
  } as any as ComponentSpec;  // Use any to bypass missing required fields for testing
  for (const testCase of testCases) {
          // Test access
          const testData = ${testCase.code};
          setData(testData || []);
    const lintResult: LintResult = await ComponentLinter.lintComponent(code, 'TestComponent', spec, true);
    // Check for property access violations
    const propertyViolations = violations.filter((v: Violation) => 
      v.message.toLowerCase().includes('property') &&
      (v.message.includes('result.') || v.message.includes('Results'))
    const hasViolation = propertyViolations.length > 0;
    const passed = hasViolation === testCase.expected;
    if (passed) {
      console.log(`✅ ${testCase.name}: ${hasViolation ? 'Correctly caught' : 'Correctly allowed'}`);
      console.log(`❌ ${testCase.name}: ${hasViolation ? 'Incorrectly caught' : 'Should have been caught'}`);
      if (hasViolation) {
        console.log(`   Found violations: ${propertyViolations.map(v => v.message).join('; ')}`);
  console.log('\n---\nSummary:');
  console.log(`✅ Passed: ${passCount}/${testCases.length}`);
  console.log(`❌ Failed: ${failCount}/${testCases.length}`);
  // Test destructuring with whitelist approach
  console.log('\n---\nTesting destructuring validation...\n');
  const destructuringCode = `
        // Try to destructure various invalid properties
        const { Success, data, rows, items, Results } = queryResult;
        return Results;
  const destructResult = await ComponentLinter.lintComponent(destructuringCode, 'TestComponent', spec, true);
  const destructViolations = destructResult.violations.filter((v: Violation) =>
    v.message.includes('Destructuring')
  console.log(`Found ${destructViolations.length} destructuring violations:`);
  destructViolations.forEach((v: Violation) => {
    console.log(`  - ${v.message.split('.')[0]}`);
  // The whitelist approach should catch 'data', 'rows', 'items' but allow 'Success' and 'Results'
  const invalidProps = ['data', 'rows', 'items'];
  const validProps = ['Success', 'Results'];
  for (const prop of invalidProps) {
    const found = destructViolations.some(v => v.message.includes(`"${prop}"`));
      console.log(`✅ Correctly caught destructuring of invalid property: ${prop}`);
      console.log(`❌ Failed to catch destructuring of invalid property: ${prop}`);
  for (const prop of validProps) {
    // Check if there's a violation specifically about this property being invalid
    const found = destructViolations.some(v => 
      v.message.includes(`invalid property "${prop}"`) ||
      v.message.includes(`Destructuring "${prop}" from`)
      console.log(`✅ Correctly allowed destructuring of valid property: ${prop}`);
      console.log(`❌ Incorrectly caught destructuring of valid property: ${prop}`);
      const violation = destructViolations.find(v => 
      if (violation) {
        console.log(`   Violation message: ${violation.message}`);
testWhitelistValidation().catch(console.error);