import { MJEnvironmentEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Environments') // Tell MemberJunction about this class
    selector: 'gen-mjenvironment-form',
    templateUrl: './mjenvironment.form.component.html'
export class MJEnvironmentFormComponent extends BaseFormComponent {
    public record!: MJEnvironmentEntity;
            { sectionKey: 'environmentDefinition', sectionName: 'Environment Definition', isExpanded: true },
            { sectionKey: 'environmentSettings', sectionName: 'Environment Settings', isExpanded: false },
            { sectionKey: 'mJCollections', sectionName: 'MJ: Collections', isExpanded: false },
            { sectionKey: 'mJProjects', sectionName: 'MJ: Projects', isExpanded: false },
