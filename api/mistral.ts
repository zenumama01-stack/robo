import { BaseLLM, ChatParams, ChatResult, ChatResultChoice, ChatMessageRole, ClassifyParams, ClassifyResult, SummarizeParams, SummarizeResult, ModelUsage, ChatMessage, ErrorAnalyzer } from '@memberjunction/ai';
import { Mistral } from "@mistralai/mistralai";
import { ChatCompletionChoice, ResponseFormat, CompletionEvent, CompletionResponseStreamChoice, ChatCompletionStreamRequest } from '@mistralai/mistralai/models/components';
@RegisterClass(BaseLLM, "MistralLLM")
export class MistralLLM extends BaseLLM {
    private _client: Mistral;
        this._client = new Mistral({
            apiKey: apiKey
    public get Client(): Mistral {return this._client;}
     * Mistral supports streaming
     * Implementation of non-streaming chat completion for Mistral
        let responseFormat: ResponseFormat | undefined = undefined;
            if(params.responseFormat === 'JSON') {
        // Convert messages to format expected by Mistral
        const messages = this.MapMJMessagesToMistral(params.messages);
        // Create params object
        const params_obj: any = {
            maxTokens: params.maxOutputTokens,
            responseFormat: responseFormat
            params_obj.temperature = params.temperature;
            params_obj.topP = params.topP;
            params_obj.topK = params.topK;
            params_obj.randomSeed = params.seed; // Mistral uses randomSeed instead of seed
            params_obj.stop = params.stopSequences;
        // Mistral doesn't support these parameters - warn if provided
            console.warn('Mistral provider does not support frequencyPenalty parameter, ignoring');
            console.warn('Mistral provider does not support presencePenalty parameter, ignoring');
            console.warn('Mistral provider does not support minP parameter, ignoring');
        // Note: Mistral doesn't have a direct equivalent to effortLevel/reasoning_effort as of current API version
        // If/when Mistral adds this functionality, it should be added here
        const chatResponse = await this.Client.chat.complete(params_obj);
        let choices: ChatResultChoice[] = chatResponse.choices.map((choice: ChatCompletionChoice) => {
            let rawContent: string = "";
            if(choice.message.content && typeof choice.message.content === 'string') {
                rawContent = choice.message.content;
            else if(choice.message.content && Array.isArray(choice.message.content)) {
                rawContent = choice.message.content.join(' ');
            // Extract thinking content from Magistral models
                finish_reason: choice.finishReason,
        // Create ModelUsage
        const usage = new ModelUsage(chatResponse.usage.promptTokens, chatResponse.usage.completionTokens);
            provider: 'mistral',
            id: chatResponse.id,
            object: chatResponse.object,
            created: chatResponse.created
     * Create a streaming request for Mistral
        const params_obj: ChatCompletionStreamRequest = {
            responseFormat: responseFormat,
            (params_obj as any).temperature = params.temperature;
            (params_obj as any).topP = params.topP;
            (params_obj as any).topK = params.topK;
            (params_obj as any).randomSeed = params.seed; // Mistral uses randomSeed instead of seed
            (params_obj as any).stop = params.stopSequences;
        return this.Client.chat.stream(params_obj);
     * Process a streaming chunk from Mistral
        if (chunk?.data?.choices && chunk.data.choices.length > 0) {
            const choice = chunk.data.choices[0];
                let rawContent = '';
                if (typeof choice.delta.content === 'string') {
                    rawContent = choice.delta.content;
                } else if (Array.isArray(choice.delta.content)) {
                    rawContent = choice.delta.content.join('');
            if (choice?.finishReason) {
                finishReason = choice.finishReason;
            if (chunk?.data?.usage) {
                    chunk.data.usage.promptTokens || 0,
                    chunk.data.usage.completionTokens || 0
                    // with Mistral Magistral models, but handle it just in case
     * Create the final response from streaming results for Mistral
                finish_reason: lastChunk?.data?.choices?.[0]?.finishReason || 'stop',
    protected MapMJMessagesToMistral(messages: ChatMessage[]): Array<any> {
        const returnMessages = messages.map(m => {
                    content: m.content.map(block => {
                        let mistralType = undefined;
                        switch (block.type) {
                            case 'text':
                                mistralType = 'text';
                                mistralType = 'image_url';
                                mistralType = 'document_url';
                                console.warn(`${block.type} type is not supported in Mistral`);
                            type: block.type,
                            content: block.content
                    }).filter(block => block.type !== undefined)
        // Mistral expects the last message to either be a user message or a tool message
        if (returnMessages.length > 0) {
            const lastMessage = returnMessages[returnMessages.length - 1];
            if (lastMessage.role !== 'user' /*&& lastMessage.role !== 'tool' -- in future if BaseLLM supports tool messages*/) {
                returnMessages.push({
                    content: 'ok' // Placeholder message to satisfy Mistral's requirement
        return returnMessages;
