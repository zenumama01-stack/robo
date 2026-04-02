import { MJFileCategoryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: File Categories') // Tell MemberJunction about this class
    selector: 'gen-mjfilecategory-form',
    templateUrl: './mjfilecategory.form.component.html'
export class MJFileCategoryFormComponent extends BaseFormComponent {
    public record!: MJFileCategoryEntity;
            { sectionKey: 'hierarchyIdentifiers', sectionName: 'Hierarchy Identifiers', isExpanded: true },
            { sectionKey: 'categoryInformation', sectionName: 'Category Information', isExpanded: true },
            { sectionKey: 'fileCategories', sectionName: 'File Categories', isExpanded: false },
            { sectionKey: 'files', sectionName: 'Files', isExpanded: false }
