import { Component, Input, Output, EventEmitter } from '@angular/core';
  selector: 'mj-ai-agent-run-step-node',
  templateUrl: './ai-agent-run-step-node.component.html',
  styleUrls: ['./ai-agent-run-step-node.component.css']
export class AIAgentRunStepNodeComponent {
  @Input() item!: TimelineItem;
  @Input() isSelected = false;
  @Output() itemClick = new EventEmitter<TimelineItem>();
  @Output() expandToggle = new EventEmitter<Event>();
  @Output() navigateToEntity = new EventEmitter<{ entityName: string; recordId: string }>();
  get hasChildren(): boolean {
    return !!this.item.children && this.item.children.length > 0;
  get isSubAgent(): boolean {
    return this.item.type === 'subrun' || (this.item.type === 'step' && this.item.data?.StepType === 'Sub-Agent');
  get isParentStep(): boolean {
    // Check if this step has children via ParentID relationships (non-Sub-Agent hierarchy)
    return this.item.type === 'step' &&
           this.item.data?.StepType !== 'Sub-Agent' &&
           this.hasChildren;
  get canExpand(): boolean {
    // Can expand if it's a sub-agent OR if it's a parent step with children
    return this.isSubAgent || this.isParentStep;
  get canNavigateToEntity(): boolean {
    // For steps, check if it's a type that has a target and if TargetLogID exists
    if (this.item.type === 'step' && this.item.data) {
      const stepType = this.item.data.StepType;
      return (stepType === 'Actions' || stepType === 'Prompt' || stepType === 'Sub-Agent') 
        && !!this.item.data.TargetLogID;
  get entityNavigationText(): string {
    // For step types, check the StepType
      switch (stepType.trim().toLowerCase()) {
        case 'actions':
          return 'View Action Log';
        case 'prompt':
          return 'View Prompt Run';
        case 'sub-agent':
          return 'View Agent Run';
          return 'View Details';
  getStatusIcon(status: string): string {
      'Running': 'fa-circle-notch fa-spin',
      'Completed': 'fa-check-circle',
      'Failed': 'fa-times-circle',
      'Cancelled': 'fa-ban',
      'Paused': 'fa-pause-circle'
    return iconMap[status] || 'fa-question-circle';
  handleClick() {
    this.itemClick.emit(this.item);
  handleExpandToggle(event: Event) {
    if (this.canExpand) {
      this.expandToggle.emit(event);
  navigateToRecord(event: Event) {
    if (!this.canNavigateToEntity) return;
    let recordId = '';
    // For step types, use TargetLogID and determine entity based on StepType
      recordId = this.item.data.TargetLogID;
          entityName = 'MJ: Action Execution Logs';
          entityName = 'MJ: AI Prompt Runs';
          entityName = 'MJ: AI Agent Runs';
      // For direct types, use the item ID
      recordId = this.item.id;
      switch (this.item.type.trim().toLowerCase()) {
    if (entityName && recordId) {
      this.navigateToEntity.emit({ entityName, recordId });
  getAdditionalInfo(): string {
      const step = this.item.data;
      const parts = [];
      if (step.TargetActionName) {
        parts.push(`Action: ${step.TargetActionName}`);
      if (step.Error) {
        parts.push('Error occurred');
      return parts.join(' • ');
    if (this.item.type === 'action' && this.item.data) {
      const log = this.item.data;
      if (log.Message) {
        return log.Message.substring(0, 100) + (log.Message.length > 100 ? '...' : '');
  onLogoError(event: Event): void {
    // Hide the broken image and show the icon instead by clearing logoUrl
    const imgElement = event.target as HTMLImageElement;
    imgElement.style.display = 'none';
    this.item.logoUrl = undefined;
