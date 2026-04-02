const MockGroq = vi.hoisted(() => vi.fn().mockImplementation(function (this: Record<string, unknown>) {
// Mock the groq-sdk
vi.mock('groq-sdk', () => ({
    default: MockGroq
        queueTime?: number;
        promptTime?: number;
        completionTime?: number;
        ErrorAnalyzer: { analyzeError: vi.fn() }
import { GroqLLM } from '../models/groq';
describe('GroqLLM', () => {
    let instance: GroqLLM;
        instance = new GroqLLM('test-api-key');
            expect(instance).toBeInstanceOf(GroqLLM);
            expect(MockGroq).toHaveBeenCalledWith({ apiKey: 'test-api-key' });
        it('should expose the Groq client via getters', () => {
            expect(instance.GroqClient).toBeDefined();
            expect(instance.client).toBe(instance.GroqClient);
    describe('setGroqParamsEffortLevel', () => {
        const callMethod = (groqParams: Record<string, unknown>, params: { effortLevel?: string; model: string }): void => {
            (instance as ReturnType<typeof Object.create>)['setGroqParamsEffortLevel'](groqParams, params);
                const groqParams: Record<string, unknown> = {};
                callMethod(groqParams, { effortLevel: '0', model: 'gpt-oss-large' });
                expect(groqParams.reasoning_effort).toBe('low');
                callMethod(groqParams, { effortLevel: '33', model: 'gpt-oss-large' });
                callMethod(groqParams, { effortLevel: '34', model: 'gpt-oss-large' });
                expect(groqParams.reasoning_effort).toBe('medium');
                callMethod(groqParams, { effortLevel: '66', model: 'gpt-oss-large' });
                callMethod(groqParams, { effortLevel: '67', model: 'gpt-oss-large' });
                expect(groqParams.reasoning_effort).toBe('high');
                callMethod(groqParams, { effortLevel: '100', model: 'gpt-oss-large' });
                callMethod(groqParams, { effortLevel: 'medium', model: 'gpt-oss-model' });
        describe('Qwen models', () => {
            it('should map numeric 0 to "none"', () => {
                callMethod(groqParams, { effortLevel: '0', model: 'qwen-2.5-72b' });
                expect(groqParams.reasoning_effort).toBe('none');
            it('should map non-zero numeric to "default"', () => {
                callMethod(groqParams, { effortLevel: '50', model: 'qwen-model' });
                expect(groqParams.reasoning_effort).toBe('default');
            it('should keep "default" string value as "default"', () => {
                callMethod(groqParams, { effortLevel: 'default', model: 'qwen-large' });
            it('should map non-numeric, non-default string to "none"', () => {
                callMethod(groqParams, { effortLevel: 'low', model: 'qwen-7b' });
        describe('Other models', () => {
            it('should not set reasoning_effort for non-GPT-OSS and non-Qwen models', () => {
                callMethod(groqParams, { effortLevel: '50', model: 'llama-3.1-70b' });
                expect(groqParams.reasoning_effort).toBeUndefined();
            it('should not set reasoning_effort for mixtral models', () => {
                callMethod(groqParams, { effortLevel: 'high', model: 'mixtral-8x7b' });
                callMethod(groqParams, { effortLevel: undefined, model: 'gpt-oss-large' });
                callMethod(groqParams, { effortLevel: '', model: 'gpt-oss-large' });
    describe('convertToGroqMessages', () => {
        const callMethod = (messages: Array<{ role: string; content: unknown }>): unknown[] => {
            return (instance as ReturnType<typeof Object.create>)['convertToGroqMessages'](messages);
        it('should convert simple string messages', () => {
                { role: 'user', content: 'Hello' }
            expect(result[0]).toEqual({ role: 'system', content: 'You are helpful' });
            expect(result[1]).toEqual({ role: 'user', content: 'Hello' });
        it('should convert multimodal content blocks', () => {
                        { type: 'text', content: 'What is this?' },
                        { type: 'image_url', content: 'https://example.com/img.png' }
            const result = callMethod(messages) as Array<{ role: string; content: unknown }>;
            const content = result[0].content as Array<{ type: string }>;
            expect(content).toHaveLength(2);
            expect(content[0]).toEqual({ type: 'text', text: 'What is this?' });
            expect(content[1]).toEqual({ type: 'image_url', image_url: { url: 'https://example.com/img.png' } });
