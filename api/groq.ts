import { BaseLLM, ChatParams, ChatResult, ChatResultChoice, ChatMessageRole, ClassifyParams, ClassifyResult, SummarizeParams, SummarizeResult, ModelUsage, ErrorAnalyzer, ChatMessage, ChatMessageContentBlock } from '@memberjunction/ai';
import Groq from 'groq-sdk';
import { ChatCompletionCreateParamsNonStreaming, ChatCompletionCreateParamsStreaming, ChatCompletionMessageParam, ChatCompletionContentPart } from 'groq-sdk/resources/chat/completions';
 * Groq implementation of the BaseLLM class
@RegisterClass(BaseLLM, "GroqLLM")
export class GroqLLM extends BaseLLM {
    private _client: Groq;
        this._client = new Groq({ apiKey: apiKey });
     * Read only getter method to get the Groq client instance
    public get GroqClient(): Groq {
    public get client(): Groq {
        return this.GroqClient;
     * Groq supports streaming
     * Groq supports thinking models with <think> blocks
    protected setGroqParamsEffortLevel(groqParams: any, params: ChatParams): void {
            const isQwenModel = params.model.toLowerCase().includes("qwen");
                // map our efforts as follows:
            else if (isQwenModel) {
                // either default or none, map any non numeric value other than default as well as the number 0, to "none" and map anything else to "default"
                if (convertedEffortLevel.trim().toLowerCase() !== "default") {
                    convertedEffortLevel = numericEffortLevel ? "default" : "none";
            if (isGptOSSModel || isQwenModel){
                // right now, Groq only supports reasoning_effort with Qwen and GPT OSS models
                groqParams.reasoning_effort = convertedEffortLevel;
     * Convert MJ messages to Groq-compatible format with proper multimodal support
     * Groq uses OpenAI-compatible format: { type: "image_url", image_url: { url: "..." } }
    private convertToGroqMessages(messages: ChatMessage[]): ChatCompletionMessageParam[] {
        const groqMessages: ChatCompletionMessageParam[] = [];
        for (const msg of messages) {
            // Simple string content
                groqMessages.push({
                    role: msg.role as 'system' | 'user' | 'assistant',
            // Array of content blocks - convert to Groq format
            const groqContent: ChatCompletionContentPart[] = [];
                    groqContent.push({
                        type: 'image_url',
                        image_url: {
                            url: block.content
                // Note: audio_url, video_url, file_url not yet supported by Groq
            // If we have converted content blocks, use them; otherwise fall back to empty text
                content: groqContent.length > 0 ? groqContent : ''
            } as ChatCompletionMessageParam);
        return groqMessages;
     * Implementation of non-streaming chat completion for Groq
        // Convert to Groq-compatible message format with proper multimodal support
        const messages = this.convertToGroqMessages(params.messages);
        // Groq requires the last message to be a user message
        if (messages.length > 0 && messages[messages.length - 1].role !== 'user') {
                content: 'OK' // Dummy message to satisfy Groq's requirement
        const groqParams: ChatCompletionCreateParamsNonStreaming = {
        this.setGroqParamsEffortLevel(groqParams, params);
            groqParams.top_p = params.topP;
            groqParams.seed = params.seed;
            groqParams.stop = params.stopSequences;
            groqParams.frequency_penalty = params.frequencyPenalty;
            groqParams.presence_penalty = params.presencePenalty;
        // Groq doesn't support topK - warn if provided
            console.warn('Groq provider does not support topK parameter, ignoring');
                groqParams.response_format = { type: "json_object" };
                groqParams.response_format = params.modelSpecificResponseFormat;
        const chatResponse = await this.client.chat.completions.create(groqParams);
        let choices: ChatResultChoice[] = chatResponse.choices.map((choice: any) => {
            // in some cases, Groq models do thinking and return that as the first part 
                    thinking: extracted.thinking || choice.message.reasoning               
        // Create ModelUsage with timing data if available
        const usage = new ModelUsage(chatResponse.usage.prompt_tokens, chatResponse.usage.completion_tokens);
        // Groq provides detailed timing in the usage object
        const groqUsage = chatResponse.usage;
        if (groqUsage.queue_time !== undefined) {
            // Convert from seconds to milliseconds
            usage.queueTime = groqUsage.queue_time * 1000;
        if (groqUsage.prompt_time !== undefined) {
            usage.promptTime = groqUsage.prompt_time * 1000;
        if (groqUsage.completion_time !== undefined) {
            usage.completionTime = groqUsage.completion_time * 1000;
        result.modelSpecificResponseDetails = {
            provider: 'groq',
            model: chatResponse.model,
            systemFingerprint: (chatResponse as any).system_fingerprint
     * Create a streaming request for Groq
        const groqParams: ChatCompletionCreateParamsStreaming = {
        return this.client.chat.completions.create(groqParams);
     * Process a streaming chunk from Groq
        // Groq doesn't provide usage in streaming chunks
     * Create the final response from streaming results for Groq
        // For Groq, we don't have precise usage metrics from streaming
        // We'll use the accumulated ones or defaults
