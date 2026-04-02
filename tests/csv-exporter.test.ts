import { CSVExporter } from '../csv-exporter';
describe('CSVExporter', () => {
    let exporter: CSVExporter;
        exporter = new CSVExporter();
    describe('getMimeType', () => {
        it('should return text/csv with charset', () => {
            expect(exporter.getMimeType()).toBe('text/csv;charset=utf-8');
    describe('getFileExtension', () => {
        it('should return csv', () => {
            expect(exporter.getFileExtension()).toBe('csv');
    describe('export', () => {
        it('should export simple object data to CSV', async () => {
                { ID: '1', Name: 'Alice' },
                { ID: '2', Name: 'Bob' }
            const result = await exporter.export(data);
            expect(result.rowCount).toBe(2);
            expect(result.columnCount).toBe(2);
            expect(result.data).toBeDefined();
        it('should include headers by default', async () => {
            const data = [{ Col1: 'value1' }];
            // Decode the buffer to check content
            const text = new TextDecoder().decode(result.data);
            expect(text).toContain('Col1');
            expect(text).toContain('value1');
        it('should return proper mime type and file name in result', async () => {
            const data = [{ A: '1' }];
            expect(result.mimeType).toBe('text/csv;charset=utf-8');
            expect(result.fileName).toContain('.csv');
        it('should handle empty data array', async () => {
            const result = await exporter.export([]);
            expect(result.error).toContain('No columns');
        it('should handle values with commas', async () => {
            const data = [{ Description: 'Hello, World' }];
            // Should be quoted since it contains a comma
            expect(text).toContain('"Hello, World"');
        it('should handle values with double quotes', async () => {
            const data = [{ Text: 'She said "hello"' }];
            // Quotes should be doubled
            expect(text).toContain('""hello""');
        it('should handle values with newlines', async () => {
            const data = [{ Notes: 'Line 1\nLine 2' }];
            expect(text).toContain('"Line 1\nLine 2"');
        it('should handle null and undefined values', async () => {
            const data = [{ A: null, B: undefined }];
        it('should handle boolean values', async () => {
            const data = [{ Active: true, Deleted: false }];
            expect(text).toContain('TRUE');
            expect(text).toContain('FALSE');
        it('should handle Date values', async () => {
            const testDate = new Date('2025-01-15T12:00:00Z');
            const data = [{ CreatedAt: testDate }];
            expect(text).toContain('2025');
        it('should handle large datasets efficiently', async () => {
            const data = Array.from({ length: 1000 }, (_, i) => ({
                ID: String(i),
                Name: `Row ${i}`,
                Value: String(Math.random())
            expect(result.rowCount).toBe(1000);
        it('should handle array-of-arrays data', async () => {
                [1, 'Alice', true],
                [2, 'Bob', false]
        it('should derive column names from object keys', async () => {
            const data = [{ firstName: 'John', lastName: 'Doe' }];
            // formatColumnName converts camelCase to "First Name"
            expect(text).toContain('First Name');
            expect(text).toContain('Last Name');
    describe('custom delimiter', () => {
        it('should use semicolon as delimiter', async () => {
            const semicolonExporter = new CSVExporter({ delimiter: ';' });
            const data = [{ A: '1', B: '2' }];
            const result = await semicolonExporter.export(data);
            expect(text).toContain(';');
        it('should use tab as delimiter', async () => {
            const tabExporter = new CSVExporter({ delimiter: '\t' });
            const result = await tabExporter.export(data);
            expect(text).toContain('\t');
