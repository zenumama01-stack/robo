// Hoist mock variables so they can be referenced inside vi.mock() factories
const mockModelClientFn = vi.hoisted(() => vi.fn());
const mockAzureKeyCredential = vi.hoisted(() => vi.fn());
const mockDefaultAzureCredential = vi.hoisted(() => vi.fn());
// Mock external SDK dependencies
vi.mock('@azure-rest/ai-inference', () => ({
    default: mockModelClientFn,
vi.mock('@azure/core-auth', () => ({
    AzureKeyCredential: mockAzureKeyCredential,
vi.mock('@azure/identity', () => ({
    DefaultAzureCredential: mockDefaultAzureCredential,
    RegisterClass: () => (target: Function) => target,
import { AzureLLM } from '../models/azure';
describe('AzureLLM', () => {
    let instance: AzureLLM;
    const testApiKey = 'test-azure-api-key';
        mockModelClientFn.mockReturnValue({
            path: vi.fn().mockReturnValue({
                post: vi.fn(),
        mockAzureKeyCredential.mockImplementation(function (this: Record<string, string>, key: string) {
            this.key = key;
        mockDefaultAzureCredential.mockImplementation(function (this: Record<string, string>) {
            this.type = 'default';
        instance = new AzureLLM(testApiKey);
        it('should create an instance with the provided API key', () => {
            expect(instance).toBeInstanceOf(AzureLLM);
        it('should not initialize client until SetAdditionalSettings is called', () => {
            expect(() => (instance as ReturnType<typeof Object.create>)['Client']).toThrow(
                'Azure client not initialized'
    describe('SetAdditionalSettings', () => {
        it('should throw when endpoint is missing', () => {
            expect(() => instance.SetAdditionalSettings({})).toThrow(
                'Azure AI requires an endpoint URL'
        it('should initialize client with AzureKeyCredential when endpoint is provided', () => {
            instance.SetAdditionalSettings({ endpoint: 'https://my-endpoint.azure.com' });
            expect(mockModelClientFn).toHaveBeenCalledWith(
                'https://my-endpoint.azure.com',
                expect.objectContaining({ key: testApiKey })
        it('should initialize client with DefaultAzureCredential when useAzureAD is true', () => {
            instance.SetAdditionalSettings({
                endpoint: 'https://my-endpoint.azure.com',
                useAzureAD: true,
            expect(mockDefaultAzureCredential).toHaveBeenCalled();
                expect.objectContaining({ type: 'default' })
    describe('ClearAdditionalSettings', () => {
        it('should reset the client to null', () => {
            instance.ClearAdditionalSettings();
    describe('Endpoint getter', () => {
        it('should return the endpoint when set', () => {
            expect(instance.Endpoint).toBe('https://my-endpoint.azure.com');
        it('should throw when endpoint is not set', () => {
            expect(() => instance.Endpoint).toThrow('Azure endpoint URL not set');
    describe('nonStreamingChatCompletion', () => {
        it('should throw when client is not initialized', async () => {
                model: 'gpt-4',
                messages: [{ role: 'user' as const, content: 'Hello' }],
                maxOutputTokens: 100,
                (instance as ReturnType<typeof Object.create>)['nonStreamingChatCompletion'](params)
            ).rejects.toThrow('Azure client not initialized');
        it('should make a successful request and return mapped response', async () => {
            const mockPost = vi.fn().mockResolvedValue({
                body: {
                            message: { role: 'assistant', content: 'Hello there!' },
                    usage: { prompt_tokens: 10, completion_tokens: 5, total_tokens: 15 },
                path: vi.fn().mockReturnValue({ post: mockPost }),
            const result = await (instance as ReturnType<typeof Object.create>)['nonStreamingChatCompletion'](params);
            expect(result.data.choices).toHaveLength(1);
            expect(result.data.choices[0].message.content).toBe('Hello there!');
            expect(result.data.choices[0].message.role).toBe('assistant');
        it('should map response format JSON correctly', async () => {
                        { index: 0, message: { role: 'assistant', content: '{}' }, finish_reason: 'stop' },
                    usage: { prompt_tokens: 5, completion_tokens: 2, total_tokens: 7 },
                messages: [{ role: 'user' as const, content: 'Give JSON' }],
                temperature: 0.5,
                maxOutputTokens: 50,
                responseFormat: 'JSON' as const,
            const requestBody = mockPost.mock.calls[0][0].body;
            expect(requestBody.response_format).toEqual({ type: 'json_object' });
        it('should apply optional parameters (topP, frequencyPenalty, presencePenalty, seed, stopSequences)', async () => {
                        { index: 0, message: { role: 'assistant', content: 'ok' }, finish_reason: 'stop' },
                    usage: { prompt_tokens: 5, completion_tokens: 1, total_tokens: 6 },
                messages: [{ role: 'user' as const, content: 'Test' }],
                topP: 0.8,
                frequencyPenalty: 0.3,
                presencePenalty: 0.5,
                seed: 42,
                stopSequences: ['STOP'],
            await (instance as ReturnType<typeof Object.create>)['nonStreamingChatCompletion'](params);
            expect(requestBody.top_p).toBe(0.8);
            expect(requestBody.frequency_penalty).toBe(0.3);
            expect(requestBody.presence_penalty).toBe(0.5);
            expect(requestBody.seed).toBe(42);
            expect(requestBody.stop).toEqual(['STOP']);
        it('should default top_p to 0.95 when not provided', async () => {
            expect(requestBody.top_p).toBe(0.95);
        it('should return error result on API failure', async () => {
            const mockPost = vi.fn().mockRejectedValue(new Error('API unavailable'));
            expect(result.errorMessage).toBe('API unavailable');
        it('should extract content from a valid chunk', () => {
                        delta: { content: 'Hello' },
                        finish_reason: null,
            const result = (instance as ReturnType<typeof Object.create>)['processStreamingChunk'](chunk);
            expect(result.content).toBe('Hello');
            expect(result.finishReason).toBeUndefined();
        it('should return empty content when no delta content', () => {
                        delta: {},
        it('should extract usage from chunk when available', () => {
                choices: [{ delta: { content: '' }, finish_reason: 'stop', index: 0 }],
                usage: { prompt_tokens: 10, completion_tokens: 5 },
            expect(result.usage).toBeDefined();
    describe('finalizeStreamingResponse', () => {
        it('should create a successful ChatResult from accumulated content', () => {
            const result = (instance as ReturnType<typeof Object.create>)['finalizeStreamingResponse'](
                'Hello there!',
                { choices: [{ finish_reason: 'stop' }] },
            expect(result.data.choices[0].finish_reason).toBe('stop');
        it('should handle empty accumulated content', () => {
            expect(result.data.choices[0].message.content).toBe('');
    describe('getStringFromChatMessageContent', () => {
        it('should return string content as-is', () => {
            const result = (instance as ReturnType<typeof Object.create>)['getStringFromChatMessageContent']('Hello world');
        it('should join text blocks from array content', () => {
                { type: 'text', content: 'First part' },
                { type: 'text', content: 'Second part' },
                { type: 'image_url', content: 'data:image/png;base64,...' },
            const result = (instance as ReturnType<typeof Object.create>)['getStringFromChatMessageContent'](blocks);
            expect(result).toBe('First part\n\nSecond part');
        it('should return empty string for non-string non-array content', () => {
            const result = (instance as ReturnType<typeof Object.create>)['getStringFromChatMessageContent'](42);
