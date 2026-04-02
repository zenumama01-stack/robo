 * @fileoverview Engine for managing and executing scheduled jobs
    LogStatusEx,
    IsVerboseLoggingEnabled
import { MJScheduledJobEntity, MJScheduledJobRunEntity } from '@memberjunction/core-entities';
import { ScheduledJobResult, NotificationChannel } from '@memberjunction/scheduling-base-types';
import { SchedulingEngineBase } from '@memberjunction/scheduling-engine-base';
import { BaseScheduledJob, ScheduledJobExecutionContext } from './BaseScheduledJob';
import { CronExpressionHelper } from './CronExpressionHelper';
import { NotificationManager } from './NotificationManager';
 * Engine for managing scheduled job execution
 * This engine extends SchedulingEngineBase with execution capabilities:
 * - Evaluates cron expressions to determine which jobs are due
 * - Instantiates the appropriate plugin for each job type
 * - Executes jobs and tracks results in ScheduledJobRun
 * - Sends notifications based on job configuration
 * - Updates job statistics (RunCount, SuccessCount, FailureCount)
 * - Manages distributed locking for multi-server environments
 * @description ONLY USE ON SERVER-SIDE. For metadata only, use SchedulingEngineBase which can be used anywhere.
 * const engine = SchedulingEngine.Instance;
 * const runs = await engine.ExecuteScheduledJobs(contextUser);
 * console.log(`Executed ${runs.length} scheduled jobs`);
