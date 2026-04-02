import { MJErrorLogEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Error Logs') // Tell MemberJunction about this class
    selector: 'gen-mjerrorlog-form',
    templateUrl: './mjerrorlog.form.component.html'
export class MJErrorLogFormComponent extends BaseFormComponent {
    public record!: MJErrorLogEntity;
            { sectionKey: 'technicalInformation', sectionName: 'Technical Information', isExpanded: true },
            { sectionKey: 'errorClassification', sectionName: 'Error Classification', isExpanded: true },
            { sectionKey: 'errorContent', sectionName: 'Error Content', isExpanded: false },
