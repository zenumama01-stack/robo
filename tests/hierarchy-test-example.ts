 * Example of testing React components with hierarchies using the enhanced test harness
import { ReactTestHarness, ComponentSpec } from '@memberjunction/react-test-harness';
async function runHierarchyTests() {
  const harness = new ReactTestHarness({ debug: true });
    await harness.initialize();
    // Example 1: Test a component hierarchy with Skip-style specifications
    const dashboardSpec: ComponentSpec = {
        function Component({ title, components }) {
              <h1>{title}</h1>
              <div className="widgets">
                {components && components.MetricCard && (
                  <components.MetricCard value={42} label="Total Sales" />
                {components && components.ChartWidget && (
                  <components.ChartWidget data={[10, 20, 30]} />
            function Component({ value, label }) {
                  <div className="label">{label}</div>
          componentName: 'ChartWidget',
            function Component({ data }) {
                <div className="chart-widget">
                  <div className="chart">
                    {data.map((value, index) => (
                      <div key={index} className="bar" style={{ height: value * 5 + 'px' }}>
    console.log('Testing component hierarchy...');
      { title: 'Sales Dashboard' }
    console.log('Test completed:', result.success ? 'PASSED' : 'FAILED');
    // Example 2: Test a component from file with additional child components
    const childComponents: ComponentSpec[] = [
        componentName: 'CustomButton',
          function Component({ onClick, children }) {
              <button className="custom-button" onClick={onClick}>
    // This would test a component from a file and inject the child components
    // await harness.testComponentFromFileWithChildren(
    //   './my-component.js',
    //   childComponents,
    //   { someProp: 'value' }
    // );
    // Example 3: Testing with registerChildren disabled
    const resultNoChildren = await harness.testComponent(
      dashboardSpec.componentCode!,
      { title: 'Dashboard Without Children' },
      { registerChildren: false } // Children won't be registered
    console.log('Test without children:', resultNoChildren.success ? 'PASSED' : 'FAILED');
    await harness.close();
// Run the example
runHierarchyTests().catch(console.error);