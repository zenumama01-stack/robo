import { Component, OnInit, OnDestroy, Output, EventEmitter } from '@angular/core';
import { RunView, LogError } from '@memberjunction/core';
import { Subject, BehaviorSubject, combineLatest } from 'rxjs';
import { debounceTime, takeUntil, distinctUntilChanged } from 'rxjs/operators';
  category: MJActionCategoryEntity;
  children: CategoryTreeNode[];
  selector: 'mj-actions-list-view',
  templateUrl: './actions-list-view.component.html',
  styleUrls: ['./actions-list-view.component.css']
export class ActionsListViewComponent implements OnInit, OnDestroy {
  @Output() openEntityRecord = new EventEmitter<{entityName: string; recordId: string}>();
  public actions: MJActionEntity[] = [];
  public filteredActions: MJActionEntity[] = [];
  public categories: Map<string, MJActionCategoryEntity> = new Map();
  public categoryTree: CategoryTreeNode[] = [];
  public categoryDescendants: Map<string, Set<string>> = new Map();
  public selectedStatus$ = new BehaviorSubject<string>('all');
  public selectedType$ = new BehaviorSubject<string>('all');
  public selectedCategory$ = new BehaviorSubject<string>('all');
  public expandedCategories: Set<string> = new Set();
    { text: 'AI Generated', value: 'Generated' },
    { text: 'Custom', value: 'Custom' }
  public categoryOptions: Array<{text: string; value: string}> = [
    { text: 'All Categories', value: 'all' }
    this.setupFilters();
  private setupFilters(): void {
      this.searchTerm$.pipe(debounceTime(300), distinctUntilChanged()),
      this.selectedStatus$.pipe(distinctUntilChanged()),
      this.selectedType$.pipe(distinctUntilChanged()),
      this.selectedCategory$.pipe(distinctUntilChanged())
  private async loadData(): Promise<void> {
      if (!actionsResult.Success || !categoriesResult.Success) {
        if (!actionsResult.Success) 
          errors.push('Actions: ' + actionsResult.ErrorMessage);
        if (!categoriesResult.Success) 
          errors.push('Categories: ' + categoriesResult.ErrorMessage);
        throw new Error('Failed to load data: ' + errors.join(', '));
      const actions = (actionsResult.Results || []) as MJActionEntity[];
      const categories = (categoriesResult.Results || []) as MJActionCategoryEntity[];
      this.actions = actions;
      this.populateCategoriesMap(categories);
      this.buildCategoryOptions(categories);
      console.error('Error loading actions list data:', error);
      LogError('Failed to load actions list data', undefined, error);
  private populateCategoriesMap(categories: MJActionCategoryEntity[]): void {
    this.categories.clear();
      this.categories.set(category.ID, category);
    // Build the category tree
    this.buildCategoryTree(categories);
    // Build descendant mapping for efficient filtering
    this.buildDescendantMapping(categories);
  private buildCategoryOptions(categories: MJActionCategoryEntity[]): void {
      ...categories.map(category => ({
        value: category.ID
  private buildCategoryTree(categories: MJActionCategoryEntity[]): void {
    // First pass: create all nodes
      categoryMap.set(category.ID, {
    // Second pass: build tree structure
    const rootNodes: CategoryTreeNode[] = [];
    categoryMap.forEach(node => {
      const parentId = node.category.ParentID;
      if (parentId && categoryMap.has(parentId)) {
        const parent = categoryMap.get(parentId)!;
    // Sort children at each level by name
    const sortChildren = (nodes: CategoryTreeNode[]) => {
      nodes.sort((a, b) => a.category.Name.localeCompare(b.category.Name));
      nodes.forEach(node => sortChildren(node.children));
    sortChildren(rootNodes);
    this.categoryTree = rootNodes;
  private buildDescendantMapping(categories: MJActionCategoryEntity[]): void {
    this.categoryDescendants.clear();
    // Initialize each category with itself
      this.categoryDescendants.set(category.ID, new Set([category.ID]));
    // Build descendant sets
    const addDescendants = (categoryId: string, descendantId: string) => {
      const descendants = this.categoryDescendants.get(categoryId);
      if (descendants) {
        descendants.add(descendantId);
        // Add this category as a descendant of all its ancestors
        let currentParentId: string | null = category.ParentID;
        while (currentParentId) {
          addDescendants(currentParentId, category.ID);
          const parent = this.categories.get(currentParentId);
          currentParentId = parent?.ParentID || null;
    let filtered = [...this.actions];
        action.Name.toLowerCase().includes(searchTerm) ||
        (action.Description || '').toLowerCase().includes(searchTerm)
    const status = this.selectedStatus$.value;
    if (status !== 'all') {
      filtered = filtered.filter(action => action.Status === status);
    // Apply type filter
    const type = this.selectedType$.value;
    if (type !== 'all') {
      filtered = filtered.filter(action => action.Type === type);
    // Apply category filter (includes descendants)
    const categoryId = this.selectedCategory$.value;
      const descendantIds = this.categoryDescendants.get(categoryId);
      if (descendantIds) {
        // Filter actions that belong to the selected category or any of its descendants
          action.CategoryID && descendantIds.has(action.CategoryID)
        console.warn(`Category ID ${categoryId} not found in category hierarchy`);
        filtered = [];
    this.filteredActions = filtered;
  public onSearchChange(searchTerm: string): void {
    this.searchTerm$.next(searchTerm);
  public onStatusFilterChange(status: string): void {
    this.selectedStatus$.next(status);
  public onTypeFilterChange(type: string): void {
    this.selectedType$.next(type);
  public onCategoryFilterChange(categoryId: string): void {
    this.selectedCategory$.next(categoryId);
  public openAction(action: MJActionEntity): void {
    this.openEntityRecord.emit({
      recordId: action.ID
  public getCategoryName(categoryId: string | null): string {
    if (!categoryId) return 'No Category';
    return this.categories.get(categoryId)?.Name || 'Unknown Category';
  public getStatusColor(status: string): 'success' | 'warning' | 'error' | 'info' {
      case 'Active': return 'success';
      case 'Pending': return 'warning';
      case 'Disabled': return 'error';
      case 'Generated': return 'fa-solid fa-robot';
      case 'Custom': return 'fa-solid fa-code';
      default: return 'fa-solid fa-cog';
   * Falls back to type-based icon if no IconClass is set
    return action?.IconClass || this.getTypeIcon(action.Type);
  // Tree view methods
  public toggleCategoryExpanded(categoryId: string): void {
    if (this.expandedCategories.has(categoryId)) {
      this.expandedCategories.delete(categoryId);
      this.expandedCategories.add(categoryId);
  public isCategoryExpanded(categoryId: string): boolean {
    return this.expandedCategories.has(categoryId);
  public selectCategory(categoryId: string): void {
  public getCategoryActionCount(categoryId: string): number {
    if (!descendantIds) return 0;
    return this.actions.filter(action => 
  public showCategoryTree = false;
  public toggleCategoryTree(): void {
    this.showCategoryTree = !this.showCategoryTree;
