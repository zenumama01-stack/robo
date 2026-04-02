import { MJReportCategoryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Report Categories') // Tell MemberJunction about this class
    selector: 'gen-mjreportcategory-form',
    templateUrl: './mjreportcategory.form.component.html'
export class MJReportCategoryFormComponent extends BaseFormComponent {
    public record!: MJReportCategoryEntity;
            { sectionKey: 'reportCategories', sectionName: 'Report Categories', isExpanded: false },
