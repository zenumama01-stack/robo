  MJConversationArtifactVersionEntity
import { ComponentStudioStateService } from './component-studio-state.service';
 * Represents a single entry in the version history for a component artifact.
export interface VersionHistoryEntry {
  VersionNumber: number;
  AuthorName?: string;
 * Service that handles explicit, user-triggered versioning for Component Studio.
 * Follows a git-commit style model where each save creates a new immutable version.
export class ComponentVersionService {
  // --- State Tracking ---
  private _currentArtifactID: string | null = null;
  private _currentVersionNumber = 0;
  private _versionHistory: VersionHistoryEntry[] = [];
  private _componentArtifactTypeID: string | null = null;
  private _cachedConversationID: string | null = null;
  get CurrentArtifactID(): string | null { return this._currentArtifactID; }
  set CurrentArtifactID(value: string | null) { this._currentArtifactID = value; }
  get CurrentVersionNumber(): number { return this._currentVersionNumber; }
  get VersionHistory(): VersionHistoryEntry[] { return this._versionHistory; }
  constructor(private stateService: ComponentStudioStateService) {}
  // PUBLIC API
   * Save current working state as a new version.
   * Creates the ConversationArtifact if one does not yet exist for this component.
   * @param comment - optional user comment describing the version
   * @returns true if the version was saved successfully
  async SaveVersion(comment?: string): Promise<boolean> {
      const spec = this.stateService.GetCurrentSpec();
        console.error('ComponentVersionService: No current spec available to save.');
      const artifactId = await this.ensureArtifactExists(spec);
      if (!artifactId) {
        console.error('ComponentVersionService: Failed to create or resolve artifact.');
      const nextVersion = await this.resolveNextVersionNumber(artifactId);
      const versionComment = comment || `Auto-saved at ${new Date().toISOString()}`;
      const saved = await this.createVersion(artifactId, nextVersion, spec, versionComment);
        this._currentArtifactID = artifactId;
        this._currentVersionNumber = nextVersion;
        // Refresh the cached history
        this._versionHistory = await this.fetchVersionHistory(artifactId);
      console.error('ComponentVersionService: Error saving version:', error);
   * Update the current (latest) version in place instead of creating a new one.
   * @param comment - optional updated comment
   * @returns true if the version was updated successfully
  async UpdateCurrentVersion(comment?: string): Promise<boolean> {
      if (!this._currentArtifactID || this._currentVersionNumber === 0) {
        console.error('ComponentVersionService: No current version to update.');
        console.error('ComponentVersionService: No current spec available.');
      const latestVersion = await this.findLatestVersionEntity(this._currentArtifactID);
      if (!latestVersion) {
        console.error('ComponentVersionService: Could not find latest version entity.');
      latestVersion.Content = JSON.stringify(spec, null, 2);
      if (comment) {
        latestVersion.Comments = comment;
      const saved = await latestVersion.Save();
        this._versionHistory = await this.fetchVersionHistory(this._currentArtifactID);
      console.error('ComponentVersionService: Error updating current version:', error);
   * Load all versions for a given artifact, ordered by version number descending.
   * @param artifactId - the ConversationArtifact ID to load history for
   * @returns array of version history entries, newest first
  async LoadVersionHistory(artifactId: string): Promise<VersionHistoryEntry[]> {
    const history = await this.fetchVersionHistory(artifactId);
    this._versionHistory = history;
    return history;
   * Restore a previous version by creating a NEW version with the old content.
   * This preserves the full history chain rather than overwriting.
   * @param versionId - the ConversationArtifactVersion ID to restore
   * @returns true if the version was restored successfully
  async RestoreVersion(versionId: string): Promise<boolean> {
      const versionEntity = await this.loadVersionEntity(versionId);
      if (!versionEntity) {
        console.error('ComponentVersionService: Could not load version', versionId);
      const spec = this.parseVersionContent(versionEntity);
        console.error('ComponentVersionService: Could not parse version content');
      const restoredVersionNumber = versionEntity.Version;
      const restoreComment = `Restored from v${restoredVersionNumber}`;
      // Update the state with the restored spec before saving
      this.stateService.UpdateSpec(spec);
      // SaveVersion will pick up the restored spec from the state service
      return await this.SaveVersion(restoreComment);
      console.error('ComponentVersionService: Error restoring version:', error);
   * Load two versions and return their content for diffing.
   * @param versionId1 - the first version ID (typically the older one)
   * @param versionId2 - the second version ID (typically the newer one)
   * @returns an object with the before and after content strings
  async GetVersionDiff(versionId1: string, versionId2: string): Promise<{ before: string; after: string }> {
    const [version1, version2] = await Promise.all([
      this.loadVersionEntity(versionId1),
      this.loadVersionEntity(versionId2)
      before: version1?.Content || '',
      after: version2?.Content || ''
   * Reset the service state, typically when switching components.
  Reset(): void {
    this._currentArtifactID = null;
    this._currentVersionNumber = 0;
    this._versionHistory = [];
    // Keep _cachedConversationID — it's user-level, not component-level
  // PRIVATE: ARTIFACT MANAGEMENT
   * Ensures a ConversationArtifact record exists for the current component.
   * If CurrentArtifactID is already set, returns that. Otherwise creates a new one.
  private async ensureArtifactExists(spec: ComponentSpec): Promise<string | null> {
    if (this._currentArtifactID) {
      return this._currentArtifactID;
    return await this.createArtifact(spec);
   * Creates a new ConversationArtifact for the given component spec.
  private async createArtifact(spec: ComponentSpec): Promise<string | null> {
      const artifactTypeId = await this.resolveComponentArtifactTypeID();
      if (!artifactTypeId) {
        console.error('ComponentVersionService: Could not resolve "Component" artifact type.');
      const conversationId = await this.resolveConversationID();
        console.error('ComponentVersionService: No conversation ID available for artifact creation.');
      const artifact = await this.metadata.GetEntityObject<MJConversationArtifactEntity>('MJ: Conversation Artifacts');
      artifact.Name = spec.name || 'Untitled Component';
      artifact.Description = spec.description || null;
      artifact.ConversationID = conversationId;
      artifact.ArtifactTypeID = artifactTypeId;
      artifact.SharingScope = 'None';
      const saved = await artifact.Save();
        this._currentArtifactID = artifact.ID;
        return artifact.ID;
      console.error('ComponentVersionService: Failed to save artifact.');
      console.error('ComponentVersionService: Error creating artifact:', error);
   * Looks up the artifact type ID for "Component" from MJ: Artifact Types.
   * Caches the result to avoid repeated lookups.
  private async resolveComponentArtifactTypeID(): Promise<string | null> {
    if (this._componentArtifactTypeID) {
      return this._componentArtifactTypeID;
      ExtraFilter: `Name='Component'`,
      this._componentArtifactTypeID = result.Results[0].ID;
    console.error('ComponentVersionService: "Component" artifact type not found.');
   * Resolves or creates a Conversation record for Component Studio artifacts.
   * Caches the result for subsequent saves within the same session.
  private async resolveConversationID(): Promise<string | null> {
    if (this._cachedConversationID) {
      return this._cachedConversationID;
    const currentUser = this.metadata.CurrentUser;
    if (!currentUser) {
    // Look for an existing Component Studio conversation for this user
      EntityName: 'MJ: Conversations',
      ExtraFilter: `UserID='${currentUser.ID}' AND Name='Component Studio'`,
      this._cachedConversationID = result.Results[0].ID;
    // Create a new conversation for Component Studio
    const conversation = await this.metadata.GetEntityObject<MJConversationEntity>('MJ: Conversations');
    conversation.UserID = currentUser.ID;
    conversation.Name = 'Component Studio';
    conversation.Type = 'Skip';
    const saved = await conversation.Save();
      this._cachedConversationID = conversation.ID;
    console.error('ComponentVersionService: Failed to create Component Studio conversation.');
  // PRIVATE: VERSION MANAGEMENT
   * Determines the next version number for the given artifact.
  private async resolveNextVersionNumber(artifactId: string): Promise<number> {
    const result = await rv.RunView<{ Version: number }>({
      EntityName: 'MJ: Conversation Artifact Versions',
      ExtraFilter: `ConversationArtifactID='${artifactId}'`,
      Fields: ['Version'],
      OrderBy: 'Version DESC',
      return result.Results[0].Version + 1;
   * Creates and saves a new ConversationArtifactVersion record.
  private async createVersion(
    artifactId: string,
    versionNumber: number,
    spec: ComponentSpec,
    comment: string
      const version = await this.metadata.GetEntityObject<MJConversationArtifactVersionEntity>(
        'MJ: Conversation Artifact Versions'
      version.ConversationArtifactID = artifactId;
      version.Version = versionNumber;
      version.Content = JSON.stringify(spec, null, 2);
      version.Comments = comment;
      return await version.Save();
      console.error('ComponentVersionService: Error creating version record:', error);
   * Loads a single ConversationArtifactVersion entity by ID.
  private async loadVersionEntity(versionId: string): Promise<MJConversationArtifactVersionEntity | null> {
      const entity = await this.metadata.GetEntityObject<MJConversationArtifactVersionEntity>(
      const loaded = await entity.Load(versionId);
      return loaded ? entity : null;
      console.error('ComponentVersionService: Error loading version entity:', error);
   * Finds the latest version entity for the given artifact.
  private async findLatestVersionEntity(artifactId: string): Promise<MJConversationArtifactVersionEntity | null> {
    const result = await rv.RunView<MJConversationArtifactVersionEntity>({
   * Parses the Content field of a version entity back into a ComponentSpec.
  private parseVersionContent(versionEntity: MJConversationArtifactVersionEntity): ComponentSpec | null {
      const content = versionEntity.Content;
      return JSON.parse(content) as ComponentSpec;
      console.error('ComponentVersionService: Failed to parse version content:', error);
  // PRIVATE: HISTORY LOADING
   * Fetches the full version history for an artifact, ordered newest-first.
  private async fetchVersionHistory(artifactId: string): Promise<VersionHistoryEntry[]> {
      console.error('ComponentVersionService: Failed to load version history:', result.ErrorMessage);
    return result.Results.map(v => this.mapVersionToHistoryEntry(v));
   * Maps a MJConversationArtifactVersionEntity to a VersionHistoryEntry.
  private mapVersionToHistoryEntry(version: MJConversationArtifactVersionEntity): VersionHistoryEntry {
      ID: version.ID,
      VersionNumber: version.Version,
      Timestamp: version.__mj_CreatedAt,
      Comment: version.Comments || ''
