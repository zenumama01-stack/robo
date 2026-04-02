import { BaseLLM, ChatParams, ChatResult, ChatResultChoice, ChatMessageRole, ClassifyParams, ClassifyResult, SummarizeParams, SummarizeResult, ModelUsage } from '@memberjunction/ai';
import { Cerebras } from '@cerebras/cerebras_cloud_sdk';
import { Chat, ChatCompletion } from '@cerebras/cerebras_cloud_sdk/resources/chat';
 * Cerebras implementation of the BaseLLM class
@RegisterClass(BaseLLM, "CerebrasLLM")
export class CerebrasLLM extends BaseLLM {
    private _client: Cerebras;
     * Creates a new instance of the CerebrasLLM class
     * @param apiKey The Cerebras API key to use for authentication
        this._client = new Cerebras({ apiKey });
     * Read only getter method to get the Cerebras client instance
    public get CerebrasClient(): Cerebras {
    public get client(): Cerebras {
        return this.CerebrasClient;
     * Cerebras supports streaming
     * Cerebras supports thinking models with <think> blocks
     * Set the reasoning_effort parameter for Cerebras models
     * Currently only supported for OpenAI GPT OSS models
    protected setCerebrasParamsEffortLevel(cerebrasParams: Chat.ChatCompletionCreateParams, params: ChatParams): void {
        let convertedEffortLevel = params.effortLevel;
        if (convertedEffortLevel) {
            const isGptOSSModel = params.model.toLowerCase().includes("gpt-oss");
            const numericEffortLevel = Number.isNaN(params.effortLevel) ? null : Number.parseInt(params.effortLevel);
            if (isGptOSSModel) {
                // Map our effort levels as follows:
                // 0-33 = "low"
                // 34-66 = "medium"
                // 67-100 = "high"
                if (numericEffortLevel !== null) {
                    if (numericEffortLevel >= 0 && numericEffortLevel <= 33) {
                        convertedEffortLevel = "low";
                    } else if (numericEffortLevel > 33 && numericEffortLevel <= 66) {
                        convertedEffortLevel = "medium";
                    } else if (numericEffortLevel > 66 && numericEffortLevel <= 100) {
                        convertedEffortLevel = "high";
                // Only set reasoning_effort for GPT OSS models
                cerebrasParams.reasoning_effort = convertedEffortLevel as "low" | "medium" | "high";
     * Implementation of non-streaming chat completion for Cerebras
        // Convert messages to format expected by Cerebras
        const messages = params.messages.map(m => {
            if (typeof m.content === 'string') {
                    content: m.content
                // Multimodal content not fully supported yet
                // Convert to string by joining text content
                const contentStr = m.content
                    content: contentStr
        const cerebrasParams: Chat.ChatCompletionCreateParams = {
            temperature: params.temperature
        // Add reasoning_effort if supported by the model
        this.setCerebrasParamsEffortLevel(cerebrasParams, params);
        // Handle response format if specified
        switch (params.responseFormat) {
            case 'Any':
            case 'Text':
            case 'Markdown':
            case 'JSON':
                cerebrasParams.response_format = { type: "json_object" };
                cerebrasParams.response_format = params.modelSpecificResponseFormat;
        const chatResponse = await this.client.chat.completions.create(cerebrasParams);
        // Cast to any to extract the choices
        const choices: ChatResultChoice[] = (chatResponse.choices as Array<ChatCompletion.ChatCompletionResponse.Choice>).map((choice: any) => {
            const rawMessage = choice.message.content;
            // in some cases, Cerebras models do thinking and return that as the first part 
            // of the message the very first characters will be <think> and it ends with
            // </think> so we need to remove that and put that into a new thinking response element
            // Extract thinking content if present using base class helper
            const extracted = this.extractThinkingFromContent(rawMessage);
            const res: ChatResultChoice = {
                    content: extracted.content,
                    thinking: extracted.thinking || choice.message.reasoning // Include reasoning field if present
        const usage = chatResponse.usage as ChatCompletion.ChatCompletionResponse.Usage
                usage: new ModelUsage(usage.prompt_tokens, usage.completion_tokens)
     * Create a streaming request for Cerebras
        // Initialize streaming state for thinking extraction if supported
        if (this.supportsThinkingModels()) {
            this.initializeThinkingStreamState();
        // Set response format if specified
        return this.client.chat.completions.create(cerebrasParams);
     * Process a streaming chunk from Cerebras
        let usage = undefined;
        if (chunk?.choices && chunk.choices.length > 0) {
            const choice = chunk.choices[0];
                const rawContent = choice.delta.content;
                // Process the content with thinking extraction if supported
                content = this.supportsThinkingModels() 
                    ? this.processStreamChunkWithThinking(rawContent)
                    : rawContent;
            if (choice?.finish_reason) {
                finishReason = choice.finish_reason;
        // Cerebras sends usage metrics in the final chunk 
        if (chunk?.usage) {
                chunk.usage.prompt_tokens,
                chunk.usage.completion_tokens
     * Create the final response from streaming results for Cerebras
        // Extract finish reason from last chunk if available
        let finishReason = 'stop';
        if (lastChunk?.choices && lastChunk.choices.length > 0 && lastChunk.choices[0].finish_reason) {
            finishReason = lastChunk.choices[0].finish_reason;
        // Get thinking content from streaming state if available
        const thinkingContent = this.thinkingStreamState?.accumulatedThinking.trim();
        // For Cerebras, usage metrics come in the final chunk
                message: this.addThinkingToMessage({
                }, thinkingContent),
                finish_reason: finishReason,
     * Summarizes text using Cerebras LLM capabilities
     * @param params Parameters for the summarization
     * @returns A summary result
     * Classifies text into categories using Cerebras LLM capabilities
     * @param params Parameters for classification
     * @returns A classification result
