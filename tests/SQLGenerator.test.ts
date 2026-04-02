import { SQLGenerator } from '../generators/SQLGenerator';
      totalSchemas: 1, totalTables: 1, totalColumns: 2
              { name: 'ID', dataType: 'int', isNullable: false, isPrimaryKey: true, isForeignKey: false, descriptionIterations: [], description: 'Primary key identifier' },
              { name: 'Name', dataType: 'nvarchar(100)', isNullable: false, isPrimaryKey: false, isForeignKey: false, descriptionIterations: [], description: "User's full name" }
              { description: 'Stores user accounts', reasoning: 'Main user table', generatedAt: '2024-01-01', modelUsed: 'test', confidence: 0.9, triggeredBy: 'initial' as const }
describe('SQLGenerator', () => {
  let generator: SQLGenerator;
    generator = new SQLGenerator();
    it('should include SQL header comments', () => {
      const sql = generator.generate(createState());
      expect(sql).toContain('-- Database Documentation Script');
      expect(sql).toContain('-- Database: TestDB');
      expect(sql).toContain('-- Server: localhost');
    it('should generate schema description sp_addextendedproperty', () => {
      expect(sql).toContain("@level0type = N'SCHEMA'");
      expect(sql).toContain("@level0name = N'dbo'");
      expect(sql).toContain('Default schema');
    it('should generate table description sp_addextendedproperty', () => {
      expect(sql).toContain("@level1type = N'TABLE'");
      expect(sql).toContain("@level1name = N'Users'");
      expect(sql).toContain('Stores user accounts');
    it('should generate column description sp_addextendedproperty', () => {
      expect(sql).toContain("@level2type = N'COLUMN'");
      expect(sql).toContain("@level2name = N'ID'");
      expect(sql).toContain('Primary key identifier');
    it('should include drop-before-add pattern', () => {
      expect(sql).toContain('sp_dropextendedproperty');
      expect(sql).toContain('sp_addextendedproperty');
    it('should include GO statement separators', () => {
      expect(sql).toContain('GO');
    it('should escape single quotes in descriptions', () => {
      state.schemas[0].tables[0].description = "User's account data";
      const sql = generator.generate(state);
      expect(sql).toContain("User''s account data");
    it('should skip tables without descriptions', () => {
      state.schemas[0].tables[0].description = undefined;
      // Table-level description block should be skipped
      expect(sql).not.toContain("-- Table: dbo.Users");
      // But column descriptions for that table's columns are still emitted
    it('should skip columns without descriptions', () => {
      state.schemas[0].tables[0].columns[0].description = undefined;
      // ID column should not be included (no description), but Name column should
      expect(sql).not.toContain("@level2name = N'ID'");
      expect(sql).toContain("@level2name = N'Name'");
      const sql = generator.generate(state, { approvedOnly: true });
      expect(sql).not.toContain("@level1name = N'Users'");
      const sql = generator.generate(state, { confidenceThreshold: 0.8 });
    it('should include tables above threshold', () => {
