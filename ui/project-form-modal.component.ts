import { Component, Input, Output, EventEmitter, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
export interface ProjectFormData {
const DEFAULT_PROJECT_COLORS = [
  '#0076B6', // MJ Blue
  '#673AB7', // Deep Purple
  '#3F51B5', // Indigo
  '#03A9F4', // Light Blue
  '#009688', // Teal
  '#8BC34A', // Light Green
  '#CDDC39', // Lime
  '#FFEB3B', // Yellow
  '#FFC107', // Amber
  '#FF5722', // Deep Orange
  '#795548', // Brown
  '#607D8B', // Blue Grey
  '#9E9E9E'  // Grey
const DEFAULT_PROJECT_ICONS = [
  'fa-folder',
  'fa-folder-open',
  'fa-briefcase',
  'fa-project-diagram',
  'fa-chart-line',
  'fa-tasks',
  'fa-clipboard-list',
  'fa-bullseye',
  'fa-rocket',
  'fa-lightbulb',
  'fa-brain',
  'fa-cogs',
  'fa-code',
  'fa-database',
  'fa-server',
  'fa-cloud',
  'fa-mobile-alt',
  'fa-desktop',
  'fa-globe',
  'fa-users'
  selector: 'mj-project-form-modal',
      [title]="isEditMode ? 'Edit Project' : 'New Project'"
      <div class="project-form">
        <!-- Name Input -->
          <label for="projectName" class="required">
            id="projectName"
            placeholder="Enter project name"
            class="k-textbox full-width"
            (keydown.enter)="onSave()"
            autofocus />
          @if (showNameError) {
            <div class="error-message">Project name is required</div>
        <!-- Description Textarea -->
          <label for="projectDescription">
            id="projectDescription"
            placeholder="Enter project description (optional)"
            class="k-textarea full-width"
        <!-- Color Picker -->
          <div class="color-picker-section">
            <div class="color-palette">
              @for (color of availableColors; track color) {
                  class="color-swatch"
                  [class.selected]="formData.color === color"
                  [style.backgroundColor]="color"
                  (click)="selectColor(color)"
                  [title]="color">
            <div class="custom-color-input">
              <label for="customColor">Custom:</label>
                id="customColor"
                type="color"
                [(ngModel)]="formData.color"
                class="custom-color-picker" />
              <span class="color-value">{{ formData.color }}</span>
        <!-- Icon Selector -->
          <div class="icon-selector-section">
            <div class="selected-icon-preview">
              <i class="fa-solid {{ formData.icon }}" [style.color]="formData.color"></i>
              <span>{{ formData.icon }}</span>
            <div class="icon-grid">
              @for (icon of availableIcons; track icon) {
                  [class.selected]="formData.icon === icon"
                  [title]="icon">
                  <i class="fa-solid {{ icon }}"></i>
        <button kendoButton [themeColor]="'primary'" (click)="onSave()">
          {{ isEditMode ? 'Save' : 'Create' }}
    .project-form {
    .form-field label.required::after {
      color: #F44336;
    /* Color Picker Styles */
    .color-picker-section {
      background: #F9F9F9;
    .color-palette {
      grid-template-columns: repeat(10, 1fr);
    .color-swatch {
    .color-swatch:hover {
      box-shadow: 0 2px 6px rgba(0,0,0,0.2);
    .color-swatch.selected {
      box-shadow: 0 0 0 2px #fff, 0 0 0 4px #0076B6;
    .custom-color-input {
    .custom-color-input label {
    .custom-color-picker {
    .color-value {
    /* Icon Selector Styles */
    .icon-selector-section {
    .selected-icon-preview {
    .selected-icon-preview i {
    .selected-icon-preview span {
      background: #F0F0F0;
      background: #E3F2FD;
    /* Scrollbar Styles */
    .icon-grid::-webkit-scrollbar {
    .icon-grid::-webkit-scrollbar-track {
    .icon-grid::-webkit-scrollbar-thumb {
    .icon-grid::-webkit-scrollbar-thumb:hover {
      background: #BFBFBF;
export class ProjectFormModalComponent implements OnInit {
  @Input() dialogRef!: DialogRef;
  @Input() project: MJProjectEntity | null = null;
  @Output() projectSaved = new EventEmitter<MJProjectEntity>();
  public formData: ProjectFormData = {
    color: '#0076B6',
    icon: 'fa-folder'
  public showNameError = false;
  public availableColors = DEFAULT_PROJECT_COLORS;
  public availableIcons = DEFAULT_PROJECT_ICONS;
    this.isEditMode = this.project != null;
    if (this.project) {
      this.loadProjectData();
  private loadProjectData(): void {
    if (!this.project) return;
      name: this.project.Name || '',
      description: this.project.Description || '',
      color: this.project.Color || '#0076B6',
      icon: this.project.Icon || 'fa-folder'
  selectColor(color: string): void {
    this.formData.color = color;
  selectIcon(icon: string): void {
    this.formData.icon = icon;
    // Validate
    if (!this.formData.name.trim()) {
      this.showNameError = true;
    this.showNameError = false;
      const project = this.project || await md.GetEntityObject<MJProjectEntity>('MJ: Projects', this.currentUser);
      project.Name = this.formData.name.trim();
      project.Description = this.formData.description.trim() || null;
      project.Color = this.formData.color;
      project.Icon = this.formData.icon;
      if (!this.isEditMode) {
        project.EnvironmentID = this.environmentId;
        project.IsArchived = false;
      const saved = await project.Save();
        this.projectSaved.emit(project);
        throw new Error('Failed to save project');
      console.error('Error saving project:', error);
      alert('Failed to save project. Please try again.');
