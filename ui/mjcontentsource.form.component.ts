import { MJContentSourceEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Content Sources') // Tell MemberJunction about this class
    selector: 'gen-mjcontentsource-form',
    templateUrl: './mjcontentsource.form.component.html'
export class MJContentSourceFormComponent extends BaseFormComponent {
    public record!: MJContentSourceEntity;
            { sectionKey: 'contentClassification', sectionName: 'Content Classification', isExpanded: true },
            { sectionKey: 'connectionDetails', sectionName: 'Connection Details', isExpanded: false },
            { sectionKey: 'contentItems', sectionName: 'Content Items', isExpanded: false },
            { sectionKey: 'contentProcessRuns', sectionName: 'Content Process Runs', isExpanded: false },
            { sectionKey: 'contentSourceParams', sectionName: 'Content Source Params', isExpanded: false }
