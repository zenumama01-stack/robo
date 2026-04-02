import { Component, Input, Output, EventEmitter, OnInit, ViewChild, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { AIAgentEntityExtended, AIPromptEntityExtended, AIPromptRunEntityExtended } from '@memberjunction/ai-core-plus';
import { AITestHarnessComponent } from './ai-test-harness.component';
 * Configuration data interface for the AI Test Harness Dialog.
 * Provides all necessary options for initializing the dialog with appropriate
 * agent/prompt data, dimensions, and initial variable configurations.
export interface AITestHarnessDialogData {
    /** ID of the AI agent to load (alternative to providing agent entity) */
    /** Pre-loaded AI agent entity (alternative to providing agentId) */
    /** ID of the AI prompt to load (alternative to providing prompt entity) */
    /** Pre-loaded AI prompt entity (alternative to providing promptId) */
    /** Custom dialog title (defaults to agent/prompt name) */
    /** Dialog width in CSS units or viewport percentage */
    width?: string | number;
    /** Dialog height in CSS units or viewport percentage */
    height?: string | number;
    /** Initial data context variables for agent execution */
    initialDataContext?: Record<string, any>;
    /** Initial template data variables for prompt rendering */
    initialTemplateData?: Record<string, any>;
    /** Initial template variables for prompt execution */
    initialTemplateVariables?: Record<string, any>;
    /** Pre-selected AI model ID for prompt execution */
    selectedModelId?: string;
    /** Pre-selected AI vendor ID for prompt execution */
    selectedVendorId?: string;
    /** Pre-selected AI configuration ID for prompt execution */
    selectedConfigurationId?: string;
    /** Mode of operation - 'agent' or 'prompt' */
    mode?: 'agent' | 'prompt';
    /** ID of an existing prompt run to preload data from */
    promptRunId?: string;
 * Dialog wrapper component for the AI Agent Test Harness.
 * Provides a modal dialog interface with proper sizing, header, and close functionality.
 * Automatically loads agent data and initializes the test harness with provided configuration.
 * ## Features:
 * - **Automatic Agent Loading**: Loads agent by ID or uses provided entity
 * - **Configurable Dimensions**: Supports custom dialog sizing
 * - **Initial Data Setup**: Pre-populates data context and template variables
 * - **Clean Dialog Interface**: Professional header with close button
 * - **Responsive Layout**: Adapts to content and screen size
 * This component is typically opened through the `TestHarnessDialogService` rather than directly:
 * const dialogRef = this.testHarnessService.openAgentTestHarness({
 *   agentId: 'agent-123',
 *   initialDataContext: { userId: 'user-456' }
    selector: 'mj-ai-test-harness-dialog',
        <div class="test-harness-dialog">
                <h2>{{ title }}</h2>
                <button class="close-button" (click)="close()">
                    #testHarness
                    [mode]="mode"
                    [entity]="mode === 'agent' ? agent : prompt"
                    [isVisible]="true">
        .test-harness-dialog {
        .dialog-header h2 {
        .close-button {
        .close-button:hover {
            background-color: rgba(0, 0, 0, 0.04);
        :host ::ng-deep .test-harness-container {
export class AITestHarnessDialogComponent implements OnInit, AfterViewInit {
    /** Reference to the embedded test harness component */
    @ViewChild('testHarness', { static: false }) testHarness!: AITestHarnessComponent;
    /** The loaded AI agent entity for testing */
    /** The loaded AI prompt entity for testing */
    prompt: AIPromptEntityExtended | null = null;
    /** The mode of operation - either 'agent' or 'prompt' */
    mode: 'agent' | 'prompt' = 'agent';
    /** Display title for the dialog header */
    title: string = 'AI Test Harness';
    /** Configuration data passed from the dialog service */
    @Input() data: AITestHarnessDialogData = {};
    /** Event emitted when the dialog should be closed */
    @Output() closeDialog = new EventEmitter<void>();
     * Initializes the dialog component by loading agent/prompt data and configuring
     * the embedded test harness with initial variables and settings.
        // Set mode from data
        if (this.data.mode) {
            this.mode = this.data.mode;
        if (this.data.title) {
            this.title = this.data.title;
        // Load entity based on mode
        if (this.mode === 'agent' || (!this.data.promptId && !this.data.prompt)) {
            // Agent mode
            if (this.data.agentId && !this.data.agent) {
                this.agent = await md.GetEntityObject<AIAgentEntityExtended>('MJ: AI Agents');
                await this.agent.Load(this.data.agentId);
                if (this.agent) {
                    this.title = this.title || `Test Harness: ${this.agent.Name}`;
            } else if (this.data.agent) {
                this.agent = this.data.agent;
            // Prompt mode
            this.mode = 'prompt';
            if (this.data.promptId && !this.data.prompt) {
                await this.prompt.Load(this.data.promptId);
                    this.title = this.title || `Test Harness: ${this.prompt.Name}`;
            } else if (this.data.prompt) {
                this.prompt = this.data.prompt;
     * AfterViewInit lifecycle hook to set initial data after view is initialized
        console.log('🚀 ngAfterViewInit - testHarness available:', !!this.testHarness);
        console.log('📊 Dialog data:', this.data);
        console.log('🎯 Mode:', this.mode);
        if (this.testHarness) {
            // Check if we need to load from a prompt run
            if (this.data.promptRunId && this.mode === 'prompt') {
                console.log('🔄 Loading from prompt run in AfterViewInit:', this.data.promptRunId);
                await this.loadFromPromptRun(this.data.promptRunId);
                console.log('📌 Not loading from prompt run - promptRunId:', this.data.promptRunId, 'mode:', this.mode);
                if (this.mode === 'agent') {
                    // Agent mode: set agent variables
                    if (this.data.initialDataContext) {
                        const variables = Object.entries(this.data.initialDataContext).map(([name, value]) => ({
                            value: typeof value === 'object' ? JSON.stringify(value) : String(value),
                            type: this.detectVariableType(value)
                        this.testHarness.agentVariables = variables;
                    if (this.data.initialTemplateData) {
                        const templateVariables = Object.entries(this.data.initialTemplateData).map(([name, value]) => ({
                        this.testHarness.agentVariables = [...this.testHarness.agentVariables, ...templateVariables];
                    // Prompt mode: set template variables
                    if (this.data.initialTemplateVariables) {
                        const variables = Object.entries(this.data.initialTemplateVariables).map(([name, value]) => ({
                        this.testHarness.templateVariables = variables;
                    // Set selected model if provided
                    if (this.data.selectedModelId) {
                        this.testHarness.selectedModelId = this.data.selectedModelId;
                    if (this.data.selectedVendorId) {
                        this.testHarness.selectedVendorId = this.data.selectedVendorId;
                    if (this.data.selectedConfigurationId) {
                        this.testHarness.selectedConfigurationId = this.data.selectedConfigurationId;
            // Trigger change detection to ensure view updates
            console.log('🔄 Triggering change detection');
            // Check after change detection
                console.log('⏱️ After timeout - conversationMessages:', this.testHarness?.conversationMessages);
                console.log('⏱️ Test harness component state:', {
                    mode: this.testHarness?.mode,
                    entity: this.testHarness?.entity?.Name,
                    messagesLength: this.testHarness?.conversationMessages?.length
     * Determines the appropriate variable type for initial data configuration.
     * @param value - The value to analyze for type detection
     * @returns The detected variable type
    private detectVariableType(value: any): 'string' | 'number' | 'boolean' | 'object' {
        if (typeof value === 'boolean') return 'boolean';
        if (typeof value === 'number') return 'number';
        if (typeof value === 'object') return 'object';
     * Loads data from an existing prompt run to pre-populate the test harness
     * @param promptRunId - The ID of the prompt run to load
    private async loadFromPromptRun(promptRunId: string): Promise<void> {
        console.log('🔄 Loading from prompt run:', promptRunId);
        const promptRun = await md.GetEntityObject<AIPromptRunEntityExtended>('MJ: AI Prompt Runs');
        if (await promptRun.Load(promptRunId)) {
            console.log('✅ Prompt run loaded successfully');
            // Load the prompt if not already loaded
            if (!this.prompt && promptRun.PromptID) {
                await this.prompt.Load(promptRun.PromptID);
                this.testHarness.entity = this.prompt;
                // Update title to indicate we're re-running
                this.title = `Re-Run: ${this.prompt.Name}`;
            // Set the model/vendor/configuration
            if (promptRun.ModelID) {
                this.testHarness.selectedModelId = promptRun.ModelID;
            if (promptRun.VendorID) {
                this.testHarness.selectedVendorId = promptRun.VendorID;
            if (promptRun.ConfigurationID) {
                this.testHarness.selectedConfigurationId = promptRun.ConfigurationID;
            // Note: We do NOT extract template variables because we want to use
            // the already-rendered system prompt from the previous run, not re-render it
            // Set advanced parameters
            if (promptRun.Temperature != null) {
                this.testHarness.advancedParams.temperature = promptRun.Temperature;
            if (promptRun.TopP != null) {
                this.testHarness.advancedParams.topP = promptRun.TopP;
            if (promptRun.TopK != null) {
                this.testHarness.advancedParams.topK = promptRun.TopK;
            if (promptRun.MinP != null) {
                this.testHarness.advancedParams.minP = promptRun.MinP;
            if (promptRun.FrequencyPenalty != null) {
                this.testHarness.advancedParams.frequencyPenalty = promptRun.FrequencyPenalty;
            if (promptRun.PresencePenalty != null) {
                this.testHarness.advancedParams.presencePenalty = promptRun.PresencePenalty;
            if (promptRun.Seed != null) {
                this.testHarness.advancedParams.seed = promptRun.Seed;
            // Note: responseFormat is handled separately, not in advancedParams
            // Use the extended entity methods to get conversation messages
            console.log('📝 Raw Messages field:', promptRun.Messages);
            const parsedData = promptRun.ParseMessagesData();
            console.log('🔍 Parsed messages data:', parsedData);
            const chatMessages = promptRun.GetChatMessages();
            console.log('💬 Extracted chat messages:', chatMessages);
            if (chatMessages.length > 0) {
                // Convert messages to the format expected by the test harness
                const convertedMessages = chatMessages.map((msg, index) => ({
                    id: `msg-${Date.now()}-${index}`,
                    content: typeof msg.content === 'string' ? msg.content : 
                             Array.isArray(msg.content) ? 
                             msg.content.filter(block => block.type === 'text').map(block => block.content).join('\n') : 
                             '',
                console.log('🎯 Converted messages for test harness:', convertedMessages);
                this.testHarness.conversationMessages = convertedMessages;
                console.log('✅ Test harness conversationMessages set:', this.testHarness.conversationMessages);
                console.log('⚠️ No chat messages found in prompt run');
            // Store the original prompt run ID for reference
            this.testHarness.originalPromptRunId = promptRunId;
            // Extract and store the system prompt for re-run
            const systemPrompt = promptRun.GetSystemPrompt();
                this.testHarness.systemPromptOverride = systemPrompt;
            // Add a note indicating this is a re-run
            if (this.testHarness.conversationMessages.length > 0) {
                // Add a system message indicating this is a re-run
                this.testHarness.conversationMessages.unshift({
                    id: `system-${Date.now()}`,
                    content: `[Re-running from Prompt Run #${promptRunId.substring(0, 8)}]`,
     * Closes the dialog by emitting the close event.
     * This method is called by the close button in the header.
        this.closeDialog.emit();
