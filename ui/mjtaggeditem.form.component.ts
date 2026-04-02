import { MJTaggedItemEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Tagged Items') // Tell MemberJunction about this class
    selector: 'gen-mjtaggeditem-form',
    templateUrl: './mjtaggeditem.form.component.html'
export class MJTaggedItemFormComponent extends BaseFormComponent {
    public record!: MJTaggedItemEntity;
            { sectionKey: 'tagDefinition', sectionName: 'Tag Definition', isExpanded: true },
            { sectionKey: 'linkedRecord', sectionName: 'Linked Record', isExpanded: true },
