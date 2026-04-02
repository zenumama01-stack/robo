import { CustomerEntity } from 'mj_generatedentities';
@RegisterClass(BaseFormComponent, 'Customers') // Tell MemberJunction about this class
    selector: 'gen-customer-form',
    templateUrl: './customer.form.component.html'
export class CustomerFormComponent extends BaseFormComponent {
    public record!: CustomerEntity;
            { sectionKey: 'personalInformation', sectionName: 'Personal Information', isExpanded: true },
            { sectionKey: 'accountDetails', sectionName: 'Account Details', isExpanded: true },
            { sectionKey: 'location', sectionName: 'Location', isExpanded: false },
            { sectionKey: 'orders', sectionName: 'Orders', isExpanded: false }
