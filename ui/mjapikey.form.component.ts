import { MJAPIKeyEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: API Keys') // Tell MemberJunction about this class
    selector: 'gen-mjapikey-form',
    templateUrl: './mjapikey.form.component.html'
export class MJAPIKeyFormComponent extends BaseFormComponent {
    public record!: MJAPIKeyEntity;
            { sectionKey: 'keyInformation', sectionName: 'Key Information', isExpanded: true },
            { sectionKey: 'ownership', sectionName: 'Ownership', isExpanded: true },
            { sectionKey: 'statusUsage', sectionName: 'Status & Usage', isExpanded: false },
            { sectionKey: 'mJAPIKeyApplications', sectionName: 'MJ: API Key Applications', isExpanded: false },
            { sectionKey: 'mJAPIKeyScopes', sectionName: 'MJ: API Key Scopes', isExpanded: false },
            { sectionKey: 'mJAPIKeyUsageLogs', sectionName: 'MJ: API Key Usage Logs', isExpanded: false }
