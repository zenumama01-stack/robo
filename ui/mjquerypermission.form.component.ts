import { MJQueryPermissionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Query Permissions') // Tell MemberJunction about this class
    selector: 'gen-mjquerypermission-form',
    templateUrl: './mjquerypermission.form.component.html'
export class MJQueryPermissionFormComponent extends BaseFormComponent {
    public record!: MJQueryPermissionEntity;
            { sectionKey: 'permissionRecord', sectionName: 'Permission Record', isExpanded: true },
            { sectionKey: 'descriptiveLabels', sectionName: 'Descriptive Labels', isExpanded: true },
