import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { Observable, Subject, BehaviorSubject } from 'rxjs';
import { takeUntil, map, shareReplay } from 'rxjs/operators';
  TestTrendData,
  TestAnalytics,
  TestRunSummary,
  VersionMetrics
} from '../services/testing-instrumentation.service';
// Local interfaces
interface TimeRangeOption {
interface ScoreDistributionBucket {
interface VersionRow {
  gitCommitShort: string;
  agentVersion: string;
  passRateDelta: number | null;
  costDelta: number | null;
// Component
  selector: 'app-testing-analytics',
    <div class="testing-analytics">
      <!-- ===== 1. Page Header ===== -->
      <div class="page-header">
            Testing Analytics
          <div class="time-range-pills">
            @for (range of TimeRanges; track range.days) {
                [class.active]="SelectedDays === range.days"
                (click)="OnTimeRangeSelect(range.days)">
                {{ range.label }}
          <button class="refresh-btn" (click)="Refresh()">
      <!-- ===== 2. Trend Overview (2-column CSS charts) ===== -->
      <div class="section-title">
        Trend Overview
      <div class="trend-grid">
        <!-- Pass Rate Trend -->
        <div class="card trend-card">
          <h4><i class="fa-solid fa-check-double"></i> Pass Rate Trend</h4>
          @if (DisplayTrends$ | async; as trends) {
            @if (trends.length === 0) {
              <div class="empty-mini"><i class="fa-solid fa-chart-area"></i><span>No trend data</span></div>
              <div class="bar-chart">
                @for (t of trends; track trackTrend($index, t)) {
                  <div class="bar-row">
                    <span class="bar-label">{{ FormatBucketDate(t.timestamp) }}</span>
                    <div class="bar-track">
                      <div class="bar-fill pass-rate-bar" [style.width.%]="PassRate(t)"></div>
                    <span class="bar-value" [class]="PassRateClass(t)">{{ PassRate(t) | number:'1.0-0' }}%</span>
              <div class="trend-direction">
                @if (PassRateTrendDirection$ | async; as dir) {
                  @if (dir === 'up') {
                    <i class="fa-solid fa-arrow-trend-up trend-up"></i> <span class="trend-up">Improving</span>
                  } @else if (dir === 'down') {
                    <i class="fa-solid fa-arrow-trend-down trend-down"></i> <span class="trend-down">Declining</span>
                    <i class="fa-solid fa-minus trend-neutral"></i> <span class="trend-neutral">Stable</span>
        <!-- Run Volume -->
          <h4><i class="fa-solid fa-layer-group"></i> Run Volume</h4>
              <div class="empty-mini"><i class="fa-solid fa-chart-bar"></i><span>No volume data</span></div>
                    <div class="bar-track stacked">
                      <div class="bar-segment passed-seg" [style.width.%]="SegmentPct(t, 'passed')"></div>
                      <div class="bar-segment failed-seg" [style.width.%]="SegmentPct(t, 'failed')"></div>
                      <div class="bar-segment skipped-seg" [style.width.%]="SegmentPct(t, 'skipped')"></div>
                    <span class="bar-value">{{ t.totalRuns }}</span>
              <div class="legend-row">
                <span class="legend-dot passed-seg"></span> Passed
                <span class="legend-dot failed-seg"></span> Failed
                <span class="legend-dot skipped-seg"></span> Skipped
        <!-- Score Trend -->
          <h4><i class="fa-solid fa-star-half-stroke"></i> Score Trend</h4>
              <div class="empty-mini"><i class="fa-solid fa-star"></i><span>No score data</span></div>
                      <div class="bar-fill" [class]="ScoreBarClass(t.averageScore)" [style.width.%]="t.averageScore * 100"></div>
                    <span class="bar-value" [class]="ScoreTextClass(t.averageScore)">{{ (t.averageScore * 100) | number:'1.0-0' }}%</span>
        <!-- Cost Trend -->
          <h4><i class="fa-solid fa-dollar-sign"></i> Cost Trend</h4>
              <div class="empty-mini"><i class="fa-solid fa-wallet"></i><span>No cost data</span></div>
                      <div class="bar-fill cost-bar" [style.width.%]="CostBarPct(t, trends)"></div>
                    <span class="bar-value">\${{ t.totalCost | number:'1.2-2' }}</span>
      <!-- ===== 3. Insights Section ===== -->
        Insights
      <div class="insights-grid">
        <!-- Top Failing Tests -->
        <div class="card insight-card">
          <h4><i class="fa-solid fa-triangle-exclamation"></i> Top Failing Tests</h4>
          @if (TopFailingTests$ | async; as tests) {
            @if (tests.length === 0) {
              <div class="empty-mini"><i class="fa-solid fa-circle-check"></i><span>No failing tests</span></div>
              <div class="insight-list">
                @for (t of tests; track t.testName) {
                  <div class="insight-row">
                    <span class="insight-name" [title]="t.testName">{{ t.testName }}</span>
                    <span class="badge-fail">{{ t.failureCount }}</span>
                    <span class="insight-secondary">{{ t.failureRate | number:'1.0-1' }}%</span>
        <!-- Slowest Tests -->
          <h4><i class="fa-solid fa-clock"></i> Slowest Tests</h4>
          @if (SlowestTests$ | async; as tests) {
              <div class="empty-mini"><i class="fa-solid fa-bolt"></i><span>No duration data</span></div>
                    <span class="insight-primary">{{ FormatDuration(t.avgDuration) }}</span>
                    <span class="insight-secondary">max {{ FormatDuration(t.maxDuration) }}</span>
        <!-- Most Expensive Tests -->
          <h4><i class="fa-solid fa-coins"></i> Most Expensive Tests</h4>
          @if (MostExpensiveTests$ | async; as tests) {
              <div class="empty-mini"><i class="fa-solid fa-dollar-sign"></i><span>No cost data</span></div>
                    <span class="insight-primary">\${{ t.totalCost | number:'1.2-2' }}</span>
                    <span class="insight-secondary">\${{ t.avgCost | number:'1.4-4' }}/run</span>
      <!-- ===== 4. Score Distribution ===== -->
        Score Distribution
      <div class="card distribution-card">
        @if (ScoreDistribution$ | async; as buckets) {
          @if (buckets.length === 0) {
            <div class="empty-mini"><i class="fa-solid fa-chart-simple"></i><span>No score data available</span></div>
              @for (b of buckets; track b.label) {
                <div class="dist-row">
                  <span class="dist-label">{{ b.label }}</span>
                  <div class="dist-track">
                    <div class="dist-fill" [style.width.%]="b.percentage" [style.background]="b.color"></div>
                  <span class="dist-count">{{ b.count }}</span>
                  <span class="dist-pct">{{ b.percentage | number:'1.0-1' }}%</span>
      <!-- ===== 5. Version Comparison ===== -->
        Version Comparison
        <span class="section-subtitle">Compare metrics across different builds</span>
      <div class="card version-card">
        @if (IsLoadingVersions) {
          <div class="empty-mini"><i class="fa-solid fa-spinner fa-spin"></i><span>Loading versions...</span></div>
        } @else if (VersionRows.length === 0) {
          <div class="empty-mini"><i class="fa-solid fa-code-branch"></i><span>No version data available</span></div>
          <div class="version-table-wrap">
            <table class="version-table">
                  <th>Version</th>
                  <th class="num">Tests</th>
                  <th class="num">Pass Rate</th>
                  <th class="num">Cost</th>
                @for (row of VersionRows; track row.version) {
                      <span class="version-label">{{ row.gitCommitShort }}</span>
                      <span class="version-agent">{{ row.agentVersion }}</span>
                    <td>{{ row.date | date:'mediumDate' }}</td>
                    <td class="num">{{ row.totalTests }}</td>
                    <td class="num">
                      <span>{{ row.passRate | number:'1.1-1' }}%</span>
                      @if (row.passRateDelta !== null) {
                        <span class="delta" [class.delta-up]="row.passRateDelta > 0" [class.delta-down]="row.passRateDelta < 0" [class.delta-neutral]="row.passRateDelta === 0">
                          @if (row.passRateDelta > 0) {
                          } @else if (row.passRateDelta < 0) {
                          {{ AbsNum(row.passRateDelta) | number:'1.1-1' }}%
                      <span>\${{ row.totalCost | number:'1.2-2' }}</span>
                      @if (row.costDelta !== null) {
                        <span class="delta" [class.delta-up]="row.costDelta < 0" [class.delta-down]="row.costDelta > 0" [class.delta-neutral]="row.costDelta === 0">
                          @if (row.costDelta > 0) {
                          } @else if (row.costDelta < 0) {
                          {{ AbsNum(row.costDelta) | number:'1.1-1' }}%
    /*  Layout & page                                                      */
    .testing-analytics {
    /*  Page header                                                        */
    .page-header {
    .header-left h2 {
    .header-left h2 i { color: #2196f3; }
    /* time-range pills */
    .time-range-pills {
    .pill:hover { background: #e0e0e0; }
    .pill.active {
      box-shadow: 0 1px 4px rgba(33,150,243,0.3);
    .refresh-btn:hover { background: #f5f5f5; border-color: #bbb; }
    /*  Section titles                                                     */
      color: #444;
    .section-title i { color: #2196f3; }
    .section-subtitle {
    /*  Cards                                                              */
    .card:hover {
      box-shadow: 0 4px 16px rgba(0,0,0,0.12);
    .card h4 {
      margin: 0 0 14px 0;
    .card h4 i { color: #2196f3; font-size: 14px; }
    /*  Trend grid (2 columns)                                             */
    .trend-grid {
    /* CSS bar charts */
    .bar-chart {
    .bar-row {
      grid-template-columns: 60px 1fr 52px;
    .bar-track {
      border-radius: 7px;
    .bar-track.stacked {
    .bar-value {
    /* colour classes for bars */
      background: linear-gradient(90deg, #66bb6a, #43a047);
      background: linear-gradient(90deg, #ffa726, #f57c00);
    .score-excellent { background: linear-gradient(90deg, #66bb6a, #43a047); }
    .score-good      { background: linear-gradient(90deg, #42a5f5, #1e88e5); }
    .score-fair      { background: linear-gradient(90deg, #ffa726, #f57c00); }
    .score-poor      { background: linear-gradient(90deg, #ef5350, #e53935); }
    /* text colour classes */
    .text-excellent { color: #43a047; }
    .text-good      { color: #1e88e5; }
    .text-fair      { color: #f57c00; }
    .text-poor      { color: #e53935; }
    .text-pass-good { color: #43a047; }
    .text-pass-warn { color: #f57c00; }
    .text-pass-bad  { color: #e53935; }
    /* stacked segments */
    .bar-segment { height: 100%; transition: width 0.4s ease; }
    .passed-seg  { background: #66bb6a; }
    .failed-seg  { background: #ef5350; }
    .skipped-seg { background: #bdbdbd; }
    .legend-row {
    .legend-dot:first-child { margin-left: 0; }
    .legend-dot.passed-seg  { background: #66bb6a; }
    .legend-dot.failed-seg  { background: #ef5350; }
    .legend-dot.skipped-seg { background: #bdbdbd; }
    .trend-direction {
    .trend-up      { color: #43a047; }
    .trend-down    { color: #e53935; }
    .trend-neutral { color: #9e9e9e; }
    /*  Insights grid (3 columns)                                          */
    .insights-grid {
    .insight-list {
    .insight-row {
      grid-template-columns: 1fr auto auto;
    .insight-row:hover { background: #f8f9fa; }
    .insight-name {
    .badge-fail {
    .insight-primary {
      color: #1e88e5;
    .insight-secondary {
    /*  Score Distribution                                                 */
    .distribution-card {
    .dist-row {
      grid-template-columns: 110px 1fr 40px 50px;
    .dist-label {
    .dist-track {
    .dist-fill {
    .dist-count {
    .dist-pct {
    /*  Version Comparison                                                 */
    .version-card {
    .version-table-wrap {
    .version-table {
    .version-table th {
      border-bottom: 2px solid #eee;
    .version-table th.num,
    .version-table td.num { text-align: right; }
    .version-table td {
    .version-table tbody tr:hover { background: #f8f9fa; }
    .version-label {
      font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
    .version-agent {
    .delta {
    .delta i { font-size: 9px; }
    .delta-up   { background: #e8f5e9; color: #43a047; }
    .delta-down { background: #ffebee; color: #e53935; }
    .delta-neutral { background: #f5f5f5; color: #9e9e9e; }
    /*  Empty / mini empty states                                          */
    .empty-mini {
    .empty-mini i { font-size: 28px; opacity: 0.5; }
    .empty-mini span { font-size: 12px; }
    /*  Responsive                                                         */
    @media (max-width: 1100px) {
      .trend-grid { grid-template-columns: 1fr; }
      .insights-grid { grid-template-columns: 1fr 1fr; }
    @media (max-width: 720px) {
      .page-header { flex-direction: column; align-items: flex-start; }
      .insights-grid { grid-template-columns: 1fr; }
      .dist-row { grid-template-columns: 80px 1fr 32px 42px; }
export class TestingAnalyticsComponent implements OnInit, OnDestroy {
  // ------- Inputs / Outputs -------
  @Input() initialState: Record<string, unknown> | null = null;
  // ------- Public state -------
  TimeRanges: TimeRangeOption[] = [
    { label: '7 Days', days: 7 },
    { label: '30 Days', days: 30 },
    { label: '90 Days', days: 90 }
  SelectedDays = 30;
  VersionRows: VersionRow[] = [];
  IsLoadingVersions = false;
  // ------- Observables -------
  DisplayTrends$!: Observable<TestTrendData[]>;
  PassRateTrendDirection$!: Observable<'up' | 'down' | 'stable'>;
  TopFailingTests$!: Observable<TestAnalytics['topFailingTests']>;
  SlowestTests$!: Observable<TestAnalytics['slowestTests']>;
  MostExpensiveTests$!: Observable<TestAnalytics['mostExpensiveTests']>;
  ScoreDistribution$!: Observable<ScoreDistributionBucket[]>;
  // ------- Private -------
  private maxTrendRuns = 0; // cached for cost bar scaling
    private instrumentationService: TestingInstrumentationService,
  // ===================================================================
    this.setupObservables();
    this.loadVersionMetrics();
  // Public methods (template-bound)
  OnTimeRangeSelect(days: number): void {
    this.SelectedDays = days;
    const start = new Date(end.getTime() - days * 24 * 60 * 60 * 1000);
  Refresh(): void {
  // -- Trend helpers (called from template) --
  PassRate(t: TestTrendData): number {
    return t.totalRuns === 0 ? 0 : (t.passed / t.totalRuns) * 100;
  PassRateClass(t: TestTrendData): string {
    const rate = this.PassRate(t);
    if (rate >= 80) return 'text-pass-good';
    if (rate >= 60) return 'text-pass-warn';
    return 'text-pass-bad';
  SegmentPct(t: TestTrendData, segment: 'passed' | 'failed' | 'skipped'): number {
    if (t.totalRuns === 0) return 0;
    const value = segment === 'passed' ? t.passed : segment === 'failed' ? t.failed : t.skipped;
    return (value / t.totalRuns) * 100;
  ScoreBarClass(score: number): string {
    if (score >= 0.9) return 'bar-fill score-excellent';
    if (score >= 0.7) return 'bar-fill score-good';
    if (score >= 0.5) return 'bar-fill score-fair';
    return 'bar-fill score-poor';
  ScoreTextClass(score: number): string {
    if (score >= 0.9) return 'text-excellent';
    if (score >= 0.7) return 'text-good';
    if (score >= 0.5) return 'text-fair';
    return 'text-poor';
  CostBarPct(t: TestTrendData, all: TestTrendData[]): number {
    const maxCost = Math.max(...all.map(x => x.totalCost), 0.01);
    return (t.totalCost / maxCost) * 100;
  FormatBucketDate(d: Date): string {
    const dt = d instanceof Date ? d : new Date(d);
    return `${dt.getMonth() + 1}/${dt.getDate()}`;
  FormatDuration(ms: number): string {
    if (ms < 1000) return `${Math.round(ms)}ms`;
    const s = Math.floor(ms / 1000);
    const m = Math.floor(s / 60);
    if (m > 0) return `${m}m ${s % 60}s`;
    return `${s}s`;
  AbsNum(n: number): number {
    return Math.abs(n);
  /** trackBy for trend @for loops */
  trackTrend(index: number, t: TestTrendData): number {
  // Private helpers
    if (this.initialState != null) {
      if (typeof this.initialState['selectedDays'] === 'number') {
        this.SelectedDays = this.initialState['selectedDays'] as number;
    // Apply initial date range
    const start = new Date(end.getTime() - this.SelectedDays * 24 * 60 * 60 * 1000);
  private setupObservables(): void {
    // Limit trends to last 10 buckets for readability
    this.DisplayTrends$ = this.instrumentationService.trends$.pipe(
      map(trends => trends.slice(-10)),
    this.PassRateTrendDirection$ = this.DisplayTrends$.pipe(
      map(trends => this.calculateTrendDirection(trends)),
    const analytics$ = this.instrumentationService.analytics$;
    this.TopFailingTests$ = analytics$.pipe(
      map(a => a.topFailingTests.slice(0, 5)),
    this.SlowestTests$ = analytics$.pipe(
      map(a => a.slowestTests.slice(0, 5)),
    this.MostExpensiveTests$ = analytics$.pipe(
      map(a => a.mostExpensiveTests.slice(0, 5)),
    this.ScoreDistribution$ = this.instrumentationService.testRuns$.pipe(
      map(runs => this.buildScoreDistribution(runs)),
  private calculateTrendDirection(trends: TestTrendData[]): 'up' | 'down' | 'stable' {
    if (trends.length < 2) return 'stable';
    const recentHalf = trends.slice(Math.floor(trends.length / 2));
    const olderHalf = trends.slice(0, Math.floor(trends.length / 2));
    const avgRecent = this.averagePassRate(recentHalf);
    const avgOlder = this.averagePassRate(olderHalf);
    const diff = avgRecent - avgOlder;
    if (diff > 2) return 'up';
    if (diff < -2) return 'down';
  private averagePassRate(trends: TestTrendData[]): number {
    const withRuns = trends.filter(t => t.totalRuns > 0);
    if (withRuns.length === 0) return 0;
    return withRuns.reduce((s, t) => s + (t.passed / t.totalRuns) * 100, 0) / withRuns.length;
  private buildScoreDistribution(runs: TestRunSummary[]): ScoreDistributionBucket[] {
    const defs: Array<{ label: string; color: string; min: number; max: number }> = [
      { label: 'Excellent (>=0.9)', color: '#43a047', min: 0.9, max: 1.01 },
      { label: 'Good (0.7-0.89)',   color: '#1e88e5', min: 0.7, max: 0.9 },
      { label: 'Fair (0.5-0.69)',   color: '#f57c00', min: 0.5, max: 0.7 },
      { label: 'Poor (0.3-0.49)',   color: '#e64a19', min: 0.3, max: 0.5 },
      { label: 'Fail (<0.3)',       color: '#e53935', min: 0,   max: 0.3 }
    const total = runs.length || 1;
    return defs.map(d => {
      const count = runs.filter(r => r.score >= d.min && r.score < d.max).length;
        label: d.label,
        color: d.color,
        percentage: (count / total) * 100
  private async loadVersionMetrics(): Promise<void> {
    this.IsLoadingVersions = true;
      const metrics = await this.instrumentationService.getVersionMetrics();
      this.VersionRows = this.buildVersionRows(metrics);
      this.VersionRows = [];
      this.IsLoadingVersions = false;
  private buildVersionRows(metrics: VersionMetrics[]): VersionRow[] {
    // metrics are already sorted newest-first by the service
    return metrics.map((m, i) => {
      const prev = i < metrics.length - 1 ? metrics[i + 1] : null;
      let passRateDelta: number | null = null;
      let costDelta: number | null = null;
      if (prev) {
        passRateDelta = m.passRate - prev.passRate;
        costDelta = prev.totalCost !== 0
          ? ((m.totalCost - prev.totalCost) / prev.totalCost) * 100
        version: m.version,
        gitCommitShort: m.gitCommit.length > 7 ? m.gitCommit.substring(0, 7) : m.gitCommit,
        agentVersion: m.agentVersion,
        date: m.runDate,
        totalTests: m.totalTests,
        passRate: m.passRate,
        totalCost: m.totalCost,
        passRateDelta,
        costDelta
      selectedDays: this.SelectedDays
