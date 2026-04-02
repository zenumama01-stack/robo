// Re-export shared types from the main types file to avoid duplication
// These are used by both EntityGridComponent and EntityDataGridComponent
  SortState,
  // View grid types - re-exported from types.ts which gets them from core-entities
  ViewGridState
} from '../../types';
// Enums and Union Types
 * Selection mode for the grid
 * - 'none': No selection allowed
 * - 'single': Only one row can be selected at a time
 * - 'multiple': Multiple rows can be selected (click to toggle)
 * - 'checkbox': Checkbox column for selection
export type GridSelectionMode = 'none' | 'single' | 'multiple' | 'checkbox';
 * Edit mode for the grid
 * - 'none': No editing allowed
 * - 'cell': Individual cell editing
 * - 'row': Full row editing
 * - 'batch': Batch editing with explicit save
export type GridEditMode = 'none' | 'cell' | 'row' | 'batch';
 * Grid lines display mode
export type GridLinesMode = 'none' | 'horizontal' | 'vertical' | 'both';
 * Filter operators for column filtering
export type FilterOperator =
  | 'eq' | 'neq'
  | 'gt' | 'gte' | 'lt' | 'lte'
  | 'contains' | 'startswith' | 'endswith'
  | 'isnull' | 'isnotnull'
  | 'in' | 'notin';
 * Column data type for formatting and editing
export type GridColumnType =
  | 'string'
  | 'number'
  | 'boolean'
  | 'date'
  | 'datetime'
  | 'currency'
  | 'percent'
  | 'custom';
// Column Configuration
 * Configuration for a single grid column
