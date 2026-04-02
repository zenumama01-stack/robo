import { MJAICredentialBindingEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Credential Bindings') // Tell MemberJunction about this class
    selector: 'gen-mjaicredentialbinding-form',
    templateUrl: './mjaicredentialbinding.form.component.html'
export class MJAICredentialBindingFormComponent extends BaseFormComponent {
    public record!: MJAICredentialBindingEntity;
            { sectionKey: 'details', sectionName: 'Details', isExpanded: true }
