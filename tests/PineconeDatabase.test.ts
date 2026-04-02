const mockDescribeIndex = vi.hoisted(() => vi.fn());
const mockListIndexes = vi.hoisted(() => vi.fn());
const mockCreateIndex = vi.hoisted(() => vi.fn());
const mockDeleteIndex = vi.hoisted(() => vi.fn());
const mockIndex = vi.hoisted(() => vi.fn());
const mockIndexInstance = vi.hoisted(() => ({
  query: vi.fn(),
  upsert: vi.fn(),
  fetch: vi.fn(),
  update: vi.fn(),
  deleteOne: vi.fn(),
  deleteMany: vi.fn(),
  deleteAll: vi.fn(),
  namespace: vi.fn().mockReturnValue({ deleteAll: vi.fn() }),
vi.mock('@pinecone-database/pinecone', () => {
    Pinecone: class MockPinecone {
      describeIndex = mockDescribeIndex;
      listIndexes = mockListIndexes;
      createIndex = mockCreateIndex;
      deleteIndex = mockDeleteIndex;
      Index = mockIndex.mockReturnValue(mockIndexInstance);
      index = mockIndex.mockReturnValue(mockIndexInstance);
    Index: class {},
    FetchResponse: class {},
    QueryOptions: class {},
  pineconeDefaultIndex: 'test-default-index',
  openAIAPIKey: 'fake-openai-key',
  pineconeHost: 'fake-host',
  pineconeAPIKey: 'fake-pinecone-key',
vi.mock('@memberjunction/ai-vectordb', () => {
  class MockVectorDBBase {
    VectorDBBase: MockVectorDBBase,
    BaseRequestParams: class {},
    BaseResponse: class {},
    CreateIndexParams: class {},
    EditIndexParams: class {},
    IndexDescription: class {},
    IndexList: class {},
    QueryResponse: class {},
    RecordMetadata: class {},
    VectorRecord: class {},
import { PineconeDatabase } from '../models/PineconeDatabase';
describe('PineconeDatabase', () => {
  let db: PineconeDatabase;
    db = new PineconeDatabase('test-pinecone-key');
    it('should create an instance with a Pinecone client', () => {
      expect(db).toBeInstanceOf(PineconeDatabase);
      expect(db.pinecone).toBeDefined();
  /* ---- getIndex ---- */
  describe('getIndex', () => {
    it('should return index using provided id', () => {
      const result = db.getIndex({ id: 'my-index' } as never);
      expect(mockIndex).toHaveBeenCalledWith('my-index');
    it('should use default index when no id is provided', () => {
      const result = db.getIndex();
      expect(mockIndex).toHaveBeenCalledWith('test-default-index');
  /* ---- listIndexes ---- */
  describe('listIndexes', () => {
    it('should call Pinecone listIndexes', async () => {
      const mockIndexList = { indexes: [{ name: 'idx-1' }] };
      mockListIndexes.mockResolvedValueOnce(mockIndexList);
      const result = await db.listIndexes();
      expect(result).toEqual(mockIndexList);
  /* ---- getIndexDescription ---- */
  describe('getIndexDescription', () => {
    it('should call describeIndex with the correct id', async () => {
      mockDescribeIndex.mockResolvedValueOnce({ name: 'my-index', dimension: 1536 });
      const result = await db.getIndexDescription({ id: 'my-index' } as never);
      expect(result).toEqual({ name: 'my-index', dimension: 1536 });
      expect(mockDescribeIndex).toHaveBeenCalledWith('my-index');
  /* ---- createIndex ---- */
  describe('createIndex', () => {
    it('should call Pinecone createIndex with correct params', async () => {
      mockCreateIndex.mockResolvedValueOnce({ name: 'new-idx' });
      const result = await db.createIndex({
        id: 'new-idx',
        dimension: 1536,
        metric: 'cosine',
        additionalParams: { serverless: { cloud: 'aws', region: 'us-west-2' } },
    it('should return failure on error', async () => {
      mockCreateIndex.mockRejectedValueOnce(new Error('create fail'));
        id: 'bad-idx',
  /* ---- deleteIndex ---- */
  describe('deleteIndex', () => {
    it('should call Pinecone deleteIndex', async () => {
      mockDeleteIndex.mockResolvedValueOnce(undefined);
      const result = await db.deleteIndex({ id: 'del-idx' } as never);
      expect(mockDeleteIndex).toHaveBeenCalledWith('del-idx');
      mockDeleteIndex.mockRejectedValueOnce(new Error('delete fail'));
      const result = await db.deleteIndex({ id: 'fail-idx' } as never);
  /* ---- queryIndex ---- */
  describe('queryIndex', () => {
    it('should query the index and return results', async () => {
      mockIndexInstance.query.mockResolvedValueOnce({ matches: [{ id: 'match1' }] });
      const result = await db.queryIndex({ vector: [1, 2, 3], topK: 5 } as never);
      mockIndexInstance.query.mockRejectedValueOnce(new Error('query fail'));
      const result = await db.queryIndex({ vector: [1] } as never);
  /* ---- createRecord / createRecords ---- */
  describe('createRecord', () => {
    it('should upsert a single record', async () => {
      mockIndexInstance.upsert.mockResolvedValueOnce(undefined);
      const result = await db.createRecord({ id: 'rec1', values: [1, 2] } as never);
  /* ---- deleteRecord / deleteRecords ---- */
  describe('deleteRecord', () => {
    it('should delete a single record', async () => {
      mockIndexInstance.deleteOne.mockResolvedValueOnce(undefined);
      const result = await db.deleteRecord({ id: 'rec1' } as never);
  describe('deleteRecords', () => {
    it('should delete multiple records by ID', async () => {
      mockIndexInstance.deleteMany.mockResolvedValueOnce(undefined);
      const result = await db.deleteRecords([
        { id: 'r1' }, { id: 'r2' },
      ] as never);
      expect(mockIndexInstance.deleteMany).toHaveBeenCalledWith(['r1', 'r2']);
  /* ---- getDefaultIndex ---- */
  describe('getDefaultIndex', () => {
    it('should return cached default index on second call', async () => {
      const idx = await db.getDefaultIndex();
      expect(idx).toBeDefined();
      const idx2 = await db.getDefaultIndex();
      // second call should return same cached instance
      expect(idx2).toBe(idx);
