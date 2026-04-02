import { MJWorkflowEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Workflows') // Tell MemberJunction about this class
    selector: 'gen-mjworkflow-form',
    templateUrl: './mjworkflow.form.component.html'
export class MJWorkflowFormComponent extends BaseFormComponent {
    public record!: MJWorkflowEntity;
            { sectionKey: 'coreWorkflowDetails', sectionName: 'Core Workflow Details', isExpanded: true },
            { sectionKey: 'schedulingSettings', sectionName: 'Scheduling Settings', isExpanded: true },
            { sectionKey: 'workflowRuns', sectionName: 'Workflow Runs', isExpanded: false }
