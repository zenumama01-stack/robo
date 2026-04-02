import { Component, ViewEncapsulation, ChangeDetectorRef, OnDestroy, ElementRef, HostListener } from '@angular/core';
import { ResourceData, MJListCategoryEntity } from '@memberjunction/core-entities';
import { MJListEntity, MJListDetailEntity } from '@memberjunction/core-entities';
import { TabService } from '@memberjunction/ng-base-application';
import { ListSharingService, ListSharingSummary, ListShareDialogConfig, ListShareDialogResult } from '@memberjunction/ng-list-management';
interface BrowseListItem {
  list: MJListEntity;
  ownerName: string;
  sharingInfo?: ListSharingSummary;
  category: MJListCategoryEntity | null;
  lists: BrowseListItem[];
  isExpanded: boolean;
type ViewMode = 'table' | 'card' | 'hierarchy';
@RegisterClass(BaseResourceComponent, 'ListsBrowseResource')
  selector: 'mj-lists-browse-resource',
    <div class="lists-browse-container">
      <div class="browse-header">
        <div class="header-row">
            <h2>Lists</h2>
          <button class="btn-create" (click)="createNewList()">
            <span>New List</span>
              placeholder="Search lists..."
              (ngModelChange)="onSearchChange($event)" />
            @if (searchTerm) {
              [(ngModel)]="selectedOwner"
              (ngModelChange)="onOwnerFilterChange($event)"
              title="Filter by owner">
              @for (opt of ownerOptions; track opt) {
                <option [value]="opt.value">{{opt.name}}</option>
              [(ngModel)]="selectedEntity"
              (ngModelChange)="onEntityFilterChange($event)"
              title="Filter by entity">
              @for (opt of entityOptions; track opt) {
          <div class="view-toggle-group">
              class="view-toggle"
              title="Table view">
              [class.active]="viewMode === 'card'"
              (click)="setViewMode('card')"
              title="Card view">
              [class.active]="viewMode === 'hierarchy'"
              (click)="setViewMode('hierarchy')"
              title="Category view">
          <mj-loading text="Loading lists..." size="medium"></mj-loading>
      <!-- Empty State - No Lists -->
      @if (!isLoading && allLists.length === 0) {
          <div class="empty-state-icon-wrapper">
            <div class="icon-bg"></div>
          <h3>No Lists Yet</h3>
          <p>Lists help you organize and track groups of records across your data.</p>
          <div class="empty-state-features">
            <div class="feature-item">
              <span>Group records from any entity</span>
              <span>Organize with categories</span>
              <span>Quick access from any view</span>
          <button class="btn-create-large" (click)="createNewList()">
            Create Your First List
      <!-- Empty State - No Results -->
      @if (!isLoading && allLists.length > 0 && filteredLists.length === 0) {
        <div class="empty-state search-empty">
          <div class="empty-state-icon-wrapper search">
          <h3>No Results Found</h3>
          <p>No lists match your current filters.</p>
          <p class="empty-hint">Try adjusting your search or filters.</p>
          <button class="btn-clear" (click)="clearFilters()">Clear All Filters</button>
      <!-- Results Content -->
      @if (!isLoading && filteredLists.length > 0) {
        <div class="browse-content">
          <div class="results-header">
            <span class="result-count">{{filteredLists.length}} list{{filteredLists.length !== 1 ? 's' : ''}}</span>
            <div class="sort-options">
              <label>Sort:</label>
                [(ngModel)]="selectedSort"
                (ngModelChange)="onSortChange($event)"
                class="filter-select sort-select">
                @for (opt of sortOptions; track opt) {
          @if (viewMode === 'table') {
            <div class="lists-table">
              <table role="grid" aria-label="Lists table">
                    <th class="col-name" scope="col">Name</th>
                    <th class="col-entity" scope="col">Entity</th>
                    <th class="col-items" scope="col">Items</th>
                    <th class="col-sharing" scope="col">Shared</th>
                    <th class="col-owner" scope="col">Owner</th>
                    <th class="col-updated" scope="col">Updated</th>
                    <th class="col-actions" scope="col"><span class="sr-only">Actions</span></th>
                  @for (item of filteredLists; track item) {
                      (click)="openList(item)"
                      (keydown.enter)="openList(item)"
                      tabindex="0"
                      role="row">
                      <td class="col-name" role="gridcell">
                          <div class="list-icon" [style.background-color]="getEntityColor(item.entityName)" aria-hidden="true">
                            <i [class]="getEntityIcon(item.entityName)"></i>
                          <div class="name-content">
                            <span class="list-name">{{item.list.Name}}</span>
                            @if (item.list.Description) {
                              <span class="list-desc">{{item.list.Description}}</span>
                      <td class="col-entity" role="gridcell">
                        <span class="entity-badge">{{item.entityName}}</span>
                      <td class="col-items" role="gridcell">{{item.itemCount}}</td>
                      <td class="col-sharing" role="gridcell">
                        @if (item.sharingInfo; as sharing) {
                          @if (sharing.totalShares > 0) {
                            <span class="sharing-indicator">
                              <span class="share-count">{{sharing.totalShares}}</span>
                          @if (sharing.totalShares === 0) {
                            <span class="sharing-private">
                        @if (!item.sharingInfo) {
                      <td class="col-owner" role="gridcell">
                        <span class="owner-name" [class.is-me]="item.isOwner">
                          {{item.isOwner ? 'You' : item.ownerName}}
                      <td class="col-updated" role="gridcell">{{formatDate(item.list.__mj_UpdatedAt)}}</td>
                      <td class="col-actions" role="gridcell">
                        @if (item.isOwner) {
                            (click)="openListMenu($event, item)"
                            title="More options">
                            <i class="fa-solid fa-ellipsis-v" aria-hidden="true"></i>
          @if (viewMode === 'card') {
            <div class="lists-grid" role="list" aria-label="Lists">
                  class="list-card"
                  role="listitem">
                    <div class="card-icon" [style.background-color]="getEntityColor(item.entityName)" aria-hidden="true">
                      <div class="card-menu">
                        <button class="menu-btn" (click)="openListMenu($event, item)">
                    <h3 class="card-title">{{item.list.Name}}</h3>
                      <p class="card-description">{{item.list.Description}}</p>
                    <div class="card-meta">
                        {{item.entityName}}
                        {{item.itemCount}} item{{item.itemCount !== 1 ? 's' : ''}}
                    <span class="owner-tag" [class.is-me]="item.isOwner">
                    <div class="card-footer-right">
                          <span class="sharing-badge" [title]="'Shared with ' + sharing.totalShares + ' user(s)/role(s)'">
                      <span class="date-info">{{formatDate(item.list.__mj_UpdatedAt)}}</span>
          <!-- Hierarchy View -->
          @if (viewMode === 'hierarchy') {
            <div class="category-tree">
              @for (node of categoryTree; track node) {
                <ng-container *ngTemplateOutlet="categoryNodeTemplate; context: { node: node, depth: 0 }"></ng-container>
      <!-- Category Node Template -->
      <ng-template #categoryNodeTemplate let-node="node" let-depth="depth">
        <div class="category-section" [style.margin-left.px]="depth * 20">
          <!-- Category Header -->
          @if (node.category) {
              class="category-header"
              (click)="toggleCategory(node)">
              <i [class]="node.isExpanded ? 'fa-solid fa-chevron-down' : 'fa-solid fa-chevron-right'"></i>
              <i class="fa-solid fa-folder" [class.fa-folder-open]="node.isExpanded"></i>
              <span class="category-name">{{node.category.Name}}</span>
              <span class="category-count">{{getListCountInCategory(node)}}</span>
          <!-- Uncategorized Header -->
          @if (!node.category && node.lists.length > 0) {
              class="category-header uncategorized"
              <span class="category-name">Uncategorized</span>
              <span class="category-count">{{node.lists.length}}</span>
          <!-- Lists in this category -->
          @if (node.isExpanded) {
            <div class="category-lists" role="list">
              @for (item of node.lists; track item) {
                  class="list-row hierarchy-row"
                  <div class="list-info">
                    <span class="list-meta">
                      {{item.entityName}} &middot; {{item.itemCount}} items
                      @if (!item.isOwner) {
                        <span> &middot; {{item.ownerName}}</span>
                    <div class="list-actions">
                      <button class="action-btn" (click)="openListMenu($event, item)">
          <!-- Child categories -->
              <ng-container *ngTemplateOutlet="categoryNodeTemplate; context: { node: child, depth: depth + 1 }"></ng-container>
      <!-- Context Menu -->
      @if (showContextMenu) {
        <div class="context-menu-overlay" (click)="closeContextMenu()"></div>
        <div class="context-menu" [style.top.px]="contextMenuY" [style.left.px]="contextMenuX">
          <button class="menu-item" (click)="editList()">
            Edit
          <button class="menu-item" (click)="openShareDialog()">
            Share
          <button class="menu-item" (click)="duplicateList()">
            Duplicate
          <div class="menu-divider"></div>
          <button class="menu-item danger" (click)="confirmDeleteList()">
      @if (showCreateDialog) {
        <div class="modal-overlay" (click)="closeCreateDialog()"></div>
        <div class="modal-dialog">
          <div class="modal-header">
            <h3>{{editingList ? 'Edit List' : 'Create New List'}}</h3>
            <button class="modal-close" (click)="closeCreateDialog()">
          <div class="modal-body">
              <label>Name *</label>
                [(ngModel)]="newListName"
                placeholder="Enter list name"
                [(ngModel)]="newListDescription"
                placeholder="Optional description"
            @if (!editingList) {
                <label>Entity *</label>
                <div class="custom-select-wrapper">
                    #entityInput
                    (ngModelChange)="filterEntities($event)"
                    (focus)="openEntityDropdown(entityInput)"
                    placeholder="Search and select an entity"
            @if (editingList) {
                <label>Entity</label>
                <input type="text" [value]="entitySearchTerm" class="form-input" disabled />
              <select [(ngModel)]="selectedCategoryId" class="form-input">
                <option [ngValue]="null">No category</option>
                @for (cat of flatCategories; track cat) {
                  <option [ngValue]="cat.ID">{{cat.displayName}}</option>
          <div class="modal-footer">
              (click)="saveList()"
              [disabled]="!newListName || (!editingList && !selectedEntityId) || isSaving">
              @if (isSaving) {
              {{isSaving ? 'Saving...' : (editingList ? 'Save' : 'Create')}}
            <button class="btn-secondary" (click)="closeCreateDialog()" [disabled]="isSaving">Cancel</button>
      <!-- Delete Confirmation Dialog -->
      @if (showDeleteConfirm) {
        <div class="modal-overlay" (click)="cancelDelete()"></div>
        <div class="modal-dialog confirm-dialog">
            <h3>Delete List</h3>
            <button class="modal-close" (click)="cancelDelete()">
            <p>Are you sure you want to delete "<strong>{{deleteListName}}</strong>"?</p>
            <p class="warning-text">This will also remove all items in the list.</p>
            <button class="btn-danger" (click)="deleteList()" [disabled]="isDeleting">
              @if (isDeleting) {
              {{isDeleting ? 'Deleting...' : 'Delete'}}
            <button class="btn-secondary" (click)="cancelDelete()" [disabled]="isDeleting">Cancel</button>
      @if (shareDialogConfig) {
          (complete)="onShareComplete($event)"
          (cancel)="onShareCancel()">
      <!-- Entity Dropdown Portal -->
      @if (showEntityDropdown && !editingList) {
          class="entity-dropdown-portal"
          [style.top.px]="entityDropdownPosition.top"
          [style.left.px]="entityDropdownPosition.left"
          [style.width.px]="entityDropdownPosition.width"
          [class.dropdown-above]="entityDropdownPosition.openAbove">
          <div class="entity-dropdown-backdrop" (click)="closeEntityDropdown()"></div>
          <div class="entity-dropdown-content" [class.open-above]="entityDropdownPosition.openAbove">
            @for (entity of filteredEntitiesList; track entity) {
                class="dropdown-item"
                {{entity.Name}}
            @if (filteredEntitiesList.length === 0) {
              <div class="dropdown-empty">
    .lists-browse-container {
    .browse-header {
    .header-row {
    .header-title h2 {
    .search-box i.fa-search {
      transition: border-color 0.2s, box-shadow 0.2s;
    .sort-select {
    .view-toggle-group {
    .view-toggle:last-child {
    .view-toggle:hover {
    .view-toggle.active {
      padding: 48px 40px;
    .empty-state-icon-wrapper {
    .empty-state-icon-wrapper .icon-bg {
      background: linear-gradient(135deg, rgba(33, 150, 243, 0.1) 0%, rgba(33, 150, 243, 0.05) 100%);
    .empty-state-icon-wrapper > i {
    .empty-state-icon-wrapper.search > i {
    .empty-state p:last-of-type {
      color: #999 !important;
      font-size: 13px !important;
    .empty-state-features {
    .feature-item {
    .feature-item i {
      font-size: 14px !important;
      color: #4CAF50 !important;
    .btn-create-large {
      background: linear-gradient(135deg, #2196F3 0%, #1976D2 100%);
    .btn-create-large:hover {
      background: linear-gradient(135deg, #1976D2 0%, #1565C0 100%);
      box-shadow: 0 4px 12px rgba(33, 150, 243, 0.4);
    .btn-clear {
    .btn-clear:hover {
    .browse-content {
    .results-header {
    .sort-options {
    .sort-options label {
    .lists-table {
      box-shadow: 0 1px 3px rgba(0,0,0,0.08);
    .lists-table table {
    .lists-table th {
    .lists-table td {
    .list-row:focus {
      background: #e8f4fd;
    .list-row:focus-visible {
      box-shadow: inset 3px 0 0 #2196F3;
    .list-row:last-child td {
    .sr-only {
      margin: -1px;
      clip: rect(0, 0, 0, 0);
    .col-name { width: 30%; }
    .col-entity { width: 15%; }
    .col-items { width: 8%; text-align: center; }
    .col-sharing { width: 8%; text-align: center; }
    .col-owner { width: 14%; }
    .col-updated { width: 15%; }
    .col-actions { width: 10%; text-align: right; }
    /* Sharing indicators */
    .sharing-indicator {
    .sharing-indicator i {
    .share-count {
    .sharing-private {
    .sharing-badge {
    .card-footer-right {
    .list-icon {
    .name-content {
    .list-desc {
    .owner-name.is-me {
    /* Card View */
    .lists-grid {
    .list-card {
      transition: transform 0.2s, box-shadow 0.2s, outline 0.1s;
      outline: 2px solid transparent;
    .list-card:hover {
      box-shadow: 0 4px 12px rgba(0,0,0,0.12);
    .list-card:focus {
      outline: 2px solid #2196F3;
    .list-card:focus:not(:focus-visible) {
    .list-card:focus-visible {
      padding: 16px 16px 0;
    .menu-btn {
    .menu-btn:hover {
    .owner-tag {
    .owner-tag.is-me {
    .date-info {
    /* Hierarchy View */
    .category-tree {
    .category-header i:first-child {
    .category-header .fa-folder,
    .category-header .fa-folder-open {
    .category-header.uncategorized .fa-inbox {
    .category-lists {
    .hierarchy-row {
      padding: 12px 16px 12px 40px;
    .hierarchy-row:hover {
    .hierarchy-row:focus {
    .hierarchy-row:focus-visible {
    .list-info {
    .list-meta {
    .list-actions {
    .hierarchy-row:hover .list-actions {
    /* Context Menu */
    .context-menu-overlay {
    .context-menu {
      box-shadow: 0 4px 16px rgba(0,0,0,0.15);
    .menu-item {
    .menu-item:hover {
    .menu-item.danger {
    .menu-item.danger:hover {
    .menu-divider {
    /* Modal Styles */
    .modal-dialog {
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2);
    .confirm-dialog {
    .modal-header {
    .modal-header h3 {
    .modal-close {
    .modal-close:hover {
    .modal-body {
      max-height: 60vh;
    .modal-body p {
    .warning-text {
      color: #d32f2f !important;
    .modal-footer {
    .btn-primary:disabled {
      background: #c62828;
    /* Form Styles */
    .form-group:last-child {
    .form-input:disabled {
    textarea.form-input {
    select.form-input {
    .custom-select-wrapper {
    /* Portal Dropdown */
    .entity-dropdown-portal {
      z-index: 10002;
    .entity-dropdown-backdrop {
    .entity-dropdown-content {
      box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);
    .entity-dropdown-content.open-above {
    .dropdown-item {
    .dropdown-item:hover {
    .dropdown-item:first-child {
      border-radius: 6px 6px 0 0;
    .dropdown-item:last-child {
      border-radius: 0 0 6px 6px;
    .dropdown-empty {
      .col-entity, .col-items, .col-updated {
        width: 95vw;
export class ListsBrowseResource extends BaseResourceComponent implements OnDestroy {
  viewMode: ViewMode = 'card';
  selectedEntity = 'all';
  selectedOwner = 'mine';
  selectedSort = 'name';
  allLists: BrowseListItem[] = [];
  filteredLists: BrowseListItem[] = [];
  categories: MJListCategoryEntity[] = [];
  categoryTree: CategoryNode[] = [];
  flatCategories: Array<{ ID: string; displayName: string }> = [];
  availableEntities: Array<{ ID: string; Name: string }> = [];
  filteredEntitiesList: Array<{ ID: string; Name: string }> = [];
  entityOptions: Array<{ name: string; value: string }> = [{ name: 'All Entities', value: 'all' }];
  ownerOptions: Array<{ name: string; value: string }> = [
    { name: 'My Lists', value: 'mine' },
    { name: 'All Lists', value: 'all' },
    { name: 'Others', value: 'others' }
  sortOptions: Array<{ name: string; value: string }> = [
    { name: 'Name', value: 'name' },
    { name: 'Recently Updated', value: 'updated' },
    { name: 'Most Items', value: 'items' },
    { name: 'Entity', value: 'entity' }
  // Context menu state
  showContextMenu = false;
  contextMenuX = 0;
  contextMenuY = 0;
  selectedContextItem: BrowseListItem | null = null;
  // Create/Edit dialog state
  showCreateDialog = false;
  editingList: MJListEntity | null = null;
  newListName = '';
  newListDescription = '';
  selectedEntityId = '';
  selectedCategoryId: string | null = null;
  entitySearchTerm = '';
  showEntityDropdown = false;
  entityDropdownPosition = { top: 0, left: 0, width: 0, openAbove: false };
  // Delete confirmation state
  showDeleteConfirm = false;
  deleteListName = '';
  listToDelete: MJListEntity | null = null;
  // Operation states
  isSaving = false;
  isDeleting = false;
  // Sharing dialog state
  showShareDialog = false;
  shareDialogConfig: ListShareDialogConfig | null = null;
  private entityColorMap: Map<string, string> = new Map();
  private entityIconMap: Map<string, string> = new Map();
  private categoryMap: Map<string, MJListCategoryEntity> = new Map();
  private currentUserId = '';
    private tabService: TabService,
    private notificationService: MJNotificationService,
    private elementRef: ElementRef,
    private listSharingService: ListSharingService
  onDocumentClick(event: MouseEvent) {
    if (this.showEntityDropdown) {
      if (!target.closest('.custom-select-wrapper')) {
        this.showEntityDropdown = false;
  onEscapeKey() {
    if (this.showContextMenu) {
      this.closeContextMenu();
    if (this.showCreateDialog) {
      this.closeCreateDialog();
    if (this.showDeleteConfirm) {
      this.cancelDelete();
      this.closeEntityDropdown();
      this.currentUserId = md.CurrentUser?.ID || '';
      // Load all lists, categories, details, and users in parallel
      const [listsResult, categoriesResult, detailsResult, usersResult] = await rv.RunViews([
          Fields: ['ListID'],
          Fields: ['ID', 'Name'],
        console.error('Failed to load lists');
      const lists = listsResult.Results as MJListEntity[];
      this.categories = (categoriesResult.Results || []) as MJListCategoryEntity[];
      const details = (detailsResult.Results || []) as Array<{ ListID: string }>;
      const users = (usersResult.Results || []) as Array<{ ID: string; Name: string }>;
      // Build category map
      this.categoryMap.clear();
      for (const cat of this.categories) {
        this.categoryMap.set(cat.ID, cat);
      // Build flat categories for dropdown
      this.flatCategories = this.buildFlatCategories(this.categories);
      // Build user map
      const userMap = new Map<string, string>();
      for (const user of users) {
        userMap.set(user.ID, user.Name);
      // Count items per list
      const itemCounts = new Map<string, number>();
        const count = itemCounts.get(detail.ListID) || 0;
        itemCounts.set(detail.ListID, count + 1);
      // Build entity info
      const entities = md.Entities;
      const entitySet = new Set<string>();
        this.entityColorMap.set(entity.Name, this.generateEntityColor(entity.Name));
        this.entityIconMap.set(entity.Name, entity.Icon || 'fa-solid fa-table');
      // Build available entities for dropdown
      this.availableEntities = entities
        .filter(e => e.IncludeInAPI)
        .map(e => ({ ID: e.ID, Name: e.Name }))
      this.filteredEntitiesList = [...this.availableEntities];
      // Build list items
      this.allLists = lists.map(list => {
        const entityName = list.Entity || 'Unknown';
        entitySet.add(entityName);
          list,
          itemCount: itemCounts.get(list.ID) || 0,
          ownerName: userMap.get(list.UserID) || 'Unknown',
          isOwner: list.UserID === this.currentUserId
      // Build entity filter options
      this.entityOptions = [
        { name: 'All Entities', value: 'all' },
        ...Array.from(entitySet).sort().map(e => ({ name: e, value: e }))
      // Load sharing info in the background
      this.loadSharingInfo();
      console.error('Error loading lists:', error);
  private buildFlatCategories(categories: MJListCategoryEntity[]): Array<{ ID: string; displayName: string }> {
    const result: Array<{ ID: string; displayName: string }> = [];
    const topLevel = categories.filter(c => !c.ParentID);
    const processCategory = (cat: MJListCategoryEntity, level: number) => {
      const indent = '\u00A0\u00A0'.repeat(level);
      result.push({ ID: cat.ID, displayName: `${indent}${cat.Name}` });
      const children = categories.filter(c => c.ParentID === cat.ID);
      for (const child of children) {
        processCategory(child, level + 1);
    for (const cat of topLevel) {
      processCategory(cat, 0);
    const rootNodes: CategoryNode[] = [];
    const categoryNodes = new Map<string, CategoryNode>();
      categoryNodes.set(cat.ID, {
        category: cat,
        lists: [],
        isExpanded: true
      const node = categoryNodes.get(cat.ID)!;
      if (cat.ParentID && categoryNodes.has(cat.ParentID)) {
        categoryNodes.get(cat.ParentID)!.children.push(node);
    // Assign lists to categories
    const uncategorizedLists: BrowseListItem[] = [];
    for (const item of this.filteredLists) {
      if (item.list.CategoryID && categoryNodes.has(item.list.CategoryID)) {
        categoryNodes.get(item.list.CategoryID)!.lists.push(item);
        uncategorizedLists.push(item);
    // Add uncategorized node if there are uncategorized lists
    if (uncategorizedLists.length > 0) {
      rootNodes.unshift({
        lists: uncategorizedLists,
  setViewMode(mode: ViewMode) {
  onSearchChange(_term: string) {
  onEntityFilterChange(_value: string) {
  onOwnerFilterChange(_value: string) {
  onSortChange(_value: string) {
  clearFilters() {
    this.selectedEntity = 'all';
    this.selectedOwner = 'mine';
  private applyFilters() {
    let result = [...this.allLists];
    // Owner filter
    if (this.selectedOwner === 'mine') {
      result = result.filter(item => item.isOwner);
    } else if (this.selectedOwner === 'others') {
      result = result.filter(item => !item.isOwner);
      result = result.filter(item =>
        item.list.Name.toLowerCase().includes(term) ||
        (item.list.Description && item.list.Description.toLowerCase().includes(term)) ||
        item.entityName.toLowerCase().includes(term) ||
        item.ownerName.toLowerCase().includes(term)
    // Entity filter
    if (this.selectedEntity !== 'all') {
      result = result.filter(item => item.entityName === this.selectedEntity);
    // Sort
    switch (this.selectedSort) {
        result.sort((a, b) => a.list.Name.localeCompare(b.list.Name));
          const dateA = new Date(a.list.__mj_UpdatedAt).getTime();
          const dateB = new Date(b.list.__mj_UpdatedAt).getTime();
        result.sort((a, b) => b.itemCount - a.itemCount);
        result.sort((a, b) => a.entityName.localeCompare(b.entityName));
    this.filteredLists = result;
  toggleCategory(node: CategoryNode) {
    node.isExpanded = !node.isExpanded;
  getListCountInCategory(node: CategoryNode): number {
    let count = node.lists.length;
      count += this.getListCountInCategory(child);
  getEntityColor(entityName: string): string {
    return this.entityColorMap.get(entityName) || '#607D8B';
  getEntityIcon(entityName: string): string {
    return this.entityIconMap.get(entityName) || 'fa-solid fa-table';
  private generateEntityColor(entityName: string): string {
    for (let i = 0; i < entityName.length; i++) {
      hash = entityName.charCodeAt(i) + ((hash << 5) - hash);
      '#2196F3', '#4CAF50', '#FF9800', '#9C27B0', '#F44336',
      '#00BCD4', '#795548', '#607D8B', '#E91E63', '#3F51B5'
  openList(item: BrowseListItem) {
    const appId = this.Data?.Configuration?.applicationId || '';
    this.tabService.OpenList(item.list.ID, item.list.Name, appId);
  openListMenu(event: Event, item: BrowseListItem) {
    const mouseEvent = event as MouseEvent;
    this.selectedContextItem = item;
    this.contextMenuX = mouseEvent.clientX;
    this.contextMenuY = mouseEvent.clientY;
    this.showContextMenu = true;
  closeContextMenu() {
    this.showContextMenu = false;
    this.selectedContextItem = null;
  createNewList() {
    this.editingList = null;
    this.newListName = '';
    this.newListDescription = '';
    this.selectedEntityId = '';
    this.entitySearchTerm = '';
    this.selectedCategoryId = null;
    this.showCreateDialog = true;
  editList() {
    if (!this.selectedContextItem) return;
    const list = this.selectedContextItem.list;
    this.editingList = list;
    this.newListName = list.Name;
    this.newListDescription = list.Description || '';
    this.selectedEntityId = list.EntityID;
    this.entitySearchTerm = list.Entity || '';
    this.selectedCategoryId = list.CategoryID || null;
  selectEntity(entity: { ID: string; Name: string }) {
    this.selectedEntityId = entity.ID;
    this.entitySearchTerm = entity.Name;
  filterEntities(term: string) {
    const lowerTerm = term.toLowerCase();
    this.filteredEntitiesList = this.availableEntities.filter(e =>
      e.Name.toLowerCase().includes(lowerTerm)
  openEntityDropdown(inputElement: HTMLInputElement) {
    const rect = inputElement.getBoundingClientRect();
    const viewportHeight = window.innerHeight;
    const dropdownHeight = 200;
    const spaceBelow = viewportHeight - rect.bottom;
    const openAbove = spaceBelow < dropdownHeight && rect.top > dropdownHeight;
    this.entityDropdownPosition = {
      top: openAbove ? rect.top - dropdownHeight : rect.bottom,
      left: rect.left,
      width: rect.width,
      openAbove
    this.showEntityDropdown = true;
  closeEntityDropdown() {
  async duplicateList() {
    const listToDuplicate = this.selectedContextItem.list;
      const newList = await md.GetEntityObject<MJListEntity>('MJ: Lists');
      newList.Name = `${listToDuplicate.Name} (Copy)`;
      newList.Description = listToDuplicate.Description;
      newList.EntityID = listToDuplicate.EntityID;
      newList.CategoryID = listToDuplicate.CategoryID;
      newList.UserID = md.CurrentUser!.ID;
      const listSaved = await newList.Save();
      if (!listSaved) {
        this.notificationService.CreateSimpleNotification('Failed to duplicate list', 'error', 4000);
      const itemsResult = await rv.RunView<MJListDetailEntity>({
        ExtraFilter: `ListID = '${listToDuplicate.ID}'`,
      if (itemsResult.Success && itemsResult.Results.length > 0) {
        let copiedCount = 0;
        for (const item of itemsResult.Results) {
          const newItem = await md.GetEntityObject<MJListDetailEntity>('MJ: List Details');
          newItem.ListID = newList.ID;
          newItem.RecordID = item.RecordID;
          newItem.Sequence = item.Sequence;
          const itemSaved = await newItem.Save();
          if (itemSaved) copiedCount++;
          `List duplicated with ${copiedCount} item${copiedCount !== 1 ? 's' : ''}`,
        this.notificationService.CreateSimpleNotification('List duplicated successfully', 'success', 3000);
      console.error('Error duplicating list:', error);
      this.notificationService.CreateSimpleNotification('Error duplicating list. Please try again.', 'error', 4000);
  confirmDeleteList() {
    this.listToDelete = this.selectedContextItem.list;
    this.deleteListName = this.selectedContextItem.list.Name;
    this.showDeleteConfirm = true;
  cancelDelete() {
    this.showDeleteConfirm = false;
    this.listToDelete = null;
    this.deleteListName = '';
  async deleteList() {
    if (!this.listToDelete) return;
    const listToDelete = this.listToDelete;
    const listName = listToDelete.Name;
    this.isDeleting = true;
      const deleted = await listToDelete.Delete();
        this.notificationService.CreateSimpleNotification(`"${listName}" deleted`, 'success', 3000);
        const errorMessage = listToDelete.LatestResult?.Message || 'Unknown error occurred';
        console.error('Failed to delete list:', listToDelete.LatestResult);
        this.notificationService.CreateSimpleNotification(`Failed to delete list: ${errorMessage}`, 'error', 6000);
      console.error('Error deleting list:', error);
      this.notificationService.CreateSimpleNotification(`Error deleting list: ${errorMessage}`, 'error', 6000);
      this.isDeleting = false;
  closeCreateDialog() {
    this.showCreateDialog = false;
  async saveList() {
    this.isSaving = true;
    const isEditing = !!this.editingList;
    const listName = this.newListName;
      let list: MJListEntity;
      if (this.editingList) {
        list = this.editingList;
        list = await md.GetEntityObject<MJListEntity>('MJ: Lists');
        list.UserID = md.CurrentUser!.ID;
        list.EntityID = this.selectedEntityId;
      list.Name = this.newListName;
      list.Description = this.newListDescription || null;
      list.CategoryID = this.selectedCategoryId || null;
      const saved = await list.Save();
          isEditing ? `"${listName}" updated` : `"${listName}" created`,
        const errorMessage = list.LatestResult?.Message || 'Unknown error occurred';
        const action = isEditing ? 'update' : 'create';
        console.error(`Failed to ${action} list:`, list.LatestResult);
          `Failed to ${action} list: ${errorMessage}`,
      console.error('Error saving list:', error);
      this.notificationService.CreateSimpleNotification(`Error saving list: ${errorMessage}`, 'error', 6000);
      this.isSaving = false;
  // Sharing methods
  openShareDialog() {
    const item = this.selectedContextItem;
      listId: item.list.ID,
      listName: item.list.Name,
      currentUserId: this.currentUserId,
      isOwner: item.isOwner
  onShareComplete(_result: ListShareDialogResult) {
    // Reload sharing info for all lists
  onShareCancel() {
  private async loadSharingInfo() {
    // Load sharing summaries for all lists that the user owns
    const ownedLists = this.allLists.filter(item => item.isOwner);
    for (const item of ownedLists) {
        const summary = await this.listSharingService.getListSharingSummary(item.list.ID);
        item.sharingInfo = summary;
        console.error(`Error loading sharing info for list ${item.list.ID}:`, error);
    return 'Lists';
    return 'fa-solid fa-list-check';
