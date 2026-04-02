import { BaseLLM, ChatMessage, ChatMessageRole, ChatParams, ChatResult, ClassifyParams, ClassifyResult, GetUserMessageFromChatParams, ModelUsage, SummarizeParams, SummarizeResult } from "@memberjunction/ai";
import { OpenAI } from "openai";
import { ChatCompletionAssistantMessageParam, ChatCompletionMessageParam, ChatCompletionSystemMessageParam, ChatCompletionUserMessageParam } from "openai/resources";
 * Fireworks.ai implementation of the BaseLLM class
 * Fireworks.ai provides an OpenAI-compatible API, so we use the OpenAI SDK with a custom baseURL
@RegisterClass(BaseLLM, 'FireworksLLM')
export class FireworksLLM extends BaseLLM {
    private _client: OpenAI;
    private static readonly DEFAULT_BASE_URL = 'https://api.fireworks.ai/inference/v1';
    constructor(apiKey: string, baseURL?: string) {
        // Create the OpenAI-compatible client instance with Fireworks.ai endpoint
        this._client = new OpenAI({
            baseURL: baseURL || FireworksLLM.DEFAULT_BASE_URL
     * Read-only getter method to get the client instance
    public get Client(): OpenAI {
     * Fireworks.ai supports streaming
     * Implementation of non-streaming chat completion for Fireworks.ai
        const formattedMessages = this.ConvertMJToOpenAIChatMessages(params.messages);
        const fireworksParams: OpenAI.Chat.Completions.ChatCompletionCreateParamsNonStreaming = {
            messages: formattedMessages,
            max_completion_tokens: params.maxOutputTokens,
        // Add sampling and generation parameters
            fireworksParams.top_p = params.topP;
            fireworksParams.frequency_penalty = params.frequencyPenalty;
            fireworksParams.presence_penalty = params.presencePenalty;
            fireworksParams.seed = params.seed;
            fireworksParams.stop = params.stopSequences;
        // Fireworks.ai supports topK via their extended API
            // Add topK as additional setting if supported
            (fireworksParams as unknown as Record<string, unknown>).top_k = params.topK;
        // Handle response format
                fireworksParams.response_format = { type: "json_object" };
                fireworksParams.response_format = params.modelSpecificResponseFormat;
        const result = await this.Client.chat.completions.create(fireworksParams);
        const timeElapsed = endTime.getTime() - startTime.getTime();
        // Create ModelUsage with token information
        const usage = new ModelUsage(result.usage.prompt_tokens, result.usage.completion_tokens);
                choices: result.choices.map((c: unknown) => {
                    const choice = c as {
                        message: { content: string; role: string };
                            content: choice.message.content,
                        index: choice.index,
            success: !!result,
            timeElapsed: timeElapsed,
            exception: null
        } as ChatResult;
            provider: 'fireworks',
            systemFingerprint: result.system_fingerprint,
            created: result.created,
            object: result.object,
     * Create a streaming request for Fireworks.ai
    protected async createStreamingRequest(params: ChatParams): Promise<AsyncIterable<OpenAI.Chat.Completions.ChatCompletionChunk>> {
        const fireworksParams: OpenAI.Chat.Completions.ChatCompletionCreateParamsStreaming = {
            stream: true,
        // Fireworks.ai supports topK
        return this.Client.chat.completions.create(fireworksParams);
     * Process a streaming chunk from Fireworks.ai
    protected processStreamingChunk(chunk: OpenAI.Chat.Completions.ChatCompletionChunk): {
        usage?: {
        const content = chunk?.choices?.[0]?.delta?.content || '';
        const usage = chunk?.usage;
            finishReason: chunk?.choices?.[0]?.finish_reason,
            usage: usage ? {
                promptTokens: usage.prompt_tokens || 0,
                completionTokens: usage.completion_tokens || 0,
                totalTokens: (usage.prompt_tokens || 0) + (usage.completion_tokens || 0)
     * Create the final response from streaming results for Fireworks.ai
        lastChunk: OpenAI.Chat.Completions.ChatCompletionChunk | null | undefined,
        usage: { promptTokens: number; completionTokens: number; totalTokens: number } | null | undefined
        const messages = this.ConvertMJToOpenAIChatMessages(params.messages);
        const result = await this.Client.chat.completions.create({
            messages: messages
        const success = result && result.choices && result.choices.length > 0;
            summaryText = result.choices[0].message.content;
    public ConvertMJToOpenAIChatMessages(messages: ChatMessage[]): ChatCompletionMessageParam[] {
        return messages.map(m => {
            const role = this.ConvertMJToOpenAIRole(m.role);
            let content: unknown = m.content;
            // Process content if it's an array
            if (Array.isArray(content)) {
                // Filter out unsupported types and convert to OpenAI's expected format
                const contentParts = content
                    .map(c => {
                        // For text type
                        if (c.type === 'text') {
                                type: 'text' as const,
                                text: c.content
                        // For image_url type
                        else if (c.type === 'image_url') {
                                type: 'image_url' as const,
                                image_url: { url: c.content }
                        // Warn about unsupported types
                            console.warn(`Unsupported content type for Fireworks.ai API: ${c.type}. This content will be skipped.`);
                    .filter(part => part !== null);
                content = contentParts;
            // Create the appropriate message type based on role
                        role: 'system' as const,
                        content
                    } as ChatCompletionSystemMessageParam;
                    } as ChatCompletionUserMessageParam;
                        role: 'assistant' as const,
                    } as ChatCompletionAssistantMessageParam;
                    throw new Error(`Unknown role ${m.role}`);
    public ConvertMJToOpenAIRole(role: string): 'system' | 'user' | 'assistant' {
        switch (role.trim().toLowerCase()) {
                throw new Error(`Unknown role ${role}`)
