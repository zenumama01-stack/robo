import { MJWorkflowEngineEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Workflow Engines') // Tell MemberJunction about this class
    selector: 'gen-mjworkflowengine-form',
    templateUrl: './mjworkflowengine.form.component.html'
export class MJWorkflowEngineFormComponent extends BaseFormComponent {
    public record!: MJWorkflowEngineEntity;
            { sectionKey: 'engineSpecification', sectionName: 'Engine Specification', isExpanded: true },
            { sectionKey: 'workflows', sectionName: 'Workflows', isExpanded: false }
