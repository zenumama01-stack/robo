 * Example demonstrating how to use the component hierarchy registration functionality
 * from @memberjunction/react-runtime
  type HierarchyRegistrationResult,
  type ComponentSpec
// Example Skip component root specification (similar to SkipComponentRootSpec)
const exampleRootSpec: ComponentSpec = {
  componentName: 'SalesReport',
  componentCode: `
    function SalesReport({ data, userState, utilities, callbacks, components }) {
      const { React } = utilities;
      const { SalesChart, SalesTable } = components;
      return React.createElement('div', { className: 'sales-report' },
        React.createElement('h1', null, 'Sales Report'),
        React.createElement(SalesChart, { data: data.chartData }),
        React.createElement(SalesTable, { data: data.tableData })
  childComponents: [
      componentName: 'SalesChart',
        function SalesChart({ data }) {
          return React.createElement('div', { className: 'sales-chart' },
            React.createElement('h2', null, 'Sales Chart'),
            React.createElement('div', null, 'Chart visualization here')
      components: [] // No nested children
      componentName: 'SalesTable',
        function SalesTable({ data }) {
          return React.createElement('div', { className: 'sales-table' },
            React.createElement('h2', null, 'Sales Table'),
            React.createElement('table', null, '...table content...')
// Example usage in a test harness or application
async function registerComponentHierarchyExample() {
  // Initialize compiler and registry
  const compiler = new ComponentCompiler({
    cache: true
  // Set the Babel instance (in real usage, this would come from the environment)
  // compiler.setBabelInstance(Babel);
  const registry = new ComponentRegistry({
  // Runtime context (in real usage, this would include React and other libraries)
    React: null, // Would be the actual React library
    ReactDOM: null, // Would be the actual ReactDOM library
    utilities: {}
  // Validate the component specification first
  const validationErrors = validateComponentSpec(exampleRootSpec);
    console.error('Validation errors:', validationErrors);
  // Get component statistics
  const totalComponents = countComponentsInHierarchy(exampleRootSpec, true);
  const componentsWithCode = countComponentsInHierarchy(exampleRootSpec, false);
  console.log(`Total components: ${totalComponents}, With code: ${componentsWithCode}`);
  // Flatten the hierarchy for inspection
  const allComponents = flattenComponentHierarchy(exampleRootSpec);
  console.log('All components in hierarchy:', allComponents.map(c => c.componentName));
  const result: HierarchyRegistrationResult = await registerComponentHierarchy(
    exampleRootSpec,
      styles: {
        className: 'skip-component',
        globalCss: '.skip-component { font-family: sans-serif; }'
      namespace: 'SkipComponents',
      allowOverride: false
  // Handle the registration result
    console.error('Registration failed with errors:', result.errors);
      console.warn('Warnings:', result.warnings);
  // Now the components can be retrieved from the registry
  const salesReport = registry.get('SalesReport', 'SkipComponents', 'v1');
  const salesChart = registry.get('SalesChart', 'SkipComponents', 'v1');
  const salesTable = registry.get('SalesTable', 'SkipComponents', 'v1');
  console.log('Components registered:', {
    salesReport: !!salesReport,
    salesChart: !!salesChart,
    salesTable: !!salesTable
// Usage from Angular component (simplified)
export async function registerFromAngular(
  rootSpec: any, // SkipComponentRootSpec
  runtimeContext: any,
  styles?: any
  // Convert SkipComponentRootSpec to ComponentSpec if needed
  const componentSpec: ComponentSpec = {
    componentName: rootSpec.componentName,
    componentCode: rootSpec.componentCode,
    childComponents: rootSpec.childComponents || []
  return registerComponentHierarchy(
    componentSpec,
      continueOnError: true
export { registerComponentHierarchyExample };