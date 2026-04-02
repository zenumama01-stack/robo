import { Input, Output, EventEmitter, ChangeDetectorRef, Directive, OnInit, OnDestroy } from '@angular/core';
    DashboardNavRequest,
    OpenEntityRecordNavRequest,
    OpenDashboardNavRequest,
    OpenQueryNavRequest,
    OpenApplicationNavRequest
 * Event emitted when a part requests configuration
export interface PartConfigureEvent {
    /** The panel requesting configuration */
    Panel: DashboardPanel;
    /** The part type */
 * Event emitted when a part requests removal
export interface PartRemoveEvent {
    /** The panel to remove */
 * Event emitted when a part's data changes
export interface PartDataChangeEvent {
    /** The panel whose data changed */
    /** The new data (type depends on part type) */
    Data: unknown;
 * Base class for dashboard part renderers.
 * Each part type (View, Query, Artifact, WebURL, etc.) has a renderer component
 * that extends this class. The renderer is responsible for:
 * 1. Displaying the part's content based on its configuration
 * 2. Handling edit mode UI (configure/remove buttons)
 * 3. Emitting events for user interactions
 * Subclasses are registered with @RegisterClass(BaseDashboardPart, 'PartTypeName')
 * and instantiated via ClassFactory using the DashboardPartType.DriverClass field.
 * This allows new part types to be added by:
 * 1. Creating a new component that extends BaseDashboardPart
 * 2. Registering it with @RegisterClass
 * 3. Adding a DashboardPartType record with the DriverClass set to the registered name
export abstract class BaseDashboardPart implements OnInit, OnDestroy {
     * The panel data including ID, title, icon, and configuration
    set Panel(value: DashboardPanel | null) {
        const previous = this._panel;
        this._panel = value;
        if (value !== previous) {
            this.onPanelChanged(value, previous);
    get Panel(): DashboardPanel | null {
        return this._panel;
    private _panel: DashboardPanel | null = null;
     * The part type metadata
     * Whether the dashboard is in edit mode
    set IsEditing(value: boolean) {
            this.onEditModeChanged(value);
    get IsEditing(): boolean {
     * Emitted when user clicks configure button
    @Output() ConfigureRequested = new EventEmitter<PartConfigureEvent>();
     * Emitted when user clicks remove button
    @Output() RemoveRequested = new EventEmitter<PartRemoveEvent>();
     * Emitted when the part's data changes (e.g., selection in a grid)
    @Output() DataChanged = new EventEmitter<PartDataChangeEvent>();
     * Emitted when the part requests navigation to another resource.
     * Parent component handles actual routing based on the request type.
    @Output() NavigationRequested = new EventEmitter<DashboardNavRequestEvent>();
     * Whether the part is currently loading
     * Error message if the part failed to load
        this.cleanup();
     * Initialize the part. Called in ngOnInit.
     * Override to perform async initialization like loading data.
    protected initialize(): void {
     * Cleanup resources. Called in ngOnDestroy.
     * Override to unsubscribe from observables, etc.
    protected cleanup(): void {
     * Called when the panel input changes.
     * Override to react to configuration changes.
    protected onPanelChanged(newPanel: DashboardPanel | null, oldPanel: DashboardPanel | null): void {
        // Default implementation triggers reload
        if (newPanel) {
     * Called when edit mode changes.
     * Override to adjust UI for edit mode.
    protected onEditModeChanged(isEditing: boolean): void {
     * Load or reload the part's content based on current configuration.
     * Override to implement actual data loading.
    public abstract loadContent(): Promise<void>;
     * Get the typed configuration for this part.
     * Subclasses should override to return the specific config type.
    public getConfig<T extends PanelConfig>(): T | null {
        return this.Panel?.config as T | null;
     * Request configuration (edit the part settings)
    public requestConfigure(): void {
        if (this.Panel && this.PartType) {
            this.ConfigureRequested.emit({
                Panel: this.Panel,
                PartType: this.PartType
     * Request removal of this part
    public requestRemove(): void {
            this.RemoveRequested.emit({
                Panel: this.Panel
     * Emit a data change event
    protected emitDataChanged(data: unknown): void {
            this.DataChanged.emit({
                Data: data
     * Set loading state
    protected setLoading(loading: boolean): void {
     * Set error state
    protected setError(message: string | null): void {
    // Navigation Request Methods
     * Request navigation to open a specific entity record
    public RequestOpenEntityRecord(
        recordId: string,
        mode: 'view' | 'edit' = 'view',
        openInNewTab = false
        const request: OpenEntityRecordNavRequest = {
            type: 'OpenEntityRecord',
            sourcePanelId: this.Panel?.id ?? '',
            openInNewTab
        this.emitNavigationRequest(request);
     * Request navigation to another dashboard
    public RequestOpenDashboard(
        categoryId?: string,
        const request: OpenDashboardNavRequest = {
            type: 'OpenDashboard',
            categoryId,
     * Request navigation to a query
    public RequestOpenQuery(
        parameters?: Record<string, unknown>,
        autoExecute = true,
        const request: OpenQueryNavRequest = {
            type: 'OpenQuery',
            autoExecute,
     * Request navigation to another application
    public RequestOpenApplication(
        resourceName?: string,
        const request: OpenApplicationNavRequest = {
            type: 'OpenApplication',
            applicationId,
            resourceName,
     * Emit a navigation request event
    protected emitNavigationRequest(request: DashboardNavRequest): void {
            this.NavigationRequested.emit({
                panel: this.Panel
