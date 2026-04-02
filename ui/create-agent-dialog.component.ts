import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, HostListener, ChangeDetectorRef } from '@angular/core';
import { CreateAgentConfig, CreateAgentResult } from './create-agent-panel.component';
 * A centered modal dialog that wraps the CreateAgentPanel.
 * Use this when you want a focused, overlay-based agent creation experience.
 * <mj-create-agent-dialog
 *     [Config]="{ Title: 'Create Agent' }"
 *     (Created)="onAgentCreated($event)"
 *     (Closed)="showDialog = false">
 * </mj-create-agent-dialog>
    selector: 'mj-create-agent-dialog',
        <div class="cad-backdrop" [class.cad-visible]="IsVisible" (click)="OnClose()"></div>
        <div class="cad-dialog" [class.cad-visible]="IsVisible">
          <div class="cad-header">
            <div class="cad-title-group">
              <i class="fa-solid fa-robot cad-title-icon"></i>
                <h2 class="cad-title">{{ DialogTitle }}</h2>
                @if (Config.ParentAgentName) {
                  <p class="cad-subtitle">
                    Sub-agent of {{ Config.ParentAgentName }}
            <button class="cad-close-btn" (click)="OnClose()">
          <div class="cad-body">
            <mj-create-agent-panel
              [Config]="Config"
              (Created)="OnCreated($event)"
              (Cancelled)="OnClose()">
            </mj-create-agent-panel>
        .cad-backdrop {
        .cad-backdrop.cad-visible {
        .cad-dialog {
            width: 680px;
        .cad-dialog.cad-visible {
        .cad-header {
        .cad-title-group {
        .cad-title-icon {
        .cad-title {
        .cad-subtitle {
        .cad-close-btn {
        .cad-close-btn:hover {
        .cad-body {
export class CreateAgentDialogComponent {
    @Input() Config: CreateAgentConfig = {};
    @Output() Created = new EventEmitter<CreateAgentResult>();
        if (this.Config?.Title) return this.Config.Title;
        return this.Config?.ParentAgentId ? 'Create Sub-Agent' : 'Create New Agent';
    public OnCreated(result: CreateAgentResult): void {
        this.Created.emit(result);