export class SchedulingEngine extends SchedulingEngineBase {
    public static get Instance(): SchedulingEngine {
        return super.getInstance<SchedulingEngine>();
    private pollingTimer?: NodeJS.Timeout;
    private isPolling: boolean = false;
    private hasInitialized: boolean = false;
     * Start continuous polling for scheduled jobs
     * Uses adaptive interval based on ActivePollingInterval
     * @param contextUser - User context for execution
    public StartPolling(contextUser: UserInfo): void {
        if (this.isPolling) {
            this.log('Polling already started');
        this.isPolling = true;
        const poll = async () => {
            // Initialize NextRunAt and clean up stale locks on first poll only
            if (!this.hasInitialized) {
                await this.initializeNextRunTimes(contextUser);
                await this.cleanupStaleLocks(contextUser);
                // Force reload after cleaning locks to ensure we have fresh data
                this.hasInitialized = true;
                // Check if there are no jobs after initialization
                if (this.ScheduledJobs.length === 0) {
                    console.log(`📅 Scheduled Jobs: No active jobs found, stopping polling`);
                    this.StopPolling();
                const runs = await this.ExecuteScheduledJobs(contextUser);
                // Only log if jobs were actually executed
                if (runs.length > 0) {
                    console.log(`📅 Scheduled Jobs: Executed ${runs.length} job(s)`);
                // Schedule next poll based on current ActivePollingInterval
                    const interval = this.ActivePollingInterval;
                    // If interval is null (no jobs), stop polling
                    if (interval === null) {
                        console.log(`📅 Scheduled Jobs: All jobs removed, stopping polling`);
                    this.pollingTimer = setTimeout(poll, interval);
                this.logError('Error during polling', error);
                // Continue polling even after errors
                    this.pollingTimer = setTimeout(poll, 60000); // Fallback to 1 minute
        // Start first poll immediately
        poll();
     * Stop continuous polling
    public StopPolling(): void {
        if (!this.isPolling) {
        this.isPolling = false;
        if (this.pollingTimer) {
            clearTimeout(this.pollingTimer);
            this.pollingTimer = undefined;
        this.log('Stopped scheduled job polling');
     * Check if polling is currently active
    public get IsPolling(): boolean {
        return this.isPolling;
     * Handle job changes (create, update, delete)
     * Reloads job metadata and restarts polling if needed
     * This method is called automatically by ScheduledJobEntityExtended.Save() and Delete()
     * @param contextUser - User context for reloading metadata
    public async OnJobChanged(contextUser: UserInfo): Promise<void> {
        // Reload jobs from database
        // Recalculate polling interval
        this.UpdatePollingInterval();
        // If polling is stopped and we now have jobs, restart it
        if (!this.isPolling && this.ScheduledJobs.length > 0) {
            console.log(`📅 Scheduled Jobs: Jobs detected, starting polling`);
            this.StartPolling(contextUser);
     * Execute all scheduled jobs that are currently due
     * Evaluates each active job's cron expression against the current time.
     * Jobs that are due are executed via their plugin's Execute method.
     * @param evalTime - Optional time to evaluate against (defaults to now)
     * @returns Array of scheduled job run records
    public async ExecuteScheduledJobs(
        evalTime: Date = new Date()
    ): Promise<MJScheduledJobRunEntity[]> {
        console.log(`📅 Polling: Checking ${this.ScheduledJobs.length} job(s) at ${evalTime.toISOString()}`);
        const runs: MJScheduledJobRunEntity[] = [];
        for (const job of this.ScheduledJobs) {
            console.log(`  - ${job.Name}: NextRunAt=${job.NextRunAt?.toISOString() || 'NULL'}, Status=${job.Status}`);
            if (this.isJobDue(job, evalTime)) {
                console.log(`    ✓ Job is due, executing...`);
                    const run = await this.executeJob(job, contextUser);
                    if (run) { // null if skipped
                        runs.push(run);
                        console.log(`    ⊘ Job was skipped (locked or queued)`);
                    this.logError(`Failed to execute job ${job.Name}`, error);
                console.log(`    ⊗ Job is not due yet`);
        // Recalculate polling interval after each poll cycle
        // This ensures we adapt to the narrowest active job schedule
     * Execute a specific scheduled job by ID
     * @param jobId - ID of the job to execute
     * @returns The scheduled job run record
    public async ExecuteScheduledJob(
    ): Promise<MJScheduledJobRunEntity> {
        const job = this.ScheduledJobs.find(j => j.ID === jobId);
        if (!job) {
            throw new Error(`Scheduled job ${jobId} not found or not active`);
        return await this.executeJob(job, contextUser);
     * Determine if a job is currently due for execution
     * @param job - The scheduled job
     * @returns True if the job should execute now
    private isJobDue(job: MJScheduledJobEntity, evalTime: Date): boolean {
        if (job.StartAt && evalTime < job.StartAt) {
        if (job.EndAt && evalTime > job.EndAt) {
        // Check if NextRunAt is set and has passed
        if (!job.NextRunAt) {
        // Job is due if NextRunAt is in the past or very close to now (within 1 second tolerance)
        return job.NextRunAt.getTime() <= evalTime.getTime() + 1000;
     * Execute a single scheduled job
     * @param job - The job to execute
     * @returns The created job run record
    private async executeJob(
        job: MJScheduledJobEntity,
        // Try to acquire lock for this job
        const lockAcquired = await this.tryAcquireLock(job);
        if (!lockAcquired) {
            // Handle based on concurrency mode
            if (job.ConcurrencyMode === 'Skip') {
                this.log(`Job ${job.Name} is locked, skipping (ConcurrencyMode=Skip)`);
                return null; // Skip this execution
            } else if (job.ConcurrencyMode === 'Queue') {
                this.log(`Job ${job.Name} is locked, queueing (ConcurrencyMode=Queue)`);
                // Create a queued run record for future processing
                return await this.createQueuedJobRun(job, contextUser);
            // Concurrent mode: proceed without lock
        // Create run record
        const run = await this.createJobRun(job, contextUser);
            // Get job type
            const jobType = this.ScheduledJobTypes.find(t => t.ID === job.JobTypeID);
            if (!jobType) {
                throw new Error(`Job type ${job.JobTypeID} not found`);
            // Instantiate plugin using ClassFactory
            const plugin = MJGlobal.Instance.ClassFactory.CreateInstance<BaseScheduledJob>(
                BaseScheduledJob,
                jobType.DriverClass
            if (!plugin) {
                throw new Error(`Failed to create plugin instance: ${jobType.DriverClass}`);
            // Console log job start
            console.log(`  ▶️  Starting: ${job.Name}`);
            // Build execution context
            const context: ScheduledJobExecutionContext = {
                Schedule: job,
                Run: run,
            // Execute the job via plugin
            const result = await plugin.Execute(context);
            // Update run record with result
            run.CompletedAt = new Date();
            run.Status = result.Success ? 'Completed' : 'Failed';
            run.Success = result.Success;
            run.ErrorMessage = result.ErrorMessage || null;
            run.Details = result.Details ? JSON.stringify(result.Details) : null;
            await run.Save();
            // Update job statistics
            await this.updateJobStatistics(job, result.Success, run.ID);
            // Send notifications if configured
            await this.sendNotificationsIfNeeded(job, context, result, plugin);
            // Console log job completion
            const duration = run.CompletedAt.getTime() - run.StartedAt.getTime();
            const status = result.Success ? '✅' : '❌';
            console.log(`  ${status} Completed: ${job.Name} (${duration}ms)`);
            // Update run with failure
            run.Status = 'Failed';
            run.Success = false;
            run.ErrorMessage = error instanceof Error ? error.message : 'Unknown error';
            // Update job failure count
            await this.updateJobStatistics(job, false, run.ID);
            this.logError(`Job failed: ${job.Name}`, error);
            // Release lock if we acquired it
            if (lockAcquired) {
                await this.releaseLock(job);
     * Create a new ScheduledJobRun record
     * @returns The created run entity
    private async createJobRun(
        const run = await md.GetEntityObject<MJScheduledJobRunEntity>(
            'MJ: Scheduled Job Runs',
        run.ScheduledJobID = job.ID;
        run.ExecutedByUserID = contextUser.ID;
        run.Status = 'Running';
     * Update job statistics after execution
     * @param success - Whether the run succeeded
     * @param runId - The run ID
    private async updateJobStatistics(
        runId: string
        job.RunCount++;
            job.SuccessCount++;
            job.FailureCount++;
        job.LastRunAt = new Date();
        job.NextRunAt = CronExpressionHelper.GetNextRunTime(
            job.CronExpression,
            job.Timezone
        await job.Save();
     * Send notifications if configured
     * @param result - Execution result
     * @param plugin - The job plugin
    private async sendNotificationsIfNeeded(
        result: ScheduledJobResult,
        plugin: BaseScheduledJob
        const shouldNotify = (result.Success && job.NotifyOnSuccess) ||
            (!result.Success && job.NotifyOnFailure);
        if (!shouldNotify) {
        const recipientUserId = job.NotifyUserID || job.OwnerUserID;
        if (!recipientUserId) {
        const channels: NotificationChannel[] = [];
        if (job.NotifyViaEmail) channels.push('Email');
        if (job.NotifyViaInApp) channels.push('InApp');
        if (channels.length === 0) {
        const content = plugin.FormatNotification(context, result);
        await NotificationManager.SendScheduledJobNotification(
            recipientUserId,
            channels
     * Try to acquire a lock for job execution
     * Uses atomic database update to prevent race conditions
     * @param job - The job to lock
     * @returns True if lock acquired, false if already locked
    private async tryAcquireLock(job: MJScheduledJobEntity): Promise<boolean> {
        // Check if already locked and not stale
        console.log(`    🔒 tryAcquireLock: job.LockToken=${job.LockToken?.substring(0, 8) || 'NULL'}, ExpectedCompletionAt=${job.ExpectedCompletionAt?.toISOString() || 'NULL'}`);
        if (job.LockToken != null) {
            console.log(`      Lock exists! Checking if stale: ExpectedCompletionAt=${job.ExpectedCompletionAt?.toISOString()}, now=${now.toISOString()}`);
            if (job.ExpectedCompletionAt && now > job.ExpectedCompletionAt) {
                console.log(`      → Lock is STALE, cleaning up...`);
                this.log(`Detected stale lock on job ${job.Name}, cleaning up`);
                await this.cleanupStaleLock(job);
                // Lock is active and not stale
                console.log(`      → Lock is ACTIVE (not stale), returning false`);
            console.log(`      → No lock exists, will try to acquire`);
        // Try to acquire lock using BaseEntity Save for proper change tracking
        const lockToken = this.generateGuid();
        const instanceId = this.getInstanceIdentifier();
        const expectedCompletion = new Date(Date.now() + 10 * 60 * 1000); // 10 minutes default
            // Reload job from database to ensure we have latest state and enable dirty checking
            await job.Load(job.ID);
            // Verify lock is still null after reload (race condition check)
                console.log(`      ❌ Lock was acquired by another process during reload`);
            // Set lock fields
            job.LockToken = lockToken;
            job.LockedAt = new Date();
            job.LockedByInstance = instanceId;
            job.ExpectedCompletionAt = expectedCompletion;
            console.log(`      → Attempting to save with lock: ${lockToken.substring(0, 8)}...`);
            // Save will use optimistic concurrency - fails if another process updated the record
                console.log(`      ✅ Lock acquired successfully!`);
                console.log(`      ❌ Save failed - likely race condition with another server`);
                // Clear lock state on failure
            this.logError(`Failed to acquire lock for job ${job.Name}`, error);
            // Clear any partial lock state
     * Release a lock after job execution
     * @param job - The job to unlock
    private async releaseLock(job: MJScheduledJobEntity): Promise<void> {
            this.logError(`Failed to release lock for job ${job.Name}`, error);
     * Clean up a stale lock (when ExpectedCompletionAt has passed)
     * @param job - The job with stale lock
    private async cleanupStaleLock(job: MJScheduledJobEntity): Promise<void> {
        this.log(`Cleaning up stale lock on job ${job.Name} (locked by ${job.LockedByInstance})`);
     * Create a queued job run for later execution
     * @param job - The job to queue
     * @returns The queued run entity
    private async createQueuedJobRun(
        run.Status = 'Running'; // Will be picked up by queue processor
        run.QueuedAt = new Date();
        this.log(`Queued job ${job.Name} for later execution (Run ID: ${run.ID})`);
     * Get unique identifier for this server instance
     * @returns Server instance identifier
    private getInstanceIdentifier(): string {
        // Use hostname + process ID for unique instance identification
        return `${os.hostname()}-${process.pid}`;
     * Generate a GUID for lock tokens
     * @returns Generated GUID
    private generateGuid(): string {
     * Initialize NextRunAt for jobs that don't have it set
    private async initializeNextRunTimes(contextUser: UserInfo): Promise<void> {
                job.NextRunAt = CronExpressionHelper.GetNextRunTime(job.CronExpression, job.Timezone);
                    console.log(`  ⚙️  Initialized NextRunAt for ${job.Name} -> ${job.NextRunAt.toISOString()}`);
                    this.logError(`Failed to initialize NextRunAt for job ${job.Name}`, error);
     * Clean up stale locks on startup
     * Releases any locks that have expired (ExpectedCompletionAt in the past)
    private async cleanupStaleLocks(contextUser: UserInfo): Promise<void> {
        console.log(`  🔍 Checking for stale locks (current time: ${now.toISOString()})...`);
            if (job.LockToken) {
                console.log(`    Job "${job.Name}": LockToken=${job.LockToken?.substring(0, 8)}..., ExpectedCompletionAt=${job.ExpectedCompletionAt?.toISOString() || 'NULL'}`);
                if (job.ExpectedCompletionAt) {
                    const isStale = job.ExpectedCompletionAt < now;
                    console.log(`      → Is stale? ${isStale} (${job.ExpectedCompletionAt.getTime()} < ${now.getTime()} = ${job.ExpectedCompletionAt.getTime() < now.getTime()})`);
                    if (isStale) {
                        console.log(`      🔓 Cleaning stale lock (locked by ${job.LockedByInstance})`);
                            this.logError(`Failed to clean stale lock for job ${job.Name}`, error);
                    console.log(`      ⚠️  Lock exists but no ExpectedCompletionAt - clearing anyway`);
            console.log(`  ✅ Cleaned ${cleanedCount} stale lock(s)`);
            console.log(`  ✓ No stale locks found`);
    private log(message: string): void {
            message: `[ScheduledJobEngine] ${message}`,
            verboseOnly: false,
            isVerboseEnabled: () => false
    private logError(message: string, error?: any): void {
        LogError(`[ScheduledJobEngine] ${message}`, undefined, error);
