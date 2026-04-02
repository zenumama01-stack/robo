import { MJEntityAIActionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity AI Actions') // Tell MemberJunction about this class
    selector: 'gen-mjentityaiaction-form',
    templateUrl: './mjentityaiaction.form.component.html'
export class MJEntityAIActionFormComponent extends BaseFormComponent {
    public record!: MJEntityAIActionEntity;
            { sectionKey: 'identificationRelationships', sectionName: 'Identification & Relationships', isExpanded: true },
            { sectionKey: 'aIActionConfiguration', sectionName: 'AI Action Configuration', isExpanded: true },
            { sectionKey: 'metadataNotes', sectionName: 'Metadata & Notes', isExpanded: false },
