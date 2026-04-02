import { MJEntityRelationshipEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Relationships') // Tell MemberJunction about this class
    selector: 'gen-mjentityrelationship-form',
    templateUrl: './mjentityrelationship.form.component.html'
export class MJEntityRelationshipFormComponent extends BaseFormComponent {
    public record!: MJEntityRelationshipEntity;
            { sectionKey: 'relationshipCore', sectionName: 'Relationship Core', isExpanded: true },
            { sectionKey: 'aPIQuerySettings', sectionName: 'API & Query Settings', isExpanded: true },
            { sectionKey: 'displayConfiguration', sectionName: 'Display Configuration', isExpanded: false },
            { sectionKey: 'technicalMetadata', sectionName: 'Technical Metadata', isExpanded: false },
