import { MJOutputTriggerTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Output Trigger Types') // Tell MemberJunction about this class
    selector: 'gen-mjoutputtriggertype-form',
    templateUrl: './mjoutputtriggertype.form.component.html'
export class MJOutputTriggerTypeFormComponent extends BaseFormComponent {
    public record!: MJOutputTriggerTypeEntity;
            { sectionKey: 'triggerDetails', sectionName: 'Trigger Details', isExpanded: true },
