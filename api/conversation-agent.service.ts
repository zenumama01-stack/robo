import { DestroyRef, Injectable } from '@angular/core';
import { GraphQLAIClient } from '@memberjunction/graphql-dataprovider';
import { ExecuteAgentParams, ExecuteAgentResult, AgentExecutionProgressCallback, ConversationUtility, AttachmentData } from '@memberjunction/ai-core-plus';
import { ChatMessage, ChatMessageContent } from '@memberjunction/ai';
import { AIEngineBase, AIAgentPermissionHelper } from '@memberjunction/ai-engine-base';
import { MJConversationDetailEntity, MJConversationDetailArtifactEntity, MJArtifactVersionEntity, MJConversationDetailAttachmentEntity } from '@memberjunction/core-entities';
import { LazyArtifactInfo } from '../models/lazy-artifact-info';
import { MentionParserService } from './mention-parser.service';
 * Context for artifact lookups - provides pre-loaded data from conversation
 * to avoid redundant database queries
export interface ArtifactLookupContext {
  artifactsByDetailId: Map<string, LazyArtifactInfo[]>;
 * Result from intent check - indicates whether to continue with agent
 * and which artifact version to use as payload
export interface IntentCheckResult {
  decision: 'YES' | 'NO' | 'UNSURE';
  reasoning: string;
  targetArtifactVersionId?: string;
 * Service for managing agent interactions within conversations.
 * Handles communication with the ambient Sage Agent and other agents.
export class ConversationAgentService {
  private _aiClient: GraphQLAIClient | null = null;
  private _conversationManagerAgent: AIAgentEntityExtended | null = null;
  private _sessionIds: Map<string, string> = new Map(); // conversationId -> sessionId
  private _isProcessing$ = new BehaviorSubject<boolean>(false);
   * Observable indicating if the ambient agent is currently processing
  public readonly isProcessing$: Observable<boolean> = this._isProcessing$.asObservable();
  constructor(private mentionParser: MentionParserService) {
    this.initializeAIClient();
   * Initialize the GraphQL AI Client
  private initializeAIClient(): void {
        this._aiClient = new GraphQLAIClient(provider);
        console.warn('GraphQLDataProvider not available, agent functionality will be limited');
      console.error('Failed to initialize GraphQL AI Client:', error);
   * Get or load the Sage Agent (formerly Conversation Manager Agent)
  public async getConversationManagerAgent(): Promise<AIAgentEntityExtended | null> {
    if (this._conversationManagerAgent) {
      return this._conversationManagerAgent;
      // Find the Sage Agent
      const agents = AIEngineBase.Instance.Agents;
      this._conversationManagerAgent = agents.find(
        (agent: AIAgentEntityExtended) => agent.Name === 'Sage'
      if (!this._conversationManagerAgent) {
        const errorMsg = 'Sage Agent not found in AIEngineBase.Agents';
        console.warn(errorMsg);
        MJNotificationService.Instance?.CreateSimpleNotification(errorMsg, 'error', 5000);
      const errorMsg = 'Error loading Sage Agent: ' + (error instanceof Error ? error.message : String(error));
      console.error('Error loading Sage Agent:', error);
   * Process a message through the ambient Sage Agent.
   * This should be called for every message sent in a conversation.
   * Uses the optimized RunAIAgentFromConversationDetail mutation which loads
   * conversation history (including attachments) server-side for better performance.
   * @param message The message that was just sent
   * @param conversationHistory Recent messages in the conversation for context (kept for backwards compatibility but not used)
   * @param conversationDetailId The ID of the conversation detail record to link to the agent run
   * @param onProgress Optional callback for receiving progress updates during execution
   * @returns The agent's response, or null if the agent chooses not to respond
  async processMessage(
    message: MJConversationDetailEntity,
    conversationHistory: MJConversationDetailEntity[],
    onProgress?: AgentExecutionProgressCallback
  ): Promise<ExecuteAgentResult | null> {
    // Don't process if user is tagging someone else (future enhancement)
    // For now, we'll always send to the ambient agent
    if (!this._aiClient) {
      const errorMsg = 'AI Client not initialized, cannot process message through agent';
      MJNotificationService.Instance?.CreateSimpleNotification(errorMsg, 'warning', 5000);
    const agent = await this.getConversationManagerAgent();
    if (!agent || !agent.ID) {
      const errorMsg = 'Sage Agent not available';
      // Indicate agent is processing
      this._isProcessing$.next(true);
      // Get current user for permission filtering
      const currentUser = Metadata.Provider.CurrentUser;
        console.warn('⚠️ No current user available for permission filtering, using unfiltered agents');
      // Filter agents by status and hierarchy first
      const candidateAgents = AIEngineBase.Instance.Agents.filter(
        a => a.ID !== agent.ID &&
             !a.ParentID &&
             a.InvocationMode !== 'Sub-Agent' // ensure that the agent is intended to run as top-level
      // Filter by user permissions if user context available
      const availAgents = currentUser
        ? await this.filterAgentsByPermissions(candidateAgents, currentUser)
        : candidateAgents;
      console.log(`📋 Available agents for Sage: ${availAgents.length} (filtered from ${candidateAgents.length} candidates)`);
      // Use optimized mutation that loads conversation history server-side
      // This avoids sending large attachment data from client to server
      const result = await this._aiClient.RunAIAgentFromConversationDetail({
        conversationDetailId: conversationDetailId,
        maxHistoryMessages: 20,
          ALL_AVAILABLE_AGENTS: availAgents.map(a => ({
            ID: a.ID,
            Name: a.Name,
            Description: a.Description
          latestMessageId: message.ID
        createArtifacts: true,
        createNotification: true,
        // Adapt progress callback format: GraphQL uses currentStep, AgentExecutionProgressCallback uses step
        onProgress: onProgress ? (progress) => {
          onProgress({
            step: progress.currentStep as 'initialization' | 'validation' | 'prompt_execution' | 'action_execution' | 'subagent_execution' | 'decision_processing' | 'finalization',
            percentage: progress.percentage,
            message: progress.message,
            metadata: progress.metadata
      const errorMsg = 'Error processing message through agent: ' + (error instanceof Error ? error.message : String(error));
      console.error('Error processing message through agent:', error);
      // Always clear processing state
      this._isProcessing$.next(false);
   * Find the configuration preset ID from a previous @mention of an agent in conversation history.
   * Searches backwards through User messages to find the most recent @mention of the specified agent
   * that includes a configId.
   * @param agentId The agent ID to search for
   * @param conversationHistory The conversation history to search through
   * @returns The configuration preset ID if found, undefined otherwise
  public findConfigurationPresetFromHistory(
    conversationHistory: MJConversationDetailEntity[]
  ): string | undefined {
    // Search backwards through history for User messages that @mention this agent with a configId
    const userMentionWithConfig = conversationHistory
      .find(msg => {
        if (msg.Role !== 'User' || !msg.Message) return false;
        // Parse the message to check for an @mention of this agent with a configId
          msg.Message,
          AIEngineBase.Instance.Agents,
          []
        return mentionResult.agentMention?.id === agentId && mentionResult.agentMention?.configurationId;
    if (userMentionWithConfig) {
        userMentionWithConfig.Message,
      if (mentionResult.agentMention?.configurationId) {
        console.log(`🎯 Found configuration preset from @mention: ${mentionResult.agentMention.configurationId}`);
        return mentionResult.agentMention.configurationId;
   * Build the message array for the agent from conversation history.
   * Note: conversationHistory already includes the current message, so we don't add it separately.
   * IMPORTANT: This method loads artifacts and attachments for each message.
   * @deprecated This method is no longer used by processMessage() which now uses the optimized
   * RunAIAgentFromConversationDetail mutation that loads history server-side. Kept for backwards
   * compatibility with other callers that may need client-side message building.
  private async buildAgentMessages(
    history: MJConversationDetailEntity[]
  ): Promise<ChatMessage[]> {
    // Add historical messages (limit to recent context, e.g., last 20 messages)
    // History already includes the current message from the caller
    const recentHistory = history.slice(-20);
    // Get IDs of all messages in history
    const messageIds = recentHistory.map(msg => msg.ID).filter(id => id); // Filter out any undefined IDs
    const artifactsByDetailId = new Map<string, string[]>(); // DetailID -> array of artifact JSON strings
    const attachmentsByDetailId = new Map<string, AttachmentData[]>(); // DetailID -> array of AttachmentData
      // Load artifacts and attachments in parallel
      const [artifactsLoaded, attachmentsLoaded] = await Promise.all([
        this.loadArtifactsForMessages(rv, messageIds, artifactsByDetailId),
        this.loadAttachmentsForMessages(rv, messageIds, attachmentsByDetailId)
      if (artifactsLoaded) {
        console.log(`📦 Loaded ${artifactsByDetailId.size} artifact groups for ${messageIds.length} messages`);
      if (attachmentsLoaded) {
        console.log(`🖼️ Loaded ${attachmentsByDetailId.size} attachment groups for ${messageIds.length} messages`);
    // Build messages with proper multimodal content
    for (const msg of recentHistory) {
      const messageText = msg.Message || '';
      const artifacts = artifactsByDetailId.get(msg.ID);
      const attachments = attachmentsByDetailId.get(msg.ID);
      // Build content - may be string or ChatMessageContentBlock[]
      let content: ChatMessageContent;
        // Use BuildChatMessageContent to create proper content blocks for multimodal
        content = ConversationUtility.BuildChatMessageContent(messageText, attachments);
        // Append artifacts to text content if present
          const artifactText = artifacts.map(json => `\n\n# Artifact\n${json}\n`).join('');
            content = content + artifactText;
            // Find or create text block and append artifacts
            const textBlock = content.find(b => b.type === 'text');
            if (textBlock && textBlock.content) {
              textBlock.content += artifactText;
              content.push({ type: 'text', content: artifactText });
        // No attachments - use simple string content
        content = messageText;
        // Append artifacts
          for (const artifactJson of artifacts) {
            content += `\n\n# Artifact\n${artifactJson}\n`;
        role: this.mapRoleToAgentRole(msg.Role) as 'system' | 'user' | 'assistant',
   * Load artifacts for messages (OUTPUT direction only)
  private async loadArtifactsForMessages(
    messageIds: string[],
    artifactsByDetailId: Map<string, string[]>
      const junctionResult = await rv.RunView<MJConversationDetailArtifactEntity>({
        ExtraFilter: `ConversationDetailID IN ('${messageIds.join("','")}') AND Direction='Output'`,
      if (junctionResult.Success && junctionResult.Results && junctionResult.Results.length > 0) {
        // Collect unique version IDs
        const versionIds = new Set<string>();
        for (const junction of junctionResult.Results) {
          versionIds.add(junction.ArtifactVersionID);
        // Batch load all artifact versions
          ExtraFilter: `ID IN ('${Array.from(versionIds).join("','")}')`,
          const versionMap = new Map(versionResult.Results.map(v => [v.ID, v]));
            const version = versionMap.get(junction.ArtifactVersionID);
            if (version && version.Content) {
              const existing = artifactsByDetailId.get(junction.ConversationDetailID) || [];
              existing.push(version.Content);
              artifactsByDetailId.set(junction.ConversationDetailID, existing);
      console.error('Error loading artifacts for conversation context:', error);
   * Load attachments for messages and convert to AttachmentData format
  private async loadAttachmentsForMessages(
    attachmentsByDetailId: Map<string, AttachmentData[]>
    console.log('[AgentService] loadAttachmentsForMessages - querying for messageIds:', messageIds);
      const filter = `ConversationDetailID IN ('${messageIds.join("','")}')`;
      console.log('[AgentService] loadAttachmentsForMessages - filter:', filter);
      const attachmentResult = await rv.RunView<MJConversationDetailAttachmentEntity>({
        OrderBy: 'DisplayOrder ASC, __mj_CreatedAt ASC',
      console.log('[AgentService] loadAttachmentsForMessages - query result:', {
        success: attachmentResult.Success,
        count: attachmentResult.Results?.length || 0,
        error: attachmentResult.ErrorMessage
      if (attachmentResult.Success && attachmentResult.Results && attachmentResult.Results.length > 0) {
        for (const att of attachmentResult.Results) {
          console.log('[AgentService] loadAttachmentsForMessages - processing attachment:', {
            id: att.ID,
            detailId: att.ConversationDetailID,
            mimeType: att.MimeType,
            hasInlineData: !!att.InlineData,
            hasFileID: !!att.FileID
          // Convert to AttachmentData format
          const attachmentData = this.convertToAttachmentData(att);
          if (attachmentData) {
            const existing = attachmentsByDetailId.get(att.ConversationDetailID) || [];
            existing.push(attachmentData);
            attachmentsByDetailId.set(att.ConversationDetailID, existing);
      console.error('Error loading attachments for conversation context:', error);
   * Convert a MJConversationDetailAttachmentEntity to AttachmentData format
  private convertToAttachmentData(att: MJConversationDetailAttachmentEntity): AttachmentData | null {
    // Get the content - either inline data or file URL
    let content: string | null = null;
    if (att.InlineData) {
      // Create data URL from inline base64 data
      content = `data:${att.MimeType};base64,${att.InlineData}`;
    } else if (att.FileID) {
      // TODO: Get pre-authenticated URL from MJStorage
      // For now, skip attachments stored in external storage
      console.warn(`Attachment ${att.ID} uses FileID storage - external URLs not yet supported`);
    // Determine attachment type from modality or MIME type
    const attachmentType = ConversationUtility.GetAttachmentTypeFromMime(att.MimeType);
      type: attachmentType,
      fileName: att.FileName ?? undefined,
      sizeBytes: att.FileSizeBytes ?? undefined,
      width: att.Width ?? undefined,
      height: att.Height ?? undefined,
      durationSeconds: att.DurationSeconds ?? undefined,
   * Map ConversationDetail Role to agent message role
  private mapRoleToAgentRole(role: string): string {
    const roleLower = (role || '').toLowerCase();
    if (roleLower === 'user') return 'user';
    if (roleLower === 'assistant' || roleLower === 'agent') return 'assistant';
    return 'user'; // Default to user
   * Check if a message is tagging another user or agent.
   * Returns true if the message contains @mentions that are NOT the ambient agent.
   * Future enhancement: parse @mentions and determine if ambient agent should process.
  private isTaggingOthers(message: string): boolean {
    // Future implementation: check for @mentions
    // For now, always return false (always process through ambient agent)
   * Invoke a sub-agent based on Sage Agent's payload.
   * This is called when Sage decides to delegate to a specialist agent.
   * @param agentName Name of the agent to invoke
   * @param message The user message that triggered this
   * @param conversationHistory Recent conversation history for context
   * @param reasoning Why this agent is being invoked
   * @param payload Optional payload to pass to the agent (e.g., previous OUTPUT artifact for continuity)
   * @param sourceArtifactId Optional source artifact ID for versioning
   * @param sourceArtifactVersionId Optional source artifact version ID for versioning
   * @param agentConfigurationPresetId Optional ID of the AIAgentConfiguration preset (will be mapped to AIConfigurationID)
   * @returns The agent's execution result, or null if agent not found
  async invokeSubAgent(
    reasoning: string,
    payload?: any,
    onProgress?: AgentExecutionProgressCallback,
    sourceArtifactId?: string,
    sourceArtifactVersionId?: string,
    agentConfigurationPresetId?: string
      const errorMsg = 'AI Client not initialized, cannot invoke sub-agent';
      // Find the agent by name
        const errorMsg = `Sub-agent "${agentName}" not found`;
        console.warn(`❌ ${errorMsg}`);
      console.log(`🎯 Invoking sub-agent: ${agentName}`, { reasoning, hasPayload: !!payload, hasConfigPreset: !!agentConfigurationPresetId });
      // Map AIAgentConfiguration preset ID to actual AIConfiguration ID
      let aiConfigurationId: string | undefined = undefined;
      if (agentConfigurationPresetId) {
        // Get the preset from AIEngineBase to extract the AIConfigurationID
        const presets = AIEngineBase.Instance.GetAgentConfigurationPresets(agent.ID, false);
        // check by preset ID or AIConfigurationID - since sometimes we have the actual
        // configuration ID. Since both UUID no collisions should ever be possible.
        const preset = presets.find(p => p.ID === agentConfigurationPresetId || p.AIConfigurationID === agentConfigurationPresetId);
        if (preset) {
          aiConfigurationId = preset.AIConfigurationID || undefined;
          console.log(`🎯 Mapped agent configuration preset "${preset.Name}" to AIConfigurationID: ${aiConfigurationId || 'default'}`);
          console.warn(`⚠️ Agent configuration preset ${agentConfigurationPresetId} not found for agent ${agent.ID}`);
      // Build conversation messages for the sub-agent
      // Note: conversationHistory already includes the current message
      const conversationMessages = await this.buildAgentMessages(conversationHistory);
      // Prepare parameters with optional payload and progress callback
      const params: ExecuteAgentParams = {
        conversationMessages: conversationMessages,
          latestMessageId: message.ID,
          invocationReason: reasoning
        ...(payload ? { payload } : {}),
        ...(aiConfigurationId ? { configurationId: aiConfigurationId } : {}),
        onProgress: onProgress
      // Run the sub-agent with optional source artifact info for versioning (GraphQL layer only)
      const result = await this._aiClient.RunAIAgent(params, sourceArtifactId, sourceArtifactVersionId);
      const errorMsg = `Error invoking sub-agent "${agentName}": ` + (error instanceof Error ? error.message : String(error));
      console.error(`Error invoking sub-agent "${agentName}":`, error);
   * Check if user's latest message should continue with the previous agent or route through Sage.
   * Uses fast inference (<500ms) to determine intent and avoid unnecessary Sage overhead.
   * @param agentId The ID of the previous agent
   * @param latestMessage The user's new message
   * @param conversationHistory Recent conversation history for context (last 10 messages)
   * @param context Pre-loaded artifact and agent run data to avoid database queries
   * @returns IntentCheckResult with decision, reasoning, and optional target artifact version
  async checkAgentContinuityIntent(
    latestMessage: string,
    context: ArtifactLookupContext
  ): Promise<IntentCheckResult> {
      console.warn('AI Client not initialized, defaulting to UNSURE for intent check');
      return { decision: 'UNSURE', reasoning: 'AI Client not initialized' };
      // Load the Check Sage Intent prompt
      const prompt = AIEngineBase.Instance.Prompts.find(p => p.Name === 'Check Sage Intent');
        console.warn('⚠️ Check Sage Intent prompt not found, defaulting to UNSURE');
        return { decision: 'UNSURE', reasoning: 'Check Sage Intent prompt not found' };
      // Get agent details
        console.warn('⚠️ Previous agent not found, defaulting to UNSURE');
        return { decision: 'UNSURE', reasoning: 'Previous agent not found' };
      // Find all artifacts from this agent in this conversation
      const agentArtifacts = this.findAllAgentArtifacts(
        conversationHistory,
        context
      // Build compact conversation history (last 10 messages)
      const recentHistory = conversationHistory.slice(-10);
      const compactHistory = recentHistory.map((msg, idx) => {
        const role = msg.Role === 'User' ? 'User' : agent.Name || 'Agent';
        const content = msg.Message || '';
        return `${idx + 1}. ${role}: ${content.substring(0, 150)}${content.length > 150 ? '...' : ''}`;
      // Build artifact context if available
      let artifactContext = '';
      if (agentArtifacts.length > 0) {
        artifactContext = '\n\n**Prior Artifacts Created by This Agent**:\n';
        agentArtifacts.forEach((artifact, idx) => {
          artifactContext += `${idx + 1}. ${artifact.artifactName} (${artifact.artifactType})\n`;
          artifactContext += `   - Versions: ${artifact.versions.length}\n`;
          if (artifact.versions.length > 0) {
            artifactContext += `   - Latest: v${artifact.versions[0].versionNumber}`;
            if (artifact.versions[0].versionName) {
              artifactContext += ` - ${artifact.versions[0].versionName}`;
            artifactContext += '\n';
      // Build user message with context
      const userMessage = `**Previous Agent**: ${agent.Name} - ${agent.Description || 'No description'}
**Conversation History** (last ${recentHistory.length} messages):
${compactHistory}${artifactContext}
**Latest User Message**: "${latestMessage}"`;
      console.log('🔍 Checking agent continuity intent...', {
        messagePreview: latestMessage.substring(0, 50),
        artifactCount: agentArtifacts.length
      // Run the prompt with artifact data included
      const result = await this._aiClient.RunAIPrompt({
        messages: [{ role: 'user', content: userMessage }],
          hasPriorArtifact: agentArtifacts.length > 0,
          priorArtifacts: agentArtifacts
        if (parsed && parsed.continuesWith) {
          const decision = parsed.continuesWith.toUpperCase();
          const reasoning = parsed.reasoning || 'No reasoning provided';
          const targetArtifactVersionId = parsed.targetArtifactVersionId || undefined;
          console.log(`✅ Intent check result: ${decision}`, {
            targetArtifactVersionId,
            latency: result.executionTimeMs || 'unknown'
          // Validate the response
          if (decision === 'YES' || decision === 'NO' || decision === 'UNSURE') {
              decision: decision as 'YES' | 'NO' | 'UNSURE',
              targetArtifactVersionId
      console.warn('⚠️ Intent check failed or returned invalid format, defaulting to UNSURE');
      return { decision: 'UNSURE', reasoning: 'Invalid format from intent check prompt' };
      console.error('❌ Error checking agent continuity intent:', error);
      // On error, default to UNSURE (safer to let Sage evaluate)
        decision: 'UNSURE',
        reasoning: `Error during intent check: ${error instanceof Error ? error.message : String(error)}`
   * Clear the session for a conversation (useful when starting a new topic)
  clearSession(conversationId: string): void {
    this._sessionIds.delete(conversationId);
   * Filter agents based on user's 'run' permission.
   * Only returns agents that the user has permission to run.
   * @param agents List of candidate agents to filter
   * @param user User to check permissions for
   * @returns Filtered list of agents the user can run
  private async filterAgentsByPermissions(
    user: any
  ): Promise<AIAgentEntityExtended[]> {
    const permittedAgents: AIAgentEntityExtended[] = [];
        const hasPermission = await AIAgentPermissionHelper.HasPermission(
          agent.ID,
          user,
        if (hasPermission) {
          permittedAgents.push(agent);
        console.error(`Error checking permission for agent ${agent.Name}:`, error);
        // On error, exclude agent (fail closed)
    return permittedAgents;
   * Find all artifacts created by the specified agent in this conversation.
   * Returns artifacts grouped by artifact with versions, ordered most recent first.
   * Enables LLM to reason about which artifact/version user is referencing.
   * Uses pre-loaded data from ArtifactLookupContext for performance (no database queries).
  private findAllAgentArtifacts(
  ): Array<{
    artifactType: string;
    artifactDescription: string | null;
    versions: Array<{
      versionName: string | null;
      versionDescription: string | null;
    // Iterate backwards through conversation details (most recent first)
    for (let i = conversationDetails.length - 1; i >= 0; i--) {
      const detail = conversationDetails[i];
      // Skip non-AI messages and errors
      if (detail.Role !== 'AI' || detail.Status === 'Error') continue;
      // O(1) lookup for agent run from pre-loaded data
      const agentRun = context.agentRunsByDetailId.get(detail.ID);
      if (!agentRun || agentRun.AgentID !== agentId || agentRun.Status !== 'Completed') {
      // O(1) lookup for artifacts from pre-loaded data
      const artifacts = context.artifactsByDetailId.get(detail.ID);
      if (!artifacts || artifacts.length === 0) continue;
      // Process each artifact
      for (const lazyArtifact of artifacts) {
        const mainArtifactId = lazyArtifact.artifactId;
        // Get or create artifact entry
        if (!artifactMap.has(mainArtifactId)) {
          artifactMap.set(mainArtifactId, {
            artifactId: mainArtifactId,
            artifactName: lazyArtifact.artifactName,
            artifactType: lazyArtifact.artifactType,
            artifactDescription: lazyArtifact.artifactDescription || null,
            versions: []
        // Add version to artifact
        const artifactEntry = artifactMap.get(mainArtifactId)!;
        artifactEntry.versions.push({
          versionId: lazyArtifact.artifactVersionId,
          versionNumber: lazyArtifact.versionNumber,
          versionName: lazyArtifact.versionName,
          versionDescription: lazyArtifact.versionDescription,
          createdAt: lazyArtifact.versionCreatedAt
    // Convert map to array (most recent artifacts first based on their latest version)
    return Array.from(artifactMap.values()).sort((a, b) => {
      const aLatest = a.versions[0]?.createdAt || new Date(0);
      const bLatest = b.versions[0]?.createdAt || new Date(0);
      return bLatest.getTime() - aLatest.getTime();
