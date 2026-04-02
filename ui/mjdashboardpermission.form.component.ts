import { MJDashboardPermissionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Dashboard Permissions') // Tell MemberJunction about this class
    selector: 'gen-mjdashboardpermission-form',
    templateUrl: './mjdashboardpermission.form.component.html'
export class MJDashboardPermissionFormComponent extends BaseFormComponent {
    public record!: MJDashboardPermissionEntity;
            { sectionKey: 'assignmentDetails', sectionName: 'Assignment Details', isExpanded: true },
