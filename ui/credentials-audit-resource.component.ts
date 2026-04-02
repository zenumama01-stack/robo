import { ResourceData, MJAuditLogEntity } from '@memberjunction/core-entities';
// The Credential Access AuditLogType ID from metadata
const CREDENTIAL_ACCESS_AUDIT_LOG_TYPE_ID = 'E8D4D100-E785-42D3-997F-ECFF3B0BCFC0';
interface AuditLogWithDetails extends MJAuditLogEntity {
    parsedDetails?: ParsedDetails;
interface ParsedDetails {
    operation?: string;
    subsystem?: string;
    credentialType?: string;
    ipAddress?: string;
interface TimelineGroup {
    displayDate: string;
    logs: AuditLogWithDetails[];
@RegisterClass(BaseResourceComponent, 'CredentialsAuditResource')
    selector: 'mj-credentials-audit-resource',
    templateUrl: './credentials-audit-resource.component.html',
    styleUrls: ['./credentials-audit-resource.component.css'],
export class CredentialsAuditResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    public auditLogs: AuditLogWithDetails[] = [];
    public filteredLogs: AuditLogWithDetails[] = [];
    public timelineGroups: TimelineGroup[] = [];
    public selectedStatus = '';
    public selectedOperation = '';
    public dateRange = '7'; // days
    public searchText = '';
    public viewMode: 'table' | 'timeline' = 'timeline';
    public expandedLogId: string | null = null;
    public hourlyData: { hour: string; success: number; failed: number }[] = [];
    public operationCounts: Map<string, number> = new Map();
        // Cleanup if needed
        return 'Audit Trail';
        return 'fa-solid fa-clipboard-list';
            // Calculate date filter
            startDate.setDate(startDate.getDate() - parseInt(this.dateRange, 10));
            const dateFilter = `AuditLogTypeID = '${CREDENTIAL_ACCESS_AUDIT_LOG_TYPE_ID}' AND __mj_CreatedAt >= '${startDate.toISOString()}'`;
            const result = await rv.RunView<AuditLogWithDetails>({
                EntityName: 'MJ: Audit Logs',
                this.auditLogs = result.Results;
                this.parseAllDetails();
                this.buildChartData();
            console.error('Error loading audit logs:', error);
    private parseAllDetails(): void {
        for (const log of this.auditLogs) {
            log.parsedDetails = this.parseDetails(log);
    private parseDetails(log: MJAuditLogEntity): ParsedDetails {
            if (log.Details) {
                return JSON.parse(log.Details);
    public onOperationFilterChange(operation: string): void {
        this.selectedOperation = operation;
    public onDateRangeChange(days: string): void {
        this.dateRange = days;
        this.searchText = value;
        this.searchText = '';
    public setViewMode(mode: 'table' | 'timeline'): void {
    public toggleLogExpand(logId: string): void {
        this.expandedLogId = this.expandedLogId === logId ? null : logId;
        let filtered = [...this.auditLogs];
        if (this.selectedStatus) {
            filtered = filtered.filter(log => log.Status === this.selectedStatus);
        if (this.selectedOperation) {
            filtered = filtered.filter(log => {
                const op = log.parsedDetails?.operation || 'Access';
                return op === this.selectedOperation;
        if (this.searchText) {
                const user = (log.User || '').toLowerCase();
                const desc = (log.Description || '').toLowerCase();
                const subsystem = (log.parsedDetails?.subsystem || '').toLowerCase();
                const credType = (log.parsedDetails?.credentialType || '').toLowerCase();
                return user.includes(searchLower) ||
                    desc.includes(searchLower) ||
                    subsystem.includes(searchLower) ||
                    credType.includes(searchLower);
        this.buildTimelineGroups();
    private buildTimelineGroups(): void {
        const groups = new Map<string, AuditLogWithDetails[]>();
        for (const log of this.filteredLogs) {
            const date = new Date(log.__mj_CreatedAt);
            const dateKey = date.toISOString().split('T')[0];
            if (!groups.has(dateKey)) {
                groups.set(dateKey, []);
            groups.get(dateKey)!.push(log);
        this.timelineGroups = Array.from(groups.entries())
            .sort((a, b) => b[0].localeCompare(a[0]))
            .map(([date, logs]) => ({
                displayDate: this.formatGroupDate(date),
                logs
    private formatGroupDate(dateString: string): string {
        const yesterday = new Date(today);
        if (date.toDateString() === today.toDateString()) {
            return 'Today';
        } else if (date.toDateString() === yesterday.toDateString()) {
            return 'Yesterday';
                year: date.getFullYear() !== today.getFullYear() ? 'numeric' : undefined
    private buildChartData(): void {
        // Build hourly distribution for today
        const hourCounts: { [key: string]: { success: number; failed: number } } = {};
        const today = new Date().toDateString();
            const hour = i.toString().padStart(2, '0') + ':00';
            hourCounts[hour] = { success: 0, failed: 0 };
            if (date.toDateString() === today) {
                const hour = date.getHours().toString().padStart(2, '0') + ':00';
                if (log.Status === 'Success') {
                    hourCounts[hour].success++;
                    hourCounts[hour].failed++;
        this.hourlyData = Object.entries(hourCounts).map(([hour, counts]) => ({
            hour,
            ...counts
        // Build operation counts
        this.operationCounts.clear();
            this.operationCounts.set(op, (this.operationCounts.get(op) || 0) + 1);
    public getMaxHourlyCount(): number {
        return Math.max(...this.hourlyData.map(d => d.success + d.failed), 1);
    public getOperationList(): string[] {
        return Array.from(this.operationCounts.keys());
            case 'Success': return 'success';
            case 'Failed': return 'failed';
            default: return 'unknown';
    public getOperationType(log: AuditLogWithDetails): string {
        return log.parsedDetails?.operation || 'Access';
    public getSubsystem(log: AuditLogWithDetails): string {
        return log.parsedDetails?.subsystem || '';
    public getOperationIcon(operation: string): string {
            case 'access': return 'fa-solid fa-eye';
            case 'create': return 'fa-solid fa-plus';
            case 'update': return 'fa-solid fa-pen';
            case 'delete': return 'fa-solid fa-trash';
            case 'rotate': return 'fa-solid fa-rotate';
            case 'validate': return 'fa-solid fa-check-circle';
    public getOperationColor(operation: string): string {
            case 'access': return '#6366f1';
            case 'create': return '#10b981';
            case 'update': return '#f59e0b';
            case 'delete': return '#ef4444';
            case 'rotate': return '#8b5cf6';
            case 'validate': return '#06b6d4';
    public formatDate(date: Date | string | null): string {
        return d.toLocaleDateString('en-US', {
            minute: '2-digit',
            second: '2-digit'
    public formatTime(date: Date | string | null): string {
        return d.toLocaleTimeString('en-US', {
    public formatDuration(ms: number | undefined): string {
        return `${(ms / 1000).toFixed(2)}s`;
    public refresh(): void {
    public getSuccessCount(): number {
        return this.auditLogs.filter(log => log.Status === 'Success').length;
    public getFailedCount(): number {
        return this.auditLogs.filter(log => log.Status === 'Failed').length;
        if (this.auditLogs.length === 0) return 0;
        return Math.round((this.getSuccessCount() / this.auditLogs.length) * 100);
    public getUniqueUserCount(): number {
        const users = new Set(this.auditLogs.map(log => log.User).filter(Boolean));
        return users.size;
    public exportToCSV(): void {
        const headers = ['Timestamp', 'User', 'Operation', 'Status', 'Description', 'Subsystem', 'Credential Type'];
        const rows = this.filteredLogs.map(log => [
            this.formatDate(log.__mj_CreatedAt),
            log.User || '',
            this.getOperationType(log),
            log.Status || '',
            log.Description || '',
            this.getSubsystem(log),
            log.parsedDetails?.credentialType || ''
            ...rows.map(row => row.map(cell => `"${cell.replace(/"/g, '""')}"`).join(','))
        link.download = `credential-audit-log-${new Date().toISOString().split('T')[0]}.csv`;
