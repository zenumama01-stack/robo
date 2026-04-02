  ValidationErrorClass,
  ValidationWarningClass,
} from '../types/validation';
  ValidationSummary,
  ParsedReference,
describe('ValidationErrorClass', () => {
  it('should create an instance with all properties', () => {
    const error: ValidationError = {
      file: '/path/to/file.json',
    const errorClass = new ValidationErrorClass(error);
    expect(errorClass).toBeInstanceOf(Error);
    expect(errorClass).toBeInstanceOf(ValidationErrorClass);
    expect(errorClass.name).toBe('ValidationError');
    expect(errorClass.type).toBe('field');
    expect(errorClass.severity).toBe('error');
    expect(errorClass.entity).toBe('Users');
    expect(errorClass.field).toBe('Email');
    expect(errorClass.file).toBe('/path/to/file.json');
    expect(errorClass.message).toBe('Field "Email" does not exist on entity "Users"');
    expect(errorClass.suggestion).toBe('Check field name spelling');
  it('should extract line number from message', () => {
      message: 'Parse error at line 42',
    expect(errorClass.line).toBe(42);
    expect(errorClass.column).toBeUndefined();
  it('should extract both line and column from message', () => {
      message: 'Error at line 10, column 5',
    expect(errorClass.line).toBe(10);
    expect(errorClass.column).toBe(5);
  it('should handle messages without line/column info', () => {
      message: 'File reference not found',
    expect(errorClass.line).toBeUndefined();
  describe('getFormattedMessage', () => {
    it('should return base message when no line info', () => {
        message: 'Simple error',
      expect(errorClass.getFormattedMessage()).toBe('Simple error');
    it('should append line info when available', () => {
        message: 'Error at line 15',
      expect(errorClass.getFormattedMessage()).toBe('Error at line 15 (line 15)');
    it('should append line and column info when both available', () => {
        message: 'Error at line 15, column 20',
      expect(errorClass.getFormattedMessage()).toBe('Error at line 15, column 20 (line 15, column 20)');
describe('ValidationWarningClass', () => {
    const warning: ValidationWarning = {
    const warningClass = new ValidationWarningClass(warning);
    expect(warningClass).toBeInstanceOf(Error);
    expect(warningClass).toBeInstanceOf(ValidationWarningClass);
    expect(warningClass.name).toBe('ValidationWarning');
    expect(warningClass.type).toBe('bestpractice');
    expect(warningClass.severity).toBe('warning');
    expect(warningClass.entity).toBe('Users');
    expect(warningClass.field).toBe('Name');
    expect(warningClass.file).toBe('/path/to/file.json');
      type: 'nesting',
      message: 'Deep nesting at line 100',
    expect(warningClass.line).toBe(100);
      type: 'naming',
      message: 'Consider renaming this field',
    expect(warningClass.line).toBeUndefined();
    expect(warningClass.column).toBeUndefined();
        message: 'Simple warning',
      expect(warningClass.getFormattedMessage()).toBe('Simple warning');
        message: 'Warning at line 5',
      expect(warningClass.getFormattedMessage()).toBe('Warning at line 5 (line 5)');
describe('Type interfaces shape validation', () => {
  it('should allow creating a valid ValidationResult', () => {
    expect(result.warnings).toHaveLength(0);
    expect(result.summary.totalFiles).toBe(5);
    expect(result.summary.totalEntities).toBe(10);
  it('should allow creating a FileValidationResult', () => {
    const fileResult: FileValidationResult = {
      entityCount: 3,
    expect(fileResult.file).toBe('/path/to/file.json');
    expect(fileResult.entityCount).toBe(3);
  it('should allow creating EntityDependency with Set', () => {
    const dep: EntityDependency = {
      dependsOn: new Set(['Roles', 'Departments']),
    expect(dep.entityName).toBe('MJ: Users');
    expect(dep.dependsOn.size).toBe(2);
    expect(dep.dependsOn.has('Roles')).toBe(true);
  it('should allow creating ValidationOptions with defaults', () => {
    const opts: ValidationOptions = {
      verbose: false,
      outputFormat: 'human',
      maxNestingDepth: 10,
      checkBestPractices: true,
    expect(opts.verbose).toBe(false);
    expect(opts.outputFormat).toBe('human');
  it('should allow creating ValidationOptions with include/exclude', () => {
    expect(opts.include).toEqual(['users', 'roles']);
    expect(opts.exclude).toEqual(['temp']);
  it('should allow creating a ParsedReference', () => {
    const ref: ParsedReference = {
      type: '@lookup:',
      value: 'Admin',
      entity: 'Roles',
      fields: [{ field: 'Name', value: 'Admin' }],
      createIfMissing: true,
      additionalFields: { Description: 'Administrator role' },
    expect(ref.type).toBe('@lookup:');
    expect(ref.entity).toBe('Roles');
    expect(ref.createIfMissing).toBe(true);
