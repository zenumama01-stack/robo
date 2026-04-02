import { MJTestRunFeedbackEntity } from '@memberjunction/core-entities';
import { CompositeKey } from '@memberjunction/core';
import { MJTestRunFeedbackFormComponent } from '../../generated/Entities/MJTestRunFeedback/mjtestrunfeedback.form.component';
@RegisterClass(BaseFormComponent, 'MJ: Test Run Feedbacks')
  selector: 'mj-test-run-feedback-form',
    <div class="feedback-form">
      <div class="feedback-header">
        <h2><i class="fas fa-comment-dots"></i> Test Run Feedback</h2>
        @if (record.TestRunID) {
          <button kendoButton (click)="openTestRun()">
            <i class="fas fa-external-link"></i> View Test Run
      <div class="feedback-content">
          <label>Rating (1-5)</label>
          <input type="number" [(ngModel)]="record.Rating" min="1" max="5" />
          <label>Is Correct?</label>
          <input type="checkbox" [(ngModel)]="record.IsCorrect" />
          <label>Comments</label>
          <textarea [(ngModel)]="record.Comments" rows="6"></textarea>
          <label>Reviewer</label>
          <div>{{ record.ReviewerUser }}</div>
          <label>Submitted At</label>
          <div>{{ record.__mj_CreatedAt | date:'medium' }}</div>
    .feedback-form { padding: 20px; }
    .feedback-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    .feedback-header h2 { margin: 0; font-size: 20px; display: flex; align-items: center; gap: 12px; }
    .feedback-content { background: white; padding: 24px; border-radius: 8px; }
    .field-group input[type="number"], .field-group textarea { width: 100%; padding: 8px 12px; border: 1px solid #ddd; border-radius: 4px; }
    .field-group input[type="checkbox"] { width: auto; }
export class TestRunFeedbackFormComponentExtended extends MJTestRunFeedbackFormComponent {
  public override record!: MJTestRunFeedbackEntity;
  openTestRun() {
    if (this.record.TestRunID) {
      SharedService.Instance.OpenEntityRecord('MJ: Test Runs', CompositeKey.FromID(this.record.TestRunID));
