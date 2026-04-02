import { ResourceData, MJCommunicationLogEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseResourceComponent, 'CommunicationLogsResource')
    selector: 'mj-communication-logs-resource',
    <div class="logs-wrapper">
        <div class="logs-toolbar">
          <div class="search-input-wrapper">
            <input type="text" placeholder="Search messages, providers, recipients..." (input)="onSearch($event)">
          <div class="filter-chip" [class.active]="statusFilter === ''"
            (click)="onStatusFilter('')">
            <i class="fa-solid fa-filter"></i> All
          <div class="filter-chip" [class.active]="statusFilter === 'Complete'"
            (click)="onStatusFilter('Complete')">
            <i class="fa-solid fa-check-circle"></i> Sent
          <div class="filter-chip" [class.active]="statusFilter === 'Failed'"
            (click)="onStatusFilter('Failed')">
            <i class="fa-solid fa-times-circle"></i> Failed
          <div class="filter-chip" [class.active]="statusFilter === 'Pending'"
            (click)="onStatusFilter('Pending')">
            <i class="fa-solid fa-clock"></i> Pending
          <button class="tb-btn" (click)="loadData()">
            <i class="fa-solid fa-rotate" [class.spinning]="isLoading"></i> Refresh
          <table class="log-table">
                <th>Direction</th>
                <th>Provider</th>
                <th>Error</th>
              @for (log of filteredLogs; track log) {
                    <span class="log-status-badge" [ngClass]="getStatusClass(log.Status)">
                      <i [class]="getStatusIcon(log.Status)"></i>
                      {{log.Status}}
                    <span class="log-direction" [ngClass]="log.Direction.toLowerCase()">
                      <i [class]="log.Direction === 'Sending' ? 'fa-solid fa-arrow-up-right' : 'fa-solid fa-arrow-down-left'"></i>
                      {{log.Direction}}
                    <span class="log-provider-badge">
                      <i [class]="getProviderIcon(log.CommunicationProvider)" [style.color]="getProviderColor(log.CommunicationProvider)"></i>
                      {{log.CommunicationProvider}}
                  <td>{{log.CommunicationProviderMessageType}}</td>
                  <td>{{log.MessageDate | date:'medium'}}</td>
                    @if (log.ErrorMessage) {
                      <span class="log-error-text" [title]="log.ErrorMessage">
                        {{log.ErrorMessage}}
                    @if (!log.ErrorMessage) {
                      <span class="no-error">&mdash;</span>
              @if (filteredLogs.length === 0 && !isLoading) {
                  <td colspan="6" class="no-data">
                      <p>No logs found matching your criteria</p>
    .logs-wrapper {
        border-radius: var(--mat-sys-corner-medium, 12px);
    /* TOOLBAR */
    .logs-toolbar {
        background: var(--mat-sys-surface-container-low);
    .search-input-wrapper {
        transition: border-color 0.15s, box-shadow 0.15s;
    .search-input-wrapper:focus-within {
        border-color: var(--mat-sys-primary);
        box-shadow: 0 0 0 2px color-mix(in srgb, var(--mat-sys-primary) 15%, transparent);
    .search-input-wrapper i { color: var(--mat-sys-on-surface-variant); font-size: 12px; }
    .search-input-wrapper input {
        flex: 1; border: none; outline: none;
        background: transparent; font-size: 12px;
        font-family: inherit; color: var(--mat-sys-on-surface);
    .search-input-wrapper input::placeholder { color: var(--mat-sys-on-surface-variant); }
    .filter-chip {
        display: inline-flex; align-items: center;
        gap: 4px; padding: 4px 10px;
        font-size: 11px; font-weight: 500;
        cursor: pointer; transition: all 0.15s;
    .filter-chip:hover {
    .filter-chip.active {
    .filter-chip i { font-size: 10px; }
    .toolbar-spacer { flex: 1; }
        gap: 6px; padding: 6px 12px;
        font-size: 12px; font-weight: 500;
        cursor: pointer; transition: all 0.15s ease;
    .tb-btn i { font-size: 12px; }
    @keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
    .spinning { animation: spin 1s linear infinite; }
    /* TABLE */
    .table-wrapper { flex: 1; overflow-y: auto; }
    .log-table {
    .log-table thead {
        position: sticky; top: 0; z-index: 1;
    .log-table th {
        font-weight: 700; font-size: 10px;
        text-transform: uppercase; letter-spacing: 0.5px;
    .log-table td {
        border-bottom: 1px solid var(--mat-sys-surface-container);
    .log-table tbody tr { transition: background 0.15s; }
    .log-table tbody tr:hover { background: var(--mat-sys-surface-container-low); }
    .log-status-badge {
        gap: 4px; font-size: 11px; font-weight: 600;
    .log-status-badge.complete { background: #d4f8e0; color: #1b873f; }
    .log-status-badge.failed { background: #ffdce0; color: #cf222e; }
    .log-status-badge.pending { background: #fff0c7; color: #9a6700; }
    .log-status-badge.in-progress { background: #ddf4ff; color: #0969da; }
    .log-direction {
        display: flex; align-items: center;
        gap: 4px; font-size: 11px;
    .log-direction i { font-size: 10px; }
    .log-direction.sending i { color: var(--mat-sys-primary); }
    .log-direction.receiving i { color: #1b873f; }
    .log-provider-badge {
        gap: 6px; padding: 3px 10px;
    .log-error-text {
        color: #cf222e;
        overflow: hidden; text-overflow: ellipsis;
        white-space: nowrap; font-size: 11px;
    .no-error { color: var(--mat-sys-on-surface-variant); }
    .no-data { padding: 0 !important; }
        display: flex; flex-direction: column;
        align-items: center; justify-content: center;
        padding: 48px 0; color: var(--mat-sys-on-surface-variant);
    .empty-state i { font-size: 2rem; margin-bottom: 12px; opacity: 0.5; }
    .empty-state p { margin: 0; font-size: 13px; }
export class CommunicationLogsResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    public logs: MJCommunicationLogEntity[] = [];
    public filteredLogs: MJCommunicationLogEntity[] = [];
    public statusFilter = '';
    private searchTerm = '';
    ngOnDestroy(): void { }
            const result = await rv.RunView<MJCommunicationLogEntity>({
                EntityName: 'MJ: Communication Logs',
                OrderBy: 'MessageDate DESC',
                this.logs = result.Results;
            console.error('Error loading logs:', error);
    public onSearch(event: Event): void {
        this.searchTerm = (event.target as HTMLInputElement).value.toLowerCase();
    public onStatusFilter(status: string): void {
        this.statusFilter = status;
        const s = (status || '').toLowerCase();
        if (s === 'complete') return 'complete';
        if (s === 'failed') return 'failed';
        if (s === 'pending') return 'pending';
        if (s === 'in-progress') return 'in-progress';
        if (s === 'complete') return 'fa-solid fa-check';
        if (s === 'failed') return 'fa-solid fa-xmark';
        if (s === 'pending') return 'fa-solid fa-clock';
        if (s === 'in-progress') return 'fa-solid fa-spinner';
        return 'fa-solid fa-circle';
    public getProviderIcon(name: string): string {
        if (!name) return 'fa-solid fa-server';
        const n = name.toLowerCase();
        if (n.includes('sendgrid')) return 'fa-solid fa-envelope';
        if (n.includes('twilio')) return 'fa-solid fa-comment-sms';
        if (n.includes('gmail') || n.includes('google')) return 'fa-brands fa-google';
        if (n.includes('microsoft') || n.includes('graph')) return 'fa-brands fa-microsoft';
        return 'fa-solid fa-server';
    public getProviderColor(name: string): string {
        if (!name) return 'inherit';
        if (n.includes('sendgrid')) return '#1A82E2';
        if (n.includes('twilio')) return '#F22F46';
        if (n.includes('gmail') || n.includes('google')) return '#EA4335';
        if (n.includes('microsoft') || n.includes('graph')) return '#0078D4';
        return 'inherit';
        let filtered = this.logs;
        if (this.statusFilter) {
            filtered = filtered.filter(l => l.Status === this.statusFilter);
            filtered = filtered.filter(l =>
                l.CommunicationProvider?.toLowerCase().includes(this.searchTerm) ||
                l.CommunicationProviderMessageType?.toLowerCase().includes(this.searchTerm) ||
                l.Status?.toLowerCase().includes(this.searchTerm) ||
                l.ErrorMessage?.toLowerCase().includes(this.searchTerm)
        this.filteredLogs = filtered;
        return 'Message Logs';
        return 'fa-solid fa-list-ul';
