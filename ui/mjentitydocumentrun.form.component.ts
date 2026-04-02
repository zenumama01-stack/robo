import { MJEntityDocumentRunEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Document Runs') // Tell MemberJunction about this class
    selector: 'gen-mjentitydocumentrun-form',
    templateUrl: './mjentitydocumentrun.form.component.html'
export class MJEntityDocumentRunFormComponent extends BaseFormComponent {
    public record!: MJEntityDocumentRunEntity;
            { sectionKey: 'documentAssociation', sectionName: 'Document Association', isExpanded: true },
            { sectionKey: 'runTimingStatus', sectionName: 'Run Timing & Status', isExpanded: true },
