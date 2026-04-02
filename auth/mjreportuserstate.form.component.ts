import { MJReportUserStateEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Report User States') // Tell MemberJunction about this class
    selector: 'gen-mjreportuserstate-form',
    templateUrl: './mjreportuserstate.form.component.html'
export class MJReportUserStateFormComponent extends BaseFormComponent {
    public record!: MJReportUserStateEntity;
            { sectionKey: 'recordKeys', sectionName: 'Record Keys', isExpanded: true },
            { sectionKey: 'interactionDetails', sectionName: 'Interaction Details', isExpanded: true },
