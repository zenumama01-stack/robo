@RegisterClass(BaseResourceComponent, 'NotificationsResource')
    selector: 'mj-notifications-resource',
    template: `<app-user-notifications></app-user-notifications>`
export class NotificationsResource extends BaseResourceComponent implements OnInit {
        return 'Notifications';
        return 'fa-solid fa-bell';
