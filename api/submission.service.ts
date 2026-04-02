import { SubmissionEntity } from 'mj_generatedentities';
export class SubmissionService {
  async getAllSubmissions(): Promise<SubmissionEntity[]> {
  async getSubmissionsByEvent(eventId: string): Promise<SubmissionEntity[]> {
      ExtraFilter: `EventID='${eventId}'`,
  async getSubmissionById(id: string): Promise<SubmissionEntity | null> {
    const submission = await md.GetEntityObject('Submissions') as unknown as SubmissionEntity;
    const loaded = await submission.Load(id);
    return loaded ? submission : null;
  async createSubmission(): Promise<SubmissionEntity> {
    return await md.GetEntityObject('Submissions') as unknown as SubmissionEntity;
  async getSubmissionStatistics(eventId?: string): Promise<{
    total: number;
    accepted: number;
    underReview: number;
    rejected: number;
    const submissions = eventId
      ? await this.getSubmissionsByEvent(eventId)
      : await this.getAllSubmissions();
      total: submissions.length,
      accepted: submissions.filter(s => s.Status === 'Accepted').length,
      underReview: submissions.filter(s => s.Status === 'Under Review').length,
      rejected: submissions.filter(s => s.Status === 'Rejected').length
