import { Component, Input, Output, EventEmitter, OnInit, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { SchedulingInstrumentationService, JobStatistics } from '../services/scheduling-instrumentation.service';
  selector: 'app-job-slideout',
  templateUrl: './job-slideout.component.html',
  styleUrls: ['./job-slideout.component.css'],
export class JobSlideoutComponent implements OnInit {
  @Input() Mode: 'create' | 'edit' = 'create';
  @Input() Job: JobStatistics | null = null;
  @Output() Saved = new EventEmitter<void>();
  public JobTypes: { id: string; name: string }[] = [];
  public IsDeleting = false;
  public ShowDeleteConfirm = false;
  public JobTypeID = '';
  public CronExpression = '';
  public Timezone = 'UTC';
  public Status: 'Pending' | 'Active' | 'Paused' | 'Disabled' | 'Expired' = 'Pending';
  public ConcurrencyMode: 'Concurrent' | 'Queue' | 'Skip' = 'Skip';
  public Configuration = '';
  public NotifyOnSuccess = false;
  public NotifyOnFailure = true;
  public StatusOptions = ['Pending', 'Active', 'Paused', 'Disabled'];
  public ConcurrencyOptions = ['Skip', 'Queue', 'Concurrent'];
  public TimezoneOptions = [
    'UTC',
    'America/New_York',
    'America/Chicago',
    'America/Denver',
    'America/Los_Angeles',
    'America/Phoenix',
    'Europe/London',
    'Europe/Paris',
    'Europe/Berlin',
    'Asia/Tokyo',
    'Asia/Shanghai',
    'Australia/Sydney',
    'Pacific/Auckland'
    private schedulingService: SchedulingInstrumentationService,
    this.loadJobTypes();
    if (this.Mode === 'edit' && this.Job) {
      this.populateFromJob();
  private async loadJobTypes(): Promise<void> {
    this.JobTypes = await this.schedulingService.loadJobTypesForDropdown();
    if (this.Mode === 'create' && this.JobTypes.length > 0 && !this.JobTypeID) {
      this.JobTypeID = this.JobTypes[0].id;
  private populateFromJob(): void {
    if (!this.Job) return;
    this.Name = this.Job.jobName;
    this.Description = this.Job.description || '';
    this.JobTypeID = this.Job.jobTypeId;
    this.CronExpression = this.Job.cronExpression;
    this.Timezone = this.Job.timezone;
    this.Status = this.Job.status as 'Pending' | 'Active' | 'Paused' | 'Disabled';
    this.ConcurrencyMode = this.Job.concurrencyMode as 'Concurrent' | 'Queue' | 'Skip';
    this.Configuration = this.Job.configuration || '';
    this.NotifyOnSuccess = this.Job.notifyOnSuccess;
    this.NotifyOnFailure = this.Job.notifyOnFailure;
  public get IsValid(): boolean {
    return !!(this.Name.trim() && this.JobTypeID && this.CronExpression.trim());
  public get Title(): string {
    return this.Mode === 'create' ? 'Create New Job' : 'Edit Job';
  public async Save(): Promise<void> {
    if (!this.IsValid || this.IsSaving) return;
      Name: this.Name.trim(),
      Description: this.Description.trim() || null,
      JobTypeID: this.JobTypeID,
      CronExpression: this.CronExpression.trim(),
      Timezone: this.Timezone,
      Status: this.Status,
      ConcurrencyMode: this.ConcurrencyMode,
      Configuration: this.Configuration.trim() || null,
      NotifyOnSuccess: this.NotifyOnSuccess,
      NotifyOnFailure: this.NotifyOnFailure
    const jobId = this.Mode === 'edit' && this.Job ? this.Job.jobId : null;
    const success = await this.schedulingService.saveJob(jobId, data);
      this.Saved.emit();
      this.ErrorMessage = 'Failed to save job. Please try again.';
  public async Delete(): Promise<void> {
    if (!this.Job || this.IsDeleting) return;
    this.IsDeleting = true;
    const success = await this.schedulingService.deleteJob(this.Job.jobId);
    this.IsDeleting = false;
      this.ErrorMessage = 'Failed to delete job. It may have dependent records.';
      this.ShowDeleteConfirm = false;
  public OnConfigurationChange(value: string): void {
    this.Configuration = value;
  public OnClose(): void {
