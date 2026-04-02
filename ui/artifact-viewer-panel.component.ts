import { Component, Input, Output, EventEmitter, OnInit, OnChanges, OnDestroy, SimpleChanges, ViewChild, ViewContainerRef, ComponentRef, Type, ChangeDetectorRef } from '@angular/core';
import { UserInfo, Metadata, RunView, LogError, CompositeKey } from '@memberjunction/core';
import { MJArtifactEntity, MJArtifactVersionEntity, MJArtifactVersionAttributeEntity, MJArtifactTypeEntity, MJCollectionEntity, MJCollectionArtifactEntity, ArtifactMetadataEngine, MJConversationEntity, MJConversationDetailArtifactEntity, MJConversationDetailEntity, MJArtifactUseEntity } from '@memberjunction/core-entities';
import { ArtifactTypePluginViewerComponent } from './artifact-type-plugin-viewer.component';
import { ArtifactViewerTab } from './base-artifact-viewer.component';
  selector: 'mj-artifact-viewer-panel',
  templateUrl: './artifact-viewer-panel.component.html',
  styleUrls: ['./artifact-viewer-panel.component.css']
export class ArtifactViewerPanelComponent implements OnInit, OnChanges, OnDestroy {
  @Input() environmentId!: string;
  @Input() versionNumber?: number; // Version to display
  @Input() showSaveToCollection: boolean = true; // Control whether Save to Collection button is shown
  @Input() showHeader: boolean = true; // Control whether the header section is shown
  @Input() showTabs: boolean = true; // Control whether the tab navigation is shown (false = show only Display tab content)
  @Input() showCloseButton: boolean = true; // Control whether the close button is shown in header
  @Input() showMaximizeButton: boolean = true; // Control whether the maximize/restore button is shown in header
  @Input() refreshTrigger?: Subject<{artifactId: string; versionNumber: number}>;
  @Input() viewContext: 'conversation' | 'collection' | null = null; // Where artifact is being viewed
  @Input() contextCollectionId?: string; // If viewing in collection, which collection
  @Input() canShare?: boolean; // Whether user can share this artifact
  @Input() canEdit?: boolean; // Whether user can edit this artifact
  @Input() isMaximized: boolean = false; // Whether the panel is currently maximized
  @Output() saveToCollectionRequested = new EventEmitter<{artifactId: string; excludedCollectionIds: string[]}>();
  @Output() navigateToLink = new EventEmitter<{type: 'conversation' | 'collection'; id: string; artifactId?: string; versionNumber?: number; versionId?: string}>();
  @Output() shareRequested = new EventEmitter<string>(); // Emits artifactId when share is clicked
  @Output() maximizeToggled = new EventEmitter<void>(); // Emits when user clicks maximize/restore button
  @ViewChild(ArtifactTypePluginViewerComponent) pluginViewer?: ArtifactTypePluginViewerComponent;
  public artifact: MJArtifactEntity | null = null;
  public artifactVersion: MJArtifactVersionEntity | null = null;
  public allVersions: MJArtifactVersionEntity[] = [];
  public selectedVersionNumber: number = 1;
  public jsonContent = '';
  public showVersionDropdown = false;
  public artifactCollections: MJCollectionArtifactEntity[] = []; // All collections for ALL versions
  public currentVersionCollections: MJCollectionArtifactEntity[] = []; // Collections containing CURRENT version only
  public primaryCollection: MJCollectionEntity | null = null;
  // Tabbed interface
  public activeTab: string = 'display'; // Changed to string to support dynamic tabs
  public displayMarkdown: string | null = null;
  public displayHtml: string | null = null;
  public versionAttributes: MJArtifactVersionAttributeEntity[] = [];
  private artifactTypeDriverClass: string | null = null;
  // Links tab data
  public originConversation: MJConversationEntity | null = null;
  public allCollections: MJCollectionEntity[] = [];
  public hasAccessToOriginConversation: boolean = false;
  public originConversationVersionId: string | null = null; // Version ID that came from origin conversation
  // Dynamic tabs from plugin
  public get allTabs(): string[] {
    const tabs: string[] = [];
    // Only add Display tab if there's content to display
    if (this.hasDisplayTab) {
      tabs.push('Display');
    // Get plugin tabs directly from plugin instance (no caching needed - plugin always exists)
    if (this.pluginViewer?.pluginInstance?.GetAdditionalTabs) {
      const pluginTabs = this.pluginViewer.pluginInstance.GetAdditionalTabs();
      const pluginTabLabels = pluginTabs.map((t: ArtifactViewerTab) => t.label);
      tabs.push(...pluginTabLabels);
    // Get tabs to remove from plugin (case-insensitive)
    const removals = this.pluginViewer?.pluginInstance?.GetStandardTabRemovals?.() || [];
    const removalsLower = removals.map(r => r.toLowerCase());
    // Add standard tabs (unless plugin removed them)
    if (!removalsLower.includes('json')) {
      tabs.push('JSON');
    if (!removalsLower.includes('details')) {
      tabs.push('Details');
    // Only add Links tab if there are links to show (unless plugin explicitly removes it)
    if (!removalsLower.includes('links') && this.linksToShow.length > 0) {
      tabs.push('Links');
    return tabs;
   * Get the full tab definition for a given tab name.
   * Returns the ArtifactViewerTab which may include component info for custom component tabs.
  public GetTabDefinition(tabName: string): ArtifactViewerTab | null {
    // Check if this is a plugin-provided tab
      const pluginTab = pluginTabs.find((t: ArtifactViewerTab) =>
        t.label.toLowerCase() === tabName.toLowerCase()
      if (pluginTab) {
        return pluginTab;
    // Handle base tabs
    switch (tabName.toLowerCase()) {
        return { label: 'JSON', contentType: 'json', content: this.jsonContent, language: 'json' };
      case 'details':
        return { label: 'Details', contentType: 'html', content: this.displayMarkdown || this.displayHtml || '' };
   * Get resolved tab content for string-based tabs (non-component tabs).
   * For component tabs, use GetTabDefinition() and render the component directly.
  public GetTabContent(tabName: string): { type: string; content: string; language?: string } | null {
    const tabDef = this.GetTabDefinition(tabName);
    if (!tabDef) return null;
    // Component tabs don't have string content
    if (tabDef.contentType === 'component') {
    const content = typeof tabDef.content === 'function'
      ? tabDef.content()
      : tabDef.content || '';
      type: tabDef.contentType,
      language: tabDef.language
   * Check if a tab is a component tab (renders a custom Angular component)
  public IsComponentTab(tabName: string): boolean {
    return tabDef?.contentType === 'component' && !!tabDef.component;
   * Get the component type for a component tab.
   * Returns null if the tab is not a component tab (used for template type safety).
  public GetComponentTabType(tabName: string): Type<any> | null {
    if (tabDef?.contentType === 'component' && tabDef.component) {
      return tabDef.component;
   * Get the component inputs for a component tab
  public GetComponentInputs(tabName: string): Record<string, any> {
    if (!tabDef || tabDef.contentType !== 'component') {
    return typeof tabDef.componentInputs === 'function'
      ? tabDef.componentInputs()
      : tabDef.componentInputs || {};
    private artifactIconService: ArtifactIconService
    // Subscribe to refresh trigger for dynamic version changes
    if (this.refreshTrigger) {
      this.refreshTrigger.pipe(takeUntil(this.destroy$)).subscribe(async (data) => {
        if (data.artifactId === this.artifactId) {
          // Reload all versions to get any new ones
          await this.loadArtifact(data.versionNumber);
    // Load artifact with specified version if provided
    await this.loadArtifact(this.versionNumber);
    // Track that user viewed this artifact
    if (this.artifactVersion?.ID && this.currentUser) {
      this.trackArtifactUsage('Viewed');
      // Also log to User Record Logs for recents feature (fire-and-forget)
      this.recentAccessService.logAccess('MJ: Artifacts', this.artifactId, 'artifact');
    // Reload artifact when artifactId changes
    if (changes['artifactId'] && !changes['artifactId'].firstChange) {
    // Switch to new version when versionNumber changes (but artifactId stays the same)
    if (changes['versionNumber'] && !changes['versionNumber'].firstChange) {
      const newVersionNumber = changes['versionNumber'].currentValue;
      if (newVersionNumber != null) {
        // Check if we already have this version loaded (avoid reload if possible)
        const targetVersion = this.allVersions.find(v => v.VersionNumber === newVersionNumber);
        if (targetVersion) {
          // Just switch to the version we already have
          this.artifactVersion = targetVersion;
          this.selectedVersionNumber = targetVersion.VersionNumber || 1;
          this.jsonContent = this.FormatJSON(targetVersion.Content || '{}');
          // Load version attributes
          await this.loadVersionAttributes();
          // Reload collection associations for this version
          await this.loadCollectionAssociations();
          // Reload links data
          await this.loadLinksData();
          // Need to reload to get this version (shouldn't normally happen)
          await this.loadArtifact(newVersionNumber);
  private async loadArtifact(targetVersionNumber?: number): Promise<void> {
      // Clear links data from previous artifact to prevent stale Links tab
      this.clearLinksData();
      // Load artifact
      this.artifact = await md.GetEntityObject<MJArtifactEntity>('MJ: Artifacts', this.currentUser);
      const loaded = await this.artifact.Load(this.artifactId);
        this.error = 'Failed to load artifact';
      // Load artifact type to check for DriverClass
      await this.loadArtifactType();
      // Load ALL versions
        ExtraFilter: `ArtifactID='${this.artifactId}'`,
        this.allVersions = result.Results;
        // If target version specified, try to load it
        if (targetVersionNumber) {
          const targetVersion = this.allVersions.find(v => v.VersionNumber === targetVersionNumber);
            // Target version not found, default to latest
            this.artifactVersion = result.Results[0];
            this.selectedVersionNumber = this.artifactVersion.VersionNumber || 1;
            this.jsonContent = this.FormatJSON(this.artifactVersion.Content || '{}');
          // No target version, default to latest version (first in DESC order)
        // Load collection associations
        // Load links data
        this.error = 'No artifact version found';
      this.error = 'Error loading artifact: ' + (err as Error).message;
   * Clear all links-related data to prevent stale data when switching artifacts
  private clearLinksData(): void {
    this.allCollections = [];
    this.originConversation = null;
    this.hasAccessToOriginConversation = false;
    this.originConversationVersionId = null;
  private async loadArtifactType(): Promise<void> {
    if (!this.artifact?.Type) {
      await ArtifactMetadataEngine.Instance.Config(false, this.currentUser);
      const artifactType = ArtifactMetadataEngine.Instance.FindArtifactType(this.artifact.Type);
      if (artifactType) {
        this.artifactTypeDriverClass = await this.resolveDriverClassForType(artifactType);
      // Don't fail the whole load if we can't get the artifact type
  private async loadVersionAttributes(): Promise<void> {
    if (!this.artifactVersion) return;
      const result = await rv.RunView<MJArtifactVersionAttributeEntity>({
        EntityName: 'MJ: Artifact Version Attributes',
        ExtraFilter: `ArtifactVersionID='${this.artifactVersion.ID}'`,
        this.versionAttributes = result.Results;
        // Check for displayMarkdown or displayHtml attributes
        const displayMarkdownAttr = this.versionAttributes.find(a => a.Name?.toLowerCase() === 'displaymarkdown');
        const displayHtmlAttr = this.versionAttributes.find(a => a.Name?.toLowerCase() === 'displayhtml');
        // Parse values - they might be JSON-encoded strings
        this.displayMarkdown = this.parseAttributeValue(displayMarkdownAttr?.Value);
        this.displayHtml = this.parseAttributeValue(displayHtmlAttr?.Value);
        // Clean up double-escaped characters in HTML (from LLM generation)
        if (this.displayHtml) {
          this.displayHtml = this.cleanEscapedCharacters(this.displayHtml);
        // Set active tab to the first available tab
        this.setActiveTabToFirstAvailable();
      console.error('Error loading version attributes:', err);
    if (this.artifactVersion?.Name) {
      return this.artifactVersion.Name;
    return this.artifact?.Name || 'Artifact';
  get displayDescription(): string | null {
    if (this.artifactVersion?.Description) {
      return this.artifactVersion.Description;
    return this.artifact?.Description || null;
  get hasDisplayTab(): boolean {
    // Show Display tab if:
    // 1. We have a plugin AND it reports having content to display, OR
    // 2. We have displayMarkdown or displayHtml attributes from extract rules
    // Note: hasDisplayContent defaults to false in base class, so plugins must
    // explicitly opt-in by overriding to return true when they have content.
    // This prevents showing Display tab before plugin loads or when plugin has no content.
    const pluginHasContent = this.pluginViewer?.pluginInstance?.hasDisplayContent ?? false;
    return pluginHasContent || !!this.displayMarkdown || !!this.displayHtml;
  get hasPlugin(): boolean {
    // Check if the artifact type has a DriverClass configured
    // If DriverClass is set, we have a plugin available
    return !!this.artifactTypeDriverClass;
  get hasJsonTab(): boolean {
    // Query plugin directly (no cache needed - plugin always exists when it should)
    const pluginInstance = this.pluginViewer?.pluginInstance;
    return pluginInstance?.parentShouldShowRawContent || false;
  get artifactTypeName(): string {
    return this.artifact?.Type || '';
  get contentType(): string | undefined {
    // Try to get content type from artifact type or attributes
    const contentTypeAttr = this.versionAttributes.find(a => a.Name?.toLowerCase() === 'contenttype');
    return contentTypeAttr?.Value || undefined;
  get filteredAttributes(): MJArtifactVersionAttributeEntity[] {
    // Filter out displayMarkdown and displayHtml as they're shown in the Display tab
    return this.versionAttributes.filter(attr => {
      const name = attr.Name?.toLowerCase();
      return name !== 'displaymarkdown' && name !== 'displayhtml';
  setActiveTab(tab: 'display' | 'json' | 'details' | 'links'): void {
   * Sets the active tab to the first available tab in the list.
   * Called when tabs change or when the currently active tab becomes unavailable.
  private setActiveTabToFirstAvailable(): void {
    const tabs = this.allTabs;
    if (tabs.length > 0) {
      // If current tab is still available, keep it; otherwise switch to first
      const currentTabStillAvailable = tabs.some(t => t.toLowerCase() === this.activeTab.toLowerCase());
      if (!currentTabStillAvailable) {
        this.activeTab = tabs[0].toLowerCase();
      // Fallback to details if no tabs available (shouldn't happen)
      this.activeTab = 'details';
   * Called when the plugin viewer finishes loading.
   * Selects the first available tab now that plugin tabs are available.
  onPluginLoaded(): void {
    // Now that plugin is loaded, we have accurate tab information
    // Always select the first tab since this is the initial load
  private parseAttributeValue(value: string | null | undefined): string | null {
    // Check if it's a JSON-encoded string (starts and ends with quotes)
    if (value.startsWith('"') && value.endsWith('"')) {
        console.warn('Failed to parse attribute value as JSON:', e);
   * Clean up double-escaped characters that appear in LLM-generated HTML
   * Removes literal "\\n" and "\\t" which cause rendering issues
  private cleanEscapedCharacters(html: string): string {
    // Remove escaped newlines (\\n becomes nothing)
    // HTML doesn't need whitespace for formatting, and these cause display issues
    let cleaned = html.replace(/\\n/g, '');
    // Remove escaped tabs
    cleaned = cleaned.replace(/\\t/g, '');
    // Remove double-escaped tabs
    cleaned = cleaned.replace(/\\\\t/g, '');
    // Remove double-escaped newlines
    cleaned = cleaned.replace(/\\\\n/g, '');
  private async loadCollectionAssociations(): Promise<void> {
    if (!this.artifactId) return;
      // Load ALL collection associations for ALL versions of this artifact
      const result = await rv.RunView<MJCollectionArtifactEntity>({
        EntityName: 'MJ: Collection Artifacts',
        ExtraFilter: `ArtifactVersionID IN (
          SELECT ID FROM [__mj].[vwArtifactVersions] WHERE ArtifactID='${this.artifactId}'
        this.artifactCollections = result.Results;
        // Filter to get only collections containing the CURRENT version
        const currentVersionId = this.artifactVersion?.ID;
        if (currentVersionId) {
          // Type-safe comparison: ensure both IDs are strings and match exactly
          const currentIdStr = String(currentVersionId).toLowerCase();
          this.currentVersionCollections = result.Results.filter(ca => {
            const versionIdStr = String(ca.ArtifactVersionID || '').toLowerCase();
            return versionIdStr && currentIdStr && versionIdStr === currentIdStr;
          this.currentVersionCollections = [];
        // Load the primary collection details if exists
        if (this.artifactCollections.length > 0) {
          const collectionId = this.artifactCollections[0].CollectionID;
          this.primaryCollection = await md.GetEntityObject<MJCollectionEntity>('MJ: Collections', this.currentUser);
          await this.primaryCollection.Load(collectionId);
          this.primaryCollection = null;
      console.error('Error loading collection associations:', err);
  get isInCollection(): boolean {
    return this.currentVersionCollections.length > 0;
   * Get collection IDs that already contain the current version
   * Used to exclude them from the save picker
  get currentVersionCollectionIds(): string[] {
    return this.currentVersionCollections.map(ca => ca.CollectionID);
  onCopyToClipboard(): void {
    // Get content from the currently active tab instead of always copying jsonContent
    const tabData = this.GetTabContent(this.activeTab);
    if (tabData?.content) {
      navigator.clipboard.writeText(tabData.content);
    } else if (this.jsonContent) {
      // Fallback to jsonContent if tab content not found
      navigator.clipboard.writeText(this.jsonContent);
  onCopyDisplayContent(): void {
    const content = this.displayHtml || this.displayMarkdown;
      navigator.clipboard.writeText(content).catch(err => {
  onPrintDisplayContent(): void {
    // Try to delegate to the plugin viewer's print method
    if (this.pluginViewer?.pluginInstance) {
      const plugin = this.pluginViewer.pluginInstance as any;
      if (typeof plugin.printHtml === 'function') {
        plugin.printHtml();
    // Fallback: create a temporary print window with displayHtml or displayMarkdown
      const printWindow = window.open('', '_blank');
      if (printWindow) {
          printWindow.document.write(content);
        } else if (this.displayMarkdown) {
          // Wrap markdown in basic HTML for printing
          printWindow.document.write(`
              <title>Print</title>
                body { font-family: sans-serif; padding: 20px; }
                pre { background: #f5f5f5; padding: 10px; border-radius: 4px; }
              <pre>${content}</pre>
        printWindow.document.close();
        printWindow.focus();
          printWindow.print();
          printWindow.close();
        }, 250);
  toggleVersionDropdown(): void {
    if (this.allVersions.length > 1) {
      this.showVersionDropdown = !this.showVersionDropdown;
  async selectVersion(version: MJArtifactVersionEntity): Promise<void> {
    this.artifactVersion = version;
    this.selectedVersionNumber = version.VersionNumber || 1;
    this.jsonContent = this.FormatJSON(version.Content || '{}');
    this.showVersionDropdown = false;
    // Load attributes for the selected version
    // CRITICAL FIX: Reload collection associations for this version
    // This ensures bookmark button and Links tab reflect the correct state
    // Also reload links data to update conversation/collection links
  async onSaveToLibrary(): Promise<void> {
    // Always show the collection picker modal
    // Artifacts can be saved to multiple collections
    this.saveToCollectionRequested.emit({
      artifactId: this.artifactId,
      excludedCollectionIds: this.excludedCollectionIds
  get excludedCollectionIds(): string[] {
    // Return IDs of collections that already contain the CURRENT VERSION
    // This allows saving different versions to the same collection
    const excluded = this.currentVersionCollections
      .filter(ca => ca.CollectionID)
      .map(ca => String(ca.CollectionID));
    return excluded;
   * Called by parent component after user selects collections in the picker.
   * Saves the artifact to the selected collections.
  async saveToCollections(collectionIds: string[]): Promise<boolean> {
    if (!this.artifactId || collectionIds.length === 0) {
      // Get current version ID - save the version being viewed
      if (!currentVersionId) {
        console.error('No current version ID available');
          'Cannot save: no version selected',
      // Save artifact version to each selected collection
      for (const collectionId of collectionIds) {
        // Double check this exact version doesn't already exist in the collection
        const existingResult = await rv.RunView<MJCollectionArtifactEntity>({
          ExtraFilter: `CollectionID='${collectionId}' AND ArtifactVersionID='${currentVersionId}'`,
        if (existingResult.Success && existingResult.Results && existingResult.Results.length > 0) {
        // Create junction record with version ID
        const collectionArtifact = await md.GetEntityObject<MJCollectionArtifactEntity>('MJ: Collection Artifacts', this.currentUser);
        collectionArtifact.CollectionID = collectionId;
        collectionArtifact.ArtifactVersionID = currentVersionId;
        collectionArtifact.Sequence = 0;
        const saved = await collectionArtifact.Save();
          console.error(`Failed to save artifact version to collection ${collectionId}`);
          `Artifact saved to ${successCount} collection(s) successfully!`,
        // Reload collection associations to update the bookmark icon state
          'Failed to save artifact to any collections',
      console.error('Error saving to collections:', err);
        'Error saving artifact to collections. Please try again.',
   * Load links data: origin conversation and all collections containing this artifact
  private async loadLinksData(): Promise<void> {
    // Clear old links data first to prevent stale data from previous artifact
      // Load all collections containing any version of this artifact
      const collArtifactsResult = await rv.RunView<MJCollectionArtifactEntity>({
      if (collArtifactsResult.Success && collArtifactsResult.Results) {
        // Get unique collection IDs
        const collectionIds = [...new Set(collArtifactsResult.Results.map(ca => ca.CollectionID))];
        if (collectionIds.length > 0) {
          const collectionsFilter = collectionIds.map(id => `ID='${id}'`).join(' OR ');
          const collectionsResult = await rv.RunView<MJCollectionEntity>({
            ExtraFilter: collectionsFilter,
          if (collectionsResult.Success && collectionsResult.Results) {
            this.allCollections = collectionsResult.Results;
      // Load origin conversation (if artifact came from conversation)
      // Artifacts are linked to conversations via ConversationDetailArtifact -> ConversationDetail -> Conversation
      // Get all version IDs for this artifact
      const versionIds = this.allVersions.map(v => v.ID);
      if (versionIds.length > 0) {
        const versionFilter = versionIds.map(id => `ArtifactVersionID='${id}'`).join(' OR ');
        const convDetailArtifactsResult = await rv.RunView<MJConversationDetailArtifactEntity>({
          ExtraFilter: versionFilter,
        if (convDetailArtifactsResult.Success && convDetailArtifactsResult.Results && convDetailArtifactsResult.Results.length > 0) {
          const conversationDetailId = convDetailArtifactsResult.Results[0].ConversationDetailID;
          const artifactVersionId = convDetailArtifactsResult.Results[0].ArtifactVersionID;
          // Store which version came from the origin conversation
          this.originConversationVersionId = artifactVersionId;
          // Load the conversation detail to get the conversation ID
          const conversationDetail = await md.GetEntityObject<MJConversationDetailEntity>('MJ: Conversation Details', this.currentUser);
          const detailLoaded = await conversationDetail.Load(conversationDetailId);
          if (detailLoaded && conversationDetail.ConversationID) {
            const conversation = await md.GetEntityObject<MJConversationEntity>('MJ: Conversations', this.currentUser);
            const loaded = await conversation.Load(conversationDetail.ConversationID);
              this.originConversation = conversation;
              // Check if user has access (is owner or participant)
              const userIsOwner = conversation.UserID === this.currentUser.ID;
              // Check if user is a participant
              const participantResult = await rv.RunView({
                EntityName: 'MJ: Conversation Details',
                ExtraFilter: `ConversationID='${conversation.ID}' AND UserID='${this.currentUser.ID}'`,
              const userIsParticipant = participantResult.Success &&
                                         participantResult.Results &&
                                         participantResult.Results.length > 0;
              this.hasAccessToOriginConversation = userIsOwner || userIsParticipant;
      console.error('Error loading links data:', error);
  get linksToShow(): Array<{type: 'conversation' | 'collection'; id: string; name: string; hasAccess: boolean}> {
    const links: Array<{type: 'conversation' | 'collection'; id: string; name: string; hasAccess: boolean}> = [];
    // Get current version ID being viewed
    // RULE: In conversation context, show ONLY collection links
    // RULE: In collection context, show ONLY conversation links
    if (this.viewContext === 'conversation') {
      // Show all collections containing this artifact (any version)
      for (const collection of this.allCollections) {
          type: 'collection',
          id: collection.ID,
          name: collection.Name,
          hasAccess: true
    } else if (this.viewContext === 'collection') {
      // Show origin conversation if it exists
      // Show for ALL versions of the artifact, not just the original version that was added
      if (this.originConversation) {
          type: 'conversation',
          id: this.originConversation.ID,
          name: this.originConversation.Name || 'Untitled Conversation',
          hasAccess: this.hasAccessToOriginConversation
    // If viewContext is null, show nothing (no links)
    return links;
   * Navigate to a linked conversation or collection
  onNavigateToLink(link: {type: 'conversation' | 'collection'; id: string; name: string; hasAccess: boolean}): void {
    if (!link.hasAccess) {
    // Include artifact ID, version number, and version ID so destination can show the artifact with correct URL
    this.navigateToLink.emit({
      type: link.type,
      id: link.id,
      versionNumber: this.selectedVersionNumber,
      versionId: this.artifactVersion?.ID
  onShare(): void {
    this.shareRequested.emit(this.artifactId);
  onMaximizeToggle(): void {
    this.maximizeToggled.emit();
   * Handle entity record open request from artifact viewer plugin (React component)
   * Propagates the event up to parent components
   * Returns the first DriverClass found, or null if none found in the hierarchy.
  private async resolveDriverClassForType(artifactType: MJArtifactTypeEntity): Promise<string | null> {
        return await this.resolveDriverClassForType(parentType);
    // Reached root with no DriverClass
      const artifactType = await md.GetEntityObject<MJArtifactTypeEntity>('MJ: Artifact Types', this.currentUser);
   * Format JSON content using ParseJSONRecursive for deep parsing and formatting
  private FormatJSON(content: string): string {
      // First parse the JSON string to an object
      const obj = JSON.parse(content);
      // Then use ParseJSONRecursive to extract any inline JSON strings
      const parsed = ParseJSONRecursive(obj, parseOptions);
      // Finally stringify with formatting
      // Fallback to simple parse/stringify if ParseJSONRecursive fails
      } catch (e2) {
        // If even simple parse fails, return as-is
   * Get icon class for a tab
  public GetTabIcon(tabName: string): string | null {
    // Base tabs
    const baseIcons: Record<string, string> = {
      'Display': 'fas fa-eye',
      'Code': 'fas fa-code',
      'JSON': 'fas fa-file-code',
      'Details': 'fas fa-info-circle',
      'Links': 'fas fa-link'
    if (baseIcons[tabName]) {
      return baseIcons[tabName];
    // Check plugin tabs
    const plugin = this.pluginViewer?.pluginInstance;
    if (plugin?.GetAdditionalTabs) {
      const pluginTab = plugin.GetAdditionalTabs().find((t: ArtifactViewerTab) => t.label === tabName);
      if (pluginTab?.icon) {
        return 'fas ' + pluginTab.icon; // Ensure full Font Awesome class
  public SetActiveTab(tabName: string): void {
    this.activeTab = tabName.toLowerCase();
   * Track artifact usage event
  private async trackArtifactUsage(usageType: 'Viewed' | 'Opened' | 'Shared' | 'Saved' | 'Exported'): Promise<void> {
      if (!this.artifactVersion?.ID || !this.currentUser?.ID) {
      const usage = await md.GetEntityObject<MJArtifactUseEntity>('MJ: Artifact Uses');
      usage.ArtifactVersionID = this.artifactVersion.ID;
      usage.UserID = this.currentUser.ID;
      usage.UsageType = usageType;
      usage.UsageContext = JSON.stringify({
        viewContext: this.viewContext,
        contextCollectionId: this.contextCollectionId,
      // Save asynchronously - don't block UI
      usage.Save().catch(error => {
        console.error('Failed to track artifact usage:', error);
      console.error('Error tracking artifact usage:', error);
    if (!this.artifact) return 'fa-file';
    return this.artifactIconService.getArtifactIcon(this.artifact);
