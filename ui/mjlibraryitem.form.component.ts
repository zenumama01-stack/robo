import { MJLibraryItemEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Library Items') // Tell MemberJunction about this class
    selector: 'gen-mjlibraryitem-form',
    templateUrl: './mjlibraryitem.form.component.html'
export class MJLibraryItemFormComponent extends BaseFormComponent {
    public record!: MJLibraryItemEntity;
            { sectionKey: 'itemInformation', sectionName: 'Item Information', isExpanded: true },
