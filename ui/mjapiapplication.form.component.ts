import { MJAPIApplicationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: API Applications') // Tell MemberJunction about this class
    selector: 'gen-mjapiapplication-form',
    templateUrl: './mjapiapplication.form.component.html'
export class MJAPIApplicationFormComponent extends BaseFormComponent {
    public record!: MJAPIApplicationEntity;
            { sectionKey: 'applicationDetails', sectionName: 'Application Details', isExpanded: true },
            { sectionKey: 'operationalStatus', sectionName: 'Operational Status', isExpanded: true },
            { sectionKey: 'mJAPIApplicationScopes', sectionName: 'MJ: API Application Scopes', isExpanded: false },
            { sectionKey: 'mJAPIKeyUsageLogs', sectionName: 'MJ: API Key Usage Logs', isExpanded: false },
            { sectionKey: 'mJAPIKeyApplications', sectionName: 'MJ: API Key Applications', isExpanded: false }
