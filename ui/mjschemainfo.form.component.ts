import { MJSchemaInfoEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Schema Info') // Tell MemberJunction about this class
    selector: 'gen-mjschemainfo-form',
    templateUrl: './mjschemainfo.form.component.html'
export class MJSchemaInfoFormComponent extends BaseFormComponent {
    public record!: MJSchemaInfoEntity;
            { sectionKey: 'identifierRange', sectionName: 'Identifier Range', isExpanded: true },
            { sectionKey: 'schemaInformation', sectionName: 'Schema Information', isExpanded: true },
