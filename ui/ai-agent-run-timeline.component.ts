import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Subject, Observable, combineLatest, interval, of, from, Subscription } from 'rxjs';
import { takeUntil, map, shareReplay, switchMap, filter } from 'rxjs/operators';
import { AIAgentRunDataHelper } from './ai-agent-run-data.service';
export interface TimelineItem {
  type: 'step' | 'subrun' | 'action' | 'prompt';
  subtitle: string;
  endTime?: Date;
  duration?: string;
  data: any;
  children?: TimelineItem[];
  isExpanded?: boolean;
  childrenLoaded?: boolean;
  hasNoChildren?: boolean;
  selector: 'mj-ai-agent-run-timeline',
  templateUrl: './ai-agent-run-timeline.component.html',
  styleUrls: ['./ai-agent-run-timeline.component.css'],
export class AIAgentRunTimelineComponent implements OnInit, OnDestroy {
  @Input() aiAgentRunId!: string;
  @Input() autoRefresh = false;
  @Input() refreshInterval = 30000; // Minimum 30 seconds
  @Input() dataHelper!: AIAgentRunDataHelper; // Data helper passed from parent
  @Output() itemSelected = new EventEmitter<TimelineItem>();
  @Output() agentRunCompleted = new EventEmitter<string>();
  // Public observables from data helper
  steps$!: Observable<MJAIAgentRunStepEntity[]>;
  subRuns$!: Observable<MJAIAgentRunEntity[]>;
  actionLogs$!: Observable<MJActionExecutionLogEntity[]>;
  promptRuns$!: Observable<MJAIPromptRunEntity[]>;
  timelineItems$!: Observable<TimelineItem[]>;
  selectedItem: TimelineItem | null = null;
  private refreshTimer: any;
  private refreshSubscription: Subscription | null = null;
    // Initialize observables from the data helper
    this.steps$ = this.dataHelper.steps$;
    this.subRuns$ = this.dataHelper.subRuns$;
    this.actionLogs$ = this.dataHelper.actionLogs$;
    this.promptRuns$ = this.dataHelper.promptRuns$;
    // Combine all data sources to build timeline
    this.timelineItems$ = combineLatest([
      this.steps$,
      this.subRuns$,
      this.actionLogs$,
      this.promptRuns$
      map(([steps, subRuns, actionLogs, promptRuns]) => 
        this.buildTimelineItems(steps, subRuns, actionLogs, promptRuns)
      shareReplay(1)
    // Data loading is now handled by the parent component through the helper
    // Subscribe to loading state from helper
    this.dataHelper.loading$.pipe(takeUntil(this.destroy$)).subscribe(loading => {
      this.loading = loading;
    this.dataHelper.error$.pipe(takeUntil(this.destroy$)).subscribe(error => {
    // Auto-refresh logic
    if (this.autoRefresh) {
    if (this.refreshTimer) {
      clearInterval(this.refreshTimer);
    // Ensure minimum 30 second interval
    const refreshIntervalMs = Math.max(30000, this.refreshInterval);
    // Don't create multiple subscriptions - subscribe once and use interval
    this.refreshSubscription = interval(refreshIntervalMs)
        takeUntil(this.destroy$),
        // Get the latest agent run status
        switchMap(() => {
          if (!this.aiAgentRunId) return of(null);
          return from(rv.RunView({
            ExtraFilter: `ID = '${this.aiAgentRunId}'`,
        filter(result => result !== null && result.Success && result.Results?.length > 0),
        map(result => result!.Results[0])
      .subscribe(agentRun => {
        // Check if the agent run is still running
        if (agentRun.Status === 'Running') {
          // Reload data
          this.dataHelper.loadAgentRunData(this.aiAgentRunId);
          // Agent run completed/failed - stop refresh
          console.log(`Agent run ${agentRun.Status} - stopping auto-refresh`);
          this.stopAutoRefresh();
          // Emit event to parent to update status
          this.agentRunCompleted.emit(agentRun.Status);
  private stopAutoRefresh() {
    if (this.refreshSubscription) {
      this.refreshSubscription.unsubscribe();
      this.refreshSubscription = null;
      this.refreshTimer = null;
  // This method is now just for compatibility - actual loading is done by parent
    if (!this.aiAgentRunId) return;
    // The parent component should handle data loading through the helper
    return this.dataHelper.loadAgentRunData(this.aiAgentRunId);
  private buildTimelineItems(
    steps: MJAIAgentRunStepEntity[],
    subRuns: MJAIAgentRunEntity[],
    actionLogs: MJActionExecutionLogEntity[],
    promptRuns: MJAIPromptRunEntity[]
  ): TimelineItem[] {
    return this.buildHierarchicalItems(steps, 0, promptRuns);
  private buildHierarchicalItems(
    baseLevel: number,
    promptRuns?: MJAIPromptRunEntity[]
    // Create a map of all timeline items by step ID
    const itemMap = new Map<string, TimelineItem>();
    // First pass: create all timeline items
    steps.forEach(step => {
      const item = this.createTimelineItemFromStep(step, baseLevel, promptRuns);
      itemMap.set(step.ID, item);
    // Second pass: build parent-child relationships based on ParentID
      if (step.ParentID) {
        const parentItem = itemMap.get(step.ParentID);
        const childItem = itemMap.get(step.ID);
        if (parentItem && childItem) {
          // Initialize children array if needed
          if (!parentItem.children) {
            parentItem.children = [];
          // Set child's level based on parent's level
          childItem.level = parentItem.level + 1;
          // Add child to parent's children array
          parentItem.children.push(childItem);
    // Return only root-level items (those without a ParentID)
    const rootItems: TimelineItem[] = [];
      if (!step.ParentID) {
        const item = itemMap.get(step.ID);
          rootItems.push(item);
    return rootItems;
  private createTimelineItemFromStep(step: MJAIAgentRunStepEntity, level: number, promptRuns?: MJAIPromptRunEntity[]): TimelineItem {
    let subtitle = `Type: ${step.StepType}`;
    // For prompt steps, try to find the associated prompt run to get model/vendor info
    if (step.StepType === 'Prompt' && step.TargetLogID && promptRuns) {
      const promptRun = promptRuns.find(pr => pr.ID === step.TargetLogID);
        subtitle = `Model: ${promptRun.Model || 'Unknown'} | Vendor: ${promptRun.Vendor || 'Unknown'}`;
    // Get icon and logoUrl based on step type
    const iconInfo = this.getStepIconInfo(step);
      id: step.ID,
      type: 'step',
      title: step.StepName || `Step ${step.StepNumber}`,
      subtitle: subtitle,
      startTime: step.StartedAt,
      endTime: step.CompletedAt || undefined,
      duration: this.calculateDuration(step.StartedAt, step.CompletedAt),
      icon: iconInfo.icon,
      logoUrl: iconInfo.logoUrl,
      color: this.getStatusColor(step.Status),
      data: step,
      isExpanded: false
  private getStepIconInfo(step: MJAIAgentRunStepEntity): { icon: string; logoUrl?: string } {
    // For sub-agents, try to get agent-specific icon/logo
    if (step.StepType === 'Sub-Agent' && step.TargetID) {
      const agent = AIEngineBase.Instance.Agents.find(a => a.ID === step.TargetID);
        // Prefer LogoURL - if present, use it with robot as fallback icon (icon won't be shown when logoUrl exists)
        if (agent.LogoURL) {
          return { icon: 'fa-robot', logoUrl: agent.LogoURL };
        // Next preference: IconClass from agent metadata
        else if (agent.IconClass) {
          return { icon: agent.IconClass };
        // Agent exists but has no custom icon or logo - use default robot icon
          return { icon: 'fa-robot' };
    // Default icons for each step type (includes fa-robot for sub-agents without agent metadata)
    const icon = this.getStepIcon(step.StepType);
    return { icon };
  private getStepIcon(stepType: string): string {
      'Prompt': 'fa-brain',
      'Tool': 'fa-tools',
      'Actions': 'fa-wrench',
      'Validation': 'fa-square-check',
      'ForEach': 'fa-repeat',
      'While': 'fa-rotate'
  private getStatusColor(status: string): string {
    const colorMap: Record<string, string> = {
      'Running': 'info',
      'Completed': 'success',
      'Failed': 'error',
      'Cancelled': 'warning',
      'Paused': 'secondary'
    return colorMap[status] || 'secondary';
  calculateDuration(start: Date, end?: Date | null): string {
    if (!end) return 'Running...';
    const startTime = new Date(start).getTime();
    const endTime = new Date(end).getTime();
    if (duration < 1000) return `${duration}ms`;
    if (duration < 60000) return `${(duration / 1000).toFixed(1)}s`;
    if (duration < 3600000) return `${Math.floor(duration / 60000)}m ${Math.floor((duration % 60000) / 1000)}s`;
    return `${Math.floor(duration / 3600000)}h ${Math.floor((duration % 3600000) / 60000)}m`;
  selectItem(item: TimelineItem) {
    this.selectedItem = item;
    this.itemSelected.emit(item);
  async toggleItemExpansion(item: TimelineItem, event: Event) {
    // Toggle expansion state
    item.isExpanded = !item.isExpanded;
    // For Sub-Agent steps, load their run data on demand (requires DB query)
    if (item.isExpanded && !item.childrenLoaded && item.type === 'step' && item.data?.StepType === 'Sub-Agent') {
      await this.loadSubAgentChildren(item);
    // For parent steps (loop containers like ForEach/While), children are already loaded via ParentID
    // Just toggle - no additional loading needed since we already have all steps from the run
    // The children were already attached in buildTimelineItems()
  private async loadSubAgentChildren(item: TimelineItem) {
      const subAgentRunId = item.data?.TargetLogID;
      if (!subAgentRunId) {
        item.hasNoChildren = true;
        item.children = [];
        item.childrenLoaded = true;
      // Load sub-agent data through service
      const data = await this.dataHelper.loadSubAgentData(subAgentRunId);
      if (!data.steps || data.steps.length === 0) {
      // Build hierarchical timeline items with ParentID relationships
      // This ensures that loop steps (ForEach/While) within sub-agents also show their children
      item.children = this.buildHierarchicalItems(data.steps, item.level + 1, data.promptRuns);
      // Trigger change detection after updating the data
      console.error('🔄 Timeline: Error loading sub-agent children:', error);
      // Trigger change detection for error state
  navigateToSubRun(runId: string, event: Event) {
    this.navigateToEntity.emit({ entityName: 'MJ: AI Agent Runs', recordId: runId });
  navigateToActionLog(logId: string, event: Event) {
    this.navigateToEntity.emit({ entityName: 'MJ: Action Execution Logs', recordId: logId });
  navigateToPromptRun(runId: string, event: Event) {
    this.navigateToEntity.emit({ entityName: 'MJ: AI Prompt Runs', recordId: runId });
   * TrackBy function for timeline items
  trackByItemId(index: number, item: TimelineItem): string {
    return item.id;
  createSubRunDataHelper(): AIAgentRunDataHelper {
    // Create a new data helper instance for sub-runs to prevent caching conflicts
    return new AIAgentRunDataHelper();
