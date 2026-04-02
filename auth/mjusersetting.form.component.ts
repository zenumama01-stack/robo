import { MJUserSettingEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: User Settings') // Tell MemberJunction about this class
    selector: 'gen-mjusersetting-form',
    templateUrl: './mjusersetting.form.component.html'
export class MJUserSettingFormComponent extends BaseFormComponent {
    public record!: MJUserSettingEntity;
            { sectionKey: 'userPreferenceSettings', sectionName: 'User Preference Settings', isExpanded: true },
