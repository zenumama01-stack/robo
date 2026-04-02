import { ResourceData, MJVersionLabelEntityType, MJVersionLabelItemEntityType, UserInfoEngine } from '@memberjunction/core-entities';
import { EntityLinkClickEvent } from '@memberjunction/ng-versions';
    Scope: string;
    Color: string;
interface StatusStat {
interface VersionLabelPreferences {
    ViewMode: 'card' | 'list';
    ActiveScopeFilter: string;
    ActiveStatusFilter: string;
    DefaultDetailTab: string;
    SortField: string;
    SortDirection: 'asc' | 'desc';
type SortField = 'Name' | 'Scope' | 'Status' | 'Items' | 'Date';
@RegisterClass(BaseResourceComponent, 'VersionHistoryLabelsResource')
    selector: 'mj-version-history-labels-resource',
    templateUrl: './labels-resource.component.html',
    styleUrls: ['./labels-resource.component.css'],
export class VersionHistoryLabelsResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    private static readonly PREFS_KEY = 'VersionHistory.Labels.UserPreferences';
    public ViewMode: 'card' | 'list' = 'card';
    public SelectedLabel: MJVersionLabelEntityType | null = null;
    public ShowDetailPanel = false;
    public TotalLabels = 0;
    public ActiveLabels = 0;
    public ArchivedLabels = 0;
    public RestoredLabels = 0;
    public StatusStats: StatusStat[] = [];
    public Labels: MJVersionLabelEntityType[] = [];
    public FilteredLabels: MJVersionLabelEntityType[] = [];
    public ItemCountMap = new Map<string, number>();
    public ScopeFilter = '';
    public SortField: SortField = 'Date';
    // User preferences
    // Create Label Wizard
    public ShowCreateWizard = false;
        return 'Labels';
    // =======================================================================
    // Data loading
    public async LoadData(): Promise<void> {
            const [labelsResult, itemsResult] = await rv.RunViews([
                    Fields: ['VersionLabelID'],
            if (labelsResult.Success) {
                this.Labels = labelsResult.Results as MJVersionLabelEntityType[];
                const items = itemsResult.Success
                    ? itemsResult.Results as MJVersionLabelItemEntityType[]
                this.ItemCountMap = this.buildItemCountMap(items);
            console.error('Error loading version labels:', error);
    private buildItemCountMap(items: MJVersionLabelItemEntityType[]): Map<string, number> {
            const labelId = item.VersionLabelID ?? '';
            counts.set(labelId, (counts.get(labelId) ?? 0) + 1);
        return counts;
    public GetItemCount(labelId: string | undefined): number {
        return this.ItemCountMap.get(labelId ?? '') ?? 0;
    public ResolveEntityName(entityId: string | undefined): string {
        if (!entityId) return '';
        return entity ? entity.Name : '';
        // Only count root-level labels (exclude children) for stats
        const rootLabels = this.Labels.filter(l => !l.ParentID);
        this.TotalLabels = rootLabels.length;
        this.ActiveLabels = rootLabels.filter(l => l.Status === 'Active').length;
        this.ArchivedLabels = rootLabels.filter(l => l.Status === 'Archived').length;
        this.RestoredLabels = rootLabels.filter(l => l.Status === 'Restored').length;
        this.ScopeStats = this.computeScopeStats();
        this.StatusStats = this.computeStatusStats();
    private computeScopeStats(): ScopeStat[] {
        const scopeMap = new Map<string, number>();
        for (const label of this.Labels) {
            const scope = label.Scope ?? 'Record';
            scopeMap.set(scope, (scopeMap.get(scope) ?? 0) + 1);
        const scopeConfig: Record<string, { Icon: string; Color: string }> = {
            'System': { Icon: 'fa-solid fa-globe', Color: '#6366f1' },
            'Entity': { Icon: 'fa-solid fa-table', Color: '#3b82f6' },
            'Record': { Icon: 'fa-solid fa-file', Color: '#10b981' }
        return Array.from(scopeMap.entries()).map(([scope, count]) => ({
            Scope: scope,
            Count: count,
            Icon: scopeConfig[scope]?.Icon ?? 'fa-solid fa-tag',
            Color: scopeConfig[scope]?.Color ?? '#64748b'
    private computeStatusStats(): StatusStat[] {
        const statusMap = new Map<string, number>();
            const status = label.Status ?? 'Active';
            statusMap.set(status, (statusMap.get(status) ?? 0) + 1);
            'Active': '#10b981',
            'Archived': '#6b7280',
            'Restored': '#f59e0b'
        return Array.from(statusMap.entries()).map(([status, count]) => ({
            Color: statusColors[status] ?? '#64748b'
        // First: exclude child labels (those with ParentID) from top-level list
        let result = this.Labels.filter(l => !l.ParentID);
        // Apply scope filter
        if (this.ScopeFilter) {
            result = result.filter(l => l.Scope === this.ScopeFilter);
            result = result.filter(l => l.Status === this.StatusFilter);
        // Apply search
            result = result.filter(l => {
                const name = (l.Name ?? '').toLowerCase();
                const desc = (l.Description ?? '').toLowerCase();
                const entityName = (l.Entity ?? this.ResolveEntityName(l.EntityID)).toLowerCase();
                return name.includes(search) || desc.includes(search) || entityName.includes(search);
        result = this.sortLabels(result);
        this.FilteredLabels = result;
    private sortLabels(labels: MJVersionLabelEntityType[]): MJVersionLabelEntityType[] {
        return [...labels].sort((a, b) => {
                    return dir * (a.Name ?? '').localeCompare(b.Name ?? '');
                case 'Scope':
                    return dir * (a.Scope ?? '').localeCompare(b.Scope ?? '');
                    return dir * (a.Status ?? '').localeCompare(b.Status ?? '');
                case 'Items': {
                    const aCount = this.GetItemCount(a.ID);
                    const bCount = this.GetItemCount(b.ID);
                    return dir * (aCount - bCount);
                    const aDate = (a as Record<string, unknown>)['__mj_CreatedAt'] ?? '';
                    const bDate = (b as Record<string, unknown>)['__mj_CreatedAt'] ?? '';
                    return dir * String(aDate).localeCompare(String(bDate));
    public OnSortChange(field: SortField): void {
            this.SortDirection = field === 'Date' ? 'desc' : 'asc';
    public GetSortIcon(field: SortField): string {
    public OnScopeFilterChange(scope: string): void {
        this.ScopeFilter = this.ScopeFilter === scope ? '' : scope;
        this.StatusFilter = this.StatusFilter === status ? '' : status;
    // View mode and detail panel
    public ToggleViewMode(): void {
        this.ViewMode = this.ViewMode === 'card' ? 'list' : 'card';
    public SetViewMode(mode: 'card' | 'list'): void {
        if (this.ViewMode !== mode) {
    public OnLabelClick(label: MJVersionLabelEntityType): void {
        this.SelectedLabel = label;
        this.ShowDetailPanel = true;
    public OnDetailPanelClose(): void {
        this.ShowDetailPanel = false;
        this.SelectedLabel = null;
    public OnLabelUpdated(): void {
    public OnEntityLinkClick(event: EntityLinkClickEvent): void {
        this.navigationService.OpenEntityRecord(event.EntityName, event.CompositeKey);
            const json = UserInfoEngine.Instance.GetSetting(VersionHistoryLabelsResourceComponent.PREFS_KEY);
            if (json) {
                const prefs = JSON.parse(json) as VersionLabelPreferences;
                this.ViewMode = prefs.ViewMode ?? 'card';
                this.ScopeFilter = prefs.ActiveScopeFilter ?? '';
                this.StatusFilter = prefs.ActiveStatusFilter ?? '';
                if (prefs.SortField) this.SortField = prefs.SortField as SortField;
                if (prefs.SortDirection) this.SortDirection = prefs.SortDirection;
            console.warn('[VersionLabels] Failed to load user preferences:', e);
        const prefs: VersionLabelPreferences = {
            ViewMode: this.ViewMode,
            ActiveScopeFilter: this.ScopeFilter,
            ActiveStatusFilter: this.StatusFilter,
            DefaultDetailTab: 'overview',
            SortField: this.SortField,
            SortDirection: this.SortDirection,
            VersionHistoryLabelsResourceComponent.PREFS_KEY,
    public OpenCreateDialog(): void {
        this.ShowCreateWizard = true;
    public OnLabelCreated(_event: { LabelCount: number; ItemCount: number }): void {
        this.ShowCreateWizard = false;
    public OnCreateWizardCancel(): void {
    public GetScopeIcon(scope: string | undefined): string {
            'System': 'fa-solid fa-globe',
            'Entity': 'fa-solid fa-table',
            'Record': 'fa-solid fa-file'
        return icons[scope ?? ''] ?? 'fa-solid fa-tag';
    public GetStatusClass(status: string | undefined): string {
            'Active': 'status-active',
            'Archived': 'status-archived',
            'Restored': 'status-restored'
        return classes[status ?? ''] ?? '';
    public FormatDate(date: Date | string | undefined): string {
    public FormatNumber(num: number): string {
        if (num >= 1000) return (num / 1000).toFixed(1) + 'k';
        if (!ms) return '';
    public IsGroupParent(label: MJVersionLabelEntityType): boolean {
        return !label.RecordID && !label.ParentID && !!label.EntityID;
    public GetChildLabels(parentId: string | undefined): MJVersionLabelEntityType[] {
        if (!parentId) return [];
        return this.Labels.filter(l => l.ParentID === parentId);
