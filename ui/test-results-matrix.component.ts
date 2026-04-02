import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
  MatrixCellData,
  MatrixColumnData,
  MatrixRowData,
  TestResultsMatrixData
} from '../../models/testing.models';
 * Event emitted when a cell is clicked
export interface MatrixCellClickEvent {
  suiteRunId: string;
 * Event emitted when a row header (test name) is clicked
export interface MatrixRowClickEvent {
 * Event emitted when a column header (suite run) is clicked
export interface MatrixColumnClickEvent {
 * Reusable Test Results Matrix Component
 * Displays a matrix view of test results across multiple suite runs.
 * - Rows: Individual tests
 * - Columns: Suite runs (sorted by date, most recent first)
 * - Cells: Color-coded status with click navigation
 * <mj-test-results-matrix
 *   [data]="matrixData"
 *   [loading]="isLoading"
 *   [maxColumns]="10"
 *   [showTags]="true"
 *   [showPassRate]="true"
 *   (cellClick)="onCellClick($event)"
 *   (rowClick)="onTestClick($event)"
 *   (columnClick)="onSuiteRunClick($event)">
 * </mj-test-results-matrix>
  selector: 'mj-test-results-matrix',
    <div class="matrix-container" [class.loading]="loading">
        <div class="matrix-loading">
          <span>Loading matrix data...</span>
      @if (!loading && (!data || data.columns.length === 0)) {
        <div class="matrix-empty">
          <h4>{{ emptyTitle }}</h4>
          <p>{{ emptyMessage }}</p>
      <!-- Matrix table -->
      @if (!loading && data && data.columns.length > 0) {
        <div class="matrix-wrapper">
              <tr class="header-row">
                <th class="test-name-header sticky-col">
                    <span>Test</span>
                @for (col of data.columns; track trackColumn(i, col); let i = $index) {
                  <th class="run-header"
                    [class.highlighted]="highlightedColumn === col.suiteRunId"
                    (click)="onColumnHeaderClick(col)"
                    (mouseenter)="highlightedColumn = col.suiteRunId"
                    (mouseleave)="highlightedColumn = null">
                      @if (showTags && col.tags.length > 0) {
                          @for (tag of col.tags.slice(0, 2); track tag) {
                          @if (col.tags.length > 2) {
                            <span class="tag-more">+{{ col.tags.length - 2 }}</span>
                      <div class="run-date">{{ formatDate(col.date) }}</div>
                      @if (showPassRate) {
                        <div class="run-pass-rate">
                          <span class="pass-rate-value" [class.high]="col.passRate >= 80" [class.medium]="col.passRate >= 50 && col.passRate < 80" [class.low]="col.passRate < 50">
                            {{ col.passRate.toFixed(0) }}%
              @for (row of data.rows; track trackRow($index, row)) {
                <tr class="data-row"
                  [class.highlighted]="highlightedRow === row.testId"
                  (mouseenter)="highlightedRow = row.testId"
                  (mouseleave)="highlightedRow = null">
                  <td class="test-name-cell sticky-col"
                    (click)="onRowHeaderClick(row)">
                    <div class="test-name-content" [title]="row.testName">
                      <span class="test-name-text">{{ row.testName }}</span>
                  @for (col of data.columns; track trackColumn($index, col)) {
                      (click)="onCellClicked(row, col)">
                      <div class="cell-content"
                        [ngClass]="getCellClass(getCell(row.testId, col))"
                        [title]="getCellTooltip(getCell(row.testId, col))">
                        <i class="cell-icon" [ngClass]="getCellIcon(getCell(row.testId, col))"></i>
                        @if (showScores && getCell(row.testId, col)?.score != null) {
                          <span class="cell-score">
                            {{ ((getCell(row.testId, col)?.score ?? 0) * 100).toFixed(0) }}%
      @if (!loading && data && data.columns.length > 0 && showLegend) {
        <div class="matrix-legend">
          <span class="legend-item pending"><i class="fas fa-clock"></i> Pending</span>
          <span class="legend-item none"><i class="fas fa-minus"></i> Not Run</span>
    .matrix-container {
    .matrix-container.loading {
    .matrix-loading {
    .matrix-empty {
    .matrix-empty h4 {
    .matrix-empty p {
    /* Matrix Wrapper */
    .matrix-wrapper {
    /* Matrix Table */
      border-spacing: 0;
    /* Sticky Column */
    .sticky-col {
      border-right: 2px solid #e2e8f0;
    /* Header Row */
    .test-name-header {
    .header-content i {
    /* Run Header (Column) */
      max-width: 140px;
      border-left: 1px solid #e2e8f0;
      vertical-align: bottom;
    .run-header:hover,
    .run-header.highlighted {
    .pass-rate-value {
    .pass-rate-value.high {
    .pass-rate-value.medium {
    .pass-rate-value.low {
    /* Data Rows */
    .data-row {
    .data-row:hover,
    .data-row.highlighted {
    .data-row:hover .sticky-col,
    .data-row.highlighted .sticky-col {
    /* Test Name Cell */
    .test-name-cell:hover {
    .test-name-content {
    .test-name-text {
    /* Result Cells */
      border-left: 1px solid #f1f5f9;
    .result-cell:hover {
    .result-cell.highlighted {
      background: rgba(59, 130, 246, 0.05);
    .cell-content {
    .cell-icon {
    /* Cell Status Colors */
    .cell-passed {
    .cell-failed {
      background: rgba(239, 68, 68, 0.15);
    .cell-error {
      background: rgba(245, 158, 11, 0.15);
    .cell-skipped {
      background: rgba(107, 114, 128, 0.15);
    .cell-pending {
      background: rgba(139, 92, 246, 0.15);
    .cell-running {
      background: rgba(59, 130, 246, 0.15);
    .cell-none {
    /* Legend */
    .matrix-legend {
    .legend-item i {
    .legend-item.passed i { color: #10b981; }
    .legend-item.failed i { color: #ef4444; }
    .legend-item.error i { color: #f59e0b; }
    .legend-item.skipped i { color: #6b7280; }
    .legend-item.pending i { color: #8b5cf6; }
    .legend-item.none i { color: #cbd5e1; }
      .test-name-header,
        max-width: 100px;
        height: 30px;
export class TestResultsMatrixComponent {
  /** Matrix data to display */
  @Input() data: TestResultsMatrixData | null = null;
  @Input() loading = false;
  /** Show tags in column headers */
  @Input() showTags = true;
  /** Show pass rate in column headers */
  @Input() showPassRate = true;
  /** Show scores in cells */
  @Input() showScores = false;
  /** Show legend below matrix */
  /** Empty state title */
  @Input() emptyTitle = 'No Data Available';
  /** Empty state message */
  @Input() emptyMessage = 'Test results will appear here once suite runs are completed.';
  /** Emitted when a cell is clicked */
  @Output() cellClick = new EventEmitter<MatrixCellClickEvent>();
  /** Emitted when a row header (test name) is clicked */
  @Output() rowClick = new EventEmitter<MatrixRowClickEvent>();
  /** Emitted when a column header (suite run) is clicked */
  @Output() columnClick = new EventEmitter<MatrixColumnClickEvent>();
  // Highlight tracking
  highlightedRow: string | null = null;
  highlightedColumn: string | null = null;
   * Get the cell data for a specific test and suite run
  getCell(testId: string, column: MatrixColumnData): MatrixCellData | null {
    return column.testResults.get(testId) ?? null;
   * Get CSS class for cell based on status
  getCellClass(cell: MatrixCellData | null): string {
    if (!cell) return 'cell-none';
    switch (cell.status) {
      case 'Skipped': return 'cell-skipped';
      case 'Pending': return 'cell-pending';
      default: return 'cell-none';
   * Get icon class for cell based on status
  getCellIcon(cell: MatrixCellData | null): string {
    if (!cell) return 'fas fa-minus';
      case 'Passed': return 'fas fa-check';
      case 'Failed': return 'fas fa-times';
      case 'Error': return 'fas fa-exclamation';
      case 'Skipped': return 'fas fa-forward';
      case 'Running': return 'fas fa-spinner fa-spin';
      case 'Pending': return 'fas fa-clock';
      default: return 'fas fa-minus';
   * Get tooltip text for cell
  getCellTooltip(cell: MatrixCellData | null): string {
    if (!cell) return 'Not run in this suite run';
    let tooltip = `${cell.testName}\nStatus: ${cell.status}`;
    if (cell.score != null) tooltip += `\nScore: ${(cell.score * 100).toFixed(1)}%`;
    if (cell.duration != null) tooltip += `\nDuration: ${cell.duration.toFixed(1)}s`;
    if (cell.cost != null) tooltip += `\nCost: $${cell.cost.toFixed(4)}`;
   * Format date for column header
    if (diffDays === 0) {
      return d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    } else if (diffDays === 1) {
    } else if (diffDays < 7) {
      return `${diffDays}d ago`;
      return d.toLocaleDateString([], { month: 'short', day: 'numeric' });
   * Handle cell click
  onCellClicked(row: MatrixRowData, column: MatrixColumnData): void {
    const cell = this.getCell(row.testId, column);
    if (cell) {
      this.cellClick.emit({
        testRunId: cell.testRunId,
        testId: cell.testId,
        testName: cell.testName,
        suiteRunId: column.suiteRunId,
        status: cell.status
   * Handle row header click
  onRowHeaderClick(row: MatrixRowData): void {
    this.rowClick.emit({
      testId: row.testId,
      testName: row.testName
   * Handle column header click
  onColumnHeaderClick(column: MatrixColumnData): void {
    this.columnClick.emit({
      date: column.date,
      tags: column.tags
   * TrackBy function for columns
  trackColumn(index: number, col: MatrixColumnData): string {
    return col.suiteRunId;
   * TrackBy function for rows
  trackRow(index: number, row: MatrixRowData): string {
    return row.testId;
