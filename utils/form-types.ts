 * Width mode for the form panels container.
 * - 'centered': Max-width constrained and centered (default, good for readability)
 * - 'full-width': No max-width constraint, uses full available width
export type FormWidthMode = 'centered' | 'full-width';
 * Variant for collapsible panel styling.
 * - 'default': Standard white card with accent border
 * - 'related-entity': Blue-accented card for related entity grid sections
 * - 'inherited': Purple-accented card for IS-A inherited field sections
export type PanelVariant = 'default' | 'related-entity' | 'inherited';
 * Information about a section within the form.
 * Used to initialize and track section state.
 * @template M Optional metadata type for custom per-section data
export interface FormSectionInfo<M = unknown> {
  /** Unique key for this section (used in state persistence) */
  /** Whether the section starts expanded. Default: true for field sections, false for related entities */
  /** Row count for related entity sections (shown as badge) */
  RowCount?: number;
  /** Custom metadata attached to this section */
  Metadata?: M;
  /** Panel variant for styling */
  Variant?: PanelVariant;
  /** Font Awesome icon class for the section header */
   * If set, indicates this section contains fields inherited from a parent entity.
   * The value is the parent entity name (e.g., "Products" for a Meeting IS-A Product).
  InheritedFromEntity?: string;
 * Event emitted when a panel drag-to-reorder operation starts.
export interface PanelDragStartEvent {
  Event: DragEvent;
 * Event emitted when a panel is dropped onto another panel for reordering.
export interface PanelDropEvent {
  SourceSectionKey: string;
  TargetSectionKey: string;
 * Context object passed from the form to all child components (panels, fields).
 * Eliminates the need for many individual @Input properties.
 * Property names use camelCase to maintain structural compatibility with
 * {@link BaseFormContext} from `@memberjunction/ng-base-forms`. This allows
 * BaseFormComponent.formContext to be passed directly to ng-forms components.
export interface FormContext {
  /** Current search filter string for highlighting/filtering sections and fields */
  sectionFilter?: string;
  /** Whether to show fields that have empty values in read-only mode */
  showEmptyFields?: boolean;
   * When true, all fields show their validation errors regardless of touched state.
   * Set to true after a failed save attempt; reset on successful save or cancel.
  showValidation?: boolean;
   * Validation errors from the most recent full-record Validate() call.
   * Fields filter by `ValidationErrorInfo.Source === FieldName` to find their errors.
  /** Whether drag-and-drop section reordering is allowed. Read by panels to show/hide drag handles. */
  allowSectionReorder?: boolean;
 * Result of a form save operation.
export interface FormSaveResult {
  /** Whether the save was successful */
  /** Error message if save failed */
  /** Number of milliseconds the save took */
 * Event emitted after a record is saved.
export interface RecordSavedEvent {
  RecordId: string;
  /** The display name of the saved record (for tab title updates, etc.) */
  RecordDisplayName?: string;
  Result: FormSaveResult;
 * Event emitted after a record is deleted.
export interface RecordDeletedEvent {
 * Event emitted when a record save fails.
export interface RecordSaveFailedEvent {
 * Event emitted when a record delete fails.
export interface RecordDeleteFailedEvent {
 * Event emitted when validation fails before save.
export interface ValidationFailedEvent {
  Errors: string[];
 * Notification event emitted by BaseFormComponent.
 * The host application subscribes and maps to its own notification system (toasts, snackbars, etc.).
export interface FormNotificationEvent {
  /** The notification message text */
  /** Notification severity */
  Type: 'success' | 'error' | 'warning' | 'info';
  /** Duration in milliseconds the notification should display */
  Duration: number;
 * Backward-compatible alias for FormContext.
 * Explorer's BaseFormComponent used this name; new code should use FormContext.
export type BaseFormContext = FormContext;
 * Creates a default FormContext with sensible defaults.
export function createDefaultFormContext(): FormContext {
    sectionFilter: '',
    showValidation: false,
    validationErrors: []
