import { Metadata, QueryInfo, QueryCategoryInfo, CompositeKey } from '@memberjunction/core';
    QueryEntityLinkClickEvent,
    QueryRowClickEvent
} from '@memberjunction/ng-query-viewer';
 * Tree node for the query category hierarchy
    category: QueryCategoryInfo;
    queries: QueryInfo[];
 * A resource component for browsing and executing stored queries.
 * - Hierarchical category navigation
 * - Query search
 * - Query viewer with parameter input
 * - Entity linking for clickable record IDs
@RegisterClass(BaseResourceComponent, 'QueryBrowserResource')
    selector: 'mj-query-browser-resource',
    templateUrl: './query-browser-resource.component.html',
    styleUrls: ['./query-browser-resource.component.css'],
export class QueryBrowserResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    public categories: QueryCategoryInfo[] = [];
    public queries: QueryInfo[] = [];
    public filteredQueries: QueryInfo[] = [];
    public selectedQuery: QueryInfo | null = null;
    private dataLoaded = false;
    private skipUrlUpdate = true; // Skip URL updates during initialization
    private lastNavigatedUrl = ''; // Track URL to avoid reacting to our own navigation
        private router: Router
                if (currentUrl !== this.lastNavigatedUrl && this.dataLoaded) {
        return 'Queries';
        return 'fa-solid fa-database';
            // Load from metadata (already cached)
            this.categories = this.metadata.QueryCategories || [];
            this.queries = (this.metadata.Queries || []).filter(q =>
                q.Status === 'Approved' && q.UserCanRun(this.metadata.CurrentUser)
            this.filteredQueries = [...this.queries];
            // Mark data as loaded and apply any query params for deep linking
            this.dataLoaded = true;
            if (urlState?.queryId) {
                const query = this.queries.find(q => q.ID === urlState.queryId);
                    this.selectedQuery = query;
                    this.expandCategoryForQuery(query);
            console.error('Error loading queries:', error);
    private buildCategoryTree(): void {
            const queriesInCategory = this.queries.filter(q => q.CategoryID === category.ID);
                queries: queriesInCategory,
        // Add uncategorized queries to a virtual root
        const uncategorizedQueries = this.queries.filter(q => !q.CategoryID);
        if (uncategorizedQueries.length > 0) {
            const uncategorizedCategory = new QueryCategoryInfo({
                ID: '__uncategorized__',
                Name: 'Uncategorized',
                Description: 'Queries without a category'
            roots.push({
                category: uncategorizedCategory,
                queries: uncategorizedQueries,
                node.queries.sort((a, b) => a.Name.localeCompare(b.Name));
        if (!value.trim()) {
            const searchLower = value.toLowerCase();
            this.filteredQueries = this.queries.filter(q =>
                q.Name.toLowerCase().includes(searchLower) ||
                q.Description?.toLowerCase().includes(searchLower) ||
                q.Category?.toLowerCase().includes(searchLower)
        // Expand all categories when searching
    // Tree Navigation
    public selectQuery(query: QueryInfo, event?: Event): void {
    public isQueryVisible(query: QueryInfo): boolean {
        if (!this.searchText) return true;
        return this.filteredQueries.some(q => q.ID === query.ID);
    public hasVisibleContent(node: CategoryNode): boolean {
        // Check if any queries in this category match
        if (node.queries.some(q => this.isQueryVisible(q))) {
        // Check if any child categories have visible content
        return node.children.some(child => this.hasVisibleContent(child));
    // Query Viewer Events
    public onEntityLinkClick(event: QueryEntityLinkClickEvent): void {
        // Open the entity record using navigation service
        // Convert the recordId string to a CompositeKey (assumes single-field primary key)
        const compositeKey = CompositeKey.FromID(event.recordId);
        this.navigationService.OpenEntityRecord(event.entityName, compositeKey);
    public onRowDoubleClick(event: QueryRowClickEvent): void {
        // Could show record details or other action
    public onOpenQueryRecord(event: { queryId: string; queryName: string }): void {
        // Open the Query entity record using navigation service
        const compositeKey = CompositeKey.FromID(event.queryId);
        this.navigationService.OpenEntityRecord('MJ: Queries', compositeKey);
    public openQueryDetails(query: QueryInfo, event: Event): void {
        // Stop propagation so clicking the button doesn't also select the query
        // Open the Query entity record
        const compositeKey = CompositeKey.FromID(query.ID);
    // Utilities
    public getTotalQueryCount(): number {
        return this.queries.length;
    public getNodeQueryCount(node: CategoryNode): number {
        let count = node.queries.length;
            count += this.getNodeQueryCount(child);
        this.selectedQuery = null;
    public trackByCategory(index: number, node: CategoryNode): string {
        return node.category.ID;
    public trackByQuery(index: number, query: QueryInfo): string {
        return query.ID;
    // Deep Linking
     * Parse URL query string for query state.
     * Query params: queryId
    private parseUrlState(): { queryId?: string } | null {
        const queryId = params.get('queryId');
        if (!queryId) return null;
        return { queryId };
     * Update URL query string to reflect current state.
        // Add query ID if selected, null to remove
        if (this.selectedQuery?.ID) {
            queryParams['queryId'] = this.selectedQuery.ID;
            queryParams['queryId'] = null;
        // Only handle if we're still on the same base path (same component instance)
                if (this.selectedQuery?.ID !== query.ID) {
            // No queryId means clear selection
    private parseUrlFromString(url: string): { queryId?: string } | null {
     * Expands the category tree to show the given query
    private expandCategoryForQuery(query: QueryInfo): void {
        if (!query.CategoryID) return;
        const expandCategory = (nodes: CategoryNode[], targetCategoryId: string): boolean => {
                if (node.category.ID === targetCategoryId) {
                if (this.expandCategoryForQueryRecursive(node.children, targetCategoryId)) {
        expandCategory(this.categoryTree, query.CategoryID);
     * Helper for recursive category expansion
    private expandCategoryForQueryRecursive(nodes: CategoryNode[], targetCategoryId: string): boolean {
            if (node.children.length > 0 && this.expandCategoryForQueryRecursive(node.children, targetCategoryId)) {
