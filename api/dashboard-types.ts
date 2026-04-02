 * Types and interfaces for the Dashboard Viewer component.
 * These define the configuration schema for metadata-driven dashboards.
 * DESIGN PRINCIPLE: Single Source of Truth
 * - Golden Layout's native config is the ground truth for layout AND panel data
 * - Full DashboardPanel objects are stored in componentState within the layout
 * - No redundant arrays or conversion functions needed
// Dashboard Configuration
 * Root configuration for a dashboard.
 * Stored in Dashboard.UIConfigDetails as JSON.
 * The layout contains EVERYTHING - both geometry AND panel configurations.
 * Panel data is embedded in each component's componentState within the layout tree.
     * Golden Layout configuration - THE SINGLE SOURCE OF TRUTH
     * Contains both layout geometry AND panel configurations (in componentState)
     * Stored as GL's native format for lossless persistence
    layout: ResolvedLayoutConfig | null;
    /** Dashboard-level settings (not per-panel) */
    settings: DashboardSettings;
 * Dashboard-level settings
export interface DashboardSettings {
    /** Visual theme */
    /** Show panel headers with title/actions */
    showHeaders: boolean;
    /** Allow panels to pop out into separate windows */
    enablePopout: boolean;
    /** Allow panels to be maximized */
    enableMaximize: boolean;
    /** Allow users to rearrange panels */
    enableDragDrop: boolean;
    /** Allow users to resize panels */
    enableResize: boolean;
 * Default dashboard settings
export const DEFAULT_DASHBOARD_SETTINGS: DashboardSettings = {
    showHeaders: true,
    enablePopout: false,
    enableMaximize: true,
    enableDragDrop: true,
    enableResize: true
// Panel Configuration (stored in componentState)
 * Complete panel data stored in Golden Layout's componentState.
 * This is the ONLY place panel configuration lives.
export interface DashboardPanel {
    /** Unique panel identifier */
    /** Optional icon class (Font Awesome) */
    /** Reference to DashboardPartType.ID */
    partTypeId: string;
    /** Type-specific configuration */
    /** Persisted panel state (e.g., scroll position, selections) */
    state?: Record<string, unknown>;
 * Generic panel configuration interface.
 * Each panel type (View, Query, Artifact, WebURL, etc.) defines its own config shape.
 * The `type` field must match the DashboardPartType.Name from metadata.
 * This is intentionally generic to support pluggable panel types - new types can be
 * added via metadata without requiring code changes to this interface.
export interface PanelConfig {
    /** Type discriminator - must match DashboardPartType.Name */
    /** Any additional configuration properties specific to the panel type */
// Events
 * Event emitted when a panel interacts with content
export interface PanelInteractionEvent {
    /** Panel that emitted the event */
    panelId: string;
    /** Type of interaction */
    interactionType: 'record-select' | 'record-open' | 'entity-link' | 'query-execute' | 'custom';
    /** Interaction payload */
 * Event emitted when dashboard configuration changes
export interface DashboardConfigChangedEvent {
    /** Updated configuration */
    config: DashboardConfig;
    /** What changed */
    changeType: 'panel-added' | 'panel-removed' | 'panel-config' | 'layout' | 'settings';
 * Event emitted when layout changes (resize, move, etc.)
    /** Updated layout - Golden Layout's native format */
    layout: ResolvedLayoutConfig;
    changeType: 'resize' | 'move' | 'stack' | 'close' | 'maximize';
// Navigation Request Events
 * Union type for all navigation requests from dashboard parts.
 * These are high-level abstractions - the parent component handles routing.
export type DashboardNavRequest =
    | OpenEntityRecordNavRequest
    | OpenDashboardNavRequest
    | OpenQueryNavRequest
    | OpenApplicationNavRequest;
 * Base interface for navigation requests
export interface BaseNavRequest {
    /** Type discriminator for the navigation request */
    /** Source panel ID that initiated the request */
    sourcePanelId: string;
    /** Whether to open in a new tab/window */
    openInNewTab?: boolean;
 * Request to open a specific entity record by entity name and ID
export interface OpenEntityRecordNavRequest extends BaseNavRequest {
    type: 'OpenEntityRecord';
    /** Entity name */
    /** Record primary key value (URL segment format) */
    /** Optional: Open in edit or view mode */
 * Request to navigate to another dashboard
export interface OpenDashboardNavRequest extends BaseNavRequest {
    type: 'OpenDashboard';
    /** Dashboard ID */
    dashboardId: string;
    /** Optional: Dashboard category to navigate to first */
 * Request to navigate to a query
export interface OpenQueryNavRequest extends BaseNavRequest {
    type: 'OpenQuery';
    /** Query ID */
    queryId: string;
    /** Optional: Pre-filled parameter values */
    parameters?: Record<string, unknown>;
    /** Optional: Auto-execute on open */
    autoExecute?: boolean;
 * Request to navigate to another application
export interface OpenApplicationNavRequest extends BaseNavRequest {
    type: 'OpenApplication';
    /** Application name or ID */
    /** Optional: Specific resource/tab within the app */
 * Event wrapper for navigation requests emitted from dashboard parts
export interface DashboardNavRequestEvent {
    /** The navigation request */
    request: DashboardNavRequest;
    /** Panel data at time of request (for context) */
    panel: DashboardPanel;
 * Result of configuration validation
export interface ValidationResult {
    /** Validation errors */
    errors: ValidationError[];
    /** Validation warnings */
    warnings: ValidationWarning[];
export interface ValidationError {
    field: string;
export interface ValidationWarning {
// Utility Functions
 * Creates a default dashboard configuration with empty layout
export function createDefaultDashboardConfig(): DashboardConfig {
        layout: null,
        settings: { ...DEFAULT_DASHBOARD_SETTINGS }
 * Generates a unique panel ID
export function generatePanelId(): string {
    return `panel-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`;
 * Extract all panels from a Golden Layout config.
 * Walks the layout tree and extracts DashboardPanel from each component's componentState.
export function extractPanelsFromLayout(layout: ResolvedLayoutConfig | null): DashboardPanel[] {
    if (!layout?.root) {
    const panels: DashboardPanel[] = [];
    const extractFromNode = (node: ResolvedLayoutConfig['root']): void => {
        // If this is a component, extract the panel from componentState
            const componentNode = node as unknown as { componentState?: DashboardPanel };
            if (componentNode.componentState?.id) {
                panels.push(componentNode.componentState);
                extractFromNode(child);
    extractFromNode(layout.root);
    return panels;
 * Find a panel in the layout by ID
export function findPanelInLayout(layout: ResolvedLayoutConfig | null, panelId: string): DashboardPanel | null {
    const panels = extractPanelsFromLayout(layout);
    return panels.find(p => p.id === panelId) || null;
