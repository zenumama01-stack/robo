import { MJAIModelCostEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Model Costs') // Tell MemberJunction about this class
    selector: 'gen-mjaimodelcost-form',
    templateUrl: './mjaimodelcost.form.component.html'
export class MJAIModelCostFormComponent extends BaseFormComponent {
    public record!: MJAIModelCostEntity;
            { sectionKey: 'modelProvider', sectionName: 'Model & Provider', isExpanded: true },
            { sectionKey: 'validityProcessing', sectionName: 'Validity & Processing', isExpanded: true },
            { sectionKey: 'pricingDetails', sectionName: 'Pricing Details', isExpanded: false },
