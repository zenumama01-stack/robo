export interface SaveVersionResult {
  Comment: string;
  Mode: 'new' | 'update';
  selector: 'mj-save-version-dialog',
        [title]="'Save Version'"
        [width]="420"
        (close)="OnCancel()">
        <div class="dialog-body">
          <div class="version-context">
            @if (CurrentVersion > 0) {
              <span class="version-badge">Current: v{{ CurrentVersion }}</span>
              <span class="version-badge new-badge">First version</span>
            <label class="field-label" for="versionComment">Comment</label>
              kendoTextBox
              id="versionComment"
              [(ngModel)]="Comment"
              placeholder="Describe what changed..."
              class="comment-input" />
            <div class="save-mode">
              <label class="radio-option" [class.selected]="Mode === 'new'">
                <input type="radio" name="saveMode" value="new" [(ngModel)]="Mode" />
                <div class="radio-content">
                  <span class="radio-label">Save as new version</span>
                  <span class="radio-desc">Creates v{{ CurrentVersion + 1 }}</span>
              <label class="radio-option" [class.selected]="Mode === 'update'">
                <input type="radio" name="saveMode" value="update" [(ngModel)]="Mode" />
                  <span class="radio-label">Update current version</span>
                  <span class="radio-desc">Overwrites v{{ CurrentVersion }}</span>
          <button kendoButton [themeColor]="'primary'" (click)="OnSave()">
          <button kendoButton [themeColor]="'base'" (click)="OnCancel()">
    .dialog-body {
    .version-context {
      color: var(--mat-sys-on-primary-container, #1e1b4b);
    .new-badge {
      background: var(--mat-sys-tertiary-container, #f3e8ff);
      color: var(--mat-sys-on-tertiary-container, #4a1d96);
      color: var(--mat-sys-on-surface, #1f2937);
    .comment-input {
    .save-mode {
      border: 1px solid var(--mat-sys-outline-variant, #d1d5db);
    .radio-option:hover {
      background: var(--mat-sys-surface-container, #f3f4f6);
    .radio-option.selected {
      border-color: var(--mat-sys-primary, #6366f1);
    .radio-option input[type="radio"] {
      accent-color: var(--mat-sys-primary, #6366f1);
    .radio-content {
    .radio-desc {
      color: var(--mat-sys-on-surface-variant, #6b7280);
export class SaveVersionDialogComponent {
  @Input() CurrentVersion = 0;
  @Output() Save = new EventEmitter<SaveVersionResult>();
  @Output() Cancel = new EventEmitter<void>();
  Comment = '';
  Mode: 'new' | 'update' = 'new';
  OnSave(): void {
    this.Save.emit({
      Comment: this.Comment,
      Mode: this.Mode
    this.ResetForm();
    this.Cancel.emit();
  private ResetForm(): void {
    this.Comment = '';
    this.Mode = 'new';
