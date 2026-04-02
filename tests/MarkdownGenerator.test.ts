import { MarkdownGenerator } from '../generators/MarkdownGenerator';
function createMinimalState(): DatabaseDocumentation {
      totalIterations: 2,
      totalPromptsRun: 10,
      totalInputTokens: 5000,
      totalOutputTokens: 3000,
      totalTokens: 8000,
      estimatedCost: 0.10,
      totalSchemas: 1,
      totalTables: 2,
      totalColumns: 4
      descriptionGeneration: [
          completedAt: '2024-01-01T01:00:00Z',
          status: 'converged',
          levelsProcessed: 2,
          backpropagationCount: 1,
          sanityCheckCount: 1,
          converged: true,
          convergenceReason: 'All tables above threshold',
          totalTokensUsed: 8000,
            dependents: [{ schema: 'dbo', table: 'Orders', column: 'UserID', referencedColumn: 'ID' }],
              { name: 'Name', dataType: 'nvarchar(100)', isNullable: false, isPrimaryKey: false, isForeignKey: false, descriptionIterations: [], description: 'Full name' }
                description: 'Stores user accounts',
                reasoning: 'Contains user data',
                generatedAt: '2024-01-01T00:10:00Z',
                confidence: 0.95,
                triggeredBy: 'initial'
            description: 'Stores user accounts'
            dependencyLevel: 1,
            dependsOn: [{ schema: 'dbo', table: 'Users', column: 'UserID', referencedColumn: 'ID' }],
              { name: 'ID', dataType: 'int', isNullable: false, isPrimaryKey: true, isForeignKey: false, descriptionIterations: [], description: 'Order ID' },
              { name: 'UserID', dataType: 'int', isNullable: false, isPrimaryKey: false, isForeignKey: true, descriptionIterations: [], description: 'Foreign key to Users' }
                description: 'First attempt at orders description',
                reasoning: 'Initial',
                generatedAt: '2024-01-01T00:15:00Z',
                confidence: 0.7,
                description: 'Customer purchase orders',
                reasoning: 'Refined after backpropagation',
                generatedAt: '2024-01-01T00:30:00Z',
                confidence: 0.9,
                triggeredBy: 'backpropagation'
            description: 'Customer purchase orders'
describe('MarkdownGenerator', () => {
  let generator: MarkdownGenerator;
    generator = new MarkdownGenerator();
    it('should include database name in header', () => {
      const state = createMinimalState();
      const md = generator.generate(state);
      expect(md).toContain('# Database Documentation: TestDB');
    it('should include server info', () => {
      expect(md).toContain('**Server**: localhost');
    it('should include analysis summary', () => {
      expect(md).toContain('## Analysis Summary');
      expect(md).toContain('**Status**: converged');
      expect(md).toContain('gemini-3-flash-preview');
      expect(md).toContain('google');
    it('should include table of contents', () => {
      expect(md).toContain('## Table of Contents');
      expect(md).toContain('Users');
      expect(md).toContain('Orders');
    it('should include schema sections', () => {
      expect(md).toContain('## Schema: dbo');
    it('should include table descriptions', () => {
      expect(md).toContain('Stores user accounts');
      expect(md).toContain('Customer purchase orders');
    it('should render column table with correct headers', () => {
      expect(md).toContain('| Column | Type | Description |');
      expect(md).toContain('|--------|------|-------------|');
    it('should mark PK columns', () => {
      expect(md).toContain('PK');
    it('should mark FK columns', () => {
      expect(md).toContain('FK');
    it('should show dependency relationships', () => {
      expect(md).toContain('**Depends On**:');
      expect(md).toContain('**Referenced By**:');
    it('should include row count', () => {
      expect(md).toContain('**Row Count**: 100');
      expect(md).toContain('**Row Count**: 500');
    it('should include confidence percentage', () => {
      expect(md).toContain('**Confidence**: 95%');
    it('should include iteration analysis appendix', () => {
      expect(md).toContain('## Appendix: Iteration Analysis');
    it('should show backpropagation refinements in appendix', () => {
      expect(md).toContain('Total Tables with Refinements');
      expect(md).toContain('Refinements Triggered by Backpropagation');
    it('should include mermaid ERD diagram', () => {
      expect(md).toContain('```mermaid');
      expect(md).toContain('erDiagram');
    it('should handle seed context when provided', () => {
      state.seedContext = {
        overallPurpose: 'E-commerce platform',
        industryContext: 'Retail',
        businessDomains: ['Sales', 'Users']
      expect(md).toContain('## Database Context');
      expect(md).toContain('E-commerce platform');
      expect(md).toContain('Retail');
      expect(md).toContain('Sales, Users');
    it('should handle empty schemas array', () => {
      state.phases.descriptionGeneration = [];
