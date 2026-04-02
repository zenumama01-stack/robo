import { MJEntityActionInvocationTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Action Invocation Types') // Tell MemberJunction about this class
    selector: 'gen-mjentityactioninvocationtype-form',
    templateUrl: './mjentityactioninvocationtype.form.component.html'
export class MJEntityActionInvocationTypeFormComponent extends BaseFormComponent {
    public record!: MJEntityActionInvocationTypeEntity;
            { sectionKey: 'invocationTypeDefinition', sectionName: 'Invocation Type Definition', isExpanded: true },
            { sectionKey: 'entityActionInvocations', sectionName: 'Entity Action Invocations', isExpanded: false }
