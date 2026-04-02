import { xAILLM } from '../models/x.ai';
describe('xAILLM', () => {
  let llm: xAILLM;
    llm = new xAILLM('test-xai-key');
      expect(llm).toBeInstanceOf(xAILLM);
    it('should set the base URL to xAI API', () => {
      expect((llm as unknown as Record<string, unknown>)['_baseUrl']).toBe('https://api.x.ai/v1');
      expect((llm as unknown as Record<string, unknown>)['_apiKey']).toBe('test-xai-key');
