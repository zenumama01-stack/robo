import { MJListDetailEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: List Details') // Tell MemberJunction about this class
    selector: 'gen-mjlistdetail-form',
    templateUrl: './mjlistdetail.form.component.html'
export class MJListDetailFormComponent extends BaseFormComponent {
    public record!: MJListDetailEntity;
            { sectionKey: 'listReference', sectionName: 'List Reference', isExpanded: true },
            { sectionKey: 'detailAttributes', sectionName: 'Detail Attributes', isExpanded: true },
