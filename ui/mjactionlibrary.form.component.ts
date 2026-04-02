import { MJActionLibraryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Action Libraries') // Tell MemberJunction about this class
    selector: 'gen-mjactionlibrary-form',
    templateUrl: './mjactionlibrary.form.component.html'
export class MJActionLibraryFormComponent extends BaseFormComponent {
    public record!: MJActionLibraryEntity;
            { sectionKey: 'referenceIDs', sectionName: 'Reference IDs', isExpanded: true },
            { sectionKey: 'actionLibraryInformation', sectionName: 'Action & Library Information', isExpanded: true },
