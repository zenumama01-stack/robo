import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges, ViewEncapsulation, HostListener, ElementRef, ViewChild } from '@angular/core';
  FilterOperator
import { getOperatorsForType, OperatorInfo, operatorRequiresValue } from '../types/operators';
 * FilterRuleComponent - A single filter condition row
 * Displays field selector, operator selector, and value editor
 * based on the field type.
  selector: 'mj-filter-rule',
  templateUrl: './filter-rule.component.html',
  styleUrls: ['./filter-rule.component.css'],
export class FilterRuleComponent implements OnInit, OnChanges {
   * The filter descriptor for this rule
  @Input() filter!: FilterDescriptor;
   * Whether to show the delete button
  @Input() showDelete: boolean = true;
  @Output() filterChange = new EventEmitter<FilterDescriptor>();
   * Emitted when the delete button is clicked
   * Currently selected field info
  public selectedField: FilterFieldInfo | null = null;
   * Available operators for the selected field type
  public availableOperators: OperatorInfo[] = [];
   * Whether the current operator requires a value
  public requiresValue: boolean = true;
  // Dropdown state
  public fieldDropdownOpen = false;
  public operatorDropdownOpen = false;
  public valueDropdownOpen = false;
  // Keyboard navigation state
  public fieldHighlightIndex = -1;
  public operatorHighlightIndex = -1;
  public valueHighlightIndex = -1;
  // ViewChild references for dropdown buttons (Safari focus fix)
  @ViewChild('fieldDropdownBtn') fieldDropdownBtn!: ElementRef<HTMLButtonElement>;
  @ViewChild('operatorDropdownBtn') operatorDropdownBtn!: ElementRef<HTMLButtonElement>;
  @ViewChild('valueDropdownBtn') valueDropdownBtn!: ElementRef<HTMLButtonElement>;
  constructor(private elementRef: ElementRef) {}
   * Close dropdowns when clicking outside the component
    if (!this.elementRef.nativeElement.contains(event.target)) {
    this.updateFieldSelection();
    if (changes['filter'] || changes['fields']) {
   * Update the selected field and available operators
  private updateFieldSelection(): void {
    if (!this.filter || !this.fields.length) return;
    this.selectedField = this.fields.find(f => f.name === this.filter.field) || null;
    if (this.selectedField) {
      this.availableOperators = getOperatorsForType(this.selectedField.type);
      this.requiresValue = operatorRequiresValue(this.filter.operator);
      this.availableOperators = [];
      this.requiresValue = true;
  // DROPDOWN TOGGLE METHODS
  toggleFieldDropdown(): void {
    if (this.disabled) return;
    const wasOpen = this.fieldDropdownOpen;
    this.fieldDropdownOpen = !wasOpen;
    if (this.fieldDropdownOpen) {
      // Ensure button retains focus for keyboard events (Safari fix)
      setTimeout(() => this.fieldDropdownBtn?.nativeElement?.focus(), 0);
  toggleOperatorDropdown(): void {
    if (this.disabled || !this.selectedField) return;
    const wasOpen = this.operatorDropdownOpen;
    this.operatorDropdownOpen = !wasOpen;
    if (this.operatorDropdownOpen) {
      setTimeout(() => this.operatorDropdownBtn?.nativeElement?.focus(), 0);
  toggleValueDropdown(): void {
    const wasOpen = this.valueDropdownOpen;
    this.valueDropdownOpen = !wasOpen;
    if (this.valueDropdownOpen) {
      setTimeout(() => this.valueDropdownBtn?.nativeElement?.focus(), 0);
  closeFieldDropdown(): void {
    this.fieldDropdownOpen = false;
  closeOperatorDropdown(): void {
    this.operatorDropdownOpen = false;
  closeValueDropdown(): void {
    this.valueDropdownOpen = false;
  closeAllDropdowns(): void {
    this.resetHighlightIndices();
  resetHighlightIndices(): void {
    this.fieldHighlightIndex = -1;
    this.operatorHighlightIndex = -1;
    this.valueHighlightIndex = -1;
  // KEYBOARD NAVIGATION
   * Handle keyboard events on the field dropdown trigger
  onFieldKeydown(event: KeyboardEvent): void {
    if (!this.fieldDropdownOpen) {
      // Open dropdown on Enter, Space, or arrow keys
      if (event.key === 'Enter' || event.key === ' ' || event.key === 'ArrowDown' || event.key === 'ArrowUp') {
        this.toggleFieldDropdown();
        this.fieldHighlightIndex = this.fields.findIndex(f => f.name === this.filter.field);
        if (this.fieldHighlightIndex < 0) this.fieldHighlightIndex = 0;
    this.handleDropdownKeydown(
      this.fields,
      this.fieldHighlightIndex,
      (index) => this.fieldHighlightIndex = index,
      (item) => this.selectField(item.name),
      (item) => item.displayName,
      () => this.closeFieldDropdown()
   * Handle keyboard events on the operator dropdown trigger
  onOperatorKeydown(event: KeyboardEvent): void {
    if (!this.operatorDropdownOpen) {
        this.toggleOperatorDropdown();
        this.operatorHighlightIndex = this.availableOperators.findIndex(o => o.value === this.filter.operator);
        if (this.operatorHighlightIndex < 0) this.operatorHighlightIndex = 0;
      this.availableOperators,
      this.operatorHighlightIndex,
      (index) => this.operatorHighlightIndex = index,
      (item) => this.selectOperator(item.value),
      (item) => item.label,
      () => this.closeOperatorDropdown()
   * Handle keyboard events on the value dropdown trigger
  onValueKeydown(event: KeyboardEvent): void {
    if (this.disabled || !this.selectedField?.valueList) return;
    if (!this.valueDropdownOpen) {
        this.toggleValueDropdown();
        const valueList = this.selectedField.valueList;
        this.valueHighlightIndex = valueList.findIndex(v => v.value === this.filter.value);
        if (this.valueHighlightIndex < 0) this.valueHighlightIndex = 0;
      this.selectedField.valueList,
      this.valueHighlightIndex,
      (index) => this.valueHighlightIndex = index,
      (item) => this.selectValueFromOption(item.value),
      () => this.closeValueDropdown()
   * Generic handler for dropdown keyboard navigation
  private handleDropdownKeydown<T>(
    event: KeyboardEvent,
    currentIndex: number,
    setIndex: (index: number) => void,
    onSelect: (item: T) => void,
    getLabel: (item: T) => string,
    onClose: () => void
        setIndex(Math.min(currentIndex + 1, items.length - 1));
        this.scrollHighlightedItemIntoView();
        setIndex(Math.max(currentIndex - 1, 0));
        if (currentIndex >= 0 && currentIndex < items.length) {
          onSelect(items[currentIndex]);
      case 'Home':
        setIndex(0);
      case 'End':
        setIndex(items.length - 1);
        // Type-ahead: jump to first item starting with typed character
        if (event.key.length === 1 && /[a-zA-Z0-9]/.test(event.key)) {
          const char = event.key.toLowerCase();
          const startIndex = currentIndex + 1;
          // Search from current position to end, then from start
            const idx = (startIndex + i) % items.length;
            const label = getLabel(items[idx]).toLowerCase();
            if (label.startsWith(char)) {
              setIndex(idx);
   * Scroll the highlighted dropdown item into view
  private scrollHighlightedItemIntoView(): void {
    // Use setTimeout to let Angular update the DOM first
      const highlightedEl = this.elementRef.nativeElement.querySelector('.dropdown-item.highlighted');
      if (highlightedEl) {
        highlightedEl.scrollIntoView({ block: 'nearest', behavior: 'smooth' });
  // DISPLAY HELPERS
   * Get the display name for the currently selected field
  getSelectedFieldDisplayName(): string {
    if (!this.filter.field) return 'Select field...';
    const field = this.fields.find(f => f.name === this.filter.field);
    return field?.displayName || this.filter.field;
   * Get the label for the currently selected operator
  getSelectedOperatorLabel(): string {
    if (!this.filter.operator) return 'Select...';
    const op = this.availableOperators.find(o => o.value === this.filter.operator);
    return op?.label || this.filter.operator;
   * Get the label for the currently selected value (for value list dropdowns)
  getSelectedValueLabel(): string {
    if (!this.filter.value || !this.selectedField?.valueList) return 'Select...';
    const option = this.selectedField.valueList.find(o => o.value === this.filter.value);
    return option?.label || String(this.filter.value);
  // SELECTION HANDLERS
   * Handle field selection from custom dropdown
  selectField(fieldName: string): void {
    this.closeFieldDropdown();
    this.onFieldChange(fieldName);
   * Handle operator selection from custom dropdown
  selectOperator(operator: string): void {
    this.closeOperatorDropdown();
    this.onOperatorChange(operator as FilterOperator);
   * Handle value selection from custom dropdown
  selectValue(value: string): void {
    this.closeValueDropdown();
    this.onValueChange(value);
   * Handle value selection from value list option (supports union type)
  selectValueFromOption(value: string | number | boolean): void {
   * Handle field selection change
  onFieldChange(fieldName: string): void {
    const field = this.fields.find(f => f.name === fieldName);
    if (!field) return;
    this.availableOperators = getOperatorsForType(field.type);
    // Get default operator for the new field type
    const defaultOperator = this.availableOperators[0]?.value || 'eq';
    this.requiresValue = operatorRequiresValue(defaultOperator);
    // Emit updated filter with new field and reset value
      operator: defaultOperator,
      value: this.getDefaultValue(field.type)
   * Handle operator selection change
  onOperatorChange(operator: FilterOperator): void {
    this.requiresValue = operatorRequiresValue(operator);
    // If operator doesn't require value, clear it
    const value = this.requiresValue ? this.filter.value : null;
      operator,
   * Handle value change
  onValueChange(value: unknown): void {
   * Handle boolean toggle
  onBooleanChange(value: boolean): void {
   * Handle date change
  onDateChange(event: Event): void {
    const value = input.value ? new Date(input.value).toISOString() : null;
   * Handle delete button click
  private emitChange(filter: FilterDescriptor): void {
   * Get default value for a field type
  private getDefaultValue(type: FilterFieldType): unknown {
      case 'lookup':
   * Get the date value formatted for the date input
  getDateInputValue(): string {
    if (!this.filter.value) return '';
      const date = new Date(this.filter.value as string);
   * Check if the current field has a value list (dropdown options)
  hasValueList(): boolean {
    return !!(this.selectedField?.valueList && this.selectedField.valueList.length > 0);
