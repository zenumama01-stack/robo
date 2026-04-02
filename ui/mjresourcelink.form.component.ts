import { MJResourceLinkEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Resource Links') // Tell MemberJunction about this class
    selector: 'gen-mjresourcelink-form',
    templateUrl: './mjresourcelink.form.component.html'
export class MJResourceLinkFormComponent extends BaseFormComponent {
    public record!: MJResourceLinkEntity;
