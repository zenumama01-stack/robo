import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import { DialogRef, WindowRef } from '@progress/kendo-angular-dialog';
import { Subject, BehaviorSubject, combineLatest, debounceTime, distinctUntilChanged, takeUntil, startWith } from 'rxjs';
import { RunView, Metadata } from '@memberjunction/core';
import { MJActionEntity, MJActionCategoryEntity } from '@memberjunction/core-entities';
export interface CategoryTreeNode {
  children?: CategoryTreeNode[];
  expanded?: boolean;
export interface ActionDisplayItem extends MJActionEntity {
  selected: boolean;
 * Modern, clean dialog for selecting actions to add to an AI Agent.
 * Features searchable action list, category filtering, and multi-select capability.
  selector: 'mj-add-action-dialog',
  templateUrl: './add-action-dialog.component.html',
  styleUrls: ['./add-action-dialog.component.css']
export class AddActionDialogComponent implements OnInit, OnDestroy {
  // Input properties set by service
  agentId: string = '';
  agentName: string = '';
  existingActionIds: string[] = [];
  // Reactive state management
  private destroy$ = new Subject<void>();
  public result = new Subject<MJActionEntity[]>();
  // Data streams
  allActions$ = new BehaviorSubject<ActionDisplayItem[]>([]);
  categories$ = new BehaviorSubject<MJActionCategoryEntity[]>([]);
  filteredActions$ = new BehaviorSubject<ActionDisplayItem[]>([]);
  categoryTree$ = new BehaviorSubject<CategoryTreeNode[]>([]);
  selectedActions$ = new BehaviorSubject<Set<string>>(new Set());
  isLoading$ = new BehaviorSubject<boolean>(false);
  searchControl = new FormControl('');
  selectedCategoryId$ = new BehaviorSubject<string>('all');
  viewMode$ = new BehaviorSubject<'grid' | 'list'>('grid');
  expandedCategories = new Set<string>();
  // Computed values
  get selectedCount(): number {
    return this.selectedActions$.value.size;
  get totalActionCount(): number {
    return this.allActions$.value.length;
  get filteredCount(): number {
    return this.filteredActions$.value.length;
    private dialogRef: WindowRef,
    this.initializeData();
    this.setupFiltering();
    this.preselectExistingActions();
  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  private async initializeData() {
    this.isLoading$.next(true);
      await this.loadActionsAndCategories();
      this.buildCategoryTree();
      console.error('Error loading dialog data:', error);
      this.isLoading$.next(false);
  private async loadActionsAndCategories() {
    const [actionsResult, categoriesResult] = await rv.RunViews([
        ExtraFilter: 'Status = \'Active\'',
        OrderBy: 'Category, Name',
        MaxRows: 5000
    if (actionsResult.Success) {
      const actions = (actionsResult.Results as MJActionEntity[] || []).map(action => ({
        ...action.GetAll(),
        selected: false,
        categoryName: action.Category || 'Uncategorized'
      } as ActionDisplayItem));
      this.allActions$.next(actions);
    if (categoriesResult.Success) {
      this.categories$.next(categoriesResult.Results as MJActionCategoryEntity[] || []);
  private buildCategoryTree() {
    const actions = this.allActions$.value;
    const categories = this.categories$.value;
    // Count actions per category
    const categoryCounts = new Map<string, number>();
      const categoryName = action.categoryName || 'Uncategorized';
      categoryCounts.set(categoryName, (categoryCounts.get(categoryName) || 0) + 1);
    // Build tree nodes
    const treeNodes: CategoryTreeNode[] = [
        id: 'all',
        name: 'All Actions',
        count: actions.length,
        icon: 'fa-th'
    // Add category nodes
    categories.forEach(category => {
      const count = categoryCounts.get(category.Name) || 0;
      if (count > 0) {
        treeNodes.push({
          id: category.ID,
          name: category.Name,
          icon: this.getCategoryIcon(category.Name)
    // Add uncategorized if needed
    const uncategorizedCount = categoryCounts.get('Uncategorized') || 0;
    if (uncategorizedCount > 0) {
        id: 'uncategorized',
        name: 'Uncategorized',
        count: uncategorizedCount,
        icon: 'fa-question-circle'
    this.categoryTree$.next(treeNodes);
  private getCategoryIcon(categoryName: string): string {
    const iconMap: { [key: string]: string } = {
      'Data': 'fa-database',
      'Communication': 'fa-envelope',
      'Integration': 'fa-plug',
      'Security': 'fa-shield-alt',
      'Workflow': 'fa-project-diagram',
      'AI': 'fa-brain',
      'Files': 'fa-file-alt',
      'Utilities': 'fa-tools',
      'System': 'fa-cog',
      'Analytics': 'fa-chart-line'
    return iconMap[categoryName] || 'fa-folder';
  private setupFiltering() {
    combineLatest([
      this.allActions$,
      this.searchControl.valueChanges.pipe(
        debounceTime(300),
        distinctUntilChanged(),
        startWith('')  // Emit initial empty value
      this.selectedCategoryId$
    ]).pipe(
      takeUntil(this.destroy$)
    ).subscribe(([actions, searchTerm, categoryId]) => {
      this.filterActions(actions, searchTerm || '', categoryId);
  private filterActions(actions: ActionDisplayItem[], searchTerm: string, categoryId: string) {
    let filtered = [...actions];
    if (categoryId !== 'all') {
      if (categoryId === 'uncategorized') {
        filtered = filtered.filter(action => !action.Category);
        const categoryName = this.getCategoryNameById(categoryId);
        filtered = filtered.filter(action => action.categoryName === categoryName);
      const term = searchTerm.toLowerCase();
      filtered = filtered.filter(action =>
        action.Name.toLowerCase().includes(term) ||
        (action.Description && action.Description.toLowerCase().includes(term)) ||
        (action.categoryName && action.categoryName.toLowerCase().includes(term))
    this.filteredActions$.next(filtered);
  private getCategoryNameById(categoryId: string): string {
    const category = this.categories$.value.find(c => c.ID === categoryId);
    return category?.Name || '';
  private preselectExistingActions() {
    if (this.existingActionIds.length > 0) {
      const selected = new Set(this.existingActionIds);
      this.selectedActions$.next(selected);
      // Update action selection state
        action.selected = selected.has(action.ID);
  // === UI Event Handlers ===
  selectCategory(categoryId: string) {
    this.selectedCategoryId$.next(categoryId);
  toggleViewMode() {
    const currentMode = this.viewMode$.value;
    this.viewMode$.next(currentMode === 'grid' ? 'list' : 'grid');
  toggleActionSelection(action: ActionDisplayItem) {
    const selected = this.selectedActions$.value;
    // Find the action and toggle its selection
    const actionToUpdate = actions.find(a => a.ID === action.ID);
    if (actionToUpdate) {
      actionToUpdate.selected = !actionToUpdate.selected;
      if (actionToUpdate.selected) {
        selected.add(action.ID);
        selected.delete(action.ID);
      this.selectedActions$.next(new Set(selected));
      // Update filtered actions to reflect selection state
      const filtered = this.filteredActions$.value;
      const filteredAction = filtered.find(a => a.ID === action.ID);
      if (filteredAction) {
        filteredAction.selected = actionToUpdate.selected;
  clearSearch() {
    this.searchControl.reset();
  getActionIcon(action: ActionDisplayItem): string {
    // Return custom icon if set
    if (action.IconClass) {
      return action.IconClass;
    // Fallback icon mapping based on action name/type
    const name = action.Name.toLowerCase();
    if (name.includes('create') || name.includes('add')) return 'fa-plus-circle';
    if (name.includes('update') || name.includes('edit')) return 'fa-edit';
    if (name.includes('delete') || name.includes('remove')) return 'fa-trash';
    if (name.includes('email') || name.includes('send')) return 'fa-envelope';
    if (name.includes('export')) return 'fa-file-export';
    if (name.includes('import')) return 'fa-file-import';
    if (name.includes('report')) return 'fa-file-alt';
    if (name.includes('api')) return 'fa-plug';
    return 'fa-bolt'; // Default action icon
  // === Dialog Actions ===
  cancel() {
    this.result.next([]);
    this.dialogRef.close();
  addSelectedActions() {
    const selectedIds = this.selectedActions$.value;
    const allActions = this.allActions$.value;
    // Get the selected action display items (excluding existing ones)
    const selectedDisplayItems = allActions
      .filter(action => selectedIds.has(action.ID) && !this.existingActionIds.includes(action.ID));
    // Convert ActionDisplayItem to MJActionEntity by casting (they have the same structure)
    const selectedActions: MJActionEntity[] = selectedDisplayItems.map(item => item as MJActionEntity);
    this.result.next(selectedActions);
