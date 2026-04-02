import { BaseLLM, BaseModel, BaseResult, ChatParams, ChatMessage, ChatMessageRole,
         ParallelChatCompletionsCallbacks, GetAIAPIKey,
         EmbedTextResult,
         EmbedTextParams,
         BaseEmbeddings} from "@memberjunction/ai";
import { SummarizeResult } from "@memberjunction/ai";
import { ClassifyResult } from "@memberjunction/ai";
import { ChatResult } from "@memberjunction/ai";
import { BaseEntity, LogError, Metadata, UserInfo, IMetadataProvider } from "@memberjunction/core";
import { BaseSingleton, MJGlobal } from "@memberjunction/global";
import { MJAIActionEntity, MJActionEntity,
         MJAIAgentActionEntity, MJAIAgentNoteEntity, MJAIAgentNoteTypeEntity,
         MJAIModelActionEntity, MJAIPromptModelEntity, MJAIPromptTypeEntity,
         MJAIResultCacheEntity, MJAIVendorTypeDefinitionEntity, MJArtifactTypeEntity,
         MJEntityAIActionEntity, MJVectorDatabaseEntity, MJAIAgentPromptEntity,
         MJAIAgentTypeEntity, MJAIVendorEntity, MJAIModelVendorEntity, MJAIModelTypeEntity,
         MJAIModelCostEntity, MJAIModelPriceTypeEntity, MJAIModelPriceUnitTypeEntity,
         MJAIConfigurationEntity, MJAIConfigurationParamEntity, MJAIAgentStepEntity,
         MJAIAgentStepPathEntity, MJAIAgentRelationshipEntity, MJAIAgentPermissionEntity,
         MJAIAgentDataSourceEntity, MJAIAgentConfigurationEntity, MJAIAgentExampleEntity,
         MJAICredentialBindingEntity, MJAIModalityEntity, MJAIAgentModalityEntity,
         MJAIModelModalityEntity } from "@memberjunction/core-entities";
import { AIEngineBase } from "@memberjunction/ai-engine-base";
import { SimpleVectorService } from "@memberjunction/ai-vectors-memory";
import { AgentEmbeddingService } from "./services/AgentEmbeddingService";
import { ActionEmbeddingService } from "./services/ActionEmbeddingService";
import { AgentEmbeddingMetadata, AgentMatchResult } from "./types/AgentMatchResult";
import { ActionEmbeddingMetadata, ActionMatchResult } from "./types/ActionMatchResult";
import { NoteEmbeddingMetadata, NoteMatchResult } from "./types/NoteMatchResult";
import { ExampleEmbeddingMetadata, ExampleMatchResult } from "./types/ExampleMatchResult";
import { ActionEngineBase } from "@memberjunction/actions-base";
import { AIAgentEntityExtended, AIModelEntityExtended, AIPromptEntityExtended, AIPromptCategoryEntityExtended } from "@memberjunction/ai-core-plus";
import { EffectiveAgentPermissions } from "@memberjunction/ai-engine-base";
 * @deprecated AI Actions are deprecated. Use AIPromptRunner with the new AI Prompt system instead.
