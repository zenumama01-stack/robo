import { Component, Input, OnInit, OnDestroy, ViewChild, ElementRef, ChangeDetectorRef, AfterViewInit, ChangeDetectionStrategy } from '@angular/core';
import { AIAgentRunCostService } from './ai-agent-run-cost.service';
interface PromptMetrics {
  totalCount: number;
  totalExecutionTime: number;
  averageExecutionTime: number;
  byModel: Map<string, { count: number; totalTime: number; avgTime: number }>;
  byVendor: Map<string, { count: number; totalTime: number; avgTime: number }>;
  byPrompt: Map<string, { count: number; totalTime: number; avgTime: number }>;
  statusBreakdown: { success: number; failed: number; timeout: number };
  costBreakdown: { totalCost: number; byModel: Map<string, number>; byVendor: Map<string, number> };
  tokenUsage: { totalInput: number; totalOutput: number; byModel: Map<string, { input: number; output: number }> };
interface ActionMetrics {
  byAction: Map<string, { count: number; totalTime: number; avgTime: number; successRate: number }>;
  byType: Map<string, { count: number; totalTime: number; avgTime: number }>;
  errorAnalysis: Map<string, number>; // Error message to count
interface TimelineMetrics {
  stepsByType: Map<string, number>;
  parallelExecutions: number;
  deepestNesting: number;
  criticalPath: { steps: string[]; totalTime: number };
interface SimpleAgentRun {
  AgentID?: string;
  Status?: string;
  // Add other fields as needed
interface SimpleAgentRunStep {
  StepType: string;
  TargetLogID?: string;
interface SimpleActionLog {
  Action: string | null;
  StartedAt: Date | null;
  ResultCode: string | null;
  Message: string | null;
  selector: 'mj-ai-agent-run-analytics',
  templateUrl: './ai-agent-run-analytics.component.html',
  styleUrls: ['./ai-agent-run-analytics.component.css'],
export class AIAgentRunAnalyticsComponent implements OnInit, OnDestroy, AfterViewInit {
  @Input() agentRunId!: string;
  private setTrackedTimeout(callback: () => void, delay: number = 0): number {
  // Chart expansion states
  expandedCharts: { [key: string]: boolean } = {
    modelDistribution: false,
    executionTime: false,
    costByVendor: false,
    tokenUsage: false,
    promptTime: false,
    promptToken: false,
    promptCost: false,
    promptCount: false,
    actionSuccess: false,
    stepType: false
  viewMode: 'grid' | 'expanded' = 'grid';
  isLoading = true;
  agentRun: SimpleAgentRun | null = null;
  allPromptRuns: MJAIPromptRunEntity[] = [];
  allActionLogs: SimpleActionLog[] = [];
  allSteps: SimpleAgentRunStep[] = [];
  subAgentRuns: SimpleAgentRun[] = [];
  // Metrics
  promptMetrics: PromptMetrics = this.initializePromptMetrics();
  actionMetrics: ActionMetrics = this.initializeActionMetrics();
  timelineMetrics: TimelineMetrics = this.initializeTimelineMetrics();
  // Chart configurations
  modelDistributionChartData: any;
  executionTimeChartData: any;
  costByVendorChartData: any;
  tokenUsageChartData: any;
  actionSuccessRateChartData: any;
  stepTypeDistributionChartData: any;
  // Expanded sections
  promptDetailsExpanded = false;
  actionDetailsExpanded = false;
  panelStates = {
    summary: true,
    models: true,
    timeline: true
  // Chart element references
  @ViewChild('modelDistributionChart', { static: false }) modelDistributionChart!: ElementRef;
  @ViewChild('executionTimeChart', { static: false }) executionTimeChart!: ElementRef;
  @ViewChild('costByVendorChart', { static: false }) costByVendorChart!: ElementRef;
  @ViewChild('tokenUsageChart', { static: false }) tokenUsageChart!: ElementRef;
  @ViewChild('actionSuccessRateChart', { static: false }) actionSuccessRateChart!: ElementRef;
  @ViewChild('stepTypeChart', { static: false }) stepTypeChart!: ElementRef;
  @ViewChild('promptTimeDistributionChart', { static: false }) promptTimeDistributionChart!: ElementRef;
  @ViewChild('promptTokenDistributionChart', { static: false }) promptTokenDistributionChart!: ElementRef;
  @ViewChild('promptCostDistributionChart', { static: false }) promptCostDistributionChart!: ElementRef;
  @ViewChild('promptCountByNameChart', { static: false }) promptCountByNameChart!: ElementRef;
    private costService: AIAgentRunCostService
    if (this.agentRunId) {
    // Clean up all D3 charts (must be done last)
    this.cleanupAllCharts();
    console.log('AI Agent Run Analytics component destroyed and cleaned up');
  private cleanupAllCharts() {
    // List of all chart element refs
    const chartRefs = [
      this.modelDistributionChart,
      this.executionTimeChart,
      this.costByVendorChart,
      this.tokenUsageChart,
      this.actionSuccessRateChart,
      this.stepTypeChart,
      this.promptTimeDistributionChart,
      this.promptTokenDistributionChart,
      this.promptCostDistributionChart,
      this.promptCountByNameChart
    // More comprehensive D3 cleanup
    chartRefs.forEach(chartRef => {
      if (chartRef?.nativeElement) {
        const element = chartRef.nativeElement;
        const d3Element = d3.select(element);
        // Remove ALL possible event listeners (comprehensive list)
        const allEventTypes = [
          'click', 'dblclick', 'mousedown', 'mouseup', 'mouseover', 'mouseout', 
          'mousemove', 'mouseenter', 'mouseleave', 'contextmenu',
          'touchstart', 'touchend', 'touchmove', 'touchcancel',
          'wheel', 'scroll', 'resize', 'focus', 'blur',
          'keydown', 'keyup', 'keypress',
          'drag', 'dragstart', 'dragend', 'dragover', 'dragenter', 'dragleave', 'drop'
        // Remove event listeners from all child elements
        d3Element.selectAll('*').each(function() {
          const node = d3.select(this);
          allEventTypes.forEach(eventType => {
            node.on(eventType, null);
        // Remove event listeners from the main element too
          d3Element.on(eventType, null);
        // Cancel any ongoing transitions
        d3Element.selectAll('*').interrupt();
        // Remove all SVG elements and clear the container
        d3Element.selectAll('*').remove();
        // Clear innerHTML as final cleanup
        element.innerHTML = '';
    // Clear any tooltips that might be attached to body or other elements
    d3.selectAll('.d3-tooltip, .tooltip, .chart-tooltip').remove();
    // Clear any D3 selections that might be cached globally
    // Note: This is more aggressive but necessary for preventing leaks
      d3.selectAll('[data-chart-element="true"]').remove();
      d3.selectAll('.d3-tooltip').remove();
      d3.selectAll('.chart-tooltip').remove();
      console.warn('D3 global cleanup had issues:', error);
    // Charts will be rendered after data is loaded
  async loadData() {
      // Load all data including nested sub-agent runs
      await this.loadAllRunData();
      this.calculatePromptMetrics();
      this.calculateActionMetrics();
      this.calculateTimelineMetrics();
      this.prepareChartData();
      // Render charts after view updates
        this.renderCharts();
      console.error('Error loading analytics data:', error);
      this.error = 'Failed to load analytics data';
  private async loadAllRunData() {
    // Get all agent run IDs in hierarchy (including root and children)
    const agentRunIds = await this.getAllAgentRunIds(this.agentRunId);
    // Batch load all data (except prompt runs which we'll load via shared service)
      // Main agent run
        ExtraFilter: `ID = '${this.agentRunId}'`,
      // All sub-agent runs
        ExtraFilter: agentRunIds.length > 1 ? `ID IN ('${agentRunIds.slice(1).join("','")}')` : `ID = '00000000-0000-0000-0000-000000000000'`,
      // All action logs - need to get via steps
        ExtraFilter: `AgentRunID IN ('${agentRunIds.join("','")}') AND StepType = 'Actions'`,
      // All steps for timeline analysis - only need basic fields like StepType
        ExtraFilter: `AgentRunID IN ('${agentRunIds.join("','")}')`,
    if (results[0].Success && results[0].Results && results[0].Results.length > 0) {
      this.agentRun = results[0].Results[0];
      this.subAgentRuns = results[1].Results || [];
    // Load all prompt runs for the agent run hierarchy
    this.allPromptRuns = await this.loadAllPromptRuns(agentRunIds);
    if (results[2].Success) {
      const actionSteps = results[2].Results || [];
      // Now load the actual action logs
      if (actionSteps.length > 0) {
        const actionLogIds = actionSteps
          .map(s => s.TargetLogID)
          .filter(id => id != null);
        if (actionLogIds.length > 0) {
          const actionResult = await rv.RunView({
            ExtraFilter: `ID IN ('${actionLogIds.join("','")}')`
            this.allActionLogs = actionResult.Results || [];
    if (results[3].Success) {
      this.allSteps = results[3].Results || [];
  private initializePromptMetrics(): PromptMetrics {
      totalCount: 0,
      totalExecutionTime: 0,
      averageExecutionTime: 0,
      byModel: new Map(),
      byVendor: new Map(),
      byPrompt: new Map(),
      statusBreakdown: { success: 0, failed: 0, timeout: 0 },
      costBreakdown: { totalCost: 0, byModel: new Map(), byVendor: new Map() },
      tokenUsage: { totalInput: 0, totalOutput: 0, byModel: new Map() }
  private initializeActionMetrics(): ActionMetrics {
      byAction: new Map(),
      byType: new Map(),
      errorAnalysis: new Map()
  private initializeTimelineMetrics(): TimelineMetrics {
      stepsByType: new Map(),
      parallelExecutions: 0,
      deepestNesting: 0,
      criticalPath: { steps: [], totalTime: 0 }
  private calculatePromptMetrics() {
    const metrics = this.initializePromptMetrics();
    for (const promptRun of this.allPromptRuns) {
      metrics.totalCount++;
      // Calculate execution time
      const execTime = this.calculateExecutionTime(promptRun.RunAt, promptRun.CompletedAt);
      metrics.totalExecutionTime += execTime;
      // Update model metrics
      const model = promptRun.Model || 'Unknown';
      const modelMetric = metrics.byModel.get(model) || { count: 0, totalTime: 0, avgTime: 0 };
      modelMetric.count++;
      modelMetric.totalTime += execTime;
      modelMetric.avgTime = modelMetric.totalTime / modelMetric.count;
      metrics.byModel.set(model, modelMetric);
      // Update vendor metrics
      const vendor = promptRun.Vendor || 'Unknown';
      const vendorMetric = metrics.byVendor.get(vendor) || { count: 0, totalTime: 0, avgTime: 0 };
      vendorMetric.count++;
      vendorMetric.totalTime += execTime;
      vendorMetric.avgTime = vendorMetric.totalTime / vendorMetric.count;
      metrics.byVendor.set(vendor, vendorMetric);
      // Update prompt name metrics
      const promptName = promptRun.Prompt || 'Unknown';
      const promptMetric = metrics.byPrompt.get(promptName) || { count: 0, totalTime: 0, avgTime: 0 };
      promptMetric.count++;
      promptMetric.totalTime += execTime;
      promptMetric.avgTime = promptMetric.totalTime / promptMetric.count;
      metrics.byPrompt.set(promptName, promptMetric);
      // Status breakdown
      if (promptRun.Success) {
        metrics.statusBreakdown.success++;
      } else if (promptRun.ErrorMessage?.includes('timeout')) {
        metrics.statusBreakdown.timeout++;
        metrics.statusBreakdown.failed++;
      // Cost tracking
      const cost = promptRun.TotalCost || 0;
      metrics.costBreakdown.totalCost += cost;
      const modelCost = metrics.costBreakdown.byModel.get(model) || 0;
      metrics.costBreakdown.byModel.set(model, modelCost + cost);
      const vendorCost = metrics.costBreakdown.byVendor.get(vendor) || 0;
      metrics.costBreakdown.byVendor.set(vendor, vendorCost + cost);
      // Token usage
      const inputTokens = promptRun.TokensPrompt || 0;
      const outputTokens = promptRun.TokensCompletion || 0;
      metrics.tokenUsage.totalInput += inputTokens;
      metrics.tokenUsage.totalOutput += outputTokens;
      const modelTokens = metrics.tokenUsage.byModel.get(model) || { input: 0, output: 0 };
      modelTokens.input += inputTokens;
      modelTokens.output += outputTokens;
      metrics.tokenUsage.byModel.set(model, modelTokens);
    // Calculate overall average
    if (metrics.totalCount > 0) {
      metrics.averageExecutionTime = metrics.totalExecutionTime / metrics.totalCount;
    this.promptMetrics = metrics;
  private calculateActionMetrics() {
    const metrics = this.initializeActionMetrics();
    for (const actionLog of this.allActionLogs) {
      const execTime = this.calculateExecutionTime(actionLog.StartedAt, actionLog.EndedAt);
      // Update action metrics
      const actionName = actionLog.Action || 'Unknown';
      const actionMetric = metrics.byAction.get(actionName) || { count: 0, totalTime: 0, avgTime: 0, successRate: 0 };
      actionMetric.count++;
      actionMetric.totalTime += execTime;
      actionMetric.avgTime = actionMetric.totalTime / actionMetric.count;
      // Track success rate
      if (actionLog.ResultCode === 'Success') {
        const successCount = (actionMetric.successRate * (actionMetric.count - 1) + 1);
        actionMetric.successRate = successCount / actionMetric.count;
        actionMetric.successRate = (actionMetric.successRate * (actionMetric.count - 1)) / actionMetric.count;
      metrics.byAction.set(actionName, actionMetric);
      // Update type metrics
      // Action type is not directly available on MJActionExecutionLogEntity
      const actionType = 'Action'; // Generic type for now
      const typeMetric = metrics.byType.get(actionType) || { count: 0, totalTime: 0, avgTime: 0 };
      typeMetric.count++;
      typeMetric.totalTime += execTime;
      typeMetric.avgTime = typeMetric.totalTime / typeMetric.count;
      metrics.byType.set(actionType, typeMetric);
      } else if (actionLog.ResultCode === 'Timeout') {
        // Error analysis
        if (actionLog.Message) {
          const errorCount = metrics.errorAnalysis.get(actionLog.Message) || 0;
          metrics.errorAnalysis.set(actionLog.Message, errorCount + 1);
    this.actionMetrics = metrics;
  private calculateTimelineMetrics() {
    const metrics = this.initializeTimelineMetrics();
    metrics.totalSteps = this.allSteps.length;
    // Count steps by type
    for (const step of this.allSteps) {
      const type = step.StepType || 'Unknown';
      const count = metrics.stepsByType.get(type) || 0;
      metrics.stepsByType.set(type, count + 1);
    // TODO: Calculate parallel executions, deepest nesting, and critical path
    // This would require more complex analysis of the step relationships
    this.timelineMetrics = metrics;
  private calculateExecutionTime(start: Date | null, end: Date | null): number {
    if (!start || !end) return 0;
    return new Date(end).getTime() - new Date(start).getTime();
  private prepareChartData() {
    // Model distribution pie chart
    this.modelDistributionChartData = {
      labels: Array.from(this.promptMetrics.byModel.keys()),
        data: Array.from(this.promptMetrics.byModel.values()).map(m => m.count),
        backgroundColor: this.generateColors(this.promptMetrics.byModel.size)
    // Execution time by vendor bar chart
    this.executionTimeChartData = {
      labels: Array.from(this.promptMetrics.byVendor.keys()),
        label: 'Average Execution Time (ms)',
        data: Array.from(this.promptMetrics.byVendor.values()).map(v => v.avgTime),
        backgroundColor: 'rgba(54, 162, 235, 0.5)',
        borderColor: 'rgba(54, 162, 235, 1)',
    // Cost by vendor doughnut chart
    this.costByVendorChartData = {
      labels: Array.from(this.promptMetrics.costBreakdown.byVendor.keys()),
        data: Array.from(this.promptMetrics.costBreakdown.byVendor.values()),
        backgroundColor: this.generateColors(this.promptMetrics.costBreakdown.byVendor.size)
    // Token usage stacked bar chart
    const tokenModels = Array.from(this.promptMetrics.tokenUsage.byModel.keys());
    this.tokenUsageChartData = {
      labels: tokenModels,
      datasets: [
          label: 'Input Tokens',
          data: tokenModels.map(m => this.promptMetrics.tokenUsage.byModel.get(m)?.input || 0),
          backgroundColor: 'rgba(255, 99, 132, 0.5)'
          label: 'Output Tokens',
          data: tokenModels.map(m => this.promptMetrics.tokenUsage.byModel.get(m)?.output || 0),
          backgroundColor: 'rgba(75, 192, 192, 0.5)'
    // Action success rate bar chart
    this.actionSuccessRateChartData = {
      labels: Array.from(this.actionMetrics.byAction.keys()),
        label: 'Success Rate (%)',
        data: Array.from(this.actionMetrics.byAction.values()).map(a => a.successRate * 100),
        backgroundColor: 'rgba(75, 192, 192, 0.5)',
        borderColor: 'rgba(75, 192, 192, 1)',
    // Step type distribution pie chart
    this.stepTypeDistributionChartData = {
      labels: Array.from(this.timelineMetrics.stepsByType.keys()),
        data: Array.from(this.timelineMetrics.stepsByType.values()),
        backgroundColor: this.generateColors(this.timelineMetrics.stepsByType.size)
  private generateColors(count: number): string[] {
      'rgba(255, 99, 132, 0.5)',
      'rgba(54, 162, 235, 0.5)',
      'rgba(255, 206, 86, 0.5)',
      'rgba(75, 192, 192, 0.5)',
      'rgba(153, 102, 255, 0.5)',
      'rgba(255, 159, 64, 0.5)',
      'rgba(199, 199, 199, 0.5)',
      'rgba(83, 102, 255, 0.5)',
      'rgba(255, 99, 255, 0.5)',
      'rgba(99, 255, 132, 0.5)'
    // Repeat colors if needed
    const result: string[] = [];
    for (let i = 0; i < count; i++) {
      result.push(colors[i % colors.length]);
    if (ms < 1000) return `${ms.toFixed(0)}ms`;
    if (ms < 3600000) return `${Math.floor(ms / 60000)}m ${Math.floor((ms % 60000) / 1000)}s`;
    return `${Math.floor(ms / 3600000)}h ${Math.floor((ms % 3600000) / 60000)}m`;
    if (cost < 0.01) return `$${cost.toFixed(4)}`;
    if (cost < 1) return `$${cost.toFixed(3)}`;
  refresh() {
  private renderCharts() {
    if (!this.modelDistributionChart) return; // Charts not ready yet
    this.renderModelDistributionChart();
    this.renderExecutionTimeChart();
    this.renderCostByVendorChart();
    this.renderTokenUsageChart();
    this.renderActionSuccessRateChart();
    this.renderStepTypeChart();
    // Additional prompt analytics charts
    this.renderPromptTimeDistributionChart();
    this.renderPromptTokenDistributionChart();
    this.renderPromptCostDistributionChart();
    this.renderPromptCountByNameChart();
  private renderModelDistributionChart() {
    const element = this.modelDistributionChart.nativeElement;
    const data = Array.from(this.promptMetrics.byModel.entries()).map(([name, metrics]) => ({
      value: metrics.count
    d3.select(element).selectAll('*').remove();
    const isExpanded = this.expandedCharts['modelDistribution'];
    const width = isExpanded ? 500 : 300;
    const height = isExpanded ? 400 : 220;
    const radius = Math.min(width, height) / 2 - 40;
    const svg = d3.select(element)
      .attr('height', height)
      .attr('transform', `translate(${width / 2}, ${height / 2})`);
    const color = d3.scaleOrdinal(d3.schemeCategory10);
    const pie = d3.pie<any>()
      .value(d => d.value)
    const arc = d3.arc()
    const arcs = svg.selectAll('arc')
      .data(pie(data))
      .append('g');
    arcs.append('path')
      .attr('d', arc as any)
      .attr('fill', (d, i) => color(i.toString()))
      .attr('stroke', 'white')
      .style('stroke-width', '2px');
    // Add labels
    arcs.append('text')
      .attr('transform', (d: any) => `translate(${arc.centroid(d)})`)
      .style('fill', 'white')
      .text((d: any) => d.data.value > 0 ? d.data.value : '');
      .attr('y', -radius - 25)
      .style('font-size', '16px')
      .text('Prompts by Model');
  private renderExecutionTimeChart() {
    const element = this.executionTimeChart.nativeElement;
    const data = Array.from(this.promptMetrics.byVendor.entries()).map(([name, metrics]) => ({
      value: metrics.avgTime
    const isExpanded = this.expandedCharts['executionTime'];
    const margin = { top: 40, right: 20, bottom: 70, left: 60 };
    const width = (isExpanded ? 600 : 320) - margin.left - margin.right;
    const height = (isExpanded ? 350 : 200) - margin.top - margin.bottom;
    const x = d3.scaleBand()
      .domain(data.map(d => d.name))
      .padding(0.1);
    const y = d3.scaleLinear()
      .domain([0, d3.max(data, d => d.value) || 0])
    // Add bars
      .data(data)
      .enter().append('rect')
      .attr('x', d => x(d.name) || 0)
      .attr('y', d => y(d.value))
      .attr('height', d => height - y(d.value))
      .attr('fill', '#36a2eb');
    // Add x axis
      .call(d3.axisBottom(x))
      .style('text-anchor', 'end');
    // Add y axis
      .call(d3.axisLeft(y).tickFormat(d => `${d}ms`));
      .attr('y', -20)
      .text('Average Execution Time by Vendor');
  private renderCostByVendorChart() {
    const element = this.costByVendorChart.nativeElement;
    const data = Array.from(this.promptMetrics.costBreakdown.byVendor.entries()).map(([name, cost]) => ({
      value: cost
    const width = 300;
    const height = 220;
    const color = d3.scaleOrdinal(d3.schemeSet2);
      .innerRadius(radius * 0.5) // Doughnut chart
      .text('Cost Distribution by Vendor');
  private renderTokenUsageChart() {
    const element = this.tokenUsageChart.nativeElement;
    const models = Array.from(this.promptMetrics.tokenUsage.byModel.keys());
    const inputData = models.map(model => ({
      type: 'Input',
      value: this.promptMetrics.tokenUsage.byModel.get(model)?.input || 0
    const outputData = models.map(model => ({
      type: 'Output',
      value: this.promptMetrics.tokenUsage.byModel.get(model)?.output || 0
    const data = [...inputData, ...outputData];
    const margin = { top: 40, right: 100, bottom: 70, left: 60 };
    const width = 320 - margin.left - margin.right;
    const height = 200 - margin.top - margin.bottom;
    const x0 = d3.scaleBand()
      .rangeRound([0, width])
      .paddingInner(0.1)
      .domain(models);
    const x1 = d3.scaleBand()
      .padding(0.05)
      .domain(['Input', 'Output'])
      .rangeRound([0, x0.bandwidth()]);
      .rangeRound([height, 0])
      .domain([0, d3.max(data, d => d.value) || 0]);
    const color = d3.scaleOrdinal()
      .range(['#ff6384', '#4bc0c0']);
    const grouped = d3.group(data, d => d.model);
      .data(grouped)
      .attr('transform', d => `translate(${x0(d[0]) || 0},0)`)
      .selectAll('rect')
      .data(d => d[1])
      .attr('x', d => x1(d.type) || 0)
      .attr('width', x1.bandwidth())
      .attr('fill', d => color(d.type) as string);
      .call(d3.axisBottom(x0))
      .call(d3.axisLeft(y));
      .attr('font-family', 'sans-serif')
      .attr('font-size', 10)
      .data(['Input', 'Output'])
      .attr('transform', (d, i) => `translate(0,${i * 20})`);
      .attr('x', width + 70)
      .attr('width', 19)
      .attr('height', 19)
      .attr('fill', d => color(d) as string);
      .attr('x', width + 65)
      .attr('y', 9.5)
      .attr('dy', '0.32em')
      .text(d => d);
      .text('Token Usage by Model');
  private renderActionSuccessRateChart() {
    const element = this.actionSuccessRateChart.nativeElement;
    const data = Array.from(this.actionMetrics.byAction.entries()).map(([name, metrics]) => ({
      value: metrics.successRate * 100
    const margin = { top: 40, right: 20, bottom: 100, left: 60 };
      .domain([0, 100])
    // Add bars with color based on success rate
      .attr('fill', d => d.value > 90 ? '#4bc0c0' : d.value > 70 ? '#ffce56' : '#ff6384');
      .call(d3.axisLeft(y).tickFormat(d => `${d}%`));
      .text('Action Success Rates');
  private renderStepTypeChart() {
    const element = this.stepTypeChart.nativeElement;
    const data = Array.from(this.timelineMetrics.stepsByType.entries()).map(([name, value]) => ({
    const color = d3.scaleOrdinal(d3.schemeSet3);
      .attr('transform', (d: any) => {
        const centroid = arc.centroid(d);
      .text('Step Type Distribution');
  calculatePromptSuccessRate(promptName: string): string {
    const successfulRuns = this.allPromptRuns.filter(run => 
      run.Prompt === promptName && run.Success === true
    const totalRuns = this.allPromptRuns.filter(run => 
      run.Prompt === promptName
    if (totalRuns === 0) return '0';
    return ((successfulRuns / totalRuns) * 100).toFixed(1);
  toggleChartExpansion(chartKey: string): void {
    this.expandedCharts[chartKey] = !this.expandedCharts[chartKey];
    // Re-render the chart after expansion state changes
  toggleViewMode(): void {
    if (this.viewMode === 'grid') {
      this.viewMode = 'expanded';
      // Expand all charts
      Object.keys(this.expandedCharts).forEach(key => {
        this.expandedCharts[key] = true;
      this.viewMode = 'grid';
      // Collapse all charts
        this.expandedCharts[key] = false;
  getActionType(actionName: string): string {
    return 'Action';
  getTopErrors(): Array<{ message: string; count: number }> {
    return Array.from(this.actionMetrics.errorAnalysis.entries())
      .map(([message, count]) => {
        // Try to extract meaningful error message from potentially complex error strings
        let cleanMessage = message;
        // If it looks like a view execution result, extract the key part
        if (message.includes('View executed successfully but returned no data')) {
          cleanMessage = 'View executed successfully but returned no data';
        } else if (message.length > 200) {
          // For very long error messages, truncate and add ellipsis
          cleanMessage = message.substring(0, 200) + '...';
        return { message: cleanMessage, count };
      .slice(0, 5); // Top 5 errors
  getModelPerformanceData(): any[] {
    const modelData: Map<string, any> = new Map();
      const key = `${model}|${vendor}`;
      if (!modelData.has(key)) {
        modelData.set(key, {
          name: model,
          vendor: vendor,
          totalTime: 0,
          avgTime: 0,
          avgCost: 0,
          inputTokens: 0,
          outputTokens: 0
      const data = modelData.get(key)!;
      data.count++;
      data.totalTime += execTime;
      data.avgTime = data.totalTime / data.count;
      data.totalCost += promptRun.TotalCost || 0;
      data.avgCost = data.totalCost / data.count;
      data.inputTokens += promptRun.TokensPrompt || 0;
      data.outputTokens += promptRun.TokensCompletion || 0;
    return Array.from(modelData.values()).sort((a, b) => b.count - a.count);
  getStepTypeIcon(stepType: string): string {
    const iconMap: Record<string, string> = {
      'Prompts': 'fa-microchip',
      'Actions': 'fa-cog',
      'Sub-Agent': 'fa-robot',
      'Start': 'fa-play-circle',
      'End': 'fa-stop-circle',
      'Decision': 'fa-code-branch',
      'Loop': 'fa-sync',
      'Error': 'fa-exclamation-triangle'
    return iconMap[stepType] || 'fa-circle';
  getModelColor(model: string): string {
    // Return a color from the same palette used in generateColors
    // Use a simple hash of the model name to consistently pick a color
    for (let i = 0; i < model.length; i++) {
      hash = model.charCodeAt(i) + ((hash << 5) - hash);
    return colors[Math.abs(hash) % colors.length];
  private renderPromptTimeDistributionChart() {
    const element = this.promptTimeDistributionChart.nativeElement;
    const data = Array.from(this.promptMetrics.byPrompt.entries())
      .map(([name, metrics]) => ({
        name: name.length > 20 ? name.substring(0, 20) + '...' : name,
      .sort((a, b) => b.value - a.value)
      .slice(0, 10); // Top 10 prompts by avg time
    const margin = { top: 40, right: 20, bottom: 120, left: 80 };
      .attr('fill', '#667eea');
      .style('font-size', '10px');
      .text('Average Execution Time by Prompt');
  private renderPromptTokenDistributionChart() {
    const element = this.promptTokenDistributionChart.nativeElement;
    // Aggregate token data by prompt
    const promptTokenData = new Map<string, { input: number; output: number }>();
    for (const run of this.allPromptRuns) {
      const promptName = run.Prompt || 'Unknown';
      const existing = promptTokenData.get(promptName) || { input: 0, output: 0 };
      existing.input += run.TokensPrompt || 0;
      existing.output += run.TokensCompletion || 0;
      promptTokenData.set(promptName, existing);
    const topPrompts = Array.from(promptTokenData.entries())
      .sort((a, b) => (b[1].input + b[1].output) - (a[1].input + a[1].output))
      .slice(0, 8)
      .map(([name]) => name);
    const inputData = topPrompts.map(prompt => ({
      prompt: prompt.length > 15 ? prompt.substring(0, 15) + '...' : prompt,
      value: promptTokenData.get(prompt)?.input || 0
    const outputData = topPrompts.map(prompt => ({
      value: promptTokenData.get(prompt)?.output || 0
    const margin = { top: 40, right: 100, bottom: 100, left: 80 };
      .domain(topPrompts.map(p => p.length > 15 ? p.substring(0, 15) + '...' : p));
      .range(['#764ba2', '#667eea']);
    const grouped = d3.group(data, d => d.prompt);
      .call(d3.axisLeft(y).tickFormat(d => d3.format('.2s')(d)));
      .text('Token Usage by Prompt');
  private renderPromptCostDistributionChart() {
    const element = this.promptCostDistributionChart.nativeElement;
    // Aggregate cost data by prompt
    const promptCostData = new Map<string, number>();
      const existing = promptCostData.get(promptName) || 0;
      promptCostData.set(promptName, existing + (run.TotalCost || 0));
    const data = Array.from(promptCostData.entries())
      .filter(([_, cost]) => cost > 0)
      .map(([name, cost]) => ({
      .slice(0, 10); // Top 10 prompts by cost
    const color = d3.scaleOrdinal(d3.schemePurples[9].slice(2));
      .innerRadius(radius * 0.5)
      .text('Cost Distribution by Prompt');
    // Add total cost in center
      .attr('dy', '-0.5em')
      .text('Total Cost');
      .style('font-size', '18px')
      .text(this.formatCost(data.reduce((sum, d) => sum + d.value, 0)));
  private renderPromptCountByNameChart() {
    const element = this.promptCountByNameChart.nativeElement;
      .slice(0, 10); // Top 10 prompts by count
    const margin = { top: 40, right: 20, bottom: 120, left: 60 };
    // Create gradient
    const gradient = svg.append('defs')
      .append('linearGradient')
      .attr('id', 'promptCountGradient')
      .attr('stop-color', '#667eea')
      .attr('stop-color', '#764ba2')
      .attr('fill', 'url(#promptCountGradient)');
    svg.selectAll('.bar-label')
      .attr('class', 'bar-label')
      .attr('x', d => (x(d.name) || 0) + x.bandwidth() / 2)
      .attr('y', d => y(d.value) - 5)
      .text(d => d.value);
      .text('Prompt Execution Count');
   * Get all agent run IDs in hierarchy, starting from the root run
  private async getAllAgentRunIds(rootRunId: string): Promise<string[]> {
    const agentRunIds: string[] = [rootRunId];
    // Simple recursive approach to find all child runs
    const findChildRuns = async (parentId: string): Promise<void> => {
        ExtraFilter: `ParentRunID = '${parentId}'`,
        for (const childRun of result.Results) {
          if (!agentRunIds.includes(childRun.ID)) {
            agentRunIds.push(childRun.ID);
            await findChildRuns(childRun.ID); // Recursively find children
    await findChildRuns(rootRunId);
    return agentRunIds;
   * Load all prompt runs for the given agent run IDs
   * Uses the same approach as the cost calculation: find prompt runs via agent run steps
  private async loadAllPromptRuns(agentRunIds: string[]): Promise<any[]> {
    if (agentRunIds.length === 0) return [];
    // First, get all the prompt steps for the agent runs
    const stepsResult = await rv.RunView({
      ExtraFilter: `AgentRunID IN ('${agentRunIds.join("','")}') AND StepType = 'Prompt'`,
    if (!stepsResult.Success || !stepsResult.Results || stepsResult.Results.length === 0) {
    // Extract the TargetLogID values (these are the prompt run IDs)
    const promptRunIds = stepsResult.Results
      .map(step => step.TargetLogID)
      .filter(id => id); // Remove any null/undefined values
    if (promptRunIds.length === 0) {
    // Now get the actual prompt runs
    const promptResult = await rv.RunView({
      ExtraFilter: `ID IN ('${promptRunIds.join("','")}')`,
    return promptResult.Success ? (promptResult.Results || []) : [];
   * TrackBy function for keyvalue pipe
  trackByKey(index: number, item: { key: string; value: any }): string {
    return item.key;
   * TrackBy function for error messages
  trackByErrorMessage(index: number, error: { message: string; count: number }): string {
    return error.message;
   * TrackBy function for model performance data
  trackByModelName(index: number, model: any): string {
    return model.name || index.toString();
