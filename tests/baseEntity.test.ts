import { EntityField } from '../generic/baseEntity';
import { EntityFieldInfo, EntityFieldTSType } from '../generic/entityInfo';
import { ValidationResult, ValidationErrorInfo, ValidationErrorType } from '@memberjunction/global';
// Create minimal mock EntityFieldInfo for testing EntityField
function createMockFieldInfo(overrides: Partial<EntityFieldInfo> = {}): EntityFieldInfo {
    const base = {
        DisplayNameOrName: 'Test Field',
        SQLFullType: 'nvarchar(100)',
        TSType: EntityFieldTSType.String,
        MaxLength: 100,
        ReadOnly: false,
        SkipValidation: false,
        Entity: 'MJTestEntity',
        IsSpecialDateField: false,
    return base as unknown as EntityFieldInfo;
describe('EntityField', () => {
        it('should create a field with null value by default', () => {
            const fieldInfo = createMockFieldInfo();
            const field = new EntityField(fieldInfo);
            // When no initial value or default is provided, the constructor sets Value to null
            expect(field.Value).toBeNull();
            expect(field.Name).toBe('TestField');
        it('should set initial value when provided', () => {
            const field = new EntityField(fieldInfo, 'initial value');
            expect(field.Value).toBe('initial value');
        it('should set boolean default value from string "1"', () => {
            const fieldInfo = createMockFieldInfo({
                TSType: EntityFieldTSType.Boolean,
                DefaultValue: '1'
            expect(field.Value).toBe(true);
        it('should set boolean default value from string "true"', () => {
                DefaultValue: 'true'
        it('should set boolean default to false for other string', () => {
                DefaultValue: '0'
            expect(field.Value).toBe(false);
        it('should set numeric default value', () => {
                TSType: EntityFieldTSType.Number,
                DefaultValue: '42'
            expect(field.Value).toBe(42);
        it('should handle numeric default of null string', () => {
                DefaultValue: 'null'
        it('should set uniqueidentifier default to null', () => {
                Type: 'uniqueidentifier',
                DefaultValue: 'newsequentialid()'
    describe('Dirty tracking', () => {
            const field = new EntityField(fieldInfo, 'initial');
            expect(field.Dirty).toBe(false);
        it('should be dirty after value change', () => {
            // Constructor with initial value sets both Value and OldValue to 'initial'
            field.Value = 'changed';
            expect(field.Dirty).toBe(true);
        it('should not be dirty when value is set to same value', () => {
            // Constructor with initial value sets both Value and OldValue to 'same'
            const field = new EntityField(fieldInfo, 'same');
            field.Value = 'same';
    describe('Validate', () => {
        it('should pass validation for valid string within maxLength', () => {
            const fieldInfo = createMockFieldInfo({ MaxLength: 50 });
            const field = new EntityField(fieldInfo, 'short string');
            const result = field.Validate();
        it('should fail validation when string exceeds maxLength', () => {
                MaxLength: 5,
                TSType: EntityFieldTSType.String
            const field = new EntityField(fieldInfo, 'this is too long');
            expect(result.Errors.length).toBeGreaterThan(0);
        it('should fail validation when required field is null', () => {
                AllowsNull: false,
                DefaultValue: null
            field.Value = null;
        it('should pass validation when field is ReadOnly', () => {
                ReadOnly: true,
                AllowsNull: false
        it('should pass validation when SkipValidation is true', () => {
                SkipValidation: true,
describe('ValidationResult', () => {
    it('should default to not success', () => {
        expect(result.Errors).toEqual([]);
describe('ValidationErrorInfo', () => {
    it('should construct with field name, message, and value', () => {
        const error = new ValidationErrorInfo('Name', 'Name is required', null);
        expect(error.Source).toBe('Name');
        expect(error.Message).toBe('Name is required');
        expect(error.Value).toBeNull();
    it('should construct with error type', () => {
        const error = new ValidationErrorInfo('Name', 'Name is required', null, ValidationErrorType.Failure);
        expect(error.Type).toBe(ValidationErrorType.Failure);
describe('EntityFieldTSType', () => {
    it('should have expected type constants', () => {
        expect(EntityFieldTSType.String).toBeDefined();
        expect(EntityFieldTSType.Number).toBeDefined();
        expect(EntityFieldTSType.Boolean).toBeDefined();
        expect(EntityFieldTSType.Date).toBeDefined();
