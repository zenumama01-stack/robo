import { MJAccessControlRuleEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Access Control Rules') // Tell MemberJunction about this class
    selector: 'gen-mjaccesscontrolrule-form',
    templateUrl: './mjaccesscontrolrule.form.component.html'
export class MJAccessControlRuleFormComponent extends BaseFormComponent {
    public record!: MJAccessControlRuleEntity;
            { sectionKey: 'accessTarget', sectionName: 'Access Target', isExpanded: true },
            { sectionKey: 'granteePermissions', sectionName: 'Grantee & Permissions', isExpanded: true },
            { sectionKey: 'validityAdministration', sectionName: 'Validity & Administration', isExpanded: false },
