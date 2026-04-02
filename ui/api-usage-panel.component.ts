import { MJAPIKeyEntity, MJAPIKeyUsageLogEntity } from '@memberjunction/core-entities';
/** Time bucket for aggregation */
interface TimeBucket {
    requests: number;
    avgResponseTime: number;
/** Endpoint stats */
interface EndpointStats {
/** Key stats */
interface KeyStats {
    lastUsed: Date | null;
/** Status code group */
interface StatusGroup {
/** Usage log item for display */
 * API Usage Analytics Panel Component
 * Comprehensive usage statistics and drill-down capabilities
    selector: 'mj-api-usage-panel',
    templateUrl: './api-usage-panel.component.html',
    styleUrls: ['./api-usage-panel.component.css']
export class APIUsagePanelComponent implements OnInit {
    public IsLoadingLogs = false;
    // Time range filter
    public TimeRange: 'day' | 'week' | 'month' | 'all' = 'week';
    // Summary KPIs
    public TotalRequests = 0;
    public TotalErrors = 0;
    public AvgResponseTime = 0;
    public SuccessRate = 0;
    public UniqueKeys = 0;
    public UniqueEndpoints = 0;
    // Trend data (vs previous period)
    public RequestsTrend = 0;
    public ErrorsTrend = 0;
    public ResponseTimeTrend = 0;
    // Chart data
    public TimeBuckets: TimeBucket[] = [];
    public MaxRequests = 0;
    public MaxErrors = 0;
    // Breakdown data
    public TopEndpoints: EndpointStats[] = [];
    public TopKeys: KeyStats[] = [];
    public StatusGroups: StatusGroup[] = [];
    // Recent logs
    public RecentLogs: UsageLogItem[] = [];
    public ShowLogsPanel = false;
    public LogsFilter: { endpoint?: string; keyId?: string } = {};
    // Key map for labels
    public KeyMap = new Map<string, string>();
     * Load all usage data
            // Load keys for label lookup
            // Load usage data
            await this.loadUsageStats();
            console.error('Error loading usage data:', error);
     * Load API keys for label lookup
    private async loadKeys(): Promise<void> {
            Fields: ['ID', 'Label'],
            for (const key of result.Results) {
                this.KeyMap.set(key.ID, key.Label);
     * Load usage statistics based on time range
    private async loadUsageStats(): Promise<void> {
        const filter = this.getTimeFilter();
            MaxRows: 5000,
            this.calculateSummaryKPIs(logs);
            this.buildTimeBuckets(logs);
            this.buildEndpointStats(logs);
            this.buildKeyStats(logs);
            this.buildStatusGroups(logs);
            this.RecentLogs = logs.slice(0, 20).map(log => this.mapLogToItem(log));
     * Get time filter for RunView based on selected range
    private getTimeFilter(): string {
        let startDate: Date;
        switch (this.TimeRange) {
                startDate = new Date(now.getTime() - 24 * 60 * 60 * 1000);
                startDate = new Date(now.getTime() - 7 * 24 * 60 * 60 * 1000);
                startDate = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000);
        return `__mj_CreatedAt >= '${startDate.toISOString()}'`;
     * Calculate summary KPIs
    private calculateSummaryKPIs(logs: MJAPIKeyUsageLogEntity[]): void {
        this.TotalRequests = logs.length;
        // Count errors (4xx and 5xx status codes)
        this.TotalErrors = logs.filter(l => l.StatusCode >= 400).length;
        // Calculate average response time
        const totalResponseTime = logs.reduce((sum, l) => sum + (l.ResponseTimeMs || 0), 0);
        this.AvgResponseTime = this.TotalRequests > 0
            ? Math.round(totalResponseTime / this.TotalRequests)
        // Success rate
        this.SuccessRate = this.TotalRequests > 0
            ? Math.round(((this.TotalRequests - this.TotalErrors) / this.TotalRequests) * 100)
        // Unique counts
        const keySet = new Set(logs.map(l => l.APIKeyID));
        const endpointSet = new Set(logs.map(l => l.Endpoint));
        this.UniqueKeys = keySet.size;
        this.UniqueEndpoints = endpointSet.size;
     * Build time buckets for chart
    private buildTimeBuckets(logs: MJAPIKeyUsageLogEntity[]): void {
        const bucketCount = this.TimeRange === 'day' ? 24 : this.TimeRange === 'week' ? 7 : 30;
        const bucketDuration = this.TimeRange === 'day' ? 60 * 60 * 1000 : 24 * 60 * 60 * 1000;
        const buckets: TimeBucket[] = [];
        for (let i = bucketCount - 1; i >= 0; i--) {
            const bucketDate = new Date(now.getTime() - i * bucketDuration);
            const bucketLogs = logs.filter(l => {
                const logDate = new Date(l.__mj_CreatedAt);
                const nextBucket = new Date(bucketDate.getTime() + bucketDuration);
                return logDate >= bucketDate && logDate < nextBucket;
            const errors = bucketLogs.filter(l => l.StatusCode >= 400).length;
            const totalTime = bucketLogs.reduce((sum, l) => sum + (l.ResponseTimeMs || 0), 0);
            buckets.push({
                label: this.formatBucketLabel(bucketDate),
                date: bucketDate,
                requests: bucketLogs.length,
                avgResponseTime: bucketLogs.length > 0 ? Math.round(totalTime / bucketLogs.length) : 0
        this.TimeBuckets = buckets;
        this.MaxRequests = Math.max(...buckets.map(b => b.requests), 1);
        this.MaxErrors = Math.max(...buckets.map(b => b.errors), 1);
     * Format bucket label based on time range
    private formatBucketLabel(date: Date): string {
        if (this.TimeRange === 'day') {
            return date.toLocaleTimeString('en-US', { hour: 'numeric' });
        return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
     * Build endpoint statistics
    private buildEndpointStats(logs: MJAPIKeyUsageLogEntity[]): void {
        const endpointMap = new Map<string, {
            totalTime: number;
            errors: number
            const key = `${log.Method}:${log.Endpoint}`;
            const existing = endpointMap.get(key);
                existing.requests++;
                existing.totalTime += log.ResponseTimeMs || 0;
                if (log.StatusCode >= 400) existing.errors++;
                endpointMap.set(key, {
                    endpoint: log.Endpoint,
                    method: log.Method,
                    requests: 1,
                    totalTime: log.ResponseTimeMs || 0,
                    errors: log.StatusCode >= 400 ? 1 : 0
        this.TopEndpoints = Array.from(endpointMap.values())
                endpoint: e.endpoint,
                method: e.method,
                requests: e.requests,
                avgResponseTime: Math.round(e.totalTime / e.requests),
                errorRate: Math.round((e.errors / e.requests) * 100)
            .sort((a, b) => b.requests - a.requests)
     * Build key statistics
    private buildKeyStats(logs: MJAPIKeyUsageLogEntity[]): void {
        const keyMap = new Map<string, { requests: number; lastUsed: Date | null }>();
            const existing = keyMap.get(log.APIKeyID);
            const logDate = new Date(log.__mj_CreatedAt);
                if (!existing.lastUsed || logDate > existing.lastUsed) {
                    existing.lastUsed = logDate;
                keyMap.set(log.APIKeyID, {
                    lastUsed: logDate
        this.TopKeys = Array.from(keyMap.entries())
            .map(([keyId, stats]) => ({
                keyId,
                label: this.KeyMap.get(keyId) || 'Unknown Key',
                requests: stats.requests,
                lastUsed: stats.lastUsed
     * Build status code groups
    private buildStatusGroups(logs: MJAPIKeyUsageLogEntity[]): void {
        const groups: Record<string, { count: number; label: string; color: string }> = {
            '2xx': { count: 0, label: 'Success (2xx)', color: '#10b981' },
            '3xx': { count: 0, label: 'Redirect (3xx)', color: '#3b82f6' },
            '4xx': { count: 0, label: 'Client Error (4xx)', color: '#f59e0b' },
            '5xx': { count: 0, label: 'Server Error (5xx)', color: '#ef4444' }
            const code = Math.floor(log.StatusCode / 100);
            const key = `${code}xx`;
            if (groups[key]) {
                groups[key].count++;
        this.StatusGroups = Object.entries(groups)
            .filter(([_, v]) => v.count > 0)
            .map(([code, v]) => ({
                label: v.label,
                count: v.count,
                percentage: this.TotalRequests > 0 ? Math.round((v.count / this.TotalRequests) * 100) : 0,
                color: v.color
     * Map log entity to display item
    private mapLogToItem(log: MJAPIKeyUsageLogEntity): UsageLogItem {
            id: log.ID,
            timestamp: new Date(log.__mj_CreatedAt),
            statusCode: log.StatusCode,
            keyLabel: this.KeyMap.get(log.APIKeyID) || 'Unknown',
     * Change time range and reload
    public async setTimeRange(range: 'day' | 'week' | 'month' | 'all'): Promise<void> {
        this.TimeRange = range;
     * Get bar height percentage for chart
    public getBarHeight(value: number): number {
        if (this.MaxRequests === 0) return 0;
        return Math.max(2, (value / this.MaxRequests) * 100);
     * Get error bar height for chart
    public getErrorBarHeight(bucket: TimeBucket): number {
        if (bucket.requests === 0) return 0;
        return (bucket.errors / bucket.requests) * 100;
     * Drill down into endpoint
    public drillDownEndpoint(endpoint: EndpointStats): void {
        this.LogsFilter = { endpoint: endpoint.endpoint };
        this.loadFilteredLogs();
        this.ShowLogsPanel = true;
     * Drill down into key
    public drillDownKey(key: KeyStats): void {
        this.LogsFilter = { keyId: key.keyId };
     * Load filtered logs for drill-down
    private async loadFilteredLogs(): Promise<void> {
        let filter = this.getTimeFilter();
        if (this.LogsFilter.endpoint) {
            filter += filter ? ' AND ' : '';
            filter += `Endpoint = '${this.LogsFilter.endpoint}'`;
        if (this.LogsFilter.keyId) {
            filter += `APIKeyID = '${this.LogsFilter.keyId}'`;
            this.RecentLogs = result.Results.map(log => this.mapLogToItem(log));
     * Close logs panel
    public closeLogsPanel(): void {
        this.ShowLogsPanel = false;
        this.LogsFilter = {};
        if (statusCode >= 300 && statusCode < 400) return 'status-info';
     * Get method badge class
    public getMethodClass(method: string): string {
        const m = method.toUpperCase();
        if (m === 'GET') return 'method-get';
        if (m === 'POST') return 'method-post';
        if (m === 'PUT' || m === 'PATCH') return 'method-put';
        if (m === 'DELETE') return 'method-delete';
        return 'method-other';
        return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit' });
     * Format number with K/M suffix
        if (num >= 1000000) return (num / 1000000).toFixed(1) + 'M';
        if (num >= 1000) return (num / 1000).toFixed(1) + 'K';
