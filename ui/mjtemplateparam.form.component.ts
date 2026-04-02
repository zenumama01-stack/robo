import { MJTemplateParamEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Template Params') // Tell MemberJunction about this class
    selector: 'gen-mjtemplateparam-form',
    templateUrl: './mjtemplateparam.form.component.html'
export class MJTemplateParamFormComponent extends BaseFormComponent {
    public record!: MJTemplateParamEntity;
            { sectionKey: 'templateAssociation', sectionName: 'Template Association', isExpanded: true },
            { sectionKey: 'parameterSpecification', sectionName: 'Parameter Specification', isExpanded: true },
            { sectionKey: 'dynamicLinkingFilters', sectionName: 'Dynamic Linking & Filters', isExpanded: false },
