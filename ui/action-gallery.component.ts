import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { Subject, BehaviorSubject, combineLatest, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { MJActionEntity, MJActionCategoryEntity, MJActionParamEntity, MJActionResultCodeEntity } from '@memberjunction/core-entities';
export interface ActionGalleryConfig {
  selectionMode?: boolean;
  showCategories?: boolean;
  showSearch?: boolean;
  defaultView?: 'grid' | 'list';
  gridColumns?: number;
  enableQuickTest?: boolean;
  theme?: 'light' | 'dark';
export interface CategoryNode {
  parent?: string;
  children?: CategoryNode[];
export interface ActionWithDetails extends MJActionEntity {
  parameters?: MJActionParamEntity[];
  resultCodes?: MJActionResultCodeEntity[];
  selected?: boolean;
  selector: 'mj-action-gallery',
  templateUrl: './action-gallery.component.html',
  styleUrls: ['./action-gallery.component.css']
export class ActionGalleryComponent implements OnInit, OnDestroy {
  @Input() config: ActionGalleryConfig = {
    showCategories: true,
    defaultView: 'grid',
    gridColumns: 3,
    enableQuickTest: true,
    theme: 'light'
  @Input() preSelectedActions: string[] = [];
  @Output() actionSelected = new EventEmitter<MJActionEntity>();
  @Output() actionsSelected = new EventEmitter<MJActionEntity[]>();
  @Output() actionTestRequested = new EventEmitter<MJActionEntity>();
  @ViewChild('searchInput', { static: false }) searchInput: ElementRef<HTMLInputElement>;
  actions$ = new BehaviorSubject<ActionWithDetails[]>([]);
  filteredActions$ = new BehaviorSubject<ActionWithDetails[]>([]);
  categoryTree$ = new BehaviorSubject<CategoryNode[]>([]);
  selectedCategory$ = new BehaviorSubject<string>('all');
  // Form controls
  hoveredAction: string | null = null;
  animateCards = false;
  totalActions = 0;
  categoryCounts = new Map<string, number>();
    // Set initial view mode
    this.viewMode$.next(this.config.defaultView || 'grid');
    // Set up filtering
      this.actions$,
        distinctUntilChanged()
      this.selectedCategory$
    ).subscribe(([actions, searchTerm, category]) => {
      this.filterActions(actions, searchTerm || '', category);
    // Initialize with pre-selected actions
    if (this.preSelectedActions.length > 0) {
      this.selectedActions$.next(new Set(this.preSelectedActions));
    // Enable animations after initial load
      this.animateCards = true;
      // Load actions and categories in parallel
      if (actionsResult.Success && categoriesResult.Success) {
        const actions = actionsResult.Results as MJActionEntity[] || [];
        const categories = categoriesResult.Results as MJActionCategoryEntity[] || [];
        // Process actions
        const actionsWithDetails = actions.map(action => ({
          selected: this.preSelectedActions.includes(action.ID)
        } as ActionWithDetails));
        this.actions$.next(actionsWithDetails);
        this.totalActions = actions.length;
        // Process categories
        this.buildCategoryTree(categories, actions);
        // Initial filter
        this.filterActions(actionsWithDetails, '', 'all');
      console.error('Error loading gallery data:', error);
  private buildCategoryTree(categories: MJActionCategoryEntity[], actions: MJActionEntity[]) {
    this.categoryCounts.clear();
      const count = this.categoryCounts.get(action.Category || 'Uncategorized') || 0;
      this.categoryCounts.set(action.Category || 'Uncategorized', count + 1);
    const nodeMap = new Map<string, CategoryNode>();
      const node: CategoryNode = {
        parent: cat.ParentID || undefined,
        count: this.categoryCounts.get(cat.Name) || 0,
        icon: this.getCategoryIcon(cat.Name)
      nodeMap.set(cat.ID, node);
      if (node.parent && nodeMap.has(node.parent)) {
        const parent = nodeMap.get(node.parent)!;
        parent.children!.push(node);
        // Accumulate counts up the tree
        parent.count = (parent.count || 0) + (node.count || 0);
    // Add "All" category at the top
    const allNode: CategoryNode = {
      count: this.totalActions,
    // Add "Uncategorized" if needed
    const uncategorizedCount = this.categoryCounts.get('Uncategorized') || 0;
      const uncategorizedNode: CategoryNode = {
      rootNodes.push(uncategorizedNode);
    this.categoryTree$.next([allNode, ...rootNodes]);
      'Security': 'fa-shield',
      'Files': 'fa-file',
  getCategoryIconClass(category: CategoryNode): string {
    // Ensure we have a valid icon
    if (!category.icon) {
      return 'fa-solid fa-folder category-icon';
    return `fa-solid ${category.icon} category-icon`;
  private filterActions(actions: ActionWithDetails[], searchTerm: string, category: string) {
    if (category && category !== 'all') {
      if (category === 'uncategorized') {
        filtered = filtered.filter(a => !a.Category);
        const categoryName = this.getCategoryName(category);
        filtered = filtered.filter(a => a.Category === categoryName);
        action.Description?.toLowerCase().includes(term) ||
        action.Category?.toLowerCase().includes(term)
  private getCategoryName(categoryId: string): string {
  toggleCategoryExpanded(categoryId: string) {
  async toggleActionExpanded(action: ActionWithDetails) {
    action.expanded = !action.expanded;
    // Load details if expanding and not already loaded
    if (action.expanded && !action.parameters) {
      await this.loadActionDetails(action);
  async loadActionDetails(action: ActionWithDetails) {
      const [paramsResult, resultCodesResult] = await rv.RunViews([
          OrderBy: 'ResultCode',
      if (paramsResult.Success) {
        action.parameters = paramsResult.Results as MJActionParamEntity[] || [];
      if (resultCodesResult.Success) {
        action.resultCodes = resultCodesResult.Results as MJActionResultCodeEntity[] || [];
      console.error('Error loading action details:', error);
  toggleActionSelection(action: ActionWithDetails) {
    if (!this.config.selectionMode) return;
      if (!action.selected) {
        action.selected = true;
        this.actionSelected.emit(action);
      if (action.selected) {
        action.selected = false;
      const selectedActions = this.actions$.value.filter(a => selected.has(a.ID));
      this.actionsSelected.emit(selectedActions);
  testAction(action: MJActionEntity, event: Event) {
    if (this.config.enableQuickTest) {
      // TODO: Implement test harness integration
      console.log('Test action:', action.Name);
    this.actionTestRequested.emit(action);
    if (this.searchInput) {
  getSelectedActions(): MJActionEntity[] {
    return this.actions$.value.filter(a => selected.has(a.ID));
  getActionIcon(action: MJActionEntity): string {
    const typeIcons: { [key: string]: string } = {
      'Create': 'fa-plus-circle',
      'Update': 'fa-edit',
      'Delete': 'fa-trash',
      'Query': 'fa-search',
      'Process': 'fa-cogs',
      'Email': 'fa-envelope',
      'Export': 'fa-file-export',
      'Import': 'fa-file-import',
      'API': 'fa-plug',
      'Script': 'fa-code'
    for (const [key, icon] of Object.entries(typeIcons)) {
      if (action.Name.toLowerCase().includes(key.toLowerCase()) || 
          action.Type?.toLowerCase().includes(key.toLowerCase())) {
    return 'fa-bolt';
