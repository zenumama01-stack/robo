import OpenAI from "openai";
import { type Uploadable } from "openai";
 * Extended image generation params for GPT Image models (gpt-image-1, gpt-image-1.5).
 * The OpenAI SDK v5.x doesn't include these types yet, so we define them locally.
 * These match the OpenAI API spec for GPT Image models introduced in 2025.
interface GptImageGenerateParams {
    size?: string;
    output_format?: 'png' | 'jpeg' | 'webp';
    quality?: 'high' | 'medium' | 'low';
 * Extended image edit params for GPT Image models.
interface GptImageEditParams {
    image: Uploadable;
    mask?: Uploadable;
 * Extended image variation params for GPT Image models.
interface GptImageVariationParams {
 * OpenAI GPT-4o Image implementation of the BaseImageGenerator class.
 * Supports gpt-image-1.5 (December 2025) for native multimodal image generation.
 * This replaces the older DALL-E models with OpenAI's GPT-4o native image generation
 * capabilities, which produce higher quality results through the ChatGPT API.
@RegisterClass(BaseImageGenerator, 'OpenAIImageGenerator')
export class OpenAIImageGenerator extends BaseImageGenerator {
        const params: Record<string, string> = { apiKey };
            params.baseURL = baseURL;
     * Read-only getter for the OpenAI client instance
     * Generate image(s) from a text prompt using GPT-4o Image Generation
            const openAIParams = this.buildGenerationParams(params);
            // The OpenAI SDK v5.x types don't include GPT Image model parameters (output_format, etc.)
            // but the API accepts them. We cast here at the API boundary since we've validated
            // our params match the actual API spec. SDK v6.x includes proper GPT Image types.
            const response = await this._openAI.images.generate(
                openAIParams as OpenAI.Images.ImageGenerateParams
            // Handle the response - ensure it's not a stream
            if ('data' in response) {
                return this.processGenerationResponse(response, startTime, params);
                return this.createErrorResult(startTime, 'Unexpected streaming response from OpenAI');
     * Edit an existing image.
     * Note: GPT-4o Image models support image editing through the chat interface with vision.
     * This method uses the images.edit endpoint for supported operations.
            // Create File objects for OpenAI API
            const imageFile = new File([new Uint8Array(imageInput.buffer)], 'image.png', { type: 'image/png' });
            const maskFile = maskInput
                ? new File([maskInput.buffer as BlobPart], 'mask.png', { type: 'image/png' })
            const openAIParams = this.buildEditParams(params, imageFile, maskFile);
            // Cast at API boundary - see comment in GenerateImage for rationale
            const response = await this._openAI.images.edit(
                openAIParams as OpenAI.Images.ImageEditParams
                return this.processEditResponse(response, startTime, params);
     * Build edit params for either GPT Image or DALL-E models
    private buildEditParams(
        params: ImageEditParams,
        imageFile: File,
        maskFile?: File
    ): GptImageEditParams | OpenAI.Images.ImageEditParams {
        const model = params.model || 'gpt-image-1.5';
        if (this.isGptImageModel(model)) {
            const gptParams: GptImageEditParams = {
                image: imageFile,
                n: params.n || 1,
                size: this.normalizeSize(params.size),
            if (maskFile) {
                gptParams.mask = maskFile;
            return gptParams;
            const dalleParams: OpenAI.Images.ImageEditParams = {
                size: this.normalizeSize(params.size) as OpenAI.Images.ImageEditParams['size'],
                response_format: params.outputFormat === 'url' ? 'url' : 'b64_json'
                dalleParams.mask = maskFile;
            return dalleParams;
     * Uses GPT-4o's multimodal capabilities to generate variations.
            // Create File object for OpenAI API
            const openAIParams = this.buildVariationParams(params, imageFile);
            const response = await this._openAI.images.createVariation(
                openAIParams as OpenAI.Images.ImageCreateVariationParams
            return this.processVariationResponse(response, startTime, params);
     * Build variation params for either GPT Image or DALL-E models
    private buildVariationParams(
        params: ImageVariationParams,
        imageFile: File
    ): GptImageVariationParams | OpenAI.Images.ImageCreateVariationParams {
        const variationSize = this.normalizeVariationSize(params.size);
                size: variationSize,
                size: variationSize as OpenAI.Images.ImageCreateVariationParams['size'],
     * Get available GPT-4o Image models
                id: 'gpt-image-1.5',
                name: 'GPT-4o Image 1.5',
                description: 'OpenAI\'s latest native multimodal image generation model (December 2025). High quality image generation through GPT-4o architecture.',
                supportedSizes: ['1024x1024', '1536x1024', '1024x1536', '2048x2048', 'auto'],
                id: 'gpt-image-1',
                name: 'GPT-4o Image 1.0',
                description: 'OpenAI\'s GPT-4o native image generation model (April 2025). Multimodal image generation.',
                supportedSizes: ['1024x1024', '1536x1024', '1024x1536', 'auto'],
     * Get supported methods for this provider
     * Build OpenAI-specific generation parameters.
     * Returns either GPT Image params or DALL-E params based on model.
    private buildGenerationParams(params: ImageGenerationParams): GptImageGenerateParams | OpenAI.Images.ImageGenerateParams {
            return this.buildGptImageGenerateParams(params, model);
            return this.buildDalleGenerateParams(params, model);
     * Build params for GPT Image models (gpt-image-1, gpt-image-1.5)
    private buildGptImageGenerateParams(params: ImageGenerationParams, model: string): GptImageGenerateParams {
        const gptParams: GptImageGenerateParams = {
        // GPT Image models use: high, medium, low
            gptParams.quality = params.quality === 'hd' ? 'high' : 'medium';
        if (params.user) {
            gptParams.user = params.user;
     * Build params for DALL-E models (dall-e-2, dall-e-3)
    private buildDalleGenerateParams(params: ImageGenerationParams, model: string): OpenAI.Images.ImageGenerateParams {
        const dalleParams: OpenAI.Images.ImageGenerateParams = {
            size: this.normalizeSize(params.size) as OpenAI.Images.ImageGenerateParams['size'],
        // DALL-E models use: hd, standard
            dalleParams.quality = params.quality === 'hd' ? 'hd' : 'standard';
        // Style parameter (DALL-E 3 only)
            dalleParams.style = params.style === 'natural' ? 'natural' : 'vivid';
            dalleParams.user = params.user;
     * Check if the model is a GPT Image model
    private isGptImageModel(model: string): boolean {
        return model.startsWith('gpt-image');
     * Normalize size parameter to OpenAI-supported sizes for GPT-4o Image
    private normalizeSize(size: string | undefined): string {
        if (!size) {
            return '1024x1024';
        // GPT-4o Image supported sizes
        const supportedSizes = ['1024x1024', '1536x1024', '1024x1536', '2048x2048', 'auto'];
        if (supportedSizes.includes(size)) {
        // Map common aspect ratios
        if (size.includes('portrait') || size === '9:16') {
            return '1024x1536';
        if (size.includes('landscape') || size === '16:9') {
            return '1536x1024';
        if (size.includes('2k') || size.includes('2048')) {
            return '2048x2048';
     * Normalize size for variation API which only supports limited sizes
    private normalizeVariationSize(size: string | undefined): '256x256' | '512x512' | '1024x1024' {
        // Variation API only supports these sizes
        if (size === '256x256' || size === '512x512' || size === '1024x1024') {
        // Default to largest supported size
     * Process the generation API response into our result type
    private processGenerationResponse(
        response: OpenAI.Images.ImagesResponse,
    ): ImageGenerationResult {
        const result = this.createSuccessResult(startTime, []);
        const data = response.data ?? [];
        for (let i = 0; i < data.length; i++) {
            const imageData = data[i];
            if (imageData.b64_json) {
                generatedImage.base64 = imageData.b64_json;
                generatedImage.data = Buffer.from(imageData.b64_json, 'base64');
            if (imageData.url) {
                generatedImage.url = imageData.url;
            if (imageData.revised_prompt) {
                result.revisedPrompt = imageData.revised_prompt;
            generatedImage.index = i;
            // Parse size for dimensions
            const size = params.size || '1024x1024';
            if (size !== 'auto') {
                const [width, height] = size.split('x').map(Number);
            result.images.push(generatedImage);
     * Process the edit API response
    private processEditResponse(
        _params: ImageEditParams
     * Process the variation API response
    private processVariationResponse(
        _params: ImageVariationParams
     * Handle errors and create error result
        console.error('OpenAI Image Generation error:', errorMessage, errorInfo);
