import { ResourceData, MJCredentialEntity, MJCredentialTypeEntity, MJCredentialCategoryEntity, MJAuditLogEntity } from '@memberjunction/core-entities';
interface CategoryStat {
interface TypeStat {
    typeName: string;
    credentialCount: number;
    activeCount: number;
    expiringCount: number;
    credentialName: string;
    credentialId: string;
    action: 'Created' | 'Updated' | 'Accessed' | 'Rotated';
interface UsageTrendPoint {
    accessCount: number;
    uniqueCredentials: number;
@RegisterClass(BaseResourceComponent, 'CredentialsOverviewResource')
    selector: 'mj-credentials-overview-resource',
    templateUrl: './credentials-overview-resource.component.html',
    styleUrls: ['./credentials-overview-resource.component.css'],
export class CredentialsOverviewResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    // Summary stats
    public totalCredentials = 0;
    public activeCredentials = 0;
    public expiredCredentials = 0;
    public expiringSoonCount = 0;
    public credentialTypes = 0;
    public categories = 0;
    // Grouped data
    public categoryStats: CategoryStat[] = [];
    public typeStats: TypeStat[] = [];
    public usageTrend: UsageTrendPoint[] = [];
    // Raw data
    private credentials: MJCredentialEntity[] = [];
    private types: MJCredentialTypeEntity[] = [];
    private categoryList: MJCredentialCategoryEntity[] = [];
    private auditLogs: MJAuditLogEntity[] = [];
    // Category colors for charts
    private categoryColors: Record<string, string> = {
        'AI': '#8b5cf6',
        'Communication': '#3b82f6',
        'Storage': '#10b981',
        'Database': '#f59e0b',
        'Authentication': '#ef4444',
        'Integration': '#6366f1'
        return 'Overview';
        return 'fa-solid fa-chart-pie';
    public get UserCanCreateCredentials(): boolean {
    public get UserCanUpdateCredentials(): boolean {
            // Calculate date range for audit logs (last 30 days)
            const dateFilter = `__mj_CreatedAt >= '${thirtyDaysAgo.toISOString()}'`;
            // Load all data in parallel using RunViews
            const [credResult, typeResult, categoryResult, auditResult] = await rv.RunViews([
                    ExtraFilter: `AuditLogType LIKE '%Credential%' AND ${dateFilter}`,
                this.processCredentialStats();
                this.credentialTypes = this.types.length;
                this.processTypeStats();
            if (categoryResult.Success) {
                this.categoryList = categoryResult.Results as MJCredentialCategoryEntity[];
                this.categories = this.categoryList.length;
                this.processCategoryStats();
            if (auditResult.Success) {
                this.auditLogs = auditResult.Results as MJAuditLogEntity[];
                this.processActivityAndTrends();
            // Build recent activity from credentials if no audit logs
            if (this.recentActivity.length === 0) {
                this.buildActivityFromCredentials();
            console.error('Error loading credentials overview:', error);
    private processCredentialStats(): void {
        this.totalCredentials = this.credentials.length;
        this.activeCredentials = this.credentials.filter(c => c.IsActive).length;
        this.expiredCredentials = this.credentials.filter(c =>
        this.expiringSoonCount = this.credentials.filter(c =>
    private processTypeStats(): void {
        this.typeStats = this.types.map(type => {
            const typeCredentials = this.credentials.filter(c => c.CredentialTypeID === type.ID);
                typeId: type.ID,
                typeName: type.Name,
                category: type.Category,
                credentialCount: typeCredentials.length,
                activeCount: typeCredentials.filter(c => c.IsActive).length,
                expiringCount: typeCredentials.filter(c =>
                    new Date(c.ExpiresAt) <= thirtyDaysFromNow
        }).sort((a, b) => b.credentialCount - a.credentialCount);
    private processCategoryStats(): void {
        const categoryMap = new Map<string, CategoryStat>();
        // Initialize from credential types
            const existing = categoryMap.get(category);
            const categoryCredentials = this.credentials.filter(c => c.CredentialTypeID === type.ID);
                existing.count += categoryCredentials.length;
                categoryMap.set(category, {
                    categoryId: category, // Use category name as ID for filtering
                    count: categoryCredentials.length,
                    iconClass: this.getCategoryIcon(category),
                    color: this.categoryColors[category] || '#64748b',
                    percentage: 0
        // Calculate percentages
        const total = this.totalCredentials || 1;
        categoryMap.forEach(stat => {
            stat.percentage = Math.round((stat.count / total) * 100);
        this.categoryStats = Array.from(categoryMap.values())
    private processActivityAndTrends(): void {
        // Process recent activity from audit logs
        this.recentActivity = this.auditLogs
            .map(log => ({
                credentialName: this.extractCredentialName(log.Description || ''),
                credentialId: '', // Would need to parse from log
                typeName: 'Credential',
                action: this.extractAction(log.Description || '') as ActivityItem['action'],
                date: new Date(log.__mj_CreatedAt),
                user: log.User
        // Build usage trend data (group by day)
        const trendMap = new Map<string, UsageTrendPoint>();
        const uniqueCredentialsPerDay = new Map<string, Set<string>>();
            const dateKey = new Date(log.__mj_CreatedAt).toISOString().split('T')[0];
            if (!trendMap.has(dateKey)) {
                trendMap.set(dateKey, {
                    timestamp: new Date(dateKey),
                    accessCount: 0,
                    uniqueCredentials: 0,
                    successRate: 100
                uniqueCredentialsPerDay.set(dateKey, new Set());
            const point = trendMap.get(dateKey)!;
            point.accessCount++;
            // Track unique credentials (would need proper parsing)
            const credId = this.extractCredentialId(log.Description || '');
            if (credId) {
                uniqueCredentialsPerDay.get(dateKey)!.add(credId);
        // Finalize unique counts
        trendMap.forEach((point, dateKey) => {
            point.uniqueCredentials = uniqueCredentialsPerDay.get(dateKey)?.size || 0;
        this.usageTrend = Array.from(trendMap.values())
            .sort((a, b) => a.timestamp.getTime() - b.timestamp.getTime());
    private buildActivityFromCredentials(): void {
        this.recentActivity = this.credentials
            .filter(c => c.__mj_UpdatedAt)
            .sort((a, b) => new Date(b.__mj_UpdatedAt).getTime() - new Date(a.__mj_UpdatedAt).getTime())
                id: c.ID,
                credentialName: c.Name,
                credentialId: c.ID,
                typeName: c.CredentialType || 'Unknown',
                action: 'Updated' as const,
                date: new Date(c.__mj_UpdatedAt),
                user: undefined
    private extractCredentialName(description: string): string {
        // Try to extract credential name from audit log description
        const match = description.match(/credential[:\s]+['"]?([^'"]+)['"]?/i);
        return match ? match[1] : 'Unknown Credential';
    private extractAction(description: string): string {
        if (description.toLowerCase().includes('creat')) return 'Created';
        if (description.toLowerCase().includes('rotat')) return 'Rotated';
        if (description.toLowerCase().includes('access')) return 'Accessed';
        return 'Updated';
    private extractCredentialId(description: string): string {
        const match = description.match(/[a-f0-9-]{36}/i);
        return match ? match[0] : '';
    private getCategoryIcon(category: string): string {
        return iconMap[category] || 'fa-solid fa-key';
    // === Navigation Actions ===
        // Navigate to Credentials tab with openCreatePanel flag to show the slide-in editor
    public openCredential(credentialId: string): void {
        const key = new CompositeKey([{ FieldName: 'ID', Value: credentialId }]);
        this.navigationService.OpenEntityRecord('MJ: Credentials', key);
    public onCategoryClick(category: CategoryStat): void {
        // Navigate to types nav item with category filter
            categoryFilter: category.category
    public onTypeClick(typeStat: TypeStat): void {
        // Navigate to credentials nav item with type filter
            typeId: typeStat.typeId
        if (activity.credentialId) {
            this.openCredential(activity.credentialId);
    public viewAllCredentials(): void {
        this.navigationService.OpenNavItemByName('Credentials');
    public viewAuditLog(): void {
        this.navigationService.OpenNavItemByName('Audit Log');
    public viewAllTypes(): void {
        this.navigationService.OpenNavItemByName('Types');
    public viewAllCategories(): void {
        this.navigationService.OpenNavItemByName('Categories');
    public viewExpiringCredentials(): void {
            filter: 'expiring'
    // === Formatting Helpers ===
    public getActionIcon(action: string): string {
            'Created': 'fa-solid fa-plus',
            'Updated': 'fa-solid fa-pen',
            'Accessed': 'fa-solid fa-eye',
            'Rotated': 'fa-solid fa-rotate'
        return iconMap[action] || 'fa-solid fa-circle';
    public getActionClass(action: string): string {
        const classMap: Record<string, string> = {
            'Created': 'action-created',
            'Updated': 'action-updated',
            'Accessed': 'action-accessed',
            'Rotated': 'action-rotated'
        return classMap[action] || '';
        if (this.totalCredentials === 0) return 100;
        const activeRatio = this.activeCredentials / this.totalCredentials;
        const expiredPenalty = (this.expiredCredentials / this.totalCredentials) * 30;
        const expiringPenalty = (this.expiringSoonCount / this.totalCredentials) * 15;
        return Math.max(0, Math.min(100, Math.round((activeRatio * 100) - expiredPenalty - expiringPenalty)));
        if (score >= 80) return 'health-good';
        if (score >= 60) return 'health-warning';
        if (score >= 80) return 'Healthy';
        if (score >= 60) return 'Needs Attention';
        return 'Critical';
        // Calculate cumulative offset for donut chart segments
        // Each segment starts where the previous one ended
        // The circumference is 251 (2 * PI * 40)
        let offset = 63; // Start at top (25% of circumference = 90 degrees rotation)
            offset -= this.categoryStats[i].percentage * 2.51;
