import { Component, Input, Output, EventEmitter, ViewChild, OnInit, OnDestroy, OnChanges, SimpleChanges, AfterViewInit } from '@angular/core';
import { MJConversationDetailEntity, EnvironmentEntityExtended } from '@memberjunction/core-entities';
import { DataCacheService } from '../../services/data-cache.service';
import { ConversationStreamingService, MessageProgressUpdate, MessageProgressMetadata } from '../../services/conversation-streaming.service';
import { GraphQLDataProvider, GraphQLAIClient } from '@memberjunction/graphql-dataprovider';
import { ExecuteAgentResult, AgentExecutionProgressCallback, AgentResponseForm, ActionableCommand, AutomaticCommand, ConversationUtility } from '@memberjunction/ai-core-plus';
import { MentionParserService } from '../../services/mention-parser.service';
import { Mention, MentionParseResult } from '../../models/conversation-state.model';
import { MessageInputBoxComponent } from './message-input-box.component';
  selector: 'mj-message-input',
  templateUrl: './message-input.component.html',
  styleUrl: './message-input.component.scss'
export class MessageInputComponent implements OnInit, OnDestroy, OnChanges, AfterViewInit {
  // Default artifact type ID for JSON (when agent doesn't specify DefaultArtifactTypeID)
  private readonly JSON_ARTIFACT_TYPE_ID = 'ae674c7e-ea0d-49ea-89e4-0649f5eb20d4';
  @Input() conversationId!: string;
  @Input() conversationName?: string | null; // For task tracking display
  @Input() placeholder: string = 'Type a message... (Ctrl+Enter to send)';
  @Input() parentMessageId?: string; // Optional: for replying in threads
  @Input() enableAttachments: boolean = true; // Whether to show attachment button (based on agent modality support)
  @Input() maxAttachments: number = 10; // Maximum number of attachments per message
  @Input() maxAttachmentSizeBytes: number = 20 * 1024 * 1024; // Maximum size per attachment (20MB default)
  @Input() acceptedFileTypes: string = 'image/*'; // Accepted MIME types pattern
  @Input() artifactsByDetailId?: Map<string, LazyArtifactInfo[]>; // Pre-loaded artifact data for performance
  @Input() systemArtifactsByDetailId?: Map<string, LazyArtifactInfo[]>; // Pre-loaded system artifact data (Visibility='System Only')
  @Input() agentRunsByDetailId?: Map<string, AIAgentRunEntityExtended>; // Pre-loaded agent run data for performance
  @Input() emptyStateMode: boolean = false; // When true, emits emptyStateSubmit instead of creating messages directly
  // Initial message to send automatically - using getter/setter for precise control
  private _initialMessage: string | null = null;
  private _initialAttachments: PendingAttachment[] | null = null;
  private _isComponentReady = false; // Track if component is ready to send
  set initialMessage(value: string | null) {
    // Handle case where an object with {text, attachments} is passed instead of just a string
    // This can happen if there's a type mismatch in the binding chain
    let actualValue = value;
      actualValue = (value as { text: string }).text;
    const previousValue = this._initialMessage;
    this._initialMessage = actualValue;
    // If component is ready and we have a new non-null message, trigger send
    if (this._isComponentReady && actualValue && actualValue !== previousValue) {
      this.triggerInitialSend();
  get initialMessage(): string | null {
    return this._initialMessage;
  set initialAttachments(value: PendingAttachment[] | null) {
    this._initialAttachments = value;
  get initialAttachments(): PendingAttachment[] | null {
    return this._initialAttachments;
  private _conversationHistory: MJConversationDetailEntity[] = [];
  public get conversationHistory(): MJConversationDetailEntity[] {
    return this._conversationHistory;
  public set conversationHistory(value: MJConversationDetailEntity[]) {
    this._conversationHistory = value;
  // Message IDs that are in-progress and need streaming reconnection
  // Using getter/setter to react immediately when value changes (avoids timing issues with ngOnChanges)
  private _inProgressMessageIds?: string[];
  set inProgressMessageIds(value: string[] | undefined) {
    this._inProgressMessageIds = value;
    // React immediately when input changes (after component initialized)
    // This ensures callbacks are registered without relying on ngOnChanges timing
    if (this.streamingService && value && value.length > 0) {
      this.reconnectInProgressMessages();
  get inProgressMessageIds(): string[] | undefined {
    return this._inProgressMessageIds;
  @Output() messageSent = new EventEmitter<MJConversationDetailEntity>();
  @Output() agentResponse = new EventEmitter<{message: MJConversationDetailEntity, agentResult: any}>();
  @Output() agentRunDetected = new EventEmitter<{conversationDetailId: string; agentRunId: string}>();
  @Output() agentRunUpdate = new EventEmitter<{conversationDetailId: string; agentRun?: any, agentRunId?: string}>(); // Emits when agent run data updates during progress
  @Output() messageComplete = new EventEmitter<{conversationDetailId: string; agentId?: string}>(); // Emits when message completes (success or error)
  @Output() artifactCreated = new EventEmitter<{artifactId: string; versionId: string; versionNumber: number; conversationDetailId: string; name: string}>();
  @Output() intentCheckStarted = new EventEmitter<void>(); // Emits when intent checking starts
  @Output() intentCheckCompleted = new EventEmitter<void>(); // Emits when intent checking completes
  @Output() emptyStateSubmit = new EventEmitter<{text: string; attachments: PendingAttachment[]}>(); // Emitted when in emptyStateMode
  @Output() uploadStateChanged = new EventEmitter<{isUploading: boolean; message: string}>(); // Emits when attachment upload state changes
  @ViewChild('inputBox') inputBox!: MessageInputBoxComponent;
  public isSending: boolean = false;
  public isProcessing: boolean = false; // True when waiting for agent/naming response
  public processingMessage: string = 'AI is responding...'; // Message shown during processing
  public isUploadingAttachments: boolean = false; // True when uploading attachments to server
  public uploadingMessage: string = 'Uploading attachments...'; // Message shown during upload
  public converationManagerAgent: AIAgentEntityExtended | null = null;
  // Track completion timestamps to prevent race conditions with late progress updates
  private completionTimestamps = new Map<string, number>();
  // Track registered streaming callbacks for cleanup
  private registeredCallbacks = new Map<string, (progress: MessageProgressUpdate) => Promise<void>>();
  // Track pending attachments from the input box
  private pendingAttachments: PendingAttachment[] = [];
    private agentService: ConversationAgentService,
    private dataCache: DataCacheService,
    private mentionParser: MentionParserService,
    private attachmentService: ConversationAttachmentService
    this.converationManagerAgent = await this.agentService.getConversationManagerAgent();
    // Initialize mention autocomplete (needed for parsing mentions in messages)
    // Reconnect to any in-progress messages for streaming updates (via global streaming service)
    // When conversation changes, focus the input
    if (changes['conversationId'] && !changes['conversationId'].firstChange) {
    // Note: initialMessage/initialAttachments handled by setters, inProgressMessageIds handled by setter
    // Focus input on initial load
    // Mark component as ready
    this._isComponentReady = true;
    // If there's an initial message to send (from empty state), send it automatically
    if (this._initialMessage || (this._initialAttachments && this._initialAttachments.length > 0)) {
   * Triggers sending of initial message and attachments.
   * Called from setter or ngAfterViewInit when conditions are met.
  private triggerInitialSend(): void {
    const message = this._initialMessage;
    const attachments = this._initialAttachments;
    // Set pending attachments before sending
    if (attachments && attachments.length > 0) {
      this.pendingAttachments = [...attachments];
    // Use setTimeout to ensure we're outside of change detection cycle
      this.sendMessageWithText(message || '');
    // Unregister all streaming callbacks
    this.unregisterAllCallbacks();
   * Focus the message input textarea
    // Use setTimeout to ensure DOM is ready
      if (this.inputBox) {
        this.inputBox.focus();
   * Reconnect to in-progress messages for streaming updates via global streaming service.
   * This is called when:
   * 1. Component initializes (ngOnInit)
   * 2. Conversation changes (ngOnChanges)
   * 3. User returns to a conversation with in-progress messages
   * 4. Parent component explicitly triggers reconnection
  public reconnectInProgressMessages(): void {
    if (!this.inProgressMessageIds || this.inProgressMessageIds.length === 0) {
    // Unregister any previously registered callbacks for this component
    // Register new callbacks for each in-progress message
    for (const messageId of this.inProgressMessageIds) {
      // Create callback bound to this message ID
      const callback = this.createMessageProgressCallback(messageId);
      this.registeredCallbacks.set(messageId, callback);
      // Register with streaming service
      this.streamingService.registerMessageCallback(messageId, callback);
   * Create a progress callback for a specific message ID.
   * This callback will be invoked by the streaming service when progress updates arrive.
  private createMessageProgressCallback(messageId: string): (progress: MessageProgressUpdate) => Promise<void> {
    return async (progress: MessageProgressUpdate) => {
        // Get message from cache (single source of truth)
        const message = await this.dataCache.getConversationDetail(messageId, this.currentUser);
          console.warn(`[StreamingCallback] Message ${messageId} not found in cache`);
        // Skip if already complete or errored
        if (message.Status === 'Complete' || message.Status === 'Error') {
          console.log(`[StreamingCallback] Message ${messageId} is ${message.Status}, ignoring progress update`);
        // Check if message was marked as completed (prevents race condition)
        const completionTime = this.completionTimestamps.get(messageId);
        if (completionTime) {
          console.log(`[StreamingCallback] Message ${messageId} marked complete at ${new Date(completionTime).toISOString()}, ignoring late progress update`);
        // Default: plain message (used by RunAIAgentResolver and TaskOrchestrator without step info)
        message.Message = progress.message;
        // TaskOrchestrator with step info: add formatted header
        // Prefer hierarchical step (e.g., "2.1.3") over flat stepCount
        if (progress.resolver === 'TaskOrchestrator') {
          const stepDisplay = progress.metadata?.progress?.hierarchicalStep || progress.stepCount;
          if (stepDisplay != null) {
            message.Message = `**Step ${stepDisplay}**\n\n${progress.message}`;
        // Server now saves progress - client only updates in-memory and emits for UI
        // (Prevents race condition where client's late save overwrites server's final Status)
        // CRITICAL: Emit update to trigger UI refresh
        this.messageSent.emit(message);
        // CRITICAL: Update ActiveTasksService to keep the tasks dropdown in sync
        this.activeTasks.updateStatusByConversationDetailId(message.ID, progress.message);
        console.log(`[StreamingCallback] Updated message ${messageId}: ${progress.taskName || 'Agent'}`);
        console.error(`[StreamingCallback] Error updating message ${messageId}:`, error);
   * Unregister all callbacks registered by this component.
   * Called during cleanup and when switching conversations.
  private unregisterAllCallbacks(): void {
    if (this.registeredCallbacks.size === 0) {
    console.log(`🧹 Unregistering ${this.registeredCallbacks.size} message callbacks`);
    for (const [messageId, callback] of this.registeredCallbacks) {
      this.streamingService.unregisterMessageCallback(messageId, callback);
    this.registeredCallbacks.clear();
    return !this.disabled && !this.isSending && this.messageText.trim().length > 0;
   * Handle attachments changed from the input box
    this.pendingAttachments = attachments;
   * Handle attachment errors from the input box
    this.toastService.error(error);
   * Handle text submitted from the input box
  async onTextSubmitted(text: string): Promise<void> {
    // Check if we have either text or attachments
    const hasText = text && text.trim().length > 0;
    const hasAttachments = this.pendingAttachments.length > 0;
    if (!hasText && !hasAttachments) {
    // In empty state mode, just emit the data and let parent handle conversation creation
    if (this.emptyStateMode) {
      const attachmentsToEmit = [...this.pendingAttachments];
      this.messageText = '';
      this.emptyStateSubmit.emit({ text: text?.trim() || '', attachments: attachmentsToEmit });
    this.isSending = true;
    // Store attachments locally since we'll clear them after send
    const attachmentsToSave = [...this.pendingAttachments];
      const messageDetail = await this.createMessageDetailFromText(text?.trim() || '');
      const saved = await messageDetail.Save();
        // Save attachments if any were pending
        // Attachments are stored in ConversationDetailAttachment table and loaded
        // separately when building AI messages - no need to add tokens to Message field
        if (attachmentsToSave.length > 0) {
          // Show upload indicator for attachments
          this.isUploadingAttachments = true;
          this.uploadingMessage = `Uploading ${attachmentsToSave.length} attachment${attachmentsToSave.length > 1 ? 's' : ''}...`;
          this.uploadStateChanged.emit({ isUploading: true, message: this.uploadingMessage });
            await this.attachmentService.saveAttachments(
              messageDetail.ID,
              attachmentsToSave,
          } catch (attachmentError) {
            console.error('Failed to save attachments:', attachmentError);
            this.toastService.error('Some attachments could not be saved');
            this.isUploadingAttachments = false;
            this.uploadStateChanged.emit({ isUploading: false, message: '' });
        // Clear pending attachments after successful send
        await this.handleSuccessfulSend(messageDetail);
        this.handleSendFailure(messageDetail);
      this.handleSendError(error);
      this.isSending = false;
  async onSend(): Promise<void> {
    if (!this.canSend) return;
      const messageDetail = await this.createMessageDetail();
   * Send a message with custom text WITHOUT modifying the visible messageText input
   * Used for suggested responses and initial messages from empty state.
   * Also saves any pending attachments.
  public async sendMessageWithText(text: string): Promise<void> {
    if (this.isSending) {
      const detail = await this.dataCache.createConversationDetail(this.currentUser);
      detail.ConversationID = this.conversationId;
      detail.Message = text?.trim() || '';
      detail.Role = 'User';
      detail.UserID = this.currentUser.ID; // Set the user who sent the message
      if (this.parentMessageId) {
        detail.ParentID = this.parentMessageId;
      const saved = await detail.Save();
              detail.ID,
        this.messageSent.emit(detail);
        const mentionResult = this.parseMentionsFromMessage(detail.Message);
        const isFirstMessage = this.conversationHistory.length === 0;
        await this.routeMessage(detail, mentionResult, isFirstMessage);
        this.handleSendFailure(detail);
   * Creates and configures a new conversation detail message
  private async createMessageDetail(): Promise<MJConversationDetailEntity> {
    detail.Message = this.messageText.trim();
   * Creates and configures a new conversation detail message from provided text
  private async createMessageDetailFromText(text: string): Promise<MJConversationDetailEntity> {
    detail.Message = text;
   * Handles successful message send - routes to appropriate agent
  private async handleSuccessfulSend(messageDetail: MJConversationDetailEntity): Promise<void> {
    this.messageSent.emit(messageDetail);
    const mentionResult = this.parseMentionsFromMessage(messageDetail.Message);
    await this.routeMessage(messageDetail, mentionResult, isFirstMessage);
    this.refocusTextarea();
   * Parses mentions from the message for routing decisions
  private parseMentionsFromMessage(message: string): MentionParseResult {
    const mentionResult = this.mentionParser.parseMentions(
      this.mentionAutocomplete.getAvailableAgents(),
      this.mentionAutocomplete.getAvailableUsers()
    return mentionResult;
   * Routes the message to the appropriate agent or Sage based on context
   * Priority: @mention > intent check > Sage
  private async routeMessage(
    messageDetail: MJConversationDetailEntity,
    mentionResult: MentionParseResult,
    isFirstMessage: boolean
    // Priority 1: Direct @mention
    if (mentionResult.agentMention) {
      await this.handleDirectMention(messageDetail, mentionResult.agentMention, isFirstMessage);
    // Priority 2: Check for previous agent with intent check
    const lastAgentId = this.findLastNonSageAgentId();
    if (lastAgentId) {
      await this.handleAgentContinuity(messageDetail, lastAgentId, mentionResult, isFirstMessage);
    // Priority 3: Check if Sage was explicitly @mentioned with a config preset
    // If so, treat it like agent continuity so the config preset is preserved
    if (this.converationManagerAgent?.ID) {
      const sageConfigPreset = this.agentService.findConfigurationPresetFromHistory(
        this.converationManagerAgent.ID,
        this.conversationHistory
      if (sageConfigPreset) {
        // User explicitly @mentioned Sage with a config - use the shared execution helper directly
        // Pass the already-found config preset to avoid redundant history search
        await this.executeRouteWithNaming(
          () => this.executeAgentContinuation(
            messageDetail,
            this.converationManagerAgent!.ID,
            this.converationManagerAgent!.Name || 'Sage',
            this.conversationId,
            null, // Sage doesn't use payload continuity
            null, // Sage doesn't use artifact info
            sageConfigPreset // Pass the already-found config preset
          messageDetail.Message,
          isFirstMessage
    // Priority 4: No context - use Sage with default config
    await this.handleNoAgentContext(messageDetail, mentionResult, isFirstMessage);
   * Handles routing when user directly mentions an agent with @
  private async handleDirectMention(
    agentMention: Mention,
    // The agentMention already has configurationId from JSON parsing
    // If it wasn't in JSON (legacy format), try to get from chip data
    if (!agentMention.configurationId) {
      const chipData = this.inputBox?.getMentionChipsData() || [];
      const agentChip = chipData.find(chip => chip.id === agentMention.id && chip.type === 'agent');
      if (agentChip?.presetId) {
        agentMention.configurationId = agentChip.presetId;
    if (agentMention.configurationId) {
      //console.log(`🎯 Agent mention has configuration ID: ${agentMention.configurationId}`);
      () => this.invokeAgentDirectly(messageDetail, agentMention, this.conversationId),
   * Handles routing when there's a previous agent - checks intent first
  private async handleAgentContinuity(
    lastAgentId: string,
    const intentResult = await this.checkContinuityIntent(lastAgentId, messageDetail.Message);
    if (intentResult.decision === 'YES') {
        () => this.continueWithAgent(
          lastAgentId,
          intentResult.targetArtifactVersionId
        () => this.processMessageThroughAgent(messageDetail, mentionResult),
   * Handles routing when there's no previous agent context
  private async handleNoAgentContext(
   * Finds the last agent ID that isn't Sage
  private findLastNonSageAgentId(): string | null {
    const lastAIMessage = this.conversationHistory
        msg.AgentID !== this.converationManagerAgent?.ID
    return lastAIMessage?.AgentID || null;
   * Checks if message should continue with the previous agent
   * Emits events to show temporary intent checking message in conversation
  private async checkContinuityIntent(agentId: string, message: string) {
    // FAST PATH: If message contains form response syntax, skip the intent check entirely
    // Form responses always continue with the agent that requested the form
    // Don't show "Analyzing intent..." message for this obvious case
    if (ConversationUtility.ContainsFormResponse(message)) {
      console.log('✅ Form response detected, skipping intent check UI (fast path)');
        decision: 'YES' as const,
        reasoning: 'User submitted a form response to the previous agent'
    // Emit event to show temporary "Analyzing intent..." message in conversation
    this.intentCheckStarted.emit();
      // Build context from pre-loaded maps (if available)
      if (!this.artifactsByDetailId || !this.agentRunsByDetailId) {
        console.warn('⚠️ Artifact/agent run context not available for intent check');
        return { decision: 'UNSURE' as const, reasoning: 'Context not available' };
      const intent = await this.agentService.checkAgentContinuityIntent(
        this.conversationHistory,
          artifactsByDetailId: this.artifactsByDetailId,
          agentRunsByDetailId: this.agentRunsByDetailId
      console.error('❌ Intent check failed, defaulting to UNSURE:', error);
      return { decision: 'UNSURE' as const, reasoning: 'Intent check failed with error' };
      // Emit event to remove temporary intent checking message
      this.intentCheckCompleted.emit();
   * Executes a routing function, optionally with conversation naming for first message
   * IMPORTANT: Conversation naming runs asynchronously in the background and does NOT
   * block the agent invocation. This prevents UI blocking if naming times out.
  private async executeRouteWithNaming(
    routeFunction: () => Promise<void>,
    if (isFirstMessage) {
      // Fire conversation naming in background (don't await)
      // This prevents 2+ minute UI blocking if naming times out
      this.nameConversation(userMessage);
      // Execute route immediately (don't wait for naming)
      await routeFunction();
   * Returns focus to the message textarea
  private refocusTextarea(): void {
   * Handles message send failure
  private handleSendFailure(messageDetail: MJConversationDetailEntity): void {
    console.error('Failed to send message:', messageDetail.LatestResult?.Message);
    this.toastService.error('Failed to send message. Please try again.');
   * Handles message send error
  private handleSendError(error: unknown): void {
    console.error('Error sending message:', error);
    this.toastService.error('Error sending message. Please try again.');
   * Create a progress callback for agent execution
   * This callback updates both the active task and the ConversationDetail message
   * IMPORTANT: Filters by agentRunId to prevent cross-contamination when multiple agents run in parallel
  private createProgressCallback(
    conversationDetail: MJConversationDetailEntity,
  ): AgentExecutionProgressCallback {
    // Use closure to capture the agent run ID from the first progress message
    // This allows us to filter out progress messages from other concurrent agents
    let capturedAgentRunId: string | null = null;
    return async (progress) => {
      const metadata = progress.metadata as MessageProgressMetadata | undefined;
      const progressAgentRun = metadata?.agentRun;
      const progressAgentRunId = metadata?.agentRun?.ID || metadata?.agentRunId;
      // Capture the agent run ID from the first progress message
      if (!capturedAgentRunId && progressAgentRunId) {
        capturedAgentRunId = progressAgentRunId;
      // Filter out progress messages from other concurrent agents
      // This prevents cross-contamination when multiple agents run in parallel
      if (capturedAgentRunId && progressAgentRunId && progressAgentRunId !== capturedAgentRunId) {
      // Format progress message with visual indicator
      const progressText = progress.message;
      // Update the active task with progress details (if it exists)
      this.activeTasks.updateStatusByConversationDetailId(conversationDetail.ID, progressText);
      // Update the ConversationDetail message in real-time
        if (conversationDetail) {
          // Check 1: Skip if message is already complete or errored
          if (conversationDetail.Status === 'Complete' || conversationDetail.Status === 'Error') {
          // Check 2: Skip if message was marked as completed (prevents race condition)
          // Once a message is marked complete, we reject ALL further progress updates
          const completionTime = this.completionTimestamps.get(conversationDetail.ID);
          // CRITICAL FIX: Emit FULL agent run object for incremental updates
          // This contains live timestamps, status, and other fields that change during execution
          if (progressAgentRun || progressAgentRunId) {
            this.agentRunUpdate.emit({
              conversationDetailId: conversationDetail.ID,
              agentRun: progressAgentRun,
              agentRunId: progressAgentRunId
          } else if (progressAgentRunId && !capturedAgentRunId) {
            // Fallback: If we don't have the full object but have the ID, emit agentRunDetected
            // This will trigger a database query to load the agent run
            this.agentRunDetected.emit({
          if (conversationDetail.Status === 'In-Progress') {
            conversationDetail.Message = progressText;
            this.messageSent.emit(conversationDetail);
        console.warn('Failed to update progress in ConversationDetail:', error);
   * Process the message through agents (multi-stage: Sage -> possible sub-agent)
   * Only called when there's no @mention and no implicit agent context
  private async processMessageThroughAgent(
    userMessage: MJConversationDetailEntity,
    mentionResult: MentionParseResult
    let taskId: string | null = null;
    let conversationManagerMessage: MJConversationDetailEntity | null = null;
    // CRITICAL: Capture conversationId from user message at start
    // This prevents race condition when user switches conversations during async processing
    const conversationId = userMessage.ConversationID;
      // Create AI message for Sage BEFORE invoking
      conversationManagerMessage = await this.dataCache.createConversationDetail(this.currentUser);
      conversationManagerMessage.ConversationID = conversationId;
      conversationManagerMessage.Role = 'AI';
      conversationManagerMessage.Message = '⏳ Starting...';
      conversationManagerMessage.ParentID = userMessage.ID;
      conversationManagerMessage.Status = 'In-Progress';
      conversationManagerMessage.HiddenToUser = false;
      // Use the preloaded Sage agent instead of looking it up
        conversationManagerMessage.AgentID = this.converationManagerAgent.ID;
      await conversationManagerMessage.Save();
      this.messageSent.emit(conversationManagerMessage);
      // Use Sage to evaluate and route
      // Stage 1: Sage evaluates the message
      taskId = this.activeTasks.add({
        agentName: 'Sage',
        status: 'Evaluating message...',
        relatedMessageId: userMessage.ID,
        conversationDetailId: conversationManagerMessage.ID,
        conversationId: this.conversationId,
        conversationName: this.conversationName
      const result = await this.agentService.processMessage(
        conversationManagerMessage.ID,
        this.createProgressCallback(conversationManagerMessage, 'Sage')
      // Task will be removed automatically in markMessageComplete()
      // DO NOT remove here - agent may still be streaming/processing
      taskId = null; // Clear reference but don't remove from service
      if (!result || !result.success) {
        // Evaluation failed - use updateConversationDetail to ensure task cleanup
        const errorMsg = result?.agentRun?.ErrorMessage || 'Agent evaluation failed';
        conversationManagerMessage.Error = errorMsg;
        await this.updateConversationDetail(conversationManagerMessage, `❌ Evaluation failed`, 'Error');
        await this.updateConversationDetail(userMessage, userMessage.Message, 'Complete');
        console.warn('⚠️ Sage failed:', result?.agentRun?.ErrorMessage);
        // Clean up completion timestamp
        this.cleanupCompletionTimestamp(conversationManagerMessage.ID);
      // Stage 2: Check for task graph (multi-step orchestration)
      if (result.payload?.taskGraph) {
        await this.handleTaskGraphExecution(userMessage, result, this.conversationId, conversationManagerMessage);
        // Remove CM from active tasks
        if (taskId) {
          // Task removed in markMessageComplete() - this.activeTasks.remove(taskId);
      // Stage 3: Check for sub-agent invocation (single-step delegation)
      else if (result.agentRun.FinalStep === 'Success' && result.payload?.invokeAgent) {
        // Reuse the existing conversationManagerMessage instead of creating new ones
        await this.handleSubAgentInvocation(userMessage, result, this.conversationId, conversationManagerMessage);
      // Stage 4: Direct chat response from Agent
      else if (result.agentRun.FinalStep === 'Chat' && result.agentRun.Message) {
        // Normal chat response
        // use update helper to ensure that if there is a race condition with more streaming updates we don't allow that to override this final message
        // Note: updateConversationDetail will call markMessageComplete() for us
        await this.updateConversationDetail(conversationManagerMessage, result.agentRun.Message, 'Complete', result);
        // Handle artifacts if any (but NOT task graphs - those are intermediate work products)
        // Server already created artifacts - just emit event to trigger UI reload
        if (result.payload && Object.keys(result.payload).length > 0) {
          this.artifactCreated.emit({
            artifactId: '',
            versionId: '',
            versionNumber: 0,
            name: ''
        // Clean up completion timestamp after delay
      // Stage 5: Silent observation - but check for message content first
        // Check if there's a message to display even without payload/taskGraph
        if (result.agentRun.Message) {
          // Mark message as completing BEFORE setting final content
          this.markMessageComplete(conversationManagerMessage);
          // Mark message as completing
          // Hide the Sage message
          conversationManagerMessage.HiddenToUser = true;
          await this.updateConversationDetail(conversationManagerMessage, conversationManagerMessage.Message, 'Complete', result);
          await this.handleSilentObservation(userMessage, this.conversationId);
      console.error('❌ Error processing message through agents:', error);
      // Update conversationManagerMessage status to Error
      if (conversationManagerMessage && conversationManagerMessage.ID) {
        // Use updateConversationDetail to ensure task cleanup
        conversationManagerMessage.Error = String(error);
        await this.updateConversationDetail(conversationManagerMessage, `❌ Error: ${String(error)}`, 'Error');
      // Mark user message as complete
      // Clean up active task
   * Handle task graph execution based on Sage's payload
   * Creates tasks and orchestrates their execution
  private async handleTaskGraphExecution(
    managerResult: ExecuteAgentResult,
    conversationId: string,
    conversationManagerMessage: MJConversationDetailEntity
    const taskGraph = managerResult.payload.taskGraph;
    const workflowName = taskGraph.workflowName || 'Workflow';
    const reasoning = taskGraph.reasoning || 'Executing multi-step workflow';
    const taskCount = taskGraph.tasks?.length || 0;
    // Deduplicate tasks by tempId (LLM sometimes returns duplicates)
    const seenTempIds = new Set<string>();
    const uniqueTasks = taskGraph.tasks.filter((task: any) => {
      if (seenTempIds.has(task.tempId)) {
        console.warn(`⚠️ Duplicate tempId detected on client, filtering: ${task.tempId} (${task.name})`);
      seenTempIds.add(task.tempId);
    const uniqueTaskCount = uniqueTasks.length;
    const isSingleTask = uniqueTaskCount === 1;
    // If single task, use direct agent execution (existing pattern with great PubSub support)
    if (isSingleTask) {
      const task = uniqueTasks[0];
      const agentName = task.agentName;
      // Update CM message
      const delegationMessage = `👉 Delegating to **${agentName}**`;
      await this.updateConversationDetail(conversationManagerMessage, delegationMessage, 'Complete');
      // Execute single agent directly using existing pattern
      await this.handleSingleTaskExecution(
        conversationManagerMessage
    // Multi-step workflow - use server-side task orchestration
    console.log(`📋 Multi-step workflow detected (${uniqueTaskCount} tasks), using task orchestration`);
    // Update CM message with task summary (use unique tasks only)
    const taskSummary = uniqueTasks.map((t: any) => `• ${t.name}`).join('\n');
    await this.updateConversationDetail(conversationManagerMessage, `📋 Setting up multi-step workflow...\n\n**${workflowName}**\n${taskSummary}`, 'Complete');
    // Step 2: Create new ConversationDetail for task execution updates
    const taskExecutionMessage = await this.dataCache.createConversationDetail(this.currentUser);
    taskExecutionMessage.ConversationID = conversationId;
    taskExecutionMessage.Role = 'AI';
    taskExecutionMessage.Message = '⏳ Starting workflow execution...';
    taskExecutionMessage.ParentID = conversationManagerMessage.ID; // Thread under delegation message
    taskExecutionMessage.Status = 'In-Progress';
    taskExecutionMessage.HiddenToUser = false;
    // No AgentID for now - this represents the task orchestration system
    await taskExecutionMessage.Save();
    this.messageSent.emit(taskExecutionMessage);
    // Register for streaming updates via global streaming service
    const callback = this.createMessageProgressCallback(taskExecutionMessage.ID);
    this.registeredCallbacks.set(taskExecutionMessage.ID, callback);
    this.streamingService.registerMessageCallback(taskExecutionMessage.ID, callback);
      // Get default environment ID (MJ standard environment used across all installations)
      const environmentId = EnvironmentEntityExtended.DefaultEnvironmentID;
      // Get session ID for PubSub subscriptions
      const sessionId = GraphQLDataProvider.Instance.sessionId || '';
      // Step 3: Call ExecuteTaskGraph mutation (links to taskExecutionMessage)
        mutation ExecuteTaskGraph($taskGraphJson: String!, $conversationDetailId: String!, $environmentId: String!, $sessionId: String!, $createNotifications: Boolean) {
          ExecuteTaskGraph(
            taskGraphJson: $taskGraphJson
            conversationDetailId: $conversationDetailId
            environmentId: $environmentId
            sessionId: $sessionId
            createNotifications: $createNotifications
            results {
              taskId
        taskGraphJson: JSON.stringify(taskGraph),
        conversationDetailId: taskExecutionMessage.ID, // Link tasks to execution message, not CM message
        environmentId: environmentId,
        sessionId: sessionId,
        createNotifications: true
      const result = await GraphQLDataProvider.Instance.ExecuteGQL(mutation, variables);
      // Step 4: Update task execution message with results
      // ExecuteGQL returns data directly (not wrapped in {data, errors})
      if (result?.ExecuteTaskGraph?.success) {
        await this.updateConversationDetail(taskExecutionMessage, `✅ **${workflowName}** completed successfully`, 'Complete');
        const errorMsg = result?.ExecuteTaskGraph?.errorMessage || 'Unknown error';
        console.error('❌ Task graph execution failed:', errorMsg);
        taskExecutionMessage.Error = errorMsg;
        await this.updateConversationDetail(taskExecutionMessage, `❌ **${workflowName}** failed: ${errorMsg}`, 'Error');
      // Trigger artifact reload for this message
      // Artifacts were created on server during task execution and linked to this message
      // This event triggers the parent component to reload artifacts from the database
        artifactId: '', // Placeholder - reload will fetch actual artifacts from DB
        versionNumber: 1,
        conversationDetailId: taskExecutionMessage.ID,
      // Unregister streaming callback (task complete)
      const callback = this.registeredCallbacks.get(taskExecutionMessage.ID);
      if (callback) {
        this.streamingService.unregisterMessageCallback(taskExecutionMessage.ID, callback);
        this.registeredCallbacks.delete(taskExecutionMessage.ID);
      // Mark agent response message as complete (removes task from active tasks)
      await this.updateConversationDetail(conversationManagerMessage, conversationManagerMessage.Message, 'Complete');
      console.error('❌ Error executing task graph:', error);
      taskExecutionMessage.Error = String(error);
      await this.updateConversationDetail(taskExecutionMessage, `❌ **${workflowName}** - Error: ${String(error)}`, 'Error');
      // Trigger artifact reload even on error - partial artifacts may have been created
      // Unregister streaming callback (task failed)
      await this.updateConversationDetail(conversationManagerMessage, conversationManagerMessage.Message, 'Error');
  protected async updateConversationDetail(convoDetail: MJConversationDetailEntity, message: string, status: 'In-Progress' | 'Complete' | 'Error', result?: ExecuteAgentResult): Promise<void> {
    // Mark as completing FIRST if status is Complete or Error
    // This ensures task cleanup happens even if we return early due to guard clause
    if (status === 'Complete' || status === 'Error') {
      this.markMessageComplete(convoDetail);
    // Guard clause: Don't re-save if already complete/errored (prevents duplicate saves)
    // Task has already been removed by markMessageComplete() above
    if (convoDetail.Status === 'Complete' || convoDetail.Status === 'Error') {
      return; // Already complete, no need to save again
    const maxAttempts = 2;
    let attempts = 0, done = false;
    while (attempts < maxAttempts && !done) {
      // Set response form and command fields before saving
      if (result?.responseForm) {
        convoDetail.ResponseForm = JSON.stringify(result.responseForm);
      if (result?.actionableCommands) {
        convoDetail.ActionableCommands = JSON.stringify(result.actionableCommands);
      if (result?.automaticCommands) {
        convoDetail.AutomaticCommands = JSON.stringify(result.automaticCommands);
      convoDetail.Message = message;
      convoDetail.Status = status;
      await convoDetail.Save();
      if (convoDetail.Message === message && convoDetail.Status === status) {
        done = true;
        this.messageSent.emit(convoDetail);
        console.warn(`   ⚠️ ConversationDetail update attempt ${attempts + 1} did not persist. ${attempts + 1 < maxAttempts ? 'Retrying...' : 'Giving up.'}`);
      attempts++;
      this.cleanupCompletionTimestamp(convoDetail.ID);
   * Load previous payload for an agent from its most recent OUTPUT artifact.
   * Searches backwards through all messages from this agent until an artifact is found.
   * This ensures payload continuity even after clarifying exchanges without artifacts.
   * Checks both user-visible and system artifacts to support agents like Agent Manager.
  private async loadPreviousPayloadForAgent(agentId: string): Promise<{
    payload: any;
    artifactInfo: {artifactId: string; versionId: string; versionNumber: number} | null;
    // Get all messages from this agent in reverse order (most recent first)
    const agentMessages = this.conversationHistory
      .filter(msg => msg.Role === 'AI' && msg.AgentID === agentId);
    if (agentMessages.length === 0) {
      return { payload: null, artifactInfo: null };
    // Search through all agent messages until we find one with an artifact
    for (const message of agentMessages) {
      // Check user-visible artifacts first
      let artifacts = this.artifactsByDetailId?.get(message.ID);
      // If not found, check system artifacts (Agent Manager, etc.)
      if (!artifacts || artifacts.length === 0) {
        artifacts = this.systemArtifactsByDetailId?.get(message.ID);
      // Try to load artifact content as payload
      if (artifacts && artifacts.length > 0) {
        const artifact = artifacts[0];
          const version = await artifact.getVersion();
            console.log(`📦 Loaded previous payload for agent ${agentId} from artifact (message: ${message.ID})`);
              payload: JSON.parse(version.Content),
              artifactInfo: {
                artifactId: artifact.artifactId,
                versionId: artifact.artifactVersionId,
                versionNumber: artifact.versionNumber
          console.error('Error loading payload from artifact:', error);
          // Continue to next message
    console.log(`📦 No previous payload found for agent ${agentId} after searching ${agentMessages.length} messages`);
   * Handle single task execution from task graph using direct agent execution
   * Uses the existing agent execution pattern with PubSub support
  private async handleSingleTaskExecution(
    task: any, // Task definition from taskGraph
      // Look up the agent
      const agent = AIEngineBase.Instance.Agents.find(a => a.Name === agentName);
        throw new Error(`Agent not found: ${agentName}`);
      // Create AI response message for the agent execution
      const agentResponseMessage = await this.dataCache.createConversationDetail(this.currentUser);
      agentResponseMessage.ConversationID = conversationId;
      agentResponseMessage.Role = 'AI';
      agentResponseMessage.Message = '⏳ Starting...';
      agentResponseMessage.ParentID = conversationManagerMessage.ID; // Thread under delegation
      agentResponseMessage.Status = 'In-Progress';
      agentResponseMessage.HiddenToUser = false;
      agentResponseMessage.AgentID = agent.ID;
      await agentResponseMessage.Save();
      this.messageSent.emit(agentResponseMessage);
      // Add to active tasks
      const newTaskId = this.activeTasks.add({
        agentName: agentName,
        status: 'Starting...',
        conversationDetailId: agentResponseMessage.ID,
      // Load previous payload if agent has been invoked before
      const { payload: previousPayload, artifactInfo } = await this.loadPreviousPayloadForAgent(agent.ID);
      // Merge Sage's task payload with previous agent payload (Sage's takes precedence)
      const mergedPayload = previousPayload
        ? { ...previousPayload, ...task.inputPayload }
        : task.inputPayload;
      // Invoke agent with merged payload
      const agentResult = await this.agentService.invokeSubAgent(
        task.description || task.name,
        agentResponseMessage.ID,
        mergedPayload, // Pass merged payload for continuity
        this.createProgressCallback(agentResponseMessage, agentName),
        artifactInfo?.artifactId,
        artifactInfo?.versionId
      // Task will be removed automatically in markMessageComplete() when status changes to Complete/Error
      // DO NOT remove here - allows UI to show task during entire execution
      if (agentResult && agentResult.success) {
        // Update message with result
        await this.updateConversationDetail(agentResponseMessage, agentResult.agentRun?.Message || `✅ **${agentName}** completed`, 'Complete', agentResult);
        // Server created artifacts - emit event to trigger UI reload
        if (agentResult.payload && Object.keys(agentResult.payload).length > 0) {
          console.log('🎨 Server created artifact from single task execution');
        // Handle failure
        const errorMsg = agentResult?.agentRun?.ErrorMessage || 'Agent execution failed';
        agentResponseMessage.Error = errorMsg;
        await this.updateConversationDetail(agentResponseMessage, `❌ **${agentName}** failed: ${errorMsg}`, 'Error');
      console.error('❌ Error in single task execution:', error);
   * Handle sub-agent invocation based on Sage's payload
   * Reuses the existing conversationManagerMessage to avoid creating multiple records
  private async handleSubAgentInvocation(
    const payload = managerResult.payload;
    const agentName = payload.invokeAgent;
    const reasoning = payload.reasoning || 'Delegating to specialist agent';
    // Now create a NEW message for the sub-agent execution
      // Look up the agent to get its ID
      // Create AI response message BEFORE invoking agent (for duration tracking)
      agentResponseMessage.Message = '⏳ Starting...'; // Initial message
      agentResponseMessage.ParentID = conversationManagerMessage.ID; // Thread under delegation message
      // Set AgentID immediately for proper attribution
      if (agent?.ID) {
      // Save the record to establish __mj_CreatedAt timestamp
      // Add sub-agent to active tasks
      const { payload: previousPayload, artifactInfo } = agent?.ID
        ? await this.loadPreviousPayloadForAgent(agent.ID)
        : { payload: null, artifactInfo: null };
      // Find configuration preset from previous @mention in conversation history
      const configurationPresetId = agent?.ID
        ? this.agentService.findConfigurationPresetFromHistory(agent.ID, this.conversationHistory)
      // Invoke the sub-agent with progress callback
      const subResult = await this.agentService.invokeSubAgent(
        reasoning,
        previousPayload, // Pass previous payload for continuity
        artifactInfo?.versionId,
        configurationPresetId // Pass configuration from previous @mention for continuity
      if (subResult && subResult.success) {
        // Update the response message with agent result
        // Store the agent ID for display
        if (subResult.agentRun.AgentID) {
          agentResponseMessage.AgentID = subResult.agentRun.AgentID;
        await this.updateConversationDetail(agentResponseMessage, subResult.agentRun?.Message || `✅ **${agentName}** completed`, 'Complete', subResult);
        if (subResult.payload && Object.keys(subResult.payload).length > 0) {
          console.log('🎨 Server created artifact for sub-agent message:', agentResponseMessage.ID);
          // Re-emit to trigger artifact display
        // Sub-agent failed - attempt auto-retry once
        console.log(`⚠️ ${agentName} failed, attempting auto-retry...`);
        await this.updateConversationDetail(conversationManagerMessage, `👉 **${agentName}** will handle this request...\n\n⚠️ First attempt failed, retrying...`, conversationManagerMessage.Status);
        // Update the existing agentResponseMessage to show retry status
        await this.updateConversationDetail(agentResponseMessage, "Retrying...", agentResponseMessage.Status);
        // Retry the sub-agent (reuse previously loaded payload and config from first attempt)
        const retryResult = await this.agentService.invokeSubAgent(
          previousPayload, // Pass same payload as first attempt
          this.createProgressCallback(agentResponseMessage, `${agentName} (retry)`),
          configurationPresetId // Pass same config as first attempt
        if (retryResult && retryResult.success) {
          // Retry succeeded - update the same message
          if (retryResult.agentRun.AgentID) {
            agentResponseMessage.AgentID = retryResult.agentRun.AgentID;
          await this.updateConversationDetail(agentResponseMessage, retryResult.agentRun?.Message || `✅ **${agentName}** completed`, 'Complete', retryResult);
          if (retryResult.payload && Object.keys(retryResult.payload).length > 0) {
          // Retry also failed - show error with manual retry option
          conversationManagerMessage.Error = retryResult?.agentRun?.ErrorMessage || null;
          await this.updateConversationDetail(conversationManagerMessage, `❌ **${agentName}** failed after retry\n\n${retryResult?.agentRun?.ErrorMessage || 'Unknown error'}`, 'Error');
      console.error(`❌ Error invoking sub-agent ${agentName}:`, error);
      await this.updateConversationDetail(conversationManagerMessage, `❌ **${agentName}** encountered an error\n\n${String(error)}`, 'Error');
   * Handle silent observation - when Sage stays silent,
   * check if we should continue with the last agent for iterative refinement
  private async handleSilentObservation(
    conversationId: string
    // Find the last AI message (excluding Sage) in the conversation history
    if (!lastAIMessage || !lastAIMessage.AgentID) {
      // No previous specialist agent - just mark user message as complete
      console.log('🔇 No previous specialist agent found - marking complete');
    // Load the agent entity to get its name
    const previousAgent = AIEngineBase.Instance.Agents.find(a => a.ID === lastAIMessage.AgentID);
    if (!previousAgent) {
      console.warn('⚠️ Could not load previous agent - marking complete');
    const agentName = previousAgent.Name || 'Agent';
    let previousPayload: any = null;
    let previousArtifactInfo: {artifactId: string; versionId: string; versionNumber: number} | null = null;
    // Use pre-loaded artifact data (no DB queries!)
    // Check both user-visible and system artifacts
    let artifacts = this.artifactsByDetailId?.get(lastAIMessage.ID);
      artifacts = this.systemArtifactsByDetailId?.get(lastAIMessage.ID);
        // Use the first artifact (should only be one OUTPUT per message)
          previousPayload = JSON.parse(version.Content);
          previousArtifactInfo = {
          console.log('📦 Loaded previous OUTPUT artifact as payload for continuity', previousArtifactInfo);
        console.warn('⚠️ Could not parse previous artifact content:', error);
    // Create status message showing agent continuity
    const statusMessage = await this.dataCache.createConversationDetail(this.currentUser);
    statusMessage.ConversationID = conversationId;
    statusMessage.Role = 'AI';
    statusMessage.Message = `Continuing with **${agentName}** for refinement...`;
    statusMessage.ParentID = userMessage.ID;
    statusMessage.Status = 'Complete';
    statusMessage.HiddenToUser = false;
    statusMessage.AgentID = this.converationManagerAgent?.ID || null;
    await statusMessage.Save();
    this.messageSent.emit(statusMessage);
    // Add agent to active tasks
    const taskId = this.activeTasks.add({
      status: 'Processing refinement...',
      conversationDetailId: statusMessage.ID,
      // Invoke the agent with the previous payload
      const continuityResult = await this.agentService.invokeSubAgent(
        'Continuing previous work based on user feedback',
        statusMessage.ID,
        previousPayload,
        this.createProgressCallback(statusMessage, agentName),
        previousArtifactInfo?.artifactId,
        previousArtifactInfo?.versionId
      // Remove from active tasks
      if (continuityResult && continuityResult.success) {
        agentResponseMessage.Message = continuityResult.agentRun?.Message || `✅ **${agentName}** completed refinement`;
        agentResponseMessage.ParentID = statusMessage.ID;
        agentResponseMessage.Status = 'Complete';
        agentResponseMessage.AgentID = lastAIMessage.AgentID;
        // Server created artifacts (handles versioning automatically) - emit event to trigger UI reload
        if (continuityResult.payload && Object.keys(continuityResult.payload).length > 0) {
          console.log('🎨 Server created artifact (versioned) from agent continuity');
        // Agent failed
        statusMessage.Error = continuityResult?.agentRun?.ErrorMessage || null;
        await this.updateConversationDetail(statusMessage, `❌ **${agentName}** failed during refinement\n\n${continuityResult?.agentRun?.ErrorMessage || 'Unknown error'}`, 'Error');
      console.error(`❌ Error in agent continuity with ${agentName}:`, error);
      statusMessage.Error = String(error);
      await this.updateConversationDetail(statusMessage, `❌ **${agentName}** encountered an error\n\n${String(error)}`, 'Error');
   * Invoke an agent directly when mentioned with @ symbol
   * Bypasses Sage completely - no status messages
  private async invokeAgentDirectly(
    const agentName = agentMention.name;
      status: 'Processing...',
      conversationDetailId: userMessage.ID,
    // Declare agentResponseMessage outside try block so it's accessible in catch
    let agentResponseMessage: MJConversationDetailEntity | undefined = undefined;
      // User message is sent successfully - mark complete immediately
      // (No UI uses User message 'In-Progress' - only AI messages need that status)
      userMessage.Status = 'Complete';
      await userMessage.Save();
      this.messageSent.emit(userMessage);
      agentResponseMessage = await this.dataCache.createConversationDetail(this.currentUser);
      agentResponseMessage.ParentID = userMessage.ID;
      // Invoke the agent directly
      const result = await this.agentService.invokeSubAgent(
        `User mentioned agent directly with @${agentName}`,
        agentMention.configurationId // Pass configuration preset ID
        if (result.agentRun.AgentID) {
          agentResponseMessage.AgentID = result.agentRun.AgentID;
        // Multi-stage response handling (same logic as ambient Sage)
        // Stage 1: Check for task graph (multi-step orchestration)
          console.log('📋 Task graph detected from @mention, starting task orchestration');
          await this.handleTaskGraphExecution(userMessage, result, conversationId, agentResponseMessage);
        // Stage 2: Check for sub-agent invocation (single-step delegation)
          console.log('🎯 Sub-agent invocation detected from @mention');
          await this.handleSubAgentInvocation(userMessage, result, conversationId, agentResponseMessage);
        // Stage 3: Normal chat response
          await this.updateConversationDetail(agentResponseMessage, result.agentRun?.Message || `✅ **${agentName}** completed`, 'Complete', result)
        // Agent failed - update the existing message instead of creating a new one
        agentResponseMessage.Error = result?.agentRun?.ErrorMessage || null;
        await this.updateConversationDetail(agentResponseMessage, `❌ **@${agentName}** failed\n\n${result?.agentRun?.ErrorMessage || 'Unknown error'}`, 'Error');
      console.error(`❌ Error invoking mentioned agent ${agentName}:`, error);
      // Update the existing agent response message if it was created
      if (agentResponseMessage) {
        agentResponseMessage.Error = String(error);
        await this.updateConversationDetail(agentResponseMessage, `❌ **@${agentName}** encountered an error\n\n${String(error)}`, 'Error');
   * Continue with the same agent from previous message (implicit continuation)
   * Bypasses Sage - no status messages
   * @param targetArtifactVersionId Optional specific artifact version to use as payload (from intent check)
  private async continueWithAgent(
    targetArtifactVersionId?: string
      console.warn('⚠️ Could not load agent for continuation - falling back to Sage');
      await this.processMessageThroughAgent(userMessage, { mentions: [], agentMention: null, userMentions: [] });
    const agentName = agent.Name || 'Agent';
    let previousConfigurationId: string | undefined = undefined;
    // Use targetArtifactVersionId if specified (from intent check)
    if (targetArtifactVersionId) {
      // Find the artifact in pre-loaded data (check both user-visible and system artifacts)
      for (const [detailId, artifacts] of (this.artifactsByDetailId?.entries() || [])) {
        const targetArtifact = artifacts.find(a => a.artifactVersionId === targetArtifactVersionId);
        if (targetArtifact) {
            // Lazy load the full version entity to get Content
            const version = await targetArtifact.getVersion();
                artifactId: targetArtifact.artifactId,
                versionId: targetArtifact.artifactVersionId,
                versionNumber: targetArtifact.versionNumber
              console.log('📦 Loaded target artifact version as payload', previousArtifactInfo);
            console.warn('⚠️ Could not load target artifact version:', error);
      // If not found in user-visible artifacts, check system artifacts
      if (!previousPayload && this.systemArtifactsByDetailId) {
        for (const [detailId, artifacts] of this.systemArtifactsByDetailId.entries()) {
                console.log('📦 Loaded target artifact version as payload (from system artifacts)', previousArtifactInfo);
    // Extract configuration preset from the User message that @mentioned this agent
    // Uses the shared helper method in the agent service
    previousConfigurationId = this.agentService.findConfigurationPresetFromHistory(agentId, this.conversationHistory);
    // Fall back to searching through all agent messages for an artifact
    // This ensures payload continuity even after clarifying exchanges without artifacts
    if (!previousPayload && agentMessages.length > 0) {
      console.log('📦 Searching through agent messages for most recent artifact...');
        // Get artifacts from pre-loaded data (check both user-visible and system artifacts)
              console.log(`📦 Loaded artifact as payload from message ${message.ID}`, previousArtifactInfo);
              break; // Found an artifact, stop searching
            console.warn('⚠️ Could not parse artifact content:', error);
      if (!previousPayload) {
        console.log(`📦 No artifact found after searching ${agentMessages.length} messages from agent`);
    // Execute the agent with the gathered context
    await this.executeAgentContinuation(
      previousArtifactInfo,
      previousConfigurationId
   * Executes agent continuation with all context already gathered.
   * This is the shared execution logic used by both continueWithAgent and direct Sage config path.
   * @param userMessage The user's message entity
   * @param agentId The agent ID to invoke
   * @param agentName The agent's display name
   * @param conversationId The conversation ID
   * @param previousPayload Optional payload from previous artifact
   * @param previousArtifactInfo Optional artifact info (id, versionId, versionNumber)
   * @param configurationId Optional configuration preset ID to use
  private async executeAgentContinuation(
    previousPayload: Record<string, unknown> | null,
    previousArtifactInfo: {artifactId: string; versionId: string; versionNumber: number} | null,
      agentResponseMessage.AgentID = agentId;
      // Invoke the agent directly (continuation) with previous payload if available
        'Continuing previous conversation with user',
        previousPayload, // Pass previous OUTPUT artifact payload for continuity
        previousArtifactInfo?.versionId,
        configurationId // Pass configuration for continuity
        await this.updateConversationDetail(agentResponseMessage,result.agentRun?.Message || `✅ **${agentName}** completed`, 'Complete', result);
        // Server created artifacts (handles versioning) - emit event to trigger UI reload
        await this.updateConversationDetail(agentResponseMessage, `❌ **${agentName}** failed\n\n${result?.agentRun?.ErrorMessage || 'Unknown error'}`, 'Error');
      console.error(`❌ Error continuing with agent ${agentName}:`, error);
        await this.updateConversationDetail(agentResponseMessage, `❌ **${agentName}** encountered an error\n\n${String(error)}`, 'Error');
   * Name the conversation based on the first message using GraphQL AI client
   * IMPORTANT: This runs asynchronously in the background and has a 30-second timeout
   * to prevent long delays. Failures are logged but don't affect the user experience.
  private async nameConversation(message: string): Promise<void> {
      // Load the Name Conversation prompt to get its ID
      const p = AIEngineBase.Instance.Prompts.find(pr => pr.Name === 'Name Conversation');
      if (!p) {
        console.warn('⚠️ Name Conversation prompt not found');
      const promptId = p.ID;
      // Use GraphQL AI client to run the prompt (same client as agent)
        console.warn('⚠️ GraphQLDataProvider not available');
      // Convert message to plain text (strips JSON-encoded mentions like @{"id":"...","name":"Sage"} to @Sage)
      const plainTextMessage = this.mentionParser.toPlainText(
      const aiClient = new GraphQLAIClient(provider);
      // Add 30-second timeout to prevent long delays
      // If this times out, the conversation will keep its default name
      const timeoutPromise = new Promise<never>((_, reject) => {
        setTimeout(() => reject(new Error('Conversation naming timed out after 30 seconds')), 30000);
      const result = await Promise.race([
        aiClient.RunAIPrompt({
          messages: [{ role: 'user', content: plainTextMessage }],
        timeoutPromise
      if (result && result.success && (result.parsedResult || result.output)) {
        // Use parsedResult if available, otherwise parse output
        const parsed = result.parsedResult ||
          (result.output ? JSON.parse(result.output) : null);
            // Update the conversation name and description in database AND state immediately
              { Name: name, Description: description || '' },
            // Emit event for animation in conversation list
            this.conversationRenamed.emit({
            console.log(`✅ Conversation renamed to: "${name}"`);
        console.warn('⚠️ Failed to generate conversation name - using default');
      // Log timeout or other errors but don't disrupt user experience
      if (error instanceof Error && error.message.includes('timed out')) {
        console.warn('⏱️ Conversation naming timed out - conversation will keep default name');
        console.error('❌ Error naming conversation:', error);
      // Don't show error to user - naming failures should be silent
   * Marks a conversation detail as complete and records timestamp to prevent race conditions
   * Emits event to parent to refresh agent run data from database
  private markMessageComplete(conversationDetail: MJConversationDetailEntity): void {
    this.completionTimestamps.set(conversationDetail.ID, now);
    // Unregister streaming callback for this message (no more updates needed)
    const callback = this.registeredCallbacks.get(conversationDetail.ID);
      this.streamingService.unregisterMessageCallback(conversationDetail.ID, callback);
      this.registeredCallbacks.delete(conversationDetail.ID);
      console.log(`[MarkComplete] Unregistered streaming callback for completed message ${conversationDetail.ID}`);
    // Remove task from active tasks if it exists
    const task = this.activeTasks.getByConversationDetailId(conversationDetail.ID);
      console.log(`✅ Task found for message ${conversationDetail.ID} - removing from active tasks:`, {
        agentName: task.agentName,
        conversationId: task.conversationId,
        conversationName: task.conversationName
      // Show completion notification
      MJNotificationService.Instance?.CreateSimpleNotification(
        `${task.agentName} completed in ${task.conversationName || 'conversation'}`,
      console.warn(`⚠️ No task found for completed message ${conversationDetail.ID} - task may have been removed prematurely or not added`);
    // Emit completion event to parent so it can refresh agent run data
    this.messageComplete.emit({
      agentId: conversationDetail.AgentID || undefined
   * Cleans up completion timestamps for completed messages (prevents memory leak)
  private cleanupCompletionTimestamp(conversationDetailId: string): void {
    // Keep timestamp for a short period to catch any late progress updates
      this.completionTimestamps.delete(conversationDetailId);
    }, 5000); // 5 seconds should be more than enough
