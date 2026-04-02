import { ResourceData, UserInfoEngine, MJVersionLabelRestoreEntityType } from '@memberjunction/core-entities';
interface VersionRestorePreferences {
    StatusFilter: string;
@RegisterClass(BaseResourceComponent, 'VersionHistoryRestoreResource')
    selector: 'mj-version-history-restore-resource',
    templateUrl: './restore-resource.component.html',
    styleUrls: ['./restore-resource.component.css'],
export class VersionHistoryRestoreResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    public TotalRestores = 0;
    public SuccessfulRestores = 0;
    public FailedRestores = 0;
    public PartialRestores = 0;
    public Restores: MJVersionLabelRestoreEntityType[] = [];
    public FilteredRestores: MJVersionLabelRestoreEntityType[] = [];
    // Expanded detail
    public ExpandedRestoreId = '';
    private static readonly PREFS_KEY = 'VersionHistory.Restore.UserPreferences';
        return 'Restore History';
            const result = await rv.RunView<MJVersionLabelRestoreEntityType>({
                EntityName: 'MJ: Version Label Restores',
                this.Restores = result.Results;
            console.error('Error loading restore history:', error);
        this.TotalRestores = this.Restores.length;
        this.SuccessfulRestores = this.Restores.filter(r => r.Status === 'Complete').length;
        this.FailedRestores = this.Restores.filter(r => r.Status === 'Error').length;
        this.PartialRestores = this.Restores.filter(r => r.Status === 'Partial').length;
        this.FilteredRestores = this.Restores.filter(r => {
            if (this.StatusFilter && r.Status !== this.StatusFilter) return false;
            const raw = UserInfoEngine.Instance.GetSetting(VersionHistoryRestoreResourceComponent.PREFS_KEY);
                const prefs: VersionRestorePreferences = JSON.parse(raw);
                if (prefs.StatusFilter != null) {
                    this.StatusFilter = prefs.StatusFilter;
            console.error('Error loading restore preferences:', error);
            this.StatusFilter = '';
        const prefs: VersionRestorePreferences = {
            StatusFilter: this.StatusFilter
        UserInfoEngine.Instance.SetSettingDebounced(VersionHistoryRestoreResourceComponent.PREFS_KEY, JSON.stringify(prefs));
    public ToggleExpand(restoreId: string | undefined): void {
        const id = restoreId ?? '';
        this.ExpandedRestoreId = this.ExpandedRestoreId === id ? '' : id;
    public IsExpanded(restoreId: string | undefined): boolean {
        return this.ExpandedRestoreId === (restoreId ?? '');
            'Complete': 'status-success',
            'Error': 'status-failed',
            'In Progress': 'status-progress',
            'Pending': 'status-pending',
            'Partial': 'status-partial'
    public GetStatusIcon(status: string | undefined): string {
            'Complete': 'fa-solid fa-circle-check',
            'Error': 'fa-solid fa-circle-xmark',
            'In Progress': 'fa-solid fa-spinner fa-spin',
            'Pending': 'fa-solid fa-clock',
            'Partial': 'fa-solid fa-circle-half-stroke'
        return icons[status ?? ''] ?? 'fa-solid fa-circle';
    public GetProgressPercent(restore: MJVersionLabelRestoreEntityType): number {
        if (!restore.TotalItems) return 0;
        return Math.round(((restore.CompletedItems ?? 0) / restore.TotalItems) * 100);
