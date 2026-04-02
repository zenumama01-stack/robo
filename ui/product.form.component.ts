import { ProductEntity } from 'mj_generatedentities';
@RegisterClass(BaseFormComponent, 'Products') // Tell MemberJunction about this class
    selector: 'gen-product-form',
    templateUrl: './product.form.component.html'
export class ProductFormComponent extends BaseFormComponent {
    public record!: ProductEntity;
            { sectionKey: 'pricingInventory', sectionName: 'Pricing & Inventory', isExpanded: true },
            { sectionKey: 'meetings', sectionName: 'Meetings', isExpanded: false },
            { sectionKey: 'publications', sectionName: 'Publications', isExpanded: false }
