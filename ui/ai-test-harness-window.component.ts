import { Component, Input, Output, EventEmitter, ViewChild, OnInit } from '@angular/core';
export interface AITestHarnessWindowData {
    selector: 'mj-ai-test-harness-window',
        <div class="window-content">
                    <mj-loading [text]="'Loading ' + (mode === 'agent' ? 'AI Agent' : 'AI Prompt') + '...'" size="large"></mj-loading>
            @else if (error) {
                    [entity]="(agent || prompt) || null"
                    [isVisible]="true"
                    [originalPromptRunId]="data.promptRunId || null"
                    (minimizeRequested)="onMinimizeRequested()">
        .window-content {
        mj-ai-test-harness {
export class AITestHarnessWindowComponent implements OnInit {
    @Input() data: AITestHarnessWindowData = {};
    @Output() closeWindow = new EventEmitter<void>();
    windowTitle = 'AI Test Harness';
    width: number = 1200;
    height: number = 800;
    error = '';
        console.log('🪟 AITestHarnessWindowComponent.ngOnInit - data:', this.data);
        console.log('📌 promptRunId:', this.data.promptRunId);
        // Set window dimensions
        this.width = this.convertToNumber(this.data.width) || 1200;
        this.height = this.convertToNumber(this.data.height) || 800;
        // Determine mode
        this.mode = this.data.mode || (this.data.promptId || this.data.prompt ? 'prompt' : 'agent');
        // Load entity
        this.loadEntity();
    async loadEntity() {
                if (this.data.agent) {
                    this.windowTitle = this.data.title || `Test Agent: ${this.agent.Name}`;
                } else if (this.data.agentId) {
                    const agentEntity = await this.metadata.GetEntityObject<AIAgentEntityExtended>('MJ: AI Agents');
                    await agentEntity.Load(this.data.agentId);
                    if (agentEntity.IsSaved) {
                        this.agent = agentEntity;
                        throw new Error('Agent not found');
                    throw new Error('No agent provided');
                if (this.data.prompt) {
                    this.windowTitle = this.data.title || `Test Prompt: ${this.prompt.Name}`;
                } else if (this.data.promptId) {
                    const promptEntity = await this.metadata.GetEntityObject<AIPromptEntityExtended>('MJ: AI Prompts');
                    await promptEntity.Load(this.data.promptId);
                    if (promptEntity.IsSaved) {
                        this.prompt = promptEntity;
                        throw new Error('Prompt not found');
                    throw new Error('No prompt provided');
            this.error = err.message || 'Failed to load entity';
    onClose() {
        this.closeWindow.emit();
    onMinimizeRequested() {
        // Since Kendo Window doesn't support minimize functionality,
        // we'll close the window when navigating to view the agent run
    private convertToNumber(value: string | number | undefined): number | undefined {
        if (!value) return undefined;
        if (typeof value === 'number') return value;
        // Handle percentage values
        if (value.endsWith('vw') || value.endsWith('vh')) {
            const percentage = parseFloat(value) / 100;
            if (value.endsWith('vw')) {
                return window.innerWidth * percentage;
                return window.innerHeight * percentage;
        // Handle pixel values
        if (value.endsWith('px')) {
            return parseFloat(value);
        // Try to parse as number
        const parsed = parseFloat(value);
        return isNaN(parsed) ? undefined : parsed;
