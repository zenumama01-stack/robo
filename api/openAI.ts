import { BaseLLM, ChatMessage, ChatMessageRole, ChatParams, ChatResult, ClassifyParams, ClassifyResult, GetUserMessageFromChatParams, ModelUsage, SummarizeParams, SummarizeResult, StreamingChatCallbacks, ErrorAnalyzer } from "@memberjunction/ai";
import { ChatCompletionAssistantMessageParam, ChatCompletionContentPart, ChatCompletionMessageParam, ChatCompletionSystemMessageParam, ChatCompletionUserMessageParam } from "openai/resources";
 * OpenAI implementation of the BaseLLM class
@RegisterClass(BaseLLM, 'OpenAILLM')
export class OpenAILLM extends BaseLLM {
    private _openAI: OpenAI;
        // now create the OpenAI instance
        const params: Record<string, any> = {
        if (baseURL && baseURL.length > 0) {
            params.baseURL = baseURL; // OpenAI base URL is optional, lots of sub-classes and users might use other providers that are Open AI compatible
        this._openAI = new OpenAI(params);
     * Read only getter method to get the OpenAI instance
    public get OpenAI(): OpenAI {
        return this._openAI;
     * OpenAI supports streaming
     * Check if the model supports reasoning via system prompt keywords
     * GPT-OSS models use "Reasoning: low/medium/high" in system prompt
    private supportsReasoningViaSystemPrompt(modelName: string): boolean {
        return lowerModel.includes('gpt-oss') || lowerModel.includes('gptoss');
     * Convert effort level to reasoning level string for system prompt
    private getReasoningLevel(effortLevel: string): 'low' | 'medium' | 'high' {
        const numValue = Number.parseInt(effortLevel);
        if (isNaN(numValue)) {
            const level = effortLevel.trim().toLowerCase();
            if (level === 'low' || level === 'medium' || level === 'high') {
                return level as 'low' | 'medium' | 'high';
            throw new Error(`Invalid effortLevel: ${effortLevel}`);
        // Map numeric values to levels
        if (numValue <= 33) return 'low';
        if (numValue <= 66) return 'medium';
        return 'high';
     * Implementation of non-streaming chat completion for OpenAI
        let messages = params.messages;
        // Handle reasoning for GPT-OSS models via system prompt
        const supportsReasoningInSystemPrompt = this.supportsReasoningViaSystemPrompt(params.model);
        if (params.effortLevel && supportsReasoningInSystemPrompt) {
            const reasoningLevel = this.getReasoningLevel(params.effortLevel);
            // Add or append to system message
            const systemMsg = messages.find(m => m.role === 'system');
                // Append to existing system message
                systemMsg.content = `${systemMsg.content}\n\nReasoning: ${reasoningLevel}`;
                // Prepend new system message
                    { role: 'system', content: `Reasoning: ${reasoningLevel}` },
                    ...messages
        const formattedMessages = this.ConvertMJToOpenAIChatMessages(messages);
        const openAIParams: OpenAI.Chat.Completions.ChatCompletionCreateParamsNonStreaming = {
            logprobs: params.includeLogProbs === true ? true : false,
            top_logprobs: params.includeLogProbs && params.topLogProbs ? params.topLogProbs : undefined,
        //Reasoning effort level has been provided and it wasn't handled via system prompt
        if (params.effortLevel && !supportsReasoningInSystemPrompt) {
            openAIParams.reasoning_effort = reasoningLevel;
            openAIParams.top_p = params.topP;
            openAIParams.frequency_penalty = params.frequencyPenalty;
            openAIParams.presence_penalty = params.presencePenalty;
            openAIParams.seed = params.seed;
            openAIParams.stop = params.stopSequences;
        // OpenAI doesn't support topK or minP - warn if provided
            console.warn('OpenAI provider does not support topK parameter, ignoring');
            console.warn('OpenAI provider does not support minP parameter, ignoring');
                openAIParams.response_format = { type: "json_object" };
                openAIParams.response_format = params.modelSpecificResponseFormat;
        // GPT 5 doesn't support a temperature value other than 1.
        if ((openAIParams.model.toLowerCase() === 'gpt-5') && (openAIParams.temperature)) {
            if (openAIParams.temperature !== 1) {
                openAIParams.temperature = 1;
        const result = await this.OpenAI.chat.completions.create(openAIParams);
        // Create ModelUsage with any available timing data
        // OpenAI doesn't provide the same timing metrics as Groq,
        // but we can check for any extended usage data
        const extendedUsage = result.usage as any;
        if (extendedUsage.prompt_tokens_details) {
            // Store prompt token details in usage if needed in future
        if (extendedUsage.completion_tokens_details) {
            // Store completion token details in usage if needed in future
                choices: result.choices.map((c: any) => {
                    // Extract thinking/reasoning content if present
                    let content = c.message.content;
                    // For o1 models, OpenAI may include reasoning in a specific field or format
                    // Check if the message has any reasoning-related data
                    if (c.message.reasoning_content) {
                        thinking = c.message.reasoning_content;
                    } else if (c.message.reasoning) {
                        thinking = c.message.reasoning;
                    // Some o1 models might include reasoning in the content with special markers
                    // Check for common reasoning patterns in the content itself
                    if (!thinking && content && typeof content === 'string') {
                        // Check for thinking tags similar to Anthropic's format
                        if (content.startsWith('<thinking>') && content.includes('</thinking>')) {
                            thinking = content.substring(thinkStart, thinkEnd).trim();
                            // Remove thinking content from main content
                            content = content.substring(thinkEnd + '</thinking>'.length).trim();
                        finish_reason: c.finish_reason,
                        index: c.index,
                        logprobs: c.logprobs // Include logprobs if present
            provider: 'openai',
            service_tier: (result as any).service_tier,
            usage_details: {
                reasoning_tokens: extendedUsage.reasoning_tokens,
                cached_tokens: extendedUsage.cached_tokens,
                prompt_tokens_details: extendedUsage.prompt_tokens_details,
                completion_tokens_details: extendedUsage.completion_tokens_details
     * Create a streaming request for OpenAI
        if (params.effortLevel && this.supportsReasoningViaSystemPrompt(params.model)) {
        const openAIParams: OpenAI.Chat.Completions.ChatCompletionCreateParamsStreaming = {
        if (params.effortLevel) {
        return this.OpenAI.chat.completions.create(openAIParams);
     * Process a streaming chunk from OpenAI
        // Handle potential null/undefined values safely
        const usage = chunk?.usage || null;
        // Check if chunk contains reasoning content (for o1 models)
        const delta = chunk?.choices?.[0]?.delta;
        if (delta) {
            // Check for reasoning fields specific to o1 models
            if (delta.reasoning_content) {
                this._streamingState.accumulatedThinking += delta.reasoning_content;
                // Don't emit reasoning as regular content
                    } : null
            } else if (delta.reasoning) {
                this._streamingState.accumulatedThinking += delta.reasoning;
            // Process regular content
            const rawContent = delta.content || '';
            if (rawContent) {
     * Create the final response from streaming results for OpenAI
        const result = await this.OpenAI.chat.completions.create({
                            console.warn(`Unsupported content type for OpenAI API: ${c.type}. This content will be skipped.`);
     *  - system maps to system
     *  - anything else throws an error
