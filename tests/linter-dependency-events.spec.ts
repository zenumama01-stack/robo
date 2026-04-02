import { ComponentLinter } from '../src/lib/component-linter';
describe('ComponentLinter - Dependency Event Validation', () => {
  describe('dependency-prop-validation rule - events', () => {
    it('should detect incorrect event handler names passed to dependency components', async () => {
        function TestChart({ utilities, styles, components }) {
          const [data, setData] = useState([]);
          const handleClick = (clickData) => {
            console.log('Clicked:', clickData);
              valueField="Amount"
              dataPointClick={handleClick}
        name: 'TestChart',
        title: 'Test Chart Component',
        dependencies: [
            name: 'SimpleDrilldownChart',
            title: 'Simple Drilldown Chart',
            location: 'registry',
              { name: 'data', type: 'Array<object>', required: true },
              { name: 'groupBy', type: 'string', required: true },
              { name: 'valueField', type: 'string', required: false }
            events: [
              { name: 'onDataPointClick', description: 'Fired when a data point is clicked' },
              { name: 'onSegmentSelected', description: 'Fired when a segment is selected' }
      const result = await ComponentLinter.lintComponent(code, spec, 'TestChart');
      // Should find the incorrect event name (dataPointClick vs onDataPointClick)
      const eventViolation = result.violations.find(v =>
        v.rule === 'dependency-prop-validation' &&
        v.message.includes('dataPointClick')
      expect(eventViolation).toBeDefined();
      expect(eventViolation?.severity).toBe('high'); // Should suggest the correct name
      expect(eventViolation?.message).toContain('onDataPointClick');
    it('should accept correct event handler names', async () => {
              onDataPointClick={handleClick}
              { name: 'onDataPointClick', description: 'Fired when a data point is clicked' }
      // Should NOT find any violations for onDataPointClick
        v.message.includes('onDataPointClick')
      expect(eventViolation).toBeUndefined();
    it('should validate both properties and events together', async () => {
              data={[]}
              wrongPropName="test"
              wrongEventName={() => {}}
            name: 'SimpleChart',
            title: 'Simple Chart',
              { name: 'groupBy', type: 'string', required: true }
              { name: 'onChartRendered', description: 'Fired when chart is rendered' }
      // Should find both incorrect prop and event
      const violations = result.violations.filter(v =>
        (v.message.includes('wrongPropName') || v.message.includes('wrongEventName'))
      expect(violations.length).toBeGreaterThanOrEqual(2);
    it('should provide helpful suggestions for typos in event names', async () => {
              columns={['Name', 'Email']}
              onRowClicked={(row) => console.log(row)}
        type: 'table',
        title: 'Test Grid Component',
            title: 'Data Grid',
              { name: 'columns', type: 'Array<string>', required: false }
              { name: 'onRowClick', description: 'Fired when a row is clicked' },
              { name: 'onSelectionChanged', description: 'Fired when selection changes' }
      // Should suggest onRowClick (without 'ed')
        v.message.includes('onRowClicked')
      expect(eventViolation?.message).toContain('onRowClick'); // Should suggest correct name
