import { MJWorkspaceEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Workspaces') // Tell MemberJunction about this class
    selector: 'gen-mjworkspace-form',
    templateUrl: './mjworkspace.form.component.html'
export class MJWorkspaceFormComponent extends BaseFormComponent {
    public record!: MJWorkspaceEntity;
            { sectionKey: 'workspaceIdentification', sectionName: 'Workspace Identification', isExpanded: true },
            { sectionKey: 'workspaceDetails', sectionName: 'Workspace Details', isExpanded: true },
            { sectionKey: 'administrativeInfo', sectionName: 'Administrative Info', isExpanded: false },
            { sectionKey: 'workspaceItems', sectionName: 'Workspace Items', isExpanded: false }
