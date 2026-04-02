import { MJAIArchitectureEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Architectures') // Tell MemberJunction about this class
    selector: 'gen-mjaiarchitecture-form',
    templateUrl: './mjaiarchitecture.form.component.html'
export class MJAIArchitectureFormComponent extends BaseFormComponent {
    public record!: MJAIArchitectureEntity;
            { sectionKey: 'coreArchitecture', sectionName: 'Core Architecture', isExpanded: true },
            { sectionKey: 'hierarchy', sectionName: 'Hierarchy', isExpanded: true },
            { sectionKey: 'publicationReferences', sectionName: 'Publication & References', isExpanded: false },
            { sectionKey: 'mJAIArchitectures', sectionName: 'MJ: AI Architectures', isExpanded: false },
            { sectionKey: 'mJAIModelArchitectures', sectionName: 'MJ: AI Model Architectures', isExpanded: false }
