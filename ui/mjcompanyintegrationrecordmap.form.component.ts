import { MJCompanyIntegrationRecordMapEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Company Integration Record Maps') // Tell MemberJunction about this class
    selector: 'gen-mjcompanyintegrationrecordmap-form',
    templateUrl: './mjcompanyintegrationrecordmap.form.component.html'
export class MJCompanyIntegrationRecordMapFormComponent extends BaseFormComponent {
    public record!: MJCompanyIntegrationRecordMapEntity;
            { sectionKey: 'integrationKeys', sectionName: 'Integration Keys', isExpanded: true },
            { sectionKey: 'mappingDetails', sectionName: 'Mapping Details', isExpanded: true },
