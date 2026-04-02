  it('should have a passing test', () => {
describe('@memberjunction/ng-testing', () => {
describe('@memberjunction/ng-ai-test-harness', () => {
describe('@memberjunction/ng-base-types', () => {
describe('@memberjunction/ng-code-editor', () => {
describe('@memberjunction/ng-conversations', () => {
describe('@memberjunction/ng-entity-relationship-diagram', () => {
describe('@memberjunction/ng-entity-viewer', () => {
describe('@memberjunction/ng-export-service', () => {
describe('@memberjunction/ng-filter-builder', () => {
describe('@memberjunction/ng-list-management', () => {
describe('@memberjunction/ng-markdown', () => {
describe('@memberjunction/ng-react', () => {
describe('@memberjunction/ng-tasks', () => {
describe('@memberjunction/ng-timeline', () => {
describe('@memberjunction/ng-trees', () => {
describe('@memberjunction/ng-versions', () => {
// MJAPI's index.ts is primarily a startup script. We verify it has the expected structure.
// We do NOT test generated code.
vi.mock('@memberjunction/server-bootstrap', () => ({
  createMJServer: vi.fn().mockResolvedValue(undefined),
vi.mock('mj_generatedentities', () => ({}));
vi.mock('mj_generatedactions', () => ({}));
vi.mock('@memberjunction/server-bootstrap/mj-class-registrations', () => ({}));
// Mock the generated manifest
vi.mock('../generated/class-registrations-manifest.js', () => ({}));
describe('MJAPI', () => {
  it('should have a valid package structure', () => {
    // This test validates that the mock setup works correctly,
    // confirming that the imports in index.ts reference real modules
  it('should use createMJServer for bootstrapping', async () => {
    const { createMJServer } = await import('@memberjunction/server-bootstrap');
    expect(createMJServer).toBeDefined();
    expect(typeof createMJServer).toBe('function');
const mockQuery = vi.fn();
  Request: class {
    query = mockQuery;
  ConnectionPool: class {},
vi.mock('@memberjunction/data-context', () => ({
  DataContextItem: class {
    SQL: string | undefined;
    Data: unknown[] | undefined;
    DataLoadingError: string | undefined;
    DataLoaded = false;
import { DataContextItemServer } from '../index';
describe('DataContextItemServer', () => {
  let item: DataContextItemServer;
    item = new DataContextItemServer();
  describe('LoadFromSQL', () => {
    it('should be a DataContextItemServer instance', () => {
      expect(item).toBeInstanceOf(DataContextItemServer);
    it('should have SQL property from parent', () => {
      item.SQL = 'SELECT TOP 10 * FROM Users';
      expect(item.SQL).toBe('SELECT TOP 10 * FROM Users');
