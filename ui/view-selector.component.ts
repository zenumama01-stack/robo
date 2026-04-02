import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, ChangeDetectorRef, OnDestroy, ViewEncapsulation } from '@angular/core';
import { EntityInfo, Metadata, RunView } from '@memberjunction/core';
import { UserViewEntityExtended, ViewInfo } from '@memberjunction/core-entities';
 * Represents a view in the dropdown
export interface ViewListItem {
  isOwned: boolean;
  isShared: boolean;
  isDefault: boolean;
  userCanEdit: boolean;
  entity: UserViewEntityExtended;
 * Event emitted when a view is selected
export interface ViewSelectedEvent {
  viewId: string | null;
  view: UserViewEntityExtended | null;
 * Event emitted when user requests to save the current view
export interface SaveViewRequestedEvent {
  /** True if user wants to save as a new view */
  saveAsNew: boolean;
 * ViewSelectorComponent - Dropdown for selecting saved views
 * - Shows "(Default)" option when no view is selected
 * - Groups views into "My Views" and "Shared Views"
 * - "Save Current View" and "Manage Views" actions
 * - "Open in Tab" button for saved views
  selector: 'mj-view-selector',
  templateUrl: './view-selector.component.html',
  styleUrls: ['./view-selector.component.css'],
export class ViewSelectorComponent implements OnChanges, OnDestroy {
   * The entity to load views for
  @Input() entity: EntityInfo | null = null;
   * Currently selected view ID (null = default view)
  @Input() selectedViewId: string | null = null;
   * Whether the current view has unsaved modifications
  @Input() viewModified: boolean = false;
   * Emitted when a view is selected
  @Output() viewSelected = new EventEmitter<ViewSelectedEvent>();
   * Emitted when user requests to save the current view
  @Output() saveViewRequested = new EventEmitter<SaveViewRequestedEvent>();
   * Emitted when user wants to open the view management panel
  @Output() manageViewsRequested = new EventEmitter<void>();
   * Emitted when user wants to open the current view in its own tab
  @Output() openInTabRequested = new EventEmitter<string>();
   * Emitted when user wants to configure the current view
  @Output() configureViewRequested = new EventEmitter<void>();
   * Emitted when user wants to create a new record
  @Output() createNewRecordRequested = new EventEmitter<void>();
   * Emitted when user wants to export data to Excel
  @Output() exportRequested = new EventEmitter<void>();
   * Emitted when user wants to duplicate a view (F-005)
  @Output() duplicateViewRequested = new EventEmitter<string>();
   * Emitted when user wants to use the quick save dialog (F-001)
   * Emits true when user explicitly requested "Save As New", false for general save
  @Output() quickSaveRequested = new EventEmitter<boolean>();
   * Emitted when user wants to revert to saved state (F-007)
  @Output() revertRequested = new EventEmitter<void>();
  // Internal state
  public isLoading: boolean = false;
  public isDropdownOpen: boolean = false;
  public myViews: ViewListItem[] = [];
  public sharedViews: ViewListItem[] = [];
  public selectedView: UserViewEntityExtended | null = null;
    if (changes['entity']) {
        this.loadViews();
        this.myViews = [];
        this.sharedViews = [];
        this.selectedView = null;
    if (changes['selectedViewId'] && this.selectedViewId) {
      // If we have a selected view ID and it's in our lists, update selectedView
      this.updateSelectedViewFromId();
   * Load views for the current entity
  public async loadViews(): Promise<void> {
    if (!this.entity) {
      // Load all views for this entity that the user owns OR that are shared
        ExtraFilter: `EntityID = '${this.entity.ID}' AND (UserID = '${userId}' OR IsShared = 1)`,
        // Filter views the user can actually view
        const accessibleViews = result.Results.filter(v => v.UserCanView);
        // Separate into owned and shared
        this.myViews = accessibleViews
          .filter(v => v.UserID === userId)
          .map(v => this.mapViewToListItem(v, true));
        this.sharedViews = accessibleViews
          .filter(v => v.UserID !== userId)
          .map(v => this.mapViewToListItem(v, false));
        // Update selected view reference
      console.error('Failed to load views:', error);
   * Map a view entity to a list item
  private mapViewToListItem(view: UserViewEntityExtended, isOwned: boolean): ViewListItem {
      id: view.ID,
      name: view.Name,
      isOwned,
      isShared: view.IsShared,
      isDefault: view.IsDefault,
      userCanEdit: view.UserCanEdit,
      entity: view
   * Update the selectedView reference based on selectedViewId
  private updateSelectedViewFromId(): void {
    if (!this.selectedViewId) {
    // Search in both lists
    const myView = this.myViews.find(v => v.id === this.selectedViewId);
    if (myView) {
      this.selectedView = myView.entity;
    const sharedView = this.sharedViews.find(v => v.id === this.selectedViewId);
    if (sharedView) {
      this.selectedView = sharedView.entity;
    // View not in lists yet - it might need to be loaded
   * Get the display name for the current selection
  get displayName(): string {
    if (this.selectedView) {
      return this.selectedView.Name;
    return '(Default)';
   * Toggle dropdown open/closed
  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
   * Close the dropdown
  closeDropdown(): void {
    this.isDropdownOpen = false;
   * Select the default view (no saved view)
  selectDefault(): void {
    this.viewSelected.emit({ viewId: null, view: null });
    this.closeDropdown();
   * Select a view from the list
  selectView(item: ViewListItem): void {
    this.selectedView = item.entity;
    this.viewSelected.emit({ viewId: item.id, view: item.entity });
   * Request to save the current view
  onSaveView(): void {
    this.saveViewRequested.emit({ saveAsNew: false });
   * Request to save as a new view - opens Quick Save dialog in "Save As New" mode
  onSaveAsNewView(): void {
    this.quickSaveRequested.emit(true);
   * Request to manage views
  onManageViews(): void {
    this.manageViewsRequested.emit();
   * Request to open the current view in a tab
  onOpenInTab(): void {
    if (this.selectedViewId) {
      this.openInTabRequested.emit(this.selectedViewId);
   * Request to configure the current view
  onConfigureView(): void {
    this.configureViewRequested.emit();
   * Request to create a new record
  onCreateNewRecord(): void {
    this.createNewRecordRequested.emit();
   * Request to export data to Excel
  onExport(): void {
    this.exportRequested.emit();
   * Request to duplicate a view (F-005)
  onDuplicateView(viewId: string, event: MouseEvent): void {
    event.stopPropagation(); // Don't select the view
    this.duplicateViewRequested.emit(viewId);
   * Request to open the quick save dialog (F-001)
  onQuickSave(): void {
    this.quickSaveRequested.emit(false);
   * Request to revert view to saved state (F-007)
  onRevert(): void {
    this.revertRequested.emit();
   * Check if there are any views to show
  get hasViews(): boolean {
    return this.myViews.length > 0 || this.sharedViews.length > 0;
   * Check if the current user can edit the selected view
  get canEditSelectedView(): boolean {
    return this.selectedView?.UserCanEdit ?? false;
   * Handle click outside to close dropdown
  onClickOutside(event: Event): void {
  // RICH VIEW PANEL: Search & Metadata
   * My views filtered by search text
  get filteredMyViews(): ViewListItem[] {
    if (!this.searchText.trim()) return this.myViews;
    const term = this.searchText.toLowerCase();
    return this.myViews.filter(v =>
      v.name.toLowerCase().includes(term) ||
      (v.entity.Description || '').toLowerCase().includes(term)
   * Shared views filtered by search text
  get filteredSharedViews(): ViewListItem[] {
    if (!this.searchText.trim()) return this.sharedViews;
    return this.sharedViews.filter(v =>
   * Get the number of filters configured in a view by parsing its FilterState
  getViewFilterCount(view: ViewListItem): number {
      const filterState = view.entity.FilterState;
      if (!filterState) return 0;
      const parsed = JSON.parse(filterState);
      if (parsed?.filters?.length) return parsed.filters.length;
      // CompositeFilterDescriptor format
      if (parsed?.logic && parsed?.filters) return parsed.filters.length;
   * Get the number of visible columns in a view by parsing GridState
  getViewColumnCount(view: ViewListItem): number {
      const gridState = view.entity.GridState;
      if (!gridState) return 0;
      const parsed = JSON.parse(gridState);
      if (Array.isArray(parsed)) {
        return parsed.filter((c: Record<string, unknown>) => !c['hidden']).length;
   * Get sort info string from a view's SortState
  getViewSortInfo(view: ViewListItem): string {
      const sortState = view.entity.SortState;
      if (!sortState) return '';
      const parsed = JSON.parse(sortState);
      if (Array.isArray(parsed) && parsed.length > 0) {
        return `${parsed.length} sort${parsed.length > 1 ? 's' : ''}`;
   * Select a specific view and open the config panel for it
  onConfigureViewById(viewId: string, event: MouseEvent): void {
    // First select the view, then open config
    const item = this.myViews.find(v => v.id === viewId) || this.sharedViews.find(v => v.id === viewId);
   * Open a specific view in a new tab
  onOpenViewInTab(viewId: string, event: MouseEvent): void {
    this.openInTabRequested.emit(viewId);
   * Reset to default view (select no view)
  onResetToDefault(): void {
    this.selectDefault();
