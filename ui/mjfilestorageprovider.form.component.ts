import { MJFileStorageProviderEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: File Storage Providers') // Tell MemberJunction about this class
    selector: 'gen-mjfilestorageprovider-form',
    templateUrl: './mjfilestorageprovider.form.component.html'
export class MJFileStorageProviderFormComponent extends BaseFormComponent {
    public record!: MJFileStorageProviderEntity;
            { sectionKey: 'providerIdentification', sectionName: 'Provider Identification', isExpanded: true },
            { sectionKey: 'driverConfiguration', sectionName: 'Driver Configuration', isExpanded: true },
            { sectionKey: 'selectionAvailability', sectionName: 'Selection & Availability', isExpanded: false },
            { sectionKey: 'authenticationAccess', sectionName: 'Authentication & Access', isExpanded: false },
            { sectionKey: 'files', sectionName: 'Files', isExpanded: false },
