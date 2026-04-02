import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, HostListener, NgZone, ChangeDetectorRef } from '@angular/core';
 * A slide-in panel from the right that wraps the AgentPermissionsPanel.
 * Includes a resizable edge handle and backdrop click-to-close.
 * <mj-agent-permissions-slideover
 *     *ngIf="showPanel"
 *     (Closed)="showPanel = false"
 * </mj-agent-permissions-slideover>
    selector: 'mj-agent-permissions-slideover',
        <div class="aps-backdrop" [class.aps-visible]="IsVisible" (click)="OnClose()"></div>
        <!-- Slide panel -->
        <div class="aps-panel" [class.aps-visible]="IsVisible" [style.width.px]="WidthPx">
          <!-- Resize handle -->
          <div class="aps-resize-handle" (mousedown)="OnResizeStart($event)">
            <div class="aps-resize-grip"></div>
          <div class="aps-header">
            <div class="aps-title-group">
              <i class="fa-solid fa-shield-halved aps-title-icon"></i>
                <h2 class="aps-title">Permissions</h2>
                  <p class="aps-subtitle">{{ Agent.Name }}</p>
            <button class="aps-close-btn" (click)="OnClose()">
          <div class="aps-body">
        .aps-backdrop {
        .aps-backdrop.aps-visible {
        .aps-panel {
            box-shadow: -8px 0 32px rgba(0, 0, 0, 0.12);
            transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1);
            max-width: 92vw;
        .aps-panel.aps-visible {
        .aps-resize-handle {
        .aps-resize-handle:hover .aps-resize-grip,
        .aps-resize-handle:active .aps-resize-grip {
        .aps-resize-grip {
            background: var(--border-color, #d1d5db);
            transition: opacity 0.2s ease, background 0.2s ease;
        .aps-resize-handle:hover {
        .aps-header {
        .aps-title-group {
        .aps-title-icon {
        .aps-title {
        .aps-subtitle {
        .aps-close-btn {
        .aps-close-btn:hover {
        .aps-body {
export class AgentPermissionsSlideoverComponent {
    public WidthPx = 560;
    private readonly minWidth = 400;
    private readonly maxWidthRatio = 0.92;
    private boundOnResizeMove = this.onResizeMove.bind(this);
    private boundOnResizeEnd = this.onResizeEnd.bind(this);
        document.removeEventListener('mousemove', this.boundOnResizeMove);
        document.removeEventListener('mouseup', this.boundOnResizeEnd);
        setTimeout(() => this.Closed.emit(), 300);
    // Resize
        this.ngZone.runOutsideAngular(() => {
            document.addEventListener('mousemove', this.boundOnResizeMove);
            document.addEventListener('mouseup', this.boundOnResizeEnd);
        const maxWidth = window.innerWidth * this.maxWidthRatio;
        this.WidthPx = Math.max(this.minWidth, Math.min(maxWidth, window.innerWidth - event.clientX));
        this.ngZone.run(() => this.cdr.markForCheck());
    private onResizeEnd(): void {
