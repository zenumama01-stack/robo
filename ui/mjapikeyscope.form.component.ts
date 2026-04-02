import { MJAPIKeyScopeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: API Key Scopes') // Tell MemberJunction about this class
    selector: 'gen-mjapikeyscope-form',
    templateUrl: './mjapikeyscope.form.component.html'
export class MJAPIKeyScopeFormComponent extends BaseFormComponent {
    public record!: MJAPIKeyScopeEntity;
            { sectionKey: 'keyScopeMapping', sectionName: 'Key Scope Mapping', isExpanded: true },
            { sectionKey: 'accessRules', sectionName: 'Access Rules', isExpanded: true },
