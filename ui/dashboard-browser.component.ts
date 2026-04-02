import { MJDashboardEntity, MJDashboardCategoryEntity, DashboardUserPermissions } from '@memberjunction/core-entities';
// Event Types
 * View mode for the dashboard browser
export type DashboardBrowserViewMode = 'cards' | 'list';
 * Event emitted when a dashboard is selected for viewing
export interface DashboardOpenEvent {
    Dashboard: MJDashboardEntity;
    OpenInNewTab: boolean;
 * Event emitted when a dashboard is selected for editing
export interface DashboardEditEvent {
 * Event emitted when dashboards are requested for deletion
export interface DashboardDeleteEvent {
    Dashboards: MJDashboardEntity[];
 * Event emitted when dashboards are requested to be moved to a folder
export interface DashboardMoveEvent {
 * Event emitted when the category filter changes
export interface CategoryChangeEvent {
 * Event emitted when view mode changes
export interface ViewModeChangeEvent {
    Mode: DashboardBrowserViewMode;
 * Event emitted when a new dashboard should be created
export interface DashboardCreateEvent {
 * Event emitted when a new category should be created
export interface CategoryCreateEvent {
    ParentCategoryId: string | null;
 * Event emitted when a category should be deleted
export interface CategoryDeleteEvent {
    Category: MJDashboardCategoryEntity;
    IncludeContents: boolean;
 * Event emitted when view preference should be persisted
export interface ViewPreferenceChangeEvent {
    ViewMode: DashboardBrowserViewMode;
 * Generic dashboard browser component.
 * Displays dashboards in card or list view with multi-select, filtering, and bulk actions.
 * This component is GENERIC and has no routing dependencies.
 * All navigation and persistence events are bubbled up for the parent to handle.
    selector: 'mj-dashboard-browser',
    templateUrl: './dashboard-browser.component.html',
    styleUrls: ['./dashboard-browser.component.css'],
export class DashboardBrowserComponent implements OnInit, OnDestroy {
    /** All dashboards to display */
    private _dashboards: MJDashboardEntity[] = [];
    set Dashboards(value: MJDashboardEntity[]) {
        this._dashboards = value || [];
    get Dashboards(): MJDashboardEntity[] {
        return this._dashboards;
    /** All categories for filtering */
        // Update child categories when categories change (async loading)
        this.updateChildCategories();
    /** Currently selected category ID (for deep linking) */
    private _selectedCategoryId: string | null = null;
    set SelectedCategoryId(value: string | null) {
        if (value !== this._selectedCategoryId) {
            this._selectedCategoryId = value;
    get SelectedCategoryId(): string | null {
        return this._selectedCategoryId;
    /** Initial view mode */
    private _viewMode: DashboardBrowserViewMode = 'cards';
    set ViewMode(value: DashboardBrowserViewMode) {
        if (value !== this._viewMode) {
            this._viewMode = value;
    get ViewMode(): DashboardBrowserViewMode {
        return this._viewMode;
    /** Whether the browser is in loading state */
    @Input() IsLoading = false;
    /** Whether to show the create button */
    @Input() ShowCreateButton = true;
    /** Whether to allow multi-select (enables selection mode toggle) */
    @Input() AllowMultiSelect = true;
    /** Whether to allow drag and drop */
    /** Whether currently in selection mode (checkboxes visible) */
    public IsSelectionMode = false;
    /** Title to display in the header */
    @Input() Title = 'Dashboards';
    /** Icon class for the header */
    @Input() IconClass = 'fa-solid fa-gauge-high';
     * Map of dashboard ID to user permissions.
     * Used to show shared indicators and control edit/delete button visibility.
    private _dashboardPermissions: Map<string, DashboardUserPermissions> = new Map();
    set DashboardPermissions(value: Map<string, DashboardUserPermissions> | null) {
        this._dashboardPermissions = value || new Map();
    get DashboardPermissions(): Map<string, DashboardUserPermissions> {
        return this._dashboardPermissions;
     * Map of dashboard ID to effective category ID for display.
     * Used to show shared dashboards in the user's chosen category instead of owner's category.
     * If a dashboard ID is not in this map, its actual CategoryID is used.
     * Use empty string '' to indicate root/uncategorized.
    private _effectiveCategoryMap: Map<string, string | null> = new Map();
    set EffectiveCategoryMap(value: Map<string, string | null> | null) {
        this._effectiveCategoryMap = value || new Map();
    get EffectiveCategoryMap(): Map<string, string | null> {
        return this._effectiveCategoryMap;
    /** Emitted when a dashboard is opened for viewing */
    @Output() DashboardOpen = new EventEmitter<DashboardOpenEvent>();
    /** Emitted when a dashboard is opened for editing */
    @Output() DashboardEdit = new EventEmitter<DashboardEditEvent>();
    /** Emitted when dashboards are requested for deletion */
    @Output() DashboardDelete = new EventEmitter<DashboardDeleteEvent>();
    /** Emitted when dashboards are requested to move to a folder */
    @Output() DashboardMove = new EventEmitter<DashboardMoveEvent>();
    /** Emitted when a new dashboard should be created */
    @Output() DashboardCreate = new EventEmitter<DashboardCreateEvent>();
    /** Emitted when a new category should be created */
    @Output() CategoryCreate = new EventEmitter<CategoryCreateEvent>();
    /** Emitted when a category should be deleted */
    @Output() CategoryDelete = new EventEmitter<CategoryDeleteEvent>();
    /** Emitted when the category filter changes */
    @Output() CategoryChange = new EventEmitter<CategoryChangeEvent>();
    /** Emitted when view mode changes (for persistence) */
    @Output() ViewModeChange = new EventEmitter<ViewModeChangeEvent>();
    /** Emitted when view preference should be persisted */
    @Output() ViewPreferenceChange = new EventEmitter<ViewPreferenceChangeEvent>();
    /** Filtered dashboards based on search and category */
    public FilteredDashboards: MJDashboardEntity[] = [];
    /** Current search text */
    /** Set of selected dashboard IDs */
    public SelectedIds = new Set<string>();
    /** Last clicked dashboard ID (for shift-click range selection) */
    private lastClickedId: string | null = null;
    /** Whether delete confirmation dialog is visible */
    /** Whether move-to-folder dialog is visible */
    public ShowMoveDialog = false;
    /** Dashboards pending deletion (for confirm dialog) */
    public DashboardsPendingDelete: MJDashboardEntity[] = [];
    /** Currently dragging dashboard ID */
    public DraggingId: string | null = null;
    /** Drop target category ID */
    public DropTargetCategoryId: string | null = null;
    /** Whether the "New" dropdown menu is open */
    public ShowNewMenu = false;
    /** Whether the create category dialog is visible */
    public ShowCreateCategoryDialog = false;
    /** New category form values */
    public NewCategoryName = '';
    public NewCategoryDescription = '';
    /** Child categories of the current folder */
    public ChildCategories: MJDashboardCategoryEntity[] = [];
    /** Filtered child categories (based on search) */
    public FilteredChildCategories: MJDashboardCategoryEntity[] = [];
    /** Whether the delete category dialog is visible */
    public ShowDeleteCategoryConfirm = false;
    /** Category pending deletion */
    public CategoryPendingDelete: MJDashboardCategoryEntity | null = null;
    /** Whether to include contents when deleting category */
    public DeleteCategoryIncludeContents = false;
    /** Drop target category ID for drag-over highlighting on category cards */
    public DragOverChildCategoryId: string | null = null;
    private readonly destroy$ = new Subject<void>();
    // Public Methods - View Mode
     * Toggle between card and list view
        this.ViewMode = this.ViewMode === 'cards' ? 'list' : 'cards';
        this.ViewModeChange.emit({ Mode: this.ViewMode });
        this.ViewPreferenceChange.emit({ ViewMode: this.ViewMode });
    public SetViewMode(mode: DashboardBrowserViewMode): void {
        if (mode !== this.ViewMode) {
            this.ViewModeChange.emit({ Mode: mode });
            this.ViewPreferenceChange.emit({ ViewMode: mode });
    // Public Methods - Filtering
     * Handle search text change
    public OnSearchChange(): void {
     * Clear search text
    public ClearSearch(): void {
     * Handle category filter change
    public OnCategoryChange(categoryId: string | null): void {
        this._selectedCategoryId = categoryId;
        this.CategoryChange.emit({ CategoryId: categoryId, Category: category });
    public ClearFilters(): void {
        this._selectedCategoryId = null;
        this.CategoryChange.emit({ CategoryId: null, Category: null });
     * Navigate into a category folder
    public NavigateToCategory(categoryId: string | null): void {
     * Navigate up to parent category
    public NavigateUp(): void {
        if (!this._selectedCategoryId) return;
        const currentCategory = this.Categories.find(c => c.ID === this._selectedCategoryId);
        const parentId = currentCategory?.ParentID || null;
        this.NavigateToCategory(parentId);
        return !this._selectedCategoryId;
     * Get the current category (for display)
        if (!this._selectedCategoryId) return null;
        return this.Categories.find(c => c.ID === this._selectedCategoryId) || null;
    // Public Methods - Selection
     * Handle dashboard click with multi-select support
    public OnDashboardClick(dashboard: MJDashboardEntity, event: MouseEvent): void {
        if (!this.AllowMultiSelect) {
            // Single select mode - just open the dashboard
            this.DashboardOpen.emit({ Dashboard: dashboard, OpenInNewTab: event.ctrlKey || event.metaKey });
        if (event.shiftKey && this.lastClickedId) {
            // Shift-click: range selection
            this.selectRange(this.lastClickedId, dashboard.ID);
        } else if (event.ctrlKey || event.metaKey) {
            // Ctrl/Cmd-click: toggle selection
            this.ToggleSelection(dashboard.ID);
            // Normal click: if not selected, open; if selected with others, open
            if (this.SelectedIds.size <= 1) {
                this.SelectedIds.clear();
                this.DashboardOpen.emit({ Dashboard: dashboard, OpenInNewTab: false });
            } else if (this.SelectedIds.has(dashboard.ID)) {
                // Clicking on one of multiple selected items - open just this one
                // Clicking on unselected item - clear selection and open
        this.lastClickedId = dashboard.ID;
     * Handle double-click to edit
    public OnDashboardDoubleClick(dashboard: MJDashboardEntity, event: MouseEvent): void {
        this.DashboardEdit.emit({ Dashboard: dashboard });
     * Check if a dashboard is selected
    public IsSelected(dashboardId: string): boolean {
        return this.SelectedIds.has(dashboardId);
     * Select all visible dashboards
    public SelectAll(): void {
        for (const dashboard of this.FilteredDashboards) {
            this.SelectedIds.add(dashboard.ID);
    public ClearSelection(): void {
        this.lastClickedId = null;
     * Enter selection mode (show checkboxes)
    public EnterSelectionMode(): void {
        if (!this.AllowMultiSelect) return;
        this.IsSelectionMode = true;
     * Exit selection mode (hide checkboxes and clear selections)
    public ExitSelectionMode(): void {
        this.IsSelectionMode = false;
     * Toggle selection mode
    public ToggleSelectionMode(): void {
        if (this.IsSelectionMode) {
            this.ExitSelectionMode();
            this.EnterSelectionMode();
     * Get count of selected dashboards
    public get SelectedCount(): number {
        return this.SelectedIds.size;
     * Get selected dashboards
    public GetSelectedDashboards(): MJDashboardEntity[] {
        return this.Dashboards.filter(d => this.SelectedIds.has(d.ID));
    // Public Methods - Actions
     * Toggle the "New" dropdown menu
    public ToggleNewMenu(): void {
        this.ShowNewMenu = !this.ShowNewMenu;
     * Close the "New" dropdown menu
    public CloseNewMenu(): void {
        this.ShowNewMenu = false;
     * Request to create a new dashboard
    public OnCreateDashboard(): void {
        this.CloseNewMenu();
        this.DashboardCreate.emit({ CategoryId: this.SelectedCategoryId });
     * Open the create category dialog
    public OpenCreateCategoryDialog(): void {
        this.NewCategoryName = '';
        this.NewCategoryDescription = '';
        this.ShowCreateCategoryDialog = true;
     * Close the create category dialog
    public CloseCreateCategoryDialog(): void {
        this.ShowCreateCategoryDialog = false;
     * Confirm category creation
    public ConfirmCreateCategory(): void {
        if (!this.NewCategoryName.trim()) return;
        this.CategoryCreate.emit({
            ParentCategoryId: this.SelectedCategoryId,
            Name: this.NewCategoryName.trim(),
            Description: this.NewCategoryDescription.trim() || null
        this.CloseCreateCategoryDialog();
     * Open edit dialog for single dashboard via context menu
    public OnEditDashboard(dashboard: MJDashboardEntity, event: Event): void {
     * Request to delete a single dashboard
    public OnDeleteDashboard(dashboard: MJDashboardEntity, event: Event): void {
        this.DashboardsPendingDelete = [dashboard];
        this.ShowDeleteConfirm = true;
     * Request to delete selected dashboards.
     * Only includes dashboards the user has permission to delete.
    public OnDeleteSelected(): void {
        if (this.SelectedIds.size === 0) return;
        // Only include dashboards the user can delete
        this.DashboardsPendingDelete = this.GetDeletableSelectedDashboards();
        if (this.DashboardsPendingDelete.length === 0) {
            // No deletable dashboards selected
     * Confirm deletion
    public ConfirmDelete(): void {
        if (this.DashboardsPendingDelete.length > 0) {
            this.DashboardDelete.emit({ Dashboards: this.DashboardsPendingDelete });
            // Clear selection for deleted items
            for (const d of this.DashboardsPendingDelete) {
                this.SelectedIds.delete(d.ID);
        this.CloseDeleteConfirm();
        // Exit selection mode after bulk operation
     * Cancel deletion
    public CloseDeleteConfirm(): void {
        this.DashboardsPendingDelete = [];
     * Open move-to-folder dialog for selected dashboards
    public OnMoveSelected(): void {
        this.ShowMoveDialog = true;
     * Confirm move to folder
    public ConfirmMove(targetCategoryId: string | null): void {
        const dashboards = this.GetSelectedDashboards();
        if (dashboards.length > 0) {
            this.DashboardMove.emit({
                Dashboards: dashboards,
                TargetCategoryId: targetCategoryId
        this.CloseMoveDialog();
     * Cancel move
    public CloseMoveDialog(): void {
        this.ShowMoveDialog = false;
    // Public Methods - Category Actions
     * Request to delete a category
    public OnDeleteCategory(category: MJDashboardCategoryEntity, event: Event): void {
        this.CategoryPendingDelete = category;
        this.DeleteCategoryIncludeContents = false;
        this.ShowDeleteCategoryConfirm = true;
     * Confirm category deletion
    public ConfirmDeleteCategory(): void {
        if (this.CategoryPendingDelete) {
            this.CategoryDelete.emit({
                Category: this.CategoryPendingDelete,
                IncludeContents: this.DeleteCategoryIncludeContents
        this.CloseDeleteCategoryConfirm();
     * Cancel category deletion
    public CloseDeleteCategoryConfirm(): void {
        this.ShowDeleteCategoryConfirm = false;
        this.CategoryPendingDelete = null;
     * Get count of dashboards and sub-categories in a category
    public GetCategoryContentCount(category: MJDashboardCategoryEntity): { dashboards: number; categories: number } {
        const dashboards = this.Dashboards.filter(d => d.CategoryID === category.ID).length;
        const categories = this.Categories.filter(c => c.ParentID === category.ID).length;
        return { dashboards, categories };
    // Public Methods - Drag and Drop
     * Handle drag start
    public OnDragStart(dashboard: MJDashboardEntity, event: DragEvent): void {
        this.DraggingId = dashboard.ID;
        // If dragging an unselected item, select only it
        if (!this.SelectedIds.has(dashboard.ID)) {
        // Set drag data
        const dragData = {
            type: 'dashboards',
            ids: Array.from(this.SelectedIds)
        event.dataTransfer?.setData('application/json', JSON.stringify(dragData));
        // Create custom drag image for list view (smaller than full row)
        if (this._viewMode === 'list' && event.dataTransfer) {
            const dragPreview = this.createDragPreview(dashboard);
            document.body.appendChild(dragPreview);
            event.dataTransfer.setDragImage(dragPreview, 12, 12);
            // Clean up the preview element after drag starts
            setTimeout(() => dragPreview.remove(), 0);
     * Create a compact drag preview element
    private createDragPreview(dashboard: MJDashboardEntity): HTMLElement {
        const count = this.SelectedIds.size;
        const preview = document.createElement('div');
        preview.style.cssText = `
            left: -9999px;
            top: -9999px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.3);
        icon.className = 'fa-solid fa-chart-line';
        preview.appendChild(icon);
        const text = document.createElement('span');
        text.textContent = count > 1 ? `${count} dashboards` : dashboard.Name;
        preview.appendChild(text);
     * Handle drag end
    public OnDragEnd(): void {
        this.DraggingId = null;
        this.DropTargetCategoryId = null;
     * Handle drag over category
    public OnDragOverCategory(categoryId: string | null, event: DragEvent): void {
        this.DropTargetCategoryId = categoryId;
     * Handle drag leave category
    public OnDragLeaveCategory(): void {
     * Handle drop on category
    public OnDropOnCategory(categoryId: string | null, event: DragEvent): void {
                    const dashboards = this.Dashboards.filter(d => dragData.ids.includes(d.ID));
                            TargetCategoryId: categoryId
    // Public Methods - Helpers
     * Get category name for a dashboard
    public GetCategoryName(categoryId: string | null): string {
        if (!categoryId) return 'Uncategorized';
        const category = this.Categories.find(c => c.ID === categoryId);
    public FormatDate(date: Date): string {
            return `${diffDays} days ago`;
    public TrackByDashboard(_index: number, dashboard: MJDashboardEntity): string {
        return dashboard.ID;
     * Track by function for categories
    // Public Methods - Category Helpers
     * Gets the effective category ID for a dashboard for display purposes.
     * For shared dashboards, this may differ from the actual CategoryID
     * based on the user's DashboardCategoryLink.
    public GetEffectiveCategoryId(dashboard: MJDashboardEntity): string | null {
        // If we have an effective category mapping, use it
        if (this._effectiveCategoryMap.has(dashboard.ID)) {
            return this._effectiveCategoryMap.get(dashboard.ID) ?? null;
        // Otherwise use the dashboard's actual category
        return dashboard.CategoryID || null;
    // Public Methods - Permission Checks
     * Check if a dashboard is shared with the current user (not owned)
    public IsShared(dashboardId: string): boolean {
        const perms = this._dashboardPermissions.get(dashboardId);
        // If permissions exist and user is not owner but can read, it's shared
        return perms ? !perms.IsOwner && perms.CanRead : false;
     * Check if user can edit a dashboard
    public CanEdit(dashboardId: string): boolean {
        // Default to true if no permissions provided (backwards compatibility)
        return perms ? perms.CanEdit : true;
     * Check if user can delete a dashboard
    public CanDelete(dashboardId: string): boolean {
        return perms ? perms.CanDelete : true;
     * Check if any of the currently selected dashboards can be deleted.
     * Returns true only if at least one selected dashboard can be deleted.
     * Used to enable/disable the bulk delete button in selection mode.
    public get CanDeleteAnySelected(): boolean {
        if (this.SelectedIds.size === 0) return false;
        for (const id of this.SelectedIds) {
            if (this.CanDelete(id)) {
     * Gets the list of selected dashboards that can be deleted.
     * Filters out dashboards the user doesn't have permission to delete.
    public GetDeletableSelectedDashboards(): MJDashboardEntity[] {
        return this.GetSelectedDashboards().filter(d => this.CanDelete(d.ID));
     * Highlight matching search text in a string
     * Returns HTML with <mark> tags around matches
    public HighlightMatch(text: string): string {
        if (!this.SearchText.trim() || !text) return text;
        const search = this.SearchText.trim();
        const regex = new RegExp(`(${this.escapeRegex(search)})`, 'gi');
     * Escape special regex characters
    // Drag and Drop on Category Cards
     * Handle drag over a category card
    public OnDragOverChildCategory(categoryId: string, event: DragEvent): void {
        if (!this.AllowDragDrop || !this.DraggingId) return;
        this.DragOverChildCategoryId = categoryId;
     * Handle drag leave from a category card
    public OnDragLeaveChildCategory(): void {
        this.DragOverChildCategoryId = null;
     * Handle drop on a category card
    public OnDropOnChildCategory(categoryId: string, event: DragEvent): void {
                        // Exit selection mode after move
     * Handle drop from breadcrumb component
    public OnBreadcrumbDrop(event: { TargetCategoryId: string | null; DashboardIds: string[] }): void {
        const dashboards = this.Dashboards.filter(d => event.DashboardIds.includes(d.ID));
                TargetCategoryId: event.TargetCategoryId
        let filtered = [...this._dashboards];
        // Filter out non-Config dashboards (Code and Dynamic Code types are not viewable/editable in browser)
        filtered = filtered.filter(d => d.Type === 'Config');
            filtered = filtered.filter(d =>
                d.Name.toLowerCase().includes(search) ||
                (d.Description || '').toLowerCase().includes(search)
        // Filter by current folder (category)
        // When at root (null), show only uncategorized dashboards
        // When in a category, show only dashboards directly in that category
        // Uses effective category (from EffectiveCategoryMap) for shared dashboards
        if (this._selectedCategoryId) {
            filtered = filtered.filter(d => this.GetEffectiveCategoryId(d) === this._selectedCategoryId);
            // At root level, show only uncategorized dashboards (effective CategoryID is null or empty)
            filtered = filtered.filter(d => !this.GetEffectiveCategoryId(d));
        this.FilteredDashboards = filtered;
     * Update the list of child categories for the current folder
    private updateChildCategories(): void {
        // Find categories that are children of the current category
        // Handle both null and undefined ParentID for root-level categories
            this.ChildCategories = this.Categories.filter(c => c.ParentID === this._selectedCategoryId);
            // At root level - show categories with no parent (null or undefined)
            this.ChildCategories = this.Categories.filter(c => !c.ParentID);
        // Apply search filter to categories if search text exists
            this.FilteredChildCategories = this.ChildCategories.filter(c =>
                (c.Description || '').toLowerCase().includes(search)
            this.FilteredChildCategories = [...this.ChildCategories];
     * Update breadcrumb trail from root to current category
        let currentId: string | null = this._selectedCategoryId;
     * Toggle selection for a single dashboard
    public ToggleSelection(dashboardId: string): void {
        if (this.SelectedIds.has(dashboardId)) {
            this.SelectedIds.delete(dashboardId);
            this.SelectedIds.add(dashboardId);
    private selectRange(fromId: string, toId: string): void {
        const fromIndex = this.FilteredDashboards.findIndex(d => d.ID === fromId);
        const toIndex = this.FilteredDashboards.findIndex(d => d.ID === toId);
        if (fromIndex === -1 || toIndex === -1) return;
        const start = Math.min(fromIndex, toIndex);
        const end = Math.max(fromIndex, toIndex);
        for (let i = start; i <= end; i++) {
            this.SelectedIds.add(this.FilteredDashboards[i].ID);
