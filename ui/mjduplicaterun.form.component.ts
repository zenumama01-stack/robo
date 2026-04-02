import { MJDuplicateRunEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Duplicate Runs') // Tell MemberJunction about this class
    selector: 'gen-mjduplicaterun-form',
    templateUrl: './mjduplicaterun.form.component.html'
export class MJDuplicateRunFormComponent extends BaseFormComponent {
    public record!: MJDuplicateRunEntity;
            { sectionKey: 'approvalInformation', sectionName: 'Approval Information', isExpanded: true },
            { sectionKey: 'processingStatus', sectionName: 'Processing Status', isExpanded: false },
            { sectionKey: 'duplicateRunDetails', sectionName: 'Duplicate Run Details', isExpanded: false }
