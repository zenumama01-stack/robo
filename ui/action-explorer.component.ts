  ViewChild
import { Router, NavigationEnd } from '@angular/router';
import { takeUntil, filter, debounceTime, distinctUntilChanged, combineLatestWith } from 'rxjs/operators';
import { CompositeKey, LogError, RunView } from '@memberjunction/core';
import { MJActionCategoryEntity, MJActionEntity, MJActionParamEntity, ResourceData } from '@memberjunction/core-entities';
import { ActionEngineBase, ActionEntityExtended } from '@memberjunction/actions-base';
  ActionExplorerStateService,
  ActionViewMode,
  SortConfig,
  ActionFilters
} from '../../services/action-explorer-state.service';
import { ActionTreePanelComponent } from './action-tree-panel.component';
@RegisterClass(BaseResourceComponent, 'ActionExplorerResource')
  selector: 'mj-action-explorer',
  templateUrl: './action-explorer.component.html',
  styleUrls: ['./action-explorer.component.css'],
  providers: [ActionExplorerStateService]
export class ActionExplorerComponent extends BaseResourceComponent implements OnInit, OnDestroy {
  @ViewChild(ActionTreePanelComponent) TreePanel!: ActionTreePanelComponent;
  public Actions: ActionEntityExtended[] = [];
  public FilteredActions: ActionEntityExtended[] = [];
  public Categories: MJActionCategoryEntity[] = [];
  public CategoriesMap = new Map<string, MJActionCategoryEntity>();
  public ViewMode: ActionViewMode = 'card';
  public SelectedCategoryId = 'all';
  public NewCategoryParentId: string | null = null;
  // Run dialog state
  public IsRunDialogOpen = false;
  public SelectedActionForRun: MJActionEntity | null = null;
  public SelectedActionParams: MJActionParamEntity[] = [];
  private lastNavigatedUrl = '';
  private skipUrlUpdate = false;
    public StateService: ActionExplorerStateService,
    this.subscribeToState();
    this.subscribeToRouterEvents();
    this.loadInitialState();
  private subscribeToState(): void {
    // Subscribe to view mode changes
    this.StateService.ViewMode$.pipe(
    ).subscribe(mode => {
      this.ViewMode = mode;
    // Subscribe to selected category changes
    this.StateService.SelectedCategoryId$.pipe(
    ).subscribe(id => {
      this.SelectedCategoryId = id;
      this.updateUrl();
    // Subscribe to filter and sort changes
    this.StateService.Filters$.pipe(
      combineLatestWith(this.StateService.SortConfig$),
      debounceTime(50),
  private subscribeToRouterEvents(): void {
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
    ).subscribe(event => {
      const currentUrl = event.urlAfterRedirects || event.url;
      if (currentUrl !== this.lastNavigatedUrl) {
        this.onExternalNavigation(currentUrl);
  private loadInitialState(): void {
    // Parse URL query params
    const url = this.router.url;
    const queryIndex = url.indexOf('?');
    if (queryIndex !== -1) {
      const queryString = url.substring(queryIndex + 1);
      const params = new URLSearchParams(queryString);
      this.skipUrlUpdate = true;
      this.StateService.parseQueryParams(params);
      this.skipUrlUpdate = false;
    // Load saved state from UserInfoEngine
    this.StateService.loadSavedState();
      // Initialize ActionEngineBase
      // Get actions and categories from engine
      this.Actions = ActionEngineBase.Instance.Actions;
      this.Categories = ActionEngineBase.Instance.ActionCategories;
      // Build categories map
      this.CategoriesMap.clear();
      this.Categories.forEach(c => this.CategoriesMap.set(c.ID, c));
      // Build tree panel if available
      if (this.TreePanel) {
        this.TreePanel.Categories = this.Categories;
        this.TreePanel.Actions = this.Actions;
        this.TreePanel.buildCategoryTree();
      // Expand path to selected category if needed
      if (this.SelectedCategoryId !== 'all' && this.SelectedCategoryId !== 'uncategorized') {
        const categoryParentMap = new Map<string, string | null>();
        this.Categories.forEach(c => categoryParentMap.set(c.ID, c.ParentID || null));
        this.StateService.expandPathToCategory(this.SelectedCategoryId, categoryParentMap);
      LogError('Failed to load action explorer data', undefined, error);
    let filtered = [...this.Actions];
    const filters = this.StateService.Filters;
    const sortConfig = this.StateService.SortConfig;
      filtered = filtered.filter(a => !a.CategoryID);
    } else if (this.SelectedCategoryId !== 'all') {
      // Include descendants
      const descendants = this.getDescendantCategoryIds(this.SelectedCategoryId);
      filtered = filtered.filter(a => a.CategoryID && descendants.has(a.CategoryID));
    if (filters.searchTerm) {
      const term = filters.searchTerm.toLowerCase();
      filtered = filtered.filter(a =>
        a.Name.toLowerCase().includes(term) ||
        (a.Description || '').toLowerCase().includes(term)
    if (filters.statuses.length > 0) {
      filtered = filtered.filter(a => filters.statuses.includes(a.Status));
    if (filters.types.length > 0) {
      filtered = filtered.filter(a => filters.types.includes(a.Type));
    filtered = this.sortActions(filtered, sortConfig);
    this.FilteredActions = filtered;
  private getDescendantCategoryIds(categoryId: string): Set<string> {
    const descendants = new Set<string>([categoryId]);
      this.Categories.forEach(c => {
        if (c.ParentID === parentId && !descendants.has(c.ID)) {
          descendants.add(c.ID);
          addDescendants(c.ID);
    addDescendants(categoryId);
    return descendants;
  private sortActions(actions: ActionEntityExtended[], config: SortConfig): ActionEntityExtended[] {
    const sorted = [...actions];
    const direction = config.direction === 'asc' ? 1 : -1;
      switch (config.field) {
          comparison = a.Name.localeCompare(b.Name);
          const dateA = a.__mj_UpdatedAt ? new Date(a.__mj_UpdatedAt).getTime() : 0;
          const dateB = b.__mj_UpdatedAt ? new Date(b.__mj_UpdatedAt).getTime() : 0;
          comparison = dateA - dateB;
          comparison = (a.Status || '').localeCompare(b.Status || '');
          comparison = (a.Type || '').localeCompare(b.Type || '');
        case 'category':
          const catA = this.CategoriesMap.get(a.CategoryID || '')?.Name || '';
          const catB = this.CategoriesMap.get(b.CategoryID || '')?.Name || '';
          comparison = catA.localeCompare(catB);
      return comparison * direction;
  private updateUrl(): void {
    if (this.skipUrlUpdate) return;
    const queryParams = this.StateService.buildQueryParams();
    this.navigationService.UpdateActiveTabQueryParams(queryParams);
    this.lastNavigatedUrl = this.router.url;
  private onExternalNavigation(url: string): void {
  public async onRefresh(): Promise<void> {
    await ActionEngineBase.Instance.Config(true); // Force refresh
  public onCategorySelect(categoryId: string): void {
    this.StateService.setSelectedCategoryId(categoryId);
  public onNewCategory(parentId: string | null): void {
    this.NewCategoryParentId = parentId;
    this.StateService.openNewCategoryPanel();
  public onEditCategory(category: MJActionCategoryEntity): void {
    const key = new CompositeKey([{ FieldName: 'ID', Value: category.ID }]);
  public onNewAction(): void {
    this.StateService.openNewActionPanel();
  public onActionClick(action: ActionEntityExtended): void {
  public onActionEdit(action: ActionEntityExtended): void {
    this.onActionClick(action);
  public async onActionRun(action: ActionEntityExtended): Promise<void> {
    if (action.Status !== 'Active') {
      return; // Can't run inactive actions
      // Load action params
        ExtraFilter: `ActionID='${action.ID}'`,
      this.SelectedActionParams = result.Success ? result.Results || [] : [];
      // ActionEntityExtended extends MJActionEntity, so this cast is safe
      this.SelectedActionForRun = action as unknown as MJActionEntity;
      this.IsRunDialogOpen = true;
      LogError('Failed to open action run dialog', undefined, error);
  public OnRunDialogClose(): void {
    this.IsRunDialogOpen = false;
    this.SelectedActionForRun = null;
    this.SelectedActionParams = [];
  public onCategoryClick(categoryId: string): void {
  public async onCategoryCreated(category: MJActionCategoryEntity): Promise<void> {
    // Refresh data to include new category
    // Select the new category
    this.StateService.setSelectedCategoryId(category.ID);
  public async onActionCreated(): Promise<void> {
    // Refresh data to include new action
    return 'Action Explorer';
    return 'fa-solid fa-folder-tree';
