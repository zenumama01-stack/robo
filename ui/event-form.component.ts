import { Component, ChangeDetectorRef, ElementRef, OnInit } from '@angular/core';
import { EventEntity, SubmissionEntity } from 'mj_generatedentities';
import { BaseFormComponent } from '@memberjunction/ng-base-forms';
import { ActivatedRoute, Router } from '@angular/router';
  selector: 'mj-event-form',
  templateUrl: './event-form.component.html',
  styleUrls: ['../shared/form-styles.css', './event-form.component.css']
@RegisterClass(BaseFormComponent, 'Events')
export class EventFormComponent extends BaseFormComponent implements OnInit {
  public record!: EventEntity;
  // Submissions data
  public loadingSubmissions = false;
  // Status options (from entity metadata)
  public statusOptions: string[] = [
    'Planning', 'Open for Submissions', 'Submissions Closed',
    'Review in Progress', 'Completed', 'Cancelled'
  // Collapsible section state
  public sectionsExpanded = {
    basicInfo: true,
    dates: true,
    details: true,
    submissions: true,
    metadata: false
    elementRef: ElementRef,
    sharedService: SharedService,
    router: Router,
    route: ActivatedRoute,
    cdr: ChangeDetectorRef
    super(elementRef, sharedService, router, route, cdr);
  override async ngOnInit() {
    if (this.record?.ID) {
      await this.loadSubmissions();
  private async loadSubmissions(): Promise<void> {
    if (!this.record?.ID) return;
    this.loadingSubmissions = true;
      const result = await rv.RunView<SubmissionEntity>({
        ExtraFilter: `EventID='${this.record.ID}'`,
      }, md.CurrentUser);
      if (result.Success && result.Results) {
        this.submissions = result.Results;
      console.error('Error loading submissions:', error);
      this.sharedService.CreateSimpleNotification('Error loading submissions', 'error', 3000);
      this.loadingSubmissions = false;
  public get isRecordReady(): boolean {
    return !!this.record;
  public get submissionsStats() {
    const total = this.submissions.length;
    const accepted = this.submissions.filter(s => s.Status === 'Accepted').length;
    const rejected = this.submissions.filter(s => s.Status === 'Rejected').length;
    const underReview = this.submissions.filter(s => s.Status === 'Under Review').length;
    const passedInitial = this.submissions.filter(s => s.Status === 'Passed Initial').length;
    return { total, accepted, rejected, underReview, passedInitial };
  public get statusBadgeClass(): string {
    const status = this.record.Status;
    if (!status) return 'badge-default';
  public onStartDateChange(value: string | null): void {
    if (value) {
      this.record.StartDate = new Date(value);
  public onEndDateChange(value: string | null): void {
      this.record.EndDate = new Date(value);
  public onSubmissionDeadlineChange(value: string | null): void {
      this.record.SubmissionDeadline = new Date(value);
  public toggleSection(section: keyof typeof this.sectionsExpanded): void {
    this.sectionsExpanded[section] = !this.sectionsExpanded[section];
  public getSubmissionStatusClass(status: string): string {
    if (submissionId) {
  public goToDashboard(): void {
    this.router.navigate(['app', 'Events']);
export function LoadEventFormComponent() {
