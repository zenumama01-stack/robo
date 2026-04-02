import { MJScheduledActionParamEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Scheduled Action Params') // Tell MemberJunction about this class
    selector: 'gen-mjscheduledactionparam-form',
    templateUrl: './mjscheduledactionparam.form.component.html'
export class MJScheduledActionParamFormComponent extends BaseFormComponent {
    public record!: MJScheduledActionParamEntity;
            { sectionKey: 'parameterSettings', sectionName: 'Parameter Settings', isExpanded: true },
