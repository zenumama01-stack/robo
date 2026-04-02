  EmbedTextsParams, 
  EmbedTextsResult,
} from "@memberjunction/ai";
  BedrockRuntimeClient, 
  InvokeModelCommand 
} from '@aws-sdk/client-bedrock-runtime';
@RegisterClass(BaseEmbeddings, 'BedrockEmbedding')
export class BedrockEmbedding extends BaseEmbeddings {
  private _client: BedrockRuntimeClient;
  private _region: string;
  constructor(apiKey: string, region: string = 'us-east-1') {
    this._region = region;
    // Initialize AWS Bedrock client
    this._client = new BedrockRuntimeClient({
      region: this._region,
      credentials: {
        accessKeyId: apiKey.split(':')[0],
        secretAccessKey: apiKey.split(':')[1]
  public get Client(): BedrockRuntimeClient {
   * Embeds a single text using Amazon Bedrock embedding models
      // The modelId should be specified in the model parameter, e.g., "amazon.titan-embed-text-v1"
      const modelId = params.model || 'amazon.titan-embed-text-v1';
      // Create request body - differs based on the provider's embedding model
      let requestBody: any;
      if (modelId.startsWith('amazon.titan-embed')) {
        requestBody = { inputText: params.text };
      } else if (modelId.startsWith('cohere.embed')) {
        requestBody = { texts: [params.text], input_type: "search_document" };
        throw new Error(`Unsupported embedding model provider for Bedrock: ${modelId}`);
      // Invoke the model
      const command = new InvokeModelCommand({
        body: JSON.stringify(requestBody),
        contentType: 'application/json',
        accept: 'application/json'
      const response = await this._client.send(command);
      const responseBody = JSON.parse(new TextDecoder().decode(response.body));
      // Parse response body based on the model provider
      let embedding: number[] = [];
      let tokensUsed = 0;
        embedding = responseBody.embedding;
        tokensUsed = responseBody.inputTextTokenCount || 0;
        embedding = responseBody.embeddings[0];
        tokensUsed = responseBody.tokens || 0;
        object: "object" as "object" | "list",
        model: modelId,
        ModelUsage: new ModelUsage(tokensUsed, 0),
        vector: embedding
      const errorInfo = ErrorAnalyzer.analyzeError(error, 'Bedrock');
      console.error('Bedrock embedding error:', errorInfo);
        model: params.model || 'amazon.titan-embed-text-v1',
        ModelUsage: new ModelUsage(0, 0),
        vector: []
   * Embeds multiple texts using Amazon Bedrock embedding models
      // Some embedding models may support batch processing, others may not
      // For those that don't, we'll process texts one by one
      if (modelId.startsWith('cohere.embed')) {
        // Cohere supports batch embedding
        const requestBody = { 
          texts: params.texts, 
          input_type: "search_document" 
          object: "list",
          ModelUsage: new ModelUsage(responseBody.tokens || 0, 0),
          vectors: responseBody.embeddings
        // For models that don't support batch embedding, process one by one
        // and collect the results
        const embeddings: number[][] = [];
        for (const text of params.texts) {
          const result = await this.EmbedText({
            model: modelId
          embeddings.push(result.vector);
          totalTokens += result.ModelUsage.promptTokens;
          ModelUsage: new ModelUsage(totalTokens, 0),
          vectors: embeddings
        vectors: []
   * Get available embedding models from Bedrock
      // In practice, you would list models from Bedrock and filter for embedding models
        "amazon.titan-embed-text-v1",
        "amazon.titan-embed-image-v1",
        "cohere.embed-english-v3",
        "cohere.embed-multilingual-v3"
      console.error('Error listing Bedrock embedding models:', ErrorAnalyzer.analyzeError(error, 'Bedrock'));
