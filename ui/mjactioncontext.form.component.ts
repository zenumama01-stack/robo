import { MJActionContextEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Action Contexts') // Tell MemberJunction about this class
    selector: 'gen-mjactioncontext-form',
    templateUrl: './mjactioncontext.form.component.html'
export class MJActionContextFormComponent extends BaseFormComponent {
    public record!: MJActionContextEntity;
            { sectionKey: 'actionCore', sectionName: 'Action Core', isExpanded: true },
            { sectionKey: 'contextMapping', sectionName: 'Context Mapping', isExpanded: true },
