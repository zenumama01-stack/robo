import { MJUserNotificationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: User Notifications') // Tell MemberJunction about this class
    selector: 'gen-mjusernotification-form',
    templateUrl: './mjusernotification.form.component.html'
export class MJUserNotificationFormComponent extends BaseFormComponent {
    public record!: MJUserNotificationEntity;
            { sectionKey: 'notificationOverview', sectionName: 'Notification Overview', isExpanded: true },
            { sectionKey: 'relatedResource', sectionName: 'Related Resource', isExpanded: true },
