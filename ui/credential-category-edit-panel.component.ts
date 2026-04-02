import { Component, Input, Output, EventEmitter, ChangeDetectorRef, ChangeDetectionStrategy, OnInit } from '@angular/core';
    selector: 'mj-credential-category-edit-panel',
    templateUrl: './credential-category-edit-panel.component.html',
    styleUrls: ['./credential-category-edit-panel.component.css'],
export class CredentialCategoryEditPanelComponent implements OnInit {
    @Input() category: MJCredentialCategoryEntity | null = null;
    @Input() isOpen = false;
    @Output() saved = new EventEmitter<MJCredentialCategoryEntity>();
    @Output() deleted = new EventEmitter<string>();
    public isSaving = false;
    public isNew = false;
    // All categories for parent selection
    public allCategories: MJCredentialCategoryEntity[] = [];
    public name = '';
    public description = '';
    public parentId = '';
    public iconClass = '';
    // Icon suggestions
    public iconSuggestions: { icon: string; label: string }[] = [
        { icon: 'fa-solid fa-folder', label: 'Folder' },
        { icon: 'fa-solid fa-lock', label: 'Lock' },
        { icon: 'fa-solid fa-shield-halved', label: 'Shield' },
        { icon: 'fa-solid fa-key', label: 'Key' },
        { icon: 'fa-solid fa-cloud', label: 'Cloud' },
        { icon: 'fa-solid fa-database', label: 'Database' },
        { icon: 'fa-solid fa-brain', label: 'AI' },
        { icon: 'fa-solid fa-envelope', label: 'Email' },
        { icon: 'fa-solid fa-plug', label: 'Integration' },
        { icon: 'fa-solid fa-server', label: 'Server' },
        { icon: 'fa-solid fa-code', label: 'Code' },
        { icon: 'fa-solid fa-globe', label: 'Web' }
        this.loadCategories();
    public get panelTitle(): string {
        return this.isNew ? 'Create Category' : 'Edit Category';
    public get canSave(): boolean {
        return this.name.trim().length > 0;
    public get availableParentCategories(): MJCredentialCategoryEntity[] {
        // Exclude the current category and its descendants from parent options
        if (!this.category || this.isNew) {
            return this.allCategories;
        const currentId = this.category.ID;
        const descendantIds = this.getDescendantIds(currentId);
        descendantIds.add(currentId);
        return this.allCategories.filter(c => !descendantIds.has(c.ID));
    private getDescendantIds(categoryId: string): Set<string> {
        const descendants = new Set<string>();
        const findChildren = (parentId: string): void => {
            for (const cat of this.allCategories) {
                if (cat.ParentID === parentId) {
                    descendants.add(cat.ID);
                    findChildren(cat.ID);
        findChildren(categoryId);
    public async open(category: MJCredentialCategoryEntity | null, preselectedParentId?: string): Promise<void> {
        this.isOpen = true;
        this.category = category;
        this.isNew = !category || !category.ID;
        if (category && category.ID) {
            this.populateFromCategory(category);
        } else if (preselectedParentId) {
            this.parentId = preselectedParentId;
            const result = await rv.RunView<MJCredentialCategoryEntity>({
                this.allCategories = result.Results;
        this.name = '';
        this.description = '';
        this.parentId = '';
        this.iconClass = '';
    private populateFromCategory(category: MJCredentialCategoryEntity): void {
        this.name = category.Name || '';
        this.description = category.Description || '';
        this.parentId = category.ParentID || '';
        this.iconClass = category.IconClass || '';
    public selectIcon(icon: string): void {
        this.iconClass = icon;
        if (!this.canSave) {
            MJNotificationService.Instance.CreateSimpleNotification('Please enter a category name', 'warning', 3000);
            let entity: MJCredentialCategoryEntity;
            if (this.isNew) {
                entity = await this._metadata.GetEntityObject<MJCredentialCategoryEntity>('MJ: Credential Categories');
                entity = this.category!;
            entity.Name = this.name.trim();
            entity.Description = this.description.trim() || null;
            entity.ParentID = this.parentId || null;
            entity.IconClass = this.iconClass.trim() || null;
            const success = await entity.Save();
                const action = this.isNew ? 'created' : 'updated';
                    `Category "${entity.Name}" ${action} successfully`,
                this.saved.emit(entity);
                const errorMessage = entity.LatestResult?.Message || 'Unknown error';
                console.error('Save failed:', errorMessage);
                    `Failed to save category: ${errorMessage}`,
                'Error saving category',
    public async deleteCategory(): Promise<void> {
        if (this.isNew || !this.category) return;
        const confirmed = confirm(`Are you sure you want to delete "${this.category.Name}"? This action cannot be undone.`);
            const success = await this.category.Delete();
                    `Category "${this.category.Name}" deleted successfully`,
                this.deleted.emit(this.category.ID);
                const errorMessage = this.category.LatestResult?.Message || 'Unknown error';
                    `Failed to delete category: ${errorMessage}`,
                'Error deleting category',
        this.category = null;
    public getParentPath(categoryId: string): string {
        let current = this.allCategories.find(c => c.ID === categoryId);
            parts.unshift(current.Name);
            current = current.ParentID ? this.allCategories.find(c => c.ID === current!.ParentID) : undefined;
