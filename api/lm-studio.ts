import { BaseLLM, ChatParams, ChatResult, ChatResultChoice, ChatMessageRole, ClassifyParams, ClassifyResult, SummarizeParams, SummarizeResult, ModelUsage, ErrorAnalyzer } from '@memberjunction/ai';
import { LMStudioClient } from '@lmstudio/sdk';
 * LM Studio implementation of the BaseLLM class
@RegisterClass(BaseLLM, "LMStudioLLM")
export class LMStudioLLM extends BaseLLM {
    private _client: LMStudioClient;
    constructor(apiKey?: string) {
        super(apiKey || ''); // LM Studio doesn't require API key for local usage
        this._client = new LMStudioClient();
     * Read only getter method to get the LM Studio client instance
    public get LMStudioClient(): LMStudioClient {
    public get client(): LMStudioClient {
        return this.LMStudioClient;
     * LM Studio supports streaming
     * LM Studio can support thinking models depending on the loaded model
     * Override SetAdditionalSettings to handle LM Studio specific settings
        // Handle LM Studio-specific settings like base URL
        if (settings.baseUrl) {
            // LM Studio client can be configured with custom base URL
            this._client = new LMStudioClient({
                baseUrl: settings.baseUrl
     * Implementation of non-streaming chat completion for LM Studio
            // Get the model instance
            const model = await this.client.llm.model(params.model);
            // Convert MJ messages to LM Studio format
            const messages = params.messages.map(m => ({
                content: Array.isArray(m.content) ? 
                    m.content.map(block => block.content).join('\n') : 
                    m.content
            // Create options for respond() method
            const respondOptions: any = {};
            // Add optional parameters with LM Studio naming conventions
                respondOptions.temperature = params.temperature;
            if (params.maxOutputTokens != null && params.maxOutputTokens > 0) {
                respondOptions.maxPredictedTokens = params.maxOutputTokens;
                respondOptions.topP = params.topP;
                respondOptions.seed = params.seed;
                respondOptions.stopStrings = params.stopSequences;
                respondOptions.frequencyPenalty = params.frequencyPenalty;
                respondOptions.presencePenalty = params.presencePenalty;
            // LM Studio doesn't support topK in the same way - warn if provided
                console.warn('LM Studio provider may not support topK parameter in the expected way, ignoring');
                    // LM Studio may support JSON mode depending on the model
                    respondOptions.responseFormat = { type: "json_object" };
                    respondOptions.responseFormat = params.modelSpecificResponseFormat;
            // Make the chat completion request using respond()
            const response = await model.respond(messages, respondOptions);
                    content: response.nonReasoningContent,
                    thinking: response.reasoningContent  
                finish_reason: 'stop', // LM Studio doesn't provide detailed finish reasons
            // Create ModelUsage - LM Studio may not provide token counts
            const usage = new ModelUsage(0, 0); // Will be updated if available
            // Try to extract usage information if available
            if (response.stats) {
                if (response.stats.promptTokensCount) {
                    usage.promptTokens = response.stats.promptTokensCount;
                if (response.stats.predictedTokensCount) {
                    usage.completionTokens = response.stats.predictedTokensCount;
                // Note: totalTokens is computed automatically by the getter
                provider: 'lmstudio',
                stats: response.stats
                errorMessage: errorMessage,
     * Create a streaming request for LM Studio
        // Create options for respond() method with streaming
        const respondOptions: any = {
        return model.respond(messages, respondOptions);
     * Process a streaming chunk from LM Studio
        // LM Studio streaming format may be different
        // This will need to be adjusted based on actual LM Studio streaming response format
        if (chunk && typeof chunk === 'string') {
            const rawContent = chunk;
        } else if (chunk?.content) {
            const rawContent = chunk.content;
        // Check for finish reason
        if (chunk?.finished) {
            usage: chunk?.stats || null
     * Create the final response from streaming results for LM Studio
        if (lastChunk?.finished) {
        // For LM Studio, we may have usage metrics from the final chunk
        const promptTokens = usage?.promptTokensCount || lastChunk?.stats?.promptTokensCount || 0;
        const completionTokens = usage?.predictedTokensCount || lastChunk?.stats?.predictedTokensCount || 0;
    public async SummarizeText(_params: SummarizeParams): Promise<SummarizeResult> {
    public async ClassifyText(_params: ClassifyParams): Promise<ClassifyResult> {
