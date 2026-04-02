@RegisterClass(BaseFormComponent, 'MJ: AI Models') // Tell MemberJunction about this class
    selector: 'gen-mjaimodel-form',
    templateUrl: './mjaimodel.form.component.html'
export class MJAIModelFormComponent extends BaseFormComponent {
    public record!: MJAIModelEntity;
            { sectionKey: 'modelOverview', sectionName: 'Model Overview', isExpanded: true },
            { sectionKey: 'performanceMetrics', sectionName: 'Performance Metrics', isExpanded: true },
            { sectionKey: 'technicalSpecifications', sectionName: 'Technical Specifications', isExpanded: false },
            { sectionKey: 'aIActions', sectionName: 'AI Actions', isExpanded: false },
            { sectionKey: 'entityDocuments', sectionName: 'Entity Documents', isExpanded: false },
            { sectionKey: 'vectorIndexes', sectionName: 'Vector Indexes', isExpanded: false },
            { sectionKey: 'mJAIModelArchitectures', sectionName: 'MJ: AI Model Architectures', isExpanded: false },
            { sectionKey: 'contentTypes', sectionName: 'Content Types', isExpanded: false },
            { sectionKey: 'entityAIActions', sectionName: 'Entity AI Actions', isExpanded: false },
            { sectionKey: 'mJAIModelVendors', sectionName: 'MJ: AI Model Vendors', isExpanded: false },
            { sectionKey: 'mJAIModelCosts', sectionName: 'MJ: AI Model Costs', isExpanded: false },
            { sectionKey: 'generatedCodes', sectionName: 'Generated Codes', isExpanded: false },
            { sectionKey: 'queries', sectionName: 'Queries', isExpanded: false },
            { sectionKey: 'aIModels', sectionName: 'AI Models', isExpanded: false }
