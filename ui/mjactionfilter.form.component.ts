import { MJActionFilterEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Action Filters') // Tell MemberJunction about this class
    selector: 'gen-mjactionfilter-form',
    templateUrl: './mjactionfilter.form.component.html'
export class MJActionFilterFormComponent extends BaseFormComponent {
    public record!: MJActionFilterEntity;
            { sectionKey: 'filterDetails', sectionName: 'Filter Details', isExpanded: true },
            { sectionKey: 'entityActionFilters', sectionName: 'Entity Action Filters', isExpanded: false }
