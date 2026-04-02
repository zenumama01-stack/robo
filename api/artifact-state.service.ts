import { BehaviorSubject, Observable, combineLatest } from 'rxjs';
import { map, shareReplay } from 'rxjs/operators';
import { ArtifactPermissionService } from './artifact-permission.service';
 * State management for artifacts and the artifact panel
 * Handles artifact CRUD operations and caching with permission enforcement
export class ArtifactStateService {
  private _activeArtifactId$ = new BehaviorSubject<string | null>(null);
  private _activeVersionNumber$ = new BehaviorSubject<number | null>(null);
  private _artifacts$ = new BehaviorSubject<Map<string, MJArtifactEntity>>(new Map());
  private _isPanelOpen$ = new BehaviorSubject<boolean>(false);
  private _panelMode$ = new BehaviorSubject<'view' | 'edit'>('view');
  public readonly activeArtifactId$ = this._activeArtifactId$.asObservable();
  public readonly activeVersionNumber$ = this._activeVersionNumber$.asObservable();
  public readonly isPanelOpen$ = this._isPanelOpen$.asObservable();
  public readonly panelMode$ = this._panelMode$.asObservable();
  // Derived observable for active artifact
  public readonly activeArtifact$: Observable<MJArtifactEntity | null> = combineLatest([
    this.activeArtifactId$,
    this._artifacts$
    map(([id, artifacts]) => id ? artifacts.get(id) || null : null),
  constructor(private artifactPermissionService: ArtifactPermissionService) {}
   * Check if current user can read an artifact
   * @param artifactId The artifact ID
   * @returns True if user has read permission
  async canReadArtifact(artifactId: string, currentUser: UserInfo): Promise<boolean> {
    return this.artifactPermissionService.checkPermission(artifactId, currentUser.ID, 'read', currentUser);
   * Check if current user can edit an artifact
   * @returns True if user has edit permission
  async canEditArtifact(artifactId: string, currentUser: UserInfo): Promise<boolean> {
    return this.artifactPermissionService.checkPermission(artifactId, currentUser.ID, 'edit', currentUser);
   * Check if current user can share an artifact
   * @returns True if user has share permission
  async canShareArtifact(artifactId: string, currentUser: UserInfo): Promise<boolean> {
    return this.artifactPermissionService.checkPermission(artifactId, currentUser.ID, 'share', currentUser);
   * Opens an artifact in the panel
   * @param id The artifact ID
   * @param versionNumber Optional specific version number
  openArtifact(id: string, versionNumber?: number): void {
    this._activeArtifactId$.next(id);
    this._activeVersionNumber$.next(versionNumber || null);
    this._isPanelOpen$.next(true);
   * Opens an artifact by version ID
   * @param versionId The artifact version ID
  async openArtifactByVersionId(versionId: string): Promise<void> {
      const version = await md.GetEntityObject<MJArtifactVersionEntity>('MJ: Artifact Versions');
      const loaded = await version.Load(versionId);
        // Open the artifact with the specific version number
        this.openArtifact(version.ArtifactID, version.VersionNumber);
        console.error('Failed to load artifact version:', versionId);
      console.error('Error loading artifact version:', error);
   * Closes the artifact panel
  closeArtifact(): void {
    this._activeArtifactId$.next(null);
    this._activeVersionNumber$.next(null);
    this._isPanelOpen$.next(false);
    this._panelMode$.next('view');
   * Toggles the panel open/closed state
  togglePanel(): void {
    this._isPanelOpen$.next(!this._isPanelOpen$.value);
   * Sets the panel mode
   * @param mode The mode ('view' or 'edit')
  setPanelMode(mode: 'view' | 'edit'): void {
    this._panelMode$.next(mode);
   * Caches an artifact in memory
   * @param artifact The artifact to cache
  cacheArtifact(artifact: MJArtifactEntity): void {
    const current = this._artifacts$.value;
    current.set(artifact.ID, artifact);
    this._artifacts$.next(new Map(current));
   * Removes an artifact from cache
  removeCachedArtifact(id: string): void {
   * Clears all cached artifacts
    this._artifacts$.next(new Map());
   * Loads artifacts for a conversation
   * @returns Array of artifacts
  async loadArtifactsForConversation(conversationId: string, currentUser: UserInfo): Promise<MJArtifactEntity[]> {
      const result = await rv.RunView<MJArtifactEntity>(
        // Cache all artifacts
        result.Results.forEach(artifact => this.cacheArtifact(artifact));
   * Loads artifacts for a collection
   * @param collectionId The collection ID
  async loadArtifactsForCollection(collectionId: string, currentUser: UserInfo): Promise<MJArtifactEntity[]> {
      // Load artifacts through the collection join - use subquery to get artifact IDs from versions
      const artifactsResult = await rv.RunView<MJArtifactEntity>(
            FROM [__mj].[vwArtifactVersions] av
            INNER JOIN [__mj].[vwCollectionArtifacts] ca ON ca.ArtifactVersionID = av.ID
            WHERE ca.CollectionID = '${collectionId}'
        artifactsResult.Results.forEach(artifact => this.cacheArtifact(artifact));
        return artifactsResult.Results;
   * Loads artifact VERSIONS for a collection (all versions, not deduplicated by artifact ID)
   * This method returns each version as a separate item with its parent artifact metadata
   * @returns Array of objects containing version and parent artifact info
  async loadArtifactVersionsForCollection(
    collectionId: string,
  ): Promise<Array<{ version: MJArtifactVersionEntity; artifact: MJArtifactEntity }>> {
      // Load ALL versions in collection (no DISTINCT - each version is separate)
          WHERE ca.CollectionID='${collectionId}'
      if (!versionResult.Success || !versionResult.Results) {
      // Load parent artifacts for display metadata
          artifactResult.Results.forEach(a => {
            artifactMap.set(a.ID, a);
            this.cacheArtifact(a); // Cache parent artifacts
      // Combine version + artifact
      return versionResult.Results
        .filter(item => item.artifact != null);
      console.error('Error loading collection artifact versions:', error);
   * Loads a single artifact by ID
   * @returns The artifact entity or null
  async loadArtifact(id: string, currentUser: UserInfo): Promise<MJArtifactEntity | null> {
      const artifact = await md.GetEntityObject<MJArtifactEntity>('MJ: Artifacts', currentUser);
      const loaded = await artifact.Load(id);
        this.cacheArtifact(artifact);
      console.error('Error loading artifact:', error);
   * Creates a new artifact
   * @param data Artifact data
   * @returns The created artifact
  async createArtifact(data: Partial<MJArtifactEntity>, currentUser: UserInfo): Promise<MJArtifactEntity> {
    Object.assign(artifact, data);
      throw new Error(artifact.LatestResult?.Message || 'Failed to create artifact');
   * Updates an artifact
   * @param updates The fields to update
   * @returns True if successful
  async updateArtifact(id: string, updates: Partial<MJArtifactEntity>, currentUser: UserInfo): Promise<boolean> {
    // Check edit permission
    const canEdit = await this.artifactPermissionService.checkPermission(id, currentUser.ID, 'edit', currentUser);
    if (!canEdit) {
      throw new Error('You do not have permission to edit this artifact');
      throw new Error('Artifact not found');
    Object.assign(artifact, updates);
      throw new Error(artifact.LatestResult?.Message || 'Failed to update artifact');
   * Deletes an artifact
  async deleteArtifact(id: string, currentUser: UserInfo): Promise<boolean> {
    // Check edit permission (required for deletion)
      throw new Error('You do not have permission to delete this artifact');
    const deleted = await artifact.Delete();
      this.removeCachedArtifact(id);
      if (this._activeArtifactId$.value === id) {
        this.closeArtifact();
      throw new Error(artifact.LatestResult?.Message || 'Failed to delete artifact');
   * Adds an artifact version to a collection
   * @param artifactId The artifact ID (for permission checking)
   * @param versionId Optional specific version ID. If not provided, uses latest version
  async addToCollection(artifactId: string, collectionId: string, currentUser: UserInfo, versionId?: string): Promise<void> {
    // Check edit permission (required to modify collection membership)
    const canEdit = await this.artifactPermissionService.checkPermission(artifactId, currentUser.ID, 'edit', currentUser);
      throw new Error('You do not have permission to add this artifact to a collection');
    // Get version ID if not provided
    let targetVersionId = versionId;
    if (!targetVersionId) {
      const versionResult = await rv.RunView<any>(
        throw new Error('No versions found for artifact');
      targetVersionId = versionResult.Results[0].ID;
    const collectionArtifact = await md.GetEntityObject('MJ: Collection Artifacts', currentUser);
    (collectionArtifact as any).CollectionID = collectionId;
    (collectionArtifact as any).ArtifactVersionID = targetVersionId;
      throw new Error('Failed to add artifact to collection');
   * Removes all versions of an artifact from a collection
  async removeFromCollection(artifactId: string, collectionId: string, currentUser: UserInfo): Promise<void> {
      throw new Error('You do not have permission to remove this artifact from a collection');
    // Find all versions of this artifact in the collection
    const result = await rv.RunView<any>(
        ExtraFilter: `CollectionID='${collectionId}' AND ArtifactVersionID IN (
      throw new Error(result.ErrorMessage || 'Failed to find collection artifact');
      throw new Error('Collection artifact link not found');
    // Delete all version associations
    for (const collectionArtifact of result.Results) {
      const deleted = await collectionArtifact.Delete();
        const errorMsg = collectionArtifact.LatestResult?.Message || 'Failed to remove artifact from collection';
