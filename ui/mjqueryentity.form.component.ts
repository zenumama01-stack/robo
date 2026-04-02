import { MJQueryEntityEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Query Entities') // Tell MemberJunction about this class
    selector: 'gen-mjqueryentity-form',
    templateUrl: './mjqueryentity.form.component.html'
export class MJQueryEntityFormComponent extends BaseFormComponent {
    public record!: MJQueryEntityEntity;
            { sectionKey: 'queryMapping', sectionName: 'Query Mapping', isExpanded: true },
