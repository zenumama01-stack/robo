import { MJWorkflowRunEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Workflow Runs') // Tell MemberJunction about this class
    selector: 'gen-mjworkflowrun-form',
    templateUrl: './mjworkflowrun.form.component.html'
export class MJWorkflowRunFormComponent extends BaseFormComponent {
    public record!: MJWorkflowRunEntity;
            { sectionKey: 'workflowIdentification', sectionName: 'Workflow Identification', isExpanded: true },
            { sectionKey: 'executionTimelineOutcome', sectionName: 'Execution Timeline & Outcome', isExpanded: true },
