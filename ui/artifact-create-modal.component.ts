import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { MJArtifactEntity, MJArtifactTypeEntity, MJArtifactVersionEntity, MJCollectionEntity } from '@memberjunction/core-entities';
import { CollectionPermissionService } from '../../services/collection-permission.service';
 * Modal for creating new artifacts and adding them to collections
  selector: 'mj-artifact-create-modal',
        title="Create Artifact"
        <div class="artifact-form">
              Name <span class="required">*</span>
              class="k-textbox form-control"
              [(ngModel)]="formData.name"
              placeholder="Artifact name"
              #nameInput>
              Type <span class="required">*</span>
              [data]="artifactTypes"
              [(ngModel)]="formData.selectedType"
              [valuePrimitive]="false"
              [loading]="isLoadingTypes">
              class="k-textarea form-control"
              [(ngModel)]="formData.description"
              rows="2">
              Content <span class="required">*</span>
              class="k-textarea form-control content-area"
              [(ngModel)]="formData.content"
              placeholder="Paste your content here..."
              rows="12">
            <div class="content-hint">
              Paste or type the artifact content. The content will be saved as version 1.
            <div class="form-error">
          <button kendoButton (click)="onCancel()" [disabled]="isSaving">
            [disabled]="!canSave || isSaving">
            {{ isSaving ? 'Creating...' : 'Create Artifact' }}
    .artifact-form {
      font-family: 'Monaco', 'Menlo', 'Ubuntu Mono', 'Consolas', monospace;
    .content-hint {
      border: 1px solid #BFDBFE;
    .content-hint i {
    .form-error {
      border: 1px solid #FCA5A5;
    .form-error i {
export class ArtifactCreateModalComponent implements OnChanges {
  @Input() collectionId!: string;
  @Output() saved = new EventEmitter<MJArtifactEntity>();
  public formData = {
    selectedType: null as MJArtifactTypeEntity | null
  public artifactTypes: MJArtifactTypeEntity[] = [];
  public isLoadingTypes: boolean = false;
      this.loadArtifactTypes();
  get canSave(): boolean {
    return this.formData.name.trim().length > 0 &&
           this.formData.content.trim().length > 0 &&
           this.formData.selectedType !== null;
  private async loadArtifactTypes(): Promise<void> {
    this.isLoadingTypes = true;
      const result = await rv.RunView<MJArtifactTypeEntity>(
          ExtraFilter: 'IsEnabled=1',
        this.artifactTypes = result.Results;
        // Default to "Text" or first type
        const textType = this.artifactTypes.find(t => t.Name === 'Text' || t.Name === 'Markdown');
        if (textType) {
          this.formData.selectedType = textType;
        } else if (this.artifactTypes.length > 0) {
          this.formData.selectedType = this.artifactTypes[0];
      console.error('Error loading artifact types:', error);
      this.toastService.error('Failed to load artifact types');
      this.isLoadingTypes = false;
    if (!this.canSave) return;
      // Validate permission to add artifacts to collection
      await collection.Load(this.collectionId);
      // Check if user has Edit permission on collection
      if (collection.OwnerID && collection.OwnerID !== this.currentUser.ID) {
        const permission = await this.permissionService.checkPermission(
          this.collectionId,
        if (!permission?.canEdit) {
          this.errorMessage = 'You do not have Edit permission to add artifacts to this collection.';
      // Step 1: Create the artifact
      const artifact = await md.GetEntityObject<MJArtifactEntity>('MJ: Artifacts', this.currentUser);
      artifact.Name = this.formData.name.trim();
      artifact.Description = this.formData.description.trim() || null;
      artifact.TypeID = this.formData.selectedType!.ID;
      artifact.EnvironmentID = this.environmentId;
      artifact.UserID = this.currentUser.ID;
      const artifactSaved = await artifact.Save();
      if (!artifactSaved) {
        this.errorMessage = artifact.LatestResult?.Message || 'Failed to create artifact';
        this.toastService.error(this.errorMessage);
      // Step 2: Create the first version
      const version = await md.GetEntityObject<MJArtifactVersionEntity>('MJ: Artifact Versions', this.currentUser);
      version.VersionNumber = 1;
      version.Content = this.formData.content.trim();
      version.UserID = this.currentUser.ID;
      version.Name = this.formData.name.trim(); // Version inherits name
      version.Description = this.formData.description.trim() || null;
      const versionSaved = await version.Save();
      if (!versionSaved) {
        // Rollback: delete the artifact if version creation fails
        await artifact.Delete();
        this.errorMessage = version.LatestResult?.Message || 'Failed to create artifact version';
      // Step 3: Add to collection
      const collectionArtifact = await md.GetEntityObject('MJ: Collection Artifacts', this.currentUser);
      (collectionArtifact as any).CollectionID = this.collectionId;
      (collectionArtifact as any).ArtifactVersionID = version.ID;
      const collectionLinkSaved = await collectionArtifact.Save();
      if (!collectionLinkSaved) {
        // Rollback: delete version and artifact if collection link fails
        await version.Delete();
        this.errorMessage = 'Failed to add artifact to collection';
      this.toastService.success('Artifact created successfully');
      this.saved.emit(artifact);
      this.errorMessage = 'An unexpected error occurred';
    this.formData = {
      selectedType: this.artifactTypes.length > 0 ? this.artifactTypes[0] : null
