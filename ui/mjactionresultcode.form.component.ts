import { MJActionResultCodeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Action Result Codes') // Tell MemberJunction about this class
    selector: 'gen-mjactionresultcode-form',
    templateUrl: './mjactionresultcode.form.component.html'
export class MJActionResultCodeFormComponent extends BaseFormComponent {
    public record!: MJActionResultCodeEntity;
            { sectionKey: 'actionMapping', sectionName: 'Action Mapping', isExpanded: true },
            { sectionKey: 'resultCodeDetails', sectionName: 'Result Code Details', isExpanded: true },
