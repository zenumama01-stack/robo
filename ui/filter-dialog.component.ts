import { Component, Input, Output, EventEmitter, OnInit, HostListener } from '@angular/core';
 * FilterDialogComponent - Modal dialog for editing filters
 * Provides a full-width editing experience for the filter builder,
 * suitable for complex filter expressions.
  selector: 'mj-filter-dialog',
  templateUrl: './filter-dialog.component.html',
  styleUrls: ['./filter-dialog.component.css']
export class FilterDialogComponent implements OnInit {
   * Whether the dialog is open
  @Input() isOpen: boolean = false;
   * Available fields to filter on
  @Input() fields: FilterFieldInfo[] = [];
   * Current filter state
  @Input() filter: CompositeFilterDescriptor = createEmptyFilter();
   * Whether the filter builder is disabled (read-only mode)
  @Input() disabled: boolean = false;
   * Emitted when the dialog should close
   * Emitted when the filter is applied
  @Output() apply = new EventEmitter<CompositeFilterDescriptor>();
   * Internal working copy of the filter
  public workingFilter: CompositeFilterDescriptor = createEmptyFilter();
   * Handle Escape key to close dialog
  handleEscape(): void {
    if (this.isOpen) {
    this.initializeWorkingFilter();
   * Initialize working filter when input changes
  ngOnChanges(): void {
   * Create a working copy of the filter
  private initializeWorkingFilter(): void {
    // Deep clone to avoid mutating the original
    this.workingFilter = JSON.parse(JSON.stringify(this.filter || createEmptyFilter()));
   * Handle filter changes from the filter builder
  onFilterChange(filter: CompositeFilterDescriptor): void {
    this.workingFilter = filter;
   * Apply the filter and close
  onApply(): void {
    this.apply.emit(this.workingFilter);
   * Cancel and close without saving
  onCancel(): void {
   * Clear all filters
  onClear(): void {
    this.workingFilter = createEmptyFilter();
   * Get filter count for display
  getFilterCount(): number {
    return this.countFilters(this.workingFilter);
   * Count filters recursively
  private countFilters(filter: CompositeFilterDescriptor): number {
    for (const item of filter.filters || []) {
      if ('logic' in item && 'filters' in item) {
        count += this.countFilters(item as CompositeFilterDescriptor);
      } else if ('field' in item) {
