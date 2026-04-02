import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { MJQueryCategoryEntity } from '@memberjunction/core-entities';
interface CategoryNode {
    fullPath: string;
    children: CategoryNode[];
    selector: 'mj-query-category-dialog',
    templateUrl: './query-category-dialog.component.html',
    styleUrls: ['./query-category-dialog.component.css']
export class QueryCategoryDialogComponent implements OnInit {
    @Input() isVisible = false;
    @Output() isVisibleChange = new EventEmitter<boolean>();
    @Output() onCategoryCreated = new EventEmitter<MJQueryCategoryEntity>();
    public categoryName = '';
    public selectedParentId: string | null = null;
    public categories: MJQueryCategoryEntity[] = [];
    public categoryTree: CategoryNode[] = [];
    public flattenedCategories: CategoryNode[] = [];
    public isCreating = false;
    async loadCategories() {
            const result = await rv.RunView<MJQueryCategoryEntity>({
                EntityName: 'MJ: Query Categories',
                this.categoryTree = this.buildCategoryTree(this.categories);
                this.flattenedCategories = this.flattenCategories(this.categoryTree, 0);
    private buildCategoryTree(categories: MJQueryCategoryEntity[]): CategoryNode[] {
        const categoryMap = new Map<string, CategoryNode>();
        const rootCategories: CategoryNode[] = [];
        // Create nodes for all categories
        categories.forEach(cat => {
            categoryMap.set(cat.ID, {
                id: cat.ID,
                name: cat.Name,
                fullPath: cat.Name,
                children: [],
                level: 0
        // Build the tree structure
            const node = categoryMap.get(cat.ID)!;
            if (cat.ParentID && categoryMap.has(cat.ParentID)) {
                const parent = categoryMap.get(cat.ParentID)!;
                parent.children.push(node);
                node.fullPath = `${parent.fullPath} / ${node.name}`;
                rootCategories.push(node);
        // Sort children alphabetically
        const sortNodes = (nodes: CategoryNode[]) => {
            nodes.sort((a, b) => a.name.localeCompare(b.name));
            nodes.forEach(node => sortNodes(node.children));
        sortNodes(rootCategories);
        return rootCategories;
    private flattenCategories(nodes: CategoryNode[], level: number): CategoryNode[] {
        const result: CategoryNode[] = [];
            node.level = level;
            result.push(node);
            if (node.children.length > 0) {
                result.push(...this.flattenCategories(node.children, level + 1));
    async createCategory() {
        if (!this.categoryName.trim()) {
                'Please enter a category name',
                'warning'
        this.isCreating = true;
            const newCategory = await md.GetEntityObject<MJQueryCategoryEntity>('MJ: Query Categories');
            newCategory.Name = this.categoryName.trim();
            newCategory.ParentID = this.selectedParentId;
            newCategory.UserID = md.CurrentUser.ID;
            const saved = await newCategory.Save();
                    'Category created successfully',
                    'success'
                this.onCategoryCreated.emit(newCategory);
                this.close();
                    `Failed to create category: ${newCategory.LatestResult?.Message || 'Unknown error'}`,
                    'error'
            console.error('Error creating category:', error);
                'Error creating category',
            this.isCreating = false;
    close() {
        this.categoryName = '';
        this.selectedParentId = null;
        this.isVisible = false;
        this.isVisibleChange.emit(false);
    getCategoryIndent(category: CategoryNode): string {
        return `${category.level * 20}px`;
    getFullPath(): string {
        const parent = this.flattenedCategories.find(c => c.id === this.selectedParentId);
        const name = this.categoryName || 'New Category';
        return parent ? `${parent.fullPath} / ${name}` : name;
