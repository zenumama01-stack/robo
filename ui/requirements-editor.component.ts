import { Component, Input, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
type RequirementsField = 'functionalRequirements' | 'technicalDesign';
type EditorMode = 'preview' | 'edit';
  selector: 'mj-requirements-editor',
    <div class="requirements-editor">
          <i class="fa-solid" [ngClass]="FieldIcon"></i>
          {{ Title }}
              <button kendoButton [themeColor]="'primary'" (click)="ApplyChanges()" class="action-btn">
              <button kendoButton [themeColor]="'base'" (click)="CancelChanges()" class="action-btn">
          <div class="mode-toggle">
            <button class="mode-btn" [class.active]="ViewMode === 'preview'" (click)="SetViewMode('preview')">
            <button class="mode-btn" [class.active]="ViewMode === 'edit'" (click)="SetViewMode('edit')">
              <i class="fa-solid fa-pencil"></i> Edit
          @if (ViewMode === 'preview') {
            <div class="preview-container">
              @if (EditableContent) {
                <mj-markdown [data]="EditableContent" [enableCodeCopy]="true"></mj-markdown>
                <div class="empty-preview">
                  <p>No {{ Title | lowercase }} defined yet.</p>
                  <button class="edit-link" (click)="SetViewMode('edit')">
                    <i class="fa-solid fa-pencil"></i> Start writing
                [language]="'markdown'"
                [placeholder]="'Enter ' + Title + ' in markdown format...'"
            <p>Select a component to view its {{ Title | lowercase }}.</p>
    .requirements-editor {
    .mode-toggle {
    .preview-container {
    .code-editor-container mj-code-editor {
    .empty-preview {
    .empty-preview i {
    .empty-preview p {
    .edit-link {
    .edit-link:hover {
      background: var(--mat-sys-primary-container, #e0e7ff);
export class RequirementsEditorComponent implements OnInit, OnDestroy {
  @Input() Field: RequirementsField = 'functionalRequirements';
  @Input() Title: string = 'Functional Requirements';
  ViewMode: EditorMode = 'preview';
  get FieldIcon(): string {
    return this.Field === 'functionalRequirements' ? 'fa-clipboard-list' : 'fa-drafting-compass';
      if (!this.IsEditing) {
  SetViewMode(mode: EditorMode): void {
      spec[this.Field] = this.EditableContent;
      this.ViewMode = 'preview';
      console.error('Error applying requirements changes:', error);
      this.EditableContent = spec[this.Field] || '';
