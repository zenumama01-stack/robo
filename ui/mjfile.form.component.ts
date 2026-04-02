import { MJFileEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Files') // Tell MemberJunction about this class
    selector: 'gen-mjfile-form',
    templateUrl: './mjfile.form.component.html'
export class MJFileFormComponent extends BaseFormComponent {
    public record!: MJFileEntity;
            { sectionKey: 'fileBasics', sectionName: 'File Basics', isExpanded: true },
            { sectionKey: 'classificationStatus', sectionName: 'Classification & Status', isExpanded: true },
