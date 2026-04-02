// Import MJ modules
// Import plugin components (note: base component is abstract and NOT declared)
import { JsonArtifactViewerComponent } from './components/plugins/json-artifact-viewer.component';
import { CodeArtifactViewerComponent } from './components/plugins/code-artifact-viewer.component';
import { MarkdownArtifactViewerComponent } from './components/plugins/markdown-artifact-viewer.component';
import { HtmlArtifactViewerComponent } from './components/plugins/html-artifact-viewer.component';
import { SvgArtifactViewerComponent } from './components/plugins/svg-artifact-viewer.component';
import { ComponentArtifactViewerComponent } from './components/plugins/component-artifact-viewer.component';
import { DataRequirementsViewerComponent } from './components/plugins/data-requirements-viewer/data-requirements-viewer.component';
// Import artifact type plugin viewer component
import { ArtifactTypePluginViewerComponent } from './components/artifact-type-plugin-viewer.component';
import { ArtifactVersionHistoryComponent } from './components/artifact-version-history.component';
import { ArtifactViewerPanelComponent } from './components/artifact-viewer-panel.component';
import { ArtifactMessageCardComponent } from './components/artifact-message-card.component';
 * Module for artifact viewer plugin system.
 * Provides components for viewing different types of artifacts (JSON, Code, Markdown, HTML, SVG, Components).
 * Plugins are automatically registered via @RegisterClass decorator and can be instantiated
 * using MJGlobal.Instance.ClassFactory.CreateInstance('PluginClassName').
    ArtifactTypePluginViewerComponent,
    ArtifactViewerPanelComponent,
    ArtifactVersionHistoryComponent,
    ArtifactMessageCardComponent,
    // Custom tab components (used by plugins via dynamic component tabs)
    DataRequirementsViewerComponent
    // Export artifact type plugin viewer
    // Export artifact viewer UI components
    // Export plugin components
    ComponentArtifactViewerComponent
    // Plugins are registered via @RegisterClass decorator on component classes, no providers needed
export class ArtifactsModule {
    // Ensure plugin components are registered on module load by referencing their classes
    // The @RegisterClass decorator on each component handles the actual registration with MJGlobal
