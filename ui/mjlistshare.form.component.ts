import { MJListShareEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: List Shares') // Tell MemberJunction about this class
    selector: 'gen-mjlistshare-form',
    templateUrl: './mjlistshare.form.component.html'
export class MJListShareFormComponent extends BaseFormComponent {
    public record!: MJListShareEntity;
            { sectionKey: 'shareDetails', sectionName: 'Share Details', isExpanded: true },
