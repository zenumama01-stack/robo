import { vwCustomerOrderSummaryEntity } from 'mj_generatedentities';
@RegisterClass(BaseFormComponent, 'Customer Order Summaries') // Tell MemberJunction about this class
    selector: 'gen-vwcustomerordersummary-form',
    templateUrl: './vwcustomerordersummary.form.component.html'
export class vwCustomerOrderSummaryFormComponent extends BaseFormComponent {
    public record!: vwCustomerOrderSummaryEntity;
