import { ZhipuLLM } from '../models/zhipu';
describe('ZhipuLLM', () => {
  let llm: ZhipuLLM;
    llm = new ZhipuLLM('test-zhipu-key');
      expect(llm).toBeInstanceOf(ZhipuLLM);
    it('should set the base URL to Z.AI API', () => {
      expect((llm as unknown as Record<string, unknown>)['_baseUrl']).toBe('https://api.z.ai/api/paas/v4');
      expect((llm as unknown as Record<string, unknown>)['_apiKey']).toBe('test-zhipu-key');
