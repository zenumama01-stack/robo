const MockOpenAI = vi.hoisted(() => vi.fn().mockImplementation(function (this: Record<string, unknown>) {
// Mock the openai SDK
vi.mock('openai', () => ({
    OpenAI: MockOpenAI
// Mock @memberjunction/ai - provide the classes and constants the provider imports
        parseBase64DataUrl: vi.fn()
import { OpenAILLM } from '../models/openAI';
describe('OpenAILLM', () => {
    let instance: OpenAILLM;
        instance = new OpenAILLM('test-api-key');
            expect(instance).toBeInstanceOf(OpenAILLM);
            expect(MockOpenAI).toHaveBeenCalledWith({ apiKey: 'test-api-key' });
        it('should accept an optional baseURL', () => {
            const customInstance = new OpenAILLM('test-key', 'https://custom.openai.com/v1');
            expect(customInstance).toBeInstanceOf(OpenAILLM);
            expect(MockOpenAI).toHaveBeenCalledWith({
                apiKey: 'test-key',
                baseURL: 'https://custom.openai.com/v1'
        it('should not pass baseURL when it is an empty string', () => {
            new OpenAILLM('test-key', '');
            expect(MockOpenAI).toHaveBeenCalledWith({ apiKey: 'test-key' });
        it('should expose the OpenAI client via getter', () => {
            expect(instance.OpenAI).toBeDefined();
            expect(instance.OpenAI.chat).toBeDefined();
    describe('supportsReasoningViaSystemPrompt', () => {
        const callMethod = (modelName: string): boolean => {
            return (instance as ReturnType<typeof Object.create>)['supportsReasoningViaSystemPrompt'](modelName);
        it('should return true for gpt-oss models', () => {
            expect(callMethod('gpt-oss-v1')).toBe(true);
        it('should return true for gptoss models (no hyphen)', () => {
            expect(callMethod('gptoss-large')).toBe(true);
        it('should return true for GPT-OSS models (case insensitive)', () => {
            expect(callMethod('GPT-OSS-2025')).toBe(true);
        it('should return false for non-gpt-oss models', () => {
            expect(callMethod('gpt-4')).toBe(false);
        it('should return false for claude models', () => {
            expect(callMethod('claude-3-sonnet')).toBe(false);
    describe('getReasoningLevel', () => {
        const callMethod = (effortLevel: string): 'low' | 'medium' | 'high' => {
            return (instance as ReturnType<typeof Object.create>)['getReasoningLevel'](effortLevel);
        it('should pass through string "low"', () => {
            expect(callMethod('low')).toBe('low');
        it('should pass through string "medium"', () => {
            expect(callMethod('medium')).toBe('medium');
        it('should pass through string "high"', () => {
            expect(callMethod('high')).toBe('high');
        it('should handle case-insensitive string values', () => {
            expect(callMethod('LOW')).toBe('low');
            expect(callMethod('Medium')).toBe('medium');
            expect(callMethod('HIGH')).toBe('high');
        it('should map numeric value 0 to "low"', () => {
            expect(callMethod('0')).toBe('low');
        it('should map numeric value 33 to "low"', () => {
            expect(callMethod('33')).toBe('low');
        it('should map numeric value 34 to "medium"', () => {
            expect(callMethod('34')).toBe('medium');
        it('should map numeric value 66 to "medium"', () => {
            expect(callMethod('66')).toBe('medium');
        it('should map numeric value 67 to "high"', () => {
            expect(callMethod('67')).toBe('high');
        it('should map numeric value 100 to "high"', () => {
            expect(callMethod('100')).toBe('high');
        it('should throw for invalid string values', () => {
            expect(() => callMethod('extreme')).toThrow('Invalid effortLevel: extreme');
    describe('ConvertMJToOpenAIRole', () => {
        it('should map "system" to "system"', () => {
            expect(instance.ConvertMJToOpenAIRole('system')).toBe('system');
            expect(instance.ConvertMJToOpenAIRole('user')).toBe('user');
            expect(instance.ConvertMJToOpenAIRole('assistant')).toBe('assistant');
        it('should handle roles with whitespace', () => {
            expect(instance.ConvertMJToOpenAIRole('  system  ')).toBe('system');
        it('should handle roles case-insensitively', () => {
            expect(instance.ConvertMJToOpenAIRole('SYSTEM')).toBe('system');
            expect(instance.ConvertMJToOpenAIRole('User')).toBe('user');
            expect(instance.ConvertMJToOpenAIRole('Assistant')).toBe('assistant');
        it('should throw for unknown roles', () => {
            expect(() => instance.ConvertMJToOpenAIRole('unknown')).toThrow('Unknown role unknown');
    describe('ConvertMJToOpenAIChatMessages', () => {
        it('should convert a simple string message array', () => {
            const result = instance.ConvertMJToOpenAIChatMessages(messages);
            expect(result[2]).toEqual({ role: 'assistant', content: 'Hi there!' });
        it('should convert multimodal content with text blocks', () => {
                        { type: 'text' as const, content: 'Describe this image' }
            expect(Array.isArray(result[0].content)).toBe(true);
            const contentArray = result[0].content as Array<{ type: string; text?: string }>;
            expect(contentArray[0]).toEqual({ type: 'text', text: 'Describe this image' });
        it('should convert multimodal content with image_url blocks', () => {
                        { type: 'text' as const, content: 'What is this?' },
                        { type: 'image_url' as const, content: 'https://example.com/img.png' }
            const contentArray = result[0].content as Array<{ type: string; text?: string; image_url?: { url: string } }>;
            expect(contentArray).toHaveLength(2);
            expect(contentArray[0]).toEqual({ type: 'text', text: 'What is this?' });
            expect(contentArray[1]).toEqual({ type: 'image_url', image_url: { url: 'https://example.com/img.png' } });
        it('should filter out unsupported content types', () => {
                        { type: 'video_url' as const, content: 'https://example.com/vid.mp4' }
            const contentArray = result[0].content as Array<{ type: string }>;
            expect(contentArray).toHaveLength(1);
            expect(contentArray[0].type).toBe('text');
        it('should throw for unknown message roles', () => {
                { role: 'unknown_role' as 'system', content: 'test' }
            expect(() => instance.ConvertMJToOpenAIChatMessages(messages)).toThrow();
            // Access private method
            state.pendingContent = 'some content';
