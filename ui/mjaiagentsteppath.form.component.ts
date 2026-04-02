import { MJAIAgentStepPathEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Step Paths') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentsteppath-form',
    templateUrl: './mjaiagentsteppath.form.component.html'
export class MJAIAgentStepPathFormComponent extends BaseFormComponent {
    public record!: MJAIAgentStepPathEntity;
            { sectionKey: 'pathCoreDetails', sectionName: 'Path Core Details', isExpanded: true },
            { sectionKey: 'routingRules', sectionName: 'Routing Rules', isExpanded: true },
