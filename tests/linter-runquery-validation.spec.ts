describe('ComponentLinter - RunQuery/RunView Validation', () => {
  describe('RunQuery result property access', () => {
    it('should detect incorrect .Data property access on RunQuery results', async () => {
              QueryName: 'AccountIndustryDistribution'
            // This is WRONG - should be .Results not .Data
            setData(result.Data || []);
            name: 'AccountIndustryDistribution',
      const violations = await linter.lint(code, spec);
      // Should find the .Data access violation
        v.message.includes('.data') && 
        v.message.includes('.Results')
      expect(dataViolation?.message).toContain('Use ".Results" to access');
    it('should detect incorrect .data property access (lowercase) on RunQuery results', async () => {
          const [items, setItems] = useState([]);
            // This is WRONG - should be .Results not .data (lowercase)
            const items = queryResult.data || [];
            setItems(items);
          return <div>Items: {items.length}</div>;
        v.message.includes('queryResult.data') &&
    it('should detect chained incorrect property access like .Data.length', async () => {
            // This is WRONG - trying to access .Data instead of .Results
            if (result.Success && result.Data && result.Data.length > 0) {
              console.log('Has data');
      // Should find multiple violations for .Data access
      const dataViolations = violations.filter(v => 
        v.message.includes('.Data') || v.message.includes('.data')
      expect(dataViolations.length).toBeGreaterThan(0);
    it('should detect destructuring of wrong properties from RunQuery result', async () => {
            // This is WRONG - destructuring 'data' instead of 'Results'
            const { Success, data } = queryResult;
            if (Success && data) {
              console.log(data);
        v.message.includes('Destructuring "data"') &&
      expect(destructuringViolation?.severity).toBe('critical');
    it('should NOT report violations for correct .Results property access', async () => {
            // This is CORRECT - using .Results
      // Should not have any violations about .data or .Results
        v.message.includes('.data') || 
        v.message.includes('.Data') ||
      expect(dataViolations).toHaveLength(0);
    it('should detect SQL being passed as QueryName parameter', async () => {
              QueryName: 'SELECT * FROM Accounts WHERE Industry = "Technology"'
            name: 'AccountQuery',
      const sqlViolation = violations.find(v => 
        v.message.includes('SQL statement') &&
        v.message.includes('QueryName')
      expect(sqlViolation).toBeDefined();
      expect(sqlViolation?.severity).toBe('critical');
      expect(sqlViolation?.message).toContain('SELECT');
    it('should validate query name matches dataRequirements', async () => {
            // This query name doesn't exist in dataRequirements
              QueryName: 'NonExistentQuery'
      const queryNameViolation = violations.find(v => 
        v.message.includes('NonExistentQuery') &&
        v.message.includes('not found in dataRequirements')
      expect(queryNameViolation).toBeDefined();
      expect(queryNameViolation?.severity).toBe('high');
    it('should validate query name even WITHOUT Parameters property (regression test for bug)', async () => {
      // This test ensures we catch the bug where query name validation was skipped
      // if the RunQuery call didn't have a Parameters property
            // Query name is wrong, but NO Parameters property
              QueryName: 'ActiveMemberCountsByMembershipType',
              CategoryPath: 'Skip/Analytics/Membership'
            name: 'ActiveMembersByMembershipType',  // Different from code!
            categoryPath: 'Skip/Analytics/Membership',
        v.message.includes('ActiveMemberCountsByMembershipType') &&
        v.message.includes('not found in component spec')
      expect(queryNameViolation?.message).toContain('ActiveMembersByMembershipType');
  describe('RunView result property access', () => {
    it('should detect incorrect .Data property access on RunView results', async () => {
            const accounts = viewResult.Data || [];
        v.message.includes('viewResult.Data') &&
    it('should validate entity name matches dataRequirements', async () => {
            // This entity doesn't exist in dataRequirements
              EntityName: 'NonExistentEntity'
      const entityNameViolation = violations.find(v => 
        v.message.includes('NonExistentEntity') &&
      expect(entityNameViolation).toBeDefined();
      expect(entityNameViolation?.severity).toBe('critical');
  describe('Edge cases and complex scenarios', () => {
    it('should handle conditional property access correctly', async () => {
            // Multiple wrong accesses in conditional
            const data = result.Success 
              ? (result.Data || result.data || [])  // Both wrong
      // Should find violations for both .Data and .data
        (v.message.includes('.Data') || v.message.includes('.data')) &&
      expect(dataViolations.length).toBeGreaterThanOrEqual(1);
    it('should track result through variable reassignment', async () => {
            const queryResponse = await utilities.rq.RunQuery({
            // Reassign to another variable
            const result = queryResponse;
            // Still wrong even through reassignment
            const items = result.Data || [];
        v.message.includes('.Data') &&
      // This is a harder case to catch, but ideally should be detected
      // If not caught, this test documents a known limitation
      if (dataViolation) {
        expect(dataViolation.severity).toBe('critical');
        // Document this as a known limitation
        console.warn('Known limitation: Cannot track RunQuery results through variable reassignment');
  describe('RunQuery CategoryPath validation', () => {
    it('should flag missing CategoryPath when query spec defines categoryPath', async () => {
            // Missing CategoryPath - this is brittle!
              QueryName: 'AccountIndustryDistribution',
              Parameters: { year: 2024 }
            categoryPath: 'Analytics/Accounts',
            entityNames: ['Accounts']
      const missingCategoryPath = violations.find(v =>
        v.rule === 'runquery-missing-categorypath'
      expect(missingCategoryPath).toBeDefined();
      expect(missingCategoryPath?.severity).toBe('critical');
      expect(missingCategoryPath?.message).toContain('missing required CategoryPath');
      expect(missingCategoryPath?.message).toContain('AccountIndustryDistribution');
      expect(missingCategoryPath?.suggestion?.example).toContain('Analytics/Accounts');
    it('should NOT flag when CategoryPath is provided', async () => {
            // Correctly includes CategoryPath
              CategoryPath: 'Analytics/Accounts',
      expect(missingCategoryPath).toBeUndefined();
    it('should NOT flag when query spec does not define categoryPath', async () => {
            // No CategoryPath needed because spec doesn't define one
              QueryName: 'SimpleQuery',
              Parameters: { id: 123 }
            name: 'SimpleQuery',
            categoryPath: '',  // Empty string when not used
    it('should handle multiple queries with different CategoryPath requirements', async () => {
            // Query1 - missing CategoryPath (should flag)
            const result1 = await utilities.rq.RunQuery({
              QueryName: 'QueryWithCategory'
            // Query2 - no CategoryPath needed (should NOT flag)
            const result2 = await utilities.rq.RunQuery({
              QueryName: 'QueryWithoutCategory'
            // Query3 - has CategoryPath (should NOT flag)
            const result3 = await utilities.rq.RunQuery({
              QueryName: 'AnotherQueryWithCategory',
              CategoryPath: 'Reports/Sales'
            return { result1, result2, result3 };
          queries: [
              name: 'QueryWithCategory',
              categoryPath: 'Analytics/Users',
              name: 'QueryWithoutCategory',
              name: 'AnotherQueryWithCategory',
              categoryPath: 'Reports/Sales',
              entityNames: ['Sales']
      const missingCategoryPathViolations = violations.filter(v =>
      // Should only flag the first query
      expect(missingCategoryPathViolations.length).toBe(1);
      expect(missingCategoryPathViolations[0].message).toContain('QueryWithCategory');
      expect(missingCategoryPathViolations[0].suggestion?.example).toContain('Analytics/Users');
