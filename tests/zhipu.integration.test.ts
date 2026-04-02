  let zhipu: ZhipuLLM;
  const apiKey = process.env.AI_VENDOR_API_KEY__ZhipuLLM || 'test-key';
    zhipu = new ZhipuLLM(apiKey);
      expect(zhipu).toBeInstanceOf(ZhipuLLM);
      expect(zhipu.OpenAI).toBeDefined();
      expect(zhipu.SupportsStreaming).toBe(true);
      const role = zhipu.ConvertMJToOpenAIRole('system');
      const role = zhipu.ConvertMJToOpenAIRole('user');
      const role = zhipu.ConvertMJToOpenAIRole('assistant');
      expect(() => zhipu.ConvertMJToOpenAIRole('unknown')).toThrow();
      const converted = zhipu.ConvertMJToOpenAIChatMessages(messages);
    const hasApiKey = !!process.env.AI_VENDOR_API_KEY__ZhipuLLM;
    (hasApiKey ? it : it.skip)('should make successful chat completion with GLM 5', async () => {
        model: 'glm-5',
      const result = await zhipu.ChatCompletion(params);
    (hasApiKey ? it : it.skip)('should make successful chat completion with GLM 4.7', async () => {
        model: 'glm-4-plus',
    (hasApiKey ? it : it.skip)('should support streaming response', async () => {
          { role: ChatMessageRole.user, content: 'Say hello in one word' }
      // Test that streaming capability is inherited
      // Make a standard completion to verify the model works
