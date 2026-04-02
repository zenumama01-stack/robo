describe('JSONExporter', () => {
    let exporter: JSONExporter;
        exporter = new JSONExporter();
        it('should return application/json with charset', () => {
            expect(exporter.getMimeType()).toBe('application/json;charset=utf-8');
        it('should return json', () => {
            expect(exporter.getFileExtension()).toBe('json');
        it('should export data as valid JSON', async () => {
            expect(Array.isArray(parsed)).toBe(true);
            expect(parsed).toHaveLength(2);
        it('should return proper row and column counts', async () => {
                { A: 1, B: 2 },
                { A: 3, B: 4 }
        it('should pretty-print by default', async () => {
            const data = [{ ID: '1' }];
            expect(text).toContain('\n');
        it('should not pretty-print when disabled', async () => {
            const compactExporter = new JSONExporter({ prettyPrint: false });
            const result = await compactExporter.export(data);
            // Compact JSON should not have indentation newlines after opening bracket
            expect(text.startsWith('[{')).toBe(true);
            // Empty arrays succeed - they just produce empty JSON array
        it('should preserve numeric types', async () => {
            const data = [{ Count: 42, Ratio: 3.14 }];
            expect(parsed[0].Count).toBe(42);
            expect(parsed[0].Ratio).toBe(3.14);
        it('should preserve boolean types', async () => {
            expect(parsed[0].Active).toBe(true);
            expect(parsed[0].Deleted).toBe(false);
        it('should handle null values', async () => {
            const data = [{ Value: null }];
            // JSON preserves null values
            expect(parsed[0].Value).toBeNull();
        it('should handle special characters in strings', async () => {
            const data = [{ Text: 'Line1\nLine2\t"quoted"' }];
            expect(parsed[0].Text).toContain('\n');
        it('should handle large datasets', async () => {
            const data = Array.from({ length: 500 }, (_, i) => ({
                ID: i,
                Name: `Item ${i}`
            expect(result.rowCount).toBe(500);
                [1, 'Alice'],
                [2, 'Bob']
        it('should use custom indent size', async () => {
            const indentExporter = new JSONExporter({ indent: 4 });
            const result = await indentExporter.export(data);
            expect(text).toContain('    '); // 4-space indent
        it('should return correct file name', async () => {
            const result = await exporter.export([{ ID: '1' }]);
            expect(result.fileName).toContain('.json');
        it('should filter to specified columns', async () => {
            const filteredExporter = new JSONExporter({
                columns: [{ name: 'ID', displayName: 'Identifier' }]
            const data = [{ ID: '1', Name: 'Alice', Extra: 'hidden' }];
            const result = await filteredExporter.export(data);
            expect(parsed[0]).toHaveProperty('Identifier');
            expect(parsed[0]).not.toHaveProperty('Name');
            expect(parsed[0]).not.toHaveProperty('Extra');
