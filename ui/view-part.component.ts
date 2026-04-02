import { Component, ChangeDetectorRef, AfterViewInit, OnDestroy } from '@angular/core';
import { EntityViewMode, RecordSelectedEvent, RecordOpenedEvent } from '@memberjunction/ng-entity-viewer';
 * Runtime renderer for View dashboard parts.
 * Displays entity data using mj-entity-viewer with grid, cards, or timeline layout.
@RegisterClass(BaseDashboardPart, 'ViewPanelRenderer')
    selector: 'mj-view-part',
        <div class="view-part" [class.loading]="IsLoading" [class.error]="ErrorMessage">
              <mj-loading text="Loading view..."></mj-loading>
          <!-- No view configured -->
          @if (!IsLoading && !ErrorMessage && !hasView) {
              <h4>No View Selected</h4>
              <p>Click the configure button to select a view for this part.</p>
          @if (!IsLoading && !ErrorMessage && hasView && entityInfo) {
              [(viewMode)]="viewMode"
              [gridSelectionMode]="selectionMode"
              (recordSelected)="onRecordSelected($event)"
              (recordOpened)="onRecordOpened($event)">
        .view-part {
export class ViewPartComponent extends BaseDashboardPart implements AfterViewInit, OnDestroy {
    public hasView = false;
    public viewMode: EntityViewMode = 'grid';
    public selectionMode: 'single' | 'multiple' = 'single';
        const viewId = config?.['viewId'] as string | undefined;
        const entityName = config?.['entityName'] as string | undefined;
            this.hasView = false;
            // Set view mode from config
            this.viewMode = (config?.['displayMode'] as EntityViewMode) || 'grid';
            this.selectionMode = config?.['selectionMode'] === 'multiple' ? 'multiple' : 'single';
            if (viewId) {
                // Load saved view by ID
                const viewEntity = await md.GetEntityObject<UserViewEntityExtended>('MJ: User Views');
                const loaded = await viewEntity.Load(viewId);
                this.viewEntity = viewEntity; // IMPORTANT - only set this.viewEntity AFTER we have it loaded in the above
                    throw new Error('View not found');
                // Get entity info from the view - prefer ViewEntityInfo if available (set by UserViewEntityExtended.Load)
                // Fall back to looking up by Entity name (virtual field) or EntityID
                if (viewEntity.ViewEntityInfo) {
                    this.entityInfo = viewEntity.ViewEntityInfo;
                } else if (viewEntity.Entity) {
                    this.entityInfo = md.Entities.find(e => e.Name === viewEntity!.Entity) || null;
                } else if (viewEntity.EntityID) {
                    // Last resort: look up by EntityID
                    this.entityInfo = md.Entities.find(e => e.ID === viewEntity!.EntityID) || null;
                    throw new Error(`Could not determine entity for view "${this.viewEntity.Name}" (ID: ${viewId})`);
            } else if (entityName) {
                // Create dynamic view for entity (no saved view)
                this.entityInfo = md.Entities.find(e => e.Name === entityName) || null;
                    throw new Error(`Entity "${entityName}" not found`);
                // No viewEntity means the entity-viewer will show all records
            this.hasView = true;
            this.setError(error instanceof Error ? error.message : 'Failed to load view');
    public onRecordSelected(event: RecordSelectedEvent): void {
        // Emit data change event with selected record
            type: 'record-selected',
            primaryKey: event.record?.PrimaryKey
        // Emit data change event for record open (for any listeners that need it)
            type: 'record-opened',
        if (event.entity && event.compositeKey) {
                event.entity.Name,
        // EntityViewer handles its own cleanup
        this.entityInfo = null;
