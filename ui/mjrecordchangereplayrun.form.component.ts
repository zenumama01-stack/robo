import { MJRecordChangeReplayRunEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Record Change Replay Runs') // Tell MemberJunction about this class
    selector: 'gen-mjrecordchangereplayrun-form',
    templateUrl: './mjrecordchangereplayrun.form.component.html'
export class MJRecordChangeReplayRunFormComponent extends BaseFormComponent {
    public record!: MJRecordChangeReplayRunEntity;
