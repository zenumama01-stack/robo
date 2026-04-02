import { MJScheduledJobEntity, MJScheduledJobRunEntity, MJScheduledJobTypeEntity } from '@memberjunction/core-entities';
export interface SchedulingKPIs {
  totalActiveJobs: number;
  jobsDueInNextHour: number;
  recentExecutions24h: number;
  successRate24h: number;
  lockedJobs: number;
  totalCost24h: number;
  failureRate7d: number;
  totalFailures7d: number;
export interface JobExecution {
  jobId: string;
  jobName: string;
  jobType: string;
  status: 'Running' | 'Completed' | 'Failed' | 'Cancelled' | 'Timeout';
  success?: boolean;
export interface UpcomingExecution {
  nextRunAt: Date;
  cronExpression: string;
export interface JobStatistics {
  jobTypeId: string;
  description: string | null;
  lastRunAt?: Date;
  nextRunAt?: Date;
  concurrencyMode: string;
  configuration: string | null;
  ownerUserID: string | null;
  ownerUser: string | null;
  notifyOnSuccess: boolean;
  notifyOnFailure: boolean;
  startAt?: Date;
  endAt?: Date;
export interface JobTypeStatistics {
  activeJobsCount: number;
export interface ExecutionTrendData {
export interface LockInfo {
  lockToken: string;
  lockedAt: Date;
  lockedBy: string;
  expectedCompletion: Date;
  isStale: boolean;
export interface AlertCondition {
  type: 'stale-lock' | 'high-failure' | 'job-expired';
  severity: 'warning' | 'error';
  jobId?: string;
  jobName?: string;
export class SchedulingInstrumentationService {
    start: new Date(Date.now() - 24 * 60 * 60 * 1000),
    tap(() => this._isLoading$.next(false)),
  readonly liveExecutions$ = this._refreshTrigger$.pipe(
  readonly upcomingExecutions$ = this._refreshTrigger$.pipe(
    switchMap(() => from(this.loadUpcomingExecutions())),
  readonly executionHistory$ = combineLatest([this._refreshTrigger$, this._dateRange$]).pipe(
    switchMap(() => from(this.loadExecutionHistory())),
  readonly executionTrends$ = combineLatest([this._refreshTrigger$, this._dateRange$]).pipe(
    switchMap(() => from(this.loadExecutionTrends())),
  readonly jobStatistics$ = this._refreshTrigger$.pipe(
    switchMap(() => from(this.loadJobStatistics())),
  readonly jobTypes$ = this._refreshTrigger$.pipe(
    switchMap(() => from(this.loadJobTypes())),
  readonly lockInfo$ = this._refreshTrigger$.pipe(
    switchMap(() => from(this.loadLockInfo())),
  readonly alerts$ = combineLatest([this.lockInfo$, this.kpis$, this.jobStatistics$]).pipe(
    switchMap(([locks, kpis, jobs]) => from(this.buildAlerts(locks, kpis, jobs))),
  get CurrentDateRange(): { start: Date; end: Date } {
    return this._dateRange$.value;
  // ── KPIs ──────────────────────────────────────────────────
  private async loadKPIs(): Promise<SchedulingKPIs> {
    const oneHourFromNow = new Date(now.getTime() + 60 * 60 * 1000);
    const sevenDaysAgo = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
    const [jobsResult, runsResult, runs7dResult] = await rv.RunViews([
        EntityName: 'MJ: Scheduled Jobs',
        ExtraFilter: "Status='Active'",
        EntityName: 'MJ: Scheduled Job Runs',
        ExtraFilter: `StartedAt >= '${start.toISOString()}' AND StartedAt <= '${end.toISOString()}'`,
        ExtraFilter: `StartedAt >= '${sevenDaysAgo.toISOString()}'`,
    const jobs = jobsResult.Results as MJScheduledJobEntity[];
    const runs24h = runsResult.Results as MJScheduledJobRunEntity[];
    const runs7d = runs7dResult.Results as MJScheduledJobRunEntity[];
    const jobsDueInNextHour = jobs.filter(j => {
      if (!j.NextRunAt) return false;
      const nextRun = new Date(j.NextRunAt);
      return nextRun >= now && nextRun <= oneHourFromNow;
    const successfulRuns24h = runs24h.filter(r => r.Success).length;
    const currentlyRunning = runs24h.filter(r => r.Status === 'Running').length;
    let totalCost24h = 0;
    for (const run of runs24h) {
      totalCost24h += this.extractCostFromRun(run);
    const failedRuns7d = runs7d.filter(r => !r.Success && r.Status !== 'Running');
      totalActiveJobs: jobs.length,
      jobsDueInNextHour,
      recentExecutions24h: runs24h.length,
      successRate24h: runs24h.length > 0 ? successfulRuns24h / runs24h.length : 0,
      currentlyRunning,
      lockedJobs: jobs.filter(j => j.LockToken != null).length,
      totalCost24h,
      failureRate7d: runs7d.length > 0 ? failedRuns7d.length / runs7d.length : 0,
      totalFailures7d: failedRuns7d.length
  private extractCostFromRun(run: MJScheduledJobRunEntity): number {
    if (!run.Details) return 0;
      const details = JSON.parse(run.Details);
      return (details.Cost || 0) + (details.TotalCost || 0);
  // ── Live Executions ───────────────────────────────────────
  private async loadLiveExecutions(): Promise<JobExecution[]> {
    const result = await rv.RunView<MJScheduledJobRunEntity>({
    if (!result.Success) return [];
    return this.mapRunsToExecutions(result.Results || [], now);
  // ── Upcoming Executions ───────────────────────────────────
  private async loadUpcomingExecutions(): Promise<UpcomingExecution[]> {
    const next24Hours = new Date(now.getTime() + 24 * 60 * 60 * 1000);
    const result = await rv.RunView<MJScheduledJobEntity>({
      ExtraFilter: `Status='Active' AND NextRunAt IS NOT NULL AND NextRunAt >= '${now.toISOString()}' AND NextRunAt <= '${next24Hours.toISOString()}'`,
      OrderBy: 'NextRunAt ASC',
    return (result.Results || []).map(job => ({
      jobId: job.ID,
      jobName: job.Name,
      jobType: job.JobType || 'Unknown',
      nextRunAt: new Date(job.NextRunAt!),
      cronExpression: job.CronExpression,
      timezone: job.Timezone || 'UTC'
  // ── Execution History ─────────────────────────────────────
  private async loadExecutionHistory(): Promise<JobExecution[]> {
    return this.mapRunsToExecutions(result.Results || []);
  // ── Execution Trends ──────────────────────────────────────
  private async loadExecutionTrends(): Promise<ExecutionTrendData[]> {
    const buckets = this.createTimeBuckets(start, end);
    const allRuns = result.Results || [];
    const bucketSizeMs = this.getBucketSizeMs(start, end);
    return buckets.map(bucket => {
      const runsInBucket = allRuns.filter(r => {
        const started = new Date(r.StartedAt);
        return started >= bucket && started < bucketEnd;
        executions: runsInBucket.length,
        successes: runsInBucket.filter(r => r.Success).length,
        failures: runsInBucket.filter(r => !r.Success && r.Status !== 'Running').length
  // ── Job Statistics ────────────────────────────────────────
  private async loadJobStatistics(): Promise<JobStatistics[]> {
      jobTypeId: job.JobTypeID,
      status: job.Status,
      description: job.Description,
      timezone: job.Timezone || 'UTC',
      totalRuns: job.RunCount || 0,
      successCount: job.SuccessCount || 0,
      failureCount: job.FailureCount || 0,
      successRate: job.RunCount > 0 ? (job.SuccessCount || 0) / job.RunCount : 0,
      lastRunAt: job.LastRunAt ? new Date(job.LastRunAt) : undefined,
      nextRunAt: job.NextRunAt ? new Date(job.NextRunAt) : undefined,
      concurrencyMode: job.ConcurrencyMode,
      configuration: job.Configuration,
      ownerUserID: job.OwnerUserID,
      ownerUser: job.OwnerUser,
      notifyOnSuccess: job.NotifyOnSuccess,
      notifyOnFailure: job.NotifyOnFailure,
      startAt: job.StartAt ? new Date(job.StartAt) : undefined,
      endAt: job.EndAt ? new Date(job.EndAt) : undefined,
      createdAt: new Date(job.__mj_CreatedAt)
  // ── Job Types ─────────────────────────────────────────────
  private async loadJobTypes(): Promise<JobTypeStatistics[]> {
    const [typesResult, jobsResult, runsResult] = await rv.RunViews([
        EntityName: 'MJ: Scheduled Job Types',
    const types = typesResult.Results as MJScheduledJobTypeEntity[];
    const allJobs = jobsResult.Results as MJScheduledJobEntity[];
    const allRuns = runsResult.Results as MJScheduledJobRunEntity[];
    return types.map(type => {
      const jobsOfType = allJobs.filter(j => j.JobTypeID === type.ID);
      const activeJobs = jobsOfType.filter(j => j.Status === 'Active');
      const jobIds = new Set(jobsOfType.map(j => j.ID));
      const runsOfType = allRuns.filter(r => jobIds.has(r.ScheduledJobID));
      const successfulRuns = runsOfType.filter(r => r.Success).length;
        activeJobsCount: activeJobs.length,
        totalRuns: runsOfType.length,
        successRate: runsOfType.length > 0 ? successfulRuns / runsOfType.length : 0
  // ── Lock Info ─────────────────────────────────────────────
  private async loadLockInfo(): Promise<LockInfo[]> {
      ExtraFilter: 'LockToken IS NOT NULL',
    return (result.Results || []).map(job => {
      const expectedCompletion = job.ExpectedCompletionAt ? new Date(job.ExpectedCompletionAt) : now;
        lockToken: job.LockToken!,
        lockedAt: new Date(job.LockedAt!),
        lockedBy: job.LockedByInstance || 'Unknown',
        expectedCompletion,
        isStale: expectedCompletion < now
  // ── Alerts ────────────────────────────────────────────────
  private async buildAlerts(locks: LockInfo[], kpis: SchedulingKPIs, jobs: JobStatistics[]): Promise<AlertCondition[]> {
    const alerts: AlertCondition[] = [];
    for (const lock of locks) {
      if (lock.isStale) {
        alerts.push({
          type: 'stale-lock',
          title: 'Stale Lock Detected',
          message: `Job "${lock.jobName}" has a stale lock held by ${lock.lockedBy}`,
          jobId: lock.jobId,
          jobName: lock.jobName
    if (kpis.failureRate7d > 0.1) {
        type: 'high-failure',
        title: 'High Failure Rate',
        message: `${(kpis.failureRate7d * 100).toFixed(1)}% failure rate over the last 7 days (${kpis.totalFailures7d} failures)`
    for (const job of jobs) {
      if (job.status === 'Expired') {
          type: 'job-expired',
          title: 'Expired Job',
          message: `Job "${job.jobName}" has expired and is no longer running`,
          jobId: job.jobId,
          jobName: job.jobName
    return alerts;
  // ── Helpers ───────────────────────────────────────────────
  private mapRunsToExecutions(runs: MJScheduledJobRunEntity[], now?: Date): JobExecution[] {
    const currentTime = now || new Date();
    return runs.map(run => {
      const duration = run.CompletedAt
        ? new Date(run.CompletedAt).getTime() - new Date(run.StartedAt).getTime()
        : currentTime.getTime() - new Date(run.StartedAt).getTime();
        jobId: run.ScheduledJobID,
        jobName: run.ScheduledJob || 'Unknown Job',
        jobType: 'Job',
        status: run.Status as JobExecution['status'],
        startedAt: new Date(run.StartedAt),
        completedAt: run.CompletedAt ? new Date(run.CompletedAt) : undefined,
        success: run.Success != null ? run.Success : undefined,
  private createTimeBuckets(start: Date, end: Date): Date[] {
    const hours = (end.getTime() - start.getTime()) / (1000 * 60 * 60);
  private getBucketSizeMs(start: Date, end: Date): number {
    if (hours <= 24) return 60 * 60 * 1000;
    if (hours <= 24 * 7) return 4 * 60 * 60 * 1000;
    return 24 * 60 * 60 * 1000;
  // ── CRUD Operations ───────────────────────────────────────
  async updateJobStatus(jobId: string, status: 'Pending' | 'Active' | 'Paused' | 'Disabled' | 'Expired'): Promise<boolean> {
      const job = await md.GetEntityObject<MJScheduledJobEntity>('MJ: Scheduled Jobs');
      await job.Load(jobId);
      job.Status = status;
      const result = await job.Save();
      if (result) this.refresh();
      console.error('Failed to update job status:', error);
  async saveJob(jobId: string | null, data: Partial<{
    JobTypeID: string;
    CronExpression: string;
    Timezone: string;
    Status: 'Pending' | 'Active' | 'Paused' | 'Disabled' | 'Expired';
    Configuration: string | null;
    ConcurrencyMode: 'Concurrent' | 'Queue' | 'Skip';
    StartAt: Date | null;
    EndAt: Date | null;
    NotifyOnSuccess: boolean;
    NotifyOnFailure: boolean;
  }>): Promise<boolean> {
      if (jobId) {
        job.NewRecord();
      if (data.Name !== undefined) job.Name = data.Name;
      if (data.Description !== undefined) job.Description = data.Description;
      if (data.JobTypeID !== undefined) job.JobTypeID = data.JobTypeID;
      if (data.CronExpression !== undefined) job.CronExpression = data.CronExpression;
      if (data.Timezone !== undefined) job.Timezone = data.Timezone;
      if (data.Status !== undefined) job.Status = data.Status;
      if (data.Configuration !== undefined) job.Configuration = data.Configuration;
      if (data.ConcurrencyMode !== undefined) job.ConcurrencyMode = data.ConcurrencyMode;
      if (data.StartAt !== undefined) job.StartAt = data.StartAt;
      if (data.EndAt !== undefined) job.EndAt = data.EndAt;
      if (data.NotifyOnSuccess !== undefined) job.NotifyOnSuccess = data.NotifyOnSuccess;
      if (data.NotifyOnFailure !== undefined) job.NotifyOnFailure = data.NotifyOnFailure;
      console.error('Failed to save job:', error);
  async deleteJob(jobId: string): Promise<boolean> {
      const result = await job.Delete();
      console.error('Failed to delete job:', error);
  async releaseLock(jobId: string): Promise<boolean> {
      job.LockToken = null;
      job.LockedAt = null;
      job.LockedByInstance = null;
      job.ExpectedCompletionAt = null;
      console.error('Failed to release lock:', error);
  async loadJobTypesForDropdown(): Promise<{ id: string; name: string }[]> {
    const result = await rv.RunView<MJScheduledJobTypeEntity>({
    return (result.Results || []).map(t => ({ id: t.ID, name: t.Name }));
