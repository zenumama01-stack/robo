@RegisterClass(BaseFormComponent, 'MJ: AI Agent Data Sources') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentdatasource-form',
    templateUrl: './mjaiagentdatasource.form.component.html'
export class MJAIAgentDataSourceFormComponent extends BaseFormComponent {
    public record!: MJAIAgentDataSourceEntity;
            { sectionKey: 'agentAssociation', sectionName: 'Agent Association', isExpanded: true },
            { sectionKey: 'sourceSpecification', sectionName: 'Source Specification', isExpanded: true },
            { sectionKey: 'retrievalMapping', sectionName: 'Retrieval & Mapping', isExpanded: false },
            { sectionKey: 'cachingStatus', sectionName: 'Caching & Status', isExpanded: false },
