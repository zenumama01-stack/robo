import { ComponentLinter, Violation, LintResult } from '../src/lib/component-linter';
async function testDependencyShadowing() {
  console.log('🔍 Testing dependency-shadowing lint rule...\n');
  // The problematic code from the user's component
  const problematicCode = `
function AccountIndustryChart({ utilities, styles, components, callbacks, savedUserSettings, onSaveUserSettings }) {
  const [allData, setAllData] = useState([]);
  const [displayedData, setDisplayedData] = useState([]);
  // BAD: These components shadow the dependencies!
  const IndustryFilterDropdown = ({ options, selected, onFilterChange, styles, utilities, components }) => {
    const industries = [...new Set(allData.map(item => item.Industry))].filter(Boolean);
      <select value={selected || ''}>
        <option value="">All Industries</option>
        {industries.map(industry => (
          <option key={industry} value={industry}>{industry}</option>
  const ResetButton = ({ onReset, styles, utilities, components }) => (
    <button onClick={onReset}>
  const IndustryPieChart = ({ data, onSliceClick, styles, utilities, components }) => {
        {data.length === 0 ? 'No data to display' : 'Chart placeholder'}
      <IndustryFilterDropdown 
        options={allData}
        selected={null}
        onFilterChange={() => {}}
      <ResetButton onReset={() => {}} />
      <IndustryPieChart data={displayedData} onSliceClick={() => {}} />
    name: 'AccountIndustryChart',
    title: 'Account Industry Distribution',
        name: 'IndustryPieChart',
        description: 'Renders a pie chart',
        code: 'function IndustryPieChart() { return <div>Real Chart</div>; }'
        name: 'IndustryFilterDropdown',
        description: 'Filter dropdown',
        type: 'form',
        code: 'function IndustryFilterDropdown() { return <select></select>; }'
        name: 'ResetButton',
        description: 'Reset button',
        type: 'button',
        code: 'function ResetButton() { return <button>Reset</button>; }'
  console.log('Testing PROBLEMATIC code with inline components that shadow dependencies...\n');
  const lintResult: LintResult = await ComponentLinter.lintComponent(
    problematicCode, 
    'AccountIndustryChart', 
  const shadowingViolations = lintResult.violations.filter((v: Violation) => 
    v.rule === 'dependency-shadowing'
  if (shadowingViolations.length > 0) {
    console.log('✅ SUCCESS: Lint rule caught the shadowing issues!\n');
    console.log('Violations found:');
    shadowingViolations.forEach((v: Violation) => {
      console.log(`  - [${v.severity}] Line ${v.line}: ${v.message}`);
    console.log('❌ FAILURE: Lint rule did NOT catch the shadowing!\n');
    console.log('Expected to find violations for IndustryPieChart, IndustryFilterDropdown, and ResetButton');
  // Test correct code that properly uses dependencies
  console.log('\n---\nTesting CORRECT code that properly uses dependencies...\n');
  const correctCode = `
  // GOOD: Destructure from components prop
  const { IndustryPieChart, IndustryFilterDropdown, ResetButton } = components;
  const correctLintResult = await ComponentLinter.lintComponent(
    correctCode, 
  const correctShadowingViolations = correctLintResult.violations.filter((v: Violation) => 
  if (correctShadowingViolations.length === 0) {
    console.log('✅ Correct code has no dependency-shadowing violations');
    console.log('⚠️  Correct code unexpectedly has violations:');
    correctShadowingViolations.forEach((v: Violation) => {
      console.log(`  - [${v.severity}] ${v.message}`);
  // Test alternative correct approach with direct access
  console.log('\n---\nTesting alternative approach with direct components.X access...\n');
  const directAccessCode = `
  // GOOD: Access via components.X directly
      <components.IndustryFilterDropdown 
      <components.ResetButton onReset={() => {}} />
      <components.IndustryPieChart data={displayedData} onSliceClick={() => {}} />
  const directLintResult = await ComponentLinter.lintComponent(
    directAccessCode, 
  const directShadowingViolations = directLintResult.violations.filter((v: Violation) => 
  if (directShadowingViolations.length === 0) {
    console.log('✅ Direct access code has no dependency-shadowing violations');
    console.log('⚠️  Direct access code unexpectedly has violations:');
    directShadowingViolations.forEach((v: Violation) => {
testDependencyShadowing().catch(console.error);