import { MJTemplateContentEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Template Contents') // Tell MemberJunction about this class
    selector: 'gen-mjtemplatecontent-form',
    templateUrl: './mjtemplatecontent.form.component.html'
export class MJTemplateContentFormComponent extends BaseFormComponent {
    public record!: MJTemplateContentEntity;
            { sectionKey: 'templateIdentification', sectionName: 'Template Identification', isExpanded: true },
            { sectionKey: 'contentDetails', sectionName: 'Content Details', isExpanded: true },
            { sectionKey: 'templateParams', sectionName: 'Template Params', isExpanded: false }
