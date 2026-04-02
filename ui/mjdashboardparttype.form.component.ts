import { MJDashboardPartTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Dashboard Part Types') // Tell MemberJunction about this class
    selector: 'gen-mjdashboardparttype-form',
    templateUrl: './mjdashboardparttype.form.component.html'
export class MJDashboardPartTypeFormComponent extends BaseFormComponent {
    public record!: MJDashboardPartTypeEntity;
            { sectionKey: 'panelTypeInformation', sectionName: 'Panel Type Information', isExpanded: true },
            { sectionKey: 'rendererSettings', sectionName: 'Renderer Settings', isExpanded: true },
