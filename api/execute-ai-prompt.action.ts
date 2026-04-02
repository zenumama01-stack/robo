import { RunView } from "@memberjunction/core";
import { AIPromptParams} from "@memberjunction/ai-core-plus";
 * Action that executes MemberJunction AI prompts
 * // Execute a simple prompt
 *   ActionName: 'Execute AI Prompt',
 *     Name: 'PromptName',
 *     Value: 'Summarize Text'
 *     Name: 'Variables',
 *       text: 'Long article text here...',
 *       maxLength: 200
 * // Execute with model override
 *     Value: 'Generate Code'
 *       language: 'TypeScript',
 *       description: 'Function to calculate fibonacci'
 *     Name: 'ModelOverride',
 *     Value: 'gpt-4'
 *     Name: 'TemperatureOverride',
 *     Value: 0.7
 * // Execute with streaming
 *     Value: 'Creative Writing'
 *     Value: { topic: 'space exploration' }
 *     Name: 'Stream',
@RegisterClass(BaseAction, "Execute AI Prompt")
export class ExecuteAIPromptAction extends BaseAction {
     * Executes a MemberJunction AI prompt
     *   - PromptName: Name of the AI prompt to execute (required)
     *   - Variables: Object with variables for the prompt (optional)
     *   - ModelOverride: Override the default model (optional)
     *   - TemperatureOverride: Override temperature setting (optional)
     *   - MaxTokensOverride: Override max tokens (optional)
     *   - Stream: Boolean - stream responses (default: false)
     *   - SystemPromptOverride: Override system prompt (optional)
     *   - IncludeMetadata: Include prompt metadata in response (default: false)
     * @returns AI prompt response
            const promptName = this.getParamValue(params, 'promptname');
            const variables = this.getParamValue(params, 'variables') || {};
            const modelOverride = this.getParamValue(params, 'modeloverride');
            const temperatureOverride = this.getParamValue(params, 'temperatureoverride');
            const maxTokensOverride = this.getParamValue(params, 'maxtokensoverride');
            const stream = this.getBooleanParam(params, 'stream', false);
            const systemPromptOverride = this.getParamValue(params, 'systempromptoverride');
            const includeMetadata = this.getBooleanParam(params, 'includemetadata', false);
            // Validate prompt name
            if (!promptName) {
                    Message: "PromptName parameter is required",
                    ResultCode: "MISSING_PROMPT_NAME"
            // Load the prompt
            const prompt = await this.loadPrompt(promptName, params.ContextUser);
                    Message: `AI Prompt '${promptName}' not found`,
                    ResultCode: "PROMPT_NOT_FOUND"
            // Check if prompt is active
                    Message: `AI Prompt '${promptName}' is not active (status: ${prompt.Status})`,
                    ResultCode: "PROMPT_NOT_ACTIVE"
            // Prepare prompt configuration
            promptParams.data = variables;
            promptParams.contextUser = params.ContextUser;
            // Apply overrides if provided
            if (modelOverride) {
                // We'll need to handle model override through additionalParameters
                promptParams.additionalParameters = promptParams.additionalParameters || {};
                promptParams.additionalParameters.model = modelOverride;
            if (temperatureOverride !== undefined && temperatureOverride !== null) {
                promptParams.additionalParameters.temperature = temperatureOverride;
            if (maxTokensOverride) {
                promptParams.additionalParameters.max_tokens = maxTokensOverride;
            if (systemPromptOverride) {
                promptParams.additionalParameters.system = systemPromptOverride;
                        Message: `Prompt execution failed: ${result.errorMessage || 'Unknown error'}`,
                        ResultCode: "PROMPT_EXECUTION_FAILED"
                    Value: result.rawResult || result.result
                    Name: 'Model',
                    Value: result.modelInfo?.modelName || result.chatResult?.data?.model || modelOverride || prompt.AIModelType
                    Name: 'TokensUsed',
                    Value: (result.promptTokens || 0) + (result.completionTokens || 0)
                        Name: 'PromptMetadata',
                            promptName: prompt.Name,
                            promptType: prompt.Type,
                            model: result.modelInfo?.modelName || result.chatResult?.data?.model || modelOverride || prompt.AIModelType,
                            temperature: temperatureOverride !== undefined ? temperatureOverride : prompt.Temperature,
                            maxTokens: maxTokensOverride || null
                // Build response message
                const responseData: any = {
                    message: "AI prompt executed successfully",
                    tokensUsed: (result.promptTokens || 0) + (result.completionTokens || 0),
                    response: result.rawResult || result.result
                    responseData.metadata = {
                        maxTokens: maxTokensOverride || null,
                        streaming: stream
                    ResultCode: "PROMPT_EXECUTED",
                    Message: JSON.stringify(responseData, null, 2)
                    Message: `Failed to execute prompt: ${error instanceof Error ? error.message : String(error)}`,
                    ResultCode: "EXECUTION_ERROR"
                Message: `Execute AI prompt failed: ${error instanceof Error ? error.message : String(error)}`,
                ResultCode: "ACTION_FAILED"
     * Load AI prompt by name
    private async loadPrompt(promptName: string, contextUser?: any): Promise<AIPromptEntityExtended | null> {
        const result = await rv.RunView<AIPromptEntityExtended>({
            ExtraFilter: `Name = '${promptName.replace(/'/g, "''")}'`,
     * Get boolean parameter with default
    private getBooleanParam(params: RunActionParams, name: string, defaultValue: boolean): boolean {
        const value = this.getParamValue(params, name);
        if (value === undefined || value === null) return defaultValue;
            return value.toLowerCase() === 'true';
     * Get parameter value by name (case-insensitive)
    private getParamValue(params: RunActionParams, name: string): any {
        const param = params.Params.find(p => p.Name.toLowerCase() === name.toLowerCase());
