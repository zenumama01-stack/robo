import { MJRecommendationProviderEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Recommendation Providers') // Tell MemberJunction about this class
    selector: 'gen-mjrecommendationprovider-form',
    templateUrl: './mjrecommendationprovider.form.component.html'
export class MJRecommendationProviderFormComponent extends BaseFormComponent {
    public record!: MJRecommendationProviderEntity;
            { sectionKey: 'providerInformation', sectionName: 'Provider Information', isExpanded: true },
            { sectionKey: 'recommendationRuns', sectionName: 'Recommendation Runs', isExpanded: false }
