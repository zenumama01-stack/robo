import { MJApplicationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Applications') // Tell MemberJunction about this class
    selector: 'gen-mjapplication-form',
    templateUrl: './mjapplication.form.component.html'
export class MJApplicationFormComponent extends BaseFormComponent {
    public record!: MJApplicationEntity;
            { sectionKey: 'applicationConfiguration', sectionName: 'Application Configuration', isExpanded: true },
            { sectionKey: 'generalInformation', sectionName: 'General Information', isExpanded: true },
            { sectionKey: 'navigationSettings', sectionName: 'Navigation Settings', isExpanded: false },
            { sectionKey: 'entities', sectionName: 'Entities', isExpanded: false },
            { sectionKey: 'applicationSettings', sectionName: 'Application Settings', isExpanded: false },
            { sectionKey: 'userApplications', sectionName: 'User Applications', isExpanded: false },
            { sectionKey: 'dashboards', sectionName: 'Dashboards', isExpanded: false },
            { sectionKey: 'mJDashboardUserPreferences', sectionName: 'MJ: Dashboard User Preferences', isExpanded: false }
