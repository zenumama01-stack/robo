 * Cancellable event base class for form actions.
 * Allows event handlers to cancel default behavior by setting Cancel = true.
export class CancellableFormEvent {
   * Set to true to cancel the default behavior of this event.
  Cancel = false;
 * Event args for BeforeSave event
export class BeforeSaveEventArgs extends CancellableFormEvent {
   * Whether validation should be performed before save
  ValidateBeforeSave: boolean;
  constructor(validateBeforeSave: boolean) {
    this.ValidateBeforeSave = validateBeforeSave;
 * Event args for BeforeDelete event
export class BeforeDeleteEventArgs extends CancellableFormEvent {}
 * Event args for BeforeCancel event
export class BeforeCancelEventArgs extends CancellableFormEvent {}
 * Event args for BeforeHistoryView event
export class BeforeHistoryViewEventArgs extends CancellableFormEvent {}
 * Event args for BeforeListManagement event
export class BeforeListManagementEventArgs extends CancellableFormEvent {}
 * Event args for custom toolbar button clicks
export interface CustomToolbarButtonClickEventArgs {
   * The key/identifier of the button that was clicked
  ButtonKey: string;
   * The button configuration that was clicked
  Button: CustomToolbarButton;
 * Configuration for a custom toolbar button
export interface CustomToolbarButton {
   * Unique identifier for this button
  Key: string;
   * Display name/label for the button (used for accessibility)
   * Tooltip text shown on hover
   * Font Awesome icon class (e.g., "fa-solid fa-star")
   * Whether the button should be visible
  Visible?: boolean;
   * Whether the button should be disabled
  Disabled?: boolean;
   * Optional CSS class to apply to the button
  CssClass?: string;
