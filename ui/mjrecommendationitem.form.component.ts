import { MJRecommendationItemEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Recommendation Items') // Tell MemberJunction about this class
    selector: 'gen-mjrecommendationitem-form',
    templateUrl: './mjrecommendationitem.form.component.html'
export class MJRecommendationItemFormComponent extends BaseFormComponent {
    public record!: MJRecommendationItemEntity;
            { sectionKey: 'recommendationData', sectionName: 'Recommendation Data', isExpanded: true },
