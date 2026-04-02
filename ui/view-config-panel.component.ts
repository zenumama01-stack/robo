 * Configuration panel for View parts.
 * Uses tree dropdown for category-based view selection.
@RegisterClass(BaseConfigPanel, 'ViewPanelConfigDialog')
    selector: 'mj-dashboard-view-config-panel',
    templateUrl: './view-config-panel.component.html',
export class ViewConfigPanelComponent extends BaseConfigPanel {
    @ViewChild('viewDropdown') viewDropdown!: TreeDropdownComponent;
    public entityName = '';
    public viewId = '';
    public viewName = '';
    public extraFilter = '';
    public displayMode: 'grid' | 'cards' | 'timeline' = 'grid';
    public allowModeSwitch = true;
    public enableSelection = true;
    public selectionMode: 'none' | 'single' | 'multiple' = 'single';
    private previousViewName = '';
    // Collapsible section states
    public showDisplayOptions = false;
    public viewError = '';
    // Tree configuration for User View Categories (branches) and User Views (leaves)
    public ViewCategoryConfig: TreeBranchConfig = {
        EntityName: 'MJ: User View Categories',
    public ViewLeafConfig: TreeLeafConfig = {
        DefaultIcon: 'fa-solid fa-table',
     * Get the viewId as a CompositeKey for the tree dropdown
    public get ViewIdAsKey(): CompositeKey | null {
        return this.viewId ? CompositeKey.FromID(this.viewId) : null;
        if (config && config.type === 'View') {
            this.entityName = (config['entityName'] as string) || '';
            this.viewId = (config['viewId'] as string) || '';
            this.extraFilter = (config['extraFilter'] as string) || '';
            this.displayMode = (config['displayMode'] as 'grid' | 'cards' | 'timeline') || 'grid';
            this.allowModeSwitch = (config['allowModeSwitch'] as boolean) ?? true;
            this.enableSelection = (config['enableSelection'] as boolean) ?? true;
            this.selectionMode = (config['selectionMode'] as 'none' | 'single' | 'multiple') || 'single';
            // Defaults for new View panel
            this.entityName = '';
            this.viewId = '';
            this.extraFilter = '';
            this.displayMode = 'grid';
            this.allowModeSwitch = true;
            this.enableSelection = true;
            this.selectionMode = 'single';
        this.viewName = '';
        this.previousViewName = '';
        this.viewError = '';
            type: 'View',
            entityName: this.entityName.trim() || undefined,
            viewId: this.viewId.trim() || undefined,
            extraFilter: this.extraFilter.trim() || undefined,
            displayMode: this.displayMode,
            allowModeSwitch: this.allowModeSwitch,
            enableSelection: this.enableSelection,
            selectionMode: this.selectionMode
        // At least entity name or view ID should be provided
        if (!this.entityName.trim() && !this.viewId.trim()) {
            this.viewError = 'Please select a saved view or enter an entity name';
            errors.push(this.viewError);
        if (this.viewName) {
            return this.viewName;
        if (this.entityName) {
            return this.entityName;
        return 'View';
     * Handle view selection from tree dropdown
    public onViewSelection(node: TreeNode | TreeNode[] | null): void {
            // Only accept leaf nodes (actual views, not categories)
                const oldViewName = this.viewName;
                this.viewId = node.ID;
                this.viewName = node.Label;
                // Extract entity name from the view data if available
                if (node.Data && node.Data['Entity']) {
                    this.entityName = String(node.Data['Entity']);
                if (!this.title || this.title === oldViewName || this.title === this.previousViewName) {
                this.previousViewName = node.Label;
    public onEntityChange(): void {
    public onDisplayModeChange(): void {
    public onSelectionModeChange(): void {
    public toggleDisplayOptions(): void {
        this.showDisplayOptions = !this.showDisplayOptions;
    public getDisplayModeDescription(): string {
        switch (this.displayMode) {
            case 'cards':
                return 'Display records as cards in a responsive grid layout';
            case 'timeline':
                return 'Display records chronologically along a timeline';
                return 'Display records in a traditional table/grid format';
import { Component, Input, Output, EventEmitter, OnChanges, OnInit, SimpleChanges, ChangeDetectorRef, HostListener } from '@angular/core';
  ViewColumnInfo,
  ColumnFormat,
  ColumnTextStyle,
  ColumnConditionalRule,
  ViewGridAggregatesConfig,
  DEFAULT_AGGREGATE_DISPLAY,
  CompositeFilterDescriptor,
  FilterFieldInfo,
  FilterFieldType,
  createEmptyFilter
} from '@memberjunction/ng-filter-builder';
 * Column configuration for the view (internal use)
export interface ColumnConfig {
  /** Original display name from entity metadata */
  /** User-defined custom display name (overrides displayName when set) */
  userDisplayName?: string;
  orderIndex: number;
  field: EntityFieldInfo;
  /** Column formatting configuration */
  format?: ColumnFormat;
 * Sort item for multi-column sorting
export interface SortItem {
 * Event emitted when saving the view
export interface ViewSaveEvent {
  columns: ColumnConfig[];
  /** @deprecated Use sortItems instead for multi-sort support */
  sortField: string | null;
  /** Multi-column sort configuration (ordered by priority) */
  sortItems: SortItem[];
  /** Traditional filter state in Kendo-compatible JSON format */
  filterState: CompositeFilterDescriptor | null;
  /** Aggregates configuration */
  aggregatesConfig: ViewGridAggregatesConfig | null;
 * ViewConfigPanelComponent - Sliding panel for configuring view settings
 * - Column visibility and ordering with drag-drop
 * - Column formatting (number, currency, date, etc.)
 * - Sort configuration
 * - View name and description editing
 * - Share settings
 * - Save / Save As New / Cancel actions
  selector: 'mj-view-config-panel',
  styleUrls: ['./view-config-panel.component.css']
export class ViewConfigPanelComponent implements OnInit, OnChanges {
   * The entity being viewed
   * The current view entity (null for default view)
  @Input() viewEntity: UserViewEntityExtended | null = null;
   * Whether the panel is open
   * Current grid state from the grid (includes live column widths/order from user interaction)
   * This takes precedence over viewEntity.Columns for showing current state
  @Input() currentGridState: ViewGridState | null = null;
   * Sample data for column format preview (first few records)
  @Input() sampleData: Record<string, unknown>[] = [];
   * Emitted when the panel should close
   * Emitted when the view should be saved
  @Output() save = new EventEmitter<ViewSaveEvent>();
   * Emitted when default view settings should be saved to user settings
   * (Used for dynamic/default views that persist to MJ: User Settings)
  @Output() saveDefaults = new EventEmitter<ViewSaveEvent>();
   * Emitted when the view should be deleted
  @Output() delete = new EventEmitter<void>();
   * Emitted when filter dialog should be opened (at dashboard level for full width)
  @Output() openFilterDialogRequest = new EventEmitter<{
    filterState: CompositeFilterDescriptor;
    filterFields: FilterFieldInfo[];
   * Filter state from external dialog (set by parent after dialog closes)
  @Input() externalFilterState: CompositeFilterDescriptor | null = null;
   * When true, auto-focus on Settings tab when panel opens (BUG-011: forward saveAsNew intent)
   * Emitted when user wants to duplicate the current view (F-005)
  @Output() duplicate = new EventEmitter<void>();
  public viewName: string = '';
  public viewDescription: string = '';
  public columns: ColumnConfig[] = [];
  /** @deprecated Use sortItems instead */
  public sortField: string | null = null;
  public sortItems: SortItem[] = [];
  // Sort drag state
  public draggedSortItem: SortItem | null = null;
  public dropTargetSortItem: SortItem | null = null;
  public sortDropPosition: 'before' | 'after' | null = null;
  // Available sort directions for dropdown
  public sortDirections = [
    { name: 'Ascending', value: 'asc' as const },
    { name: 'Descending', value: 'desc' as const }
  // Smart Filter state
  public smartFilterEnabled: boolean = false;
  public smartFilterPrompt: string = '';
  public smartFilterExplanation: string = '';
  // Traditional Filter state
  public filterState: CompositeFilterDescriptor = createEmptyFilter();
  public filterFields: FilterFieldInfo[] = [];
  // Filter mode: 'smart' or 'traditional' (mutually exclusive)
  public filterMode: 'smart' | 'traditional' = 'smart';
  // Aggregates state
  public aggregates: ViewGridAggregate[] = [];
  public showAggregateDialog: boolean = false;
  public editingAggregate: ViewGridAggregate | null = null;
  // Saved filter state for mode switching (BUG-006: preserve both modes' data)
  private savedSmartFilterPrompt: string = '';
  private savedTraditionalFilter: CompositeFilterDescriptor = createEmptyFilter();
  // Filter mode switch confirmation (BUG-006)
  public showFilterModeSwitchConfirm: boolean = false;
  public pendingFilterModeSwitch: 'smart' | 'traditional' | null = null;
  // Local saving guard (BUG-003: race condition on double-click)
  private _localSaving: boolean = false;
  public activeTab: 'columns' | 'sorting' | 'filters' | 'aggregates' | 'settings' = 'columns';
  @Input() isSaving: boolean = false;
  public columnSearchText: string = '';
  // Drag state for column reordering
  public draggedColumn: ColumnConfig | null = null;
  public dropTargetColumn: ColumnConfig | null = null;
  public dropPosition: 'before' | 'after' | null = null;
  // Column format editing state
  public formatEditingColumn: ColumnConfig | null = null;
  // Panel resize state
  public isResizing: boolean = false;
  public panelWidth: number = 520;
  private readonly MIN_PANEL_WIDTH = 360;
  private readonly DEFAULT_PANEL_WIDTH = 520;
  /** Width threshold below which tabs show icons only */
  private readonly ICON_ONLY_THRESHOLD = 440;
  private readonly PANEL_WIDTH_SETTING_KEY = 'view-config-panel/width';
   * Whether tabs should show icons only (narrow panel mode)
  get isIconOnlyMode(): boolean {
    return this.panelWidth < this.ICON_ONLY_THRESHOLD;
   * Escape: Close the panel or format sub-panel
    if (this.formatEditingColumn) {
      this.closeFormatEditor();
    } else if (this.isOpen) {
  // PANEL RESIZE HANDLERS
    this.resizeStartWidth = this.panelWidth;
    // Add document-level listeners for mouse move and up
   * Handle resize movement (bound to document)
    // Calculate new width (panel is on the right, so moving left increases width)
    let newWidth = this.resizeStartWidth + deltaX;
    newWidth = Math.max(this.MIN_PANEL_WIDTH, Math.min(this.MAX_PANEL_WIDTH, newWidth));
    this.panelWidth = newWidth;
   * End resizing the panel (bound to document)
    // Persist the panel width to user settings
    this.initializeFromEntity();
          this.panelWidth = width;
      console.warn('[ViewConfigPanel] Failed to load saved panel width:', error);
      await UserInfoEngine.Instance.SetSetting(this.PANEL_WIDTH_SETTING_KEY, String(this.panelWidth));
      console.warn('[ViewConfigPanel] Failed to save panel width:', error);
    // Reset to first tab and clear search when panel opens
      // BUG-011: If DefaultSaveAsNew is set, open on Settings tab with name focused
      this.activeTab = this.DefaultSaveAsNew ? 'settings' : 'columns';
      this.columnSearchText = '';
      this.formatEditingColumn = null;
      this._localSaving = false;
      // Also close any open aggregate dialog
      this.showAggregateDialog = false;
      this.editingAggregate = null;
      // Close filter mode switch confirm
      this.showFilterModeSwitchConfirm = false;
      this.pendingFilterModeSwitch = null;
      // Re-initialize from entity to get fresh state
    // BUG-003: Reset local saving guard when isSaving transitions from true to false
    if (changes['isSaving'] && !this.isSaving && changes['isSaving'].previousValue === true) {
    if (changes['entity'] || changes['viewEntity'] || changes['currentGridState']) {
    // Apply external filter state when it changes (from dashboard-level dialog)
    if (changes['externalFilterState'] && this.externalFilterState) {
      this.filterState = this.externalFilterState;
   * Initialize form state from entity and view
   * Priority for column state: currentGridState > viewEntity.Columns > entity defaults
  private initializeFromEntity(): void {
      this.columns = [];
    // Initialize columns from entity fields (including __mj_ fields for audit/timestamp info)
    this.columns = this.entity.Fields
      .map((field, index) => ({
        fieldId: field.ID,
        fieldName: field.Name,
        displayName: field.DisplayNameOrName,
        userDisplayName: undefined,
        visible: field.DefaultInView,
        width: field.DefaultColumnWidth || null,
        orderIndex: index,
        field,
        format: undefined
    // Priority 1: Use currentGridState if available (reflects live grid state including resizes)
      this.applyGridStateToColumns(this.currentGridState.columnSettings);
      // Also apply sort from currentGridState (supports multi-sort)
      if (this.currentGridState.sortSettings && this.currentGridState.sortSettings.length > 0) {
        this.sortItems = this.currentGridState.sortSettings.map(s => ({
          direction: s.dir
        // Keep legacy fields in sync for backward compatibility
        this.sortField = this.currentGridState.sortSettings[0].field;
        this.sortDirection = this.currentGridState.sortSettings[0].dir;
        this.sortItems = [];
    // Priority 2: If we have a view, apply its column configuration
    else if (this.viewEntity) {
      const viewColumns = this.viewEntity.Columns;
      if (viewColumns && viewColumns.length > 0) {
        // Mark all columns as hidden initially
        this.columns.forEach(c => c.visible = false);
        // Apply view column settings
        viewColumns.forEach((vc, idx) => {
          const column = this.columns.find(c => c.fieldName.toLowerCase() === vc.Name?.toLowerCase());
            column.visible = !vc.hidden;
            column.width = vc.width || null;
            column.orderIndex = vc.orderIndex ?? idx;
            // Apply userDisplayName if present
            if (vc.userDisplayName) {
              column.userDisplayName = vc.userDisplayName;
            // Apply format if present
            if (vc.format) {
              column.format = vc.format;
        // Sort by orderIndex
        this.columns.sort((a, b) => a.orderIndex - b.orderIndex);
      // Apply view's sort configuration (supports multi-sort)
      const sortInfo = this.viewEntity.ViewSortInfo;
      if (sortInfo && sortInfo.length > 0) {
        this.sortItems = sortInfo.map(s => ({
          direction: s.direction === 'Desc' ? 'desc' as const : 'asc' as const
        this.sortField = sortInfo[0].field;
        this.sortDirection = sortInfo[0].direction === 'Desc' ? 'desc' : 'asc';
    // Initialize filter fields from entity
    this.filterFields = this.buildFilterFields();
    // Apply view entity metadata (name, description, etc.) if available
      this.viewName = this.viewEntity.Name;
      this.viewDescription = this.viewEntity.Description || '';
      this.isShared = this.viewEntity.IsShared;
      // Apply view's smart filter configuration
      this.smartFilterEnabled = this.viewEntity.SmartFilterEnabled || false;
      this.smartFilterPrompt = this.viewEntity.SmartFilterPrompt || '';
      this.smartFilterExplanation = this.viewEntity.SmartFilterExplanation || '';
      // Apply view's traditional filter state
      this.filterState = this.parseFilterState(this.viewEntity.FilterState);
      // Set filter mode based on which type of filter is active
      // Smart filter takes precedence if enabled
      if (this.smartFilterEnabled && this.smartFilterPrompt) {
        this.filterMode = 'smart';
      } else if (this.getFilterCount() > 0) {
        this.filterMode = 'traditional';
        // Default to smart mode for new/empty filters (promote AI filtering)
        this.smartFilterEnabled = true;
      // Default view - use entity defaults
      this.viewDescription = '';
      this.isShared = false;
      if (!this.currentGridState?.sortSettings?.length) {
        this.sortField = null;
      this.smartFilterPrompt = '';
      this.smartFilterExplanation = '';
      this.filterState = createEmptyFilter();
      // Default to smart mode (promote AI filtering)
    // Load aggregates from currentGridState if available
    if (this.currentGridState?.aggregates?.expressions) {
      this.aggregates = [...this.currentGridState.aggregates.expressions];
      this.aggregates = [];
   * Build filter fields from entity fields (including __mj_ fields for filtering by timestamps)
  private buildFilterFields(): FilterFieldInfo[] {
    return this.entity.Fields
      .filter(f => !f.IsBinaryFieldType)
        type: this.mapFieldType(field),
        lookupEntityName: field.RelatedEntity || undefined,
        valueList: field.ValueListType === 'List' && field.EntityFieldValues?.length > 0
          ? field.EntityFieldValues.map(v => ({ value: v.Value, label: v.Value }))
   * Map entity field type to filter field type
  private mapFieldType(field: EntityFieldInfo): FilterFieldType {
    // Check for lookup first - RelatedEntity is a string (entity name) if it's a lookup field
      return 'lookup';
    // Map based on SQL type
    const sqlType = field.Type.toLowerCase();
    if (sqlType.includes('bit') || sqlType === 'boolean') {
    if (sqlType.includes('date') || sqlType.includes('time')) {
    if (sqlType.includes('int') || sqlType.includes('decimal') ||
        sqlType.includes('numeric') || sqlType.includes('float') ||
        sqlType.includes('real') || sqlType.includes('money')) {
   * Parse the filter state from JSON string
  private parseFilterState(filterStateJson: string | null | undefined): CompositeFilterDescriptor {
    if (!filterStateJson) {
      return createEmptyFilter();
      const parsed = JSON.parse(filterStateJson);
      // Validate it has the expected structure
      if (parsed && typeof parsed === 'object' && 'logic' in parsed && 'filters' in parsed) {
        return parsed as CompositeFilterDescriptor;
   * Handle filter state change from filter builder
    this.filterState = filter;
   * Open the filter dialog - emits event to parent (dashboard) which renders the dialog at viewport level
  openFilterDialog(): void {
    this.openFilterDialogRequest.emit({
      filterState: this.filterState,
      filterFields: this.filterFields
   * Get the count of active filter rules
    return this.countFilters(this.filterState);
   * Get a human-readable summary of the filter state
  getFilterSummary(): string {
    const count = this.getFilterCount();
    if (count === 0) {
      return 'No filters applied';
    return `${count} filter${count !== 1 ? 's' : ''} active`;
  clearFilters(): void {
   * Apply grid state column settings to the columns array
  private applyGridStateToColumns(gridColumns: ViewGridColumnSetting[]): void {
    // Apply grid state column settings
    gridColumns.forEach((gc, idx) => {
      const column = this.columns.find(c => c.fieldName.toLowerCase() === gc.Name.toLowerCase());
        column.visible = !gc.hidden;
        column.width = gc.width || null;
        column.orderIndex = gc.orderIndex ?? idx;
        if (gc.DisplayName) {
          column.displayName = gc.DisplayName;
        if (gc.userDisplayName) {
          column.userDisplayName = gc.userDisplayName;
        if (gc.format) {
          column.format = gc.format;
   * Get visible columns
  get visibleColumns(): ColumnConfig[] {
    return this.columns.filter(c => c.visible);
   * Get hidden columns
  get hiddenColumns(): ColumnConfig[] {
    return this.columns.filter(c => !c.visible);
   * Get filtered columns for search
  get filteredHiddenColumns(): ColumnConfig[] {
    if (!this.columnSearchText) {
      return this.hiddenColumns;
    const search = this.columnSearchText.toLowerCase();
    return this.hiddenColumns.filter(c =>
      c.displayName.toLowerCase().includes(search) ||
      c.fieldName.toLowerCase().includes(search)
   * Get sortable fields for dropdown (including __mj_ fields for sorting by timestamps)
  get sortableFields(): EntityFieldInfo[] {
    return this.entity.Fields.filter(f =>
      !f.IsBinaryFieldType // Exclude binary fields from sorting
   * Check if the current user can edit the view
  get canEdit(): boolean {
    if (!this.viewEntity) return true; // Can always create new
    return this.viewEntity.UserCanEdit;
   * Check if the current user can delete the view
  get canDelete(): boolean {
    if (!this.viewEntity) return false; // Can't delete default
    return this.viewEntity.UserCanDelete;
   * Toggle column visibility
  toggleColumnVisibility(column: ColumnConfig): void {
    column.visible = !column.visible;
    if (column.visible) {
      // Add to end of visible columns
      column.orderIndex = this.visibleColumns.length;
   * Move column up in order
  moveColumnUp(column: ColumnConfig): void {
    const visibleCols = this.visibleColumns;
    const currentIndex = visibleCols.indexOf(column);
    if (currentIndex > 0) {
      const prevColumn = visibleCols[currentIndex - 1];
      const tempOrder = column.orderIndex;
      column.orderIndex = prevColumn.orderIndex;
      prevColumn.orderIndex = tempOrder;
   * Move column down in order
  moveColumnDown(column: ColumnConfig): void {
    if (currentIndex < visibleCols.length - 1) {
      const nextColumn = visibleCols[currentIndex + 1];
      column.orderIndex = nextColumn.orderIndex;
      nextColumn.orderIndex = tempOrder;
  // DRAG AND DROP WITH DROP INDICATOR
   * Handle drag start for column reordering
  onDragStart(event: DragEvent, column: ColumnConfig): void {
    this.draggedColumn = column;
      event.dataTransfer.setData('text/plain', column.fieldId);
    // Add dragging class to the element
    (event.target as HTMLElement).classList.add('dragging');
   * Handle drag over for column reordering - determines drop position
  onDragOver(event: DragEvent, column: ColumnConfig): void {
    if (!this.draggedColumn || this.draggedColumn === column) {
      this.dropTargetColumn = null;
      this.dropPosition = null;
    // Calculate if we're in the top or bottom half of the target
    const rect = (event.currentTarget as HTMLElement).getBoundingClientRect();
    const y = event.clientY - rect.top;
    const threshold = rect.height / 2;
    this.dropTargetColumn = column;
    this.dropPosition = y < threshold ? 'before' : 'after';
   * Handle drag leave - clear drop indicator
    // Only clear if we're leaving the column item, not entering a child element
    const relatedTarget = event.relatedTarget as HTMLElement;
    const currentTarget = event.currentTarget as HTMLElement;
    if (!currentTarget.contains(relatedTarget)) {
   * Handle drop for column reordering
  onDrop(event: DragEvent, targetColumn: ColumnConfig): void {
    if (this.draggedColumn && this.draggedColumn !== targetColumn && this.dropPosition) {
      const draggedIndex = visibleCols.indexOf(this.draggedColumn);
      let targetIndex = visibleCols.indexOf(targetColumn);
      // Adjust target index based on drop position
      if (this.dropPosition === 'after') {
        targetIndex++;
      // If dragging from before target, adjust for removal
      if (draggedIndex < targetIndex) {
        targetIndex--;
      // Reorder the columns
      this.reorderColumn(this.draggedColumn, targetIndex);
    this.clearDragState();
    (event.target as HTMLElement).classList.remove('dragging');
   * Clear all drag state
  private clearDragState(): void {
    this.draggedColumn = null;
   * Reorder a column to a new position
  private reorderColumn(column: ColumnConfig, newIndex: number): void {
    // Remove from current position
    if (currentIndex === newIndex) return;
    // Update order indices
    visibleCols.forEach((col, idx) => {
      if (col === column) {
        col.orderIndex = newIndex;
      } else if (currentIndex < newIndex) {
        // Dragging down - shift items between old and new position up
        if (idx > currentIndex && idx <= newIndex) {
          col.orderIndex = idx - 1;
        // Dragging up - shift items between new and old position down
        if (idx >= newIndex && idx < currentIndex) {
          col.orderIndex = idx + 1;
    // Re-sort all columns by orderIndex
   * Check if drop indicator should show before a column
  isDropBefore(column: ColumnConfig): boolean {
    return this.dropTargetColumn === column && this.dropPosition === 'before';
   * Check if drop indicator should show after a column
  isDropAfter(column: ColumnConfig): boolean {
    return this.dropTargetColumn === column && this.dropPosition === 'after';
  // MULTI-SORT MANAGEMENT
   * Add a new sort level
  addSortLevel(): void {
    // Find the first sortable field not already in use
    const usedFields = new Set(this.sortItems.map(s => s.field));
    const availableField = this.sortableFields.find(f => !usedFields.has(f.Name));
    if (availableField) {
      this.sortItems.push({
        field: availableField.Name,
        direction: 'asc'
      this.syncLegacySortFields();
   * Remove a sort level
  removeSortLevel(sortItem: SortItem): void {
    const index = this.sortItems.indexOf(sortItem);
      this.sortItems.splice(index, 1);
   * Get the display name for a field
  getFieldDisplayName(fieldName: string): string {
    const field = this.sortableFields.find(f => f.Name === fieldName);
    return field?.DisplayNameOrName || fieldName;
   * Get fields available for a sort item (excludes already selected fields except current)
  getAvailableFieldsForSort(currentSortItem: SortItem): EntityFieldInfo[] {
    return this.sortableFields.filter(f =>
      f.Name === currentSortItem.field || !usedFields.has(f.Name)
   * Handle sort field change
  onSortFieldChange(sortItem: SortItem, fieldName: string): void {
    sortItem.field = fieldName;
   * Handle sort direction change
  onSortDirectionChange(sortItem: SortItem, direction: 'asc' | 'desc'): void {
    sortItem.direction = direction;
   * Keep legacy sortField/sortDirection in sync with sortItems[0]
  private syncLegacySortFields(): void {
    if (this.sortItems.length > 0) {
      this.sortField = this.sortItems[0].field;
      this.sortDirection = this.sortItems[0].direction;
  // ----------------------------------------
  // Sort Drag & Drop
   * Handle drag start for sort item reordering
  onSortDragStart(event: DragEvent, sortItem: SortItem): void {
    this.draggedSortItem = sortItem;
      event.dataTransfer.setData('text/plain', sortItem.field);
   * Handle drag over for sort item reordering
  onSortDragOver(event: DragEvent, sortItem: SortItem): void {
    if (!this.draggedSortItem || this.draggedSortItem === sortItem) {
      this.dropTargetSortItem = null;
      this.sortDropPosition = null;
    this.dropTargetSortItem = sortItem;
    this.sortDropPosition = y < threshold ? 'before' : 'after';
   * Handle drag leave for sort item
  onSortDragLeave(event: DragEvent): void {
   * Handle drop for sort item reordering
  onSortDrop(event: DragEvent, targetSortItem: SortItem): void {
    if (this.draggedSortItem && this.draggedSortItem !== targetSortItem && this.sortDropPosition) {
      const draggedIndex = this.sortItems.indexOf(this.draggedSortItem);
      let targetIndex = this.sortItems.indexOf(targetSortItem);
      if (this.sortDropPosition === 'after') {
      // Remove from old position
      this.sortItems.splice(draggedIndex, 1);
      this.sortItems.splice(targetIndex, 0, this.draggedSortItem);
    this.clearSortDragState();
   * Handle drag end for sort item
  onSortDragEnd(event: DragEvent): void {
   * Clear sort drag state
  private clearSortDragState(): void {
    this.draggedSortItem = null;
   * Check if drop indicator should show before a sort item
  isSortDropBefore(sortItem: SortItem): boolean {
    return this.dropTargetSortItem === sortItem && this.sortDropPosition === 'before';
   * Check if drop indicator should show after a sort item
  isSortDropAfter(sortItem: SortItem): boolean {
    return this.dropTargetSortItem === sortItem && this.sortDropPosition === 'after';
  // COLUMN FORMAT EDITOR
   * Open the format editor for a column
  openFormatEditor(column: ColumnConfig): void {
    // Initialize format if not present
    if (!column.format) {
      column.format = this.getDefaultFormat(column.field);
    this.formatEditingColumn = column;
   * Close the format editor
  closeFormatEditor(): void {
   * Get default format based on field type
  private getDefaultFormat(field: EntityFieldInfo): ColumnFormat {
    if (sqlType.includes('money') || sqlType.includes('currency')) {
      return { type: 'currency', decimals: 2, currencyCode: 'USD', thousandsSeparator: true };
    if (sqlType.includes('percent')) {
      return { type: 'percent', decimals: 1 };
    if (sqlType.includes('decimal') || sqlType.includes('numeric') || sqlType.includes('float') || sqlType.includes('real')) {
      return { type: 'number', decimals: 2, thousandsSeparator: true };
    if (sqlType.includes('int')) {
      return { type: 'number', decimals: 0, thousandsSeparator: true };
    if (sqlType.includes('datetime')) {
      return { type: 'datetime', dateFormat: 'medium' };
    if (sqlType.includes('date')) {
      return { type: 'date', dateFormat: 'medium' };
      return { type: 'boolean', trueLabel: 'Yes', falseLabel: 'No', booleanDisplay: 'text' };
    return { type: 'auto' };
   * Check if a column has custom formatting applied
  hasCustomFormat(column: ColumnConfig): boolean {
    return !!column.format && column.format.type !== 'auto';
   * Clear formatting for a column
  clearColumnFormat(column: ColumnConfig): void {
    column.format = undefined;
   * Get sample values for preview
  getSampleValues(column: ColumnConfig): unknown[] {
    if (!this.sampleData || this.sampleData.length === 0) {
      return this.getPlaceholderSamples(column.field);
    return this.sampleData
      .map(row => row[column.fieldName])
      .filter(v => v != null);
   * Get placeholder sample values when no data available
  private getPlaceholderSamples(field: EntityFieldInfo): unknown[] {
    if (sqlType.includes('money') || sqlType.includes('decimal') || sqlType.includes('numeric')) {
      return [1234.56, -567.89, 10000.00, 0.50, 999999.99];
      return [42, 100, 1500, 0, -25];
        new Date(now.getTime() - 86400000),
        new Date(now.getTime() - 172800000),
        now,
        new Date(now.getTime() + 86400000),
        new Date(now.getTime() - 604800000)
      return [true, false, true, false, true];
    return ['Sample', 'Text', 'Values', 'Here', 'Preview'];
   * Format a value for preview display
  formatPreviewValue(value: unknown, format: ColumnFormat | undefined): string {
    if (!format || format.type === 'auto') return String(value);
        return this.formatNumber(value as number, format);
        return this.formatCurrency(value as number, format);
        return this.formatPercent(value as number, format);
        return this.formatDate(value as Date, format);
        return this.formatBoolean(value as boolean, format);
  private formatNumber(value: number, format: ColumnFormat): string {
  private formatCurrency(value: number, format: ColumnFormat): string {
  private formatPercent(value: number, format: ColumnFormat): string {
    return new Intl.NumberFormat('en-US', options).format(value / 100);
  private formatDate(value: Date, format: ColumnFormat): string {
    const date = value instanceof Date ? value : new Date(value);
  private formatBoolean(value: boolean, format: ColumnFormat): string {
  // CLOSE / SAVE / DELETE
   * Check if filter state has any active filters
  private hasActiveFilters(): boolean {
    return this.filterState?.filters?.length > 0;
  // VALIDATION (BUG-004)
   * Get validation errors for the current form state
  get ValidationErrors(): string[] {
    if (!this.viewName || !this.viewName.trim()) {
      errors.push('View name is required');
    if (this.visibleColumns.length === 0) {
      errors.push('At least one column must be visible');
   * Whether the form is valid for saving
    return this.ValidationErrors.length === 0;
   * Whether the form is valid for save-as-new (name can default to 'New View')
  get IsValidForSaveAsNew(): boolean {
    return this.visibleColumns.length > 0;
   * Build a ViewConfigSummary for quick-save preview (F-003)
  BuildSummary(): ViewConfigSummary {
      ColumnCount: this.visibleColumns.length,
      FilterCount: this.filterMode === 'traditional' ? this.getFilterCount() : 0,
      SortCount: this.sortItems.length,
      SmartFilterActive: this.smartFilterEnabled && !!this.smartFilterPrompt.trim(),
      SmartFilterPrompt: this.smartFilterPrompt,
      AggregateCount: this.aggregates.filter(a => a.enabled !== false).length
   * Save the view
    // BUG-003: Guard against double-clicks with local flag
    if (this.isSaving || this._localSaving) return;
    // BUG-004: Validate before saving
    this._localSaving = true;
    this.save.emit({
      name: this.viewName,
      description: this.viewDescription,
      isShared: this.isShared,
      saveAsNew: false,
      columns: this.visibleColumns,
      sortField: this.sortField,
      sortItems: [...this.sortItems],
      smartFilterEnabled: this.smartFilterEnabled,
      smartFilterPrompt: this.smartFilterPrompt,
      filterState: this.hasActiveFilters() ? this.filterState : null,
      aggregatesConfig: this.buildAggregatesConfig()
   * Save as a new view
  onSaveAsNew(): void {
    // BUG-004: Validate (name defaults to 'New View' if empty)
    if (!this.IsValidForSaveAsNew) return;
      name: this.viewName || 'New View',
      saveAsNew: true,
   * Save default view settings to user settings
   * Used for dynamic/default views that don't have a stored view entity
  onSaveDefaults(): void {
    this.saveDefaults.emit({
      name: 'Default',
      isShared: false,
  public showDeleteConfirm: boolean = false;
   * Delete the view - shows confirmation dialog
  onDelete(): void {
   * Confirmed delete from dialog
  OnDeleteConfirmed(): void {
    this.delete.emit();
   * Cancelled delete from dialog
  OnDeleteCancelled(): void {
   * Duplicate the current view (F-005)
    this.duplicate.emit();
   * Set the active tab
  setActiveTab(tab: 'columns' | 'sorting' | 'filters' | 'aggregates' | 'settings'): void {
    this.formatEditingColumn = null; // Close format editor when switching tabs
   * Set the filter mode (smart or traditional)
   * BUG-006: Shows confirmation when switching if active mode has data
  setFilterMode(mode: 'smart' | 'traditional'): void {
    if (this.filterMode === mode) return;
    // Check if current mode has data that would be lost
    const currentModeHasData = this.currentFilterModeHasData();
    if (currentModeHasData) {
      // Show confirmation dialog before switching
      this.pendingFilterModeSwitch = mode;
      this.showFilterModeSwitchConfirm = true;
    this.applyFilterModeSwitch(mode);
   * Check if the current filter mode has user-entered data
  private currentFilterModeHasData(): boolean {
    if (this.filterMode === 'smart') {
      return !!this.smartFilterPrompt.trim();
      return this.getFilterCount() > 0;
   * Confirm the filter mode switch (called from ConfirmDialog)
  OnFilterModeSwitchConfirmed(): void {
    if (this.pendingFilterModeSwitch) {
      this.applyFilterModeSwitch(this.pendingFilterModeSwitch);
   * Cancel the filter mode switch (called from ConfirmDialog)
  OnFilterModeSwitchCancelled(): void {
   * Apply the filter mode switch - saves current mode data and restores target mode data
  private applyFilterModeSwitch(mode: 'smart' | 'traditional'): void {
    // Save current mode's data before switching
      this.savedSmartFilterPrompt = this.smartFilterPrompt;
      this.savedTraditionalFilter = this.filterState;
    this.filterMode = mode;
    // Restore target mode's saved data or clear
    if (mode === 'smart') {
      this.smartFilterPrompt = this.savedSmartFilterPrompt;
      this.smartFilterEnabled = false;
      this.filterState = this.savedTraditionalFilter;
   * Apply a smart filter example to the prompt field
  applySmartFilterExample(example: string): void {
    this.smartFilterPrompt = example;
  // STYLE UPDATE HELPERS
   * Toggle a header style property
  toggleHeaderStyle(prop: keyof ColumnTextStyle): void {
    if (!this.formatEditingColumn?.format) return;
    const format = this.formatEditingColumn.format;
    if (!format.headerStyle) {
      format.headerStyle = {};
    if (prop === 'bold' || prop === 'italic' || prop === 'underline') {
      format.headerStyle[prop] = !format.headerStyle[prop];
   * Update the user-defined display name for a column
  updateUserDisplayName(value: string): void {
    if (!this.formatEditingColumn) return;
    // Set to undefined if empty string, otherwise use the value
    this.formatEditingColumn.userDisplayName = value.trim() || undefined;
   * Update a header style color property
  updateHeaderColor(prop: 'color' | 'backgroundColor', value: string): void {
    format.headerStyle[prop] = value;
   * Toggle a cell style property
  toggleCellStyle(prop: keyof ColumnTextStyle): void {
    if (!format.cellStyle) {
      format.cellStyle = {};
      format.cellStyle[prop] = !format.cellStyle[prop];
   * Update a cell style color property
  updateCellColor(prop: 'color' | 'backgroundColor', value: string): void {
    format.cellStyle[prop] = value;
  // AGGREGATE MANAGEMENT
   * Open dialog to add a new aggregate
  openAddAggregateDialog(): void {
    this.showAggregateDialog = true;
   * Open dialog to edit an existing aggregate
  editAggregate(aggregate: ViewGridAggregate): void {
    this.editingAggregate = { ...aggregate };
   * Close the aggregate dialog
  closeAggregateDialog(): void {
   * Handle saving an aggregate from the dialog
  onAggregateSave(aggregate: ViewGridAggregate): void {
    const existingIndex = this.aggregates.findIndex(a => a.id === aggregate.id);
      this.aggregates[existingIndex] = aggregate;
      // Add new with order at end
      aggregate.order = this.aggregates.length;
      this.aggregates.push(aggregate);
    this.closeAggregateDialog();
   * Remove an aggregate
  removeAggregate(aggregate: ViewGridAggregate): void {
    const index = this.aggregates.findIndex(a => a.id === aggregate.id);
      this.aggregates.splice(index, 1);
      // Re-order remaining aggregates
      this.aggregates.forEach((a, i) => a.order = i);
   * Toggle aggregate enabled state (BUG-012: immutable update, no excessive logging)
  toggleAggregateEnabled(aggregate: ViewGridAggregate, event?: MouseEvent): void {
    // Stop event propagation to prevent any parent handlers
    // Find by ID first, fall back to object reference or label
    let index = -1;
    if (aggregate.id) {
      index = this.aggregates.findIndex(a => a.id === aggregate.id);
      index = this.aggregates.indexOf(aggregate);
    if (index < 0 && aggregate.label) {
      index = this.aggregates.findIndex(a => a.label === aggregate.label && a.expression === aggregate.expression);
      const updatedAggregate: ViewGridAggregate = {
        ...this.aggregates[index],
        enabled: this.aggregates[index].enabled === false
      // Replace entire array to trigger change detection
      const newAggregates = [...this.aggregates];
      newAggregates[index] = updatedAggregate;
      this.aggregates = newAggregates;
   * Move aggregate up in order
  moveAggregateUp(aggregate: ViewGridAggregate): void {
    const index = this.aggregates.indexOf(aggregate);
      const prev = this.aggregates[index - 1];
      this.aggregates[index - 1] = aggregate;
      this.aggregates[index] = prev;
   * Move aggregate down in order
  moveAggregateDown(aggregate: ViewGridAggregate): void {
    if (index < this.aggregates.length - 1) {
      const next = this.aggregates[index + 1];
      this.aggregates[index + 1] = aggregate;
      this.aggregates[index] = next;
   * Get enabled aggregates count
  get enabledAggregatesCount(): number {
    return this.aggregates.filter(a => a.enabled !== false).length;
   * Get card aggregates
  get cardAggregates(): ViewGridAggregate[] {
    return this.aggregates.filter(a => a.displayType === 'card');
   * Get column aggregates
  get columnAggregates(): ViewGridAggregate[] {
    return this.aggregates.filter(a => a.displayType === 'column');
   * Build aggregates config from current state
  private buildAggregatesConfig(): ViewGridAggregatesConfig | null {
    if (this.aggregates.length === 0) return null;
      display: { ...DEFAULT_AGGREGATE_DISPLAY },
      expressions: [...this.aggregates]
