import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
    selector: 'mj-execution-node',
        <div class="tree-node" 
             [class.expanded]="expanded"
             [class.has-children]="hasChildren()"
             [class.details-expanded]="detailsExpanded"
             [class]="'depth-' + depth + ' type-' + getStepTypeClass()">
            <!-- Node Header -->
            <div class="node-header" 
                 (dblclick)="onDoubleClick()">
                <!-- Expand/Collapse Icon - Only show if node has children -->
                @if (hasChildren()) {
                    <i class="expand-icon fa-solid"
                       [class.fa-chevron-down]="expanded"
                       [class.fa-chevron-right]="!expanded"
                       (click)="onToggleChildren($event)"
                       title="Toggle children"></i>
                <!-- Status Icon -->
                <span class="status-icon" [class]="'status-' + getStatusClass()">
                    @switch (getStatusClass()) {
                        @case ('pending') {
                        @case ('running') {
                        @case ('completed') {
                        @case ('failed') {
                <span class="type-icon" [title]="getNodeTitle()">
                    @switch (getStepTypeClass()) {
                        @case ('validation') {
                        @case ('prompt') {
                        @case ('action') {
                            @if (getActionIconClass()) {
                                <i [class]="getActionIconClass()"></i>
                        @case ('sub-agent') {
                            @if (getAgentLogoURL()) {
                                <img [src]="getAgentLogoURL()" [alt]="getAgentName() || 'Agent'" class="agent-logo-icon">
                            } @else if (getAgentIconClass()) {
                                <i [class]="getAgentIconClass()"></i>
                        @case ('decision') {
                        @case ('chat') {
                <!-- Node Name with depth indicator for sub-agents -->
                <span class="node-name">
                    @if (step.StepType === 'Sub-Agent' && depth > 0) {
                        <small style="color: #666; margin-right: 8px;">[Level {{ depth }}]</small>
                    {{ getTruncatedName() }}
                <!-- Duration -->
                @if (getDuration()) {
                    <span class="node-duration">{{ formatDuration(getDuration()) }}</span>
                <!-- Tokens/Cost -->
                @if (getTokensUsed() || getCost()) {
                    <span class="node-metrics">
                        @if (getTokensUsed()) {
                            <span class="tokens">{{ getTokensUsed() }} tokens</span>
                        @if (getCost()) {
                            <span class="cost">\${{ getCost()!.toFixed(4) }}</span>
                <!-- Details Toggle Button - Only show if node has details -->
                @if (hasNodeDetails()) {
                    <button class="details-toggle-btn"
                            (click)="onToggleDetails($event)"
                            [title]="detailsExpanded ? 'Hide details' : 'Show details'">
                           [class.fa-info]="!detailsExpanded"
                           [class.fa-times]="detailsExpanded"></i>
            <!-- Node Details (when details are expanded) -->
            @if (detailsExpanded) {
                <!-- Show markdown details first if available -->
                @if (getDetailsMarkdown() || isNameTruncated()) {
                    <div class="markdown-details">
                        @if (isNameTruncated()) {
                            <div class="full-name">{{ step.StepName }}</div>
                        @if (getDetailsMarkdown()) {
                            <div class="detail-content markdown" [innerHTML]="formatMarkdown(getDetailsMarkdown()!)"></div>
                <!-- Always show details section if node is expanded, even if some content is empty -->
                <div class="node-details">
                    @if (step.ErrorMessage) {
                        <div class="detail-section error">
                            <div class="detail-label">
                                <i class="fa-solid fa-exclamation-triangle"></i> Error
                            <div class="detail-content">{{ step.ErrorMessage }}</div>
                    @if (getInputPreview()) {
                                <i class="fa-solid fa-sign-in-alt"></i> Input
                            <div class="detail-content">{{ getInputPreview() }}</div>
                    @if (getOutputPreview()) {
                                <i class="fa-solid fa-sign-out-alt"></i> Output
                            <div class="detail-content">{{ getOutputPreview() }}</div>
                    @if (!step.ErrorMessage && !getInputPreview() && !getOutputPreview() && !getDetailsMarkdown() && !isNameTruncated()) {
                            <div class="detail-content">No additional details available for this step.</div>
            <!-- Note: Sub-agent children are rendered by the parent component to maintain proper depth tracking -->
        /* Depth-based indentation - each level indents by 24px */
        /* Root level nodes (depth 0) */
        .depth-0 { 
        .depth-0::after {
        .depth-0 .node-header { 
        /* Sub-level nodes with increasing indentation */
        .depth-1 {
        .depth-1 .node-header {
        .depth-2 {
            margin-left: 48px;
        .depth-2 .node-header {
            background: #fcfcfc;
            border-color: #c0c0c0;
        .depth-3 {
            margin-left: 72px;
        .depth-3 .node-header {
            background: #fdfdfd;
            border-color: #b0b0b0;
        .depth-4 {
            margin-left: 96px;
        .depth-4 .node-header {
            background: #fefefe;
            border-color: #a0a0a0;
        .depth-5 {
            margin-left: 120px;
        .depth-5 .node-header {
        .depth-6 {
            margin-left: 144px;
        .depth-6 .node-header {
        /* Root level - higher z-index to hide lines behind it */
        /* Only add left padding for nodes with children (that show chevrons) */
        .depth-0.has-children::before {
            border-left: 2px dotted #c0c7d0;
            z-index: 0; /* Behind everything */
        /* Child level - only for nodes that actually have parent chevrons */
            margin-left: 30px;
        /* Horizontal line connecting to each child node - only when parent has children */
        .depth-1::after {
            left: -15px;
            width: 25px;
            border-bottom: 2px dotted #c0c7d0;
        /* Visual indicator when details are expanded */
        .tree-node.details-expanded > .node-header {
            border-bottom-left-radius: 0;
            border-bottom-right-radius: 0;
            border-bottom: 1px solid #2196f3;
        .node-header {
        .node-header:hover {
            background: var(--gray-700);
            border-color: var(--mj-blue) !important;
        /* Sub-agent specific styling */
        .tree-node.type-sub-agent > .node-header {
            border-left: 4px solid var(--mj-blue);
        /* Action specific styling */
        .tree-node.type-action > .node-header {
            border-left: 4px solid #4caf50;
            z-index: 10; /* Ensure expand icon is clickable */
        .status-pending { color: #999; }
        .status-running { color: #2196f3; }
        .status-completed { color: #4caf50; }
        .status-failed { color: #f44336; }
        .agent-logo-icon {
        .node-duration {
        .node-metrics {
        .tokens {
        .cost {
        /* Details Toggle Button */
        .details-toggle-btn {
        .details-toggle-btn:hover {
        .details-toggle-btn:active {
        /* When details are expanded, style the button differently */
        .tree-node.details-expanded .details-toggle-btn {
        .tree-node.details-expanded .details-toggle-btn:hover {
        .node-details {
            background: var(--gray-600);
            border: 1px solid var(--gray-700);
            z-index: 4;
        .detail-section.error {
        .markdown-details {
            padding: 16px 16px 0 16px;
        .markdown h3, .markdown h4 {
            margin: 8px 0 4px 0;
        .markdown h3 {
        .markdown h4 {
        .markdown ul {
        .markdown li {
        .markdown code {
        .markdown pre {
        .markdown pre code {
        .markdown strong {
        .markdown em {
        .full-name {
export class ExecutionNodeComponent {
    @Input() step!: AIAgentRunStepEntityExtended;
    @Input() depth: number = 0;
    @Input() agentPath: string[] = [];
    @Input() expanded: boolean = false;
    @Input() detailsExpanded: boolean = false;
    @Input() overrideDisplayStatus?: string; // Allow parent to override the displayed status
    @Output() toggleNode = new EventEmitter<void>();
    @Output() toggleDetails = new EventEmitter<void>();
    @Output() userInteracted = new EventEmitter<void>();
    hasChildren(): boolean {
        return this.step.StepType === 'Sub-Agent' && 
               !!this.step.SubAgentRun?.Steps && 
               this.step.SubAgentRun.Steps.length > 0;
    onToggleChildren(event?: Event): void {
        if (this.hasChildren()) {
            this.toggleNode.emit();
            this.userInteracted.emit();
    onToggleDetails(event?: Event): void {
        if (this.hasNodeDetails()) {
            this.toggleDetails.emit();
    onDoubleClick(): void {
    hasNodeDetails(): boolean {
        return !!this.step.InputData || 
               !!this.step.OutputData || 
               !!this.step.ErrorMessage || 
               !!this.getDetailsMarkdown() ||
               this.isNameTruncated();
    getTruncatedName(): string {
        const maxLength = 120;
        const name = this.getStepName();
        if (name.length <= maxLength) {
        return name.substring(0, maxLength) + '...';
    isNameTruncated(): boolean {
        return this.step.StepName.length > 120;
    getNodeTitle(): string {
        if (this.step.StepType === 'Sub-Agent' && this.getAgentName()) {
            return `Sub-agent: ${this.getAgentName()}`;
        if (this.step.StepType === 'Actions' && this.getActionName()) {
            return `Action: ${this.getActionName()}`;
        return this.step.StepType;
    // Getter methods for step data
    getStepName(): string {
        // Extract just the first line if the name contains markdown
        const lines = this.step.StepName.split('\n');
        return lines[0].trim();
    getStepTypeClass(): string {
        return typeMap[this.step.StepType] || 'prompt';
        // Use override if provided, otherwise use actual status
        const status = this.overrideDisplayStatus || this.step.Status;
    getDuration(): number {
        if (!this.step.StartedAt || !this.step.CompletedAt) return 0;
        return new Date(this.step.CompletedAt).getTime() - new Date(this.step.StartedAt).getTime();
    getTokensUsed(): number | undefined {
        // Check if this is a prompt step with token data
        if (this.step.StepType === 'Prompt' && this.step.PromptRun) {
            return this.step.PromptRun.TokensUsed || undefined;
    getCost(): number | undefined {
        // Check if this is a prompt step with cost data
            return this.step.PromptRun.TotalCost || undefined;
    getDetailsMarkdown(): string | undefined {
        // Check if the step name contains markdown details after the first line
        if (lines.length > 1) {
            return lines.slice(1).join('\n').trim();
    getInputPreview(): string | undefined {
        if (!this.step.InputData) return undefined;
            const parsed = JSON.parse(this.step.InputData);
            // Extract meaningful preview
            if (parsed.actionName) return `Action: ${parsed.actionName}`;
            if (parsed.subAgentName) return `Sub-agent: ${parsed.subAgentName}`;
            if (parsed.message) return parsed.message;
            if (parsed.userMessage) return parsed.userMessage;
            return typeof this.step.InputData === 'string' ? this.step.InputData : JSON.stringify(this.step.InputData);
    getOutputPreview(): string | undefined {
        if (!this.step.OutputData) return undefined;
            const parsed = JSON.parse(this.step.OutputData);
                    preview += `Message: ${result.message}\n`;
                    preview += `Content: ${result.content}`;
            return typeof this.step.OutputData === 'string' ? this.step.OutputData : JSON.stringify(this.step.OutputData);
    // Methods to extract metadata from input/output data
    getAgentName(): string | undefined {
        if (this.step.StepType === 'Sub-Agent' && this.step.SubAgentRun) {
            return this.step.SubAgentRun.Agent || undefined;
        return this.parseMetadata('subAgentName');
    getAgentIconClass(): string | undefined {
        return this.parseMetadata('subAgentIconClass') || this.parseMetadata('agentIconClass');
    getAgentLogoURL(): string | undefined {
        return this.parseMetadata('subAgentLogoURL') || this.parseMetadata('agentLogoURL');
    getActionName(): string | undefined {
        if (this.step.StepType === 'Actions' && this.step.ActionExecutionLog) {
            return this.step.ActionExecutionLog.Action;
        return this.parseMetadata('actionName');
    getActionIconClass(): string | undefined {
        return this.parseMetadata('actionIconClass');
    private parseMetadata(key: string): string | undefined {
            return parsed[key];
            return `<a href="${href}" target="_blank" rel="noopener noreferrer" style="color: #2196f3; text-decoration: underline;">${url}</a>`;
