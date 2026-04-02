import { MJEntityFieldValueEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Field Values') // Tell MemberJunction about this class
    selector: 'gen-mjentityfieldvalue-form',
    templateUrl: './mjentityfieldvalue.form.component.html'
export class MJEntityFieldValueFormComponent extends BaseFormComponent {
    public record!: MJEntityFieldValueEntity;
            { sectionKey: 'entityAssociation', sectionName: 'Entity Association', isExpanded: true },
            { sectionKey: 'lookupDefinition', sectionName: 'Lookup Definition', isExpanded: true },
