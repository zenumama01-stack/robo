const MockCerebras = vi.hoisted(() => vi.fn().mockImplementation(function (this: Record<string, unknown>) {
    this.chat = {
        completions: {
// Mock the Cerebras SDK
vi.mock('@cerebras/cerebras_cloud_sdk', () => ({
    Cerebras: MockCerebras
        protected thinkingStreamState: {
        } | null = null;
        protected extractThinkingFromContent(content: string): { content: string; thinking: string | undefined } {
            if (content.startsWith('<think>') && content.includes('</think>')) {
                const thinkStart = content.indexOf('<think>') + '<think>'.length;
                const thinkEnd = content.indexOf('</think>');
                    thinking: content.substring(thinkStart, thinkEnd).trim(),
                    content: content.substring(thinkEnd + '</think>'.length).trim()
            return { content, thinking: undefined };
            if (!this.thinkingStreamState) return rawContent;
            message: { role: string; content: string },
            thinking: string | undefined
        ): { role: string; content: string; thinking?: string } {
            if (thinking) {
                return { ...message, thinking };
        model: string = '';
        ChatResultChoice: {} as unknown,
        ClassifyResult: class {}
import { CerebrasLLM } from '../models/cerebras';
describe('CerebrasLLM', () => {
    let instance: CerebrasLLM;
        instance = new CerebrasLLM('test-api-key');
            expect(instance).toBeInstanceOf(CerebrasLLM);
            expect(MockCerebras).toHaveBeenCalledWith({ apiKey: 'test-api-key' });
        it('should expose the Cerebras client via getters', () => {
            expect(instance.CerebrasClient).toBeDefined();
            expect(instance.client).toBeDefined();
            expect(instance.client).toBe(instance.CerebrasClient);
    describe('supportsThinkingModels', () => {
            const result = (instance as ReturnType<typeof Object.create>)['supportsThinkingModels']();
    describe('setCerebrasParamsEffortLevel', () => {
        const callMethod = (cerebrasParams: Record<string, unknown>, params: { effortLevel?: string; model: string }): void => {
            (instance as ReturnType<typeof Object.create>)['setCerebrasParamsEffortLevel'](cerebrasParams, params);
        describe('GPT-OSS models', () => {
            it('should map numeric 0 to "low"', () => {
                const params: Record<string, unknown> = {};
                callMethod(params, { effortLevel: '0', model: 'gpt-oss-v1' });
                expect(params.reasoning_effort).toBe('low');
            it('should map numeric 33 to "low"', () => {
                callMethod(params, { effortLevel: '33', model: 'gpt-oss-large' });
            it('should map numeric 34 to "medium"', () => {
                callMethod(params, { effortLevel: '34', model: 'gpt-oss-large' });
                expect(params.reasoning_effort).toBe('medium');
            it('should map numeric 66 to "medium"', () => {
                callMethod(params, { effortLevel: '66', model: 'gpt-oss-large' });
            it('should map numeric 67 to "high"', () => {
                callMethod(params, { effortLevel: '67', model: 'gpt-oss-large' });
                expect(params.reasoning_effort).toBe('high');
            it('should map numeric 100 to "high"', () => {
                callMethod(params, { effortLevel: '100', model: 'gpt-oss-large' });
            it('should pass through string effort levels for GPT-OSS', () => {
                callMethod(params, { effortLevel: 'high', model: 'gpt-oss-model' });
        describe('Non-GPT-OSS models', () => {
            it('should not set reasoning_effort for llama models', () => {
                callMethod(params, { effortLevel: '50', model: 'llama-3.3-70b' });
                expect(params.reasoning_effort).toBeUndefined();
            it('should not set reasoning_effort for other models', () => {
                callMethod(params, { effortLevel: 'high', model: 'deepseek-r1' });
        describe('No effort level', () => {
            it('should not set reasoning_effort when effortLevel is undefined', () => {
                callMethod(params, { effortLevel: undefined, model: 'gpt-oss-large' });
            it('should not set reasoning_effort when effortLevel is empty string', () => {
                callMethod(params, { effortLevel: '', model: 'gpt-oss-large' });
    describe('Thinking extraction logic', () => {
        // This tests the extractThinkingFromContent base class method
        // used by Cerebras in nonStreamingChatCompletion
        it('should extract thinking from <think> tags', () => {
            const extractMethod = (instance as ReturnType<typeof Object.create>)['extractThinkingFromContent'].bind(instance);
            const result = extractMethod('<think>Let me analyze this</think>The actual response');
            expect(result.thinking).toBe('Let me analyze this');
            expect(result.content).toBe('The actual response');
        it('should return content unchanged when no thinking tags', () => {
            const result = extractMethod('Just a regular response');
            expect(result.content).toBe('Just a regular response');
        it('should handle empty content between think tags', () => {
            const result = extractMethod('<think></think>Response here');
            expect(result.thinking).toBe('');
            expect(result.content).toBe('Response here');
        it('should handle multiline thinking content', () => {
            const result = extractMethod('<think>Line 1\nLine 2\nLine 3</think>Final answer');
            expect(result.thinking).toBe('Line 1\nLine 2\nLine 3');
            expect(result.content).toBe('Final answer');
            // Initialize thinking stream state since supportsThinkingModels returns true
            (instance as ReturnType<typeof Object.create>)['initializeThinkingStreamState']();
        it('should extract content from streaming chunk', () => {
                    delta: { content: 'Hello from Cerebras' },
                    finish_reason: null
            expect(result.content).toBe('Hello from Cerebras');
        it('should detect finish reason', () => {
                    finish_reason: 'stop'
        it('should extract usage from final chunk', () => {
                    prompt_tokens: 15,
                    completion_tokens: 25
        it('should return empty content for null chunk', () => {
        it('should return empty content for chunk with no choices', () => {
            const result = callMethod({ choices: [] });
        const callMethod = (
            content: string | null | undefined,
            lastChunk: unknown,
            usage: unknown
        ): unknown => {
            return (instance as ReturnType<typeof Object.create>)['finalizeStreamingResponse'](content, lastChunk, usage);
        it('should create a ChatResult with accumulated content', () => {
            const result = callMethod('Final response text', null, null) as {
                data: { choices: Array<{ message: { content: string } }> };
            expect(result.data.choices[0].message.content).toBe('Final response text');
            expect(result.statusText).toBe('success');
        it('should handle null content gracefully', () => {
            const result = callMethod(null, null, null) as {
        it('should extract finish reason from lastChunk', () => {
            const lastChunk = {
                choices: [{ finish_reason: 'length' }]
            const result = callMethod('text', lastChunk, null) as {
                data: { choices: Array<{ finish_reason: string }> };
            expect(result.data.choices[0].finish_reason).toBe('length');
