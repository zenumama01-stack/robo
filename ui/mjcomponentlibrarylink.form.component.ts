import { MJComponentLibraryLinkEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Component Library Links') // Tell MemberJunction about this class
    selector: 'gen-mjcomponentlibrarylink-form',
    templateUrl: './mjcomponentlibrarylink.form.component.html'
export class MJComponentLibraryLinkFormComponent extends BaseFormComponent {
    public record!: MJComponentLibraryLinkEntity;
            { sectionKey: 'componentLinkDetails', sectionName: 'Component Link Details', isExpanded: true },
            { sectionKey: 'libraryDependency', sectionName: 'Library Dependency', isExpanded: true },
