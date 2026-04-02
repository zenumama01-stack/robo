 * Tree Component for @memberjunction/ng-trees
 * A generic, reusable tree component for displaying hierarchical entity data.
    TreeJunctionConfig,
    TreeKeyboardConfig,
    createDefaultTreeNode
    BeforeNodeDeselectEventArgs,
    AfterNodeDeselectEventArgs,
    BeforeNodeExpandEventArgs,
    AfterNodeExpandEventArgs,
    BeforeNodeCollapseEventArgs,
    AfterNodeCollapseEventArgs,
    BeforeNodeClickEventArgs,
    AfterNodeClickEventArgs,
    BeforeNodeDoubleClickEventArgs,
    AfterNodeDoubleClickEventArgs,
    BeforeKeyboardNavigateEventArgs,
    AfterKeyboardNavigateEventArgs
    selector: 'mj-tree',
    templateUrl: './tree.component.html',
    styleUrls: ['./tree.component.css']
export class TreeComponent implements OnInit, OnDestroy {
    // Configuration Inputs with Getter/Setter
    private _branchConfig: TreeBranchConfig | null = null;
    set BranchConfig(value: TreeBranchConfig) {
        const previousValue = this._branchConfig;
        this._branchConfig = value;
        // Only reload if config actually changed and we have a valid config
        if (value && previousValue !== value && this.isInitialized) {
            if (this._autoLoad) {
    get BranchConfig(): TreeBranchConfig {
        return this._branchConfig!;
    private _leafConfig?: TreeLeafConfig;
    set LeafConfig(value: TreeLeafConfig | undefined) {
        const previousValue = this._leafConfig;
        this._leafConfig = value;
        // Only reload if config actually changed
        if (previousValue !== value && this._branchConfig && this.isInitialized) {
    get LeafConfig(): TreeLeafConfig | undefined {
        return this._leafConfig;
    /** Selection mode: 'single', 'multiple', or 'none' */
    private _selectionMode: TreeSelectionMode = 'single';
    set SelectionMode(value: TreeSelectionMode) {
    get SelectionMode(): TreeSelectionMode {
    private _selectableTypes: TreeSelectableTypes = 'both';
    set SelectableTypes(value: TreeSelectableTypes) {
        this._selectableTypes = value;
    get SelectableTypes(): TreeSelectableTypes {
        return this._selectableTypes;
    /** Initially selected node IDs */
    private _selectedIDs: string[] = [];
    set SelectedIDs(value: string[]) {
        const previousValue = this._selectedIDs;
        this._selectedIDs = value || [];
        // Only sync selection if the IDs actually changed
        if (this.isInitialized && this.IsLoaded && JSON.stringify(previousValue) !== JSON.stringify(value)) {
            this.syncSelectionFromIDs();
    get SelectedIDs(): string[] {
        return this._selectedIDs;
    /** Initially expanded node IDs (null = auto-expand first level) */
    private _expandedIDs: string[] | null = null;
    set ExpandedIDs(value: string[] | null) {
        this._expandedIDs = value;
    get ExpandedIDs(): string[] | null {
        return this._expandedIDs;
    /** Show expand/collapse all buttons */
    private _showExpandCollapseAll: boolean = false;
    set ShowExpandCollapseAll(value: boolean) {
        this._showExpandCollapseAll = value;
    get ShowExpandCollapseAll(): boolean {
        return this._showExpandCollapseAll;
    /** Enable keyboard navigation */
    private _enableKeyboardNavigation: boolean = true;
    set EnableKeyboardNavigation(value: boolean) {
        this._enableKeyboardNavigation = value;
    get EnableKeyboardNavigation(): boolean {
        return this._enableKeyboardNavigation;
    /** Keyboard configuration */
    private _keyboardConfig: TreeKeyboardConfig = {};
    set KeyboardConfig(value: TreeKeyboardConfig) {
        this._keyboardConfig = value || {};
    get KeyboardConfig(): TreeKeyboardConfig {
        return this._keyboardConfig;
    /** CSS style overrides */
    private _styleConfig: TreeStyleConfig = {};
    set StyleConfig(value: TreeStyleConfig) {
        this._styleConfig = value || {};
    get StyleConfig(): TreeStyleConfig {
        return this._styleConfig;
    /** Indent per level in pixels */
    private _indentSize: number = 20;
    set IndentSize(value: number) {
        this._indentSize = value;
    get IndentSize(): number {
        return this._indentSize;
    /** Show loading indicator */
    private _showLoading: boolean = true;
    set ShowLoading(value: boolean) {
        this._showLoading = value;
    get ShowLoading(): boolean {
        return this._showLoading;
    private _emptyMessage: string = 'No items found';
    set EmptyMessage(value: string) {
        this._emptyMessage = value;
    get EmptyMessage(): string {
        return this._emptyMessage;
    /** Empty state icon */
    private _emptyIcon: string = 'fa-solid fa-folder-open';
    set EmptyIcon(value: string) {
        this._emptyIcon = value;
    get EmptyIcon(): string {
        return this._emptyIcon;
    private _autoLoad: boolean = true;
    set AutoLoad(value: boolean) {
        this._autoLoad = value;
    get AutoLoad(): boolean {
        return this._autoLoad;
    /** Show node icons */
    private _showIcons: boolean = true;
    set ShowIcons(value: boolean) {
        this._showIcons = value;
    get ShowIcons(): boolean {
        return this._showIcons;
    /** Show node descriptions */
    private _showDescriptions: boolean = false;
    set ShowDescriptions(value: boolean) {
        this._showDescriptions = value;
    get ShowDescriptions(): boolean {
        return this._showDescriptions;
    /** Show node badges */
    private _showBadges: boolean = true;
    set ShowBadges(value: boolean) {
        this._showBadges = value;
    get ShowBadges(): boolean {
        return this._showBadges;
    /** Animate expand/collapse */
    private _animateExpandCollapse: boolean = true;
    set AnimateExpandCollapse(value: boolean) {
        this._animateExpandCollapse = value;
    get AnimateExpandCollapse(): boolean {
        return this._animateExpandCollapse;
    @Output() BeforeNodeDeselect = new EventEmitter<BeforeNodeDeselectEventArgs>();
    @Output() AfterNodeDeselect = new EventEmitter<AfterNodeDeselectEventArgs>();
    @Output() BeforeNodeExpand = new EventEmitter<BeforeNodeExpandEventArgs>();
    @Output() AfterNodeExpand = new EventEmitter<AfterNodeExpandEventArgs>();
    @Output() BeforeNodeCollapse = new EventEmitter<BeforeNodeCollapseEventArgs>();
    @Output() AfterNodeCollapse = new EventEmitter<AfterNodeCollapseEventArgs>();
    @Output() BeforeNodeClick = new EventEmitter<BeforeNodeClickEventArgs>();
    @Output() AfterNodeClick = new EventEmitter<AfterNodeClickEventArgs>();
    @Output() BeforeNodeDoubleClick = new EventEmitter<BeforeNodeDoubleClickEventArgs>();
    @Output() AfterNodeDoubleClick = new EventEmitter<AfterNodeDoubleClickEventArgs>();
    @Output() BeforeKeyboardNavigate = new EventEmitter<BeforeKeyboardNavigateEventArgs>();
    @Output() AfterKeyboardNavigate = new EventEmitter<AfterKeyboardNavigateEventArgs>();
    /** Emitted when selection changes (convenience event with just the selected nodes) */
    @Output() SelectionChange = new EventEmitter<TreeNode[]>();
    /** Root nodes of the tree */
    public Nodes: TreeNode[] = [];
    /** All branch nodes (flat) */
    public AllBranches: TreeNode[] = [];
    /** All leaf nodes (flat) */
    public AllLeaves: TreeNode[] = [];
    /** Currently selected nodes */
    /** Currently focused node (for keyboard navigation) */
    public FocusedNode: TreeNode | null = null;
    /** Has data been loaded */
    /** Current search text (for highlighting) */
    public CurrentSearchText: string = '';
    private nodeMap: Map<string, TreeNode> = new Map();
    private isCurrentlyLoading: boolean = false;
    /** Instance-level cache for loaded data */
    private cachedNodes: TreeNode[] | null = null;
    private cachedBranches: TreeNode[] | null = null;
    private cachedLeaves: TreeNode[] | null = null;
        private readonly elementRef: ElementRef
        if (this._autoLoad && this._branchConfig) {
     * Reload tree data (forces fresh load from database)
        await this.loadData(true);
     * Get currently selected nodes
    public GetSelectedNodes(): TreeNode[] {
        return [...this.SelectedNodes];
     * Get selected IDs
    public GetSelectedIDs(): string[] {
        return this.SelectedNodes.map(n => n.ID);
     * Programmatically select node(s)
     * @param emitChange Whether to emit SelectionChange event (default: true).
     *                   Set to false during sync operations to avoid unnecessary events.
    public SelectNodes(ids: string[], emitChange: boolean = true): void {
        const previousSelection = [...this.SelectedNodes];
        const previousIds = previousSelection.map(n => n.ID).sort().join(',');
        if (this._selectionMode === 'single' && ids.length > 0) {
            ids = [ids[0]];
            const node = this.nodeMap.get(id);
            if (node && this.isNodeSelectable(node)) {
                node.Selected = true;
                this.SelectedNodes.push(node);
        // Deselect previously selected nodes not in new selection
        for (const prev of previousSelection) {
            if (!this.SelectedNodes.includes(prev)) {
                prev.Selected = false;
        // Only emit if selection actually changed
        const newIds = this.SelectedNodes.map(n => n.ID).sort().join(',');
        if (emitChange && previousIds !== newIds) {
            this.SelectionChange.emit([...this.SelectedNodes]);
        for (const node of this.SelectedNodes) {
            node.Selected = false;
        this.SelectionChange.emit([]);
    public ExpandAll(): void {
        this.expandAllRecursive(this.Nodes);
    public CollapseAll(): void {
        this.collapseAllRecursive(this.Nodes);
     * Expand to specific node (expands all ancestors)
    public ExpandToNode(id: string): TreeNode | null {
        const target = this.nodeMap.get(id);
        // Walk up the tree and expand each ancestor
        let current = target;
        while (current.ParentID && this.nodeMap.has(current.ParentID)) {
            const parent = this.nodeMap.get(current.ParentID)!;
            parent.Expanded = true;
     * Find node by ID
    public FindNode(id: string): TreeNode | null {
        return this.nodeMap.get(id) || null;
     * Get all nodes matching a predicate
    public FindNodes(predicate: (node: TreeNode) => boolean): TreeNode[] {
        const results: TreeNode[] = [];
        for (const node of this.nodeMap.values()) {
            if (predicate(node)) {
                results.push(node);
     * Filter nodes by search text
    public FilterNodes(
            caseSensitive?: boolean;
            searchBranches?: boolean;
            searchLeaves?: boolean;
            searchDescription?: boolean;
        } = {}
    ): TreeNode[] {
            caseSensitive = false,
            searchBranches = true,
            searchLeaves = true,
            searchDescription = false
        // Store search text for highlighting
        this.CurrentSearchText = searchText.trim();
            // Reset all nodes to visible
            this.setVisibilityRecursive(this.Nodes, true, false);
        const searchLower = caseSensitive ? searchText : searchText.toLowerCase();
        const matchedNodes: TreeNode[] = [];
        // First pass: mark matching nodes
        this.markMatchingNodesRecursive(
            this.Nodes,
            searchLower,
            caseSensitive,
            searchBranches,
            searchLeaves,
            searchDescription,
            matchedNodes
        // Second pass: make ancestors of matches visible
        this.makeAncestorsVisibleRecursive(this.Nodes);
        return matchedNodes;
     * Clear cache (call before Refresh if you want fresh data)
     * Handle node click
    public onNodeClick(node: TreeNode, event: MouseEvent): void {
        const beforeClickEvent = new BeforeNodeClickEventArgs(this, node, event);
        this.BeforeNodeClick.emit(beforeClickEvent);
        if (beforeClickEvent.Cancel) {
        // Set focus
        this.FocusedNode = node;
        // Handle toggle for branches
        if (node.Type === 'branch' && node.Children.length > 0) {
            // Click on the chevron area toggles expansion
            if (target.closest('.tree-node-toggle')) {
                this.toggleNodeExpansion(node);
        if (this._selectionMode !== 'none' && this.isNodeSelectable(node)) {
            this.handleNodeSelection(node, event);
        const afterClickEvent = new AfterNodeClickEventArgs(this, node, event);
        this.AfterNodeClick.emit(afterClickEvent);
     * Handle node double-click
    public onNodeDoubleClick(node: TreeNode, event: MouseEvent): void {
        const beforeEvent = new BeforeNodeDoubleClickEventArgs(this, node, event);
        this.BeforeNodeDoubleClick.emit(beforeEvent);
        // Toggle expansion for branches
        if (node.Type === 'branch') {
        const afterEvent = new AfterNodeDoubleClickEventArgs(this, node, event);
        this.AfterNodeDoubleClick.emit(afterEvent);
     * Handle toggle click
    public onToggleClick(node: TreeNode, event: MouseEvent): void {
    public onKeyDown(event: KeyboardEvent): void {
        if (!this._enableKeyboardNavigation) {
        const visibleNodes = this.getVisibleNodesInOrder(this.Nodes);
        if (visibleNodes.length === 0) {
        const currentIndex = this.FocusedNode
            ? visibleNodes.indexOf(this.FocusedNode)
            : -1;
        let targetNode: TreeNode | null = null;
        let handled = false;
                if (currentIndex < visibleNodes.length - 1) {
                    targetNode = visibleNodes[currentIndex + 1];
                handled = true;
                    targetNode = visibleNodes[currentIndex - 1];
                } else if (currentIndex === -1 && visibleNodes.length > 0) {
                    targetNode = visibleNodes[visibleNodes.length - 1];
                if (this.FocusedNode?.Type === 'branch') {
                    if (!this.FocusedNode.Expanded && this.FocusedNode.Children.length > 0) {
                        this.expandNode(this.FocusedNode);
                    } else if (this.FocusedNode.Expanded && this.FocusedNode.Children.length > 0) {
                        // Move to first child
                        const firstVisible = this.FocusedNode.Children.find(c => c.Visible);
                        if (firstVisible) {
                            targetNode = firstVisible;
                if (this.FocusedNode?.Type === 'branch' && this.FocusedNode.Expanded) {
                    this.collapseNode(this.FocusedNode);
                } else if (this.FocusedNode?.ParentID) {
                    // Move to parent
                    targetNode = this.nodeMap.get(this.FocusedNode.ParentID) || null;
                if (this.FocusedNode && this._selectionMode !== 'none' && this.isNodeSelectable(this.FocusedNode)) {
                    this.handleNodeSelection(this.FocusedNode, event as unknown as MouseEvent);
                    targetNode = visibleNodes[0];
        if (handled) {
        if (targetNode && targetNode !== this.FocusedNode) {
            // Fire before navigate event
            const beforeEvent = new BeforeKeyboardNavigateEventArgs(
                this.FocusedNode,
                targetNode
            this.BeforeKeyboardNavigate.emit(beforeEvent);
            if (!beforeEvent.Cancel) {
                const previousNode = this.FocusedNode;
                this.FocusedNode = targetNode;
                // Fire after navigate event
                const afterEvent = new AfterKeyboardNavigateEventArgs(
                    event.key,
                    previousNode,
                    this.FocusedNode
                this.AfterKeyboardNavigate.emit(afterEvent);
                this.scrollNodeIntoView(targetNode);
     * Load tree data
    private async loadData(forceRefresh: boolean): Promise<void> {
        if (!this._branchConfig) {
            console.warn('[TreeComponent] loadData() - BranchConfig is required');
        // Prevent concurrent loads
        if (this.isCurrentlyLoading) {
        // Check instance cache first (unless force refresh)
        if (!forceRefresh && this.cachedNodes) {
            this.Nodes = this.cloneNodes(this.cachedNodes);
            this.AllBranches = this.cloneNodes(this.cachedBranches || []);
            this.AllLeaves = this.cloneNodes(this.cachedLeaves || []);
            this.nodeMap = this.buildNodeMap(this.Nodes);
            this.applyInitialExpansion();
        const beforeEvent = new BeforeDataLoadEventArgs(this, this._branchConfig, this._leafConfig);
        this.BeforeDataLoad.emit(beforeEvent);
        this.isCurrentlyLoading = true;
        // Apply any modified filters from event
        const branchConfig = { ...this._branchConfig };
        if (beforeEvent.ModifiedBranchFilter !== undefined) {
            branchConfig.ExtraFilter = beforeEvent.ModifiedBranchFilter;
        const leafConfig = this._leafConfig ? { ...this._leafConfig } : undefined;
        if (leafConfig && beforeEvent.ModifiedLeafFilter !== undefined) {
            leafConfig.ExtraFilter = beforeEvent.ModifiedLeafFilter;
            // Load branches and leaves in parallel
            const [branchData, leafData] = await Promise.all([
                this.loadBranches(branchConfig),
                leafConfig ? this.loadLeaves(leafConfig) : Promise.resolve([])
            const { rootNodes, allBranches, branchMap } = this.buildBranchHierarchy(branchData, branchConfig);
            // Load junction mappings if configured (for M2M relationships)
            let junctionMappings: Map<string, string[]> | null = null;
            if (leafConfig?.JunctionConfig) {
                junctionMappings = await this.loadJunctionMappings(leafConfig.JunctionConfig, leafConfig.IDField || 'ID');
            // Attach leaves to branches (or root if orphans)
            const allLeaves = this.attachLeavesToBranches(rootNodes, branchMap, leafData, leafConfig, junctionMappings);
            // Store in instance cache
            this.cachedNodes = this.cloneNodes(rootNodes);
            this.cachedBranches = this.cloneNodes(allBranches);
            this.cachedLeaves = this.cloneNodes(allLeaves);
            // Set state
            this.Nodes = rootNodes;
            this.AllBranches = allBranches;
            this.AllLeaves = allLeaves;
            this.isCurrentlyLoading = false;
            // Apply initial expansion
            // Apply initial selection
            const afterEvent = new AfterDataLoadEventArgs(
                allBranches.length,
                allLeaves.length,
            this.AfterDataLoad.emit(afterEvent);
            console.error('[TreeComponent] loadData() error:', errorMessage, error);
            this.ErrorMessage = errorMessage;
            this.Nodes = [];
            this.AllBranches = [];
            this.AllLeaves = [];
            // Fire after event with error
     * Load branch entities from database
    private async loadBranches(config: TreeBranchConfig): Promise<Record<string, unknown>[]> {
            EntityName: config.EntityName,
            ExtraFilter: config.ExtraFilter || '',
            OrderBy: config.OrderBy || 'Name ASC',
            CacheLocal: config.CacheLocal ?? true
        console.log('[TreeComponent] Branches query result:', {
            filter: config.ExtraFilter || '(none)',
            cacheLocal: config.CacheLocal ?? true,
            recordCount: result.Results?.length || 0,
            records: result.Results?.map((r: Record<string, unknown>) => ({ ID: r['ID'], Name: r['Name'] }))
            throw new Error(`Failed to load branches: ${result.ErrorMessage}`);
     * Load leaf entities from database
    private async loadLeaves(config: TreeLeafConfig): Promise<Record<string, unknown>[]> {
        console.log('[TreeComponent] Leaves query result:', {
            throw new Error(`Failed to load leaves: ${result.ErrorMessage}`);
     * Load junction mappings for M2M relationships.
     * Returns a map of leafId -> branchIds[]
    private async loadJunctionMappings(
        junctionConfig: TreeJunctionConfig,
        leafIdField: string
        const mappings = new Map<string, string[]>();
        // Load junction records
        const junctionResult = await rv.RunView({
            EntityName: junctionConfig.EntityName,
            ExtraFilter: junctionConfig.ExtraFilter || '',
            CacheLocal: junctionConfig.CacheLocal ?? true
        console.log('[TreeComponent] Junction query result:', {
            entityName: junctionConfig.EntityName,
            filter: junctionConfig.ExtraFilter || '(none)',
            cacheLocal: junctionConfig.CacheLocal ?? true,
            success: junctionResult.Success,
            recordCount: junctionResult.Results?.length || 0,
            records: junctionResult.Results
        if (!junctionResult.Success) {
            console.warn(`Failed to load junction data: ${junctionResult.ErrorMessage}`);
            return mappings;
        const junctionRecords = junctionResult.Results as Record<string, unknown>[];
        // If there's an indirect mapping, we need to resolve it
        if (junctionConfig.IndirectLeafMapping) {
            const indirect = junctionConfig.IndirectLeafMapping;
            // Load the intermediate entity records to build the mapping
            const intermediateResult = await rv.RunView({
                EntityName: indirect.IntermediateEntity,
                ExtraFilter: indirect.ExtraFilter || '',
                CacheLocal: indirect.CacheLocal ?? true
            console.log('[TreeComponent] Intermediate entity query result:', {
                entityName: indirect.IntermediateEntity,
                filter: indirect.ExtraFilter || '(none)',
                cacheLocal: indirect.CacheLocal ?? true,
                success: intermediateResult.Success,
                recordCount: intermediateResult.Results?.length || 0,
                records: intermediateResult.Results
            if (!intermediateResult.Success) {
                console.warn(`Failed to load intermediate data: ${intermediateResult.ErrorMessage}`);
            // Build intermediate ID -> leaf ID map
            const intermediateToLeaf = new Map<string, string>();
            for (const record of intermediateResult.Results as Record<string, unknown>[]) {
                const intermediateId = String(record[indirect.IntermediateIDField] || '');
                const leafId = String(record[indirect.LeafIDField] || '');
                if (intermediateId && leafId) {
                    intermediateToLeaf.set(intermediateId, leafId);
            console.log('[TreeComponent] Intermediate to leaf mapping:', {
                mapSize: intermediateToLeaf.size,
                entries: Array.from(intermediateToLeaf.entries())
            // Now process junction records using the intermediate mapping
            for (const junction of junctionRecords) {
                const intermediateId = String(junction[junctionConfig.LeafForeignKey] || '');
                const branchId = String(junction[junctionConfig.BranchForeignKey] || '');
                if (intermediateId && branchId) {
                    const leafId = intermediateToLeaf.get(intermediateId);
                    if (leafId) {
                        if (!mappings.has(leafId)) {
                            mappings.set(leafId, []);
                        const branchIds = mappings.get(leafId)!;
                        if (!branchIds.includes(branchId)) {
                            branchIds.push(branchId);
            // Direct mapping - junction directly references the leaf
                const leafId = String(junction[junctionConfig.LeafForeignKey] || '');
                if (leafId && branchId) {
        console.log('[TreeComponent] Final junction mappings (leafId -> branchIds):', {
            mapSize: mappings.size,
            entries: Array.from(mappings.entries())
     * Build branch hierarchy from flat data
    private buildBranchHierarchy(
        branchData: Record<string, unknown>[],
        config: TreeBranchConfig
    ): { rootNodes: TreeNode[]; allBranches: TreeNode[]; branchMap: Map<string, TreeNode> } {
        const idField = config.IDField || 'ID';
        const parentIdField = config.ParentIDField || 'ParentID';
        const displayField = config.DisplayField || 'Name';
        const nodeMap = new Map<string, TreeNode>();
        const allBranches: TreeNode[] = [];
        for (const data of branchData) {
            const id = String(data[idField] || '');
            const parentId = data[parentIdField] ? String(data[parentIdField]) : null;
                Label: String(data[displayField] || ''),
                ParentID: parentId,
                Icon: this.getNodeIcon(data, config.IconField, config.DefaultIcon || 'fa-solid fa-folder'),
                Color: this.getNodeColor(data, config.ColorField, config.DefaultColor),
                Data: { ...data },
                Description: config.DescriptionField ? String(data[config.DescriptionField] || '') : undefined,
                Badge: config.BadgeField ? String(data[config.BadgeField] || '') : undefined,
                EntityName: config.EntityName
            nodeMap.set(id, node);
            allBranches.push(node);
        // Build parent-child relationships
        const rootNodes: TreeNode[] = [];
        for (const node of allBranches) {
                const parent = nodeMap.get(node.ParentID)!;
                parent.Children.push(node);
                node.Level = this.calculateLevel(node, nodeMap);
                node.Level = 0;
        // Sort children at each level
        this.sortChildrenRecursive(rootNodes);
        return { rootNodes, allBranches, branchMap: nodeMap };
     * Attach leaf nodes to their parent branches or root level.
     * Supports both direct parent field relationships and M2M junction mappings.
     * @param rootNodes Root nodes to attach orphan leaves to
     * @param branchMap Map of branch IDs to branch nodes
     * @param leafData Raw leaf data from database
     * @param config Leaf configuration
     * @param junctionMappings Optional M2M mappings (leafId -> branchIds[])
    private attachLeavesToBranches(
        rootNodes: TreeNode[],
        branchMap: Map<string, TreeNode>,
        leafData: Record<string, unknown>[],
        config?: TreeLeafConfig,
        junctionMappings?: Map<string, string[]> | null
        if (!config || leafData.length === 0) {
        const parentField = config.ParentField;
        const useJunction = !!junctionMappings && junctionMappings.size > 0;
        console.log('[TreeComponent] attachLeavesToBranches input:', {
            leafDataCount: leafData.length,
            leafIds: leafData.map((d: Record<string, unknown>) => ({ id: d[idField], name: d[displayField] })),
            branchMapKeys: Array.from(branchMap.keys()),
            branchMapEntries: Array.from(branchMap.entries()).map(([k, v]) => ({ id: k, name: v.Label })),
            useJunction,
            junctionMappingKeys: junctionMappings ? Array.from(junctionMappings.keys()) : []
        const allLeaves: TreeNode[] = [];
        const addedLeafIds = new Set<string>(); // Track leaves already added to avoid duplicates
        for (const data of leafData) {
            // If using junction mappings, only include leaves that have junction entries
            if (useJunction && !junctionMappings!.has(id)) {
                console.log(`[TreeComponent] Skipping leaf ${id} (${data[displayField]}) - not in junction mappings`);
                continue; // Skip leaves not in any branch via junction
            const leaf = createDefaultTreeNode({
                ParentID: null, // Will be set based on attachment
                Icon: this.getNodeIcon(data, config.IconField, config.DefaultIcon || 'fa-solid fa-file'),
            allLeaves.push(leaf);
            console.log(`[TreeComponent] Processing leaf ${id} (${leaf.Label})`);
            if (useJunction) {
                // M2M relationship: attach to all mapped branches
                const branchIds = junctionMappings!.get(id) || [];
                let attached = false;
                console.log(`[TreeComponent] Leaf ${id} junction branchIds:`, branchIds);
                for (const branchId of branchIds) {
                    if (branchMap.has(branchId)) {
                        const parent = branchMap.get(branchId)!;
                        console.log(`[TreeComponent] Attaching leaf ${id} to branch ${branchId} (${parent.Label})`);
                        if (!attached) {
                            // First attachment: use the original leaf
                            leaf.ParentID = branchId;
                            leaf.Level = parent.Level + 1;
                            parent.Children.push(leaf);
                            attached = true;
                            // Additional attachments: create a clone of the leaf
                            // This allows the same artifact to appear under multiple collections
                            const leafClone = createDefaultTreeNode({
                                ...leaf,
                                ParentID: branchId,
                                Level: parent.Level + 1,
                                Data: { ...leaf.Data }
                            parent.Children.push(leafClone);
                        console.log(`[TreeComponent] Branch ${branchId} NOT FOUND in branchMap for leaf ${id}`);
                // If no valid branch found, add to root
                    console.log(`[TreeComponent] Leaf ${id} not attached to any branch, adding to root`);
                    rootNodes.push(leaf);
                    leaf.Level = 0;
                // Direct parent field relationship
                const parentFieldValue = parentField ? data[parentField] : null;
                const parentId = parentFieldValue ? String(parentFieldValue) : null;
                leaf.ParentID = parentId;
                if (parentId && branchMap.has(parentId)) {
                    const parent = branchMap.get(parentId)!;
                    // Orphan leaf (no parent or parent not found) - add to root level
        // Re-sort children to interleave branches and leaves properly
        this.sortChildrenByTypeAndName(rootNodes);
        for (const branch of branchMap.values()) {
            this.sortChildrenByTypeAndName(branch.Children);
        return allLeaves;
     * Calculate node level in hierarchy
    private calculateLevel(node: TreeNode, nodeMap: Map<string, TreeNode>): number {
        let level = 0;
        let current = node;
        while (current.ParentID && nodeMap.has(current.ParentID)) {
            level++;
            current = nodeMap.get(current.ParentID)!;
            if (level > 100) break; // Safety for circular refs
     * Sort children recursively (alphabetically)
    private sortChildrenRecursive(nodes: TreeNode[]): void {
        nodes.sort((a, b) => a.Label.localeCompare(b.Label));
            if (node.Children.length > 0) {
                this.sortChildrenRecursive(node.Children);
     * Sort children: branches first (alphabetically), then leaves (alphabetically)
    private sortChildrenByTypeAndName(nodes: TreeNode[]): void {
        nodes.sort((a, b) => {
            if (a.Type !== b.Type) {
                return a.Type === 'branch' ? -1 : 1;
            return a.Label.localeCompare(b.Label);
     * Get icon for a node from field or default
    private getNodeIcon(
        iconField?: string,
        defaultIcon: string = 'fa-solid fa-folder'
        if (iconField && data[iconField]) {
            return String(data[iconField]);
        return defaultIcon;
     * Get color for a node from field or default
    private getNodeColor(
        colorField?: string,
        defaultColor?: string
        if (colorField && data[colorField]) {
            return String(data[colorField]);
        return defaultColor;
     * Clear instance cache
    private clearCache(): void {
        this.cachedNodes = null;
        this.cachedBranches = null;
        this.cachedLeaves = null;
     * Deep clone tree nodes
    private cloneNodes(nodes: TreeNode[]): TreeNode[] {
        return nodes.map(node => this.cloneNode(node));
    private cloneNode(node: TreeNode): TreeNode {
            Data: { ...node.Data },
            Children: this.cloneNodes(node.Children)
     * Build a flat map of all nodes by ID
    private buildNodeMap(nodes: TreeNode[]): Map<string, TreeNode> {
        const map = new Map<string, TreeNode>();
        this.addNodesToMapRecursive(nodes, map);
    private addNodesToMapRecursive(nodes: TreeNode[], map: Map<string, TreeNode>): void {
            map.set(node.ID, node);
            this.addNodesToMapRecursive(node.Children, map);
    // Private Methods - Tree Operations
     * Apply initial expansion state
    private applyInitialExpansion(): void {
        if (this._expandedIDs === null) {
            // Auto-expand first level
                    node.Expanded = true;
            // Expand specified nodes
            for (const id of this._expandedIDs) {
                if (node && node.Type === 'branch') {
     * Sync selection state from SelectedIDs input
    private syncSelectionFromIDs(): void {
        // Clear current selection
        // Apply new selection
        for (const id of this._selectedIDs) {
     * Check if a node can be selected based on SelectableTypes
    private isNodeSelectable(node: TreeNode): boolean {
        if (this._selectableTypes === 'both') return true;
        return node.Type === this._selectableTypes;
     * Handle node selection logic
    private handleNodeSelection(node: TreeNode, event: MouseEvent | KeyboardEvent): void {
        const isCtrlClick = event.ctrlKey || event.metaKey;
        const isSelected = node.Selected;
        if (isSelected) {
            const beforeEvent = new BeforeNodeDeselectEventArgs(this, node, previousSelection);
            this.BeforeNodeDeselect.emit(beforeEvent);
            this.SelectedNodes = this.SelectedNodes.filter(n => n !== node);
            const afterEvent = new AfterNodeDeselectEventArgs(
                [...this.SelectedNodes],
            this.AfterNodeDeselect.emit(afterEvent);
            // Select
            const isAdditive = this._selectionMode === 'multiple' && isCtrlClick;
            const beforeEvent = new BeforeNodeSelectEventArgs(
                isAdditive,
            this.BeforeNodeSelect.emit(beforeEvent);
            if (this._selectionMode === 'single' || !isAdditive) {
                // Clear previous selection
                for (const prev of this.SelectedNodes) {
            const afterEvent = new AfterNodeSelectEventArgs(
            this.AfterNodeSelect.emit(afterEvent);
    private toggleNodeExpansion(node: TreeNode): void {
        if (node.Expanded) {
            this.collapseNode(node);
            this.expandNode(node);
     * Expand a node
    private expandNode(node: TreeNode): void {
        if (node.Type !== 'branch' || node.Children.length === 0) {
        const beforeEvent = new BeforeNodeExpandEventArgs(this, node);
        this.BeforeNodeExpand.emit(beforeEvent);
        const afterEvent = new AfterNodeExpandEventArgs(
            node.Children.filter(c => c.Visible).length
        this.AfterNodeExpand.emit(afterEvent);
     * Collapse a node
    private collapseNode(node: TreeNode): void {
        if (node.Type !== 'branch') {
        const beforeEvent = new BeforeNodeCollapseEventArgs(this, node);
        this.BeforeNodeCollapse.emit(beforeEvent);
        node.Expanded = false;
        const afterEvent = new AfterNodeCollapseEventArgs(this, node);
        this.AfterNodeCollapse.emit(afterEvent);
     * Expand all nodes recursively
    private expandAllRecursive(nodes: TreeNode[]): void {
                this.expandAllRecursive(node.Children);
     * Collapse all nodes recursively
    private collapseAllRecursive(nodes: TreeNode[]): void {
                this.collapseAllRecursive(node.Children);
     * Get all visible nodes in tree order
     * Scroll a node into view
    private scrollNodeIntoView(node: TreeNode): void {
            `[data-node-id="${node.ID}"]`
            element.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
    // Private Methods - Search/Filter
     * Set visibility recursively
    private setVisibilityRecursive(nodes: TreeNode[], visible: boolean, matchesSearch: boolean): void {
            node.Visible = visible;
            node.MatchesSearch = matchesSearch;
            this.setVisibilityRecursive(node.Children, visible, matchesSearch);
     * Recursively mark nodes that match the search
    private markMatchingNodesRecursive(
        nodes: TreeNode[],
        caseSensitive: boolean,
        searchBranches: boolean,
        searchLeaves: boolean,
        searchDescription: boolean,
        let hasVisibleChild = false;
            const shouldSearch =
                (node.Type === 'branch' && searchBranches) ||
                (node.Type === 'leaf' && searchLeaves);
            let matches = false;
            if (shouldSearch) {
                const label = caseSensitive ? node.Label : node.Label.toLowerCase();
                matches = label.includes(searchText);
                if (!matches && searchDescription && node.Description) {
                    const desc = caseSensitive ? node.Description : node.Description.toLowerCase();
                    matches = desc.includes(searchText);
            node.MatchesSearch = matches;
            const childHasMatch = this.markMatchingNodesRecursive(
                node.Children,
            node.Visible = matches || childHasMatch;
                matchedNodes.push(node);
                hasVisibleChild = true;
        return hasVisibleChild;
     * Make ancestors of visible nodes also visible
    private makeAncestorsVisibleRecursive(nodes: TreeNode[]): boolean {
            const childVisible = this.makeAncestorsVisibleRecursive(node.Children);
            if (childVisible && !node.Visible) {
                node.Visible = true;
     * Get the padding for a node based on its level
    public getNodePadding(node: TreeNode): string {
        const basePadding = node.Level * this._indentSize;
        // Root-level leaves need a small indent since they have no toggle button
        // to align them with nested items
        if (node.Type === 'leaf' && node.Level === 0) {
            return `${basePadding + 8}px`;
        return `${basePadding}px`;
     * Get CSS classes for a node
    public getNodeClasses(node: TreeNode): Record<string, boolean> {
            'tree-node': true,
            'tree-node--branch': node.Type === 'branch',
            'tree-node--leaf': node.Type === 'leaf',
            'tree-node--root-level': node.Level === 0,
            'tree-node--selected': node.Selected,
            'tree-node--focused': node === this.FocusedNode,
            'tree-node--expanded': node.Expanded,
            'tree-node--has-children': node.Children.length > 0,
            'tree-node--match': node.MatchesSearch,
            [this._styleConfig.NodeClass || '']: !!this._styleConfig.NodeClass,
            [this._styleConfig.SelectedClass || '']: node.Selected && !!this._styleConfig.SelectedClass,
            [this._styleConfig.BranchClass || '']: node.Type === 'branch' && !!this._styleConfig.BranchClass,
            [this._styleConfig.LeafClass || '']: node.Type === 'leaf' && !!this._styleConfig.LeafClass,
            [this._styleConfig.ExpandedClass || '']: node.Expanded && !!this._styleConfig.ExpandedClass
     * Get container classes
    public getContainerClasses(): Record<string, boolean> {
            'tree-container': true,
            'tree-container--loading': this.IsLoading,
            'tree-container--empty': this.IsLoaded && this.Nodes.length === 0,
            [this._styleConfig.ContainerClass || '']: !!this._styleConfig.ContainerClass
     * Track nodes for ngFor
    public trackNode(index: number, node: TreeNode): string {
     * Get label HTML with search text highlighted
    public getHighlightedLabel(node: TreeNode): string {
        const label = node.Label;
        // No search text or node doesn't match - return plain label (escaped)
        if (!this.CurrentSearchText || !node.MatchesSearch) {
            return this.escapeHtml(label);
        // Find and highlight the matching portion (case-insensitive)
        const searchLower = this.CurrentSearchText.toLowerCase();
        const labelLower = label.toLowerCase();
        const matchIndex = labelLower.indexOf(searchLower);
        if (matchIndex === -1) {
        // Split into before, match, and after parts
        const before = label.substring(0, matchIndex);
        const match = label.substring(matchIndex, matchIndex + this.CurrentSearchText.length);
        const after = label.substring(matchIndex + this.CurrentSearchText.length);
        return `${this.escapeHtml(before)}<mark class="tree-search-highlight">${this.escapeHtml(match)}</mark>${this.escapeHtml(after)}`;
