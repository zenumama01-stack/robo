import { ResourceData, MJCommunicationProviderEntity, MJCommunicationLogEntity } from '@memberjunction/core-entities';
interface ProviderCardData {
    Entity: MJCommunicationProviderEntity;
    FailedCount: number;
    LogoClass: string;
@RegisterClass(BaseResourceComponent, 'CommunicationProvidersResource')
    selector: 'mj-communication-providers-resource',
    <div class="providers-wrapper">
      <div class="providers-header">
          <h2>Communication Providers</h2>
          <p>Manage your messaging service integrations</p>
        <button class="tb-btn primary" (click)="addNewProvider()">
          <i class="fa-solid fa-plus"></i> Add Provider
          <mj-loading text="Loading providers..."></mj-loading>
        <div class="providers-grid">
          @for (card of providerCards; track card) {
            <div class="provider-card" [class.disabled]="card.Entity.Status === 'Disabled'">
              <div class="provider-card-header">
                <div class="provider-card-logo" [ngClass]="card.LogoClass">
                  <i [class]="card.IconClass"></i>
                <div class="provider-card-title">
                  <div class="provider-card-name">{{card.Entity.Name}}</div>
                  <div class="provider-card-desc">{{card.Entity.Description || 'No description'}}</div>
                <span class="provider-card-status" [ngClass]="card.Entity.Status.toLowerCase()">
                  {{card.Entity.Status}}
              <div class="provider-card-body">
                <div class="provider-capabilities">
                  <span class="capability-chip" [class.supported]="card.Entity.SupportsSending" [class.unsupported]="!card.Entity.SupportsSending">
                    <i [class]="card.Entity.SupportsSending ? 'fa-solid fa-check' : 'fa-solid fa-xmark'"></i> Sending
                  <span class="capability-chip" [class.supported]="card.Entity.SupportsReceiving" [class.unsupported]="!card.Entity.SupportsReceiving">
                    <i [class]="card.Entity.SupportsReceiving ? 'fa-solid fa-check' : 'fa-solid fa-xmark'"></i> Receiving
                  @if (card.Entity.SupportsScheduledSending) {
                    <span class="capability-chip supported">
                      <i class="fa-solid fa-check"></i> Scheduled
                  @if (card.Entity.SupportsDrafts) {
                      <i class="fa-solid fa-check"></i> Drafts
                  @if (card.Entity.SupportsForwarding) {
                      <i class="fa-solid fa-check"></i> Forward
                  @if (card.Entity.SupportsReplying) {
                      <i class="fa-solid fa-check"></i> Reply
                <div class="provider-card-stats">
                  <div class="provider-stat">
                    <div class="provider-stat-value">{{card.SentCount}}</div>
                    <div class="provider-stat-label">Sent (24h)</div>
                    <div class="provider-stat-value">{{card.SuccessRate}}%</div>
                    <div class="provider-stat-label">Success</div>
                    <div class="provider-stat-value">{{card.FailedCount}}</div>
                    <div class="provider-stat-label">Failed</div>
              <div class="provider-card-footer">
                <button (click)="configureProvider(card.Entity)">
                  <i class="fa-solid fa-gear"></i> Configure
                <button (click)="viewProviderLogs(card.Entity)">
                  <i class="fa-solid fa-chart-line"></i> Analytics
    .providers-wrapper {
    .providers-header {
    .providers-header h2 {
    .providers-header p {
        margin: 4px 0 0;
        gap: 6px; padding: 8px 16px;
    .tb-btn.primary {
        background: var(--mat-sys-primary);
        color: var(--mat-sys-on-primary, #fff);
    .tb-btn.primary:hover { filter: brightness(1.1); }
        padding: 80px 0;
    /* GRID */
    .providers-grid {
        grid-template-columns: repeat(auto-fill, minmax(340px, 1fr));
    .provider-card {
    .provider-card:hover {
        box-shadow: 0 2px 6px 2px rgba(0,0,0,.08), 0 1px 2px rgba(0,0,0,.04);
    .provider-card.disabled { opacity: 0.65; }
    .provider-card-header {
    .provider-card-logo {
        width: 48px; height: 48px;
        font-size: 22px; flex-shrink: 0;
    .provider-card-logo.sendgrid { background: #E8F4FD; color: #1A82E2; }
    .provider-card-logo.twilio { background: #FEECEE; color: #F22F46; }
    .provider-card-logo.gmail { background: #FEECED; color: #EA4335; }
    .provider-card-logo.msgraph { background: #E6F0FA; color: #0078D4; }
    .provider-card-title { flex: 1; min-width: 0; }
    .provider-card-name {
        font-size: 15px; font-weight: 700;
    .provider-card-desc {
    .provider-card-status {
        padding: 3px 10px; border-radius: 10px;
    .provider-card-status.active { background: #d4f8e0; color: #1b873f; }
    .provider-card-status.disabled { background: var(--mat-sys-surface-container-high); color: var(--mat-sys-on-surface-variant); }
    .provider-card-body { padding: 0 20px 16px; }
    .provider-capabilities {
        display: flex; flex-wrap: wrap;
        gap: 6px; margin-bottom: 16px;
    .capability-chip {
        border-radius: 12px; font-size: 11px; font-weight: 500;
    .capability-chip.supported { background: #d4f8e0; color: #1b873f; }
    .capability-chip.unsupported {
        color: var(--mat-sys-outline);
    .provider-card-stats {
        gap: 12px; padding: 12px;
    .provider-stat { text-align: center; }
    .provider-stat-value {
        font-size: 16px; font-weight: 800;
    .provider-stat-label {
    .provider-card-footer {
    .provider-card-footer button {
        flex: 1; padding: 12px;
        border: none; background: transparent;
        cursor: pointer; transition: background 0.15s;
        justify-content: center; gap: 6px;
    .provider-card-footer button:hover {
    .provider-card-footer button + button {
        border-left: 1px solid var(--mat-sys-outline-variant);
export class CommunicationProvidersResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    public providerCards: ProviderCardData[] = [];
    constructor(private cdr: ChangeDetectorRef, private navService: NavigationService) {
            const [providersResult, logsResult] = await Promise.all([
            if (providersResult.Success) {
                const logs = logsResult.Success ? logsResult.Results : [];
                this.providerCards = providersResult.Results.map(p => this.buildProviderCard(p, logs));
            console.error('Error loading providers:', error);
    private buildProviderCard(provider: MJCommunicationProviderEntity, logs: MJCommunicationLogEntity[]): ProviderCardData {
        const providerLogs = logs.filter(l => l.CommunicationProvider === provider.Name);
            Entity: provider,
            FailedCount: failed,
            IconClass: this.getProviderIcon(provider.Name),
            LogoClass: this.getProviderLogoClass(provider.Name)
    private getProviderIcon(name: string): string {
        if (n.includes('aws') || n.includes('ses')) return 'fa-brands fa-aws';
        if (n.includes('slack')) return 'fa-brands fa-slack';
    private getProviderLogoClass(name: string): string {
    public configureProvider(provider: MJCommunicationProviderEntity): void {
        const pk = new CompositeKey();
        pk.LoadFromEntityInfoAndRecord(new Metadata().Entities.find(e => e.Name === 'MJ: Communication Providers')!, provider);
        this.navService.OpenEntityRecord('MJ: Communication Providers', pk);
    public viewProviderLogs(provider: MJCommunicationProviderEntity): void {
        console.log('View analytics for provider:', provider.Name);
    public addNewProvider(): void {
        this.navService.OpenEntityRecord('MJ: Communication Providers', new CompositeKey());
        return 'Providers';
