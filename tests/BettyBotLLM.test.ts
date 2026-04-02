/*  Hoisted mocks – must be declared before any import that pulls     */
/*  the module under test.                                            */
const mockAxiosPost = vi.hoisted(() => vi.fn());
const mockAxiosIsAxiosError = vi.hoisted(() => vi.fn().mockReturnValue(false));
vi.mock('axios', () => ({
    post: mockAxiosPost,
    isAxiosError: mockAxiosIsAxiosError,
vi.mock('../config', () => ({
  BETTY_BOT_BASE_URL: 'https://betty-api.test.co/',
    get SupportsStreaming() { return false; }
    errorMessage: string | null;
    exception: unknown;
    errorInfo: unknown;
vi.mock('env-var', () => ({
    get: () => ({
      default: () => ({ asString: () => 'https://betty-api.test.co/' }),
      asString: () => 'https://betty-api.test.co/',
import { BettyBotLLM } from '../models/BettyBotLLM';
describe('BettyBotLLM', () => {
  let llm: BettyBotLLM;
    llm = new BettyBotLLM('test-api-key');
      expect(llm).toBeInstanceOf(BettyBotLLM);
    it('should initialize JWTToken as empty string', () => {
      // The JWT starts empty; GetJWTToken fetches a fresh one
      expect((llm as unknown as Record<string, unknown>)['JWTToken']).toBe('');
    it('should initialize TokenExpiration as a Date', () => {
      expect((llm as unknown as Record<string, unknown>)['TokenExpiration']).toBeInstanceOf(Date);
    it('should return false', () => {
  /* ---- GetJWTToken ---- */
  describe('GetJWTToken', () => {
    it('should call settings endpoint and return token on success', async () => {
      mockAxiosPost.mockResolvedValueOnce({
          status: 'SUCCESS',
          enabledFeatures: [],
          token: 'jwt-123',
      const result = await llm.GetJWTToken();
      expect(result!.status).toBe('SUCCESS');
      expect(result!.token).toBe('jwt-123');
      expect(mockAxiosPost).toHaveBeenCalledWith(
        'https://betty-api.test.co/settings',
        { token: 'test-api-key' },
    it('should return cached token if still valid', async () => {
      // First call – fetches the token
          token: 'jwt-cached',
      await llm.GetJWTToken();
      // Second call should use cached token (no second axios call)
      expect(result!.token).toBe('jwt-cached');
      expect(mockAxiosPost).toHaveBeenCalledTimes(1);
    it('should force-refresh token when forceRefresh is true', async () => {
      mockAxiosPost
        .mockResolvedValueOnce({
          data: { status: 'SUCCESS', errorMessage: '', enabledFeatures: [], token: 'jwt-1' },
          data: { status: 'SUCCESS', errorMessage: '', enabledFeatures: [], token: 'jwt-2' },
      const result = await llm.GetJWTToken(true);
      expect(result!.token).toBe('jwt-2');
      expect(mockAxiosPost).toHaveBeenCalledTimes(2);
    it('should return null when axios throws', async () => {
      mockAxiosPost.mockRejectedValueOnce(new Error('network error'));
  /* ---- nonStreamingChatCompletion (via ChatCompletion) ---- */
    it('should return error result when no user message is present', async () => {
      // First mock JWT call
        data: { status: 'SUCCESS', errorMessage: '', enabledFeatures: [], token: 'jwt' },
        messages: [{ role: ChatMessageRole.system, content: 'system prompt' }],
        model: 'betty',
      // Access the protected method
      const result = await (llm as unknown as { nonStreamingChatCompletion: (p: unknown) => Promise<unknown> })
        .nonStreamingChatCompletion(params);
      const chatResult = result as Record<string, unknown>;
      expect(chatResult.success).toBe(false);
      expect(chatResult.errorMessage).toBe('No user message found in params');
    it('should return error result when JWT retrieval fails', async () => {
      mockAxiosPost.mockRejectedValueOnce(new Error('jwt fail'));
        messages: [{ role: ChatMessageRole.user, content: 'hello' }],
    it('should return successful result with response data', async () => {
      // JWT call
      // Betty response call
          status: 'ok',
          conversationId: 1,
          response: 'Hello from Betty!',
          references: [],
        messages: [{ role: ChatMessageRole.user, content: 'hi' }],
      expect(chatResult.success).toBe(true);
      expect(chatResult.statusText).toBe('OK');
      const data = chatResult.data as { choices: Array<{ message: { content: string } }> };
      expect(data.choices[0].message.content).toBe('Hello from Betty!');
    it('should include references as additional choices', async () => {
          response: 'Here is info',
          references: [
            { title: 'Doc 1', link: 'https://doc1.com', type: 'article' },
            { title: 'Doc 2', link: 'https://doc2.com', type: 'article' },
        messages: [{ role: ChatMessageRole.user, content: 'question' }],
      const data = chatResult.data as { choices: Array<{ message: { content: string }; finish_reason: string }> };
      expect(data.choices).toHaveLength(3);
      expect(data.choices[1].message.content).toContain('Doc 1');
      expect(data.choices[2].finish_reason).toBe('references_json');
