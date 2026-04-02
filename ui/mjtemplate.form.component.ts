@RegisterClass(BaseFormComponent, 'MJ: Templates') // Tell MemberJunction about this class
    selector: 'gen-mjtemplate-form',
    templateUrl: './mjtemplate.form.component.html'
export class MJTemplateFormComponent extends BaseFormComponent {
            { sectionKey: 'templateContent', sectionName: 'Template Content', isExpanded: true },
            { sectionKey: 'availabilityStatus', sectionName: 'Availability & Status', isExpanded: false },
            { sectionKey: 'templateContents', sectionName: 'Template Contents', isExpanded: false },
            { sectionKey: 'mJUserNotificationTypes', sectionName: 'MJ: User Notification Types', isExpanded: false },
            { sectionKey: 'mJUserNotificationTypes1', sectionName: 'MJ: User Notification Types', isExpanded: false },
