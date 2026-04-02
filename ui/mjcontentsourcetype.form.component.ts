import { MJContentSourceTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Content Source Types') // Tell MemberJunction about this class
    selector: 'gen-mjcontentsourcetype-form',
    templateUrl: './mjcontentsourcetype.form.component.html'
export class MJContentSourceTypeFormComponent extends BaseFormComponent {
    public record!: MJContentSourceTypeEntity;
