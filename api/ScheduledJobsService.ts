 * @fileoverview Service module for managing scheduled job lifecycle
 * @module MJServer/services
import { SchedulingEngine } from '@memberjunction/scheduling-engine';
import { ScheduledJobsConfig } from '../config.js';
 * Service for managing scheduled jobs lifecycle
 * Handles initialization, starting/stopping polling, and graceful shutdown
export class ScheduledJobsService {
    private engine: SchedulingEngine;
    private systemUser: UserInfo | null = null;
    private isRunning: boolean = false;
    private config: ScheduledJobsConfig;
    constructor(config: ScheduledJobsConfig) {
        this.engine = SchedulingEngine.Instance;
     * Initialize the scheduled jobs service
     * Loads metadata and prepares the engine
    public async Initialize(): Promise<void> {
        if (!this.config.enabled) {
            LogStatus('[ScheduledJobsService] Scheduled jobs are disabled in configuration');
            // Get system user for job execution
            this.systemUser = await this.getSystemUser();
            if (!this.systemUser) {
                throw new Error(`System user not found with email: ${this.config.systemUserEmail}`);
            // Pre-load metadata cache
            await this.engine.Config(false, this.systemUser);
            LogError('[ScheduledJobsService] Failed to initialize', undefined, error);
     * Start the scheduled jobs polling
    public async Start(): Promise<void> {
        if (this.isRunning) {
            LogStatus('[ScheduledJobsService] Already running');
            throw new Error('Service not initialized - call Initialize() first');
            this.engine.StartPolling(this.systemUser);
            // Single consolidated console message
            const jobCount = this.engine.ScheduledJobs.length;
            if (jobCount === 0) {
                console.log(`📅 Scheduled Jobs: No active jobs, polling suspended (will auto-start when jobs are added)`);
                const interval = this.engine.ActivePollingInterval;
                if (interval !== null) {
                    const intervalDisplay = interval >= 60000
                        ? `${Math.round(interval / 60000)} minute(s)`
                        : `${Math.round(interval / 1000)} second(s)`;
                    console.log(`📅 Scheduled Jobs: ${jobCount} active job(s), polling every ${intervalDisplay}`);
                    // This shouldn't happen if jobCount > 0, but handle it gracefully
                    console.log(`📅 Scheduled Jobs: ${jobCount} active job(s), polling interval not set`);
            LogError('[ScheduledJobsService] Failed to start polling', undefined, error);
     * Stop the scheduled jobs polling gracefully
    public async Stop(): Promise<void> {
            LogStatus('[ScheduledJobsService] Stopping scheduled job polling');
            this.engine.StopPolling();
            LogStatus('[ScheduledJobsService] Polling stopped successfully');
            LogError('[ScheduledJobsService] Error stopping polling', undefined, error);
     * Get the system user for job execution
     * Uses the email configured in scheduledJobs.systemUserEmail
    private async getSystemUser(): Promise<UserInfo | null> {
        const systemUserEmail = this.config.systemUserEmail;
        // Search UserCache for system user
        const user = UserCache.Users.find(u =>
            u.Email?.toLowerCase() === systemUserEmail.toLowerCase()
        LogError(`[ScheduledJobsService] System user not found with email: ${systemUserEmail}`);
     * Get current service status
    public GetStatus(): {
        running: boolean;
        activeJobs: number;
        pollingInterval: number;
            enabled: this.config.enabled,
            running: this.isRunning,
            activeJobs: this.engine?.ScheduledJobs?.length || 0,
            pollingInterval: this.engine?.ActivePollingInterval || 0
     * Check if service is enabled in configuration
        return this.config.enabled;
     * Check if service is currently running
    public get IsRunning(): boolean {
        return this.isRunning;
