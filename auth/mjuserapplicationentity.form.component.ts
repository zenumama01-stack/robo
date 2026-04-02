import { MJUserApplicationEntityEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: User Application Entities') // Tell MemberJunction about this class
    selector: 'gen-mjuserapplicationentity-form',
    templateUrl: './mjuserapplicationentity.form.component.html'
export class MJUserApplicationEntityFormComponent extends BaseFormComponent {
    public record!: MJUserApplicationEntityEntity;
            { sectionKey: 'userPersonalization', sectionName: 'User Personalization', isExpanded: true },
