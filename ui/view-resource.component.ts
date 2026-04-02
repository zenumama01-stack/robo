import { Component, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { ResourceData, UserViewEntityExtended, ViewInfo } from '@memberjunction/core-entities';
import { RegisterClass, MJGlobal, MJEventType } from '@memberjunction/global';
import { CompositeKey, Metadata, EntityInfo, RunView } from '@memberjunction/core';
import { RecordOpenedEvent, ViewGridState, EntityViewerComponent } from '@memberjunction/ng-entity-viewer';
import { ExcelExportComponent } from '@progress/kendo-angular-excel-export';
 * UserViewResource - Resource wrapper for displaying User Views in tabs
 * This component wraps the EntityViewerComponent to display view data.
 * It loads the view configuration and entity, then renders the data grid/cards.
 * - Loads view by ID from ResourceRecordID
 * - Supports dynamic views by entity name + extra filter
 * - Applies view's WhereClause, GridState, and SortState
 * - Opens records in new tabs via NavigationService
@RegisterClass(BaseResourceComponent, 'ViewResource')
    selector: 'mj-userview-resource',
    templateUrl: './view-resource.component.html',
        .view-resource-container {
        .view-header {
            padding: 16px 20px 8px 20px;
        .view-title {
            color: var(--text-primary, #1a1a1a);
        .view-description {
            border: 1px solid #d4d4d4;
        .action-button:hover:not(:disabled) {
            border-color: #b4b4b4;
        .action-button:disabled {
        .create-button {
        .create-button:hover:not(:disabled) {
        .export-button:hover:not(:disabled) {
        .view-loading-state,
        .view-error-state {
            color: var(--danger-color, #dc3545);
        .view-error-state i {
        .view-error-state p {
        mj-entity-viewer {
        kendo-excelexport {
export class UserViewResource extends BaseResourceComponent {
    @ViewChild('entityViewer') entityViewerRef?: EntityViewerComponent;
    @ViewChild('excelExport') excelExportRef?: ExcelExportComponent;
    public viewEntity: UserViewEntityExtended | null = null;
    public gridState: ViewGridState | null = null;
    // Export state
    public isExporting: boolean = false;
    public exportData: any[] = [];
    public exportColumns: { Name: string; DisplayName: string }[] = [];
    public exportFileName: string = 'export.xlsx';
            this.loadView();
     * Load the view and entity based on ResourceData
    private async loadView(): Promise<void> {
            // Case 1: Load view by ID
                await this.loadViewById(data.ResourceRecordID);
            // Case 2: Load dynamic view by entity name
            else if (data.Configuration?.Entity) {
                await this.loadDynamicView(
                    data.Configuration.Entity as string,
                    data.Configuration.ExtraFilter as string | undefined
                this.errorMessage = 'No view ID or entity specified';
            console.error('Error loading view:', error);
            this.errorMessage = error instanceof Error ? error.message : 'Failed to load view';
            // If there was an error, notify load complete now
            if (this.errorMessage) {
            // Otherwise, wait for dataLoaded event from entity-viewer
     * Load a saved view by its ID
    private async loadViewById(viewId: string): Promise<void> {
        // Load the view entity
        const view = await ViewInfo.GetViewEntity(viewId);
        if (!view) {
            throw new Error(`View with ID ${viewId} not found`);
        this.viewEntity = view as UserViewEntityExtended;
        if (!this.viewEntity.UserCanView) {
            throw new Error('You do not have permission to view this view');
        // Load the entity info
        const entity = this.metadata.Entities.find(e => e.ID === this.viewEntity!.EntityID);
            throw new Error(`Entity for view not found`);
        this.entityInfo = entity;
        // Parse grid state if available
        if (this.viewEntity.GridState) {
                this.gridState = JSON.parse(this.viewEntity.GridState) as ViewGridState;
                console.warn('Failed to parse GridState:', e);
                this.gridState = null;
     * Load a dynamic view (no saved view, just entity + filter)
    private async loadDynamicView(entityName: string, _extraFilter?: string): Promise<void> {
        const entity = this.metadata.Entities.find(
            e => e.Name.trim().toLowerCase() === entityName.trim().toLowerCase()
            throw new Error(`Entity '${entityName}' not found`);
        this.viewEntity = null;
        // For dynamic views, we could create a synthetic viewEntity with just the WhereClause
        // but for now, we'll rely on the entity-viewer's default behavior
     * Handle record opened event - open in new tab
    public onRecordOpened(event: RecordOpenedEvent): void {
        if (event && event.entity && event.compositeKey) {
            this.navigationService.OpenEntityRecord(event.entity.Name, event.compositeKey);
     * Handle data loaded event from entity-viewer
    public onDataLoaded(): void {
     * Get display name for the resource tab
            const name = await this.metadata.GetEntityRecordName('MJ: User Views', compositeKey);
            return name ? name : `View: ${data.ResourceRecordID}`;
            const entityName = data.Configuration.Entity as string;
            const hasFilter = data.Configuration.ExtraFilter;
            return `${entityName} [Dynamic${hasFilter ? ' - Filtered' : ' - All'}]`;
        return 'User Views [Error]';
     * Get icon class for the resource tab
    override async GetResourceIconClass(_data: ResourceData): Promise<string> {
        return 'fa-solid fa-table-list';
        this.navigationService.OpenNewEntityRecord(this.entityInfo.Name);
     * Handle export to Excel request
        if (!this.excelExportRef || !this.entityInfo) {
            console.error('Cannot export: Excel export component or entity not available');
        this.isExporting = true;
            this.showNotification('Working on the export, will notify you when it is complete...', 'info', 2000);
            // Determine which columns to export based on grid state or view entity
            if (this.gridState?.columnSettings && this.gridState.columnSettings.length > 0) {
                // Use grid state - only export visible columns in grid order
                const visibleColumns = this.gridState.columnSettings.filter(col => col.hidden !== true);
                this.exportColumns = visibleColumns.map(col => ({
                    Name: col.Name,
                    DisplayName: col.DisplayName || col.Name
            } else if (this.viewEntity?.Columns) {
                // Use view's column configuration - only export visible columns in view order
                const visibleColumns = this.viewEntity.Columns.filter(col => !col.hidden);
                // Fall back to all non-virtual fields
                const visibleFields = this.entityInfo.Fields.filter(f => !f.IsVirtual);
                this.exportColumns = visibleFields.map(f => ({
                    DisplayName: f.DisplayNameOrName
            this.exportData = data;
            // Set the export filename
            const viewName = this.viewEntity?.Name || 'Data';
            this.exportFileName = `${this.entityInfo.Name}_${viewName}_${new Date().toISOString().split('T')[0]}.xlsx`;
            // Wait for Angular to update the DOM with the new data before triggering save
                this.excelExportRef!.save();
                this.showNotification('Excel Export Complete', 'success', 2000);
                this.isExporting = false;
            this.showNotification('Error exporting data', 'error', 5000);
    private async getExportData(): Promise<any[]> {
        if (!this.entityInfo) {
        // Build the filter for the export - combine view's WhereClause with any smart filter
        if (this.viewEntity?.WhereClause) {
            filter = this.viewEntity.WhereClause;
            OrderBy: '', // Let view handle sorting
            ResultType: 'simple' // Get plain objects for export
            throw new Error(result.ErrorMessage || 'Failed to load data for export');
    private showNotification(message: string, style: 'info' | 'success' | 'error' | 'warning', duration: number): void {
            eventCode: '',
            args: {
