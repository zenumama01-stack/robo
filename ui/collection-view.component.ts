import { Component, Input, OnInit, OnChanges, OnDestroy, SimpleChanges, ChangeDetectorRef } from '@angular/core';
import { MJCollectionEntity, MJArtifactEntity, MJArtifactVersionEntity, MJCollectionArtifactEntity } from '@memberjunction/core-entities';
type SortBy = 'name' | 'date' | 'type';
  selector: 'mj-collection-view',
    <div class="collection-view">
        <h2>{{ collection.Name || 'Collection' }}</h2>
              <i class="fas fa-grid"></i>
            [data]="sortOptions"
            [textField]="'label'"
            [valueField]="'value'"
            [(value)]="sortBy"
            (valueChange)="onSortChange()"
            [style.width.px]="150"
            placeholder="Sort by...">
            <button class="btn-add" (click)="onAddArtifact()" title="Add Artifact">
      <div class="view-content" [class.grid-mode]="viewMode === 'grid'" [class.list-mode]="viewMode === 'list'">
        @if (artifactVersions.length === 0) {
            <p>This collection is empty</p>
              <button class="btn-add-primary" (click)="onAddArtifact()">
                <i class="fas fa-plus"></i> Add Artifact
        @for (item of artifactVersions; track item.version.ID) {
          <mj-collection-artifact-card
            [artifact]="item.artifact"
            [version]="item.version"
            (selected)="onArtifactSelected(item)"
            (viewed)="onViewArtifact(item)"
            (edited)="onEditArtifact(item)"
            (removed)="onRemoveArtifact(item)">
          </mj-collection-artifact-card>
    <!-- Artifact Viewer Panel -->
    @if (showArtifactViewer && selectedArtifactId) {
      <div class="artifact-viewer-overlay" (click)="onCloseArtifactViewer()">
        <div class="artifact-viewer-container" (click)="$event.stopPropagation()">
            [artifactId]="selectedArtifactId"
            [versionNumber]="selectedVersionNumber"
            [contextCollectionId]="collection.ID"
            (closed)="onCloseArtifactViewer()">
    .collection-view { display: flex; flex-direction: column; height: 100%; background: white; }
    .view-header { padding: 20px 24px; border-bottom: 1px solid #D9D9D9; display: flex; justify-content: space-between; align-items: center; }
    .view-header h2 { margin: 0; font-size: 20px; flex: 1; }
    .header-actions { display: flex; align-items: center; gap: 12px; }
    .view-mode-toggle { display: flex; border: 1px solid #D9D9D9; border-radius: 4px; overflow: hidden; }
    .mode-btn { padding: 8px 12px; background: white; border: none; border-right: 1px solid #D9D9D9; cursor: pointer; color: #666; transition: all 150ms ease; }
    .mode-btn:last-child { border-right: none; }
    .mode-btn:hover { background: #F4F4F4; }
    .mode-btn.active { background: #0076B6; color: white; }
    .btn-add { padding: 8px 16px; background: #0076B6; color: white; border: none; border-radius: 4px; cursor: pointer; display: flex; align-items: center; gap: 6px; }
    .btn-add:hover { background: #005A8C; }
    .view-content { flex: 1; overflow-y: auto; padding: 24px; }
    .view-content.grid-mode { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 16px; }
    .view-content.list-mode { display: flex; flex-direction: column; gap: 12px; }
    .empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 64px 24px; color: #999; }
    .empty-state i { font-size: 64px; margin-bottom: 24px; }
    .empty-state p { margin: 0 0 24px 0; font-size: 16px; }
    .btn-add-primary { padding: 12px 24px; background: #0076B6; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 14px; display: flex; align-items: center; gap: 8px; }
    .btn-add-primary:hover { background: #005A8C; }
    .artifact-viewer-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0, 0, 0, 0.5); display: flex; align-items: center; justify-content: center; z-index: 10000; }
    .artifact-viewer-container { width: 90%; max-width: 1200px; height: 90vh; background: white; border-radius: 12px; overflow: hidden; box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3); }
export class CollectionViewComponent implements OnInit, OnChanges, OnDestroy {
  @Input() collection!: MJCollectionEntity;
  @Input() canEdit: boolean = true;
  // Store versions with parent artifact info for display
  public artifactVersions: Array<{
    version: MJArtifactVersionEntity;
  public sortBy: SortBy = 'date';
  public selectedArtifactId: string | null = null;
  public selectedVersionNumber: number | undefined = undefined;
  public showArtifactViewer = false;
    { label: 'Name', value: 'name' },
    { label: 'Date Modified', value: 'date' },
    { label: 'Type', value: 'type' }
    this.loadArtifacts();
    if (changes['collection'] && !changes['collection'].firstChange) {
    // Close artifact viewer when navigating away from collection
    if (this.showArtifactViewer) {
      this.onCloseArtifactViewer();
  private async loadArtifacts(): Promise<void> {
      // Load ALL VERSIONS in this collection (no DISTINCT - each version is separate)
      const versionResult = await rv.RunView<MJArtifactVersionEntity>({
          SELECT ca.ArtifactVersionID
          FROM [__mj].[vwCollectionArtifacts] ca
          WHERE ca.CollectionID='${this.collection.ID}'
        OrderBy: this.getVersionOrderBy(),
      if (versionResult.Success && versionResult.Results) {
        // Get unique artifact IDs
        const artifactIds = [...new Set(versionResult.Results.map(v => v.ArtifactID))];
        // Load parent artifact info (just for display metadata - no visibility filtering)
        const artifactMap = new Map<string, MJArtifactEntity>();
        if (artifactIds.length > 0) {
          const artifactFilter = artifactIds.map(id => `ID='${id}'`).join(' OR ');
          const artifactResult = await rv.RunView<MJArtifactEntity>({
            ExtraFilter: artifactFilter,
          if (artifactResult.Success && artifactResult.Results) {
            artifactResult.Results.forEach(a => artifactMap.set(a.ID, a));
        // Combine version + artifact info
        this.artifactVersions = versionResult.Results
          .map(version => ({
            version,
            artifact: artifactMap.get(version.ArtifactID)!
          .filter(item => item.artifact != null); // Filter out any without parent artifact
      console.error('Failed to load collection artifacts:', error);
  private getVersionOrderBy(): string {
        return 'Name ASC, VersionNumber DESC';
        // Will sort by parent artifact type (handled in template)
        return 'ArtifactID ASC, VersionNumber DESC';
        return '__mj_UpdatedAt DESC';
  onSortChange(): void {
  onArtifactSelected(item: { version: MJArtifactVersionEntity; artifact: MJArtifactEntity }): void {
    // TODO: Emit event or navigate to artifact detail view
  onViewArtifact(item: { version: MJArtifactVersionEntity; artifact: MJArtifactEntity }): void {
    this.selectedArtifactId = item.artifact.ID;
    this.selectedVersionNumber = item.version.VersionNumber;
    // Force change detection to ensure Input bindings propagate before component creation
    this.showArtifactViewer = true;
  onCloseArtifactViewer(): void {
    this.showArtifactViewer = false;
    this.selectedArtifactId = null;
    this.selectedVersionNumber = undefined;
  onEditArtifact(item: { version: MJArtifactVersionEntity; artifact: MJArtifactEntity }): void {
    // TODO: Open artifact editor
  async onRemoveArtifact(item: { version: MJArtifactVersionEntity; artifact: MJArtifactEntity }): Promise<void> {
    const versionLabel = `"${item.artifact.Name}" v${item.version.VersionNumber}`;
    if (!confirm(`Remove ${versionLabel} from this collection?`)) return;
      // Delete THIS SPECIFIC VERSION from the collection
        ExtraFilter: `CollectionID='${this.collection.ID}' AND ArtifactVersionID='${item.version.ID}'`,
        // Delete this version association
        for (const joinRecord of result.Results) {
          await joinRecord.Delete();
          `Removed ${versionLabel} from collection`,
      console.error('Failed to remove artifact version from collection:', error);
        'Failed to remove artifact version from collection',
  async onAddArtifact(): Promise<void> {
    // TODO: Open artifact picker dialog
    // For now, just show a simple prompt
    const name = prompt('Enter artifact name:');
      // Create new artifact
      artifact.Name = name;
      // Type is read-only, set via TypeID instead
      // For now, skip setting type - it has a default
        // Get the latest version of this artifact to add to collection
        const versionResult = await rv.RunView({
          ExtraFilter: `ArtifactID='${artifact.ID}'`,
        if (!versionResult.Success || !versionResult.Results || versionResult.Results.length === 0) {
          alert('Failed to get artifact version');
        // Add to collection via join table using version ID
        const joinRecord = await md.GetEntityObject<MJCollectionArtifactEntity>('MJ: Collection Artifacts', this.currentUser);
        joinRecord.CollectionID = this.collection.ID;
        joinRecord.ArtifactVersionID = versionResult.Results[0].ID;
        await joinRecord.Save();
      console.error('Failed to add artifact:', error);
      alert('Failed to add artifact');
