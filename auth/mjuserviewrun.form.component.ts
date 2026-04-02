import { MJUserViewRunEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: User View Runs') // Tell MemberJunction about this class
    selector: 'gen-mjuserviewrun-form',
    templateUrl: './mjuserviewrun.form.component.html'
export class MJUserViewRunFormComponent extends BaseFormComponent {
    public record!: MJUserViewRunEntity;
            { sectionKey: 'viewDefinition', sectionName: 'View Definition', isExpanded: true },
            { sectionKey: 'details', sectionName: 'Details', isExpanded: false }
