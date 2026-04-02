import { MJUserViewRunDetailEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: User View Run Details') // Tell MemberJunction about this class
    selector: 'gen-mjuserviewrundetail-form',
    templateUrl: './mjuserviewrundetail.form.component.html'
export class MJUserViewRunDetailFormComponent extends BaseFormComponent {
    public record!: MJUserViewRunDetailEntity;
            { sectionKey: 'runDetails', sectionName: 'Run Details', isExpanded: true },
