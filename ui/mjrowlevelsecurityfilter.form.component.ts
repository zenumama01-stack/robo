import { MJRowLevelSecurityFilterEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Row Level Security Filters') // Tell MemberJunction about this class
    selector: 'gen-mjrowlevelsecurityfilter-form',
    templateUrl: './mjrowlevelsecurityfilter.form.component.html'
export class MJRowLevelSecurityFilterFormComponent extends BaseFormComponent {
    public record!: MJRowLevelSecurityFilterEntity;
            { sectionKey: 'filterDefinition', sectionName: 'Filter Definition', isExpanded: true },
            { sectionKey: 'entityPermissions', sectionName: 'Entity Permissions', isExpanded: false }
