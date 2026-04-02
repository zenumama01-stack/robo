import { MJDashboardUserStateEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Dashboard User States') // Tell MemberJunction about this class
    selector: 'gen-mjdashboarduserstate-form',
    templateUrl: './mjdashboarduserstate.form.component.html'
export class MJDashboardUserStateFormComponent extends BaseFormComponent {
    public record!: MJDashboardUserStateEntity;
            { sectionKey: 'identifiersKeys', sectionName: 'Identifiers & Keys', isExpanded: true },
            { sectionKey: 'dashboardStateDetails', sectionName: 'Dashboard State Details', isExpanded: true },
