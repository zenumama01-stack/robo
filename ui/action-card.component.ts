  ChangeDetectorRef
import { MJActionCategoryEntity, MJActionExecutionLogEntity } from '@memberjunction/core-entities';
import { ActionEntityExtended } from '@memberjunction/actions-base';
export interface ActionExecutionStats {
  lastExecuted: Date | null;
  selector: 'mj-action-card',
  templateUrl: './action-card.component.html',
  styleUrls: ['./action-card.component.css'],
export class ActionCardComponent {
  @Input() Action!: ActionEntityExtended;
  @Input() Categories: Map<string, MJActionCategoryEntity> = new Map();
  @Output() ActionClick = new EventEmitter<ActionEntityExtended>();
  @Output() EditClick = new EventEmitter<ActionEntityExtended>();
  @Output() RunClick = new EventEmitter<ActionEntityExtended>();
  @Output() CategoryClick = new EventEmitter<string>();
  public IsExpanded = false;
  public ExecutionStats: ActionExecutionStats = {
    lastExecuted: null,
    isLoaded: false
  public onCardClick(): void {
    this.ActionClick.emit(this.Action);
  public onEditClick(event: MouseEvent): void {
    this.EditClick.emit(this.Action);
  public onRunClick(event: MouseEvent): void {
    this.RunClick.emit(this.Action);
  public onCategoryClick(event: MouseEvent): void {
    if (this.Action.CategoryID) {
      this.CategoryClick.emit(this.Action.CategoryID);
  public toggleExpanded(event: MouseEvent): void {
    this.IsExpanded = !this.IsExpanded;
    if (this.IsExpanded && !this.ExecutionStats.isLoaded && !this.ExecutionStats.isLoading) {
      this.loadExecutionStats();
  private async loadExecutionStats(): Promise<void> {
    this.ExecutionStats.isLoading = true;
      // Load both executions and result codes in parallel
      const [executionsResult, resultCodesResult] = await rv.RunViews([
          ExtraFilter: `ActionID='${this.Action.ID}'`,
          Fields: ['ID', 'ResultCode', 'StartedAt']
          Fields: ['ResultCode', 'IsSuccess']
      if (executionsResult.Success && executionsResult.Results) {
        const executions = executionsResult.Results as { ID: string; ResultCode: string; StartedAt: Date }[];
        // Build a map of result codes to IsSuccess
        const successMap = new Map<string, boolean>();
        if (resultCodesResult.Success && resultCodesResult.Results) {
          const resultCodes = resultCodesResult.Results as { ResultCode: string; IsSuccess: boolean }[];
          resultCodes.forEach(rc => {
            successMap.set(rc.ResultCode, rc.IsSuccess);
        // Count successful executions using result code lookup
          if (!e.ResultCode) return false;
          // Look up the result code in the map
          const isSuccess = successMap.get(e.ResultCode);
          // If result code is defined in the map, use it; otherwise fall back to heuristic
          if (isSuccess !== undefined) {
          // Fallback heuristic for unknown result codes
          const code = e.ResultCode.toLowerCase();
          return code === 'success' || code === 'ok' || code === 'completed';
        this.ExecutionStats = {
          successRate: executions.length > 0 ? Math.round((successful / executions.length) * 100) : 0,
          lastExecuted: executions.length > 0 ? new Date(executions[0].StartedAt) : null,
          isLoaded: true
        this.ExecutionStats.isLoading = false;
        this.ExecutionStats.isLoaded = true;
      console.error('Failed to load execution stats:', error);
  public getCategoryName(): string {
    if (!this.Action.CategoryID) return 'Uncategorized';
    return this.Categories.get(this.Action.CategoryID)?.Name || 'Unknown Category';
  public getStatusColor(): 'success' | 'warning' | 'error' | 'info' {
    switch (this.Action.Status) {
    // Use custom icon if set, otherwise derive from type
    if (this.Action.IconClass) {
      return this.Action.IconClass;
    switch (this.Action.Type) {
      default: return 'fa-solid fa-bolt';
  public getTypeLabel(): string {
    return this.Action.Type === 'Generated' ? 'AI Generated' : 'Custom';
  public getApprovalStatusIcon(): string {
    switch (this.Action.CodeApprovalStatus) {
      case 'Approved': return 'fa-solid fa-check-circle';
      case 'Pending': return 'fa-solid fa-clock';
      case 'Rejected': return 'fa-solid fa-times-circle';
      default: return 'fa-solid fa-question-circle';
  public getApprovalStatusColor(): string {
      case 'Approved': return 'success';
      case 'Rejected': return 'error';
    const diff = now.getTime() - date.getTime();
