import { MJEmployeeRoleEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Employee Roles') // Tell MemberJunction about this class
    selector: 'gen-mjemployeerole-form',
    templateUrl: './mjemployeerole.form.component.html'
export class MJEmployeeRoleFormComponent extends BaseFormComponent {
    public record!: MJEmployeeRoleEntity;
            { sectionKey: 'entityKeys', sectionName: 'Entity Keys', isExpanded: true },
            { sectionKey: 'roleAssignment', sectionName: 'Role Assignment', isExpanded: true },
