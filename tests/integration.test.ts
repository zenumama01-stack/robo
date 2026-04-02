 * Integration Tests for Constraint Validators
 * These tests verify that validators work correctly with the linter integration.
import { SubsetOfEntityFieldsValidator } from './subset-of-entity-fields-validator';
import { SqlWhereClauseValidator } from './sql-where-clause-validator';
import { ConstraintValidatorRegistry } from './constraint-validator-registry';
import type { ValidationContext, PropertyConstraint } from './validation-context';
describe('Constraint Validator Integration', () => {
  describe('ConstraintValidatorRegistry', () => {
    it('should have subset-of-entity-fields validator registered', () => {
      expect(ConstraintValidatorRegistry.has('subset-of-entity-fields')).toBe(true);
    it('should have sql-where-clause validator registered', () => {
      expect(ConstraintValidatorRegistry.has('sql-where-clause')).toBe(true);
    it('should return validator instances', () => {
      const validator = ConstraintValidatorRegistry.get('subset-of-entity-fields');
      expect(validator).toBeInstanceOf(SubsetOfEntityFieldsValidator);
  describe('SubsetOfEntityFieldsValidator', () => {
    const validator = new SubsetOfEntityFieldsValidator();
    it('should validate correct field names', () => {
      const context = createMockContext({
        propertyValue: ['FirstName', 'LastName', 'Email'],
        siblingProps: new Map([['entityName', 'Members']]),
        entityFields: [
          { name: 'ID', type: 'uniqueidentifier' },
          { name: 'FirstName', type: 'nvarchar' },
          { name: 'LastName', type: 'nvarchar' },
          { name: 'Email', type: 'nvarchar' },
      const constraint: PropertyConstraint = {
        type: 'subset-of-entity-fields',
        dependsOn: 'entityName',
      const violations = validator.validate(context, constraint);
    it('should detect invalid field names', () => {
        propertyValue: ['FullName', 'Status', 'StartDate'],
          { name: 'JoinDate', type: 'datetime' },
      expect(violations.length).toBeGreaterThan(0);
      expect(violations).toEqual(
            type: expect.stringContaining('invalid-field'),
            message: expect.stringContaining('FullName'),
    it('should provide "Did you mean?" suggestions', () => {
        propertyValue: ['StartDate'], // Should suggest JoinDate
          { name: 'CreatedAt', type: 'datetime' },
      expect(violations[0].suggestion).toBeDefined();
  describe('SqlWhereClauseValidator', () => {
    const validator = new SqlWhereClauseValidator();
    it('should validate correct WHERE clause', () => {
        propertyValue: "Status='Active' AND CreatedAt > '2024-01-01'",
        siblingProps: new Map([['entityName', 'Products']]),
          { name: 'Name', type: 'nvarchar' },
          { name: 'Status', type: 'nvarchar' },
        type: 'sql-where-clause',
    it('should detect invalid field in WHERE clause', () => {
        propertyValue: "Status='Active' AND LastModified > '2024-01-01'",
            message: expect.stringContaining('LastModified'),
    it('should ignore SQL functions and their arguments', () => {
      // Test with YEAR() function which doesn't have identifier arguments like DATEDIFF's 'day'
        propertyValue: "YEAR(CreatedAt) = 2024 AND Status = 'Active'",
      if (violations.length > 0) {
        console.log('Unexpected violations:', JSON.stringify(violations, null, 2));
 * Helper to create a mock ValidationContext
function createMockContext(overrides: Partial<ValidationContext> & { entityFields?: Array<{ name: string; type: string }> }): ValidationContext {
  const entityFields = overrides.entityFields || [];
  const entityName = overrides.siblingProps?.get('entityName') as string || 'MJTestEntity';
    node: {} as any,
    path: {} as any,
    componentSpec: {} as ComponentSpec,
    propertyName: 'fields',
    propertyValue: overrides.propertyValue || [],
    siblingProps: overrides.siblingProps || new Map(),
    getEntityFields: (name: string) => {
      if (name === entityName) {
        return entityFields.map(f => ({
          type: f.type,
    getEntityFieldType: (name: string, fieldName: string) => {
        const field = entityFields.find(f => f.name === fieldName);
    findSimilarFieldNames: (fieldName: string, name: string) => {
        // Simple Levenshtein-like matching
        return entityFields
          .map(f => f.name)
          .filter(fn => {
            const dist = levenshteinDistance(fieldName.toLowerCase(), fn.toLowerCase());
            return dist <= 3;
    hasEntity: (name: string) => name === entityName,
 * Simple Levenshtein distance implementation
function levenshteinDistance(a: string, b: string): number {
