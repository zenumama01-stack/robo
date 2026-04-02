type SpecEditorMode = 'form' | 'json';
interface SpecFormModel {
  exampleUsage: string;
const COMPONENT_TYPES = [
  'Report', 'Dashboard', 'Form', 'Chart', 'Table', 'Widget', 'Navigation', 'Search', 'Utility'
  selector: 'mj-spec-editor',
    <div class="spec-editor">
          <button class="mode-btn" [class.active]="Mode === 'form'" (click)="SetMode('form')">
            <i class="fa-solid fa-wpforms"></i> Form
          <button class="mode-btn" [class.active]="Mode === 'json'" (click)="SetMode('json')">
            <i class="fa-solid fa-code"></i> JSON
        @if (State.IsEditingSpec) {
        @if (Mode === 'form') {
          <div class="form-mode">
                <label class="form-label">Name</label>
                <input class="form-input" [(ngModel)]="FormModel.name" (ngModelChange)="OnFormChanged()" />
                <label class="form-label">Type</label>
                <select class="form-select" [(ngModel)]="FormModel.type" (ngModelChange)="OnFormChanged()">
                  @for (t of ComponentTypes; track t) {
                    <option [value]="t">{{ t }}</option>
              <label class="form-label">Title</label>
              <input class="form-input" [(ngModel)]="FormModel.title" (ngModelChange)="OnFormChanged()" />
              <textarea class="form-textarea" [(ngModel)]="FormModel.description" (ngModelChange)="OnFormChanged()" rows="3"></textarea>
                <label class="form-label">Location</label>
                <select class="form-select" [(ngModel)]="FormModel.location" (ngModelChange)="OnFormChanged()">
                  <option value="embedded">embedded</option>
                  <option value="standalone">standalone</option>
                <label class="form-label">Example Usage</label>
                <input class="form-input" [(ngModel)]="FormModel.exampleUsage" (ngModelChange)="OnFormChanged()" placeholder="Optional" />
          <div class="json-mode">
              [(ngModel)]="State.EditableSpec"
              (ngModelChange)="OnJsonChanged()">
    .spec-editor {
    .form-mode {
    .form-input:focus {
    .form-textarea:focus {
    .form-select {
    .form-select:focus {
    .json-mode {
    .json-mode mj-code-editor {
export class SpecEditorComponent implements OnInit, OnDestroy {
  Mode: SpecEditorMode = 'form';
  FormModel: SpecFormModel = { name: '', title: '', description: '', type: '', location: '', exampleUsage: '' };
  ComponentTypes: string[] = COMPONENT_TYPES;
    this.syncSpecToForm();
      if (this.Mode === 'form' && !this.State.IsEditingSpec) {
  SetMode(mode: SpecEditorMode): void {
    if (mode === 'form') {
      this.syncFormToSpec();
    this.Mode = mode;
  OnFormChanged(): void {
    this.State.IsEditingSpec = true;
  OnJsonChanged(): void {
    if (this.Mode === 'form') {
    this.State.ApplySpecChanges();
  private syncSpecToForm(): void {
      this.FormModel = {
        name: spec.name || '',
        title: spec.title || '',
        description: spec.description || '',
        type: spec.type || '',
        location: spec.location || '',
        exampleUsage: spec.exampleUsage || ''
      // If JSON is invalid, keep current form values
  private syncFormToSpec(): void {
      spec.name = this.FormModel.name;
      spec.title = this.FormModel.title;
      spec.description = this.FormModel.description;
      spec.type = this.FormModel.type;
      spec.location = this.FormModel.location;
      spec.exampleUsage = this.FormModel.exampleUsage;
      this.State.EditableSpec = JSON.stringify(spec, null, 2);
      // If JSON is invalid, skip sync
