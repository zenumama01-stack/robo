import { MJAIModelArchitectureEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Model Architectures') // Tell MemberJunction about this class
    selector: 'gen-mjaimodelarchitecture-form',
    templateUrl: './mjaimodelarchitecture.form.component.html'
export class MJAIModelArchitectureFormComponent extends BaseFormComponent {
    public record!: MJAIModelArchitectureEntity;
            { sectionKey: 'modelArchitectureLink', sectionName: 'Model-Architecture Link', isExpanded: true },
            { sectionKey: 'contributionSettings', sectionName: 'Contribution Settings', isExpanded: true },
