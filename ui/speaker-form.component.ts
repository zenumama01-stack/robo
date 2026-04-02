import { SpeakerEntity, ContactEntity } from 'mj_generatedentities';
  selector: 'mj-speaker-form',
  templateUrl: './speaker-form.component.html',
  styleUrls: ['../shared/form-styles.css', './speaker-form.component.css']
@RegisterClass(BaseFormComponent, 'Speakers')
export class SpeakerFormComponent extends BaseFormComponent implements OnInit {
  public record!: SpeakerEntity;
  // Form data
  public contacts: ContactEntity[] = [];
  public loadingContacts = false;
    await this.loadContacts();
  private async loadContacts(): Promise<void> {
    this.loadingContacts = true;
      const result = await rv.RunView<ContactEntity>({
        EntityName: 'Contacts',
        OrderBy: 'LastName, FirstName',
        this.contacts = result.Results;
      console.error('Error loading contacts:', error);
      this.sharedService.CreateSimpleNotification('Error loading contacts', 'error', 3000);
      this.loadingContacts = false;
    return !!(this.record && !this.loadingContacts);
  public get hasDossierData(): boolean {
    return !!(this.record.DossierSummary || this.record.CredibilityScore != null);
  public get hasResearchData(): boolean {
    return !!(this.record.SpeakingHistory || this.record.Expertise || this.record.PublicationsCount || this.record.SocialMediaReach);
  public get hasRedFlags(): boolean {
    return !!(this.record.RedFlags);
  public get credibilityScoreClass(): string {
    const score = this.record.CredibilityScore;
    if (score == null) return '';
    if (score >= 80) return 'score-excellent';
    if (score >= 60) return 'score-good';
    if (score >= 40) return 'score-fair';
    return 'score-poor';
  public getContactName(contactId: number): string {
    const contact = this.contacts.find(c => c.ID === contactId);
    return contact ? contact.FullName : 'Unknown Contact';
  public parseJsonArray(jsonString: string | null): string[] {
    if (!jsonString) return [];
      const parsed = JSON.parse(jsonString);
      return Array.isArray(parsed) ? parsed : [];
      return [];
  public formatNumber(num: number | null): string {
    if (num == null) return '0';
  public openContactRecord(): void {
    if (this.record.ContactID) {
      this.sharedService.OpenEntityRecord('Contacts', CompositeKey.FromID(this.record.ContactID));
export function LoadSpeakerFormComponent() {
