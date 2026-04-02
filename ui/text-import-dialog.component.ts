  selector: 'app-text-import-dialog',
    <div class="text-import-dialog-content">
        <h3>Import Component from Text</h3>
        <p>Paste or type your component specification JSON below:</p>
      <div class="editor-container">
          [(ngModel)]="componentJson"
          [autoFocus]="true"
          [indentWithTab]="true"
          [placeholder]="'Paste your component specification JSON here...'"
          style="height: 400px;">
      @if (errorMessage) {
          {{ errorMessage }}
        <button kendoButton (click)="cancel()" [themeColor]="'base'">
        <button kendoButton (click)="import()" [themeColor]="'primary'" [disabled]="!componentJson">
          <i class="fa-solid fa-file-import"></i> Import
    .text-import-dialog-content {
    .dialog-header p {
    .editor-container {
      border: 1px solid #feb2b2;
export class TextImportDialogComponent {
  @Output() importSpec = new EventEmitter<ComponentSpec>();
  @Output() cancelDialog = new EventEmitter<void>();
  public componentJson = '';
  public errorMessage = '';
  public import(): void {
    this.errorMessage = '';
    if (!this.componentJson.trim()) {
      this.errorMessage = 'Please enter a component specification';
      const spec = JSON.parse(this.componentJson) as ComponentSpec;
        this.errorMessage = 'Invalid specification: missing required fields (name and code)';
      // Emit the parsed spec
      this.importSpec.emit(spec);
      this.errorMessage = 'Invalid JSON format. Please check your syntax.';
  public cancel(): void {
    this.cancelDialog.emit();
