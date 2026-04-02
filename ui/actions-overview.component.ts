import { CompositeKey, RunView, LogError } from '@memberjunction/core';
import { MJActionEntity, MJActionCategoryEntity, MJActionExecutionLogEntity, ResourceData } from '@memberjunction/core-entities';
  totalActions: number;
  activeActions: number;
  pendingActions: number;
  disabledActions: number;
  recentExecutions: number;
  totalCategories: number;
  aiGeneratedActions: number;
  customActions: number;
interface CategoryStats {
  categoryName: string;
  executionCount: number;
interface ExecutionWithExpanded extends MJActionExecutionLogEntity {
 * Actions Overview Resource - displays action management dashboard
@RegisterClass(BaseResourceComponent, 'ActionsOverviewResource')
  selector: 'mj-actions-overview',
  templateUrl: './actions-overview.component.html',
  styleUrls: ['./actions-overview.component.css']
export class ActionsOverviewComponent extends BaseResourceComponent implements OnInit, OnDestroy {
  public metrics: ActionMetrics = {
    totalActions: 0,
    activeActions: 0,
    pendingActions: 0,
    disabledActions: 0,
    totalExecutions: 0,
    recentExecutions: 0,
    totalCategories: 0,
    aiGeneratedActions: 0,
    customActions: 0
  public categoryStats: CategoryStats[] = [];
  public recentActions: MJActionEntity[] = [];
  public recentExecutions: ExecutionWithExpanded[] = [];
  public topCategories: MJActionCategoryEntity[] = [];
  constructor(private navigationService: NavigationService, 
              private cdr: ChangeDetectorRef ) {
      this.selectedType$.pipe(distinctUntilChanged())
      this.loadFilteredData();
      // Load all data in a single batch using RunViews
      const [actionsResult, categoriesResult, executionsResult] = await rv.RunViews([
          OrderBy: '__mj_UpdatedAt DESC' 
      if (!actionsResult.Success || !categoriesResult.Success || !executionsResult.Success) {
        if (!actionsResult.Success) errors.push('Actions: ' + actionsResult.ErrorMessage);
        if (!categoriesResult.Success) errors.push('Categories: ' + categoriesResult.ErrorMessage);
        if (!executionsResult.Success) errors.push('Executions: ' + executionsResult.ErrorMessage);
      const executions = (executionsResult.Results || []) as MJActionExecutionLogEntity[];
      this.calculateMetrics(actions, categories, executions);
      this.calculateCategoryStats(actions, categories, executions);
      this.recentActions = actions.slice(0, 10);
      this.recentExecutions = executions.slice(0, 10).map(e => ({ ...e, isExpanded: false } as ExecutionWithExpanded));
      this.topCategories = categories.slice(0, 5);
      console.error('Error loading actions overview data:', error);
      LogError('Failed to load actions overview data', undefined, error);
  private calculateMetrics(
    categories: MJActionCategoryEntity[], 
    executions: MJActionExecutionLogEntity[]
    this.metrics = {
      activeActions: actions.filter(a => a.Status === 'Active').length,
      pendingActions: actions.filter(a => a.Status === 'Pending').length,
      disabledActions: actions.filter(a => a.Status === 'Disabled').length,
      totalExecutions: executions.length,
      recentExecutions: executions.filter(e => {
        const dayAgo = new Date();
        dayAgo.setDate(dayAgo.getDate() - 1);
        return e.StartedAt && new Date(e.StartedAt) > dayAgo;
      successRate: this.calculateSuccessRate(executions),
      totalCategories: categories.length,
      aiGeneratedActions: actions.filter(a => a.Type === 'Generated').length,
      customActions: actions.filter(a => a.Type === 'Custom').length
  private calculateSuccessRate(executions: MJActionExecutionLogEntity[]): number {
    if (!executions || executions.length === 0) return 0;
    // Check for success based on result code - Actions may use different success codes
    const successful = executions.filter(e => {
      const code = e.ResultCode?.toLowerCase();
    }).length;
    return Math.round((successful / executions.length) * 100);
  private calculateCategoryStats(
    this.categoryStats = categories.map(category => {
      const categoryActions = actions.filter(a => a.CategoryID === category.ID);
      const categoryExecutions = executions.filter(e => 
        categoryActions.some(a => a.ID === e.ActionID)
        categoryId: category.ID,
        categoryName: category.Name,
        actionCount: categoryActions.length,
        executionCount: categoryExecutions.length,
        successRate: this.calculateSuccessRate(categoryExecutions)
  private async loadFilteredData(): Promise<void> {
    // Implement filtered data loading based on current filter values
    const searchTerm = this.searchTerm$.value;
    let extraFilter = '';
      filters.push(`Status = '${status}'`);
      filters.push(`Type = '${type}'`);
      extraFilter = filters.join(' AND ');
        OrderBy: '__mj_UpdatedAt DESC',
        UserSearchString: searchTerm,
      this.recentActions = ((result.Results || []) as MJActionEntity[]).slice(0, 10);
      LogError('Failed to load filtered actions', undefined, error);
    const key = new CompositeKey([{ FieldName: 'ID', Value: action.ID }]);
    this.navigationService.OpenEntityRecord('MJ: Actions', key);
  public openCategory(categoryId: string): void {
    const key = new CompositeKey([{ FieldName: 'ID', Value: categoryId }]);
    this.navigationService.OpenEntityRecord('MJ: Action Categories', key);
  public openExecution(execution: MJActionExecutionLogEntity): void {
    const key = new CompositeKey([{ FieldName: 'ID', Value: execution.ID }]);
    this.navigationService.OpenEntityRecord('MJ: Action Execution Logs', key);
  public isExecutionSuccess(execution: MJActionExecutionLogEntity): boolean {
  // Metric card click handlers - these now filter the current view
  public onTotalActionsClick(): void {
    // Reset filters to show all actions
    this.selectedStatus$.next('all');
    this.selectedType$.next('all');
  public onExecutionsClick(): void {
    // This would navigate to execution monitoring resource
  public onCategoriesClick(): void {
    // This would navigate to categories view
  public onAIGeneratedClick(): void {
    // Filter to show AI generated actions in the current view
    this.selectedType$.next('Generated');
  public onActionGalleryClick(): void {
    // This would navigate to action gallery view
  public toggleExecutionExpanded(execution: ExecutionWithExpanded): void {
    execution.isExpanded = !execution.isExpanded;
  public formatJsonParams(params: string | null): string {
    if (!params) return '{}';
      // Try to parse and reformat
      const parsed = JSON.parse(params);
      // If parse fails, return as is
    return 'Actions Overview';
