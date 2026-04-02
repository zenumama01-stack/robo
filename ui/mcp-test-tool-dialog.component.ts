 * @fileoverview MCP Test Tool Slide-Out Panel Component
 * Provides a beautiful UX for testing MCP tools with:
 * - Server/Connection/Tool selection
 * - Dynamic parameter input UI based on JSON Schema
 * - Tool execution with results display
 * - User settings caching via UserInfoEngine
 * - Resizable slide-out panel with width persistence
import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ChangeDetectorRef, HostListener, ElementRef } from '@angular/core';
import { GraphQLDataProvider, gql } from '@memberjunction/graphql-dataprovider';
 * Interface for JSON Schema property definition
interface JsonSchemaProperty {
    items?: JsonSchemaProperty;
    properties?: Record<string, JsonSchemaProperty>;
    format?: string;
    minimum?: number;
    maximum?: number;
    minLength?: number;
 * Interface for JSON Schema
interface JsonSchema {
 * Parameter input configuration derived from schema
interface ParameterConfig {
    defaultValue: unknown;
    enumValues: unknown[];
    format: string | null;
 * Tool execution result
interface ToolExecutionResult {
    Result?: unknown;
    DurationMs?: number;
 * Server data for selection
export interface TestToolServerData {
 * Connection data for selection
export interface TestToolConnectionData {
 * Tool data for selection
export interface TestToolData {
 * GraphQL mutation for executing MCP tool
const ExecuteMCPToolMutation = gql`
    mutation ExecuteMCPTool($input: ExecuteMCPToolInput!) {
        ExecuteMCPTool(input: $input) {
            DurationMs
    selector: 'mj-mcp-test-tool-dialog',
    templateUrl: './mcp-test-tool-dialog.component.html',
    styleUrls: ['./mcp-test-tool-dialog.component.css'],
export class MCPTestToolDialogComponent implements OnInit, OnDestroy {
    @Input() Servers: TestToolServerData[] = [];
    @Input() Connections: TestToolConnectionData[] = [];
    @Input() Tools: TestToolData[] = [];
    /** Pre-selected server ID */
    @Input() SelectedServerID: string | null = null;
    /** Pre-selected connection ID */
    @Input() SelectedConnectionID: string | null = null;
    /** Pre-selected tool ID */
    @Input() SelectedToolID: string | null = null;
    /** Current step: 'select' | 'configure' | 'results' */
    CurrentStep: 'select' | 'configure' | 'results' = 'select';
    /** Selected IDs */
    ServerID: string | null = null;
    ConnectionID: string | null = null;
    ToolID: string | null = null;
    /** Filtered lists based on selection */
    FilteredConnections: TestToolConnectionData[] = [];
    FilteredTools: TestToolData[] = [];
    /** Display lists for dropdowns (filtered by search) */
    DisplayServers: TestToolServerData[] = [];
    DisplayConnections: TestToolConnectionData[] = [];
    DisplayTools: TestToolData[] = [];
    /** No connections warning */
    NoConnectionsWarning: string | null = null;
    /** Selected tool details */
    SelectedTool: TestToolData | null = null;
    ParameterConfigs: ParameterConfig[] = [];
    ParameterValues: Record<string, unknown> = {};
    /** Execution state */
    IsExecuting = false;
    ExecutionResult: ToolExecutionResult | null = null;
    ExecutionError: string | null = null;
    /** User settings key prefix */
    private readonly SETTINGS_PREFIX = 'mcp-tool-test/';
    /** Panel width settings key */
    private readonly PANEL_WIDTH_SETTING_KEY = 'mcp-test-tool-panel/width';
    /** Panel width constraints */
    private readonly MAX_PANEL_WIDTH = 900;
    private readonly DEFAULT_PANEL_WIDTH = 700;
    /** Current panel width in pixels */
    /** Whether user is currently resizing */
    /** Whether panel should be full-width (mobile mode) */
    /** Subject for debounced width persistence */
    private gqlProvider = GraphQLDataProvider.Instance;
        // Debounce width persistence to avoid excessive writes
        // Load saved panel width
        // Initialize display arrays
        this.DisplayServers = [...this.Servers];
        // Apply pre-selected values
        if (this.SelectedServerID) {
            this.ServerID = this.SelectedServerID;
            this.onServerChange();
        if (this.SelectedConnectionID) {
            this.ConnectionID = this.SelectedConnectionID;
            this.onConnectionChange();
        if (this.SelectedToolID) {
            this.ToolID = this.SelectedToolID;
    // Selection Handlers
    onServerChange(): void {
        this.NoConnectionsWarning = null;
        // Filter connections by selected server
        if (this.ServerID) {
            this.FilteredConnections = this.Connections.filter(
                c => c.MCPServerID === this.ServerID && c.Status === 'Active'
            // Filter tools by selected server
            this.FilteredTools = this.Tools.filter(
                t => t.MCPServerID === this.ServerID && t.Status === 'Active'
            // Check if server requires authentication
            const server = this.Servers.find(s => s.ID === this.ServerID);
            const serverRequiresAuth = server?.Status === 'Active'; // All servers currently need connections
            // Auto-select connection logic
            if (this.FilteredConnections.length === 0) {
                this.ConnectionID = null;
                if (serverRequiresAuth) {
                    this.NoConnectionsWarning = 'No active connections available for this server. Please create a connection first.';
            } else if (this.FilteredConnections.length === 1) {
                // Auto-select the only connection
                this.ConnectionID = this.FilteredConnections[0].ID;
                // Select the first connection by default
            this.FilteredConnections = [];
            this.FilteredTools = [];
        // Update display arrays for filtering
        this.DisplayConnections = [...this.FilteredConnections];
        this.DisplayTools = [...this.FilteredTools];
        // Reset tool selection
        this.ToolID = null;
    onConnectionChange(): void {
        // Connection changed - tools are filtered by server, not connection
    onToolChange(): void {
    // Dropdown Filter Handlers
     * Handle server dropdown filter change
    onServerFilterChange(filter: string): void {
        const filterLower = (filter || '').toLowerCase();
        this.DisplayServers = this.Servers.filter(s =>
            s.Name.toLowerCase().includes(filterLower) ||
            (s.Description?.toLowerCase().includes(filterLower) ?? false)
     * Handle connection dropdown filter change
    onConnectionFilterChange(filter: string): void {
        this.DisplayConnections = this.FilteredConnections.filter(c =>
            c.Name.toLowerCase().includes(filterLower) ||
            (c.Description?.toLowerCase().includes(filterLower) ?? false)
     * Handle tool dropdown filter change
    onToolFilterChange(filter: string): void {
        this.DisplayTools = this.FilteredTools.filter(t =>
            t.ToolName.toLowerCase().includes(filterLower) ||
            (t.ToolTitle?.toLowerCase().includes(filterLower) ?? false) ||
            (t.ToolDescription?.toLowerCase().includes(filterLower) ?? false)
     * Can proceed to configuration step
    get CanProceedToConfig(): boolean {
        return this.ServerID != null && this.ConnectionID != null && this.ToolID != null;
     * Proceed to configuration step
    async proceedToConfig(): Promise<void> {
        if (!this.CanProceedToConfig) return;
        this.SelectedTool = this.Tools.find(t => t.ID === this.ToolID) || null;
        if (!this.SelectedTool) return;
        // Parse input schema and create parameter configs
        this.parseInputSchema();
        // Load cached parameter values
        await this.loadCachedParameters();
        this.CurrentStep = 'configure';
     * Parse the tool's input schema to create parameter configurations
    private parseInputSchema(): void {
        if (!this.SelectedTool?.InputSchema) {
            this.ParameterConfigs = [];
            const schema: JsonSchema = JSON.parse(this.SelectedTool.InputSchema);
            this.ParameterConfigs = Object.entries(properties).map(([name, prop]) => {
                const propDef = prop as JsonSchemaProperty;
                    type: this.normalizeType(propDef.type),
                    description: propDef.description || '',
                    required: required.includes(name),
                    defaultValue: propDef.default,
                    enumValues: propDef.enum || [],
                    format: propDef.format || null,
                    minimum: propDef.minimum,
                    maximum: propDef.maximum
            // Sort: required first, then alphabetically
            this.ParameterConfigs.sort((a, b) => {
                if (a.required !== b.required) return a.required ? -1 : 1;
                return a.name.localeCompare(b.name);
            // Initialize parameter values with defaults
            this.ParameterValues = {};
            for (const config of this.ParameterConfigs) {
                if (config.defaultValue !== undefined) {
                    this.ParameterValues[config.name] = config.defaultValue;
                    this.ParameterValues[config.name] = this.getDefaultForType(config.type);
            console.error('Failed to parse input schema:', error);
     * Normalize JSON Schema type to string
    private normalizeType(type: string | string[] | undefined): string {
        if (!type) return 'string';
        if (Array.isArray(type)) {
            // Filter out 'null' and take first type
            const nonNull = type.filter(t => t !== 'null');
            return nonNull.length > 0 ? nonNull[0] : 'string';
     * Get default value for a type
    private getDefaultForType(type: string): unknown {
            case 'string': return '';
            case 'integer': return null;
            case 'boolean': return false;
            case 'array': return [];
            case 'object': return {};
     * Load cached parameter values from UserInfoEngine
    private async loadCachedParameters(): Promise<void> {
        const cachedValue = engine.GetSetting(settingKey);
        if (cachedValue) {
                const cached = JSON.parse(cachedValue);
                // Merge cached values with defaults (cached takes precedence)
                this.ParameterValues = { ...this.ParameterValues, ...cached };
                console.warn('Failed to parse cached parameters:', error);
     * Save current parameter values to cache
    private async saveCachedParameters(): Promise<void> {
            await engine.SetSetting(settingKey, JSON.stringify(this.ParameterValues));
            console.warn('Failed to save cached parameters:', error);
     * Get the setting key for caching parameters
        return `${this.SETTINGS_PREFIX}${this.ServerID}/${this.ToolID}`;
    // Parameter Input Helpers
     * Get input type for a parameter
    getInputType(config: ParameterConfig): string {
        if (config.enumValues.length > 0) return 'select';
        if (config.format === 'date') return 'date';
        if (config.format === 'date-time') return 'datetime-local';
        if (config.format === 'email') return 'email';
        if (config.format === 'uri' || config.format === 'url') return 'url';
        switch (config.type) {
            case 'boolean': return 'checkbox';
            case 'number': return 'number';
            case 'object': return 'textarea';
            default: return 'text';
     * Check if parameter should use textarea
    isTextarea(config: ParameterConfig): boolean {
        return config.type === 'array' || config.type === 'object' ||
               (config.description != null && config.description.length > 100);
     * Handle parameter value change
    onParameterChange(name: string, value: unknown): void {
        this.ParameterValues[name] = value;
     * Get parameter value as string for textarea display
    getTextareaValue(name: string): string {
        const value = this.ParameterValues[name];
     * Handle textarea change - parse JSON if needed
    onTextareaChange(name: string, value: string, config: ParameterConfig): void {
        if (config.type === 'array' || config.type === 'object') {
                this.ParameterValues[name] = JSON.parse(value || (config.type === 'array' ? '[]' : '{}'));
    // Execution
     * Validate that all required parameters have values
    get IsValid(): boolean {
            if (config.required) {
                const value = this.ParameterValues[config.name];
                if (value === null || value === undefined || value === '') {
     * Execute the tool
    async executeTool(): Promise<void> {
        if (!this.IsValid || !this.ConnectionID || !this.ToolID) return;
        this.IsExecuting = true;
        this.ExecutionError = null;
        // Save parameters before execution
        await this.saveCachedParameters();
        // Build input arguments, filtering out empty optional values
        const inputArgs: Record<string, unknown> = {};
            if (config.required || (value !== null && value !== undefined && value !== '')) {
                inputArgs[config.name] = value;
            const result = await this.gqlProvider.ExecuteGQL(ExecuteMCPToolMutation, {
                    ConnectionID: this.ConnectionID,
                    ToolID: this.ToolID,
                    ToolName: this.SelectedTool?.ToolName,
                    InputArgs: JSON.stringify(inputArgs)
            this.ExecutionResult = result?.ExecuteMCPTool || {
                ErrorMessage: 'No result returned from server'
            this.CurrentStep = 'results';
            this.ExecutionError = error instanceof Error ? error.message : String(error);
            this.ExecutionResult = {
                ErrorMessage: this.ExecutionError
            this.IsExecuting = false;
    // Results Helpers
     * Format the result for display
        if (!this.ExecutionResult?.Result) return '';
            if (typeof this.ExecutionResult.Result === 'string') {
                // Try to parse as JSON for pretty printing
                const parsed = JSON.parse(this.ExecutionResult.Result);
            return JSON.stringify(this.ExecutionResult.Result, null, 2);
            return String(this.ExecutionResult.Result);
     * Check if result is JSON
    get IsResultJson(): boolean {
        if (!this.ExecutionResult?.Result) return false;
                JSON.parse(this.ExecutionResult.Result);
            return typeof this.ExecutionResult.Result === 'object';
     * Copy result to clipboard
        if (!this.FormattedResult) return;
            await navigator.clipboard.writeText(this.FormattedResult);
     * Go back to previous step
    goBack(): void {
        if (this.CurrentStep === 'results') {
        } else if (this.CurrentStep === 'configure') {
            this.CurrentStep = 'select';
     * Run the tool again with same parameters
    async runAgain(): Promise<void> {
        this.ExecutionResult = null;
    closeDialog(): void {
     * Start resize operation
        if (this.IsMobileMode) return; // No resize on mobile
        // Calculate width from right edge of viewport to cursor
        // Clamp to bounds
            Math.min(this.MAX_PANEL_WIDTH, window.innerWidth - 50) // Leave 50px margin
     * End resize operation
            // Persist panel width (debounced)
     * Handle window resize for mobile mode
        // In mobile mode, always use full width
            // Ensure panel doesn't exceed viewport
            if (this.PanelWidth > window.innerWidth - 50) {
            console.warn('[MCPTestToolPanel] Failed to load saved panel width:', error);
     * Persist panel width to user settings
            console.warn('[MCPTestToolPanel] Failed to persist panel width:', error);
     * Get display name for selected server
    get SelectedServerName(): string {
        return server?.Name || '';
     * Get display name for selected connection
    get SelectedConnectionName(): string {
        const connection = this.Connections.find(c => c.ID === this.ConnectionID);
        return connection?.Name || '';
     * Get display name for selected tool
    get SelectedToolName(): string {
        return this.SelectedTool?.ToolTitle || this.SelectedTool?.ToolName || '';
