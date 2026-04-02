import { BaseLLM, ChatParams, ChatResult, ChatResultChoice, ChatMessageRole, ClassifyParams, ClassifyResult, SummarizeParams, SummarizeResult, ModelUsage, ChatMessage, ChatMessageContentBlock, parseBase64DataUrl } from '@memberjunction/ai';
import { Ollama, ChatRequest, ChatResponse, GenerateRequest, GenerateResponse, Message } from 'ollama';
 * Ollama implementation of the BaseLLM class for local LLM inference
 * Supports chat, generation, and streaming with various open-source models
@RegisterClass(BaseLLM, "OllamaLLM")
export class OllamaLLM extends BaseLLM {
     * Ollama supports streaming
     * Ollama can support thinking models depending on the loaded model
     * Convert MJ messages to Ollama format with proper image handling
     * Ollama expects images in a separate 'images' array as base64 strings
    private convertToOllamaMessages(messages: ChatMessage[]): Message[] {
            const role = msg.role as 'system' | 'user' | 'assistant';
                return { role, content: msg.content };
            // Array of content blocks - extract text and images separately
            const textParts: string[] = [];
            const images: string[] = [];
                    textParts.push(block.content);
                    // Extract base64 image data for Ollama
                    const imageData = this.extractBase64ForOllama(block);
                    if (imageData) {
                        images.push(imageData);
                // Note: audio_url, video_url, file_url not yet supported by Ollama
            const result: Message = {
                content: textParts.join('\n')
            // Add images array if we have any
            if (images.length > 0) {
     * Extract base64 image data for Ollama API
     * Ollama expects raw base64 strings (not data URLs)
    private extractBase64ForOllama(block: ChatMessageContentBlock): string | null {
            return parsed.data; // Return just the base64 data, not the full data URL
        // If it doesn't start with http, assume it's already base64
        if (!content.startsWith('http://') && !content.startsWith('https://')) {
        // Ollama doesn't support image URLs - only base64
        console.warn('Ollama does not support image URLs, only base64. Skipping image.');
     * Implementation of non-streaming chat completion for Ollama
            // Convert MJ messages to Ollama format with proper image handling
            const messages = this.convertToOllamaMessages(params.messages);
            // Create chat request parameters
            const chatRequest: ChatRequest & { stream?: false } = {
                stream: false,
            // Add optional parameters
                chatRequest.options = {
                    ...chatRequest.options,
                    num_predict: params.maxOutputTokens
                    top_p: params.topP
                    top_k: params.topK
                    seed: params.seed
                    stop: params.stopSequences
                    frequency_penalty: params.frequencyPenalty
                    presence_penalty: params.presencePenalty
                    // Ollama supports JSON mode through format parameter
                    chatRequest.format = 'json';
                    if (params.modelSpecificResponseFormat) {
                        chatRequest.format = params.modelSpecificResponseFormat;
            // Make the chat completion request
            const response = await this.client.chat(chatRequest) as ChatResponse;
            // Process thinking content if present (for models that support it)
            let content = response.message.content;
            let thinking: string | undefined = undefined;
            if (this.supportsThinkingModels() && content) {
                const extracted = this.extractThinkingFromContent(content);
                content = extracted.content;
                thinking = extracted.thinking;
                    thinking: thinking
                finish_reason: response.done ? 'stop' : 'length',
            // Create ModelUsage from Ollama response
            const usage = new ModelUsage(
                response.prompt_eval_count || 0,
                response.eval_count || 0
                provider: 'ollama',
                total_duration: response.total_duration,
                load_duration: response.load_duration,
                prompt_eval_duration: response.prompt_eval_duration,
                eval_duration: response.eval_duration
     * Create a streaming request for Ollama
        // Create streaming chat request parameters
        const chatRequest: ChatRequest = {
        // Return the streaming response
        // Cast stream to true for TypeScript overload resolution
        return this.client.chat({ ...chatRequest, stream: true } as ChatRequest & { stream: true });
     * Process a streaming chunk from Ollama
        // Ollama streaming chunks have a specific format
        if (chunk && typeof chunk === 'object') {
            if (chunk.message && chunk.message.content) {
                const rawContent = chunk.message.content;
            // Check if this is the final chunk
            if (chunk.done === true) {
                // Extract usage information from final chunk
                if (chunk.prompt_eval_count || chunk.eval_count) {
                    usage = {
                        promptTokens: chunk.prompt_eval_count || 0,
                        completionTokens: chunk.eval_count || 0
     * Create the final response from streaming results for Ollama
        if (lastChunk?.done === false) {
            finishReason = 'length';
        // Extract usage metrics from accumulated usage or last chunk
            promptTokens = usage.promptTokens || 0;
            completionTokens = usage.completionTokens || 0;
        } else if (lastChunk) {
            promptTokens = lastChunk.prompt_eval_count || 0;
            completionTokens = lastChunk.eval_count || 0;
        // Add Ollama-specific details if available
        if (lastChunk) {
                model: lastChunk.model,
                total_duration: lastChunk.total_duration,
                load_duration: lastChunk.load_duration,
                prompt_eval_duration: lastChunk.prompt_eval_duration,
                eval_duration: lastChunk.eval_duration
     * Generate endpoint implementation for Ollama (alternative to chat)
     * This can be useful for simple completion tasks
    public async generate(params: {
        maxOutputTokens?: number;
    }): Promise<any> {
        const generateRequest: GenerateRequest = {
            stream: params.stream || false,
        if (params.maxOutputTokens) {
            generateRequest.options = {
                ...generateRequest.options,
        // Handle TypeScript overload by explicitly typing based on stream value
        if (params.stream) {
            return await this.client.generate({ ...generateRequest, stream: true } as GenerateRequest & { stream: true });
            return await this.client.generate({ ...generateRequest, stream: false } as GenerateRequest & { stream: false });
     * List available models in Ollama
    public async listModels(): Promise<any> {
        return await this.client.list();
     * Pull a model from Ollama registry
    public async pullModel(modelName: string): Promise<void> {
     * Check if a model is available locally
    public async isModelAvailable(modelName: string): Promise<boolean> {
            const models = await this.listModels();
            return models.models.some((m: any) => m.name === modelName || m.name.startsWith(modelName + ':'));
        throw new Error("Method not implemented. Use Chat with a summarization prompt instead.");
        throw new Error("Method not implemented. Use Chat with a classification prompt instead.");
