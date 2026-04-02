import { MJDashboardCategoryPermissionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Dashboard Category Permissions') // Tell MemberJunction about this class
    selector: 'gen-mjdashboardcategorypermission-form',
    templateUrl: './mjdashboardcategorypermission.form.component.html'
export class MJDashboardCategoryPermissionFormComponent extends BaseFormComponent {
    public record!: MJDashboardCategoryPermissionEntity;
            { sectionKey: 'userAccess', sectionName: 'User Access', isExpanded: true },
            { sectionKey: 'permissionSettings', sectionName: 'Permission Settings', isExpanded: false },
