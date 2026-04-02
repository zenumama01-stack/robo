import { MJWorkspaceItemEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Workspace Items') // Tell MemberJunction about this class
    selector: 'gen-mjworkspaceitem-form',
    templateUrl: './mjworkspaceitem.form.component.html'
export class MJWorkspaceItemFormComponent extends BaseFormComponent {
    public record!: MJWorkspaceItemEntity;
            { sectionKey: 'presentationSettings', sectionName: 'Presentation Settings', isExpanded: false },
