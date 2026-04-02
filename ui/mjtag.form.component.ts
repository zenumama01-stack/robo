import { MJTagEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Tags') // Tell MemberJunction about this class
    selector: 'gen-mjtag-form',
    templateUrl: './mjtag.form.component.html'
export class MJTagFormComponent extends BaseFormComponent {
    public record!: MJTagEntity;
            { sectionKey: 'tagBasics', sectionName: 'Tag Basics', isExpanded: true },
            { sectionKey: 'tagHierarchy', sectionName: 'Tag Hierarchy', isExpanded: true },
            { sectionKey: 'tags', sectionName: 'Tags', isExpanded: false }
