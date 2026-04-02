const mockRerank = vi.hoisted(() => vi.fn());
vi.mock('cohere-ai', () => {
    CohereClient: class MockCohereClient {
      rerank = mockRerank;
  class MockBaseReranker {
    constructor(apiKey: string, modelName: string) {
      this._modelName = modelName;
    get apiKey() { return this._apiKey; }
    BaseReranker: MockBaseReranker,
    RerankParams: class {},
    RerankResult: class {},
import { CohereReranker, createCohereReranker } from '../models/CohereReranker';
describe('CohereReranker', () => {
  let reranker: CohereReranker;
    reranker = new CohereReranker('test-cohere-key');
    it('should create an instance with default model name', () => {
      expect(reranker).toBeInstanceOf(CohereReranker);
      expect((reranker as unknown as Record<string, unknown>)['_modelName']).toBe('rerank-v3.5');
    it('should accept a custom model name', () => {
      const custom = new CohereReranker('key', 'rerank-multilingual-v3.0');
      expect((custom as unknown as Record<string, unknown>)['_modelName']).toBe('rerank-multilingual-v3.0');
    it('should create a CohereClient with the provided API key', () => {
      expect((reranker as unknown as Record<string, unknown>)['_client']).toBeDefined();
  /* ---- doRerank ---- */
  describe('doRerank', () => {
    it('should call Cohere rerank API and map results', async () => {
      mockRerank.mockResolvedValueOnce({
        results: [
          { index: 1, relevanceScore: 0.95 },
          { index: 0, relevanceScore: 0.72 },
        query: 'What is the capital of France?',
        documents: [
          { id: 'doc1', text: 'Paris is in France.' },
          { id: 'doc2', text: 'London is in England.' },
        topK: 2,
      const fn = (reranker as unknown as Record<string, (...args: unknown[]) => Promise<unknown>>)['doRerank']
        .bind(reranker);
      const results = await fn(params) as Array<Record<string, unknown>>;
      expect(results[0].id).toBe('doc2');
      expect(results[0].relevanceScore).toBe(0.95);
      expect(results[0].rank).toBe(0);
      expect(results[1].id).toBe('doc1');
      expect(results[1].relevanceScore).toBe(0.72);
      expect(results[1].rank).toBe(1);
    it('should pass correct parameters to Cohere API', async () => {
      mockRerank.mockResolvedValueOnce({ results: [] });
        documents: [{ id: 'd1', text: 'some text' }],
        topK: 5,
      await fn(params);
      expect(mockRerank).toHaveBeenCalledWith(
          model: 'rerank-v3.5',
          documents: ['some text'],
          topN: 5,
          returnDocuments: false,
    it('should use document count as topN when topK is not provided', async () => {
        query: 'query',
          { id: 'd1', text: 'text 1' },
          { id: 'd2', text: 'text 2' },
          { id: 'd3', text: 'text 3' },
        expect.objectContaining({ topN: 3 }),
    it('should preserve document metadata in results', async () => {
        results: [{ index: 0, relevanceScore: 0.88 }],
      const originalDoc = { id: 'doc-with-meta', text: 'sample text', metadata: { source: 'test' } };
        documents: [originalDoc],
        topK: 1,
      expect(results[0].document).toBe(originalDoc);
  /* ---- createCohereReranker factory ---- */
  describe('createCohereReranker', () => {
    it('should create a CohereReranker instance', () => {
      const instance = createCohereReranker('key123');
      expect(instance).toBeInstanceOf(CohereReranker);
    it('should pass model name to constructor', () => {
      const instance = createCohereReranker('key123', 'custom-model');
      expect((instance as unknown as Record<string, unknown>)['_modelName']).toBe('custom-model');
