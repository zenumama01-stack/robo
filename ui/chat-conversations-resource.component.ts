import { Component, ViewEncapsulation, OnDestroy, ViewChild, ChangeDetectorRef, HostListener } from '@angular/core';
import { ResourceData, EnvironmentEntityExtended, MJConversationEntity, MJUserSettingEntity, UserInfoEngine } from '@memberjunction/core-entities';
import { ConversationDataService, ConversationChatAreaComponent, ConversationListComponent, MentionAutocompleteService, ConversationStreamingService, ActiveTasksService, PendingAttachment, UICommandHandlerService } from '@memberjunction/ng-conversations';
import { ActionableCommand, OpenResourceCommand } from '@memberjunction/ai-core-plus';
import { Subject, takeUntil, filter } from 'rxjs';
 * Chat Conversations Resource - wraps the conversation chat area for tab-based display
 * Displays conversation list sidebar + active conversation chat interface
 * Designed to work with the tab system for multi-tab conversation management
 * This component manages its own selection state locally, following the encapsulation pattern:
 * - Services (ConversationDataService) are used for shared DATA (caching, loading, saving)
 * - Local state variables manage SELECTION state (which conversation is active)
 * - State flows down to children via @Input, events flow up via @Output
@RegisterClass(BaseResourceComponent, 'ChatConversationsResource')
  selector: 'mj-chat-conversations-resource',
    @if (isReady) {
      <div class="chat-conversations-container">
        <!-- Left sidebar: Conversation list -->
        @if (isSidebarSettingsLoaded) {
          <div class="conversation-sidebar"
            [class.collapsed]="isSidebarCollapsed"
            [class.no-transition]="!sidebarTransitionsEnabled"
            [style.width.px]="isSidebarCollapsed ? 0 : sidebarWidth">
              <mj-conversation-list
                #conversationList
                [selectedConversationId]="selectedConversationId"
                [renamedConversationId]="renamedConversationId"
                [isSidebarPinned]="isSidebarPinned"
                [isMobileView]="isMobileView"
                (conversationSelected)="onConversationSelected($event)"
                (newConversationRequested)="onNewConversationRequested()"
                (pinSidebarRequested)="pinSidebar()"
                (unpinSidebarRequested)="unpinSidebar()">
              </mj-conversation-list>
        <!-- Resize handle for sidebar (only when expanded and settings loaded) -->
        @if (!isSidebarCollapsed && isSidebarSettingsLoaded) {
          <div class="sidebar-resize-handle"
          (mousedown)="onSidebarResizeStart($event)"></div>
        <!-- Main area: Chat interface -->
        <div class="conversation-main">
            <mj-conversation-chat-area
              #chatArea
              [conversationId]="selectedConversationId"
              [conversation]="selectedConversation"
              [threadId]="selectedThreadId"
              [isNewConversation]="isNewUnsavedConversation"
              [pendingMessage]="pendingMessageToSend"
              [pendingAttachments]="pendingAttachmentsToSend"
              [pendingArtifactId]="pendingArtifactId"
              [pendingArtifactVersionNumber]="pendingArtifactVersionNumber"
              [showSidebarToggle]="isSidebarCollapsed && isSidebarSettingsLoaded"
              (sidebarToggleClicked)="expandSidebar()"
              (conversationRenamed)="onConversationRenamed($event)"
              (conversationCreated)="onConversationCreated($event)"
              (threadOpened)="onThreadOpened($event)"
              (threadClosed)="onThreadClosed()"
              (pendingArtifactConsumed)="onPendingArtifactConsumed()"
              (pendingMessageConsumed)="onPendingMessageConsumed()"
              (pendingMessageRequested)="onPendingMessageRequested($event)"
              (artifactLinkClicked)="onArtifactLinkClicked($event)"
            </mj-conversation-chat-area>
      <div class="initializing-container">
        <mj-loading text="Initializing..." size="large"></mj-loading>
    <!-- Toast notifications container -->
    <mj-toast></mj-toast>
    .chat-conversations-container {
    .conversation-sidebar {
    /* Disable transitions during initial load to prevent jarring animation */
    .conversation-sidebar.no-transition {
    .conversation-sidebar.collapsed {
      width: 0 !important;
    /* Resize handle for sidebar */
    .sidebar-resize-handle {
    .sidebar-resize-handle:hover {
      background: #1e40af;
    .sidebar-resize-handle:active {
      background: #1e3a8a;
    .sidebar-resize-handle::before {
      left: -4px;
    .conversation-main {
    .initializing-container {
export class ChatConversationsResource extends BaseResourceComponent implements OnDestroy {
  @ViewChild('conversationList') conversationList?: ConversationListComponent;
  @ViewChild('chatArea') chatArea?: ConversationChatAreaComponent;
  // Ready flag - blocks child rendering until AIEngine is initialized
  public isReady: boolean = false;
  // LOCAL SELECTION STATE - each wrapper instance manages its own selection
  public selectedConversationId: string | null = null;
  public selectedConversation: MJConversationEntity | null = null;
  public selectedThreadId: string | null = null;
  public isNewUnsavedConversation: boolean = false;
  public renamedConversationId: string | null = null;
  public isSidebarCollapsed: boolean = false;
  public isSidebarPinned: boolean = true; // Whether sidebar stays open after selection
  public isMobileView: boolean = false;
  public sidebarTransitionsEnabled: boolean = false; // Disabled during initial load to prevent jarring animation
  public isSidebarSettingsLoaded: boolean = false; // Prevents UI flash while loading settings
  // Sidebar resize state
  public sidebarWidth: number = 300; // Default width in pixels
  private isSidebarResizing: boolean = false;
  private sidebarResizeStartX: number = 0;
  private sidebarResizeStartWidth: number = 0;
  private readonly SIDEBAR_MIN_WIDTH = 200;
  private readonly SIDEBAR_MAX_WIDTH = 500;
  // Pending navigation state
  public pendingArtifactId: string | null = null;
  public pendingArtifactVersionNumber: number | null = null;
  public pendingMessageToSend: string | null = null;
  public pendingAttachmentsToSend: PendingAttachment[] | null = null;
  private readonly USER_SETTING_SIDEBAR_KEY = 'Conversations.SidebarState';
    private conversationData: ConversationDataService,
    private mentionAutocompleteService: MentionAutocompleteService,
    private streamingService: ConversationStreamingService,
    private activeTasksService: ActiveTasksService,
    private uiCommandHandler: UICommandHandlerService
    // Check initial mobile state and set default collapsed
    this.checkMobileView();
    if (this.isMobileView) {
      this.isSidebarCollapsed = true;
      this.isSidebarSettingsLoaded = true; // Mobile uses defaults, no need to load from server
      // Enable transitions after a brief delay to ensure initial state is applied
        this.sidebarTransitionsEnabled = true;
      // Load sidebar state from User Settings (non-blocking)
      this.loadSidebarState().then(() => {
        // Enable transitions after state is loaded and applied
    // CRITICAL: Initialize AIEngine and mention service BEFORE children render
    // This prevents the slow first-load issue where each child would trigger initialization
    await this.initializeEngines();
    // Initialize global streaming service for PubSub updates
    // This enables reconnection to in-progress agents after browser refresh
    this.streamingService.initialize();
    // CRITICAL: Set selectedConversationId SYNCHRONOUSLY before child components initialize
    // Parse URL first and apply state synchronously for the ID
      // Set conversationId synchronously so child components see it immediately
      if (urlState.conversationId) {
        this.selectedConversationId = urlState.conversationId;
        this.isNewUnsavedConversation = false;
      if (urlState.artifactId) {
        this.pendingArtifactId = urlState.artifactId;
        this.pendingArtifactVersionNumber = urlState.versionNumber || null;
      // Load the conversation entity asynchronously (non-blocking)
      this.loadConversationEntity(urlState.conversationId);
      // Check if we have navigation params from config (e.g., from Collections linking here)
    // Subscribe to actionable commands (open:resource) from the UI command handler service.
    // open:url commands are handled directly by the service; open:resource needs NavigationService.
    this.uiCommandHandler.actionableCommandRequested
      .subscribe(command => this.handleActionableCommand(command));
    // Clean up resize event listeners
    document.removeEventListener('mousemove', this.onSidebarResizeMove);
    document.removeEventListener('mouseup', this.onSidebarResizeEnd);
   * Initialize AI Engine, conversations, and services BEFORE child components render.
   * This prevents the slow first-load issue where initialization would block conversation loading.
   * The `false` parameter means "don't force refresh if already initialized".
  private async initializeEngines(): Promise<void> {
      // Initialize AIEngine, conversations, and mention service in parallel
        this.conversationData.loadConversations(this.environmentId, this.currentUser),
        this.mentionAutocompleteService.initialize(this.currentUser)
      // Restore active tasks AFTER conversations are cached (uses in-memory lookup)
      await this.activeTasksService.restoreFromDatabase(this.currentUser);
      // Mark as ready - child components can now render
      this.isReady = true;
      console.error('Failed to initialize AI engines:', error);
      // Still mark as ready so UI isn't blocked forever
   * Parse URL query string for conversation state.
   * Query params: conversationId, artifactId, versionNumber
  private parseUrlState(): { conversationId?: string; artifactId?: string; versionNumber?: number } | null {
    const conversationId = params.get('conversationId');
    if (!conversationId && !artifactId) return null;
      conversationId: conversationId || undefined,
   * Load the conversation entity asynchronously (non-blocking).
   * The conversationId is already set synchronously, this just loads the full entity.
  private async loadConversationEntity(conversationId: string | undefined): Promise<void> {
    if (!conversationId) return;
    // Try to get from cache first
    const conversation = this.conversationData.getConversationById(conversationId);
    if (conversation) {
      this.selectedConversation = conversation;
    // If not in cache, the chat area component will handle loading it
   * Apply configuration params from resource data (e.g., from deep-linking via Collections).
   * Sets state synchronously so child components see values immediately.
    // Set pending artifact if provided
      this.pendingArtifactId = config.artifactId as string;
      this.pendingArtifactVersionNumber = (config.versionNumber as number) || null;
    if (config.conversationId) {
      this.selectedConversationId = config.conversationId as string;
      // Load entity asynchronously
      this.loadConversationEntity(config.conversationId as string);
   * Apply navigation state to local selection state.
  private applyNavigationState(state: { conversationId?: string; artifactId?: string; versionNumber?: number }): void {
    // Set pending artifact if provided (will be consumed by chat area after loading)
      this.pendingArtifactId = state.artifactId;
      this.pendingArtifactVersionNumber = state.versionNumber || null;
    // Set the conversation synchronously
    if (state.conversationId) {
      this.selectedConversationId = state.conversationId;
      this.loadConversationEntity(state.conversationId);
   * Select a conversation by ID - loads the entity and updates local state
  private async selectConversation(conversationId: string): Promise<void> {
    this.selectedConversationId = conversationId;
    // Load the conversation entity from data service
      // Conversation might not be loaded yet - the chat area will handle loading
      this.selectedConversation = null;
    // Update URL if not skipping
    // Add conversation ID
    if (this.selectedConversationId) {
      queryParams['conversationId'] = this.selectedConversationId;
      queryParams['conversationId'] = null;
    // Add artifact ID if we have a pending artifact (will be cleared once opened)
    if (this.pendingArtifactId) {
      queryParams['artifactId'] = this.pendingArtifactId;
      if (this.pendingArtifactVersionNumber) {
        queryParams['versionNumber'] = this.pendingArtifactVersionNumber.toString();
      this.applyNavigationState(urlState);
      this.selectedConversationId = null;
      this.selectedThreadId = null;
      this.pendingArtifactId = null;
      this.pendingArtifactVersionNumber = null;
  private parseUrlFromString(url: string): { conversationId?: string; artifactId?: string; versionNumber?: number } | null {
   * Get the display name for chat conversations
    // If we have a specific conversation ID, we could load the conversation name
    // For now, just return a generic name
    if (data.Configuration?.conversationId) {
      return `Conversation: ${(data.Configuration.conversationId as string).substring(0, 8)}...`;
    return 'Conversations';
   * Get the icon class for chat conversations
  // ============================================
  // EVENT HANDLERS FROM CHILD COMPONENTS
   * Handle conversation selection from the list
  async onConversationSelected(conversationId: string): Promise<void> {
    await this.selectConversation(conversationId);
    this.selectedThreadId = null; // Clear thread when switching conversations
    // Auto-collapse if mobile OR if sidebar is not pinned
    if (this.isMobileView || !this.isSidebarPinned) {
      this.collapseSidebar();
   * Handle clicks outside the sidebar to auto-collapse when unpinned
  onDocumentClick(event: MouseEvent): void {
    // Only handle when sidebar is expanded but unpinned
    if (this.isSidebarCollapsed || this.isSidebarPinned) {
    // Check if click is outside the sidebar
    const sidebarElement = target.closest('.conversation-sidebar');
    const expandHandle = target.closest('.sidebar-expand-handle');
    // If click is outside sidebar and expand handle, collapse it
    if (!sidebarElement && !expandHandle) {
   * Check if we're in mobile view and handle state accordingly
  private checkMobileView(): void {
    const wasMobile = this.isMobileView;
    this.isMobileView = window.innerWidth < 768;
    if (this.isMobileView && !wasMobile) {
      // Switched to mobile - default to collapsed
   * Collapse sidebar
  collapseSidebar(): void {
   * Expand sidebar (pinned - stays open until unpinned)
  expandSidebar(): void {
    this.isSidebarCollapsed = false;
    this.isSidebarPinned = true; // Pin it so it stays open
    this.saveSidebarState();
   * Handle sidebar resize start
  onSidebarResizeStart(event: MouseEvent): void {
    this.isSidebarResizing = true;
    this.sidebarResizeStartX = event.clientX;
    this.sidebarResizeStartWidth = this.sidebarWidth;
    // Disable transitions during resize for immediate feedback
    this.sidebarTransitionsEnabled = false;
    // Add event listeners for mousemove and mouseup
    document.addEventListener('mousemove', this.onSidebarResizeMove);
    document.addEventListener('mouseup', this.onSidebarResizeEnd);
   * Handle sidebar resize move (bound method for proper 'this' context)
  private onSidebarResizeMove = (event: MouseEvent): void => {
    if (!this.isSidebarResizing) return;
    const delta = event.clientX - this.sidebarResizeStartX;
    const newWidth = this.sidebarResizeStartWidth + delta;
    // Clamp to min/max bounds
    this.sidebarWidth = Math.max(this.SIDEBAR_MIN_WIDTH, Math.min(this.SIDEBAR_MAX_WIDTH, newWidth));
   * Handle sidebar resize end (bound method for proper 'this' context)
  private onSidebarResizeEnd = (): void => {
    this.isSidebarResizing = false;
    // Re-enable transitions for collapse/expand animations
    // Remove event listeners
    // Save the new width to User Settings
   * Pin sidebar - keep it open after selection
  pinSidebar(): void {
    this.isSidebarPinned = true;
   * Unpin sidebar - will auto-collapse on next selection
  unpinSidebar(): void {
    this.isSidebarPinned = false;
   * Save sidebar state to User Settings (server only - no localStorage fallback)
   * Uses debouncing to avoid excessive database writes
  private saveSidebarState(): void {
    // Debounce the server save to avoid excessive writes
      this.saveSidebarStateToServer();
   * Save sidebar state to User Settings entity on server using UserInfoEngine for cached lookup
  private async saveSidebarStateToServer(): Promise<void> {
      const userId = this.currentUser?.ID;
        collapsed: this.isSidebarCollapsed,
        pinned: this.isSidebarPinned,
        width: this.sidebarWidth
      let setting = engine.UserSettings.find(s => s.Setting === this.USER_SETTING_SIDEBAR_KEY);
        setting.Setting = this.USER_SETTING_SIDEBAR_KEY;
      console.warn('Failed to save sidebar state to User Settings:', error);
   * Load sidebar state from User Settings (server) using UserInfoEngine
   * For new users with no saved state, defaults to collapsed with new conversation
  private async loadSidebarState(): Promise<void> {
        const setting = engine.UserSettings.find(s => s.Setting === this.USER_SETTING_SIDEBAR_KEY);
          const state = JSON.parse(setting.Value);
          this.isSidebarCollapsed = state.collapsed ?? true;
          this.isSidebarPinned = state.pinned ?? false;
          this.sidebarWidth = state.width ?? 300;
          // Clamp width to valid range
          this.sidebarWidth = Math.max(this.SIDEBAR_MIN_WIDTH, Math.min(this.SIDEBAR_MAX_WIDTH, this.sidebarWidth));
          this.isSidebarSettingsLoaded = true;
      // No saved state found - NEW USER DEFAULT:
      // Start with sidebar collapsed and show new conversation screen
      this.sidebarWidth = 300;
      this.isNewUnsavedConversation = true;
      console.warn('Failed to load sidebar state:', error);
      // Default to collapsed for new users on error
   * Handle new conversation request from the list
  onNewConversationRequested(): void {
   * Handle conversation created from chat area (after first message in new conversation).
   * The event now includes pending message and attachments for atomic state update.
  async onConversationCreated(event: {
    conversation: MJConversationEntity;
    pendingMessage?: string;
    pendingAttachments?: PendingAttachment[];
  }): Promise<void> {
    // Set ALL state atomically before Angular change detection runs
    this.pendingMessageToSend = event.pendingMessage || null;
    this.pendingAttachmentsToSend = event.pendingAttachments || null;
    this.selectedConversationId = event.conversation.ID;
    this.selectedConversation = event.conversation;
   * Handle conversation rename event
  onConversationRenamed(event: { conversationId: string; name: string; description: string }): void {
    // Trigger rename animation in the list
    this.renamedConversationId = event.conversationId;
    // Clear the animation trigger after it completes
      this.renamedConversationId = null;
   * Handle thread opened event
  onThreadOpened(threadId: string): void {
    this.selectedThreadId = threadId;
   * Handle thread closed event
  onThreadClosed(): void {
   * Handle pending artifact consumed event
  onPendingArtifactConsumed(): void {
    // Update URL to remove artifact params
   * Handle pending message consumed event
  onPendingMessageConsumed(): void {
    this.pendingMessageToSend = null;
    this.pendingAttachmentsToSend = null;
   * Handle pending message requested event (from empty state creating conversation).
   * @deprecated Use onConversationCreated with pendingMessage instead - this is kept for backwards compatibility.
  onPendingMessageRequested(event: {text: string; attachments: PendingAttachment[]}): void {
    this.pendingMessageToSend = event.text;
    this.pendingAttachmentsToSend = event.attachments || null;
   * Handle navigation request from artifact viewer panel within the chat area.
  onArtifactLinkClicked(event: {
   * Handle entity record open request from chat area (from React component grids).
   * Handle actionable commands that require app-specific navigation (open:resource).
   * open:url commands are already handled directly by UICommandHandlerService.
  private handleActionableCommand(command: ActionableCommand): void {
    if (command.type === 'open:resource') {
      const resourceCommand = command as OpenResourceCommand;
      if (resourceCommand.resourceType === 'Record' && resourceCommand.entityName) {
        const compositeKey = new CompositeKey([{
          Value: resourceCommand.resourceId
        this.navigationService.OpenEntityRecord(resourceCommand.entityName, compositeKey);
      } else if (resourceCommand.resourceType === 'Report' || resourceCommand.resourceType === 'Dashboard') {
        // Reports and dashboards from agents are stored as conversation artifacts.
        // Find the most recent artifact in the active conversation and open it.
        this.openMostRecentArtifact();
   * Open the most recent artifact in the active conversation's artifact viewer.
   * Used for open:resource commands with resourceType Report/Dashboard where
   * the resourceId is an agent-internal ID, not a database artifact ID.
  private openMostRecentArtifact(): void {
    if (!this.chatArea) return;
    // Find the last artifact across all messages
    const artifactMap = this.chatArea.artifactsByDetailId;
    let latestArtifact: { artifactId: string; versionId?: string } | null = null;
    for (const artifacts of artifactMap.values()) {
      if (artifacts.length > 0) {
        const last = artifacts[artifacts.length - 1];
        latestArtifact = {
          artifactId: last.artifactId,
          versionId: last.artifactVersionId
    if (latestArtifact) {
      this.chatArea.onArtifactClicked(latestArtifact);
      console.warn('No artifacts found in conversation to open for Report/Dashboard command');
