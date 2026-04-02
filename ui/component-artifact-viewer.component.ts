import { Component, ViewChild, AfterViewInit, OnInit, OnChanges, SimpleChanges, Output, EventEmitter } from '@angular/core';
import { RegisterClass, SafeJSONParse } from '@memberjunction/global';
import { BaseArtifactViewerPluginComponent, ArtifactViewerTab } from '../base-artifact-viewer.component';
import { MJReactComponent, AngularAdapterService } from '@memberjunction/ng-react';
import { BuildComponentCompleteCode, ComponentSpec } from '@memberjunction/interactive-component-types';
import { DataRequirementsViewerComponent } from './data-requirements-viewer/data-requirements-viewer.component';
 * Viewer component for interactive Component artifacts (React-based UI components)
 * - Live component preview with React rendering
 * - Dynamic tabs for component metadata (via GetAdditionalTabs)
 * - Provides tabs for: Code, Functional Requirements, Technical Design, Data Requirements
  selector: 'mj-component-artifact-viewer',
  templateUrl: './component-artifact-viewer.component.html',
  styleUrls: ['./component-artifact-viewer.component.css']
@RegisterClass(BaseArtifactViewerPluginComponent, 'ComponentArtifactViewerPlugin')
export class ComponentArtifactViewerComponent extends BaseArtifactViewerPluginComponent implements OnInit, AfterViewInit, OnChanges {
  @ViewChild('reactComponent') reactComponent?: MJReactComponent;
  @Output() tabsChanged = new EventEmitter<void>();
  // Component data
  public component: ComponentSpec | null = null;
  public componentCode: string = "";
  public componentName: string = '';
  public get resolvedComponentSpec(): ComponentSpec | null {
    return this.reactComponent?.resolvedComponentSpec || this.component;
  public hasError = false;
  public errorDetails = '';
   * Returns true only if the component has code that can be rendered.
   * IMPORTANT: Uses this.component (synchronously loaded from artifact JSON)
   * instead of resolvedComponentSpec (which depends on async React loading).
   * This ensures hasDisplayContent returns correct value immediately when
   * pluginLoaded fires, before React component finishes loading.
    // Use this.component directly - it's available synchronously after loadComponentSpec()
    return !!this.component?.namespace || !!this.component?.code
  constructor(private adapter: AngularAdapterService) {
    // When artifactVersion input changes, reload the component spec
    if (changes['artifactVersion']) {
      this.loadComponentSpec();
      // Notify parent that tabs may have changed (on subsequent changes)
      if (!changes['artifactVersion'].firstChange) {
        this.tabsChanged.emit();
   * Synchronously load the component spec from artifact content.
   * This is intentionally synchronous so that tabs are available immediately
   * when the parent queries GetAdditionalTabs() after pluginLoaded fires.
  private loadComponentSpec(): void {
      if (this.artifactVersion?.Content) {
        this.component = SafeJSONParse(this.artifactVersion.Content) as ComponentSpec;
        this.extractComponentParts();
        throw new Error('Artifact content is empty');
      console.error('Failed to load component spec:', error);
      this.errorMessage = 'Failed to load component';
      this.errorDetails = error instanceof Error ? error.message : String(error);
   * Component initialization.
   * Note: loadComponentSpec() is called in ngOnChanges which runs before ngOnInit,
   * ensuring tabs are available when pluginLoaded fires.
   * The async adapter initialization happens here and doesn't block tab availability.
    // Initialize Angular adapter for React components (async operation)
      await this.adapter.initialize();
      console.error('Failed to initialize Angular adapter:', error);
      this.errorMessage = 'Failed to initialize component runtime';
    // Component initialization happens automatically via mj-react-component
   * Provide additional tabs for viewing component metadata
  public GetAdditionalTabs(): ArtifactViewerTab[] {
    const tabs: ArtifactViewerTab[] = [];
    const resolvedComponent = this.resolvedComponentSpec;
    if (!resolvedComponent) {
    // Functional Requirements tab
    if (resolvedComponent.functionalRequirements) {
      tabs.push({
        label: 'Functional',
        icon: 'fa-clipboard-list',
        contentType: 'markdown',
        content: resolvedComponent.functionalRequirements
    // Technical Design tab
    if (resolvedComponent.technicalDesign) {
        label: 'Technical',
        icon: 'fa-wrench',
        content: resolvedComponent.technicalDesign
    // Data Requirements tab - uses custom component for rich visualization
    if (resolvedComponent.dataRequirements) {
        label: 'Data',
        icon: 'fa-database',
        contentType: 'component',
        component: DataRequirementsViewerComponent,
        componentInputs: { dataRequirements: resolvedComponent.dataRequirements }
    // Code tab (lazy-loaded) - only show if there's actual code
    if (resolvedComponent.code && resolvedComponent.code.trim()) {
        label: 'Code',
        icon: 'fa-code',
        contentType: 'code',
        language: 'typescript',
        content: () => BuildComponentCompleteCode(resolvedComponent)
    // Spec tab - Shows fully resolved component spec in JSON format (rightmost)
      label: 'Spec',
      icon: 'fa-file-code',
      contentType: 'json',
      content: () => JSON.stringify(resolvedComponent, null, 2),
      language: 'json'
   * Remove standard JSON tab since we provide "Resolved JSON" custom tab
   * The custom tab shows the fully resolved component spec instead of raw artifact JSON
  public GetStandardTabRemovals(): string[] {
    return ['JSON'];
  private extractComponentParts(): void {
    if (this.resolvedComponentSpec?.name) {
      this.componentName = this.resolvedComponentSpec.name;
    if (this.resolvedComponentSpec?.code) {
      this.componentCode = BuildComponentCompleteCode(this.resolvedComponentSpec);
  onComponentEvent(event: unknown): void {
    console.log('Component event:', event);
    // Handle error events from React component
    if (event && typeof event === 'object' && 'type' in event && event.type === 'error') {
      const errorEvent = event as { type: 'error'; payload: { error: string; source: string } };
      this.errorMessage = 'Component Failed to Load';
      this.errorDetails = errorEvent.payload.error || 'Unknown error occurred while loading the component';
   * Handle entity record open request from React component
  onOpenEntityRecord(event: {entityName: string; key: CompositeKey}): void {
    // Transform to use 'compositeKey' name for consistency with Angular components
      compositeKey: event.key
