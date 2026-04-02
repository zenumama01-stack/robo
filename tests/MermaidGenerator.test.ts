import { MermaidGenerator } from '../generators/MermaidGenerator';
      totalIterations: 1, totalPromptsRun: 0, totalInputTokens: 0,
      totalSchemas: 1, totalTables: 2, totalColumns: 4
        description: 'Default schema',
              { name: 'Name', dataType: 'nvarchar(100)', isNullable: true, isPrimaryKey: false, isForeignKey: false, descriptionIterations: [], description: 'User name' }
            descriptionIterations: [{ description: 'Users', reasoning: '', generatedAt: '2024-01-01', modelUsed: 'test', confidence: 0.9, triggeredBy: 'initial' as const }],
            description: 'Stores users'
              { name: 'UserID', dataType: 'int', isNullable: false, isPrimaryKey: false, isForeignKey: true, descriptionIterations: [], description: 'FK to Users' }
            descriptionIterations: [{ description: 'Orders', reasoning: '', generatedAt: '2024-01-01', modelUsed: 'test', confidence: 0.85, triggeredBy: 'initial' as const }],
            description: 'Customer orders'
describe('MermaidGenerator', () => {
  let generator: MermaidGenerator;
    generator = new MermaidGenerator();
    it('should output erDiagram header', () => {
      const mmd = generator.generate(createState());
      expect(mmd).toContain('erDiagram');
    it('should include entity definitions', () => {
      expect(mmd).toContain('Users {');
      expect(mmd).toContain('Orders {');
    it('should include column types', () => {
      expect(mmd).toContain('int ID');
      expect(mmd).toContain('nvarchar(100) Name');
    it('should include PK and FK constraints', () => {
      // Constraints are combined in the output, e.g. "PK,NOT_NULL" and "FK,NOT_NULL"
      expect(mmd).toContain('PK');
      expect(mmd).toContain('FK');
    it('should include NOT_NULL constraint', () => {
      expect(mmd).toContain('NOT_NULL');
    it('should include relationships', () => {
      expect(mmd).toContain('Users ||--o{ Orders : "has"');
    it('should include header comments by default', () => {
      expect(mmd).toContain('%% Entity Relationship Diagram');
      expect(mmd).toContain('%% Database: TestDB');
    it('should suppress comments when includeComments is false', () => {
      const mmd = generator.generate(createState(), { includeComments: false });
      expect(mmd).not.toContain('%%');
    it('should include column descriptions as comments', () => {
      expect(mmd).toContain('%% Primary key');
    it('should not duplicate relationships', () => {
      const matches = mmd.match(/Users \|\|--o\{ Orders/g);
      expect(matches).toHaveLength(1);
  describe('generate with filters', () => {
      state.schemas[0].tables[1].userApproved = true;
      const mmd = generator.generate(state, { approvedOnly: true });
      expect(mmd).not.toContain('Users {');
      state.schemas[0].tables[1].descriptionIterations[0].confidence = 0.9;
      const mmd = generator.generate(state, { confidenceThreshold: 0.8 });
  describe('generateHtml', () => {
    it('should return valid HTML document', () => {
      const html = generator.generateHtml(createState());
      expect(html).toContain('<!DOCTYPE html>');
      expect(html).toContain('<html');
      expect(html).toContain('</html>');
    it('should include mermaid script', () => {
      expect(html).toContain('mermaid');
    it('should include database name in title', () => {
      expect(html).toContain('TestDB');
    it('should escape HTML in database name', () => {
      state.database.name = 'Test<DB>';
      const html = generator.generateHtml(state);
      expect(html).toContain('Test&lt;DB&gt;');
