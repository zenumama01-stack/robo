import { MJConversationDetailAttachmentEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Conversation Detail Attachments') // Tell MemberJunction about this class
    selector: 'gen-mjconversationdetailattachment-form',
    templateUrl: './mjconversationdetailattachment.form.component.html'
export class MJConversationDetailAttachmentFormComponent extends BaseFormComponent {
    public record!: MJConversationDetailAttachmentEntity;
            { sectionKey: 'attachmentMetadata', sectionName: 'Attachment Metadata', isExpanded: false },
            { sectionKey: 'mediaProperties', sectionName: 'Media Properties', isExpanded: true },
            { sectionKey: 'storageDetails', sectionName: 'Storage Details', isExpanded: false },
