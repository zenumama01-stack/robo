import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { switchMap } from 'rxjs/operators';
import { DialogService } from '../../services/dialog.service';
import { AgentStateService, AgentStatus, AgentWithStatus } from '../../services/agent-state.service';
interface AgentProcess extends AgentWithStatus {
  selector: 'mj-agent-process-panel',
    @if (activeProcesses.length > 0) {
      <div class="agent-panel" [class.minimized]="isMinimized">
            Active Agents ({{ activeProcesses.length }})
            <button class="header-btn" (click)="isMinimized = !isMinimized" [title]="isMinimized ? 'Expand' : 'Minimize'">
              <i class="fas" [ngClass]="isMinimized ? 'fa-chevron-up' : 'fa-chevron-down'"></i>
        @if (!isMinimized) {
            @for (process of activeProcesses; track process.run.ID) {
              <div class="process-item">
                <div class="process-header" (click)="toggleProcess(process)">
                  <div class="process-info">
                    <div class="agent-avatar-small"
                         [class.status-acknowledging]="process.status === 'acknowledging'"
                         [class.status-working]="process.status === 'working'"
                         [class.status-completing]="process.status === 'completing'"
                         [class.status-completed]="process.status === 'completed'"
                         [class.status-error]="process.status === 'error'">
                      @if (process.status !== 'completed') {
                        <div class="pulse-dot"></div>
                      <span class="process-name">{{ process.run.Agent || 'Agent' }}</span>
                      <span class="process-status" [class]="'status-' + process.status">
                        {{ getStatusText(process.status) }}
                    @if (process.confidence != null) {
                      <div class="confidence-indicator" [title]="'Confidence: ' + (process.confidence * 100).toFixed(0) + '%'">
                        <i class="fas fa-gauge-high"></i>
                        <span>{{ (process.confidence * 100).toFixed(0) }}%</span>
                  <i class="fas" [ngClass]="process.expanded ? 'fa-chevron-up' : 'fa-chevron-down'"></i>
                @if (process.expanded) {
                  <div class="process-details">
                    @if (process.run.StartedAt) {
                        <span>Started: {{ process.run.StartedAt | date:'short' }}</span>
                    @if (getElapsedTime(process.run)) {
                        <i class="fas fa-hourglass-half"></i>
                        <span>Duration: {{ getElapsedTime(process.run) }}</span>
                    <div class="process-actions">
                      @if (process.run.Status === 'Running' || process.run.Status === 'Paused') {
                        <button class="btn-action btn-cancel" (click)="onCancelProcess(process)">
                          <i class="fas fa-stop"></i> Cancel
                      <button class="btn-action" (click)="onViewDetails(process)">
                        <i class="fas fa-external-link-alt"></i> View Details
    .agent-panel {
      bottom: 24px;
      box-shadow: 0 8px 32px rgba(0,0,0,0.12);
      transition: all 300ms ease;
    .agent-panel.minimized { width: 280px; max-height: 60px; }
      background: linear-gradient(135deg, #F9FAFB, #F3F4F6);
    .header-actions { display: flex; gap: 4px; }
    .header-btn {
    .header-btn:hover { background: rgba(0,0,0,0.08); color: #111827; }
    .process-item {
    .process-item:last-child { border-bottom: none; }
    .process-item:hover { background: #FAFAFA; }
    .process-header {
    .process-info {
    .agent-avatar-small {
    .agent-avatar-small.status-acknowledging { background: linear-gradient(135deg, #3B82F6, #2563EB); }
    .agent-avatar-small.status-working { background: linear-gradient(135deg, #F59E0B, #D97706); }
    .agent-avatar-small.status-completing { background: linear-gradient(135deg, #10B981, #059669); }
    .agent-avatar-small.status-completed { background: linear-gradient(135deg, #6B7280, #4B5563); opacity: 0.6; }
    .agent-avatar-small.status-error { background: linear-gradient(135deg, #EF4444, #DC2626); }
    .pulse-dot {
      0%, 100% { opacity: 1; transform: scale(1); }
      50% { opacity: 0.5; transform: scale(1.5); }
    .process-name {
    .process-status {
      width: fit-content;
    .status-acknowledging { background: #DBEAFE; color: #1E40AF; }
    .status-working { background: #FEF3C7; color: #B45309; }
    .status-completing { background: #D1FAE5; color: #065F46; }
    .status-completed { background: #F3F4F6; color: #6B7280; }
    .status-error { background: #FEE2E2; color: #991B1B; }
    .confidence-indicator {
    .process-details {
      padding: 0 20px 16px 68px;
      animation: slideDown 200ms ease;
      from { opacity: 0; transform: translateY(-10px); }
    .detail-row i {
    .process-actions {
      border-top: 1px solid #F3F4F6;
    .btn-action {
    .btn-action:hover {
      background: #EF4444;
      background: #DC2626;
      box-shadow: 0 2px 4px rgba(239, 68, 68, 0.3);
export class AgentProcessPanelComponent implements OnInit, OnDestroy {
  public activeProcesses: AgentProcess[] = [];
  public isMinimized: boolean = false;
    private agentStateService: AgentStateService
    // Start polling for active agents
    this.agentStateService.startPolling(this.currentUser, this.conversationId);
    // Subscribe to active agents
        // Preserve expanded state for existing processes
        this.activeProcesses = agents.map(agent => {
          const existing = this.activeProcesses.find(p => p.run.ID === agent.run.ID);
            ...agent,
            expanded: existing ? existing.expanded : false
    // Note: We don't stop polling here as other components may be using the service
  toggleProcess(process: AgentProcess): void {
    process.expanded = !process.expanded;
      case 'acknowledging': return 'Acknowledging';
      case 'working': return 'Working';
      case 'error': return 'Error';
  getElapsedTime(run: MJAIAgentRunEntity): string | null {
    if (!run.StartedAt) return null;
    const start = new Date(run.StartedAt).getTime();
    const end = run.CompletedAt ? new Date(run.CompletedAt).getTime() : Date.now();
    const elapsed = end - start;
  async onCancelProcess(process: AgentProcess): Promise<void> {
    const confirmed = await this.dialogService.confirm({
      title: 'Cancel Agent',
      message: `Cancel agent "${process.run.Agent || 'Agent'}"?`,
      okText: 'Cancel Agent',
      cancelText: 'Keep Running'
      const success = await this.agentStateService.cancelAgent(process.run.ID);
        await this.dialogService.alert('Error', 'Failed to cancel agent process');
      console.error('Failed to cancel agent process:', error);
  onViewDetails(process: AgentProcess): void {
    // TODO: Navigate to agent run details page or open modal
    console.log('View agent run details:', process.run.ID);
