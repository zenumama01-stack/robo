  Component, Input, Output, EventEmitter, ViewChild, OnInit, OnDestroy,
  ChangeDetectorRef, ViewEncapsulation, ChangeDetectionStrategy, HostListener
import { FFlowComponent, FCanvasComponent, FZoomDirective, FCreateConnectionEvent,
         FCreateNodeEvent, FSelectionChangeEvent, FCanvasChangeEvent, FMoveNodesEvent,
         FReassignConnectionEvent } from '@foblex/flow';
  FlowNode, FlowConnection, FlowNodeTypeConfig, FlowNodeAddedEvent,
  FlowNodeMovedEvent, FlowConnectionCreatedEvent, FlowConnectionReassignedEvent,
  FlowSelectionChangedEvent,
  FlowCanvasClickEvent, FlowPosition, FlowLayoutDirection,
  FlowContextMenuTarget, FlowContextMenuAction
} from '../interfaces/flow-types';
import { FlowStateService } from '../services/flow-state.service';
import { FlowLayoutService } from '../services/flow-layout.service';
 * Generic, entity-agnostic visual flow editor component.
 * Wraps Foblex Flow to provide a clean MJ-style API.
 * <mj-flow-editor
 *   [Nodes]="myNodes"
 *   [Connections]="myConnections"
 *   [NodeTypes]="myNodeTypes"
 *   (NodeSelected)="onNodeSelected($event)"
 *   (ConnectionCreated)="onConnectionCreated($event)">
 * </mj-flow-editor>
  selector: 'mj-flow-editor',
  templateUrl: './flow-editor.component.html',
  styleUrls: ['./flow-editor.component.css'],
  changeDetection: ChangeDetectionStrategy.Default,
  providers: [FlowStateService, FlowLayoutService]
