const { mockRunView, mockRunViews, mockEntities, mockModels, mockVectorDBs } = vi.hoisted(() => {
  const mockRunView = vi.fn();
  const mockRunViews = vi.fn();
  const mockEntities = [
    { ID: 'entity-1', Name: 'MJTestEntity', FirstPrimaryKey: { Name: 'ID', NeedsQuotes: true } },
  const mockModels = [
    { ID: 'model-1', AIModelType: 'Embeddings', DriverClass: 'TestDriver' },
    { ID: 'model-2', AIModelType: 'LLM', DriverClass: 'TestLLM' },
  const mockVectorDBs = [
    { ID: 'vdb-1', ClassKey: 'TestVectorDB' },
  return { mockRunView, mockRunViews, mockEntities, mockModels, mockVectorDBs };
    Entities = mockEntities;
    CurrentUser = { ID: 'user-1', Email: 'test@test.com' };
    EntityByID(id: string) { return mockEntities.find(e => e.ID === id); }
  class MockRunViewClass {
    RunView = mockRunView;
    RunViews = mockRunViews;
    RunView: MockRunViewClass,
    CompositeKey: vi.fn().mockImplementation((pairs: Array<{ FieldName: string; Value: string }>) => ({
      KeyValuePairs: pairs,
      Values: () => pairs.map((p: { Value: string }) => p.Value).join(','),
      ToString: () => pairs.map((p: { FieldName: string; Value: string }) => `${p.FieldName}=${p.Value}`).join(', '),
    BaseEntity: vi.fn(),
    EntityInfo: vi.fn(),
    RunViewResult: vi.fn(),
      Models: mockModels,
      VectorDatabases: mockVectorDBs,
  MJVectorDatabaseEntity: vi.fn(),
  AIModelEntityExtended: vi.fn(),
import { VectorBase } from '../models/VectorBase';
describe('VectorBase', () => {
  let vectorBase: VectorBase;
    vectorBase = new VectorBase();
    it('should initialize with RunView and Metadata', () => {
      expect(vectorBase.Metadata).toBeDefined();
      expect(vectorBase.RunView).toBeDefined();
      expect(vectorBase.CurrentUser).toBeDefined();
  describe('CurrentUser', () => {
    it('should get current user', () => {
      const user = vectorBase.CurrentUser;
      expect(user).toBeDefined();
    it('should set current user', () => {
      const newUser = { ID: 'user-2', Email: 'new@test.com' } as never;
      vectorBase.CurrentUser = newUser;
      expect(vectorBase.CurrentUser).toBe(newUser);
  describe('BuildExtraFilter (protected)', () => {
    it('should build filter from composite keys', () => {
      const compositeKeys = [
        { KeyValuePairs: [{ FieldName: 'ID', Value: '123' }] },
        { KeyValuePairs: [{ FieldName: 'ID', Value: '456' }] },
      const filter = (vectorBase as unknown as { BuildExtraFilter: (keys: typeof compositeKeys) => string }).BuildExtraFilter(compositeKeys);
      expect(filter).toContain("ID = '123'");
      expect(filter).toContain("ID = '456'");
      expect(filter).toContain(' OR ');
    it('should handle composite keys with multiple fields', () => {
          KeyValuePairs: [
            { FieldName: 'ID', Value: '1' },
            { FieldName: 'Type', Value: 'A' },
      expect(filter).toContain("ID = '1'");
      expect(filter).toContain("Type = 'A'");
      expect(filter).toContain(' AND ');
  describe('GetAIModel (protected)', () => {
    it('should return default embeddings model when no ID provided', () => {
      const model = (vectorBase as unknown as { GetAIModel: (id?: string) => { ID: string; AIModelType: string } }).GetAIModel();
      expect(model).toBeDefined();
      expect(model.AIModelType).toBe('Embeddings');
    it('should return specific model when ID provided', () => {
      const model = (vectorBase as unknown as { GetAIModel: (id?: string) => { ID: string } }).GetAIModel('model-1');
      expect(model.ID).toBe('model-1');
    it('should throw when no embeddings model found', () => {
      const origModels = [...mockModels];
      mockModels.push({ ID: 'model-x', AIModelType: 'LLM', DriverClass: 'X' });
      expect(() =>
        (vectorBase as unknown as { GetAIModel: (id?: string) => unknown }).GetAIModel()
      ).toThrow('No AI Model Entity found');
      origModels.forEach(m => mockModels.push(m));
  describe('GetVectorDatabase (protected)', () => {
    it('should return default vector database when no ID provided', () => {
      const db = (vectorBase as unknown as { GetVectorDatabase: (id?: string) => { ID: string } }).GetVectorDatabase();
      expect(db).toBeDefined();
      expect(db.ID).toBe('vdb-1');
    it('should return specific vector database when ID provided', () => {
      const db = (vectorBase as unknown as { GetVectorDatabase: (id?: string) => { ID: string } }).GetVectorDatabase('vdb-1');
    it('should throw when no vector database found', () => {
      const origDBs = [...mockVectorDBs];
      mockVectorDBs.length = 0;
        (vectorBase as unknown as { GetVectorDatabase: (id?: string) => unknown }).GetVectorDatabase()
      ).toThrow('No Vector Database Entity found');
      origDBs.forEach(d => mockVectorDBs.push(d));
  describe('GetRecordsByEntityID (protected)', () => {
    it('should retrieve records by entity ID', async () => {
      const mockRecords = [{ ID: 'rec-1' }, { ID: 'rec-2' }];
      mockRunView.mockResolvedValue({
        Results: mockRecords,
      const records = await (vectorBase as unknown as { GetRecordsByEntityID: (entityID: string) => Promise<unknown[]> }).GetRecordsByEntityID('entity-1');
      expect(records).toEqual(mockRecords);
    it('should throw when entity not found', async () => {
        (vectorBase as unknown as { GetRecordsByEntityID: (entityID: string) => Promise<unknown[]> }).GetRecordsByEntityID('non-existent')
      ).rejects.toThrow('Entity with ID non-existent not found');
    it('should throw when RunView fails', async () => {
        ErrorMessage: 'Query failed',
        (vectorBase as unknown as { GetRecordsByEntityID: (entityID: string) => Promise<unknown[]> }).GetRecordsByEntityID('entity-1')
      ).rejects.toThrow('Query failed');
  describe('RunViewForSingleValue (protected)', () => {
    it('should return single entity when found', async () => {
      const mockEntity = { ID: 'rec-1', Name: 'Test' };
        RowCount: 1,
        Results: [mockEntity],
      const result = await (vectorBase as unknown as { RunViewForSingleValue: <T>(entityName: string, filter: string) => Promise<T | null> })
        .RunViewForSingleValue('MJTestEntity', "ID = '1'");
      expect(result).toBe(mockEntity);
    it('should return null when no results', async () => {
        .RunViewForSingleValue('MJTestEntity', "ID = 'missing'");
    it('should return null on RunView failure', async () => {
        ErrorMessage: 'Failed',
  describe('SaveEntity (protected)', () => {
    it('should set ContextCurrentUser and save', async () => {
      const mockSave = vi.fn().mockResolvedValue(true);
        ContextCurrentUser: null as unknown,
        Save: mockSave,
      const result = await (vectorBase as unknown as { SaveEntity: (entity: typeof mockEntity) => Promise<boolean> })
        .SaveEntity(mockEntity);
      expect(mockEntity.ContextCurrentUser).toBe(vectorBase.CurrentUser);
      expect(mockSave).toHaveBeenCalled();
  describe('PageRecordsByEntityID (protected)', () => {
    it('should page records correctly', async () => {
      const mockRecords = [{ ID: '1' }, { ID: '2' }];
      const result = await (vectorBase as unknown as {
        PageRecordsByEntityID: <T>(params: { EntityID: string; PageNumber: number; PageSize: number; ResultType: string }) => Promise<T[]>
      }).PageRecordsByEntityID({
        EntityID: 'entity-1',
        PageNumber: 1,
        PageSize: 10,
      expect(result).toEqual(mockRecords);
        (vectorBase as unknown as {
          EntityID: 'non-existent',
