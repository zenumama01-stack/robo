 * Tabular list view of agent steps and paths.
 * Alternative to the visual flow canvas — useful for quick overview and bulk editing.
  selector: 'mj-agent-step-list',
    <div class="mj-step-list">
      <!-- Steps Section -->
      <div class="mj-step-list-section">
        <div class="mj-step-list-section-header">
          <h4><i class="fa-solid fa-circle-nodes"></i> Steps ({{ Steps.length }})</h4>
        <div class="mj-step-list-table-wrap">
          @if (Steps.length > 0) {
            <table class="mj-step-list-table">
                  <th>Start</th>
                  <th>Configured</th>
                @for (step of Steps; track step) {
                    [class.mj-step-list-row--selected]="SelectedStepID === step.ID"
                    (click)="StepClicked.emit(step)">
                    <td class="mj-step-list-name">{{ step.Name }}</td>
                      <span class="mj-step-list-type-badge" [attr.data-type]="step.StepType">
                        {{ step.StepType }}
                      <span class="mj-step-list-status" [attr.data-status]="step.Status">
                        {{ step.Status }}
                      @if (step.StartingStep) {
                        <i class="fa-solid fa-check mj-step-list-check"></i>
                    <td class="mj-step-list-configured">{{ getConfiguredItem(step) }}</td>
          @if (Steps.length === 0) {
            <div class="mj-step-list-empty">
              No steps defined
      <!-- Paths Section -->
          <h4><i class="fa-solid fa-link"></i> Paths ({{ Paths.length }})</h4>
          @if (Paths.length > 0) {
                  <th>From</th>
                  <th>To</th>
                  <th>Condition</th>
                  <th>Priority</th>
                @for (path of Paths; track path) {
                    <td>{{ path.OriginStep }}</td>
                    <td>{{ path.DestinationStep }}</td>
                    <td class="mj-step-list-condition">{{ path.Condition || '(always)' }}</td>
                    <td>{{ path.Priority }}</td>
          @if (Paths.length === 0) {
              No paths defined
    .mj-step-list {
    .mj-step-list-section {
    .mj-step-list-section-header h4 {
      i { font-size: 13px; color: #64748b; }
    .mj-step-list-table-wrap {
    .mj-step-list-table {
      tbody tr {
        &:hover { background: #f8fafc; }
        &:last-child td { border-bottom: none; }
    .mj-step-list-row--selected {
      background: #eff6ff !important;
    .mj-step-list-name { font-weight: 500; }
    .mj-step-list-type-badge {
    .mj-step-list-status {
      &[data-status="Active"] { color: #10b981; }
      &[data-status="Disabled"] { color: #94a3b8; }
      &[data-status="Pending"] { color: #f59e0b; }
    .mj-step-list-check { color: #10b981; font-size: 12px; }
    .mj-step-list-configured {
    .mj-step-list-condition {
      font-family: 'SF Mono', monospace;
    .mj-step-list-empty {
export class AgentStepListComponent {
  @Input() Steps: MJAIAgentStepEntity[] = [];
  @Input() Paths: MJAIAgentStepPathEntity[] = [];
  @Input() SelectedStepID: string | null = null;
  @Output() StepClicked = new EventEmitter<MJAIAgentStepEntity>();
  getConfiguredItem(step: MJAIAgentStepEntity): string {
      case 'Action': return step.Action || '—';
      case 'Prompt': return step.Prompt || '—';
      case 'Sub-Agent': return step.SubAgent || '—';
      case 'While': return `${step.LoopBodyType ?? 'Action'} loop`;
      default: return '—';
