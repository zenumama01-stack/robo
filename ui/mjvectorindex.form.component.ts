import { MJVectorIndexEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Vector Indexes') // Tell MemberJunction about this class
    selector: 'gen-mjvectorindex-form',
    templateUrl: './mjvectorindex.form.component.html'
export class MJVectorIndexFormComponent extends BaseFormComponent {
    public record!: MJVectorIndexEntity;
            { sectionKey: 'indexProfile', sectionName: 'Index Profile', isExpanded: true },
            { sectionKey: 'associatedResources', sectionName: 'Associated Resources', isExpanded: true },
