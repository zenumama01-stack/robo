import { MJAIModelActionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Model Actions') // Tell MemberJunction about this class
    selector: 'gen-mjaimodelaction-form',
    templateUrl: './mjaimodelaction.form.component.html'
export class MJAIModelActionFormComponent extends BaseFormComponent {
    public record!: MJAIModelActionEntity;
            { sectionKey: 'modelConfiguration', sectionName: 'Model Configuration', isExpanded: true },
            { sectionKey: 'actionSettings', sectionName: 'Action Settings', isExpanded: true },
