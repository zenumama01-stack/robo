import { MJAuthorizationRoleEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Authorization Roles') // Tell MemberJunction about this class
    selector: 'gen-mjauthorizationrole-form',
    templateUrl: './mjauthorizationrole.form.component.html'
export class MJAuthorizationRoleFormComponent extends BaseFormComponent {
    public record!: MJAuthorizationRoleEntity;
            { sectionKey: 'referenceKeys', sectionName: 'Reference Keys', isExpanded: true },
            { sectionKey: 'accessSettings', sectionName: 'Access Settings', isExpanded: true },
