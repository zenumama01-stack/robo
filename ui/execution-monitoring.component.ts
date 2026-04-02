import { Component, OnInit, OnDestroy, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { map, takeUntil, debounceTime } from 'rxjs/operators';
  DashboardKPIs,
  TrendData,
  LiveExecution,
  ChartData,
  ExecutionDetails
} from '../services/ai-instrumentation.service';
import { DataPointClickEvent } from './charts/time-series-chart.component';
import { KPICardData } from './widgets/kpi-card.component';
import { HeatmapData } from './charts/performance-heatmap.component';
import { RunView, CompositeKey } from '@memberjunction/core';
import { ResourceData } from "@memberjunction/core-entities";
import { AIPromptRunEntityExtended, AIAgentRunEntityExtended, AIModelEntityExtended } from '@memberjunction/ai-core-plus';
import { BaseResourceComponent, NavigationService } from '@memberjunction/ng-shared';
export interface DrillDownTab {
  type: 'chart' | 'executions' | 'model-detail';
  timestamp?: Date;
  metric?: string;
  closeable: boolean;
export interface ExecutionRecord {
  type: 'prompt' | 'agent';
  tokens: number;
export interface ExecutionMonitoringState {
  selectedTimeRange: string;
  refreshInterval: number;
  panelStates: {
    cost: boolean;
    efficiency: boolean;
    executions: boolean;
  drillDownTabs: Array<{
    timestamp?: string;
  activeTabId: string;
  splitterSizes?: number[];
 * AI Monitor Resource - displays AI execution monitoring and analytics
 * Extends BaseResourceComponent to work with the resource type system
@RegisterClass(BaseResourceComponent, 'AIMonitorResource')
  selector: 'app-execution-monitoring',
  changeDetection: ChangeDetectionStrategy.OnPush,
    <div class="execution-monitoring" [class.loading]="isLoading">
      <!-- Loading Overlay -->
          <mj-loading text="Loading dashboard data..." size="large"></mj-loading>
      <!-- Header Controls -->
      <div class="monitoring-header">
        <h2 class="monitoring-title">
          AI Execution Monitoring
        <div class="monitoring-controls">
          <div class="time-range-control">
            <label>Time Range:</label>
            <select [(ngModel)]="selectedTimeRange" (change)="onTimeRangeChange()" [disabled]="isLoading">
              <option value="1h">Last Hour</option>
              <option value="6h">Last 6 Hours</option>
              <option value="24h">Last 24 Hours</option>
          <button class="refresh-btn" (click)="refreshData()" [disabled]="isLoading">
            <i class="fa-solid fa-refresh" [class.spinning]="isLoading"></i>
      <!-- KPI Dashboard -->
      <div class="kpi-dashboard">
        <div class="kpi-grid">
          @for (kpi of kpiCards$ | async; track kpi.title) {
            <app-kpi-card 
              [data]="kpi"
              (click)="onKpiClick(kpi)"
              [class.clickable]="isKpiClickable(kpi)"
            ></app-kpi-card>
      <!-- Main Dashboard with Kendo Splitter -->
      <kendo-splitter class="dashboard-splitter" orientation="vertical" (layoutChange)="onSplitterLayoutChange($event)">
        <!-- Top Row: System Health and Trends Chart -->
        <kendo-splitter-pane size="45%" [resizable]="true" [collapsible]="false">
          <kendo-splitter orientation="horizontal" (layoutChange)="onSplitterLayoutChange($event)">
            <!-- System Health -->
            <kendo-splitter-pane size="30%" [resizable]="true" [collapsible]="true" [collapsed]="false">
              <div class="dashboard-section system-status">
                <div class="status-container">
                    <h4 class="chart-title">
                      <i class="fa-solid fa-heartbeat"></i>
                      System Health
                  @if (kpis$ | async; as kpis) {
                    <div class="status-metrics">
                      <div class="status-metric">
                        <div class="status-icon status-icon--success">
                        <div class="status-info">
                          <div class="status-label">Success Rate</div>
                          <div class="status-value">{{ (kpis.successRate * 100).toFixed(1) }}%</div>
                          <div class="status-subtitle">Last {{ selectedTimeRange }}</div>
                        <div class="status-icon status-icon--warning">
                          <div class="status-label">Error Rate</div>
                          <div class="status-value">{{ (kpis.errorRate * 100).toFixed(1) }}%</div>
                          <div class="status-subtitle">{{ kpis.totalExecutions }} total executions</div>
                        <div class="status-icon status-icon--info">
                          <div class="status-label">Avg Response Time</div>
                          <div class="status-value">{{ (kpis.avgExecutionTime / 1000).toFixed(2) }}s</div>
                          <div class="status-subtitle">Across all models</div>
                        <div class="status-icon status-icon--primary">
                          <div class="status-label">Active Executions</div>
                          <div class="status-value">{{ kpis.activeExecutions }}</div>
                          <div class="status-subtitle">Currently running</div>
            <!-- Drill-down Tab Container -->
            <kendo-splitter-pane [resizable]="true" [collapsible]="false">
              <div class="dashboard-section drill-down-container">
                <div class="drill-down-tabs">
                  <div class="tab-header">
                    @for (tab of drillDownTabs; track tab.id) {
                        class="tab-item"
                        [class.active]="activeTabId === tab.id"
                        (click)="selectTab(tab.id)"
                        <span class="tab-title">{{ tab.title }}</span>
                        @if (tab.closeable) {
                            class="tab-close"
                            (click)="closeTab($event, tab.id)"
                            title="Close tab"
                    @if (activeTab?.type === 'chart') {
                      <div class="tab-pane trends-chart">
                        <app-time-series-chart
                          [data]="(trends$ | async) ?? []"
                          title="Execution Trends"
                          [config]="timeSeriesConfig"
                          (dataPointClick)="onDataPointClick($event)"
                          (timeRangeChange)="onChartTimeRangeChange($event)"
                        ></app-time-series-chart>
                    @if (activeTab?.type === 'executions') {
                      <div class="tab-pane executions-drill-down">
                        <div class="drill-down-header">
                            {{ activeTab?.title }}
                          <div class="drill-down-meta">
                            @if (activeTab?.timestamp) {
                              <span class="timestamp">{{ getFormattedTimestamp(activeTab) }}</span>
                            @if (activeTab?.metric) {
                              <span class="metric-badge">{{ getFormattedMetricLabel(activeTab) }}</span>
                        @if (loadingDrillDown) {
                            <mj-loading text="Loading execution details..." size="small"></mj-loading>
                        } @else if (activeTab?.data?.length > 0) {
                            <div class="table-header">
                              <div class="header-cell">Type</div>
                              <div class="header-cell">Name</div>
                              <div class="header-cell">Model</div>
                              <div class="header-cell">Status</div>
                              <div class="header-cell">Duration</div>
                              <div class="header-cell">Cost</div>
                              <div class="header-cell">Tokens</div>
                              <div class="header-cell">Time</div>
                            @for (execution of activeTab?.data; track execution.id) {
                                class="table-row"
                                (click)="viewExecutionDetail(execution)"
                                <div class="table-cell">
                                  <span class="type-badge" [class]="'type-badge--' + execution.type">
                                    {{ execution.type }}
                                <div class="table-cell">{{ execution.name }}</div>
                                <div class="table-cell">{{ execution.model || 'N/A' }}</div>
                                  <span class="status-badge" [class]="'status-badge--' + execution.status">
                                    {{ execution.status }}
                                <div class="table-cell">{{ formatDuration(execution.duration) }}</div>
                                <div class="table-cell">{{ formatCurrency(execution.cost) }}</div>
                                <div class="table-cell">{{ execution.tokens.toLocaleString() }}</div>
                                <div class="table-cell">{{ formatTime(execution.startTime) }}</div>
                          <div class="no-data">
                            <p>No executions found for this time period</p>
                    @if (activeTab?.type === 'model-detail') {
                      <div class="tab-pane model-detail">
                            Model Details: {{ activeTab?.data?.name }}
                            <mj-loading text="Loading model details..." size="small"></mj-loading>
                        } @else if (activeTab?.data) {
                            <div class="model-info-grid">
                                <label>Model Name:</label>
                                <span>{{ activeTab?.data?.name }}</span>
                                <label>Vendor:</label>
                                <span>{{ activeTab?.data?.vendor }}</span>
                                <label>API Name:</label>
                                <span>{{ activeTab?.data?.apiName }}</span>
                                <label>Input Cost:</label>
                                <span>\${{ activeTab?.data?.inputTokenCost?.toFixed(6) || '0' }} per token</span>
                                <label>Output Cost:</label>
                                <span>\${{ activeTab?.data?.outputTokenCost?.toFixed(6) || '0' }} per token</span>
                                <label>Active:</label>
                                <span class="status-indicator" [class.active]="activeTab?.data?.isActive">
                                  {{ activeTab?.data?.isActive ? 'Yes' : 'No' }}
                            @if (activeTab?.data?.description) {
                              <div class="model-description">
                                <h5>Description</h5>
                                <p>{{ activeTab?.data?.description }}</p>
        <!-- Bottom Row: Analysis Panels with Expansion Layout -->
            <!-- Left: Performance Heatmap -->
            <kendo-splitter-pane size="50%" [resizable]="true" [collapsible]="false">
              <div class="dashboard-section performance-matrix">
                <app-performance-heatmap
                  [data]="(performanceMatrix$ | async) ?? []"
                  title="Agent vs Model Performance"
                  [config]="heatmapConfig"
                ></app-performance-heatmap>
            <!-- Right: Analysis Panels with Collapsible Sections -->
              <div class="dashboard-section analysis-panels">
                <!-- Cost Analysis Panel -->
                <div class="analysis-panel">
                  <div class="panel-header" (click)="togglePanel('cost')">
                      Cost Analysis
                    <i class="fa-solid panel-toggle-icon" [class.fa-chevron-down]="!panelStates.cost" [class.fa-chevron-up]="panelStates.cost"></i>
                  @if (panelStates.cost) {
                      @if (costData$ | async; as costData) {
                        <div class="cost-bars">
                          @for (item of costData.slice(0, 8); track item.model) {
                            <div class="cost-bar-item">
                              <div class="cost-bar-info">
                                <span class="model-name">{{ item.model }}</span>
                                <span class="cost-value">{{ formatCurrency(item.cost) }}</span>
                              <div class="cost-bar-container">
                                  class="cost-bar"
                                  [style.width.%]="getCostBarWidth(item.cost, getMaxCost(costData))"
                              <div class="token-info">
                                {{ formatTokens(item.tokens) }} tokens
                <!-- Token Efficiency Panel -->
                  <div class="panel-header" (click)="togglePanel('efficiency')">
                      <i class="fa-solid fa-chart-pie"></i>
                      Token Efficiency
                    <i class="fa-solid panel-toggle-icon" [class.fa-chevron-down]="!panelStates.efficiency" [class.fa-chevron-up]="panelStates.efficiency"></i>
                  @if (panelStates.efficiency) {
                      @if (tokenEfficiency$ | async; as efficiencyData) {
                        <div class="efficiency-items">
                          @for (item of efficiencyData.slice(0, 6); track item.model) {
                            <div class="efficiency-item">
                              <div class="efficiency-header">
                                <span class="efficiency-ratio">
                                  {{ getTokenRatio(item.inputTokens, item.outputTokens) }}
                              <div class="token-breakdown">
                                <div class="token-bar">
                                    class="token-segment token-segment--input"
                                    [style.width.%]="getTokenPercentage(item.inputTokens, item.inputTokens + item.outputTokens)"
                                    class="token-segment token-segment--output"
                                    [style.width.%]="getTokenPercentage(item.outputTokens, item.inputTokens + item.outputTokens)"
                                <div class="token-labels">
                                  <span class="input-label">Input: {{ formatTokens(item.inputTokens) }}</span>
                                  <span class="output-label">Output: {{ formatTokens(item.outputTokens) }}</span>
                              <div class="cost-per-token">
                                {{ formatCostPerToken(item.cost, item.inputTokens + item.outputTokens) }}
                <!-- Live Executions Panel -->
                  <div class="panel-header" (click)="togglePanel('executions')">
                      Live Executions
                    <i class="fa-solid panel-toggle-icon" [class.fa-chevron-down]="!panelStates.executions" [class.fa-chevron-up]="panelStates.executions"></i>
                  @if (panelStates.executions) {
                    <div class="panel-content live-executions-panel">
                      <app-live-execution-widget
                        [executions]="(liveExecutions$ | async) ?? []"
                        (executionClick)="onExecutionClick($event)"
                      ></app-live-execution-widget>
      <!-- Execution Details Modal -->
      @if (selectedExecution) {
        <div class="execution-modal" (click)="closeExecutionModal()">
        <div class="execution-modal-content" (click)="$event.stopPropagation()">
          <div class="execution-modal-header">
            <h3>Execution Details</h3>
            <div class="modal-header-actions">
              <button class="open-record-btn" (click)="openFullRecord()">
              <button class="close-btn" (click)="closeExecutionModal()">
          <div class="execution-modal-body">
            @if (executionDetails) {
              <div class="execution-details">
                <h4>Basic Information</h4>
                    <label>Type:</label>
                    <span>{{ executionDetails.type | titlecase }}</span>
                    <label>Name:</label>
                    <span>{{ executionDetails.name }}</span>
                    <label>Status:</label>
                    <span class="status-badge" [class]="'status-badge--' + executionDetails.status">
                      {{ executionDetails.status | titlecase }}
                    <label>Started:</label>
                    <span>{{ executionDetails.startTime | date:'medium' }}</span>
                  @if (executionDetails.endTime) {
                      <label>Completed:</label>
                      <span>{{ executionDetails.endTime | date:'medium' }}</span>
                    <label>Duration:</label>
                    <span>{{ formatDuration(getDuration(executionDetails)) }}</span>
                <h4>Resource Usage</h4>
                    <label>Cost:</label>
                    <span>{{ formatCurrency(executionDetails.cost, 6) }}</span>
                    <label>Tokens:</label>
                    <span>{{ executionDetails.tokens.toLocaleString() }}</span>
                  @if (executionDetails.model) {
                      <label>Model:</label>
                      <span>{{ executionDetails.model }}</span>
              @if (executionDetails.errorMessage) {
                  <h4>Error Information</h4>
                  <div class="error-message">{{ executionDetails.errorMessage }}</div>
              @if (executionDetails.children.length > 0) {
                  <h4>Child Executions ({{ executionDetails.children.length }})</h4>
                  <div class="child-executions">
                    @for (child of executionDetails.children; track child.id) {
                      <div class="child-execution">
                    <div class="child-info">
                      <span class="child-name">{{ child.name }}</span>
                      <span class="child-type">{{ child.type }}</span>
                      <span class="child-status" [class]="'status-badge--' + child.status">
                        {{ child.status }}
                    <div class="child-metrics">
                        <span>{{ formatCurrency(child.cost) }}</span>
                        <span>{{ child.tokens.toLocaleString() }} tokens</span>
            @if (loadingExecutionDetails) {
              <div class="loading-details">
                <mj-loading text="Loading execution details..." size="medium"></mj-loading>
      background: linear-gradient(135deg, #f5f7fa 0%, #e4e8ed 100%);
    .execution-monitoring.loading {
    /* Loading Overlay */
      background: rgba(255, 255, 255, 0.7);
      z-index: 999;
      backdrop-filter: blur(4px);
    /* === Dashboard Header - Clean White Style === */
    .monitoring-header {
    .monitoring-title {
      color: #1e293b;
    .monitoring-title i {
    .monitoring-controls {
    .time-range-control {
    .time-range-control label {
    .time-range-control select {
    .time-range-control select:hover:not(:disabled) {
    .time-range-control select:focus {
      border-color: #6366f1;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    .time-range-control select:disabled {
    .time-range-control select option {
    .refresh-btn {
      background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%);
      box-shadow: 0 2px 8px rgba(99, 102, 241, 0.25);
    .refresh-btn:hover:not(:disabled) {
      background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%);
      box-shadow: 0 4px 12px rgba(99, 102, 241, 0.35);
    .refresh-btn:disabled {
    .refresh-btn i.spinning {
    .kpi-dashboard {
    .kpi-grid {
    .dashboard-splitter {
      margin: 0 20px 20px 20px;
    .dashboard-section {
      box-shadow: 0 4px 16px rgba(99, 102, 241, 0.08);
      border: 1px solid rgba(99, 102, 241, 0.08);
      transition: box-shadow 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    .dashboard-section:hover {
      box-shadow: 0 8px 24px rgba(99, 102, 241, 0.12);
    /* Ensure splitter panes take full height */
    :host ::ng-deep .k-splitter .k-splitter-pane {
    :host ::ng-deep .k-splitter-horizontal > .k-splitter-pane {
      padding: 10px 5px;
    :host ::ng-deep .k-splitter-vertical > .k-splitter-pane {
      padding: 5px 10px;
    /* Cost Analysis Styles */
    .cost-chart-container, .efficiency-chart-container, .status-container {
    .chart-title {
    .chart-title i {
    .cost-bars, .efficiency-items {
    .cost-bar-item {
    .cost-bar-item:last-child {
    .cost-bar-info {
    .model-name {
    .cost-value {
    .cost-bar-container {
      background: linear-gradient(90deg, rgba(99, 102, 241, 0.1) 0%, rgba(139, 92, 246, 0.1) 100%);
    .cost-bar {
      background: linear-gradient(90deg, #6366f1 0%, #8b5cf6 50%, #a78bfa 100%);
      transition: width 0.4s cubic-bezier(0.4, 0, 0.2, 1);
    .token-info {
    /* Token Efficiency Styles */
    .efficiency-item {
    .efficiency-item:last-child {
    .efficiency-header {
    .efficiency-ratio {
    .token-breakdown {
    .token-bar {
    .token-segment {
    .token-segment--input {
      background: linear-gradient(90deg, #6366f1 0%, #818cf8 100%);
    .token-segment--output {
      background: linear-gradient(90deg, #8b5cf6 0%, #a78bfa 100%);
    .token-labels {
    .input-label {
    .output-label {
    .cost-per-token {
    /* System Status Styles */
    .status-metrics {
    .status-metric {
    .status-icon {
    .status-icon--success {
      background: linear-gradient(135deg, rgba(16, 185, 129, 0.15) 0%, rgba(5, 150, 105, 0.15) 100%);
    .status-icon--warning {
      background: linear-gradient(135deg, rgba(245, 158, 11, 0.15) 0%, rgba(217, 119, 6, 0.15) 100%);
    .status-icon--info {
      background: linear-gradient(135deg, rgba(99, 102, 241, 0.15) 0%, rgba(139, 92, 246, 0.15) 100%);
    .status-icon--primary {
      background: linear-gradient(135deg, rgba(139, 92, 246, 0.15) 0%, rgba(167, 139, 250, 0.15) 100%);
    .status-info {
    .status-value {
      margin: 2px 0;
    .status-subtitle {
    /* Execution Modal Styles */
    .execution-modal {
    .execution-modal-content {
      max-height: 80vh;
    .execution-modal-header {
    .execution-modal-header h3 {
    .modal-header-actions {
    .open-record-btn {
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    .open-record-btn:hover {
    .open-record-btn i {
    .execution-modal-body {
      padding-bottom: 6px;
    .status-badge--completed {
    .status-badge--running {
    .status-badge--failed {
      background: linear-gradient(135deg, rgba(239, 68, 68, 0.15) 0%, rgba(220, 38, 38, 0.15) 100%);
      border: 1px solid #ffcc02;
      color: #e65100;
    .child-executions {
    .child-execution {
    .child-info {
    .child-name {
    .child-type {
      background: #e0e0e0;
    .child-status {
    .child-metrics {
    .loading-details {
    /* Drill-down Tab Styles */
    .drill-down-container {
    .drill-down-tabs {
    .tab-header {
      border-bottom: 1px solid rgba(99, 102, 241, 0.1);
      background: linear-gradient(180deg, #f8f9ff 0%, #f3f4f6 100%);
    .tab-item {
      padding: 10px 18px;
    .tab-item:hover {
      background: rgba(99, 102, 241, 0.05);
    .tab-item.active {
      border-bottom-color: #6366f1;
    .tab-title {
    .tab-close {
    .tab-close:hover {
      background: rgba(0, 0, 0, 0.1);
    .tab-pane {
    .trends-chart {
    .trends-chart app-time-series-chart {
    /* Ensure chart fits within tab pane */
    .tab-pane.trends-chart {
    /* Drill-down specific styles */
    .executions-drill-down {
    .drill-down-header {
    .drill-down-header h4 {
    .drill-down-meta {
    .metric-badge {
      box-shadow: 0 2px 4px rgba(99, 102, 241, 0.25);
    .table-header {
      grid-template-columns: 80px 1fr 120px 100px 100px 80px 100px 120px;
    .table-row {
    .table-row:hover {
    .table-cell {
      background: linear-gradient(135deg, rgba(100, 116, 139, 0.1) 0%, rgba(71, 85, 105, 0.1) 100%);
    .type-badge--prompt {
    .type-badge--agent {
      color: #ddd;
    /* Model detail styles */
    .model-detail {
    .model-details {
    .model-info-grid {
    .info-item label {
    .info-item span {
    .status-indicator.active {
      color: #4caf50;
    .status-indicator.active::before {
      background: #4caf50;
    .model-description h5 {
    .model-description p {
    /* Clickable KPI cards */
      transition: transform 0.2s ease, box-shadow 0.2s ease;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    .analysis-panels {
    .analysis-panel {
      box-shadow: 0 2px 8px rgba(99, 102, 241, 0.08);
    .analysis-panel:hover {
      box-shadow: 0 4px 12px rgba(99, 102, 241, 0.12);
    .analysis-panel:last-child {
      background: linear-gradient(180deg, #fafbff 0%, #f8f9fc 100%);
      border-bottom: 1px solid rgba(99, 102, 241, 0.08);
    .panel-header:hover {
      background: linear-gradient(180deg, #f0f1ff 0%, #e8e9ff 100%);
    .panel-toggle-icon {
      transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      border-top: 1px solid rgba(99, 102, 241, 0.05);
      animation: slideDown 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    .live-executions-panel {
    .live-executions-panel app-live-execution-widget {
      .table-header,
        grid-template-columns: 60px 1fr 100px 80px 80px 60px 80px 100px;
        min-height: 350px;
      /* Reduce padding on smaller screens */
        padding: 5px 2px;
        padding: 2px 5px;
      .table-cell:before {
        content: attr(data-label) ': ';
      .executions-drill-down,
export class ExecutionMonitoringComponent extends BaseResourceComponent implements OnInit, OnDestroy {
  private stateChangeSubject$ = new Subject<ExecutionMonitoringState>();
  selectedTimeRange = '24h';
  isLoading = false;
  timeSeriesConfig = {
    showGrid: true,
    showTooltip: true,
    animationDuration: 500,
    useDualAxis: true
  heatmapConfig = {
    animationDuration: 300
  kpis$: Observable<DashboardKPIs>;
  trends$: Observable<TrendData[]>;
  liveExecutions$: Observable<LiveExecution[]>;
  chartData$: Observable<ChartData>;
  // Derived data streams
  kpiCards$: Observable<KPICardData[]>;
  performanceMatrix$: Observable<HeatmapData[]>;
  costData$: Observable<{ model: string; cost: number; tokens: number }[]>;
  tokenEfficiency$: Observable<{ inputTokens: number; outputTokens: number; cost: number; model: string }[]>;
  // Modal state
  selectedExecution: LiveExecution | null = null;
  executionDetails: ExecutionDetails | null = null;
  loadingExecutionDetails = false;
  // Drill-down tab state
  drillDownTabs: DrillDownTab[] = [];
  activeTabId: string = 'main-chart';
  loadingDrillDown = false;
  // Panel state for collapsible sections
    cost: true,
    efficiency: true,  // Expanded by default
    executions: false
  get activeTab(): DrillDownTab | undefined {
    return this.drillDownTabs.find(tab => tab.id === this.activeTabId);
    private instrumentationService: AIInstrumentationService,
    // Initialize data streams
    this.kpis$ = this.instrumentationService.kpis$;
    this.trends$ = this.instrumentationService.trends$;
    this.liveExecutions$ = this.instrumentationService.liveExecutions$;
    this.chartData$ = this.instrumentationService.chartData$;
    // Subscribe to loading state from service
    this.instrumentationService.isLoading$.pipe(
    ).subscribe(loading => {
      this.isLoading = loading;
    // Derived streams
    this.kpiCards$ = this.kpis$.pipe(
      map(kpis => this.createKPICards(kpis))
    this.performanceMatrix$ = this.chartData$.pipe(
      map(data => data.performanceMatrix.map(item => ({
        agent: item.agent,
        model: item.model,
        avgTime: item.avgTime,
        successRate: item.successRate
      })))
    this.costData$ = this.chartData$.pipe(
      map(data => data.costByModel)
    this.tokenEfficiency$ = this.chartData$.pipe(
      map(data => data.tokenEfficiency)
    // Load initial state if provided from resource configuration
    if (this.Data?.Configuration) {
      this.loadUserState(this.Data.Configuration);
      // Default initialization
      this.setTimeRange(this.selectedTimeRange);
      // Initialize with main chart tab
      this.drillDownTabs = [
          id: 'main-chart',
          title: 'Execution Trends',
          type: 'chart',
          closeable: false
      // Trigger initial data load
      this.instrumentationService.refresh();
    // Set up debounced state change - could be used for persistence in the future
    this.stateChangeSubject$.pipe(
      debounceTime(2000), // 2 second debounce
    ).subscribe(_state => {
      // State change handling placeholder
    // Notify that the resource has finished loading
    this.NotifyLoadComplete();
    this.stateChangeSubject$.complete();
  private getCurrentState(): ExecutionMonitoringState {
      selectedTimeRange: this.selectedTimeRange,
      refreshInterval: 0, // Always manual refresh now
      panelStates: { ...this.panelStates },
      drillDownTabs: this.drillDownTabs.map(tab => ({
        id: tab.id,
        title: tab.title,
        type: tab.type,
        timestamp: tab.timestamp?.toISOString(),
        metric: tab.metric
      activeTabId: this.activeTabId
  private emitStateChange(): void {
    const currentState = this.getCurrentState();
    this.stateChangeSubject$.next(currentState);
  public loadUserState(state: Partial<ExecutionMonitoringState>): void {
    if (state.selectedTimeRange) {
      this.selectedTimeRange = state.selectedTimeRange;
      this.setTimeRange(state.selectedTimeRange);
    // No longer need to handle refreshInterval since we removed auto-refresh
    if (state.panelStates) {
      // Only override if state has explicit panel states, otherwise keep defaults
      this.panelStates = { ...this.panelStates, ...state.panelStates };
    if (state.drillDownTabs && state.drillDownTabs.length > 0) {
      this.drillDownTabs = state.drillDownTabs.map(tab => ({
        type: tab.type as 'chart' | 'executions' | 'model-detail',
        timestamp: tab.timestamp ? new Date(tab.timestamp) : undefined,
        closeable: tab.id !== 'main-chart'
      // Initialize with default tab if not provided
    if (state.activeTabId) {
      this.activeTabId = state.activeTabId;
  private createKPICards(kpis: DashboardKPIs): KPICardData[] {
        title: 'Total Executions',
        value: kpis.totalExecutions,
        icon: 'fa-chart-bar',
        color: 'primary',
        subtitle: `${kpis.activeExecutions} active`
        title: 'Total Cost',
        value: `$${kpis.totalCost.toFixed(4)}`,
        icon: 'fa-dollar-sign',
        color: 'warning',
        subtitle: `${kpis.costCurrency} • $${kpis.dailyCostBurn.toFixed(2)}/day`
        title: 'Success Rate',
        value: `${(kpis.successRate * 100).toFixed(1)}%`,
        icon: 'fa-check-circle',
        color: 'success',
        subtitle: `${(kpis.errorRate * 100).toFixed(1)}% errors`
        title: 'Avg Response Time',
        value: `${(kpis.avgExecutionTime / 1000).toFixed(2)}s`,
        icon: 'fa-clock',
        color: 'info',
        subtitle: 'All models average'
        title: 'Token Usage',
        value: this.formatTokens(kpis.totalTokens),
        icon: 'fa-coins',
        subtitle: `$${kpis.costPerToken.toFixed(6)}/token`
        title: 'Top Model',
        value: kpis.topModel,
        subtitle: 'Most used'
  onTimeRangeChange(): void {
    // Simply change time range - loading state is managed by the service
    this.emitStateChange();
  private setTimeRange(range: string): void {
    const { start, end } = this.getTimeRangeFromSelection(range);
    this.instrumentationService.setDateRange(start, end);
  private getTimeRangeFromSelection(range?: string): { start: Date; end: Date } {
    const selectedRange = range || this.selectedTimeRange;
    switch (selectedRange) {
      case '1h':
        start = new Date(now.getTime() - 60 * 60 * 1000);
      case '6h':
        start = new Date(now.getTime() - 6 * 60 * 60 * 1000);
      case '24h':
        start = new Date(now.getTime() - 24 * 60 * 60 * 1000);
        start = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
        start = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
    return { start, end: now };
  refreshData(): void {
    // Simply trigger refresh - loading state is managed by the service
  onExecutionClick(execution: LiveExecution): void {
    this.selectedExecution = execution;
    this.loadExecutionDetails(execution);
  onDataPointClick(event: DataPointClickEvent): void {
    const timestamp = event.data.timestamp;
    const metric = event.metric;
    // Create new drill-down tab
    const tabId = `drill-down-${timestamp.getTime()}-${metric}`;
    const tabTitle = `${this.getMetricDisplayLabel(metric)} - ${this.formatTimestamp(timestamp)}`;
    const newTab: DrillDownTab = {
      id: tabId,
      title: tabTitle,
      type: 'executions',
      timestamp: timestamp,
      metric: metric,
      closeable: true
    // Add tab if it doesn't exist
    if (!this.drillDownTabs.find(tab => tab.id === tabId)) {
      this.drillDownTabs.push(newTab);
      this.emitStateChange(); // Emit state when new tab is added
    // Switch to the new tab
    this.selectTab(tabId);
    // Load drill-down data
    this.loadDrillDownData(newTab);
  onChartTimeRangeChange(range: string): void {
    this.selectedTimeRange = range;
    this.setTimeRange(range);
  private getMetricValue(data: TrendData, metric: string): number {
      case 'executions': return data.executions;
      case 'cost': return data.cost;
      case 'tokens': return data.tokens;
      case 'avgTime': return data.avgTime;
      case 'errors': return data.errors;
  private formatMetricValue(metric: string, value: number): string {
      case 'executions': return value.toLocaleString();
      case 'cost': return `$${value.toFixed(4)}`;
      case 'tokens': return value.toLocaleString();
      case 'avgTime': return `${(value / 1000).toFixed(1)}s`;
      case 'errors': return value.toString();
      default: return value.toString();
  private async loadExecutionDetails(execution: LiveExecution): Promise<void> {
    this.loadingExecutionDetails = true;
    this.executionDetails = null;
      const details = await this.instrumentationService.getExecutionDetails(
        execution.id,
        execution.type
      this.executionDetails = details;
      console.error('Error loading execution details:', error);
      this.loadingExecutionDetails = false;
  closeExecutionModal(): void {
    this.selectedExecution = null;
  openFullRecord(): void {
    if (this.selectedExecution) {
      // Determine the entity name based on the execution type
      const entityName = this.selectedExecution.type === 'prompt'
        ? 'MJ: AI Prompt Runs'
        : 'MJ: AI Agent Runs';
      // Open the record using NavigationService
      const compositeKey = new CompositeKey([{ FieldName: 'ID', Value: this.selectedExecution.id }]);
      this.navigationService.OpenEntityRecord(entityName, compositeKey);
      // Close the modal
      this.closeExecutionModal();
  // Utility methods for templates
  trackByKpiTitle(index: number, kpi: KPICardData): string {
    return kpi.title;
  trackByCostModel(index: number, item: { model: string; cost: number; tokens: number }): string {
    return item.model;
  trackByEfficiencyModel(index: number, item: { model: string; inputTokens: number; outputTokens: number; cost: number }): string {
  formatTokens(tokens: number): string {
  formatCurrency(amount: number, decimals: number = 4): string {
    return `$${amount.toFixed(decimals)}`;
  formatCostPerToken(cost: number, tokens: number): string {
    const costPer1K = tokens > 0 ? (cost / tokens) * 1000 : 0;
    return `$${costPer1K.toFixed(4)}/1K tokens`;
  getCostBarWidth(cost: number, maxCost: number): number {
    return maxCost > 0 ? (cost / maxCost) * 100 : 0;
  getMaxCost(costData: { cost: number }[]): number {
    return Math.max(...costData.map(item => item.cost));
  getTokenRatio(input: number, output: number): string {
    const total = input + output;
    if (total === 0) return '0:0';
    const ratio = output / input;
    return `1:${ratio.toFixed(1)}`;
  getTokenPercentage(tokens: number, total: number): number {
    return total > 0 ? (tokens / total) * 100 : 0;
  getCostPerToken(cost: number, tokens: number): string {
    return costPer1K.toFixed(4);
  // Tab management methods
  selectTab(tabId: string): void {
    this.activeTabId = tabId;
    // Trigger chart resize after tab switch to fix chart rendering
      window.dispatchEvent(new Event('resize'));
  closeTab(event: MouseEvent, tabId: string): void {
    const tabIndex = this.drillDownTabs.findIndex(tab => tab.id === tabId);
    // Remove the tab
    this.drillDownTabs.splice(tabIndex, 1);
    // If we closed the active tab, switch to another tab
    if (this.activeTabId === tabId) {
      if (this.drillDownTabs.length > 0) {
        // Switch to the previous tab or first tab
        const newActiveIndex = Math.max(0, tabIndex - 1);
        this.activeTabId = this.drillDownTabs[newActiveIndex].id;
        // Trigger resize after tab switch
        // No tabs left, this shouldn't happen as main chart is not closeable
        this.activeTabId = 'main-chart';
  // KPI click handling
  onKpiClick(kpi: KPICardData): void {
    if (kpi.title === 'Top Model' && kpi.value !== 'N/A') {
      this.openModelDrillDown(String(kpi.value));
    // Add other KPI drill-downs as needed
  isKpiClickable(kpi: KPICardData): boolean {
    return kpi.title === 'Top Model' && kpi.value !== 'N/A';
  private async openModelDrillDown(modelName: string): Promise<void> {
    const tabId = `model-detail-${modelName.replace(/[^a-zA-Z0-9]/g, '-')}`;
    const tabTitle = `Model: ${modelName}`;
    // Check if tab already exists
    if (this.drillDownTabs.find(tab => tab.id === tabId)) {
    // Create new model detail tab
      type: 'model-detail',
    // Load model details
    this.loadModelDetails(newTab, modelName);
  private async loadDrillDownData(tab: DrillDownTab): Promise<void> {
    if (!tab.timestamp) return;
    this.loadingDrillDown = true;
      // Determine bucket size based on selected time range
      const { start, end } = this.getTimeRangeFromSelection();
      const duration = end.getTime() - start.getTime();
      const hours = duration / (1000 * 60 * 60);
      let windowSizeMs: number;
      let alignToDay = false;
      if (hours <= 24) {
        // For up to 24 hours, use 1 hour window (30 min before and after)
        windowSizeMs = 30 * 60 * 1000;
      } else if (hours <= 24 * 7) {
        // For up to 7 days, use full day window
        // Since data is aggregated into 4-hour buckets, we need to capture the full day
        windowSizeMs = 12 * 60 * 60 * 1000; // 12 hours before/after = 24 hour window
        alignToDay = true;
        // For more than 7 days, use full day window
      // Create time window around the clicked point
      let startTime = new Date(tab.timestamp.getTime() - windowSizeMs);
      let endTime = new Date(tab.timestamp.getTime() + windowSizeMs);
      // For day-aligned queries, expand to full day boundaries
      if (alignToDay) {
        // Set start to beginning of the day
        startTime = new Date(tab.timestamp);
        startTime.setHours(0, 0, 0, 0);
        // Set end to end of the day
        endTime = new Date(tab.timestamp);
        endTime.setHours(23, 59, 59, 999);
      // Load executions for this time period
      const [promptResults, agentResults] = await Promise.all([
        new RunView().RunView<AIPromptRunEntityExtended>({
          ExtraFilter: `RunAt >= '${startTime.toISOString()}' AND RunAt <= '${endTime.toISOString()}'`,
        new RunView().RunView<AIAgentRunEntityExtended>({
          ExtraFilter: `StartedAt >= '${startTime.toISOString()}' AND StartedAt <= '${endTime.toISOString()}'`,
      // Convert to ExecutionRecord format
      const executions: ExecutionRecord[] = [];
      // Add prompt executions
      for (const run of promptResults.Results) {
        const duration = run.CompletedAt ? 
          new Date(run.CompletedAt).getTime() - new Date(run.RunAt).getTime() : 
          Date.now() - new Date(run.RunAt).getTime();
        executions.push({
          type: 'prompt',
          name: run.Prompt || 'Unnamed Prompt',
          model: run.Model || undefined,
          status: run.Success ? 'completed' : (run.Success === false ? 'failed' : 'running'),
          startTime: new Date(run.RunAt),
          endTime: run.CompletedAt ? new Date(run.CompletedAt) : undefined,
          cost: run.Cost || 0,
          tokens: run.TokensUsed || 0,
          errorMessage: run.ErrorMessage || undefined
      // Add agent executions
      for (const run of agentResults.Results) {
          new Date(run.CompletedAt).getTime() - new Date(run.StartedAt).getTime() : 
          Date.now() - new Date(run.StartedAt).getTime();
          type: 'agent',
          name: run.Agent || 'Unnamed Agent',
          status: run.Status.toLowerCase(),
          startTime: new Date(run.StartedAt),
          cost: run.TotalCost || 0,
          tokens: run.TotalTokensUsed || 0,
      // Sort by start time (most recent first)
      executions.sort((a, b) => b.startTime.getTime() - a.startTime.getTime());
      // Update tab data
      tab.data = executions;
      console.error('Error loading drill-down data:', error);
      tab.data = [];
      this.loadingDrillDown = false;
  private async loadModelDetails(tab: DrillDownTab, modelName: string): Promise<void> {
      // Find model by name
      const result = await rv.RunView<AIModelEntityExtended>({
        ExtraFilter: `Name = '${modelName.replace(/'/g, "''")}'` 
      const model = result.Results[0];
        tab.data = {
          inputTokenCost: 0, // Not available in current model
          outputTokenCost: 0, // Not available in current model  
          isActive: model.IsActive,
        tab.data = null;
  // Helper methods for drill-down
  formatTimestamp(timestamp: Date): string {
    return timestamp.toLocaleString();
  formatTime(time: Date): string {
    return time.toLocaleTimeString();
  getMetricDisplayLabel(metric: string): string {
    const labels: { [key: string]: string } = {
      executions: 'Executions',
      cost: 'Cost',
      tokens: 'Tokens',
      avgTime: 'Avg Time',
      errors: 'Errors'
    return labels[metric] || metric;
  getFormattedTimestamp(tab: DrillDownTab | undefined): string {
    return tab?.timestamp ? this.formatTimestamp(tab.timestamp) : '';
  getFormattedMetricLabel(tab: DrillDownTab | undefined): string {
    return tab?.metric ? this.getMetricDisplayLabel(tab.metric) : '';
  // Panel management methods
  togglePanel(panelName: 'cost' | 'efficiency' | 'executions'): void {
    this.panelStates[panelName] = !this.panelStates[panelName];
  viewExecutionDetail(execution: ExecutionRecord): void {
    // Convert ExecutionRecord to LiveExecution format for the modal
    const liveExecution: LiveExecution = {
      id: execution.id,
      type: execution.type,
      name: execution.name,
      status: execution.status as 'running' | 'completed' | 'failed',
      startTime: execution.startTime,
      duration: execution.duration,
      cost: execution.cost,
      tokens: execution.tokens
    this.onExecutionClick(liveExecution);
    const seconds = Math.floor(milliseconds / 1000);
    const minutes = Math.floor(seconds / 60);
    const hours = Math.floor(minutes / 60);
      return `${hours}h ${minutes % 60}m ${seconds % 60}s`;
      return `${minutes}m ${seconds % 60}s`;
  getDuration(details: ExecutionDetails): number {
    const start = details.startTime.getTime();
    const end = details.endTime ? details.endTime.getTime() : Date.now();
    return end - start;
  onSplitterLayoutChange(event: any): void {
    // Trigger window resize event to force charts to recalculate dimensions
    // Emit state change when splitter changes
  // === BaseResourceComponent Required Methods ===
   * Get the display name for this resource
  async GetResourceDisplayName(data: ResourceData): Promise<string> {
    return 'Monitor';
   * Get the icon class for this resource
  async GetResourceIconClass(data: ResourceData): Promise<string> {
    return 'fa-solid fa-chart-line';
import { MJActionExecutionLogEntity, MJActionEntity, ResourceData } from '@memberjunction/core-entities';
interface ExecutionMetrics {
  successfulExecutions: number;
  failedExecutions: number;
  averageDuration: number;
  executionsToday: number;
  executionsThisWeek: number;
  currentlyRunning: number;
interface ExecutionTrend {
  date: string;
  successful: number;
  failed: number;
 * Execution Monitoring Resource - displays action execution logs and metrics
@RegisterClass(BaseResourceComponent, 'ActionsMonitorResource')
  selector: 'mj-execution-monitoring',
  templateUrl: './execution-monitoring.component.html',
  styleUrls: ['./execution-monitoring.component.css']
  public executions: MJActionExecutionLogEntity[] = [];
  public filteredExecutions: MJActionExecutionLogEntity[] = [];
  public actions: Map<string, MJActionEntity> = new Map();
  public metrics: ExecutionMetrics = {
    successfulExecutions: 0,
    failedExecutions: 0,
    averageDuration: 0,
    executionsToday: 0,
    executionsThisWeek: 0,
    currentlyRunning: 0
  public executionTrends: ExecutionTrend[] = [];
  public selectedResult$ = new BehaviorSubject<string>('all');
  public selectedTimeRange$ = new BehaviorSubject<string>('7days');
  public selectedAction$ = new BehaviorSubject<string>('all');
  public timeRangeOptions = [
    { text: 'Last 24 Hours', value: '24hours' },
    { text: 'Last 7 Days', value: '7days' },
    { text: 'Last 30 Days', value: '30days' },
    { text: 'Last 90 Days', value: '90days' }
  public resultOptions = [
    { text: 'All Results', value: 'all' },
    { text: 'Success', value: 'Success' },
    { text: 'Failed', value: 'Failed' },
    { text: 'Error', value: 'Error' },
    { text: 'Running', value: 'Running' }
  public actionOptions: Array<{text: string; value: string}> = [
    { text: 'All Actions', value: 'all' }
  constructor(private navigationService: NavigationService, private cdr: ChangeDetectorRef) {
      this.selectedResult$.pipe(distinctUntilChanged()),
      this.selectedTimeRange$.pipe(distinctUntilChanged()),
      this.selectedAction$.pipe(distinctUntilChanged())
      const [executionsResult, actionsResult] = await rv.RunViews([
      if (!executionsResult.Success || !actionsResult.Success) {
        throw new Error('Failed to load data');
      const executions = executionsResult.Results as MJActionExecutionLogEntity[];
      const actions = actionsResult.Results as MJActionEntity[];
      this.executions = executions;
      this.populateActionsMap(actions);
      this.buildActionOptions(actions);
      this.calculateMetrics();
      this.generateExecutionTrends();
      LogError('Failed to load execution monitoring data', undefined, error);
  private populateActionsMap(actions: MJActionEntity[]): void {
    this.actions.clear();
      this.actions.set(action.ID, action);
  private buildActionOptions(actions: MJActionEntity[]): void {
    this.actionOptions = [
      { text: 'All Actions', value: 'all' },
      ...actions.map(action => ({
        text: action.Name,
        value: action.ID
  private calculateMetrics(): void {
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const weekAgo = new Date(today.getTime() - 7 * 24 * 60 * 60 * 1000);
      totalExecutions: this.executions.length,
      successfulExecutions: this.executions.filter(e => e.ResultCode === 'Success').length,
      failedExecutions: this.executions.filter(e => 
        e.ResultCode && ['Failed', 'Error'].includes(e.ResultCode)
      averageDuration: this.calculateAverageDuration(),
      executionsToday: this.executions.filter(e => 
        new Date(e.StartedAt!) >= today
      executionsThisWeek: this.executions.filter(e => 
        new Date(e.StartedAt!) >= weekAgo
      currentlyRunning: this.executions.filter(e => 
        e.ResultCode === 'Running' || !e.EndedAt
  private calculateAverageDuration(): number {
    const completedExecutions = this.executions.filter(e => 
      e.StartedAt && e.EndedAt
    if (completedExecutions.length === 0) return 0;
    const totalDuration = completedExecutions.reduce((sum, execution) => {
      const start = new Date(execution.StartedAt!).getTime();
      const end = new Date(execution.EndedAt!).getTime();
      return sum + (end - start);
    return Math.round(totalDuration / completedExecutions.length / 1000); // Average in seconds
  private generateExecutionTrends(): void {
    const trends = new Map<string, ExecutionTrend>();
    const last7Days = Array.from({ length: 7 }, (_, i) => {
      date.setDate(date.getDate() - i);
    }).reverse();
    // Initialize trends for last 7 days
    last7Days.forEach(date => {
      trends.set(date, {
        successful: 0,
        failed: 0,
        total: 0
    // Populate trends with execution data
    this.executions.forEach(execution => {
      if (!execution.StartedAt) return;
      const date = new Date(execution.StartedAt).toISOString().split('T')[0];
      const trend = trends.get(date);
      if (trend) {
        trend.total++;
        if (execution.ResultCode === 'Success') {
          trend.successful++;
        } else if (execution.ResultCode && ['Failed', 'Error'].includes(execution.ResultCode)) {
          trend.failed++;
    this.executionTrends = Array.from(trends.values());
    let filtered = [...this.executions];
    const timeRange = this.selectedTimeRange$.value;
    if (timeRange !== 'all') {
      const cutoffDate = this.getTimeRangeCutoff(timeRange);
      filtered = filtered.filter(e => 
        e.StartedAt && new Date(e.StartedAt) >= cutoffDate
    // Apply result filter
    const result = this.selectedResult$.value;
    if (result !== 'all') {
      if (result === 'Running') {
        filtered = filtered.filter(e => !e.EndedAt || e.ResultCode === 'Running');
        filtered = filtered.filter(e => e.ResultCode === result);
    // Apply action filter
    const actionId = this.selectedAction$.value;
    if (actionId !== 'all') {
      filtered = filtered.filter(e => e.ActionID === actionId);
      filtered = filtered.filter(e => {
        const action = this.actions.get(e.ActionID!);
          action?.Name.toLowerCase().includes(searchTerm) ||
          e.ResultCode?.toLowerCase().includes(searchTerm) ||
          e.UserID?.toLowerCase().includes(searchTerm)
    this.filteredExecutions = filtered;
  private getTimeRangeCutoff(timeRange: string): Date {
      case '24hours':
        return new Date(now.getTime() - 24 * 60 * 60 * 1000);
      case '7days':
        return new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
      case '30days':
        return new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
      case '90days':
        return new Date(now.getTime() - 90 * 24 * 60 * 60 * 1000);
        return new Date(0);
  public onResultFilterChange(result: string): void {
    this.selectedResult$.next(result);
  public onTimeRangeChange(timeRange: string): void {
    this.selectedTimeRange$.next(timeRange);
  public onActionFilterChange(actionId: string): void {
    this.selectedAction$.next(actionId);
  public openAction(actionId: string): void {
    const key = new CompositeKey([{ FieldName: 'ID', Value: actionId }]);
  public getActionName(actionId: string): string {
    return this.actions.get(actionId)?.Name || `Action ${actionId}`;
  public getResultColor(resultCode: string | null): 'success' | 'warning' | 'error' | 'info' {
    if (!resultCode) return 'info';
    switch (resultCode.toLowerCase()) {
      case 'success': return 'success';
      case 'error': return 'error';
      case 'running': return 'warning';
  public getResultIcon(resultCode: string | null): string {
    if (!resultCode) return 'fa-solid fa-question';
      case 'success': return 'fa-solid fa-check-circle';
      case 'error': return 'fa-solid fa-exclamation-circle';
      case 'running': return 'fa-solid fa-spinner fa-spin';
      default: return 'fa-solid fa-info-circle';
  public getDuration(execution: MJActionExecutionLogEntity): string {
    if (!execution.StartedAt || !execution.EndedAt) {
      return execution.EndedAt ? 'Unknown' : 'Running';
    const start = new Date(execution.StartedAt).getTime();
    const end = new Date(execution.EndedAt).getTime();
    const duration = Math.round((end - start) / 1000);
    if (duration < 60) return `${duration}s`;
    if (duration < 3600) return `${Math.round(duration / 60)}m`;
    return `${Math.round(duration / 3600)}h`;
  public getSuccessRate(): number {
    if (this.metrics.totalExecutions === 0) return 0;
    return Math.round((this.metrics.successfulExecutions / this.metrics.totalExecutions) * 100);
  public refreshData(): void {
  // Metric card click handlers
  public onTotalExecutionsClick(): void {
    // Reset filters to show all executions
    this.selectedResult$.next('all');
    this.selectedTimeRange$.next('7days');
    this.selectedAction$.next('all');
  public onSuccessRateClick(): void {
    this.selectedResult$.next('Success');
  public onFailedExecutionsClick(): void {
    this.selectedResult$.next('Failed');
  public onRunningExecutionsClick(): void {
    this.selectedResult$.next('Running');
    return 'Execution Monitoring';
