import { MJAIResultCacheEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Result Cache') // Tell MemberJunction about this class
    selector: 'gen-mjairesultcache-form',
    templateUrl: './mjairesultcache.form.component.html'
export class MJAIResultCacheFormComponent extends BaseFormComponent {
    public record!: MJAIResultCacheEntity;
            { sectionKey: 'executionDetails', sectionName: 'Execution Details', isExpanded: true },
            { sectionKey: 'resultInformation', sectionName: 'Result Information', isExpanded: false },
            { sectionKey: 'stakeholderLinks', sectionName: 'Stakeholder Links', isExpanded: false },
