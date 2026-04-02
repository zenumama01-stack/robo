import { OrderEntity } from 'mj_generatedentities';
@RegisterClass(BaseFormComponent, 'Orders') // Tell MemberJunction about this class
    selector: 'gen-order-form',
    templateUrl: './order.form.component.html'
export class OrderFormComponent extends BaseFormComponent {
    public record!: OrderEntity;
            { sectionKey: 'orderSummary', sectionName: 'Order Summary', isExpanded: true },
            { sectionKey: 'fulfillment', sectionName: 'Fulfillment', isExpanded: true },
