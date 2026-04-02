  Component, OnInit, OnDestroy, Input, Output, EventEmitter,
  ChangeDetectorRef, ChangeDetectionStrategy
import { takeUntil, map } from 'rxjs/operators';
  TestingDashboardKPIs,
  SuiteHierarchyNode
import { KPICardData } from '../../AI/components/widgets/kpi-card.component';
import { TestEngineBase } from '@memberjunction/testing-engine-base';
/** Status type union matching TestRunSummary */
type TestRunStatus = 'Passed' | 'Failed' | 'Skipped' | 'Error' | 'Running' | 'Timeout';
/** Alert item for the alerts section */
interface TestAlert {
  reason: 'regression' | 'low-score';
  status: TestRunStatus;
  runDateTime: Date;
  selector: 'app-testing-dashboard-tab',
    <!-- Full-page loading state -->
      <div class="full-page-loading">
        <mj-loading text="Loading Testing Dashboard..."></mj-loading>
        <!-- Page Header -->
          <h2 class="page-title">
            Testing Dashboard
          <button class="refresh-btn" (click)="OnRefresh()" [disabled]="IsLoading">
            <i class="fa-solid fa-refresh" [class.spinning]="IsLoading"></i>
        <!-- KPI Row -->
          @for (kpi of KpiCards; track kpi.title) {
            <app-kpi-card [data]="kpi"></app-kpi-card>
        <!-- Live Activity Section -->
        <div class="live-activity-card">
            @if (RunningTests.length > 0) {
              <span class="live-dot"></span>
              <h3>{{ RunningTests.length }} test{{ RunningTests.length === 1 ? '' : 's' }} running</h3>
              <i class="fa-solid fa-circle-pause live-idle-icon"></i>
              <h3>No tests currently running</h3>
            <div class="running-tests-list">
              @for (run of RunningTests; track run.id) {
                <div class="running-test-item">
                  <div class="running-test-name">{{ run.testName }}</div>
                  <div class="running-test-meta">
                    <span class="running-elapsed">{{ FormatDuration(run.duration) }}</span>
                    <span class="running-progress-bar">
                      <span class="running-progress-fill"></span>
        <!-- Lower Content: 2-column layout -->
        <div class="lower-grid">
          <!-- Left Column: Recent Runs -->
          <div class="card recent-runs-card">
              <h3><i class="fa-solid fa-history"></i> Recent Runs</h3>
              @for (run of RecentRuns; track run.id) {
                <div class="run-row" (click)="OnOpenRun(run)">
                  <div class="run-row-left">
                    <span class="run-row-name">{{ run.testName }}</span>
                    <span class="run-row-time">{{ FormatTimestamp(run.runDateTime) }}</span>
                  <div class="run-row-right">
                    <app-test-status-badge [status]="run.status"></app-test-status-badge>
                    <app-score-indicator [score]="run.score" [showBar]="true"></app-score-indicator>
                    <span class="run-row-duration">{{ FormatDuration(run.duration) }}</span>
                    <app-cost-display [cost]="run.cost"></app-cost-display>
                <div class="empty-state">No completed test runs found</div>
          <!-- Right Column: Suite Health + Alerts -->
            <!-- Suite Health -->
            <div class="card suite-health-card">
                <h3><i class="fa-solid fa-heartbeat"></i> Suite Health</h3>
              <div class="suite-health-list">
                @for (suite of SortedSuites; track suite.id) {
                  <div class="suite-health-row">
                    <div class="suite-health-info">
                      <span class="suite-health-name">{{ suite.name }}</span>
                      <span class="suite-health-count">{{ suite.testCount }} tests</span>
                    <div class="suite-health-bar-wrapper">
                      <div class="suite-health-bar">
                          class="suite-health-bar-fill"
                          [style.width.%]="suite.passRate"
                          [style.background]="GetPassRateColor(suite.passRate)">
                      <span class="suite-health-pct" [style.color]="GetPassRateColor(suite.passRate)">
                        {{ suite.passRate | number:'1.0-0' }}%
                  <div class="empty-state">No test suites found</div>
            <div class="card alerts-card">
                <h3><i class="fa-solid fa-triangle-exclamation"></i> Alerts</h3>
              <div class="alerts-list">
                @for (alert of Alerts; track alert.id) {
                  <div class="alert-row" (click)="OnOpenAlert(alert)">
                       [class.fa-arrow-trend-down]="alert.reason === 'regression'"
                       [class.fa-circle-exclamation]="alert.reason === 'low-score'"
                       [style.color]="alert.reason === 'regression' ? '#ef4444' : '#f59e0b'">
                    </i>
                    <div class="alert-info">
                      <span class="alert-name">{{ alert.testName }}</span>
                      <span class="alert-reason">
                        @if (alert.reason === 'regression') {
                          Regression detected
                          Score below 0.5 ({{ alert.score | number:'1.2-2' }})
                    <app-test-status-badge [status]="alert.status"></app-test-status-badge>
                    <i class="fa-solid fa-check-circle" style="color: #22c55e; margin-right: 6px;"></i>
                    No alerts - all tests healthy
    /* ===== Layout ===== */
    .full-page-loading {
    /* ===== Page Header ===== */
    .page-title {
    .page-title i { color: #6366f1; }
    .refresh-btn:hover:not(:disabled) { background: #4f46e5; }
    .refresh-btn:disabled { opacity: 0.6; cursor: not-allowed; }
    .refresh-btn i.spinning { animation: spin 1s linear infinite; }
    /* ===== KPI Row ===== */
    /* ===== Card Base ===== */
    .section-header h3 i { color: #6366f1; font-size: 13px; }
    /* ===== Live Activity ===== */
    .live-activity-card {
    .live-dot {
      0%, 100% { box-shadow: 0 0 0 0 rgba(34,197,94,0.5); }
      50% { box-shadow: 0 0 0 6px rgba(34,197,94,0); }
    .live-idle-icon { color: #9ca3af; font-size: 14px; }
    .running-tests-list {
      padding: 0 20px 16px;
    .running-test-item {
      border: 1px solid #bbf7d0;
    .running-test-name {
    .running-test-meta {
    .running-elapsed {
    .running-progress-bar {
    .running-progress-fill {
      background: linear-gradient(90deg, #22c55e, #86efac);
      animation: progress-slide 1.2s ease-in-out infinite;
    @keyframes progress-slide {
      0% { transform: translateX(-100%); }
      100% { transform: translateX(100%); }
    /* ===== Lower Grid ===== */
    .lower-grid {
    /* ===== Recent Runs ===== */
      max-height: 520px;
    .run-row {
      border-bottom: 1px solid #f8fafc;
    .run-row:hover { background: #f8fafc; }
    .run-row-left {
    .run-row-name {
    .run-row-time {
    .run-row-right {
    .run-row-duration {
      min-width: 44px;
    /* ===== Suite Health ===== */
    .suite-health-list {
    .suite-health-row {
    .suite-health-info {
    .suite-health-name {
    .suite-health-count {
    .suite-health-bar-wrapper {
    .suite-health-bar {
    .suite-health-bar-fill {
    .suite-health-pct {
    /* ===== Alerts ===== */
    .alerts-list {
      max-height: 220px;
    .alert-row {
    .alert-row:hover { background: #fef2f2; }
    .alert-info {
    .alert-name {
    .alert-reason {
    /* ===== Empty State ===== */
    /* ===== Responsive ===== */
      .dashboard-container { padding: 16px; }
      .kpi-row { grid-template-columns: 1fr; }
export class TestingDashboardTabComponent implements OnInit, OnDestroy {
  /** Resolved data for the template */
  KpiCards: KPICardData[] = [];
  RunningTests: TestRunSummary[] = [];
  RecentRuns: TestRunSummary[] = [];
  SortedSuites: SuiteHierarchyNode[] = [];
  Alerts: TestAlert[] = [];
    await TestEngineBase.Instance.Config(false);
    this.subscribeToStreams();
  // Public methods (PascalCase)
  OnRefresh(): void {
  OnOpenRun(run: TestRunSummary): void {
    SharedService.Instance.OpenEntityRecord(
      'MJ: Test Runs',
      CompositeKey.FromID(run.id)
  OnOpenAlert(alert: TestAlert): void {
      CompositeKey.FromID(alert.id)
    const totalSeconds = Math.floor(ms / 1000);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;
    if (minutes > 0) return `${minutes}m ${seconds}s`;
    const d = date instanceof Date ? date : new Date(date);
  FormatCost(cost: number): string {
  GetPassRateColor(rate: number): string {
    if (rate >= 90) return '#22c55e';
    if (rate >= 70) return '#f59e0b';
  // Private helpers (camelCase)
  private subscribeToStreams(): void {
    this.subscribeLoading();
    this.subscribeKpis();
    this.subscribeTestRuns();
    this.subscribeSuites();
  private subscribeLoading(): void {
      this.IsLoading = loading;
  private subscribeKpis(): void {
    this.instrumentationService.kpis$.pipe(
      map(kpis => this.buildKpiCards(kpis))
    ).subscribe(cards => {
      this.KpiCards = cards;
  private subscribeTestRuns(): void {
    this.instrumentationService.testRuns$.pipe(
    ).subscribe(runs => {
      this.RunningTests = runs.filter(r => r.status === 'Running');
      this.RecentRuns = runs
        .filter(r => r.status !== 'Running')
        .slice(0, 15);
      this.Alerts = this.buildAlerts(runs);
  private subscribeSuites(): void {
    this.instrumentationService.suiteHierarchy$.pipe(
    ).subscribe(suites => {
      this.SortedSuites = this.flattenAndSort(suites);
  private buildKpiCards(kpis: TestingDashboardKPIs): KPICardData[] {
    const trendDir = kpis.passRateTrend > 0
      ? 'up' as const
      : kpis.passRateTrend < 0
        ? 'down' as const
        : 'stable' as const;
        title: 'Active Tests',
        value: kpis.totalTestsActive,
        icon: 'fa-vial',
        subtitle: `${kpis.totalTestRuns} runs this period`
        title: 'Pass Rate',
        value: `${kpis.passRateThisMonth.toFixed(1)}%`,
        color: kpis.passRateThisMonth >= 90 ? 'success' : kpis.passRateThisMonth >= 75 ? 'warning' : 'danger',
        trend: kpis.passRateTrend !== 0 ? {
          direction: trendDir,
          percentage: Math.abs(Math.round(kpis.passRateTrend * 10) / 10),
          period: 'vs previous period'
        value: `$${kpis.totalCostThisMonth.toFixed(2)}`,
        subtitle: 'This period'
        title: 'Avg Duration',
        value: this.FormatDuration(kpis.averageDuration),
        subtitle: 'Per test run'
        title: 'Pending Review',
        value: kpis.testsPendingReview,
        icon: 'fa-clipboard-check',
        color: kpis.testsPendingReview > 10 ? 'warning' : 'success',
        subtitle: 'Tests need feedback'
  private buildAlerts(runs: TestRunSummary[]): TestAlert[] {
    const alerts: TestAlert[] = [];
    // Group runs by test name to detect regressions
    const byTest = new Map<string, TestRunSummary[]>();
      const existing = byTest.get(run.testName);
        existing.push(run);
        byTest.set(run.testName, [run]);
    this.detectRegressions(byTest, alerts);
    this.detectLowScores(runs, alerts);
    return alerts.slice(0, 20);
  /** Find tests that previously passed but now fail */
  private detectRegressions(
    byTest: Map<string, TestRunSummary[]>,
    alerts: TestAlert[]
    const seenIds = new Set<string>();
    byTest.forEach((testRuns, testName) => {
      if (testRuns.length < 2) return;
      const sorted = [...testRuns].sort(
        (a, b) => b.runDateTime.getTime() - a.runDateTime.getTime()
      const latest = sorted[0];
      const previous = sorted[1];
        (latest.status === 'Failed' || latest.status === 'Error') &&
        previous.status === 'Passed' &&
        !seenIds.has(latest.id)
        seenIds.add(latest.id);
          id: latest.id,
          testName,
          reason: 'regression',
          score: latest.score,
          status: latest.status,
          runDateTime: latest.runDateTime
  /** Flag tests with scores below 0.5 */
  private detectLowScores(runs: TestRunSummary[], alerts: TestAlert[]): void {
    const alertIds = new Set(alerts.map(a => a.id));
      if (run.score < 0.5 && run.status !== 'Running' && !alertIds.has(run.id)) {
          id: run.id,
          testName: run.testName,
          reason: 'low-score',
          score: run.score,
          status: run.status,
          runDateTime: run.runDateTime
  /** Flatten suite hierarchy and sort by worst health first */
  private flattenAndSort(nodes: SuiteHierarchyNode[]): SuiteHierarchyNode[] {
    const flat: SuiteHierarchyNode[] = [];
    this.collectNodes(nodes, flat);
    return flat.sort((a, b) => a.passRate - b.passRate);
  private collectNodes(nodes: SuiteHierarchyNode[], out: SuiteHierarchyNode[]): void {
      out.push(node);
        this.collectNodes(node.children, out);
