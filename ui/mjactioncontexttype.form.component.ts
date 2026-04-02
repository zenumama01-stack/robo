import { MJActionContextTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Action Context Types') // Tell MemberJunction about this class
    selector: 'gen-mjactioncontexttype-form',
    templateUrl: './mjactioncontexttype.form.component.html'
export class MJActionContextTypeFormComponent extends BaseFormComponent {
    public record!: MJActionContextTypeEntity;
            { sectionKey: 'contextDefinition', sectionName: 'Context Definition', isExpanded: true },
            { sectionKey: 'actionContexts', sectionName: 'Action Contexts', isExpanded: false }
