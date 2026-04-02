import { MJExplorerNavigationItemEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Explorer Navigation Items') // Tell MemberJunction about this class
    selector: 'gen-mjexplorernavigationitem-form',
    templateUrl: './mjexplorernavigationitem.form.component.html'
export class MJExplorerNavigationItemFormComponent extends BaseFormComponent {
    public record!: MJExplorerNavigationItemEntity;
            { sectionKey: 'coreNavigationItem', sectionName: 'Core Navigation Item', isExpanded: true },
            { sectionKey: 'presentationSettings', sectionName: 'Presentation Settings', isExpanded: true },
