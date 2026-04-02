import { DialogService as KendoDialogService, DialogRef } from '@progress/kendo-angular-dialog';
import { InputDialogComponent } from '../components/dialogs/input-dialog.component';
export interface DialogButton {
  primary?: boolean;
  action: () => void;
export interface InputDialogOptions {
  inputLabel: string;
  inputValue?: string;
  inputType?: 'text' | 'textarea' | 'number' | 'email';
  secondInputLabel?: string;
  secondInputValue?: string;
  secondInputPlaceholder?: string;
  secondInputRequired?: boolean;
  okText?: string;
  cancelText?: string;
export interface ConfirmDialogOptions {
  dangerous?: boolean;
 * Dialog service for displaying Kendo-based dialogs
 * Replaces browser alert() and confirm() with proper UI components
export class DialogService {
  constructor(private kendoDialogService: KendoDialogService) {}
   * Show a confirmation dialog
   * @returns Promise<boolean> - true if user clicked OK, false if cancelled
  confirm(options: ConfirmDialogOptions): Promise<boolean> {
        content: options.message,
            text: options.okText || 'OK',
            primary: true,
            themeColor: options.dangerous ? 'error' : 'primary'
            text: options.cancelText || 'Cancel',
            primary: false
        minWidth: 250
        if (result instanceof Object && 'text' in result) {
          resolve(result.text === (options.okText || 'OK'));
          resolve(false);
   * Show an alert dialog
  alert(title: string, message: string, okText: string = 'OK'): Promise<void> {
            text: okText,
            primary: true
      dialogRef.result.subscribe(() => {
   * Show an input dialog
   * @returns Promise<string | {value: string; secondValue?: string} | null> -
   *          If single input: returns string
   *          If dual input: returns object with both values
   *          Returns null if cancelled
  input(options: InputDialogOptions): Promise<string | {value: string; secondValue?: string} | null> {
        content: InputDialogComponent,
        minWidth: 300
      const componentInstance = dialogRef.content.instance as InputDialogComponent;
      componentInstance.message = options.message;
      componentInstance.inputLabel = options.inputLabel;
      componentInstance.inputType = options.inputType || 'text';
      componentInstance.placeholder = options.placeholder || '';
      componentInstance.required = options.required || false;
      componentInstance.value = options.inputValue || '';
      componentInstance.secondInputLabel = options.secondInputLabel || '';
      componentInstance.secondInputPlaceholder = options.secondInputPlaceholder || '';
      componentInstance.secondInputRequired = options.secondInputRequired || false;
      componentInstance.secondValue = options.secondInputValue || '';
      // Focus and select input after dialog opens
        const inputElement = document.querySelector('.k-dialog input, .k-dialog textarea') as HTMLInputElement | HTMLTextAreaElement;
        if (inputElement) {
          inputElement.focus();
          inputElement.select();
        if (result instanceof Object && 'text' in result && result.text === (options.okText || 'OK')) {
          const value = componentInstance.getValue();
          if (options.required && !value) {
            // If second input is present, return object with both values
            if (options.secondInputLabel) {
                secondValue: componentInstance.getSecondValue()
              // Single input - return string for backward compatibility
              resolve(value);
   * Show a custom dialog with custom content and actions
  custom(title: string, content: string, buttons: DialogButton[], width: number = 500): DialogRef {
    const actions = buttons.map(btn => ({
      text: btn.text,
      primary: btn.primary || false
      actions: actions,
      width: width,
        const button = buttons.find(b => b.text === result.text);
        if (button && button.action) {
          button.action();
