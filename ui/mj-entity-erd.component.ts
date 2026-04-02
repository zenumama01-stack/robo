import { ERDDiagramComponent } from './erd-diagram.component';
  ERDZoomEvent
  buildERDDataFromEntities,
  getOriginalEntityFromERDNode
} from '../utils/entity-to-erd-adapter';
 * Event emitted when an entity is selected in the ERD.
 * Set `cancel = true` in your handler to prevent default behavior.
export interface EntitySelectedEvent {
  /** The selected EntityInfo */
  entity: EntityInfo;
  /** The corresponding ERD node */
   * Set to true to cancel default behavior.
   * Default behavior: updates internal selection state.
  cancel?: boolean;
 * Event emitted when requesting to open an entity record.
export interface OpenEntityRecordEvent {
  /** Entity name for the record */
  /** Record ID to open */
  /** The EntityInfo being opened */
   * Default behavior: none (just emits the event).
   * Container is expected to handle navigation.
 * Higher-level MemberJunction Entity ERD component that provides a simplified API
 * for displaying entity relationship diagrams using EntityInfo objects.
 * This component wraps the generic `ERDDiagramComponent` and handles the transformation
 * of MemberJunction EntityInfo objects to the generic ERD format automatically.
 * - **Simple Input**: Just pass `EntityInfo[]` - no manual transformation needed
 * - **Auto-Discovery**: Automatically discovers and displays relationships
 * - **Configurable Depth**: Control how many relationship hops to include
 * - **State Control**: Parent controls selection/state - component doesn't persist
 * ## Usage Modes
 * ### Single Entity Mode (Entity Form)
 * Show one entity and its immediate relationships:
 * ### Multi-Entity Mode (Schema Explorer)
 * Show multiple entities with persistence handled by parent:
 *   [entities]="filteredEntities"
 *   [selectedEntityId]="savedSelectedId"
 *   [depth]="savedDepth"
 *   (entitySelected)="onEntitySelected($event); saveState()"
 *   (stateChange)="onStateChange($event); saveState()"
 * ## Important Design Decisions
 * - **No State Persistence**: This component does NOT persist user state (selection, zoom, etc.).
 *   The parent is responsible for saving/restoring state via the inputs and events.
 * - **No Filtering**: The caller filters entities before passing them. This keeps the component simple.
 * - **Controlled Selection**: Selection is controlled via `selectedEntityId` input. The component
 *   emits `entitySelected` when user clicks, but parent decides what to do.
 * @see ERDDiagramComponent for the underlying generic component
  selector: 'mj-entity-erd',
    <mj-erd-diagram
      [nodes]="erdNodes"
      [selectedNodeId]="selectedEntityId"
      [highlightedNodeIds]="highlightedEntityIds"
      [focusNodeId]="focusEntityId"
      [focusDepth]="focusDepth"
      [config]="config"
      [headerTitle]="headerTitle"
      [isRefreshing]="isRefreshing"
      [readOnly]="readOnly"
      (nodeDoubleClick)="onNodeDoubleClick($event)"
      (nodeSelected)="onNodeSelected($event)"
      (nodeDeselected)="onNodeDeselected()"
      (zoomChange)="onZoomChange($event)"
      (refreshRequested)="onRefreshRequested()">
    </mj-erd-diagram>
export class MJEntityERDComponent implements OnChanges {
  @ViewChild(ERDDiagramComponent) erdDiagram!: ERDDiagramComponent;
   * The entities to display in the ERD.
   * Pass one or more EntityInfo objects. The component will automatically
   * discover and display related entities based on the `depth` setting.
   * // Single entity
   * [entities]="[currentEntity]"
   * // Multiple entities (schema view)
   * [entities]="filteredEntities"
   * All entities in the system for relationship discovery.
   * If not provided, uses Metadata.Entities automatically.
   * This is used to look up related entities when building the ERD.
  @Input() allEntities?: EntityInfo[];
   * ID of the currently selected entity.
   * Controlled by parent - the component emits selection events but doesn't
   * manage selection state internally.
  @Input() selectedEntityId: string | null = null;
   * IDs of entities to highlight (in addition to selected).
  @Input() highlightedEntityIds: string[] = [];
   * ID of entity to focus on (shows only this entity and related up to depth).
   * If null, shows all provided entities.
  @Input() focusEntityId: string | null = null;
   * Focus depth when focusEntityId is set.
  // INPUTS - Relationship Options
   * Number of relationship hops to include when auto-discovering related entities.
   * - `0`: Show only the provided entities (no auto-discovery)
   * - `1`: Show provided entities + directly related entities (default)
   * - `2+`: Show entities within N relationship hops
  @Input() depth = 1;
   * Include incoming relationships (entities that reference these entities).
  @Input() includeIncoming = true;
   * Include outgoing relationships (entities these reference via FK).
  @Input() includeOutgoing = true;
   * Configuration options passed to the underlying ERD component.
   * Whether to show the header bar with controls.
   * Title for the header bar.
   * Whether the diagram is in a loading/refreshing state.
   * Whether the diagram is read-only (no selection/dragging).
  // OUTPUTS
   * Emitted when an entity is selected by clicking.
   * The parent should update `selectedEntityId` in response.
  @Output() entitySelected = new EventEmitter<EntitySelectedEvent>();
   * Emitted when the selection is cleared.
  @Output() entityDeselected = new EventEmitter<void>();
   * Emitted when an entity is double-clicked.
   * Typically used to open the entity record.
  @Output() entityDoubleClick = new EventEmitter<EntitySelectedEvent>();
   * Emitted when requesting to open an entity record.
   * This is the standard MJ pattern for navigation.
  @Output() openRecord = new EventEmitter<OpenEntityRecordEvent>();
   * Emitted when zoom/pan changes.
   * Parent can use this for state persistence.
   * Emitted when diagram state changes (selection, zoom, etc.).
   * Emitted when user requests a refresh via the header button.
  // INTERNAL STATE
  /** Computed ERD nodes */
  erdNodes: ERDNode[] = [];
  // LIFECYCLE
    // Rebuild ERD data when entities or relationship options change
      changes['entities'] ||
      changes['allEntities'] ||
      changes['depth'] ||
      changes['includeIncoming'] ||
      changes['includeOutgoing']
      this.buildERDData();
  // ERD DATA BUILDING
  private buildERDData(): void {
    if (!this.entities || this.entities.length === 0) {
      this.erdNodes = [];
    // Use provided allEntities or fall back to Metadata
    const allEntities = this.allEntities || this._metadata.Entities;
    const result = buildERDDataFromEntities(this.entities, {
      allEntities,
      includeIncoming: this.includeIncoming,
      includeOutgoing: this.includeOutgoing,
      depth: this.depth
    // Note: Links are derived automatically by the ERD diagram component
    // from the node field relationships (relatedNodeId)
    this.erdNodes = result.nodes;
  // EVENT HANDLERS
  onNodeClick(_event: ERDNodeClickEvent): void {
    // Let the base component handle the click
    // Selection is handled via nodeSelected
  onNodeDoubleClick(event: ERDNodeDoubleClickEvent): void {
    const entity = getOriginalEntityFromERDNode(event.node);
      this.entityDoubleClick.emit({ entity, node: event.node });
      // Also emit openRecord for convenience - container handles navigation
      this.openRecord.emit({
        RecordID: entity.ID,
        entity
  onNodeSelected(node: ERDNode): void {
    const entity = getOriginalEntityFromERDNode(node);
      this.entitySelected.emit({ entity, node });
  onNodeDeselected(): void {
    this.entityDeselected.emit();
  onZoomChange(event: ERDZoomEvent): void {
    this.zoomChange.emit(event);
  onStateChange(state: ERDState): void {
  onRefreshRequested(): void {
    this.refreshRequested.emit();
  // PUBLIC API (Delegates to underlying component)
   * Zoom in on the diagram.
    this.erdDiagram?.zoomIn();
   * Zoom out on the diagram.
    this.erdDiagram?.zoomOut();
   * Reset zoom to default.
    this.erdDiagram?.resetZoom();
   * Fit all nodes in view.
  public zoomToFit(padding?: number): void {
    this.erdDiagram?.zoomToFit(padding);
   * Zoom to a specific entity.
  public zoomToEntity(entityId: string, scale?: number): void {
    this.erdDiagram?.zoomToNode(entityId, scale);
   * Get current diagram state for persistence.
  public getState(): ERDState | null {
    return this.erdDiagram?.getState() || null;
   * Restore diagram state.
  public setState(state: Partial<ERDState>, restorePositions?: boolean): void {
    this.erdDiagram?.setState(state, restorePositions);
   * Refresh the diagram.
    this.erdDiagram?.refresh();
   * Export diagram as SVG.
    return this.erdDiagram?.exportAsSVG() || '';
   * Trigger a resize recalculation. Call this when the container size changes.
    this.erdDiagram?.triggerResize();
