import { GoogleGenAI, Modality, GenerateContentResponse } from "@google/genai";
 * Google Gemini 3 Pro Image (Nano Banana Pro) implementation of the BaseImageGenerator class.
 * Supports Google's most advanced image generation model with 2K/4K resolution support.
 * Model: gemini-3-pro-image-preview (November 2025)
 * - Native multimodal image generation
 * - High resolution output (up to 4K)
 * - Advanced style control and prompt understanding
@RegisterClass(BaseImageGenerator, 'GeminiImageGenerator')
export class GeminiImageGenerator extends BaseImageGenerator {
    protected _gemini: GoogleGenAI | null = null;
    private _geminiPromise: Promise<GoogleGenAI> | null = null;
     * Factory method to create the GoogleGenAI client instance.
     * Subclasses can override this to provide custom configuration.
    protected async createClient(): Promise<GoogleGenAI> {
        return new GoogleGenAI({ apiKey: this.apiKey });
     * Ensure the Gemini client is initialized before use.
    private async ensureGeminiClient(): Promise<GoogleGenAI> {
        if (this._gemini) {
            return this._gemini;
        if (!this._geminiPromise) {
            this._geminiPromise = this.createClient();
        this._gemini = await this._geminiPromise;
     * Generate image(s) from a text prompt using Gemini 3 Pro Image (Nano Banana Pro).
            const client = await this.ensureGeminiClient();
            const modelName = params.model || 'gemini-3-pro-image-preview';
            // Build the generation config
            const generateConfig = this.buildGenerationConfig(params);
            // Generate images
            const images: GeneratedImage[] = [];
            const numImages = params.n || 1;
            // Generate images one at a time for consistency
            for (let i = 0; i < numImages; i++) {
                const response = await client.models.generateContent({
                    model: modelName,
                    contents: params.prompt,
                    config: generateConfig
                const generatedImage = this.extractImageFromResponse(response, i, params);
                if (generatedImage) {
                    images.push(generatedImage);
            if (images.length === 0) {
                return this.createErrorResult(startTime, 'No images were generated');
            return this.createSuccessResult(startTime, images);
     * Edit an existing image using Gemini's multimodal capabilities.
     * Uses image-to-image generation with the original image as context.
            // Normalize the input image
            // Build multimodal content with the image and edit prompt
                    inlineData: {
                        data: imageInput.base64,
                        mimeType: 'image/png'
                    text: `Edit this image: ${params.prompt}`
            const generateConfig = this.buildGenerationConfig({
                prompt: params.prompt
            } as ImageGenerationParams);
                contents: content,
            const generatedImage = this.extractImageFromResponse(response, 0, params as unknown as ImageGenerationParams);
            if (!generatedImage) {
                return this.createErrorResult(startTime, 'Image editing did not produce a result');
     * Uses the original image as context with a variation prompt.
            const variationPrompt = params.prompt || 'Create a variation of this image with similar style and content but with subtle creative differences';
            // Build multimodal content with the image and variation prompt
                    text: variationPrompt
                prompt: variationPrompt
            const numVariations = params.n || 1;
            for (let i = 0; i < numVariations; i++) {
                const generatedImage = this.extractImageFromResponse(response, i, params as unknown as ImageGenerationParams);
                return this.createErrorResult(startTime, 'No image variations were generated');
     * Get available Gemini image generation models.
                id: 'gemini-3-pro-image-preview',
                name: 'Gemini 3 Pro Image (Nano Banana Pro)',
                description: 'Google\'s most advanced image generation model (November 2025). Supports up to 4K resolution with exceptional quality and style control.',
                supportedSizes: ['1024x1024', '1536x1024', '1024x1536', '2048x2048', '3840x2160', '2160x3840'],
                maxImages: 4,
                supportsNegativePrompt: true,
     * Build generation config for Gemini API.
    private buildGenerationConfig(params: ImageGenerationParams): Record<string, unknown> {
        const config: Record<string, unknown> = {
            // Request image output modality
            responseModalities: [Modality.IMAGE, Modality.TEXT]
        // Add size/aspect ratio if specified
                // Gemini 3 Pro Image supports explicit dimensions
                config.imageSize = { width, height };
                // Also set aspect ratio for compatibility
                if (width > height) {
                    config.aspectRatio = '16:9';
                } else if (height > width) {
                    config.aspectRatio = '9:16';
                    config.aspectRatio = '1:1';
        if (params.aspectRatio) {
            config.aspectRatio = params.aspectRatio;
        // Add negative prompt if supported
        if (params.negativePrompt) {
            config.negativePrompt = params.negativePrompt;
        // Add seed for reproducibility
            config.seed = params.seed;
        // Quality setting
        if (params.quality) {
            config.quality = params.quality;
        // Style setting
        if (params.style) {
            config.style = params.style;
        // Merge any provider-specific options
        if (params.providerOptions) {
            Object.assign(config, params.providerOptions);
     * Extract image data from Gemini response.
    private extractImageFromResponse(
        response: GenerateContentResponse,
        params: ImageGenerationParams
    ): GeneratedImage | null {
        const candidates = response.candidates;
        if (!candidates || candidates.length === 0) {
        const candidate = candidates[0];
        if (!candidate.content || !candidate.content.parts) {
        // Find the image part in the response
        for (const part of candidate.content.parts) {
            // Check for inline data (base64 image)
            if (part.inlineData && part.inlineData.data) {
                generatedImage.base64 = part.inlineData.data;
                generatedImage.data = Buffer.from(part.inlineData.data, 'base64');
                generatedImage.format = this.getMimeTypeFormat(part.inlineData.mimeType || 'image/png');
                // Set dimensions from params if available
                        generatedImage.width = width;
                        generatedImage.height = height;
            // Check for file data (URL reference)
            if (part.fileData && part.fileData.fileUri) {
                generatedImage.url = part.fileData.fileUri;
                generatedImage.format = this.getMimeTypeFormat(part.fileData.mimeType || 'image/png');
     * Convert MIME type to image format.
    private getMimeTypeFormat(mimeType: string): 'png' | 'jpg' | 'jpeg' | 'webp' | 'gif' {
        if (mimeType.includes('jpeg') || mimeType.includes('jpg')) {
            return 'jpeg';
        if (mimeType.includes('webp')) {
            return 'webp';
        if (mimeType.includes('gif')) {
            return 'gif';
        return 'png';
        const errorInfo = ErrorAnalyzer.analyzeError(error, 'Gemini');
        console.error('Gemini Image Generation error:', errorMessage, errorInfo);