export interface GridColumnConfig {
  /** Field name on the entity */
  /** Display title (defaults to field name) */
  /** Column width in pixels (or 'auto') */
  width?: number | 'auto';
  /** Minimum width for resizing */
  /** Maximum width for resizing */
  /** Column is visible */
  /** Column is sortable */
  /** Column is filterable */
  filterable?: boolean;
  /** Column is editable */
  editable?: boolean;
  /** Column is resizable */
  /** Column is reorderable */
  reorderable?: boolean;
  /** Data type for formatting and editing */
  type?: GridColumnType;
  /** Format string (e.g., 'yyyy-MM-dd' for dates, '#,##0.00' for numbers) */
  /** Text alignment */
  align?: 'left' | 'center' | 'right';
  /** Header alignment (defaults to align) */
  headerAlign?: 'left' | 'center' | 'right';
  /** CSS class for cells - can be string or function */
  cellClass?: string | ((row: Record<string, unknown>, column: GridColumnConfig) => string);
  /** CSS class for header */
  headerClass?: string;
  /** Custom cell template reference */
  cellTemplate?: TemplateRef<GridCellTemplateContext>;
  /** Custom header template reference */
  headerTemplate?: TemplateRef<GridHeaderTemplateContext>;
  /** Custom editor template reference */
  editorTemplate?: TemplateRef<GridEditorTemplateContext>;
  /** Cell value formatter function */
  formatter?: (value: unknown, row: Record<string, unknown>, column: GridColumnConfig) => string;
  /** Cell style function */
  cellStyle?: (row: Record<string, unknown>, column: GridColumnConfig) => Record<string, string>;
  /** Whether to show tooltip on hover */
  /** Custom tooltip content */
  tooltip?: string | ((row: Record<string, unknown>, column: GridColumnConfig) => string);
  /** Frozen column position (alias for pinned, for backward compatibility) */
  frozen?: 'left' | 'right' | false;
  /** Column pinning position (AG Grid terminology) */
  pinned?: 'left' | 'right' | null;
  /** Flex grow factor for auto-sizing columns */
  flex?: number;
  /** Column group (for grouped headers) */
  /** Filter options for dropdown filters */
  filterOptions?: Array<{ value: unknown; label: string }>;
  /** Sort order index (for default multi-sort) */
  sortIndex?: number;
  /** Sort direction (for default sort) */
  sortDirection?: 'asc' | 'desc';
 * Context provided to custom cell templates
export interface GridCellTemplateContext {
  row: Record<string, unknown>;
  /** Column configuration */
  column: GridColumnConfig;
  /** The cell value */
  value: unknown;
  /** Row index in the current view */
  rowIndex: number;
  /** Whether the cell is currently being edited */
 * Context provided to custom header templates
export interface GridHeaderTemplateContext {
  /** Current sort direction for this column */
  sortDirection: 'asc' | 'desc' | 'none';
  /** Current filter value for this column */
  filterValue: unknown;
 * Context provided to custom editor templates
export interface GridEditorTemplateContext {
  /** Current cell value */
  /** Function to commit the edit with a new value */
  commitEdit: (newValue: unknown) => void;
  /** Function to cancel the edit */
  cancelEdit: () => void;
// Toolbar Configuration
 * Configuration for the grid toolbar
export interface GridToolbarConfig {
  /** Show search input */
  /** Search placeholder text */
  searchPlaceholder?: string;
  /** Search debounce time in ms */
  searchDebounce?: number;
  /** Show refresh button */
  showRefresh?: boolean;
  /** Show add button */
  showAdd?: boolean;
  /** Show delete button (for selected rows) */
  showDelete?: boolean;
  /** Show export button */
  showExport?: boolean;
  /** Export formats available */
  exportFormats?: Array<'excel' | 'csv' | 'json'>;
  /** Show column chooser button */
  showColumnChooser?: boolean;
  /** Show filter toggle button */
  showFilterToggle?: boolean;
  /** Custom toolbar buttons */
  customButtons?: GridToolbarButton[];
  /** Toolbar position */
  position?: 'top' | 'bottom' | 'both';
  /** Show row count */
  showRowCount?: boolean;
  /** Show selection count */
  showSelectionCount?: boolean;
 * Custom toolbar button configuration
export interface GridToolbarButton {
  /** Unique button ID */
  /** Button text */
  /** Button icon (Font Awesome class) */
  /** Button tooltip */
  /** Button is disabled - can be boolean or function */
  disabled?: boolean | (() => boolean);
  /** Button is visible - can be boolean or function */
  visible?: boolean | (() => boolean);
  /** Button CSS class */
  /** Button position: 'left' | 'right' */
  /** Click handler (if not using event) */
  onClick?: () => void;
// State Types (Extended for EntityDataGrid)
 * Extended sort state with multi-sort index
 * Extends the base SortState from types.ts with an index for multi-sort ordering
export interface DataGridSortState {
  /** Field name being sorted */
  /** Index for multi-sort ordering */
 * Filter state for a column
export interface FilterState {
  /** Field name being filtered */
  /** Filter operator */
  operator: FilterOperator;
  /** Filter value */
 * Pending change for batch editing
export interface PendingChange {
  /** Row key (usually ID) */
  rowKey: string;
  /** The row entity */
  /** Field that was changed */
  oldValue: unknown;
  /** New value */
  newValue: unknown;
  /** Type of change */
  changeType: 'update' | 'insert' | 'delete';
 * Complete grid state for persistence (internal to EntityDataGrid)
 * Note: For UserView persistence, use ViewGridState from @memberjunction/core-entities
export interface GridState {
  /** Column states */
  columns: Array<{
    /** Column pinning position */
    /** Flex grow factor for auto-sizing */
  /** Sort state */
  sort: DataGridSortState[];
  /** Filter state */
  filters: FilterState[];
  /** Selected row keys */
  selection: string[];
 * Export options for grid data
  /** Export only visible columns */
  visibleColumnsOnly?: boolean;
  /** Export only selected rows */
  selectedRowsOnly?: boolean;
  /** Include headers */
  includeHeaders?: boolean;
  /** Custom column mapping */
  columnMapping?: Record<string, string>;
 * Event emitted when a foreign key link is clicked in the grid.
 * The parent component should handle navigation to the related record.
export interface ForeignKeyClickEvent {
  /** The ID of the related entity (from EntityFieldInfo.RelatedEntityID) */
  relatedEntityId: string;
  /** The ID of the related record (the FK value) */
  /** The field name that was clicked */
  /** The entity name of the related entity (if available) */
  relatedEntityName?: string;
// Internal Types
 * Internal representation of a rendered row
export interface GridRowData {
  /** Row index in the data array */
  /** The entity object */
  entity: Record<string, unknown>;
  /** Whether the row is selected */
  /** Whether the row is being edited */
  editing: boolean;
  /** Whether the row has unsaved changes */
  dirty: boolean;
  /** CSS classes for the row */
  cssClasses: string[];
 * Virtual scroll viewport info
export interface VirtualScrollState {
  /** First visible row index */
  /** Last visible row index */
  /** Scroll offset from top */
  scrollTop: number;
  /** Total scroll height */
  totalHeight: number;
  /** Viewport height */
  viewportHeight: number;
  /** Number of buffer rows above and below */
  bufferSize: number;
 * Column runtime state (internal)
export interface ColumnRuntimeState {
  config: GridColumnConfig;
  /** Computed width */
  computedWidth: number;
  /** Current sort direction */
  /** Sort index for multi-sort */
  sortIndex: number;
  /** Current filter value */
  /** Current visibility */
  /** Current order/position */
 * RunView parameters subset for data loading
export interface GridRunViewParams {
  /** Extra filter clause */
  /** Order by clause */
  /** Maximum rows to fetch */
  maxRows?: number;
  /** Fields to retrieve */
  /** User search string */
  searchString?: string;
// Entity Action Types
 * Configuration for an entity action displayed in the grid toolbar.
 * This is a simplified representation of EntityAction for the UI layer.
export interface EntityActionConfig {
  /** Unique action ID */
  /** Optional icon (Font Awesome class) */
  /** Whether the action requires selected records */
  requiresSelection?: boolean;
  /** Minimum number of selected records required */
  minSelectedRecords?: number;
  /** Maximum number of selected records allowed */
  maxSelectedRecords?: number;
  /** Invocation type (e.g., 'View', 'SingleRecord', 'MultiRecord') */
  invocationType?: string;
  /** Additional action metadata */
// Visual Customization Types
 * Header style preset options
export type GridHeaderStyle = 'flat' | 'elevated' | 'gradient' | 'bold';
 * Configuration for grid visual appearance
 * All properties are optional - defaults provide an attractive "out of the box" experience
export interface GridVisualConfig {
  // Header Styling
  /** Header style preset: 'flat' | 'elevated' | 'gradient' | 'bold' */
  headerStyle?: GridHeaderStyle;
  /** Custom header background color (overrides preset) */
  headerBackground?: string;
  /** Custom header text color */
  headerTextColor?: string;
  /** Show bottom shadow/border on header */
  headerShadow?: boolean;
  // Row Styling
  /** Enable alternating row colors (zebra striping) */
  alternateRows?: boolean;
  /** Contrast level for alternating rows: 'subtle' | 'medium' | 'strong' */
  alternateRowContrast?: 'subtle' | 'medium' | 'strong';
  /** Enable smooth hover transitions */
  hoverTransitions?: boolean;
  /** Hover transition duration in ms */
  hoverTransitionDuration?: number;
  // Cell Formatting
  /** Right-align numeric columns automatically */
  rightAlignNumbers?: boolean;
  /** Format dates with a friendly format (e.g., "Jan 15, 2024" instead of "2024-01-15") */
  friendlyDates?: boolean;
  /** Render email cells as clickable mailto links */
  clickableEmails?: boolean;
  /** Render boolean cells as checkmark/x icons instead of text */
  booleanIcons?: boolean;
  /** Render URL cells as clickable links */
  clickableUrls?: boolean;
  // Selection Styling
  /** Color for selection indicator (left border on selected rows) */
  selectionIndicatorColor?: string;
  /** Width of selection indicator in pixels */
  selectionIndicatorWidth?: number;
  /** Selection background color */
  selectionBackground?: string;
  // Checkbox Column
  /** Style for checkbox column: 'default' | 'rounded' | 'filled' */
  checkboxStyle?: 'default' | 'rounded' | 'filled';
  /** Checkbox accent color */
  checkboxColor?: string;
  // Loading States
  /** Show skeleton loading rows instead of spinner */
  skeletonLoading?: boolean;
  /** Number of skeleton rows to show */
  skeletonRowCount?: number;
  // Borders & Spacing
  /** Border radius for the grid container */
  borderRadius?: number;
  /** Cell padding preset: 'compact' | 'normal' | 'comfortable' */
  cellPadding?: 'compact' | 'normal' | 'comfortable';
  // Accent Color (used for sort indicators, focus states, etc.)
  /** Primary accent color for interactive elements */
  accentColor?: string;
 * Default visual configuration - provides attractive defaults out of the box
export const DEFAULT_VISUAL_CONFIG: Required<GridVisualConfig> = {
  // Header - elevated style with shadow
  headerStyle: 'elevated',
  headerBackground: '',  // Empty = use CSS variable
  headerTextColor: '',   // Empty = use CSS variable
  headerShadow: true,
  // Rows - medium contrast zebra striping with transitions
  alternateRows: true,
  alternateRowContrast: 'medium',
  hoverTransitions: true,
  hoverTransitionDuration: 150,
  // Cell formatting - smart defaults
  rightAlignNumbers: true,
  friendlyDates: true,
  clickableEmails: true,
  booleanIcons: true,
  clickableUrls: true,
  // Selection - mellow yellow accent (avoids conflict with blue hyperlinks)
  selectionIndicatorColor: '#f9a825',
  selectionIndicatorWidth: 3,
  selectionBackground: '#fff9e6',
  // Checkbox - rounded style
  checkboxStyle: 'rounded',
  checkboxColor: '#2196F3',
  // Loading - skeleton for modern feel
  skeletonLoading: true,
  skeletonRowCount: 8,
  borderRadius: 0,
  cellPadding: 'normal',
  // Accent color
  accentColor: '#2196F3'
