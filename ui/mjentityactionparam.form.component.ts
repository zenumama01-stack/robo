import { MJEntityActionParamEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Action Params') // Tell MemberJunction about this class
    selector: 'gen-mjentityactionparam-form',
    templateUrl: './mjentityactionparam.form.component.html'
export class MJEntityActionParamFormComponent extends BaseFormComponent {
    public record!: MJEntityActionParamEntity;
            { sectionKey: 'identifierRelationships', sectionName: 'Identifier & Relationships', isExpanded: true },
