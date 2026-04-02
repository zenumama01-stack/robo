import { Component, ChangeDetectorRef, AfterViewInit, OnDestroy, Input } from '@angular/core';
import { BaseDashboardPart } from './base-dashboard-part';
import { UserInfo, Metadata, CompositeKey } from '@memberjunction/core';
 * Runtime renderer for Artifact dashboard parts.
 * Displays artifacts using mj-artifact-viewer-panel including reports, charts, and AI-generated content.
@RegisterClass(BaseDashboardPart, 'ArtifactPanelRenderer')
    selector: 'mj-artifact-part',
        <div class="artifact-part" [class.loading]="IsLoading" [class.error]="ErrorMessage">
              <mj-loading text="Loading artifact..."></mj-loading>
          <!-- No artifact configured -->
          @if (!IsLoading && !ErrorMessage && !hasArtifact) {
              <h4>No Artifact Selected</h4>
              <p>Click the configure button to select an artifact for this part.</p>
          @if (!IsLoading && !ErrorMessage && hasArtifact && artifactId) {
              [versionNumber]="versionNumber"
              [showHeader]="showHeader"
              [showTabs]="showTabs"
              [showCloseButton]="showCloseButton"
              [showMaximizeButton]="showMaximizeButton"
              [viewContext]="null"
              [canShare]="false"
              [canEdit]="false"
              [isMaximized]="false"
              [refreshTrigger]="refreshTrigger"
        .artifact-part {
        .error-state,
        .error-state i,
        mj-artifact-viewer-panel {
export class ArtifactPartComponent extends BaseDashboardPart implements AfterViewInit, OnDestroy {
     * Current user - required by the artifact viewer panel.
     * Should be provided by the dashboard host or retrieved from a service.
    @Input() CurrentUser: UserInfo | null = null;
     * Environment ID - required by the artifact viewer panel.
     * Should be provided by the dashboard host.
    @Input() EnvironmentId: string = '';
    public hasArtifact = false;
    public artifactId: string | null = null;
    public versionNumber: number | undefined;
    public showHeader: boolean = false; // Default to false for dashboard embedding
    public showTabs: boolean = true;
    public showCloseButton: boolean = false; // Always false in dashboard context - close handled by dashboard
    public showMaximizeButton: boolean = false; // Always false in dashboard context - maximize handled by dashboard
    public refreshTrigger = new Subject<{ artifactId: string; versionNumber: number }>();
    // Expose for template
    public get currentUser(): UserInfo {
        // Use provided CurrentUser, or fall back to Metadata.CurrentUser
        // In client-side Angular context, Metadata.CurrentUser should always be available
        const user = this.CurrentUser || new Metadata().CurrentUser;
            throw new Error('No current user available - user must be logged in to view artifacts');
    public get environmentId(): string {
        return this.EnvironmentId || '';
        if (this.Panel) {
    public async loadContent(): Promise<void> {
        const config = this.getConfig<PanelConfig>();
        const artifactId = config?.['artifactId'] as string | undefined;
            this.hasArtifact = false;
        this.setLoading(true);
            // Set artifact ID and version from config
            this.artifactId = artifactId;
            this.versionNumber = config?.['versionNumber'] as number | undefined;
            // Display options - showHeader defaults to false for clean dashboard embedding
            this.showHeader = (config?.['showHeader'] as boolean) ?? false;
            this.showTabs = (config?.['showTabs'] as boolean) ?? true;
            this.hasArtifact = true;
            this.setLoading(false);
            this.setError(error instanceof Error ? error.message : 'Failed to load artifact');
     * Refresh the artifact display
        if (this.artifactId && this.versionNumber) {
            this.refreshTrigger.next({
                versionNumber: this.versionNumber
     * Handle navigation link events from artifact viewer (conversation/collection links)
    public onNavigateToLink(event: { type: 'conversation' | 'collection'; id: string; artifactId?: string; versionNumber?: number; versionId?: string }): void {
        // Emit data change event for navigation link (for listeners)
        this.emitDataChanged({
            type: 'navigate-to-link',
            linkType: event.type,
            linkId: event.id,
            artifactId: event.artifactId,
            versionNumber: event.versionNumber,
        // TODO: Add navigation request methods for conversation/collection when needed
        // For now, these are emitted as data change events for parent components to handle
     * Handle entity record navigation events from artifact viewer
    public onOpenEntityRecord(event: { entityName: string; compositeKey: CompositeKey }): void {
        // Emit data change event for listeners
            type: 'open-entity-record',
            compositeKey: event.compositeKey
        // Use proper navigation request to bubble up through the stack
        if (event.entityName && event.compositeKey) {
            this.RequestOpenEntityRecord(
                event.entityName,
                event.compositeKey.ToURLSegment(),
                'view',
    protected override cleanup(): void {
        this.refreshTrigger.complete();
        this.artifactId = null;
        this.versionNumber = undefined;
