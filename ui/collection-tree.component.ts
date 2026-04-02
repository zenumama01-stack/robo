import { UserInfo, RunView, Metadata, LogError } from '@memberjunction/core';
import { CollectionPermission, CollectionPermissionService } from '../../services/collection-permission.service';
interface TreeNode {
  children: TreeNode[];
interface DragData {
  collectionId: string;
  parentId: string | null;
  selector: 'mj-collection-tree',
    <div class="collection-tree">
        <h3>Collections</h3>
        @if (canCreateAtRoot()) {
          <button class="btn-new" (click)="onCreateCollection(null)" title="New Collection">
        class="tree-content"
        [class.drag-over-root]="dragOverNodeId === 'root'"
        (dragover)="onDragOverRoot($event)"
        (drop)="onDropRoot($event)">
        @for (node of treeNodes; track node.collection.ID) {
          <div class="tree-node-wrapper">
              [class.selected]="node.collection.ID === selectedCollectionId"
              [class.drag-over]="dragOverNodeId === node.collection.ID"
              [class.dragging]="draggedNode?.collection?.ID === node.collection.ID"
              [style.padding-left.px]="node.level * 20"
              (click)="onSelectCollection(node.collection)"
              (dragstart)="onDragStart($event, node)"
              (dragover)="onDragOver($event, node)"
              (dragleave)="onDragLeave($event, node)"
              (drop)="onDrop($event, node)">
                  class="fas toggle-icon"
                  [ngClass]="node.expanded ? 'fa-chevron-down' : 'fa-chevron-right'"
                  (click)="toggleNode(node, $event)">
              <div class="node-actions" (click)="$event.stopPropagation()">
                @if (canEdit(node.collection)) {
                  <button class="node-action-btn" (click)="onCreateCollection(node.collection.ID)" title="Add sub-collection">
                @if (canDelete(node.collection)) {
                  <button class="node-action-btn" (click)="onDeleteCollection(node.collection)" title="Delete">
              @for (child of node.children; track child.collection.ID) {
                <ng-container *ngTemplateOutlet="recursiveTree; context: { node: child }"></ng-container>
    <ng-template #recursiveTree let-node="node">
    .collection-tree { display: flex; flex-direction: column; height: 100%; }
    .tree-header { padding: 16px; border-bottom: 1px solid #D9D9D9; display: flex; justify-content: space-between; align-items: center; }
    .tree-header h3 { margin: 0; font-size: 16px; }
    .btn-new { padding: 6px 10px; background: #0076B6; color: white; border: none; border-radius: 4px; cursor: pointer; }
    .btn-new:hover { background: #005A8C; }
    .tree-content { flex: 1; overflow-y: auto; position: relative; }
    .tree-content.drag-over-root { background: rgba(0, 118, 182, 0.1); }
    .tree-node:hover { background: #F4F4F4; }
    .tree-node.selected { background: #AAE7FD; }
    .tree-node.dragging {
    .tree-node.drag-over {
      border: 2px dashed #0076B6;
    .toggle-icon { font-size: 10px; color: #AAA; cursor: pointer; width: 12px; }
    .collection-icon { font-size: 14px; }
    .collection-name { flex: 1; font-size: 14px; user-select: none; }
    .node-actions { display: none; gap: 4px; }
    .tree-node:hover .node-actions { display: flex; }
    .node-action-btn { padding: 4px 6px; background: transparent; border: none; cursor: pointer; border-radius: 3px; color: #666; }
    .node-action-btn:hover { background: rgba(0,0,0,0.1); }
export class CollectionTreeComponent implements OnInit {
  @Input() selectedCollectionId: string | null = null;
  @Input() userPermissions: Map<string, CollectionPermission> = new Map();
  @Output() collectionSelected = new EventEmitter<MJCollectionEntity>();
  @Output() collectionCreated = new EventEmitter<MJCollectionEntity>();
  @Output() collectionDeleted = new EventEmitter<MJCollectionEntity>();
  public collections: MJCollectionEntity[] = [];
  public treeNodes: TreeNode[] = [];
  public draggedNode: TreeNode | null = null;
  public dragOverNodeId: string | null = null;
  constructor(private permissionService: CollectionPermissionService) {}
    this.loadCollections();
    const rootCollections = this.collections.filter(c => !c.ParentID);
    this.treeNodes = rootCollections.map(c => this.buildNode(c, 0));
  private buildNode(collection: MJCollectionEntity, level: number): TreeNode {
    const children = this.collections.filter(c => c.ParentID === collection.ID);
      children: children.map(c => this.buildNode(c, level + 1)),
      expanded: level === 0,
  toggleNode(node: TreeNode, event: Event): void {
  onSelectCollection(collection: MJCollectionEntity): void {
    this.selectedCollectionId = collection.ID;
    this.collectionSelected.emit(collection);
  async onCreateCollection(parentId: string | null): Promise<void> {
    // Validate permission if creating child collection
      const parentCollection = this.collections.find(c => c.ID === parentId);
      if (parentCollection) {
        // Check if user has Edit permission on parent
        if (parentCollection.OwnerID && parentCollection.OwnerID !== this.currentUser.ID) {
            alert('You do not have Edit permission to create a sub-collection.');
    const name = prompt('Enter collection name:');
    if (!name) return;
      collection.Name = name;
        // Child collection - inherit parent's owner and set parent
        collection.ParentID = parentId;
        collection.OwnerID = parentCollection?.OwnerID || this.currentUser.ID;
        this.collectionCreated.emit(collection);
      alert('Failed to create collection');
  async onDeleteCollection(collection: MJCollectionEntity): Promise<void> {
    // Validate Delete permission
      if (!permission?.canDelete) {
        alert('You do not have Delete permission for this collection.');
    if (!confirm(`Delete collection "${collection.Name}"?`)) return;
      const deleted = await collection.Delete();
        this.collectionDeleted.emit(collection);
      console.error('Error deleting collection:', error);
      alert('Failed to delete collection');
  onDragStart(event: DragEvent, node: TreeNode): void {
    this.draggedNode = node;
    const dragData: DragData = {
      collectionId: node.collection.ID,
      parentId: node.collection.ParentID || null
    event.dataTransfer!.setData('application/json', JSON.stringify(dragData));
    // Add visual feedback
    (event.target as HTMLElement).style.opacity = '0.4';
    // Clean up visual feedback
    (event.target as HTMLElement).style.opacity = '1';
    this.draggedNode = null;
    this.dragOverNodeId = null;
  onDragOver(event: DragEvent, targetNode: TreeNode): void {
    event.preventDefault(); // Required to allow drop
    if (!this.draggedNode || this.draggedNode.collection.ID === targetNode.collection.ID) {
      event.dataTransfer!.dropEffect = 'none';
    // Check if trying to drop into a descendant
    if (this.isDescendant(targetNode, this.draggedNode)) {
    this.dragOverNodeId = targetNode.collection.ID;
  onDragLeave(event: DragEvent, targetNode: TreeNode): void {
    if (this.dragOverNodeId === targetNode.collection.ID) {
  async onDrop(event: DragEvent, targetNode: TreeNode): Promise<void> {
    if (!this.draggedNode) {
    // Don't allow dropping on itself
    if (this.draggedNode.collection.ID === targetNode.collection.ID) {
      alert('Cannot move a collection into its own descendant');
      const collection = this.draggedNode.collection;
      const newParentId = targetNode.collection.ID;
      // Update the collection's parent
      collection.ParentID = newParentId;
        // Reload the tree to reflect changes
        alert('Failed to move collection');
      alert('Error moving collection');
  onDragOverRoot(event: DragEvent): void {
    this.dragOverNodeId = 'root';
  async onDropRoot(event: DragEvent): Promise<void> {
      // Move to root level
      collection.ParentID = null;
        alert('Failed to move collection to root');
      alert('Error moving collection to root');
  private isDescendant(potentialDescendant: TreeNode, ancestor: TreeNode): boolean {
    if (potentialDescendant.collection.ParentID === ancestor.collection.ID) {
    for (const child of ancestor.children) {
      if (this.isDescendant(potentialDescendant, child)) {
  // Permission checking methods
  canDelete(collection: MJCollectionEntity): boolean {
    return permission?.canDelete || false;
  canCreateAtRoot(): boolean {
    // Anyone can create at root level
