import { MJDashboardUserPreferenceEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Dashboard User Preferences') // Tell MemberJunction about this class
    selector: 'gen-mjdashboarduserpreference-form',
    templateUrl: './mjdashboarduserpreference.form.component.html'
export class MJDashboardUserPreferenceFormComponent extends BaseFormComponent {
    public record!: MJDashboardUserPreferenceEntity;
            { sectionKey: 'identificationOwnership', sectionName: 'Identification & Ownership', isExpanded: true },
            { sectionKey: 'dashboardAssignment', sectionName: 'Dashboard Assignment', isExpanded: true },
            { sectionKey: 'scopeSettings', sectionName: 'Scope Settings', isExpanded: false },
