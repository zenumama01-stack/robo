import { MJContentSourceTypeParamEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Content Source Type Params') // Tell MemberJunction about this class
    selector: 'gen-mjcontentsourcetypeparam-form',
    templateUrl: './mjcontentsourcetypeparam.form.component.html'
export class MJContentSourceTypeParamFormComponent extends BaseFormComponent {
    public record!: MJContentSourceTypeParamEntity;
