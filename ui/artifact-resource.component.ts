import { Component, ViewEncapsulation } from '@angular/core';
 * Artifact Resource - displays versioned content artifacts
 * Wraps the artifact-viewer-panel component for tab-based display
 * Supports reports, dashboards, forms, and other artifact types
@RegisterClass(BaseResourceComponent, 'ArtifactResource')
  selector: 'mj-artifact-resource',
    <div class="artifact-container">
      @if (currentUser && artifactId) {
        <mj-artifact-viewer-panel
          [artifactId]="artifactId"
          [currentUser]="currentUser"
          [environmentId]="environmentId"
          [showSaveToCollection]="true"
          [canShare]="true"
          [canEdit]="true">
        </mj-artifact-viewer-panel>
    .artifact-container {
export class ArtifactResource extends BaseResourceComponent {
  public currentUser: any = null;
  public artifactId: string = '';
  public environmentId: string = '';
    this.currentUser = md.CurrentUser;
    // Get artifact ID from resource data
    if (this.Data && this.Data.ResourceRecordID) {
      this.artifactId = this.Data.ResourceRecordID;
    // Get environment ID (default to empty string if not available)
    this.environmentId = '';  // TODO: Get from configuration if needed
    setTimeout(() => this.NotifyLoadComplete(), 100);
    // Try to load artifact name from database
    if (data.ResourceRecordID) {
        const artifact = await md.GetEntityObject<any>('MJ: Conversation Artifacts');
        await artifact.Load(data.ResourceRecordID);
        return artifact.Name || `Artifact - ${data.ResourceRecordID}`;
        console.error('Error loading artifact name:', error);
    return 'Artifact';
    return 'fa-solid fa-file-code';
