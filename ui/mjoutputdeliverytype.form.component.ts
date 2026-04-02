import { MJOutputDeliveryTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Output Delivery Types') // Tell MemberJunction about this class
    selector: 'gen-mjoutputdeliverytype-form',
    templateUrl: './mjoutputdeliverytype.form.component.html'
export class MJOutputDeliveryTypeFormComponent extends BaseFormComponent {
    public record!: MJOutputDeliveryTypeEntity;
            { sectionKey: 'deliveryTypeDetails', sectionName: 'Delivery Type Details', isExpanded: true },
            { sectionKey: 'reports', sectionName: 'Reports', isExpanded: false }
