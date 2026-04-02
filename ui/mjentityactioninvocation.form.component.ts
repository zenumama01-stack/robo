import { MJEntityActionInvocationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Action Invocations') // Tell MemberJunction about this class
    selector: 'gen-mjentityactioninvocation-form',
    templateUrl: './mjentityactioninvocation.form.component.html'
export class MJEntityActionInvocationFormComponent extends BaseFormComponent {
    public record!: MJEntityActionInvocationEntity;
            { sectionKey: 'invocationConfiguration', sectionName: 'Invocation Configuration', isExpanded: true },
            { sectionKey: 'invocationStatus', sectionName: 'Invocation Status', isExpanded: true },
