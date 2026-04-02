import { MJArtifactVersionAttributeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Artifact Version Attributes') // Tell MemberJunction about this class
    selector: 'gen-mjartifactversionattribute-form',
    templateUrl: './mjartifactversionattribute.form.component.html'
export class MJArtifactVersionAttributeFormComponent extends BaseFormComponent {
    public record!: MJArtifactVersionAttributeEntity;
            { sectionKey: 'relationshipContext', sectionName: 'Relationship Context', isExpanded: true },
            { sectionKey: 'attributeDefinition', sectionName: 'Attribute Definition', isExpanded: true },
            { sectionKey: 'extractedValue', sectionName: 'Extracted Value', isExpanded: false },
