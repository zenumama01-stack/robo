import { MJVersionLabelItemEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Version Label Items') // Tell MemberJunction about this class
    selector: 'gen-mjversionlabelitem-form',
    templateUrl: './mjversionlabelitem.form.component.html'
export class MJVersionLabelItemFormComponent extends BaseFormComponent {
    public record!: MJVersionLabelItemEntity;
            { sectionKey: 'versionMapping', sectionName: 'Version Mapping', isExpanded: true },
