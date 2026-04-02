import { AgentStateService, AgentStatus } from '../../services/agent-state.service';
 * Displays active agent indicators in the chat header with status animations
 * Shows multiple agents with avatars, status colors, and click-to-expand functionality
  selector: 'mj-active-agent-indicator',
    @if (activeAgents.length > 0) {
      <div class="active-agents-container">
        <div class="agents-wrapper" [class.expanded]="isExpanded">
          @for (agent of displayAgents; track agent.run.ID) {
            <div class="agent-avatar"
              [class.status-acknowledging]="agent.status === 'acknowledging'"
              [class.status-working]="agent.status === 'working'"
              [class.status-completing]="agent.status === 'completing'"
              [class.status-completed]="agent.status === 'completed'"
              [class.status-error]="agent.status === 'error'"
              [title]="getAgentTooltip(agent)"
              (click)="onAgentClick(agent)">
              <div class="avatar-content">
              @if (agent.status !== 'completed') {
                <div class="status-indicator">
                  <div class="pulse-ring"></div>
              @if (agent.confidence != null) {
                <div class="confidence-badge" [title]="'Confidence: ' + (agent.confidence * 100).toFixed(0) + '%'">
                  {{ (agent.confidence * 100).toFixed(0) }}%
          @if (activeAgents.length > maxVisibleAgents && !isExpanded) {
            <button class="more-agents" (click)="toggleExpanded()" [title]="'Show all ' + activeAgents.length + ' agents'">
              +{{ activeAgents.length - maxVisibleAgents }}
        <button class="panel-toggle" (click)="onTogglePanel()" title="Open agent process panel">
          <span class="agent-count">{{ activeAgents.length }}</span>
    .active-agents-container {
      background-color: #F4F4F4;
    .agents-wrapper {
      transition: max-width 300ms ease;
    .agents-wrapper.expanded {
    .agent-avatar {
    .agent-avatar:hover {
    .avatar-content {
    /* Status-based colors */
    .status-acknowledging .avatar-content {
      background: linear-gradient(135deg, #3B82F6, #2563EB);
    .status-working .avatar-content {
      background: linear-gradient(135deg, #F59E0B, #D97706);
    .status-completing .avatar-content {
      background: linear-gradient(135deg, #10B981, #059669);
    .status-completed .avatar-content {
      background: linear-gradient(135deg, #6B7280, #4B5563);
    .status-error .avatar-content {
      background: linear-gradient(135deg, #EF4444, #DC2626);
    /* Animated status indicator */
    .pulse-ring {
      background: currentColor;
      animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
    .status-acknowledging .pulse-ring {
      background: #3B82F6;
    .status-working .pulse-ring {
      animation: pulse 1.5s cubic-bezier(0.4, 0, 0.6, 1) infinite;
    .status-completing .pulse-ring {
      animation: pulse 1s cubic-bezier(0.4, 0, 0.6, 1) infinite;
        transform: scale(1.5);
    /* Confidence badge */
      bottom: -4px;
      padding: 1px 3px;
      z-index: 3;
    .more-agents {
    .more-agents:hover {
      background: #D1D5DB;
    .panel-toggle {
    .panel-toggle:hover {
      background: #2563EB;
    .agent-count {
export class ActiveAgentIndicatorComponent implements OnInit, OnDestroy {
  @Input() conversationId?: string;
  @Input() maxVisibleAgents: number = 3;
  @Output() togglePanel = new EventEmitter<void>();
  @Output() agentSelected = new EventEmitter<MJAIAgentRunEntity>();
  public activeAgents: Array<{ run: MJAIAgentRunEntity; status: AgentStatus; confidence: number | null }> = [];
  public isExpanded: boolean = false;
  private subscription?: Subscription;
  constructor(private agentStateService: AgentStateService) {}
    // Subscribe to active agents for this conversation
    this.subscription = this.agentStateService
      .getActiveAgents(this.conversationId)
      .subscribe(agents => {
        this.activeAgents = agents;
    this.subscription?.unsubscribe();
  get displayAgents(): Array<{ run: MJAIAgentRunEntity; status: AgentStatus; confidence: number | null }> {
    if (this.isExpanded) {
      return this.activeAgents;
    return this.activeAgents.slice(0, this.maxVisibleAgents);
  getAgentTooltip(agent: { run: MJAIAgentRunEntity; status: AgentStatus; confidence: number | null }): string {
    const statusText = this.getStatusText(agent.status);
    const confidenceText = agent.confidence != null
      ? ` (Confidence: ${(agent.confidence * 100).toFixed(0)}%)`
    return `${agent.run.Agent || 'Agent'} - ${statusText}${confidenceText}`;
  getStatusText(status: AgentStatus): string {
      case 'acknowledging': return 'Acknowledging request';
      case 'working': return 'Working on task';
      case 'completing': return 'Completing';
      case 'completed': return 'Completed';
      case 'error': return 'Error occurred';
      default: return 'Active';
  onAgentClick(agent: { run: MJAIAgentRunEntity; status: AgentStatus; confidence: number | null }): void {
    this.agentSelected.emit(agent.run);
  onTogglePanel(): void {
    this.togglePanel.emit();
