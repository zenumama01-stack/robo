import { MJAIModelPriceUnitTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Model Price Unit Types') // Tell MemberJunction about this class
    selector: 'gen-mjaimodelpriceunittype-form',
    templateUrl: './mjaimodelpriceunittype.form.component.html'
export class MJAIModelPriceUnitTypeFormComponent extends BaseFormComponent {
    public record!: MJAIModelPriceUnitTypeEntity;
            { sectionKey: 'unitDefinition', sectionName: 'Unit Definition', isExpanded: true },
