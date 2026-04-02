  HostListener
import { ActionExplorerStateService } from '../../services/action-explorer-state.service';
  totalActionCount: number; // Including descendants
  selector: 'mj-action-tree-panel',
  templateUrl: './action-tree-panel.component.html',
  styleUrls: ['./action-tree-panel.component.css'],
export class ActionTreePanelComponent implements OnInit, OnDestroy {
  @Input() Actions: ActionEntityExtended[] = [];
  @Output() CategorySelected = new EventEmitter<string>();
  @Output() NewCategoryClick = new EventEmitter<string | null>(); // parent ID or null for root
  @Output() EditCategoryClick = new EventEmitter<MJActionCategoryEntity>();
  public CategoryTree: CategoryTreeNode[] = [];
  public ExpandedCategories: Set<string> = new Set();
  public TreeWidth = 280;
  public IsCollapsed = false;
  public SearchTerm = '';
  public FilteredTree: CategoryTreeNode[] = [];
  private categoryParentMap = new Map<string, string | null>();
  private categoryDescendants = new Map<string, Set<string>>();
    this.StateService.TreeWidth$.pipe(takeUntil(this.destroy$)).subscribe(width => {
      this.TreeWidth = width;
    this.StateService.TreeCollapsed$.pipe(takeUntil(this.destroy$)).subscribe(collapsed => {
      this.IsCollapsed = collapsed;
    this.StateService.SelectedCategoryId$.pipe(takeUntil(this.destroy$)).subscribe(id => {
    this.StateService.ExpandedCategories$.pipe(takeUntil(this.destroy$)).subscribe(expanded => {
      this.ExpandedCategories = expanded;
  public buildCategoryTree(): void {
    this.categoryParentMap.clear();
    // Build action counts per category
    const actionCounts = new Map<string, number>();
    this.Actions.forEach(action => {
      if (action.CategoryID) {
        actionCounts.set(action.CategoryID, (actionCounts.get(action.CategoryID) || 0) + 1);
    this.Categories.forEach(category => {
        level: 0,
        actionCount: actionCounts.get(category.ID) || 0,
        totalActionCount: 0
      this.categoryParentMap.set(category.ID, category.ParentID || null);
    // Build descendant mapping
          const descendants = this.categoryDescendants.get(currentParentId);
            descendants.add(category.ID);
          const parent = categoryMap.get(currentParentId);
          currentParentId = parent?.category.ParentID || null;
    // Calculate total action counts (including descendants)
    const calculateTotalCounts = (node: CategoryTreeNode): number => {
      const descendants = this.categoryDescendants.get(node.category.ID);
        let total = 0;
        descendants.forEach(descId => {
          total += actionCounts.get(descId) || 0;
        node.totalActionCount = total;
      return node.actionCount;
    // Sort children at each level and calculate totals
    const sortAndCalculate = (nodes: CategoryTreeNode[]) => {
        sortAndCalculate(node.children);
        calculateTotalCounts(node);
    sortAndCalculate(rootNodes);
    this.CategoryTree = rootNodes;
    this.FilteredTree = this.SearchTerm ? this.filterTree(rootNodes, this.SearchTerm.toLowerCase()) : rootNodes;
  private filterTree(nodes: CategoryTreeNode[], searchTerm: string): CategoryTreeNode[] {
    const result: CategoryTreeNode[] = [];
      const matchesSearch = node.category.Name.toLowerCase().includes(searchTerm);
      const filteredChildren = this.filterTree(node.children, searchTerm);
      if (matchesSearch || filteredChildren.length > 0) {
          children: filteredChildren
    this.SearchTerm = term;
    this.FilteredTree = term ? this.filterTree(this.CategoryTree, term.toLowerCase()) : this.CategoryTree;
  public getTotalActionCount(): number {
    return this.Actions.length;
  public getUncategorizedCount(): number {
    return this.Actions.filter(a => !a.CategoryID).length;
    this.CategorySelected.emit(categoryId);
  public toggleExpanded(categoryId: string, event: MouseEvent): void {
    this.StateService.toggleCategoryExpanded(categoryId);
  public isExpanded(categoryId: string): boolean {
    return this.ExpandedCategories.has(categoryId);
  public isSelected(categoryId: string): boolean {
    return this.SelectedCategoryId === categoryId;
  public toggleCollapse(): void {
    this.StateService.toggleTreeCollapsed();
  public onNewCategory(parentId: string | null, event: MouseEvent): void {
    this.NewCategoryClick.emit(parentId);
  public onEditCategory(category: MJActionCategoryEntity, event: MouseEvent): void {
    this.EditCategoryClick.emit(category);
    this.StateService.expandAllCategories(this.Categories.map(c => c.ID));
    this.StateService.collapseAllCategories();
  // Resize handling
  public onResizeStart(event: MouseEvent): void {
    document.body.style.cursor = 'ew-resize';
    document.body.style.userSelect = 'none';
    const newWidth = event.clientX;
    const clampedWidth = Math.min(
      Math.max(newWidth, this.StateService.TreeWidthMin),
      Math.min(this.StateService.TreeWidthMax, window.innerWidth - 400)
    this.TreeWidth = clampedWidth;
      document.body.style.cursor = '';
      document.body.style.userSelect = '';
      this.StateService.setTreeWidth(this.TreeWidth);
  public getCategoryDescendants(categoryId: string): Set<string> {
    return this.categoryDescendants.get(categoryId) || new Set([categoryId]);
