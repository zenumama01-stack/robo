import { Component, OnInit, OnDestroy } from '@angular/core';
  MJCollectionArtifactEntity
import { SkipAPIAnalysisCompleteResponse } from '@memberjunction/skip-types';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
export interface ArtifactLoadResult {
  spec: ComponentSpec;
  artifactID: string;
  versionID: string;
  artifactName: string;
  selector: 'app-artifact-load-dialog',
  templateUrl: './artifact-load-dialog.component.html',
  styleUrl: './artifact-load-dialog.component.css'
export class ArtifactLoadDialogComponent implements OnInit, OnDestroy {
  // Tab state
  activeTab = 0; // 0 = Artifacts, 1 = Collections
  // Artifacts data
  artifacts: MJArtifactEntity[] = [];
  artifactVersions: MJArtifactVersionEntity[] = [];
  selectedArtifact: MJArtifactEntity | null = null;
  selectedVersion: MJArtifactVersionEntity | null = null;
  // Collections data
  collections: MJCollectionEntity[] = [];
  selectedCollection: MJCollectionEntity | null = null;
  collectionArtifacts: MJArtifactEntity[] = [];
  // Search and filter
  searchTerm = '';
  selectedArtifactType = '';
  userEmail = '';
  // Paging
  currentPage = 0;
  pageSize = 25;
  totalArtifacts = 0;
  hasMorePages = false;
  isLoadingVersions = false;
  isLoadingCollections = false;
  isFilterPanelCollapsed = false;
  // Preview
  previewSpec: ComponentSpec | null = null;
  previewError: string | null = null;
  showJsonPreview = false;
  constructor(public dialog: DialogRef) {}
    // Setup search debouncing
      this.filterArtifacts();
      this.loadArtifacts(),
      this.loadCollections()
  async loadArtifacts() {
      const startRow = this.currentPage * this.pageSize;
      const result = await rv.RunView<MJArtifactEntity>({
        EntityName: 'MJ: Artifacts',
        ExtraFilter: this.buildArtifactFilter(),
        MaxRows: this.pageSize,
        StartRow: startRow,
        this.artifacts = result.Results;
        this.totalArtifacts = result.TotalRowCount || 0;
        const totalPages = Math.ceil(this.totalArtifacts / this.pageSize);
        this.hasMorePages = this.currentPage < totalPages - 1;
      console.error('Error loading artifacts:', error);
  async loadCollections() {
    this.isLoadingCollections = true;
      const currentUserId = this.metadata.CurrentUser?.ID;
        this.collections = [];
      const result = await rv.RunView<MJCollectionEntity>({
        EntityName: 'MJ: Collections',
        ExtraFilter: `UserID = '${currentUserId}' OR ID IN (
          SELECT CollectionID FROM __mj.vwCollectionPermissions
          WHERE UserID = '${currentUserId}' AND CanRead = 1
        )`,
        this.collections = result.Results || [];
      console.error('Error loading collections:', error);
      this.isLoadingCollections = false;
  async selectCollection(collection: MJCollectionEntity) {
    this.selectedCollection = collection;
    this.selectedArtifact = null;
    this.selectedVersion = null;
    this.artifactVersions = [];
    // Load artifacts in this collection
        ExtraFilter: `ID IN (
          SELECT DISTINCT av.ArtifactID
          FROM __mj.vwArtifactVersions av
          INNER JOIN __mj.vwCollectionArtifacts ca ON ca.ArtifactVersionID = av.ID
          WHERE ca.CollectionID = '${collection.ID}'
        this.collectionArtifacts = result.Results || [];
      console.error('Error loading collection artifacts:', error);
      this.collectionArtifacts = [];
  private buildArtifactFilter(): string {
    // Always filter to Component type by default
    if (this.selectedArtifactType) {
      filters.push(`TypeID IN (SELECT ID FROM __mj.vwArtifactTypes WHERE Name = '${this.selectedArtifactType}')`);
      filters.push(`TypeID IN (SELECT ID FROM __mj.vwArtifactTypes WHERE Name = 'Component')`);
    if (this.searchTerm?.trim()) {
      const term = this.searchTerm.toLowerCase();
      filters.push(`(Name LIKE '%${term}%' OR Description LIKE '%${term}%')`);
    // User email filter
    if (this.userEmail?.trim()) {
      const schemaName = md.EntityByName("MJ: Users")?.SchemaName || "__mj";
      filters.push(`UserID IN (SELECT ID FROM ${schemaName}.vwUsers WHERE Email LIKE '%${this.userEmail.trim()}%')`);
    return filters.length > 0 ? filters.join(' AND ') : '';
  async selectArtifact(artifact: MJArtifactEntity) {
    this.selectedArtifact = artifact;
    this.previewSpec = null;
    this.previewError = null;
    await this.loadVersions(artifact.ID);
  async loadVersions(artifactId: string) {
    this.isLoadingVersions = true;
        ExtraFilter: `ArtifactID = '${artifactId}'`,
        this.artifactVersions = result.Results;
        // Auto-select the latest version
        if (this.artifactVersions.length > 0) {
          await this.selectVersion(this.artifactVersions[0]);
      console.error('Error loading versions:', error);
      this.isLoadingVersions = false;
  async selectVersion(version: MJArtifactVersionEntity) {
    await this.loadPreview(version);
  async loadPreview(version: MJArtifactVersionEntity) {
      // Try Content field first (new schema)
      if (version.Content) {
        this.previewSpec = JSON.parse(version.Content) as ComponentSpec;
      // Fallback to Configuration field (legacy)
      else if (version.Configuration) {
        const config = JSON.parse(version.Configuration);
        // Extract from SkipAPIAnalysisCompleteResponse if needed
        if (config.componentOptions && config.componentOptions.length > 0) {
          this.previewSpec = config.componentOptions[0].option;
          this.previewSpec = config;
        this.previewError = 'No content found in this version';
      this.previewError = `Failed to parse: ${error}`;
  async filterArtifacts() {
    this.currentPage = 0;
    await this.loadArtifacts();
  onSearchInput() {
    this.searchSubject.next(this.searchTerm);
  onArtifactTypeChange() {
  async nextPage() {
    if (this.hasMorePages) {
      this.currentPage++;
  async previousPage() {
    if (this.currentPage > 0) {
      this.currentPage--;
  canGoNext(): boolean {
    return this.hasMorePages;
  canGoPrevious(): boolean {
    return this.currentPage > 0;
  getTotalPages(): number {
    return Math.ceil(this.totalArtifacts / this.pageSize);
  toggleFilterPanel() {
    this.isFilterPanelCollapsed = !this.isFilterPanelCollapsed;
  getActiveFilterCount(): number {
    if (this.searchTerm?.trim()) count++;
    if (this.selectedArtifactType) count++;
    if (this.userEmail?.trim()) count++;
  canLoad(): boolean {
    return this.selectedArtifact !== null &&
           this.selectedVersion !== null &&
           this.previewSpec !== null;
    this.dialog.close(undefined);
  load() {
    if (!this.canLoad()) return;
    const result: ArtifactLoadResult = {
      spec: this.previewSpec!,
      artifactID: this.selectedArtifact!.ID,
      versionID: this.selectedVersion!.ID,
      versionNumber: this.selectedVersion!.VersionNumber,
      artifactName: this.selectedArtifact!.Name
    this.dialog.close(result);
  onTabSelect(index: number) {
    this.activeTab = index;
  getArtifactsByTab(): MJArtifactEntity[] {
    return this.activeTab === 0 ? this.artifacts : this.collectionArtifacts;
  toggleJsonPreview(): void {
    this.showJsonPreview = !this.showJsonPreview;
  getPreviewJSON(): string {
    return this.previewSpec ? JSON.stringify(this.previewSpec, null, 2) : '';
