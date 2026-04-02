import { MJAPIApplicationScopeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: API Application Scopes') // Tell MemberJunction about this class
    selector: 'gen-mjapiapplicationscope-form',
    templateUrl: './mjapiapplicationscope.form.component.html'
export class MJAPIApplicationScopeFormComponent extends BaseFormComponent {
    public record!: MJAPIApplicationScopeEntity;
            { sectionKey: 'scopeRuleDetails', sectionName: 'Scope Rule Details', isExpanded: true },
            { sectionKey: 'applicationAssignment', sectionName: 'Application Assignment', isExpanded: true },
