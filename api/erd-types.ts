 * Generic ERD Types for the Entity Relationship Diagram component.
 * These types are designed to be framework-agnostic and can work with any data source.
 * Represents a field in an ERD node (entity).
export interface ERDField {
  /** Unique identifier for the field */
  /** Display name of the field */
  /** Data type of the field (e.g., 'string', 'int', 'uuid') */
  /** Whether this field is a primary key */
  isPrimaryKey: boolean;
  /** ID of the related node (for foreign keys) */
  relatedNodeId?: string;
  /** Name of the related node (for display purposes) */
  relatedNodeName?: string;
  /** Name of the field in the related node (for FK relationships) */
  relatedFieldName?: string;
  /** Optional description of the field */
  /** Whether the field allows null values */
  allowsNull?: boolean;
  /** Default value for the field */
  defaultValue?: string;
  /** Maximum length (for string fields) */
  length?: number;
  /** Precision (for numeric fields) */
  precision?: number;
  /** Scale (for numeric fields) */
  scale?: number;
  /** Whether the field is virtual/computed */
  isVirtual?: boolean;
  /** Whether the field is auto-increment */
  autoIncrement?: boolean;
  /** Possible values for the field (for enums/dropdowns) */
  possibleValues?: ERDFieldValue[];
  /** Additional custom data */
  customData?: Record<string, unknown>;
 * Represents a possible value for an ERD field (enum values, dropdown options, etc.)
export interface ERDFieldValue {
  /** Unique identifier for the value */
  /** The actual value */
  /** Code representation (may differ from value) */
  /** Description of what this value means */
  /** Sort order */
  sequence?: number;
 * Represents a node (entity/table) in the ERD.
export interface ERDNode {
  /** Unique identifier for the node */
  /** Display name of the node */
  /** Optional schema/namespace name */
  /** Status of the node (e.g., 'Active', 'Deprecated') */
  /** Base table name (if different from display name) */
  baseTable?: string;
  /** All fields in this node */
  fields: ERDField[];
  /** Additional custom data that can be used by the consumer */
 * Represents a link (relationship) between two nodes in the ERD.
export interface ERDLink {
  /** ID of the source node */
  sourceNodeId: string;
  /** ID of the target node */
  targetNodeId: string;
  /** The field that creates this relationship (usually the FK field) */
  /** The field in the target node (usually the PK field) */
  /** Whether this is a self-referencing relationship */
  /** Relationship type (e.g., 'one-to-many', 'many-to-one') */
  relationshipType?: 'one-to-one' | 'one-to-many' | 'many-to-one' | 'many-to-many';
  /** Optional label for the relationship */
 * Configuration options for the ERD diagram.
export interface ERDConfig {
  /** Width of each node box in pixels */
  nodeWidth?: number;
  /** Base height of each node box in pixels */
  nodeBaseHeight?: number;
  /** Height per field in pixels */
  fieldHeight?: number;
  /** Maximum height of a node in pixels */
  maxNodeHeight?: number;
  /** Charge strength for force simulation (negative = repel) */
  chargeStrength?: number;
  /** Link distance for force simulation */
  /** Collision radius padding */
  collisionPadding?: number;
  /** Whether to show field details in nodes */
  showFieldDetails?: boolean;
  /** Whether to show relationship labels */
  showRelationshipLabels?: boolean;
  /** Whether to enable node dragging */
  enableDragging?: boolean;
  /** Whether to enable zoom/pan */
  enableZoom?: boolean;
  /** Initial zoom level */
  initialZoom?: number;
  /** Colors for different node states */
  colors?: ERDColorScheme;
 * Color scheme for the ERD diagram.
export interface ERDColorScheme {
  /** Background color for node rectangles */
  nodeBackground?: string;
  /** Border color for node rectangles */
  nodeBorder?: string;
  /** Header background color for nodes */
  nodeHeader?: string;
  /** Text color for node headers */
  nodeHeaderText?: string;
  /** Background color for primary key fields */
  primaryKeyBackground?: string;
  /** Text color for primary key fields */
  primaryKeyText?: string;
  /** Background color for foreign key fields */
  foreignKeyBackground?: string;
  /** Text color for foreign key fields */
  foreignKeyText?: string;
  /** Color for relationship links */
  linkColor?: string;
  /** Color for selected nodes */
  selectedBorder?: string;
  /** Color for highlighted nodes */
  highlightBorder?: string;
  /** Color for related nodes (when showing relationships) */
  relatedBorder?: string;
 * Filter configuration for the ERD.
export interface ERDFilter {
  /** Filter by schema name */
  schemaName?: string | null;
  /** Filter by node name (search) */
  nodeName?: string;
  /** Filter by node status */
  nodeStatus?: string | null;
  /** Filter by base table name (search) */
  /** Custom filter function */
  customFilter?: (node: ERDNode) => boolean;
 * Event data for node selection events.
export interface ERDNodeSelectionEvent {
  /** The selected node (null if deselected) */
  node: ERDNode | null;
  /** Previous selected node (null if none) */
  previousNode: ERDNode | null;
  /** Whether the consumer can cancel this selection */
  cancel: boolean;
 * Event data for node click events.
export interface ERDNodeClickEvent {
  /** The clicked node */
  /** The mouse event */
  mouseEvent: MouseEvent;
  /** Whether to cancel default behavior (selection) */
 * Event data for node double-click events.
export interface ERDNodeDoubleClickEvent {
  /** The double-clicked node */
  /** Whether to cancel default behavior */
 * Event data for link click events.
export interface ERDLinkClickEvent {
  /** The clicked link */
  link: ERDLink;
  /** Source node */
  sourceNode: ERDNode;
  /** Target node */
  targetNode: ERDNode;
 * Event data for zoom change events.
export interface ERDZoomEvent {
  /** Current zoom level */
  /** X translation */
  translateX: number;
  /** Y translation */
  translateY: number;
 * State of the ERD diagram for saving/restoring.
 * Use with `getState()` and `setState()` methods for persistence.
 * // Save state to localStorage
 * const state = erdComponent.getState();
 * erdComponent.setState(savedState);
export interface ERDState {
  /** ID of the currently selected node */
  selectedNodeId: string | null;
  /** IDs of highlighted nodes */
  highlightedNodeIds: string[];
  /** Current zoom level (1 = 100%) */
  /** X translation (pan position) */
  /** Y translation (pan position) */
  /** Focus node ID (if in focus mode) */
  focusNodeId: string | null;
  /** Focus depth (number of relationship hops to show) */
  focusDepth: number;
  /** Node positions for restoring exact layout */
  nodePositions: Record<string, { x: number; y: number; fx?: number | null; fy?: number | null }>;
 * Event data for node hover events.
export interface ERDNodeHoverEvent {
  /** The hovered node */
  /** Nodes directly connected to this node via relationships */
  relatedNodes: ERDNode[];
  /** Position for displaying tooltips */
  position: { x: number; y: number };
 * Event data for link hover events.
export interface ERDLinkHoverEvent {
  /** The hovered link */
  /** Source node of the relationship */
  /** Target node of the relationship */
 * Event data for context menu events on nodes.
 * Set `cancel = true` to prevent default browser context menu.
export interface ERDNodeContextMenuEvent {
  /** The right-clicked node */
  /** Set to true to prevent default context menu */
  /** Screen position for showing custom context menu */
 * Event data for context menu events on links.
export interface ERDLinkContextMenuEvent {
  /** The right-clicked link */
 * Event data for context menu events on the diagram background.
export interface ERDDiagramContextMenuEvent {
  /** Diagram coordinates of the click */
  diagramPosition: { x: number; y: number };
 * Event data for node drag events.
 * Set `cancel = true` in dragStart to prevent dragging.
export interface ERDNodeDragEvent {
  /** The dragged node */
  /** Position when drag started */
  startPosition: { x: number; y: number };
  /** Current position during drag */
  currentPosition: { x: number; y: number };
  /** Set to true in dragStart to cancel the drag */
 * Layout algorithm options for the ERD diagram.
 * - 'force': D3 force-directed layout (dynamic, animated)
 * - 'dagre': Dagre hierarchical layout with orthogonal edges (static, clean)
 * - 'horizontal': Left-to-right hierarchical layout using Dagre
 * - 'vertical': Top-to-bottom hierarchical layout using Dagre
 * - 'radial': Radial layout (not yet implemented)
export type ERDLayoutAlgorithm = 'force' | 'dagre' | 'horizontal' | 'vertical' | 'radial';
 * Dagre-specific layout configuration options.
export interface ERDDagreConfig {
  /** Direction of the graph layout. Default: 'LR' (left-to-right) */
  rankDir?: 'TB' | 'BT' | 'LR' | 'RL';
  /** Horizontal separation between nodes. Default: 50 */
  /** Vertical separation between ranks/layers. Default: 100 */
  /** Separation between different edge paths. Default: 10 */
  edgeSep?: number;
  /** Algorithm for ranking nodes: 'network-simplex', 'tight-tree', 'longest-path'. Default: 'network-simplex' */
  ranker?: 'network-simplex' | 'tight-tree' | 'longest-path';
  /** Alignment of nodes within their rank: 'UL', 'UR', 'DL', 'DR'. Default: undefined (centered) */
  align?: 'UL' | 'UR' | 'DL' | 'DR';
 * Extended configuration options for the ERD diagram.
  // Node Sizing
  /** Width of each node box in pixels. Default: 180 */
  /** Base height of each node box in pixels (before adding fields). Default: 60 */
  /** Height per field row in pixels. Default: 20 */
  /** Maximum height of a node in pixels. Default: 300 */
  // Force Simulation
  /** Charge strength for force simulation (negative = repel). Default: -800 */
  /** Base distance between linked nodes. Default: 80 */
  /** Extra padding for collision detection. Default: 20 */
  // Display Options
  /** Whether to show field details (PKs, FKs) in nodes. Default: true */
  /** Whether to show relationship labels on links. Default: true */
  /** Whether to show the header bar with controls. Default: true */
  showHeader?: boolean;
  /** Whether to show node count in header. Default: true */
  showNodeCount?: boolean;
  /** Whether to show a minimap for navigation. Default: false */
  showMinimap?: boolean;
  /** Whether to show a legend explaining colors. Default: false */
  showLegend?: boolean;
  // Interaction Options
  /** Enable node dragging. Default: true */
  /** Enable zoom with mouse wheel. Default: true */
  /** Enable panning by dragging background. Default: true */
  enablePan?: boolean;
  /** Minimum zoom level. Default: 0.1 */
  minZoom?: number;
  /** Maximum zoom level. Default: 4 */
  maxZoom?: number;
  /** Initial zoom level (1 = 100%). Default: 1 */
  /** Enable multi-select with Ctrl+click. Default: false */
  enableMultiSelect?: boolean;
  // Animation
  /** Duration of zoom/pan animations in milliseconds. Default: 750 */
  /** Whether to auto-fit diagram to container on initial load. Default: true */
  fitOnLoad?: boolean;
   * Skip the D3 force simulation animation and render immediately.
   * When true, the simulation runs to completion synchronously and nodes
   * are positioned immediately without animation.
  skipAnimation?: boolean;
  // Empty State
  /** Message to show when no nodes are provided. Default: 'No entities to display' */
  emptyStateMessage?: string;
  /** Icon class for empty state. Default: 'fa-solid fa-diagram-project' */
  emptyStateIcon?: string;
  // Layout
  /** Layout algorithm to use. Default: 'force' */
  layoutAlgorithm?: ERDLayoutAlgorithm;
  /** Dagre-specific layout configuration (only used when layoutAlgorithm is 'dagre', 'horizontal', or 'vertical') */
  dagreConfig?: ERDDagreConfig;
  // Colors
  /** Color scheme for the diagram */
 * Information about a relationship for highlighting purposes.
export interface ERDRelationshipInfo {
  /** The related node */
  /** The link connecting them */
  /** Direction of relationship from the perspective of the source node */
  direction: 'outgoing' | 'incoming';
  /** The field creating this relationship */
  field: ERDField;
