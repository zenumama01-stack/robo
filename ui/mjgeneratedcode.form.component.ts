import { MJGeneratedCodeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Generated Codes') // Tell MemberJunction about this class
    selector: 'gen-mjgeneratedcode-form',
    templateUrl: './mjgeneratedcode.form.component.html'
export class MJGeneratedCodeFormComponent extends BaseFormComponent {
    public record!: MJGeneratedCodeEntity;
            { sectionKey: 'codeMetadata', sectionName: 'Code Metadata', isExpanded: false },
            { sectionKey: 'timelineAudit', sectionName: 'Timeline & Audit', isExpanded: true },
            { sectionKey: 'sourceRelationships', sectionName: 'Source & Relationships', isExpanded: false },
