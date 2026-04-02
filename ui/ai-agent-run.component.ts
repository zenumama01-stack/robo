import { Component, OnInit, OnDestroy, ViewChild, inject } from '@angular/core';
import { CompositeKey, Metadata } from '@memberjunction/core';
import { AIAgentRunEntityExtended, AIAgentEntityExtended } from '@memberjunction/ai-core-plus';
import { TimelineItem, AIAgentRunTimelineComponent } from './ai-agent-run-timeline.component';
import { MJAIAgentRunFormComponent } from '../../generated/Entities/MJAIAgentRun/mjaiagentrun.form.component';
import { AIAgentRunAnalyticsComponent } from './ai-agent-run-analytics.component';
import { AIAgentRunVisualizationComponent } from './ai-agent-run-visualization.component';
import { AIAgentRunCostService, AgentRunCostMetrics } from './ai-agent-run-cost.service';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Runs') 
  selector: 'mj-ai-agent-run-form',
  templateUrl: './ai-agent-run.component.html',
  styleUrls: ['./ai-agent-run.component.css']
export class AIAgentRunFormComponentExtended extends MJAIAgentRunFormComponent implements OnInit, OnDestroy {
  public record!: AIAgentRunEntityExtended;
  activeTab = 'timeline';
  selectedTimelineItem: TimelineItem | null = null;
  jsonPanelExpanded = false;
  visualizationLoaded = false;
  agent: AIAgentEntityExtended | null = null;
  // Cost metrics using shared service
  costMetrics: AgentRunCostMetrics | null = null;
  // Cached parsed results to prevent redundant JSON parsing
  private _cachedParsedResult: string | null = null;
  private _cachedParsedStartingPayload: string | null = null;
  private _cachedParsedFinalPayload: string | null = null;
  private _cachedParsedData: string | null = null;
  // Simple parsing state - true when all parsing is complete
  private _allParsingComplete = false;
  @ViewChild(AIAgentRunTimelineComponent) timelineComponent?: AIAgentRunTimelineComponent;
  @ViewChild(AIAgentRunAnalyticsComponent) analyticsComponent?: AIAgentRunAnalyticsComponent;
  @ViewChild(AIAgentRunVisualizationComponent) visualizationComponent?: AIAgentRunVisualizationComponent;
  private costService = inject(AIAgentRunCostService);
  // Instance of data helper per component
  public dataHelper = new AIAgentRunDataHelper();
      await this.dataHelper.loadAgentRunData(this.record.ID);
      await this.loadAgent();
      await this.loadCostMetrics();
      // Parse all JSON fields on form load for instant access later
      this.parseAllFields();
    this.clearParsedCache();
    this.dataHelper.clearData();
    this.costMetrics = null;
    this.agent = null;
  private async loadAgent() {
    if (!this.record?.AgentID) return;
      if (agent && await agent.Load(this.record.AgentID)) {
        this.agent = agent;
      console.error('Error loading agent:', error);
  private async loadCostMetrics() {
      this.costMetrics = await this.costService.getAgentRunCostMetrics(this.record.ID);
      console.error('Error loading cost metrics:', error);
      this.costMetrics = {
        error: 'Failed to load cost data'
    // Lazy load visualization when the tab is first accessed
    if (tab === 'visualization' && !this.visualizationLoaded) {
      this.visualizationLoaded = true;
    // Lazy load analytics when the tab is first accessed
    if (tab === 'analytics' && !this.analyticsLoaded) {
    const ms = end.getTime() - start.getTime();
  selectTimelineItem(item: TimelineItem) {
    this.selectedTimelineItem = item;
    this.jsonPanelExpanded = true;
  closeJsonPanel() {
    this.selectedTimelineItem = null;
  navigateToSubRun(runId: string) {
    SharedService.Instance.OpenEntityRecord("MJ: AI Agent Runs", CompositeKey.FromID(runId));
  navigateToParentRun() {
    if (this.record.ParentRunID) {
      SharedService.Instance.OpenEntityRecord("MJ: AI Agent Runs", CompositeKey.FromID(this.record.ParentRunID));
    SharedService.Instance.OpenEntityRecord("MJ: Action Execution Logs", CompositeKey.FromID(logId));
  openEntityRecord(entityName: string, recordId: string | null) {
    if (recordId) {
  navigateToEntityRecord(event: { entityName: string; recordId: string }) {
    SharedService.Instance.OpenEntityRecord(event.entityName, CompositeKey.FromID(event.recordId));
   * Navigate to the conversation in the Chat application
  navigateToConversation() {
    if (!this.record?.ConversationID) return;
    // Find the Chat app
    const chatApp = this.appManager.GetAllApps().find(app => app.Name === 'Chat');
    if (!chatApp) {
      console.warn('Chat application not found');
    // Navigate to the Conversations nav item with the conversationId parameter
    this.navigationService.OpenNavItemByName(
      'Conversations',
      { conversationId: this.record.ConversationID },
      chatApp.ID
  refreshData() {
    // Reload the agent run record to get latest status
      // Clear parsed cache when refreshing data
      // No panel states to reset in simplified approach
      this.record.Load(this.record.ID).then(() => {
        // Clear cost cache and reload
        this.costService.clearCache(this.record.ID);
        this.loadCostMetrics();
        // Reload data through helper - this will update all components (force reload for refresh)
        this.dataHelper.loadAgentRunData(this.record.ID, true);
        // Trigger analytics refresh
        if (this.analyticsComponent) {
          this.analyticsComponent.loadData();
  onAgentRunCompleted(status: string) {
    // Update the record status
    this.record.Status = status as 'Running' | 'Completed' | 'Failed' | 'Cancelled' | 'Paused';
    // Reload the full record to get updated data
    this.refreshData();
   * Get the Result field with recursive JSON parsing applied
  get parsedResult(): string {
    if (!this.record?.Result) {
    // Return cached result if available
    if (this._cachedParsedResult !== null) {
      return this._cachedParsedResult;
      // First, check if Result is a JSON string that needs to be parsed
      let resultData = this.record.Result;
        // If Result is a JSON string, parse it first
        resultData = JSON.parse(this.record.Result);
        resultData = this.record.Result;
        debug: false // Disable debug logging - regex issue fixed
      // Re-enabled ParseJSONRecursive with performance optimizations
      const parsed = ParseJSONRecursive(resultData, parseOptions);
      const result = JSON.stringify(parsed, null, 2);
      this._cachedParsedResult = result;
      const fallbackResult = this.record.Result;
      this._cachedParsedResult = fallbackResult;
      return fallbackResult;
   * Get the Starting Payload field with recursive JSON parsing applied
  get parsedStartingPayload(): string {
    if (!this.record?.StartingPayload) {
    if (this._cachedParsedStartingPayload !== null) {
      return this._cachedParsedStartingPayload;
      // First, check if StartingPayload is a JSON string that needs to be parsed
      let payloadData = this.record.StartingPayload;
        // If StartingPayload is a JSON string, parse it first
        payloadData = JSON.parse(this.record.StartingPayload);
        payloadData = this.record.StartingPayload;
      const parsed = ParseJSONRecursive(payloadData, parseOptions);
      this._cachedParsedStartingPayload = result;
      const fallbackResult = this.record.StartingPayload;
      this._cachedParsedStartingPayload = fallbackResult;
   * Get the Final Payload (state) field with recursive JSON parsing applied
  get parsedFinalPayload(): string {
    if (!this.record?.FinalPayload) return '';
    if (this._cachedParsedFinalPayload !== null) {
      return this._cachedParsedFinalPayload;
      // First, check if FinalPayload is a JSON string that needs to be parsed
      let payloadData = this.record.FinalPayload;
        // If FinalPayload is a JSON string, parse it first
        payloadData = JSON.parse(this.record.FinalPayload);
        payloadData = this.record.FinalPayload;
      this._cachedParsedFinalPayload = result;
      const fallbackResult = this.record.FinalPayload;
      this._cachedParsedFinalPayload = fallbackResult;
   * Get the Data field with recursive JSON parsing applied
  get parsedData(): string {
    if (!this.record?.Data) return '';
    if (this._cachedParsedData !== null) {
      return this._cachedParsedData;
      // First, check if Data is a JSON string that needs to be parsed
      let data = this.record.Data;
        // If Data is a JSON string, parse it first
        data = JSON.parse(this.record.Data);
        data = this.record.Data;
      const parsed = ParseJSONRecursive(data, parseOptions);
      this._cachedParsedData = result;
      const fallbackResult = this.record.Data;
      this._cachedParsedData = fallbackResult;
   * Get parsed Starting Payload as an object for deep diff
  get startingPayloadObject(): any {
    if (!this.record?.StartingPayload) return null;
   * Get parsed Final Payload as an object for deep diff
  get finalPayloadObject(): any {
    if (!this.record?.FinalPayload) return null;
   * Clear all cached parsed results
  private clearParsedCache(): void {
    this._cachedParsedResult = null;
    this._cachedParsedStartingPayload = null;
    this._cachedParsedFinalPayload = null;
    this._cachedParsedData = null;
    this._allParsingComplete = false;
   * Parse all JSON fields at once and cache results
  private parseAllFields(): void {
      let parsedCount = 0;
      // Parse all fields that exist
      if (this.record?.Result) {
        this.parsedResult; // Triggers parsing and caching
        parsedCount++;
      if (this.record?.StartingPayload) {
        this.parsedStartingPayload; // Triggers parsing and caching
      if (this.record?.FinalPayload) {
        this.parsedFinalPayload; // Triggers parsing and caching
      if (this.record?.Data) {
        this.parsedData; // Triggers parsing and caching
      this._allParsingComplete = true;
      console.error('Error during JSON parsing:', error);
   * Check if all parsing is complete - used by template
  get isParsingComplete(): boolean {
    return this._allParsingComplete;
   * Check if we have both payloads to show diff
  get showPayloadDiff(): boolean {
    return !!(this.record?.StartingPayload && this.record?.FinalPayload);
    if (!stepData || !stepData.PayloadAtStart) {
    if (!stepData || !stepData.PayloadAtEnd) {
