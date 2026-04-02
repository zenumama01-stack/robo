import { Component, ViewEncapsulation, OnInit, OnDestroy } from '@angular/core';
import { ResourceData, EnvironmentEntityExtended } from '@memberjunction/core-entities';
import { ArtifactStateService, ArtifactPermissionService, CollectionStateService } from '@memberjunction/ng-conversations';
import { Subject, takeUntil, distinctUntilChanged, combineLatest, filter } from 'rxjs';
 * Chat Collections Resource - displays the collections full view for tab-based display
 * Shows all collections and their artifacts in a comprehensive view
 * Includes artifact panel support for viewing selected artifacts
@RegisterClass(BaseResourceComponent, 'ChatCollectionsResource')
  selector: 'mj-chat-collections-resource',
    <div class="chat-collections-container">
      <!-- Collections view -->
      <div class="collections-main" [class.with-artifact-panel]="isArtifactPanelOpen && activeArtifactId">
        @if (currentUser) {
          <mj-collections-full-view
            [currentUser]="currentUser">
          </mj-collections-full-view>
      <!-- Artifact Panel -->
      @if (isArtifactPanelOpen && activeArtifactId) {
        @if (!isArtifactPanelMaximized) {
          <div class="artifact-panel-resize-handle"
          (mousedown)="onResizeStart($event)"></div>
        <div class="artifact-panel"
          [style.width.%]="artifactPanelWidth"
          [class.maximized]="isArtifactPanelMaximized">
            [artifactId]="activeArtifactId"
            [versionNumber]="activeVersionNumber ?? undefined"
            [showSaveToCollection]="false"
            [viewContext]="'collection'"
            [contextCollectionId]="collectionState.activeCollectionId ?? undefined"
            [canShare]="canShareActiveArtifact"
            [canEdit]="canEditActiveArtifact"
            [isMaximized]="isArtifactPanelMaximized"
            (closed)="closeArtifactPanel()"
            (maximizeToggled)="toggleMaximizeArtifactPanel()"
            (navigateToLink)="onNavigateToLink($event)"
            (openEntityRecord)="onOpenEntityRecord($event)">
    .chat-collections-container {
      position: relative; /* Required for absolute positioning of maximized panel */
    .collections-main {
    .collections-main.with-artifact-panel {
      /* When artifact panel is open, main area shrinks */
    .artifact-panel-resize-handle {
      transition: background 150ms ease;
    .artifact-panel-resize-handle:hover {
      background: #0076B6;
    .artifact-panel-resize-handle:active {
    .artifact-panel {
      transition: width 0.2s ease;
    .artifact-panel.maximized {
export class ChatCollectionsResource extends BaseResourceComponent implements OnInit, OnDestroy {
  // Artifact panel state
  public isArtifactPanelOpen: boolean = false;
  public activeArtifactId: string | null = null;
  public activeVersionNumber: number | null = null;
  public canShareActiveArtifact: boolean = false;
  public canEditActiveArtifact: boolean = false;
  public artifactPanelWidth: number = 40; // Default 40% width (percentage-based)
  public isArtifactPanelMaximized: boolean = false;
  private artifactPanelWidthBeforeMaximize: number = 40; // Store width before maximizing
  private isResizing: boolean = false;
  private resizeStartX: number = 0;
  private resizeStartWidth: number = 0;
  private lastNavigatedUrl: string = ''; // Track URL to avoid reacting to our own navigation
    private artifactState: ArtifactStateService,
    private artifactPermissionService: ArtifactPermissionService,
    public collectionState: CollectionStateService,
    // Subscribe to artifact state changes
    this.subscribeToArtifactState();
    // Setup resize listeners
    this.setupResizeListeners();
    // Parse URL first and apply state
      // Check if we have navigation params from config (e.g., from Conversations linking here)
      this.applyNavigationParams();
    // Subscribe to state changes to update URL
    this.subscribeToUrlStateChanges();
    // Update URL to reflect current state
    // Notify load complete after user is set
   * Parse URL query string for collection state.
   * Query params: collectionId, artifactId, versionNumber
  private parseUrlState(): { collectionId?: string; artifactId?: string; versionNumber?: number } | null {
    const collectionId = params.get('collectionId');
    const artifactId = params.get('artifactId');
    const versionNumber = params.get('versionNumber');
    if (!collectionId && !artifactId) return null;
      collectionId: collectionId || undefined,
      artifactId: artifactId || undefined,
      versionNumber: versionNumber ? parseInt(versionNumber, 10) : undefined
   * Apply URL state to collection services.
  private applyUrlState(state: { collectionId?: string; artifactId?: string; versionNumber?: number }): void {
    // Set active collection if specified
    if (state.collectionId) {
      this.collectionState.setActiveCollection(state.collectionId);
    // Open artifact if specified
    if (state.artifactId) {
      this.artifactState.openArtifact(state.artifactId, state.versionNumber);
   * Apply navigation parameters from configuration.
   * This handles deep-linking from other resources (e.g., clicking a link in Conversations).
  private applyNavigationParams(): void {
    if (config.collectionId) {
      this.collectionState.setActiveCollection(config.collectionId as string);
    if (config.artifactId) {
      const versionNumber = config.versionNumber ? (config.versionNumber as number) : undefined;
      this.artifactState.openArtifact(config.artifactId as string, versionNumber);
   * Subscribe to state changes for URL updates.
  private subscribeToUrlStateChanges(): void {
    // Combine collection and artifact state changes
      this.collectionState.activeCollectionId$.pipe(distinctUntilChanged()),
      this.artifactState.activeArtifactId$.pipe(distinctUntilChanged()),
      this.artifactState.activeVersionNumber$.pipe(distinctUntilChanged())
        if (!this.skipUrlUpdate) {
    // Add collection ID
    const collectionId = this.collectionState.activeCollectionId;
    if (collectionId) {
      queryParams['collectionId'] = collectionId;
      queryParams['collectionId'] = null;
    // Add artifact ID if panel is open
    if (this.activeArtifactId) {
      queryParams['artifactId'] = this.activeArtifactId;
      if (this.activeVersionNumber) {
        queryParams['versionNumber'] = this.activeVersionNumber.toString();
      queryParams['artifactId'] = null;
      queryParams['versionNumber'] = null;
      // No params means clear state
      this.collectionState.setActiveCollection(null as unknown as string);
      this.artifactState.closeArtifact();
  private parseUrlFromString(url: string): { collectionId?: string; artifactId?: string; versionNumber?: number } | null {
    this.removeResizeListeners();
   * Subscribe to artifact state service for panel open/close
  private subscribeToArtifactState(): void {
    // Subscribe to panel open state
    this.artifactState.isPanelOpen$
      .subscribe(isOpen => {
        this.isArtifactPanelOpen = isOpen;
    // Subscribe to active artifact ID
    this.artifactState.activeArtifactId$
      .subscribe(async id => {
        this.activeArtifactId = id;
        if (id) {
          await this.loadArtifactPermissions(id);
          this.canShareActiveArtifact = false;
          this.canEditActiveArtifact = false;
    // Subscribe to active version number
    this.artifactState.activeVersionNumber$
      .subscribe(versionNumber => {
        this.activeVersionNumber = versionNumber;
   * Load permissions for the active artifact
  private async loadArtifactPermissions(artifactId: string): Promise<void> {
    if (!artifactId || !this.currentUser) {
      const permissions = await this.artifactPermissionService.getUserPermissions(artifactId, this.currentUser);
      this.canShareActiveArtifact = permissions.canShare;
      this.canEditActiveArtifact = permissions.canEdit;
      console.error('Failed to load artifact permissions:', error);
   * Close the artifact panel
  closeArtifactPanel(): void {
   * Toggle maximize/restore state for artifact panel
  toggleMaximizeArtifactPanel(): void {
    if (this.isArtifactPanelMaximized) {
      // Restore to previous width
      this.artifactPanelWidth = this.artifactPanelWidthBeforeMaximize;
      this.isArtifactPanelMaximized = false;
      // Maximize - store current width and set to 100%
      this.artifactPanelWidthBeforeMaximize = this.artifactPanelWidth;
      this.artifactPanelWidth = 100;
      this.isArtifactPanelMaximized = true;
   * Handle navigation request from artifact viewer panel.
   * Converts the link event to a generic navigation request and uses NavigationService.
  onNavigateToLink(event: {
    type: 'conversation' | 'collection';
    artifactId?: string;
    versionNumber?: number;
    versionId?: string;
    // Map the link type to the nav item name
    const navItemName = event.type === 'conversation' ? 'Conversations' : 'Collections';
    // Build configuration params to pass to the target resource
    if (event.type === 'conversation') {
      params['conversationId'] = event.id;
      params['collectionId'] = event.id;
    // Include artifact info so destination can open it
    if (event.artifactId) {
      params['artifactId'] = event.artifactId;
      if (event.versionNumber) {
        params['versionNumber'] = event.versionNumber;
    // Navigate using the generic nav item method
    this.navigationService.OpenNavItemByName(navItemName, params);
   * Handle entity record open request from artifact viewer (from React component grids).
   * Uses NavigationService to open the record in a new tab.
  onOpenEntityRecord(event: {entityName: string; compositeKey: CompositeKey}): void {
    this.navigationService.OpenEntityRecord(event.entityName, event.compositeKey);
   * Get the environment ID from configuration or use default
  get environmentId(): string {
    return this.Data?.Configuration?.environmentId || EnvironmentEntityExtended.DefaultEnvironmentID;
   * Get the display name for chat collections
    return 'Collections';
   * Get the icon class for chat collections
    return 'fa-solid fa-folder-open';
  private setupResizeListeners(): void {
    this.boundOnResizeMove = this.onResizeMove.bind(this);
    this.boundOnResizeEnd = this.onResizeEnd.bind(this);
  private removeResizeListeners(): void {
    window.removeEventListener('mousemove', this.boundOnResizeMove);
    window.removeEventListener('mouseup', this.boundOnResizeEnd);
  private boundOnResizeMove: (e: MouseEvent) => void = () => {};
  private boundOnResizeEnd: (e: MouseEvent) => void = () => {};
    this.resizeStartWidth = this.artifactPanelWidth;
    window.addEventListener('mousemove', this.boundOnResizeMove);
    window.addEventListener('mouseup', this.boundOnResizeEnd);
  private onResizeMove(event: MouseEvent): void {
    if (!this.isResizing) return;
    const container = document.querySelector('.chat-collections-container') as HTMLElement;
    const containerWidth = container.offsetWidth;
    const deltaX = event.clientX - this.resizeStartX;
    const deltaPercent = (deltaX / containerWidth) * -100; // Negative because we're pulling from the right
    let newWidth = this.resizeStartWidth + deltaPercent;
    // Constrain between 20% and 80%
    newWidth = Math.max(20, Math.min(80, newWidth));
    this.artifactPanelWidth = newWidth;
  private onResizeEnd(event: MouseEvent): void {
