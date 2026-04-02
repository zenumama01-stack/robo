import { MJAIModelPriceTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Model Price Types') // Tell MemberJunction about this class
    selector: 'gen-mjaimodelpricetype-form',
    templateUrl: './mjaimodelpricetype.form.component.html'
export class MJAIModelPriceTypeFormComponent extends BaseFormComponent {
    public record!: MJAIModelPriceTypeEntity;
            { sectionKey: 'pricingMetricDetails', sectionName: 'Pricing Metric Details', isExpanded: true },
            { sectionKey: 'mJAIModelCosts', sectionName: 'MJ: AI Model Costs', isExpanded: false }
