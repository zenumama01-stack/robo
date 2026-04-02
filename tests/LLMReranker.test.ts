 * Unit tests for LLMReranker and createLLMReranker
// Mock dependencies before imports
        constructor(_apiKey: string, modelName?: string) {
        get ModelName(): string {
        protected sortByRelevance<T extends { relevanceScore: number }>(results: T[]): T[] {
    return { BaseReranker: MockBaseReranker };
const mockPromptsArray: Array<{ ID: string; Name: string }> = [];
            get Prompts() {
                return mockPromptsArray;
const mockExecutePrompt = vi.fn();
vi.mock('@memberjunction/ai-prompts', () => ({
    AIPromptRunner: class {
        ExecutePrompt = mockExecutePrompt;
    AIPromptParams: class {
        prompt: unknown = null;
        contextUser: unknown = null;
        attemptJSONRepair = false;
        data: unknown = null;
    AIPromptEntityExtended: class {},
import { LLMReranker, createLLMReranker } from '../LLMReranker';
// Helper to access protected doRerank
interface RerankCallable {
    doRerank(params: {
        documents: Array<{ id: string; text: string; metadata?: Record<string, unknown> }>;
    }): Promise<Array<{ id: string; relevanceScore: number; document: { id: string; text: string }; rank: number }>>;
describe('LLMReranker', () => {
    const mockUser = { ID: 'user-1', Name: 'Test' } as never;
        mockPromptsArray.length = 0;
        mockExecutePrompt.mockReset();
        it('should store the promptID', () => {
            const reranker = new LLMReranker('', '', 'prompt-123', mockUser);
            expect(reranker.PromptID).toBe('prompt-123');
        it('should default modelName to LLM when empty string passed', () => {
            expect(reranker.ModelName).toBe('LLM');
        it('should use provided modelName when given', () => {
            const reranker = new LLMReranker('', 'custom-model', 'prompt-123', mockUser);
            expect(reranker.ModelName).toBe('custom-model');
    describe('PromptID getter', () => {
        it('should return the configured prompt ID', () => {
            const reranker = new LLMReranker('', '', 'prompt-456', mockUser);
            expect(reranker.PromptID).toBe('prompt-456');
        let reranker: LLMReranker;
        let doRerank: RerankCallable['doRerank'];
            reranker = new LLMReranker('', '', 'prompt-123', mockUser);
            doRerank = (reranker as unknown as RerankCallable).doRerank.bind(reranker);
        it('should throw when prompt is not found', async () => {
                doRerank({ query: 'test', documents: [{ id: '1', text: 'doc' }] })
            ).rejects.toThrow('Rerank prompt not found');
        it('should throw when prompt execution fails', async () => {
            mockPromptsArray.push({ ID: 'prompt-123', Name: 'Test Prompt' });
            mockExecutePrompt.mockResolvedValue({
                errorMessage: 'Model error',
            ).rejects.toThrow('Prompt execution failed');
        it('should parse array result from prompt execution', async () => {
                result: [
                    { index: 0, score: 0.95 },
                    { index: 1, score: 0.72 },
                rawResult: null,
            const results = await doRerank({
                    { id: 'doc-1', text: 'First document' },
                    { id: 'doc-2', text: 'Second document' },
            expect(results[0].id).toBe('doc-1');
            expect(results[1].id).toBe('doc-2');
        it('should parse string JSON result from rawResult', async () => {
                rawResult: '[{"index":0,"score":0.85}]',
                query: 'test',
                documents: [{ id: 'doc-1', text: 'A document' }],
            expect(results).toHaveLength(1);
            expect(results[0].relevanceScore).toBe(0.85);
        it('should return empty array for invalid JSON string', async () => {
                rawResult: 'not valid json',
                documents: [{ id: 'doc-1', text: 'Doc' }],
            expect(results).toHaveLength(0);
        it('should return empty array for unexpected response type', async () => {
                result: 42,
        it('should skip items with out-of-bounds index', async () => {
                    { index: 0, score: 0.9 },
                    { index: 5, score: 0.8 },
                    { id: 'doc-1', text: 'First' },
                    { id: 'doc-2', text: 'Second' },
        it('should skip items with negative index', async () => {
                    { index: -1, score: 0.9 },
                    { index: 0, score: 0.8 },
            expect(results[0].relevanceScore).toBe(0.8);
        it('should skip items with score outside 0-1 range', async () => {
                    { index: 0, score: 1.5 },
                    { index: 1, score: -0.1 },
                    { index: 0, score: 0.7 },
            expect(results[0].relevanceScore).toBe(0.7);
        it('should skip items with non-numeric index or score', async () => {
                    { index: 'zero', score: 0.9 },
                    { index: 0, score: 'high' },
        it('should use cached prompt on subsequent calls', async () => {
                result: [{ index: 0, score: 0.9 }],
            await doRerank({
                query: 'test1',
                documents: [{ id: '1', text: 'doc' }],
            // Remove prompt from the list to verify caching
                query: 'test2',
                documents: [{ id: '2', text: 'another' }],
        it('should pass topK from params to prompt data', async () => {
            const callArgs = mockExecutePrompt.mock.calls[0][0];
            expect(callArgs.data.topK).toBe(5);
        it('should default topK to document count', async () => {
                    { id: '1', text: 'doc1' },
                    { id: '2', text: 'doc2' },
                    { id: '3', text: 'doc3' },
            expect(callArgs.data.topK).toBe(3);
            expect(callArgs.data.documentCount).toBe(3);
        it('should format documents as indexed text in prompt data', async () => {
                result: [],
                query: 'my query',
                    { id: '1', text: 'Alpha' },
                    { id: '2', text: 'Beta' },
            expect(callArgs.data.documents).toContain('[0] Alpha');
            expect(callArgs.data.documents).toContain('[1] Beta');
            expect(callArgs.data.query).toBe('my query');
        it('should set attemptJSONRepair to true', async () => {
            expect(callArgs.attemptJSONRepair).toBe(true);
        it('should sort results by relevance score descending', async () => {
                    { index: 0, score: 0.3 },
                    { index: 1, score: 0.9 },
                    { index: 2, score: 0.6 },
                    { id: 'doc-1', text: 'Low' },
                    { id: 'doc-2', text: 'High' },
                    { id: 'doc-3', text: 'Mid' },
            expect(results[0].relevanceScore).toBe(0.9);
            expect(results[1].relevanceScore).toBe(0.6);
            expect(results[2].relevanceScore).toBe(0.3);
describe('createLLMReranker', () => {
    it('should create an LLMReranker instance', () => {
        const mockUser = { ID: 'user-1' } as never;
        const reranker = createLLMReranker('prompt-123', mockUser);
        expect(reranker).toBeInstanceOf(LLMReranker);
    it('should set the promptID on the created instance', () => {
        const reranker = createLLMReranker('prompt-789', mockUser);
        expect(reranker.PromptID).toBe('prompt-789');
    it('should default model name to LLM', () => {
