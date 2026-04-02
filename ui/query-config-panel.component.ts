 * Configuration panel for Query parts.
 * Uses tree dropdown for category-based query selection.
@RegisterClass(BaseConfigPanel, 'QueryPanelConfigDialog')
    selector: 'mj-query-config-panel',
    templateUrl: './query-config-panel.component.html',
export class QueryConfigPanelComponent extends BaseConfigPanel {
    @ViewChild('queryDropdown') queryDropdown!: TreeDropdownComponent;
    public queryId = '';
    public queryName = '';
    public showParameterControls = true;
    public parameterLayout: 'header' | 'sidebar' | 'dialog' = 'header';
    public autoRefreshSeconds = 0;
    public showExecutionMetadata = true;
    // Track previous selection name for smart title updates
    private previousQueryName = '';
    public showAdvancedOptions = false;
    public queryError = '';
    // Tree configuration for Query Categories (branches) and Queries (leaves)
    public QueryCategoryConfig: TreeBranchConfig = {
    public QueryLeafConfig: TreeLeafConfig = {
        ParentField: 'CategoryID',
        DefaultIcon: 'fa-solid fa-flask',
     * Get the queryId as a CompositeKey for the tree dropdown
    public get QueryIdAsKey(): CompositeKey | null {
        return this.queryId ? CompositeKey.FromID(this.queryId) : null;
    public initFromConfig(config: PanelConfig | null): void {
        if (config && config.type === 'Query') {
            this.queryId = (config['queryId'] as string) || '';
            this.queryName = (config['queryName'] as string) || '';
            this.showParameterControls = (config['showParameterControls'] as boolean) ?? true;
            this.parameterLayout = (config['parameterLayout'] as 'header' | 'sidebar' | 'dialog') || 'header';
            this.autoRefreshSeconds = (config['autoRefreshSeconds'] as number) || 0;
            this.showExecutionMetadata = (config['showExecutionMetadata'] as boolean) ?? true;
            // Defaults for new Query panel
            this.queryId = '';
            this.queryName = '';
            this.showParameterControls = true;
            this.parameterLayout = 'header';
            this.autoRefreshSeconds = 0;
            this.showExecutionMetadata = true;
        this.previousQueryName = '';
        this.queryError = '';
            type: 'Query',
            queryId: this.queryId.trim() || undefined,
            queryName: this.queryName.trim() || undefined,
            showParameterControls: this.showParameterControls,
            parameterLayout: this.parameterLayout,
            autoRefreshSeconds: this.autoRefreshSeconds,
            showExecutionMetadata: this.showExecutionMetadata
        if (!this.queryId.trim() && !this.queryName.trim()) {
            this.queryError = 'Please select a query';
            errors.push(this.queryError);
        if (this.queryName) {
            return this.queryName;
        return 'Query';
     * Handle query selection from tree dropdown
    public onQuerySelection(node: TreeNode | TreeNode[] | null): void {
            // Only accept leaf nodes (actual queries, not categories)
                const oldQueryName = this.queryName;
                this.queryId = node.ID;
                this.queryName = node.Label;
                if (!this.title || this.title === oldQueryName || this.title === this.previousQueryName) {
                this.previousQueryName = node.Label;
    public onParameterLayoutChange(): void {
    public onAutoRefreshChange(): void {
    public toggleAdvancedOptions(): void {
    public getParameterLayoutDescription(): string {
        switch (this.parameterLayout) {
            case 'sidebar':
                return 'Parameters displayed in a collapsible sidebar';
            case 'dialog':
                return 'Parameters shown in a popup dialog when needed';
                return 'Parameters displayed in the header area above results';
