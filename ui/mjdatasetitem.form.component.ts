import { MJDatasetItemEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Dataset Items') // Tell MemberJunction about this class
    selector: 'gen-mjdatasetitem-form',
    templateUrl: './mjdatasetitem.form.component.html'
export class MJDatasetItemFormComponent extends BaseFormComponent {
    public record!: MJDatasetItemEntity;
            { sectionKey: 'itemIdentity', sectionName: 'Item Identity', isExpanded: true },
            { sectionKey: 'processingSettings', sectionName: 'Processing Settings', isExpanded: true },
            { sectionKey: 'displayDocumentation', sectionName: 'Display & Documentation', isExpanded: false },
