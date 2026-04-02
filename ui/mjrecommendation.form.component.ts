import { MJRecommendationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Recommendations') // Tell MemberJunction about this class
    selector: 'gen-mjrecommendation-form',
    templateUrl: './mjrecommendation.form.component.html'
export class MJRecommendationFormComponent extends BaseFormComponent {
    public record!: MJRecommendationEntity;
            { sectionKey: 'recommendationCore', sectionName: 'Recommendation Core', isExpanded: true },
            { sectionKey: 'recommendationItems', sectionName: 'Recommendation Items', isExpanded: false }
