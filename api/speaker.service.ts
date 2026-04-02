import { SpeakerEntity, SubmissionSpeakerEntity } from 'mj_generatedentities';
export class SpeakerService {
  async getAllSpeakers(): Promise<SpeakerEntity[]> {
    const result = await rv.RunView<SpeakerEntity>({
  async getSpeakerById(id: string): Promise<SpeakerEntity | null> {
    const speaker = await md.GetEntityObject('Speakers') as unknown as SpeakerEntity;
    const loaded = await speaker.Load(id);
    return loaded ? speaker : null;
  async createSpeaker(): Promise<SpeakerEntity> {
    return await md.GetEntityObject('Speakers') as unknown as SpeakerEntity;
  async getSpeakersForSubmission(submissionId: string): Promise<SpeakerEntity[]> {
    // First get the SubmissionSpeaker junction records
    const junctionResult = await rv.RunView<SubmissionSpeakerEntity>({
      EntityName: 'Submission Speakers',
      ExtraFilter: `SubmissionID='${submissionId}'`,
    if (!junctionResult.Success || !junctionResult.Results) {
    // Get all speaker IDs
    const speakerIds = junctionResult.Results.map(ss => ss.SpeakerID);
    if (speakerIds.length === 0) {
    // Load the speakers
    const speakersResult = await rv.RunView<SpeakerEntity>({
      ExtraFilter: `ID IN ('${speakerIds.join("','")}')`,
    return speakersResult.Success ? (speakersResult.Results || []) : [];
