import { MeetingEntity } from 'mj_generatedentities';
@RegisterClass(BaseFormComponent, 'Meetings') // Tell MemberJunction about this class
    selector: 'gen-meeting-form',
    templateUrl: './meeting.form.component.html'
export class MeetingFormComponent extends BaseFormComponent {
    public record!: MeetingEntity;
            { sectionKey: 'meetingScheduleLogistics', sectionName: 'Meeting Schedule & Logistics', isExpanded: true },
            { sectionKey: 'productDetails', sectionName: 'Product Details', isExpanded: true },
            { sectionKey: 'webinars', sectionName: 'Webinars', isExpanded: false }
