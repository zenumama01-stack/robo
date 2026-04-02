import { MJAPIKeyApplicationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: API Key Applications') // Tell MemberJunction about this class
    selector: 'gen-mjapikeyapplication-form',
    templateUrl: './mjapikeyapplication.form.component.html'
export class MJAPIKeyApplicationFormComponent extends BaseFormComponent {
    public record!: MJAPIKeyApplicationEntity;
            { sectionKey: 'aPIKeyAssignment', sectionName: 'API Key Assignment', isExpanded: true },
