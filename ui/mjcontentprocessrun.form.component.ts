import { MJContentProcessRunEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Content Process Runs') // Tell MemberJunction about this class
    selector: 'gen-mjcontentprocessrun-form',
    templateUrl: './mjcontentprocessrun.form.component.html'
export class MJContentProcessRunFormComponent extends BaseFormComponent {
    public record!: MJContentProcessRunEntity;
            { sectionKey: 'runMetadata', sectionName: 'Run Metadata', isExpanded: false }
