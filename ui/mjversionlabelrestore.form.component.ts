import { MJVersionLabelRestoreEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Version Label Restores') // Tell MemberJunction about this class
    selector: 'gen-mjversionlabelrestore-form',
    templateUrl: './mjversionlabelrestore.form.component.html'
export class MJVersionLabelRestoreFormComponent extends BaseFormComponent {
    public record!: MJVersionLabelRestoreEntity;
            { sectionKey: 'restoreOverview', sectionName: 'Restore Overview', isExpanded: true },
            { sectionKey: 'progressMetrics', sectionName: 'Progress Metrics', isExpanded: true },
