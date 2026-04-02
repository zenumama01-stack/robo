import { CSVGenerator } from '../generators/CSVGenerator';
import { DatabaseDocumentation } from '../types/state';
function createState(): DatabaseDocumentation {
    version: '1.0.0',
      createdAt: '2024-01-01', lastModified: '2024-01-01',
      totalIterations: 1, totalPromptsRun: 1, totalInputTokens: 0,
      totalOutputTokens: 0, totalTokens: 0, estimatedCost: 0,
      totalSchemas: 1, totalTables: 1, totalColumns: 3
    database: { name: 'TestDB', server: 'localhost', analyzedAt: '2024-01-01' },
    phases: { descriptionGeneration: [] },
    schemas: [
        name: 'dbo',
        tables: [
            rowCount: 100,
            dependencyLevel: 0,
            dependsOn: [],
            dependents: [],
              { name: 'ID', dataType: 'int', isNullable: false, isPrimaryKey: true, isForeignKey: false, descriptionIterations: [], description: 'Primary key' },
              { name: 'Name', dataType: 'nvarchar(100)', isNullable: true, isPrimaryKey: false, isForeignKey: false, descriptionIterations: [], description: 'User name' },
              { name: 'Status', dataType: 'nvarchar(50)', isNullable: false, isPrimaryKey: false, isForeignKey: true, foreignKeyReferences: { schema: 'dbo', table: 'Statuses', column: 'ID', referencedColumn: 'ID' }, descriptionIterations: [{ description: 'Status ref', reasoning: 'Links', generatedAt: '2024-01-01', modelUsed: 'test', confidence: 0.85 }], description: 'Status reference' }
            descriptionIterations: [
              { description: 'User accounts table', reasoning: 'Contains users', generatedAt: '2024-01-01', modelUsed: 'test', confidence: 0.9, triggeredBy: 'initial' as const }
            description: 'User accounts table',
            userApproved: true
        descriptionIterations: []
describe('CSVGenerator', () => {
  let generator: CSVGenerator;
    generator = new CSVGenerator();
  describe('generate', () => {
    it('should return both tables and columns CSV', () => {
      const result = generator.generate(createState());
      expect(result.tables).toBeDefined();
      expect(result.columns).toBeDefined();
    it('should include CSV headers in tables CSV', () => {
      expect(result.tables).toContain('Schema');
      expect(result.tables).toContain('Table');
      expect(result.tables).toContain('Description');
      expect(result.tables).toContain('Row Count');
    it('should include CSV headers in columns CSV', () => {
      expect(result.columns).toContain('Schema');
      expect(result.columns).toContain('Table');
      expect(result.columns).toContain('Column');
      expect(result.columns).toContain('Data Type');
      expect(result.columns).toContain('Is Primary Key');
    it('should include table data', () => {
      expect(result.tables).toContain('dbo');
      expect(result.tables).toContain('Users');
      expect(result.tables).toContain('User accounts table');
      expect(result.tables).toContain('100');
    it('should include column data', () => {
      expect(result.columns).toContain('ID');
      expect(result.columns).toContain('int');
      expect(result.columns).toContain('Primary key');
    it('should include foreign key references', () => {
      expect(result.columns).toContain('dbo.Statuses.ID');
    it('should escape CSV fields with commas', () => {
      const state = createState();
      state.schemas[0].tables[0].description = 'This table stores users, orders, and products';
      const result = generator.generate(state);
      // The description should be quoted because it contains commas
      expect(result.tables).toContain('"This table stores users, orders, and products"');
    it('should escape CSV fields with quotes', () => {
      state.schemas[0].tables[0].description = 'Contains "important" data';
      // Quotes should be doubled inside the quoted field
      expect(result.tables).toContain('"Contains ""important"" data"');
    it('should escape CSV fields with newlines', () => {
      state.schemas[0].tables[0].description = 'Line one\nLine two';
      expect(result.tables).toContain('"Line one\nLine two"');
  describe('generate with options', () => {
    it('should filter by approvedOnly', () => {
      state.schemas[0].tables[0].userApproved = false;
      const result = generator.generate(state, { approvedOnly: true });
      // Should only have header row, no data rows
      const tableLines = result.tables.split('\n');
      expect(tableLines).toHaveLength(1); // Just the header
    it('should include approved tables when approvedOnly is true', () => {
      state.schemas[0].tables[0].userApproved = true;
      expect(tableLines.length).toBeGreaterThan(1);
    it('should filter by confidenceThreshold', () => {
      // Set confidence to 0.5 which is below 0.8 threshold
      state.schemas[0].tables[0].descriptionIterations[0].confidence = 0.5;
      const result = generator.generate(state, { confidenceThreshold: 0.8 });
    it('should include tables above confidence threshold', () => {
      state.schemas[0].tables[0].descriptionIterations[0].confidence = 0.95;
