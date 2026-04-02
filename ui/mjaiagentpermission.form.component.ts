import { MJAIAgentPermissionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Permissions') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentpermission-form',
    templateUrl: './mjaiagentpermission.form.component.html'
export class MJAIAgentPermissionFormComponent extends BaseFormComponent {
    public record!: MJAIAgentPermissionEntity;
            { sectionKey: 'administrativeMetadata', sectionName: 'Administrative Metadata', isExpanded: false },
            { sectionKey: 'assignmentTargets', sectionName: 'Assignment Targets', isExpanded: true },
            { sectionKey: 'permissionLevels', sectionName: 'Permission Levels', isExpanded: false },
