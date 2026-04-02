import { MJQueryFieldEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Query Fields') // Tell MemberJunction about this class
    selector: 'gen-mjqueryfield-form',
    templateUrl: './mjqueryfield.form.component.html'
export class MJQueryFieldFormComponent extends BaseFormComponent {
    public record!: MJQueryFieldEntity;
            { sectionKey: 'fieldDefinitionPresentation', sectionName: 'Field Definition & Presentation', isExpanded: true },
            { sectionKey: 'dataTypeSourceMapping', sectionName: 'Data Type & Source Mapping', isExpanded: true },
