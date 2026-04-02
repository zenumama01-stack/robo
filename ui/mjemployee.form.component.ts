import { MJEmployeeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Employees') // Tell MemberJunction about this class
    selector: 'gen-mjemployee-form',
    templateUrl: './mjemployee.form.component.html'
export class MJEmployeeFormComponent extends BaseFormComponent {
    public record!: MJEmployeeEntity;
            { sectionKey: 'personalContact', sectionName: 'Personal & Contact', isExpanded: true },
            { sectionKey: 'employmentDetails', sectionName: 'Employment Details', isExpanded: false },
            { sectionKey: 'employeeRoles', sectionName: 'Employee Roles', isExpanded: false },
            { sectionKey: 'employeeSkills', sectionName: 'Employee Skills', isExpanded: false },
            { sectionKey: 'directReports', sectionName: 'Direct Reports', isExpanded: false },
            { sectionKey: 'users', sectionName: 'Users', isExpanded: false }
