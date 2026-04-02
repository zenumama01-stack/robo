import { MJDashboardEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Dashboards') // Tell MemberJunction about this class
    selector: 'gen-mjdashboard-form',
    templateUrl: './mjdashboard.form.component.html'
export class MJDashboardFormComponent extends BaseFormComponent {
    public record!: MJDashboardEntity;
            { sectionKey: 'dashboardIdentityDescription', sectionName: 'Dashboard Identity & Description', isExpanded: true },
            { sectionKey: 'accessScopeSettings', sectionName: 'Access & Scope Settings', isExpanded: true },
            { sectionKey: 'technicalConfiguration', sectionName: 'Technical Configuration', isExpanded: false },
            { sectionKey: 'mJDashboardCategoryLinks', sectionName: 'MJ: Dashboard Category Links', isExpanded: false },
            { sectionKey: 'mJDashboardUserStates', sectionName: 'MJ: Dashboard User States', isExpanded: false },
            { sectionKey: 'mJDashboardPermissions', sectionName: 'MJ: Dashboard Permissions', isExpanded: false },
