import { MJEntityRelationshipDisplayComponentEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Relationship Display Components') // Tell MemberJunction about this class
    selector: 'gen-mjentityrelationshipdisplaycomponent-form',
    templateUrl: './mjentityrelationshipdisplaycomponent.form.component.html'
export class MJEntityRelationshipDisplayComponentFormComponent extends BaseFormComponent {
    public record!: MJEntityRelationshipDisplayComponentEntity;
            { sectionKey: 'componentDefinition', sectionName: 'Component Definition', isExpanded: true },
            { sectionKey: 'entityRelationships', sectionName: 'Entity Relationships', isExpanded: false }
