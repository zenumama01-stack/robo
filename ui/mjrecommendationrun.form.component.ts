import { MJRecommendationRunEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Recommendation Runs') // Tell MemberJunction about this class
    selector: 'gen-mjrecommendationrun-form',
    templateUrl: './mjrecommendationrun.form.component.html'
export class MJRecommendationRunFormComponent extends BaseFormComponent {
    public record!: MJRecommendationRunEntity;
            { sectionKey: 'runScheduleStatus', sectionName: 'Run Schedule & Status', isExpanded: true },
            { sectionKey: 'runDescription', sectionName: 'Run Description', isExpanded: false },
            { sectionKey: 'recommendations', sectionName: 'Recommendations', isExpanded: false }
