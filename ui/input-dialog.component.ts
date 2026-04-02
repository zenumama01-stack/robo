  selector: 'mj-input-dialog',
    <div class="input-dialog-content">
      <div class="input-field">
        <label class="input-label">
          {{ inputLabel }}
          @if (required) {
            <span class="required-mark">*</span>
        @if (inputType === 'textarea') {
            [(ngModel)]="value"
            [placeholder]="placeholder"
            class="k-textarea">
        @if (inputType !== 'textarea') {
            [type]="inputType || 'text'"
            class="k-textbox"
            (keydown.enter)="onEnterKey($event)">
      @if (secondInputLabel) {
            {{ secondInputLabel }}
            @if (secondInputRequired) {
            [(ngModel)]="secondValue"
            [placeholder]="secondInputPlaceholder"
    .input-dialog-content {
    .input-field {
    .required-mark {
    .k-textbox,
    .k-textarea {
    .k-textbox:focus,
    .k-textarea:focus {
export class InputDialogComponent {
  @Input() message: string = '';
  @Input() inputLabel: string = '';
  @Input() inputType: 'text' | 'textarea' | 'number' | 'email' = 'text';
  @Input() placeholder: string = '';
  @Input() required: boolean = false;
  @Input() value: string = '';
  @Input() secondInputLabel: string = '';
  @Input() secondInputPlaceholder: string = '';
  @Input() secondInputRequired: boolean = false;
  @Input() secondValue: string = '';
  constructor(public dialogRef: DialogRef) {}
  onEnterKey(event: Event): void {
    const keyEvent = event as KeyboardEvent;
    if (this.inputType !== 'textarea') {
      keyEvent.preventDefault();
      // Trigger OK button click
      const okButton = document.querySelector('.k-dialog-actions button.k-primary') as HTMLButtonElement;
      if (okButton) {
        okButton.click();
  getValue(): string {
    return this.value.trim();
  getSecondValue(): string {
    return this.secondValue.trim();
