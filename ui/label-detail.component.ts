import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ChangeDetectorRef, ChangeDetectionStrategy, HostListener, ElementRef, NgZone } from '@angular/core';
import { RunView, Metadata, EntityInfo, CompositeKey, UserInfo, EntityRecordNameInput } from '@memberjunction/core';
import { MJVersionLabelEntityType, MJVersionLabelItemEntityType, MJVersionLabelRestoreEntityType, MJVersionLabelEntity } from '@memberjunction/core-entities';
import { MicroViewData, FieldChangeView } from '../types';
import { EntityLinkClickEvent } from '../record-micro-view/record-micro-view.component';
type DetailTab = 'overview' | 'snapshots' | 'dependencies' | 'changes' | 'history';
interface SnapshotEntityGroup {
    Items: SnapshotItemView[];
interface SnapshotItemView {
    FieldPreview: string;
interface ClientDiffResult {
    Summary: { Changed: number; Unchanged: number; EntitiesAffected: number };
    EntityGroups: ClientDiffEntityGroup[];
interface ClientDiffEntityGroup {
    Records: ClientRecordDiff[];
interface ClientRecordDiff {
    ChangeType: 'Modified' | 'Unchanged';
interface DependencyEntityView {
    RelationshipField: string;
    Children: DependencyEntityView[];
    ChangedAt: string;
    selector: 'mj-label-detail-panel',
    templateUrl: './label-detail.component.html',
    styleUrls: ['./label-detail.component.css'],
export class MjLabelDetailComponent implements OnInit, OnDestroy {
    @Input() Label!: MJVersionLabelEntityType;
    @Input() AllLabels: MJVersionLabelEntityType[] = [];
    @Input() ItemCountMap = new Map<string, number>();
    @Output() LabelUpdated = new EventEmitter<void>();
    @Output() EntityLinkClick = new EventEmitter<EntityLinkClickEvent>();
    public ActiveTab: DetailTab = 'overview';
    public LabelItems: MJVersionLabelItemEntityType[] = [];
    public ChildLabels: MJVersionLabelEntityType[] = [];
    public IsLoadingItems = true;
    public IsArchiving = false;
    // Overview stats
    public UniqueEntityCount = 0;
    public CreatorName = '';
    /** Record display name resolved via IsNameField (shown on overview when Scope=Record) */
    public OverviewRecordName = '';
    /** Entity icon for the overview tab */
    public OverviewEntityIcon = 'fa-solid fa-table';
    // Snapshot tab
    public SnapshotGroups: SnapshotEntityGroup[] = [];
    public SnapshotViewMode: 'card' | 'list' = 'list';
    public SnapshotSearch = '';
    public SnapshotSortBy: 'name' | 'count' = 'count';
    public SnapshotSortDir: 'asc' | 'desc' = 'desc';
    public FilteredSnapshotGroups: SnapshotEntityGroup[] = [];
    // Changes tab (lazy)
    public DiffResult: ClientDiffResult | null = null;
    public IsLoadingDiff = false;
    private diffLoaded = false;
    // Dependencies tab (lazy)
    public DependencyTree: DependencyEntityView[] = [];
    public IsLoadingDependencies = false;
    private dependenciesLoaded = false;
    // History tab (lazy)
    public RelatedLabels: MJVersionLabelEntityType[] = [];
    public IsLoadingHistory = false;
    private historyLoaded = false;
    // Micro view (inline)
    public MicroViewRecord: MicroViewData | null = null;
    public ShowMicroView = false;
    public BreadcrumbLabel = '';
    public PanelWidthPx = 0;
    private resizeMinWidth = 400;
    private resizeMaxWidthRatio = 0.92;
    private static readonly PREFS_KEY = 'VersionHistory.DetailPanel.UserPreferences';
    // Bound handlers for resize (need references for removeEventListener)
    constructor(private cdr: ChangeDetectorRef, private ngZone: NgZone, private elRef: ElementRef) {}
        this.loadPanelPreferences();
        this.computeChildLabels();
        this.loadLabelItems();
        // Clean up any in-progress resize
        if (this.ShowMicroView) {
            this.OnBackFromMicroView();
    // Panel actions
        // Wait for CSS transition to complete
        setTimeout(() => this.Close.emit(), 300);
    public OnBackdropClick(): void {
        if (!this.ShowMicroView) {
    public OnTabChange(tab: DetailTab): void {
        this.loadTabData(tab);
    // Label actions
    public async OnArchive(): Promise<void> {
        if (this.IsArchiving || this.Label.Status === 'Archived') return;
        this.IsArchiving = true;
            const label = await md.GetEntityObject<MJVersionLabelEntity>('MJ: Version Labels');
            await label.InnerLoad(new CompositeKey([{ FieldName: 'ID', Value: this.Label.ID }]));
            label.Status = 'Archived';
            const saved = await label.Save();
                this.Label.Status = 'Archived';
                this.LabelUpdated.emit();
            console.error('Error archiving label:', e);
                this.IsArchiving = false;
    private computeChildLabels(): void {
        this.ChildLabels = this.AllLabels.filter(l => l.ParentID === this.Label.ID);
    private async loadLabelItems(): Promise<void> {
        this.IsLoadingItems = true;
                ExtraFilter: `VersionLabelID = '${this.Label.ID}'`,
                this.LabelItems = result.Results;
                this.computeOverviewStats();
                this.buildSnapshotGroups();
            console.error('Error loading label items:', e);
                this.IsLoadingItems = false;
    private computeOverviewStats(): void {
        const entityIds = new Set(this.LabelItems.map(i => i.EntityID ?? '').filter(Boolean));
        this.UniqueEntityCount = entityIds.size;
        this.CreatorName = this.Label.CreatedByUser ?? '';
        // Resolve entity icon for overview
        if (this.Label.EntityID) {
            this.OverviewEntityIcon = this.resolveEntityIcon(this.Label.EntityID);
        // Resolve record display name for Record-scoped labels
        if (this.Label.Scope === 'Record' && this.Label.RecordID && this.Label.EntityID) {
            this.loadOverviewRecordName();
     * For Record-scoped labels, load the record's display name via GetEntityRecordNames.
    private async loadOverviewRecordName(): Promise<void> {
        const entityName = this.resolveEntityName(this.Label.EntityID ?? '');
        if (!entityName || entityName === 'Unknown') return;
        const rawId = this.extractRawRecordId(this.Label.RecordID ?? '');
        if (!rawId) return;
            input.EntityName = entityName;
            const results = await this.metadata.GetEntityRecordNames([input]);
            const recordName = results.length > 0 && results[0].Success ? results[0].RecordName : undefined;
            if (recordName) {
                    this.OverviewRecordName = recordName;
            console.error('Error loading overview record name:', e);
    private buildSnapshotGroups(): void {
        const groupMap = new Map<string, { entityId: string; items: SnapshotItemView[] }>();
        for (const item of this.LabelItems) {
            const key = entityName || entityId;
            if (!groupMap.has(key)) {
                groupMap.set(key, { entityId, items: [] });
            const displayName = this.buildInitialDisplayName(item);
            groupMap.get(key)!.items.push({
                RecordChangeID: item.RecordChangeID ?? '',
                DisplayName: displayName,
                FieldPreview: '',
                FullRecordJSON: null
        this.SnapshotGroups = Array.from(groupMap.entries())
            .map(([name, group]) => ({
                EntityName: name,
                EntityID: group.entityId,
                EntityIcon: this.resolveEntityIcon(group.entityId),
                Items: group.items,
                IsLoadingNames: false,
                NamesLoaded: false
        this.applySortAndFilter();
     * Initial display name before lazy-loading (just the shortened raw ID).
     * Real names are loaded via loadGroupRecordNames() when the group is expanded.
    private buildInitialDisplayName(item: MJVersionLabelItemEntityType): string {
        const rawId = this.extractRawRecordId(item.RecordID ?? '');
        return rawId.length > 20 ? rawId.substring(0, 20) + '...' : rawId;
    /** Extract the raw ID value from a potentially formatted "ID|<uuid>" string. */
    /** Apply current sort + search filter to snapshot groups. */
    private applySortAndFilter(): void {
        const sorted = [...this.SnapshotGroups].sort((a, b) => {
            if (this.SnapshotSortBy === 'name') {
                return this.SnapshotSortDir === 'asc' ? cmp : -cmp;
            // sort by count
        if (!this.SnapshotSearch) {
            this.FilteredSnapshotGroups = sorted;
        const search = this.SnapshotSearch.toLowerCase();
        this.FilteredSnapshotGroups = sorted
    private loadTabData(tab: DetailTab): void {
        switch (tab) {
            case 'changes':
                if (!this.diffLoaded) {
                    this.loadDiffData();
            case 'dependencies':
                if (!this.dependenciesLoaded) {
                    this.loadDependencyData();
            case 'history':
                if (!this.historyLoaded) {
                    this.loadHistoryData();
    // Changes tab
    private async loadDiffData(): Promise<void> {
        this.IsLoadingDiff = true;
            const entityGroups: ClientDiffEntityGroup[] = [];
            let totalChanged = 0;
            let totalUnchanged = 0;
            // Group label items by entity
            const entityItemMap = new Map<string, MJVersionLabelItemEntityType[]>();
                if (!entityItemMap.has(entityId)) {
                    entityItemMap.set(entityId, []);
                entityItemMap.get(entityId)!.push(item);
            // For each entity group, load latest RecordChanges and compare
            for (const [entityId, items] of entityItemMap) {
                const records: ClientRecordDiff[] = [];
                    const latestChange = await this.loadLatestRecordChange(rv, entityId, item.RecordID ?? '');
                    const isModified = latestChange != null && latestChange !== item.RecordChangeID;
                    if (isModified) {
                        totalChanged++;
                        records.push({
                            ChangeType: 'Modified',
                            FieldChanges: await this.computeFieldChanges(rv, item.RecordChangeID ?? '', latestChange)
                        totalUnchanged++;
                            ChangeType: 'Unchanged',
                            FieldChanges: []
                if (records.some(r => r.ChangeType === 'Modified')) {
                    entityGroups.push({
                        Records: records.filter(r => r.ChangeType === 'Modified'),
                        IsExpanded: false
            this.DiffResult = {
                Summary: {
                    Changed: totalChanged,
                    Unchanged: totalUnchanged,
                    EntitiesAffected: entityGroups.length
                EntityGroups: entityGroups
            this.diffLoaded = true;
            console.error('Error loading diff data:', e);
                this.IsLoadingDiff = false;
    private async computeFieldChanges(rv: RunView, oldChangeId: string, newChangeId: string): Promise<FieldChangeView[]> {
                // Skip MJ system fields
    // Dependencies tab
    private loadDependencyData(): void {
        this.IsLoadingDependencies = true;
            const entityId = this.Label.EntityID;
            if (!entityId) {
                this.DependencyTree = [];
                this.dependenciesLoaded = true;
            const entityInfo = this.metadata.Entities.find(e => e.ID === entityId);
            // Build set of entity names that are actually in the snapshot
            const snapshotEntityNames = this.buildSnapshotEntityNameSet();
            this.DependencyTree = this.buildDependencyTree(entityInfo, 0, new Set<string>(), snapshotEntityNames);
            console.error('Error loading dependency data:', e);
            this.IsLoadingDependencies = false;
     * Build a set of entity names that have at least one item in the snapshot.
     * Used to filter the dependency tree to only show entities with captured records.
    private buildSnapshotEntityNameSet(): Set<string> {
            if (entityName && entityName !== 'Unknown') {
                names.add(entityName);
     * Build the dependency tree, but only include entities that are in the
     * actual snapshot (snapshotEntities). This prevents showing relationship
     * paths that weren't captured (e.g., Conversation Details when there are
     * no conversation records in the snapshot).
    private buildDependencyTree(
        snapshotEntities: Set<string>
    ): DependencyEntityView[] {
        if (depth > 3 || visited.has(entity.Name)) return [];
        visited.add(entity.Name);
        const deps: DependencyEntityView[] = [];
        // Get One-To-Many relationships (entities that depend on this one)
        const dependents = entity.RelatedEntities.filter(r =>
        for (const rel of dependents) {
            // Only include entities that are actually in the snapshot
            if (!snapshotEntities.has(rel.RelatedEntity)) continue;
            const childEntity = this.metadata.Entities.find(e => e.Name === rel.RelatedEntity);
            const children = childEntity
                ? this.buildDependencyTree(childEntity, depth + 1, new Set(visited), snapshotEntities)
            deps.push({
                RelationshipField: rel.RelatedEntityJoinField,
                Children: children,
                Depth: depth + 1,
                IsExpanded: depth < 1
        return deps.sort((a, b) => a.EntityName.localeCompare(b.EntityName));
    // History tab
    private async loadHistoryData(): Promise<void> {
        this.IsLoadingHistory = true;
            // Find related labels (same entity + record)
            this.RelatedLabels = this.AllLabels.filter(l =>
                l.ID !== this.Label.ID &&
                l.EntityID === this.Label.EntityID &&
                l.RecordID === this.Label.RecordID &&
                l.RecordID != null
            console.error('Error loading history data:', e);
                this.IsLoadingHistory = false;
    // Snapshot interactions
    public ToggleSnapshotGroup(group: SnapshotEntityGroup): void {
        if (group.IsExpanded && !group.NamesLoaded) {
     * Lazy-load record display names for a group using Metadata.GetEntityRecordNames.
     * Only called when a group is first expanded.
    private async loadGroupRecordNames(group: SnapshotEntityGroup): Promise<void> {
                    // Match result back to the item by composite key value
    public OnSnapshotSearchChange(text: string): void {
        this.SnapshotSearch = text;
    public OnSnapshotSortChange(sortBy: 'name' | 'count'): void {
        if (this.SnapshotSortBy === sortBy) {
            // Toggle direction if clicking same sort
            this.SnapshotSortDir = this.SnapshotSortDir === 'asc' ? 'desc' : 'asc';
            this.SnapshotSortBy = sortBy;
            this.SnapshotSortDir = sortBy === 'name' ? 'asc' : 'desc';
    public ToggleSnapshotViewMode(): void {
        this.SnapshotViewMode = this.SnapshotViewMode === 'card' ? 'list' : 'card';
    // Diff interactions
    public ToggleDiffGroup(group: ClientDiffEntityGroup): void {
    public ExpandAllDiffGroups(): void {
        if (!this.DiffResult) return;
        for (const group of this.DiffResult.EntityGroups) {
    public CollapseAllDiffGroups(): void {
    // Dependency interactions
    public ToggleDependencyNode(node: DependencyEntityView): void {
        node.IsExpanded = !node.IsExpanded;
    // Micro view (inline navigation)
    public OpenMicroView(entityName: string, recordId: string, recordChangeId: string, displayName?: string): void {
        const label = displayName || this.extractRawRecordId(recordId).substring(0, 12) + '...';
        this.BreadcrumbLabel = `${entityName} / ${label}`;
        this.MicroViewRecord = {
            EntityID: '',
            RecordID: recordId,
            RecordChangeID: recordChangeId,
        this.ShowMicroView = true;
    public OnBackFromMicroView(): void {
        this.ShowMicroView = false;
        this.MicroViewRecord = null;
    public OnOpenRecordClick(event: EntityLinkClickEvent): void {
    /** Open the record referenced by a Record-scoped label via navigation. */
    public OnOpenOverviewRecord(): void {
        if (!this.Label.EntityID || !this.Label.RecordID) return;
        const entityName = this.resolveEntityName(this.Label.EntityID);
        const rawId = this.extractRawRecordId(this.Label.RecordID);
            RecordID: rawId,
            CompositeKey: pkey
        // Run outside Angular zone for performance during drag
        const maxWidth = viewportWidth * this.resizeMaxWidthRatio;
        const newWidth = Math.max(this.resizeMinWidth, Math.min(maxWidth, viewportWidth - event.clientX));
        this.PanelWidthPx = newWidth;
        this.savePanelPreferences();
    // Panel preferences
    private loadPanelPreferences(): void {
        // Default to 65% of viewport
        this.PanelWidthPx = Math.max(this.resizeMinWidth, Math.min(window.innerWidth * 0.65, 1000));
            const raw = UserInfoEngine.Instance.GetSetting(MjLabelDetailComponent.PREFS_KEY);
                const prefs = JSON.parse(raw) as { PanelWidthPx?: number };
                if (prefs.PanelWidthPx && prefs.PanelWidthPx >= this.resizeMinWidth) {
                    this.PanelWidthPx = Math.min(prefs.PanelWidthPx, window.innerWidth * this.resizeMaxWidthRatio);
            // Use defaults
    private savePanelPreferences(): void {
            const prefs = JSON.stringify({ PanelWidthPx: Math.round(this.PanelWidthPx) });
            UserInfoEngine.Instance.SetSettingDebounced(MjLabelDetailComponent.PREFS_KEY, prefs);
            // Ignore save errors
    public resolveEntityName(entityId: string | null | undefined): string {
    /** Resolve icon CSS class for an entity by ID, falling back to generic table icon. */
    public resolveEntityIcon(entityId: string): string {
    /** Resolve icon CSS class for an entity by name, falling back to generic table icon. */
    public resolveEntityIconByName(entityName: string): string {
        if (!entityName) return 'fa-solid fa-table';
        const entity = this.metadata.Entities.find(e => e.Name === entityName);
    /** Format a record ID for display — strips 'ID|' prefix for single-value PKs. */
    public FormatRecordID(recordId: string | undefined): string {
        return this.extractRawRecordId(recordId ?? '');
            'Removed': 'change-removed'
            'Added': 'fa-solid fa-plus-circle',
            'Modified': 'fa-solid fa-pen-to-square',
            'Removed': 'fa-solid fa-minus-circle'
    public FormatRelativeDate(date: Date | string | undefined): string {
