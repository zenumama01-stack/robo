import { Subject, Subscription } from 'rxjs';
import { SchedulingInstrumentationService } from './services/scheduling-instrumentation.service';
interface SchedulingDashboardState {
  dashboardState: Record<string, unknown>;
  jobsState: Record<string, unknown>;
  activityState: Record<string, unknown>;
  selector: 'mj-scheduling-dashboard',
  templateUrl: './scheduling-dashboard.component.html',
  styleUrls: ['./scheduling-dashboard.component.css'],
@RegisterClass(BaseDashboard, 'SchedulingDashboard')
export class SchedulingDashboardComponent extends BaseDashboard implements AfterViewInit, OnDestroy {
  public ActiveTab = 'dashboard';
  public DashboardState: Record<string, unknown> = {};
  public JobsState: Record<string, unknown> = {};
  public ActivityState: Record<string, unknown> = {};
  private stateChangeSubject = new Subject<SchedulingDashboardState>();
  public ActiveJobCount = 0;
  public AlertCount = 0;
  private kpiSub: Subscription | undefined;
  private alertSub: Subscription | undefined;
  public NavigationItems = [
    { id: 'dashboard', text: 'Dashboard', icon: 'fa-solid fa-gauge-high' },
    { id: 'jobs', text: 'Jobs', icon: 'fa-solid fa-calendar-check' },
    { id: 'activity', text: 'Activity', icon: 'fa-solid fa-clock-rotate-left' }
    private schedulingService: SchedulingInstrumentationService
    return 'Scheduling';
    this.visitedTabs.add(this.ActiveTab);
    this.loadSidebarCounts();
    if (this.kpiSub) this.kpiSub.unsubscribe();
    if (this.alertSub) this.alertSub.unsubscribe();
  private loadSidebarCounts(): void {
    this.kpiSub = this.schedulingService.kpis$.subscribe(kpis => {
      this.ActiveJobCount = kpis.totalActiveJobs;
    this.alertSub = this.schedulingService.alerts$.subscribe(alerts => {
      this.AlertCount = alerts.length;
  public OnTabChange(tabId: string): void {
    this.ActiveTab = tabId;
  public HasVisited(tabId: string): boolean {
    this.stateChangeSubject.next({
      activeTab: this.ActiveTab,
      dashboardState: this.DashboardState,
      jobsState: this.JobsState,
      activityState: this.ActivityState
  public OnDashboardStateChange(state: Record<string, unknown>): void {
    this.DashboardState = state;
  public OnJobsStateChange(state: Record<string, unknown>): void {
    this.JobsState = state;
  public OnActivityStateChange(state: Record<string, unknown>): void {
    this.ActivityState = state;
  public GetCurrentTabLabel(): string {
    const item = this.NavigationItems.find(n => n.id === this.ActiveTab);
    return item ? item.text : 'Scheduling';
      const state = this.Config.userState as Partial<SchedulingDashboardState>;
        this.ActiveTab = state.activeTab;
      if (state.dashboardState) this.DashboardState = state.dashboardState;
      if (state.jobsState) this.JobsState = state.jobsState;
      if (state.activityState) this.ActivityState = state.activityState;
