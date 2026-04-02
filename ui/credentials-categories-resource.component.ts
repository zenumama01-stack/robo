import { Component, OnInit, OnDestroy, ChangeDetectorRef, ChangeDetectionStrategy, ViewChild } from '@angular/core';
import { ResourceData, MJCredentialCategoryEntity, MJCredentialTypeEntity } from '@memberjunction/core-entities';
import { CredentialCategoryEditPanelComponent } from '@memberjunction/ng-credentials';
    category: MJCredentialCategoryEntity;
    typeCount: number;
@RegisterClass(BaseResourceComponent, 'CredentialsCategoriesResource')
    selector: 'mj-credentials-categories-resource',
    templateUrl: './credentials-categories-resource.component.html',
    styleUrls: ['./credentials-categories-resource.component.css'],
export class CredentialsCategoriesResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    public categories: MJCredentialCategoryEntity[] = [];
    public types: MJCredentialTypeEntity[] = [];
    public selectedNode: CategoryNode | null = null;
    // Permissions
    @ViewChild('categoryEditPanel') categoryEditPanel!: CredentialCategoryEditPanelComponent;
        return 'Categories';
    public get UserCanCreate(): boolean {
        return this.checkEntityPermission('MJ: Credential Categories', 'Create');
    public get UserCanUpdate(): boolean {
        return this.checkEntityPermission('MJ: Credential Categories', 'Update');
    public get UserCanDelete(): boolean {
        return this.checkEntityPermission('MJ: Credential Categories', 'Delete');
                case 'Create': hasPermission = userPermissions.CanCreate; break;
                case 'Read': hasPermission = userPermissions.CanRead; break;
                case 'Update': hasPermission = userPermissions.CanUpdate; break;
                case 'Delete': hasPermission = userPermissions.CanDelete; break;
            const [catResult, typeResult] = await rv.RunViews([
                    EntityName: 'MJ: Credential Categories',
            if (catResult.Success) {
                this.categories = catResult.Results as MJCredentialCategoryEntity[];
                this.types = typeResult.Results as MJCredentialTypeEntity[];
            console.error('Error loading credential categories:', error);
            MJNotificationService.Instance.CreateSimpleNotification('Error loading categories', 'error', 3000);
        // Create nodes for all categories with stats
        for (const category of this.categories) {
            const typesInCategory = this.types.filter(t => t.Category === category.Name);
                typeCount: typesInCategory.length
        const roots: CategoryNode[] = [];
            const node = categoryMap.get(category.ID)!;
                const parent = categoryMap.get(category.ParentID);
                    roots.push(node);
        // Sort children recursively
        const sortNodes = (nodes: CategoryNode[]): void => {
                sortNodes(node.children);
        sortNodes(roots);
        this.categoryTree = roots;
    // === CRUD Operations ===
    public createNewCategory(): void {
        if (this.categoryEditPanel) {
            this.categoryEditPanel.open(null);
    public createChildCategory(parentNode: CategoryNode, event?: Event): void {
            this.categoryEditPanel.open(null, parentNode.category.ID);
    public editCategory(node: CategoryNode, event?: Event): void {
            this.categoryEditPanel.open(node.category);
    public async deleteCategory(node: CategoryNode, event?: Event): Promise<void> {
        if (!this.UserCanDelete) {
            MJNotificationService.Instance.CreateSimpleNotification('You do not have permission to delete categories', 'warning', 3000);
                `Cannot delete "${node.category.Name}" - it has ${node.children.length} subcategories`,
        if (node.typeCount > 0) {
                `Cannot delete "${node.category.Name}" - it has ${node.typeCount} credential type(s) using it`,
        const confirmed = confirm(`Are you sure you want to delete "${node.category.Name}"? This action cannot be undone.`);
        if (!confirmed) return;
            const success = await node.category.Delete();
                MJNotificationService.Instance.CreateSimpleNotification(`Category "${node.category.Name}" deleted successfully`, 'success', 3000);
                this.categories = this.categories.filter(c => c.ID !== node.category.ID);
                if (this.selectedNode?.category.ID === node.category.ID) {
                    this.selectedNode = null;
                MJNotificationService.Instance.CreateSimpleNotification('Failed to delete category', 'error', 3000);
            console.error('Error deleting category:', error);
            MJNotificationService.Instance.CreateSimpleNotification('Error deleting category', 'error', 3000);
    // === Panel Event Handlers ===
    public onCategorySaved(category: MJCredentialCategoryEntity): void {
        const existingIndex = this.categories.findIndex(c => c.ID === category.ID);
        if (existingIndex >= 0) {
            this.categories[existingIndex] = category;
            this.categories.push(category);
    public onCategoryDeleted(categoryId: string): void {
        this.categories = this.categories.filter(c => c.ID !== categoryId);
        if (this.selectedNode?.category.ID === categoryId) {
    // === Selection ===
    public selectNode(node: CategoryNode): void {
        this.selectedNode = this.selectedNode?.category.ID === node.category.ID ? null : node;
    public toggleExpand(node: CategoryNode, event?: Event): void {
    // === Search ===
            this.expandAll();
        const expand = (nodes: CategoryNode[]): void => {
        expand(this.categoryTree);
        const collapse = (nodes: CategoryNode[]): void => {
        collapse(this.categoryTree);
    public getFlattenedNodes(): CategoryNode[] {
        const searchLower = this.searchText.toLowerCase().trim();
        const flatten = (nodes: CategoryNode[]): void => {
                if (searchLower) {
                    const matches = this.nodeMatchesSearch(node, searchLower);
                    if (matches) {
                if (node.expanded && node.children.length > 0) {
                    flatten(node.children);
        flatten(this.categoryTree);
    private nodeMatchesSearch(node: CategoryNode, searchLower: string): boolean {
        const nameMatch = node.category.Name.toLowerCase().includes(searchLower);
        const descMatch = node.category.Description?.toLowerCase().includes(searchLower);
        return nameMatch || descMatch || false;
    public getTotalTypeCount(): number {
        return this.types.length;
    public getTypesForCategory(categoryName: string): MJCredentialTypeEntity[] {
        return this.types.filter(t => t.Category === categoryName);
    public createCredentialWithCategory(categoryId: string, event?: Event): void {
        // Navigate to Credentials nav item with the category pre-selected and create panel open
        this.navigationService.OpenNavItemByName('Credentials', {
            openCreatePanel: true
    public viewTypesForCategory(categoryName: string, event?: Event): void {
        // Navigate to Types nav item filtered by this category
        this.navigationService.OpenNavItemByName('Types', {
            categoryFilter: categoryName
    public getCategoryColor(index: number): string {
        const colors = ['#6366f1', '#8b5cf6', '#ec4899', '#f59e0b', '#10b981', '#3b82f6', '#06b6d4'];
