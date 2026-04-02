import { SubmissionEntity, EventEntity } from 'mj_generatedentities';
  selector: 'mj-submission-form',
  templateUrl: './submission-form.component.html',
  styleUrls: ['../shared/form-styles.css', './submission-form.component.css']
@RegisterClass(BaseFormComponent, 'Submissions')
export class SubmissionFormComponent extends BaseFormComponent implements OnInit {
  public record!: SubmissionEntity;
  public loadingEvents = false;
    'New', 'Analyzing', 'Passed Initial', 'Failed Initial',
    'Under Review', 'Accepted', 'Rejected', 'Waitlisted', 'Resubmitted'
  // Session format options (from entity metadata)
  public sessionFormatOptions: string[] = [
    'Workshop', 'Keynote', 'Panel', 'Lightning Talk',
    'Tutorial', 'Presentation', 'Roundtable', 'Other'
  // Target audience level options (from entity metadata)
  public audienceLevelOptions: string[] = [
    'Beginner', 'Intermediate', 'Advanced', 'All Levels'
  // Final decision options
  public finalDecisionOptions: string[] = [
    'Accepted', 'Rejected', 'Waitlisted'
    basicInfo: true,     // Basic info expanded by default
    aiEvaluation: true,  // AI section expanded by default
    content: true,
    materials: false,
    reviewDecision: false,
    metadata: false      // Metadata collapsed by default
    await this.loadEvents();
  private async loadEvents(): Promise<void> {
    this.loadingEvents = true;
      const result = await rv.RunView<EventEntity>({
        this.events = result.Results;
      console.error('Error loading events:', error);
      this.sharedService.CreateSimpleNotification('Error loading events', 'error', 3000);
      this.loadingEvents = false;
    return !!(this.record && !this.loadingEvents);
  public get eventName(): string {
    if (!this.record.EventID) return 'Submission';
    const event = this.events.find(e => e.ID === this.record.EventID);
    return event ? event.Name : 'Submission';
  public get showAIEvaluation(): boolean {
    // Check if ANY AI evaluation data exists
    return !!(
      this.record.AIEvaluationScore != null ||
      this.record.AIEvaluationReasoning ||
      this.record.SubmissionSummary ||
      this.record.PassedInitialScreening != null ||
      this.record.KeyTopics ||
      this.record.FailureReasons
  public get showFinalDecision(): boolean {
    return !!(this.record.FinalDecision || this.record.FinalDecisionDate);
  public get aiScoreClass(): string {
    const score = this.record.AIEvaluationScore;
  public getEventName(eventId: string): string {
    const event = this.events.find(e => e.ID === eventId);
    return event ? event.Name : 'Unknown Event';
  public onDecisionDateChange(value: string | null): void {
    this.record.FinalDecisionDate = value ? new Date(value) : null;
  public openEventRecord(): void {
    if (this.record.EventID) {
      this.sharedService.OpenEntityRecord('Events', CompositeKey.FromID(this.record.EventID));
export function LoadSubmissionFormComponent() {
