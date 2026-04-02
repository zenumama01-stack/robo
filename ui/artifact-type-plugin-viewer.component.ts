import { MJArtifactVersionEntity, MJArtifactTypeEntity, ArtifactMetadataEngine } from '@memberjunction/core-entities';
import { Metadata, LogError, RunView, CompositeKey } from '@memberjunction/core';
import { IArtifactViewerComponent } from '../interfaces/artifact-viewer-plugin.interface';
import { BaseArtifactViewerPluginComponent } from './base-artifact-viewer.component';
 * Artifact type plugin viewer that loads the appropriate plugin based on the artifact's DriverClass.
 * Uses MJGlobal.Instance.ClassFactory.CreateInstance() to dynamically load viewer plugins.
  selector: 'mj-artifact-type-plugin-viewer',
    <div class="artifact-type-plugin-viewer">
          <span>Loading artifact viewer...</span>
          @if (errorTitle) {
            <div class="error-title">{{ errorTitle }}</div>
          <div class="error-details">{{ error }}</div>
            <div class="error-tech-details">{{ errorDetails }}</div>
      <ng-container #viewerContainer></ng-container>
    .artifact-type-plugin-viewer {
    .artifact-type-plugin-viewer ::ng-deep > * {
    .error-state .error-title {
    .error-state .error-details {
    .error-state .error-tech-details {
export class ArtifactTypePluginViewerComponent implements OnInit, OnChanges {
  @Input() artifactVersion!: MJArtifactVersionEntity;
  @Input() artifactTypeName!: string;
  @Input() contentType?: string;
  @Input() height?: string;
  @Input() readonly: boolean = true;
  @Input() cssClass?: string;
  @Output() openEntityRecord = new EventEmitter<{entityName: string; compositeKey: CompositeKey}>();
  @Output() pluginLoaded = new EventEmitter<void>();
  @ViewChild('viewerContainer', { read: ViewContainerRef, static: true })
  viewerContainer!: ViewContainerRef;
  public errorTitle: string | null = null;
  private componentRef: ComponentRef<any> | null = null;
   * Get the loaded plugin instance (if available)
  public get pluginInstance(): BaseArtifactViewerPluginComponent | null {
    return this.componentRef?.instance as BaseArtifactViewerPluginComponent || null;
    await this.loadViewer();
    if ((changes['artifactVersion'] || changes['artifactTypeName']) &&
        !changes['artifactVersion']?.firstChange) {
    this.destroyCurrentViewer();
   * Load the appropriate viewer plugin for the artifact
  private async loadViewer(): Promise<void> {
      this.errorTitle = null;
      if (!this.artifactVersion) {
        this.setError(
          'Missing Artifact Data',
          'Unable to display this artifact because the version information is missing.',
          'artifactVersion is null or undefined'
      if (!this.artifactTypeName) {
          'Missing Artifact Type',
          'Unable to display this artifact because the type information is missing.',
          'artifactTypeName is empty'
      // Get the artifact type entity to find the DriverClass
      const artifactType = await this.getArtifactType();
      if (!artifactType) {
          'Unknown Artifact Type',
          `The artifact type "${this.artifactTypeName}" is not recognized. This might be a custom type that hasn't been properly configured.`,
          `Artifact type "${this.artifactTypeName}" not found in metadata`
      // Resolve DriverClass by traversing parent hierarchy if needed
      const driverClass = await this.resolveDriverClass(artifactType);
          'No Viewer Available',
          `This artifact type (${this.artifactTypeName}) doesn't have a viewer component configured. The artifact content may need to be viewed in the JSON tab.`,
          `No DriverClass in hierarchy and content is not valid JSON`
      // Get the component type using MJGlobal ClassFactory
      // CreateInstance returns the registered component class for the given DriverClass key
      const tempInstance = MJGlobal.Instance.ClassFactory.CreateInstance<BaseArtifactViewerPluginComponent>(
        BaseArtifactViewerPluginComponent,
      if (!tempInstance) {
          'Viewer Component Not Found',
          `The viewer component "${driverClass}" is not registered in the application. This usually means the required package or module hasn't been loaded.`,
          `Component "${driverClass}" not found in ClassFactory registry. Ensure it's registered with @RegisterClass(BaseArtifactViewerPluginComponent, '${driverClass}').`
      // Get the component type from the instance
      const componentType = tempInstance.constructor as Type<BaseArtifactViewerPluginComponent>;
      // Destroy previous viewer if exists
      // Create and configure the viewer component
      this.componentRef = this.viewerContainer.createComponent(componentType);
      // Set inputs using setInput() which properly triggers ngOnChanges
      // This is critical for plugins like ComponentArtifactViewerComponent that
      // need to process the artifactVersion in ngOnChanges before pluginLoaded fires
      this.componentRef.setInput('artifactVersion', this.artifactVersion);
      if (this.height !== undefined) {
        this.componentRef.setInput('height', this.height);
      if (this.readonly !== undefined) {
        this.componentRef.setInput('readonly', this.readonly);
      if (this.cssClass !== undefined) {
        this.componentRef.setInput('cssClass', this.cssClass);
      if (this.contentType !== undefined) {
        this.componentRef.setInput('contentType', this.contentType);
      // Subscribe to openEntityRecord event if the plugin emits it
      const componentInstance = this.componentRef.instance;
      if ((componentInstance as any).openEntityRecord) {
        (componentInstance as any).openEntityRecord.subscribe((event: {entityName: string; compositeKey: CompositeKey}) => {
          this.openEntityRecord.emit(event);
      this.componentRef.changeDetectorRef.detectChanges();
      // Notify parent that plugin has loaded (for tab selection timing)
      this.pluginLoaded.emit();
      console.error('Error loading artifact viewer:', err);
      const errorMessage = err instanceof Error ? err.message : String(err);
      const errorStack = err instanceof Error && err.stack ? err.stack : undefined;
        'Failed to Load Viewer',
        'An unexpected error occurred while loading the artifact viewer. Please try refreshing the page or contact support if the problem persists.',
        errorStack || errorMessage
   * Set a structured error message with title, user-friendly description, and technical details
  private setError(title: string, userMessage: string, technicalDetails: string): void {
    this.errorTitle = title;
    this.error = userMessage;
    this.errorDetails = technicalDetails;
   * Get the artifact type entity for the current artifact using the cached ArtifactMetadataEngine
  private async getArtifactType(): Promise<MJArtifactTypeEntity | null> {
      // Use the cached metadata engine instead of querying the database
      const artifactType = ArtifactMetadataEngine.Instance.FindArtifactType(this.artifactTypeName);
      return artifactType || null;
      console.error('Error loading artifact type:', err);
   * Resolves the DriverClass for an artifact type by traversing up the parent hierarchy.
   * Falls back to JSON viewer if content is valid JSON and no DriverClass is found.
   * @param artifactType The artifact type to resolve the DriverClass for
   * @returns The DriverClass string, or null if none found and no JSON fallback available
  private async resolveDriverClass(artifactType: MJArtifactTypeEntity): Promise<string | null> {
    // Check if current artifact type has a DriverClass
    if (artifactType.DriverClass) {
      console.log(`✅ Found DriverClass '${artifactType.DriverClass}' on artifact type '${artifactType.Name}'`);
      return artifactType.DriverClass;
    // No DriverClass on current type - check if it has a parent
    if (artifactType.ParentID) {
      console.log(`🔍 No DriverClass on '${artifactType.Name}', checking parent...`);
      const parentType = await this.getArtifactTypeById(artifactType.ParentID);
      if (parentType) {
        // Recursively check parent
        return await this.resolveDriverClass(parentType);
        console.warn(`⚠️ Parent artifact type '${artifactType.ParentID}' not found`);
    // Reached root with no DriverClass - check for JSON fallback
    console.log(`📄 No DriverClass found in hierarchy for '${artifactType.Name}', checking JSON fallback...`);
    return this.checkJsonFallback();
   * Loads an artifact type by ID
  private async getArtifactTypeById(id: string): Promise<MJArtifactTypeEntity | null> {
      const artifactType = await md.GetEntityObject<MJArtifactTypeEntity>('MJ: Artifact Types');
      const loaded = await artifactType.Load(id);
        return artifactType;
      console.error('Error loading artifact type by ID:', err);
   * Checks if the artifact content is valid JSON and returns the JSON viewer plugin if so
  private checkJsonFallback(): string | null {
    if (!this.artifactVersion || !this.artifactVersion.Content) {
      console.log('❌ No content available for JSON fallback');
      // Try to parse the content as JSON
      JSON.parse(this.artifactVersion.Content);
      console.log('✅ Content is valid JSON, using JsonArtifactViewerPlugin as fallback');
      return 'JsonArtifactViewerPlugin';
      console.log('❌ Content is not valid JSON, no fallback available');
   * Destroy the current viewer component
  private destroyCurrentViewer(): void {
      this.componentRef = null;
    this.viewerContainer.clear();
