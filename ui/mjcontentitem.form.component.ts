import { MJContentItemEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Content Items') // Tell MemberJunction about this class
    selector: 'gen-mjcontentitem-form',
    templateUrl: './mjcontentitem.form.component.html'
export class MJContentItemFormComponent extends BaseFormComponent {
    public record!: MJContentItemEntity;
            { sectionKey: 'sourceInformation', sectionName: 'Source Information', isExpanded: true },
            { sectionKey: 'contentDetails', sectionName: 'Content Details', isExpanded: false },
            { sectionKey: 'contentItemAttributes', sectionName: 'Content Item Attributes', isExpanded: false },
            { sectionKey: 'contentItemTags', sectionName: 'Content Item Tags', isExpanded: false }
