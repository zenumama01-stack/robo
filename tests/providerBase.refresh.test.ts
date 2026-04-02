 * Baseline Tests for ProviderBase.Refresh() and Config()
 * Purpose: Document current behavior before implementing thread safety fixes
 * These tests establish a baseline to ensure the fix doesn't break existing functionality
 * Related: METADATA_THREAD_SAFETY_IMPLEMENTATION.md - Phase 1, Task 1.3
describe('ProviderBase.Refresh - Baseline Behavior', () => {
            {}, // data
            '__mj', // MJCoreSchemaName
            [], // includeSchemas
            [], // excludeSchemas
            true // ignoreExistingMetadata - don't copy from global
    describe('Single Refresh Operations', () => {
        test('Config() loads metadata successfully', async () => {
            const result = await provider.Config(testConfig);
        test('Entities have Fields populated', async () => {
            const entity = provider.Entities[0];
            expect(entity.Fields.length).toBe(2);
            expect(entity.Fields[0].Name).toBe('ID');
        test('Refresh() triggers Config() and reloads metadata', async () => {
            const initialCount = provider.Entities.length;
            // Change mock data
            provider.setMockMetadata({
                ...await provider['GetAllMetadata'](),
                        ID: '3',
                        Name: 'New Entity',
                        BaseView: 'vwNewEntity',
                        BaseTable: 'NewEntity',
                        Fields: [{ ID: 'f5', Name: 'ID', Type: 'uniqueidentifier', IsPrimaryKey: true }]
            // Refresh
            // Should have new data
            expect(provider.Entities.length).toBe(1);
            expect(provider.Entities[0].Name).toBe('New Entity');
        test('AllowRefresh=false prevents refresh', async () => {
            // Disable refresh
            provider.setAllowRefresh(false);
            // Try to refresh
            const result = await provider.Refresh();
            // Should return true but not actually refresh
            expect(provider.Entities.length).toBe(initialCount);
    describe('Metadata Access During Operations', () => {
        test('Entities accessible immediately after Config()', async () => {
            // Should be able to access metadata immediately
        test('Entities array is stable across multiple reads', async () => {
            const read1 = provider.Entities;
            const read2 = provider.Entities;
            const read3 = provider.Entities;
            // Should return same array instance
            expect(read1).toBe(read2);
            expect(read2).toBe(read3);
        test('Fields arrays are accessible', async () => {
            const fields1 = entity.Fields;
            const fields2 = entity.Fields;
            expect(fields1).toBeDefined();
            expect(fields1).toBe(fields2); // Same array instance
            expect(fields1.length).toBeGreaterThan(0);
    describe('Current Behavior - Documenting Issues', () => {
        test('KNOWN ISSUE: Reading metadata during Config() may see empty data', async () => {
             * This test documents the current race condition.
             * When Config() is called, it immediately clears metadata (line 341 of providerBase.ts)
             * before GetAllMetadata() completes.
             * During this window, readers see empty metadata.
             * This test will FAIL before the fix and PASS after the atomic update fix.
            // Start config but don't await
            const configPromise = provider.Config(testConfig);
            // Try to read metadata during config (after slight delay)
            await new Promise(resolve => setTimeout(resolve, 10));
            const entitiesDuringConfig = provider.Entities;
            // CURRENT BEHAVIOR: This might be empty or undefined
            // DESIRED BEHAVIOR: Should return either old metadata (if any) or wait for new
            // Note: On first call, there is no "old" metadata, so empty is expected
            // Wait for config to complete
            await configPromise;
            // After config, should definitely have data
            // Document current behavior
            console.log('Entities during config:', entitiesDuringConfig?.length ?? 'undefined/empty');
            console.log('Entities after config:', provider.Entities.length);
        test('✅ FIXED: Multiple concurrent Config() calls now safe', async () => {
             * This test verifies that concurrent Config() calls don't cause race conditions.
             * With the atomic update fix, readers always see valid metadata (old or new).
             * Before fix: Metadata would be cleared, causing empty array errors
             * After fix: Metadata is atomically swapped, no intermediate empty state
            // Start 3 concurrent Config() calls
            const promises = [
                provider.Config(testConfig),
            // Wait for all to complete
            // All should succeed
            // Check efficiency - may coalesce some calls
            const callCount = provider.getCallCount();
            console.log('GetAllMetadata called:', callCount, 'times for 3 Config() calls');
            // ✅ FIXED: No more race conditions - metadata is always valid
            // DESIRED BEHAVIOR: All calls complete safely, metadata is consistent
