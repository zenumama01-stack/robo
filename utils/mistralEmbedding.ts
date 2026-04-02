import { EmbedTextParams, EmbedTextsParams, BaseEmbeddings, ModelUsage, EmbedTextResult, EmbedTextsResult, ErrorAnalyzer } from "@memberjunction/ai";
import { EmbeddingRequest, EmbeddingResponse } from "@mistralai/mistralai/models/components";
@RegisterClass(BaseEmbeddings, 'MistralEmbedding')
export class MistralEmbedding extends BaseEmbeddings {
     * Mistral AI embedding endpoint outputs vectors in 1024 dimensions
            const request: EmbeddingRequest = {
                inputs: params.text,
                model: params.model || "mistral-embed",
            params.model = params.model || "mistral-embed";
            const response: EmbeddingResponse = await this.Client.embeddings.create(request);
            let vector: number[] = [];
            if (response.data.length > 0){
                vector = response.data[0].embedding;
                object: response.object as "object" | "list",
                model: response.model,
                ModelUsage: new ModelUsage(response.usage.promptTokens, response.usage.completionTokens),
                vector: vector
            const errorInfo = ErrorAnalyzer.analyzeError(error, 'Mistral');
            console.error('Mistral embedding error:', errorInfo);
                object: "object",
                inputs: params.texts,
                vectors: response.data.map((data) => data.embedding)
            let allModels = await this.Client.models.list();
            return allModels;
            console.error('Error listing Mistral embedding models:', ErrorAnalyzer.analyzeError(error, 'Mistral'));
