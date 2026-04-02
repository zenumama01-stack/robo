    ChatResult: class {},
    ModelUsage: class {},
vi.mock('@memberjunction/ai-openai', () => {
  class MockOpenAILLM {
    protected _baseUrl: string;
      this._baseUrl = baseUrl || 'https://api.openai.com/v1';
  return { OpenAILLM: MockOpenAILLM };
import { OpenRouterLLM } from '../models/openRouter';
describe('OpenRouterLLM', () => {
  let llm: OpenRouterLLM;
    llm = new OpenRouterLLM('test-openrouter-key');
      expect(llm).toBeInstanceOf(OpenRouterLLM);
    it('should set the base URL to OpenRouter API', () => {
      expect((llm as unknown as Record<string, unknown>)['_baseUrl']).toBe('https://openrouter.ai/api/v1');
    it('should pass the API key to the parent class', () => {
      expect((llm as unknown as Record<string, unknown>)['_apiKey']).toBe('test-openrouter-key');
  /* ---- Inheritance ---- */
  describe('inheritance', () => {
    it('should inherit SupportsStreaming from OpenAILLM', () => {
