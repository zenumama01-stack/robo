import { MJListEntity, MJListCategoryEntity, MJListDetailEntity } from '@memberjunction/core-entities';
interface ListViewModel {
  lists: ListViewModel[];
@RegisterClass(BaseResourceComponent, 'ListsMyListsResource')
  selector: 'mj-lists-my-lists-resource',
    <div class="lists-my-lists-container">
      <div class="lists-header">
          <h2>My Lists</h2>
      @if (!isLoading && filteredLists.length === 0 && !searchTerm) {
      <!-- No Results State -->
      @if (!isLoading && filteredLists.length === 0 && searchTerm) {
          <p>No lists match "<strong>{{searchTerm}}</strong>"</p>
          <p class="empty-hint">Try a different search term or clear your search.</p>
          <button class="btn-clear" (click)="clearSearch()">Clear Search</button>
      <!-- Lists Grid -->
        <div class="lists-content">
              (click)="viewMode = 'grid'"
              title="Grid view">
              (click)="viewMode = 'list'"
            <span class="list-count">{{filteredLists.length}} list{{filteredLists.length !== 1 ? 's' : ''}}</span>
          <!-- Category Tree View -->
            <div class="lists-grid" role="list" aria-label="My Lists">
                  (click)="openList(item.list)"
                  (keydown.enter)="openList(item.list)"
                  (keydown.space)="openList(item.list); $event.preventDefault()"
                  role="listitem"
                  [attr.aria-label]="item.list.Name + ' - ' + item.entityName + ' - ' + item.itemCount + ' items'">
                    <div class="card-icon" [style.background-color]="getEntityColor(item.list.Entity)" aria-hidden="true">
                      <i [class]="getEntityIcon(item.list.Entity)"></i>
                      <button class="menu-btn" (click)="openListMenu($event, item.list)" [attr.aria-label]="'More options for ' + item.list.Name">
                    @if (item.list.CategoryID) {
                      <span class="category-tag">
                        {{getCategoryName(item.list.CategoryID)}}
                    <span class="date-info">Updated {{formatDate(item.list.__mj_UpdatedAt)}}</span>
                  <div class="list-icon" [style.background-color]="getEntityColor(item.list.Entity)" aria-hidden="true">
                    <span class="list-meta">{{item.entityName}} &middot; {{item.itemCount}} items</span>
                    <button class="action-btn" (click)="openListMenu($event, item.list)" [attr.aria-label]="'More options for ' + item.list.Name">
      <!-- Context Menu (native) -->
      <!-- Create/Edit List Dialog (native modal) -->
                  [disabled]="!!editingList" />
              [disabled]="!newListName || !selectedEntityId || isSaving">
            <button class="btn-danger" (click)="deleteList()">
      <!-- Entity Dropdown Portal (fixed positioning) -->
            @for (entity of filteredEntities; track entity) {
    .lists-my-lists-container {
    .lists-header {
    .search-empty .empty-state-icon-wrapper {
    .lists-content {
    .list-count {
    /* Category Tree */
    .list-row:hover .action-btn {
    /* Context Menu (native) */
    /* Portal Dropdown - Fixed positioning to escape modal z-index */
      z-index: 10002; /* Above modal (1001) */
export class ListsMyListsResource extends BaseResourceComponent implements OnDestroy {
  viewMode: 'grid' | 'list' = 'grid';
  allLists: ListViewModel[] = [];
  filteredLists: ListViewModel[] = [];
  flatCategories: Array<{ ID: string | null; displayName: string }> = [];
  filteredEntities: Array<{ ID: string; Name: string }> = [];
  // Context menu
  selectedContextList: MJListEntity | null = null;
  // Create/Edit dialog
  // Delete confirmation
    private elementRef: ElementRef
    // Close entity dropdown when clicking outside
        console.error('No current user');
      // Load lists, categories, and item counts in parallel
      const [listsResult, categoriesResult, detailsResult] = await rv.RunViews([
          ExtraFilter: `UserID = '${userId}'`,
          ExtraFilter: `ListID IN (SELECT ID FROM __mj.List WHERE UserID = '${userId}')`,
      if (!listsResult.Success || !categoriesResult.Success) {
        console.error('Failed to load lists data');
      const details = detailsResult.Results as Array<{ ListID: string }>;
      // Build list view models
      this.allLists = lists.map(list => ({
        entityName: list.Entity || 'Unknown'
      this.filteredEntities = [...this.availableEntities];
    const uncategorizedLists: ListViewModel[] = [];
  private applyFilter() {
    if (!this.searchTerm) {
      this.filteredLists = [...this.allLists];
      this.filteredLists = this.allLists.filter(item =>
        item.entityName.toLowerCase().includes(term)
  getCategoryName(categoryId: string): string {
    return this.categoryMap.get(categoryId)?.Name || 'Unknown';
    if (diffDays === 0) return 'today';
    if (diffDays === 1) return 'yesterday';
    if (diffDays < 30) return `${Math.floor(diffDays / 7)} weeks ago`;
  openList(list: MJListEntity) {
    this.tabService.OpenList(list.ID, list.Name, appId);
  openListMenu(event: Event, list: MJListEntity) {
    this.selectedContextList = list;
    this.selectedContextList = null;
    if (!this.selectedContextList) return;
    this.editingList = this.selectedContextList;
    this.newListName = this.selectedContextList.Name;
    this.newListDescription = this.selectedContextList.Description || '';
    this.selectedEntityId = this.selectedContextList.EntityID;
    this.entitySearchTerm = this.selectedContextList.Entity || '';
    this.selectedCategoryId = this.selectedContextList.CategoryID || null;
    this.filteredEntities = this.availableEntities.filter(e =>
    const dropdownHeight = 200; // estimated max height
    const listToDuplicate = this.selectedContextList;
    this.listToDelete = this.selectedContextList;
    this.deleteListName = this.selectedContextList.Name;
    return 'My Lists';
