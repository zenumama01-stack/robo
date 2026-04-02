import { MJPublicLinkEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Public Links') // Tell MemberJunction about this class
    selector: 'gen-mjpubliclink-form',
    templateUrl: './mjpubliclink.form.component.html'
export class MJPublicLinkFormComponent extends BaseFormComponent {
    public record!: MJPublicLinkEntity;
            { sectionKey: 'resourceReference', sectionName: 'Resource Reference', isExpanded: true },
            { sectionKey: 'linkCore', sectionName: 'Link Core', isExpanded: true },
            { sectionKey: 'accessControls', sectionName: 'Access Controls', isExpanded: false },
