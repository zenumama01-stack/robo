import { RunView, Metadata, UserInfo } from '@memberjunction/core';
import { MJArtifactEntity, MJArtifactVersionEntity } from '@memberjunction/core-entities';
export interface ArtifactSelectionResult {
  artifact: MJArtifactEntity;
  action: 'new-version' | 'update-version';
  versionToUpdate?: MJArtifactVersionEntity;
  selector: 'app-artifact-selection-dialog',
  templateUrl: './artifact-selection-dialog.component.html',
  styleUrl: './artifact-selection-dialog.component.css'
export class ArtifactSelectionDialogComponent implements OnInit, OnDestroy {
  // Paging State
  showNewArtifactForm = false;
  // Selection State
  versionAction: 'new' | 'update' = 'new';
  // New Artifact Form
  newArtifactName = '';
  newArtifactDescription = '';
  private currentUser: UserInfo | null = null;
    public dialog: DialogRef,
    await this.filterArtifacts();
      // Calculate StartRow for server-side paging
      // Load artifacts with paging
        ExtraFilter: this._artifactFilter,
        // Calculate total pages using TotalRowCount from server
  private _artifactFilter: string | undefined= undefined;
  public async filterArtifacts() {
    // Reset to first page when filters change
    // Filter by search term
    // Filter by artifact type
      filters.push(`ArtifactTypeID IN (SELECT ID FROM __mj.vwArtifactTypes WHERE Name = '${this.selectedArtifactType}')`);
    // Filter by user email if provided
      const userFilter = `UserID IN (SELECT ID FROM ${schemaName}.vwUsers WHERE Email LIKE '%${this.userEmail.trim()}%')`;
      filters.push(userFilter);
    // Combine all filters
    this._artifactFilter = filters.length > 0 ? filters.join(' AND ') : undefined;
  selectCreateNew() {
    this.showNewArtifactForm = true;
    this.showNewArtifactForm = false;
    this.versionAction = 'new';
    // Load versions for this artifact
  getNextVersionNumber(): number {
    if (this.artifactVersions.length === 0) return 1;
    return Math.max(...this.artifactVersions.map(v => v.VersionNumber)) + 1;
  // Paging methods
    // Clear selected artifact when type changes
  canSave(): boolean {
    if (this.showNewArtifactForm) {
      return this.newArtifactName.trim().length > 0;
    if (!this.selectedArtifact) return false;
    if (this.versionAction === 'update') {
      return this.selectedVersion !== null;
  getSaveButtonText(): string {
      return 'Create & Save';
    if (this.versionAction === 'update' && this.selectedVersion) {
      return `Update Version ${this.selectedVersion.VersionNumber}`;
    return `Save as Version ${this.getNextVersionNumber()}`;
    if (!this.canSave()) return;
    // Handle new artifact creation
      const newArtifact = await this.createNewArtifact();
      if (newArtifact) {
        const result: ArtifactSelectionResult = {
          artifact: newArtifact,
          action: 'new-version'
    // Handle existing artifact selection
    if (this.selectedArtifact) {
        artifact: this.selectedArtifact,
        action: this.versionAction === 'update' ? 'update-version' : 'new-version',
        versionToUpdate: this.versionAction === 'update' ? this.selectedVersion! : undefined
      // If updating, show confirmation
        const confirm = window.confirm(
          `Are you sure you want to overwrite version ${this.selectedVersion!.VersionNumber}? This action cannot be undone.`
  private async createNewArtifact(): Promise<MJArtifactEntity | null> {
      const artifact = await this.metadata.GetEntityObject<MJArtifactEntity>('MJ: Artifacts');
      artifact.Name = this.newArtifactName;
      artifact.Description = this.newArtifactDescription || null;
      // Get Component artifact type
      const typeResult = await rv.RunView({
        ExtraFilter: `Name = 'Component'`,
      if (typeResult.Success && typeResult.Results?.length > 0) {
        artifact.TypeID = typeResult.Results[0].ID;
      // Set default environment if available from current user
      // Environment ID is optional - will be set by server if not provided
      const envId = (this.metadata.CurrentUser as any)?.EnvironmentID;
      if (envId) {
        artifact.EnvironmentID = envId;
      artifact.Comments = 'Created from Component Studio';
      const saveResult = await artifact.Save();
          `Artifact "${artifact.Name}" created successfully`,
        return artifact;
        console.error('Failed to create artifact - Full LatestResult:', artifact.LatestResult);
          'Failed to create artifact',
      console.error('Error creating artifact:', error);
        'Error creating artifact',
