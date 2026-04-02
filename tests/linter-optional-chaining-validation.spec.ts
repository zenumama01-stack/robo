import { ComponentSpec } from '@memberjunction/interactivecomponents';
describe('ComponentLinter - Optional Chaining Result Property Validation', () => {
  let linter: ComponentLinter;
    linter = new ComponentLinter();
  const baseSpec: ComponentSpec = {
      mode: 'queries',
          return <div>{data.length} items</div>;
      const violations = await linter.lint(code, baseSpec);
      const recordsViolation = violations.find(v =>
      expect(recordsViolation?.message).toContain('.Results');
          return <div>Test</div>;
      const rowsViolation = violations.find(v =>
    it('should detect result?.data (lowercase) - invalid property', async () => {
            // ❌ WRONG - using result?.data
            const items = result?.data || [];
      const dataViolation = violations.find(v =>
        v.message.includes('data') &&
      expect(dataViolation).toBeDefined();
      expect(dataViolation?.severity).toBe('critical');
    it('should detect result?.Data (capitalized) - invalid property', async () => {
            // ❌ WRONG - using result?.Data
            const items = result?.Data ?? [];
        v.message.includes('Data') &&
      const invalidViolations = violations.filter(v =>
    it('should detect result?.records ?? result?.Rows ?? [] - EXACT BUG FROM broken-10.json', async () => {
            // ❌ WRONG - This is the EXACT bug pattern from broken-10.json
      // Should detect EITHER individual invalid properties OR weak fallback pattern
      const relevantViolations = violations.filter(v =>
        (v.message.includes('Results') || v.message.includes('fallback'))
      // If weak fallback detection is implemented, should have that specific violation
      const weakFallbackViolation = violations.find(v =>
        v.message.toLowerCase().includes('weak fallback') ||
        v.message.toLowerCase().includes('multiple')
      if (weakFallbackViolation) {
        expect(weakFallbackViolation.severity).toBe('critical');
        expect(weakFallbackViolation.message).toContain('Results');
    it('should detect result?.data ?? result?.rows ?? [] - multiple invalid properties', async () => {
            // ❌ WRONG - chaining multiple invalid properties
            const items = result?.data ?? result?.rows ?? [];
        (v.message.includes('data') || v.message.includes('rows')) &&
    it('should detect result?.items ?? result?.values ?? [] - other common mistakes', async () => {
            // ❌ WRONG - items and values don't exist either
            const data = result?.items ?? result?.values ?? [];
        (v.message.includes('items') || v.message.includes('values')) &&
    it('should NOT flag result?.Results ?? [] - correct fallback', async () => {
            // ✅ CORRECT - proper fallback with valid property
        v.message.includes('Results') &&
        (v.message.includes("don't have") || v.message.includes('fallback'))
  describe('RunView Results - Optional Chaining', () => {
    const viewSpec: ComponentSpec = {
        mode: 'views',
        entities: [{
          name: 'Accounts',
          displayFields: ['ID', 'Name']
    it('should detect RunView result?.records - invalid property', async () => {
            const viewResult = await utilities.rv.RunView({
              EntityName: 'Accounts'
            // ❌ WRONG - using viewResult?.records
            return viewResult?.records ?? [];
      const violations = await linter.lint(code, viewSpec);
    it('should detect RunView result?.Rows - invalid property', async () => {
            // ❌ WRONG - using viewResult?.Rows
            return viewResult?.Rows ?? [];
    it('should NOT flag RunView result?.Results - correct property', async () => {
            // ✅ CORRECT - using viewResult?.Results
            const accounts = viewResult?.Results ?? [];
            return accounts;
    it('should detect optional chaining with || fallback', async () => {
            // ❌ WRONG - using || instead of ?? but still wrong property
            const data = result?.records || [];
    it('should detect nested optional chaining', async () => {
            // ❌ WRONG - nested invalid property
            const count = result?.records?.length ?? 0;
    it('should handle ternary with optional chaining', async () => {
            // ❌ WRONG - ternary with invalid property
            const data = result?.records ? result.records : [];
      // Should catch at least one violation for .records access
      const recordsViolations = violations.filter(v =>
      expect(recordsViolations.length).toBeGreaterThan(0);
    it('should NOT flag when variable is NOT from RunQuery/RunView', async () => {
          // Some other data source, not RunQuery/RunView
          const customData = { records: [1, 2, 3] };
          // This is OK - customData is not a RunQuery/RunView result
          const items = customData?.records ?? [];
          return <div>{items.length}</div>;
      // Should NOT flag this because customData is not from RunQuery/RunView
        v.message.includes('customData') &&
      expect(recordsViolations).toHaveLength(0);
    it('should still catch destructuring with invalid properties', async () => {
            // ❌ WRONG - destructuring invalid property (existing test case)
            const { Success, records } = queryResult;
            if (Success && records) {
      const destructuringViolation = violations.find(v =>
      expect(destructuringViolation).toBeDefined();
    it('should still allow correct Result property access', async () => {
            // ✅ CORRECT - proper pattern
              setData(result.Results || []);
              console.error('Query failed:', result.ErrorMessage);
      // Should NOT have violations for valid properties
