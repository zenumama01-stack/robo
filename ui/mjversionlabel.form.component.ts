import { MJVersionLabelEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Version Labels') // Tell MemberJunction about this class
    selector: 'gen-mjversionlabel-form',
    templateUrl: './mjversionlabel.form.component.html'
export class MJVersionLabelFormComponent extends BaseFormComponent {
    public record!: MJVersionLabelEntity;
            { sectionKey: 'labelDefinition', sectionName: 'Label Definition', isExpanded: true },
            { sectionKey: 'scopeTargets', sectionName: 'Scope Targets', isExpanded: true },
            { sectionKey: 'creationMetrics', sectionName: 'Creation Metrics', isExpanded: false },
            { sectionKey: 'mJVersionLabelRestores1', sectionName: 'MJ: Version Label Restores', isExpanded: false },
            { sectionKey: 'mJVersionLabels', sectionName: 'MJ: Version Labels', isExpanded: false }
