@RegisterClass(BaseEmbeddings, 'OpenAIEmbedding')
export class OpenAIEmbedding extends BaseEmbeddings {
        this._openAI = new OpenAI({
            apiKey: apiKey,
        let body: OpenAI.Embeddings.EmbeddingCreateParams = {
            input: params.text,
            model: params.model || "text-embedding-3-small"
            let response = await this.OpenAI.embeddings.create(body);
                object: response.object,
                ModelUsage: new ModelUsage(response.usage.prompt_tokens, 0),
                vector: response.data[0].embedding
        catch(error){
            const errorInfo = ErrorAnalyzer.analyzeError(error, 'OpenAI');
            console.error('OpenAI embedding error:', errorInfo);
                model: params.model || "text-embedding-3-small",
    //openAI doesnt have an endpoint we can call
                Model: 'text-embedding-3-large',
                Description: "Most capable embedding model for both english and non-english tasks",
                OutputDimension: 3072,
                Model: 'text-embedding-3-small',
                Description: "Increased performance over 2nd generation ada embedding model",
                OutputDimension: 1536,
                Model: 'text-embedding-ada-002',
                Description: "Most capable 2nd generation embedding model, replacing 16 first generation models",
