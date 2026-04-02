import { Component, Input, Output, EventEmitter, OnInit, ViewEncapsulation } from '@angular/core';
 * FilterGroupComponent - A group of filter rules with AND/OR logic
 * Supports nested groups for complex filter expressions.
  selector: 'mj-filter-group',
  templateUrl: './filter-group.component.html',
  styleUrls: ['./filter-group.component.css'],
export class FilterGroupComponent implements OnInit {
   * The composite filter descriptor for this group
  @Input() filter!: CompositeFilterDescriptor;
   * Whether this is the root group (affects delete button visibility)
  @Input() isRoot: boolean = false;
   * Nesting depth (for visual indication)
   * Maximum nesting depth allowed
  @Input() maxDepth: number = 3;
   * Emitted when the delete button is clicked (for nested groups)
    // Ensure filter has at least one rule
    if (!this.filter.filters || this.filter.filters.length === 0) {
      this.addRule();
   * Toggle the logic between AND and OR
  toggleLogic(): void {
    const newLogic: FilterLogic = this.filter.logic === 'and' ? 'or' : 'and';
    this.emitChange({
      ...this.filter,
      logic: newLogic
   * Set specific logic
  setLogic(logic: FilterLogic): void {
    if (this.filter.logic !== logic) {
        logic
   * Add a new filter rule
  addRule(): void {
    const defaultField = this.fields[0]?.name || '';
    const newRule = createFilterRule(defaultField);
      filters: [...(this.filter.filters || []), newRule]
   * Add a new nested group
  addGroup(): void {
    if (this.depth >= this.maxDepth) return;
    const newGroup: CompositeFilterDescriptor = {
      filters: [createFilterRule(defaultField)]
      filters: [...(this.filter.filters || []), newGroup]
   * Handle rule change at specified index
  onRuleChange(index: number, updatedRule: FilterDescriptor): void {
    const filters = [...(this.filter.filters || [])];
    filters[index] = updatedRule;
      filters
   * Handle nested group change at specified index
  onGroupChange(index: number, updatedGroup: CompositeFilterDescriptor): void {
    filters[index] = updatedGroup;
   * Delete filter at specified index
  deleteFilter(index: number): void {
    filters.splice(index, 1);
    // Ensure at least one rule remains if this is the root
    if (this.isRoot && filters.length === 0) {
      filters.push(createFilterRule(defaultField));
   * Delete this group (only for nested groups)
   * Check if a filter is a composite (group) filter
  isGroup(filter: FilterDescriptor | CompositeFilterDescriptor): boolean {
    return isCompositeFilter(filter);
   * Type guard cast to FilterDescriptor
  asRule(filter: FilterDescriptor | CompositeFilterDescriptor): FilterDescriptor {
    return filter as FilterDescriptor;
   * Type guard cast to CompositeFilterDescriptor
  asGroup(filter: FilterDescriptor | CompositeFilterDescriptor): CompositeFilterDescriptor {
    return filter as CompositeFilterDescriptor;
   * Check if we can add more nested groups
  canAddGroup(): boolean {
    return this.depth < this.maxDepth;
   * Emit the filter change event
  private emitChange(filter: CompositeFilterDescriptor): void {
   * Track by function for ngFor optimization
  trackByIndex(index: number): number {
