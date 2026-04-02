import { RunView, UserInfo } from "@memberjunction/core";
    GetAIAPIKey
import { AIModelEntityExtended, MediaOutput } from "@memberjunction/ai-core-plus";
 * Action that generates images using AI image generation models (DALL-E, Gemini, etc.)
 * // Generate a simple image
 *   ActionName: 'Generate Image',
 *     Name: 'Prompt',
 *     Value: 'A serene mountain landscape at sunset with snow-capped peaks'
 * // Generate with specific model and size
 *     Value: 'A futuristic cityscape with flying vehicles'
 *     Name: 'Model',
 *     Value: 'dall-e-3'
 *     Name: 'Size',
 *     Value: '1792x1024'
 *     Name: 'Quality',
 *     Value: 'hd'
 * // Generate multiple images
 *     Value: 'Abstract art in vibrant colors'
 *     Name: 'NumberOfImages',
 *     Value: 3
 * // Image-to-image editing (transform a source image)
 *     Value: 'Transform this into a professional infographic with dark theme'
 *     Name: 'SourceImage',
 *     Value: 'base64_encoded_image_or_url'
@RegisterClass(BaseAction, "Generate Image")
export class GenerateImageAction extends BaseAction {
     * Generates or edits image(s) using AI image generation models.
     * When SourceImage is provided, performs image-to-image editing (style transfer, transformation).
     * When SourceImage is not provided, performs text-to-image generation.
     *   - Prompt: Text description of the image to generate or edit instructions (required)
     *   - Model: Model name/ID to use (optional, uses default if not specified)
     *   - NumberOfImages: Number of images to generate (optional, default: 1)
     *   - Size: Image size like "1024x1024" (optional)
     *   - Quality: Quality level - "standard" or "hd" (optional)
     *   - Style: Style preset - "vivid" or "natural" (optional)
     *   - NegativePrompt: Things to avoid in the image (optional)
     *   - OutputFormat: "base64" or "url" (optional, default: "base64")
     *   - SourceImage: Source image for image-to-image editing (optional, base64 or URL)
     *   - Mask: Mask image for inpainting - white/transparent areas are regenerated (optional)
     * @returns Generated or edited image(s) as base64 or URLs
            const prompt = this.getParamValue(params, 'prompt');
            const modelName = this.getParamValue(params, 'model');
            const numberOfImages = this.getNumberParam(params, 'numberofimages', 1);
            const size = this.getParamValue(params, 'size') || '1024x1024';
            const quality = this.getParamValue(params, 'quality');
            const style = this.getParamValue(params, 'style');
            const negativePrompt = this.getParamValue(params, 'negativeprompt');
            const outputFormat = this.getParamValue(params, 'outputformat') || 'base64';
            const sourceImage = this.getParamValue(params, 'sourceimage');
            const mask = this.getParamValue(params, 'mask');
            // Validate prompt
                    Message: "Prompt parameter is required",
                    ResultCode: "MISSING_PROMPT"
            // Get image generator model and create instance
            const { generator, model, apiName } = await this.prepareImageGenerator(
                modelName
            let result: ImageGenerationResult;
            if (sourceImage) {
                // Image-to-image: use EditImage when source image is provided
                result = await this.executeImageEdit(generator, {
                    apiName,
                    sourceImage,
                    mask,
                    numberOfImages,
                    size,
                    outputFormat,
                    negativePrompt
                // Text-to-image: use GenerateImage
                result = await this.executeImageGeneration(generator, {
                    quality,
                    style,
                    Message: `Image generation failed: ${result.errorMessage || 'Unknown error'}`,
                    ResultCode: "GENERATION_FAILED"
            if (!result.images || result.images.length === 0) {
                    Message: "No images were generated",
                    ResultCode: "NO_IMAGES"
            // Format output images
            const outputImages = result.images.map((img, index) => this.formatImageOutput(img, index));
                Name: 'Images',
                Value: outputImages
                Name: 'ImageCount',
                Value: result.images.length
            if (result.revisedPrompt) {
                    Name: 'RevisedPrompt',
                    Value: result.revisedPrompt
                Name: 'ModelUsed',
                Value: model.Name
            // Build response - NOTE: images data is in output params, not in Message
            // This keeps Message lightweight for LLM context (base64 images are ~700K tokens each)
                message: `Successfully generated ${result.images.length} image(s)`,
                model: model.Name,
                imageCount: result.images.length,
                // Images are available in the 'Images' output parameter
                // Use the provided placeholder references in your response
                revisedPrompt: result.revisedPrompt
                ResultCode: "IMAGES_GENERATED",
                Message: `Generate image failed: ${error instanceof Error ? error.message : String(error)}`,
     * Prepare an image generator instance using proper metadata lookup
    private async prepareImageGenerator(
        contextUser: UserInfo | undefined,
    ): Promise<{ generator: BaseImageGenerator; model: AIModelEntityExtended; apiName: string }> {
        // Ensure AIEngine is loaded
        await AIEngineBase.Instance.Config(false, contextUser);
        // Find image generator models
        const imageGeneratorModels = AIEngineBase.Instance.Models.filter(
            m => m.AIModelType?.toLowerCase() === 'image generator' && m.IsActive
        if (imageGeneratorModels.length === 0) {
            throw new Error('No active image generator models found');
        // Select model - use specified or highest power
        if (modelName) {
            const foundModel = imageGeneratorModels.find(
                m => m.Name.toLowerCase() === modelName.toLowerCase() ||
                     (m.APIName && m.APIName.toLowerCase() === modelName.toLowerCase())
            if (!foundModel) {
                throw new Error(`Image generator model '${modelName}' not found`);
            model = foundModel;
            // Get highest power image generator model
            model = imageGeneratorModels.reduce((best, current) =>
                (current.PowerRank || 0) > (best.PowerRank || 0) ? current : best
        // Find the inference provider from ModelVendors (populated by AIEngineBase)
        // Inference providers have DriverClass set
        const inferenceProvider = model.ModelVendors.find(mv =>
            mv.DriverClass && mv.DriverClass.length > 0 && mv.Status === 'Active'
        if (!inferenceProvider) {
            throw new Error(`No active inference provider found for model '${model.Name}'`);
        // Get API key using the vendor's driver class
        const driverClass = inferenceProvider.DriverClass;
        const apiName = inferenceProvider.APIName || model.APIName || model.Name;
        const apiKey = GetAIAPIKey(driverClass);
            // Try getting by vendor name as fallback
            const vendor = AIEngineBase.Instance.Vendors.find(v => v.ID === inferenceProvider.VendorID);
            const vendorApiKey = vendor ? GetAIAPIKey(vendor.Name) : null;
            if (!vendorApiKey) {
                throw new Error(`No API key found for ${driverClass} or vendor ${vendor?.Name || 'unknown'}`);
        const generator = MJGlobal.Instance.ClassFactory.CreateInstance<BaseImageGenerator>(
        if (!generator) {
            throw new Error(`Failed to create image generator instance for ${driverClass}. Ensure the provider is registered.`);
        return { generator, model, apiName };
     * Execute text-to-image generation
    private async executeImageGeneration(
        generator: BaseImageGenerator,
            apiName: string;
            numberOfImages: number;
            size: string;
            outputFormat: string;
            quality?: string;
            style?: string;
    ): Promise<ImageGenerationResult> {
        const genParams: ImageGenerationParams = {
            prompt: options.prompt,
            model: options.apiName,
            n: options.numberOfImages,
            size: options.size,
            outputFormat: options.outputFormat === 'url' ? 'url' : 'b64_json'
        if (options.quality) {
            genParams.quality = options.quality as ImageGenerationParams['quality'];
        if (options.style) {
            genParams.style = options.style as ImageGenerationParams['style'];
        if (options.negativePrompt) {
            genParams.negativePrompt = options.negativePrompt;
        return generator.GenerateImage(genParams);
     * Execute image-to-image editing
    private async executeImageEdit(
            sourceImage: string;
            mask?: string;
        const editParams: ImageEditParams = {
            image: options.sourceImage,
        if (options.mask) {
            editParams.mask = options.mask;
            editParams.negativePrompt = options.negativePrompt;
        return generator.EditImage(editParams);
     * Format a generated image for output as MediaOutput
    private formatImageOutput(img: GeneratedImage, index: number): MediaOutput {
            modality: 'Image',
            mimeType: img.format ? `image/${img.format}` : 'image/png',
            data: img.base64,
            url: img.url,
            width: img.width,
            height: img.height,
            label: `Generated image ${index + 1}`
    private getParamValue(params: RunActionParams, name: string): string | undefined {
     * Get number parameter with default
    private getNumberParam(params: RunActionParams, name: string, defaultValue: number): number {
        return isNaN(num) ? defaultValue : num;
