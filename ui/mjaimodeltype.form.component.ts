import { MJAIModelTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Model Types') // Tell MemberJunction about this class
    selector: 'gen-mjaimodeltype-form',
    templateUrl: './mjaimodeltype.form.component.html'
export class MJAIModelTypeFormComponent extends BaseFormComponent {
    public record!: MJAIModelTypeEntity;
            { sectionKey: 'modelInformation', sectionName: 'Model Information', isExpanded: true },
            { sectionKey: 'defaultModality', sectionName: 'Default Modality', isExpanded: true },
            { sectionKey: 'aIModels', sectionName: 'AI Models', isExpanded: false },
            { sectionKey: 'aIPrompts', sectionName: 'AI Prompts', isExpanded: false }
