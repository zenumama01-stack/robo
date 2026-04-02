import { Component, Input, forwardRef, OnInit } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormControl } from '@angular/forms';
import { FormQuestion, FormOption } from '@memberjunction/ai-core-plus';
 * Component for rendering individual form questions with various input types
 * Implements ControlValueAccessor for seamless integration with Angular forms
  selector: 'mj-form-question',
  templateUrl: './form-question.component.html',
  styleUrls: ['./form-question.component.css'],
      useExisting: forwardRef(() => FormQuestionComponent),
export class FormQuestionComponent implements ControlValueAccessor, OnInit {
  @Input() question!: FormQuestion;
  @Input() control!: FormControl;
  public value: any = null;
  public disabled: boolean = false;
  private onChange: (value: any) => void = () => {};
  private onTouched: () => void = () => {};
    // Debug logging for dropdown issues
    if (this.questionType === 'dropdown') {
      console.log('[FormQuestion] Dropdown initialized:', {
        questionId: this.question.id,
        hasControl: !!this.control,
        controlValue: this.control?.value,
        options: this.options,
        value: this.value
   * Get the question type (handles both simple string and complex types)
  public get questionType(): string {
    return typeof this.question.type === 'string'
      ? this.question.type
      : this.question.type.type;
   * Check if this is a choice-based question (buttongroup, radio, dropdown, checkbox)
  public get isChoiceQuestion(): boolean {
    const type = this.questionType;
    return ['buttongroup', 'radio', 'dropdown', 'checkbox'].includes(type);
   * Get options for choice questions
  public get options(): FormOption[] {
    if (typeof this.question.type === 'object' && 'options' in this.question.type) {
      return this.question.type.options;
   * Check if multiple selections are allowed (for checkbox type)
  public get allowMultiple(): boolean {
    return this.questionType === 'checkbox' ||
      (typeof this.question.type === 'object' && 'multiple' in this.question.type && !!this.question.type.multiple);
   * Get CSS class for field width based on widthHint or intelligent defaults
  public get widthClass(): string {
    // If explicit widthHint provided, use it
    if (this.question.widthHint) {
      return `width-${this.question.widthHint}`;
    // Otherwise, apply intelligent defaults based on question type
    // Narrow fields
    if (['number', 'currency', 'date', 'datetime', 'time'].includes(type)) {
      return 'width-narrow';
    // Wide fields for choice-based controls (need space for multiple options)
    if (['buttongroup', 'radio', 'checkbox'].includes(type)) {
      return 'width-wide';
    // Auto-width for dropdowns
    if (type === 'dropdown') {
      return 'width-auto';
    // Full-width fields
    if (type === 'textarea') {
      return 'width-full';
    // Wide fields
    if (type === 'email') {
    // Default to medium for text and other types
    return 'width-medium';
   * Get placeholder text for text inputs
  public get placeholder(): string | undefined {
    if (typeof this.question.type === 'object' && 'placeholder' in this.question.type) {
      return this.question.type.placeholder as string;
   * Get min value for number/currency inputs
  public get min(): number | undefined {
    if (typeof this.question.type === 'object' && 'min' in this.question.type) {
      return this.question.type.min as number;
   * Get max value for number/currency inputs
  public get max(): number | undefined {
    if (typeof this.question.type === 'object' && 'max' in this.question.type) {
      return this.question.type.max as number;
   * Get step value for number/currency inputs
  public get step(): number | undefined {
    if (typeof this.question.type === 'object' && 'step' in this.question.type) {
      return this.question.type.step as number;
   * Get prefix for currency inputs
  public get prefix(): string | undefined {
    if (typeof this.question.type === 'object' && 'prefix' in this.question.type) {
      return this.question.type.prefix as string;
   * Get suffix for currency inputs
  public get suffix(): string | undefined {
    if (typeof this.question.type === 'object' && 'suffix' in this.question.type) {
      return this.question.type.suffix as string;
   * Handle value changes
  public onValueChange(newValue: any): void {
    this.value = newValue;
    this.onChange(newValue);
   * Handle checkbox toggle for multiple selection
  public toggleCheckbox(option: FormOption): void {
    if (!this.allowMultiple) {
      // Single selection mode
      this.onValueChange(option.value);
      // Multiple selection mode
      const currentValues = Array.isArray(this.value) ? this.value : [];
      const index = currentValues.indexOf(option.value);
      let newValues: any[];
        // Remove if already selected
        newValues = currentValues.filter((v: any) => v !== option.value);
        // Add if not selected
        newValues = [...currentValues, option.value];
      this.onValueChange(newValues);
   * Check if a checkbox option is selected
  public isChecked(option: FormOption): boolean {
      return this.value === option.value;
      return Array.isArray(this.value) && this.value.includes(option.value);
  writeValue(value: any): void {
  registerOnChange(fn: any): void {
  registerOnTouched(fn: any): void {
   * Debug handler for dropdown changes
  public onDropdownChange(value: any): void {
    console.log('[FormQuestion] Dropdown value changed:', {
      newValue: value,
      optionsCount: this.options.length
   * Get slider configuration
  public getSliderConfig(): { min: number; max: number; step?: number; suffix?: string } {
    if (typeof this.question.type === 'object' && this.question.type.type === 'slider') {
        min: this.question.type.min,
        max: this.question.type.max,
        step: this.question.type.step,
        suffix: this.question.type.suffix
    return { min: 0, max: 100, step: 1 };
   * Handle date range start date change
  public onDateRangeStartChange(value: Date): void {
    const current = this.value || {};
    this.onValueChange({ ...current, start: value });
   * Handle date range end date change
  public onDateRangeEndChange(value: Date): void {
    this.onValueChange({ ...current, end: value });
  public trackByValue(index: number, option: FormOption): any {
