import { Metadata, EntityInfo } from '@memberjunction/core';
interface VersionGraphPreferences {
    SchemaFilter: string;
interface EntityNode {
    SchemaName: string;
    ReferencedByCount: number;
    DependsOnCount: number;
    IsSelected: boolean;
interface RelationshipEdge {
    FromEntity: string;
    ToEntity: string;
    RelatedEntityJoinField: string;
@RegisterClass(BaseResourceComponent, 'VersionHistoryGraphResource')
    selector: 'mj-version-history-graph-resource',
    templateUrl: './graph-resource.component.html',
    styleUrls: ['./graph-resource.component.css'],
export class VersionHistoryGraphResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    // Entity browser
    public AllEntities: EntityNode[] = [];
    public FilteredEntities: EntityNode[] = [];
    public SchemaFilter = '';
    public AvailableSchemas: string[] = [];
    // Selected entity detail
    public SelectedEntity: EntityNode | null = null;
    public SelectedEntityInfo: EntityInfo | null = null;
    public ReferencedByEntities: RelationshipEdge[] = [];
    public DependsOnEntities: RelationshipEdge[] = [];
    public TotalEntities = 0;
    public EntitiesWithDependents = 0;
    public TotalRelationships = 0;
    private static readonly PREFS_KEY = 'VersionHistory.Graph.UserPreferences';
        this.LoadData();
        return 'Dependency Graph';
    public LoadData(): void {
            const entities = this.metadata.Entities;
            this.AllEntities = entities.map(e => ({
                ReferencedByCount: this.countReferencedBy(e),
                DependsOnCount: this.countDependsOn(e),
                IsSelected: false
            })).sort((a, b) => a.Name.localeCompare(b.Name));
            // Extract unique schemas, sorted
            const schemaSet = new Set(this.AllEntities.map(e => e.SchemaName));
            this.AvailableSchemas = Array.from(schemaSet).sort();
            this.TotalEntities = this.AllEntities.length;
            this.EntitiesWithDependents = this.AllEntities.filter(e => e.ReferencedByCount > 0).length;
            this.TotalRelationships = this.AllEntities.reduce((sum, e) => sum + e.ReferencedByCount, 0);
            console.error('Error loading dependency graph data:', error);
    /** Count entities that reference this entity (have FKs pointing to it), excluding self-references */
    private countReferencedBy(entity: EntityInfo): number {
        return entity.RelatedEntities.filter(r =>
            r.Type.trim().toUpperCase() === 'ONE TO MANY' &&
            r.RelatedEntity !== entity.Name
    /** Count entities this entity depends on (has FKs pointing to), excluding self-references */
    private countDependsOn(entity: EntityInfo): number {
        return this.metadata.Entities.filter(e =>
            e.Name !== entity.Name &&
            e.RelatedEntities.some(r =>
                r.RelatedEntity === entity.Name
    public OnSearchChange(text: string): void {
        this.SearchText = text;
    public OnSchemaFilterChange(schema: string): void {
        this.SchemaFilter = this.SchemaFilter === schema ? '' : schema;
            const raw = UserInfoEngine.Instance.GetSetting(VersionHistoryGraphResourceComponent.PREFS_KEY);
                const prefs: VersionGraphPreferences = JSON.parse(raw);
                if (prefs.SchemaFilter != null) {
                    this.SchemaFilter = prefs.SchemaFilter;
            console.error('Error loading graph preferences:', error);
            this.SchemaFilter = '';
        const prefs: VersionGraphPreferences = {
            SchemaFilter: this.SchemaFilter
        UserInfoEngine.Instance.SetSettingDebounced(VersionHistoryGraphResourceComponent.PREFS_KEY, JSON.stringify(prefs));
        let result = this.AllEntities;
        if (this.SchemaFilter) {
            result = result.filter(e => e.SchemaName === this.SchemaFilter);
        if (this.SearchText) {
            result = result.filter(e => e.Name.toLowerCase().includes(search));
        this.FilteredEntities = result;
    public SelectEntity(entityNode: EntityNode): void {
        if (this.SelectedEntity) {
            this.SelectedEntity.IsSelected = false;
        entityNode.IsSelected = true;
        this.SelectedEntity = entityNode;
        const entityInfo = this.metadata.Entities.find(e => e.ID === entityNode.ID);
        this.SelectedEntityInfo = entityInfo ?? null;
            this.ReferencedByEntities = this.buildReferencedByList(entityInfo);
            this.DependsOnEntities = this.buildDependsOnList(entityInfo);
            this.ReferencedByEntities = [];
            this.DependsOnEntities = [];
    /** Entities that have FKs pointing TO the selected entity (it is the "one" side) */
    private buildReferencedByList(entityInfo: EntityInfo): RelationshipEdge[] {
        return entityInfo.RelatedEntities
            .filter(r =>
                r.RelatedEntity !== entityInfo.Name
            .map(r => ({
                FromEntity: entityInfo.Name,
                ToEntity: r.RelatedEntity,
                RelatedEntityJoinField: r.RelatedEntityJoinField,
                Type: r.Type
            .sort((a, b) => a.ToEntity.localeCompare(b.ToEntity));
    /** Entities the selected entity has FKs pointing to (it is the "many" side) */
    private buildDependsOnList(entityInfo: EntityInfo): RelationshipEdge[] {
        return this.metadata.Entities
            .filter(e =>
                e.Name !== entityInfo.Name &&
                    r.RelatedEntity === entityInfo.Name
            .map(e => {
                const rel = e.RelatedEntities.find(r =>
                )!;
                    FromEntity: e.Name,
                    ToEntity: entityInfo.Name,
                    Type: rel.Type
            .sort((a, b) => a.FromEntity.localeCompare(b.FromEntity));
    public NavigateToEntity(entityName: string): void {
        const node = this.AllEntities.find(e => e.Name === entityName);
            // Clear schema filter so the entity is visible
            if (this.SchemaFilter && node.SchemaName !== this.SchemaFilter) {
            this.SelectEntity(node);
    public GetDependencyLevel(count: number): string {
        if (count === 0) return 'level-none';
        if (count <= 3) return 'level-low';
        if (count <= 10) return 'level-medium';
        return 'level-high';
    public GetSchemaEntityCount(schema: string): number {
        return this.AllEntities.filter(e => e.SchemaName === schema).length;
