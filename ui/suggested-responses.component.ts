import { Component, Input, Output, EventEmitter, ViewChild, ElementRef, ChangeDetectorRef, OnInit, AfterViewInit } from '@angular/core';
 * Component for displaying suggested response options below AI messages
 * Provides quick reply buttons and optional text input for streamlined conversation flow
  selector: 'mj-suggested-responses',
  templateUrl: './suggested-responses.component.html',
  styleUrls: ['./suggested-responses.component.css']
export class SuggestedResponsesComponent implements OnInit, AfterViewInit {
  @Input() suggestedResponses: SuggestedResponse[] = [];
  @Output() responseSelected = new EventEmitter<{text: string; customInput?: string}>();
  @ViewChild('inputField') inputField?: ElementRef<HTMLInputElement>;
  public customInputValue: string = '';
    console.log('🏗️ SuggestedResponsesComponent constructed');
    console.log('🎬 SuggestedResponsesComponent.ngOnInit', {
      suggestedResponses: this.suggestedResponses,
      regularResponses: this.regularResponses,
      regularResponsesLength: this.regularResponses.length
    console.log('👁️ SuggestedResponsesComponent.ngAfterViewInit - component rendered');
   * Get all regular button responses (not input fields)
  public get regularResponses(): SuggestedResponse[] {
    return this.suggestedResponses.filter(r => !r.allowInput);
   * Get the input response option (if any)
   * Only supports one input option per message
  public get inputResponse(): SuggestedResponse | null {
    return this.suggestedResponses.find(r => r.allowInput) || null;
    return this.isLastMessage && this.isConversationOwner && this.suggestedResponses.length > 0;
   * Handle regular button click
  public onResponseClick(response: SuggestedResponse): void {
    console.log('🖱️ BUTTON CLICKED!', {
      willEmit: !this.disabled
      console.log('📤 Emitting responseSelected event');
      this.responseSelected.emit({ text: response.text });
      console.log('✅ Event emitted');
      console.warn('⚠️ Button is disabled, not emitting');
   * Handle input submission (Enter key or submit button click)
  public onInputSubmit(): void {
    const trimmedValue = this.customInputValue?.trim();
    if (!this.disabled && trimmedValue && this.inputResponse) {
      this.responseSelected.emit({
        text: this.inputResponse.text,
        customInput: trimmedValue
      this.customInputValue = '';
      // Clear input field and trigger change detection
        if (this.inputField) {
          this.inputField.nativeElement.value = '';
   * Handle Enter key in input field
  public onInputKeydown(event: KeyboardEvent): void {
      this.onInputSubmit();
   * Track by function for ngFor to help Angular track items
  public trackByIndex(index: number): number {
