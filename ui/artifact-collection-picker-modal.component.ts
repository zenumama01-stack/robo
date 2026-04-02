import { ToastService } from '../../services/toast.service';
import { CollectionPermissionService, CollectionPermission } from '../../services/collection-permission.service';
interface CollectionNode {
  collection: MJCollectionEntity;
  hasChildren: boolean;
  alreadyContainsArtifact: boolean;
 * Modal for selecting collections to save artifacts to.
 * - Permission-aware: only shows collections where user has Edit permission
 * - Hierarchical navigation: start with root collections, drill down as needed
 * - Search by name
 * - Multi-selection support
 * - Create new collection with proper permission logic
  selector: 'mj-artifact-collection-picker-modal',
        title="Save to Collection"
        [minWidth]="500">
        <div class="picker-modal">
          @if (navigationPath.length > 0) {
            <div class="breadcrumb-nav">
              <button class="breadcrumb-btn" (click)="navigateToRoot()">
                <i class="fas fa-home"></i> Root
              @for (item of navigationPath; track item.collection.ID) {
                <i class="fas fa-chevron-right breadcrumb-separator"></i>
                <button class="breadcrumb-btn" (click)="navigateToCollection(item.collection)">
                  {{ item.collection.Name }}
            <i class="fas fa-search search-icon"></i>
              class="k-textbox search-input"
              (input)="onSearchChange()"
              placeholder="Search collections..."
              [disabled]="isLoading">
          <!-- Collections List -->
          @if (!isLoading && !errorMessage) {
              @if (displayedCollections.length === 0) {
                    <p>No collections found matching "{{ searchQuery }}"</p>
                  } @else if (currentParentId) {
                    <p>No sub-collections available</p>
                    <p>No collections available</p>
                    <p class="hint">Create a new collection to get started</p>
                @for (node of displayedCollections; track node.collection.ID) {
                    [class.already-added]="node.alreadyContainsArtifact"
                    (click)="toggleSelection(node)">
                    <div class="collection-checkbox">
                        [checked]="node.selected"
                        [disabled]="node.alreadyContainsArtifact"
                        (click)="$event.stopPropagation(); toggleSelection(node)">
                    <i class="fas fa-folder collection-icon" [style.color]="node.collection.Color || '#0076B6'"></i>
                    <span class="collection-name">{{ node.collection.Name }}</span>
                    @if (node.alreadyContainsArtifact) {
                      <span class="already-added-badge">
                        <i class="fas fa-check-circle"></i> Already added
                    @if (node.hasChildren) {
                        class="drill-down-btn"
                        (click)="$event.stopPropagation(); drillIntoCollection(node.collection)"
                        title="View sub-collections">
              <mj-loading text="Loading collections..." size="medium"></mj-loading>
          <!-- Selected Collections Summary -->
          @if (selectedCollections.length > 0) {
            <div class="selected-summary">
              <span>{{ selectedCollections.length }} collection(s) selected</span>
          <!-- Create New Collection Section -->
            <div class="divider">
              <span>OR CREATE NEW</span>
            @if (!showCreateForm) {
              <button class="btn-create-collection" (click)="showCreateForm = true">
                Create New Collection
              <div class="create-form">
                  class="k-textbox create-input"
                  [(ngModel)]="newCollectionName"
                  placeholder="Enter collection name"
                  (keydown.enter)="createCollection()"
                  #newCollectionInput>
                <div class="create-actions">
                  <button class="btn-create" kendoButton (click)="createCollection()" [disabled]="isCreatingCollection || !newCollectionName.trim()">
                    @if (isCreatingCollection) {
                  <button class="btn-cancel" kendoButton (click)="showCreateForm = false; newCollectionName = ''">
          <button kendoButton (click)="onCancel()">
            [disabled]="selectedCollections.length === 0 || isSaving">
              <i class="fas fa-spinner fa-spin"></i> Saving...
              <i class="fas fa-save"></i> Save to {{ selectedCollections.length }} Collection(s)
    .picker-modal {
    .breadcrumb-nav {
    .breadcrumb-btn {
    .breadcrumb-btn:hover {
      min-height: 250px;
    .collection-item:last-child {
    .collection-item.already-added {
    .collection-item.already-added:hover {
    .collection-checkbox {
    .collection-checkbox input[type="checkbox"] {
    .already-added-badge {
      background: #DBEAFE;
      border: 1px solid #93C5FD;
    .already-added-badge i {
      color: #2563EB;
    .drill-down-btn {
    .drill-down-btn:hover {
    .loading-state, .error-state {
    .selected-summary {
    .selected-summary i {
    .btn-create-collection {
      border: 2px dashed #D1D5DB;
    .btn-create-collection:hover {
    .btn-create-collection i {
    .create-form {
    .create-input {
    .create-actions {
    .btn-create, .btn-cancel {
export class ArtifactCollectionPickerModalComponent implements OnInit, OnChanges {
  @Input() excludeCollectionIds: string[] = []; // Collections to exclude (e.g., already contains artifact)
  @Output() saved = new EventEmitter<string[]>(); // Emits selected collection IDs
  public displayedCollections: CollectionNode[] = [];
  public selectedCollections: MJCollectionEntity[] = [];
  public userPermissions: Map<string, CollectionPermission> = new Map();
  public navigationPath: CollectionNode[] = []; // Breadcrumb trail
  public currentParentId: string | null = null;
  public currentParentCollection: MJCollectionEntity | undefined = undefined;
  public searchQuery: string = '';
  public isSaving: boolean = false;
  public errorMessage: string = '';
  // Create collection form state
  public showCreateForm: boolean = false;
  public newCollectionName: string = '';
  public isCreatingCollection: boolean = false;
    private toastService: ToastService,
    private permissionService: CollectionPermissionService
      await this.loadCollections();
  async ngOnChanges(changes: any) {
    if (changes['isOpen'] && this.isOpen) {
    this.displayedCollections = [];
    this.selectedCollections = [];
    this.userPermissions.clear();
    this.navigationPath = [];
    this.currentParentId = null;
    this.currentParentCollection = undefined;
  private async loadCollections(): Promise<void> {
      // Load all collections in environment
        ExtraFilter: `EnvironmentID='${this.environmentId}'`,
        this.errorMessage = result.ErrorMessage || 'Failed to load collections';
      this.allCollections = result.Results || [];
      // Load user permissions for all collections
      await this.loadUserPermissions();
      // Filter to collections with Edit permission
      // Include collections that already contain the artifact (will be shown as disabled)
      const editableCollections = this.allCollections.filter(c => {
        return this.canEdit(c);
      // Show root collections initially
      this.displayRootCollections(editableCollections);
      this.errorMessage = 'An error occurred while loading collections';
  private async loadUserPermissions(): Promise<void> {
    // Load permissions for collections not owned by current user
    const nonOwnedCollections = this.allCollections.filter(
      c => c.OwnerID && c.OwnerID !== this.currentUser.ID
    if (nonOwnedCollections.length === 0) {
    const collectionIds = nonOwnedCollections.map(c => c.ID);
    const permissions = await this.permissionService.checkBulkPermissions(
      collectionIds,
    permissions.forEach((permission, collectionId) => {
      this.userPermissions.set(collectionId, permission);
  private displayRootCollections(editableCollections: MJCollectionEntity[]): void {
    const rootCollections = editableCollections.filter(c => !c.ParentID);
    this.displayedCollections = rootCollections.map(c => this.createNode(c, editableCollections));
  private displayChildCollections(parentId: string, editableCollections: MJCollectionEntity[]): void {
    const childCollections = editableCollections.filter(c => c.ParentID === parentId);
    this.displayedCollections = childCollections.map(c => this.createNode(c, editableCollections));
  private createNode(collection: MJCollectionEntity, allEditableCollections: MJCollectionEntity[]): CollectionNode {
    const hasChildren = allEditableCollections.some(c => c.ParentID === collection.ID);
    const alreadyContainsArtifact = this.excludeCollectionIds.includes(collection.ID);
      collection,
      selected: this.selectedCollections.some(sc => sc.ID === collection.ID),
      hasChildren,
      alreadyContainsArtifact
  canEdit(collection: MJCollectionEntity): boolean {
    // Backwards compatibility: treat null OwnerID as owned by current user
    if (!collection.OwnerID || collection.OwnerID === this.currentUser.ID) {
    // Check permission record
    const permission = this.userPermissions.get(collection.ID);
    return permission?.canEdit || false;
  toggleSelection(node: CollectionNode): void {
    // Don't allow selection of collections that already contain the artifact
    if (node.alreadyContainsArtifact) {
    const index = this.selectedCollections.findIndex(c => c.ID === node.collection.ID);
      this.selectedCollections.splice(index, 1);
      node.selected = false;
      this.selectedCollections.push(node.collection);
      node.selected = true;
  drillIntoCollection(collection: MJCollectionEntity): void {
    // Add current location to navigation path
    const node = this.createNode(collection, editableCollections);
    this.navigationPath.push(node);
    this.currentParentId = collection.ID;
    this.currentParentCollection = collection;
    // Display child collections
    this.displayChildCollections(collection.ID, editableCollections);
    // Clear search when drilling down
  navigateToRoot(): void {
  navigateToCollection(collection: MJCollectionEntity): void {
    // Find the index of this collection in the navigation path
    const index = this.navigationPath.findIndex(n => n.collection.ID === collection.ID);
      // Trim navigation path to this level
      this.navigationPath = this.navigationPath.slice(0, index + 1);
    if (!this.searchQuery.trim()) {
      // Reset to current navigation context
      if (this.currentParentId) {
        this.displayChildCollections(this.currentParentId, editableCollections);
    // Search across all editable collections
      return this.canEdit(c) && c.Name.toLowerCase().includes(query);
    this.displayedCollections = editableCollections.map(c => this.createNode(c, editableCollections));
  async createCollection(): Promise<void> {
    if (!this.newCollectionName.trim()) {
      this.toastService.warning('Please enter a collection name');
      this.isCreatingCollection = true;
      const collection = await md.GetEntityObject<MJCollectionEntity>('MJ: Collections', this.currentUser);
      collection.Name = this.newCollectionName.trim();
      collection.EnvironmentID = this.environmentId;
      // Set parent and owner based on current navigation context
      if (this.currentParentCollection) {
        // Creating sub-collection - inherit parent's owner
        collection.ParentID = this.currentParentCollection.ID;
        collection.OwnerID = this.currentParentCollection.OwnerID || this.currentUser.ID;
        // Creating root collection - current user becomes owner
        collection.OwnerID = this.currentUser.ID;
      const saved = await collection.Save();
        // Create owner permission or copy parent permissions
          // Copy permissions from parent
          await this.permissionService.copyParentPermissions(
            this.currentParentCollection.ID,
            collection.ID,
          // Create owner permission
          await this.permissionService.createOwnerPermission(
        this.toastService.success('Collection created successfully');
        this.showCreateForm = false;
        this.newCollectionName = '';
        // Reload collections to include the new one
        // Auto-select the newly created collection
        this.selectedCollections.push(collection);
        this.toastService.error(collection.LatestResult?.Message || 'Failed to create collection');
      console.error('Error creating collection:', error);
      this.toastService.error('An error occurred while creating the collection');
      this.isCreatingCollection = false;
  async onSave(): Promise<void> {
    if (this.selectedCollections.length === 0) {
      this.toastService.warning('Please select at least one collection');
    // Emit the selected collection IDs
    const collectionIds = this.selectedCollections.map(c => c.ID);
    this.saved.emit(collectionIds);
    // Note: Parent component will handle the actual saving and close the modal
