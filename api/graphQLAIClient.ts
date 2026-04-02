import { LogError, LogStatusEx } from "@memberjunction/core";
import { ExecuteAgentParams, ExecuteAgentResult } from "@memberjunction/ai-core-plus";
 * Client for executing AI operations through GraphQL.
 * This class provides an easy way to execute AI prompts and agents from a client application.
 * The GraphQLAIClient follows the same naming convention as other GraphQL clients
 * in the MemberJunction ecosystem, such as GraphQLActionClient and GraphQLSystemUserClient.
 * const aiClient = new GraphQLAIClient(graphQLProvider);
 * // Run an AI prompt
 * const promptResult = await aiClient.RunAIPrompt({
 *   promptId: "prompt-id",
 *   data: { context: "user data" },
 *   temperature: 0.7
 * // Run an AI agent
 * const agentResult = await aiClient.RunAIAgent({
 *   agentId: "agent-id",
 *   messages: [{ role: "user", content: "Hello" }],
 *   sessionId: "session-123"
export class GraphQLAIClient {
     * Creates a new GraphQLAIClient instance.
     * Run an AI prompt with the specified parameters.
     * This method invokes an AI prompt on the server through GraphQL and returns the result.
     * Parameters are automatically serialized as needed, and results are parsed for consumption.
     * @param params The parameters for running the AI prompt
     * @returns A Promise that resolves to a RunAIPromptResult object
     * const result = await aiClient.RunAIPrompt({
     *   data: { key: "value" },
     *   temperature: 0.7,
     *   topP: 0.9
     *   console.log('Output:', result.output);
     *   console.log('Parsed Result:', result.parsedResult);
     *   console.error('Error:', result.error);
    public async RunAIPrompt(params: RunAIPromptParams): Promise<RunAIPromptResult> {
            // Build the mutation with all possible parameters
                mutation RunAIPrompt(
                    $promptId: String!,
                    $data: String,
                    $overrideModelId: String,
                    $overrideVendorId: String,
                    $configurationId: String,
                    $skipValidation: Boolean,
                    $templateData: String,
                    $responseFormat: String,
                    $temperature: Float,
                    $topP: Float,
                    $topK: Int,
                    $minP: Float,
                    $frequencyPenalty: Float,
                    $presencePenalty: Float,
                    $seed: Int,
                    $stopSequences: [String!],
                    $includeLogProbs: Boolean,
                    $topLogProbs: Int,
                    $messages: String,
                    $rerunFromPromptRunID: String,
                    $systemPromptOverride: String
                    RunAIPrompt(
                        promptId: $promptId,
                        data: $data,
                        overrideModelId: $overrideModelId,
                        overrideVendorId: $overrideVendorId,
                        configurationId: $configurationId,
                        skipValidation: $skipValidation,
                        templateData: $templateData,
                        responseFormat: $responseFormat,
                        temperature: $temperature,
                        topP: $topP,
                        topK: $topK,
                        minP: $minP,
                        frequencyPenalty: $frequencyPenalty,
                        presencePenalty: $presencePenalty,
                        seed: $seed,
                        stopSequences: $stopSequences,
                        includeLogProbs: $includeLogProbs,
                        topLogProbs: $topLogProbs,
                        messages: $messages,
                        rerunFromPromptRunID: $rerunFromPromptRunID,
                        systemPromptOverride: $systemPromptOverride
                        parsedResult
                        promptRunId
                        rawResult
                        validationResult
                        chatResult
            // Prepare variables, serializing complex objects to JSON strings
            const variables = this.preparePromptVariables(params);
            const result = await this._dataProvider.ExecuteGQL(mutation, variables);
            // Process and return the result
            return this.processPromptResult(result);
            return this.handlePromptError(e);
     * Prepares variables for the AI prompt mutation
     * @param params The prompt parameters
     * @returns The prepared variables for GraphQL
    private preparePromptVariables(params: RunAIPromptParams): Record<string, any> {
            promptId: params.promptId
        // Serialize complex objects to JSON strings
        if (params.data !== undefined) {
            variables.data = typeof params.data === 'object' ? JSON.stringify(params.data) : params.data;
        if (params.templateData !== undefined) {
            variables.templateData = typeof params.templateData === 'object' ? JSON.stringify(params.templateData) : params.templateData;
        if (params.messages !== undefined) {
            variables.messages = JSON.stringify(params.messages);
        // Add optional scalar parameters
        if (params.overrideModelId !== undefined) variables.overrideModelId = params.overrideModelId;
        if (params.overrideVendorId !== undefined) variables.overrideVendorId = params.overrideVendorId;
        if (params.configurationId !== undefined) variables.configurationId = params.configurationId;
        if (params.skipValidation !== undefined) variables.skipValidation = params.skipValidation;
        if (params.responseFormat !== undefined) variables.responseFormat = params.responseFormat;
        if (params.temperature !== undefined) variables.temperature = params.temperature;
        if (params.topP !== undefined) variables.topP = params.topP;
        if (params.topK !== undefined) variables.topK = params.topK;
        if (params.minP !== undefined) variables.minP = params.minP;
        if (params.frequencyPenalty !== undefined) variables.frequencyPenalty = params.frequencyPenalty;
        if (params.presencePenalty !== undefined) variables.presencePenalty = params.presencePenalty;
        if (params.seed !== undefined) variables.seed = params.seed;
        if (params.stopSequences !== undefined) variables.stopSequences = params.stopSequences;
        if (params.includeLogProbs !== undefined) variables.includeLogProbs = params.includeLogProbs;
        if (params.topLogProbs !== undefined) variables.topLogProbs = params.topLogProbs;
        if (params.rerunFromPromptRunID !== undefined) variables.rerunFromPromptRunID = params.rerunFromPromptRunID;
        if (params.systemPromptOverride !== undefined) variables.systemPromptOverride = params.systemPromptOverride;
     * Processes the result from the AI prompt mutation
     * @param result The raw GraphQL result
     * @returns The processed RunAIPromptResult
    private processPromptResult(result: any): RunAIPromptResult {
        if (!result?.RunAIPrompt) {
            throw new Error("Invalid response from server");
        const promptResult = result.RunAIPrompt;
        // Parse JSON results if they exist
        let parsedResult: any;
        let validationResult: any;
        let chatResult: any;
            if (promptResult.parsedResult) {
                parsedResult = JSON.parse(promptResult.parsedResult);
            // Keep as string if parsing fails
            parsedResult = promptResult.parsedResult;
            if (promptResult.validationResult) {
                validationResult = JSON.parse(promptResult.validationResult);
            validationResult = promptResult.validationResult;
            if (promptResult.chatResult) {
                chatResult = JSON.parse(promptResult.chatResult);
            chatResult = promptResult.chatResult;
            success: promptResult.success,
            output: promptResult.output,
            parsedResult,
            error: promptResult.error,
            executionTimeMs: promptResult.executionTimeMs,
            tokensUsed: promptResult.tokensUsed,
            promptRunId: promptResult.promptRunId,
            rawResult: promptResult.rawResult,
            validationResult,
     * Handles errors in the AI prompt execution
     * @param e The error
     * @returns An error result
    private handlePromptError(e: unknown): RunAIPromptResult {
        const error = e as Error;
        LogError(`Error running AI prompt: ${error}`);
            error: error.message || 'Unknown error occurred'
     * Run an AI agent with the specified parameters.
     * This method invokes an AI agent on the server through GraphQL and returns the result.
     * The agent can maintain conversation context across multiple interactions.
     * If a progress callback is provided in params.onProgress, this method will subscribe
     * to real-time progress updates from the GraphQL server and forward them to the callback.
     * @param params The parameters for running the AI agent
     * @returns A Promise that resolves to a RunAIAgentResult object
     * const result = await aiClient.RunAIAgent({
     *   messages: [
     *     { role: "user", content: "What's the weather like?" }
     *   sessionId: "session-123",
     *   data: { location: "New York" },
     *   onProgress: (progress) => {
     *     console.log(`Progress: ${progress.message} (${progress.percentage}%)`);
     *   console.log('Response:', result.payload);
     *   console.log('Execution time:', result.executionTimeMs, 'ms');
     *   console.error('Error:', result.errorMessage);
    public async RunAIAgent(
        sourceArtifactVersionId?: string
    ): Promise<ExecuteAgentResult> {
        let subscription: any;
            // Subscribe to progress updates if callback provided
            if (params.onProgress) {
                subscription = this._dataProvider.PushStatusUpdates(this._dataProvider.sessionId)
                            LogStatusEx({message: '[GraphQLAIClient] Received statusUpdate message', verboseOnly: true, additionalArgs: [message]});
                            const parsed = JSON.parse(message);
                            LogStatusEx({message: '[GraphQLAIClient] Parsed message', verboseOnly: true, additionalArgs: [parsed]});
                            // Filter for ExecutionProgress messages from RunAIAgentResolver
                            if (parsed.resolver === 'RunAIAgentResolver' &&
                                parsed.type === 'ExecutionProgress' &&
                                parsed.status === 'ok' &&
                                parsed.data?.progress) {
                                LogStatusEx({message: '[GraphQLAIClient] Forwarding progress to callback', verboseOnly: true, additionalArgs: [parsed.data.progress]});
                                // Forward progress to callback with agentRunId in metadata
                                const progressWithRunId = {
                                    ...parsed.data.progress,
                                        ...(parsed.data.progress.metadata || {}),
                                        agentRunId: parsed.data.agentRunId
                                params.onProgress!(progressWithRunId);
                                LogStatusEx({message: '[GraphQLAIClient] Message does not match filter criteria', verboseOnly: true, additionalArgs: [{
                                    resolver: parsed.resolver,
                                    type: parsed.type,
                                    status: parsed.status,
                                    hasProgress: !!parsed.data?.progress
                                }]});
                            // Log parsing errors for debugging
                            console.error('[GraphQLAIClient] Failed to parse progress message:', e, 'Raw message:', message);
                mutation RunAIAgent(
                    $agentId: String!,
                    $messages: String!,
                    $sessionId: String!,
                    $payload: String,
                    $lastRunId: String,
                    $autoPopulateLastRunPayload: Boolean,
                    $conversationDetailId: String,
                    $createArtifacts: Boolean,
                    $createNotification: Boolean,
                    $sourceArtifactId: String,
                    $sourceArtifactVersionId: String
                    RunAIAgent(
                        agentId: $agentId,
                        sessionId: $sessionId,
                        payload: $payload,
                        lastRunId: $lastRunId,
                        autoPopulateLastRunPayload: $autoPopulateLastRunPayload,
                        conversationDetailId: $conversationDetailId,
                        createArtifacts: $createArtifacts,
                        createNotification: $createNotification,
                        sourceArtifactId: $sourceArtifactId,
                        sourceArtifactVersionId: $sourceArtifactVersionId
            const variables = this.prepareAgentVariables(params, sourceArtifactId, sourceArtifactVersionId);
            return this.processAgentResult(result.RunAIAgent?.result);
            return this.handleAgentError(e);
            // Always clean up subscription
            if (subscription) {
                subscription.unsubscribe();
     * Prepares variables for the AI agent mutation
     * @param params The agent parameters
    private prepareAgentVariables(
            messages: JSON.stringify(params.conversationMessages),
            sessionId: this._dataProvider.sessionId
        // Serialize optional complex objects to JSON strings
        if (params.payload !== undefined) {
            variables.payload = typeof params.payload === 'object' ? JSON.stringify(params.payload) : params.payload;
        if (params.lastRunId !== undefined) variables.lastRunId = params.lastRunId;
        if (params.autoPopulateLastRunPayload !== undefined) variables.autoPopulateLastRunPayload = params.autoPopulateLastRunPayload;
        if (params.conversationDetailId !== undefined) {
            variables.conversationDetailId = params.conversationDetailId;
            // When conversationDetailId is provided, enable server-side artifact and notification creation
            // This is a GraphQL resolver-level concern, not agent execution concern
            variables.createArtifacts = true;
            variables.createNotification = true;
        // Add source artifact tracking for versioning (GraphQL resolver-level concern)
        if (sourceArtifactId !== undefined) variables.sourceArtifactId = sourceArtifactId;
        if (sourceArtifactVersionId !== undefined) variables.sourceArtifactVersionId = sourceArtifactVersionId;
     * Processes the result from the AI agent mutation
     * @returns The processed RunAIAgentResult
    private processAgentResult(result: string): ExecuteAgentResult {
        return SafeJSONParse(result) as ExecuteAgentResult;        
     * Handles errors in the AI agent execution
    private handleAgentError(e: unknown): ExecuteAgentResult {
        LogError(`Error running AI agent: ${error}`);
            agentRun: undefined
     * Run an AI agent using an existing conversation detail ID.
     * This is an optimized method that loads conversation history server-side,
     * avoiding the need to send large attachment data from client to server.
     * - The user's message has already been saved as a ConversationDetail
     * - The conversation may have attachments (images, documents, etc.)
     * - You want optimal performance by loading history on the server
     * @param params The parameters for running the AI agent from conversation detail
     * @returns A Promise that resolves to an ExecuteAgentResult object
     * const result = await aiClient.RunAIAgentFromConversationDetail({
     *   conversationDetailId: "detail-id-123",
     *   maxHistoryMessages: 20,
     *   createArtifacts: true,
     *     console.log(`Progress: ${progress.message}`);
    public async RunAIAgentFromConversationDetail(
        params: RunAIAgentFromConversationDetailParams
        let subscription: ReturnType<typeof this._dataProvider.PushStatusUpdates.prototype.subscribe> | undefined;
                            console.error('[GraphQLAIClient] Failed to parse progress message:', e);
                mutation RunAIAgentFromConversationDetail(
                    $conversationDetailId: String!,
                    $maxHistoryMessages: Int,
                    RunAIAgentFromConversationDetail(
                        maxHistoryMessages: $maxHistoryMessages,
            const variables: Record<string, unknown> = {
                conversationDetailId: params.conversationDetailId,
                agentId: params.agentId,
            if (params.maxHistoryMessages !== undefined) variables.maxHistoryMessages = params.maxHistoryMessages;
            if (params.createArtifacts !== undefined) variables.createArtifacts = params.createArtifacts;
            if (params.createNotification !== undefined) variables.createNotification = params.createNotification;
            if (params.sourceArtifactId !== undefined) variables.sourceArtifactId = params.sourceArtifactId;
            if (params.sourceArtifactVersionId !== undefined) variables.sourceArtifactVersionId = params.sourceArtifactVersionId;
            return this.processAgentResult(result.RunAIAgentFromConversationDetail?.result);
     * Execute a simple prompt without requiring a stored AI Prompt entity.
     * This method is designed for interactive components that need quick AI responses.
     * @param params The parameters for the simple prompt execution
     * @returns A Promise that resolves to a SimplePromptResult object
     * const result = await aiClient.ExecuteSimplePrompt({
     *   systemPrompt: "You are a helpful assistant",
     *     { message: "What's the weather?", role: "user" }
     *   modelPower: "medium"
     *   console.log('Response:', result.result);
     *   if (result.resultObject) {
     *     console.log('Parsed JSON:', JSON.parse(result.resultObject));
    public async ExecuteSimplePrompt(params: ExecuteSimplePromptParams): Promise<SimplePromptResult> {
                mutation ExecuteSimplePrompt(
                    $systemPrompt: String!,
                    $preferredModels: [String!],
                    $modelPower: String,
                    $responseFormat: String
                    ExecuteSimplePrompt(
                        systemPrompt: $systemPrompt,
                        preferredModels: $preferredModels,
                        modelPower: $modelPower,
                        responseFormat: $responseFormat
                        resultObject
                systemPrompt: params.systemPrompt
            // Convert messages array to JSON string if provided
            if (params.messages && params.messages.length > 0) {
            if (params.preferredModels) {
                variables.preferredModels = params.preferredModels;
            if (params.modelPower) {
                variables.modelPower = params.modelPower;
                variables.responseFormat = params.responseFormat;
            if (!result?.ExecuteSimplePrompt) {
            const promptResult = result.ExecuteSimplePrompt;
            // Parse resultObject if it exists
            let resultObject: any;
            if (promptResult.resultObject) {
                    resultObject = JSON.parse(promptResult.resultObject);
                    resultObject = promptResult.resultObject;
                result: promptResult.result,
                resultObject,
                modelName: promptResult.modelName,
                executionTimeMs: promptResult.executionTimeMs
            LogError(`Error executing simple prompt: ${error}`);
                modelName: 'Unknown',
     * Generate embeddings for text using local embedding models.
     * This method is designed for interactive components that need fast similarity calculations.
     * @param params The parameters for embedding generation
     * @returns A Promise that resolves to an EmbedTextResult object
     * const result = await aiClient.EmbedText({
     *   textToEmbed: ["Hello world", "How are you?"],
     *   modelSize: "small"
     * console.log('Embeddings:', result.embeddings);
     * console.log('Model used:', result.modelName);
     * console.log('Dimensions:', result.vectorDimensions);
                mutation EmbedText(
                    $textToEmbed: [String!]!,
                    $modelSize: String!
                    EmbedText(
                        textToEmbed: $textToEmbed,
                        modelSize: $modelSize
                        embeddings
                        vectorDimensions
            // Prepare variables - handle both single string and array
            const textArray = Array.isArray(params.textToEmbed) 
                ? params.textToEmbed 
                : [params.textToEmbed];
                textToEmbed: textArray,
            if (!result?.EmbedText) {
            const embedResult = result.EmbedText;
            // Return single embedding or array based on input
            const returnEmbeddings = Array.isArray(params.textToEmbed)
                ? embedResult.embeddings
                : embedResult.embeddings[0];
                embeddings: returnEmbeddings,
                modelName: embedResult.modelName,
                vectorDimensions: embedResult.vectorDimensions,
                error: embedResult.error
            LogError(`Error generating embeddings: ${error}`);
                embeddings: Array.isArray(params.textToEmbed) ? [] : [],
                vectorDimensions: 0,
 * Parameters for executing a simple prompt
export interface ExecuteSimplePromptParams {
     * The system prompt to set context for the model
     * Optional message history
    messages?: Array<{message: string, role: 'user' | 'assistant'}>;
     * Preferred model names to use
    preferredModels?: string[];
     * Power level for model selection
    modelPower?: 'lowest' | 'medium' | 'highest';
     * Response format (e.g., "json")
    responseFormat?: string;
 * Result from executing a simple prompt
export interface SimplePromptResult {
     * The text response from the model
    result?: string;
     * Parsed JSON object if the response contained JSON
    resultObject?: any;
     * Name of the model used
 * Parameters for generating text embeddings
export interface EmbedTextParams {
     * Text or array of texts to embed
    textToEmbed: string | string[];
     * Size of the embedding model to use
    modelSize: 'small' | 'medium';
 * Result from generating text embeddings
export interface EmbedTextResult {
     * Single embedding vector or array of vectors
    embeddings: number[] | number[][];
     * Number of dimensions in each vector
    vectorDimensions: number;
     * Error message if generation failed
 * Parameters for running an AI prompt
export interface RunAIPromptParams {
     * The ID of the AI prompt to run
     * Data context to pass to the prompt (will be JSON serialized)
     * Override the default model ID
    overrideModelId?: string;
     * Override the default vendor ID
    overrideVendorId?: string;
     * Configuration ID to use
     * Skip validation of the prompt
     * Template data for prompt templating (will be JSON serialized)
     * Response format (e.g., "json", "text")
     * Temperature parameter for the model (0.0 to 1.0)
     * Top-p sampling parameter
     * Top-k sampling parameter
     * Min-p sampling parameter
     * Frequency penalty parameter
     * Presence penalty parameter
     * Random seed for reproducible outputs
     * Stop sequences for the model
    stopSequences?: string[];
     * Include log probabilities in the response
    includeLogProbs?: boolean;
     * Number of top log probabilities to include
     * Conversation messages (will be JSON serialized)
    messages?: Array<{ role: string; content: string }>;
     * ID of a previous prompt run to rerun from
     * Override the system prompt
 * Result from running an AI prompt
export interface RunAIPromptResult {
     * Whether the prompt execution was successful
     * The output from the prompt
     * Parsed result data (if applicable)
    parsedResult?: any;
     * Error message if the execution failed
     * Number of tokens used
     * ID of the prompt run record
     * Raw result from the model
     * Validation result data
     * Chat completion result data
    chatResult?: any;
 * Parameters for running an AI agent from an existing conversation detail.
 * This is the optimized method that loads conversation history server-side.
export interface RunAIAgentFromConversationDetailParams {
     * The ID of the conversation detail (user's message) to use as context
     * The ID of the AI agent to run
     * Maximum number of history messages to include (default: 20)
    maxHistoryMessages?: number;
     * Data context to pass to the agent (will be JSON serialized)
     * Payload to pass to the agent (will be JSON serialized)
    payload?: Record<string, unknown> | string;
     * ID of the last agent run for continuity
     * Whether to auto-populate payload from last run
     * Whether to create artifacts from the agent's payload
     * Whether to create a user notification on completion
    createNotification?: boolean;
     * Source artifact ID for versioning
     * Source artifact version ID for versioning
    sourceArtifactVersionId?: string;
     * Optional callback for progress updates
    onProgress?: (progress: {
