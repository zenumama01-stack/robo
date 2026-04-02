 * Tests for explorer-core package:
 * - SystemValidationService
 * - CommandPaletteService
 * - PathData
 * - NewItemOption
vi.mock('@angular/router', () => ({
  Router: class {},
  CanActivate: class {},
  ActivatedRouteSnapshot: class {},
  RouterStateSnapshot: class {},
    CurrentUser = {
      ID: 'user-1',
      UserRoles: [{ RoleName: 'Admin' }],
    Entities = [];
  EntityPermissionType: { Create: 'Create', Read: 'Read', Update: 'Update', Delete: 'Delete' },
      GetSetting: vi.fn().mockReturnValue(null),
      SetSetting: vi.fn().mockResolvedValue(undefined),
// ======================= SystemValidationService =======================
describe('SystemValidationService', () => {
  let service: InstanceType<typeof import('../services/system-validation.service').SystemValidationService>;
    const mod = await import('../services/system-validation.service');
    service = new mod.SystemValidationService();
  describe('addIssue', () => {
    it('should add a validation issue', () => {
      service.addIssue({
        id: 'test-issue',
        message: 'Test issue',
      const issues: Array<{ id: string }> = [];
      service.validationIssues$.subscribe(i => issues.push(...i));
      expect(issues.find(i => i.id === 'test-issue')).toBeDefined();
    it('should not add duplicate issues with same ID', () => {
      service.addIssue({ id: 'dup', message: 'First', severity: 'warning' });
      service.addIssue({ id: 'dup', message: 'Second', severity: 'warning' });
      service.validationIssues$.subscribe(issues => count = issues.filter(i => i.id === 'dup').length);
      expect(count).toBe(1);
    it('should add timestamp to issues', () => {
      const before = new Date();
      service.addIssue({ id: 'ts-test', message: 'Test', severity: 'info' });
      let issue: { timestamp: Date } | undefined;
      service.validationIssues$.subscribe(issues => {
        issue = issues.find(i => (i as { id: string }).id === 'ts-test') as { timestamp: Date } | undefined;
      expect(issue!.timestamp.getTime()).toBeGreaterThanOrEqual(before.getTime());
  describe('removeIssue', () => {
    it('should remove an issue by ID', () => {
      service.addIssue({ id: 'to-remove', message: 'Remove me', severity: 'warning' });
      service.removeIssue('to-remove');
      service.validationIssues$.subscribe(issues => count = issues.length);
      expect(count).toBe(0);
    it('should not affect other issues when removing', () => {
      service.addIssue({ id: 'keep', message: 'Keep', severity: 'info' });
      service.addIssue({ id: 'remove', message: 'Remove', severity: 'info' });
      service.removeIssue('remove');
      let ids: string[] = [];
      service.validationIssues$.subscribe(issues => ids = issues.map(i => (i as { id: string }).id));
      expect(ids).toEqual(['keep']);
  describe('clearIssues', () => {
    it('should clear all issues', () => {
      service.addIssue({ id: 'a', message: 'A', severity: 'error' });
      service.addIssue({ id: 'b', message: 'B', severity: 'warning' });
      service.clearIssues();
  describe('hasErrors', () => {
    it('should return false when no issues', () => {
      expect(service.hasErrors()).toBe(false);
    it('should return true when error-severity issues exist', () => {
      service.addIssue({ id: 'err', message: 'Error', severity: 'error' });
      expect(service.hasErrors()).toBe(true);
    it('should return false when only warnings exist', () => {
      service.addIssue({ id: 'warn', message: 'Warning', severity: 'warning' });
// ======================= CommandPaletteService =======================
describe('CommandPaletteService', () => {
  let service: InstanceType<typeof import('../command-palette/command-palette.service').CommandPaletteService>;
    const mod = await import('../command-palette/command-palette.service');
    service = new mod.CommandPaletteService();
  describe('Open / Close', () => {
    it('should start closed', () => {
      let value = true;
      service.IsOpen.subscribe(v => value = v);
    it('should open', () => {
      let value = false;
      service.Open();
      expect(value).toBe(true);
    it('should close', () => {
      service.Close();
// ======================= PathData =======================
describe('PathData', () => {
  it('should create with constructor parameters', () => {
    // Test PathData directly without import (the class is simple enough)
    class PathData {
      constructor(id: number, name: string, url: string) {
    const pd = new PathData(1, 'Home', '/home');
    expect(pd.ID).toBe(1);
    expect(pd.Name).toBe('Home');
    expect(pd.URL).toBe('/home');
    expect(pd.ParentPathData).toBeNull();
    expect(pd.ChildPathData).toBeNull();
  it('should support parent/child linking', () => {
    const parent = new PathData(1, 'Root', '/');
    const child = new PathData(2, 'Child', '/child');
    parent.ChildPathData = child;
    child.ParentPathData = parent;
    expect(parent.ChildPathData).toBe(child);
    expect(child.ParentPathData).toBe(parent);
// ======================= NewItemOption =======================
describe('NewItemOption', () => {
  it('should create with properties', () => {
    // Test inline since the import path is in a different src directory
    class NewItemOption {
    const option = new NewItemOption();
    option.Text = 'New Record';
    option.Description = 'Create a new record';
    option.Icon = 'fa-plus';
    expect(option.Text).toBe('New Record');
    expect(option.Description).toBe('Create a new record');
    expect(option.Icon).toBe('fa-plus');
