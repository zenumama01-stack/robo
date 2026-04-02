import { StateValidator, ValidationResult } from '../state/StateValidator';
function createValidState(): DatabaseDocumentation {
      totalIterations: 1,
      totalPromptsRun: 5,
      totalInputTokens: 1000,
      totalOutputTokens: 500,
      totalTokens: 1500,
      estimatedCost: 0.05,
      name: 'TestDB',
      server: 'localhost',
      analyzedAt: '2024-01-01T00:00:00Z'
          convergenceReason: 'Stability achieved',
          totalTokensUsed: 1500,
              timestamp: '2024-01-01T00:10:00Z',
              result: 'success',
              tokensUsed: 500
                    description: 'Primary key',
                    reasoning: 'Auto-incremented ID',
                    modelUsed: 'gemini-3-flash-preview'
                description: 'Primary key'
                name: 'Name',
                dataType: 'nvarchar(100)',
                description: 'User name'
                description: 'Order ID'
                name: 'UserID',
                isForeignKey: true,
                foreignKeyReferences: { schema: 'dbo', table: 'Users', column: 'ID', referencedColumn: 'ID' },
                description: 'Reference to Users table'
                description: 'Customer orders',
                reasoning: 'Tracks purchase orders',
                confidence: 0.85,
describe('StateValidator', () => {
  let validator: StateValidator;
    validator = new StateValidator();
  describe('validate', () => {
    it('should validate a well-formed state file', () => {
      const state = createValidState();
      const result = validator.validate(state);
    it('should detect missing version field', () => {
      (state as Record<string, unknown>).version = '';
      expect(result.errors).toContain('Missing version field');
    it('should detect missing database field', () => {
      (state as Record<string, unknown>).database = undefined;
      expect(result.errors.some(e => e.includes('Missing database field'))).toBe(true);
    it('should detect missing database.name', () => {
      state.database.name = '';
      expect(result.errors).toContain('Missing database.name');
    it('should detect missing database.server', () => {
      state.database.server = '';
      expect(result.errors).toContain('Missing database.server');
    it('should detect non-array schemas', () => {
      (state as Record<string, unknown>).schemas = 'not an array';
      expect(result.errors.some(e => e.includes('schemas must be an array'))).toBe(true);
    it('should detect schema missing name', () => {
      state.schemas[0].name = '';
      expect(result.errors.some(e => e.includes('missing name'))).toBe(true);
    it('should detect table missing name', () => {
      state.schemas[0].tables[0].name = '';
    it('should detect missing columns array', () => {
      (state.schemas[0].tables[0] as Record<string, unknown>).columns = undefined;
      expect(result.errors.some(e => e.includes('missing or invalid columns array'))).toBe(true);
    it('should detect missing descriptionIterations on table', () => {
      (state.schemas[0].tables[0] as Record<string, unknown>).descriptionIterations = undefined;
      expect(result.errors.some(e => e.includes('missing or invalid descriptionIterations array'))).toBe(true);
    it('should warn about references to non-existent tables', () => {
      state.schemas[0].tables[1].dependsOn = [
        { schema: 'dbo', table: 'NonExistent', column: 'ID', referencedColumn: 'ID' }
      expect(result.warnings.some(w => w.includes('non-existent table'))).toBe(true);
    it('should detect analysis run missing runId', () => {
      state.phases.descriptionGeneration[0].runId = '';
      expect(result.errors.some(e => e.includes('missing runId'))).toBe(true);
    it('should detect invalid analysis run status', () => {
      (state.phases.descriptionGeneration[0] as Record<string, unknown>).status = 'invalid_status';
      expect(result.errors.some(e => e.includes('invalid status'))).toBe(true);
    it('should accept all valid analysis run statuses', () => {
      for (const status of ['in_progress', 'completed', 'failed', 'converged'] as const) {
        state.phases.descriptionGeneration[0].status = status;
        expect(result.errors.filter(e => e.includes('invalid status'))).toHaveLength(0);
  describe('validateAndRepair', () => {
    it('should repair missing descriptionIterations arrays', () => {
      // Remove descriptionIterations to simulate old format
      (state.schemas[0] as Record<string, unknown>).descriptionIterations = undefined;
      const result = validator.validateAndRepair(state);
      // After repair, descriptionIterations should exist
      expect(state.schemas[0].descriptionIterations).toBeDefined();
      expect(Array.isArray(state.schemas[0].descriptionIterations)).toBe(true);
    it('should repair missing schemas array', () => {
      (state as Record<string, unknown>).schemas = undefined;
      validator.validateAndRepair(state);
      expect(state.schemas).toBeDefined();
      expect(Array.isArray(state.schemas)).toBe(true);
    it('should repair missing descriptionGeneration array', () => {
      (state.phases as Record<string, unknown>).descriptionGeneration = undefined;
      expect(state.phases.descriptionGeneration).toBeDefined();
      expect(Array.isArray(state.phases.descriptionGeneration)).toBe(true);
