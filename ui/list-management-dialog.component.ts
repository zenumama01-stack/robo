import { MJListEntity, MJListCategoryEntity } from '@memberjunction/core-entities';
import { ListManagementService } from '../../services/list-management.service';
  ListOperationDetail
} from '../../models/list-management.models';
 * A gorgeous, responsive dialog for managing list membership.
 * Supports adding/removing records from multiple lists with membership indicators.
 * - Search and filter lists
 * - Visual membership indicators (full/partial/none)
 * - Inline list creation
 * - Mobile-responsive bottom sheet on small screens
 * - Batch operations with progress feedback
  selector: 'mj-list-management-dialog',
  templateUrl: './list-management-dialog.component.html',
  styleUrls: ['./list-management-dialog.component.css']
export class ListManagementDialogComponent implements OnInit, OnDestroy {
   * Configuration for the dialog
  @Input() config!: ListManagementDialogConfig;
   * Controls dialog visibility
   * Emitted when dialog is closed with results
  @Output() complete = new EventEmitter<ListManagementResult>();
  @Output() cancel = new EventEmitter<void>();
  activeTab: ListFilterTab = 'all';
  sortOption: ListSortOption = 'name';
  showCreateForm = false;
  allLists: ListItemViewModel[] = [];
  filteredLists: ListItemViewModel[] = [];
  // Create form state
  newListCategoryId: string | null = null;
  // Track changes
  private originalMembership = new Map<string, boolean>();
  public addedToLists: Set<string> = new Set();
  public removedFromLists: Set<string> = new Set();
  public newlyCreatedLists: MJListEntity[] = [];
    private listService: ListManagementService,
   * Get dialog title based on config
    if (this.config?.dialogTitle) {
      return this.config.dialogTitle;
    const recordCount = this.config?.recordIds?.length || 0;
    const recordText = recordCount === 1 ? '1 record' : `${recordCount} records`;
    switch (this.config?.mode) {
      case 'add':
        return `Add ${recordText} to Lists`;
      case 'remove':
        return `Remove ${recordText} from Lists`;
        return `Manage List Membership`;
   * Get subtitle showing context
  get dialogSubtitle(): string {
    const entityName = this.config?.entityName || 'records';
    if (recordCount === 1 && this.config?.recordDisplayNames?.[0]) {
      return `"${this.config.recordDisplayNames[0]}" from ${entityName}`;
    return `${recordCount} ${entityName} record${recordCount !== 1 ? 's' : ''}`;
   * Check if there are pending changes
  get hasChanges(): boolean {
    return this.addedToLists.size > 0 || this.removedFromLists.size > 0;
   * Get count of lists to add to
  get addCount(): number {
    return this.addedToLists.size;
   * Get count of lists to remove from
  get removeCount(): number {
    return this.removedFromLists.size;
   * Setup search with debounce
    ).subscribe((searchText: string) => {
      this.searchText = searchText;
   * Initialize dialog when opened
   * Reset all state
    this.activeTab = 'all';
    this.sortOption = 'name';
    this.newListCategoryId = null;
    this.addedToLists.clear();
    this.removedFromLists.clear();
    this.newlyCreatedLists = [];
    this.originalMembership.clear();
   * Load lists and membership data
      // Load lists and membership in parallel
      const [lists, membership, categories] = await Promise.all([
        this.listService.getListsForEntity(
          this.config.entityId,
          md.CurrentUser.ID,
          true // Force refresh
        this.listService.getRecordMembership(
          this.config.recordIds
        this.listService.getListCategories()
      this.allLists = await this.listService.buildListViewModels(
        lists,
        this.config.recordIds,
        membership
      // Store original membership state
      for (const vm of this.allLists) {
        this.originalMembership.set(vm.list.ID, vm.isFullMember || vm.isPartialMember);
      // Pre-select lists if configured
      if (this.config.preSelectedListIds) {
        for (const listId of this.config.preSelectedListIds) {
          const vm = this.allLists.find(l => l.list.ID === listId);
          if (vm) {
            vm.isSelectedForAdd = true;
            this.addedToLists.add(listId);
  onSearchInput(event: Event): void {
   * Change active tab
  setActiveTab(tab: ListFilterTab): void {
   * Change sort option
  setSortOption(option: ListSortOption): void {
    this.sortOption = option;
   * Apply all filters and sorting
      result = result.filter(vm =>
        vm.list.Name.toLowerCase().includes(search) ||
        (vm.list.Description?.toLowerCase().includes(search))
    // Apply tab filter
      case 'my-lists':
        // Already filtered by user in loadData
        // Sort by last updated and take top 10
        result = result
          .sort((a, b) => b.lastUpdated.getTime() - a.lastUpdated.getTime())
      // 'all' and 'shared' - no additional filtering for now
    switch (this.sortOption) {
        result.sort((a, b) => b.lastUpdated.getTime() - a.lastUpdated.getTime());
      case 'item-count':
   * Toggle list selection for adding
  toggleListForAdd(vm: ListItemViewModel): void {
    if (this.config?.mode === 'remove') return;
    const listId = vm.list.ID;
    if (vm.isSelectedForAdd || this.addedToLists.has(listId)) {
      // Deselect
      vm.isSelectedForAdd = false;
      this.addedToLists.delete(listId);
      // Select for add
      vm.isSelectedForRemove = false;
      this.removedFromLists.delete(listId);
   * Toggle list selection for removal
  toggleListForRemove(vm: ListItemViewModel): void {
    if (this.config?.mode === 'add') return;
    if (!this.config?.allowRemove && this.config?.mode !== 'manage') return;
    if (vm.isSelectedForRemove || this.removedFromLists.has(listId)) {
      // Select for remove
      vm.isSelectedForRemove = true;
      this.removedFromLists.add(listId);
   * Get membership indicator class
  getMembershipClass(vm: ListItemViewModel): string {
    if (vm.isFullMember) return 'full-member';
    if (vm.isPartialMember) return 'partial-member';
    return 'not-member';
   * Get membership indicator text
  getMembershipText(vm: ListItemViewModel): string {
    if (vm.isFullMember) {
      return `${vm.membershipCount}/${vm.totalSelectedRecords}`;
    if (vm.isPartialMember) {
    return `0/${vm.totalSelectedRecords}`;
   * Get membership icon
  getMembershipIcon(vm: ListItemViewModel): string {
    if (vm.isFullMember) return 'fa-solid fa-check-circle';
    if (vm.isPartialMember) return 'fa-solid fa-circle-half-stroke';
    return 'fa-regular fa-circle';
   * Show create list form
  showCreateListForm(): void {
    this.showCreateForm = true;
   * Cancel create list form
  cancelCreateList(): void {
   * Create a new list
  async createList(): Promise<void> {
    if (!this.newListName.trim() || !this.config) return;
      const createConfig: CreateListConfig = {
        name: this.newListName.trim(),
        description: this.newListDescription.trim() || undefined,
        categoryId: this.newListCategoryId || undefined,
        entityId: this.config.entityId
      const newList = await this.listService.createList(createConfig);
      if (newList) {
        this.newlyCreatedLists.push(newList);
        // Add to view models and select for add
        const vm: ListItemViewModel = {
          list: newList,
          membershipCount: 0,
          totalSelectedRecords: this.config.recordIds.length,
          isFullMember: false,
          isPartialMember: false,
          isNotMember: true,
          lastUpdated: new Date(),
          isSelectedForAdd: true,
          isSelectedForRemove: false
        this.allLists.unshift(vm);
        this.addedToLists.add(newList.ID);
   * Apply changes and close dialog
  async applyChanges(): Promise<void> {
      newListsCreated: this.newlyCreatedLists
      // Process additions
      if (this.addedToLists.size > 0) {
        console.log(`[ListManagementDialog] Processing additions:`);
        console.log(`  - Lists to add to:`, [...this.addedToLists]);
        console.log(`  - Record IDs:`, this.config.recordIds);
        const addResult = await this.listService.addRecordsToLists(
          [...this.addedToLists],
          true // Skip duplicates
        console.log(`[ListManagementDialog] Add result:`, addResult);
        for (const listId of this.addedToLists) {
            result.added.push({
              listId,
              listName: vm.list.Name,
              recordIds: this.config.recordIds
      // Process removals
      if (this.removedFromLists.size > 0) {
        const removeResult = await this.listService.removeRecordsFromLists(
          [...this.removedFromLists],
        for (const listId of this.removedFromLists) {
            result.removed.push({
      this.complete.emit(result);
      console.error('Error applying changes:', error);
   * Cancel and close dialog
    this.cancel.emit();
    const days = Math.floor(hours / 24);
    if (minutes < 1) return 'just now';
   * Check if we should show the add button for a list
  showAddButton(vm: ListItemViewModel): boolean {
    if (this.config?.mode === 'remove') return false;
    return !vm.isFullMember || !this.originalMembership.get(vm.list.ID);
   * Check if we should show the remove button for a list
  showRemoveButton(vm: ListItemViewModel): boolean {
    if (this.config?.mode === 'add') return false;
    if (!this.config?.allowRemove && this.config?.mode !== 'manage') return false;
    return vm.isFullMember || vm.isPartialMember || this.originalMembership.get(vm.list.ID) === true;
