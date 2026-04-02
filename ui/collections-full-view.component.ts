import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { MJCollectionEntity, MJArtifactEntity, MJArtifactVersionEntity } from '@memberjunction/core-entities';
import { ArtifactStateService } from '../../services/artifact-state.service';
import { CollectionStateService } from '../../services/collection-state.service';
import { CollectionViewMode, CollectionViewItem, CollectionSortBy, CollectionSortOrder } from '../../models/collection-view.model';
 * Full-panel Collections view component
 * Comprehensive collection management with artifacts display
  selector: 'mj-collections-full-view',
    <div class="collections-view" (keydown)="handleKeyboardShortcut($event)">
      <!-- Mac Finder-style Header -->
      <div class="collections-header">
        <!-- Breadcrumb navigation -->
        <div class="collections-breadcrumb">
            <a class="breadcrumb-link" (click)="navigateToRoot()">Collections</a>
          @if (breadcrumbs.length > 0) {
            <span class="breadcrumb-path">
              @for (crumb of breadcrumbs; track crumb; let last = $last) {
                <a class="breadcrumb-link"
                  [class.active]="last"
                  (click)="navigateTo(crumb)">
                  {{ crumb.name }}
        <div class="collections-actions">
          <button class="btn-icon"
            [title]="viewMode === 'grid' ? 'Switch to List View' : 'Switch to Grid View'">
            <i class="fas" [ngClass]="viewMode === 'grid' ? 'fa-list' : 'fa-th'"></i>
          <!-- Select mode toggle -->
            [class.active]="isSelectMode"
            (click)="toggleSelectMode()"
            [title]="isSelectMode ? 'Exit Select Mode' : 'Select Items'">
            <i class="fas fa-check-square"></i>
          <!-- Sort dropdown (grid view only) -->
            <div class="dropdown-container">
                (click)="showSortDropdown = !showSortDropdown"
                title="Sort options">
                <i class="fas fa-sort"></i>
              @if (showSortDropdown) {
                  <button class="dropdown-item"
                    [class.active]="sortBy === 'name'"
                    (click)="setSortBy('name')">
                    <i class="fas fa-sort-alpha-down"></i>
                    <span>Sort by Name</span>
                    [class.active]="sortBy === 'date'"
                    (click)="setSortBy('date')">
                    <span>Sort by Date</span>
                    [class.active]="sortBy === 'type'"
                    (click)="setSortBy('type')">
                    <span>Sort by Type</span>
              (ngModelChange)="onSearchChange($event)">
              <button class="search-clear"
                (click)="searchQuery = ''; onSearchChange('')"
                title="Clear search">
          <!-- New dropdown -->
          @if (canEditCurrent()) {
                (click)="showNewDropdown = !showNewDropdown">
                <span>New</span>
              @if (showNewDropdown) {
                <div class="dropdown-menu dropdown-menu-right">
                  <button class="dropdown-item" (click)="createCollection()">
                    <span>New Collection</span>
                    (click)="addArtifact()"
                    [disabled]="!currentCollectionId">
                    <i class="fas fa-file-plus"></i>
                    <span>New Artifact</span>
          <!-- Refresh button -->
      <!-- Multi-select toolbar (appears when items selected) -->
        <div class="selection-toolbar">
          <div class="selection-info">
            <span class="selection-count">{{ selectedItems.size }} selected</span>
          <div class="selection-actions">
            <button class="btn-toolbar" (click)="clearSelection()">
            <button class="btn-toolbar btn-danger" (click)="deleteSelected()">
              Delete Selected
      <!-- Content area -->
      <div class="collections-content">
            <mj-loading text="Loading collections..." size="large"></mj-loading>
        @if (!isLoading && unifiedItems.length === 0) {
            <!-- Search returned no results -->
              <h3>No items found</h3>
              <p>Try adjusting your search</p>
            <!-- Empty root level -->
            @if (!searchQuery && !currentCollectionId) {
              <h3>No collections yet</h3>
              <p>Create your first collection to get started</p>
                <button class="btn-primary empty-state-cta"
                  (click)="createCollection()"
                  Create Collection
            <!-- Empty collection (has parent) -->
            @if (!searchQuery && currentCollectionId) {
              <h3>This collection is empty</h3>
              <p>Use the <strong>New</strong> button above to add collections or artifacts</p>
        <!-- Grid view -->
        @if (!isLoading && unifiedItems.length > 0 && viewMode === 'grid') {
            class="unified-grid"
            [class.select-mode]="isSelectMode">
            @for (item of unifiedItems; track item) {
                class="grid-item"
                [class.selected]="item.selected"
                [class.active]="item.type === 'artifact' && item.artifact?.ID === activeArtifactId"
                (click)="onItemClick(item, $event)"
                (dblclick)="onItemDoubleClick(item, $event)"
                (contextmenu)="onItemContextMenu(item, $event)">
                <!-- Selection checkbox (only visible in select mode) -->
                @if (isSelectMode) {
                  <div class="item-checkbox"
                    (click)="toggleItemSelection(item, $event)">
                    [ngClass]="item.selected ? 'fa-check-circle' : 'fa-circle'"></i>
                <!-- Folder item -->
                @if (item.type === 'folder') {
                    class="grid-item-content"
                    [title]="item.description || item.name">
                    <div class="grid-icon folder-icon">
                      @if (item.isShared) {
                        <div class="shared-badge" title="Shared">
                    <div class="grid-info">
                      <div class="grid-name">{{ item.name }}</div>
                      @if (item.description) {
                        <div class="grid-description">
                          {{ item.description }}
                      @if (item.itemCount !== undefined) {
                        <div class="grid-meta">
                          {{ getItemCountText(item.itemCount) }}
                      @if (item.isShared && item.owner) {
                        <div class="grid-owner">
                          {{ item.owner }}
                <!-- Artifact item -->
                @if (item.type === 'artifact') {
                    <div class="grid-icon artifact-icon">
                        @if (item.versionNumber) {
                          <span class="version-badge">
                            v{{ item.versionNumber }}
                        @if (item.artifactType) {
                          <span class="artifact-type-badge">
                            {{ item.artifactType }}
        @if (!isLoading && unifiedItems.length > 0 && viewMode === 'list') {
            class="unified-list"
            <table class="list-table">
                        [ngClass]="selectedItems.size === unifiedItems.length ? 'fa-check-square' : 'fa-square'"
                      (click)="selectedItems.size === unifiedItems.length ? clearSelection() : selectAll()"></i>
                  <th class="col-name sortable" (click)="setSortBy('name')">
                    <span>Name</span>
                    @if (sortBy !== 'name') {
                    @if (sortBy === 'name') {
                      [ngClass]="sortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down'"></i>
                  <th class="col-type sortable" (click)="setSortBy('type')">
                    @if (sortBy !== 'type') {
                    @if (sortBy === 'type') {
                  <th class="col-modified sortable" (click)="setSortBy('date')">
                    <span>Modified</span>
                    @if (sortBy !== 'date') {
                    @if (sortBy === 'date') {
                  <th class="col-owner">Owner</th>
                    class="list-item"
                          [ngClass]="item.selected ? 'fa-check-circle' : 'fa-circle'"
                        (click)="toggleItemSelection(item, $event)"></i>
                      <div class="list-name-cell">
                        [ngClass]="item.type === 'folder' ? 'fa-folder' : item.icon"></i>
                        <span>{{ item.name }}</span>
                          <i class="fas fa-users shared-indicator"
                          title="Shared"></i>
                    <td class="col-type">
                        <span>Folder</span>
                    <td class="col-modified">
                      @if (item.lastModified) {
                          {{ item.lastModified | date:'short' }}
                    <td class="col-owner">
                      @if (item.owner) {
                        <span>{{ item.owner }}</span>
    <!-- Modals (unchanged) -->
    <mj-collection-form-modal
      [isOpen]="isFormModalOpen"
      [collection]="editingCollection"
      [parentCollection]="currentCollection || undefined"
      (saved)="onCollectionSaved($event)"
      (cancelled)="onFormCancelled()">
    </mj-collection-form-modal>
    <mj-artifact-create-modal
      [isOpen]="isArtifactModalOpen"
      [collectionId]="currentCollectionId || ''"
      (saved)="onArtifactSaved($event)"
      (cancelled)="onArtifactModalCancelled()">
    </mj-artifact-create-modal>
    <mj-collection-share-modal
      [isOpen]="isShareModalOpen"
      [collection]="sharingCollection"
      [currentUserPermissions]="sharingCollection ? userPermissions.get(sharingCollection.ID) || null : null"
      (saved)="onPermissionsChanged()"
      (cancelled)="onShareModalCancelled()">
    </mj-collection-share-modal>
    .collections-header {
    .collections-breadcrumb {
    .breadcrumb-link {
      transition: color 150ms ease;
    .breadcrumb-link:hover {
      color: #0076D6;
    .breadcrumb-link.active {
    .breadcrumb-path {
      color: #D1D5DB;
    .collections-actions {
      background: #007AFF;
      background: #0051D5;
    .btn-primary i.fa-chevron-down {
      color: #007AFF;
      border-color: #007AFF;
    .btn-icon.active:hover {
    /* Dropdown menus */
    .dropdown-menu-right {
      transition: background 100ms ease;
    .dropdown-item:hover:not(:disabled) {
    .dropdown-item:disabled {
    .dropdown-item.active {
    .dropdown-item i {
    .dropdown-item.active i {
    .dropdown-divider {
      padding: 6px 32px 6px 32px;
      box-shadow: 0 0 0 3px rgba(0, 122, 255, 0.1);
    /* Selection toolbar */
    .selection-toolbar {
      border-bottom: 1px solid #BFDBFE;
    .btn-toolbar {
    .btn-toolbar:hover {
    .btn-toolbar.btn-danger {
      border-color: #FCA5A5;
    .btn-toolbar.btn-danger:hover {
      border-color: #DC2626;
    .collections-content {
    /* Loading and empty states */
    .loading-state, .empty-state {
    .empty-state > i {
    .empty-state .empty-state-cta {
    /* Grid view */
    .unified-grid {
      grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
    .grid-item {
    .grid-item:hover {
    .grid-item.selected {
    .grid-item.active {
      background: #FEF3C7;
      border-color: #F59E0B;
      box-shadow: 0 0 0 1px #F59E0B;
    .grid-item.active:hover {
      background: #FDE68A;
    /* Select mode styling for grid */
    .unified-grid.select-mode .grid-item {
    .unified-grid.select-mode .grid-item:hover {
      border-color: #93C5FD;
    .item-checkbox {
    .item-checkbox i {
    .grid-item.selected .item-checkbox i,
    .item-checkbox:hover i {
    .grid-item-content {
    .grid-icon {
    .grid-icon.folder-icon {
      background: linear-gradient(135deg, #60A5FA 0%, #3B82F6 100%);
    .grid-icon.folder-icon i {
    .grid-icon.artifact-icon {
    .grid-icon.artifact-icon i {
    .shared-badge {
    .shared-badge i {
    .grid-info {
    .grid-name {
      /* Allow wrapping to 2 lines max */
    .grid-description {
    .grid-meta {
    .grid-owner {
    .grid-owner i {
      color: #92400E;
    /* List view */
    .unified-list {
    .list-table {
    .list-table thead {
    .list-table th {
    .list-table th.sortable {
    .list-table th.sortable:hover {
    .list-table th.sortable span {
    .list-table th.sortable i {
    .list-table th.sortable:hover i {
    .list-table tbody tr {
    .list-table tbody tr:last-child {
    .list-table tbody tr:hover {
    .list-table tbody tr.selected {
    .list-table tbody tr.active {
      border-left: 3px solid #F59E0B;
    .list-table tbody tr.active:hover {
    .list-table td {
    .col-checkbox i {
    .col-checkbox i:hover,
    .list-table tbody tr.selected .col-checkbox i {
    .list-name-cell {
    .list-name-cell i {
    .list-name-cell .fa-folder {
      color: #3B82F6;
    .col-type {
      width: 150px;
    .col-modified {
export class CollectionsFullViewComponent implements OnInit, OnDestroy {
  @Output() collectionNavigated = new EventEmitter<{
    collectionId: string | null;
    versionId?: string | null;
  public artifactVersions: Array<{ version: MJArtifactVersionEntity; artifact: MJArtifactEntity }> = [];
  public filteredCollections: MJCollectionEntity[] = [];
  public filteredArtifactVersions: Array<{ version: MJArtifactVersionEntity; artifact: MJArtifactEntity }> = [];
  public breadcrumbs: Array<{ id: string; name: string }> = [];
  public currentCollectionId: string | null = null;
  public currentCollection: MJCollectionEntity | null = null;
  public isFormModalOpen: boolean = false;
  public editingCollection?: MJCollectionEntity;
  public isArtifactModalOpen: boolean = false;
  public isShareModalOpen: boolean = false;
  public sharingCollection: MJCollectionEntity | null = null;
  // New UI state for Mac Finder-style view
  public viewMode: CollectionViewMode = 'grid';
  public sortBy: CollectionSortBy = 'name';
  public sortOrder: CollectionSortOrder = 'asc';
  public unifiedItems: CollectionViewItem[] = [];
  public selectedItems: Set<string> = new Set();
  public showNewDropdown: boolean = false;
  public showSortDropdown: boolean = false;
  public activeArtifactId: string | null = null; // Track which artifact is currently being viewed
  public isSelectMode: boolean = false; // Toggle for selection mode
  private isNavigatingProgrammatically = false;
    private collectionState: CollectionStateService,
    private artifactIconService: ArtifactIconService,
    // Subscribe to collection state changes for deep linking FIRST
    // This ensures that if there's a URL with collectionId, we set it before loading data
    this.subscribeToCollectionState();
    // Subscribe to artifact state changes to track active artifact
    // Check if there's an active collection from URL params (set by parent component)
    const activeCollectionId = this.collectionState.activeCollectionId;
    if (activeCollectionId) {
      // If there's an active collection, navigate to it (which will call loadData)
      console.log('📁 Initial load with active collection:', activeCollectionId);
      this.navigateToCollectionById(activeCollectionId);
      // Otherwise, just load the root level
   * Subscribe to collection state changes for deep linking support
  private subscribeToCollectionState(): void {
    // Watch for external navigation requests (e.g., from search or URL)
    this.collectionState.activeCollectionId$
      .subscribe(collectionId => {
        // Ignore state changes that we triggered ourselves
        if (this.isNavigatingProgrammatically) {
        // Only navigate if the state is different from our current state
        // This prevents double-loading during initialization
        if (collectionId !== this.currentCollectionId) {
            console.log('📁 Collection state changed externally, navigating to:', collectionId);
            this.navigateToCollectionById(collectionId);
            console.log('📁 Collection state cleared externally, navigating to root');
            this.navigateToRoot();
   * Subscribe to artifact state changes to track which artifact is currently open
    this.artifactState.activeArtifact$
      .subscribe(artifact => {
        // Update active artifact ID for highlighting
        this.activeArtifactId = artifact?.ID || null;
  async loadData(): Promise<void> {
    // Only show loading spinner if we don't have data yet
    // This prevents flash of loading when navigating between collections
    const hasData = this.collections.length > 0 || this.unifiedItems.length > 0;
      // Load saved view preferences from localStorage
      const savedMode = localStorage.getItem('collections-view-mode');
      if (savedMode === 'grid' || savedMode === 'list') {
        this.viewMode = savedMode;
      const savedSortBy = localStorage.getItem('collections-sort-by');
      if (savedSortBy === 'name' || savedSortBy === 'date' || savedSortBy === 'type') {
        this.sortBy = savedSortBy;
      const savedSortOrder = localStorage.getItem('collections-sort-order');
      if (savedSortOrder === 'asc' || savedSortOrder === 'desc') {
        this.sortOrder = savedSortOrder;
        this.loadCollections(),
        this.loadCurrentCollectionPermission()
      // Build unified item list after data loads
      this.buildUnifiedItemList();
      // Load collections where user is owner OR has permissions
      const ownerFilter = `OwnerID='${this.currentUser.ID}'`;
      const permissionSubquery = `ID IN (
        SELECT CollectionID
        FROM [__mj].[vwCollectionPermissions]
        WHERE UserID='${this.currentUser.ID}'
      const baseFilter = `EnvironmentID='${this.environmentId}'` +
                         (this.currentCollectionId ? ` AND ParentID='${this.currentCollectionId}'` : ' AND ParentID IS NULL');
      const filter = `${baseFilter} AND (OwnerID IS NULL OR ${ownerFilter} OR ${permissionSubquery})`;
      const result = await rv.RunView<MJCollectionEntity>(
        this.filteredCollections = [...this.collections];
      console.error('Failed to load collections:', error);
    for (const collection of this.collections) {
      if (permission) {
        this.userPermissions.set(collection.ID, permission);
  private async loadCurrentCollectionPermission(): Promise<void> {
    if (!this.currentCollectionId || !this.currentCollection) {
      this.currentCollectionId,
      this.userPermissions.set(this.currentCollectionId, permission);
    if (!this.currentCollectionId) {
      this.filteredArtifactVersions = [];
      this.artifactVersions = await this.artifactState.loadArtifactVersionsForCollection(
      this.filteredArtifactVersions = [...this.artifactVersions];
      console.error('Failed to load artifact versions:', error);
  async openCollection(collection: MJCollectionEntity): Promise<void> {
    this.isNavigatingProgrammatically = true;
      this.breadcrumbs.push({ id: collection.ID, name: collection.Name });
      this.currentCollectionId = collection.ID;
      this.currentCollection = collection;
      this.activeArtifactId = null; // Clear active artifact when switching collections
      // Update state service
      this.collectionState.setActiveCollection(collection.ID);
      // Close any open artifact when switching collections
      this.collectionNavigated.emit({
        collectionId: collection.ID,
        versionId: null
      this.isNavigatingProgrammatically = false;
  async navigateTo(crumb: { id: string; name: string }): Promise<void> {
      const index = this.breadcrumbs.findIndex(b => b.id === crumb.id);
        this.breadcrumbs = this.breadcrumbs.slice(0, index + 1);
        this.currentCollectionId = crumb.id;
        // Load the collection entity
        this.currentCollection = await md.GetEntityObject<MJCollectionEntity>('MJ: Collections', this.currentUser);
        await this.currentCollection.Load(crumb.id);
        this.collectionState.setActiveCollection(crumb.id);
        // Close any open artifact when navigating collections
          collectionId: crumb.id,
  async navigateToRoot(): Promise<void> {
      this.breadcrumbs = [];
      this.currentCollectionId = null;
      this.currentCollection = null;
      this.collectionState.setActiveCollection(null);
      // Close any open artifact when navigating to root
        collectionId: null,
   * Navigate to a collection by ID, building the breadcrumb trail
   * Used for deep linking from search results or URL parameters
  async navigateToCollectionById(collectionId: string): Promise<void> {
      console.log('📁 Navigating to collection by ID:', collectionId);
      // Load the target collection
      const targetCollection = await md.GetEntityObject<MJCollectionEntity>('MJ: Collections', this.currentUser);
      await targetCollection.Load(collectionId);
      if (!targetCollection || !targetCollection.ID) {
        console.error('❌ Failed to load collection:', collectionId);
      // Build breadcrumb trail by traversing parent hierarchy
      // Note: breadcrumbs includes ALL collections in the path including the current one
      const trail: Array<{ id: string; name: string }> = [];
      let currentId: string | null = targetCollection.ParentID;
        const parentCollection = await md.GetEntityObject<MJCollectionEntity>('MJ: Collections', this.currentUser);
        await parentCollection.Load(currentId);
        if (parentCollection && parentCollection.ID) {
          // Add to front of trail (we're working backwards)
          trail.unshift({
            id: parentCollection.ID,
            name: parentCollection.Name
          currentId = parentCollection.ParentID;
      // Add the target collection to the trail (breadcrumbs includes current collection)
      trail.push({
        id: targetCollection.ID,
        name: targetCollection.Name
      // Update component state
      this.breadcrumbs = trail;
      this.currentCollectionId = targetCollection.ID;
      this.currentCollection = targetCollection;
      // Load collections and artifacts for this collection
      this.collectionState.setActiveCollection(targetCollection.ID);
      // Emit navigation event
      // NOTE: We don't emit artifactId here because this is for deep linking/programmatic navigation
      // Artifact state is managed separately by the artifact state service
        collectionId: targetCollection.ID
      console.log('✅ Successfully navigated to collection with breadcrumb trail:', trail);
      console.error('❌ Error navigating to collection:', error);
    // Validate user can edit current collection (or at root level)
    if (this.currentCollection) {
      const canEdit = await this.validatePermission(this.currentCollection, 'edit');
      if (!canEdit) return;
    this.showNewDropdown = false;
    this.editingCollection = undefined;
    this.isFormModalOpen = true;
  async editCollection(collection: MJCollectionEntity): Promise<void> {
    const canEdit = await this.validatePermission(collection, 'edit');
    this.editingCollection = collection;
  async deleteCollection(collection: MJCollectionEntity): Promise<void> {
    console.log('deleteCollection called for:', collection.Name, collection.ID);
    // Validate user has delete permission
    const canDelete = await this.validatePermission(collection, 'delete');
    if (!canDelete) return;
      title: 'Delete Collection',
      message: `Are you sure you want to delete "${collection.Name}"? This will also delete all child collections and remove all artifacts. This action cannot be undone.`,
      okText: 'Delete',
      cancelText: 'Cancel',
      dangerous: true
    console.log('Delete confirmed:', confirmed);
    if (confirmed) {
        console.log('Attempting to delete collection and all children...');
        await this.deleteCollectionRecursive(collection.ID);
        await this.dialogService.alert('Error', `An error occurred while deleting the collection: ${error}`);
  private async deleteCollectionRecursive(collectionId: string): Promise<void> {
    // Step 1: Find and delete all child collections recursively
    const childrenResult = await rv.RunView<MJCollectionEntity>(
        ExtraFilter: `ParentID='${collectionId}'`,
    if (childrenResult.Success && childrenResult.Results) {
      for (const child of childrenResult.Results) {
        await this.deleteCollectionRecursive(child.ID);
    // Step 2: Delete all permissions for this collection
    await this.permissionService.deleteAllPermissions(collectionId, this.currentUser);
    // Step 3: Delete all artifact links for this collection
    const artifactsResult = await rv.RunView<any>(
        ExtraFilter: `CollectionID='${collectionId}'`,
    if (artifactsResult.Success && artifactsResult.Results) {
      for (const ca of artifactsResult.Results) {
        await ca.Delete();
    // Step 4: Delete the collection itself
    await collection.Load(collectionId);
      throw new Error(`Failed to delete collection: ${collection.LatestResult?.Message || 'Unknown error'}`);
  async onCollectionSaved(collection: MJCollectionEntity): Promise<void> {
    this.isFormModalOpen = false;
    // Reload current collection permission (it was cleared by loadUserPermissions)
    await this.loadCurrentCollectionPermission();
    // Rebuild unified list to show new collection
  onFormCancelled(): void {
  async addArtifact(): Promise<void> {
    // Validate user can edit current collection
    this.isArtifactModalOpen = true;
  async onArtifactSaved(artifact: MJArtifactEntity): Promise<void> {
    this.isArtifactModalOpen = false;
  onArtifactModalCancelled(): void {
  async removeArtifact(item: { version: MJArtifactVersionEntity; artifact: MJArtifactEntity }): Promise<void> {
    if (!this.currentCollectionId) return;
    // Validate user has delete permission on current collection
      const canDelete = await this.validatePermission(this.currentCollection, 'delete');
      title: 'Remove Artifact Version',
      message: `Remove ${versionLabel} from this collection?`,
      okText: 'Remove',
      cancelText: 'Cancel'
          ExtraFilter: `CollectionID='${this.currentCollectionId}' AND ArtifactVersionID='${item.version.ID}'`,
          await this.dialogService.alert('Error', 'Collection artifact link not found.');
        console.error('Error removing artifact version:', error);
        await this.dialogService.alert('Error', 'Failed to remove artifact version from collection.');
  viewArtifact(item: { version: MJArtifactVersionEntity; artifact: MJArtifactEntity }): void {
    this.activeArtifactId = item.artifact.ID;
    this.artifactState.openArtifact(item.artifact.ID, item.version.VersionNumber);
  // Permission validation and checking methods
  private async validatePermission(
    collection: MJCollectionEntity | null,
    requiredPermission: 'edit' | 'delete' | 'share'
    // Owner has all permissions (including backwards compatibility for null OwnerID)
    if (!collection?.OwnerID || collection.OwnerID === this.currentUser.ID) {
    if (!permission) {
      await this.dialogService.alert(
        'Permission Denied',
        'You do not have permission to perform this action.'
    const hasPermission =
      (requiredPermission === 'edit' && permission.canEdit) ||
      (requiredPermission === 'delete' && permission.canDelete) ||
      (requiredPermission === 'share' && permission.canShare);
      const permissionName = requiredPermission.charAt(0).toUpperCase() + requiredPermission.slice(1);
        `You do not have ${permissionName} permission for this collection.`
    if (!collection.OwnerID || collection.OwnerID === this.currentUser.ID) return true;
  canShare(collection: MJCollectionEntity): boolean {
    return permission?.canShare || false;
  canEditCurrent(): boolean {
    // At root level, anyone can create
    return this.canEdit(this.currentCollection);
  canDeleteCurrent(): boolean {
    // At root level, no delete needed
    return this.canDelete(this.currentCollection);
  isShared(collection: MJCollectionEntity): boolean {
    // Collection is shared if user is not the owner and OwnerID is set
    return collection.OwnerID != null && collection.OwnerID !== this.currentUser.ID;
  async shareCollection(collection: MJCollectionEntity): Promise<void> {
    // Validate user has share permission
    const canShare = await this.validatePermission(collection, 'share');
    if (!canShare) return;
    this.sharingCollection = collection;
    this.isShareModalOpen = true;
  async onPermissionsChanged(): Promise<void> {
    // Reload collections and permissions after sharing changes
  onShareModalCancelled(): void {
    this.isShareModalOpen = false;
    this.sharingCollection = null;
   * Get the icon for an artifact using the centralized icon service.
  public getArtifactIcon(artifact: MJArtifactEntity): string {
    return this.artifactIconService.getArtifactIcon(artifact);
  // ==================== NEW MAC FINDER-STYLE METHODS ====================
   * Build unified list of folders and artifacts (Phase 1)
  private buildUnifiedItemList(): void {
    const items: CollectionViewItem[] = [];
    // Add folders first (collections)
    for (const collection of this.filteredCollections) {
        description: collection.Description || undefined,
        icon: 'fa-folder',
        itemCount: 0, // TODO: calculate actual count
        owner: collection.Owner || undefined,
        isShared: this.isShared(collection),
        selected: this.selectedItems.has(collection.ID),
        collection: collection
    // Then add artifacts
    for (const item of this.filteredArtifactVersions) {
        type: 'artifact',
        id: item.version.ID,
        name: item.artifact.Name,
        description: item.artifact.Description || undefined,
        icon: this.getArtifactIcon(item.artifact),
        versionNumber: item.version.VersionNumber,
        artifactType: item.artifact.Type,
        lastModified: item.version.__mj_UpdatedAt,
        selected: this.selectedItems.has(item.version.ID),
        artifact: item.artifact,
        version: item.version
    this.unifiedItems = this.sortItems(items);
   * Sort items by selected criteria (Phase 2)
  private sortItems(items: CollectionViewItem[]): CollectionViewItem[] {
    return items.sort((a, b) => {
      // Always keep folders before artifacts
        return a.type === 'folder' ? -1 : 1;
      // Then sort by selected criteria
          comparison = a.name.localeCompare(b.name);
          const aDate = a.lastModified || new Date(0);
          const bDate = b.lastModified || new Date(0);
          comparison = aDate.getTime() - bDate.getTime();
          comparison = (a.artifactType || '').localeCompare(b.artifactType || '');
      return this.sortOrder === 'asc' ? comparison : -comparison;
   * Toggle between grid and list view
  public toggleViewMode(): void {
    // Save preference to localStorage
    localStorage.setItem('collections-view-mode', this.viewMode);
   * Set view mode explicitly
  public setViewMode(mode: CollectionViewMode): void {
    localStorage.setItem('collections-view-mode', mode);
   * Toggle selection mode on/off
   * When entering select mode, clicks toggle selection instead of opening items
   * When exiting select mode, clears any selections
  public toggleSelectMode(): void {
    this.isSelectMode = !this.isSelectMode;
    if (!this.isSelectMode) {
      // Clear selection when exiting select mode
   * Exit selection mode (called when navigating to a new folder)
  private exitSelectMode(): void {
    if (this.isSelectMode) {
      this.isSelectMode = false;
   * Set sort order - toggles asc/desc if clicking same column
  public setSortBy(sortBy: CollectionSortBy): void {
      // Toggle order if same sort
      this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
      this.sortOrder = 'asc';
    // Save sort preferences to localStorage
    localStorage.setItem('collections-sort-by', this.sortBy);
    localStorage.setItem('collections-sort-order', this.sortOrder);
    // Close dropdown and rebuild list
    this.showSortDropdown = false;
   * Filter items by search query (Phase 2)
  public onSearchChange(query?: string): void {
    // If query parameter provided, use it; otherwise use searchQuery property
    const searchText = query !== undefined ? query : this.searchQuery;
    if (!searchText.trim()) {
      // Reset to all items
    const lowerQuery = searchText.toLowerCase();
    this.unifiedItems = this.unifiedItems.filter(item =>
      item.name.toLowerCase().includes(lowerQuery) ||
      item.description?.toLowerCase().includes(lowerQuery)
   * Multi-select: Toggle item selection (Phase 3)
  public toggleItemSelection(item: CollectionViewItem, event: MouseEvent): void {
    if (event.metaKey || event.ctrlKey) {
      // Cmd/Ctrl+Click: Toggle individual selection
      if (this.selectedItems.has(item.id)) {
        this.selectedItems.delete(item.id);
        this.selectedItems.add(item.id);
    } else if (event.shiftKey) {
      // Shift+Click: Select range (TODO: implement range selection)
      // Regular click: Select only this item
    this.buildUnifiedItemList(); // Refresh to update selected states
   * Multi-select: Select all items (Phase 3)
  public selectAll(): void {
    for (const item of this.unifiedItems) {
   * Multi-select: Clear selection (Phase 3)
   * Multi-select: Delete selected items (Phase 3)
      title: `Delete ${this.selectedItems.size} item(s)?`,
      message: 'This action cannot be undone.',
    // TODO: Implement batch delete
   * Get count of items in folder (Phase 1)
  private async getCollectionItemCount(collectionId: string): Promise<number> {
    // TODO: Query for actual count
   * Handle clicking on unified item
   * In select mode: toggles selection
   * In normal mode: opens item (folder or artifact)
  public onItemClick(item: CollectionViewItem, event?: MouseEvent): void {
    // In select mode, single click toggles selection
      this.toggleItemSelectionSimple(item);
    // Normal mode: open the item
    this.openItem(item);
   * Handle double-clicking on unified item
   * Always opens the item, even in select mode
  public onItemDoubleClick(item: CollectionViewItem, event?: MouseEvent): void {
    event?.preventDefault();
   * Open an item (folder or artifact)
  private openItem(item: CollectionViewItem): void {
    if (item.type === 'folder' && item.collection) {
      // Exit select mode when navigating to a new folder
      this.exitSelectMode();
      this.openCollection(item.collection);
    } else if (item.type === 'artifact') {
      if (!item.artifact || !item.version) {
        console.error('Artifact or version is missing for item:', item.id);
      this.viewArtifact({ artifact: item.artifact, version: item.version });
   * Simple toggle for item selection (used in select mode)
  private toggleItemSelectionSimple(item: CollectionViewItem): void {
   * Get item count text for display
  public getItemCountText(itemCount?: number): string {
    if (itemCount !== undefined) {
      if (itemCount === 0) return 'Empty';
      if (itemCount === 1) return '1 item';
      return `${itemCount} items`;
    const folders = this.unifiedItems.filter(i => i.type === 'folder').length;
    const artifacts = this.unifiedItems.filter(i => i.type === 'artifact').length;
    const total = folders + artifacts;
    if (total === 0) return 'No items';
    if (total === 1) return '1 item';
    if (folders > 0) parts.push(`${folders} folder${folders > 1 ? 's' : ''}`);
    if (artifacts > 0) parts.push(`${artifacts} artifact${artifacts > 1 ? 's' : ''}`);
   * - Cmd/Ctrl+A: Select all (enters select mode if not already)
   * - Escape: Exit select mode and clear selection
   * - Delete/Backspace: Delete selected items
  public handleKeyboardShortcut(event: KeyboardEvent): void {
    // Cmd+A / Ctrl+A: Select all (enters select mode)
    if ((event.metaKey || event.ctrlKey) && event.key === 'a') {
        this.isSelectMode = true;
      this.selectAll();
    // Escape: Exit select mode
    if (event.key === 'Escape' && this.isSelectMode) {
    // Delete/Backspace: Delete selected items
    if ((event.key === 'Delete' || event.key === 'Backspace') && this.selectedItems.size > 0) {
      // Only if not focused on an input
      if (target.tagName !== 'INPUT' && target.tagName !== 'TEXTAREA') {
        this.deleteSelected();
   * Handle right-click context menu
   * Opens browser context menu for now - can be extended with custom menu
  public onItemContextMenu(item: CollectionViewItem, event: MouseEvent): void {
    // Select the item if not already selected
    if (!item.selected) {
      this.toggleItemSelection(item, event);
    // Allow browser's default context menu for now
    // Future enhancement: implement custom context menu with actions
    // event.preventDefault();
