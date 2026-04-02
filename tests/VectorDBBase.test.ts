import { VectorDBBase } from '../generic/VectorDBBase';
  BaseRequestParams,
  BaseResponse,
  CreateIndexParams,
  EditIndexParams,
  IndexList,
  UpdateOptions,
  VectorRecord,
  RecordValues,
  RecordSparseValues,
  RecordMetadata,
  RecordMetadataValue,
  IndexDescription,
} from '../generic/record';
import { QueryOptions, QueryByRecordId, QueryByVectorValues, ScoredRecord, QueryResponse } from '../generic/query.types';
 * Concrete implementation of VectorDBBase for testing
class TestVectorDB extends VectorDBBase {
  listIndexes(): IndexList {
    return { indexes: [] };
  getIndex(params: BaseRequestParams): BaseResponse {
    return { success: true, message: 'ok', data: null };
  createIndex(params: CreateIndexParams): BaseResponse {
    return { success: true, message: 'created', data: null };
  deleteIndex(params: BaseRequestParams): BaseResponse {
    return { success: true, message: 'deleted', data: null };
  editIndex(params: EditIndexParams): BaseResponse {
    return { success: true, message: 'edited', data: null };
  queryIndex(params: QueryOptions): BaseResponse {
    return { success: true, message: 'queried', data: null };
  createRecord(record: VectorRecord): BaseResponse {
    return { success: true, message: 'record created', data: null };
  createRecords(records: VectorRecord[]): BaseResponse {
    return { success: true, message: 'records created', data: null };
  getRecord(param: BaseRequestParams): BaseResponse {
    return { success: true, message: 'record fetched', data: null };
  getRecords(params: BaseRequestParams): BaseResponse {
    return { success: true, message: 'records fetched', data: null };
  updateRecord(record: UpdateOptions): BaseResponse {
    return { success: true, message: 'record updated', data: null };
  updateRecords(records: UpdateOptions): BaseResponse {
    return { success: true, message: 'records updated', data: null };
  deleteRecord(record: VectorRecord): BaseResponse {
    return { success: true, message: 'record deleted', data: null };
  deleteRecords(records: VectorRecord[]): BaseResponse {
    return { success: true, message: 'records deleted', data: null };
  // Expose apiKey for testing
  getApiKeyForTest(): string {
describe('VectorDBBase', () => {
    it('should accept a valid API key', () => {
      const db = new TestVectorDB('test-api-key');
      expect(db.getApiKeyForTest()).toBe('test-api-key');
    it('should throw for empty API key', () => {
      expect(() => new TestVectorDB('')).toThrow('API key cannot be empty');
    it('should throw for whitespace-only API key', () => {
      expect(() => new TestVectorDB('   ')).toThrow('API key cannot be empty');
  describe('index operations', () => {
    let db: TestVectorDB;
      db = new TestVectorDB('test-key');
    it('should list indexes', () => {
      const result = db.listIndexes();
      expect(result.indexes).toEqual([]);
    it('should get an index', () => {
      const result = db.getIndex({ id: 'test-index' });
    it('should create an index', () => {
      const result = db.createIndex({
        id: 'new-index',
        dimension: 128,
      expect(result.message).toBe('created');
    it('should delete an index', () => {
      const result = db.deleteIndex({ id: 'test-index' });
    it('should edit an index', () => {
      const result = db.editIndex({ id: 'test-index' });
    it('should query an index by vector values', () => {
      const queryParams: QueryByVectorValues = {
        vector: [0.1, 0.2, 0.3],
        includeValues: true,
        includeMetadata: true,
      const result = db.queryIndex(queryParams);
    it('should query an index by record ID', () => {
      const queryParams: QueryByRecordId = {
        id: 'record-123',
  describe('record operations', () => {
    it('should create a record', () => {
      const record: VectorRecord = {
        id: 'rec1',
        values: [0.1, 0.2, 0.3],
        metadata: { label: 'test' },
      const result = db.createRecord(record);
    it('should create multiple records', () => {
      const records: VectorRecord[] = [
        { id: 'rec1', values: [0.1, 0.2] },
        { id: 'rec2', values: [0.3, 0.4] },
      const result = db.createRecords(records);
    it('should get a record', () => {
      const result = db.getRecord({ id: 'rec1' });
    it('should get records', () => {
      const result = db.getRecords({ id: 'batch-id' });
    it('should update a record', () => {
      const update: UpdateOptions = {
        values: [0.5, 0.6],
        metadata: { updated: 'true' },
      const result = db.updateRecord(update);
    it('should update records', () => {
        id: 'batch-id',
        metadata: { batch: 'true' },
      const result = db.updateRecords(update);
    it('should delete a record', () => {
      const record: VectorRecord = { id: 'rec1', values: [0.1] };
      const result = db.deleteRecord(record);
    it('should delete records', () => {
        { id: 'rec1', values: [0.1] },
        { id: 'rec2', values: [0.2] },
      const result = db.deleteRecords(records);
describe('Record types', () => {
  it('should create a valid VectorRecord', () => {
      id: 'test-id',
      metadata: { key: 'value' },
    expect(record.id).toBe('test-id');
    expect(record.values).toHaveLength(3);
    expect(record.metadata).toEqual({ key: 'value' });
  it('should create a VectorRecord with sparse values', () => {
      id: 'sparse-id',
      values: [0.1, 0.2],
      sparseValues: {
        indices: [0, 5, 10],
        values: [0.1, 0.5, 1.0],
    expect(record.sparseValues).toBeDefined();
    expect(record.sparseValues!.indices).toHaveLength(3);
  it('should support various metadata value types', () => {
      id: 'meta-test',
      values: [1.0],
        stringVal: 'hello',
        boolVal: true,
        numVal: 42,
        arrayVal: ['a', 'b', 'c'],
    expect(record.metadata!['stringVal']).toBe('hello');
    expect(record.metadata!['boolVal']).toBe(true);
    expect(record.metadata!['numVal']).toBe(42);
    expect(record.metadata!['arrayVal']).toEqual(['a', 'b', 'c']);
  it('should create valid IndexDescription', () => {
    const index: IndexDescription = {
      name: 'my-index',
      dimension: 768,
      host: 'https://example.com',
    expect(index.name).toBe('my-index');
    expect(index.dimension).toBe(768);
  it('should create valid ScoredRecord', () => {
    const scored: ScoredRecord = {
      id: 'match-1',
      score: 0.95,
    expect(scored.score).toBe(0.95);
