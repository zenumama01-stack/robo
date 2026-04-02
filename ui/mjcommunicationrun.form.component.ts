import { MJCommunicationRunEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Communication Runs') // Tell MemberJunction about this class
    selector: 'gen-mjcommunicationrun-form',
    templateUrl: './mjcommunicationrun.form.component.html'
export class MJCommunicationRunFormComponent extends BaseFormComponent {
    public record!: MJCommunicationRunEntity;
            { sectionKey: 'runMetadata', sectionName: 'Run Metadata', isExpanded: false },
            { sectionKey: 'executionTimeline', sectionName: 'Execution Timeline', isExpanded: true },
            { sectionKey: 'resultNotes', sectionName: 'Result & Notes', isExpanded: false },
