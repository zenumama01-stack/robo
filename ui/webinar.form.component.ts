import { WebinarEntity } from 'mj_generatedentities';
@RegisterClass(BaseFormComponent, 'Webinars') // Tell MemberJunction about this class
    selector: 'gen-webinar-form',
    templateUrl: './webinar.form.component.html'
export class WebinarFormComponent extends BaseFormComponent {
    public record!: WebinarEntity;
            { sectionKey: 'streamingAccess', sectionName: 'Streaming & Access', isExpanded: true },
            { sectionKey: 'scheduleCapacity', sectionName: 'Schedule & Capacity', isExpanded: true },
            { sectionKey: 'webinarOverview', sectionName: 'Webinar Overview', isExpanded: false },
