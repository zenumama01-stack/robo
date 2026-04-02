import { MJContentTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Content Types') // Tell MemberJunction about this class
    selector: 'gen-mjcontenttype-form',
    templateUrl: './mjcontenttype.form.component.html'
export class MJContentTypeFormComponent extends BaseFormComponent {
    public record!: MJContentTypeEntity;
            { sectionKey: 'aIModelSettings', sectionName: 'AI Model Settings', isExpanded: true },
