import { MJDashboardCategoryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Dashboard Categories') // Tell MemberJunction about this class
    selector: 'gen-mjdashboardcategory-form',
    templateUrl: './mjdashboardcategory.form.component.html'
export class MJDashboardCategoryFormComponent extends BaseFormComponent {
    public record!: MJDashboardCategoryEntity;
            { sectionKey: 'dashboardCategories', sectionName: 'Dashboard Categories', isExpanded: false },
            { sectionKey: 'mJDashboardCategoryPermissions', sectionName: 'MJ: Dashboard Category Permissions', isExpanded: false }
