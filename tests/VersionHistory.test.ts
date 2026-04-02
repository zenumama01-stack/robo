 * Unit tests for the VersionHistory package.
 * Tests: DiffEngine snapshot comparison, constants/SQL helpers,
 * DependencyGraphWalker flatten, and LabelManager validation.
// Mock heavy MJ dependencies before importing source modules
const mockRunViewFn = vi.fn();
const mockGetEntityObject = vi.fn();
const mockEntityByName = vi.fn();
  class FakeMetadata {
    Entities: { ID: string; Name: string }[] = [];
    EntityByName = mockEntityByName;
    GetEntityObject = mockGetEntityObject;
  class FakeRunView {
    ToWhereClause() {
      return this.KeyValuePairs.map(p => `[${p.FieldName}] = '${p.Value}'`).join(' AND ');
    Metadata: FakeMetadata,
    RunView: FakeRunView,
  MJVersionLabelEntity: class {},
  MJVersionLabelItemEntity: class {},
// Import source modules after mocks are in place
  sqlIn,
  buildIdKey,
} from '../constants';
import { DiffEngine } from '../DiffEngine';
import { DependencyGraphWalker } from '../DependencyGraphWalker';
import { LabelManager } from '../LabelManager';
import type { DependencyNode, WalkOptions } from '../types';
// 1. Constants / SQL helpers
describe('escapeSqlString', () => {
  it('should double single quotes', () => {
    expect(escapeSqlString("O'Brien")).toBe("O''Brien");
    expect(escapeSqlString(null as unknown as string)).toBe('');
  it('should leave clean strings unchanged', () => {
    expect(escapeSqlString('Hello')).toBe('Hello');
describe('sqlEquals', () => {
  it('should produce a safe equality filter', () => {
    expect(sqlEquals('Name', "O'Brien")).toBe("Name = 'O''Brien'");
describe('sqlContains', () => {
  it('should produce a LIKE filter', () => {
    expect(sqlContains('Title', 'test')).toBe("Title LIKE '%test%'");
describe('sqlIn', () => {
  it('should produce an IN clause', () => {
    expect(sqlIn('ID', ['a', 'b'])).toBe("ID IN ('a', 'b')");
  it('should escape values in the IN clause', () => {
    expect(sqlIn('Name', ["O'Brien", 'Smith'])).toBe("Name IN ('O''Brien', 'Smith')");
describe('sqlNotIn', () => {
  it('should produce a NOT IN clause', () => {
    expect(sqlNotIn('ID', ['x'])).toBe("ID NOT IN ('x')");
describe('buildIdKey', () => {
  it('should create a CompositeKey with FieldName=ID', () => {
    const key = buildIdKey('abc-123');
    expect(key.KeyValuePairs).toEqual([{ FieldName: 'ID', Value: 'abc-123' }]);
// 2. DiffEngine – compareSnapshots (via public DiffLabels fast-path)
describe('DiffEngine', () => {
  let engine: DiffEngine;
    engine = new DiffEngine();
  it('should return empty diff when comparing a label to itself', async () => {
    // loadEntityById uses RunView internally – return a mock label
      Results: [{ ID: 'label1', Name: 'v1.0' }],
    mockEntityByName.mockReturnValue({ ID: 'entity1', FirstPrimaryKey: { Name: 'ID' } });
    mockGetEntityObject.mockResolvedValue({
      InnerLoad: vi.fn().mockResolvedValue(true),
      Name: 'v1.0',
    const user = {} as InstanceType<typeof import('@memberjunction/core').UserInfo>;
    const diff = await engine.DiffLabels('label1', 'label1', user);
    expect(diff.Summary.TotalRecordsChanged).toBe(0);
    expect(diff.EntityDiffs).toEqual([]);
    expect(diff.FromLabelID).toBe('label1');
    expect(diff.ToLabelID).toBe('label1');
// 3. DependencyGraphWalker – FlattenTopological
describe('DependencyGraphWalker', () => {
  let walker: DependencyGraphWalker;
    walker = new DependencyGraphWalker();
  it('FlattenTopological should return BFS order', () => {
    const root: DependencyNode = {
      EntityName: 'Root',
      EntityInfo: {} as DependencyNode['EntityInfo'],
      RecordKey: {} as DependencyNode['RecordKey'],
      RecordID: 'root-1',
      RecordData: {},
      Children: [
          EntityName: 'ChildA',
          RecordID: 'a-1',
          Depth: 1,
              EntityName: 'GrandchildA1',
              RecordID: 'a1-1',
              Depth: 2,
          EntityName: 'ChildB',
          RecordID: 'b-1',
    const flat = walker.FlattenTopological(root);
    expect(flat.map(n => n.RecordID)).toEqual(['root-1', 'a-1', 'b-1', 'a1-1']);
  it('FlattenTopological should handle single node', () => {
    const single: DependencyNode = {
      EntityName: 'Lone',
      RecordID: 'lone-1',
    const flat = walker.FlattenTopological(single);
    expect(flat).toHaveLength(1);
    expect(flat[0].RecordID).toBe('lone-1');
// 4. LabelManager – validation
describe('LabelManager', () => {
  let manager: LabelManager;
    manager = new LabelManager();
  it('should throw when Name is empty', async () => {
      manager.CreateLabel({ Name: '' }, user)
    ).rejects.toThrow('Version label Name is required');
  it('should throw when Name is whitespace', async () => {
      manager.CreateLabel({ Name: '   ' }, user)
  it('should throw when Entity scope lacks EntityName', async () => {
      manager.CreateLabel({ Name: 'test', Scope: 'Entity' }, user)
    ).rejects.toThrow('Entity scope requires EntityName');
  it('should throw when Record scope lacks EntityName', async () => {
      manager.CreateLabel({ Name: 'test', Scope: 'Record' }, user)
    ).rejects.toThrow('Record scope requires EntityName');
  it('should throw when Record scope lacks RecordKey', async () => {
      manager.CreateLabel({ Name: 'test', Scope: 'Record', EntityName: 'MJ: Users' }, user)
    ).rejects.toThrow('Record scope requires RecordKey');
