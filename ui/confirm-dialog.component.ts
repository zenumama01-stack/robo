 * Confirmation dialog types
export type ConfirmDialogType = 'warning' | 'danger' | 'info';
 * A nice confirmation dialog to replace browser confirm().
    selector: 'mj-confirm-dialog',
    templateUrl: './confirm-dialog.component.html',
    styleUrls: ['./config-dialog.component.css']
export class ConfirmDialogComponent {
    /** Whether the dialog is visible */
    /** Dialog type (affects icon and button styling) */
    @Input() type: ConfirmDialogType = 'warning';
    /** Dialog title */
    @Input() title = 'Confirm Action';
    /** Dialog message */
    @Input() message = 'Are you sure you want to proceed?';
    /** Confirm button text */
    @Input() confirmText = 'Confirm';
    /** Cancel button text */
    @Input() cancelText = 'Cancel';
    /** Custom icon class (optional, uses default based on type if not provided) */
    /** Emitted when user confirms */
    @Output() confirmed = new EventEmitter<void>();
    /** Emitted when user cancels */
    public getIcon(): string {
        if (this.icon) return this.icon;
        switch (this.type) {
            case 'danger':
                return 'fa-solid fa-trash';
                return 'fa-solid fa-info-circle';
    public confirm(): void {
        this.confirmed.emit();
 * Style variants for the confirm button
export type ConfirmButtonStyle = 'primary' | 'danger';
 * ConfirmDialogComponent - Generic reusable confirmation dialog
 * Used for:
 * - Delete view confirmation
 * - Filter mode switch warning (data loss)
 * - Revert unsaved changes
 * - Any action requiring user confirmation
 * Follows the same @if backdrop + .dialog-panel.open pattern as AggregateSetupDialogComponent.
 * <mj-ev-confirm-dialog
 *   [IsOpen]="showDeleteConfirm"
 *   Title="Delete View"
 *   Message="Are you sure you want to delete this view?"
 *   DetailMessage="This action cannot be undone."
 *   ConfirmText="Delete"
 *   ConfirmStyle="danger"
 *   Icon="fa-solid fa-trash"
 *   (Confirmed)="onDeleteConfirmed()"
 *   (Cancelled)="showDeleteConfirm = false">
 * </mj-ev-confirm-dialog>
  selector: 'mj-ev-confirm-dialog',
  styleUrls: ['./confirm-dialog.component.css']
   * Dialog title
  @Input() Title: string = 'Confirm';
   * Primary message to display
  @Input() Message: string = 'Are you sure?';
   * Optional secondary detail message (shown smaller, below primary)
  @Input() DetailMessage: string = '';
   * Text for the confirm button
   * @default 'Confirm'
  @Input() ConfirmText: string = 'Confirm';
   * Text for the cancel button
   * @default 'Cancel'
  @Input() CancelText: string = 'Cancel';
   * Style variant for the confirm button
   * 'primary' = blue, 'danger' = red
   * @default 'primary'
  @Input() ConfirmStyle: ConfirmButtonStyle = 'primary';
   * Font Awesome icon class for the dialog header
   * @default 'fa-solid fa-circle-question'
  @Input() Icon: string = 'fa-solid fa-circle-question';
   * Emitted when user clicks the confirm button
  @Output() Confirmed = new EventEmitter<void>();
   * Emitted when user clicks cancel or backdrop
  OnConfirm(): void {
    this.Confirmed.emit();
