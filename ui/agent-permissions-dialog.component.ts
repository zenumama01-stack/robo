import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, HostListener } from '@angular/core';
 * A centered modal dialog that wraps the AgentPermissionsPanel.
 * Use this when you want a focused, overlay-based permissions editor.
 * <mj-agent-permissions-dialog
 *     *ngIf="showDialog"
 *     [Agent]="selectedAgent"
 *     (Closed)="showDialog = false"
 *     (PermissionsChanged)="onPermsChanged()">
 * </mj-agent-permissions-dialog>
    selector: 'mj-agent-permissions-dialog',
        <div class="apd-backdrop" [class.apd-visible]="IsVisible" (click)="OnClose()"></div>
        <div class="apd-dialog" [class.apd-visible]="IsVisible">
          <div class="apd-header">
            <div class="apd-title-group">
              <i class="fa-solid fa-shield-halved apd-title-icon"></i>
                <h2 class="apd-title">Manage Permissions</h2>
                @if (Agent) {
                  <p class="apd-subtitle">{{ Agent.Name }}</p>
            <button class="apd-close-btn" (click)="OnClose()">
          <div class="apd-body">
            <mj-agent-permissions-panel
              [Agent]="Agent"
              (PermissionsChanged)="PermissionsChanged.emit()">
            </mj-agent-permissions-panel>
        .apd-backdrop {
            background: rgba(0, 0, 0, 0);
            transition: background 0.25s ease;
        .apd-backdrop.apd-visible {
        .apd-dialog {
            transform: translate(-50%, -50%) scale(0.95);
            width: 640px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.2);
            transition: opacity 0.25s ease, transform 0.25s cubic-bezier(0.16, 1, 0.3, 1);
        .apd-dialog.apd-visible {
            transform: translate(-50%, -50%) scale(1);
        .apd-header {
        .apd-title-group {
        .apd-title-icon {
        .apd-title {
        .apd-subtitle {
            margin: 2px 0 0 0;
        .apd-close-btn {
        .apd-close-btn:hover {
        .apd-body {
        trigger('fadeIn', [
                style({ opacity: 0 }),
                animate('200ms ease-out', style({ opacity: 1 }))
export class AgentPermissionsDialogComponent {
    @Input() Agent: AIAgentEntityExtended | null = null;
    @Output() PermissionsChanged = new EventEmitter<void>();
    public IsVisible = false;
        // Animate in on next microtask
            this.IsVisible = true;
    public OnEscapeKey(): void {
        this.IsVisible = false;
        setTimeout(() => this.Closed.emit(), 250);