export class AIActionParams {
    actionId: string
    modelId: string
    modelName?: string
    systemPrompt?: string
    userPrompt?: string
 * @deprecated Entity AI Actions are deprecated. Use AIPromptRunner with the new AI Prompt system instead.
export class EntityAIActionParams extends AIActionParams {
    entityAIActionId: string
    entityRecord: BaseEntity
 * Server-side AI Engine that wraps AIEngineBase and adds server-only capabilities.
 * This class uses composition (containment) rather than inheritance to avoid duplicate
 * data loading. It delegates all base functionality to AIEngineBase.Instance while
 * adding server-specific features like embeddings, vector search, and LLM execution.
 * @description ONLY USE ON SERVER-SIDE. For metadata only, use the AIEngineBase class which can be used anywhere.
export class AIEngine extends BaseSingleton<AIEngine> {
    public readonly EmbeddingModelTypeName: string = 'Embeddings';
    public readonly LocalEmbeddingModelVendorName: string = 'LocalEmbeddings';
    // Vector service for agent embeddings - initialized during Config
    private _agentVectorService: SimpleVectorService<AgentEmbeddingMetadata> | null = null;
    // Vector service for action embeddings - initialized during Config
    private _actionVectorService: SimpleVectorService<ActionEmbeddingMetadata> | null = null;
    // Vector service for note embeddings - initialized during Config
    private _noteVectorService: SimpleVectorService<NoteEmbeddingMetadata> | null = null;
    // Vector service for example embeddings - initialized during Config
    private _exampleVectorService: SimpleVectorService<ExampleEmbeddingMetadata> | null = null;
    // Actions loaded from database
    private _actions: MJActionEntity[] = [];
    // Embedding caches to track which items have embeddings generated
    private _agentEmbeddingsCache: Map<string, boolean> = new Map();
    private _actionEmbeddingsCache: Map<string, boolean> = new Map();
    private _embeddingsGenerated: boolean = false;
    // Loading state management
    private _loaded: boolean = false;
    private _loading: boolean = false;
    private _loadingPromise: Promise<void> | null = null;
    private _contextUser: UserInfo | undefined;
    public static get Instance(): AIEngine {
        return super.getInstance<AIEngine>();
    // ========================================================================
    // Delegated Properties from AIEngineBase
    // All base metadata is accessed through AIEngineBase.Instance
    /** Access to the underlying AIEngineBase instance */
    protected get Base(): AIEngineBase {
        return AIEngineBase.Instance;
    /** Returns true if both the base engine and server capabilities are loaded */
    public get Loaded(): boolean {
        return this._loaded && this.Base.Loaded;
    // Delegate all AIEngineBase public getters
    public get Agents(): AIAgentEntityExtended[] { return this.Base.Agents; }
    public get AgentRelationships(): MJAIAgentRelationshipEntity[] { return this.Base.AgentRelationships; }
    public get AgentTypes(): MJAIAgentTypeEntity[] { return this.Base.AgentTypes; }
    public get AgentActions(): MJAIAgentActionEntity[] { return this.Base.AgentActions; }
    public get AgentPrompts(): MJAIAgentPromptEntity[] { return this.Base.AgentPrompts; }
    public get AgentConfigurations(): MJAIAgentConfigurationEntity[] { return this.Base.AgentConfigurations; }
    public get AgentNoteTypes(): MJAIAgentNoteTypeEntity[] { return this.Base.AgentNoteTypes; }
    public get AgentPermissions(): MJAIAgentPermissionEntity[] { return this.Base.AgentPermissions; }
    public get AgentNotes(): MJAIAgentNoteEntity[] { return this.Base.AgentNotes; }
    public get AgentExamples(): MJAIAgentExampleEntity[] { return this.Base.AgentExamples; }
    public get VendorTypeDefinitions(): MJAIVendorTypeDefinitionEntity[] { return this.Base.VendorTypeDefinitions; }
    public get Vendors(): MJAIVendorEntity[] { return this.Base.Vendors; }
    public get ModelVendors(): MJAIModelVendorEntity[] { return this.Base.ModelVendors; }
    public get CredentialBindings(): MJAICredentialBindingEntity[] { return this.Base.CredentialBindings; }
        return this.Base.GetCredentialBindingsForTarget(bindingType, targetId);
        return this.Base.HasCredentialBindings(bindingType, targetId);
    public get ModelTypes(): MJAIModelTypeEntity[] { return this.Base.ModelTypes; }
    public get Prompts(): AIPromptEntityExtended[] { return this.Base.Prompts; }
    public get PromptModels(): MJAIPromptModelEntity[] { return this.Base.PromptModels; }
    public get PromptTypes(): MJAIPromptTypeEntity[] { return this.Base.PromptTypes; }
    public get PromptCategories(): AIPromptCategoryEntityExtended[] { return this.Base.PromptCategories; }
    public get Models(): AIModelEntityExtended[] { return this.Base.Models; }
    public get ArtifactTypes(): MJArtifactTypeEntity[] { return this.Base.ArtifactTypes; }
    public get LanguageModels(): AIModelEntityExtended[] { return this.Base.LanguageModels; }
    public get VectorDatabases(): MJVectorDatabaseEntity[] { return this.Base.VectorDatabases; }
    public get ModelCosts(): MJAIModelCostEntity[] { return this.Base.ModelCosts; }
    public get ModelPriceTypes(): MJAIModelPriceTypeEntity[] { return this.Base.ModelPriceTypes; }
    public get ModelPriceUnitTypes(): MJAIModelPriceUnitTypeEntity[] { return this.Base.ModelPriceUnitTypes; }
    public get Configurations(): MJAIConfigurationEntity[] { return this.Base.Configurations; }
    public get ConfigurationParams(): MJAIConfigurationParamEntity[] { return this.Base.ConfigurationParams; }
    public get AgentDataSources(): MJAIAgentDataSourceEntity[] { return this.Base.AgentDataSources; }
    public get AgentSteps(): MJAIAgentStepEntity[] { return this.Base.AgentSteps; }
    public get AgentStepPaths(): MJAIAgentStepPathEntity[] { return this.Base.AgentStepPaths; }
    public get ModelActions(): MJAIModelActionEntity[] { return this.Base.ModelActions; }
    /** @deprecated Use the new Action system instead */
    public get Actions(): MJAIActionEntity[] { return this.Base.Actions; }
    public get EntityAIActions(): MJEntityAIActionEntity[] { return this.Base.EntityAIActions; }
    // Modality getters - delegated from AIEngineBase
    public get Modalities(): MJAIModalityEntity[] { return this.Base.Modalities; }
    public get AgentModalities(): MJAIAgentModalityEntity[] { return this.Base.AgentModalities; }
    public get ModelModalities(): MJAIModelModalityEntity[] { return this.Base.ModelModalities; }
    // Modality helper methods - delegated from AIEngineBase
        return this.Base.GetModalityByName(name);
    public GetAgentModalitiesByDirection(agentId: string, direction: 'Input' | 'Output'): MJAIModalityEntity[] {
        return this.Base.GetAgentModalities(agentId, direction);
    public GetModelModalitiesByDirection(modelId: string, direction: 'Input' | 'Output'): MJAIModalityEntity[] {
        return this.Base.GetModelModalities(modelId, direction);
        return this.Base.AgentSupportsModality(agentId, modalityName, direction);
        return this.Base.ModelSupportsModality(modelId, modalityName, direction);
        return this.Base.AgentSupportsAttachments(agentId);
        return this.Base.GetAgentSupportedInputModalities(agentId);
    // Delegate AIEngineBase public methods
        return this.Base.GetHighestPowerModel(vendorName, modelType, contextUser);
        return this.Base.GetHighestPowerLLM(vendorName, contextUser);
        return this.Base.GetActiveModelCost(modelID, vendorID, processingType);
        return this.Base.GetSubAgents(agentID, status, relationshipStatus);
        return this.Base.GetAgentByName(agentName);
        return this.Base.GetAgentConfigurationPresets(agentId, activeOnly);
        return this.Base.GetDefaultAgentConfigurationPreset(agentId);
        return this.Base.GetAgentConfigurationPresetByName(agentId, presetName);
        return this.Base.AgenteNoteTypeIDByName(agentNoteTypeName);
        return this.Base.GetConfigurationParams(configurationId);
        return this.Base.GetConfigurationParam(configurationId, paramName);
     * Delegates to AIEngineBase.GetConfigurationChain.
     * @returns Array of MJAIConfigurationEntity objects representing the inheritance chain
        return this.Base.GetConfigurationChain(configurationId);
     * parameters from parent configurations. Child parameters override parent parameters.
     * Delegates to AIEngineBase.GetConfigurationParamsWithInheritance.
     * @returns Array of MJAIConfigurationParamEntity objects, with child overrides applied
        return this.Base.GetConfigurationParamsWithInheritance(configurationId);
        return this.Base.GetAgentSteps(agentId, status);
        return this.Base.GetAgentStepByID(stepId);
        return this.Base.GetPathsFromStep(stepId);
        return this.Base.CheckResultCache(prompt);
        return this.Base.CacheResult(model, prompt, promptText, resultText);
        return this.Base.CanUserViewAgent(agentId, user);
        return this.Base.CanUserRunAgent(agentId, user);
        return this.Base.CanUserEditAgent(agentId, user);
        return this.Base.CanUserDeleteAgent(agentId, user);
        return this.Base.GetUserAgentPermissions(agentId, user);
        return this.Base.GetAccessibleAgents(user, permission);
        return this.Base.ClearAgentPermissionsCache();
        return this.Base.RefreshAgentPermissionsCache(agentId, user);
    // Server-Only Properties
     * Get the agent vector service for semantic search.
     * Initialized during Config - will be null before AIEngine.Config() completes.
    public get AgentVectorService(): SimpleVectorService<AgentEmbeddingMetadata> | null {
        return this._agentVectorService;
     * Get the action vector service for semantic search.
    public get ActionVectorService(): SimpleVectorService<ActionEmbeddingMetadata> | null {
        return this._actionVectorService;
     * Get all available actions loaded from the database.
     * Loaded during Config() - will be empty before AIEngine.Config() completes.
     * NOTE: This returns MJActionEntity (MJ Action system), not the deprecated MJAIActionEntity.
     * For deprecated AI Actions, see the inherited Actions property.
    public get SystemActions(): MJActionEntity[] {
    // Config - Main Entry Point
     * Configures the AIEngine by first ensuring AIEngineBase is configured,
     * then loading server-specific capabilities (embeddings, actions, etc.).
     * This method is safe to call from multiple places concurrently - it will
     * return the same promise to all callers during loading.
     * @param forceRefresh - If true, forces a full reload even if already loaded
     * @param contextUser - User context for server-side operations (required)
     * @param provider - Optional metadata provider override
    public async Config(forceRefresh?: boolean, contextUser?: UserInfo, provider?: IMetadataProvider): Promise<void> {
        // If already loaded and not forcing refresh, return immediately
        if (this._loaded && !forceRefresh) {
        // If currently loading, return the existing promise so all callers wait together
        if (this._loading && this._loadingPromise) {
            return this._loadingPromise;
        // Start loading
        this._loading = true;
        this._loadingPromise = this.innerLoad(forceRefresh, contextUser, provider);
            await this._loadingPromise;
            this._loading = false;
            this._loadingPromise = null;
     * Internal loading logic - separated for clean promise management
    private async innerLoad(forceRefresh?: boolean, contextUser?: UserInfo, provider?: IMetadataProvider): Promise<void> {
            // First, ensure AIEngineBase is configured
            // This is where all the metadata loading happens - we share that data
            await AIEngineBase.Instance.Config(forceRefresh ?? false, contextUser, provider);
            // Now load server-specific capabilities
            await this.RefreshServerSpecificMetadata(contextUser);
     * Refreshes the server metadata including active actions. 
     * Refreshes the embeddings in the engine's vector service for  
     *  - Agents (dynamic recalc of embeddings)
     *  - Actions (dynamic recalc of embeddings)
     *  - Notes (parsed from DB)
     *  - Examples (parsed from DB)
     * If you only need to refresh specific elements noted above, call the individual methods:
     *  - RefreshActions (refreshes just the server side action metadata - e.g. 'Active' Actions)
     *  - RefreshActionEmbeddings (dynamic recalc of embedings from stored data)
     *  - RefreshAgentEmbeddings (dynamic recalc of embeddings from stored data)
     *  - RefreshNoteEmbeddings
     *  - RefreshExampleEmbeddings
    public async RefreshServerSpecificMetadata(contextUser?: UserInfo): Promise<void> {
        // Load actions from the Action system
        await this.RefreshActions(contextUser);
        // Load all embeddings in parallel since they are independent
            this.RefreshAgentEmbeddings(),
            this.RefreshActionEmbeddings(),
            this.RefreshNoteEmbeddings(contextUser),
            this.RefreshExampleEmbeddings(contextUser)
        this._embeddingsGenerated = true;
    // Embedding Generation Methods
     * Force regeneration of all embeddings for agents and actions.
     * Use this method when:
     * - Switching to a different embedding model
     * - Agent or Action descriptions have been significantly updated
     * - You want to ensure embeddings are up-to-date after bulk changes
     * - Troubleshooting embedding-related issues
     * Note: This is an expensive operation and should not be called frequently.
     * Normal auto-refresh operations will NOT regenerate embeddings to avoid performance issues.
     * @param contextUser - User context for database operations (required on server-side)
    public async RegenerateEmbeddings(contextUser?: UserInfo): Promise<void> {
            // Clear the caches
            this._agentEmbeddingsCache.clear();
            this._actionEmbeddingsCache.clear();
            this._embeddingsGenerated = false;
            // Clear the vector services
            this._agentVectorService = null;
            this._actionVectorService = null;
            // Reload actions and regenerate embeddings
            await this.RefreshAgentEmbeddings();
            await this.RefreshActionEmbeddings();
            LogError('AIEngine: Failed to regenerate embeddings', undefined, error instanceof Error ? error : undefined);
     * Refreshes Agent embeddings - agents are pre-loaded at this point, but we need
     * to generate, dynamically, embeddings from the text stored in the agent. This is not a
     * cheap operation, use it sparingly.
    public async RefreshAgentEmbeddings(): Promise<void> {
            // Use agents already loaded by base class
            const agents = this.Agents;  // Delegates to AIEngineBase
            if (!agents || agents.length === 0) {
            // Filter out restricted agents - they should not be discoverable
            const nonRestrictedAgents = agents.filter(agent => !agent.IsRestricted);
            // Filter to only agents that don't have embeddings yet
            const agentsNeedingEmbeddings = nonRestrictedAgents.filter(agent =>
                !this._agentEmbeddingsCache.has(agent.ID)
            if (agentsNeedingEmbeddings.length === 0) {
            // Generate embeddings using static utility method
            const entries = await AgentEmbeddingService.GenerateAgentEmbeddings(
                agentsNeedingEmbeddings,
                (text) => this.EmbedTextLocal(text)
            // Mark these agents as having embeddings
            for (const agent of agentsNeedingEmbeddings) {
                this._agentEmbeddingsCache.set(agent.ID, true);
            // Load into vector service (create if needed, or add to existing)
            if (!this._agentVectorService) {
                this._agentVectorService = new SimpleVectorService();
            this._agentVectorService.LoadVectors(entries);
            LogError(`AIEngine: Failed to load agent embeddings: ${error instanceof Error ? error.message : String(error)}`);
            // Don't throw - allow AIEngine to continue loading even if embeddings fail
     * Loads Active actions from the base engine (contained within this class). Does **not** refresh from the database, simply
     * pulls the latest `Active` actions from the base class into its server side only array.
    public async RefreshActions(contextUser?: UserInfo): Promise<void> {
            await ActionEngineBase.Instance.Config(false, contextUser);
            const actions = ActionEngineBase.Instance.Actions.filter(a => a.Status === 'Active');
            if (actions && actions.length > 0) {
                this._actions = actions;
                LogError('AIEngine: No active actions found during load');
                this._actions = [];
            LogError(`AIEngine: Error loading actions: ${error instanceof Error ? error.message : String(error)}`);
     * Dynamically calculation of embeddings for all `Active` actions. Assumes that the internal Actions array is up to date, call
     * @see RefreshActions first if you do not think they are already.
     * This operation dynamically calculates embeddings from the text in the Action metadata and is an expensive operation, use it
     * sparingly.
    public async RefreshActionEmbeddings(): Promise<void> {
            const actions = this._actions;
            // Filter to only actions that don't have embeddings yet
            const actionsNeedingEmbeddings = actions.filter(action =>
                !this._actionEmbeddingsCache.has(action.ID)
            if (actionsNeedingEmbeddings.length === 0) {
            const entries = await ActionEmbeddingService.GenerateActionEmbeddings(
                actionsNeedingEmbeddings,
            // Mark these actions as having embeddings
            for (const action of actionsNeedingEmbeddings) {
                this._actionEmbeddingsCache.set(action.ID, true);
            if (!this._actionVectorService) {
                this._actionVectorService = new SimpleVectorService();
            this._actionVectorService.LoadVectors(entries);
            LogError(`AIEngine: Failed to load action embeddings: ${error instanceof Error ? error.message : String(error)}`);
     * Refresh the vector service with the latest persisted vectors that are stored in the Agent Notes
     * table. This does **not** calculate embeddings, that is done by the AI Agent Note sub-class upon save 
     * as needed. This method simply uses the stored vectors and parses them from their JSON serialized format into
     * vectors that are used by the vector service.
    public async RefreshNoteEmbeddings(contextUser?: UserInfo): Promise<void> {
            const notes = this.AgentNotes.filter(n => n.Status === 'Active' && n.EmbeddingVector);
            const entries = notes.map(note => ({
                key: note.ID,
                vector: JSON.parse(note.EmbeddingVector!),
                metadata: this.packageNoteMetadata(note)
            this._noteVectorService = new SimpleVectorService();
            this._noteVectorService.LoadVectors(entries);
            LogError(`AIEngine: Failed to load note embeddings: ${error instanceof Error ? error.message : String(error)}`);
     * Takes in a note and packages up the metadata for the vector service
     * @param note 
    protected packageNoteMetadata(note: MJAIAgentNoteEntity): NoteEmbeddingMetadata {
            id: note.ID,
            agentId: note.AgentID,
            userId: note.UserID,
            companyId: note.CompanyID,
            type: note.Type,
            noteText: note.Note!,
            noteEntity: note
     * Updates the vector service to the latest vector containd within the specified agent note that is passed in
    public AddOrUpdateSingleNoteEmbedding(note: MJAIAgentNoteEntity) {
        if (this._noteVectorService) {
            this._noteVectorService.AddOrUpdateVector(note.ID, JSON.parse(note.EmbeddingVector),  this.packageNoteMetadata(note));
            throw new Error('note vector service not initialized, error state')
     * Takes in an example and packages up the metadata for the vector service
     * @param example
    protected packageExampleMetadata(example: MJAIAgentExampleEntity): ExampleEmbeddingMetadata {
            id: example.ID,
            agentId: example.AgentID,
            userId: example.UserID,
            companyId: example.CompanyID,
            type: example.Type,
            exampleInput: example.ExampleInput,
            exampleOutput: example.ExampleOutput,
            successScore: example.SuccessScore,
            exampleEntity: example
     * Updates the vector service to the latest vector contained within the specified agent example that is passed in
    public AddOrUpdateSingleExampleEmbedding(example: MJAIAgentExampleEntity) {
        if (this._exampleVectorService) {
            this._exampleVectorService.AddOrUpdateVector(example.ID, JSON.parse(example.EmbeddingVector), this.packageExampleMetadata(example));
            throw new Error('example vector service not initialized, error state')
     * Refresh the vector service with the latest persisted vectors that are stored in the Agent Examples
     * table. This does **not** calculate embeddings, that is done by the AI Agent Example sub-class upon save 
    public async RefreshExampleEmbeddings(contextUser?: UserInfo): Promise<void> {
            const examples = this.AgentExamples.filter(e => e.Status === 'Active' && e.EmbeddingVector);
            const entries = examples.map(example => ({
                key: example.ID,
                vector: JSON.parse(example.EmbeddingVector!),
                metadata: this.packageExampleMetadata(example)
            this._exampleVectorService = new SimpleVectorService();
            this._exampleVectorService.LoadVectors(entries);
            LogError(`AIEngine: Failed to load example embeddings: ${error instanceof Error ? error.message : String(error)}`);
    // LLM Utility Methods
     * Prepares standard chat parameters with system and user messages.
     * @param userPrompt The user message/query to send to the model
     * @param systemPrompt Optional system prompt to set context/persona for the model
     * @returns Array of properly formatted chat messages
    public PrepareChatMessages(userPrompt: string, systemPrompt?: string): ChatMessage[] {
        const messages: ChatMessage[] = [];
        if (systemPrompt && systemPrompt.length > 0) {
            messages.push({ role: ChatMessageRole.system, content: systemPrompt });
        messages.push({ role: ChatMessageRole.user, content: userPrompt });
     * Prepares an LLM model instance with the appropriate parameters.
     * This method handles common tasks needed before calling an LLM:
     * - Loading AI metadata if needed
     * - Selecting the appropriate model (user-provided or highest power)
     * - Getting the correct API key
     * - Creating the LLM instance
     * @param contextUser The user context for authentication and permissions
     * @param model Optional specific model to use, otherwise uses highest power LLM
     * @param apiKey Optional API key to use with the model
     * @returns Object containing the prepared model instance and model information
    public async PrepareLLMInstance(
        model?: AIModelEntityExtended,
        apiKey?: string
        modelInstance: BaseLLM,
        modelToUse: AIModelEntityExtended
        const modelToUse = model ? model : await this.GetHighestPowerLLM(undefined, contextUser);
        const apiKeyToUse = apiKey ? apiKey : GetAIAPIKey(modelToUse.DriverClass);
        const modelInstance = MJGlobal.Instance.ClassFactory.CreateInstance<BaseLLM>(BaseLLM, modelToUse.DriverClass, apiKeyToUse);
        return { modelInstance, modelToUse };
     * Executes a simple completion task using the provided parameters.
     * @returns The text response from the LLM
     * @throws Error if user prompt is not provided or if there are issues with model creation
    public async SimpleLLMCompletion(userPrompt: string, contextUser: UserInfo, systemPrompt?: string, model?: AIModelEntityExtended, apiKey?: string): Promise<string> {
            if (!userPrompt || userPrompt.length === 0) {
                throw new Error('User prompt not provided.');
            const { modelInstance, modelToUse } = await this.PrepareLLMInstance(
                contextUser, model, apiKey
            const messages = this.PrepareChatMessages(userPrompt, systemPrompt);
            params.messages = messages;
            params.model = modelToUse.APIName;
            const result = await modelInstance.ChatCompletion(params);
            if (result && result.success) {
                return result.data.choices[0].message.content;
                throw new Error(`Error executing LLM model ${modelToUse.Name} : ${result.errorMessage}`);
            LogError(e);
     * Executes multiple parallel chat completions with the same model but potentially different parameters.
     * @param userPrompt The user's message/question to send to the model
     * @param contextUser The user context for authentication and logging
     * @param systemPrompt Optional system prompt to set the context/persona
     * @param iterations Number of parallel completions to run (default: 3)
     * @param temperatureIncrement The amount to increment temperature for each iteration (default: 0.1)
     * @param baseTemperature The starting temperature value (default: 0.7)
     * @param callbacks Optional callbacks for monitoring progress
     * @returns Array of ChatResult objects, one for each parallel completion
    public async ParallelLLMCompletions(
        userPrompt: string,
        systemPrompt?: string,
        iterations: number = 3,
        temperatureIncrement: number = 0.1,
        baseTemperature: number = 0.7,
        apiKey?: string,
            if (iterations < 1) {
                throw new Error('Iterations must be at least 1');
            const paramsArray: ChatParams[] = Array(iterations).fill(0).map((_, i) => {
                params.messages = [...messages];
                params.temperature = baseTemperature + (i * temperatureIncrement);
                return params;
            return await modelInstance.ChatCompletions(paramsArray, callbacks);
    // Embedding Methods
     * Returns an array of the local embedding models, sorted with the highest power models first
    public get LocalEmbeddingModels(): AIModelEntityExtended[] {
        const embeddingModels = this.Models.filter(m => {
            // Guard against AIModelType being non-string (defensive coding for data issues)
            const modelType = typeof m.AIModelType === 'string' ? m.AIModelType.trim().toLowerCase() : '';
            const vendor = typeof m.Vendor === 'string' ? m.Vendor.trim().toLowerCase() : '';
            return modelType === this.EmbeddingModelTypeName.toLowerCase() &&
                   vendor === this.LocalEmbeddingModelVendorName.toLowerCase();
        return embeddingModels.sort((a, b) => (b.PowerRank || 0) - (a.PowerRank || 0));
     * Returns the highest power local embedding model
    public get HighestPowerLocalEmbeddingModel(): AIModelEntityExtended | null {
        const models = this.LocalEmbeddingModels;
        return models && models.length > 0 ? models[0] : null;
     * Returns the lowest power local embedding model
    public get LowestPowerLocalEmbeddingModel(): AIModelEntityExtended | null {
        return models && models.length > 0 ? models[models.length - 1] : null;
     * Helper method that generates an embedding for the given text using the highest power local embedding model.
    public async EmbedTextLocal(text: string): Promise<{result: EmbedTextResult, model: AIModelEntityExtended} | null> {
        const model = this.HighestPowerLocalEmbeddingModel;
        if (!model) {
            LogError('AIEngine: No local embedding model found. Cannot generate embedding.');
        const result = await this.EmbedText(model, text);
        return { result, model };
     * Helper method to instantiate a class instance for the given model and calculate an embedding
     * vector from the provided text.
    public async EmbedText(model: AIModelEntityExtended, text: string, apiKey?: string): Promise<EmbedTextResult | null> {
        const params: EmbedTextParams = {
            text: text,
            model: model.APIName
        const embedding = MJGlobal.Instance.ClassFactory.CreateInstance<BaseEmbeddings>(
            BaseEmbeddings,
            model.DriverClass,
            apiKey
        if (!embedding) {
            LogError(`AIEngine: Failed to create embedding instance for model ${model.Name}. Skipping embedding generation.`);
        const result = await embedding.EmbedText(params);
    // Semantic Search Methods
     * Find agents similar to a task description using semantic search.
    public async FindSimilarAgents(
        taskDescription: string,
        topK: number = 5,
        minSimilarity: number = 0.5
    ): Promise<AgentMatchResult[]> {
            throw new Error('Agent embeddings not loaded. Ensure AIEngine.Config() has completed.');
        return AgentEmbeddingService.FindSimilarAgents(
            this._agentVectorService,
            taskDescription,
            (text) => this.EmbedTextLocal(text),
            topK,
            minSimilarity
     * Find actions similar to a task description using semantic search.
    public async FindSimilarActions(
        topK: number = 10,
    ): Promise<ActionMatchResult[]> {
            throw new Error('Action embeddings not loaded. Ensure AIEngine.Config() has completed.');
        return ActionEmbeddingService.FindSimilarActions(
            this._actionVectorService,
     * Find notes similar to query text using semantic search.
     * Falls back to returning notes from cache if vector service is unavailable.
    public async FindSimilarAgentNotes(
        queryText: string,
        agentId?: string,
        minSimilarity: number = 0.5,
        additionalFilter?: (metadata: NoteEmbeddingMetadata) => boolean
    ): Promise<NoteMatchResult[]> {
        if (!this._noteVectorService) {
            // Vector service not available - fall back to returning notes from cache filtered by scope
            LogError('FindSimilarAgentNotes: Note vector service not initialized. Falling back to cached notes without semantic ranking.');
            return this.fallbackGetNotesFromCache(agentId, userId, companyId, topK, additionalFilter);
        if (!queryText || queryText.trim().length === 0) {
            throw new Error('queryText cannot be empty');
        const queryEmbedding = await this.EmbedTextLocal(queryText);
        if (!queryEmbedding || !queryEmbedding.result || queryEmbedding.result.vector.length === 0) {
            LogError('FindSimilarAgentNotes: Failed to generate embedding for query text. Falling back to cached notes.');
        const composedFilter = this.composeNoteFilters(agentId, userId, companyId, additionalFilter);
        const results = this._noteVectorService.FindNearest(
            queryEmbedding.result.vector,
            minSimilarity,
            undefined,
            composedFilter
        return results.map(r => ({
            note: r.metadata.noteEntity,
            similarity: r.score
     * Compose base scope filters (agentId/userId/companyId) with an optional additional filter
     * into a single filter callback for use with FindNearest.
    private composeNoteFilters(
    ): ((metadata: NoteEmbeddingMetadata) => boolean) | undefined {
        const needsBaseFilter = agentId || userId || companyId;
        const baseFilter = needsBaseFilter
            ? (metadata: NoteEmbeddingMetadata): boolean => {
                if (agentId && metadata.agentId && metadata.agentId !== agentId) return false;
                if (userId && metadata.userId && metadata.userId !== userId) return false;
                if (companyId && metadata.companyId && metadata.companyId !== companyId) return false;
        if (baseFilter && additionalFilter) {
            return (metadata: NoteEmbeddingMetadata): boolean =>
                baseFilter(metadata) && additionalFilter(metadata);
        return baseFilter || additionalFilter || undefined;
     * Fallback method to get notes from cache when vector service is unavailable.
     * Returns notes filtered by scope, sorted by creation date.
    private fallbackGetNotesFromCache(
    ): NoteMatchResult[] {
        const notes = this.AgentNotes.filter(n => {
            if (n.Status !== 'Active') return false;
            if (agentId && n.AgentID !== agentId && n.AgentID !== null) return false;
            if (userId && n.UserID !== userId && n.UserID !== null) return false;
            if (companyId && n.CompanyID !== companyId && n.CompanyID !== null) return false;
            if (additionalFilter && !additionalFilter(this.packageNoteMetadata(n))) return false;
        // Sort by creation date (most recent first) and take topK
        const sorted = notes
            .sort((a, b) => (b.__mj_CreatedAt?.getTime() || 0) - (a.__mj_CreatedAt?.getTime() || 0))
            .slice(0, topK);
        // Return with similarity of 0 to indicate no semantic ranking was applied
        return sorted.map(note => ({
            note,
            similarity: 0
     * Find examples similar to query text using semantic search.
     * Falls back to returning examples from cache if vector service is unavailable.
    public async FindSimilarAgentExamples(
        topK: number = 3,
        additionalFilter?: (metadata: ExampleEmbeddingMetadata) => boolean
    ): Promise<ExampleMatchResult[]> {
        if (!this._exampleVectorService) {
            // Vector service not available - fall back to returning examples from cache filtered by scope
            LogError('FindSimilarAgentExamples: Example vector service not initialized. Falling back to cached examples without semantic ranking.');
            return this.fallbackGetExamplesFromCache(agentId, userId, companyId, topK, additionalFilter);
            LogError('FindSimilarAgentExamples: Failed to generate embedding for query text. Falling back to cached examples.');
        const composedFilter = this.composeExampleFilters(agentId, userId, companyId, additionalFilter);
        const results = this._exampleVectorService.FindNearest(
            example: r.metadata.exampleEntity,
     * into a single filter callback for use with FindNearest on examples.
    private composeExampleFilters(
    ): ((metadata: ExampleEmbeddingMetadata) => boolean) | undefined {
            ? (metadata: ExampleEmbeddingMetadata): boolean => {
            return (metadata: ExampleEmbeddingMetadata): boolean =>
     * Fallback method to get examples from cache when vector service is unavailable.
     * Returns examples filtered by scope, sorted by success score then creation date.
    private fallbackGetExamplesFromCache(
    ): ExampleMatchResult[] {
        const examples = this.AgentExamples.filter(e => {
            if (e.Status !== 'Active') return false;
            if (agentId && e.AgentID !== agentId) return false;
            if (userId && e.UserID !== userId && e.UserID !== null) return false;
            if (companyId && e.CompanyID !== companyId && e.CompanyID !== null) return false;
            if (additionalFilter && !additionalFilter(this.packageExampleMetadata(e))) return false;
        // Sort by success score (highest first), then by creation date (most recent first)
        const sorted = examples
                if (scoreB !== scoreA) return scoreB - scoreA;
                return (b.__mj_CreatedAt?.getTime() || 0) - (a.__mj_CreatedAt?.getTime() || 0);
        return sorted.map(example => ({
            example,
    // Deprecated AI Action Methods
    public async ExecuteEntityAIAction(params: EntityAIActionParams): Promise<BaseResult> {
            const entityAction = this.EntityAIActions.find(ea => ea.ID === params.entityAIActionId);
            if (!entityAction)
                throw new Error(`Entity AI Action ${params.entityAIActionId} not found.`);
            const action = this.Actions.find(a => a.ID === entityAction.AIActionID);
            if (!action)
                throw new Error(`Action ${entityAction.AIActionID} not found, from the EntityAIAction ${params.entityAIActionId}.`);
            if (entityAction.SkipIfOutputFieldNotEmpty &&
                entityAction.OutputType.trim().toLowerCase() === 'field') {
                const val = params.entityRecord.Get(entityAction.OutputField);
                if (val && val.length > 0)
                    return null as unknown as BaseResult;
            const entityPrompt = params.systemPrompt ? params.systemPrompt :
                                    (entityAction.Prompt && entityAction.Prompt.length > 0 ? entityAction.Prompt : action.DefaultPrompt);
            const userMessage = params.userPrompt ? params.userPrompt : this.markupUserMessage(params.entityRecord, entityAction.UserMessage);
            const modelId = entityAction.AIModelID || action.DefaultModelID;
            const model = this.Models.find(m => m.ID === modelId);
            const entityParams = {
                name: entityAction.Name,
                actionId: entityAction.AIActionID,
                modelId: modelId,
                systemPrompt: entityPrompt,
                userMessage: userMessage,
                apiKey: GetAIAPIKey(model!.DriverClass),
                result: null as BaseResult | null
            if (!await params.entityRecord.BeforeEntityAIAction(entityParams))
            const results = await this.ExecuteAIAction({
                actionId: entityParams.actionId,
                modelId: entityParams.modelId,
                systemPrompt: entityParams.systemPrompt,
                userPrompt: entityParams.userMessage,
                modelName: model!.Name
            if (results) {
                const sOutput = this.GetStringOutputFromActionResults(action, results);
                entityParams.result = results;
                if (!await params.entityRecord.AfterEntityAIAction(entityParams))
                if (entityAction.OutputType.trim().toLowerCase() === 'field') {
                    params.entityRecord.Set(entityAction.OutputField, sOutput);
                    if (entityAction.TriggerEvent.trim().toLowerCase() === 'after save') {
                        await params.entityRecord.Save({
                            SkipEntityAIActions: true,
                            IgnoreDirtyState: false
                else if (entityAction.OutputType.trim().toLowerCase() === 'entity') {
                    const newRecord = await md.GetEntityObject(entityAction.OutputEntity);
                    newRecord.NewRecord();
                    newRecord.Set('EntityID', params.entityRecord.EntityInfo.ID);
                    // Use concatenated key format to properly support both single and composite primary keys
                    newRecord.Set('RecordID', params.entityRecord.PrimaryKey.ToConcatenatedString());
                    newRecord.Set(entityAction.OutputField, sOutput);
                    await newRecord.Save();
            LogError('AIEngine: ExecuteEntityAIAction failed', undefined, err instanceof Error ? err : undefined);
                startTime: startTime,
                endTime: new Date(),
                timeElapsed: new Date().getTime() - startTime.getTime(),
                errorMessage: (err as Error).message,
                exception: err
    protected markupUserMessage(entityRecord: BaseEntity, userMessage: string): string {
        let temp = userMessage
        const markupTokens = temp.match(/{[a-zA-Z0-9]+}/g);
        if (markupTokens && markupTokens.length > 0) {
            markupTokens.forEach(token => {
                const fieldName = token.replace('{','').replace('}','');
                const fieldValue = entityRecord.Get(fieldName);
                temp = temp.replace(token, fieldValue ? fieldValue : '');
    public async ExecuteAIAction(params: AIActionParams): Promise<BaseResult> {
        const action = this.Actions.find(a => a.ID === params.actionId);
            throw new Error(`Action ${params.actionId} not found.`);
        if (action.IsActive === false)
            throw new Error(`Action ${params.actionId} is not active.`);
        const model = this.Models.find(m => m.ID === params.modelId);
        if (!model)
            throw new Error(`Model ${params.modelId} not found.`);
        if (model.IsActive === false)
            throw new Error(`Model ${params.modelId} is not active.`);
        const driver: BaseModel = await this.getDriver(model, GetAIAPIKey(model.DriverClass));
        if (driver) {
            const modelParams = <ChatParams>{
                model: params.modelName,
                messages: [
                        content: params.systemPrompt
                        content: params.userPrompt
            switch (action.Name.trim().toLowerCase()) {
                case 'classify':
                    const classifyResult = await (<BaseLLM>driver).ClassifyText(modelParams);
                    return classifyResult;
                case 'summarize':
                    const summarizeResult = await (<BaseLLM>driver).SummarizeText(modelParams);
                    return summarizeResult;
                case 'chat':
                    const chatResult = await (<BaseLLM>driver).ChatCompletion(modelParams);
                    return chatResult;
                    throw new Error(`Action ${action.Name} not supported.`);
            throw new Error(`Driver ${model.DriverClass} not found or couldn't be loaded.`);
     * @deprecated This method is related to deprecated AI Actions. Use AIPromptRunner instead.
    protected GetStringOutputFromActionResults(action: MJAIActionEntity, result: BaseResult): string {
                const classifyResult = <ClassifyResult>result;
                return classifyResult.tags.map(t => t.tag).join(', ');
                const summarizeResult = <SummarizeResult>result;
                return summarizeResult.summaryText;
                const chatResult = <ChatResult>result;
                return chatResult.data.choices[0].message.content;
    protected async getDriver(model: AIModelEntityExtended, apiKey: string): Promise<BaseModel> {
        const driverClassName = model.DriverClass;
        const driverModuleName = model.DriverImportPath;
            if (driverModuleName && driverModuleName.length > 0) {
                const driverModule = await import(driverModuleName);
                if (!driverModule)
                    throw new Error(`Error loading driver module '${driverModuleName}'`);
            return MJGlobal.Instance.ClassFactory.CreateInstance<BaseModel>(BaseModel, driverClassName, apiKey);
            throw new Error(`Error loading driver '${driverModuleName}' / '${driverClassName}' : ${(e as Error).message}`);
