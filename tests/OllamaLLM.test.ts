const mockChat = vi.hoisted(() => vi.fn());
const mockGenerate = vi.hoisted(() => vi.fn());
const mockList = vi.hoisted(() => vi.fn());
const mockPull = vi.hoisted(() => vi.fn());
const mockEmbeddings = vi.hoisted(() => vi.fn());
const mockShow = vi.hoisted(() => vi.fn());
vi.mock('ollama', () => {
    Ollama: class MockOllama {
      chat = mockChat;
      generate = mockGenerate;
      list = mockList;
      pull = mockPull;
      embeddings = mockEmbeddings;
      show = mockShow;
    ChatRequest: class {},
    ChatResponse: class {},
    GenerateRequest: class {},
    GenerateResponse: class {},
    Message: class {},
    EmbeddingsRequest: class {},
    EmbeddingsResponse: class {},
    protected extractThinkingFromContent(content: string) {
        const thinking = content.substring('<think>'.length, thinkEnd).trim();
        const remaining = content.substring(thinkEnd + '</think>'.length).trim();
        return { content: remaining, thinking };
      if (thinking) return { ...msg, thinking };
import { OllamaLLM } from '../models/ollama-llm';
import { OllamaEmbedding } from '../models/ollama-embeddings';
/*  OllamaLLM Tests                                                   */
describe('OllamaLLM', () => {
  let llm: OllamaLLM;
    llm = new OllamaLLM();
      expect(llm).toBeInstanceOf(OllamaLLM);
    it('should set default base URL to localhost:11434', () => {
      expect((llm as unknown as Record<string, unknown>)['_baseUrl']).toBe('http://localhost:11434');
    it('should expose OllamaClient getter', () => {
      expect(llm.OllamaClient).toBeDefined();
    it('should expose client alias', () => {
      expect(llm.client).toBe(llm.OllamaClient);
      llm.SetAdditionalSettings({ baseUrl: 'http://remote:11434' });
      expect(llm.client).not.toBe(clientBefore);
      expect((llm as unknown as Record<string, unknown>)['_baseUrl']).toBe('http://remote:11434');
    it('should accept host as alias for baseUrl', () => {
      llm.SetAdditionalSettings({ host: 'http://otherhost:11434' });
      expect((llm as unknown as Record<string, unknown>)['_baseUrl']).toBe('http://otherhost:11434');
    it('should update keepAlive setting', () => {
      llm.SetAdditionalSettings({ keepAlive: '10m' });
      expect((llm as unknown as Record<string, unknown>)['_keepAlive']).toBe('10m');
    it('should call Ollama chat and return success', async () => {
      mockChat.mockResolvedValueOnce({
        message: { content: 'Hello from Ollama!' },
        done: true,
        prompt_eval_count: 10,
        eval_count: 20,
        total_duration: 1000000,
        load_duration: 500000,
        model: 'llama3',
      expect(data.choices[0].message.content).toBe('Hello from Ollama!');
    it('should extract thinking content from response', async () => {
        message: { content: '<think>reasoning here</think>The answer is 42' },
        model: 'deepseek-r1',
      expect(data.choices[0].message.content).toBe('The answer is 42');
      expect(data.choices[0].message.thinking).toBe('reasoning here');
      mockChat.mockRejectedValueOnce(new Error('Connection refused'));
      expect(result.errorMessage).toContain('Connection refused');
    it('should handle JSON response format', async () => {
        message: { content: '{"answer": 42}' },
        messages: [{ role: 'user', content: 'Give me JSON' }],
      // Verify format was set in the chat call
      expect(mockChat).toHaveBeenCalledWith(
        expect.objectContaining({ format: 'json' }),
  /* ---- Image handling ---- */
  describe('convertToOllamaMessages', () => {
    it('should handle multimodal messages with images', () => {
      const fn = (llm as unknown as Record<string, (...args: unknown[]) => unknown>)['convertToOllamaMessages']
      const messages = [{
          { type: 'image_url', content: 'data:image/png;base64,abc123' },
      const result = fn(messages) as Array<{ role: string; content: string; images?: string[] }>;
      expect(result[0].content).toBe('What is in this image?');
      expect(result[0].images).toHaveLength(1);
      expect(result[0].images![0]).toBe('abc123');
    it('should handle string content messages', () => {
      const messages = [{ role: 'user', content: 'Hello' }];
      expect(result[0].content).toBe('Hello');
      expect(result[0].images).toBeUndefined();
  /* ---- extractBase64ForOllama ---- */
  describe('extractBase64ForOllama', () => {
    it('should extract base64 from data URL', () => {
      const fn = (llm as unknown as Record<string, (...args: unknown[]) => unknown>)['extractBase64ForOllama']
      const result = fn({ content: 'data:image/png;base64,abc123' });
      expect(result).toBe('abc123');
    it('should return raw base64 content directly', () => {
      const result = fn({ content: 'rawbase64data' });
      expect(result).toBe('rawbase64data');
      const result = fn({ content: 'https://example.com/img.png' });
  /* ---- listModels ---- */
  describe('listModels', () => {
    it('should return list of models', async () => {
      mockList.mockResolvedValueOnce({ models: [{ name: 'llama3:latest' }] });
      const result = await llm.listModels();
      expect(result.models).toHaveLength(1);
  /* ---- isModelAvailable ---- */
  describe('isModelAvailable', () => {
    it('should return true when model exists', async () => {
      const result = await llm.isModelAvailable('llama3');
    it('should return false when model does not exist', async () => {
      const result = await llm.isModelAvailable('mistral');
      expect(result).toBe(false);
    it('should return false on error', async () => {
      mockList.mockRejectedValueOnce(new Error('Connection refused'));
      await expect(llm.SummarizeText({} as never)).rejects.toThrow();
      await expect(llm.ClassifyText({} as never)).rejects.toThrow();
/*  OllamaEmbedding Tests                                             */
describe('OllamaEmbedding', () => {
  let embedder: OllamaEmbedding;
    embedder = new OllamaEmbedding();
      expect(embedder).toBeInstanceOf(OllamaEmbedding);
    it('should expose client getter', () => {
      expect(embedder.client).toBeDefined();
    it('should throw when model name is missing', async () => {
      await expect(embedder.EmbedText({ text: 'hello' } as never))
        .rejects.toThrow('Model name is required');
    it('should return embedding result on success', async () => {
      mockList.mockResolvedValueOnce({ models: [{ name: 'nomic-embed-text:latest' }] });
      mockEmbeddings.mockResolvedValueOnce({
        embedding: [0.1, 0.2, 0.3],
        prompt_eval_count: 5,
      const result = await embedder.EmbedText({ text: 'hello', model: 'nomic-embed-text' });
      expect(result.vector).toEqual([0.1, 0.2, 0.3]);
      expect(result.model).toBe('nomic-embed-text');
      const clientBefore = embedder.client;
      embedder.SetAdditionalSettings({ baseUrl: 'http://remote:11434' });
      expect(embedder.client).not.toBe(clientBefore);
