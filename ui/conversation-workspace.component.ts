 * ============================================================================
 * DEPRECATED - DO NOT USE
 * This workspace component was used when conversations, collections, and tasks
 * were all combined into a single tabbed interface.
 * The new architecture uses SEPARATE resource components for each feature:
 * - ChatConversationsResource for conversations
 * - CollectionsResource for collections
 * - TasksResource for tasks
 * These resource components are located in @memberjunction/ng-explorer-core
 * and integrate with MJExplorer's tab/navigation system.
 * This file is kept for backwards compatibility only. Any new features or
 * bug fixes should be made to the individual resource components instead.
  DoCheck,
import { MJConversationEntity, MJArtifactEntity, MJTaskEntity, ArtifactMetadataEngine, MJUserSettingEntity, UserInfoEngine } from '@memberjunction/core-entities';
import { UserInfo, CompositeKey, KeyValuePair, Metadata } from '@memberjunction/core';
import { NavigationTab, WorkspaceLayout } from '../../models/conversation-state.model';
import { SearchResult } from '../../services/search.service';
import { ActionableCommand, AutomaticCommand } from '@memberjunction/ai-core-plus';
 * Top-level workspace component for conversations
 * Provides 3-column Slack-style layout: Navigation | Sidebar | Chat Area | Artifact Panel
 * Supports context-based navigation (library or task views)
 * @deprecated Use ChatConversationsResource from @memberjunction/ng-explorer-core instead.
 * This component is maintained for backwards compatibility but the resource-wrapper pattern
 * is the preferred approach for MJExplorer integration.
  selector: 'mj-conversation-workspace',
  templateUrl: './conversation-workspace.component.html',
  styleUrls: ['./conversation-workspace.component.css']
