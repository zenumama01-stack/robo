import { MJAuthorizationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Authorizations') // Tell MemberJunction about this class
    selector: 'gen-mjauthorization-form',
    templateUrl: './mjauthorization.form.component.html'
export class MJAuthorizationFormComponent extends BaseFormComponent {
    public record!: MJAuthorizationEntity;
            { sectionKey: 'authorizationHierarchy', sectionName: 'Authorization Hierarchy', isExpanded: true },
            { sectionKey: 'authorizationCore', sectionName: 'Authorization Core', isExpanded: true },
            { sectionKey: 'actionAuthorizations', sectionName: 'Action Authorizations', isExpanded: false },
            { sectionKey: 'auditLogs', sectionName: 'Audit Logs', isExpanded: false },
            { sectionKey: 'mJAuthorizationRoles', sectionName: 'MJ: Authorization Roles', isExpanded: false },
            { sectionKey: 'authorizations', sectionName: 'Authorizations', isExpanded: false }
