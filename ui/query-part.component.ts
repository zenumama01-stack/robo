import { Component, ChangeDetectorRef, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { QueryViewerComponent, QueryEntityLinkClickEvent } from '@memberjunction/ng-query-viewer';
 * Runtime renderer for Query dashboard parts.
 * Displays query results using mj-query-viewer with parameter controls and auto-refresh support.
@RegisterClass(BaseDashboardPart, 'QueryPanelRenderer')
    selector: 'mj-query-part',
        <div class="query-part" [class.loading]="IsLoading" [class.error]="ErrorMessage">
              <mj-loading text="Loading query..."></mj-loading>
          <!-- No query configured -->
          @if (!IsLoading && !ErrorMessage && !hasQuery) {
              <h4>No Query Selected</h4>
              <p>Click the configure button to select a query for this part.</p>
          @if (!IsLoading && !ErrorMessage && hasQuery && queryId) {
            <div class="query-content">
                #queryViewer
                [QueryId]="queryId"
                [PersistState]="true"
                [PersistParameters]="true"
                (QueryError)="onQueryError($event)">
        .query-part {
        .query-content {
export class QueryPartComponent extends BaseDashboardPart implements AfterViewInit, OnDestroy {
    @ViewChild('queryViewer') queryViewer!: QueryViewerComponent;
    public hasQuery = false;
    public queryId: string | null = null;
    public showToolbar = true;
    private queryEntity: MJQueryEntity | null = null;
    private autoRefreshTimer: ReturnType<typeof setInterval> | null = null;
        const queryId = config?.['queryId'] as string | undefined;
        const queryName = config?.['queryName'] as string | undefined;
            this.hasQuery = false;
            if (queryId) {
                // Load query by ID to verify it exists
                this.queryEntity = await md.GetEntityObject<MJQueryEntity>('MJ: Queries');
                const loaded = await this.queryEntity.Load(queryId);
                    throw new Error('Query not found');
                this.queryId = queryId;
            } else if (queryName) {
                // Query by name - find the query ID from metadata
                const queryInfo = md.Queries.find(q => q.Name === queryName);
                    this.queryId = queryInfo.ID;
                    throw new Error(`Query "${queryName}" not found`);
            this.hasQuery = true;
            this.showToolbar = (config?.['showParameterControls'] as boolean) !== false;
            // Set auto-refresh if configured
            const autoRefreshSeconds = (config?.['autoRefreshSeconds'] as number) || 0;
            if (autoRefreshSeconds > 0) {
                this.startAutoRefresh(autoRefreshSeconds);
            this.setError(error instanceof Error ? error.message : 'Failed to load query');
        // Emit data change event with clicked entity info (for any listeners that need it)
            type: 'entity-link-click',
            recordId: event.recordId
        // Request navigation to open the record
        if (event.entityName && event.recordId) {
                event.recordId,
    public onQueryError(error: Error): void {
        console.error('[QueryPart] Query error:', error.message);
    public refreshQuery(): void {
        if (this.queryViewer) {
            this.queryViewer.Refresh();
    private startAutoRefresh(seconds: number): void {
            this.autoRefreshTimer = setInterval(() => {
                this.refreshQuery();
            }, seconds * 1000);
        if (this.autoRefreshTimer) {
            clearInterval(this.autoRefreshTimer);
            this.autoRefreshTimer = null;
        this.queryEntity = null;
        this.queryId = null;
