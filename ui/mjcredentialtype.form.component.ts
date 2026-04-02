import { MJCredentialTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Credential Types') // Tell MemberJunction about this class
    selector: 'gen-mjcredentialtype-form',
    templateUrl: './mjcredentialtype.form.component.html'
export class MJCredentialTypeFormComponent extends BaseFormComponent {
    public record!: MJCredentialTypeEntity;
            { sectionKey: 'technicalDetails', sectionName: 'Technical Details', isExpanded: true },
            { sectionKey: 'mJCredentials', sectionName: 'MJ: Credentials', isExpanded: false },
            { sectionKey: 'mJMCPServers', sectionName: 'MJ: MCP Servers', isExpanded: false },
            { sectionKey: 'mJAIVendors', sectionName: 'MJ: AI Vendors', isExpanded: false }
