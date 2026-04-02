import { MJVersionInstallationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Version Installations') // Tell MemberJunction about this class
    selector: 'gen-mjversioninstallation-form',
    templateUrl: './mjversioninstallation.form.component.html'
export class MJVersionInstallationFormComponent extends BaseFormComponent {
    public record!: MJVersionInstallationEntity;
            { sectionKey: 'installationExecution', sectionName: 'Installation Execution', isExpanded: true },
            { sectionKey: 'installationDocumentation', sectionName: 'Installation Documentation', isExpanded: false },
