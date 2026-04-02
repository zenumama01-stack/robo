import { MJDashboardCategoryLinkEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Dashboard Category Links') // Tell MemberJunction about this class
    selector: 'gen-mjdashboardcategorylink-form',
    templateUrl: './mjdashboardcategorylink.form.component.html'
export class MJDashboardCategoryLinkFormComponent extends BaseFormComponent {
    public record!: MJDashboardCategoryLinkEntity;
            { sectionKey: 'dashboardAssociation', sectionName: 'Dashboard Association', isExpanded: true },
            { sectionKey: 'displaySettings', sectionName: 'Display Settings', isExpanded: true },
