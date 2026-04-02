import { ResourceData, MJCommunicationRunEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseResourceComponent, 'CommunicationRunsResource')
    selector: 'mj-communication-runs-resource',
    <div class="runs-wrapper">
          <h3><i class="fa-solid fa-play-circle"></i> Bulk Communication Runs</h3>
          <!-- SUMMARY STATS -->
          <div class="runs-summary">
            <div class="run-stat-card info">
              <div class="run-stat-value">{{summary.active}}</div>
              <div class="run-stat-label">Active Runs</div>
            <div class="run-stat-card success">
              <div class="run-stat-value">{{summary.completed}}</div>
              <div class="run-stat-label">Completed (24h)</div>
            <div class="run-stat-card neutral">
              <div class="run-stat-value">{{summary.successRate}}%</div>
              <div class="run-stat-label">Success Rate</div>
          <!-- TIMELINE -->
          <div class="run-timeline">
            @for (run of runs; track run) {
              <div class="run-entry">
                <div class="run-timeline-dot" [ngClass]="getRunDotClass(run.Status)"></div>
                <div class="run-entry-content">
                  <div class="run-entry-header">
                    <span class="run-entry-title">Run #{{run.ID.substring(0, 8)}}</span>
                    <span class="run-status-badge" [ngClass]="getStatusClass(run.Status)">
                      {{run.Status}}
                  <div class="run-entry-meta">
                    <span><i class="fa-solid fa-user"></i> {{run.User || 'System'}}</span>
                    <span><i class="fa-solid fa-clock"></i> {{run.StartedAt | date:'medium'}}</span>
                    @if (run.EndedAt) {
                      <span><i class="fa-solid fa-flag-checkered"></i> {{run.EndedAt | date:'shortTime'}}</span>
                  @if (run.Comments) {
                    <div class="run-entry-comments">
                      {{run.Comments}}
            @if (runs.length === 0 && !isLoading) {
                <p>No communication runs found</p>
    .runs-wrapper {
        color: var(--mat-sys-on-surface-variant); font-size: 12px;
    .header-actions { display: flex; gap: 8px; }
    /* SUMMARY */
    .runs-summary {
    .run-stat-card {
    .run-stat-card.info { background: #ddf4ff; }
    .run-stat-card.success { background: #d4f8e0; }
    .run-stat-card.neutral { background: var(--mat-sys-surface-container-low); }
    .run-stat-value {
        font-size: 24px; font-weight: 800;
    .run-stat-card.info .run-stat-value { color: #0969da; }
    .run-stat-card.success .run-stat-value { color: #1b873f; }
    .run-stat-label {
    /* TIMELINE */
    .run-timeline { padding: 8px 20px 20px; }
    .run-entry {
        display: flex; gap: 16px;
    .run-entry:last-child { border-bottom: none; }
    .run-timeline-dot {
        width: 12px; height: 12px;
        margin-top: 4px; flex-shrink: 0;
    .run-timeline-dot.complete { background: #1b873f; }
    .run-timeline-dot.failed { background: #cf222e; }
    .run-timeline-dot.in-progress {
        background: #0969da;
        animation: pulse-dot 1.5s ease-in-out infinite;
    .run-timeline-dot.pending { background: #9a6700; }
    @keyframes pulse-dot {
        0%, 100% { box-shadow: 0 0 0 0 rgba(9,105,218,0.4); }
        50% { box-shadow: 0 0 0 6px rgba(9,105,218,0); }
    .run-entry-content { flex: 1; }
    .run-entry-header {
    .run-entry-title {
        font-size: 13px; font-weight: 600;
    .run-status-badge {
    .run-status-badge.complete { background: #d4f8e0; color: #1b873f; }
    .run-status-badge.failed { background: #ffdce0; color: #cf222e; }
    .run-status-badge.pending { background: #fff0c7; color: #9a6700; }
    .run-status-badge.in-progress { background: #ddf4ff; color: #0969da; }
    .run-entry-meta {
    .run-entry-meta span {
        display: flex; align-items: center; gap: 4px;
    .run-entry-meta i { font-size: 10px; }
    .run-entry-comments {
export class CommunicationRunsResourceComponent extends BaseResourceComponent implements OnInit, OnDestroy {
    public runs: MJCommunicationRunEntity[] = [];
    public summary = {
        completed: 0,
        successRate: 0
            const [runsResult, activeResult, completedResult, failedResult] = await Promise.all([
                rv.RunView<MJCommunicationRunEntity>({
                    EntityName: 'MJ: Communication Runs',
                    ExtraFilter: `Status = 'In-Progress'`,
                    ExtraFilter: `EndedAt >= '${yesterdayIso}' AND Status = 'Complete'`,
                    ExtraFilter: `EndedAt >= '${yesterdayIso}' AND Status = 'Failed'`,
            if (runsResult.Success) {
                this.runs = runsResult.Results;
            if (activeResult.Success) this.summary.active = activeResult.TotalRowCount;
            if (completedResult.Success) this.summary.completed = completedResult.TotalRowCount;
            const totalCompleted = this.summary.completed + (failedResult.Success ? failedResult.TotalRowCount : 0);
            this.summary.successRate = totalCompleted > 0
                ? Math.round((this.summary.completed / totalCompleted) * 100)
    public getRunDotClass(status: string): string {
        return 'pending';
        return 'Bulk Runs';
        return 'fa-solid fa-play-circle';
