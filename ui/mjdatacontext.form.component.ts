import { MJDataContextEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Data Contexts') // Tell MemberJunction about this class
    selector: 'gen-mjdatacontext-form',
    templateUrl: './mjdatacontext.form.component.html'
export class MJDataContextFormComponent extends BaseFormComponent {
    public record!: MJDataContextEntity;
            { sectionKey: 'identifiers', sectionName: 'Identifiers', isExpanded: true },
            { sectionKey: 'contextDetails', sectionName: 'Context Details', isExpanded: true },
            { sectionKey: 'dataContextItems', sectionName: 'Data Context Items', isExpanded: false },
            { sectionKey: 'conversations', sectionName: 'Conversations', isExpanded: false }
