import { Component, ViewEncapsulation, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ResourceData, MJListCategoryEntity, MJListEntity } from '@memberjunction/core-entities';
interface CategoryViewModel {
  category: MJListCategoryEntity;
  listCount: number;
  childCount: number;
@RegisterClass(BaseResourceComponent, 'ListsCategoriesResource')
  selector: 'mj-lists-categories-resource',
    <div class="lists-categories-container">
          <i class="fa-solid fa-tags"></i>
          <h2>List Categories</h2>
          <button class="btn-create" (click)="createCategory()">
          <mj-loading text="Loading categories..." size="medium"></mj-loading>
      @if (!isLoading && categoryViewModels.length === 0) {
          <h3>No Categories Yet</h3>
          <p>Categories help you organize lists into logical groups.</p>
              <span>Create hierarchical folder structures</span>
              <span>Quickly find related lists</span>
          <button class="btn-create-large" (click)="createCategory()">
            Create Your First Category
      <!-- Categories Content -->
      @if (!isLoading && categoryViewModels.length > 0) {
        <div class="categories-content">
                <h3>Categories</h3>
                <span class="count-badge">{{categories.length}}</span>
              <div class="tree-content" role="tree" aria-label="Category tree">
                @for (vm of getTopLevelCategories(); track vm) {
                  <ng-container *ngTemplateOutlet="categoryNodeTemplate; context: { vm: vm }"></ng-container>
            <!-- Category Details -->
            @if (selectedCategory) {
              <div class="category-detail-panel">
                  <h3>Category Details</h3>
                  <div class="panel-actions">
                    <button class="icon-btn" (click)="editCategory()" title="Edit">
                    <button class="icon-btn danger" (click)="deleteCategory()" title="Delete">
                    <span class="field-value">{{selectedCategory.Name}}</span>
                  @if (selectedCategory.Description) {
                      <span class="field-value">{{selectedCategory.Description}}</span>
                    <label>Parent Category</label>
                    <span class="field-value">
                      {{getParentCategoryName(selectedCategory) || '(Top Level)'}}
                      <span class="stat-value">{{getSelectedCategoryListCount()}}</span>
                      <span class="stat-label">Lists</span>
                      <span class="stat-value">{{getSelectedCategoryChildCount()}}</span>
                      <span class="stat-label">Subcategories</span>
                  @if (selectedCategoryLists.length > 0) {
                    <div class="category-lists">
                      <h4>Lists in this category</h4>
                      <div class="mini-list">
                        @for (list of selectedCategoryLists; track list) {
                          <div class="mini-list-item">
                            <span>{{list.Name}}</span>
            <!-- No Selection State -->
            @if (!selectedCategory) {
              <div class="category-detail-panel empty">
                  <p>Select a category to view details</p>
      <ng-template #categoryNodeTemplate let-vm="vm">
        <div class="category-node" [style.padding-left.px]="vm.depth * 20">
            class="node-content"
            [class.selected]="selectedCategory?.ID === vm.category.ID"
            (click)="selectCategory(vm.category)"
            (keydown.enter)="selectCategory(vm.category)"
            (keydown.space)="selectCategory(vm.category); $event.preventDefault()"
            (keydown.arrowRight)="expandNode($event, vm)"
            (keydown.arrowLeft)="collapseNode($event, vm)"
            role="treeitem"
            [attr.aria-expanded]="hasChildren(vm.category) ? vm.isExpanded : null"
            [attr.aria-selected]="selectedCategory?.ID === vm.category.ID"
            [attr.aria-label]="vm.category.Name + ' - ' + vm.listCount + ' lists'">
            @if (hasChildren(vm.category)) {
                (click)="toggleExpand($event, vm)"
                tabindex="-1"
                aria-hidden="true">
                <i [class]="vm.isExpanded ? 'fa-solid fa-chevron-down' : 'fa-solid fa-chevron-right'"></i>
            @if (!hasChildren(vm.category)) {
            <i class="fa-solid fa-folder" [class.fa-folder-open]="vm.isExpanded" aria-hidden="true"></i>
            <span class="node-name">{{vm.category.Name}}</span>
            <span class="node-count" aria-hidden="true">{{vm.listCount}}</span>
          @if (vm.isExpanded && hasChildren(vm.category)) {
            <div class="node-children" role="group">
              @for (childVm of getChildCategories(vm.category); track childVm) {
                <ng-container *ngTemplateOutlet="categoryNodeTemplate; context: { vm: childVm }"></ng-container>
      @if (showDialog) {
        <div class="modal-overlay" (click)="closeDialog()">
          <div class="modal-dialog" (click)="$event.stopPropagation()">
              <h3>{{editingCategory ? 'Edit Category' : 'Create Category'}}</h3>
              <button class="modal-close" (click)="closeDialog()" [disabled]="isSaving">
              <div class="category-form">
                    [(ngModel)]="dialogName"
                    placeholder="Enter category name"
                    [(ngModel)]="dialogDescription"
                    [(ngModel)]="dialogParentId"
                    class="form-input">
                    @for (parent of availableParents; track parent) {
                      <option [ngValue]="parent.ID">{{parent.displayName}}</option>
                (click)="saveCategory()"
                [disabled]="!dialogName || isSaving">
                {{isSaving ? 'Saving...' : (editingCategory ? 'Save' : 'Create')}}
              <button class="btn-secondary" (click)="closeDialog()" [disabled]="isSaving">Cancel</button>
        <div class="modal-overlay" (click)="cancelDelete()">
          <div class="modal-dialog modal-sm" (click)="$event.stopPropagation()">
            <div class="modal-header danger">
              <h3>Delete Category</h3>
              <p>{{deleteConfirmMessage}}</p>
              <button class="btn-danger" (click)="confirmDelete()">
              <button class="btn-secondary" (click)="cancelDelete()">Cancel</button>
    .lists-categories-container {
      margin: 16px 0 24px;
    /* Content Layout */
    .categories-content {
    .category-tree-panel,
    .category-detail-panel {
    .panel-actions {
    .icon-btn.danger:hover {
    /* Tree Content */
    .category-node {
    .node-content:focus {
    .node-content:focus-visible {
      outline-offset: -2px;
    .node-content.selected {
    .node-content.selected:focus-visible {
      outline: 2px solid #1976D2;
    .node-content .fa-folder,
    .node-content .fa-folder-open {
    .node-count {
    .field-value {
    .category-lists h4 {
    .mini-list {
    .mini-list-item {
    .mini-list-item i {
    .category-detail-panel.empty {
    .no-selection i {
    /* Form */
    .category-form {
      z-index: 10000;
      animation: fadeIn 0.15s ease-out;
    .modal-dialog.modal-sm {
      from { transform: translateY(-20px); opacity: 0; }
      to { transform: translateY(0); opacity: 1; }
    .modal-header.danger {
      border-bottom-color: #ffcdd2;
    .modal-header.danger h3 {
    .modal-close:hover:not(:disabled) {
    .modal-close:disabled {
    .btn-secondary:disabled {
export class ListsCategoriesResource extends BaseResourceComponent implements OnDestroy {
  categoryViewModels: CategoryViewModel[] = [];
  selectedCategory: MJListCategoryEntity | null = null;
  selectedCategoryLists: MJListEntity[] = [];
  // Dialog
  showDialog = false;
  editingCategory: MJListCategoryEntity | null = null;
  dialogName = '';
  dialogDescription = '';
  dialogParentId: string | null = null;
  availableParents: Array<{ ID: string | null; displayName: string }> = [];
  // Delete confirmation dialog
  deleteConfirmMessage = '';
  private categoryToDelete: MJListCategoryEntity | null = null;
  private listsByCategoryId: Map<string, MJListEntity[]> = new Map();
      const userId = md.CurrentUser?.ID;
      const [categoriesResult, listsResult] = await rv.RunViews([
          CacheLocal: true  // Categories rarely change, cache for performance
          ExtraFilter: userId ? `UserID = '${userId}'` : '',
        console.error('Failed to load categories');
      this.categories = categoriesResult.Results as MJListCategoryEntity[];
      // Group lists by category
      this.listsByCategoryId.clear();
        if (list.CategoryID) {
          const existing = this.listsByCategoryId.get(list.CategoryID) || [];
          existing.push(list);
          this.listsByCategoryId.set(list.CategoryID, existing);
      // Build view models
      this.buildCategoryViewModels();
      this.buildAvailableParents();
  private buildCategoryViewModels() {
    this.categoryViewModels = [];
    const buildVm = (category: MJListCategoryEntity, depth: number): CategoryViewModel => {
      const lists = this.listsByCategoryId.get(category.ID) || [];
      const children = this.categories.filter(c => c.ParentID === category.ID);
        listCount: lists.length,
        childCount: children.length,
    const processCategory = (category: MJListCategoryEntity, depth: number) => {
      this.categoryViewModels.push(buildVm(category, depth));
        processCategory(child, depth + 1);
    const topLevel = this.categories.filter(c => !c.ParentID);
  private buildAvailableParents() {
    this.availableParents = [{ ID: null, displayName: '(Top Level)' }];
    const addCategory = (cat: MJListCategoryEntity, prefix: string) => {
      // Exclude the editing category and its descendants
      if (this.editingCategory && this.isDescendantOf(cat, this.editingCategory)) {
      this.availableParents.push({ ID: cat.ID, displayName: prefix + cat.Name });
      const children = this.categories.filter(c => c.ParentID === cat.ID);
        addCategory(child, prefix + '\u00A0\u00A0');
      if (this.editingCategory?.ID !== cat.ID) {
        addCategory(cat, '');
  private isDescendantOf(category: MJListCategoryEntity, ancestor: MJListCategoryEntity): boolean {
    if (category.ID === ancestor.ID) return true;
    if (!category.ParentID) return false;
    const parent = this.categoryMap.get(category.ParentID);
    return parent ? this.isDescendantOf(parent, ancestor) : false;
  getTopLevelCategories(): CategoryViewModel[] {
    return this.categoryViewModels.filter(vm => !vm.category.ParentID);
  getChildCategories(parent: MJListCategoryEntity): CategoryViewModel[] {
    return this.categoryViewModels.filter(vm => vm.category.ParentID === parent.ID);
  hasChildren(category: MJListCategoryEntity): boolean {
    return this.categories.some(c => c.ParentID === category.ID);
  toggleExpand(event: Event, vm: CategoryViewModel) {
    vm.isExpanded = !vm.isExpanded;
  expandNode(event: Event, vm: CategoryViewModel) {
    if (this.hasChildren(vm.category) && !vm.isExpanded) {
      vm.isExpanded = true;
  collapseNode(event: Event, vm: CategoryViewModel) {
    if (vm.isExpanded) {
      vm.isExpanded = false;
  selectCategory(category: MJListCategoryEntity) {
    this.selectedCategory = category;
    this.selectedCategoryLists = this.listsByCategoryId.get(category.ID) || [];
  getParentCategoryName(category: MJListCategoryEntity): string | null {
    if (!category.ParentID) return null;
    return this.categoryMap.get(category.ParentID)?.Name || null;
  getSelectedCategoryListCount(): number {
    if (!this.selectedCategory) return 0;
    return this.listsByCategoryId.get(this.selectedCategory.ID)?.length || 0;
  getSelectedCategoryChildCount(): number {
    return this.categories.filter(c => c.ParentID === this.selectedCategory!.ID).length;
  createCategory() {
    this.editingCategory = null;
    this.dialogName = '';
    this.dialogDescription = '';
    this.dialogParentId = null;
    this.showDialog = true;
  editCategory() {
    if (!this.selectedCategory) return;
    this.editingCategory = this.selectedCategory;
    this.dialogName = this.selectedCategory.Name;
    this.dialogDescription = this.selectedCategory.Description || '';
    this.dialogParentId = this.selectedCategory.ParentID || null;
  deleteCategory() {
    this.categoryToDelete = this.selectedCategory;
    const categoryName = this.categoryToDelete.Name;
    const listsInCategory = this.listsByCategoryId.get(this.categoryToDelete.ID) || [];
    const childCategories = this.categories.filter(c => c.ParentID === this.categoryToDelete!.ID);
    let message = `Are you sure you want to delete "${categoryName}"?`;
    if (listsInCategory.length > 0) {
      message += ` ${listsInCategory.length} list(s) will be uncategorized.`;
    if (childCategories.length > 0) {
      message += ` ${childCategories.length} subcategory(ies) will become top-level.`;
    this.deleteConfirmMessage = message;
    this.categoryToDelete = null;
    this.deleteConfirmMessage = '';
  async confirmDelete() {
    if (!this.categoryToDelete) return;
    const categoryToDelete = this.categoryToDelete;
      const deleted = await categoryToDelete.Delete();
        this.notificationService.CreateSimpleNotification(`"${categoryName}" deleted`, 'success', 3000);
        // Get the detailed error message from LatestResult
        const errorMessage = categoryToDelete.LatestResult?.Message || 'Unknown error occurred';
        console.error('Failed to delete category:', categoryToDelete.LatestResult);
        this.notificationService.CreateSimpleNotification(`Failed to delete category: ${errorMessage}`, 'error', 6000);
      this.selectedCategory = null;
      this.notificationService.CreateSimpleNotification(`Error deleting category: ${errorMessage}`, 'error', 6000);
  closeDialog() {
    this.showDialog = false;
  async saveCategory() {
    const isEditing = !!this.editingCategory;
    const categoryName = this.dialogName;
      let category: MJListCategoryEntity;
      if (this.editingCategory) {
        category = this.editingCategory;
        category = await md.GetEntityObject<MJListCategoryEntity>('MJ: List Categories');
        category.UserID = md.CurrentUser!.ID;
      category.Name = this.dialogName;
      category.Description = this.dialogDescription || null;
      category.ParentID = this.dialogParentId || null;
          isEditing ? `"${categoryName}" updated` : `"${categoryName}" created`,
        this.closeDialog();
        // Re-select the saved category
        if (isEditing) {
        const errorMessage = category.LatestResult?.Message || 'Unknown error occurred';
        console.error(`Failed to ${action} category:`, category.LatestResult);
          `Failed to ${action} category: ${errorMessage}`,
      console.error('Error saving category:', error);
      this.notificationService.CreateSimpleNotification(`Error saving category: ${errorMessage}`, 'error', 6000);
    return 'fa-solid fa-tags';
