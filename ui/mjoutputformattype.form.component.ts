import { MJOutputFormatTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Output Format Types') // Tell MemberJunction about this class
    selector: 'gen-mjoutputformattype-form',
    templateUrl: './mjoutputformattype.form.component.html'
export class MJOutputFormatTypeFormComponent extends BaseFormComponent {
    public record!: MJOutputFormatTypeEntity;
            { sectionKey: 'formatDetails', sectionName: 'Format Details', isExpanded: true },
