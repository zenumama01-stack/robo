async function testRunQueryDataBug() {
  // This is the actual problematic code from the user's component
    function AccountsIndustryPieChart({ utilities, styles, components, callbacks, savedUserSettings, onSaveUserSettings }) {
      const [industryData, setIndustryData] = useState([]);
          // Load account industry distribution
          const industryResult = await utilities.rq.RunQuery({
            Parameters: {}
          if (industryResult.Success) {
            // THIS IS THE BUG - using .Data instead of .Results
            setIndustryData(industryResult.Data || []);
    name: 'AccountsIndustryPieChart',
    title: 'Accounts by Industry',
        categoryPath: 'Analytics/AccountDistribution',
          { name: 'Industry', type: 'nvarchar' },
          { name: 'AccountCount', type: 'int' }
  console.log('🔍 Testing lint rule for .Data vs .Results bug...\n');
  console.log('Code snippet being tested:');
  console.log('---');
  console.log('setIndustryData(industryResult.Data || []);  // ❌ WRONG');
  console.log('---\n');
    const lintResult: LintResult = await ComponentLinter.lintComponent(problematicCode, 'AccountsIndustryPieChart', spec, true);
    const violations = lintResult.violations;
    // Filter for RunQuery/RunView related violations
    const dataAccessViolations = violations.filter((v: Violation) => 
      v.message.toLowerCase().includes('data') || 
      v.message.toLowerCase().includes('results')
    if (dataAccessViolations.length > 0) {
      console.log('✅ SUCCESS: Lint rule caught the bug!\n');
      dataAccessViolations.forEach((v: Violation) => {
        if (v.code) {
          console.log(`    Code: ${v.code}`);
      console.log('❌ FAILURE: Lint rule did NOT catch the bug!\n');
      console.log('This code should have triggered a violation for using .Data instead of .Results');
      console.log('\nAll violations found:');
      violations.forEach((v: Violation) => {
        console.log(`  - [${v.severity}] ${v.rule}: ${v.message}`);
    // Also test the correct version
    console.log('\n---\nNow testing CORRECT code...\n');
    const correctCode = problematicCode.replace(
      'setIndustryData(industryResult.Data || []);',
      'setIndustryData(industryResult.Results || []);'
    const correctLintResult = await ComponentLinter.lintComponent(correctCode, 'AccountsIndustryPieChart', spec, true);
    const correctDataViolations = correctLintResult.violations.filter((v: Violation) => 
      v.message.toLowerCase().includes('.data') || 
      v.message.toLowerCase().includes('results property')
    if (correctDataViolations.length === 0) {
      console.log('✅ Correct code has no .Data/.Results violations');
      console.log('⚠️  Correct code still has violations:');
      correctDataViolations.forEach((v: Violation) => {
    console.error('Error running linter:', error);
testRunQueryDataBug().catch(console.error);