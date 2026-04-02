import { Component, Input, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { TimelineItem } from './ai-agent-run-timeline.component';
import { ParseJSONRecursive, ParseJSONOptions } from '@memberjunction/global';
  selector: 'mj-ai-agent-run-step-detail',
  templateUrl: './ai-agent-run-step-detail.component.html',
  styleUrls: ['./ai-agent-run-step-detail.component.css']
export class AIAgentRunStepDetailComponent {
  @Input() selectedTimelineItem: TimelineItem | null = null;
  @Output() closePanel = new EventEmitter<void>();
  @Output() navigateToActionLog = new EventEmitter<string>();
  @Output() copyToClipboard = new EventEmitter<string>();
  selectedItemJsonString = '{}';
  detailPaneTab: 'json' | 'diff' = 'diff';
    if (this.selectedTimelineItem) {
      this.selectedItemJsonString = this.getSelectedItemJson();
      // Default to diff tab if step has payload diff, otherwise json tab
      this.detailPaneTab = this.showStepPayloadDiff ? 'diff' : 'json';
  getSelectedItemJson(): string {
    if (!this.selectedTimelineItem) return '{}';
    // Get all the data from the entity
    let data;
    if (this.selectedTimelineItem.data instanceof MJAIAgentRunStepEntity) {
      // If it's a step entity, we need to get the full run data
      data = this.selectedTimelineItem.data.GetAll();
      data = this.selectedTimelineItem.data;
    // Apply recursive JSON parsing to the entire data object
    const parsedData = ParseJSONRecursive(data, parseOptions);
    return JSON.stringify(parsedData, null, 2);
   * Check if selected timeline item is a step with payload changes
  get showStepPayloadDiff(): boolean {
    if (!this.selectedTimelineItem || this.selectedTimelineItem.type !== 'step') {
    const stepData = this.selectedTimelineItem.data;
    if (stepData && (stepData.PayloadAtStart?.trim().length > 0 
                 || stepData.PayloadAtEnd?.trim().length > 0)) {
      return stepData.PayloadAtStart !== stepData.PayloadAtEnd;
   * Get parsed PayloadAtStart for the selected step
  get stepPayloadAtStartObject(): any {
    if (!stepData?.PayloadAtStart) return null;
      // First, check if PayloadAtStart is a JSON string that needs to be parsed
      let payloadData = stepData.PayloadAtStart;
        // If PayloadAtStart is a JSON string, parse it first
        payloadData = JSON.parse(stepData.PayloadAtStart);
        // If it's not valid JSON, use it as-is
        payloadData = stepData.PayloadAtStart;
      return ParseJSONRecursive(payloadData, parseOptions);
   * Get parsed PayloadAtEnd for the selected step
  get stepPayloadAtEndObject(): any {
    if (!stepData?.PayloadAtEnd) return null;
      // First, check if PayloadAtEnd is a JSON string that needs to be parsed
      let payloadData = stepData.PayloadAtEnd;
        // If PayloadAtEnd is a JSON string, parse it first
        payloadData = JSON.parse(stepData.PayloadAtEnd);
        payloadData = stepData.PayloadAtEnd;
  onClosePanel() {
    this.closePanel.emit();
  onNavigateToActionLog(logId: string) {
    this.navigateToActionLog.emit(logId);
  onCopyToClipboard() {
    this.copyToClipboard.emit(this.getSelectedItemJson());