export class ConversationWorkspaceComponent extends BaseAngularComponent implements OnInit, OnDestroy, DoCheck {
  @Input() initialConversationId?: string;
  @Input() layout: WorkspaceLayout = 'full';
  @Input() activeContext?: 'library' | 'task';
  @Input() contextItemId?: string;
  // Navigation properties for external control (deep linking from URL)
  @Input() set activeTabInput(value: 'conversations' | 'collections' | 'tasks' | undefined) {
    if (value && value !== this.activeTab) {
      this.activeTab = value;
  @Input() set activeConversationInput(value: string | undefined) {
    if (value && value !== this.selectedConversationId) {
      console.log('🔗 Deep link to conversation:', value);
      this.activeTab = 'conversations';
      this.setActiveConversation(value);
  @Input() set activeCollectionInput(value: string | undefined) {
    if (value && value !== this.collectionState.activeCollectionId) {
      console.log('🔗 Deep link to collection:', value);
      this.activeTab = 'collections';
      this.collectionState.setActiveCollection(value);
  @Input() set activeVersionIdInput(value: string | undefined) {
    if (value && value !== this.activeVersionId) {
      console.log('🔗 Deep link to version:', value);
      // Store the version ID immediately to prevent ngDoCheck from clearing it
      this.activeVersionId = value;
      // Open artifact by version ID
      this.artifactState.openArtifactByVersionId(value);
  @Input() set activeTaskInput(value: string | undefined) {
    if (value && value !== this._activeTaskId) {
      this._activeTaskId = value;
  private _activeTaskId?: string;
  get activeTaskId(): string | undefined {
    return this._activeTaskId;
  @Output() conversationChanged = new EventEmitter<MJConversationEntity>();
  @Output() artifactOpened = new EventEmitter<MJArtifactEntity>();
  @Output() navigationChanged = new EventEmitter<{
    tab: 'conversations' | 'collections' | 'tasks';
    collectionId?: string;
  @Output() newConversationStarted = new EventEmitter<void>();
  @Output() actionableCommandExecuted = new EventEmitter<ActionableCommand>();
  @Output() automaticCommandExecuted = new EventEmitter<AutomaticCommand>();
  public activeTab: NavigationTab = 'conversations';
  public isSidebarVisible: boolean = true;
  public isSearchPanelOpen: boolean = false;
  public isWorkspaceReady: boolean = false;
  public activeVersionId: string | null = null;
  public isSidebarPinned: boolean = false; // Default unpinned until settings load (prevents flicker)
  // Resize state - Sidebar
  public sidebarWidth: number = 260; // Default width
  public isSidebarCollapsed: boolean = true; // Default collapsed until settings load (prevents flicker)
  public isSidebarSettingsLoaded: boolean = false; // Tracks whether settings have been loaded (prevents render before state is known)
  // Resize state - Artifact Panel
  public artifactPanelWidth: number = 40; // Default 40% width
  private isArtifactPanelResizing: boolean = false;
  private artifactPanelResizeStartX: number = 0;
  private artifactPanelResizeStartWidth: number = 0;
  private previousConversationId: string | null = null;
  private previousTaskId: string | undefined = undefined;
  private previousVersionId: string | null = null; // Used to track version changes in ngDoCheck
  private previousIsNewConversation: boolean = false; // Track new conversation state changes
  // User Settings key for server-side persistence
  private isLoadingSettings: boolean = false;
  // Task filter for conversation-specific filtering
  public tasksFilter: string = '1=1';
  // LOCAL CONVERSATION STATE - enables multiple workspace instances
  // Each workspace manages its own selection state independently
    public artifactState: ArtifactStateService,
    private uiCommandHandler: UICommandHandlerService,
  // LOCAL CONVERSATION STATE MANAGEMENT
  // These methods manage the workspace's local selection state
   * Sets the active conversation for this workspace instance
   * @param id The conversation ID to activate (or null to clear)
  setActiveConversation(id: string | null): void {
    console.log('🎯 Setting active conversation:', id);
    this.selectedConversationId = id;
    this.selectedConversation = id ? this.conversationData.getConversationById(id) : null;
    // Clear unsaved state when switching to an existing conversation
   * Initiates a new unsaved conversation (doesn't create DB record yet)
   * This shows the welcome screen and delays DB creation until first message
  startNewConversation(): void {
    console.log('✨ Starting new unsaved conversation');
   * Clears the new unsaved conversation state
   * Called when the conversation is actually created or cancelled
  clearNewConversationState(): void {
   * Opens a thread panel for a specific message
   * @param messageId The parent message ID
  openThread(messageId: string): void {
    this.selectedThreadId = messageId;
   * Closes the currently open thread panel
  closeThread(): void {
   * Handler for conversation selection from sidebar/list
  onConversationSelected(conversationId: string): void {
    this.setActiveConversation(conversationId);
   * Handler for new conversation creation from chat area
   * Now includes pending message and attachments to ensure atomic state update
  onConversationCreated(event: {
      // This ensures the new message-input component receives the pending data
      // The conversation is already added to conversationData by the chat area
      console.error('onConversationCreated ERROR:', error);
   * Handler for pending message requested from chat area (empty state)
   * @deprecated Use onConversationCreated with pendingMessage instead
    this.pendingAttachmentsToSend = event.attachments;
   * Handler for thread opened from chat area
   * Handler for thread closed from chat area
    // Initialize global streaming service FIRST
    // This establishes the single PubSub connection for all conversations
    console.log('✅ Global streaming service initialized');
    // Subscribe to command events from UI Command Handler service
    // These will be bubbled up to the host application
      .subscribe(command => {
        this.onActionableCommand(command);
    this.uiCommandHandler.automaticCommandRequested
        this.onAutomaticCommand(command);
    // Check initial mobile state FIRST
    // Load sidebar state - but on mobile, always default to collapsed
      this.isSidebarVisible = false;
      this.isSidebarSettingsLoaded = true; // Mobile doesn't need to load settings
      // Load from User Settings (async) - await before continuing to prevent flicker
      await this.loadSidebarState();
    // Setup touch listeners for mobile
    // CRITICAL: Initialize engines FIRST before rendering any UI
    // The isWorkspaceReady flag blocks all child components from rendering
    // until engines are fully loaded and ready
      // Load both engines in parallel - ArtifactMetadataEngine is lightweight (just artifact types)
      // Using Promise.all ensures optimal performance with no additional delay
        ArtifactMetadataEngine.Instance.Config(false)
      console.log('✅ AI Engine initialized with', AIEngineBase.Instance.Agents?.length || 0, 'agents');
      console.log('✅ Artifact Metadata Engine initialized with',
        ArtifactMetadataEngine.Instance.ArtifactTypes?.length || 0, 'artifact types');
      // Initialize mention autocomplete service immediately after AI engine
      // This ensures the cache is built from the fully-loaded agent list
      console.log('✅ Mention autocomplete initialized');
      // Mark workspace as ready - this allows UI to render
      this.isWorkspaceReady = true;
      console.error('❌ Failed to initialize engines:', error);
    // Subscribe to artifact panel state
        // Load permissions when artifact changes
    // Set initial conversation if provided
    if (this.initialConversationId) {
      this.setActiveConversation(this.initialConversationId);
    // Handle context-based navigation
    if (this.activeContext === 'library') {
    // Task context will be handled by chat header dropdown, not navigation tabs
    // Build task filter for conversations domain
    this.buildTasksFilter();
   * Builds the SQL filter for tasks in conversations the user has access to
  private buildTasksFilter(): void {
    // Filter tasks by conversations the user owns or is a participant in, or tasks owned
    // by the user
    const cd = md.EntityByName('MJ: Conversation Details');
    const c = md.EntityByName('MJ: Conversations');
    if (!cd || !c) {
      console.warn('⚠️ Missing metadata for Conversations or Conversation Details');
      this.tasksFilter = `ParentID IS NULL AND UserID = '${this.currentUser.ID}'`; // Fallback to user-owned tasks only
    this.tasksFilter = `ParentID IS NULL AND (UserID = '${this.currentUser.ID}' OR ConversationDetailID IN (
      SELECT ID FROM [${cd.SchemaName}].[${cd.BaseView}] 
      WHERE 
      UserID ='${this.currentUser.ID}' OR 
      ConversationID IN (
        SELECT ID FROM [${c.SchemaName}].[${c.BaseView}] WHERE UserID='${this.currentUser.ID}'
    ))`;
    console.log('📝 Conversations domain tasks filter built:', this.tasksFilter);
    // Detect new unsaved conversation state changes
    const currentIsNewConversation = this.isNewUnsavedConversation;
    if (currentIsNewConversation !== this.previousIsNewConversation) {
      this.previousIsNewConversation = currentIsNewConversation;
      if (currentIsNewConversation) {
        // Emit event to clear URL conversation parameter
          this.newConversationStarted.emit();
    // Detect conversation changes and emit event
    const currentId = this.selectedConversationId;
    if (currentId !== this.previousConversationId) {
      this.previousConversationId = currentId;
      const conversation = this.selectedConversation;
        this.conversationChanged.emit(conversation);
        // Also emit navigationChanged for URL updates (only if on conversations tab)
        if (this.activeTab === 'conversations' && currentId) {
          // Defer emission until after change detection completes
            this.navigationChanged.emit({
              tab: 'conversations',
              conversationId: currentId
    // Detect task selection changes (when on tasks tab)
    if (this.activeTab === 'tasks') {
      const currentTaskId = this.activeTaskId;
      if (currentTaskId !== this.previousTaskId) {
        this.previousTaskId = currentTaskId;
        if (currentTaskId) {
              tab: 'tasks',
              taskId: currentTaskId
    // Version changes are handled by onCollectionNavigated and deep link inputs
    // We don't need ngDoCheck to track them as it causes double navigation events
    const sidebarElement = target.closest('.workspace-sidebar');
      // Switched to mobile - hide sidebar and default to collapsed
    } else if (!this.isMobileView && wasMobile) {
      // Switched to desktop - show sidebar, restore state from User Settings
      this.isSidebarVisible = true;
   * Expand sidebar (unpinned - will auto-collapse on selection)
   * Save sidebar state to User Settings (server)
   * Includes collapsed, pinned, sidebarWidth, and artifactPanelWidth
        sidebarWidth: this.sidebarWidth,
        artifactPanelWidth: this.artifactPanelWidth
        // Try loading from cached User Settings
          // Load width values if present (with validation)
          if (typeof state.sidebarWidth === 'number' && state.sidebarWidth >= 200 && state.sidebarWidth <= 500) {
            this.sidebarWidth = state.sidebarWidth;
          if (typeof state.artifactPanelWidth === 'number' && state.artifactPanelWidth >= 20 && state.artifactPanelWidth <= 70) {
            this.artifactPanelWidth = state.artifactPanelWidth;
  onTabChanged(tab: NavigationTab): void {
    const wasOnDifferentTab = this.activeTab !== tab;
    // Emit navigation change event with current state
    const navEvent: any = {
      tab: tab as 'conversations' | 'collections' | 'tasks'
    if (tab === 'conversations') {
      navEvent.conversationId = this.selectedConversationId || undefined;
    } else if (tab === 'collections') {
      // If switching TO collections tab from another tab, clear to root level
      if (wasOnDifferentTab) {
        this.activeVersionId = null;
        // Don't include collectionId or versionId - go to root
        // Already on collections tab, preserve current state
        if (this.collectionState.activeCollectionId) {
          navEvent.collectionId = this.collectionState.activeCollectionId;
        if (this.activeVersionId && this.collectionState.activeCollectionId) {
          navEvent.versionId = this.activeVersionId;
    } else if (tab === 'tasks') {
      navEvent.taskId = this.activeTaskId || undefined;
    this.navigationChanged.emit(navEvent);
    // Auto-close artifact panel when switching away from collections
    if (tab === 'conversations' || tab === 'tasks') {
    this.isSidebarVisible = !this.isSidebarVisible;
  closeSidebar(): void {
    if (this.isMobileView && this.isSidebarVisible) {
  openSearch(): void {
    this.isSearchPanelOpen = true;
    this.isSearchPanelOpen = false;
  async onRefreshAgentCache(): Promise<void> {
      await AIEngineBase.Instance.Config(true);
      // Refresh the mention autocomplete service to pick up new agents
      await this.mentionAutocompleteService.refresh(this.currentUser);
      const agentCount = AIEngineBase.Instance.Agents?.length || 0;
      this.notificationService.CreateSimpleNotification(`Agent cache refreshed (${agentCount} agents)`, 'success', 3000);
      this.notificationService.CreateSimpleNotification('Failed to refresh agent cache', 'error', 3000);
      console.error('Failed to refresh AI Engine:', error);
  handleSearchResult(result: SearchResult): void {
    console.log('🔍 Navigating to search result:', result);
    switch (result.type) {
        // Switch to conversations tab and select conversation
        this.setActiveConversation(result.id);
          conversationId: result.id
        // Switch to conversations tab, open conversation, and scroll to message (future enhancement)
        if (result.conversationId) {
          this.setActiveConversation(result.conversationId);
            conversationId: result.conversationId
            // TODO: Add messageId for scroll-to support in future
        // Switch to collections tab and open artifact
        this.artifactState.openArtifact(result.id);
        // If artifact is in a collection, navigate to that collection
        const collectionId = result.collectionId || undefined;
        // Search results don't have version ID, so just navigate to collection
        // The artifact will open with latest version
          tab: 'collections',
          collectionId
        // Switch to collections tab and navigate to collection
        this.collectionState.setActiveCollection(result.id);
          collectionId: result.id
        // Switch to tasks tab and select task
        this.activeTab = 'tasks';
        this._activeTaskId = result.id;
          taskId: result.id
    // Close search panel after navigation
    this.closeSearch();
   * Sidebar resize methods
   * Artifact panel resize methods
  onArtifactPanelResizeStart(event: MouseEvent): void {
    this.isArtifactPanelResizing = true;
    this.artifactPanelResizeStartX = event.clientX;
    this.artifactPanelResizeStartWidth = this.artifactPanelWidth;
    if (this.isSidebarResizing) {
      const deltaX = event.clientX - this.sidebarResizeStartX;
      let newWidth = this.sidebarResizeStartWidth + deltaX;
      // Constrain between 200px and 500px
      newWidth = Math.max(200, Math.min(500, newWidth));
      this.sidebarWidth = newWidth;
    } else if (this.isArtifactPanelResizing) {
      const container = document.querySelector('.workspace-content') as HTMLElement;
      const deltaX = event.clientX - this.artifactPanelResizeStartX;
      let newWidth = this.artifactPanelResizeStartWidth + deltaPercent;
      this.saveSidebarState(); // Save width to User Settings
      this.isArtifactPanelResizing = false;
  onSidebarResizeTouchStart(event: TouchEvent): void {
    this.sidebarResizeStartX = touch.clientX;
  onArtifactPanelResizeTouchStart(event: TouchEvent): void {
    this.artifactPanelResizeStartX = touch.clientX;
      const deltaX = touch.clientX - this.sidebarResizeStartX;
      const deltaX = touch.clientX - this.artifactPanelResizeStartX;
      const deltaPercent = (deltaX / containerWidth) * -100;
    console.log('✨ Workspace received rename event:', event);
    // Trigger animation in sidebar by setting the ID
    // Clear after animation completes (1500ms)
    // Convert to actionable command and emit
    const firstKeyValue = event.compositeKey.KeyValuePairs[0]?.Value || '';
    const command: ActionableCommand = {
      label: `Open ${event.entityName}`,
      resourceId: firstKeyValue,
    this.actionableCommandExecuted.emit(command);
  onOpenEntityRecordFromTasks(event: {entityName: string; recordId: string}): void {
      resourceId: event.recordId,
    // Switch to Tasks tab and set active task ID
    this._activeTaskId = task.ID;
    // Emit navigation change
      taskId: task.ID
   * Handle collection navigation events
  onCollectionNavigated(event: { collectionId: string | null; versionId?: string | null }): void {
    console.log('📁 Collection navigated:', event);
    // Store the version ID for URL sync
    // CRITICAL: Only update activeVersionId if versionId was explicitly provided in the event
    // If versionId is undefined (not provided), keep the current activeVersionId
    if (event.versionId !== undefined) {
      this.activeVersionId = event.versionId;
    // Otherwise: versionId not provided in event, preserve current activeVersionId
    // IMPORTANT: Don't emit navigationChanged here when doing programmatic navigation (deep linking)
    // The artifact state is managed separately
    // Only emit if the event explicitly includes a versionId, or if we're intentionally closing the artifact
      // Event explicitly specifies artifact state (user clicked artifact or intentionally closed it)
        collectionId: event.collectionId || undefined,
        versionId: event.versionId || undefined
    } else if (!this.activeVersionId) {
      // No artifact currently open, safe to emit collection-only navigation
        collectionId: event.collectionId || undefined
    // Otherwise: artifact is open but event doesn't specify versionId
    // Don't emit - preserve current URL state with artifact
   * Handle navigation from artifact links
  onArtifactLinkNavigation(event: {type: 'conversation' | 'collection'; id: string; artifactId?: string; versionNumber?: number; versionId?: string}): void {
    console.log('🔗 Navigating from artifact link:', event);
      // Close collection artifact viewer if it's open
      // Store pending artifact info so chat area can show it and scroll to message
        this.pendingArtifactId = event.artifactId;
        this.pendingArtifactVersionNumber = event.versionNumber || null;
        console.log('📦 Pending artifact set:', event.artifactId, 'v' + event.versionNumber);
      this.setActiveConversation(event.id);
        conversationId: event.id
    } else if (event.type === 'collection') {
      this.collectionState.setActiveCollection(event.id);
      // Open the artifact automatically when navigating to the collection
        this.artifactState.openArtifact(event.artifactId, event.versionNumber);
      // Store version ID for URL sync (same as viewArtifact does)
      if (event.versionId) {
      // Emit navigation with version ID so URL includes it
        collectionId: event.id,
        versionId: event.versionId
      await this.loadArtifactPermissions(this.activeArtifactId);
   * Handle actionable command execution from child components
   * Bubbles up to host application for handling
  onActionableCommand(command: ActionableCommand): void {
    console.log('📤 Bubbling up actionable command:', command);
   * Handle automatic command execution from child components
  onAutomaticCommand(command: AutomaticCommand): void {
    console.log('📤 Bubbling up automatic command:', command);
    this.automaticCommandExecuted.emit(command);
