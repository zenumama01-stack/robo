import { MJUserNotificationPreferenceEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: User Notification Preferences') // Tell MemberJunction about this class
    selector: 'gen-mjusernotificationpreference-form',
    templateUrl: './mjusernotificationpreference.form.component.html'
export class MJUserNotificationPreferenceFormComponent extends BaseFormComponent {
    public record!: MJUserNotificationPreferenceEntity;
            { sectionKey: 'notificationPreferences', sectionName: 'Notification Preferences', isExpanded: true },
