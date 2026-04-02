 * Represents artifact information with lazy-loading capabilities.
 * Stores minimal display data initially (from query) and loads full entities on-demand.
 * This class enables performance optimization by:
 * - Loading only basic metadata initially (name, type, version)
 * - Deferring full entity load (with Content field) until user clicks
 * - Sharing a single load promise across multiple concurrent requests
 * - Caching loaded entities to prevent redundant database queries
export class LazyArtifactInfo {
  // Display data (always available from initial query - no lazy loading needed)
  public readonly conversationDetailId: string;
  public readonly direction: string;
  public readonly artifactVersionId: string;
  public readonly versionNumber: number;
  public readonly versionName: string | null;
  public readonly versionDescription: string | null;
  public readonly versionCreatedAt: Date;
  public readonly artifactId: string;
  public readonly artifactName: string;
  public readonly artifactType: string;
  public readonly artifactDescription: string;
  public readonly visibility: string;
  // Full entities (lazy-loaded on first access via getArtifact() or getVersion())
  private _artifact: MJArtifactEntity | null = null;
  private _version: MJArtifactVersionEntity | null = null;
  private _isLoading = false;
  private _loadPromise: Promise<void> | null = null;
    queryResult: any,
    private currentUser: UserInfo,
    preloadedArtifact?: MJArtifactEntity,
    preloadedVersion?: MJArtifactVersionEntity
    // Populate display data from query result
    // These fields come from GetConversationComplete query
    this.conversationDetailId = queryResult.ConversationDetailID;
    this.direction = queryResult.Direction;
    this.artifactVersionId = queryResult.ArtifactVersionID;
    this.versionNumber = queryResult.VersionNumber;
    this.versionName = queryResult.VersionName || null;
    this.versionDescription = queryResult.VersionDescription || null;
    this.versionCreatedAt = queryResult.VersionCreatedAt ? new Date(queryResult.VersionCreatedAt) : new Date();
    this.artifactId = queryResult.ArtifactID;
    this.artifactName = queryResult.ArtifactName;
    this.artifactType = queryResult.ArtifactType;
    this.artifactDescription = queryResult.ArtifactDescription || '';
    this.visibility = queryResult.Visibility || 'User';
    // If entities were pre-loaded via batch query, use them immediately
    if (preloadedArtifact) {
      this._artifact = preloadedArtifact;
    if (preloadedVersion) {
      this._version = preloadedVersion;
   * Gets the full Artifact entity, loading it if needed.
   * Multiple concurrent calls share the same loading promise for efficiency.
   * @returns The full MJArtifactEntity with all fields including relationships
  async getArtifact(): Promise<MJArtifactEntity> {
    // If already loading, wait for existing promise to avoid duplicate queries
    if (this._loadPromise) {
      await this._loadPromise;
      return this._artifact!;
    // Start loading both entities (artifact and version) in parallel
    this._loadPromise = this.loadEntities();
   * Gets the full ArtifactVersion entity, loading it if needed.
   * @returns The full MJArtifactVersionEntity including Content field
  async getVersion(): Promise<MJArtifactVersionEntity> {
    if (this._version) {
      return this._version;
      return this._version!;
   * Checks if full entities are already loaded (without triggering load).
   * Useful for conditional logic that wants to avoid triggering a database query.
  get isLoaded(): boolean {
    return this._artifact !== null && this._version !== null;
   * Checks if entities are currently being loaded.
   * Useful for displaying loading indicators.
    return this._isLoading;
   * Loads the full Artifact and ArtifactVersion entities from the database.
   * This method is called automatically when getArtifact() or getVersion() is invoked
   * for the first time. Both entities are loaded in parallel for efficiency.
  private async loadEntities(): Promise<void> {
    if (this._artifact && this._version) {
      return; // Already loaded - nothing to do
      // Load both entities in parallel to minimize database round trips
      const [artifact, version] = await Promise.all([
        this.loadArtifact(md),
        this.loadVersion(md)
      this._artifact = artifact;
      this._version = version;
      console.error('Error lazy-loading artifact:', error);
      this._loadPromise = null; // Clear promise so retry is possible on failure
   * Loads a single Artifact entity by ID
  private async loadArtifact(md: Metadata): Promise<MJArtifactEntity> {
    await artifact.Load(this.artifactId);
   * Loads a single ArtifactVersion entity by ID
  private async loadVersion(md: Metadata): Promise<MJArtifactVersionEntity> {
    await version.Load(this.artifactVersionId);
