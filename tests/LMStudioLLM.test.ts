const mockModel = vi.hoisted(() => vi.fn());
vi.mock('@lmstudio/sdk', () => {
    LMStudioClient: class MockLMStudioClient {
      llm = { model: mockModel };
      constructor(_opts?: Record<string, unknown>) {}
    public SetAdditionalSettings(settings: Record<string, unknown>) {
      this._additionalSettings = { ...this._additionalSettings, ...settings };
    protected initializeThinkingStreamState() {}
    protected processStreamChunkWithThinking(content: string) { return content; }
    protected get thinkingStreamState() {
      return { accumulatedThinking: '' };
    protected addThinkingToMessage(msg: Record<string, unknown>, thinking?: string) {
        return { ...msg, thinking };
      return msg;
    modelSpecificResponseDetails: unknown;
import { LMStudioLLM } from '../models/lm-studio';
describe('LMStudioLLM', () => {
  let llm: LMStudioLLM;
    llm = new LMStudioLLM();
    it('should create an instance without API key', () => {
      expect(llm).toBeInstanceOf(LMStudioLLM);
    it('should create an instance with optional API key', () => {
      const inst = new LMStudioLLM('optional-key');
      expect(inst).toBeInstanceOf(LMStudioLLM);
    it('should expose LMStudioClient getter', () => {
      expect(llm.LMStudioClient).toBeDefined();
    it('should expose client getter (alias)', () => {
      expect(llm.client).toBe(llm.LMStudioClient);
  /* ---- SetAdditionalSettings ---- */
    it('should reconfigure client with new baseUrl', () => {
      const clientBefore = llm.client;
      llm.SetAdditionalSettings({ baseUrl: 'http://custom:1234' });
      // The client should be a new instance
      const clientAfter = llm.client;
      expect(clientAfter).not.toBe(clientBefore);
  /* ---- nonStreamingChatCompletion ---- */
    it('should call model.respond and return success', async () => {
      const mockRespond = vi.fn().mockResolvedValue({
        nonReasoningContent: 'Hello from LM Studio',
        reasoningContent: undefined,
        stats: { promptTokensCount: 10, predictedTokensCount: 20 },
      mockModel.mockResolvedValue({ respond: mockRespond });
        model: 'local-model',
      expect(data.choices[0].message.content).toBe('Hello from LM Studio');
    it('should include thinking content when present', async () => {
        nonReasoningContent: 'The answer is 42',
        reasoningContent: 'Let me think about this...',
        stats: {},
        messages: [{ role: 'user', content: 'What is 6x7?' }],
      const data = result.data as { choices: Array<{ message: { content: string; thinking: string } }> };
      expect(data.choices[0].message.thinking).toBe('Let me think about this...');
    it('should return error result on failure', async () => {
      mockModel.mockRejectedValue(new Error('Model not loaded'));
        model: 'missing-model',
      expect(result.errorMessage).toContain('Model not loaded');
