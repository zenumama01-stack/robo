import { getApiIntegrationsConfig } from "../../config";
 * Action that generates presentations using Gamma's Generations API
 * Creates presentations, documents, or social media posts from text input
 * // Generate a basic presentation
 *   ActionName: 'Gamma Generate Presentation',
 *     Name: 'InputText',
 *     Value: 'Create a presentation about climate change impacts and solutions'
 * // Generate with specific format and options
 *     Value: 'Quarterly sales report with revenue trends and projections'
 *     Value: 'presentation'
 *     Name: 'TextMode',
 *     Value: 'generate'
 *     Name: 'NumCards',
 *     Name: 'ThemeName',
 *     Value: 'Modern'
 * // Generate and export as PDF
 *     Value: 'Product launch announcement'
 *     Name: 'ExportAs',
 *     Value: 'pdf'
@RegisterClass(BaseAction, "Gamma Generate Presentation")
export class GammaGeneratePresentationAction extends BaseAction {
     * Executes the Gamma presentation generation
     *   - InputText: Text to generate content from (required, 1-750,000 characters)
     *   - Format: Output format - 'presentation' (default), 'document', 'social'
     *   - TextMode: How to process text - 'generate' (default), 'condense', 'preserve'
     *   - ThemeName: Visual theme name (optional)
     *   - NumCards: Number of slides/cards (1-75, depends on tier)
     *   - CardSplit: How to split cards - 'auto' (default) or 'inputTextBreaks'
     *   - AdditionalInstructions: Custom guidance for content generation (optional)
     *   - ExportAs: Export format - 'pdf' or 'pptx' (optional)
     *   - TextAmount: Amount of text - 'auto', 'minimal', 'moderate', 'detailed'
     *   - TextTone: Tone of text - 'auto', 'professional', 'casual', 'educational'
     *   - TextAudience: Target audience description (optional)
     *   - TextLanguage: Language for content (e.g., 'en', 'es', 'fr')
     *   - ImageSource: Image source - 'auto', 'web', 'ai', 'none'
     *   - ImageModel: AI image model - 'dall-e-3', 'sd-3-large', 'firefly-3'
     *   - ImageStyle: AI image style - 'auto', 'photographic', 'digital-art', 'illustration'
     *   - PollStatus: Poll for completion status (default: true)
     *   - PollInterval: Seconds between status checks (default: 5)
     *   - MaxPollTime: Maximum seconds to poll (default: 300)
     * @returns Generation results with Gamma URL and status
            const inputText = this.getStringParam(params, 'inputtext');
            if (!inputText) {
                return this.createErrorResult("InputText parameter is required", "MISSING_INPUT_TEXT");
            if (inputText.length < 1 || inputText.length > 750000) {
                    "InputText must be between 1 and 750,000 characters",
                    "INVALID_INPUT_TEXT_LENGTH"
            // Get API key from config
            const apiConfig = getApiIntegrationsConfig();
            const apiKey = apiConfig.gammaApiKey;
                    "Gamma API key not found. Set gammaApiKey in mj.config.cjs or GAMMA_API_KEY environment variable",
                    "MISSING_API_KEY"
            // Get optional parameters
            const format = this.getStringParam(params, 'format') || 'presentation';
            const textMode = this.getStringParam(params, 'textmode') || 'generate';
            const themeName = this.getStringParam(params, 'themename');
            const numCards = this.getNumericParam(params, 'numcards');
            const cardSplit = this.getStringParam(params, 'cardsplit') || 'auto';
            const additionalInstructions = this.getStringParam(params, 'additionalinstructions');
            const exportAs = this.getStringParam(params, 'exportas');
            // Text options
            const textAmount = this.getStringParam(params, 'textamount');
            const textTone = this.getStringParam(params, 'texttone');
            const textAudience = this.getStringParam(params, 'textaudience');
            const textLanguage = this.getStringParam(params, 'textlanguage');
            // Image options
            const imageSource = this.getStringParam(params, 'imagesource');
            const imageModel = this.getStringParam(params, 'imagemodel');
            const imageStyle = this.getStringParam(params, 'imagestyle');
            // Polling options
            const pollStatus = this.getBooleanParam(params, 'pollstatus', true);
            const pollInterval = this.getNumericParam(params, 'pollinterval', 5);
            const maxPollTime = this.getNumericParam(params, 'maxpolltime', 300);
                inputText,
                textMode,
                cardSplit
            // Add optional top-level parameters
            if (themeName) requestBody.themeName = themeName;
            if (numCards) requestBody.numCards = numCards;
            if (additionalInstructions) requestBody.additionalInstructions = additionalInstructions;
            if (exportAs) requestBody.exportAs = exportAs;
            // Build text options
            const textOptions: Record<string, string> = {};
            if (textAmount) textOptions.amount = textAmount;
            if (textTone) textOptions.tone = textTone;
            if (textAudience) textOptions.audience = textAudience;
            if (textLanguage) textOptions.language = textLanguage;
            if (Object.keys(textOptions).length > 0) {
                requestBody.textOptions = textOptions;
            // Build image options
            const imageOptions: Record<string, string> = {};
            if (imageSource) imageOptions.source = imageSource;
            if (imageModel) imageOptions.model = imageModel;
            if (imageStyle) imageOptions.style = imageStyle;
            if (Object.keys(imageOptions).length > 0) {
                requestBody.imageOptions = imageOptions;
            // Make API request to start generation
                'https://public-api.gamma.app/v0.2/generations',
                        'X-API-KEY': apiKey,
                    timeout: 60000 // 60 second timeout
            if (!response.data || !response.data.generationId) {
                return this.createErrorResult("Invalid response from Gamma API", "INVALID_RESPONSE");
            const generationId = response.data.generationId;
            this.addOutputParam(params, 'GenerationId', generationId);
            // If polling is disabled, return immediately
            if (!pollStatus) {
                    ResultCode: "GENERATION_STARTED",
                        generationId,
                        message: 'Generation started. Use the generationId to check status.'
            // Poll for completion
            const pollIntervalMs = pollInterval * 1000;
            const maxPollTimeMs = maxPollTime * 1000;
            while (Date.now() - startTime < maxPollTimeMs) {
                await this.sleep(pollIntervalMs);
                const statusResponse = await axios.get(
                    `https://public-api.gamma.app/v0.2/generations/${generationId}`,
                            'X-API-KEY': apiKey
                const statusData = statusResponse.data;
                if (statusData.status === 'completed') {
                    this.addOutputParam(params, 'GammaUrl', statusData.gammaUrl);
                    this.addOutputParam(params, 'Status', 'completed');
                    this.addOutputParam(params, 'Credits', statusData.credits);
                            status: 'completed',
                            gammaUrl: statusData.gammaUrl,
                            credits: statusData.credits
                } else if (statusData.status === 'failed') {
                        `Generation failed: ${statusData.error || 'Unknown error'}`,
                        "GENERATION_FAILED"
                // Status is still 'pending', continue polling
            // Timeout reached
                `Generation timed out after ${maxPollTime} seconds. Use GenerationId to check status later.`,
                "GENERATION_TIMEOUT"
                const status = error.response?.status;
                const errorData = error.response?.data;
                        "Invalid Gamma API key. Verify your API key is correct and has proper permissions.",
                        "INVALID_API_KEY"
                if (status === 429) {
                        "Gamma API rate limit exceeded (50 presentations per day during beta)",
                        "RATE_LIMITED"
                if (status === 400) {
                        `Invalid request parameters: ${errorData?.error || error.message}`,
                        "INVALID_PARAMETERS"
                    `Gamma API error: ${errorData?.error || error.message}`,
                    "API_ERROR"
                `Failed to generate presentation: ${error instanceof Error ? error.message : String(error)}`,
     * Sleep helper for polling
     * Get string parameter value
        if (param?.Value === undefined || param?.Value === null) return undefined;
        return String(param.Value);
    private getBooleanParam(params: RunActionParams, paramName: string, defaultValue: boolean = false): boolean {
        if (param?.Value === undefined || param?.Value === null) return defaultValue;
        return String(param.Value).toLowerCase() === 'true';
     * Get numeric parameter value with optional default
    private getNumericParam(params: RunActionParams, paramName: string, defaultValue?: number): number | undefined {
     * Add output parameter
     * Create error result
