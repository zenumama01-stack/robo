import { GridColumnConfig, DataGridSortState, GridRunViewParams } from '../models/grid-types';
// Forward declaration to avoid circular dependency
// The actual component type is set at runtime
export type GridComponentRef = unknown;
// Base Event Classes
 * Base class for all grid events
export class GridEventArgs {
  /** The grid component that raised the event */
  readonly grid: GridComponentRef;
  /** Timestamp when event was raised */
  readonly timestamp: Date;
  constructor(grid: GridComponentRef) {
    this.grid = grid;
    this.timestamp = new Date();
 * Base class for cancelable events (Before events)
export class CancelableGridEventArgs extends GridEventArgs {
  /** Set to true to cancel the operation */
  cancel: boolean = false;
  /** Optional reason for cancellation (for logging/debugging) */
  cancelReason?: string;
    super(grid);
 * Base class for row-related events
export class RowEventArgs extends GridEventArgs {
  /** The row data (entity) */
  readonly row: Record<string, unknown>;
  /** The row index in the current view */
  readonly rowIndex: number;
  /** The row key (ID) */
  readonly rowKey: string;
  constructor(grid: GridComponentRef, row: Record<string, unknown>, rowIndex: number, rowKey: string) {
    this.row = row;
    this.rowIndex = rowIndex;
    this.rowKey = rowKey;
 * Base class for cancelable row events
export class CancelableRowEventArgs extends RowEventArgs {
  /** Optional reason for cancellation */
    super(grid, row, rowIndex, rowKey);
// Row Selection Events
 * Fired before a row is selected - can be canceled
export class BeforeRowSelectEventArgs extends CancelableRowEventArgs {
  /** Whether this is adding to selection (multi-select) or replacing */
  readonly isAdditive: boolean;
  /** Current selection before this change */
  readonly currentSelection: string[];
    grid: GridComponentRef,
    row: Record<string, unknown>,
    rowIndex: number,
    rowKey: string,
    isAdditive: boolean,
    currentSelection: string[]
    this.isAdditive = isAdditive;
    this.currentSelection = [...currentSelection];
 * Fired after a row is selected
export class AfterRowSelectEventArgs extends RowEventArgs {
  /** Whether this was additive selection */
  readonly wasAdditive: boolean;
  /** New selection state */
  readonly newSelection: string[];
  /** Previous selection state */
  readonly previousSelection: string[];
    wasAdditive: boolean,
    newSelection: string[],
    previousSelection: string[]
    this.wasAdditive = wasAdditive;
    this.newSelection = [...newSelection];
    this.previousSelection = [...previousSelection];
 * Fired before a row is deselected - can be canceled
export class BeforeRowDeselectEventArgs extends CancelableRowEventArgs {
 * Fired after a row is deselected
export class AfterRowDeselectEventArgs extends RowEventArgs {
// Row Click Events
 * Fired before a row click is processed - can be canceled
export class BeforeRowClickEventArgs extends CancelableRowEventArgs {
  readonly mouseEvent: MouseEvent;
  /** The column that was clicked (if any) */
  readonly column?: GridColumnConfig;
  /** The cell value that was clicked (if any) */
  readonly cellValue?: unknown;
    mouseEvent: MouseEvent,
    column?: GridColumnConfig,
    cellValue?: unknown
    this.mouseEvent = mouseEvent;
    this.column = column;
    this.cellValue = cellValue;
 * Fired after a row click
export class AfterRowClickEventArgs extends RowEventArgs {
 * Fired before a row double-click - can be canceled
export class BeforeRowDoubleClickEventArgs extends CancelableRowEventArgs {
 * Fired after a row double-click
export class AfterRowDoubleClickEventArgs extends RowEventArgs {
// Cell Editing Events
 * Fired before cell edit begins - can be canceled
export class BeforeCellEditEventArgs extends CancelableRowEventArgs {
  /** The column being edited */
  readonly column: GridColumnConfig;
  /** Current value in the cell */
  readonly currentValue: unknown;
    column: GridColumnConfig,
    currentValue: unknown
    this.currentValue = currentValue;
 * Fired after cell edit begins
export class AfterCellEditBeginEventArgs extends RowEventArgs {
 * Fired before cell edit is committed - can be canceled or value modified
export class BeforeCellEditCommitEventArgs extends CancelableRowEventArgs {
  /** Original value before edit */
  readonly oldValue: unknown;
  /** New value being committed */
  readonly newValue: unknown;
  /** Set to modify the value before committing */
  modifiedValue?: unknown;
    oldValue: unknown,
    newValue: unknown
    this.oldValue = oldValue;
    this.newValue = newValue;
 * Fired after cell edit is committed
export class AfterCellEditCommitEventArgs extends RowEventArgs {
  /** The column that was edited */
  /** New value that was committed */
 * Fired before cell edit is canceled - can be canceled (forcing commit instead)
export class BeforeCellEditCancelEventArgs extends CancelableRowEventArgs {
  /** Original value */
  readonly originalValue: unknown;
  /** The edited value that would be discarded */
  readonly editedValue: unknown;
    originalValue: unknown,
    editedValue: unknown
    this.originalValue = originalValue;
    this.editedValue = editedValue;
 * Fired after cell edit is canceled
export class AfterCellEditCancelEventArgs extends RowEventArgs {
  /** The column that was being edited */
  /** Original value that was restored */
  /** The edited value that was discarded */
// Row Save/Delete Events
 * Fired before row save - can be canceled or values modified
export class BeforeRowSaveEventArgs extends CancelableRowEventArgs {
  /** The changes being saved */
  readonly changes: Record<string, { oldValue: unknown; newValue: unknown }>;
  /** Set to modify values before saving */
  modifiedValues?: Record<string, unknown>;
    changes: Record<string, { oldValue: unknown; newValue: unknown }>
    this.changes = { ...changes };
 * Fired after row save
export class AfterRowSaveEventArgs extends RowEventArgs {
  /** The changes that were saved */
  readonly success: boolean;
  readonly error?: string;
    changes: Record<string, { oldValue: unknown; newValue: unknown }>,
    error?: string
 * Fired before row delete - can be canceled
export class BeforeRowDeleteEventArgs extends CancelableRowEventArgs {
  /** Whether this is part of a batch delete */
  readonly isBatch: boolean;
    isBatch: boolean
    this.isBatch = isBatch;
 * Fired after row delete
export class AfterRowDeleteEventArgs extends RowEventArgs {
  /** Whether the delete was successful */
  /** Error message if delete failed */
// Data Loading Events
 * Fired before data load - can be canceled or params modified
export class BeforeDataLoadEventArgs extends CancelableGridEventArgs {
  /** The RunView parameters that will be used */
  readonly params: GridRunViewParams;
  /** Set to modify the parameters before loading */
  modifiedParams?: Partial<GridRunViewParams>;
  constructor(grid: GridComponentRef, params: GridRunViewParams) {
    this.params = { ...params };
 * Fired after data load
export class AfterDataLoadEventArgs extends GridEventArgs {
  /** The RunView parameters that were used */
  /** Whether the load was successful */
  /** Total number of rows matching the query */
  readonly totalRowCount: number;
  /** Number of rows actually loaded (may be limited by maxRows) */
  readonly loadedRowCount: number;
  readonly loadTimeMs: number;
  /** Error message if load failed */
    params: GridRunViewParams,
    loadedRowCount: number,
    loadTimeMs: number,
    this.totalRowCount = totalRowCount;
    this.loadedRowCount = loadedRowCount;
    this.loadTimeMs = loadTimeMs;
 * Fired before data refresh (reload) - can be canceled
export class BeforeDataRefreshEventArgs extends CancelableGridEventArgs {
  /** Whether this is an auto-refresh from parameter changes */
  readonly isAutoRefresh: boolean;
  constructor(grid: GridComponentRef, isAutoRefresh: boolean) {
    this.isAutoRefresh = isAutoRefresh;
 * Fired after data refresh
export class AfterDataRefreshEventArgs extends GridEventArgs {
  /** Whether the refresh was successful */
  /** Total number of rows after refresh */
  /** Time taken to refresh in milliseconds */
    loadTimeMs: number
// Sorting Events
 * Fired before sort - can be canceled
export class BeforeSortEventArgs extends CancelableGridEventArgs {
  /** The column being sorted */
  /** The new sort direction */
  readonly direction: 'asc' | 'desc' | 'none';
  /** Whether this is a multi-sort operation (shift+click) */
  readonly isMultiSort: boolean;
  /** Current sort state before change */
  readonly currentSortState: DataGridSortState[];
    direction: 'asc' | 'desc' | 'none',
    isMultiSort: boolean,
    currentSortState: DataGridSortState[]
    this.direction = direction;
    this.isMultiSort = isMultiSort;
    this.currentSortState = [...currentSortState];
 * Fired after sort
export class AfterSortEventArgs extends GridEventArgs {
  /** The column that was sorted */
  /** New sort state */
  readonly newSortState: DataGridSortState[];
    newSortState: DataGridSortState[]
    this.newSortState = [...newSortState];
// Column Events
 * Fired before column reorder - can be canceled
export class BeforeColumnReorderEventArgs extends CancelableGridEventArgs {
  /** The column being moved */
  /** Original index */
  readonly fromIndex: number;
  /** Target index */
  readonly toIndex: number;
    fromIndex: number,
    toIndex: number
    this.fromIndex = fromIndex;
    this.toIndex = toIndex;
 * Fired after column reorder
export class AfterColumnReorderEventArgs extends GridEventArgs {
  /** The column that was moved */
  /** New index */
 * Fired before column resize - can be canceled
export class BeforeColumnResizeEventArgs extends CancelableGridEventArgs {
  /** The column being resized */
  /** Original width */
  readonly oldWidth: number;
  /** New width */
  readonly newWidth: number;
    oldWidth: number,
    newWidth: number
    this.oldWidth = oldWidth;
    this.newWidth = newWidth;
 * Fired after column resize
export class AfterColumnResizeEventArgs extends GridEventArgs {
  /** The column that was resized */
 * Fired before column visibility change - can be canceled
export class BeforeColumnVisibilityChangeEventArgs extends CancelableGridEventArgs {
  /** The column being shown/hidden */
  /** New visibility state */
  readonly newVisibility: boolean;
    newVisibility: boolean
    this.newVisibility = newVisibility;
 * Fired after column visibility change
export class AfterColumnVisibilityChangeEventArgs extends GridEventArgs {
  /** The column that was shown/hidden */
