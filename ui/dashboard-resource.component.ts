import { Component, ViewContainerRef, ComponentRef, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { BaseResourceComponent, NavigationService, BaseDashboard, DashboardConfig } from '@memberjunction/ng-shared';
import { ResourceData, MJDashboardEntity, DashboardEngine, MJDashboardUserStateEntity, MJDashboardCategoryEntity, MJDashboardPartTypeEntity, DashboardUserPermissions } from '@memberjunction/core-entities';
import { RegisterClass, MJGlobal, SafeJSONParse } from '@memberjunction/global';
import { Metadata, CompositeKey, RunView, LogError } from '@memberjunction/core';
import { DataExplorerDashboardComponent, DataExplorerFilter, ShareDialogResult } from '@memberjunction/ng-dashboards';
import { DashboardViewerComponent, DashboardNavRequestEvent, PanelInteractionEvent, AddPanelResult, DashboardPanel } from '@memberjunction/ng-dashboard-viewer';
 * Dashboard Resource Wrapper - displays a single dashboard in a tab
 * Dynamically routes between code-based and config-based dashboards based on dashboard type
@RegisterClass(BaseResourceComponent, 'DashboardResource')
    selector: 'mj-dashboard-resource',
        <div class="dashboard-resource-wrapper">
                    <div class="error-icon">
                    <h2 class="error-title">Unable to Load Dashboard</h2>
                    <p class="error-message">{{ errorMessage }}</p>
                    @if (errorDetails) {
                        <details class="error-details">
                            <summary>Technical Details</summary>
                            <pre>{{ errorDetails }}</pre>
            <!-- View Mode Toolbar -->
            @if (configDashboard && !isEditMode && !errorMessage) {
                        <span class="dashboard-title">
                            {{ configDashboard.Name }}
                        @if (!dashboardPermissions.IsOwner && dashboardPermissions.PermissionSource !== 'none') {
                            <span class="shared-indicator" title="Shared with you">
                        @if (dashboardPermissions.CanShare) {
                        @if (dashboardPermissions.CanEdit) {
            <!-- Edit Mode Toolbar -->
            @if (configDashboard && isEditMode && !errorMessage) {
                        <button class="btn-add-part" (click)="openAddPartDialog()">
                                placeholder="Dashboard name">
                        <button class="btn-primary" (click)="saveDashboard()">
                        <button class="btn-cancel" (click)="cancelEdit()">
            <!-- Dashboard Content Container -->
            <div #container class="dashboard-resource-container"></div>
            @if (configDashboard) {
                    [Dashboard]="configDashboard"
        .dashboard-resource-wrapper {
        .dashboard-resource-container {
        /* View Mode Toolbar */
        .viewer-toolbar .toolbar-left {
        .viewer-toolbar .dashboard-title {
        .viewer-toolbar .dashboard-title i {
        /* Edit Mode Header */
        /* Add Part button */
        .btn-add-part i { font-size: 12px; }
        .btn-primary:hover { background: #3f51b5; }
        .btn-icon:hover { background: #f5f5f5; }
        /* Dashboard info inputs */
        .dashboard-name-input:hover { background: rgba(255, 255, 255, 0.9); }
        .dashboard-description-input:hover { background: rgba(255, 255, 255, 0.8); }
        .error-title {
        .error-details summary {
        .error-details pre {
            .viewer-header .header-left { flex-wrap: wrap; }
            .dashboard-description-input { max-width: none; }
export class DashboardResource extends BaseResourceComponent {
    private componentRef: ComponentRef<unknown> | null = null;
    @ViewChild('container', { static: true }) containerElement!: ElementRef<HTMLDivElement>;
    /** Error message to display when dashboard fails to load */
    public errorMessage: string | null = null;
    /** Technical error details (shown in expandable section) */
    public errorDetails: string | null = null;
    /** Cached dashboard categories for breadcrumb navigation */
    private categories: MJDashboardCategoryEntity[] = [];
    /** Reference to the dashboard viewer component (for config-based dashboards) */
    private viewerInstance: DashboardViewerComponent | null = null;
    /** The config-based dashboard entity (null for code-based dashboards) */
    public configDashboard: MJDashboardEntity | null = null;
    /** Whether we're in edit mode */
    public isEditMode = false;
    /** Editing fields */
    /** Current user's permissions for this dashboard */
    public dashboardPermissions: DashboardUserPermissions = {
    /** Whether the share dialog is visible */
     * Sets the error state with a user-friendly message and optional technical details
    private setError(message: string, error?: unknown): void {
        this.errorMessage = message;
            this.errorDetails = error.message;
            if (error.stack) {
                this.errorDetails += '\n\nStack trace:\n' + error.stack;
        } else if (error) {
            this.errorDetails = String(error);
     * Clears any previous error state
    private clearError(): void {
        this.errorMessage = null;
        this.errorDetails = null;
        private viewContainer: ViewContainerRef,
        if (!this.dataLoaded) {
            this.loadDashboard();
    // Need to override the getter too in TS otherwise the override to the setter alone above would break things
        if (this.componentRef) {
            this.componentRef.destroy();
    // Edit Mode Methods
     * Toggle between view and edit mode
        if (this.isEditMode) {
            this.cancelEdit();
            this.enterEditMode();
     * Enter edit mode
    private enterEditMode(): void {
        if (!this.configDashboard) return;
        this.isEditMode = true;
        this.editingName = this.configDashboard.Name;
        this.editingDescription = this.configDashboard.Description || '';
        // Tell the viewer to enter edit mode
        if (this.viewerInstance) {
            this.viewerInstance.isEditing = true;
     * Cancel edit mode and discard changes
        this.isEditMode = false;
        // Tell the viewer to exit edit mode
            this.viewerInstance.isEditing = false;
     * Save dashboard changes
        if (!this.configDashboard || !this.viewerInstance) return;
            // Update dashboard name and description
            this.configDashboard.Name = this.editingName;
            this.configDashboard.Description = this.editingDescription;
            // Save via the viewer (which handles layout saving)
            await this.viewerInstance.save();
            // Exit edit mode
            console.error('Error saving dashboard:', error);
     * Open the add panel dialog
            // Trigger the viewer's add panel flow
            this.viewerInstance.onAddPanelClick();
     * Open the share dialog for this dashboard
        if (result.Action === 'save' && this.configDashboard) {
            this.dashboardPermissions = DashboardEngine.Instance.GetDashboardPermissions(
                this.configDashboard.ID,
     * Load the appropriate dashboard component based on dashboard type
     * Routes between code-based dashboards (registered classes) and config-based dashboards
    private async loadDashboard(): Promise<void> {
        // Clear any previous error state
        this.clearError();
        if (!data?.ResourceRecordID) {
            this.NotifyLoadStarted();
            // Check if this is a special dashboard type (not a database record)
            if (config['dashboardType'] === 'DataExplorer' || data.ResourceRecordID === 'DataExplorer') {
                // Special case: Data Explorer dashboard with optional entity filter
                await this.loadDataExplorer(
                    config['entityFilter'],
                    config['appName'] as string | undefined,
                    config['appIcon'] as string | undefined
            await DashboardEngine.Instance.Config(false); // make sure it is configured, if already configured does nothing
            const dashboard = DashboardEngine.Instance.Dashboards.find(d => d.ID === data.ResourceRecordID);
            if (!dashboard) {
                throw new Error(`Dashboard with ID ${data.ResourceRecordID} not found.`);
            // Determine which dashboard component to load based on dashboard type
            if (dashboard.Type === 'Code') {
                // CODE-BASED DASHBOARD: Use registered class via DriverClass
                await this.loadCodeBasedDashboard(dashboard);
                // CONFIG-BASED DASHBOARD: Use the generic metadata-driven renderer
                await this.loadConfigBasedDashboard(dashboard);
            console.error('Error loading dashboard:', error);
            this.setError('The dashboard could not be loaded. This may be due to a missing component or configuration issue.', error);
     * Load the Data Explorer dashboard component with optional entity filter and context info
     * @param entityFilter Optional filter to constrain which entities are shown
     * @param contextName Optional name to display in the header (e.g., "CRM", "Association Demo")
     * @param contextIcon Optional Font Awesome icon class for the header
    private async loadDataExplorer(
        entityFilter?: DataExplorerFilter,
        contextName?: string,
        contextIcon?: string
            // Create the Data Explorer component directly (it's already registered)
            this.containerElement.nativeElement.innerHTML = '';
            const componentRef = this.viewContainer.createComponent(DataExplorerDashboardComponent);
            this.componentRef = componentRef;
            const instance = componentRef.instance;
            // Set the entity filter - ngOnInit will use this when it runs
            if (entityFilter) {
                instance.entityFilter = entityFilter;
            // Set context name and icon for customized header display
            if (contextName) {
                instance.contextName = contextName;
            if (contextIcon) {
                instance.contextIcon = contextIcon;
            // Manually append the component's native element inside the div
            const nativeElement = (componentRef.hostView as any).rootNodes[0];
            nativeElement.style.width = '100%';
            nativeElement.style.height = '100%';
            this.containerElement.nativeElement.appendChild(nativeElement);
            // Handle open entity record events
            instance.OpenEntityRecord.subscribe((eventData: { EntityName: string; RecordPKey: CompositeKey }) => {
                if (eventData && eventData.EntityName && eventData.RecordPKey) {
                    this.navigationService.OpenEntityRecord(eventData.EntityName, eventData.RecordPKey);
            // Setup LoadCompleteEvent to know when the dashboard is ready
            instance.LoadCompleteEvent = () => {
            // Initialize dashboard (no database config needed for DataExplorer)
            const config: DashboardConfig = {
                dashboard: null as unknown as MJDashboardEntity, // No database record
            instance.Config = config;
            instance.Refresh();
            // Trigger change detection to ensure the component updates
            componentRef.changeDetectorRef.detectChanges();
            console.error('Error loading Data Explorer:', error);
            this.setError('The Data Explorer could not be loaded.', error);
     * Load a code-based dashboard by looking up the registered class
    private async loadCodeBasedDashboard(dashboard: MJDashboardEntity): Promise<void> {
            if (!dashboard.DriverClass) {
                throw new Error(`Dashboard '${dashboard.Name}' is marked as Code type but has no DriverClass specified`);
            // Look up the registered class using the DriverClass name
            const classReg = MJGlobal.Instance.ClassFactory.GetRegistration(
                BaseDashboard,
                dashboard.DriverClass
            if (!classReg?.SubClass) {
                throw new Error(`Dashboard class '${dashboard.DriverClass}' is not registered. Please check the class registration.`);
            // Create the component instance
            this.componentRef = this.viewContainer.createComponent<BaseDashboard>(classReg.SubClass);
            const instance = this.componentRef.instance as BaseDashboard;
            // Setup LoadCompleteEvent() to know when the dashboard is ready
            // Initialize with dashboard data
            const userStateEntity = await this.loadDashboardUserState(dashboard.ID);
                dashboard,
                userState: userStateEntity.UserState ? SafeJSONParse(userStateEntity.UserState) : {}
            const nativeElement = (this.componentRef.hostView as any).rootNodes[0];
            // handle open entity record events in MJ Explorer with routing
            instance.OpenEntityRecord.subscribe((data: { EntityName: string; RecordPKey: CompositeKey }) => {
                // check to see if the data has entityname/pkey
                if (data && data.EntityName && data.RecordPKey) {
                    // Use NavigationService to open entity record in new tab
                    this.navigationService.OpenEntityRecord(data.EntityName, data.RecordPKey);
                    console.warn('DashboardResource - invalid data, missing EntityName or RecordPKey:', data);
            instance.UserStateChanged.subscribe(async (userState: any) => {
                if (!userState) {
                    // if the user state is null, we need to remove it from the user state
                    userState = {};
                // save the user state to the dashboard user state entity
                userStateEntity.UserState = JSON.stringify(userState);
                if (!await userStateEntity.Save()) {
                    LogError('Error saving user state', null, userStateEntity.LatestResult?.CompleteMessage);
            console.error('Error loading code-based dashboard:', error);
            this.setError(`The dashboard "${dashboard.Name}" could not be loaded. The dashboard class may not be registered or may have failed to initialize.`, error);
    protected async loadDashboardUserState(dashboardId: string): Promise<MJDashboardUserStateEntity> {
        // handle user state changes for the dashboard
        const stateResult = DashboardEngine.Instance.DashboardUserStates.filter(dus => dus.DashboardID === dashboardId && dus.UserID === md.CurrentUser.ID)
        let stateObject: MJDashboardUserStateEntity;
        if (stateResult && stateResult.length > 0) {
            stateObject = stateResult[0];
            stateObject = await md.GetEntityObject<MJDashboardUserStateEntity>('MJ: Dashboard User States');
            stateObject.DashboardID = dashboardId;
            stateObject.UserID = md.CurrentUser.ID;
            // don't save becuase we don't care about the state until something changes
        return stateObject;
     * Load a config-based dashboard using the new DashboardViewerComponent (Golden Layout)
    private async loadConfigBasedDashboard(dashboard: MJDashboardEntity): Promise<void> {
            const componentRef = this.viewContainer.createComponent(DashboardViewerComponent);
            // Store references for external toolbar control
            this.viewerInstance = instance;
            this.configDashboard = dashboard;
            // Compute user permissions for this dashboard
            // Load categories for breadcrumb navigation (if not already loaded)
            if (this.categories.length === 0) {
                this.categories = DashboardEngine.Instance.DashboardCategories;
            // Set the dashboard entity directly on the viewer
            // We provide our own external toolbar, so disable the viewer's internal toolbar
            instance.dashboard = dashboard;
            instance.showToolbar = false;         // We provide external toolbar
            instance.showBreadcrumb = false;      // Already in its own tab, no breadcrumb needed
            instance.showOpenInTabButton = false; // Already in its own tab
            instance.showEditButton = false;      // External toolbar handles edit
            instance.Categories = this.categories;
            // Wire up navigation events - handle navigation requests from the dashboard
            instance.navigationRequested.subscribe((event: DashboardNavRequestEvent) => {
                this.handleNavigationRequest(event);
            // Wire up "Open in Tab" button click
            instance.openInTab.subscribe((event: { dashboardId: string; dashboardName: string }) => {
                this.navigationService.OpenDashboard(event.dashboardId, event.dashboardName);
            // Wire up dashboard saved event
            instance.dashboardSaved.subscribe((savedDashboard: MJDashboardEntity) => {
                this.ResourceRecordSaved(savedDashboard);
            // Wire up error events
            instance.error.subscribe((errorEvent: { message: string; error?: Error }) => {
                console.error('Dashboard error:', errorEvent.message, errorEvent.error);
            // Notify load complete after a brief delay to let Golden Layout initialize
            }, 150);
            console.error('Error loading config-based dashboard:', error);
            this.setError(`The dashboard "${dashboard.Name}" could not be loaded. There may be an issue with the dashboard configuration.`, error);
     * Handle navigation requests from the dashboard viewer
    private handleNavigationRequest(event: DashboardNavRequestEvent): void {
                const entityRequest = request as { type: 'OpenEntityRecord'; entityName: string; recordId: string };
                const pkey = new CompositeKey([{ FieldName: 'ID', Value: entityRequest.recordId }]);
                this.navigationService.OpenEntityRecord(entityRequest.entityName, pkey);
                const dashRequest = request as { type: 'OpenDashboard'; dashboardId: string };
                // Load dashboard name from engine cache
                const targetDashboard = DashboardEngine.Instance.Dashboards.find(d => d.ID === dashRequest.dashboardId);
                const name = targetDashboard?.Name || 'Dashboard';
                this.navigationService.OpenDashboard(dashRequest.dashboardId, name);
                const queryRequest = request as { type: 'OpenQuery'; queryId: string };
                this.navigationService.OpenQuery(queryRequest.queryId, 'Query');
                console.warn('Unhandled navigation request type:', request.type);
     * Get the display name for a dashboard resource
     * Loads the actual dashboard name from the database if available
    override async GetResourceDisplayName(data: ResourceData): Promise<string> {
            // Try to load dashboard metadata if we have the record ID
            if (data.ResourceRecordID && data.ResourceRecordID.length > 0) {
                const compositeKey = new CompositeKey([{ FieldName: 'ID', Value: data.ResourceRecordID }]);
                const name = await md.GetEntityRecordName('Dashboards', compositeKey);
            // Silently fail and use fallback
        // Fallback: use provided name or generic label
        return data.Name || 'Dashboard';
     * Get the icon class for dashboard resources
        return 'fa-solid fa-table-columns';
