import { MJContentTypeAttributeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Content Type Attributes') // Tell MemberJunction about this class
    selector: 'gen-mjcontenttypeattribute-form',
    templateUrl: './mjcontenttypeattribute.form.component.html'
export class MJContentTypeAttributeFormComponent extends BaseFormComponent {
    public record!: MJContentTypeAttributeEntity;
