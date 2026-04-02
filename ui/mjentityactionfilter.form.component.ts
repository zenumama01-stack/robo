import { MJEntityActionFilterEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Action Filters') // Tell MemberJunction about this class
    selector: 'gen-mjentityactionfilter-form',
    templateUrl: './mjentityactionfilter.form.component.html'
export class MJEntityActionFilterFormComponent extends BaseFormComponent {
    public record!: MJEntityActionFilterEntity;
            { sectionKey: 'identifierKeys', sectionName: 'Identifier Keys', isExpanded: true },
