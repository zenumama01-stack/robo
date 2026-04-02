import { MJTemplateContentTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Template Content Types') // Tell MemberJunction about this class
    selector: 'gen-mjtemplatecontenttype-form',
    templateUrl: './mjtemplatecontenttype.form.component.html'
export class MJTemplateContentTypeFormComponent extends BaseFormComponent {
    public record!: MJTemplateContentTypeEntity;
            { sectionKey: 'templateDefinition', sectionName: 'Template Definition', isExpanded: true },
            { sectionKey: 'templateContents', sectionName: 'Template Contents', isExpanded: false }
