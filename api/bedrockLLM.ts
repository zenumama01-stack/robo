  ChatMessage,
  ErrorAnalyzer,
  parseBase64DataUrl
  InvokeModelCommand, 
  InvokeModelWithResponseStreamCommand 
@RegisterClass(BaseLLM, "BedrockLLM")
export class BedrockLLM extends BaseLLM {
   * Amazon Bedrock supports streaming for most models
   * Implementation of non-streaming chat completion for Amazon Bedrock
      // Map provider-agnostic params to Bedrock-specific format
      const bedrockParams = this.mapToBedrockParams(params);
      // The modelId should be specified in the model parameter, e.g., "anthropic.claude-v2"
      const modelId = params.model;
      // Create the request body - depends on the specific model provider in Bedrock
      if (modelId.startsWith('anthropic.')) {
        // Anthropic Claude models
        requestBody = {
          anthropic_version: "bedrock-2023-05-31",
          max_tokens: params.maxOutputTokens || 1024,
          messages: bedrockParams.messages,
          temperature: params.temperature || 0.7,
          top_p: 0.9
      } else if (modelId.startsWith('ai21.')) {
        // AI21 models
          prompt: this.convertMessagesToPrompt(bedrockParams.messages),
          maxTokens: params.maxOutputTokens || 1024,
          topP: 0.9
      } else if (modelId.startsWith('amazon.titan-')) {
        // Amazon Titan models
          inputText: this.convertMessagesToPrompt(bedrockParams.messages),
          textGenerationConfig: {
            maxTokenCount: params.maxOutputTokens || 1024,
      } else if (modelId.startsWith('meta.')) {
        // Meta Llama models
          max_gen_len: params.maxOutputTokens || 1024,
        throw new Error(`Unsupported model provider for Bedrock: ${modelId}`);
      // Parse response body based on model provider
      let tokenUsage = { promptTokens: 0, completionTokens: 0, totalTokens: 0 };
        content = responseBody.content?.[0]?.text || '';
        tokenUsage = {
          promptTokens: responseBody.usage?.input_tokens || 0,
          completionTokens: responseBody.usage?.output_tokens || 0,
          totalTokens: (responseBody.usage?.input_tokens || 0) + (responseBody.usage?.output_tokens || 0)
        content = responseBody.completions?.[0]?.data?.text || '';
          promptTokens: responseBody.prompt_tokens || 0,
          completionTokens: responseBody.completion_tokens || 0,
          totalTokens: (responseBody.prompt_tokens || 0) + (responseBody.completion_tokens || 0)
        content = responseBody.results?.[0]?.outputText || '';
          promptTokens: responseBody.inputTextTokenCount || 0,
          completionTokens: responseBody.outputTextTokenCount || 0,
          totalTokens: (responseBody.inputTextTokenCount || 0) + (responseBody.outputTextTokenCount || 0)
        content = responseBody.generation || '';
        // Llama models via Bedrock may not provide token counts
      // Create the ChatResult
      const choices: ChatResultChoice[] = [{
          role: ChatMessageRole.assistant,
          content: content
          usage: new ModelUsage(tokenUsage.promptTokens, tokenUsage.completionTokens)
        errorMessage: error.message || "Error calling Amazon Bedrock",
        errorInfo: ErrorAnalyzer.analyzeError(error, 'Bedrock')
   * Create a streaming request for Bedrock
    // Invoke the model with streaming
    const command = new InvokeModelWithResponseStreamCommand({
    return this._client.send(command);
   * Process a streaming chunk from Bedrock
    let finishReason = null;
    if (chunk.chunk?.bytes) {
      const chunkData = JSON.parse(new TextDecoder().decode(chunk.chunk.bytes));
      if (chunkData.completion) {
        content = chunkData.completion;
      } else if (chunkData.delta?.text) {
        content = chunkData.delta.text;
      } else if (chunkData.outputText) {
        content = chunkData.outputText;
      } else if (chunkData.generation) {
        content = chunkData.generation;
      } else if (chunkData.content?.[0]?.text) {
        content = chunkData.content[0].text;
      // Check for finish reason and token usage
      if (chunkData.stop_reason) {
        finishReason = chunkData.stop_reason;
      if (chunkData.usage) {
          chunkData.usage.input_tokens || 0,
          chunkData.usage.output_tokens || 0
   * Create the final response from streaming results for Bedrock
        finish_reason: lastChunk?.finishReason || 'stop',
   * Not implemented yet
   * Map MemberJunction ChatParams to Bedrock-specific params
  private mapToBedrockParams(params: ChatParams): any {
      modelId: params.model,
      messages: this.convertToBedrockMessages(params.messages),
      maxTokens: params.maxOutputTokens
   * Convert MemberJunction chat messages to Bedrock-compatible messages
   * Supports multimodal content for Claude models (images via base64)
  private convertToBedrockMessages(messages: ChatMessage[]): any[] {
    return messages.map(msg => {
      const role = this.mapRole(msg.role);
      // If content is a simple string, return as-is for text-only models
      // or wrap in content array for Claude models
      if (typeof msg.content === 'string') {
          role,
      // Content is an array of ChatMessageContentBlock - convert to Bedrock format
      const contentBlocks = msg.content as ChatMessageContentBlock[];
      const bedrockContent: any[] = [];
          bedrockContent.push({
          // Convert image to Bedrock/Claude format
          const imageBlock = this.formatImageForBedrock(block);
            bedrockContent.push(imageBlock);
        // Note: audio_url, video_url, file_url not yet supported by Bedrock Claude
        content: bedrockContent.length > 0 ? bedrockContent : msg.content
   * Format an image content block for Bedrock Claude API
   * Claude expects: { type: "image", source: { type: "base64", media_type: "image/jpeg", data: "..." } }
  private formatImageForBedrock(block: ChatMessageContentBlock): any | null {
        type: 'image',
          type: 'base64',
    // URLs are not supported by Bedrock Claude - must be base64
      console.warn('Bedrock Claude does not support image URLs, only base64. Skipping image.');
        media_type: 'image/jpeg',
   * Map MemberJunction roles to Bedrock roles
  private mapRole(role: string): string {
      case ChatMessageRole.system:
        return 'system';
      case ChatMessageRole.assistant:
      case ChatMessageRole.user:
   * Convert messages to a single prompt string for models that don't support chat format
  private convertMessagesToPrompt(messages: any[]): string {
    // Basic implementation - can be enhanced for different models
      if (msg.role === 'system') {
        return `System: ${msg.content}\n`;
      } else if (msg.role === 'assistant') {
        return `Assistant: ${msg.content}\n`;
        return `User: ${msg.content}\n`;
    }).join('');
