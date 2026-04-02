import { MJCompanyIntegrationRunAPILogEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Company Integration Run API Logs') // Tell MemberJunction about this class
    selector: 'gen-mjcompanyintegrationrunapilog-form',
    templateUrl: './mjcompanyintegrationrunapilog.form.component.html'
export class MJCompanyIntegrationRunAPILogFormComponent extends BaseFormComponent {
    public record!: MJCompanyIntegrationRunAPILogEntity;
            { sectionKey: 'executionTimingStatus', sectionName: 'Execution Timing & Status', isExpanded: true },
            { sectionKey: 'aPICallDetails', sectionName: 'API Call Details', isExpanded: true },
