import { MJUserApplicationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: User Applications') // Tell MemberJunction about this class
    selector: 'gen-mjuserapplication-form',
    templateUrl: './mjuserapplication.form.component.html'
export class MJUserApplicationFormComponent extends BaseFormComponent {
    public record!: MJUserApplicationEntity;
            { sectionKey: 'entities', sectionName: 'Entities', isExpanded: false }
