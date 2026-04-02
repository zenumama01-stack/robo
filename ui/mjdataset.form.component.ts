import { MJDatasetEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Datasets') // Tell MemberJunction about this class
    selector: 'gen-mjdataset-form',
    templateUrl: './mjdataset.form.component.html'
export class MJDatasetFormComponent extends BaseFormComponent {
    public record!: MJDatasetEntity;
            { sectionKey: 'datasetCore', sectionName: 'Dataset Core', isExpanded: true },
            { sectionKey: 'datasetItems', sectionName: 'Dataset Items', isExpanded: false }
