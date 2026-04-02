import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, HostListener, ChangeDetectorRef } from '@angular/core';
import { NotificationService } from '../../services/notification.service';
  selector: 'mj-conversation-list',
    <div class="conversation-list" kendoDialogContainer>
            placeholder="Search conversations..."
            [(ngModel)]="searchQuery">
          @if (!isSelectionMode) {
            <div class="header-menu-container">
              <button class="btn-menu" (click)="toggleHeaderMenu($event)" title="Options">
              @if (isHeaderMenuOpen) {
                <div class="header-dropdown-menu">
                  <button class="dropdown-item" (click)="onRefreshConversationsClick($event)" [disabled]="isRefreshing">
                    <span>{{ isRefreshing ? 'Refreshing...' : 'Refresh' }}</span>
                  <button class="dropdown-item" (click)="onSelectConversationsClick($event)">
                    <span>Select Conversations</span>
                  @if (!isMobileView) {
                    <button class="dropdown-item" (click)="onUnpinSidebarClick($event)">
                      <span>Hide Sidebar</span>
      <button class="btn-new-conversation" (click)="createNewConversation()" title="New Conversation">
        <span>New Conversation</span>
      <div class="list-content">
        <!-- Pinned Section (only show if there are pinned conversations) -->
        @if (pinnedConversations.length > 0) {
          <div class="sidebar-section pinned-section">
            <div class="section-header" [class.expanded]="pinnedExpanded" (click)="togglePinned()">
                <i class="fas fa-thumbtack section-icon"></i>
                <span>Pinned</span>
            <div class="chat-list" [class.expanded]="pinnedExpanded">
              @for (conversation of pinnedConversations; track conversation.ID) {
                <div class="conversation-item"
                     [class.active]="conversation.ID === selectedConversationId"
                     [class.renamed]="conversation.ID === renamedConversationId"
                     (click)="handleConversationClick(conversation)">
                  @if (isSelectionMode) {
                    <div class="conversation-checkbox">
                             [checked]="selectedConversationIds.has(conversation.ID)"
                             (click)="$event.stopPropagation(); toggleConversationSelection(conversation.ID)">
                  <div class="conversation-icon-wrapper">
                    @if (hasActiveTasks(conversation.ID)) {
                      <div class="conversation-icon has-tasks">
                        <i class="fas fa-spinner fa-pulse"></i>
                    <div class="badge-overlay">
                      <mj-notification-badge [conversationId]="conversation.ID"></mj-notification-badge>
                  <div class="conversation-info" [title]="conversation.Name + (conversation.Description ? '\n' + conversation.Description : '')">
                    <div class="conversation-name">{{ conversation.Name }}</div>
                    <div class="conversation-preview">{{ conversation.Description }}</div>
                    <div class="conversation-actions">
                      <button class="menu-btn" (click)="toggleMenu(conversation.ID, $event)" title="More options">
                        <i class="fas fa-ellipsis"></i>
                      @if (openMenuConversationId === conversation.ID) {
                        <div class="context-menu" (click)="$event.stopPropagation()">
                          <button class="menu-item" (click)="togglePin(conversation, $event)">
                            <span>Unpin</span>
                          <button class="menu-item" (click)="renameConversation(conversation); closeMenu()">
                          <button class="menu-item danger" (click)="deleteConversation(conversation); closeMenu()">
        <!-- Messages Section -->
          <div class="section-header" [class.expanded]="directMessagesExpanded" (click)="toggleDirectMessages()">
              <span>Messages</span>
          <div class="chat-list" [class.expanded]="directMessagesExpanded">
            @for (conversation of unpinnedConversations; track conversation.ID) {
                          <span>Pin</span>
      <!-- Selection Action Bar -->
        <div class="selection-action-bar">
            <span class="selection-count">{{ selectedConversationIds.size }} selected</span>
            @if (selectedConversationIds.size < filteredConversations.length) {
              <button class="link-btn" (click)="selectAll()">Select All</button>
              <button class="link-btn" (click)="deselectAll()">Deselect All</button>
            <button class="btn-delete-bulk"
                    (click)="bulkDeleteConversations()"
                    [disabled]="selectedConversationIds.size === 0">
              Delete ({{ selectedConversationIds.size }})
            <button class="btn-cancel" (click)="toggleSelectionMode()">
    :host { display: block; height: 100%; }
    .conversation-list { display: flex; flex-direction: column; height: 100%; background: #092340; }
    .list-header { padding: 8px; border-bottom: 1px solid rgba(255,255,255,0.1); }
    .search-input::placeholder { color: rgba(255,255,255,0.5); }
    .search-input:focus { outline: none; background: rgba(255,255,255,0.15); border-color: #0076B6; }
    .btn-new-conversation {
      width: calc(100% - 16px);
    .btn-new-conversation:hover { background: #005A8C; }
    .btn-new-conversation i { font-size: 14px; }
    .list-content { flex: 1; min-height: 0; overflow-y: auto; padding: 4px 0; }
    .sidebar-section { margin-bottom: 20px; }
    .pinned-section .section-header {
      background: rgba(255, 193, 7, 0.08);
    .pinned-section .section-title .section-icon {
      color: #FFC107;
    .section-header:hover { color: white; }
    .section-header.expanded .section-title i { transform: rotate(90deg); }
    .chat-list.expanded { display: block; }
    .conversation-item {
      padding: 6px 5px 6px 16px;
      min-height: 45px;
    .conversation-item:hover { background: rgba(255,255,255,0.08); color: white; }
    .conversation-item:hover .conversation-actions { opacity: 1; }
    .conversation-item.active { background: #0076B6; color: white; }
    .conversation-icon-wrapper { position: relative; flex-shrink: 0; }
    .conversation-icon { font-size: 12px; width: 16px; text-align: center; }
    .conversation-icon.has-tasks { color: #fb923c; }
    .badge-overlay { position: absolute; top: -4px; right: -4px; }
    .conversation-info { flex: 1; min-width: 0; }
    .conversation-name { font-weight: 600; font-size: 14px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .conversation-preview { font-size: 12px; color: rgba(255,255,255,0.5); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .conversation-item.active .conversation-preview { color: rgba(255,255,255,0.8); }
    .conversation-meta { display: flex; align-items: center; gap: 4px; flex-shrink: 0; }
    .conversation-item:hover .project-badge {
    .conversation-item.active .project-badge {
    .conversation-actions {
      right: 5px;
    .conversation-item:hover .conversation-actions { opacity: 1; pointer-events: auto; }
    .conversation-item.active .conversation-actions { opacity: 1; pointer-events: auto; }
    .conversation-actions > * { pointer-events: auto; }
    .pinned-icon { color: #AAE7FD; font-size: 12px; }
    /* Task Indicator */
    .task-indicator {
      color: #fb923c;
      animation: pulse-glow 2s ease-in-out infinite;
        filter: drop-shadow(0 0 2px #fb923c);
        filter: drop-shadow(0 0 4px #fb923c);
    .conversation-item.active .task-indicator {
      background: #092340 !important;
      background: rgba(255,255,255,0.15) !important;
    .conversation-item.active .menu-btn {
      background: #005A8C !important;
    .menu-btn i { font-size: 14px; }
      background: #0A2742;
      border: 1px solid rgba(255,255,255,0.15);
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.4);
      color: rgba(255,255,255,0.85);
    .menu-item i {
    .menu-item:hover i {
      color: rgba(239, 68, 68, 0.9);
    .menu-item.danger i {
      color: rgba(239, 68, 68, 0.8);
    .menu-item.danger:hover i {
    /* Rename Animation */
    .conversation-item.renamed {
      animation: renameHighlight 1500ms cubic-bezier(0.4, 0, 0.2, 1);
    @keyframes renameHighlight {
        background: linear-gradient(90deg, rgba(59, 130, 246, 0.4), rgba(147, 51, 234, 0.4));
        transform: scale(1.03);
        box-shadow: 0 0 20px rgba(59, 130, 246, 0.5);
      25% {
        background: linear-gradient(90deg, rgba(59, 130, 246, 0.35), rgba(147, 51, 234, 0.35));
        box-shadow: 0 0 15px rgba(59, 130, 246, 0.4);
        background: linear-gradient(90deg, rgba(16, 185, 129, 0.3), rgba(59, 130, 246, 0.3));
        box-shadow: 0 0 10px rgba(16, 185, 129, 0.3);
      75% {
        background: linear-gradient(90deg, rgba(16, 185, 129, 0.2), rgba(59, 130, 246, 0.2));
        box-shadow: 0 0 5px rgba(16, 185, 129, 0.2);
    /* Selection Mode Styles */
    /* Header menu button and dropdown */
    .header-menu-container {
    .btn-menu {
    .btn-menu:hover {
      border-color: rgba(255,255,255,0.3);
    .header-dropdown-menu {
    .header-dropdown-menu .dropdown-item {
    .header-dropdown-menu .dropdown-item:hover {
    .header-dropdown-menu .dropdown-item i {
    .header-dropdown-menu .dropdown-item:hover i {
    .header-dropdown-menu .dropdown-item .shortcut {
      color: rgba(255,255,255,0.4);
    .btn-select {
    .btn-select:hover {
    .conversation-checkbox {
    .conversation-checkbox input[type="checkbox"] {
      accent-color: #0076B6;
    .selection-action-bar {
      border-top: 1px solid rgba(255,255,255,0.15);
    .link-btn {
      color: #AAE7FD;
    .link-btn:hover {
    .btn-delete-bulk {
    .btn-delete-bulk:hover:not(:disabled) {
      background: #B91C1C;
    .btn-delete-bulk:disabled {
    .btn-delete-bulk i {
export class ConversationListComponent implements OnInit, OnDestroy {
  @Input() selectedConversationId: string | null = null;
  @Input() renamedConversationId: string | null = null;
  @Input() isSidebarPinned: boolean = true; // Whether sidebar is pinned (stays open after selection)
  @Input() isMobileView: boolean = false; // Whether we're on mobile (no pin options)
  @Output() conversationSelected = new EventEmitter<string>();
  @Output() newConversationRequested = new EventEmitter<void>();
  @Output() pinSidebarRequested = new EventEmitter<void>(); // Request to pin sidebar
  @Output() unpinSidebarRequested = new EventEmitter<void>(); // Request to unpin (collapse) sidebar
  public directMessagesExpanded: boolean = true;
  public pinnedExpanded: boolean = true;
  public openMenuConversationId: string | null = null;
  public conversationIdsWithTasks = new Set<string>();
  public isSelectionMode: boolean = false;
  public selectedConversationIds = new Set<string>();
  public isHeaderMenuOpen: boolean = false;
  public isRefreshing: boolean = false;
  get filteredConversations(): MJConversationEntity[] {
    if (!this.searchQuery || this.searchQuery.trim() === '') {
      return this.conversationData.conversations;
    const lowerQuery = this.searchQuery.toLowerCase();
    return this.conversationData.conversations.filter(c =>
      (c.Name?.toLowerCase().includes(lowerQuery)) ||
      (c.Description?.toLowerCase().includes(lowerQuery))
  get pinnedConversations() {
    return this.filteredConversations.filter(c => c.IsPinned);
  get unpinnedConversations() {
    return this.filteredConversations.filter(c => !c.IsPinned);
    // Load conversations on init
    this.conversationData.loadConversations(this.environmentId, this.currentUser);
    // Subscribe to conversation IDs with active tasks (hot set)
    this.activeTasksService.conversationIdsWithTasks$.pipe(
    ).subscribe(conversationIds => {
      this.conversationIdsWithTasks = conversationIds;
      this.cdr.detectChanges(); // Force change detection to ensure spinner icons update reliably
  @HostListener('document:click')
  onDocumentClick(): void {
    // Close menus when clicking outside
    if (this.openMenuConversationId) {
      this.closeMenu();
    if (this.isHeaderMenuOpen) {
      this.closeHeaderMenu();
  public toggleHeaderMenu(event: Event): void {
    this.isHeaderMenuOpen = !this.isHeaderMenuOpen;
  public closeHeaderMenu(): void {
    this.isHeaderMenuOpen = false;
  public onSelectConversationsClick(event: Event): void {
    this.toggleSelectionMode();
  public async onRefreshConversationsClick(event: Event): Promise<void> {
    if (this.isRefreshing) return;
      await this.conversationData.refreshConversations(this.environmentId, this.currentUser);
      console.error('Error refreshing conversations:', error);
      await this.dialogService.alert('Error', 'Failed to refresh conversations. Please try again.');
  public onPinSidebarClick(event: Event): void {
    this.pinSidebarRequested.emit();
  public onUnpinSidebarClick(event: Event): void {
    this.unpinSidebarRequested.emit();
  public toggleDirectMessages(): void {
    this.directMessagesExpanded = !this.directMessagesExpanded;
  public togglePinned(): void {
    this.pinnedExpanded = !this.pinnedExpanded;
  selectConversation(conversation: MJConversationEntity): void {
    this.conversationSelected.emit(conversation.ID);
    // Clear unread notifications when conversation is opened
    this.notificationService.markConversationAsRead(conversation.ID);
  async createNewConversation(): Promise<void> {
    // Don't create DB record yet - just show the welcome screen
    // Conversation will be created when user sends first message
    this.newConversationRequested.emit();
  async renameConversation(conversation: MJConversationEntity): Promise<void> {
      const result = await this.dialogService.input({
        title: 'Edit Conversation',
        message: 'Update the name and description for this conversation',
        inputLabel: 'Conversation Name',
        inputValue: conversation.Name || '',
        placeholder: 'My Conversation',
        secondInputLabel: 'Description',
        secondInputValue: conversation.Description || '',
        secondInputPlaceholder: 'Optional description',
        secondInputRequired: false,
        okText: 'Save',
        const newName = typeof result === 'string' ? result : result.value;
        const newDescription = typeof result === 'string' ? conversation.Description : result.secondValue;
        if (newName !== conversation.Name || newDescription !== conversation.Description) {
            conversation.ID,
            { Name: newName, Description: newDescription || '' },
      console.error('Error renaming conversation:', error);
      await this.dialogService.alert('Error', 'Failed to update conversation. Please try again.');
  async deleteConversation(conversation: MJConversationEntity): Promise<void> {
        title: 'Delete Conversation',
        message: `Are you sure you want to delete "${conversation.Name}"? This action cannot be undone.`,
        await this.conversationData.deleteConversation(conversation.ID, this.currentUser);
      console.error('Error deleting conversation:', error);
      await this.dialogService.alert('Error', 'Failed to delete conversation. Please try again.');
  toggleMenu(conversationId: string, event: Event): void {
    this.openMenuConversationId = this.openMenuConversationId === conversationId ? null : conversationId;
  closeMenu(): void {
    this.openMenuConversationId = null;
  async togglePin(conversation: MJConversationEntity, event?: Event): Promise<void> {
    if (event) event.stopPropagation();
      await this.conversationData.togglePin(conversation.ID, this.currentUser);
      console.error('Error toggling pin:', error);
      await this.dialogService.alert('Error', 'Failed to pin/unpin conversation. Please try again.');
  hasActiveTasks(conversationId: string): boolean {
    return this.conversationIdsWithTasks.has(conversationId);
  toggleSelectionMode(): void {
    this.isSelectionMode = !this.isSelectionMode;
    if (!this.isSelectionMode) {
      this.selectedConversationIds.clear();
  toggleConversationSelection(conversationId: string): void {
    if (this.selectedConversationIds.has(conversationId)) {
      this.selectedConversationIds.delete(conversationId);
      this.selectedConversationIds.add(conversationId);
    this.filteredConversations.forEach(c => {
      this.selectedConversationIds.add(c.ID);
  deselectAll(): void {
  async bulkDeleteConversations(): Promise<void> {
    const count = this.selectedConversationIds.size;
    if (count === 0) return;
      title: 'Delete Conversations',
      message: `Are you sure you want to delete ${count} conversation${count > 1 ? 's' : ''}? This action cannot be undone.`,
        const result = await this.conversationData.deleteMultipleConversations(
          Array.from(this.selectedConversationIds),
        // Show results if there were any failures
        if (result.failed.length > 0) {
            'Partial Success',
            `Deleted ${result.successful.length} of ${count} conversations. ${result.failed.length} failed.`
        // Exit selection mode
        console.error('Error deleting conversations:', error);
        await this.dialogService.alert('Error', 'Failed to delete conversations. Please try again.');
  handleConversationClick(conversation: MJConversationEntity): void {
    if (this.isSelectionMode) {
      this.toggleConversationSelection(conversation.ID);
      this.selectConversation(conversation);
