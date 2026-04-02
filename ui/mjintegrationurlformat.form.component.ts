import { MJIntegrationURLFormatEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Integration URL Formats') // Tell MemberJunction about this class
    selector: 'gen-mjintegrationurlformat-form',
    templateUrl: './mjintegrationurlformat.form.component.html'
export class MJIntegrationURLFormatFormComponent extends BaseFormComponent {
    public record!: MJIntegrationURLFormatEntity;
            { sectionKey: 'integrationMetadata', sectionName: 'Integration Metadata', isExpanded: false },
            { sectionKey: 'uRLTemplateConfiguration', sectionName: 'URL Template Configuration', isExpanded: true },
            { sectionKey: 'auditNotes', sectionName: 'Audit & Notes', isExpanded: false },
