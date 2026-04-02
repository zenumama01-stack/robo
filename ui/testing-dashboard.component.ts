interface TestingDashboardState {
  runsState: Record<string, unknown>;
  analyticsState: Record<string, unknown>;
  reviewState: Record<string, unknown>;
  selector: 'mj-testing-dashboard',
  templateUrl: './testing-dashboard.component.html',
  styleUrls: ['./testing-dashboard.component.css'],
@RegisterClass(BaseDashboard, 'TestingDashboard')
export class TestingDashboardComponent extends BaseDashboard implements AfterViewInit, OnDestroy {
  public activeTab = 'dashboard';
  // Component states
  public dashboardState: Record<string, unknown> | null = null;
  public runsState: Record<string, unknown> | null = null;
  public analyticsState: Record<string, unknown> | null = null;
  public reviewState: Record<string, unknown> | null = null;
  public navigationItems: string[] = ['dashboard', 'runs', 'analytics', 'review'];
  public navigationConfig = [
    { text: 'Dashboard', icon: 'fa-solid fa-gauge-high', selected: false },
    { text: 'Runs', icon: 'fa-solid fa-play-circle', selected: false },
    { text: 'Analytics', icon: 'fa-solid fa-chart-bar', selected: false },
    { text: 'Review', icon: 'fa-solid fa-clipboard-check', selected: false }
  private stateChangeSubject = new Subject<TestingDashboardState>();
    this.updateNavigationSelection();
    return 'Testing';
    const state: TestingDashboardState = {
      activeTab: this.activeTab,
      dashboardState: (this.dashboardState || {}) as Record<string, unknown>,
      runsState: (this.runsState || {}) as Record<string, unknown>,
      analyticsState: (this.analyticsState || {}) as Record<string, unknown>,
      reviewState: (this.reviewState || {}) as Record<string, unknown>
  public onDashboardStateChange(state: Record<string, unknown>): void {
    this.dashboardState = state;
  public onRunsStateChange(state: Record<string, unknown>): void {
    this.runsState = state;
  public onAnalyticsStateChange(state: Record<string, unknown>): void {
    this.analyticsState = state;
  public onReviewStateChange(state: Record<string, unknown>): void {
    this.reviewState = state;
  public loadUserState(state: Partial<TestingDashboardState>): void {
    if (state.dashboardState) this.dashboardState = state.dashboardState;
    if (state.runsState) this.runsState = state.runsState;
    if (state.analyticsState) this.analyticsState = state.analyticsState;
    if (state.reviewState) this.reviewState = state.reviewState;
      console.error('Error initializing Testing dashboard:', error);
      this.Error.emit(new Error('Failed to initialize Testing dashboard. Please try again.'));
    return tabIndex >= 0 ? this.navigationConfig[tabIndex].text : 'Testing Dashboard';
  private updateNavigationSelection(): void {
    this.navigationConfig.forEach((item, index) => {
      item.selected = this.navigationItems[index] === this.activeTab;
