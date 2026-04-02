import { MJContentItemAttributeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Content Item Attributes') // Tell MemberJunction about this class
    selector: 'gen-mjcontentitemattribute-form',
    templateUrl: './mjcontentitemattribute.form.component.html'
export class MJContentItemAttributeFormComponent extends BaseFormComponent {
    public record!: MJContentItemAttributeEntity;
            { sectionKey: 'attributeData', sectionName: 'Attribute Data', isExpanded: true }
