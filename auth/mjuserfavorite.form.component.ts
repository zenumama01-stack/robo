import { MJUserFavoriteEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: User Favorites') // Tell MemberJunction about this class
    selector: 'gen-mjuserfavorite-form',
    templateUrl: './mjuserfavorite.form.component.html'
export class MJUserFavoriteFormComponent extends BaseFormComponent {
    public record!: MJUserFavoriteEntity;
            { sectionKey: 'favoriteIdentification', sectionName: 'Favorite Identification', isExpanded: true },
            { sectionKey: 'entityMetadata', sectionName: 'Entity Metadata', isExpanded: false },
