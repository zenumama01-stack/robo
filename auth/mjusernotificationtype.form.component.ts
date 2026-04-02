import { MJUserNotificationTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: User Notification Types') // Tell MemberJunction about this class
    selector: 'gen-mjusernotificationtype-form',
    templateUrl: './mjusernotificationtype.form.component.html'
export class MJUserNotificationTypeFormComponent extends BaseFormComponent {
    public record!: MJUserNotificationTypeEntity;
            { sectionKey: 'notificationDetails', sectionName: 'Notification Details', isExpanded: true },
            { sectionKey: 'deliveryDefaults', sectionName: 'Delivery Defaults', isExpanded: true },
            { sectionKey: 'templateSettings', sectionName: 'Template Settings', isExpanded: false },
            { sectionKey: 'mJUserNotificationPreferences', sectionName: 'MJ: User Notification Preferences', isExpanded: false }
