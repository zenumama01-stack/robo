import { MJDuplicateRunDetailEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Duplicate Run Details') // Tell MemberJunction about this class
    selector: 'gen-mjduplicaterundetail-form',
    templateUrl: './mjduplicaterundetail.form.component.html'
export class MJDuplicateRunDetailFormComponent extends BaseFormComponent {
    public record!: MJDuplicateRunDetailEntity;
            { sectionKey: 'runIdentification', sectionName: 'Run Identification', isExpanded: true },
            { sectionKey: 'processingOutcomes', sectionName: 'Processing Outcomes', isExpanded: true },
            { sectionKey: 'duplicateRunDetailMatches', sectionName: 'Duplicate Run Detail Matches', isExpanded: false }
