@RegisterClass(BaseFormComponent, 'MJ: Query Categories') // Tell MemberJunction about this class
    selector: 'gen-mjquerycategory-form',
    templateUrl: './mjquerycategory.form.component.html'
export class MJQueryCategoryFormComponent extends BaseFormComponent {
    public record!: MJQueryCategoryEntity;
            { sectionKey: 'cacheSettings', sectionName: 'Cache Settings', isExpanded: true },
            { sectionKey: 'queryCategories', sectionName: 'Query Categories', isExpanded: false }
