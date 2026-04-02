import { MJIntegrationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Integrations') // Tell MemberJunction about this class
    selector: 'gen-mjintegration-form',
    templateUrl: './mjintegration.form.component.html'
export class MJIntegrationFormComponent extends BaseFormComponent {
    public record!: MJIntegrationEntity;
            { sectionKey: 'integrationOverview', sectionName: 'Integration Overview', isExpanded: true },
            { sectionKey: 'technicalSettings', sectionName: 'Technical Settings', isExpanded: true },
            { sectionKey: 'uRLFormats', sectionName: 'URL Formats', isExpanded: false },
            { sectionKey: 'recordChanges', sectionName: 'Record Changes', isExpanded: false }
