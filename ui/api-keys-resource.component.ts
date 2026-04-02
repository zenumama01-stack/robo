import { Component, OnInit, OnDestroy, ViewChild, ChangeDetectorRef } from '@angular/core';
import { BaseResourceComponent } from '@memberjunction/ng-shared';
import { MJAPIKeyEntity, MJAPIScopeEntity, MJAPIKeyUsageLogEntity, MJAPIApplicationEntity, ResourceData } from '@memberjunction/core-entities';
import { APIKeyFilter, APIKeyListComponent } from './api-key-list.component';
import { APIKeyCreateResult } from './api-key-create-dialog.component';
/** Activity types for recent activity display */
type ActivityAction = 'Created' | 'Updated' | 'Revoked' | 'Used' | 'Extended';
/** Interface for recent activity items */
    keyLabel: string;
    action: ActivityAction;
    user: string;
    keyId: string;
/** Interface for scope statistics */
interface ScopeStat {
    iconClass: string;
/** Current view type */
type ViewType = 'overview' | 'list';
/** Main tab type */
type MainTab = 'keys' | 'applications' | 'scopes' | 'usage';
 * API Keys Resource Component
 * Provides management interface for MJ API Keys including:
 * - Overview statistics and health monitoring
 * - Key listing with status filters
 * - Key creation and revocation
 * - Usage tracking and analytics
@RegisterClass(BaseResourceComponent, 'APIKeysResource')
    selector: 'mj-api-keys-resource',
    templateUrl: './api-keys-resource.component.html',
    styleUrls: ['./api-keys-resource.component.css']
export class APIKeysResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    @ViewChild('keyList') keyListComponent: APIKeyListComponent | undefined;
    public CurrentView: ViewType = 'overview';
    public ListFilter: APIKeyFilter = 'all';
    public MainTab: MainTab = 'keys';
    public NavOpen = false;
    // Application and scope counts for tab badges
    public ApplicationCount = 0;
    public ScopeCount = 0;
    public TotalKeys = 0;
    public ActiveKeys = 0;
    public RevokedKeys = 0;
    public ExpiringSoonCount = 0;
    public ExpiredKeys = 0;
    public NeverUsedKeys = 0;
    // Data collections
    public APIKeys: MJAPIKeyEntity[] = [];
    public RecentActivity: ActivityItem[] = [];
    public ScopeStats: ScopeStat[] = [];
    public TopUsedKeys: MJAPIKeyEntity[] = [];
    // Dialog states
    public ShowCreateDialog = false;
    public SelectedKeyId: string | null = null;
    // Dynamic category UI configs built from root scopes
    // User permissions
    public UserCanCreateKeys = false;
    public UserCanRevokeKeys = false;
        return 'API Keys';
        return 'fa-solid fa-key';
     * Load all dashboard data
                this.loadAPIKeys(),
                this.loadScopeStats(),
                this.loadRecentActivity(),
                this.loadCounts(),
                this.checkPermissions()
            this.calculateStatistics();
            console.error('Error loading API Keys dashboard data:', error);
     * Load application and scope counts for tab badges.
    private loadCounts(): void {
        this.ApplicationCount = base.Applications.length;
        this.ScopeCount = base.Scopes.length;
    private async loadAPIKeys(): Promise<void> {
            this.APIKeys = result.Results;
     * Load scope statistics by category.
    private loadScopeStats(): void {
        // Build category UI configs from root scopes
        const categoryMap = new Map<string, number>();
            categoryMap.set(category, (categoryMap.get(category) || 0) + 1);
        const total = scopes.length;
        this.ScopeStats = Array.from(categoryMap.entries()).map(([category, count]) => {
                percentage: total > 0 ? Math.round((count / total) * 100) : 0,
                iconClass: config.icon
     * Load recent activity from usage logs and key changes
    private async loadRecentActivity(): Promise<void> {
        // Load usage logs
        const usageResult = await rv.RunView<MJAPIKeyUsageLogEntity>({
            MaxRows: 20,
        if (usageResult.Success) {
            for (const log of usageResult.Results.slice(0, 10)) {
                    keyLabel: log.APIKey || 'Unknown Key',
                    action: 'Used',
                    user: log.APIKey || 'System',
                    date: log.__mj_CreatedAt,
                    keyId: log.APIKeyID
        // Add key creation/update activities from keys
        for (const key of this.APIKeys.slice(0, 10)) {
            if (key.Status === 'Revoked') {
                    keyLabel: key.Label,
                    action: 'Revoked',
                    user: key.User,
                    date: key.__mj_UpdatedAt,
                    keyId: key.ID
                    action: 'Created',
                    user: key.CreatedByUser,
                    date: key.__mj_CreatedAt,
        // Sort by date and take top 10
        this.RecentActivity = activities
            .sort((a, b) => b.date.getTime() - a.date.getTime())
     * Check user permissions
    private async checkPermissions(): Promise<void> {
        // Check if user can create/manage API keys
        const entityInfo = this.md.Entities.find(e => e.Name === 'MJ: API Keys');
            const permissions = entityInfo.GetUserPermisions(this.md.CurrentUser);
            this.UserCanCreateKeys = permissions.CanCreate;
            this.UserCanRevokeKeys = permissions.CanUpdate;
     * Calculate dashboard statistics from loaded data
    private calculateStatistics(): void {
        this.TotalKeys = this.APIKeys.length;
        this.ActiveKeys = this.APIKeys.filter(k => k.Status === 'Active').length;
        this.RevokedKeys = this.APIKeys.filter(k => k.Status === 'Revoked').length;
        this.ExpiredKeys = this.APIKeys.filter(k =>
        this.ExpiringSoonCount = this.APIKeys.filter(k =>
        this.NeverUsedKeys = this.APIKeys.filter(k =>
        // Get top used keys (most recently used active keys)
        this.TopUsedKeys = this.APIKeys
            .filter(k => k.Status === 'Active' && k.LastUsedAt)
                const aDate = a.LastUsedAt ? new Date(a.LastUsedAt).getTime() : 0;
                const bDate = b.LastUsedAt ? new Date(b.LastUsedAt).getTime() : 0;
                return bDate - aDate;
     * Refresh all data
    public async refresh(): Promise<void> {
        if (this.keyListComponent) {
            await this.keyListComponent.loadKeys();
     * Switch to list view
    public showListView(filter: APIKeyFilter = 'all'): void {
        this.ListFilter = filter;
        this.CurrentView = 'list';
     * Switch to overview
    public showOverview(): void {
        this.CurrentView = 'overview';
     * Open create dialog
    public openCreateDialog(): void {
        this.ShowCreateDialog = true;
     * Handle key created
    public async onKeyCreated(result: APIKeyCreateResult): Promise<void> {
            await this.refresh();
     * Handle create dialog closed
    public onCreateDialogClosed(): void {
        this.ShowCreateDialog = false;
     * Open edit panel for a key
    public openEditPanel(key: MJAPIKeyEntity): void {
        this.SelectedKeyId = key.ID;
     * Handle key from list selected
    public onKeySelected(key: MJAPIKeyEntity): void {
        this.openEditPanel(key);
     * Handle key updated
    public async onKeyUpdated(): Promise<void> {
     * Handle key revoked
    public async onKeyRevoked(): Promise<void> {
     * Handle edit panel closed
    public onEditPanelClosed(): void {
        this.SelectedKeyId = null;
     * Get health score (0-100) based on key status
    public getHealthScore(): number {
        if (this.TotalKeys === 0) return 100;
        let score = 100;
        // Deduct for expired keys
        score -= (this.ExpiredKeys / this.TotalKeys) * 40;
        // Deduct for expiring soon
        score -= (this.ExpiringSoonCount / this.TotalKeys) * 20;
        // Deduct for never used active keys (might be leaked)
        score -= (this.NeverUsedKeys / this.TotalKeys) * 10;
        // Deduct if too many active keys
        if (this.ActiveKeys > 20) {
            score -= Math.min(15, (this.ActiveKeys - 20) * 0.5);
        return Math.max(0, Math.round(score));
     * Get health label based on score
    public getHealthLabel(): string {
        const score = this.getHealthScore();
        if (score >= 90) return 'Excellent Security';
        if (score >= 75) return 'Good Security';
        if (score >= 50) return 'Needs Attention';
        return 'Critical Issues';
     * Get CSS class for health banner
    public getHealthClass(): string {
        if (score >= 75) return '';
        if (score >= 50) return 'health-warning';
        return 'health-critical';
     * Get donut chart offset for segment
    public getDonutOffset(index: number): number {
        for (let i = 0; i < index; i++) {
            offset -= this.ScopeStats[i].percentage * 2.51;
        return offset;
     * Get activity icon based on action
    public getActionIcon(action: ActivityAction): string {
        switch (action) {
            case 'Created': return 'fa-solid fa-plus';
            case 'Updated': return 'fa-solid fa-pencil';
            case 'Revoked': return 'fa-solid fa-ban';
            case 'Used': return 'fa-solid fa-arrow-right-to-bracket';
            case 'Extended': return 'fa-solid fa-clock-rotate-left';
            default: return 'fa-solid fa-circle';
     * Get CSS class for activity action
    public getActionClass(action: ActivityAction): string {
            case 'Created': return 'action-created';
            case 'Updated': return 'action-updated';
            case 'Revoked': return 'action-revoked';
            case 'Used': return 'action-used';
            case 'Extended': return 'action-extended';
        if (!date) return 'Never expires';
        const expiresAt = new Date(date);
        if (days === 0) return 'Expires today';
        if (days === 1) return 'Expires tomorrow';
        if (days < 30) return `Expires in ${days} days`;
        return `Expires ${expiresAt.toLocaleDateString()}`;
        if (!key.ExpiresAt) return '';
        if (days < 7) return 'expiring-critical';
        if (days < 30) return 'expiring-soon';
     * View activity for a key
    public onActivityClick(activity: ActivityItem): void {
        const key = this.APIKeys.find(k => k.ID === activity.keyId);
        if (key) {
     * View scope details - now navigates to scopes tab
    public onScopeClick(_stat: ScopeStat): void {
        this.MainTab = 'scopes';
     * Switch to a main tab
    public switchTab(tab: MainTab): void {
        this.MainTab = tab;
        // Reset to overview when switching back to keys
        if (tab === 'keys') {
     * Toggle mobile navigation
    public toggleNav(): void {
        this.NavOpen = !this.NavOpen;
     * Close mobile navigation
    public closeNav(): void {
        this.NavOpen = false;
     * Handle updates from child panels
    public async onDataUpdated(): Promise<void> {
        await this.loadCounts();
