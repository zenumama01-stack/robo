import { Component, OnInit, Input } from '@angular/core';
import { Metadata, UserInfo, RunView } from '@memberjunction/core';
export interface TestFeedbackDialogData {
  currentUser: UserInfo;
  selector: 'mj-test-feedback-dialog',
      [title]="existingFeedback ? 'Update Test Feedback' : 'Provide Test Feedback'"
      [minHeight]="500"
      [autoFocusedElement]="'none'"
        <div class="feedback-dialog-content">
            <label class="feedback-label">Overall Rating</label>
            <div class="rating-scale">
                    class="rating-button"
                    [class.selected]="num === rating"
                    [class.hover]="num === hoverRating"
                    (click)="setRating(num)"
                    (mouseenter)="hoverRating = num"
                    (mouseleave)="hoverRating = 0">
              <div class="rating-labels">
                <span class="label-low">Poor</span>
                <span class="label-mid">Average</span>
                <span class="label-high">Excellent</span>
              @if (rating > 0) {
                <div class="rating-description">
                  <span class="rating-value">{{ rating }}</span> / 10
                  <span class="rating-label">{{ getRatingLabel() }}</span>
            <label class="feedback-label" for="correct">Was the result correct?</label>
              <label class="radio-option">
                <input type="radio" name="correct" [value]="true" [(ngModel)]="isCorrect">
                <input type="radio" name="correct" [value]="false" [(ngModel)]="isCorrect">
                <input type="radio" name="correct" [value]="null" [(ngModel)]="isCorrect">
            <label class="feedback-label" for="comments">Correction Summary / Comments <span class="optional-hint">(optional)</span></label>
              id="comments"
              [(ngModel)]="comments"
              placeholder="Provide detailed feedback, corrections, or comments about this test execution..."
            <div class="feedback-info">
              <span>Saving feedback...</span>
            <div class="feedback-error">
            <span>Loading existing feedback...</span>
      <kendo-dialog-actions layout="start">
          [primary]="true"
          [disabled]="!canSubmit() || isSaving || isLoading">
          <i class="fas" [ngClass]="isSaving ? 'fa-spinner fa-spin' : 'fa-check'"></i>
          {{ isSaving ? 'Saving...' : (existingFeedback ? 'Update Feedback' : 'Submit Feedback') }}
        <button kendoButton (click)="onCancel()" [disabled]="isSaving || isLoading">Cancel</button>
    /* Smooth fade-in for dialog content to prevent flash */
    ::ng-deep .k-dialog-wrapper {
      animation: dialogFadeIn 0.15s ease-out;
    @keyframes dialogFadeIn {
    /* Prevent dialog content from scrolling */
    .feedback-dialog-content {
      animation: contentFadeIn 0.2s ease-out;
    @keyframes contentFadeIn {
    .optional-hint {
    .rating-scale {
    .rating-button {
    .rating-button:hover,
    .rating-button.hover {
    .rating-button.low:hover,
    .rating-button.low.hover {
    .rating-button.mid:hover,
    .rating-button.mid.hover {
    .rating-button.high:hover,
    .rating-button.high.hover {
    .rating-button.selected {
    .rating-button.selected.low {
    .rating-button.selected.mid {
    .rating-button.selected.high {
    .rating-labels {
    .label-low { color: #ef4444; }
    .label-mid { color: #f59e0b; }
    .label-high { color: #10b981; }
    .rating-description {
    .feedback-info {
    .feedback-error {
export class TestFeedbackDialogComponent implements OnInit {
  private _data!: TestFeedbackDialogData;
  set data(value: TestFeedbackDialogData) {
    // When data is set after component creation (via DialogService),
    // trigger loading since ngOnInit already ran
    if (value && !this.dataLoaded) {
      this.initializeWithData();
  get data(): TestFeedbackDialogData {
  rating = 0;
  hoverRating = 0;
  isCorrect: boolean | null = null;
  comments = '';
  existingFeedback: MJTestRunFeedbackEntity | null = null;
  constructor(private dialogRef: DialogRef) {}
    // If data was set via template binding (not DialogService), load here
    if (this._data && !this.dataLoaded) {
      await this.initializeWithData();
    // If data not set yet (DialogService pattern), the setter will trigger loading
  private async initializeWithData(): Promise<void> {
    if (this.dataLoaded) return;
    // Load existing feedback if it exists
    await this.loadExistingFeedback();
  private async loadExistingFeedback(): Promise<void> {
        ExtraFilter: `TestRunID='${this.data.testRunId}' AND ReviewerUserID='${this.data.currentUser.ID}'`,
      }, this.data.currentUser);
        this.existingFeedback = result.Results[0];
        // Pre-populate form with existing feedback
        this.rating = this.existingFeedback.Rating || 0;
        this.isCorrect = this.existingFeedback.IsCorrect;
        this.comments = this.existingFeedback.CorrectionSummary || '';
      console.error('Error loading existing feedback:', error);
      // Don't show error to user - just allow them to create new feedback
  setRating(value: number): void {
    this.rating = value;
  getRatingLabel(): string {
    if (this.rating <= 3) return 'Poor';
    if (this.rating <= 5) return 'Below Average';
    if (this.rating <= 6) return 'Average';
    if (this.rating <= 7) return 'Good';
    if (this.rating <= 8) return 'Very Good';
    if (this.rating <= 9) return 'Excellent';
  canSubmit(): boolean {
    // Rating is required, comments are optional
    return this.rating > 0;
  async onSubmit(): Promise<void> {
    if (!this.canSubmit() || this.isSaving) {
      let feedback: MJTestRunFeedbackEntity;
      if (this.existingFeedback) {
        // Update existing feedback
        feedback = this.existingFeedback;
        // Create new feedback entity
        feedback = await this.metadata.GetEntityObject<MJTestRunFeedbackEntity>(
          'MJ: Test Run Feedbacks',
          this.data.currentUser
        feedback.TestRunID = this.data.testRunId;
        feedback.ReviewerUserID = this.data.currentUser.ID;
      // Update fields (for both new and existing)
      feedback.Rating = this.rating;
      feedback.IsCorrect = this.isCorrect;
      feedback.CorrectionSummary = this.comments.trim() || null;
        this.dialogRef.close({ success: true, feedbackId: feedback.ID });
        this.errorMessage = feedback.LatestResult?.Message || 'Failed to save feedback';
      this.errorMessage = (error as Error).message || 'An error occurred while saving feedback';
    this.dialogRef.close({ success: false });
