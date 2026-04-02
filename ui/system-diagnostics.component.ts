import { Component, OnInit, OnDestroy, ChangeDetectorRef, ChangeDetectionStrategy, ElementRef, ViewChild, AfterViewInit, NgZone } from '@angular/core';
import { CompositeKey, TelemetryManager, TelemetryEvent, TelemetryPattern, TelemetryInsight, TelemetryCategory, TelemetryParamsUnion, isSingleRunViewParams, isSingleRunQueryParams, isBatchRunViewParams } from '@memberjunction/core';
import { BaseEngineRegistry, EngineMemoryStats, LocalCacheManager, CacheEntryInfo, CacheStats, CacheEntryType, Metadata } from '@memberjunction/core';
 * Interface representing a single engine's diagnostic info for display
export interface EngineDiagnosticInfo {
    className: string;
    registeredAt: Date;
    lastLoadedAt: Date | null;
    estimatedMemoryBytes: number;
    memoryDisplay: string;
 * Interface for engine config items for drill-down detail
export interface EngineConfigItemDisplay {
    type: 'entity' | 'dataset';
    datasetName?: string;
    sampleData: unknown[];
    // Paging support
    displayedData: unknown[];
    allDataLoaded: boolean;
    isLoadingMore: boolean;
    currentPage: number;
    pageSize: number;
 * Engine detail panel state
export interface EngineDetailPanelState {
    isOpen: boolean;
    engine: EngineDiagnosticInfo | null;
    configItems: EngineConfigItemDisplay[];
    isRefreshing: boolean;
 * Interface for redundant entity loading info
export interface RedundantLoadInfo {
    engines: string[];
 * Display-friendly telemetry pattern info
export interface TelemetryPatternDisplay {
    fingerprint: string;
    category: TelemetryCategory;
    entityName: string | null;
    filter: string | null;
    orderBy: string | null;
    avgElapsedMs: number;
    totalElapsedMs: number;
    minElapsedMs: number;
    maxElapsedMs: number;
    lastSeen: Date;
    sampleParams: TelemetryParamsUnion;
 * Display-friendly telemetry event for timeline
export interface TelemetryEventDisplay {
    startTime: number;
    endTime: number | undefined;
    elapsedMs: number | undefined;
    params: TelemetryParamsUnion;
 * Extended insight with expansion state
export interface TelemetryInsightDisplay extends TelemetryInsight {
    relatedEvents: TelemetryEventDisplay[];
 * Sort configuration for patterns table
export interface PatternSortConfig {
    column: 'category' | 'operation' | 'entity' | 'count' | 'avgMs' | 'totalMs';
    direction: 'asc' | 'desc';
 * Event detail panel state
export interface EventDetailPanelState {
    event: TelemetryEventDisplay | null;
    relatedPattern: TelemetryPatternDisplay | null;
 * Summary stats for telemetry
export interface TelemetrySummary {
    totalPatterns: number;
    totalInsights: number;
    activeEvents: number;
    byCategory: Record<TelemetryCategory, { events: number; avgMs: number }>;
 * Settings key for persisting user preferences
const SYSTEM_DIAGNOSTICS_SETTINGS_KEY = 'SystemDiagnostics.UserPreferences';
 * Interface for persisted user preferences
export interface SystemDiagnosticsUserPreferences {
    kpiCardsCollapsed: boolean;
    activeSection: 'engines' | 'redundant' | 'performance' | 'cache';
    perfTab: 'monitor' | 'overview' | 'events' | 'patterns' | 'insights';
    telemetrySource: 'client' | 'server';
    categoryFilter: 'all' | TelemetryCategory;
    chartZoomLevel: number;
    chartGapCompression: boolean;
    autoRefresh: boolean;
 * System Diagnostics Resource Component
 * Provides a comprehensive view of:
 * - All registered BaseEngine instances and their memory usage
 * - Entity load tracking across engines (identifies redundant loading)
 * This is a client-side only dashboard - all data comes from in-memory registries.
@RegisterClass(BaseResourceComponent, 'SystemDiagnosticsResource')
    selector: 'app-system-diagnostics',
        <div class="system-diagnostics">
          <div class="diagnostics-header">
              <i class="fa-solid fa-stethoscope"></i>
              <h2>System Diagnostics</h2>
              <div class="auto-refresh-control">
                  <input type="checkbox" [(ngModel)]="autoRefresh" (change)="toggleAutoRefresh()">
                  Auto-refresh
                @if (autoRefresh) {
                  <span class="refresh-indicator">
                    <i class="fa-solid fa-sync-alt spinning"></i>
                    Every 5s
                Refresh Now
          <!-- Overview Cards (Collapsible) -->
          <div class="overview-cards-container" [class.collapsed]="kpiCardsCollapsed">
            <button class="kpi-toggle-btn" (click)="toggleKpiCards()" [title]="kpiCardsCollapsed ? 'Expand KPI cards' : 'Collapse KPI cards'">
              <i class="fa-solid" [class.fa-chevron-up]="!kpiCardsCollapsed" [class.fa-chevron-down]="kpiCardsCollapsed"></i>
            @if (!kpiCardsCollapsed) {
              <!-- Expanded View -->
              <div class="overview-cards">
                <div class="overview-card">
                  <div class="card-icon card-icon--engines">
                    <div class="card-value">{{ engineStats?.totalEngines || 0 }}</div>
                    <div class="card-label">Registered Engines</div>
                    <div class="card-subtitle">{{ engineStats?.loadedEngines || 0 }} loaded</div>
                  <div class="card-icon card-icon--memory">
                    <div class="card-value">{{ formatBytes(engineStats?.totalEstimatedMemoryBytes || 0) }}</div>
                    <div class="card-label">Engine Memory</div>
                    <div class="card-subtitle">Estimated total</div>
                  <div class="card-icon" [class.card-icon--warning]="redundantLoads.length > 0" [class.card-icon--success]="redundantLoads.length === 0">
                    <div class="card-value">{{ redundantLoads.length }}</div>
                    <div class="card-label">Redundant Loads</div>
                    <div class="card-subtitle">
                      @if (redundantLoads.length === 0) {
                        No redundant loading detected
                        {{ redundantLoads.length }} entities loaded by multiple engines
              <!-- Collapsed View - Mini KPI bar -->
              <div class="overview-cards-mini">
                <div class="mini-kpi" title="Registered Engines">
                  <span class="mini-value">{{ engineStats?.totalEngines || 0 }}</span>
                  <span class="mini-label">Engines</span>
                <div class="mini-divider"></div>
                <div class="mini-kpi" title="Engine Memory">
                  <span class="mini-value">{{ formatBytes(engineStats?.totalEstimatedMemoryBytes || 0) }}</span>
                  <span class="mini-label">Memory</span>
                <div class="mini-kpi" [class.warning]="redundantLoads.length > 0" title="Redundant Loads">
                  <span class="mini-value">{{ redundantLoads.length }}</span>
                  <span class="mini-label">Redundant</span>
          <!-- Main Content with Left Nav -->
            <!-- Left Navigation -->
            <div class="left-nav">
              <div class="nav-section">
                <div class="nav-section-title">Diagnostics</div>
                  [class.active]="activeSection === 'engines'"
                  (click)="setActiveSection('engines')"
                  <span>Engine Registry</span>
                  <span class="nav-badge">{{ engineStats?.totalEngines || 0 }}</span>
                  [class.active]="activeSection === 'redundant'"
                  (click)="setActiveSection('redundant')"
                  <span>Redundant Loading</span>
                  @if (redundantLoads.length > 0) {
                    <span class="nav-badge nav-badge--warning">{{ redundantLoads.length }}</span>
                    <span class="nav-badge nav-badge--success">0</span>
                  [class.active]="activeSection === 'performance'"
                  (click)="setActiveSection('performance')"
                  <span>Performance</span>
                  <span class="nav-badge">{{ telemetrySummary?.totalEvents || 0 }}</span>
                  [class.active]="activeSection === 'cache'"
                  (click)="setActiveSection('cache')"
                  <span>Local Cache</span>
                  <span class="nav-badge">{{ cacheStats?.totalEntries || 0 }}</span>
              <!-- Engine Registry Section -->
              @if (activeSection === 'engines') {
                <div class="section-panel">
                      Registered Engines
                      <button class="action-btn" (click)="refreshAllEngines()" [disabled]="isRefreshingEngines">
                        <i class="fa-solid fa-sync" [class.spinning]="isRefreshingEngines"></i>
                        Refresh All Engines
                  <div class="section-panel-content">
                    @if (engines.length === 0) {
                        <p>No engines registered yet</p>
                        <span class="empty-hint">Engines register themselves when they are first configured</span>
                      <div class="engine-grid">
                        @for (engine of engines; track engine.className) {
                          <div class="engine-card" [class.loaded]="engine.isLoaded">
                            <div class="engine-header">
                              <div class="engine-name" [title]="engine.className">{{ engine.className }}</div>
                              <div class="engine-status" [class.status-loaded]="engine.isLoaded" [class.status-pending]="!engine.isLoaded">
                                {{ engine.isLoaded ? 'Loaded' : 'Not Loaded' }}
                            <div class="engine-stats">
                                <span class="stat-label">Memory:</span>
                                <span class="stat-value">{{ engine.memoryDisplay }}</span>
                                <span class="stat-label">Items:</span>
                                <span class="stat-value">{{ engine.itemCount.toLocaleString() }}</span>
                              @if (engine.lastLoadedAt) {
                                  <span class="stat-label">Loaded:</span>
                                  <span class="stat-value">{{ formatTime(engine.lastLoadedAt) }}</span>
                            <div class="engine-actions">
                              <button class="engine-action-btn" (click)="refreshSingleEngine(engine, $event)" [disabled]="!engine.isLoaded || isRefreshingSingleEngine === engine.className" title="Refresh this engine">
                                <i class="fa-solid fa-sync" [class.spinning]="isRefreshingSingleEngine === engine.className"></i>
                              <button class="engine-action-btn" (click)="openEngineDetailPanel(engine)" [disabled]="!engine.isLoaded" title="View engine details">
              <!-- Redundant Loading Section -->
              @if (activeSection === 'redundant') {
                      Redundant Entity Loading
                    <div class="info-banner">
                        <strong>What is this?</strong>
                        This section shows entities that are loaded by multiple engines.
                        Redundant loading indicates potential optimization opportunities where engines
                        could share data or consolidate their loading logic.
                      <div class="empty-state success-state">
                        <p>No redundant entity loading detected</p>
                        <span class="empty-hint">Each entity is being loaded by only one engine</span>
                      <div class="redundant-loads-table-wrapper">
                        <table class="redundant-loads-table">
                              <th>Entity Name</th>
                              <th>Loaded By Engines</th>
                              <th class="text-right">Engine Count</th>
                            @for (load of redundantLoads; track load.entityName) {
                                <td class="entity-name">{{ load.entityName }}</td>
                                <td class="engines-cell">
                                  <div class="engine-chips">
                                    @for (engine of load.engines; track engine) {
                                      <span class="engine-chip">{{ engine }}</span>
                                <td class="text-right count-cell">
                                  <span class="count-badge">{{ load.engines.length }}</span>
                      <div class="recommendation-banner">
                          <strong>Recommendation:</strong>
                          Consider consolidating data loading by having dependent engines
                          access data from a parent engine, or restructuring the engine
                          hierarchy to avoid duplicate data fetches.
              <!-- Performance Section -->
              @if (activeSection === 'performance') {
                <div class="section-panel perf-panel">
                      Performance Telemetry
                      <!-- Source toggle -->
                      <div class="source-toggle">
                        <button class="source-btn" [class.active]="telemetrySource === 'client'" (click)="setTelemetrySource('client')">
                          <i class="fa-solid fa-browser"></i>
                          Client
                        <button class="source-btn" [class.active]="telemetrySource === 'server'" (click)="setTelemetrySource('server')">
                      <span class="action-divider"></span>
                      @if (telemetrySource === 'client') {
                        <button class="action-btn" [class.active]="telemetryEnabled" (click)="toggleTelemetry()">
                          <i class="fa-solid" [class.fa-toggle-on]="telemetryEnabled" [class.fa-toggle-off]="!telemetryEnabled"></i>
                          {{ telemetryEnabled ? 'Enabled' : 'Disabled' }}
                        <button class="action-btn" (click)="clearTelemetry()" [disabled]="!telemetryEnabled">
                        <span class="status-indicator" [class.enabled]="serverTelemetryEnabled" [class.disabled]="!serverTelemetryEnabled">
                          <i class="fa-solid" [class.fa-circle-check]="serverTelemetryEnabled" [class.fa-circle-xmark]="!serverTelemetryEnabled"></i>
                          {{ serverTelemetryEnabled ? 'Enabled' : 'Disabled' }}
                          <span class="config-note" title="Configure via mj.config.cjs telemetry section">(config)</span>
                      @if (serverTelemetryLoading) {
                        <span class="loading-indicator">
                  @if (serverTelemetryError) {
                      {{ serverTelemetryError }}
                      <button class="dismiss-btn" (click)="serverTelemetryError = null">
                  <!-- Performance Sub-Navigation Tabs -->
                  <div class="perf-tabs">
                    <button class="perf-tab" [class.active]="perfTab === 'monitor'" (click)="setPerfTab('monitor')">
                    <button class="perf-tab" [class.active]="perfTab === 'overview'" (click)="setPerfTab('overview')">
                      <i class="fa-solid fa-gauge"></i>
                      @if (slowQueries.length > 0) {
                        <span class="tab-badge warning">{{ slowQueries.length }}</span>
                    <button class="perf-tab" [class.active]="perfTab === 'events'" (click)="setPerfTab('events')">
                      <span>Events</span>
                      <span class="tab-badge">{{ telemetrySummary?.totalEvents || 0 }}</span>
                    <button class="perf-tab" [class.active]="perfTab === 'patterns'" (click)="setPerfTab('patterns')">
                      <i class="fa-solid fa-fingerprint"></i>
                      <span>Patterns</span>
                      <span class="tab-badge">{{ telemetrySummary?.totalPatterns || 0 }}</span>
                    <button class="perf-tab" [class.active]="perfTab === 'insights'" (click)="setPerfTab('insights')">
                      <span>Insights</span>
                      @if (telemetryInsights.length > 0) {
                        <span class="tab-badge insight">{{ telemetryInsights.length }}</span>
                    @if (!telemetryEnabled) {
                      <div class="info-banner warning-banner">
                          <strong>Telemetry is disabled.</strong>
                          Enable telemetry to track RunView, RunQuery, and Engine loading performance.
                    <!-- Monitor Tab (PerfMon Chart) -->
                    @if (perfTab === 'monitor') {
                      <div class="perfmon-section">
                        <div class="perfmon-header">
                          <div class="perfmon-legend">
                            <span class="legend-item runview"><span class="legend-dot"></span> RunView</span>
                            <span class="legend-item runquery"><span class="legend-dot"></span> RunQuery</span>
                            <span class="legend-item engine"><span class="legend-dot"></span> Engine</span>
                            <span class="legend-item ai"><span class="legend-dot"></span> AI</span>
                          <div class="perfmon-controls">
                            <!-- Interaction Mode Toggle -->
                            <div class="mode-toggle" title="Chart Interaction Mode">
                                class="mode-btn"
                                [class.active]="chartInteractionMode === 'pointer'"
                                (click)="setChartInteractionMode('pointer')"
                                title="Pointer mode - click events to view details">
                                <i class="fa-solid fa-arrow-pointer"></i>
                                [class.active]="chartInteractionMode === 'select'"
                                (click)="setChartInteractionMode('select')"
                                title="Select mode - drag to zoom into a time range">
                                <i class="fa-solid fa-vector-square"></i>
                                [class.active]="chartInteractionMode === 'pan'"
                                (click)="setChartInteractionMode('pan')"
                                title="Pan mode - drag to pan the chart">
                                <i class="fa-solid fa-hand"></i>
                            <span class="control-divider"></span>
                            <button class="chart-control-btn" (click)="zoomPerfChart('in')" title="Zoom In">
                            <button class="chart-control-btn" (click)="zoomPerfChart('out')" title="Zoom Out">
                            <button class="chart-control-btn" (click)="resetPerfChartZoom()" title="Reset Zoom">
                            <label class="compress-toggle" title="Automatically compress gaps with no activity">
                              <input type="checkbox" [(ngModel)]="chartGapCompression" (change)="onGapCompressionChange()">
                              <span>Compress Gaps</span>
                        <div class="perfmon-chart-container">
                          <div class="perfmon-y-axis">
                            <span class="axis-label">Duration (ms)</span>
                          <div #perfChart class="perfmon-chart"></div>
                        <div class="perfmon-footer">
                          <span class="footer-note">
                            @if (chartTimeRangeStart !== null && chartTimeRangeEnd !== null) {
                              Viewing {{ formatRelativeTime(chartTimeRangeStart) }} - {{ formatRelativeTime(chartTimeRangeEnd) }}. Click Reset to show all.
                            } @else if (chartGapCompression) {
                              Drag to select a time range. Gaps >5s compressed.
                              Drag to select a time range. Hover over points for details.
                          @if (telemetrySummary) {
                            <span class="footer-stats">
                              {{ telemetrySummary.totalEvents }} events
                              @if (chartZoomLevel !== 1) {
                                &bull; {{ (chartZoomLevel * 100) | number:'1.0-0' }}% zoom
                    @if (perfTab === 'overview') {
                      <!-- Summary Stats -->
                      <div class="telemetry-summary">
                          <div class="summary-value">{{ telemetrySummary?.totalEvents || 0 }}</div>
                          <div class="summary-label">Total Events</div>
                          <div class="summary-value">{{ telemetrySummary?.totalPatterns || 0 }}</div>
                          <div class="summary-label">Unique Patterns</div>
                          <div class="summary-value">{{ telemetrySummary?.totalInsights || 0 }}</div>
                          <div class="summary-label">Insights</div>
                          <div class="summary-value">{{ telemetrySummary?.activeEvents || 0 }}</div>
                          <div class="summary-label">Active</div>
                      <!-- Category Breakdown -->
                      @if (telemetrySummary && telemetrySummary.totalEvents > 0) {
                        <div class="category-breakdown">
                          <h4>By Category</h4>
                          <div class="category-grid">
                            @for (cat of categoriesWithData; track cat.name) {
                              <div class="category-item" (click)="jumpToPatternsByCategory(cat.name)">
                                <span class="category-name">{{ cat.name }}</span>
                                <span class="category-events">{{ cat.events }}</span>
                                <span class="category-avg">avg {{ cat.avgMs | number:'1.0-0' }}ms</span>
                      <!-- Slow Queries Section -->
                        <div class="slow-queries-section">
                            <i class="fa-solid fa-turtle"></i>
                            Slow Operations (>{{ slowQueryThresholdMs }}ms)
                          <div class="slow-queries-list">
                            @for (query of slowQueries.slice(0, 10); track query.id) {
                              <div class="slow-query-item clickable" [class.cache-hit]="isCacheHit(query)" (click)="openEventDetailPanel(query)">
                                <div class="slow-query-main">
                                  <span class="category-chip small" [class]="'cat-' + query.category.toLowerCase()">
                                    {{ query.category }}
                                  <span class="slow-query-entity">{{ query.entityName || query.operation }}</span>
                                  @if (isCacheHit(query)) {
                                    <span class="cache-hit-badge small" title="Data served from local cache">
                                      CACHED
                                  <span class="slow-query-time">{{ query.elapsedMs | number:'1.0-0' }}ms</span>
                                <!-- Show entities for RunViews batch operation -->
                                @if (isRunViewsOperation(query)) {
                                  <div class="slow-query-entities">
                                    @for (entity of getRunViewsEntities(query, 4); track entity) {
                                      <span class="entity-pill small">{{ entity }}</span>
                                    @if (hasMoreEntities(query, 4)) {
                                      <span class="entity-pill small more">+{{ getRunViewsEntityCount(query) - 4 }} more</span>
                                <!-- RunView parameter pills -->
                                @if (isRunViewOperation(query) && getRunViewPills(query).length > 0) {
                                  <div class="slow-query-pills">
                                    @for (pill of getRunViewPills(query); track pill.label) {
                                      <span class="param-pill small" [class]="'pill-' + pill.type" [title]="pill.value">
                                        <span class="pill-label">{{ pill.label }}:</span>
                                        <span class="pill-value">{{ pill.value }}</span>
                                @if (query.filter) {
                                  <div class="slow-query-filter">{{ truncateString(query.filter, 60) }}</div>
                                <div class="slow-query-timestamp">{{ formatTimestamp(query.timestamp) }}</div>
                      } @else if (telemetryEnabled && telemetrySummary && telemetrySummary.totalEvents > 0) {
                        <div class="success-banner">
                          <span>No slow operations detected. All operations completed under {{ slowQueryThresholdMs }}ms.</span>
                    <!-- Events Tab (Timeline) -->
                    @if (perfTab === 'events') {
                      <!-- Filter Bar for Events -->
                      <div class="filter-bar compact">
                            placeholder="Search events..."
                            [(ngModel)]="searchQuery"
                            (ngModelChange)="onSearchChange()">
                          @if (searchQuery) {
                          <button class="filter-btn" [class.active]="categoryFilter === 'all'" (click)="setCategoryFilter('all')">
                            <button class="filter-btn" [class.active]="categoryFilter === cat.name" (click)="setCategoryFilterByName(cat.name)">
                              {{ cat.name }}
                      <div class="timeline-section">
                          @if (filteredEvents.length > 0) {
                            @for (event of filteredEvents.slice(0, 50); track event.id) {
                              <div class="timeline-item clickable" [class]="'tl-' + event.category.toLowerCase()" [class.cache-hit]="isCacheHit(event)" (click)="openEventDetailPanel(event)">
                                  @if (isCacheHit(event)) {
                                    <div class="marker-bolt">
                                    <div class="marker-dot"></div>
                                  <div class="marker-line"></div>
                                    <span class="timeline-time">{{ formatTimestamp(event.timestamp) }}</span>
                                    <span class="category-chip small" [class]="'cat-' + event.category.toLowerCase()">
                                      {{ event.category }}
                                      <span class="cache-hit-badge" title="Data served from local cache">
                                    @if (event.elapsedMs !== undefined) {
                                      <span class="timeline-duration" [class.slow]="(event.elapsedMs || 0) >= slowQueryThresholdMs">
                                        {{ event.elapsedMs | number:'1.0-0' }}ms
                                    <span class="timeline-operation">{{ event.operation }}</span>
                                    @if (event.entityName) {
                                      <span class="timeline-entity">{{ event.entityName }}</span>
                                    @if (isRunViewsOperation(event)) {
                                      <div class="timeline-entities">
                                        @for (entity of getRunViewsEntities(event, 3); track entity) {
                                          <span class="entity-pill">{{ entity }}</span>
                                        @if (hasMoreEntities(event, 3)) {
                                          <span class="entity-pill more">+{{ getRunViewsEntityCount(event) - 3 }} more</span>
                                  @if (isRunViewOperation(event) && getRunViewPills(event).length > 0) {
                                    <div class="timeline-pills">
                                      @for (pill of getRunViewPills(event); track pill.label) {
                                        <span class="param-pill" [class]="'pill-' + pill.type" [title]="pill.value">
                                  @if (event.filter) {
                                    <div class="timeline-filter">{{ truncateString(event.filter, 80) }}</div>
                              <p>No events recorded yet</p>
                    <!-- Patterns Tab -->
                    @if (perfTab === 'patterns') {
                      <!-- Filter Bar -->
                            placeholder="Search patterns..."
                      @if (filteredPatterns.length > 0) {
                        <div class="patterns-section">
                          <div class="patterns-table-wrapper">
                            <table class="patterns-table sortable">
                                  <th class="sortable-header" (click)="sortPatternsBy('category')">
                                    <i class="fa-solid" [class]="getSortIcon('category')"></i>
                                  <th class="sortable-header" (click)="sortPatternsBy('operation')">
                                    Operation
                                    <i class="fa-solid" [class]="getSortIcon('operation')"></i>
                                  <th class="sortable-header" (click)="sortPatternsBy('entity')">
                                    Entity/Query
                                    <i class="fa-solid" [class]="getSortIcon('entity')"></i>
                                  <th>Filter</th>
                                  <th class="sortable-header text-right" (click)="sortPatternsBy('count')">
                                    Count
                                    <i class="fa-solid" [class]="getSortIcon('count')"></i>
                                  <th class="sortable-header text-right" (click)="sortPatternsBy('avgMs')">
                                    Avg (ms)
                                    <i class="fa-solid" [class]="getSortIcon('avgMs')"></i>
                                  <th class="sortable-header text-right" (click)="sortPatternsBy('totalMs')">
                                    Total (ms)
                                    <i class="fa-solid" [class]="getSortIcon('totalMs')"></i>
                                @for (pattern of filteredPatterns; track pattern.fingerprint) {
                                  <tr [class.duplicate-row]="pattern.count >= 2" [class.slow-row]="pattern.avgElapsedMs >= slowQueryThresholdMs">
                                      <span class="category-chip" [class]="'cat-' + pattern.category.toLowerCase()">
                                        {{ pattern.category }}
                                    <td class="operation-cell">{{ pattern.operation }}</td>
                                    <td class="entity-cell">{{ pattern.entityName || '-' }}</td>
                                    <td class="filter-cell" [title]="pattern.filter || ''">
                                      {{ truncateString(pattern.filter, 30) }}
                                    <td class="text-right">
                                      @if (pattern.count >= 2) {
                                        <span class="count-warning">{{ pattern.count }}</span>
                                        {{ pattern.count }}
                                    <td class="text-right" [class.slow-value]="pattern.avgElapsedMs >= slowQueryThresholdMs">
                                      {{ pattern.avgElapsedMs | number:'1.1-1' }}
                                    <td class="text-right">{{ pattern.totalElapsedMs | number:'1.0-0' }}</td>
                      } @else if (telemetryEnabled && telemetryPatterns.length === 0) {
                          <p>No telemetry data yet</p>
                          <span class="empty-hint">Navigate around the app to generate performance data</span>
                      } @else if (searchQuery || categoryFilter !== 'all') {
                          <p>No patterns match your filter</p>
                    <!-- Insights Tab -->
                    @if (perfTab === 'insights') {
                        <div class="insights-section">
                          <div class="insights-list">
                            @for (insight of telemetryInsights; track insight.id) {
                              <div class="insight-card expandable" [class]="getSeverityClass(insight.severity)" [class.expanded]="insight.expanded">
                                <div class="insight-header" (click)="toggleInsightExpanded(insight)">
                                  <i class="fa-solid" [class]="getSeverityIcon(insight.severity)"></i>
                                  <span class="insight-title">{{ insight.title }}</span>
                                  <span class="insight-category">{{ insight.category }}</span>
                                  <i class="fa-solid expand-icon" [class.fa-chevron-down]="!insight.expanded" [class.fa-chevron-up]="insight.expanded"></i>
                                <!-- Always show key info for actionability -->
                                <div class="insight-key-info">
                                  @if (insight.entityName) {
                                    <div class="key-info-item">
                                      <span class="key-label">Entity:</span>
                                      <span class="key-value entity-name">{{ insight.entityName }}</span>
                                  @if (getInsightFilter(insight)) {
                                      <span class="key-label">Filter:</span>
                                      <code class="key-value filter-code">{{ getInsightFilter(insight) }}</code>
                                <div class="insight-message">{{ insight.message }}</div>
                                <div class="insight-suggestion">
                                  {{ insight.suggestion }}
                                @if (insight.expanded) {
                                  <div class="insight-details">
                                    <!-- Show all params from first related event -->
                                    @if (insight.relatedEvents.length > 0) {
                                        <div class="detail-label">Full Parameters</div>
                                        <div class="params-display">
                                          @for (param of getEventParams(insight.relatedEvents[0]); track param.key) {
                                            <div class="param-row">
                                              <span class="param-key">{{ param.key }}:</span>
                                              <span class="param-value">{{ param.value }}</span>
                                        <div class="detail-label">Related Calls ({{ insight.relatedEvents.length }})</div>
                                        <div class="related-events">
                                          @for (event of insight.relatedEvents; track event.id) {
                                            <div class="related-event">
                                              <span class="event-time">{{ formatTimestamp(event.timestamp) }}</span>
                                              <span class="event-duration">{{ event.elapsedMs | number:'1.0-0' }}ms</span>
                                                <span class="event-entity">{{ event.entityName }}</span>
                                                <span class="event-filter">{{ truncateString(event.filter, 40) }}</span>
                          <i class="fa-solid fa-check-circle" style="color: #4caf50;"></i>
                          <p>No optimization insights</p>
                          <span class="empty-hint">Insights will appear when potential optimizations are detected</span>
              <!-- Local Cache Section -->
              @if (activeSection === 'cache') {
                      Local Cache
                      <button class="action-btn" (click)="clearAllCache()" [disabled]="!cacheStats || cacheStats.totalEntries === 0">
                    @if (!cacheInitialized) {
                          <strong>Cache not initialized.</strong>
                          The LocalCacheManager requires initialization with a storage provider during app startup.
                      <!-- Cache Summary Stats -->
                      <div class="cache-summary">
                          <div class="summary-value">{{ cacheStats?.totalEntries || 0 }}</div>
                          <div class="summary-label">Total Entries</div>
                          <div class="summary-value">{{ formatBytes(cacheStats?.totalSizeBytes || 0) }}</div>
                          <div class="summary-label">Total Size</div>
                          <div class="summary-value">{{ cacheStats?.hits || 0 }}</div>
                          <div class="summary-label">Cache Hits</div>
                          <div class="summary-value">{{ cacheStats?.misses || 0 }}</div>
                          <div class="summary-label">Cache Misses</div>
                          <div class="summary-value">{{ cacheHitRate | number:'1.1-1' }}%</div>
                          <div class="summary-label">Hit Rate</div>
                      <!-- Cache Type Breakdown -->
                      <div class="cache-type-breakdown">
                        <h4>By Type</h4>
                        <div class="type-grid">
                          <div class="type-item" (click)="setCacheTypeFilter('dataset')">
                            <span class="type-icon"><i class="fa-solid fa-layer-group"></i></span>
                            <span class="type-name">Datasets</span>
                            <span class="type-count">{{ cacheStats?.byType?.dataset?.count || 0 }}</span>
                            <span class="type-size">{{ formatBytes(cacheStats?.byType?.dataset?.sizeBytes || 0) }}</span>
                          <div class="type-item" (click)="setCacheTypeFilter('runview')">
                            <span class="type-icon"><i class="fa-solid fa-table"></i></span>
                            <span class="type-name">RunViews</span>
                            <span class="type-count">{{ cacheStats?.byType?.runview?.count || 0 }}</span>
                            <span class="type-size">{{ formatBytes(cacheStats?.byType?.runview?.sizeBytes || 0) }}</span>
                          <div class="type-item" (click)="setCacheTypeFilter('runquery')">
                            <span class="type-icon"><i class="fa-solid fa-code"></i></span>
                            <span class="type-name">RunQueries</span>
                            <span class="type-count">{{ cacheStats?.byType?.runquery?.count || 0 }}</span>
                            <span class="type-size">{{ formatBytes(cacheStats?.byType?.runquery?.sizeBytes || 0) }}</span>
                      <!-- Cache Entries Table -->
                      @if (filteredCacheEntries.length > 0) {
                        <div class="cache-entries-section">
                            <h4>Cache Entries</h4>
                              <button class="filter-btn" [class.active]="cacheTypeFilter === 'all'" (click)="setCacheTypeFilter('all')">All</button>
                              <button class="filter-btn" [class.active]="cacheTypeFilter === 'dataset'" (click)="setCacheTypeFilter('dataset')">Datasets</button>
                              <button class="filter-btn" [class.active]="cacheTypeFilter === 'runview'" (click)="setCacheTypeFilter('runview')">RunViews</button>
                              <button class="filter-btn" [class.active]="cacheTypeFilter === 'runquery'" (click)="setCacheTypeFilter('runquery')">RunQueries</button>
                          <div class="cache-entries-table-wrapper">
                            <table class="cache-entries-table">
                                  <th>Name</th>
                                  <th class="text-right">Size</th>
                                  <th class="text-right">Hits</th>
                                  <th>Cached At</th>
                                  <th>Last Accessed</th>
                                @for (entry of filteredCacheEntries.slice(0, 50); track entry.key) {
                                      <span class="cache-type-chip" [class]="'type-' + entry.type">
                                        {{ entry.type }}
                                    <td class="entry-name">
                                      {{ entry.name }}
                                      @if (entry.fingerprint) {
                                        <code class="entry-fingerprint">{{ truncateString(entry.fingerprint, 20) }}</code>
                                    <td class="text-right">{{ formatBytes(entry.sizeBytes) }}</td>
                                    <td class="text-right">{{ entry.accessCount }}</td>
                                    <td>{{ formatCacheTimestamp(entry.cachedAt) }}</td>
                                    <td>{{ formatCacheTimestamp(entry.lastAccessedAt) }}</td>
                                      <button class="icon-btn" (click)="invalidateCacheEntry(entry)" title="Invalidate">
                          @if (filteredCacheEntries.length > 50) {
                            <div class="table-footer">
                              Showing 50 of {{ filteredCacheEntries.length }} entries
                      } @else if (cacheStats && cacheStats.totalEntries === 0) {
                          <p>No cached data</p>
                          <span class="empty-hint">Data will be cached as you use the application</span>
          <!-- Last Updated -->
            <span class="last-updated">
              Last updated: {{ lastUpdated | date:'medium' }}
            <button class="export-btn" (click)="exportTelemetryData()" [disabled]="!telemetryEnabled || telemetryEvents.length === 0">
              Export JSON
        <!-- Event Detail Slide-in Panel -->
        @if (eventDetailPanel.isOpen && eventDetailPanel.event) {
          <div class="event-detail-overlay" (click)="closeEventDetailPanel()"></div>
          <div class="event-detail-panel" [class.open]="eventDetailPanel.isOpen">
                <span class="category-chip" [class]="'cat-' + eventDetailPanel.event.category.toLowerCase()">
                  {{ eventDetailPanel.event.category }}
                <h3>Event Details</h3>
              <button class="close-btn" (click)="closeEventDetailPanel()">
              <!-- Key Metrics -->
              <div class="detail-metrics">
                <div class="metric">
                  <div class="metric-value" [class.slow]="(eventDetailPanel.event.elapsedMs || 0) >= slowQueryThresholdMs">
                    {{ eventDetailPanel.event.elapsedMs !== undefined ? (eventDetailPanel.event.elapsedMs | number:'1.0-0') + 'ms' : 'In Progress' }}
                  <div class="metric-value">{{ formatTimestamp(eventDetailPanel.event.timestamp) }}</div>
                  <div class="metric-label">Time</div>
                  <div class="metric-value">+{{ formatRelativeTime(eventDetailPanel.event.startTime - telemetryBootTime) }}</div>
                  <div class="metric-label">Relative</div>
              <!-- Operation Info -->
                <h4><i class="fa-solid fa-code"></i> Operation</h4>
                    <span class="detail-key">Operation:</span>
                    <span class="detail-val">{{ eventDetailPanel.event.operation }}</span>
                  @if (eventDetailPanel.event.entityName) {
                      <span class="detail-key">Entity:</span>
                      <span class="detail-val entity-highlight">{{ eventDetailPanel.event.entityName }}</span>
                  @if (eventDetailPanel.event.filter) {
                      <span class="detail-key">Filter:</span>
                      <code class="detail-val filter-val">{{ eventDetailPanel.event.filter }}</code>
              <!-- All Parameters -->
                <h4><i class="fa-solid fa-sliders"></i> Parameters</h4>
                  @for (param of getEventParams(eventDetailPanel.event); track param.key) {
                    <div class="param-item">
                      <span class="param-name">{{ param.key }}</span>
                      <span class="param-val">{{ param.value }}</span>
              <!-- Related Pattern -->
              @if (eventDetailPanel.relatedPattern) {
                  <h4><i class="fa-solid fa-fingerprint"></i> Related Pattern</h4>
                  <div class="pattern-summary">
                    <div class="pattern-stat">
                      <span class="stat-val">{{ eventDetailPanel.relatedPattern.count }}</span>
                      <span class="stat-label">Total Calls</span>
                      <span class="stat-val">{{ eventDetailPanel.relatedPattern.avgElapsedMs | number:'1.1-1' }}ms</span>
                      <span class="stat-val">{{ eventDetailPanel.relatedPattern.minElapsedMs | number:'1.0-0' }} - {{ eventDetailPanel.relatedPattern.maxElapsedMs | number:'1.0-0' }}ms</span>
                      <span class="stat-label">Range</span>
                  @if (eventDetailPanel.relatedPattern.count >= 2) {
                    <div class="pattern-warning">
                      This pattern has been called {{ eventDetailPanel.relatedPattern.count }} times. Consider caching or batching.
                <button class="action-button" (click)="copyEventToClipboard(eventDetailPanel.event)">
                  Copy JSON
                  <button class="action-button" (click)="filterByEntity(eventDetailPanel.event.entityName)">
                    Filter by Entity
        <!-- Engine Detail Slide-in Panel -->
        @if (engineDetailPanel.isOpen && engineDetailPanel.engine) {
          <div class="engine-detail-overlay" (click)="closeEngineDetailPanel()"></div>
          <div class="engine-detail-panel" [class.open]="engineDetailPanel.isOpen">
                <h3>{{ engineDetailPanel.engine.className }}</h3>
              <div class="panel-header-actions">
                <button class="icon-btn" (click)="refreshEngineInDetailPanel()" [disabled]="engineDetailPanel.isRefreshing" title="Refresh engine">
                  <i class="fa-solid fa-sync" [class.spinning]="engineDetailPanel.isRefreshing"></i>
                <button class="close-btn" (click)="closeEngineDetailPanel()">
              <!-- Engine Summary -->
              <div class="engine-summary-section">
                <div class="summary-stat">
                  <span class="summary-label">Status</span>
                  <span class="summary-value">
                    <span class="status-dot" [class.status-loaded]="engineDetailPanel.engine.isLoaded"></span>
                    {{ engineDetailPanel.engine.isLoaded ? 'Loaded' : 'Not Loaded' }}
                  <span class="summary-label">Memory</span>
                  <span class="summary-value">{{ engineDetailPanel.engine.memoryDisplay }}</span>
                  <span class="summary-label">Items</span>
                  <span class="summary-value">{{ engineDetailPanel.engine.itemCount.toLocaleString() }}</span>
                @if (engineDetailPanel.engine.lastLoadedAt) {
                    <span class="summary-label">Last Loaded</span>
                    <span class="summary-value">{{ formatTime(engineDetailPanel.engine.lastLoadedAt) }}</span>
              <!-- Config Items -->
              <div class="config-items-section">
                  Data Configs ({{ engineDetailPanel.configItems.length }})
                @if (engineDetailPanel.configItems.length === 0) {
                    <p>No config items found</p>
                  <div class="config-items-list">
                    @for (item of engineDetailPanel.configItems; track item.propertyName) {
                      <div class="config-item" [class.expanded]="item.expanded">
                        <div class="config-item-header" (click)="toggleConfigItemExpanded(item)">
                          <div class="config-item-info">
                            <span class="config-type-chip" [class]="'type-' + item.type">{{ item.type }}</span>
                            <span class="config-name">{{ item.entityName || item.datasetName || item.propertyName }}</span>
                          <div class="config-item-stats">
                            <span class="config-stat">{{ item.itemCount }} items</span>
                            <span class="config-stat">{{ item.memoryDisplay }}</span>
                            <i class="fa-solid expand-icon" [class.fa-chevron-down]="!item.expanded" [class.fa-chevron-up]="item.expanded"></i>
                          <div class="config-item-details">
                            <div class="config-detail-row">
                              <span class="detail-label">Property:</span>
                              <code class="detail-value">{{ item.propertyName }}</code>
                            @if (item.filter) {
                                <span class="detail-label">Filter:</span>
                                <code class="detail-value">{{ item.filter }}</code>
                            @if (item.orderBy) {
                                <span class="detail-label">Order By:</span>
                                <code class="detail-value">{{ item.orderBy }}</code>
                            <!-- Data Table with Paging -->
                            @if (item.displayedData.length > 0) {
                              <div class="sample-data-section">
                                <div class="sample-header">
                                  <span class="sample-title">Data ({{ item.displayedData.length }} of {{ item.itemCount }})</span>
                                  <div class="sample-header-actions">
                                    @if (!item.allDataLoaded && item.itemCount > item.displayedData.length) {
                                      <button class="load-more-btn" (click)="loadMoreData(item)" [disabled]="item.isLoadingMore" title="Load more records">
                                        @if (item.isLoadingMore) {
                                          <i class="fa-solid fa-spinner spinning"></i>
                                        Load More
                                      <button class="load-all-btn" (click)="loadAllData(item)" [disabled]="item.isLoadingMore" title="Load all records">
                                        Load All
                                    @if (item.allDataLoaded) {
                                      <span class="all-loaded-badge">
                                        All Loaded
                                <div class="sample-data-table-wrapper">
                                  <table class="sample-data-table">
                                        <th class="action-col"></th>
                                        @for (col of getSampleDataColumns(item); track col) {
                                          <th>{{ col }}</th>
                                      @for (row of item.displayedData; track $index) {
                                          <td class="action-col">
                                            @if (item.entityName && getRecordId(row)) {
                                              <button class="open-record-btn" (click)="openEntityRecord(item.entityName, row)" title="Open record">
                                            <td [title]="getSampleDataValue(row, col)">{{ truncateString(getSampleDataValue(row, col), 30) }}</td>
        .system-diagnostics {
        .diagnostics-header {
        .auto-refresh-control {
        .auto-refresh-control label {
        .auto-refresh-control input[type="checkbox"] {
        .refresh-indicator {
            background: #43a047;
            box-shadow: 0 2px 8px rgba(76, 175, 80, 0.3);
        /* Overview Cards Container (Collapsible) */
        .overview-cards-container {
        .overview-cards-container.collapsed {
        .kpi-toggle-btn {
        .kpi-toggle-btn:hover {
        .overview-cards-container.collapsed .kpi-toggle-btn {
        /* Overview Cards */
        .overview-cards {
            padding-right: 60px;
        /* Mini KPI Bar (Collapsed State) */
        .overview-cards-mini {
            padding: 10px 60px 10px 24px;
        .mini-kpi {
        .mini-kpi i {
        .mini-kpi.warning i {
        .mini-value {
        .mini-label {
        .mini-divider {
        .overview-card:hover {
        .card-icon--engines {
        .card-icon--memory {
        .card-icon--warning {
            background: linear-gradient(135deg, #ff9800 0%, #f57c00 100%);
        .card-icon--success {
            background: linear-gradient(135deg, #43e97b 0%, #38f9d7 100%);
        .card-content {
        .card-value {
        .card-label {
        .card-subtitle {
        /* Left Navigation */
        .left-nav {
        .nav-section {
        .nav-section-title {
        .nav-item span:first-of-type {
        .nav-badge--warning {
            background: #ff9800 !important;
        .nav-badge--success {
            background: #4caf50 !important;
        .section-panel {
        .section-panel-content {
        .panel-header h3 i {
        .action-divider {
        .source-toggle {
        .source-btn {
        .source-btn:hover {
        .source-btn.active {
        .source-btn i {
        .loading-indicator {
            border: 1px solid #ffcdd2;
            color: #e53935;
        .error-banner .dismiss-btn {
        .error-banner .dismiss-btn:hover {
        /* Status indicator for read-only server telemetry status */
        .status-indicator.enabled {
        .status-indicator.disabled {
        .status-indicator .config-note {
        .empty-state.success-state i {
        /* Engine Grid */
        .engine-grid {
        .engine-card {
        .engine-card.loaded {
            border-color: #4caf50;
            background: #f1f8e9;
        .engine-card:hover {
        .engine-header {
        .engine-name {
        .engine-status {
        .status-loaded {
            background: #c8e6c9;
        .status-pending {
            color: #ef6c00;
        .engine-stats {
        .engine-actions {
            border-top: 1px solid rgba(0, 0, 0, 0.08);
        .engine-action-btn {
        .engine-action-btn:hover:not(:disabled) {
        .engine-action-btn:disabled {
        /* Info Banner */
        .info-banner {
        .info-banner i {
        /* Recommendation Banner */
        .recommendation-banner {
        .recommendation-banner i {
        /* Redundant Loads Table */
        .redundant-loads-table-wrapper {
        .redundant-loads-table {
        .redundant-loads-table th {
        .redundant-loads-table th.text-right {
        .redundant-loads-table td {
        .redundant-loads-table td.text-right {
        .redundant-loads-table tbody tr:last-child td {
        .redundant-loads-table tbody tr:hover {
        .redundant-loads-table .entity-name {
        .engine-chips {
        .engine-chip {
            background: #ff9800;
        .last-updated {
        /* Performance Section Styles */
        .warning-banner {
            background: #fff3e0 !important;
            color: #e65100 !important;
        .warning-banner i {
        .telemetry-summary {
        .category-breakdown {
        .category-breakdown h4 {
        .category-grid {
        .category-events {
        .category-avg {
        .insights-section {
        .insights-section h4 {
        .insights-section h4 i {
        .insights-list {
        .insight-card {
            border-left: 4px solid #ccc;
        .insight-card.severity-info {
        .insight-card.severity-warning {
            border-left-color: #ff9800;
        .insight-card.severity-optimization {
        .insight-header {
        .insight-header i {
        .severity-info .insight-header i { color: #2196f3; }
        .severity-warning .insight-header i { color: #ff9800; }
        .severity-optimization .insight-header i { color: #4caf50; }
        .insight-title {
        .insight-category {
            background: rgba(0,0,0,0.1);
        .insight-message {
        .insight-suggestion {
        .insight-suggestion i {
        .patterns-section h4 {
        .patterns-section h4 i {
        .patterns-table-wrapper {
        .patterns-table {
        .patterns-table th {
        .patterns-table td {
        .patterns-table tbody tr:last-child td {
        .patterns-table tbody tr:hover {
        .duplicate-row {
        .duplicate-row:hover {
            background: #ffe0b2 !important;
        .category-chip {
        .cat-runview { background: #e3f2fd; color: #1565c0; }
        .cat-runquery { background: #f3e5f5; color: #7b1fa2; }
        .cat-engine { background: #e8f5e9; color: #2e7d32; }
        .cat-ai { background: #fff3e0; color: #e65100; }
        .cat-cache { background: #fce4ec; color: #c2185b; }
        .cat-network { background: #e0f2f1; color: #00695c; }
        .cat-custom { background: #f5f5f5; color: #616161; }
        .operation-cell {
        .entity-cell {
        .count-warning {
        .action-btn.active {
        /* Slow Queries Section */
        .slow-queries-section {
            border-left: 4px solid #ff9800;
        .slow-queries-section h4 {
        .slow-queries-section h4 i {
        .slow-queries-list {
        .slow-query-item {
        .slow-query-item.clickable {
        .slow-query-item.clickable:hover {
            border-color: #ffb74d;
        .slow-query-main {
        .slow-query-entity {
        .slow-query-time {
        .slow-query-filter {
        .slow-query-timestamp {
        /* Expandable Insight Cards */
        .insight-card.expandable {
        .insight-card.expandable:hover {
        .insight-card .expand-icon {
        .insight-card.expanded {
        .insight-details {
            border-top: 1px solid rgba(0, 0, 0, 0.1);
        .related-events {
        .related-event {
        .event-time {
        .event-duration {
        .event-filter {
        /* Timeline Section */
        .timeline-section h4 {
        .timeline-section h4 i {
            left: -24px;
        .marker-dot {
        .marker-line {
        .timeline-item:last-child .marker-line {
        .tl-runview .marker-dot { background: #1565c0; }
        .tl-runquery .marker-dot { background: #7b1fa2; }
        .tl-engine .marker-dot { background: #2e7d32; }
        .tl-ai .marker-dot { background: #e65100; }
        .tl-cache .marker-dot { background: #c2185b; }
        /* Cache hit bolt marker */
        .marker-bolt {
            margin-left: -4px;
        .marker-bolt i {
            filter: drop-shadow(0 0 3px rgba(245, 158, 11, 0.6));
        /* Category-specific bolt colors */
        .tl-runview.cache-hit .marker-bolt { color: #1565c0; }
        .tl-runquery.cache-hit .marker-bolt { color: #7b1fa2; }
        .tl-engine.cache-hit .marker-bolt { color: #2e7d32; }
        .tl-ai.cache-hit .marker-bolt { color: #e65100; }
        .tl-cache.cache-hit .marker-bolt { color: #c2185b; }
        /* Cache hit badge */
        .cache-hit-badge {
        .cache-hit-badge i {
        /* Highlighted background for cache hit items */
        .timeline-item.cache-hit .timeline-content {
            background: linear-gradient(135deg, #fefce8 0%, #fef9c3 100%);
            border: 1px solid #fde047;
        .timeline-duration {
        .timeline-duration.slow {
        .timeline-entity {
        .timeline-filter {
        /* Entity Pills for RunViews batch operations */
        .timeline-entities,
        .slow-query-entities {
        .entity-pill {
        .entity-pill.small {
        .entity-pill.more {
        /* Parameter Pills */
        .timeline-pills,
        .slow-query-pills {
        .param-pill {
        .param-pill.small {
        .param-pill .pill-label {
        .param-pill .pill-value {
        /* Pill type colors */
        .param-pill.pill-filter {
            border: 1px solid #ffcc80;
        .param-pill.pill-order {
            border: 1px solid #a5d6a7;
        .param-pill.pill-result {
            border: 1px solid #ce93d8;
        .param-pill.pill-limit {
        .param-pill.pill-batch {
            background: #fce4ec;
            border: 1px solid #f48fb1;
            color: #c2185b;
        .param-pill.pill-info {
            color: #616161;
        /* Slow query cache hit styling */
        .slow-query-item.cache-hit {
            border-color: #fde047;
        .cache-hit-badge.small {
        /* Filter Bar */
        .filter-bar {
            box-shadow: 0 0 0 2px rgba(102, 126, 234, 0.2);
        /* Sortable Table Headers */
        .sortable-header {
        .sortable-header:hover {
        .sortable-header i {
        .sortable-header i.fa-sort-up,
        .sortable-header i.fa-sort-down {
        /* Table Enhancements */
        .filter-cell {
        .slow-row {
        .slow-row:hover {
            background: #ffcdd2 !important;
        .slow-value {
        /* Small category chips */
        .category-chip.small {
        /* Small empty state */
        .empty-state.small i {
        /* Performance Sub-Tabs */
        .perf-tabs {
        .perf-tab {
        .perf-tab:hover {
        .perf-tab.active {
        .perf-tab i {
        .perf-tab.active .tab-badge {
        .tab-badge.warning {
        .tab-badge.insight {
            color: #f57f17;
        .perf-panel .section-panel-content {
        /* Success Banner */
        .success-banner {
        .success-banner i {
        /* Compact Filter Bar */
        .filter-bar.compact {
        /* Clickable category items */
        .category-item:hover {
        /* Insight Key Info (always visible) */
        .insight-key-info {
        .key-info-item {
            min-width: 50px;
        .key-value.entity-name {
        .key-value.filter-code {
        /* Params Display */
        .params-display {
        .param-row {
        .param-row:last-child {
        .param-key {
        .event-entity {
        /* PerfMon Chart Styles */
        .perfmon-section {
        .perfmon-header {
        .perfmon-header h4 {
            color: #00ff88;
        .perfmon-header h4 i {
        .perfmon-legend {
        .legend-item.runview .legend-dot { background: #00bcd4; }
        .legend-item.runquery .legend-dot { background: #e040fb; }
        .legend-item.engine .legend-dot { background: #00ff88; }
        .legend-item.ai .legend-dot { background: #ff9800; }
        .perfmon-chart-container {
            background: #0d0d1a;
            border: 1px solid #333;
        .perfmon-y-axis {
            border-right: 1px solid #333;
        .perfmon-y-axis .axis-label {
            writing-mode: vertical-rl;
        .perfmon-chart {
        .perfmon-chart svg {
        .perfmon-footer {
            border-top: 1px solid #333;
        .footer-note {
        .footer-note i {
        .footer-stats {
        /* D3 Chart Elements */
        .perfmon-chart :deep(.grid-line) {
            stroke-dasharray: 2,2;
        .perfmon-chart :deep(.axis-line) {
            stroke: #555;
        .perfmon-chart :deep(.axis-text) {
            fill: #888;
        .perfmon-chart :deep(.event-point) {
            transition: r 0.15s ease;
        .perfmon-chart :deep(.event-point:hover) {
            r: 6;
        .perfmon-chart :deep(.tooltip) {
        .perfmon-chart :deep(.tooltip-bg) {
            fill: rgba(0, 0, 0, 0.9);
            rx: 4;
        .perfmon-chart :deep(.tooltip-text) {
            fill: #fff;
        .perfmon-chart :deep(.area-fill) {
            opacity: 0.15;
        .perfmon-chart :deep(.line-path) {
            stroke-width: 1.5;
        /* PerfMon Chart Controls */
        .perfmon-controls {
            background: rgba(255, 255, 255, 0.05);
        .chart-control-btn {
            background: #2a2a2a;
            border: 1px solid #444;
        .chart-control-btn:hover {
            background: #333;
            border-color: #00ff88;
        .chart-control-btn:active {
            transform: scale(0.95);
        .chart-control-btn i {
        /* Mode Toggle Buttons */
            background: #1a1a1a;
        .mode-btn:first-child {
            background: #00ff88;
        .mode-btn.active:hover {
            background: #00cc6a;
        .control-divider {
        .compress-toggle {
        .compress-toggle:hover {
        .compress-toggle input[type="checkbox"] {
            accent-color: #00ff88;
        .compress-toggle span {
        /* Gap indicators in chart */
        .perfmon-chart :deep(.gap-indicator) {
        .perfmon-chart :deep(.gap-indicator:hover) {
        .perfmon-chart :deep(.gap-text) {
        .perfmon-chart :deep(.gap-expand-btn) {
        .perfmon-chart :deep(.gap-expand-btn:hover) {
        /* Selection brush overlay */
        .perfmon-chart :deep(.selection-overlay) {
            pointer-events: all;
        .perfmon-chart :deep(.selection-rect) {
        /* Zoom info display */
        .zoom-info {
        .zoom-info .zoom-level {
        .zoom-info .time-range {
        /* Export Button */
        .export-btn {
        .export-btn:hover:not(:disabled) {
        .export-btn:disabled {
        /* Event Detail Slide-in Panel */
        .event-detail-overlay {
        .event-detail-panel {
            animation: slideIn 0.25s ease;
            from { transform: translateX(100%); }
            to { transform: translateX(0); }
        .event-detail-panel .panel-header {
        .event-detail-panel .panel-title {
        .event-detail-panel .panel-title h3 {
        .event-detail-panel .close-btn {
        .event-detail-panel .close-btn:hover {
        .event-detail-panel .panel-body {
        /* Detail Metrics */
        .detail-metrics {
        .detail-metrics .metric {
        .detail-metrics .metric-value {
        .detail-metrics .metric-value.slow {
        .detail-metrics .metric-label {
        /* Detail Sections */
        .event-detail-panel .detail-section {
        .event-detail-panel .detail-section h4 {
        .event-detail-panel .detail-section h4 i {
        .detail-key {
        .detail-val {
        .detail-val.entity-highlight {
        .detail-val.filter-val {
        /* Params Grid */
        .param-item:last-child {
        .param-val {
        /* Pattern Summary */
        .pattern-summary {
        .pattern-stat {
        .pattern-stat .stat-val {
        .pattern-stat .stat-label {
        .pattern-warning {
        .pattern-warning i {
        /* Detail Actions */
        .action-button i {
        /* Clickable timeline items */
        .timeline-item.clickable {
        .timeline-item.clickable:hover {
        .timeline-item.clickable:hover .marker-dot {
            transform: scale(1.3);
           ENGINE DETAIL PANEL STYLES
        .engine-detail-overlay {
        .engine-detail-panel {
            width: 550px;
        .engine-detail-panel .panel-header {
            background: linear-gradient(135deg, #4caf50 0%, #45a049 100%);
        .engine-detail-panel .panel-title {
        .engine-detail-panel .panel-title h3 {
        .engine-detail-panel .panel-header-actions {
        .engine-detail-panel .icon-btn {
        .engine-detail-panel .icon-btn:hover:not(:disabled) {
        .engine-detail-panel .icon-btn:disabled {
        .engine-detail-panel .close-btn {
        .engine-detail-panel .close-btn:hover {
        .engine-detail-panel .panel-body {
        /* Engine Summary */
        .engine-summary-section {
        .engine-summary-section .summary-stat {
        .engine-summary-section .summary-label {
        .engine-summary-section .summary-value {
        .status-dot.status-loaded {
        /* Config Items Section */
        .config-items-section h4 {
        .config-items-section h4 i {
        .config-items-list {
        .config-item.expanded {
            box-shadow: 0 2px 8px rgba(76, 175, 80, 0.1);
        .config-item-header {
        .config-item-header:hover {
        .config-item-info {
        .config-type-chip {
        .config-type-chip.type-entity {
        .config-type-chip.type-dataset {
        .config-item-stats {
        .config-stat {
        .config-item.expanded .expand-icon {
        .config-item-details {
        .config-detail-row {
        .config-detail-row:last-child {
        .config-detail-row .detail-label {
            min-width: 70px;
        .config-detail-row .detail-value {
        /* Sample Data Section */
        .sample-data-section {
        .sample-header {
        .sample-title {
        .sample-header-actions {
        .load-more-btn,
        .load-all-btn {
            border: 1px solid #667eea;
        .load-more-btn:hover:not(:disabled),
        .load-all-btn:hover:not(:disabled) {
        .load-more-btn:disabled,
        .load-all-btn:disabled {
        .all-loaded-badge {
        .action-col {
            padding: 4px !important;
        .sample-data-table-wrapper {
        .sample-data-table {
        .sample-data-table th {
        .sample-data-table td {
        .sample-data-table tbody tr:last-child td {
        .sample-data-table tbody tr:hover {
        /* Spinning animation for refresh icon */
           LOCAL CACHE SECTION STYLES
        .cache-summary {
        .cache-type-breakdown {
        .cache-type-breakdown h4 {
        .type-grid {
            box-shadow: 0 2px 8px rgba(76, 175, 80, 0.15);
        .type-size {
        .cache-entries-section {
        .cache-entries-section .section-header {
        .cache-entries-section .section-header h4 {
        .cache-entries-table-wrapper {
        .cache-entries-table {
        .cache-entries-table th {
        .cache-entries-table td {
        .cache-entries-table tbody tr:hover {
        .cache-entries-table tbody tr:last-child td {
        .cache-type-chip {
        .cache-type-chip.type-dataset {
        .cache-type-chip.type-runview {
            background: #e0f7fa;
            color: #00838f;
        .cache-type-chip.type-runquery {
        .entry-name {
        .entry-fingerprint {
        .table-footer {
export class SystemDiagnosticsComponent extends BaseResourceComponent implements OnInit, OnDestroy, AfterViewInit {
    // User settings persistence
    autoRefresh = false;
    activeSection: 'engines' | 'redundant' | 'performance' | 'cache' = 'engines';
    lastUpdated = new Date();
    isRefreshingEngines = false;
    kpiCardsCollapsed = false;
    engineStats: EngineMemoryStats | null = null;
    engines: EngineDiagnosticInfo[] = [];
    redundantLoads: RedundantLoadInfo[] = [];
    // Telemetry data
    telemetrySummary: TelemetrySummary | null = null;
    telemetryPatterns: TelemetryPatternDisplay[] = [];
    telemetryInsights: TelemetryInsightDisplay[] = [];
    telemetryEnabled = false;
    categoriesWithData: { name: string; events: number; avgMs: number }[] = [];
    // Telemetry source toggle (client vs server)
    telemetrySource: 'client' | 'server' = 'client';
    serverTelemetryLoading = false;
    serverTelemetryError: string | null = null;
    serverTelemetryEnabled = false; // Read from server config, not changeable at runtime
    // Timeline data
    telemetryEvents: TelemetryEventDisplay[] = [];
    timelineView: 'insights' | 'timeline' | 'chart' = 'insights';
    // Performance sub-tabs
    perfTab: 'monitor' | 'overview' | 'events' | 'patterns' | 'insights' = 'monitor';
    // D3 Chart reference
    @ViewChild('perfChart', { static: false }) perfChartRef!: ElementRef<HTMLDivElement>;
    private chartInitialized = false;
    // Chart zoom and gap compression state
    chartZoomLevel = 1;
    chartGapCompression = true;
    private chartViewportStart = 0;
    private chartViewportEnd = 0;
    private expandedGaps = new Set<number>(); // Track which gaps are expanded
    // Selection-based zoom state
    private isSelecting = false;
    private selectionStartX = 0;
    private selectionRect: d3.Selection<SVGRectElement, unknown, null, undefined> | null = null;
    private chartXScale: d3.ScaleLinear<number, number> | null = null;
    private chartMarginLeft = 50;
    chartTimeRangeStart: number | null = null;  // Currently visible time range start
    // Chart interaction mode: 'pointer' to click events, 'select' for drag-to-zoom, 'pan' for panning
    chartInteractionMode: 'pointer' | 'select' | 'pan' = 'pointer';
    // Store gap segments for inverse mapping (x -> time)
    private chartGapSegments: Array<{ type: 'events' | 'gap'; startTime: number; endTime: number; gapIndex?: number; displayStart: number; displayEnd: number }> = [];
    chartTimeRangeEnd: number | null = null;    // Currently visible time range end
    // Slow queries
    slowQueries: TelemetryEventDisplay[] = [];
    slowQueryThresholdMs = 500;
    // Patterns sorting
    patternSort: PatternSortConfig = { column: 'count', direction: 'desc' };
    // Search/Filter
    searchQuery = '';
    categoryFilter: TelemetryCategory | 'all' = 'all';
    // Store telemetry boot time for relative time calculations (public for template access)
    telemetryBootTime: number = 0;
    // Event detail panel state
    eventDetailPanel: EventDetailPanelState = {
        isOpen: false,
        event: null,
        relatedPattern: null
    // Local Cache data
    cacheStats: CacheStats | null = null;
    cacheEntries: CacheEntryInfo[] = [];
    cacheTypeFilter: CacheEntryType | 'all' = 'all';
    cacheInitialized = false;
    cacheHitRate = 0;
    // Engine detail panel state
    engineDetailPanel: EngineDetailPanelState = {
        engine: null,
        configItems: [],
        isRefreshing: false
    isRefreshingSingleEngine: string | null = null;
        private ngZone: NgZone,
        private route: ActivatedRoute
        // Load user preferences first
        await this.loadUserPreferences();
        // Apply query params (override preferences if present)
        this.applyQueryParams();
        // Subscribe to query param changes
                // Only apply if we've already loaded settings
                if (this.settingsLoaded) {
        // Render the PerfMon chart if we're on the monitor tab
        // Need a small delay to ensure the DOM is fully ready
        if (this.activeSection === 'performance' && this.perfTab === 'monitor') {
            setTimeout(() => this.renderPerfChart(), 100);
    setActiveSection(section: 'engines' | 'redundant' | 'performance' | 'cache'): void {
        if (section === 'cache') {
            this.refreshCacheData();
        if (section === 'performance' && this.perfTab === 'monitor') {
            // Need to wait for DOM to render before chart can be drawn
            setTimeout(() => this.renderPerfChart(), 50);
    toggleAutoRefresh(): void {
            // Start auto-refresh interval
    toggleKpiCards(): void {
        this.kpiCardsCollapsed = !this.kpiCardsCollapsed;
    async refreshData(): Promise<void> {
            // Get engine registry stats
            this.engineStats = BaseEngineRegistry.Instance.GetMemoryStats();
            // Transform to display format
            this.engines = this.engineStats.engineStats.map(engine => ({
                className: engine.className,
                isLoaded: engine.isLoaded,
                registeredAt: engine.registeredAt,
                lastLoadedAt: engine.lastLoadedAt,
                estimatedMemoryBytes: engine.estimatedMemoryBytes,
                itemCount: engine.itemCount,
                memoryDisplay: this.formatBytes(engine.estimatedMemoryBytes)
            // Get redundantly loaded entities
            const redundantMap = BaseEngineRegistry.Instance.GetRedundantlyLoadedEntities();
            this.redundantLoads = Array.from(redundantMap.entries())
                .map(([entityName, engines]) => ({
                    engines
                .sort((a, b) => b.engines.length - a.engines.length);
            // Get telemetry data
            this.refreshTelemetryData();
            this.lastUpdated = new Date();
            console.error('Error refreshing diagnostics data:', error);
    private refreshTelemetryData(): void {
        const tm = TelemetryManager.Instance;
        this.telemetryEnabled = tm.IsEnabled;
        // Get summary stats
        const stats = tm.GetStats();
        this.telemetrySummary = {
            totalEvents: stats.totalEvents,
            totalPatterns: stats.totalPatterns,
            totalInsights: stats.totalInsights,
            activeEvents: stats.activeEvents,
            byCategory: stats.byCategory
        // Build categories with data for display
        const categoryNames: TelemetryCategory[] = ['RunView', 'RunQuery', 'Engine', 'AI', 'Cache'];
        this.categoriesWithData = categoryNames
            .filter(cat => stats.byCategory[cat]?.events > 0)
            .map(cat => ({
                name: cat,
                events: stats.byCategory[cat].events,
                avgMs: stats.byCategory[cat].avgMs
        // Get patterns and apply sorting
        const patterns = tm.GetPatterns({ minCount: 1, sortBy: 'count' });
        this.telemetryPatterns = this.sortPatterns(patterns.slice(0, 100).map(p => ({
            fingerprint: p.fingerprint,
            category: p.category,
            operation: p.operation,
            entityName: this.getEntityName(p.sampleParams),
            filter: this.getFilter(p.sampleParams),
            orderBy: this.getOrderBy(p.sampleParams),
            count: p.count,
            avgElapsedMs: Math.round(p.avgElapsedMs * 100) / 100,
            totalElapsedMs: Math.round(p.totalElapsedMs),
            minElapsedMs: p.minElapsedMs === Infinity ? 0 : Math.round(p.minElapsedMs),
            maxElapsedMs: Math.round(p.maxElapsedMs),
            lastSeen: new Date(p.lastSeen),
            sampleParams: p.sampleParams
        // Get all events for timeline
        const events = tm.GetEvents({ limit: 200 });
        this.telemetryEvents = events.map(e => this.eventToDisplay(e));
        // Get slow queries (operations above threshold)
        this.slowQueries = this.telemetryEvents
            .filter(e => e.elapsedMs !== undefined && e.elapsedMs >= this.slowQueryThresholdMs)
            .sort((a, b) => (b.elapsedMs || 0) - (a.elapsedMs || 0))
            .slice(0, 20);
        // Get insights and convert to display format with expansion support
        const insights = tm.GetInsights({ limit: 20 });
        this.telemetryInsights = insights.map(insight => ({
            ...insight,
            relatedEvents: this.getRelatedEventsForInsight(insight)
    private eventToDisplay(e: TelemetryEvent): TelemetryEventDisplay {
            id: e.id,
            category: e.category,
            operation: e.operation,
            entityName: this.getEntityName(e.params),
            filter: this.getFilter(e.params),
            startTime: e.startTime,
            endTime: e.endTime,
            elapsedMs: e.elapsedMs,
            timestamp: new Date(e.startTime),
            params: e.params
    private getRelatedEventsForInsight(insight: TelemetryInsight): TelemetryEventDisplay[] {
        const events = tm.GetEvents({ limit: 500 });
        return events
            .filter(e => insight.relatedEventIds?.includes(e.id))
            .map(e => this.eventToDisplay(e));
    private sortPatterns(patterns: TelemetryPatternDisplay[]): TelemetryPatternDisplay[] {
        return [...patterns].sort((a, b) => {
            switch (this.patternSort.column) {
                    comparison = a.category.localeCompare(b.category);
                case 'operation':
                    comparison = a.operation.localeCompare(b.operation);
                    comparison = (a.entityName || '').localeCompare(b.entityName || '');
                    comparison = a.count - b.count;
                case 'avgMs':
                    comparison = a.avgElapsedMs - b.avgElapsedMs;
                case 'totalMs':
                    comparison = a.totalElapsedMs - b.totalElapsedMs;
            return this.patternSort.direction === 'asc' ? comparison : -comparison;
    sortPatternsBy(column: PatternSortConfig['column']): void {
        if (this.patternSort.column === column) {
            // Toggle direction
            this.patternSort.direction = this.patternSort.direction === 'asc' ? 'desc' : 'asc';
            this.patternSort.column = column;
            this.patternSort.direction = 'desc';
        this.telemetryPatterns = this.sortPatterns(this.telemetryPatterns);
    getSortIcon(column: PatternSortConfig['column']): string {
        if (this.patternSort.column !== column) {
            return 'fa-sort';
        return this.patternSort.direction === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
    toggleInsightExpanded(insight: TelemetryInsightDisplay): void {
        insight.expanded = !insight.expanded;
    setTimelineView(view: 'insights' | 'timeline' | 'chart'): void {
        this.timelineView = view;
        if (view === 'chart') {
            // Render chart after view updates
            setTimeout(() => this.renderPerfChart(), 0);
    setPerfTab(tab: 'monitor' | 'overview' | 'events' | 'patterns' | 'insights'): void {
        this.perfTab = tab;
        if (tab === 'monitor') {
    jumpToPatternsByCategory(categoryName: string): void {
        this.perfTab = 'patterns';
        this.categoryFilter = categoryName as TelemetryCategory;
    getInsightFilter(insight: TelemetryInsightDisplay): string | null {
        // Get filter from first related event
        if (insight.relatedEvents.length > 0) {
            return insight.relatedEvents[0].filter;
    getEventParams(event: TelemetryEventDisplay): Array<{ key: string; value: string }> {
        const params: Array<{ key: string; value: string }> = [];
        if (!event.params) return params;
        // Cast to record for dynamic iteration - params have been validated by type guards
        const paramsRecord = event.params as unknown as Record<string, unknown>;
        // Show important params first
        const priorityKeys = ['EntityName', 'ViewName', 'ViewID', 'QueryName', 'ExtraFilter', 'OrderBy', 'ResultType', 'MaxRows'];
        const shownKeys = new Set<string>();
        // Add priority keys first if they exist
        for (const key of priorityKeys) {
            if (paramsRecord[key] !== undefined && paramsRecord[key] !== null) {
                const value = this.formatParamValue(paramsRecord[key]);
                    params.push({ key, value });
                    shownKeys.add(key);
        // Add remaining keys
        for (const [key, val] of Object.entries(paramsRecord)) {
            if (!shownKeys.has(key) && !key.startsWith('_')) {
                const value = this.formatParamValue(val);
    private formatParamValue(val: unknown): string {
        if (val === null || val === undefined) return '';
        if (typeof val === 'string') return val || '(empty)';
        if (typeof val === 'boolean') return val ? 'true' : 'false';
        if (typeof val === 'number') return val.toString();
        if (Array.isArray(val)) return val.join(', ');
        if (typeof val === 'object') return JSON.stringify(val);
        return String(val);
     * Check if this is a RunView/RunViews operation
    isRunViewOperation(event: TelemetryEventDisplay): boolean {
        return event.operation === 'ProviderBase.RunView' || event.operation === 'ProviderBase.RunViews';
     * Check if this is a batch RunViews operation
    isRunViewsOperation(event: TelemetryEventDisplay): boolean {
        return event.operation === 'ProviderBase.RunViews';
     * Get entity names for RunViews batch operation (first few for display)
    getRunViewsEntities(event: TelemetryEventDisplay, maxDisplay: number = 3): string[] {
        if (!event.params || !isBatchRunViewParams(event.params)) return [];
        const entities = event.params.Entities;
        if (!entities || !Array.isArray(entities)) return [];
        return entities.slice(0, maxDisplay);
     * Get total entity count for RunViews batch operation
    getRunViewsEntityCount(event: TelemetryEventDisplay): number {
        if (!event.params || !isBatchRunViewParams(event.params)) return 0;
        return event.params.Entities?.length || 0;
     * Check if there are more entities than displayed
    hasMoreEntities(event: TelemetryEventDisplay, maxDisplay: number = 3): boolean {
        return this.getRunViewsEntityCount(event) > maxDisplay;
     * Check if the event was a cache hit (safe accessor for union type params)
    isCacheHit(event: TelemetryEventDisplay | { params: TelemetryParamsUnion }): boolean {
        if (!event?.params) return false;
        // Use isSingleRunViewParams or isSingleRunQueryParams to safely access cacheHit
        if (isSingleRunViewParams(event.params)) {
            return event.params.cacheHit === true;
        if (isSingleRunQueryParams(event.params)) {
        // Handle batch RunViews operations - check cacheHits/cacheMisses counts
        if (isBatchRunViewParams(event.params)) {
            const p = event.params as { cacheHits?: number; cacheMisses?: number };
            // Consider it a cache hit if all items were served from cache
            return (p.cacheHits ?? 0) > 0 && (p.cacheMisses ?? 0) === 0;
     * Get entity name from telemetry params (safe accessor for union type)
    getEntityName(params: TelemetryParamsUnion | undefined): string | null {
        if (!params) return null;
        if (isSingleRunViewParams(params)) {
            return params.EntityName || null;
        if (isSingleRunQueryParams(params)) {
            return params.QueryName || null;
     * Get filter from telemetry params (safe accessor for union type)
    getFilter(params: TelemetryParamsUnion | undefined): string | null {
            return params.ExtraFilter || null;
     * Get order by from telemetry params (safe accessor for union type)
    getOrderBy(params: TelemetryParamsUnion | undefined): string | null {
            return params.OrderBy || null;
     * Get RunView parameter pills for display
    getRunViewPills(event: TelemetryEventDisplay): Array<{ label: string; value: string; type: 'filter' | 'order' | 'result' | 'limit' | 'batch' | 'info' }> {
        const pills: Array<{ label: string; value: string; type: 'filter' | 'order' | 'result' | 'limit' | 'batch' | 'info' }> = [];
        // For batch operations, show batch size
        if (this.isRunViewsOperation(event) && event.params && isBatchRunViewParams(event.params)) {
            const batchSize = event.params.BatchSize;
            if (batchSize) {
                pills.push({ label: 'Batch', value: String(batchSize), type: 'batch' });
        // For single RunView, show params
        if (!this.isRunViewsOperation(event) && event.params && isSingleRunViewParams(event.params)) {
            const extraFilter = event.params.ExtraFilter;
            if (extraFilter) {
                pills.push({ label: 'Filter', value: this.truncateString(extraFilter, 25), type: 'filter' });
            const orderBy = event.params.OrderBy;
                pills.push({ label: 'Order', value: this.truncateString(orderBy, 20), type: 'order' });
            const resultType = event.params.ResultType;
            if (resultType && resultType !== 'simple') {
                pills.push({ label: 'Type', value: resultType, type: 'result' });
            const maxRows = event.params.MaxRows;
            if (maxRows && maxRows > 0) {
                pills.push({ label: 'Limit', value: String(maxRows), type: 'limit' });
        return pills;
    // === Event Detail Panel Methods ===
    openEventDetailPanel(event: TelemetryEventDisplay): void {
        // Find related pattern
        const relatedPattern = this.telemetryPatterns.find(p =>
            p.category === event.category &&
            p.operation === event.operation &&
            p.entityName === event.entityName
        this.eventDetailPanel = {
            isOpen: true,
            relatedPattern
    closeEventDetailPanel(): void {
    copyEventToClipboard(event: TelemetryEventDisplay): void {
            category: event.category,
            operation: event.operation,
            entityName: event.entityName,
            filter: event.filter,
            startTime: event.startTime,
            endTime: event.endTime,
            elapsedMs: event.elapsedMs,
            timestamp: event.timestamp.toISOString(),
            params: event.params
        navigator.clipboard.writeText(JSON.stringify(eventData, null, 2))
            .then(() => {
                console.log('Event copied to clipboard');
            .catch(err => {
                console.error('Failed to copy event:', err);
    filterByEntity(entityName: string | null): void {
        if (!entityName) return;
        this.closeEventDetailPanel();
        this.searchQuery = entityName;
    exportTelemetryData(): void {
            bootTime: this.telemetryBootTime,
            summary: this.telemetrySummary,
            events: this.telemetryEvents.map(e => ({
                entityName: e.entityName,
                filter: e.filter,
                timestamp: e.timestamp.toISOString(),
            patterns: this.telemetryPatterns.map(p => ({
                entityName: p.entityName,
                filter: p.filter,
                avgElapsedMs: p.avgElapsedMs,
                totalElapsedMs: p.totalElapsedMs,
                minElapsedMs: p.minElapsedMs,
                maxElapsedMs: p.maxElapsedMs,
                lastSeen: p.lastSeen.toISOString()
            insights: this.telemetryInsights.map(i => ({
                id: i.id,
                category: i.category,
                severity: i.severity,
                title: i.title,
                message: i.message,
                suggestion: i.suggestion,
                entityName: i.entityName,
                timestamp: typeof i.timestamp === 'number' ? new Date(i.timestamp).toISOString() : i.timestamp
        link.download = `telemetry-export-${new Date().toISOString().slice(0, 19).replace(/:/g, '-')}.json`;
    setCategoryFilter(category: TelemetryCategory | 'all'): void {
    setCategoryFilterByName(name: string): void {
        // Cast string to TelemetryCategory since we know it comes from categoriesWithData
        this.categoryFilter = name as TelemetryCategory;
    onSearchChange(): void {
        // Debounce URL update for search to avoid too many history changes
        this.updateQueryParamsDebounced();
    clearSearch(): void {
        this.searchQuery = '';
        this.updateQueryParams();
    private searchParamsTimeout: ReturnType<typeof setTimeout> | null = null;
    private updateQueryParamsDebounced(): void {
        if (this.searchParamsTimeout) {
            clearTimeout(this.searchParamsTimeout);
        this.searchParamsTimeout = setTimeout(() => {
    get filteredPatterns(): TelemetryPatternDisplay[] {
        let patterns = this.telemetryPatterns;
        if (this.categoryFilter !== 'all') {
            patterns = patterns.filter(p => p.category === this.categoryFilter);
        if (this.searchQuery.trim()) {
            const query = this.searchQuery.toLowerCase();
            patterns = patterns.filter(p =>
                p.entityName?.toLowerCase().includes(query) ||
                p.operation.toLowerCase().includes(query) ||
                p.filter?.toLowerCase().includes(query) ||
                p.category.toLowerCase().includes(query)
    get filteredEvents(): TelemetryEventDisplay[] {
        let events = this.telemetryEvents;
            events = events.filter(e => e.category === this.categoryFilter);
            events = events.filter(e =>
                e.entityName?.toLowerCase().includes(query) ||
                e.operation.toLowerCase().includes(query) ||
                e.filter?.toLowerCase().includes(query) ||
                e.category.toLowerCase().includes(query)
        return events;
    formatTimestamp(date: Date): string {
        return date.toLocaleTimeString('en-US', {
            second: '2-digit',
            fractionalSecondDigits: 3
    truncateString(str: string | null, maxLength: number): string {
        if (!str) return '-';
        if (str.length <= maxLength) return str;
        return str.substring(0, maxLength) + '...';
    toggleTelemetry(): void {
        tm.SetEnabled(!tm.IsEnabled);
    clearTelemetry(): void {
        // Only client telemetry can be cleared (server telemetry is read-only)
        TelemetryManager.Instance.Clear();
        TelemetryManager.Instance.ClearInsights();
     * Switch between client and server telemetry sources
    setTelemetrySource(source: 'client' | 'server'): void {
        if (this.telemetrySource === source) return;
        this.telemetrySource = source;
        this.serverTelemetryError = null;
        if (source === 'server') {
            this.loadServerTelemetry();
     * Load telemetry data from the server via GraphQL
    private async loadServerTelemetry(): Promise<void> {
        this.serverTelemetryLoading = true;
            const gqlProvider = GraphQLDataProvider.Instance;
            // Query for server telemetry settings (read-only, configured via mj.config.cjs)
            const settingsQuery = `
                query GetServerTelemetrySettings {
                    GetServerTelemetrySettings {
                        enabled
                        level
            // Query for server telemetry stats
            const statsQuery = `
                query GetServerTelemetryStats {
                    GetServerTelemetryStats {
                        totalEvents
                        totalPatterns
                        totalInsights
                        activeEvents
                        byCategory {
                            category
                            events
                            avgMs
            // Query for server telemetry events
            const eventsQuery = `
                query GetServerTelemetryEvents($filter: TelemetryEventFilterInput) {
                    GetServerTelemetryEvents(filter: $filter) {
                        operation
                        fingerprint
                        elapsedMs
                        userId
                        tags
                        parentEventId
            // Query for server telemetry patterns
            const patternsQuery = `
                query GetServerTelemetryPatterns($filter: TelemetryPatternFilterInput) {
                    GetServerTelemetryPatterns(filter: $filter) {
                        sampleParams
                        totalElapsedMs
                        avgElapsedMs
                        minElapsedMs
                        maxElapsedMs
                        firstSeen
                        lastSeen
            // Query for server telemetry insights
            const insightsQuery = `
                query GetServerTelemetryInsights($filter: TelemetryInsightFilterInput) {
                    GetServerTelemetryInsights(filter: $filter) {
                        severity
                        analyzerName
                        message
                        suggestion
                        relatedEventIds
                        entityName
                        timestamp
            // Execute queries in parallel
            const [settingsResult, statsResult, eventsResult, patternsResult, insightsResult] = await Promise.all([
                gqlProvider.ExecuteGQL(settingsQuery, {}),
                gqlProvider.ExecuteGQL(statsQuery, {}),
                gqlProvider.ExecuteGQL(eventsQuery, { filter: { limit: 200 } }),
                gqlProvider.ExecuteGQL(patternsQuery, { filter: { minCount: 1 } }),
                gqlProvider.ExecuteGQL(insightsQuery, { filter: { limit: 20 } })
            // Process settings (read-only status from server config)
            if (settingsResult?.GetServerTelemetrySettings) {
                this.serverTelemetryEnabled = settingsResult.GetServerTelemetrySettings.enabled;
            // Process stats
            if (statsResult?.GetServerTelemetryStats) {
                const stats = statsResult.GetServerTelemetryStats;
                const byCategory: Record<TelemetryCategory, { events: number; avgMs: number }> = {
                    'RunView': { events: 0, avgMs: 0 },
                    'RunQuery': { events: 0, avgMs: 0 },
                    'Engine': { events: 0, avgMs: 0 },
                    'AI': { events: 0, avgMs: 0 },
                    'Cache': { events: 0, avgMs: 0 },
                    'Network': { events: 0, avgMs: 0 },
                    'Custom': { events: 0, avgMs: 0 }
                for (const cat of stats.byCategory || []) {
                    if (cat.category in byCategory) {
                        byCategory[cat.category as TelemetryCategory] = {
                            events: cat.events,
                            avgMs: cat.avgMs
                    byCategory
                // Build categories with data
                    .filter(cat => byCategory[cat]?.events > 0)
                        events: byCategory[cat].events,
                        avgMs: byCategory[cat].avgMs
            // Process events
            if (eventsResult?.GetServerTelemetryEvents) {
                this.telemetryEvents = eventsResult.GetServerTelemetryEvents.map((e: {
                    endTime?: number;
                    elapsedMs?: number;
                    params: string;
                }) => {
                    const params = e.params ? JSON.parse(e.params) : {};
                        category: e.category as TelemetryCategory,
                        entityName: params?.EntityName || params?.QueryName || null,
                        filter: params?.ExtraFilter || null,
                // Update slow queries
            // Process patterns
            if (patternsResult?.GetServerTelemetryPatterns) {
                this.telemetryPatterns = patternsResult.GetServerTelemetryPatterns.slice(0, 100).map((p: {
                    sampleParams: string;
                    lastSeen: number;
                    const sampleParams = p.sampleParams ? JSON.parse(p.sampleParams) : {};
                        category: p.category as TelemetryCategory,
                        entityName: sampleParams?.EntityName || sampleParams?.QueryName || null,
                        filter: sampleParams?.ExtraFilter || null,
                        orderBy: sampleParams?.OrderBy || null,
                        minElapsedMs: Math.round(p.minElapsedMs),
            // Process insights
            if (insightsResult?.GetServerTelemetryInsights) {
                this.telemetryInsights = insightsResult.GetServerTelemetryInsights.map((i: {
                    analyzerName: string;
                    suggestion: string;
                    relatedEventIds: string[];
                    metadata?: string;
                }) => ({
                    analyzerName: i.analyzerName,
                    relatedEventIds: i.relatedEventIds || [],
                    metadata: i.metadata ? JSON.parse(i.metadata) : undefined,
                    timestamp: i.timestamp,
                    relatedEvents: []
            this.telemetryEnabled = true; // Server telemetry is available
            console.error('Failed to load server telemetry:', error);
            this.serverTelemetryError = `Failed to load server telemetry: ${error instanceof Error ? error.message : String(error)}`;
            // Clear data on error
            this.telemetrySummary = null;
            this.telemetryEvents = [];
            this.telemetryPatterns = [];
            this.telemetryInsights = [];
            this.slowQueries = [];
            this.serverTelemetryLoading = false;
    getSeverityClass(severity: string): string {
        switch (severity) {
            case 'info': return 'severity-info';
            case 'warning': return 'severity-warning';
            case 'optimization': return 'severity-optimization';
    getSeverityIcon(severity: string): string {
            case 'info': return 'fa-info-circle';
            case 'warning': return 'fa-exclamation-triangle';
            case 'optimization': return 'fa-lightbulb';
            default: return 'fa-circle';
    async refreshAllEngines(): Promise<void> {
        this.isRefreshingEngines = true;
            const count = await BaseEngineRegistry.Instance.RefreshAllEngines();
            console.log(`Refreshed ${count} engines`);
            console.error('Error refreshing engines:', error);
            this.isRefreshingEngines = false;
    formatBytes(bytes: number): string {
        if (bytes === 0) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    formatTime(date: Date): string {
        return date.toLocaleTimeString();
    formatRelativeTime(ms: number): string {
            return `${ms.toFixed(0)}ms`;
            const secs = ((ms % 60000) / 1000).toFixed(0);
     * Renders a Windows PerfMon-style D3 time series chart
     * Shows performance events over time with duration spikes
    renderPerfChart(): void {
        if (!this.perfChartRef?.nativeElement) {
        const container = this.perfChartRef.nativeElement;
        const events = this.telemetryEvents.filter(e => e.elapsedMs !== undefined);
        if (events.length === 0) {
            container.innerHTML = '<div style="color: #666; text-align: center; padding: 100px 20px;">No telemetry events with timing data yet.<br>Navigate around the app to generate performance data.</div>';
        container.innerHTML = '';
        // Get dimensions
        const width = rect.width || 800;
        const height = rect.height || 300;
        const margin = { top: 20, right: 30, bottom: 40, left: 50 };
        // Calculate boot time (earliest event)
        this.telemetryBootTime = Math.min(...events.map(e => e.startTime));
        // Prepare data with relative time
        const allChartData = events.map(e => ({
            relativeTime: e.startTime - this.telemetryBootTime,
            duration: e.elapsedMs || 0
        })).sort((a, b) => a.relativeTime - b.relativeTime);
        // Calculate full time range
        const fullTimeRange = d3.max(allChartData, d => d.relativeTime) || 1000;
        // Determine viewport bounds
        let viewportStart = this.chartViewportStart;
        let viewportEnd = this.chartViewportEnd;
        // If no viewport set or zoom level is 1, show everything
        if (this.chartZoomLevel <= 1 || (viewportStart === 0 && viewportEnd === 0)) {
            viewportStart = 0;
            viewportEnd = fullTimeRange;
        // Filter data to only include events within the viewport (with some padding for edge visibility)
        const padding = (viewportEnd - viewportStart) * 0.05; // 5% padding
        const chartData = allChartData.filter(d =>
            d.relativeTime >= (viewportStart - padding) &&
            d.relativeTime <= (viewportEnd + padding)
        // If no data in viewport, show a message
        if (chartData.length === 0) {
            container.innerHTML = '<div style="color: #666; text-align: center; padding: 100px 20px;">No events in the current view.<br>Try zooming out or panning to see events.</div>';
        // Calculate effective width with zoom
        const effectiveWidth = innerWidth * this.chartZoomLevel;
        // Create SVG with potential scroll for zoomed view
            .attr('width', Math.max(width, effectiveWidth + margin.left + margin.right))
        const g = svg.append('g')
        // Handle gap compression
        let xScale: d3.ScaleLinear<number, number>;
        let gapSegments: Array<{ type: 'events' | 'gap'; startTime: number; endTime: number; gapIndex?: number; displayStart: number; displayEnd: number }> = [];
        if (this.chartGapCompression && chartData.length > 1) {
            // Identify gaps and create compressed scale
            const segments = this.identifyGaps(chartData, 5000); // 5 second threshold
            const compressedGapWidth = 30; // Fixed width for compressed gaps
            // Calculate total display width needed
            let currentX = 0;
            for (const segment of segments) {
                const segmentDuration = segment.endTime - segment.startTime;
                if (segment.type === 'gap') {
                    const isExpanded = segment.gapIndex !== undefined && this.expandedGaps.has(segment.gapIndex);
                    const gapWidth = isExpanded ? (segmentDuration / (d3.max(chartData, d => d.relativeTime) || 1000)) * effectiveWidth : compressedGapWidth;
                    gapSegments.push({
                        ...segment,
                        displayStart: currentX,
                        displayEnd: currentX + gapWidth
                    currentX += gapWidth;
                    // Event segments get proportional width
                    const proportionalWidth = (segmentDuration / (d3.max(chartData, d => d.relativeTime) || 1000)) * effectiveWidth;
                    const segmentWidth = Math.max(proportionalWidth, 50); // Minimum width for visibility
                        displayEnd: currentX + segmentWidth
                    currentX += segmentWidth;
            // Create custom scale function based on segments
            const totalDisplayWidth = currentX;
            const baseScale = d3.scaleLinear()
                .domain([0, d3.max(chartData, d => d.relativeTime) || 1000])
                .range([0, Math.min(totalDisplayWidth, effectiveWidth)]);
            // Store gap segments for inverse mapping
            this.chartGapSegments = gapSegments;
            // Create a custom mapping function for segment-based positioning
            const mapTimeToX = (time: number): number => {
                // Find which segment this time falls into
                for (const seg of gapSegments) {
                    if (time >= seg.startTime && time <= seg.endTime) {
                        if (seg.type === 'gap') {
                            const isExpanded = seg.gapIndex !== undefined && this.expandedGaps.has(seg.gapIndex);
                                // Linear interpolation within expanded gap
                                const ratio = (time - seg.startTime) / (seg.endTime - seg.startTime);
                                return seg.displayStart + ratio * (seg.displayEnd - seg.displayStart);
                            // Compressed gap - map to center
                            return seg.displayStart + (seg.displayEnd - seg.displayStart) / 2;
                            // Event segment - linear interpolation
                            const ratio = (time - seg.startTime) / Math.max(seg.endTime - seg.startTime, 1);
                return baseScale(time);
            // Create inverse mapping function for x -> time (used by selection brush)
            const mapXToTime = (x: number): number => {
                // Find which segment this x position falls into
                    if (x >= seg.displayStart && x <= seg.displayEnd) {
                                const displayRange = seg.displayEnd - seg.displayStart;
                                if (displayRange <= 0) return seg.startTime;
                                const ratio = (x - seg.displayStart) / displayRange;
                                return seg.startTime + ratio * (seg.endTime - seg.startTime);
                            // Compressed gap - return start time (the gap itself has no meaningful selection)
                            return seg.startTime;
                return baseScale.invert(x);
            // Use base scale but override the call behavior via a proxy-like wrapper
            xScale = Object.assign(
                (time: number) => mapTimeToX(time),
                    domain: baseScale.domain.bind(baseScale),
                    range: () => [0, Math.min(totalDisplayWidth, effectiveWidth)] as [number, number],
                    ticks: baseScale.ticks.bind(baseScale),
                    tickFormat: baseScale.tickFormat.bind(baseScale),
                    nice: baseScale.nice.bind(baseScale),
                    copy: baseScale.copy.bind(baseScale),
                    invert: mapXToTime, // Use our custom inverse function!
                    clamp: baseScale.clamp.bind(baseScale),
                    unknown: baseScale.unknown.bind(baseScale),
                    interpolate: baseScale.interpolate.bind(baseScale),
                    rangeRound: baseScale.rangeRound.bind(baseScale)
            ) as unknown as d3.ScaleLinear<number, number>;
            // Draw gap indicators
                if (seg.type === 'gap' && seg.gapIndex !== undefined) {
                    const isExpanded = this.expandedGaps.has(seg.gapIndex);
                    if (!isExpanded) {
                        this.drawGapIndicator(
                            g,
                            seg.displayStart,
                            seg.displayEnd - seg.displayStart,
                            innerHeight,
                            seg.endTime - seg.startTime,
                            seg.gapIndex
            // Standard linear scale - clear gap segments
            this.chartGapSegments = [];
            // Use viewport bounds for domain when zoomed
            xScale = d3.scaleLinear()
                .domain([viewportStart, viewportEnd])
                .range([0, innerWidth]); // Use innerWidth, not effectiveWidth for proper fit
        // Calculate Y-scale from VISIBLE data only (not all data)
        const visibleMaxDuration = d3.max(chartData, d => d.duration) || 100;
        const yScale = d3.scaleLinear()
            .domain([0, visibleMaxDuration * 1.1]) // Add 10% padding at top
            .range([innerHeight, 0])
        this.drawGridLines(g, xScale, yScale, effectiveWidth, innerHeight);
        this.drawAxes(g, xScale, yScale, innerHeight);
        // Color mapping for categories
        const categoryColors: Record<string, string> = {
            'RunView': '#00bcd4',
            'RunQuery': '#e040fb',
            'Engine': '#00ff88',
            'AI': '#ff9800',
            'Cache': '#f06292',
            'Network': '#26a69a',
            'Custom': '#78909c'
        // Draw area fill for each category (like PerfMon background)
        const categories = [...new Set(chartData.map(d => d.category))];
            const categoryData = chartData.filter(d => d.category === category);
            if (categoryData.length > 1) {
                this.drawCategoryArea(g, categoryData, xScale, yScale, innerHeight, categoryColors[category] || '#78909c');
        // Draw event points
        this.drawEventPoints(g, chartData, xScale, yScale, categoryColors, container);
        // Draw threshold line for slow queries
        this.drawThresholdLine(g, yScale, effectiveWidth, this.slowQueryThresholdMs);
        // Add selection brush for drag-to-zoom and pan
        this.addSelectionBrush(svg, g, xScale, innerHeight, margin, allChartData, fullTimeRange);
        // Store scale and dimensions for selection calculations
        this.chartXScale = xScale;
        this.chartMarginLeft = margin.left;
        this.chartInitialized = true;
    private drawGridLines(
        xScale: d3.ScaleLinear<number, number>,
        yScale: d3.ScaleLinear<number, number>,
        // Horizontal grid lines
        g.selectAll('.grid-line-h')
            .data(yTicks)
            .append('line')
            .attr('class', 'grid-line')
            .attr('x2', width)
            .attr('y1', d => yScale(d))
            .attr('y2', d => yScale(d))
            .attr('stroke-dasharray', '2,2');
        const xTicks = xScale.ticks(10);
        g.selectAll('.grid-line-v')
            .data(xTicks)
            .attr('x1', d => xScale(d))
            .attr('x2', d => xScale(d))
            .attr('y2', height)
    private drawAxes(
        // X axis with better time formatting
        const xDomain = xScale.domain();
        const timeRange = xDomain[1] - xDomain[0];
        // Choose appropriate number of ticks based on range
        const numTicks = Math.min(10, Math.max(5, Math.floor(timeRange / 1000)));
        const xAxis = d3.axisBottom(xScale)
            .ticks(numTicks)
            .tickFormat(d => this.formatAxisTime(d as number));
        const xAxisGroup = g.append('g')
            .call(xAxis)
            .attr('class', 'axis-line');
        xAxisGroup.selectAll('text')
            .attr('class', 'axis-text')
            .attr('fill', '#888')
            .attr('font-size', '11px');
        // X axis label
        xAxisGroup.append('text')
            .attr('x', xScale.range()[1] / 2)
            .attr('y', 35)
            .text('Time since process start');
        // Y axis with proper duration formatting
        const yMax = yScale.domain()[1];
        const yAxis = d3.axisLeft(yScale)
            .ticks(6)
            .tickFormat(d => this.formatAxisDuration(d as number, yMax));
        const yAxisGroup = g.append('g')
            .call(yAxis)
        yAxisGroup.selectAll('text')
        // Y axis label
        yAxisGroup.append('text')
            .attr('x', -height / 2)
            .text('Duration');
     * Format time for axis labels - shows relative time since process start
    private formatAxisTime(ms: number): string {
            return `${Math.round(ms)}ms`;
            const secs = ms / 1000;
            return secs % 1 === 0 ? `${secs}s` : `${secs.toFixed(1)}s`;
        } else if (ms < 3600000) {
            return secs > 0 ? `${mins}m${secs}s` : `${mins}m`;
            return mins > 0 ? `${hours}h${mins}m` : `${hours}h`;
     * Format duration for Y axis labels
    private formatAxisDuration(ms: number, maxValue: number): string {
        // If max is >= 1000ms, show in seconds for values >= 1000
        if (maxValue >= 1000 && ms >= 1000) {
    private drawCategoryArea(
        data: Array<{ relativeTime: number; duration: number }>,
        color: string
        const area = d3.area<{ relativeTime: number; duration: number }>()
            .x(d => xScale(d.relativeTime))
            .y0(height)
            .y1(d => yScale(d.duration))
        g.append('path')
            .datum(data)
            .attr('class', 'area-fill')
            .attr('fill', color)
            .attr('opacity', 0.1);
        // Line on top
        const line = d3.line<{ relativeTime: number; duration: number }>()
            .y(d => yScale(d.duration))
            .attr('class', 'line-path')
            .attr('stroke-width', 1.5)
            .attr('opacity', 0.6);
    private drawEventPoints(
        data: Array<TelemetryEventDisplay & { relativeTime: number; duration: number }>,
        categoryColors: Record<string, string>,
        container: HTMLDivElement
        const tooltip = g.append('g')
            .attr('class', 'tooltip')
        tooltip.append('rect')
            .attr('class', 'tooltip-bg')
            .attr('fill', 'rgba(0, 0, 0, 0.9)')
            .attr('rx', 4);
        const tooltipText = tooltip.append('text')
            .attr('class', 'tooltip-text')
            .attr('fill', '#fff')
            .attr('font-family', 'monospace');
        // Split data into cached and non-cached events
        const nonCachedData = data.filter(d => !this.isCacheHit(d));
        const cachedData = data.filter(d => this.isCacheHit(d));
        // Helper to show tooltip
        const showTooltip = (event: MouseEvent, d: TelemetryEventDisplay & { relativeTime: number; duration: number }) => {
            // Update tooltip content
            const isCached = this.isCacheHit(d);
                `${d.category}: ${d.operation}`,
                d.entityName ? `Entity: ${d.entityName}` : null,
                `Duration: ${d.duration.toFixed(0)}ms`,
                isCached ? '⚡ CACHED' : null,
                `Time: +${this.formatRelativeTime(d.relativeTime)}`
            ].filter(Boolean);
            tooltipText.selectAll('tspan').remove();
            lines.forEach((line, i) => {
                tooltipText.append('tspan')
                    .attr('x', 8)
                    .attr('dy', i === 0 ? '1.2em' : '1.4em')
                    .text(line as string);
            // Size tooltip background
            const textBBox = (tooltipText.node() as SVGTextElement).getBBox();
            tooltip.select('.tooltip-bg')
                .attr('width', textBBox.width + 16)
                .attr('height', textBBox.height + 12)
                .attr('y', textBBox.y - 6);
            // Position tooltip
            const x = xScale(d.relativeTime);
            const y = yScale(d.duration);
            const tooltipWidth = textBBox.width + 16;
            // Flip tooltip if too close to right edge
            const translateX = x + tooltipWidth + 20 > (container.clientWidth - 80) ? x - tooltipWidth - 10 : x + 10;
            tooltip.attr('transform', `translate(${translateX},${y - 20})`);
            tooltip.style('display', 'block');
        const hideTooltip = () => {
            tooltip.style('display', 'none');
        // Draw circles for non-cached events
        g.selectAll('.event-point-circle')
            .data(nonCachedData)
            .append('circle')
            .attr('class', 'event-point event-point-circle')
            .attr('cx', d => xScale(d.relativeTime))
            .attr('cy', d => yScale(d.duration))
            .attr('r', d => d.duration >= this.slowQueryThresholdMs ? 5 : 3)
            .attr('fill', d => categoryColors[d.category] || '#78909c')
            .attr('stroke', d => d.duration >= this.slowQueryThresholdMs ? '#ff5252' : 'none')
            .on('mouseenter', (event: MouseEvent, d) => {
                const target = event.target as SVGCircleElement;
                d3.select(target).attr('r', 7);
                showTooltip(event, d);
            .on('mouseleave', (event: MouseEvent, d) => {
                d3.select(target).attr('r', d.duration >= this.slowQueryThresholdMs ? 5 : 3);
                hideTooltip();
            .on('click', (_event: MouseEvent, d) => {
                // Open detail panel for this event
                    this.openEventDetailPanel(d);
        // Draw bolt symbols for cached events
        // SVG path for a lightning bolt shape
        const boltPath = 'M-3,-6 L1,-6 L0,-1 L4,-1 L-2,6 L0,1 L-4,1 Z';
        g.selectAll('.event-point-bolt')
            .data(cachedData)
            .append('path')
            .attr('class', 'event-point event-point-bolt')
            .attr('d', boltPath)
            .attr('transform', d => `translate(${xScale(d.relativeTime)},${yScale(d.duration)}) scale(${d.duration >= this.slowQueryThresholdMs ? 1.3 : 1})`)
            .attr('fill', '#f59e0b')
            .attr('stroke', d => d.duration >= this.slowQueryThresholdMs ? '#ff5252' : categoryColors[d.category] || '#78909c')
            .style('filter', 'drop-shadow(0 0 2px rgba(245, 158, 11, 0.5))')
                const target = event.target as SVGPathElement;
                d3.select(target)
                    .attr('transform', `translate(${xScale(d.relativeTime)},${yScale(d.duration)}) scale(1.5)`)
                    .style('filter', 'drop-shadow(0 0 4px rgba(245, 158, 11, 0.8))');
                    .attr('transform', `translate(${xScale(d.relativeTime)},${yScale(d.duration)}) scale(${d.duration >= this.slowQueryThresholdMs ? 1.3 : 1})`)
                    .style('filter', 'drop-shadow(0 0 2px rgba(245, 158, 11, 0.5))');
    private drawThresholdLine(
        threshold: number
        const yPos = yScale(threshold);
        // Only draw if threshold is within visible range
        if (yPos > 0 && yPos < yScale.range()[0]) {
            g.append('line')
                .attr('y1', yPos)
                .attr('y2', yPos)
                .attr('stroke', '#ff5252')
                .attr('stroke-dasharray', '5,3')
                .attr('opacity', 0.7);
                .attr('x', width - 5)
                .attr('y', yPos - 5)
                .attr('fill', '#ff5252')
                .text(`Slow (>${threshold}ms)`);
    // === Chart Zoom and Gap Compression Methods ===
     * Zoom the chart in or out
    zoomPerfChart(direction: 'in' | 'out'): void {
        const zoomFactor = 1.5;
        if (direction === 'in') {
            this.chartZoomLevel = Math.min(this.chartZoomLevel * zoomFactor, 100); // Allow up to 100x zoom
            this.chartZoomLevel = Math.max(this.chartZoomLevel / zoomFactor, 0.25); // Allow zoom out to 25%
        this.renderPerfChart();
     * Reset chart zoom to default
    resetPerfChartZoom(): void {
        this.chartZoomLevel = 1;
        this.chartViewportStart = 0;
        this.chartViewportEnd = 0;
        this.chartTimeRangeStart = null;
        this.chartTimeRangeEnd = null;
        this.expandedGaps.clear();
     * Handle gap compression toggle
    onGapCompressionChange(): void {
     * Set chart interaction mode (select for drag-to-zoom, pan for click-to-view)
    setChartInteractionMode(mode: 'pointer' | 'select' | 'pan'): void {
        this.chartInteractionMode = mode;
        this.renderPerfChart(); // Re-render to update cursor and behavior
     * Returns the appropriate cursor style based on the current chart interaction mode
    private getChartCursor(): string {
        switch (this.chartInteractionMode) {
            case 'pointer':
                return 'default';
            case 'select':
                return 'crosshair';
            case 'pan':
                return 'grab';
     * Returns whether the overlay should intercept pointer events based on the current mode
    private getOverlayPointerEvents(): string {
        // In pointer mode, let events pass through to the data points
        // In select/pan mode, the overlay needs to capture events
        return this.chartInteractionMode === 'pointer' ? 'none' : 'all';
     * Identifies gaps in the data where there's no activity
     * Returns segments with their type (events or gap)
    private identifyGaps(
        events: Array<{ relativeTime: number; duration: number }>,
        gapThresholdMs: number = 5000
    ): Array<{ type: 'events' | 'gap'; startTime: number; endTime: number; gapIndex?: number; events?: typeof events }> {
        if (events.length === 0) return [];
        const segments: Array<{ type: 'events' | 'gap'; startTime: number; endTime: number; gapIndex?: number; events?: typeof events }> = [];
        let gapIndex = 0;
        // Sort events by time
        const sortedEvents = [...events].sort((a, b) => a.relativeTime - b.relativeTime);
        let currentSegmentStart = sortedEvents[0].relativeTime;
        let currentSegmentEvents: typeof events = [];
        let lastEventTime = sortedEvents[0].relativeTime;
        for (let i = 0; i < sortedEvents.length; i++) {
            const event = sortedEvents[i];
            const timeSinceLastEvent = event.relativeTime - lastEventTime;
            if (timeSinceLastEvent > gapThresholdMs && currentSegmentEvents.length > 0) {
                // Close current event segment
                segments.push({
                    type: 'events',
                    startTime: currentSegmentStart,
                    endTime: lastEventTime,
                    events: currentSegmentEvents
                // Add gap segment
                    type: 'gap',
                    startTime: lastEventTime,
                    endTime: event.relativeTime,
                    gapIndex: gapIndex++
                // Start new segment
                currentSegmentStart = event.relativeTime;
                currentSegmentEvents = [event];
                currentSegmentEvents.push(event);
            lastEventTime = event.relativeTime;
        // Close final segment
        if (currentSegmentEvents.length > 0) {
        return segments;
     * Draws a compressed gap indicator
    private drawGapIndicator(
        gapDurationMs: number,
        gapIndex: number
        const isExpanded = this.expandedGaps.has(gapIndex);
        // Draw striped background
        const pattern = g.append('defs')
            .append('pattern')
            .attr('id', `gap-pattern-${gapIndex}`)
            .attr('patternUnits', 'userSpaceOnUse')
            .attr('width', 8)
            .attr('height', 8)
            .attr('patternTransform', 'rotate(45)');
        pattern.append('rect')
            .attr('fill', '#f5f5f5');
        pattern.append('line')
            .attr('x2', 0)
            .attr('y2', 8)
            .attr('stroke', '#e0e0e0')
            .attr('stroke-width', 4);
        // Gap rectangle
        const gapRect = g.append('rect')
            .attr('fill', `url(#gap-pattern-${gapIndex})`)
            .attr('stroke', '#ccc')
            .attr('stroke-dasharray', '4,2')
        // Vertical text showing gap duration
        const gapText = this.formatRelativeTime(gapDurationMs);
        const textG = g.append('g')
            .attr('transform', `translate(${x + width / 2}, ${height / 2})`)
            .style('pointer-events', 'none');
        textG.append('text')
            .text(`${gapText} gap`);
        // Click handler to expand/collapse
        gapRect.on('click', () => {
                if (this.expandedGaps.has(gapIndex)) {
                    this.expandedGaps.delete(gapIndex);
                    this.expandedGaps.add(gapIndex);
     * Adds a drag-to-select brush overlay for zooming into a time range
     * Only active when chartInteractionMode is 'select'
    private addSelectionBrush(
        svg: d3.Selection<SVGSVGElement, unknown, null, undefined>,
        innerHeight: number,
        _margin: { top: number; right: number; bottom: number; left: number },
        allChartData: Array<{ relativeTime: number; duration: number }>,
        fullTimeRange: number
        // Create a transparent overlay for mouse events
        // Cursor and pointer-events depend on interaction mode
        const overlay = g.append('rect')
            .attr('class', 'selection-overlay')
            .attr('width', xScale.range()[1])
            .attr('height', innerHeight)
            .attr('fill', 'transparent')
            .style('cursor', this.getChartCursor())
            .style('pointer-events', this.getOverlayPointerEvents());
        // Selection rectangle (initially hidden) - only used in select mode
        const selectionRect = g.append('rect')
            .attr('class', 'selection-rect')
            .attr('fill', 'rgba(0, 255, 136, 0.15)')
            .attr('stroke', '#00ff88')
        let startX = 0;
        let isDragging = false;
        let isPanning = false;
        let panStartX = 0;
        let panStartViewportStart = 0;
        // Store the inverse scale function for mapping x back to time
        const getTimeFromX = (x: number): number => {
            // For gap-compressed scales, we need to use invert if available
            if (typeof xScale.invert === 'function') {
                return xScale.invert(x);
            // Fallback: linear interpolation
            const domain = xScale.domain();
            const range = xScale.range();
            const ratio = (x - range[0]) / (range[1] - range[0]);
            return domain[0] + ratio * (domain[1] - domain[0]);
        overlay.on('mousedown', (event: MouseEvent) => {
            if (this.chartInteractionMode === 'select') {
                // Selection mode - drag to zoom
                this.isSelecting = true;
                const [x] = d3.pointer(event, overlay.node());
                startX = Math.max(0, Math.min(x, xScale.range()[1]));
                this.selectionStartX = startX;
                selectionRect
                    .attr('x', startX)
                    .attr('width', 0)
                    .style('display', 'block');
            } else if (this.chartInteractionMode === 'pan') {
                // Pan mode - drag to pan
                isPanning = true;
                panStartX = x;
                panStartViewportStart = this.chartViewportStart;
                overlay.style('cursor', 'grabbing');
        svg.on('mousemove', (event: MouseEvent) => {
            if (isDragging && this.chartInteractionMode === 'select') {
                const [x] = d3.pointer(event, g.node());
                const currentX = Math.max(0, Math.min(x, xScale.range()[1]));
                const rectX = Math.min(startX, currentX);
                const rectWidth = Math.abs(currentX - startX);
                    .attr('x', rectX)
                    .attr('width', rectWidth);
            } else if (isPanning && this.chartInteractionMode === 'pan') {
                const deltaX = x - panStartX;
                // Convert pixel delta to time delta
                const pixelsPerMs = xScale.range()[1] / ((xScale.domain()[1] - xScale.domain()[0]) || 1);
                const timeDelta = -deltaX / pixelsPerMs; // Negative because dragging right should move viewport left
                // Calculate new viewport position using the full time range
                const viewportSize = (this.chartViewportEnd - this.chartViewportStart) || fullTimeRange / this.chartZoomLevel;
                let newStart = panStartViewportStart + timeDelta;
                newStart = Math.max(0, Math.min(newStart, fullTimeRange - viewportSize));
                this.chartViewportStart = newStart;
                this.chartViewportEnd = newStart + viewportSize;
                // Re-render chart with new viewport
        svg.on('mouseup', (event: MouseEvent) => {
                this.isSelecting = false;
                const endX = Math.max(0, Math.min(x, xScale.range()[1]));
                selectionRect.style('display', 'none');
                // Only zoom if selection is significant (> 20 pixels)
                const selectionWidth = Math.abs(endX - startX);
                if (selectionWidth > 20) {
                    const startTime = getTimeFromX(Math.min(startX, endX));
                    const endTime = getTimeFromX(Math.max(startX, endX));
                        this.zoomToTimeRange(startTime, endTime, allChartData);
            } else if (isPanning) {
                isPanning = false;
                overlay.style('cursor', 'grab');
        // Cancel selection/pan on mouse leave
        svg.on('mouseleave', () => {
            if (isDragging) {
            if (isPanning) {
     * Zooms the chart to show only events within the specified time range
    private zoomToTimeRange(
        startTime: number,
        endTime: number,
        allData: Array<{ relativeTime: number; duration: number }>
        // Store the time range for filtering
        this.chartTimeRangeStart = startTime;
        this.chartTimeRangeEnd = endTime;
        // Calculate zoom level based on selection
        const fullRange = (d3.max(allData, d => d.relativeTime) || 1000) - (d3.min(allData, d => d.relativeTime) || 0);
        const selectedRange = endTime - startTime;
        const newZoomLevel = fullRange / Math.max(selectedRange, 10); // Allow very fine selections (down to 10ms)
        this.chartZoomLevel = Math.min(Math.max(newZoomLevel, 1), 100); // Allow up to 100x zoom
        this.chartViewportStart = startTime;
        this.chartViewportEnd = endTime;
    // === Local Cache Methods ===
     * Refreshes cache data from LocalCacheManager
    refreshCacheData(): void {
        const lcm = LocalCacheManager.Instance;
        this.cacheInitialized = lcm.IsInitialized;
        if (this.cacheInitialized) {
            this.cacheStats = lcm.GetStats();
            this.cacheEntries = lcm.GetAllEntries();
            this.cacheHitRate = lcm.GetHitRate();
            this.cacheStats = null;
            this.cacheEntries = [];
            this.cacheHitRate = 0;
     * Getter for filtered cache entries based on type filter
    get filteredCacheEntries(): CacheEntryInfo[] {
        if (this.cacheTypeFilter === 'all') {
            return this.cacheEntries;
        return this.cacheEntries.filter(e => e.type === this.cacheTypeFilter);
     * Sets the cache type filter
    setCacheTypeFilter(type: CacheEntryType | 'all'): void {
        this.cacheTypeFilter = type;
     * Clears all cache entries
    async clearAllCache(): Promise<void> {
        if (lcm.IsInitialized) {
            await lcm.ClearAll();
     * Invalidates a single cache entry
    async invalidateCacheEntry(entry: CacheEntryInfo): Promise<void> {
        if (!lcm.IsInitialized) return;
        // Remove based on type
        if (entry.type === 'runview' && entry.fingerprint) {
            await lcm.InvalidateRunViewResult(entry.fingerprint);
        } else if (entry.type === 'dataset') {
            // For datasets, we need to call ClearDataset with proper params
            // Since we don't have the full params, use the key directly
            // This is a simplified approach - in production you'd want more robust handling
            await lcm.InvalidateRunViewResult(entry.key);
        } else if (entry.type === 'runquery' && entry.fingerprint) {
     * Formats a cache timestamp (unix ms) to display string
    formatCacheTimestamp(timestamp: number): string {
    // === Engine Detail Panel Methods ===
     * Refresh a single engine
    async refreshSingleEngine(engine: EngineDiagnosticInfo, event: Event): Promise<void> {
        this.isRefreshingSingleEngine = engine.className;
            const engineInstance = BaseEngineRegistry.Instance.GetEngine<{ RefreshAllItems: () => Promise<void> }>(engine.className);
            if (engineInstance && typeof engineInstance.RefreshAllItems === 'function') {
                await engineInstance.RefreshAllItems();
                console.log(`Refreshed engine: ${engine.className}`);
            console.error(`Error refreshing engine ${engine.className}:`, error);
            this.isRefreshingSingleEngine = null;
     * Opens the engine detail panel for a specific engine
    openEngineDetailPanel(engine: EngineDiagnosticInfo): void {
        const configItems = this.getEngineConfigItems(engine.className);
        this.engineDetailPanel = {
            engine,
            configItems,
     * Closes the engine detail panel
    closeEngineDetailPanel(): void {
     * Refreshes the engine shown in the detail panel
    async refreshEngineInDetailPanel(): Promise<void> {
        if (!this.engineDetailPanel.engine) return;
        this.engineDetailPanel.isRefreshing = true;
            const engineInstance = BaseEngineRegistry.Instance.GetEngine<{ RefreshAllItems: () => Promise<void> }>(
                this.engineDetailPanel.engine.className
            // Refresh the data and reopen panel with updated info
            await this.refreshData();
            // Update the panel with refreshed data
            const updatedEngine = this.engines.find(e => e.className === this.engineDetailPanel.engine?.className);
            if (updatedEngine) {
                this.engineDetailPanel.engine = updatedEngine;
                this.engineDetailPanel.configItems = this.getEngineConfigItems(updatedEngine.className);
            console.error('Error refreshing engine in detail panel:', error);
            this.engineDetailPanel.isRefreshing = false;
     * Gets config items for an engine by examining its Configs property
    private getEngineConfigItems(className: string): EngineConfigItemDisplay[] {
        const engineInstance = BaseEngineRegistry.Instance.GetEngine<{
            Configs?: Array<{
                Type: 'entity' | 'dataset';
                PropertyName: string;
                EntityName?: string;
                DatasetName?: string;
        }>(className);
        if (!engineInstance || !engineInstance.Configs) {
        const engineObj = engineInstance as Record<string, unknown>;
        const items: EngineConfigItemDisplay[] = [];
        for (const config of engineInstance.Configs) {
            const propValue = engineObj[config.PropertyName];
            const dataArray = Array.isArray(propValue) ? propValue : [];
            const estimatedBytes = this.estimateArrayMemory(dataArray);
            const initialPageSize = 10;
            const initialData = dataArray.slice(0, initialPageSize);
            items.push({
                propertyName: config.PropertyName,
                type: config.Type || 'entity',
                entityName: config.EntityName,
                datasetName: config.DatasetName,
                filter: config.Filter,
                orderBy: config.OrderBy,
                itemCount: dataArray.length,
                estimatedMemoryBytes: estimatedBytes,
                memoryDisplay: this.formatBytes(estimatedBytes),
                sampleData: dataArray, // Store all data for paging
                displayedData: initialData,
                allDataLoaded: dataArray.length <= initialPageSize,
                isLoadingMore: false,
                currentPage: 1,
                pageSize: initialPageSize
        return items.sort((a, b) => b.itemCount - a.itemCount);
     * Estimates memory for an array of objects
    private estimateArrayMemory(arr: unknown[]): number {
        if (arr.length === 0) return 0;
        // Sample first item to estimate size
        const sample = arr[0];
        let bytesPerItem = 100; // default
        if (sample && typeof sample === 'object') {
                // For MJ entity objects, use GetAll() to get just the data values
                // This avoids serializing the massive metadata/prototype chain
                const sampleObj = sample as { GetAll?: () => Record<string, unknown> };
                const dataToSerialize = sampleObj.GetAll ? sampleObj.GetAll() : sample;
                const json = JSON.stringify(dataToSerialize);
                bytesPerItem = json.length * 2; // UTF-16
                bytesPerItem = 500;
        return arr.length * bytesPerItem;
     * Toggle expansion of a config item
    toggleConfigItemExpanded(item: EngineConfigItemDisplay): void {
        item.expanded = !item.expanded;
     * Get column names for sample data display
    getSampleDataColumns(item: EngineConfigItemDisplay): string[] {
        if (item.sampleData.length === 0) return [];
        const sample = item.sampleData[0];
        if (!sample || typeof sample !== 'object') return [];
        // For BaseEntity objects, try to get key properties
        const obj = sample as Record<string, unknown>;
        // Check if it's a BaseEntity with GetAll method
        if ('GetAll' in obj && typeof obj.GetAll === 'function') {
            const allData = (obj as { GetAll: () => Record<string, unknown> }).GetAll();
            // Return priority columns first
            const priorityKeys = ['ID', 'Name', 'Description', 'Code', 'Status', 'Type'];
            const availableKeys = Object.keys(allData);
                if (availableKeys.includes(key)) {
                    result.push(key);
            // Add remaining keys up to 6 total
            for (const key of availableKeys) {
                if (!result.includes(key) && result.length < 6 && !key.startsWith('_')) {
        // For plain objects
        const keys = Object.keys(obj).filter(k => !k.startsWith('_'));
        return keys.slice(0, 6);
     * Get a value from sample data for display
    getSampleDataValue(row: unknown, column: string): string {
        if (!row || typeof row !== 'object') return '';
        const obj = row as Record<string, unknown>;
        // For BaseEntity objects, use GetAll
            const value = allData[column];
            return this.formatValueForDisplay(value);
        const value = obj[column];
     * Format a value for display in sample data table
    private formatValueForDisplay(value: unknown): string {
        if (typeof value === 'string') return value;
        if (typeof value === 'number') return value.toString();
        if (typeof value === 'boolean') return value ? 'true' : 'false';
        if (value instanceof Date) return value.toLocaleDateString();
        if (typeof value === 'object') return JSON.stringify(value);
     * Load more data for a config item (paging)
    loadMoreData(item: EngineConfigItemDisplay): void {
        if (item.isLoadingMore || item.allDataLoaded) return;
        item.isLoadingMore = true;
        // Simulate async loading (data is already in memory)
            const nextPage = item.currentPage + 1;
            const startIndex = item.currentPage * item.pageSize;
            const endIndex = startIndex + item.pageSize;
            const newData = item.sampleData.slice(startIndex, endIndex);
            item.displayedData = [...item.displayedData, ...newData];
            item.currentPage = nextPage;
            item.allDataLoaded = item.displayedData.length >= item.sampleData.length;
            item.isLoadingMore = false;
     * Load all remaining data for a config item
    loadAllData(item: EngineConfigItemDisplay): void {
            item.displayedData = [...item.sampleData];
            item.allDataLoaded = true;
     * Get the record ID from a row (for opening entity records)
    getRecordId(row: unknown): string | null {
        if (!row || typeof row !== 'object') return null;
        // For BaseEntity objects, use the ID property
        if ('ID' in obj) {
            return String(obj.ID);
        // Try GetAll for BaseEntity
            if ('ID' in allData) {
                return String(allData.ID);
     * Open an entity record using NavigationService
    openEntityRecord(entityName: string, row: unknown): void {
        const recordId = this.getRecordId(row);
        if (!recordId || !entityName) return;
        // Create a CompositeKey with the ID
        const compositeKey = new CompositeKey([
            { FieldName: 'ID', Value: recordId }
     * Open an entity in the explorer (placeholder - would need routing integration)
    openEntityInExplorer(entityName: string): void {
        // This would integrate with the app's navigation/routing system
        // For now, just log and could be extended to emit an event or use router
        console.log(`Would open entity in explorer: ${entityName}`);
        // Could emit an event or use router:
        // this.router.navigate(['/entities', entityName]);
    // === Deep Linking via Query Parameters ===
     * Apply query parameters to component state (deep linking support)
     * Query params take precedence over saved preferences
    private applyQueryParams(): void {
        // Section: ?section=engines|redundant|performance|cache
        if (params['section']) {
            const section = params['section'] as string;
            if (['engines', 'redundant', 'performance', 'cache'].includes(section)) {
                this.activeSection = section as 'engines' | 'redundant' | 'performance' | 'cache';
        // Performance tab: ?tab=monitor|overview|events|patterns|insights
        if (params['tab']) {
            const tab = params['tab'] as string;
            if (['monitor', 'overview', 'events', 'patterns', 'insights'].includes(tab)) {
                this.perfTab = tab as 'monitor' | 'overview' | 'events' | 'patterns' | 'insights';
        // Telemetry source: ?source=client|server
        if (params['source']) {
            const source = params['source'] as string;
            if (['client', 'server'].includes(source)) {
                this.telemetrySource = source as 'client' | 'server';
        // Category filter: ?category=all|data|api|render|...
        if (params['category']) {
            const category = params['category'] as string;
            if (category === 'all') {
                this.categoryFilter = 'all';
                this.categoryFilter = category as TelemetryCategory;
        // Search query: ?search=...
        if (params['search']) {
            this.searchQuery = params['search'] as string;
        // KPI cards collapsed: ?kpi=collapsed|expanded
        if (params['kpi']) {
            this.kpiCardsCollapsed = params['kpi'] === 'collapsed';
     * Update query parameters to reflect current state (for deep linking)
    private updateQueryParams(): void {
            section: this.activeSection !== 'engines' ? this.activeSection : null,
            tab: this.perfTab !== 'monitor' ? this.perfTab : null,
            source: this.telemetrySource !== 'client' ? this.telemetrySource : null,
            category: this.categoryFilter !== 'all' ? this.categoryFilter : null,
            search: this.searchQuery.trim() || null,
            kpi: this.kpiCardsCollapsed ? 'collapsed' : null
    // === User Preferences Persistence ===
     * Load user preferences from MJ: User Settings entity using UserInfoEngine for cached access
    private async loadUserPreferences(): Promise<void> {
            const setting = engine.UserSettings.find(s => s.Setting === SYSTEM_DIAGNOSTICS_SETTINGS_KEY);
                    const prefs = JSON.parse(this.userSettingEntity.Value) as Partial<SystemDiagnosticsUserPreferences>;
            console.warn('Failed to load user preferences:', error);
     * Apply loaded user preferences to component state
    private applyUserPreferences(prefs: Partial<SystemDiagnosticsUserPreferences>): void {
        if (prefs.kpiCardsCollapsed !== undefined) this.kpiCardsCollapsed = prefs.kpiCardsCollapsed;
        if (prefs.activeSection !== undefined) this.activeSection = prefs.activeSection;
        if (prefs.perfTab !== undefined) this.perfTab = prefs.perfTab;
        if (prefs.telemetrySource !== undefined) this.telemetrySource = prefs.telemetrySource;
        if (prefs.categoryFilter !== undefined) this.categoryFilter = prefs.categoryFilter;
        if (prefs.chartZoomLevel !== undefined) this.chartZoomLevel = prefs.chartZoomLevel;
        if (prefs.chartGapCompression !== undefined) this.chartGapCompression = prefs.chartGapCompression;
        if (prefs.autoRefresh !== undefined) this.autoRefresh = prefs.autoRefresh;
     * Get current preferences as an object
    private getCurrentPreferences(): SystemDiagnosticsUserPreferences {
            kpiCardsCollapsed: this.kpiCardsCollapsed,
            activeSection: this.activeSection,
            perfTab: this.perfTab,
            telemetrySource: this.telemetrySource,
            categoryFilter: this.categoryFilter,
            chartZoomLevel: this.chartZoomLevel,
            chartGapCompression: this.chartGapCompression,
            autoRefresh: this.autoRefresh
     * Debounced save of user preferences (500ms delay)
     * Also updates query params for deep linking
        if (!this.settingsLoaded) return; // Don't save until we've loaded
        // Update query params immediately for deep linking
     * Save user preferences to MJ: User Settings entity using UserInfoEngine for cached lookup
    private async saveUserPreferences(): Promise<void> {
                    this.userSettingEntity.Setting = SYSTEM_DIAGNOSTICS_SETTINGS_KEY;
            // Save the preferences as JSON
            this.userSettingEntity.Value = JSON.stringify(this.getCurrentPreferences());
            console.warn('Failed to save user preferences:', error);
        return 'System Diagnostics';
        return 'fa-solid fa-stethoscope';
