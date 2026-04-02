import { Component, OnInit, EventEmitter, Output, ChangeDetectorRef } from '@angular/core';
/** Scope tree node structure */
interface ScopeTreeNode {
 * API Scopes Panel Component
 * Manages API Scopes in a hierarchical tree structure
    selector: 'mj-api-scopes-panel',
    templateUrl: './api-scopes-panel.component.html',
    styleUrls: ['./api-scopes-panel.component.css']
export class APIScopesPanelComponent implements OnInit {
    @Output() ScopeUpdated = new EventEmitter<void>();
    public ScopeTree: ScopeTreeNode[] = [];
    public FlatScopes: MJAPIScopeEntity[] = [];
    public EditingScope: MJAPIScopeEntity | null = null;
    public EditCategory = '';
    public EditResourceType = '';
    public EditParentId: string | null = null;
    public ShowEditDialog = false;
    public SelectedParentScope: MJAPIScopeEntity | null = null;
    // Category colors
    public readonly CategoryColors: Record<string, string> = {
        'Entities': '#6366f1',
        'Agents': '#10b981',
        'Admin': '#f59e0b',
        'Actions': '#8b5cf6',
        'Queries': '#3b82f6',
        'Reports': '#ef4444',
        'Communication': '#ec4899',
        'Other': '#6b7280'
    // Resource type options
    public readonly ResourceTypes = ['Entity', 'Agent', 'Query', 'Mutation', 'Action', 'Report', 'Admin', 'Other'];
    public readonly Categories = ['Entities', 'Agents', 'Admin', 'Actions', 'Queries', 'Reports', 'Communication', 'Other'];
     * Load all scopes
            const result = await rv.RunView<MJAPIScopeEntity>({
                OrderBy: 'FullPath',
                this.FlatScopes = result.Results;
                this.buildTree();
            this.ErrorMessage = 'Failed to load scopes';
     * Build tree structure from flat scopes
    private buildTree(): void {
        const scopeMap = new Map<string, ScopeTreeNode>();
        const rootNodes: ScopeTreeNode[] = [];
        // Create nodes
        for (const scope of this.FlatScopes) {
            scopeMap.set(scope.ID, {
        // Build hierarchy
            const node = scopeMap.get(scope.ID)!;
            if (scope.ParentID) {
                const parent = scopeMap.get(scope.ParentID);
                if (parent) {
                    node.level = parent.level + 1;
                    rootNodes.push(node);
        // Calculate levels recursively
        const calculateLevels = (nodes: ScopeTreeNode[], level: number) => {
                calculateLevels(node.children, level + 1);
        calculateLevels(rootNodes, 0);
        this.ScopeTree = rootNodes;
     * Open create dialog for new scope
    public openCreateDialog(parentScope: MJAPIScopeEntity | null = null): void {
        this.EditCategory = parentScope?.Category || 'Entities';
        this.EditResourceType = '';
        this.EditParentId = parentScope?.ID || null;
        this.EditingScope = null;
        this.SelectedParentScope = parentScope;
     * Open edit dialog for existing scope
    public openEditDialog(scope: MJAPIScopeEntity): void {
        this.EditingScope = scope;
        this.EditName = scope.Name;
        this.EditDescription = scope.Description || '';
        this.EditCategory = scope.Category || 'Entities';
        this.EditResourceType = scope.ResourceType || '';
        this.EditParentId = scope.ParentID;
        this.EditIsActive = scope.IsActive;
        this.SelectedParentScope = scope.ParentID
            ? this.FlatScopes.find(s => s.ID === scope.ParentID) || null
        this.ShowEditDialog = true;
     * Save scope (create or update)
    public async saveScope(): Promise<void> {
            let scope: MJAPIScopeEntity;
            if (this.EditingScope) {
                scope = this.EditingScope;
                scope = await this.md.GetEntityObject<MJAPIScopeEntity>('MJ: API Scopes');
                scope.NewRecord();
            scope.Name = this.EditName.trim();
            scope.Description = this.EditDescription.trim() || null;
            scope.Category = this.EditCategory;
            scope.ResourceType = this.EditResourceType || null;
            scope.ParentID = this.EditParentId;
            scope.IsActive = this.EditIsActive;
            // FullPath is auto-computed by the trigger
            const result = await scope.Save();
                this.SuccessMessage = this.EditingScope
                    ? 'Scope updated successfully'
                    : 'Scope created successfully';
                this.closeDialogs();
                this.ScopeUpdated.emit();
                this.ErrorMessage = 'Failed to save scope';
            console.error('Error saving scope:', error);
     * Toggle node expansion
    public toggleExpanded(node: ScopeTreeNode): void {
        node.expanded = !node.expanded;
     * Expand all nodes
    public expandAll(): void {
        const expand = (nodes: ScopeTreeNode[]) => {
                node.expanded = true;
                expand(node.children);
        expand(this.ScopeTree);
     * Collapse all nodes
    public collapseAll(): void {
        const collapse = (nodes: ScopeTreeNode[]) => {
                node.expanded = false;
                collapse(node.children);
        collapse(this.ScopeTree);
     * Close all dialogs
    public closeDialogs(): void {
        this.ShowEditDialog = false;
        this.SelectedParentScope = null;
     * Get parent scopes for dropdown (exclude self and descendants)
    public getParentOptions(): MJAPIScopeEntity[] {
        if (!this.EditingScope) {
            return this.FlatScopes;
        // Exclude self and all descendants
        const excludeIds = new Set<string>([this.EditingScope.ID]);
        const addDescendants = (parentId: string) => {
                if (scope.ParentID === parentId && !excludeIds.has(scope.ID)) {
                    excludeIds.add(scope.ID);
                    addDescendants(scope.ID);
        addDescendants(this.EditingScope.ID);
        return this.FlatScopes.filter(s => !excludeIds.has(s.ID));
     * Get category color
    public getCategoryColor(category: string | null): string {
        return this.CategoryColors[category || 'Other'] || this.CategoryColors['Other'];
     * Get count of total scopes
    public getTotalCount(): number {
        return this.FlatScopes.length;
     * Get count of active scopes
    public getActiveCount(): number {
        return this.FlatScopes.filter(s => s.IsActive).length;
