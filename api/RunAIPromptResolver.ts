import { Resolver, Mutation, Query, Arg, Ctx, ObjectType, Field, Int } from 'type-graphql';
import { DatabaseProviderBase, LogError, LogStatus, Metadata } from '@memberjunction/core';
import { AIPromptEntityExtended, AIModelEntityExtended } from '@memberjunction/ai-core-plus';
import { ChatParams, ChatMessage, ChatMessageRole, GetAIAPIKey, BaseLLM } from '@memberjunction/ai';
export class AIPromptRunResult {
    parsedResult?: string;
    validationResult?: string;
    chatResult?: string;
export class SimplePromptResult {
    resultObject?: string; // JSON stringified object
export class EmbedTextResult {
    @Field(() => [[Number]])
    embeddings: number[][];
export class RunAIPromptResolver extends ResolverBase {
     * Internal method that handles the core AI prompt execution logic.
    private async executeAIPrompt(
        overrideModelId?: string,
        overrideVendorId?: string,
        skipValidation?: boolean,
        responseFormat?: string,
        topK?: number,
        minP?: number,
        frequencyPenalty?: number,
        presencePenalty?: number,
        seed?: number,
        stopSequences?: string[],
        includeLogProbs?: boolean,
        topLogProbs?: number,
        messages?: string,
        rerunFromPromptRunID?: string,
        systemPromptOverride?: string
    ): Promise<AIPromptRunResult> {
            LogStatus(`=== RUNNING AI PROMPT FOR ID: ${promptId} ===`);
            // Parse data contexts (JSON strings)
            let parsedData = {};
            let parsedTemplateData = {};
                    parsedData = JSON.parse(data);
                        error: `Invalid JSON in data: ${(parseError as Error).message}`,
            if (templateData) {
                    parsedTemplateData = JSON.parse(templateData);
                        error: `Invalid JSON in template data: ${(parseError as Error).message}`,
            // Get current user from payload
                    error: 'Unable to determine current user',
            // Load the AI prompt entity
            const promptEntity = await p.GetEntityObject<AIPromptEntityExtended>('MJ: AI Prompts', currentUser);
            await promptEntity.Load(promptId);
            if (!promptEntity.IsSaved) {
                    error: `AI Prompt with ID ${promptId} not found`,
            if (promptEntity.Status !== 'Active') {
                    error: `AI Prompt "${promptEntity.Name}" is not active (Status: ${promptEntity.Status})`,
            // Create AI prompt runner and execute
            // Build execution parameters
            promptParams.prompt = promptEntity;
            promptParams.data = parsedData;
            promptParams.templateData = parsedTemplateData;
            promptParams.configurationId = configurationId;
            promptParams.contextUser = currentUser;
            promptParams.skipValidation = skipValidation || false;
            promptParams.rerunFromPromptRunID = rerunFromPromptRunID;
            promptParams.systemPromptOverride = systemPromptOverride;
            // Set override if model or vendor ID provided
            if (overrideModelId || overrideVendorId) {
                promptParams.override = {
                    modelId: overrideModelId,
                    vendorId: overrideVendorId
            // Parse and set conversation messages if provided
            if (messages) {
                    promptParams.conversationMessages = JSON.parse(messages);
                    // If parsing fails, treat as a simple user message
                    promptParams.conversationMessages = [{
                        content: messages
            // If responseFormat is provided, override the prompt's default response format
            if (responseFormat) {
                // We'll need to override the prompt's response format setting
                // This will be handled in the AIPromptRunner when it builds the ChatParams
                promptEntity.ResponseFormat = responseFormat as any;
            // Build additional parameters for chat-specific settings
            const additionalParams: Record<string, any> = {};
            if (temperature != null) additionalParams.temperature = temperature;
            if (topP != null) additionalParams.topP = topP;
            if (topK != null) additionalParams.topK = topK;
            if (minP != null) additionalParams.minP = minP;
            if (frequencyPenalty != null) additionalParams.frequencyPenalty = frequencyPenalty;
            if (presencePenalty != null) additionalParams.presencePenalty = presencePenalty;
            if (seed != null) additionalParams.seed = seed;
            if (stopSequences != null) additionalParams.stopSequences = stopSequences;
            if (includeLogProbs != null) additionalParams.includeLogProbs = includeLogProbs;
            if (topLogProbs != null) additionalParams.topLogProbs = topLogProbs;
            // Only set additionalParameters if we have any
            if (Object.keys(additionalParams).length > 0) {
                promptParams.additionalParameters = additionalParams;
            const result = await promptRunner.ExecutePrompt(promptParams);
                LogStatus(`=== AI PROMPT RUN COMPLETED FOR: ${promptEntity.Name} (${executionTime}ms) ===`);
                    output: result.rawResult,
                    parsedResult: typeof result.result === 'string' ? result.result : JSON.stringify(result.result),
                    promptRunId: result.promptRun?.ID,
                    validationResult: result.validationResult ? JSON.stringify(result.validationResult) : undefined,
                    chatResult: result.chatResult ? JSON.stringify(result.chatResult) : undefined
                LogError(`AI Prompt run failed for ${promptEntity.Name}: ${result.errorMessage}`);
                    error: result.errorMessage,
            LogError(`AI Prompt run failed:`, undefined, error);
                error: (error as Error).message || 'Unknown error occurred',
     * Public mutation for regular users to run AI prompts with authentication.
    @Mutation(() => AIPromptRunResult)
    async RunAIPrompt(
        @Arg('promptId') promptId: string,
        @Ctx() { userPayload, providers }: AppContext,
        @Arg('overrideModelId', { nullable: true }) overrideModelId?: string,
        @Arg('overrideVendorId', { nullable: true }) overrideVendorId?: string,
        @Arg('skipValidation', { nullable: true }) skipValidation?: boolean,
        @Arg('responseFormat', { nullable: true }) responseFormat?: string,
        @Arg('temperature', { nullable: true }) temperature?: number,
        @Arg('topP', { nullable: true }) topP?: number,
        @Arg('topK', () => Int, { nullable: true }) topK?: number,
        @Arg('minP', { nullable: true }) minP?: number,
        @Arg('frequencyPenalty', { nullable: true }) frequencyPenalty?: number,
        @Arg('presencePenalty', { nullable: true }) presencePenalty?: number,
        @Arg('seed', () => Int, { nullable: true }) seed?: number,
        @Arg('stopSequences', () => [String], { nullable: true }) stopSequences?: string[],
        @Arg('includeLogProbs', { nullable: true }) includeLogProbs?: boolean,
        @Arg('topLogProbs', () => Int, { nullable: true }) topLogProbs?: number,
        @Arg('messages', { nullable: true }) messages?: string,
        @Arg('rerunFromPromptRunID', { nullable: true }) rerunFromPromptRunID?: string,
        @Arg('systemPromptOverride', { nullable: true }) systemPromptOverride?: string
        // Check API key scope authorization for prompt execution
        await this.CheckAPIKeyScopeAuthorization('prompt:execute', promptId, userPayload);
        return this.executeAIPrompt(
            overrideModelId,
            overrideVendorId,
            responseFormat,
            minP,
            frequencyPenalty,
            presencePenalty,
            seed,
            stopSequences,
            includeLogProbs,
            topLogProbs,
            rerunFromPromptRunID,
            systemPromptOverride
     * System user query for running AI prompts with elevated privileges.
    @Query(() => AIPromptRunResult)
    async RunAIPromptSystemUser(
     * Helper method to select a model for simple prompt execution based on preferences or power level
    private async selectModelForSimplePrompt(
        preferredModels: string[] | undefined,
        modelPower: string,
    ): Promise<AIModelEntityExtended> {
        // Ensure AI Engine is configured
        // Get all LLM models that have API keys
        const allModels = AIEngine.Instance.Models.filter(m => 
            m.AIModelType?.trim().toLowerCase() === 'llm' &&
            m.IsActive === true
        // Filter to only models with valid API keys
        const modelsWithKeys: AIModelEntityExtended[] = [];
        for (const model of allModels) {
            const apiKey = GetAIAPIKey(model.DriverClass);
                modelsWithKeys.push(model);
        if (modelsWithKeys.length === 0) {
            throw new Error('No AI models with valid API keys found');
        // Try preferred models first if provided
        if (preferredModels && preferredModels.length > 0) {
            for (const preferred of preferredModels) {
                const model = modelsWithKeys.find(m => 
                    m.Name === preferred || 
                    m.APIName === preferred
                    LogStatus(`Selected preferred model: ${model.Name}`);
            LogStatus('No preferred models available, falling back to power selection');
        // Sort by PowerRank for power-based selection
        modelsWithKeys.sort((a, b) => (b.PowerRank || 0) - (a.PowerRank || 0));
        let selectedModel: AIModelEntityExtended;
        switch (modelPower) {
            case 'lowest':
                selectedModel = modelsWithKeys[modelsWithKeys.length - 1];
            case 'highest':
                selectedModel = modelsWithKeys[0];
                const midIndex = Math.floor(modelsWithKeys.length / 2);
                selectedModel = modelsWithKeys[midIndex];
        LogStatus(`Selected model by power (${modelPower || 'medium'}): ${selectedModel.Name}`);
        return selectedModel;
     * Helper method to select an embedding model by size
    private selectEmbeddingModelBySize(modelSize: string): AIModelEntityExtended {
        const localModels = AIEngine.Instance.LocalEmbeddingModels;
        if (!localModels || localModels.length === 0) {
            throw new Error('No local embedding models available');
        // Models are already sorted by PowerRank (highest first)
        switch (modelSize) {
            case 'small':
                return localModels[localModels.length - 1]; // Lowest power
                const midIndex = Math.floor(localModels.length / 2);
                return localModels[midIndex] || localModels[0];
     * Helper method to build chat messages from system prompt and optional message history
    private buildChatMessages(systemPrompt: string, messagesJson?: string): ChatMessage[] {
        // Add system prompt
        if (systemPrompt && systemPrompt.trim().length > 0) {
                content: systemPrompt
        // Add message history if provided
        if (messagesJson) {
                const parsedMessages = JSON.parse(messagesJson);
                if (Array.isArray(parsedMessages)) {
                    for (const msg of parsedMessages) {
                        if (msg.message && msg.role) {
                                role: msg.role === 'user' ? ChatMessageRole.user : ChatMessageRole.assistant,
                                content: msg.message
                else if (messagesJson?.length > 0) {
                    // messages maybe just has a simple string in it so add
                    // as a single message
                        content: messagesJson
                if (messagesJson?.length > 0) {
                LogError('Failed to parse messages JSON', undefined, e);
     * Helper method to format simple prompt result
    private formatSimpleResult(chatResult: any, model: AIModelEntityExtended, executionTime: number): SimplePromptResult {
        if (!chatResult || !chatResult.success) {
                error: chatResult?.errorMessage || 'Unknown error occurred',
        const resultContent = chatResult.data?.choices?.[0]?.message?.content || '';
        // Try to extract JSON from the result
        let resultObject: any = null;
            // First try to parse the entire result as JSON
            resultObject = JSON.parse(resultContent);
            // Try to find JSON within the text
            const jsonMatch = resultContent.match(/\{[\s\S]*\}|\[[\s\S]*\]/);
                    resultObject = JSON.parse(jsonMatch[0]);
                    // No valid JSON found
            resultObject: resultObject ? JSON.stringify(resultObject) : undefined,
     * This is designed for interactive components that need quick AI responses.
    @Mutation(() => SimplePromptResult)
    async ExecuteSimplePrompt(
        @Arg('systemPrompt') systemPrompt: string,
        @Ctx() { userPayload }: { userPayload: UserPayload },
        @Arg('preferredModels', () => [String], { nullable: true }) preferredModels?: string[],
        @Arg('modelPower', { nullable: true }) modelPower?: string,
        @Arg('responseFormat', { nullable: true }) responseFormat?: string
    ): Promise<SimplePromptResult> {
        // Check API key scope authorization for simple prompt execution
        await this.CheckAPIKeyScopeAuthorization('prompt:execute', '*', userPayload);
            LogStatus(`=== EXECUTING SIMPLE PROMPT ===`);
            // Select model based on preferences or power level
            const model = await this.selectModelForSimplePrompt(
                preferredModels,
                modelPower || 'medium',
            // Build chat messages
            const chatMessages = this.buildChatMessages(systemPrompt, messages);
            if (chatMessages.length === 0) {
                    error: 'No messages to send to model',
            // Create LLM instance
                    error: `Failed to create LLM instance for model ${model.Name}`,
            // Build chat parameters
            const chatParams = new ChatParams();
            chatParams.messages = chatMessages;
            chatParams.model = model.APIName;
                // Cast to valid response format type
                chatParams.responseFormat = responseFormat as 'Any' | 'Text' | 'Markdown' | 'JSON' | 'ModelSpecific';
            // Execute the chat completion
            const result = await llm.ChatCompletion(chatParams);
            LogStatus(`=== SIMPLE PROMPT COMPLETED (${executionTime}ms) ===`);
            // Format and return the result
            return this.formatSimpleResult(result, model, executionTime);
            LogError('Simple prompt execution failed:', undefined, error);
     * System user query for executing simple prompts with elevated privileges
    @Query(() => SimplePromptResult)
    async ExecuteSimplePromptSystemUser(
        // Reuse the same logic as the regular mutation
        return this.ExecuteSimplePrompt(systemPrompt, { userPayload }, messages, preferredModels, modelPower, responseFormat);
     * Designed for interactive components that need fast similarity calculations.
    @Mutation(() => EmbedTextResult)
    async EmbedText(
        @Arg('textToEmbed', () => [String]) textToEmbed: string[],
        @Arg('modelSize') modelSize: string,
        @Ctx() { userPayload }: { userPayload: UserPayload }
    ): Promise<EmbedTextResult> {
        // Check API key scope authorization for embedding generation
        await this.CheckAPIKeyScopeAuthorization('embedding:generate', '*', userPayload);
            LogStatus(`=== GENERATING EMBEDDINGS for ${textToEmbed.length} text(s) ===`);
                    embeddings: [],
                    error: 'Unable to determine current user'
            // Select embedding model by size
            const model = this.selectEmbeddingModelBySize(modelSize);
            LogStatus(`Using embedding model: ${model.Name}`);
            // Process embeddings
            for (const text of textToEmbed) {
                if (!text || text.trim().length === 0) {
                    // Return zero vector for empty text
                    embeddings.push([]);
                // Use AIEngine's EmbedText method
                const result = await AIEngine.Instance.EmbedText(model, text);
                if (result && result.vector && result.vector.length > 0) {
                    LogError(`Failed to generate embedding for text: ${text.substring(0, 50)}...`);
                    embeddings.push([]); // Add empty array for failed embeddings
            // Get vector dimensions from first successful embedding
            const vectorDimensions = embeddings.find(e => e.length > 0)?.length || 0;
            LogStatus(`=== EMBEDDINGS GENERATED: ${embeddings.length} vectors of ${vectorDimensions} dimensions ===`);
                embeddings,
                vectorDimensions,
                error: undefined
            LogError('Embedding generation failed:', undefined, error);
     * System user query for generating embeddings with elevated privileges
    @Query(() => EmbedTextResult)
    async EmbedTextSystemUser(
        return this.EmbedText(textToEmbed, modelSize, { userPayload });
