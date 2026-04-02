import { MJEntityPermissionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Permissions') // Tell MemberJunction about this class
    selector: 'gen-mjentitypermission-form',
    templateUrl: './mjentitypermission.form.component.html'
export class MJEntityPermissionFormComponent extends BaseFormComponent {
    public record!: MJEntityPermissionEntity;
            { sectionKey: 'entityRoleDetails', sectionName: 'Entity & Role Details', isExpanded: true },
            { sectionKey: 'accessRights', sectionName: 'Access Rights', isExpanded: true },
            { sectionKey: 'securityFilters', sectionName: 'Security Filters', isExpanded: false },
