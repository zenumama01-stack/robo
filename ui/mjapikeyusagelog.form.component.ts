@RegisterClass(BaseFormComponent, 'MJ: API Key Usage Logs') // Tell MemberJunction about this class
    selector: 'gen-mjapikeyusagelog-form',
    templateUrl: './mjapikeyusagelog.form.component.html'
export class MJAPIKeyUsageLogFormComponent extends BaseFormComponent {
    public record!: MJAPIKeyUsageLogEntity;
            { sectionKey: 'requestInformation', sectionName: 'Request Information', isExpanded: true },
            { sectionKey: 'responseClientInfo', sectionName: 'Response & Client Info', isExpanded: true },
            { sectionKey: 'authorizationDetails', sectionName: 'Authorization Details', isExpanded: false },
