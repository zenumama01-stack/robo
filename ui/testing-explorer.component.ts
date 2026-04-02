  ViewContainerRef
import { takeUntil, debounceTime } from 'rxjs/operators';
import { MJTestEntity, MJTestSuiteEntity, MJTestSuiteTestEntity, MJTestTypeEntity, UserInfoEngine } from '@memberjunction/core-entities';
import { TestingDialogService } from '@memberjunction/ng-testing';
import { TestingInstrumentationService } from '../services/testing-instrumentation.service';
interface SuiteCardData {
  TestCount: number;
  PassRate: number;
  AvgScore: number;
  AvgDuration: number; // ms
  LastRunDate: Date | null;
  Tags: string[];
  Tests: SuiteTestItem[]; // first 4-5 for preview
  TotalTestsInSuite: number;
interface SuiteTestItem {
  TestID: string;
  TestName: string;
  LastStatus: string;
  LastScore: number;
interface TestCardData {
  TypeName: string;
  SuiteName: string;
  LastRunStatus: string;
  TotalRuns: number;
  EstDuration: number; // seconds
  EstCost: number;
  UpdatedAt: Date;
interface SidebarSelection {
  Type: 'all' | 'standalone' | 'suite' | 'testType';
  ID: string | null;
interface SuiteTreeNode {
  Children: SuiteTreeNode[];
type DisplayMode = 'all' | 'suites' | 'tests';
type ViewMode = 'card' | 'list';
type SortField = 'name' | 'updated' | 'status';
type SortDirection = 'asc' | 'desc';
/** Simple result type for test run stats queries */
interface TestRunStatRow {
  Score: number;
  CostUSD: number;
  StartedAt: string | Date;
  CompletedAt: string | Date | null;
  selector: 'app-testing-explorer',
        <mj-loading text="Loading test explorer..."></mj-loading>
      <div class="explorer-layout">
        <!-- Left Sidebar -->
        <aside class="sidebar" [class.collapsed]="IsSidebarCollapsed">
            <h3>Explorer</h3>
            <button class="sidebar-toggle" (click)="ToggleSidebar()">
              <i class="fa-solid" [class.fa-chevron-left]="!IsSidebarCollapsed" [class.fa-chevron-right]="IsSidebarCollapsed"></i>
          @if (!IsSidebarCollapsed) {
              <!-- Browse Section -->
                <div class="sidebar-section-title">Browse</div>
                  class="sidebar-item"
                  [class.active]="SelectedSidebar.Type === 'all'"
                  (click)="SelectSidebarItem({ Type: 'all', ID: null })"
                  <span>All Items</span>
                  <span class="sidebar-count">{{ TotalItemCount }}</span>
                  [class.active]="SelectedSidebar.Type === 'standalone'"
                  (click)="SelectSidebarItem({ Type: 'standalone', ID: null })"
                  <i class="fa-solid fa-vial"></i>
                  <span>Standalone Tests</span>
                  <span class="sidebar-count">{{ StandaloneTestCount }}</span>
              <!-- Test Suites Section -->
                <div class="sidebar-section-title">Test Suites</div>
                @for (node of FilteredSuiteTree; track node.ID) {
                  <ng-container
                    *ngTemplateOutlet="suiteTreeTpl; context: { node: node, depth: 0 }"
                  ></ng-container>
                @if (FilteredSuiteTree.length === 0) {
                  <div class="sidebar-empty">No suites found</div>
              <!-- Test Types Section -->
                <div class="sidebar-section-title">Test Types</div>
                @for (tt of FilteredTestTypes; track tt.ID) {
                    [class.active]="SelectedSidebar.Type === 'testType' && SelectedSidebar.ID === tt.ID"
                    (click)="SelectSidebarItem({ Type: 'testType', ID: tt.ID })"
                    <span>{{ tt.Name }}</span>
                    <span class="sidebar-count">{{ GetTestCountForType(tt.ID) }}</span>
              <div class="toolbar-search-box">
                  placeholder="Search tests and suites..."
                  (input)="OnSearchInput($event)"
                  <button class="clear-search" (click)="ClearSearch()">
              <div class="status-chips">
                @for (status of StatusOptions; track status) {
                    class="chip"
                    [class.active]="IsStatusActive(status)"
                    [attr.data-status]="status.toLowerCase()"
                    (click)="ToggleStatus(status)"
              <span class="result-count">{{ FilteredResultCount }} results</span>
                  [class.active]="ViewMode === 'card'"
                  (click)="SetViewMode('card')"
                  [class.active]="ViewMode === 'list'"
                  (click)="SetViewMode('list')"
              <button class="btn btn-secondary" (click)="OnNewSuite()">
                New Suite
              <button class="btn btn-primary" (click)="OnNewTest()">
                New Test
          <!-- Toggle Bar -->
          <div class="toggle-bar">
            <div class="toggle-group">
                [class.active]="DisplayMode === 'all'"
                (click)="SetDisplayMode('all')"
              >All</button>
                [class.active]="DisplayMode === 'suites'"
                (click)="SetDisplayMode('suites')"
              >Suites Only</button>
                [class.active]="DisplayMode === 'tests'"
                (click)="SetDisplayMode('tests')"
              >Tests Only</button>
            <div class="sort-indicator">
              <button class="sort-btn" (click)="ToggleSortDirection()">
                <i class="fa-solid fa-arrow-down-short-wide"></i>
                {{ SortFieldLabel }}
                <i class="fa-solid" [class.fa-arrow-up]="SortDirection === 'asc'" [class.fa-arrow-down]="SortDirection === 'desc'"></i>
            <!-- Suites Section -->
            @if (DisplayMode === 'all' || DisplayMode === 'suites') {
              @if (FilteredSuites.length > 0) {
                <div class="content-section">
                    Test Suites
                    <span class="section-count">{{ FilteredSuites.length }}</span>
                    @for (suite of FilteredSuites; track suite.ID) {
                      <div class="suite-card">
                            <i class="fa-solid fa-folder-open card-icon suite-icon"></i>
                            <span class="card-name" [innerHTML]="suite.Name | highlightSearch:SearchTerm"></span>
                            <span class="status-badge" [attr.data-status]="suite.Status.toLowerCase()">{{ suite.Status }}</span>
                            {{ suite.TestCount }} tests
                            @if (suite.LastRunDate) {
                              <span class="dot-sep"></span>
                              Last run {{ FormatRelativeTime(suite.LastRunDate) }}
                          @if (suite.Description) {
                            <p class="card-description" [innerHTML]="suite.Description | highlightSearch:SearchTerm"></p>
                        <div class="card-stats">
                            <span class="stat-value" [class]="GetScoreClass(suite.PassRate / 100)">{{ FormatPercent(suite.PassRate) }}</span>
                            <span class="stat-label">Tests</span>
                            <span class="stat-value">{{ suite.TestCount }}</span>
                            <span class="stat-value" [class]="GetScoreClass(suite.AvgScore)">{{ (suite.AvgScore * 100).toFixed(0) }}%</span>
                            <span class="stat-value">{{ FormatDuration(suite.AvgDuration) }}</span>
                        @if (suite.Tests.length > 0) {
                          <div class="card-tests-preview">
                            @for (t of suite.Tests; track t.TestID; let i = $index) {
                              @if (i < 4) {
                                <div class="preview-test-row">
                                  <span class="preview-dot" [attr.data-status]="t.LastStatus.toLowerCase()"></span>
                                  <span class="preview-test-name">{{ t.TestName }}</span>
                                  <span class="preview-score" [class]="GetScoreClass(t.LastScore)">{{ (t.LastScore * 100).toFixed(0) }}%</span>
                                  <span class="preview-bar">
                                    <span class="preview-bar-fill" [style.width.%]="t.LastScore * 100" [class]="GetScoreClass(t.LastScore) + '-bg'"></span>
                                  <span class="preview-status" [attr.data-status]="t.LastStatus.toLowerCase()">{{ t.LastStatus }}</span>
                            @if (suite.TotalTestsInSuite > 4) {
                              <div class="preview-more">+{{ suite.TotalTestsInSuite - 4 }} more tests</div>
                          <button class="btn btn-sm btn-primary" (click)="RunSuite(suite.ID)">
                            <i class="fa-solid fa-play"></i> Run Suite
                          <button class="btn btn-sm btn-secondary" (click)="ViewSuiteResults(suite.ID)">
                            <i class="fa-solid fa-chart-bar"></i> Results
                          <button class="btn btn-sm btn-secondary" (click)="EditItem('MJ: Test Suites', suite.ID)">
                            <i class="fa-solid fa-pen"></i> Edit
            <!-- Tests Section -->
            @if (DisplayMode === 'all' || DisplayMode === 'tests') {
              @if (FilteredTests.length > 0) {
                    Tests
                    <span class="section-count">{{ FilteredTests.length }}</span>
                    @for (test of FilteredTests; track test.ID) {
                      <div class="test-card">
                            <i class="fa-solid fa-vial card-icon test-icon"></i>
                            <span class="card-name" [innerHTML]="test.Name | highlightSearch:SearchTerm"></span>
                            <span class="status-badge" [attr.data-status]="test.Status.toLowerCase()">{{ test.Status }}</span>
                            {{ test.TypeName }}
                            @if (test.SuiteName) {
                              {{ test.SuiteName }}
                          @if (test.Description) {
                            <p class="card-description" [innerHTML]="test.Description | highlightSearch:SearchTerm"></p>
                            @if (test.TypeName) {
                              <span class="meta-item"><i class="fa-solid fa-robot"></i> {{ test.TypeName }}</span>
                            @if (test.EstDuration > 0) {
                              <span class="meta-item"><i class="fa-solid fa-clock"></i> ~{{ FormatDurationSeconds(test.EstDuration) }}</span>
                            @if (test.EstCost > 0) {
                              <span class="meta-item"><i class="fa-solid fa-dollar-sign"></i> {{ FormatCost(test.EstCost) }}</span>
                            <span class="meta-item"><i class="fa-solid fa-calendar"></i> {{ FormatRelativeTime(test.UpdatedAt) }}</span>
                          @if (test.Tags.length > 0) {
                            <div class="card-tags">
                              @for (tag of test.Tags; track tag; let i = $index) {
                                  <span class="tag">{{ tag }}</span>
                              @if (test.Tags.length > 4) {
                                <span class="tag tag-more">+{{ test.Tags.length - 4 }}</span>
                            <span class="stat-label">Last Status</span>
                            <span class="stat-value status-text" [attr.data-status]="test.LastRunStatus.toLowerCase()">{{ test.LastRunStatus || 'N/A' }}</span>
                            <span class="stat-label">Score</span>
                            <span class="stat-value" [class]="GetScoreClass(test.LastScore)">{{ test.LastScore > 0 ? (test.LastScore * 100).toFixed(0) + '%' : 'N/A' }}</span>
                            <span class="stat-value">{{ test.TotalRuns }}</span>
                            <span class="stat-value" [class]="GetScoreClass(test.PassRate / 100)">{{ test.TotalRuns > 0 ? FormatPercent(test.PassRate) : 'N/A' }}</span>
                          <button class="btn btn-sm btn-primary" (click)="RunTest(test.ID)">
                          <button class="btn btn-sm btn-secondary" (click)="ViewTestHistory(test.ID)">
                            <i class="fa-solid fa-history"></i> History
                          <button class="btn btn-sm btn-secondary" (click)="EditItem('MJ: Tests', test.ID)">
            @if (FilteredSuites.length === 0 && FilteredTests.length === 0) {
                <p>No tests or suites found</p>
                <span class="empty-hint">Try adjusting your search or filters.</span>
    <!-- Template for recursive suite tree rendering -->
    <ng-template #suiteTreeTpl let-node="node" let-depth="depth">
        class="sidebar-item suite-tree-item"
        [style.paddingLeft.px]="16 + depth * 14"
        [class.active]="SelectedSidebar.Type === 'suite' && SelectedSidebar.ID === node.ID"
        (click)="SelectSidebarItem({ Type: 'suite', ID: node.ID })"
        @if (node.Children.length > 0) {
          <button class="tree-toggle" (click)="ToggleSuiteExpand(node, $event)">
            <i class="fa-solid" [class.fa-chevron-right]="!node.Expanded" [class.fa-chevron-down]="node.Expanded"></i>
        <span class="tree-name">{{ node.Name }}</span>
        <span class="sidebar-count">{{ node.TestCount }}</span>
      @if (node.Expanded && node.Children.length > 0) {
        @for (child of node.Children; track child.ID) {
            *ngTemplateOutlet="suiteTreeTpl; context: { node: child, depth: depth + 1 }"
    <!-- Slideout Backdrop -->
          <!-- Slideout Header -->
            <div class="slideout-title-row">
              <i class="fa-solid fa-plus-circle slideout-title-icon"></i>
              <span class="slideout-title-text">
                Create {{ SlideoutCreateType === 'test' ? 'Test' : 'Test Suite' }}
            <button class="slideout-close-btn" (click)="CloseSlideout()">
          <!-- Type Toggle -->
          <div class="slideout-type-toggle">
              class="type-toggle-btn"
              [class.active]="SlideoutCreateType === 'test'"
              (click)="SetSlideoutCreateType('test')"
              <i class="fa-solid fa-vial"></i> Test
              [class.active]="SlideoutCreateType === 'suite'"
              (click)="SetSlideoutCreateType('suite')"
              <i class="fa-solid fa-folder"></i> Suite
          <!-- Error Banner -->
          @if (FormErrorMessage) {
            <div class="slideout-error">
              <span>{{ FormErrorMessage }}</span>
          <!-- Slideout Body -->
              <div class="form-section-title">General</div>
                <label class="form-label">Name <span class="form-required">*</span></label>
                  [(ngModel)]="FormName"
                  [placeholder]="SlideoutCreateType === 'test' ? 'e.g., Agent Response Quality Test' : 'e.g., Core Agent Test Suite'"
                  [(ngModel)]="FormDescription"
                  [placeholder]="SlideoutCreateType === 'test' ? 'What does this test evaluate?' : 'What does this suite contain?'"
                @if (SlideoutCreateType === 'test') {
                    <label class="form-label">Test Type <span class="form-required">*</span></label>
                    <select class="form-input" [(ngModel)]="FormTypeID">
                      <option value="" disabled>Select type...</option>
                      @for (tt of AllTestTypes; track tt.ID) {
                        <option [value]="tt.ID">{{ tt.Name }}</option>
                @if (SlideoutCreateType === 'suite') {
                    <label class="form-label">Parent Suite</label>
                    <select class="form-input" [(ngModel)]="FormParentSuiteID">
                      <option value="">None (top-level)</option>
                      @for (s of AllSuites; track s.ID) {
                        <option [value]="s.ID">{{ s.Name }}</option>
                  <select class="form-input" [(ngModel)]="FormStatus">
                    @for (status of FormStatusOptions; track status) {
                      <option [value]="status">{{ status }}</option>
                <div class="form-section-title">Estimates</div>
                    <label class="form-label">Duration (seconds)</label>
                      [(ngModel)]="FormEstDuration"
                    <label class="form-label">Cost (USD)</label>
                      [(ngModel)]="FormEstCost"
                      placeholder="0.00"
              <div class="form-section-title">Tags</div>
                  [(ngModel)]="FormTags"
                  placeholder="Comma-separated tags, e.g., agent, quality, v2"
                <span class="form-hint">Separate multiple tags with commas</span>
          <!-- Slideout Footer -->
            <button class="btn btn-secondary" (click)="CloseSlideout()">Cancel</button>
            <button class="btn btn-primary" (click)="SaveForm()" [disabled]="!IsFormValid || IsSaving">
              Create {{ SlideoutCreateType === 'test' ? 'Test' : 'Suite' }}
    /* ==========================================
       Testing Explorer Component
       ========================================== */
    .explorer-loading {
      background: #f5f6fa;
    .explorer-layout {
       Sidebar
      border-right: 1px solid #e8ecef;
      transition: width 0.2s ease, min-width 0.2s ease;
    .sidebar.collapsed {
      min-width: 48px;
      border-bottom: 1px solid #e8ecef;
      color: #2d3436;
    .sidebar.collapsed .sidebar-header h3 {
    .sidebar-toggle {
      border: 1px solid #e8ecef;
      color: #636e72;
    .sidebar-toggle:hover {
      padding: 8px 16px 4px;
      color: #b2bec3;
    .sidebar-item.active {
      background: #e0f2f1;
      color: #00897b;
    .sidebar-item i {
    .sidebar-item span:not(.sidebar-count) {
    .sidebar-count {
    .sidebar-item.active .sidebar-count {
    /* Tree nodes */
    .suite-tree-item {
    .tree-toggle {
    .tree-toggle:hover {
      background: #e8ecef;
    .tree-name {
       Main Content
    .toolbar-search-box {
      transition: border-color 0.2s ease;
    .toolbar-search-box:focus-within {
      border-color: #00897b;
    .toolbar-search-box i {
    .toolbar-search-box input {
    .toolbar-search-box input::placeholder {
    .status-chips {
    .chip {
    .chip:hover {
    .chip.active {
    .chip.active[data-status="active"] {
      background: #2e7d32;
    .chip.active[data-status="pending"] {
      background: #e65100;
      border-color: #e65100;
    .chip.active[data-status="disabled"] {
      background: #636e72;
      border-color: #636e72;
    .view-btn:not(:last-child) {
      background: #00897b;
    /* Buttons */
      padding: 9px 16px;
      background: #00796b;
      box-shadow: 0 2px 8px rgba(0, 137, 123, 0.3);
    /* Toggle Bar */
    .toggle-bar {
    .toggle-group {
    .toggle-btn:hover:not(.active) {
    .sort-indicator {
    .sort-btn {
    .sort-btn:hover {
    .sort-btn i {
      grid-template-columns: repeat(auto-fill, minmax(380px, 1fr));
    /* Suite Card */
    .suite-card,
    .test-card {
    .suite-card:hover,
    .test-card:hover {
      border-color: #b2dfdb;
      box-shadow: 0 4px 16px rgba(0, 137, 123, 0.1);
      padding: 16px 16px 12px;
    .status-badge[data-status="active"] {
    .status-badge[data-status="disabled"] {
    .dot-sep {
      background: #b2bec3;
    .card-tags {
    .tag {
    /* Card Stats */
    .card-stats {
      border-top: 1px solid #e8ecef;
    .stat-value.good { color: #2e7d32; }
    .stat-value.warn { color: #e65100; }
    .stat-value.bad { color: #c62828; }
    .status-text[data-status="passed"] { color: #2e7d32; }
    .status-text[data-status="failed"] { color: #c62828; }
    .status-text[data-status="error"] { color: #e65100; }
    .status-text[data-status="running"] { color: #1565c0; }
    .status-text[data-status="pending"] { color: #e65100; }
    .status-text[data-status="skipped"] { color: #636e72; }
    /* Suite Tests Preview */
    .card-tests-preview {
    .preview-test-row {
    .preview-dot {
    .preview-dot[data-status="passed"] { background: #2e7d32; }
    .preview-dot[data-status="failed"] { background: #c62828; }
    .preview-dot[data-status="error"] { background: #e65100; }
    .preview-dot[data-status="running"] { background: #1565c0; }
    .preview-dot[data-status=""] { background: #b2bec3; }
    .preview-test-name {
    .preview-score {
    .preview-score.good { color: #2e7d32; }
    .preview-score.warn { color: #e65100; }
    .preview-score.bad { color: #c62828; }
    .preview-bar {
    .preview-bar-fill {
    .good-bg { background: #2e7d32; }
    .warn-bg { background: #e65100; }
    .bad-bg { background: #c62828; }
    .preview-status {
    .preview-status[data-status="passed"] { color: #2e7d32; }
    .preview-status[data-status="failed"] { color: #c62828; }
    .preview-status[data-status="error"] { color: #e65100; }
    .preview-status[data-status=""] { color: #b2bec3; }
    .preview-more {
      padding: 4px 0 0;
      padding: 80px 40px;
       Slideout Panel
      background: rgba(0, 137, 123, 0.3);
      background: rgba(0, 137, 123, 0.5);
    .slideout-title-row {
    .slideout-title-icon {
    .slideout-title-text {
    .slideout-close-btn {
    .slideout-close-btn:hover {
    /* Type Toggle */
    .slideout-type-toggle {
    .type-toggle-btn {
    .type-toggle-btn:first-child {
      border-radius: 8px 0 0 8px;
    .type-toggle-btn:last-child {
    .type-toggle-btn.active {
    .type-toggle-btn:hover:not(.active) {
    .slideout-error {
      margin: 16px 24px 0;
    .slideout-error i {
    /* Slideout Body */
    .form-required {
      color: #e53e3e;
    .form-input:focus,
      box-shadow: 0 0 0 3px rgba(0, 137, 123, 0.1);
    .form-input::placeholder,
    .slideout-footer .btn {
    .slideout-footer .btn:disabled {
    ::ng-deep mark.search-highlight {
      background: #fff9c4;
      .toolbar-left, .toolbar-right {
export class TestingExplorerComponent implements OnInit, OnDestroy {
  // Raw cached data
  private allTests: MJTestEntity[] = [];
  AllSuites: MJTestSuiteEntity[] = [];
  private allSuiteTests: MJTestSuiteTestEntity[] = [];
  AllTestTypes: MJTestTypeEntity[] = [];
  private testRunStats: TestRunStatRow[] = [];
  // Computed card data
  private suiteCards: SuiteCardData[] = [];
  private testCards: TestCardData[] = [];
  // State subjects
  private _searchTerm$ = new BehaviorSubject<string>('');
  private _statusFilters$ = new BehaviorSubject<Set<string>>(new Set());
  private _viewMode$ = new BehaviorSubject<ViewMode>('card');
  private _displayMode$ = new BehaviorSubject<DisplayMode>('all');
  private _selectedSidebar$ = new BehaviorSubject<SidebarSelection>({ Type: 'all', ID: null });
  private _sortField$ = new BehaviorSubject<SortField>('name');
  private _sortDirection$ = new BehaviorSubject<SortDirection>('asc');
  // Template-bound state
  IsLoading = true;
  IsSidebarCollapsed = false;
  SearchTerm = '';
  StatusFilters = new Set<string>();
  ViewMode: ViewMode = 'card';
  DisplayMode: DisplayMode = 'all';
  SelectedSidebar: SidebarSelection = { Type: 'all', ID: null };
  SortField: SortField = 'name';
  SortDirection: SortDirection = 'asc';
  // Filtered results
  FilteredSuites: SuiteCardData[] = [];
  FilteredTests: TestCardData[] = [];
  FilteredSuiteTree: SuiteTreeNode[] = [];
  FilteredTestTypes: MJTestTypeEntity[] = [];
  // Counts
  TotalItemCount = 0;
  StandaloneTestCount = 0;
  FilteredResultCount = 0;
  readonly StatusOptions: string[] = ['Active', 'Pending', 'Disabled'];
  readonly FormStatusOptions: string[] = ['Active', 'Pending', 'Disabled'];
  private static readonly PANEL_WIDTH_KEY = 'Testing.ExplorerPanelWidth';
  private static readonly SEARCH_STATE_KEY = 'Testing.ExplorerSearchState';
  SlideoutOpen = false;
  SlideoutCreateType: 'test' | 'suite' = 'test';
  SlideoutWidth = 560;
  IsSaving = false;
  FormErrorMessage = '';
  FormName = '';
  FormDescription = '';
  FormTypeID = '';
  FormStatus: 'Active' | 'Pending' | 'Disabled' = 'Active';
  FormParentSuiteID = '';
  FormEstDuration = 0;
  FormEstCost = 0;
  FormTags = '';
  get IsFormValid(): boolean {
    if (!this.FormName.trim()) return false;
    if (this.SlideoutCreateType === 'test' && !this.FormTypeID) return false;
  get SortFieldLabel(): string {
      case 'name': return 'Name';
      case 'updated': return 'Updated';
      case 'status': return 'Status';
      default: return 'Name';
    private viewContainerRef: ViewContainerRef,
    private testingDialogService: TestingDialogService,
    public instrumentationService: TestingInstrumentationService
      await this.LoadData();
      console.error('TestingExplorerComponent: Failed to load data', err);
    this.subscribeToStateChanges();
    UserInfoEngine.Instance.FlushPendingSettings();
  // Public Methods
  async LoadData(): Promise<void> {
    this.loadCachedMetadata();
    await this.loadTestRunStats();
    this.buildSuiteCards();
    this.buildTestCards();
    this.buildSuiteTree();
    this.computeCounts();
  SelectSidebarItem(selection: SidebarSelection): void {
    this._selectedSidebar$.next(selection);
  ToggleStatus(status: string): void {
    const current = new Set(this._statusFilters$.value);
    if (current.has(status)) {
      current.delete(status);
      current.add(status);
    this._statusFilters$.next(current);
  IsStatusActive(status: string): boolean {
    return this.StatusFilters.has(status);
  SetDisplayMode(mode: DisplayMode): void {
    this._displayMode$.next(mode);
  SetViewMode(mode: ViewMode): void {
  OnSearchInput(event: Event): void {
    this._searchTerm$.next(input.value);
  ClearSearch(): void {
    this._searchTerm$.next('');
  ToggleSidebar(): void {
    this.IsSidebarCollapsed = !this.IsSidebarCollapsed;
  ToggleSuiteExpand(node: SuiteTreeNode, event: MouseEvent): void {
    node.Expanded = !node.Expanded;
  ToggleSortDirection(): void {
    const newDir = this.SortDirection === 'asc' ? 'desc' : 'asc';
    this._sortDirection$.next(newDir);
  RunTest(testId: string): void {
    this.testingDialogService.OpenTestDialog(testId, this.viewContainerRef);
  RunSuite(suiteId: string): void {
    this.testingDialogService.OpenSuiteDialog(suiteId, this.viewContainerRef);
  EditItem(entityName: string, id: string): void {
    SharedService.Instance.OpenEntityRecord(entityName, CompositeKey.FromID(id));
  ViewTestHistory(testId: string): void {
  ViewSuiteResults(suiteId: string): void {
  OnNewTest(): void {
    this.OpenSlideout('test');
  OnNewSuite(): void {
    this.OpenSlideout('suite');
  OpenSlideout(type: 'test' | 'suite'): void {
    this.SlideoutCreateType = type;
  CloseSlideout(): void {
    this.FormErrorMessage = '';
  SetSlideoutCreateType(type: 'test' | 'suite'): void {
  async SaveForm(): Promise<void> {
    if (!this.IsFormValid || this.IsSaving) return;
      if (this.SlideoutCreateType === 'test') {
        await this.saveNewTest();
        await this.saveNewSuite();
      await TestEngineBase.Instance.Config(true);
      const message = err instanceof Error ? err.message : 'An error occurred while saving.';
      this.FormErrorMessage = message;
      console.error('TestingExplorerComponent: Save failed', err);
  // ── Resize ──────────────────────────────────────────
  OnResizeStart(event: MouseEvent): void {
      TestingExplorerComponent.PANEL_WIDTH_KEY,
  GetTestCountForType(typeId: string): number {
    return this.allTests.filter(t => t.TypeID === typeId).length;
    if (ms <= 0) return '0s';
  FormatDurationSeconds(seconds: number): string {
    if (seconds <= 0) return '0s';
    if (minutes > 0) return `${minutes}m ${secs}s`;
  FormatRelativeTime(date: Date | null): string {
    const diffMs = now - d.getTime();
    const diffMinutes = Math.floor(diffMs / 60000);
    if (diffMinutes < 1) return 'Just now';
    if (diffMinutes < 60) return `${diffMinutes}m ago`;
    return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
  FormatPercent(value: number): string {
    return `${Math.round(value)}%`;
    if (cost < 0.01) return '$0.00';
  GetScoreClass(score: number): string {
    if (score >= 0.7) return 'good';
    if (score >= 0.4) return 'warn';
    return 'bad';
  GetStatusClass(status: string): string {
      case 'passed': return 'status-passed';
      case 'failed': return 'status-failed';
      case 'error': return 'status-error';
      case 'running': return 'status-running';
      default: return 'status-default';
  // Private: Data Loading
  private loadCachedMetadata(): void {
    const engine = TestEngineBase.Instance;
    this.allTests = engine.Tests;
    this.AllSuites = engine.TestSuites;
    this.allSuiteTests = engine.TestSuiteTests;
    this.AllTestTypes = engine.TestTypes;
  private async loadTestRunStats(): Promise<void> {
    const result = await rv.RunView<TestRunStatRow>({
      Fields: ['ID', 'TestID', 'Status', 'Score', 'CostUSD', 'StartedAt', 'CompletedAt'],
    this.testRunStats = result.Success ? (result.Results || []) : [];
  // Private: Build Card Data
  private buildSuiteCards(): void {
    this.suiteCards = this.AllSuites.map(suite => this.buildSingleSuiteCard(suite));
  private buildSingleSuiteCard(suite: MJTestSuiteEntity): SuiteCardData {
    const suiteTestLinks = this.allSuiteTests.filter(st => st.SuiteID === suite.ID);
    const testIds = new Set(suiteTestLinks.map(st => st.TestID));
    const runsForSuite = this.testRunStats.filter(r => testIds.has(r.TestID));
    const passedRuns = runsForSuite.filter(r => r.Status === 'Passed');
    const passRate = runsForSuite.length > 0 ? (passedRuns.length / runsForSuite.length) * 100 : 0;
    const avgScore = runsForSuite.length > 0
      ? runsForSuite.reduce((s, r) => s + (r.Score || 0), 0) / runsForSuite.length
    const avgDuration = this.computeAvgDuration(runsForSuite);
    const lastRunDate = this.findLastRunDate(runsForSuite);
    const previewTests = this.buildSuiteTestPreviews(suiteTestLinks);
      ID: suite.ID,
      Name: suite.Name,
      Description: suite.Description || '',
      Status: suite.Status,
      TestCount: testIds.size,
      PassRate: passRate,
      AvgScore: avgScore,
      AvgDuration: avgDuration,
      LastRunDate: lastRunDate,
      Tags: this.parseTags(suite.Tags),
      Tests: previewTests,
      TotalTestsInSuite: testIds.size
  private buildSuiteTestPreviews(suiteTestLinks: MJTestSuiteTestEntity[]): SuiteTestItem[] {
    return suiteTestLinks
      .sort((a, b) => a.Sequence - b.Sequence)
      .map(st => {
        const test = this.allTests.find(t => t.ID === st.TestID);
        const lastRun = this.findLastRunForTest(st.TestID);
          TestID: st.TestID,
          TestName: test?.Name || st.Test || 'Unknown',
          LastStatus: lastRun?.Status || '',
          LastScore: lastRun?.Score || 0
  private buildTestCards(): void {
    this.testCards = this.allTests.map(test => this.buildSingleTestCard(test));
  private buildSingleTestCard(test: MJTestEntity): TestCardData {
    const runsForTest = this.testRunStats.filter(r => r.TestID === test.ID);
    const lastRun = runsForTest.length > 0 ? runsForTest[0] : null; // already sorted DESC
    const passedRuns = runsForTest.filter(r => r.Status === 'Passed');
    const passRate = runsForTest.length > 0 ? (passedRuns.length / runsForTest.length) * 100 : 0;
    const typeName = this.AllTestTypes.find(t => t.ID === test.TypeID)?.Name || '';
    const suiteName = this.findSuiteNameForTest(test.ID);
      ID: test.ID,
      Name: test.Name,
      Description: test.Description || '',
      Status: test.Status,
      TypeName: typeName,
      SuiteName: suiteName,
      LastRunStatus: lastRun?.Status || '',
      LastScore: lastRun?.Score || 0,
      TotalRuns: runsForTest.length,
      EstDuration: test.EstimatedDurationSeconds || 0,
      EstCost: test.EstimatedCostUSD || 0,
      LastRunDate: lastRun ? this.toDate(lastRun.StartedAt) : null,
      Tags: this.parseTags(test.Tags),
      UpdatedAt: test.__mj_UpdatedAt
  // Private: Build Suite Tree
  private buildSuiteTree(): void {
    const filteredSuites = this.AllSuites;
    const nodeMap = new Map<string, SuiteTreeNode>();
    for (const suite of filteredSuites) {
      nodeMap.set(suite.ID, {
        ParentID: suite.ParentID,
        TestCount: this.allSuiteTests.filter(st => st.SuiteID === suite.ID).length,
        Children: [],
        Expanded: false
    const roots: SuiteTreeNode[] = [];
    nodeMap.forEach(node => {
      if (node.ParentID && nodeMap.has(node.ParentID)) {
        nodeMap.get(node.ParentID)!.Children.push(node);
    this.FilteredSuiteTree = roots;
    this.filterTestTypes();
  private filterTestTypes(): void {
    this.FilteredTestTypes = this.AllTestTypes;
  // Private: Filtering & Sorting
  private subscribeToStateChanges(): void {
      this._searchTerm$,
      this._statusFilters$,
      this._displayMode$,
      this._selectedSidebar$,
      this._sortField$,
      this._sortDirection$,
      this._viewMode$
    ).subscribe(([searchTerm, statusFilters, displayMode, selectedSidebar, sortField, sortDirection, viewMode]) => {
      this.SearchTerm = searchTerm;
      this.StatusFilters = statusFilters;
      this.DisplayMode = displayMode;
      this.SelectedSidebar = selectedSidebar;
      this.SortField = sortField;
      this.SortDirection = sortDirection;
      this.ViewMode = viewMode;
    this.FilteredSuites = this.filterSuiteCards();
    this.FilteredTests = this.filterTestCards();
    this.FilteredResultCount = this.FilteredSuites.length + this.FilteredTests.length;
  private filterSuiteCards(): SuiteCardData[] {
    let result = [...this.suiteCards];
    result = this.applySidebarFilterToSuites(result);
    result = this.applySearchFilterToSuites(result);
    result = this.applyStatusFilterToSuites(result);
    result = this.sortSuiteCards(result);
  private filterTestCards(): TestCardData[] {
    let result = [...this.testCards];
    result = this.applySidebarFilterToTests(result);
    result = this.applySearchFilterToTests(result);
    result = this.applyStatusFilterToTests(result);
    result = this.sortTestCards(result);
  private applySidebarFilterToSuites(suites: SuiteCardData[]): SuiteCardData[] {
    const sel = this.SelectedSidebar;
    switch (sel.Type) {
      case 'suite':
        return suites.filter(s => s.ID === sel.ID);
      case 'standalone':
        return []; // standalone = tests only
      case 'testType':
        return []; // test types apply to tests only
        return suites;
  private applySidebarFilterToTests(tests: TestCardData[]): TestCardData[] {
      case 'suite': {
        const testIds = new Set(
          this.allSuiteTests.filter(st => st.SuiteID === sel.ID).map(st => st.TestID)
        return tests.filter(t => testIds.has(t.ID));
      case 'standalone': {
        const testIdsInSuites = new Set(this.allSuiteTests.map(st => st.TestID));
        return tests.filter(t => !testIdsInSuites.has(t.ID));
        return tests.filter(t => {
          const testEntity = this.allTests.find(te => te.ID === t.ID);
          return testEntity?.TypeID === sel.ID;
  private applySearchFilterToSuites(suites: SuiteCardData[]): SuiteCardData[] {
    const term = this.SearchTerm.toLowerCase().trim();
    if (!term) return suites;
    return suites.filter(s =>
      s.Name.toLowerCase().includes(term) ||
      s.Description.toLowerCase().includes(term)
  private applySearchFilterToTests(tests: TestCardData[]): TestCardData[] {
    if (!term) return tests;
    return tests.filter(t =>
      t.Name.toLowerCase().includes(term) ||
      t.Description.toLowerCase().includes(term)
  private applyStatusFilterToSuites(suites: SuiteCardData[]): SuiteCardData[] {
    if (this.StatusFilters.size === 0) return suites;
    return suites.filter(s => this.StatusFilters.has(s.Status));
  private applyStatusFilterToTests(tests: TestCardData[]): TestCardData[] {
    if (this.StatusFilters.size === 0) return tests;
    return tests.filter(t => this.StatusFilters.has(t.Status));
  private sortSuiteCards(suites: SuiteCardData[]): SuiteCardData[] {
    const dir = this.SortDirection === 'asc' ? 1 : -1;
    return suites.sort((a, b) => this.compareBySortField(a.Name, a.Status, a.LastRunDate, b.Name, b.Status, b.LastRunDate, dir));
  private sortTestCards(tests: TestCardData[]): TestCardData[] {
    return tests.sort((a, b) => this.compareBySortField(a.Name, a.Status, a.UpdatedAt, b.Name, b.Status, b.UpdatedAt, dir));
  private compareBySortField(
    aName: string, aStatus: string, aDate: Date | null,
    bName: string, bStatus: string, bDate: Date | null,
    dir: number
        return aName.localeCompare(bName) * dir;
        return aStatus.localeCompare(bStatus) * dir;
      case 'updated': {
        const aTime = aDate ? new Date(aDate).getTime() : 0;
        const bTime = bDate ? new Date(bDate).getTime() : 0;
        return (aTime - bTime) * dir;
  // Private: Helpers
  private computeCounts(): void {
    this.StandaloneTestCount = this.allTests.filter(t => !testIdsInSuites.has(t.ID)).length;
    this.TotalItemCount = this.allTests.length + this.AllSuites.length;
  private computeAvgDuration(runs: TestRunStatRow[]): number {
    const completed = runs.filter(r => r.CompletedAt != null && r.StartedAt != null);
    if (completed.length === 0) return 0;
    const totalMs = completed.reduce((sum, r) => {
      const start = this.toDate(r.StartedAt).getTime();
      const end = this.toDate(r.CompletedAt!).getTime();
    return totalMs / completed.length;
  private findLastRunDate(runs: TestRunStatRow[]): Date | null {
    if (runs.length === 0) return null;
    return this.toDate(runs[0].StartedAt);
  private findLastRunForTest(testId: string): TestRunStatRow | null {
    return this.testRunStats.find(r => r.TestID === testId) || null;
  private findSuiteNameForTest(testId: string): string {
    const suiteTest = this.allSuiteTests.find(st => st.TestID === testId);
    if (!suiteTest) return '';
    const suite = this.AllSuites.find(s => s.ID === suiteTest.SuiteID);
    return suite?.Name || suiteTest.Suite || '';
  private parseTags(tagsJson: string | null): string[] {
    if (!tagsJson) return [];
      const parsed = JSON.parse(tagsJson);
  private toDate(value: string | Date | null): Date {
    if (!value) return new Date(0);
    return value instanceof Date ? value : new Date(value);
  // Private: User Settings
      const widthStr = UserInfoEngine.Instance.GetSetting(TestingExplorerComponent.PANEL_WIDTH_KEY);
      const stateStr = UserInfoEngine.Instance.GetSetting(TestingExplorerComponent.SEARCH_STATE_KEY);
        if (state.searchTerm) {
          this.SearchTerm = state.searchTerm;
          this._searchTerm$.next(state.searchTerm);
      console.warn('[TestingExplorer] Failed to load user settings:', error);
      const state = { searchTerm: this.SearchTerm };
        TestingExplorerComponent.SEARCH_STATE_KEY,
      console.warn('[TestingExplorer] Failed to persist search state:', error);
  // Private: Form Management
    this.FormName = '';
    this.FormDescription = '';
    this.FormTypeID = this.AllTestTypes.length > 0 ? this.AllTestTypes[0].ID : '';
    this.FormStatus = 'Active';
    this.FormParentSuiteID = '';
    this.FormEstDuration = 0;
    this.FormEstCost = 0;
    this.FormTags = '';
  private async saveNewTest(): Promise<void> {
    test.NewRecord();
    test.Name = this.FormName.trim();
    test.Description = this.FormDescription.trim() || null;
    test.TypeID = this.FormTypeID;
    test.Status = this.FormStatus;
    if (this.FormEstDuration > 0) test.EstimatedDurationSeconds = this.FormEstDuration;
    if (this.FormEstCost > 0) test.EstimatedCostUSD = this.FormEstCost;
    if (this.FormTags.trim()) {
      const tags = this.FormTags.split(',').map(t => t.trim()).filter(t => t.length > 0);
      test.Tags = JSON.stringify(tags);
    const saved = await test.Save();
    if (!saved) throw new Error('Failed to save test. Please check your input and try again.');
  private async saveNewSuite(): Promise<void> {
    suite.NewRecord();
    suite.Name = this.FormName.trim();
    suite.Description = this.FormDescription.trim() || null;
    suite.Status = this.FormStatus;
    if (this.FormParentSuiteID) suite.ParentID = this.FormParentSuiteID;
      suite.Tags = JSON.stringify(tags);
    const saved = await suite.Save();
    if (!saved) throw new Error('Failed to save test suite. Please check your input and try again.');
