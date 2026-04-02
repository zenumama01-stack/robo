import { BaseReranker } from '../generic/baseReranker';
import { RerankParams, RerankResult, RerankResponse, RerankDocument } from '../generic/reranker.types';
class TestReranker extends BaseReranker {
    protected async doRerank(params: RerankParams): Promise<RerankResult[]> {
        // Simple mock: reverse order and assign decreasing scores
        return params.documents.map((doc, idx) => ({
            id: doc.id,
            relevanceScore: 1 - (idx * 0.1),
            document: doc,
            rank: idx
// Reranker that throws errors
class ErrorReranker extends BaseReranker {
        throw new Error('Provider connection failed');
describe('BaseReranker', () => {
    let reranker: TestReranker;
        reranker = new TestReranker('test-api-key', 'test-model');
        it('should set the API key and model name', () => {
            expect(reranker.ModelName).toBe('test-model');
        it('should default model name to empty string when not provided', () => {
            const r = new TestReranker('key');
            expect(r.ModelName).toBe('');
    describe('Rerank', () => {
        const makeDocuments = (count: number): RerankDocument[] =>
            Array.from({ length: count }, (_, i) => ({
                id: `doc-${i}`,
                text: `Document content ${i}`,
                metadata: { source: `test-${i}` }
        it('should rerank documents successfully', async () => {
            const response = await reranker.Rerank({
                query: 'test query',
                documents: makeDocuments(3)
            expect(response.success).toBe(true);
            expect(response.results).toHaveLength(3);
            expect(response.durationMs).toBeGreaterThanOrEqual(0);
            expect(response.modelName).toBe('test-model');
        it('should apply topK limit', async () => {
                documents: makeDocuments(5),
                topK: 2
            expect(response.results).toHaveLength(2);
        it('should update rank values after topK limiting', async () => {
                topK: 3
            expect(response.results[0].rank).toBe(0);
            expect(response.results[1].rank).toBe(1);
            expect(response.results[2].rank).toBe(2);
        it('should return error for empty query', async () => {
                query: '',
                documents: makeDocuments(1)
            expect(response.success).toBe(false);
            expect(response.errorMessage).toBe('Query cannot be empty');
        it('should return error for whitespace-only query', async () => {
                query: '   ',
        it('should return empty results for empty documents array', async () => {
                documents: []
            expect(response.results).toEqual([]);
        it('should return error for document missing id', async () => {
                documents: [{ id: '', text: 'content' }]
            expect(response.errorMessage).toContain('missing required field');
        it('should return error for document missing text', async () => {
                documents: [{ id: 'doc-1', text: '' }]
        it('should handle provider errors gracefully', async () => {
            const errorReranker = new ErrorReranker('key', 'error-model');
            const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
            const response = await errorReranker.Rerank({
            expect(response.errorMessage).toBe('Provider connection failed');
        it('should preserve document metadata through reranking', async () => {
            const docs = [
                { id: 'doc-1', text: 'content', metadata: { source: 'wiki', page: 42 } }
                documents: docs
            expect(response.results[0].document.metadata).toEqual({ source: 'wiki', page: 42 });
    describe('sortByRelevance', () => {
        it('should sort results by descending relevance score', () => {
            const results: RerankResult[] = [
                { id: '1', relevanceScore: 0.3, document: { id: '1', text: '' }, rank: 0 },
                { id: '2', relevanceScore: 0.9, document: { id: '2', text: '' }, rank: 1 },
                { id: '3', relevanceScore: 0.6, document: { id: '3', text: '' }, rank: 2 }
            const sorted = (reranker as Record<string, Function>)['sortByRelevance'](results);
            expect(sorted[0].relevanceScore).toBe(0.9);
            expect(sorted[1].relevanceScore).toBe(0.6);
            expect(sorted[2].relevanceScore).toBe(0.3);
    describe('filterByThreshold', () => {
        it('should filter results below minimum score', () => {
                { id: '1', relevanceScore: 0.9, document: { id: '1', text: '' }, rank: 0 },
                { id: '2', relevanceScore: 0.3, document: { id: '2', text: '' }, rank: 1 },
                { id: '3', relevanceScore: 0.7, document: { id: '3', text: '' }, rank: 2 }
            const filtered = (reranker as Record<string, Function>)['filterByThreshold'](results, 0.5);
            expect(filtered).toHaveLength(2);
            expect(filtered.every((r: RerankResult) => r.relevanceScore >= 0.5)).toBe(true);
