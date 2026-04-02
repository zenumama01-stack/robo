import { RunView, Metadata, CompositeKey, EntityRecordNameInput } from '@memberjunction/core';
import { ResourceData, UserInfoEngine, MJVersionLabelEntityType, MJVersionLabelItemEntityType } from '@memberjunction/core-entities';
interface VersionDiffPreferences {
    DiffMode: 'label-to-label' | 'label-to-current';
interface DiffItemView {
    EntityID: string;
    DisplayName: string;
    ChangeType: 'Added' | 'Removed' | 'Modified' | 'Unchanged';
    FieldChanges: FieldChangeView[];
    IsExpanded: boolean;
    IsLoadingFields: boolean;
    FieldsLoaded: boolean;
    /** RecordChangeID from the "from" label (for loading FullRecordJSON). */
    FromRecordChangeID: string;
    /** RecordChangeID from the "to" label (for loading FullRecordJSON). */
    ToRecordChangeID: string;
interface FieldChangeView {
    FieldName: string;
    OldValue: string;
    NewValue: string;
    ChangeType: 'Added' | 'Modified' | 'Removed';
interface RecordChangeRow {
    FullRecordJSON: string;
interface EntityGroupView {
    EntityIcon: string;
    Items: DiffItemView[];
    AddedCount: number;
    RemovedCount: number;
    ModifiedCount: number;
    NamesLoaded: boolean;
    IsLoadingNames: boolean;
@RegisterClass(BaseResourceComponent, 'VersionHistoryDiffResource')
    selector: 'mj-version-history-diff-resource',
    templateUrl: './diff-resource.component.html',
    styleUrls: ['./diff-resource.component.css'],
export class VersionHistoryDiffResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    public IsDiffLoading = false;
    public HasDiffResult = false;
    // Label selection
    public AvailableLabels: MJVersionLabelEntityType[] = [];
    public FromLabelId = '';
    public ToLabelId = '';
    public DiffMode: 'label-to-label' | 'label-to-current' = 'label-to-current';
    // Diff results
    public EntityGroups: EntityGroupView[] = [];
    public FilteredEntityGroups: EntityGroupView[] = [];
    public TotalAdded = 0;
    public TotalRemoved = 0;
    public TotalModified = 0;
    public TotalUnchanged = 0;
    // Diff result filtering/sorting
    public DiffSortBy: 'name' | 'count' = 'count';
    public DiffSortDir: 'asc' | 'desc' = 'desc';
    public DiffFilterType: 'all' | 'added' | 'removed' | 'modified' = 'all';
    public DiffSearch = '';
    public ExpandedEntities = new Set<string>();
    private static readonly PREFS_KEY = 'VersionHistory.Diff.UserPreferences';
    constructor(private cdr: ChangeDetectorRef, private navigationService: NavigationService) {
        this.LoadLabels();
        return 'Diff Viewer';
        return 'fa-solid fa-code-compare';
    public async LoadLabels(): Promise<void> {
            const result = await rv.RunView<MJVersionLabelEntityType>({
                EntityName: 'MJ: Version Labels',
                this.AvailableLabels = result.Results;
            console.error('Error loading labels for diff:', error);
    public OnDiffModeChange(mode: 'label-to-label' | 'label-to-current'): void {
        this.DiffMode = mode;
        this.HasDiffResult = false;
        this.EntityGroups = [];
            const raw = UserInfoEngine.Instance.GetSetting(VersionHistoryDiffResourceComponent.PREFS_KEY);
                const prefs: VersionDiffPreferences = JSON.parse(raw);
                if (prefs.DiffMode === 'label-to-label' || prefs.DiffMode === 'label-to-current') {
                    this.DiffMode = prefs.DiffMode;
            console.error('Error loading diff preferences:', error);
            this.DiffMode = 'label-to-current';
        const prefs: VersionDiffPreferences = {
            DiffMode: this.DiffMode
        UserInfoEngine.Instance.SetSettingDebounced(VersionHistoryDiffResourceComponent.PREFS_KEY, JSON.stringify(prefs));
    public async RunDiff(): Promise<void> {
        if (!this.FromLabelId) return;
        if (this.DiffMode === 'label-to-label' && !this.ToLabelId) return;
            this.IsDiffLoading = true;
            // Load label items for comparison
            const fromItems = await this.loadLabelItems(rv, this.FromLabelId);
            if (this.DiffMode === 'label-to-label') {
                const toItems = await this.loadLabelItems(rv, this.ToLabelId);
                this.computeDiff(fromItems, toItems);
                // Label-to-current: compare label state against live data
                await this.computeDiffToCurrentState(rv, fromItems);
            this.HasDiffResult = true;
            console.error('Error running diff:', error);
            this.IsDiffLoading = false;
    private async loadLabelItems(
        rv: RunView,
        labelId: string
    ): Promise<MJVersionLabelItemEntityType[]> {
        const result = await rv.RunView<MJVersionLabelItemEntityType>({
            EntityName: 'MJ: Version Label Items',
            ExtraFilter: `VersionLabelID = '${labelId}'`,
    private computeDiff(
        fromItems: MJVersionLabelItemEntityType[],
        toItems: MJVersionLabelItemEntityType[]
        const entityMap = new Map<string, DiffItemView[]>();
        // Build lookup maps keyed by EntityID + RecordID
        const fromMap = this.buildItemKeyMap(fromItems);
        const toMap = this.buildItemKeyMap(toItems);
        // Items in 'to' but not in 'from' = Added
        for (const [key, item] of toMap) {
            const entityName = this.resolveEntityName(item.EntityID ?? '');
            if (!entityMap.has(entityName)) entityMap.set(entityName, []);
            if (!fromMap.has(key)) {
                entityMap.get(entityName)!.push({
                    EntityID: item.EntityID ?? '',
                    RecordID: item.RecordID ?? '',
                    DisplayName: '',
                    ChangeType: 'Added',
                    FieldChanges: [],
                    IsExpanded: false,
                    IsLoadingFields: false,
                    FieldsLoaded: false,
                    FromRecordChangeID: '',
                    ToRecordChangeID: item.RecordChangeID ?? ''
        // Items in 'from' but not in 'to' = Removed
        for (const [key, item] of fromMap) {
            if (!toMap.has(key)) {
                    ChangeType: 'Removed',
                    FromRecordChangeID: item.RecordChangeID ?? '',
                    ToRecordChangeID: ''
                // Both exist - check if RecordChangeID differs (= Modified)
                const toItem = toMap.get(key)!;
                const changeType = item.RecordChangeID !== toItem.RecordChangeID
                    ? 'Modified' : 'Unchanged';
                    ChangeType: changeType,
                    ToRecordChangeID: toItem.RecordChangeID ?? ''
        this.buildEntityGroups(entityMap);
    private async computeDiffToCurrentState(
        labelItems: MJVersionLabelItemEntityType[]
        // For label-to-current, compare snapshot RecordChangeIDs against
        // the latest RecordChange for each entity/record combination.
        for (const item of labelItems) {
            const entityId = item.EntityID ?? '';
            const entityName = this.resolveEntityName(entityId);
            // Load the latest RecordChange for this record
            const latestChangeId = await this.loadLatestRecordChange(rv, entityId, item.RecordID ?? '');
            const isModified = latestChangeId != null && latestChangeId !== item.RecordChangeID;
                EntityID: entityId,
                ChangeType: isModified ? 'Modified' : 'Unchanged',
                ToRecordChangeID: latestChangeId ?? ''
    private buildItemKeyMap(
        items: MJVersionLabelItemEntityType[]
    ): Map<string, MJVersionLabelItemEntityType> {
        const map = new Map<string, MJVersionLabelItemEntityType>();
            const key = `${item.EntityID ?? ''}|${item.RecordID ?? ''}`;
            map.set(key, item);
    private buildEntityGroups(entityMap: Map<string, DiffItemView[]>): void {
        this.TotalAdded = 0;
        this.TotalRemoved = 0;
        this.TotalModified = 0;
        this.TotalUnchanged = 0;
        this.EntityGroups = Array.from(entityMap.entries()).map(([entityName, items]) => {
            const added = items.filter(i => i.ChangeType === 'Added').length;
            const removed = items.filter(i => i.ChangeType === 'Removed').length;
            const modified = items.filter(i => i.ChangeType === 'Modified').length;
            this.TotalAdded += added;
            this.TotalRemoved += removed;
            this.TotalModified += modified;
            this.TotalUnchanged += items.filter(i => i.ChangeType === 'Unchanged').length;
            // Resolve entity icon from first item's EntityID
            const firstItem = items[0];
            const entityIcon = this.resolveEntityIcon(firstItem?.EntityID ?? '');
                EntityIcon: entityIcon,
                Items: items.filter(i => i.ChangeType !== 'Unchanged'),
                AddedCount: added,
                RemovedCount: removed,
                ModifiedCount: modified,
                NamesLoaded: false,
                IsLoadingNames: false
        }).filter(g => g.Items.length > 0)
          .sort((a, b) => b.Items.length - a.Items.length);
        this.ExpandedEntities.clear();
        this.applySortAndFilterDiff();
    private resolveEntityName(entityId: string): string {
        if (!entityId) return 'Unknown';
        return entity ? entity.Name : 'Unknown';
    private resolveEntityIcon(entityId: string): string {
        if (!entityId) return 'fa-solid fa-table';
        return entity?.Icon || 'fa-solid fa-table';
    // Expand / Collapse
    public ToggleEntityGroup(group: EntityGroupView): void {
        group.IsExpanded = !group.IsExpanded;
        if (group.IsExpanded) {
            this.ExpandedEntities.add(group.EntityName);
            // Auto-expand all Modified items so field details show immediately
            for (const item of group.Items) {
                if (item.ChangeType === 'Modified') {
                    item.IsExpanded = true;
            // Auto-load field changes for modified items when group opens
            this.loadFieldChangesForGroup(group);
            // Load record display names
            if (!group.NamesLoaded && !group.IsLoadingNames) {
                this.loadGroupRecordNames(group);
            this.ExpandedEntities.delete(group.EntityName);
    public ToggleItem(item: DiffItemView): void {
        if (item.ChangeType !== 'Modified') return;
        item.IsExpanded = !item.IsExpanded;
        if (item.IsExpanded && !item.FieldsLoaded) {
            this.loadFieldChangesForItem(item);
    public ExpandAllGroups(): void {
        for (const group of this.FilteredEntityGroups) {
            group.IsExpanded = true;
    public CollapseAllGroups(): void {
            group.IsExpanded = false;
                item.IsExpanded = false;
    public IsEntityExpanded(entityName: string): boolean {
        return this.ExpandedEntities.has(entityName);
    /** Open a record in the explorer via NavigationService. */
    public OnOpenRecord(item: DiffItemView): void {
        const rawId = this.extractRawRecordId(item.RecordID);
        const pkey = new CompositeKey([{ FieldName: 'ID', Value: rawId }]);
        this.navigationService.OpenEntityRecord(item.EntityName, pkey);
    // Record name resolution
    /** Lazy-load record display names for a group using Metadata.GetEntityRecordNames. */
    private async loadGroupRecordNames(group: EntityGroupView): Promise<void> {
        group.IsLoadingNames = true;
            const inputs: EntityRecordNameInput[] = group.Items.map(item => {
                const input = new EntityRecordNameInput();
                input.EntityName = group.EntityName;
                input.CompositeKey = new CompositeKey([{ FieldName: 'ID', Value: rawId }]);
            const results = await this.metadata.GetEntityRecordNames(inputs);
                if (result.Success && result.RecordName) {
                    const resultId = result.CompositeKey?.KeyValuePairs?.[0]?.Value;
                    if (resultId) {
                        const item = group.Items.find(i =>
                            this.extractRawRecordId(i.RecordID) === String(resultId)
                            item.DisplayName = result.RecordName;
            console.error('Error loading record names for group:', group.EntityName, e);
            group.IsLoadingNames = false;
            group.NamesLoaded = true;
    /** Extract the raw UUID from a RecordID that may be in "ID|<uuid>" format. */
    private extractRawRecordId(recordId: string): string {
        if (!recordId) return '';
        const parts = recordId.split('||');
        if (parts.length === 1) {
            const singleParts = recordId.split('|');
            if (singleParts.length === 2) {
                return singleParts[1];
        return recordId;
    // Field-level diff loading
    /** Auto-load field changes for all modified items in a group (when first expanded). */
    private loadFieldChangesForGroup(group: EntityGroupView): void {
            if (item.ChangeType === 'Modified' && !item.FieldsLoaded && !item.IsLoadingFields) {
    /** Lazy-load field-level changes for a single modified item. */
    private async loadFieldChangesForItem(item: DiffItemView): Promise<void> {
        if (!item.FromRecordChangeID || !item.ToRecordChangeID) {
            item.FieldsLoaded = true;
        item.IsLoadingFields = true;
            item.FieldChanges = await this.computeFieldChanges(item.FromRecordChangeID, item.ToRecordChangeID);
            console.error('Error loading field changes:', e);
            item.IsLoadingFields = false;
    private async loadLatestRecordChange(rv: RunView, entityId: string, recordId: string): Promise<string | null> {
        if (!entityId || !recordId) return null;
        const result = await rv.RunView<RecordChangeRow>({
            EntityName: 'MJ: Record Changes',
            ExtraFilter: `EntityID = '${entityId}' AND RecordID = '${recordId}'`,
            OrderBy: 'ChangedAt DESC',
    private async computeFieldChanges(oldChangeId: string, newChangeId: string): Promise<FieldChangeView[]> {
        if (!oldChangeId || !newChangeId) return [];
            const [oldResult, newResult] = await rv.RunViews([
                    ExtraFilter: `ID = '${oldChangeId}'`,
                    Fields: ['FullRecordJSON'],
                    ExtraFilter: `ID = '${newChangeId}'`,
            if (!oldResult.Success || !newResult.Success) return [];
            if (oldResult.Results.length === 0 || newResult.Results.length === 0) return [];
            const oldRow = oldResult.Results[0] as RecordChangeRow;
            const newRow = newResult.Results[0] as RecordChangeRow;
            return this.diffRecordJson(oldRow.FullRecordJSON, newRow.FullRecordJSON);
    private diffRecordJson(oldJson: string, newJson: string): FieldChangeView[] {
        const changes: FieldChangeView[] = [];
            const oldData = JSON.parse(oldJson || '{}') as Record<string, unknown>;
            const newData = JSON.parse(newJson || '{}') as Record<string, unknown>;
            const allKeys = new Set([...Object.keys(oldData), ...Object.keys(newData)]);
            for (const key of allKeys) {
                if (key.startsWith('__mj_')) continue;
                const oldVal = oldData[key];
                const newVal = newData[key];
                if (oldVal === undefined && newVal !== undefined) {
                    changes.push({ FieldName: key, OldValue: '', NewValue: this.formatFieldValue(newVal), ChangeType: 'Added' });
                } else if (oldVal !== undefined && newVal === undefined) {
                    changes.push({ FieldName: key, OldValue: this.formatFieldValue(oldVal), NewValue: '', ChangeType: 'Removed' });
                } else if (JSON.stringify(oldVal) !== JSON.stringify(newVal)) {
                    changes.push({ FieldName: key, OldValue: this.formatFieldValue(oldVal), NewValue: this.formatFieldValue(newVal), ChangeType: 'Modified' });
            // Invalid JSON - skip
        return changes;
    private formatFieldValue(value: unknown): string {
        if (value == null) return 'null';
    // Display helpers
    public GetChangeTypeClass(changeType: string): string {
        const classes: Record<string, string> = {
            'Added': 'change-added',
            'Removed': 'change-removed',
            'Modified': 'change-modified',
            'Unchanged': 'change-unchanged'
        return classes[changeType] ?? '';
    public GetChangeTypeIcon(changeType: string): string {
            'Added': 'fa-solid fa-plus',
            'Removed': 'fa-solid fa-minus',
            'Modified': 'fa-solid fa-pen',
            'Unchanged': 'fa-solid fa-equals'
        return icons[changeType] ?? 'fa-solid fa-circle';
    public FormatRecordID(recordId: string): string {
        return this.extractRawRecordId(recordId);
    public FormatLabelOption(label: MJVersionLabelEntityType): string {
        const dateVal = label.__mj_CreatedAt;
        const date = dateVal instanceof Date
            ? dateVal.toLocaleDateString('en-US', { month: 'short', day: 'numeric' })
            : new Date(dateVal as unknown as string).toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
        return `${label.Name} (${label.Scope}, ${date})`;
    public get CanRunDiff(): boolean {
        if (!this.FromLabelId) return false;
        if (this.DiffMode === 'label-to-label' && !this.ToLabelId) return false;
    public get TotalChanges(): number {
        return this.TotalAdded + this.TotalRemoved + this.TotalModified;
    // Label selection helpers
    /** From dropdown excludes the currently selected To label. */
    public get FilteredFromLabels(): MJVersionLabelEntityType[] {
        if (!this.ToLabelId) return this.AvailableLabels;
        return this.AvailableLabels.filter(l => l.ID !== this.ToLabelId);
    /** To dropdown excludes the currently selected From label. */
    public get FilteredToLabels(): MJVersionLabelEntityType[] {
        if (!this.FromLabelId) return this.AvailableLabels;
        return this.AvailableLabels.filter(l => l.ID !== this.FromLabelId);
    /** Swap From and To label selections. */
    public SwapLabels(): void {
        const temp = this.FromLabelId;
        this.FromLabelId = this.ToLabelId;
        this.ToLabelId = temp;
    // Diff result sorting & filtering
    public OnDiffSortChange(sortBy: 'name' | 'count'): void {
        if (this.DiffSortBy === sortBy) {
            this.DiffSortDir = this.DiffSortDir === 'asc' ? 'desc' : 'asc';
            this.DiffSortBy = sortBy;
            this.DiffSortDir = sortBy === 'name' ? 'asc' : 'desc';
    public OnDiffFilterChange(filterType: 'all' | 'added' | 'removed' | 'modified'): void {
        this.DiffFilterType = filterType;
    public OnDiffSearchChange(text: string): void {
        this.DiffSearch = text;
    private applySortAndFilterDiff(): void {
        let groups = [...this.EntityGroups];
        // Filter by change type
        if (this.DiffFilterType !== 'all') {
            groups = groups.map(g => ({
                ...g,
                Items: g.Items.filter(i => i.ChangeType.toLowerCase() === this.DiffFilterType)
            })).filter(g => g.Items.length > 0);
        if (this.DiffSearch) {
            const search = this.DiffSearch.toLowerCase();
            groups = groups
                .filter(g => g.EntityName.toLowerCase().includes(search) ||
                    g.Items.some(i => i.RecordID.toLowerCase().includes(search) || i.DisplayName.toLowerCase().includes(search)))
                .map(g => ({
                    Items: g.Items.filter(i =>
                        i.RecordID.toLowerCase().includes(search) ||
                        i.DisplayName.toLowerCase().includes(search) ||
                        g.EntityName.toLowerCase().includes(search)
                .filter(g => g.Items.length > 0);
        groups.sort((a, b) => {
            if (this.DiffSortBy === 'name') {
                const cmp = a.EntityName.localeCompare(b.EntityName);
                return this.DiffSortDir === 'asc' ? cmp : -cmp;
            const cmp = a.Items.length - b.Items.length;
        this.FilteredEntityGroups = groups;
