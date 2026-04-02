import { MJConversationDetailArtifactEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Conversation Detail Artifacts') // Tell MemberJunction about this class
    selector: 'gen-mjconversationdetailartifact-form',
    templateUrl: './mjconversationdetailartifact.form.component.html'
export class MJConversationDetailArtifactFormComponent extends BaseFormComponent {
    public record!: MJConversationDetailArtifactEntity;
            { sectionKey: 'coreIdentifiers', sectionName: 'Core Identifiers', isExpanded: true },
