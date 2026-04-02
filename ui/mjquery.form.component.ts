import { MJQueryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Queries') // Tell MemberJunction about this class
    selector: 'gen-mjquery-form',
    templateUrl: './mjquery.form.component.html'
export class MJQueryFormComponent extends BaseFormComponent {
            { sectionKey: 'queryDefinition', sectionName: 'Query Definition', isExpanded: true },
            { sectionKey: 'performanceQuality', sectionName: 'Performance & Quality', isExpanded: true },
            { sectionKey: 'cachingExecutionSettings', sectionName: 'Caching & Execution Settings', isExpanded: false },
            { sectionKey: 'aIEmbeddings', sectionName: 'AI & Embeddings', isExpanded: false },
            { sectionKey: 'queryPermissions', sectionName: 'Query Permissions', isExpanded: false },
            { sectionKey: 'mJQueryParameters', sectionName: 'MJ: Query Parameters', isExpanded: false },
            { sectionKey: 'queryEntities', sectionName: 'Query Entities', isExpanded: false }
