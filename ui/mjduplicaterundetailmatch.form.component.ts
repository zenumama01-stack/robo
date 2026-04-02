import { MJDuplicateRunDetailMatchEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Duplicate Run Detail Matches') // Tell MemberJunction about this class
    selector: 'gen-mjduplicaterundetailmatch-form',
    templateUrl: './mjduplicaterundetailmatch.form.component.html'
export class MJDuplicateRunDetailMatchFormComponent extends BaseFormComponent {
    public record!: MJDuplicateRunDetailMatchEntity;
            { sectionKey: 'matchResults', sectionName: 'Match Results', isExpanded: true },
            { sectionKey: 'resolutionManagement', sectionName: 'Resolution Management', isExpanded: true },
