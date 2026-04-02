@RegisterClass(BaseFormComponent, 'MJ: AI Agent Examples') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentexample-form',
    templateUrl: './mjaiagentexample.form.component.html'
export class MJAIAgentExampleFormComponent extends BaseFormComponent {
    public record!: MJAIAgentExampleEntity;
            { sectionKey: 'ownershipScope', sectionName: 'Ownership & Scope', isExpanded: true },
            { sectionKey: 'exampleDetails', sectionName: 'Example Details', isExpanded: true },
            { sectionKey: 'sourceProvenance', sectionName: 'Source Provenance', isExpanded: false },
            { sectionKey: 'semanticIndexing', sectionName: 'Semantic Indexing', isExpanded: false },
            { sectionKey: 'usageLifecycle', sectionName: 'Usage & Lifecycle', isExpanded: false },
