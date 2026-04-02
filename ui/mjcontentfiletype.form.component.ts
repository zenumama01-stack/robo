import { MJContentFileTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Content File Types') // Tell MemberJunction about this class
    selector: 'gen-mjcontentfiletype-form',
    templateUrl: './mjcontentfiletype.form.component.html'
export class MJContentFileTypeFormComponent extends BaseFormComponent {
    public record!: MJContentFileTypeEntity;
            { sectionKey: 'contentSources', sectionName: 'Content Sources', isExpanded: false },
            { sectionKey: 'contentItems', sectionName: 'Content Items', isExpanded: false }
