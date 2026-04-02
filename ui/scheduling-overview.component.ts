import { Subscription, interval } from 'rxjs';
  SchedulingKPIs,
  UpcomingExecution,
  LockInfo,
  AlertCondition
  selector: 'app-scheduling-overview',
  templateUrl: './scheduling-overview.component.html',
  styleUrls: ['./scheduling-overview.component.css'],
export class SchedulingOverviewComponent implements OnInit, OnDestroy {
  public Kpis: SchedulingKPIs | null = null;
  public LiveExecutions: JobExecution[] = [];
  public UpcomingExecutions: UpcomingExecution[] = [];
  public Locks: LockInfo[] = [];
  public Alerts: AlertCondition[] = [];
  public AutoRefreshEnabled = true;
  private autoRefreshSub: Subscription | undefined;
      if (this.initialState['autoRefreshEnabled'] != null) {
        this.AutoRefreshEnabled = this.initialState['autoRefreshEnabled'] as boolean;
      this.schedulingService.kpis$.subscribe(kpis => {
        this.Kpis = kpis;
      this.schedulingService.liveExecutions$.subscribe(execs => {
        this.LiveExecutions = execs;
      this.schedulingService.upcomingExecutions$.subscribe(upcoming => {
        this.UpcomingExecutions = upcoming;
      this.schedulingService.lockInfo$.subscribe(locks => {
        this.Locks = locks;
      this.schedulingService.alerts$.subscribe(alerts => {
        this.Alerts = alerts;
    if (this.AutoRefreshEnabled) {
  public ToggleAutoRefresh(): void {
    this.AutoRefreshEnabled = !this.AutoRefreshEnabled;
  public async ReleaseLock(jobId: string): Promise<void> {
    await this.schedulingService.releaseLock(jobId);
  public GetAlertIcon(severity: string): string {
    return severity === 'error' ? 'fa-solid fa-circle-xmark' : 'fa-solid fa-triangle-exclamation';
    const remainingMinutes = minutes % 60;
    return `${hours}h ${remainingMinutes}m`;
  public FormatTimeAgo(date: Date): string {
    const diffSec = Math.floor(diffMs / 1000);
    if (diffSec < 60) return `${diffSec}s ago`;
    const diffMin = Math.floor(diffSec / 60);
    if (diffMin < 60) return `${diffMin}m ago`;
    const diffHr = Math.floor(diffMin / 60);
    if (diffHr < 24) return `${diffHr}h ago`;
  public FormatTimeUntil(date: Date): string {
    const diffMs = date.getTime() - now.getTime();
    if (diffMs <= 0) return 'Now';
    const diffMin = Math.floor(diffMs / 60000);
    if (diffMin < 60) return `${diffMin}m`;
    const remainingMin = diffMin % 60;
    return `${diffHr}h ${remainingMin}m`;
  public FormatCost(cost: number): string {
      month: 'numeric',
      year: '2-digit',
      hour: 'numeric',
      hour12: true
  public GetHealthScore(): number {
    if (!this.Kpis) return 100;
    score -= this.Kpis.totalFailures7d * 2;
    score -= this.Kpis.lockedJobs * 10;
    score -= this.Alerts.filter(a => a.severity === 'error').length * 15;
    score -= this.Alerts.filter(a => a.severity === 'warning').length * 5;
  public GetHealthColor(): string {
    const score = this.GetHealthScore();
    if (score >= 80) return '#10b981';
    if (score >= 50) return '#f59e0b';
  public GetHealthStrokeDasharray(): string {
    const circumference = 2 * Math.PI * 36;
    const filled = (score / 100) * circumference;
    return `${filled} ${circumference - filled}`;
  private startAutoRefresh(): void {
    this.autoRefreshSub = interval(30000).subscribe(() => {
  private stopAutoRefresh(): void {
    if (this.autoRefreshSub) {
      this.autoRefreshSub.unsubscribe();
      this.autoRefreshSub = undefined;
      autoRefreshEnabled: this.AutoRefreshEnabled
