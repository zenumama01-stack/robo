import { MJDataContextItemEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Data Context Items') // Tell MemberJunction about this class
    selector: 'gen-mjdatacontextitem-form',
    templateUrl: './mjdatacontextitem.form.component.html'
export class MJDataContextItemFormComponent extends BaseFormComponent {
    public record!: MJDataContextItemEntity;
            { sectionKey: 'itemIdentification', sectionName: 'Item Identification', isExpanded: true },
            { sectionKey: 'sourceDefinition', sectionName: 'Source Definition', isExpanded: true },
            { sectionKey: 'cachedSnapshot', sectionName: 'Cached Snapshot', isExpanded: false },
