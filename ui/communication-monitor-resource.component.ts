import { ResourceData, MJCommunicationLogEntity, MJCommunicationProviderEntity } from '@memberjunction/core-entities';
interface ProviderHealth {
    SentCount: number;
    SuccessRate: number;
    IconClass: string;
    ColorClass: string;
interface ChannelBreakdown {
    Count: number;
    Percentage: number;
interface HourlyBucket {
@RegisterClass(BaseResourceComponent, 'CommunicationMonitorResource')
    selector: 'mj-communication-monitor-resource',
    <div class="monitor-wrapper">
      <div class="monitor-container">
        <!-- KPI STRIP -->
        <div class="kpi-strip">
          <div class="kpi-card sent">
            <div class="kpi-icon"><i class="fa-solid fa-paper-plane"></i></div>
            <div class="kpi-body">
              <span class="kpi-label">Total Sent</span>
              <span class="kpi-value">{{stats.totalSent | number}}</span>
              <span class="kpi-delta" [class]="stats.totalSent > 0 ? 'up' : 'neutral'">
                <i class="fa-solid" [class.fa-arrow-up]="stats.totalSent > 0" [class.fa-minus]="stats.totalSent === 0"></i>
                Last 24 hours
          <div class="kpi-card delivered">
            <div class="kpi-icon"><i class="fa-solid fa-check-double"></i></div>
              <span class="kpi-label">Delivery Rate</span>
              <span class="kpi-value">{{stats.deliveryRate}}%</span>
              <div class="delivery-bar">
                <div class="delivery-fill" [style.width.%]="stats.deliveryRate"></div>
          <div class="kpi-card pending">
            <div class="kpi-icon"><i class="fa-solid fa-clock"></i></div>
              <span class="kpi-label">Pending</span>
              <span class="kpi-value">{{stats.pending | number}}</span>
              <span class="kpi-delta neutral">
                <i class="fa-solid fa-minus"></i> Awaiting provider
          <div class="kpi-card failed">
            <div class="kpi-icon"><i class="fa-solid fa-circle-exclamation"></i></div>
              <span class="kpi-label">Failed</span>
              <span class="kpi-value">{{stats.failed | number}}</span>
              <span class="kpi-delta" [class]="stats.failed > 0 ? 'down' : 'neutral'">
                <i class="fa-solid" [class.fa-arrow-up]="stats.failed > 0" [class.fa-minus]="stats.failed === 0"></i>
                {{stats.failed > 0 ? 'Requires attention' : 'No failures'}}
        <!-- CHARTS + ACTIVITY ROW -->
              <h3><i class="fa-solid fa-chart-bar"></i> Delivery Volume</h3>
            <div class="chart-container-inner">
                [data]="chartData"
                [showLegend]="true"
                [showControls]="false"
                [config]="chartConfig">
              </app-time-series-chart>
              <h3><i class="fa-solid fa-bolt"></i> Recent Activity</h3>
            <div class="card-body no-padding">
              <div class="activity-feed">
                @for (log of recentLogs; track log) {
                  <div class="activity-item">
                    <div class="activity-icon" [ngClass]="getActivityIconClass(log)">
                      <i [class]="getActivityIcon(log)"></i>
                    <div class="activity-body">
                      <span class="activity-title">{{log.CommunicationProviderMessageType || 'Message'}}</span>
                      <span class="activity-meta">{{log.CommunicationProvider}} &bull; {{log.MessageDate | date:'shortTime'}}</span>
                    <span class="activity-status" [ngClass]="log.Status.toLowerCase()">
                @if (recentLogs.length === 0) {
        <!-- PROVIDER HEALTH + CHANNEL BREAKDOWN -->
              <h3><i class="fa-solid fa-heart-pulse"></i> Provider Health</h3>
              <div class="provider-health-list">
                @for (provider of providerHealth; track provider) {
                  <div class="provider-row">
                    <div class="provider-status-dot" [class.active]="provider.IsActive"></div>
                    <div class="provider-logo" [ngClass]="provider.ColorClass">
                      <i [class]="provider.IconClass"></i>
                    <div class="provider-info">
                      <div class="provider-name">{{provider.Name}}</div>
                      <div class="provider-type">{{provider.Type}} &bull; {{provider.SentCount}} sent today</div>
                    <div class="provider-health-bar">
                      <div class="provider-health-fill" [ngClass]="getHealthClass(provider.SuccessRate)" [style.width.%]="provider.SuccessRate"></div>
                    <span class="provider-rate" [ngClass]="getHealthClass(provider.SuccessRate)">{{provider.SuccessRate}}%</span>
                @if (providerHealth.length === 0) {
                    <p>No providers configured</p>
              <h3><i class="fa-solid fa-layer-group"></i> Channel Breakdown</h3>
              <div class="channel-breakdown">
                @for (channel of channelBreakdown; track channel) {
                  <div class="channel-row">
                    <div class="channel-icon" [ngClass]="channel.ColorClass">
                      <i [class]="channel.IconClass"></i>
                    <div class="channel-info">
                      <div class="channel-name">{{channel.Name}}</div>
                      <div class="channel-count">{{channel.Count | number}} messages</div>
                    <div class="channel-bar-wrapper">
                      <div class="channel-bar-fill" [style.width.%]="channel.Percentage" [style.background]="channel.ColorClass === 'email' ? 'var(--mat-sys-primary)' : '#2e7d32'"></div>
                    <span class="channel-pct">{{channel.Percentage}}%</span>
                @if (channelBreakdown.length === 0) {
                    <p>No channel data available</p>
    /* MD3 MONITOR RESOURCE                                        */
    .monitor-wrapper {
    .monitor-container {
    /* KPI STRIP */
    .kpi-strip {
        box-shadow: var(--mat-sys-elevation-1);
    .kpi-card::before {
        top: 0; left: 0; right: 0;
        height: 3px;
    .kpi-card.sent::before { background: var(--mat-sys-primary); }
    .kpi-card.delivered::before { background: #1b873f; }
    .kpi-card.pending::before { background: #9a6700; }
    .kpi-card.failed::before { background: #cf222e; }
        width: 44px; height: 44px;
        font-size: 16px; flex-shrink: 0;
    .kpi-card.sent .kpi-icon { background: var(--mat-sys-primary-container); color: var(--mat-sys-primary); }
    .kpi-card.delivered .kpi-icon { background: #d4f8e0; color: #1b873f; }
    .kpi-card.pending .kpi-icon { background: #fff0c7; color: #9a6700; }
    .kpi-card.failed .kpi-icon { background: #ffdce0; color: #cf222e; }
    .kpi-body { flex: 1; display: flex; flex-direction: column; gap: 2px; }
        font-size: 11px; font-weight: 600;
        font-size: 28px; font-weight: 800;
        letter-spacing: -0.02em; line-height: 1.1;
    .kpi-delta {
        margin-top: 4px; padding: 2px 8px;
        border-radius: 10px; width: fit-content;
    .kpi-delta.up { background: #d4f8e0; color: #1b873f; }
    .kpi-delta.down { background: #ffdce0; color: #cf222e; }
    .kpi-delta.neutral { background: var(--mat-sys-surface-container); color: var(--mat-sys-on-surface-variant); }
    .delivery-bar {
        height: 6px; margin-top: 10px;
        border-radius: 3px; overflow: hidden;
    .delivery-fill {
        height: 100%; border-radius: 3px;
        background: #1b873f; transition: width 0.6s ease;
    /* CONTENT GRID */
        grid-template-columns: 1.6fr 1fr;
        padding: 16px 20px 12px;
        font-size: 13px; font-weight: 700;
        display: flex; align-items: center; gap: 8px;
    .card-body { padding: 16px 20px; }
    .card-body.no-padding { padding: 0; }
    .chart-container-inner {
    /* ACTIVITY FEED */
    .activity-feed { max-height: 370px; overflow-y: auto; }
        display: flex; align-items: center; gap: 12px;
        transition: background 0.15s ease; cursor: pointer;
    .activity-item:last-child { border-bottom: none; }
    .activity-item:hover { background: var(--mat-sys-surface-container-low); }
        width: 34px; height: 34px;
        font-size: 12px; flex-shrink: 0;
    .activity-icon.email { background: var(--mat-sys-primary-container); color: var(--mat-sys-primary); }
    .activity-icon.sms { background: #e8f5e9; color: #2e7d32; }
    .activity-icon.error { background: #ffdce0; color: #cf222e; }
    .activity-body { flex: 1; min-width: 0; }
        font-size: 12px; font-weight: 600;
        white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
        font-size: 11px; color: var(--mat-sys-on-surface-variant); margin-top: 1px;
    .activity-status {
        font-size: 10px; font-weight: 700;
        text-transform: uppercase; letter-spacing: 0.3px;
    .activity-status.complete { background: #d4f8e0; color: #1b873f; }
    .activity-status.failed { background: #ffdce0; color: #cf222e; }
    .activity-status.pending { background: #fff0c7; color: #9a6700; }
    /* PROVIDER HEALTH */
    .provider-health-list { display: flex; flex-direction: column; }
    .provider-row {
    .provider-row:last-child { border-bottom: none; }
    .provider-row:hover { background: var(--mat-sys-surface-container-low); }
    .provider-status-dot {
        width: 8px; height: 8px; border-radius: 50%; flex-shrink: 0;
        background: var(--mat-sys-outline);
    .provider-status-dot.active { background: #1b873f; }
    .provider-logo {
        width: 36px; height: 36px;
    .provider-logo.sendgrid { color: #1A82E2; }
    .provider-logo.twilio { color: #F22F46; }
    .provider-logo.gmail { color: #EA4335; }
    .provider-logo.msgraph { color: #0078D4; }
    .provider-info { flex: 1; }
    .provider-name { font-size: 13px; font-weight: 600; color: var(--mat-sys-on-surface); }
    .provider-type { font-size: 11px; color: var(--mat-sys-on-surface-variant); }
    .provider-health-bar {
        width: 80px; height: 6px;
    .provider-health-fill {
        transition: width 0.4s ease;
    .provider-health-fill.excellent { background: #1b873f; }
    .provider-health-fill.good { background: #66bb6a; }
    .provider-health-fill.warning { background: #9a6700; }
    .provider-health-fill.critical { background: #cf222e; }
    .provider-rate {
        font-size: 12px; font-weight: 700;
        min-width: 44px; text-align: right;
    .provider-rate.excellent { color: #1b873f; }
    .provider-rate.good { color: #66bb6a; }
    .provider-rate.warning { color: #9a6700; }
    .provider-rate.critical { color: #cf222e; }
    /* CHANNEL BREAKDOWN */
    .channel-breakdown { display: flex; flex-direction: column; }
    .channel-row {
    .channel-row:last-child { border-bottom: none; }
    .channel-icon {
        width: 32px; height: 32px;
        font-size: 13px; flex-shrink: 0;
    .channel-icon.email { background: var(--mat-sys-primary-container); color: var(--mat-sys-primary); }
    .channel-icon.sms { background: #e8f5e9; color: #2e7d32; }
    .channel-info { flex: 1; }
    .channel-name { font-size: 12px; font-weight: 600; color: var(--mat-sys-on-surface); }
    .channel-count { font-size: 11px; color: var(--mat-sys-on-surface-variant); }
    .channel-bar-wrapper {
        width: 100px; height: 6px;
    .channel-bar-fill { height: 100%; border-radius: 3px; }
    .channel-pct {
        min-width: 36px; text-align: right;
    /* EMPTY STATE */
        padding: 40px 0; color: var(--mat-sys-on-surface-variant);
        .kpi-strip { grid-template-columns: repeat(2, 1fr); }
        .content-grid { grid-template-columns: 1fr; }
export class CommunicationMonitorResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    public stats = {
        totalSent: 0,
        deliveryRate: 0,
        failed: 0
    public recentLogs: MJCommunicationLogEntity[] = [];
    public chartData: HourlyBucket[] = [];
    public chartConfig = {
        useDualAxis: false,
        colors: ['#4f6bed', '#cf222e']
    public providerHealth: ProviderHealth[] = [];
    public channelBreakdown: ChannelBreakdown[] = [];
            const yesterday = new Date();
            yesterday.setDate(yesterday.getDate() - 1);
            const yesterdayIso = yesterday.toISOString();
            const [totalResult, failedResult, pendingResult, recentResult, allLogsResult, providersResult] = await Promise.all([
                rv.RunView({
                    ExtraFilter: `MessageDate >= '${yesterdayIso}'`,
                    ExtraFilter: `MessageDate >= '${yesterdayIso}' AND Status = 'Failed'`,
                    ExtraFilter: `Status = 'Pending'`,
                rv.RunView<MJCommunicationLogEntity>({
                    MaxRows: 8,
                    OrderBy: 'MessageDate ASC',
                rv.RunView<MJCommunicationProviderEntity>({
            if (totalResult.Success) this.stats.totalSent = totalResult.TotalRowCount;
            if (failedResult.Success) this.stats.failed = failedResult.TotalRowCount;
            if (pendingResult.Success) this.stats.pending = pendingResult.TotalRowCount;
            this.stats.deliveryRate = this.stats.totalSent > 0
                ? parseFloat(((this.stats.totalSent - this.stats.failed) / this.stats.totalSent * 100).toFixed(1))
            if (recentResult.Success) {
                this.recentLogs = recentResult.Results;
            if (allLogsResult.Success) {
                this.chartData = this.processTrendData(allLogsResult.Results, yesterday);
                this.channelBreakdown = this.buildChannelBreakdown(allLogsResult.Results);
            if (providersResult.Success && allLogsResult.Success) {
                this.providerHealth = this.buildProviderHealth(providersResult.Results, allLogsResult.Results);
            console.error('Error loading monitor data:', error);
    public getActivityIconClass(log: MJCommunicationLogEntity): string {
        if (log.Status === 'Failed') return 'error';
        const type = (log.CommunicationProviderMessageType || '').toLowerCase();
        if (type.includes('sms')) return 'sms';
        return 'email';
    public getActivityIcon(log: MJCommunicationLogEntity): string {
        if (log.Direction === 'Receiving') return 'fa-solid fa-arrow-down';
        if (type.includes('sms')) return 'fa-solid fa-comment-sms';
        return 'fa-solid fa-envelope';
    public getHealthClass(rate: number): string {
        if (rate >= 98) return 'excellent';
        if (rate >= 95) return 'good';
        if (rate >= 85) return 'warning';
        return 'critical';
    private processTrendData(logs: MJCommunicationLogEntity[], startTime: Date): HourlyBucket[] {
        const buckets: HourlyBucket[] = [];
        const current = new Date(startTime);
        while (current <= now) {
            const bucketStart = new Date(current);
            const bucketEnd = new Date(current.getTime() + 60 * 60 * 1000);
                const d = new Date(l.MessageDate);
                return d >= bucketStart && d < bucketEnd;
                timestamp: bucketStart,
                executions: bucketLogs.length,
                errors: bucketLogs.filter(l => l.Status === 'Failed').length,
                avgTime: 0
            current.setHours(current.getHours() + 1);
    private buildProviderHealth(providers: MJCommunicationProviderEntity[], logs: MJCommunicationLogEntity[]): ProviderHealth[] {
        return providers.map(p => {
            const providerLogs = logs.filter(l => l.CommunicationProvider === p.Name);
            const sent = providerLogs.length;
            const failed = providerLogs.filter(l => l.Status === 'Failed').length;
            const rate = sent > 0 ? parseFloat(((sent - failed) / sent * 100).toFixed(1)) : 100;
                Type: this.getProviderType(p.Name),
                SentCount: sent,
                SuccessRate: rate,
                IsActive: p.Status === 'Active',
                IconClass: this.getProviderIconClass(p.Name),
                ColorClass: this.getProviderColorClass(p.Name)
    private buildChannelBreakdown(logs: MJCommunicationLogEntity[]): ChannelBreakdown[] {
        const total = logs.length;
        if (total === 0) return [];
        const emailLogs = logs.filter(l => {
            const type = (l.CommunicationProviderMessageType || '').toLowerCase();
            return type.includes('email') || (!type.includes('sms'));
        const smsLogs = logs.filter(l => {
            return type.includes('sms');
        const channels: ChannelBreakdown[] = [];
        if (emailLogs.length > 0) {
            channels.push({
                IconClass: 'fa-solid fa-envelope',
                Count: emailLogs.length,
                Percentage: Math.round((emailLogs.length / total) * 100),
                ColorClass: 'email'
        if (smsLogs.length > 0) {
                Name: 'SMS',
                IconClass: 'fa-solid fa-comment-sms',
                Count: smsLogs.length,
                Percentage: Math.round((smsLogs.length / total) * 100),
                ColorClass: 'sms'
        return channels;
    private getProviderType(name: string): string {
        if (n.includes('twilio')) return 'SMS';
        return 'Email';
    private getProviderIconClass(name: string): string {
        if (n.includes('microsoft') || n.includes('graph') || n.includes('outlook')) return 'fa-brands fa-microsoft';
    private getProviderColorClass(name: string): string {
        if (n.includes('sendgrid')) return 'sendgrid';
        if (n.includes('twilio')) return 'twilio';
        if (n.includes('gmail') || n.includes('google')) return 'gmail';
        if (n.includes('microsoft') || n.includes('graph') || n.includes('outlook')) return 'msgraph';
