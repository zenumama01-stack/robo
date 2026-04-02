import { Component, Input, Output, EventEmitter, OnInit, OnChanges } from '@angular/core';
 * Task detail panel showing task information, agent details, and agent run
 * Reusable across list and Gantt views
  selector: 'mj-task-detail-panel',
    <div class="task-detail-panel">
        <h3>{{ task.Name }}</h3>
        <button class="close-detail-btn" (click)="closePanel.emit()">
        @if (task.Description) {
            <p>{{ task.Description }}</p>
          <p>{{ task.Status }}</p>
            <label>Progress</label>
            <div class="detail-progress">
              <div class="progress-bar-detail">
                <div class="progress-fill-detail" [style.width.%]="task.PercentComplete"></div>
              <span>{{ task.PercentComplete }}%</span>
            <label>Started</label>
            <p>{{ formatDateTime(task.StartedAt) }}</p>
            <label>Due</label>
            <p>{{ formatDateTime(task.DueAt) }}</p>
            <label>Completed</label>
            <p>{{ formatDateTime(task.CompletedAt) }}</p>
            <label>Assigned User</label>
            <p>{{ task.User }}</p>
        <!-- Agent Information -->
            <div class="agent-info" (click)="openAgent()">
              <i [class]="'fas fa-' + ('robot')" class="agent-icon"></i>
              <span class="agent-name">{{ agent.Name }}</span>
              <i class="fas fa-external-link-alt link-icon"></i>
        <!-- Agent Run Information -->
        @if (agentRunId) {
            <label>Agent Run</label>
            <div class="agent-run-link" (click)="openAgentRun()">
              <span>View Run Details</span>
      height: calc(100% - 45px);
    .detail-header h3 {
    .close-detail-btn {
    .close-detail-btn:hover {
    .detail-field:last-child {
    .detail-field p {
    .detail-progress {
    .progress-bar-detail {
    .progress-fill-detail {
      height: calc(100% - 69px);
    .detail-progress span {
    .agent-info:hover {
    .agent-run-link {
    .agent-run-link:hover {
    .agent-run-link span {
export class TaskDetailPanelComponent implements OnInit, OnChanges {
  @Input() agentRunId: string | null = null;
  public agent: AIAgentEntityExtended | null = null;
    this.loadAgentInfo();
  private loadAgentInfo(): void {
    if (!this.task?.AgentID) {
    // Get agent from AIEngineBase
    this.agent = agents.find((a: AIAgentEntityExtended) => a.ID === this.task.AgentID) || null;
  public formatDateTime(date: Date | null): string {
  public openAgent(): void {
    if (this.task.AgentID) {
        recordId: this.task.AgentID
  public openAgentRun(): void {
        recordId: this.agentRunId
