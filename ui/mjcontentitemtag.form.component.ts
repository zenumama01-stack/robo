import { MJContentItemTagEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Content Item Tags') // Tell MemberJunction about this class
    selector: 'gen-mjcontentitemtag-form',
    templateUrl: './mjcontentitemtag.form.component.html'
export class MJContentItemTagFormComponent extends BaseFormComponent {
    public record!: MJContentItemTagEntity;
            { sectionKey: 'tagAssociation', sectionName: 'Tag Association', isExpanded: true }
