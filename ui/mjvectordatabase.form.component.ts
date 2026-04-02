import { MJVectorDatabaseEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Vector Databases') // Tell MemberJunction about this class
    selector: 'gen-mjvectordatabase-form',
    templateUrl: './mjvectordatabase.form.component.html'
export class MJVectorDatabaseFormComponent extends BaseFormComponent {
    public record!: MJVectorDatabaseEntity;
            { sectionKey: 'vectorDatabaseDetails', sectionName: 'Vector Database Details', isExpanded: true },
            { sectionKey: 'vectorIndexes', sectionName: 'Vector Indexes', isExpanded: false }
