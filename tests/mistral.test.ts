const mockComplete = vi.hoisted(() => vi.fn());
const MockMistral = vi.hoisted(() => vi.fn().mockImplementation(function (this: Record<string, unknown>) {
        complete: mockComplete,
        stream: mockStream
// Mock the Mistral SDK
vi.mock('@mistralai/mistralai', () => ({
    Mistral: MockMistral
import { MistralLLM } from '../models/mistral';
describe('MistralLLM', () => {
    let instance: MistralLLM;
        instance = new MistralLLM('test-api-key');
            expect(instance).toBeInstanceOf(MistralLLM);
            expect(MockMistral).toHaveBeenCalledWith({ apiKey: 'test-api-key' });
        it('should expose the Mistral client via getter', () => {
            expect(instance.Client).toBeDefined();
            expect(instance.Client.chat).toBeDefined();
    describe('resetStreamingState', () => {
        it('should reset all streaming state fields', () => {
            // Modify state first
            state.accumulatedThinking = 'deep thoughts';
            state.pendingContent = 'pending stuff';
    describe('Thinking extraction from <think> tags', () => {
        // Test the inline thinking extraction logic used in nonStreamingChatCompletion
        it('should extract thinking content from <think> tags', () => {
            const rawContent = '<think>Let me reason about this carefully</think>Here is the actual answer';
            let content = rawContent.trim();
            expect(thinkingContent).toBe('Let me reason about this carefully');
            expect(content).toBe('Here is the actual answer');
            const rawContent = 'Just a plain response with no thinking';
            expect(content).toBe('Just a plain response with no thinking');
        it('should handle empty thinking tags', () => {
            const rawContent = '<think></think>The response';
            expect(thinkingContent).toBe('');
            expect(content).toBe('The response');
    describe('processThinkingInStreamingContent', () => {
        const callMethod = (): string => {
            return (instance as ReturnType<typeof Object.create>)['processThinkingInStreamingContent']();
        const resetState = () => (instance as ReturnType<typeof Object.create>)['resetStreamingState']();
        it('should pass through content when thinking is already complete', () => {
            resetState();
            state.pendingContent = 'Hello world';
            const result = callMethod();
            expect(getState().pendingContent).toBe('');
        it('should detect and enter thinking block', () => {
            state.pendingContent = '<think>starting to think';
            expect(getState().inThinkingBlock).toBe(true);
            expect(getState().accumulatedThinking).toBe('starting to think');
        it('should accumulate thinking and extract content after close tag', () => {
            state.pendingContent = 'more thinking</think>actual output';
            expect(result).toBe('actual output');
            expect(getState().accumulatedThinking).toBe('more thinking');
            expect(getState().thinkingComplete).toBe(true);
            expect(getState().inThinkingBlock).toBe(false);
        it('should continue accumulating when still in thinking block with no close tag', () => {
            state.pendingContent = 'still thinking about it';
            expect(getState().accumulatedThinking).toBe('still thinking about it');
        it('should output regular content when no thinking tags found', () => {
            state.pendingContent = 'regular content here';
            expect(result).toBe('regular content here');
        it('should hold back content that might be a partial tag', () => {
            state.pendingContent = 'some content<';
            expect(result).toBe('some content');
            expect(getState().pendingContent).toBe('<');
        it('should hold back partial <thin tag', () => {
            state.pendingContent = 'hello<thin';
            expect(result).toBe('hello');
            expect(getState().pendingContent).toBe('<thin');
    describe('MapMJMessagesToMistral', () => {
            return (instance as ReturnType<typeof Object.create>)['MapMJMessagesToMistral'](messages);
        it('should append a user message if last message is not a user message', () => {
                { role: 'user', content: 'Hello' },
                { role: 'assistant', content: 'Hi there!' }
            const result = callMethod(messages) as Array<{ role: string; content: string }>;
            expect(result[2]).toEqual({ role: 'user', content: 'ok' });
        it('should not append a user message if last message is a user message', () => {
                { role: 'user', content: 'What is 1+1?' }
            const content = result[0].content as Array<{ type: string; content: string }>;
        it('should handle empty message array', () => {
            const result = callMethod([]);
            // Reset streaming state and set thinkingComplete
                        finishReason: null
                        finishReason: 'stop'
        it('should extract usage information when present', () => {
                        delta: { content: 'hi' }
                        promptTokens: 10,
                        completionTokens: 5
