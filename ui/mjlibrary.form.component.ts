import { MJLibraryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Libraries') // Tell MemberJunction about this class
    selector: 'gen-mjlibrary-form',
    templateUrl: './mjlibrary.form.component.html'
export class MJLibraryFormComponent extends BaseFormComponent {
    public record!: MJLibraryEntity;
            { sectionKey: 'contentAvailability', sectionName: 'Content & Availability', isExpanded: true },
            { sectionKey: 'items', sectionName: 'Items', isExpanded: false }