export class FlowEditorComponent implements OnInit, OnDestroy {
  @Input() Nodes: FlowNode[] = [];
  @Input() Connections: FlowConnection[] = [];
  @Input() NodeTypes: FlowNodeTypeConfig[] = [];
  @Input() ShowMinimap = true;
  @Input() ShowPalette = true;
  @Input() ShowGrid = true;
  @Input() ShowStatusBar = true;
  @Input() ShowToolbar = true;
  @Input() ShowLegend = true;
  @Input() GridSize = 20;
  @Input() AutoLayoutDirection: FlowLayoutDirection = 'vertical';
  /** Background color for connection labels */
  @Input() ConnectionLabelBackground = '#fffef5';
  /** Border color for connection labels */
  @Input() ConnectionLabelBorderColor = '#cbd5e1';
  /** Text color for connection labels */
  @Input() ConnectionLabelTextColor = '#334155';
  @Output() NodeSelected = new EventEmitter<FlowNode | null>();
  @Output() NodeAdded = new EventEmitter<FlowNodeAddedEvent>();
  @Output() NodeRemoved = new EventEmitter<FlowNode>();
  @Output() NodeMoved = new EventEmitter<FlowNodeMovedEvent>();
  @Output() ConnectionCreated = new EventEmitter<FlowConnectionCreatedEvent>();
  @Output() ConnectionRemoved = new EventEmitter<FlowConnection>();
  @Output() ConnectionReassigned = new EventEmitter<FlowConnectionReassignedEvent>();
  @Output() ConnectionSelected = new EventEmitter<FlowConnection | null>();
  @Output() SelectionChanged = new EventEmitter<FlowSelectionChangedEvent>();
  @Output() CanvasClicked = new EventEmitter<FlowCanvasClickEvent>();
  @Output() NodesChanged = new EventEmitter<FlowNode[]>();
  @Output() ConnectionsChanged = new EventEmitter<FlowConnection[]>();
  @Output() NodeEditRequested = new EventEmitter<FlowNode>();
  @Output() ConnectionEditRequested = new EventEmitter<FlowConnection>();
  @Output() GridToggled = new EventEmitter<boolean>();
  @Output() MinimapToggled = new EventEmitter<boolean>();
  @Output() LegendToggled = new EventEmitter<boolean>();
  @ViewChild(FFlowComponent) protected fFlow: FFlowComponent | undefined;
  @ViewChild(FCanvasComponent) protected fCanvas: FCanvasComponent | undefined;
  @ViewChild('fZoomRef') protected fZoom: FZoomDirective | undefined;
  // ── Internal State ──────────────────────────────────────────
  protected zoomLevel = 100;
  protected canvasPosition = { x: 0, y: 0 };
  protected canvasScale = 1;
  protected selectedNodeIDs: string[] = [];
  protected selectedConnectionIDs: string[] = [];
  protected panMode = false;
  /** Trigger function for canvas panning — only allows panning in pan mode */
  protected canvasMoveTriggerFn = (_event: MouseEvent | TouchEvent): boolean => {
    return this.panMode;
  /* Context menu state */
  protected contextMenuVisible = false;
  protected contextMenuX = 0;
  protected contextMenuY = 0;
  protected contextMenuTargetType: FlowContextMenuTarget = 'node';
  protected contextMenuNode: FlowNode | null = null;
  protected contextMenuConnection: FlowConnection | null = null;
  private lastConnectionClickTime = 0;
    private stateService: FlowStateService,
    private layoutService: FlowLayoutService
  // ── Public Methods ──────────────────────────────────────────
  /** Fit all nodes within the viewport */
  ZoomToFit(): void {
    this.fCanvas?.fitToScreen({ x: 150, y: 150 }, true);
  /** Zoom in one step */
  ZoomIn(): void {
    if (this.fZoom) {
      this.fZoom.zoomIn();
      const newScale = Math.min(this.canvasScale + 0.15, 3);
      this.fCanvas?.setScale(newScale);
      this.updateZoomLevel(newScale);
  /** Zoom out one step */
  ZoomOut(): void {
      this.fZoom.zoomOut();
      const newScale = Math.max(this.canvasScale - 0.15, 0.1);
  /** Run auto-layout using dagre */
  AutoArrange(direction?: FlowLayoutDirection): void {
    const dir = direction ?? this.AutoLayoutDirection;
    this.pushUndoState();
    const result = this.layoutService.CalculateLayout(this.Nodes, this.Connections, dir);
    for (const node of this.Nodes) {
      const newPos = result.Positions.get(node.ID);
      if (newPos) {
        node.Position = { ...newPos };
    this.NodesChanged.emit(this.Nodes);
    // Fit to screen after layout
    setTimeout(() => this.ZoomToFit(), 100);
  /** Center the canvas on a specific node */
  CenterOnNode(nodeId: string): void {
    this.fCanvas?.centerGroupOrNode(nodeId, true);
  /** Programmatically select a node */
  SelectNode(nodeId: string): void {
    this.fFlow?.select([nodeId], []);
    const node = this.Nodes.find(n => n.ID === nodeId) ?? null;
    this.NodeSelected.emit(node);
  /** Clear all selection */
    this.fFlow?.clearSelection();
    this.selectedNodeIDs = [];
    this.selectedConnectionIDs = [];
    this.NodeSelected.emit(null);
    this.ConnectionSelected.emit(null);
  /** Update a node's visual status */
  SetNodeStatus(nodeId: string, status: FlowNode['Status'], message?: string): void {
    const node = this.Nodes.find(n => n.ID === nodeId);
      node.Status = status;
      node.StatusMessage = message;
  /** Update a node's visual properties and push a new object reference so
   *  OnPush child components (FlowNodeComponent) detect the change. */
  UpdateNode(nodeId: string, changes: Partial<Pick<FlowNode, 'Label' | 'Subtitle' | 'Icon' | 'Status' | 'StatusMessage' | 'IsStartNode' | 'Badges' | 'Data'>>): void {
    const idx = this.Nodes.findIndex(n => n.ID === nodeId);
    if (idx === -1) return;
    // Create a new object reference so the OnPush FlowNodeComponent picks up the change
    this.Nodes[idx] = { ...this.Nodes[idx], ...changes };
  /** Update a connection's visual properties in-place (label, color, style, etc.) */
  UpdateConnection(connId: string, changes: Partial<Pick<FlowConnection, 'Label' | 'LabelIcon' | 'LabelIconColor' | 'LabelDetail' | 'Color' | 'Style' | 'Animated'>>): void {
    const conn = this.Connections.find(c => c.ID === connId);
    if (!conn) return;
    Object.assign(conn, changes);
  /** Highlight a sequence of nodes (e.g., execution path) */
  HighlightPath(nodeIds: string[]): void {
      if (nodeIds.includes(node.ID)) {
        node.Status = 'running';
    // Highlight connections along the path
    for (const conn of this.Connections) {
      const srcIdx = nodeIds.indexOf(conn.SourceNodeID);
      const tgtIdx = nodeIds.indexOf(conn.TargetNodeID);
      if (srcIdx >= 0 && tgtIdx >= 0 && tgtIdx === srcIdx + 1) {
        conn.Animated = true;
  /** Clear all node status highlights */
  ClearHighlights(): void {
      node.Status = 'default';
      node.StatusMessage = undefined;
      conn.Animated = false;
  /** Undo the last operation */
  Undo(): void {
    const snapshot = this.stateService.Undo(this.Nodes, this.Connections);
    if (snapshot) {
      this.Nodes = snapshot.Nodes;
      this.Connections = snapshot.Connections;
      this.ConnectionsChanged.emit(this.Connections);
  /** Redo the last undone operation */
  Redo(): void {
    const snapshot = this.stateService.Redo(this.Nodes, this.Connections);
  get CanUndo(): boolean {
    return this.stateService.CanUndo;
  get CanRedo(): boolean {
    return this.stateService.CanRedo;
  /** Delete the currently selected nodes and connections */
  DeleteSelected(): void {
    if (this.ReadOnly || (this.selectedNodeIDs.length === 0 && this.selectedConnectionIDs.length === 0)) {
    this.removeSelectedItems();
  // ── Foblex Event Handlers ──────────────────────────────────
  /** Called when Foblex flow finishes loading */
  protected onFlowLoaded(): void {
    if (this.Nodes.length > 0) {
      setTimeout(() => this.ZoomToFit(), 200);
  /** Canvas zoom/pan changed */
  protected onCanvasChange(event: FCanvasChangeEvent): void {
    this.canvasPosition = event.position;
    this.canvasScale = event.scale;
    this.updateZoomLevel(event.scale);
  /** User drew a new connection */
  protected onCreateConnection(event: FCreateConnectionEvent): void {
    if (!event.fInputId || this.ReadOnly) return;
    // Derive node IDs from port IDs (ports are named nodeID-input / nodeID-output)
    const sourceNodeID = this.findNodeByPortId(event.fOutputId);
    const targetNodeID = this.findNodeByPortId(event.fInputId);
    if (!sourceNodeID || !targetNodeID) return;
    // Resolve actual port IDs — when fConnectOnNode is enabled, Foblex may
    // send the node ID itself instead of a port ID. Map to the correct port.
    const sourcePortID = this.resolvePortId(event.fOutputId, sourceNodeID, 'output');
    const targetPortID = this.resolvePortId(event.fInputId, targetNodeID, 'input');
    // Prevent duplicate connections
    const exists = this.Connections.some(
      c => c.SourcePortID === sourcePortID && c.TargetPortID === targetPortID
    this.ConnectionCreated.emit({
      SourceNodeID: sourceNodeID,
      SourcePortID: sourcePortID,
      TargetNodeID: targetNodeID,
      TargetPortID: targetPortID
  /** User reassigned (dragged) an existing connection endpoint to a new node */
  protected onReassignConnection(event: FReassignConnectionEvent): void {
    if (this.ReadOnly) return;
    const conn = this.Connections.find(c => c.ID === event.connectionId);
    // If dropped on empty space (no new target/source), discard the reassignment
    if (event.isTargetReassign && !event.newTargetId) return;
    if (event.isSourceReassign && !event.newSourceId) return;
    const oldSourceNodeID = conn.SourceNodeID;
    const oldSourcePortID = conn.SourcePortID;
    const oldTargetNodeID = conn.TargetNodeID;
    const oldTargetPortID = conn.TargetPortID;
    // Resolve new endpoints
    let newSourceNodeID = oldSourceNodeID;
    let newSourcePortID = oldSourcePortID;
    let newTargetNodeID = oldTargetNodeID;
    let newTargetPortID = oldTargetPortID;
    if (event.isSourceReassign && event.newSourceId) {
      newSourceNodeID = this.findNodeByPortId(event.newSourceId) ?? oldSourceNodeID;
      newSourcePortID = this.resolvePortId(event.newSourceId, newSourceNodeID, 'output');
    if (event.isTargetReassign && event.newTargetId) {
      newTargetNodeID = this.findNodeByPortId(event.newTargetId) ?? oldTargetNodeID;
      newTargetPortID = this.resolvePortId(event.newTargetId, newTargetNodeID, 'input');
    // Prevent self-connections
    if (newSourceNodeID === newTargetNodeID) return;
    // Prevent duplicates
    const duplicate = this.Connections.some(
      c => c.ID !== conn.ID &&
           c.SourcePortID === newSourcePortID &&
           c.TargetPortID === newTargetPortID
    if (duplicate) return;
    // Update the connection in-place
    conn.SourceNodeID = newSourceNodeID;
    conn.SourcePortID = newSourcePortID;
    conn.TargetNodeID = newTargetNodeID;
    conn.TargetPortID = newTargetPortID;
    this.ConnectionReassigned.emit({
      ConnectionID: conn.ID,
      OldSourceNodeID: oldSourceNodeID,
      OldSourcePortID: oldSourcePortID,
      OldTargetNodeID: oldTargetNodeID,
      OldTargetPortID: oldTargetPortID,
      NewSourceNodeID: newSourceNodeID,
      NewSourcePortID: newSourcePortID,
      NewTargetNodeID: newTargetNodeID,
      NewTargetPortID: newTargetPortID
  /** External item dropped from palette */
  protected onCreateNode(event: FCreateNodeEvent): void {
    const nodeType = event.data as string;
    const config = this.NodeTypes.find(nt => nt.Type === nodeType);
    // event.rect is already normalized to canvas coordinates by Foblex's
    // GetNormalizedElementRectExecution (via RoundedRect.fromCenter).
    // Do NOT pass it through getPositionInFlow() again — that double-normalizes
    // and places the node far from the actual drop point.
    // Use the center of the preview rect (≈ cursor position at drop time).
    const dropPosition: FlowPosition = {
      X: event.rect.x + event.rect.width / 2,
      Y: event.rect.y + event.rect.height / 2
    const newNode = this.createDefaultNode(config, dropPosition);
    this.NodeAdded.emit({ Node: newNode, DropPosition: dropPosition });
  /** Selection changed in Foblex */
  protected onSelectionChange(event: FSelectionChangeEvent): void {
    const timeSinceClick = Date.now() - this.lastConnectionClickTime;
    // If a direct connection click just fired, skip this Foblex callback
    // to avoid double-processing / overwriting the direct click result.
    if (timeSinceClick < 200) {
    this.selectedNodeIDs = event.fNodeIds ?? [];
    this.selectedConnectionIDs = event.fConnectionIds ?? [];
    this.SelectionChanged.emit({
      SelectedNodeIDs: this.selectedNodeIDs,
      SelectedConnectionIDs: this.selectedConnectionIDs
    // Emit specific node/connection selection events.
    // IMPORTANT: When selecting a connection, emit NodeSelected(null) FIRST so
    // that consumers clear their node state before the connection event opens
    // the connection properties panel.
    if (this.selectedNodeIDs.length === 1) {
      const node = this.Nodes.find(n => n.ID === this.selectedNodeIDs[0]);
      this.NodeSelected.emit(node ?? null);
    } else if (this.selectedConnectionIDs.length === 1) {
      const conn = this.Connections.find(c => c.ID === this.selectedConnectionIDs[0]);
      this.ConnectionSelected.emit(conn ?? null);
    } else if (this.selectedNodeIDs.length === 0 && this.selectedConnectionIDs.length === 0) {
  /** Nodes moved on canvas */
  protected onNodesMoved(event: FMoveNodesEvent): void {
    if (this.ReadOnly || !event.fNodes || event.fNodes.length === 0) return;
    for (const moved of event.fNodes) {
      const node = this.Nodes.find(n => n.ID === moved.id);
        const oldPosition = { ...node.Position };
        node.Position = { X: moved.position.x, Y: moved.position.y };
        this.NodeMoved.emit({
          NodeID: node.ID,
          OldPosition: oldPosition,
          NewPosition: node.Position
  // ── Toolbar Event Handlers ─────────────────────────────────
  protected onGridToggled(show: boolean): void {
    this.ShowGrid = show;
    this.GridToggled.emit(show);
  protected onMinimapToggled(show: boolean): void {
    this.ShowMinimap = show;
    this.MinimapToggled.emit(show);
  protected onLegendToggled(show: boolean): void {
    this.ShowLegend = show;
    this.LegendToggled.emit(show);
  protected onPanModeToggled(enabled: boolean): void {
    this.panMode = enabled;
  // ── Context Menu ───────────────────────────────────────────
  protected onNodeContextMenu(event: MouseEvent, node: FlowNode): void {
    this.showContextMenu(event, 'node', node, null);
  /** Direct click on a connection — ensures ConnectionSelected fires reliably */
  protected onConnectionClicked(event: MouseEvent, conn: FlowConnection): void {
    // Do NOT stopPropagation — let Foblex handle its own visual selection.
    // Record the time so onSelectionChange can skip double-processing.
    this.lastConnectionClickTime = Date.now();
    this.selectedConnectionIDs = [conn.ID];
    this.ConnectionSelected.emit(conn);
  protected onConnectionContextMenu(event: MouseEvent, conn: FlowConnection): void {
    this.showContextMenu(event, 'connection', null, conn);
  protected onContextMenuAction(action: FlowContextMenuAction): void {
    if (action === 'edit') {
      if (this.contextMenuTargetType === 'node' && this.contextMenuNode) {
        this.NodeEditRequested.emit(this.contextMenuNode);
      } else if (this.contextMenuTargetType === 'connection' && this.contextMenuConnection) {
        this.ConnectionEditRequested.emit(this.contextMenuConnection);
    } else if (action === 'remove') {
        this.removeNodeById(this.contextMenuNode.ID);
        this.removeConnectionById(this.contextMenuConnection.ID);
  protected onDocumentClick(): void {
    if (this.contextMenuVisible) {
  private showContextMenu(
    event: MouseEvent,
    targetType: FlowContextMenuTarget,
    node: FlowNode | null,
    connection: FlowConnection | null
    this.contextMenuTargetType = targetType;
    this.contextMenuNode = node;
    this.contextMenuConnection = connection;
    this.contextMenuX = event.clientX;
    this.contextMenuY = event.clientY;
  private hideContextMenu(): void {
    this.contextMenuNode = null;
    this.contextMenuConnection = null;
  private removeNodeById(nodeId: string): void {
    this.Nodes = this.Nodes.filter(n => n.ID !== nodeId);
    /* Remove connections attached to this node */
    const orphaned = this.Connections.filter(
      c => c.SourceNodeID === nodeId || c.TargetNodeID === nodeId
    this.Connections = this.Connections.filter(
      c => c.SourceNodeID !== nodeId && c.TargetNodeID !== nodeId
    for (const conn of orphaned) {
      this.ConnectionRemoved.emit(conn);
    this.NodeRemoved.emit(node);
  private removeConnectionById(connId: string): void {
    this.Connections = this.Connections.filter(c => c.ID !== connId);
  protected onKeyDown(event: KeyboardEvent): void {
    const isCtrlOrMeta = event.ctrlKey || event.metaKey;
    if (event.key === 'Delete' || event.key === 'Backspace') {
      // Don't delete if user is typing in an input
      if (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.tagName === 'SELECT') {
      this.DeleteSelected();
    } else if (isCtrlOrMeta && event.key === 'z' && !event.shiftKey) {
      this.Undo();
    } else if (isCtrlOrMeta && (event.key === 'y' || (event.key === 'z' && event.shiftKey))) {
      this.Redo();
    } else if (isCtrlOrMeta && event.key === 'a') {
      this.fFlow?.selectAll();
  // ── TrackBy Functions ───────────────────────────────────────
  protected trackNodeById(_index: number, node: FlowNode): string {
    return node.ID;
  protected trackConnectionById(_index: number, conn: FlowConnection): string {
    return conn.ID;
  // ── Helper Methods ─────────────────────────────────────────
  GetTypeConfig(type: string): FlowNodeTypeConfig | null {
    return this.NodeTypes.find(nt => nt.Type === type) ?? null;
  /** Get the connectable side string for Foblex based on port side */
  GetConnectableSide(side: string | undefined): string {
    switch (side) {
      case 'left': return 'left';
      case 'right': return 'right';
      case 'top': return 'top';
      case 'bottom': return 'bottom';
      default: return 'auto';
    return this.selectedNodeIDs.length + this.selectedConnectionIDs.length;
  private pushUndoState(): void {
    this.stateService.PushState(this.Nodes, this.Connections);
  private updateZoomLevel(scale: number): void {
    this.zoomLevel = Math.round(scale * 100);
  private findNodeByPortId(portId: string): string | null {
      for (const port of node.Ports) {
        if (port.ID === portId) {
    // When fConnectOnNode is enabled, Foblex may send the node ID itself
    // instead of a port ID. Check for a direct node ID match as fallback.
    const directMatch = this.Nodes.find(n => n.ID === portId);
    if (directMatch) {
      return directMatch.ID;
   * When fConnectOnNode is enabled, Foblex may send a node ID instead of a port ID.
   * This resolves the raw ID to the correct port ID for the given direction.
  private resolvePortId(rawId: string, nodeId: string, direction: 'input' | 'output'): string {
    if (!node) return rawId;
    // If the rawId already matches a port on this node, use it as-is
    const exactPort = node.Ports.find(p => p.ID === rawId);
    if (exactPort) return rawId;
    // Otherwise, find the first port matching the expected direction
    const dirPort = node.Ports.find(p => p.Direction === direction);
    return dirPort ? dirPort.ID : rawId;
  private createDefaultNode(config: FlowNodeTypeConfig, position: FlowPosition): FlowNode {
    const id = this.generateId();
    const defaultPorts = config.DefaultPorts ?? [
      { ID: `${id}-input`, Direction: 'input' as const, Side: 'top' as const, Multiple: true },
      { ID: `${id}-output`, Direction: 'output' as const, Side: 'bottom' as const, Multiple: true }
      Type: config.Type,
      Label: config.Label,
      Icon: config.Icon,
      Status: 'default',
      Position: position,
      Ports: defaultPorts.map(p => ({
        ID: p.ID.startsWith(id) ? p.ID : `${id}-${p.ID}`
  private removeSelectedItems(): void {
    // Remove selected connections
    const removedConnections = this.Connections.filter(c => this.selectedConnectionIDs.includes(c.ID));
    this.Connections = this.Connections.filter(c => !this.selectedConnectionIDs.includes(c.ID));
    for (const conn of removedConnections) {
    // Remove selected nodes and their connections
    const removedNodes = this.Nodes.filter(n => this.selectedNodeIDs.includes(n.ID));
    this.Nodes = this.Nodes.filter(n => !this.selectedNodeIDs.includes(n.ID));
    // Also remove connections attached to deleted nodes
    const deletedNodeIDs = new Set(this.selectedNodeIDs);
    const orphanedConnections = this.Connections.filter(
      c => deletedNodeIDs.has(c.SourceNodeID) || deletedNodeIDs.has(c.TargetNodeID)
      c => !deletedNodeIDs.has(c.SourceNodeID) && !deletedNodeIDs.has(c.TargetNodeID)
    for (const conn of orphanedConnections) {
    for (const node of removedNodes) {
    return 'flow-' + Math.random().toString(36).substring(2, 11);
