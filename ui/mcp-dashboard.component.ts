 * @fileoverview MCP (Model Context Protocol) Management Dashboard
 * Provides a comprehensive admin interface for managing MCP servers,
 * connections, tools, and viewing execution logs.
 * Uses left sidebar navigation with query string deep linking via NavigationService.
 * @module MCP Dashboard
import { Component, OnDestroy, ChangeDetectorRef, AfterViewInit, OnInit } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { Subject, BehaviorSubject, Subscription } from 'rxjs';
    MCPEngine,
    UserInfoEngine,
    MJOAuthTokenEntity
import { MCPToolsService, MCPSyncState, MCPSyncResult } from './services/mcp-tools.service';
 * User preferences for the MCP Dashboard
interface MCPDashboardUserPreferences {
    toolsViewMode: ToolsViewMode;
    toolsSortBy: ToolsSortBy;
    toolsSortAscending: boolean;
    logsSortColumn: 'status' | 'server' | 'tool' | 'connection' | 'started' | 'duration' | 'error';
    logsSortAscending: boolean;
    serverStatus: string;
    connectionStatus: string;
    toolStatus: string;
    logStatus: string;
 * Interface for MCP Server data
export interface MCPServerData {
    Description: string | null;
    TransportType: string;
    ServerURL: string | null;
    Command: string | null;
    DefaultAuthType: string;
    RateLimitPerMinute: number | null;
    RateLimitPerHour: number | null;
    LastSyncAt: Date | null;
    ConnectionCount?: number;
    ToolCount?: number;
    // OAuth configuration fields
    OAuthIssuerURL?: string | null;
    OAuthScopes?: string | null;
    OAuthMetadataCacheTTLMinutes?: number | null;
    OAuthClientID?: string | null;
    OAuthClientSecretEncrypted?: string | null;
 * Interface for MCP Connection data
export interface MCPConnectionData {
    ServerName?: string;
    LastConnectedAt: Date | null;
    LastErrorMessage: string | null;
 * Interface for MCP Tool data
export interface MCPToolData {
    ToolTitle: string | null;
    ToolDescription: string | null;
    DiscoveredAt: Date;
    LastSeenAt: Date;
 * Interface for server group (tools grouped by server)
export interface MCPServerGroup {
    server: MCPServerData;
    tools: MCPToolData[];
 * Tools view mode
export type ToolsViewMode = 'card' | 'list';
 * Tools sort options
export type ToolsSortBy = 'name' | 'server' | 'discovered' | 'lastSeen';
 * Interface for MCP Execution Log data
export interface MCPExecutionLogData {
    ConnectionID: string;
    ConnectionName?: string;
    ToolID: string | null;
    UserName?: string;
    InputArgs?: string | null;
    Result?: string | null;
 * Dashboard filter options
export interface MCPDashboardFilters {
 * Dashboard statistics
export interface MCPDashboardStats {
    totalServers: number;
    activeServers: number;
    totalConnections: number;
    activeConnections: number;
    totalTools: number;
    activeTools: number;
 * Active tab type
export type MCPDashboardTab = 'servers' | 'connections' | 'tools' | 'logs';
 * MCP Management Dashboard Component
 * Provides a tab-based interface for managing:
 * - MCP Servers
 * - MCP Connections
 * - MCP Tools
 * - Execution Logs
@RegisterClass(BaseDashboard, 'MCPDashboard')
    selector: 'mj-mcp-dashboard',
    templateUrl: './mcp-dashboard.component.html',
    styleUrls: ['./mcp-dashboard.component.css']
export class MCPDashboardComponent extends BaseDashboard implements OnInit, AfterViewInit, OnDestroy {
    private readonly USER_SETTINGS_KEY = 'MCP.Dashboard.UserPreferences';
    public servers: MCPServerData[] = [];
    public connections: MCPConnectionData[] = [];
    public tools: MCPToolData[] = [];
    public executionLogs: MCPExecutionLogData[] = [];
    public filteredServers: MCPServerData[] = [];
    public filteredConnections: MCPConnectionData[] = [];
    public filteredTools: MCPToolData[] = [];
    public filteredLogs: MCPExecutionLogData[] = [];
    public stats: MCPDashboardStats = {
        totalServers: 0,
        activeServers: 0,
        totalConnections: 0,
        activeConnections: 0,
        totalTools: 0,
        activeTools: 0,
        failedExecutions: 0
    public ActiveTab: MCPDashboardTab = 'servers';
    public ErrorMessage: string | null = null;
    public filters$ = new BehaviorSubject<MCPDashboardFilters>({
        serverStatus: 'all',
        connectionStatus: 'all',
        toolStatus: 'all',
        logStatus: 'all'
    // Dialog state
    public ShowServerDialog = false;
    public ShowConnectionDialog = false;
    public ShowTestToolDialog = false;
    public EditingServer: MCPServerData | null = null;
    public EditingConnection: MCPConnectionData | null = null;
    // Test tool dialog pre-selection
    public TestToolServerID: string | null = null;
    public TestToolConnectionID: string | null = null;
    public TestToolID: string | null = null;
    // Log detail panel state
    public ShowLogDetailPanel = false;
    public SelectedLog: MCPExecutionLogData | null = null;
    // Expandable server/connection cards
    public ExpandedServerID: string | null = null;
    public ExpandedConnectionID: string | null = null;
    // Logs sorting state
    public LogsSortColumn: 'status' | 'server' | 'tool' | 'connection' | 'started' | 'duration' | 'error' = 'started';
    public LogsSortAscending = false; // Default to descending (most recent first)
    // Tools tab state
    public ToolsViewMode: ToolsViewMode = 'card';
    public ToolsSortBy: ToolsSortBy = 'server';
    public ToolsSortAscending = true;
    public ServerGroups: MCPServerGroup[] = [];
    public ExpandedToolId: string | null = null;
    // Navigation state
    private skipUrlUpdate = true;
    // Sync state
    public SyncStates = new Map<string, MCPSyncState>();
    private syncSubscriptions = new Map<string, Subscription>();
    // Filter panel state
    public FilterPanelVisible = true;
        private mcpToolsService: MCPToolsService
        // Check for OAuth completion redirect
        this.checkOAuthCompletion();
        // Parse initial URL state
        this.parseAndApplyUrlState();
        // Apply configuration params if passed via NavigationService
        this.applyConfigurationParams();
        // Enable URL updates after initialization
        // Subscribe to router events to handle browser back/forward
     * Parses the current URL query string and applies the tab state
    private parseAndApplyUrlState(): void {
        if (urlState?.tab) {
            this.ActiveTab = urlState.tab;
     * Parses URL query string to extract navigation state
    private parseUrlState(): { tab?: MCPDashboardTab } | null {
        if (queryIndex === -1) return null;
        const tab = params.get('tab') as MCPDashboardTab | null;
        if (tab && this.isValidTab(tab)) {
            return { tab };
     * Validates that a tab value is a valid MCPDashboardTab
    private isValidTab(tab: string): tab is MCPDashboardTab {
        return ['servers', 'connections', 'tools', 'logs'].includes(tab);
     * Applies configuration params passed via NavigationService
    private applyConfigurationParams(): void {
        if (config?.tab && this.isValidTab(config.tab as string)) {
            this.ActiveTab = config.tab as MCPDashboardTab;
     * Subscribes to router NavigationEnd events to handle browser back/forward
                const prefs = JSON.parse(savedPrefs) as MCPDashboardUserPreferences;
            console.warn('[MCPDashboard] Failed to load user preferences:', error);
    private applyUserPreferencesFromStorage(prefs: MCPDashboardUserPreferences): void {
        if (prefs.toolsViewMode) {
            this.ToolsViewMode = prefs.toolsViewMode;
        if (prefs.toolsSortBy) {
            this.ToolsSortBy = prefs.toolsSortBy;
        if (prefs.toolsSortAscending !== undefined) {
            this.ToolsSortAscending = prefs.toolsSortAscending;
        if (prefs.logsSortColumn) {
            this.LogsSortColumn = prefs.logsSortColumn;
        if (prefs.logsSortAscending !== undefined) {
            this.LogsSortAscending = prefs.logsSortAscending;
        // Apply filter preferences
        const currentFilters = this.filters$.value;
        this.filters$.next({
            searchTerm: prefs.searchTerm || '',
            serverStatus: prefs.serverStatus || 'all',
            connectionStatus: prefs.connectionStatus || 'all',
            toolStatus: prefs.toolStatus || 'all',
            logStatus: prefs.logStatus || 'all'
        // Apply filter panel visibility
            this.FilterPanelVisible = prefs.filterPanelVisible;
    private getCurrentPreferences(): MCPDashboardUserPreferences {
            toolsViewMode: this.ToolsViewMode,
            toolsSortBy: this.ToolsSortBy,
            toolsSortAscending: this.ToolsSortAscending,
            logsSortColumn: this.LogsSortColumn,
            logsSortAscending: this.LogsSortAscending,
            searchTerm: currentFilters.searchTerm,
            serverStatus: currentFilters.serverStatus,
            connectionStatus: currentFilters.connectionStatus,
            toolStatus: currentFilters.toolStatus,
            logStatus: currentFilters.logStatus,
            filterPanelVisible: this.FilterPanelVisible
            console.warn('[MCPDashboard] Failed to persist user preferences:', error);
     * Handles external navigation (browser back/forward)
        if (queryIndex === -1) return;
        if (tab && this.isValidTab(tab) && tab !== this.ActiveTab) {
            this.ActiveTab = tab;
     * Updates URL query string to reflect current state using NavigationService
        const queryParams: Record<string, string | null> = {
            tab: this.ActiveTab
    // Required by BaseResourceComponent
        return 'MCP Management';
    // Required by BaseDashboard
        this.setupFilterSubscription();
        this.loadAllData();
        // Cleanup sync subscriptions
        this.syncSubscriptions.forEach(sub => sub.unsubscribe());
        this.syncSubscriptions.clear();
    // Data Loading
     * Loads all dashboard data using MCPEngine for cached entities
     * and RunView for execution logs (historical data loaded on-demand)
     * @param forceRefresh - If true, forces MCPEngine to reload from database (use after sync operations)
    public async loadAllData(forceRefresh: boolean = false): Promise<void> {
        this.ErrorMessage = null;
            // Initialize MCPEngine and load execution logs in parallel
            // forceRefresh=true is needed after sync operations since backend changes
            // won't trigger local BaseEntity events
            const [, logsResult] = await Promise.all([
                MCPEngine.Instance.Config(forceRefresh),
                rv.RunView<MJMCPToolExecutionLogEntity>({
                    EntityName: 'MJ: MCP Tool Execution Logs',
                    ExtraFilter: `StartedAt >= DATEADD(day, -7, GETUTCDATE())`,
            // Get cached data from MCPEngine and convert to interface types
            this.servers = MCPEngine.Instance.Servers.map(s => ({
                ID: s.ID,
                Name: s.Name,
                Description: s.Description,
                TransportType: s.TransportType,
                ServerURL: s.ServerURL,
                Command: s.Command,
                DefaultAuthType: s.DefaultAuthType,
                Status: s.Status,
                RateLimitPerMinute: s.RateLimitPerMinute,
                RateLimitPerHour: s.RateLimitPerHour,
                LastSyncAt: s.LastSyncAt,
                OAuthIssuerURL: s.OAuthIssuerURL,
                OAuthScopes: s.OAuthScopes,
                OAuthMetadataCacheTTLMinutes: s.OAuthMetadataCacheTTLMinutes,
                OAuthClientID: s.OAuthClientID,
                OAuthClientSecretEncrypted: s.OAuthClientSecretEncrypted,
                OAuthRequirePKCE: s.OAuthRequirePKCE
            this.connections = MCPEngine.Instance.Connections.map(c => ({
                ID: c.ID,
                MCPServerID: c.MCPServerID,
                Name: c.Name,
                Description: c.Description,
                Status: c.Status,
                CompanyID: c.CompanyID,
                AutoSyncTools: c.AutoSyncTools,
                LogToolCalls: c.LogToolCalls,
                LastConnectedAt: c.LastConnectedAt,
                LastErrorMessage: c.LastErrorMessage
            this.tools = MCPEngine.Instance.Tools.map(t => ({
                ID: t.ID,
                MCPServerID: t.MCPServerID,
                ToolName: t.ToolName,
                ToolTitle: t.ToolTitle,
                ToolDescription: t.ToolDescription,
                InputSchema: t.InputSchema,
                Status: t.Status,
                DiscoveredAt: t.DiscoveredAt,
                LastSeenAt: t.LastSeenAt
            if (logsResult.Success) {
                // Map database column names to UI interface property names
                this.executionLogs = (logsResult.Results || []).map(log => this.mapLogFromDatabase(log));
            // Enrich data with counts and names
            this.enrichServerData();
            this.enrichConnectionData();
            this.enrichToolData();
            this.enrichLogData();
            this.ErrorMessage = `Failed to load data: ${error instanceof Error ? error.message : String(error)}`;
    private enrichServerData(): void {
        for (const server of this.servers) {
            server.ConnectionCount = this.connections.filter(c => c.MCPServerID === server.ID).length;
            server.ToolCount = this.tools.filter(t => t.MCPServerID === server.ID).length;
    private enrichConnectionData(): void {
            const server = this.servers.find(s => s.ID === conn.MCPServerID);
            conn.ServerName = server?.Name ?? 'Unknown';
    private enrichToolData(): void {
        for (const tool of this.tools) {
            const server = this.servers.find(s => s.ID === tool.MCPServerID);
            tool.ServerName = server?.Name ?? 'Unknown';
    private enrichLogData(): void {
        for (const log of this.executionLogs) {
            const conn = this.connections.find(c => c.ID === log.ConnectionID);
            log.ConnectionName = conn?.Name ?? log.ConnectionName ?? 'Unknown';
            // Add server name via connection
            if (conn) {
                log.ServerName = server?.Name ?? 'Unknown';
                log.ServerName = log.ServerName ?? 'Unknown';
        // Sort logs by StartedAt descending (most recent first)
        this.executionLogs.sort((a, b) => {
            const dateA = new Date(a.StartedAt).getTime();
            const dateB = new Date(b.StartedAt).getTime();
     * Maps database log entity to UI interface format
     * Handles column name differences between DB schema and UI interface
    private mapLogFromDatabase(dbLog: MJMCPToolExecutionLogEntity): MCPExecutionLogData {
        // Determine status from Success boolean
        let status: string;
        if (dbLog.Success === true) {
            status = 'Success';
        } else if (dbLog.Success === false) {
            status = 'Error';
            // null means still running
            status = 'Running';
            ID: dbLog.ID,
            ConnectionID: dbLog.MCPServerConnectionID,
            ToolID: dbLog.MCPServerToolID,
            ToolName: dbLog.ToolName || 'Unknown',
            StartedAt: dbLog.StartedAt,
            CompletedAt: dbLog.EndedAt,
            DurationMs: dbLog.DurationMs,
            UserID: dbLog.UserID,
            UserName: dbLog.User,
            ErrorMessage: dbLog.ErrorMessage,
            InputArgs: dbLog.InputParameters,
            Result: dbLog.OutputContent,
            ConnectionName: dbLog.MCPServerConnection,
            ServerName: undefined // Will be enriched later
            totalServers: this.servers.length,
            activeServers: this.servers.filter(s => s.Status === 'Active').length,
            totalConnections: this.connections.length,
            activeConnections: this.connections.filter(c => c.Status === 'Active').length,
            totalTools: this.tools.length,
            activeTools: this.tools.filter(t => t.Status === 'Active').length,
            recentExecutions: this.executionLogs.length,
            failedExecutions: this.executionLogs.filter(l => l.Status === 'Error').length
    private setupFilterSubscription(): void {
        this.filters$
                distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)),
        const filters = this.filters$.value;
        const search = filters.searchTerm.toLowerCase();
        // Filter servers
        this.filteredServers = this.servers.filter(s => {
            const matchesSearch = !search ||
                s.Name.toLowerCase().includes(search) ||
                (s.Description?.toLowerCase().includes(search) ?? false);
            const matchesStatus = filters.serverStatus === 'all' || s.Status === filters.serverStatus;
            return matchesSearch && matchesStatus;
        // Filter connections
        this.filteredConnections = this.connections.filter(c => {
                (c.ServerName?.toLowerCase().includes(search) ?? false);
            const matchesStatus = filters.connectionStatus === 'all' || c.Status === filters.connectionStatus;
        // Filter tools
        this.filteredTools = this.tools.filter(t => {
                t.ToolName.toLowerCase().includes(search) ||
                (t.ToolTitle?.toLowerCase().includes(search) ?? false) ||
                (t.ToolDescription?.toLowerCase().includes(search) ?? false);
            const matchesStatus = filters.toolStatus === 'all' || t.Status === filters.toolStatus;
        // Build server groups for the tools tab
        this.buildServerGroups();
        // Filter logs
        this.filteredLogs = this.executionLogs.filter(l => {
                l.ToolName.toLowerCase().includes(search) ||
                (l.ConnectionName?.toLowerCase().includes(search) ?? false) ||
                (l.ServerName?.toLowerCase().includes(search) ?? false);
            const matchesStatus = filters.logStatus === 'all' || l.Status === filters.logStatus;
        // Apply current sort
        this.sortFilteredLogs();
        const current = this.filters$.value;
        this.filters$.next({ ...current, searchTerm: term });
    public onStatusFilterChange(filterType: string, value: string): void {
        switch (filterType) {
            case 'server':
                this.filters$.next({ ...current, serverStatus: value });
            case 'connection':
                this.filters$.next({ ...current, connectionStatus: value });
            case 'tool':
                this.filters$.next({ ...current, toolStatus: value });
            case 'log':
                this.filters$.next({ ...current, logStatus: value });
    // Filter Panel
     * Toggle filter panel visibility
        this.FilterPanelVisible = !this.FilterPanelVisible;
     * Handle filter changes from the filter panel component
    public onFiltersChange(filters: MCPDashboardFilters): void {
        this.filters$.next(filters);
     * Get the current filtered count based on active tab
    public get CurrentFilteredCount(): number {
        switch (this.ActiveTab) {
            case 'servers':
                return this.filteredServers.length;
            case 'connections':
                return this.filteredConnections.length;
            case 'tools':
                return this.filteredTools.length;
                return this.filteredLogs.length;
     * Get the total count based on active tab
    public get CurrentTotalCount(): number {
                return this.servers.length;
                return this.connections.length;
                return this.tools.length;
                return this.executionLogs.length;
     * Get current filters value (non-observable)
    public get CurrentFilters(): MCPDashboardFilters {
        return this.filters$.value;
    // Tab Navigation
     * Sets the active tab and updates URL for deep linking
    public setActiveTab(tab: MCPDashboardTab): void {
        if (this.ActiveTab === tab) return;
    // Server Operations
    public createServer(): void {
        this.EditingServer = null;
        this.ShowServerDialog = true;
    public editServer(server: MCPServerData): void {
        this.EditingServer = server;
    public async deleteServer(server: MCPServerData): Promise<void> {
        // Check for related connections first
        const relatedConnections = this.connections.filter((c: MCPConnectionData) => c.MCPServerID === server.ID);
        const relatedTools = this.tools.filter((t: MCPToolData) => t.MCPServerID === server.ID);
        if (relatedConnections.length > 0 || relatedTools.length > 0) {
            if (relatedConnections.length > 0) {
                parts.push(`${relatedConnections.length} connection(s)`);
            if (relatedTools.length > 0) {
                parts.push(`${relatedTools.length} tool(s)`);
            if (!confirm(
                `Server "${server.Name}" has ${parts.join(' and ')}.\n\n` +
                `All related records will be deleted. Are you sure you want to proceed?`
            // Delete related connections first (which will cascade to connection tools/permissions)
            for (const conn of relatedConnections) {
                    await this.deleteConnectionInternal(conn.ID);
                    this.ErrorMessage = `Failed to delete related connection "${conn.Name}": ${error instanceof Error ? error.message : String(error)}`;
            // Delete related tools
            for (const tool of relatedTools) {
                    await this.deleteToolInternal(tool.ID);
                    this.ErrorMessage = `Failed to delete related tool "${tool.ToolName}": ${error instanceof Error ? error.message : String(error)}`;
            if (!confirm(`Are you sure you want to delete server "${server.Name}"?`)) {
            const entity = await md.GetEntityObject<MJMCPServerEntity>('MJ: MCP Servers');
            const loaded = await entity.Load(server.ID);
                this.ErrorMessage = `Server not found: ${server.Name}`;
                const errorMsg = entity.LatestResult?.Message || entity.LatestResult?.CompleteMessage || 'Unknown error';
                this.ErrorMessage = `Failed to delete server: ${errorMsg}`;
            this.ErrorMessage = `Failed to delete server: ${error instanceof Error ? error.message : String(error)}`;
     * Internal helper to delete a tool by ID without confirmation
    private async deleteToolInternal(toolId: string): Promise<void> {
        const entity = await md.GetEntityObject<MJMCPServerToolEntity>('MJ: MCP Server Tools');
        const loaded = await entity.Load(toolId);
            throw new Error(`Tool not found`);
            const errorMsg = entity.LatestResult?.Message || entity.LatestResult?.CompleteMessage || 'Delete failed';
            throw new Error(errorMsg);
     * Internal helper to delete a connection by ID without confirmation.
     * Handles deletion of all related records that don't have ON DELETE CASCADE:
     * - MCP Tool Execution Logs
     * - OAuth Authorization States
     * - OAuth Client Registrations
     * - OAuth Tokens
    private async deleteConnectionInternal(connectionId: string): Promise<void> {
        console.log(`[MCPDashboard] deleteConnectionInternal called for connectionId: ${connectionId}`);
        // Delete execution logs for this connection
        console.log('[MCPDashboard] Deleting execution logs...');
        const logsResult = await rv.RunView<{ ID: string }>({
        if (logsResult.Success && logsResult.Results) {
            console.log(`[MCPDashboard] Found ${logsResult.Results.length} execution logs to delete`);
            for (const log of logsResult.Results) {
                console.log(`[MCPDashboard] Loading execution log ${log.ID}...`);
                const logEntity = await md.GetEntityObject<MJMCPToolExecutionLogEntity>('MJ: MCP Tool Execution Logs');
                console.log(`[MCPDashboard] Load result for execution log ${log.ID}: ${loaded}`);
                    const deleted = await logEntity.Delete();
                        console.error(`[MCPDashboard] Failed to delete execution log ${log.ID}:`, logEntity.LatestResult);
                        throw new Error(`Failed to delete execution log: ${logEntity.LatestResult?.Message || 'Unknown error'}`);
                    console.log(`[MCPDashboard] Deleted execution log ${log.ID}`);
                    console.warn(`[MCPDashboard] Could not load execution log ${log.ID} - may have been already deleted`);
        // Delete OAuth Authorization States for this connection
        console.log('[MCPDashboard] Deleting OAuth Authorization States...');
        const authStatesResult = await rv.RunView<{ ID: string }>({
            EntityName: 'MJ: O Auth Authorization States',
        if (authStatesResult.Success && authStatesResult.Results) {
            console.log(`[MCPDashboard] Found ${authStatesResult.Results.length} OAuth Authorization States to delete`);
            for (const state of authStatesResult.Results) {
                console.log(`[MCPDashboard] Loading OAuth Authorization State ${state.ID}...`);
                const stateEntity = await md.GetEntityObject<MJOAuthAuthorizationStateEntity>('MJ: O Auth Authorization States');
                const loaded = await stateEntity.Load(state.ID);
                console.log(`[MCPDashboard] Load result for OAuth Authorization State ${state.ID}: ${loaded}`);
                    const deleted = await stateEntity.Delete();
                        console.error(`[MCPDashboard] Failed to delete OAuth Authorization State ${state.ID}:`, stateEntity.LatestResult);
                        throw new Error(`Failed to delete OAuth Authorization State: ${stateEntity.LatestResult?.Message || 'Unknown error'}`);
                    console.log(`[MCPDashboard] Deleted OAuth Authorization State ${state.ID}`);
                    console.warn(`[MCPDashboard] Could not load OAuth Authorization State ${state.ID} - may have been already deleted`);
        // Delete OAuth Client Registrations for this connection
        console.log('[MCPDashboard] Deleting OAuth Client Registrations...');
        const clientRegsResult = await rv.RunView<{ ID: string }>({
            EntityName: 'MJ: O Auth Client Registrations',
        if (clientRegsResult.Success && clientRegsResult.Results) {
            console.log(`[MCPDashboard] Found ${clientRegsResult.Results.length} OAuth Client Registrations to delete`);
            for (const reg of clientRegsResult.Results) {
                console.log(`[MCPDashboard] Loading OAuth Client Registration ${reg.ID}...`);
                const regEntity = await md.GetEntityObject<MJOAuthClientRegistrationEntity>('MJ: O Auth Client Registrations');
                const loaded = await regEntity.Load(reg.ID);
                console.log(`[MCPDashboard] Load result for OAuth Client Registration ${reg.ID}: ${loaded}`);
                    const deleted = await regEntity.Delete();
                        console.error(`[MCPDashboard] Failed to delete OAuth Client Registration ${reg.ID}:`, regEntity.LatestResult);
                        throw new Error(`Failed to delete OAuth Client Registration: ${regEntity.LatestResult?.Message || 'Unknown error'}`);
                    console.log(`[MCPDashboard] Deleted OAuth Client Registration ${reg.ID}`);
                    console.warn(`[MCPDashboard] Could not load OAuth Client Registration ${reg.ID} - may have been already deleted`);
        // Delete OAuth Tokens for this connection
        console.log('[MCPDashboard] Deleting OAuth Tokens...');
        const tokensResult = await rv.RunView<{ ID: string }>({
            EntityName: 'MJ: O Auth Tokens',
        if (tokensResult.Success && tokensResult.Results) {
            console.log(`[MCPDashboard] Found ${tokensResult.Results.length} OAuth Tokens to delete`);
            for (const token of tokensResult.Results) {
                console.log(`[MCPDashboard] Loading OAuth Token ${token.ID}...`);
                const tokenEntity = await md.GetEntityObject<MJOAuthTokenEntity>('MJ: O Auth Tokens');
                const loaded = await tokenEntity.Load(token.ID);
                console.log(`[MCPDashboard] Load result for OAuth Token ${token.ID}: ${loaded}`);
                    const deleted = await tokenEntity.Delete();
                        console.error(`[MCPDashboard] Failed to delete OAuth Token ${token.ID}:`, tokenEntity.LatestResult);
                        throw new Error(`Failed to delete OAuth Token: ${tokenEntity.LatestResult?.Message || 'Unknown error'}`);
                    console.log(`[MCPDashboard] Deleted OAuth Token ${token.ID}`);
                    console.warn(`[MCPDashboard] Could not load OAuth Token ${token.ID} - may have been already deleted`);
        // Now delete the connection itself
        console.log('[MCPDashboard] All related records deleted. Now deleting the connection itself...');
        const entity = await md.GetEntityObject<MJMCPServerConnectionEntity>('MJ: MCP Server Connections');
            throw new Error(`Connection not found`);
    public async onServerDialogClose(result: { saved: boolean }): Promise<void> {
        this.ShowServerDialog = false;
        if (result.saved) {
    // Connection Operations
    public createConnection(): void {
        this.EditingConnection = null;
        this.ShowConnectionDialog = true;
    public editConnection(connection: MCPConnectionData): void {
        this.EditingConnection = connection;
    public async deleteConnection(connection: MCPConnectionData): Promise<void> {
        // Check for related execution logs
        const relatedLogs = this.executionLogs.filter(l => l.ConnectionID === connection.ID);
        if (relatedLogs.length > 0) {
                `Connection "${connection.Name}" has ${relatedLogs.length} execution log(s).\n\n` +
                `All related logs will be deleted. Are you sure you want to proceed?`
            if (!confirm(`Are you sure you want to delete connection "${connection.Name}"?`)) {
            // Use the internal method which handles deleting related records
            await this.deleteConnectionInternal(connection.ID);
            this.ErrorMessage = `Failed to delete connection: ${error instanceof Error ? error.message : String(error)}`;
    public async onConnectionDialogClose(result: { saved: boolean }): Promise<void> {
        this.ShowConnectionDialog = false;
     * Toggle tool card expansion for details view
    public toggleToolExpand(tool: MCPToolData): void {
        if (this.ExpandedToolId === tool.ID) {
            this.ExpandedToolId = null;
            this.ExpandedToolId = tool.ID;
     * Check if a tool card is expanded
    public isToolExpanded(tool: MCPToolData): boolean {
        return this.ExpandedToolId === tool.ID;
     * Set tools view mode (card or list)
    public setToolsViewMode(mode: ToolsViewMode): void {
        this.ToolsViewMode = mode;
     * Set tools sort option
    public setToolsSort(sortBy: ToolsSortBy): void {
        if (this.ToolsSortBy === sortBy) {
            // Toggle ascending/descending
            this.ToolsSortAscending = !this.ToolsSortAscending;
            this.ToolsSortBy = sortBy;
            this.ToolsSortAscending = true;
     * Toggle server group expansion
    public toggleServerGroup(group: MCPServerGroup): void {
        group.expanded = !group.expanded;
     * Build server groups from tools data
    private buildServerGroups(): void {
        // Group tools by server
        const serverMap = new Map<string, MCPToolData[]>();
        for (const tool of this.filteredTools) {
            const serverId = tool.MCPServerID;
            if (!serverMap.has(serverId)) {
                serverMap.set(serverId, []);
            serverMap.get(serverId)!.push(tool);
        // Build server groups
        this.ServerGroups = [];
            const tools = serverMap.get(server.ID) || [];
            if (tools.length > 0) {
                // Sort tools within group
                this.sortTools(tools);
                this.ServerGroups.push({
                    server,
                    tools,
                    expanded: true // Start expanded
        // Sort server groups by tool count or name
        this.ServerGroups.sort((a, b) => {
            if (this.ToolsSortBy === 'server') {
                const nameCompare = a.server.Name.localeCompare(b.server.Name);
                return this.ToolsSortAscending ? nameCompare : -nameCompare;
            // Sort by tool count (descending by default)
            const countCompare = b.tools.length - a.tools.length;
            return this.ToolsSortAscending ? -countCompare : countCompare;
     * Sort tools array in place
    private sortTools(tools: MCPToolData[]): void {
        tools.sort((a, b) => {
            let compare = 0;
            switch (this.ToolsSortBy) {
                    compare = (a.ToolTitle || a.ToolName).localeCompare(b.ToolTitle || b.ToolName);
                case 'discovered':
                    compare = new Date(a.DiscoveredAt).getTime() - new Date(b.DiscoveredAt).getTime();
                case 'lastSeen':
                    compare = new Date(a.LastSeenAt).getTime() - new Date(b.LastSeenAt).getTime();
            return this.ToolsSortAscending ? compare : -compare;
     * Get total tool count across all groups
    public get TotalToolCount(): number {
        return this.ServerGroups.reduce((sum, group) => sum + group.tools.length, 0);
     * Open Test Tool dialog
    public openTestToolDialog(tool?: MCPToolData, connection?: MCPConnectionData): void {
        if (tool) {
            this.TestToolServerID = tool.MCPServerID;
            this.TestToolID = tool.ID;
            this.TestToolServerID = null;
            this.TestToolID = null;
        if (connection) {
            this.TestToolConnectionID = connection.ID;
            if (!this.TestToolServerID) {
                this.TestToolServerID = connection.MCPServerID;
            this.TestToolConnectionID = null;
        this.ShowTestToolDialog = true;
     * Close Test Tool dialog
    public onTestToolDialogClose(): void {
        this.ShowTestToolDialog = false;
     * Parse and return input schema as formatted JSON
    public getFormattedInputSchema(tool: MCPToolData): string {
        if (!tool.InputSchema) return 'No input schema';
            const schema = JSON.parse(tool.InputSchema);
            return JSON.stringify(schema, null, 2);
            return tool.InputSchema;
     * Get parameter count from input schema
    public getParamCount(tool: MCPToolData): number {
        if (!tool.InputSchema) return 0;
            return Object.keys(schema.properties || {}).length;
     * Get required parameter count from input schema
    public getRequiredParamCount(tool: MCPToolData): number {
            return (schema.required || []).length;
    // Sync Operations
     * Syncs tools for a specific connection
    public async syncConnectionTools(connection: MCPConnectionData): Promise<void> {
        const connectionId = connection.ID;
        // Subscribe to sync state updates for this connection
        if (!this.syncSubscriptions.has(connectionId)) {
            const sub = this.mcpToolsService.getSyncState(connectionId)
                    this.SyncStates.set(connectionId, state);
            this.syncSubscriptions.set(connectionId, sub);
            const result: MCPSyncResult = await this.mcpToolsService.syncTools(connectionId);
                // Force refresh to show updated tools - backend changes don't trigger local events
                await this.loadAllData(true);
            } else if (result.RequiresOAuth || result.RequiresReauthorization) {
                // OAuth authorization is required - initiate fresh OAuth flow with frontend callback
                // Note: We ignore result.AuthorizationUrl because it was built with the server's callback URL
                // We need to initiate a fresh flow that uses the frontend callback URL
                console.log(`[MCPDashboard] OAuth ${result.RequiresReauthorization ? 're-' : ''}authorization required, initiating frontend OAuth flow...`);
                await this.initiateOAuthFlow(connectionId);
                this.ErrorMessage = `Sync failed: ${result.ErrorMessage}`;
            this.ErrorMessage = `Sync error: ${error instanceof Error ? error.message : String(error)}`;
     * Gets the sync state for a connection
    public getSyncState(connectionId: string): MCPSyncState | undefined {
        return this.SyncStates.get(connectionId);
     * Checks if a connection is currently syncing
    public isSyncing(connectionId: string): boolean {
        return this.mcpToolsService.isSyncing(connectionId);
     * Gets sync progress message for a connection
    public getSyncProgressMessage(connectionId: string): string {
        const state = this.SyncStates.get(connectionId);
        if (!state?.progress) return '';
        return state.progress.message;
    // OAuth Operations
     * Checks for OAuth completion by looking for query params set by the OAuth callback
    private checkOAuthCompletion(): void {
        const params = this.route.snapshot.queryParams;
        if (params['oauth'] === 'success') {
            // OAuth completed successfully
            const connectionId = params['connectionId'];
            console.log(`[MCPDashboard] OAuth authorization completed for connection: ${connectionId}`);
            this.showSuccessNotification('OAuth authorization completed successfully');
            // Clear the query params without triggering a full navigation
            this.router.navigate([], {
                relativeTo: this.route,
                queryParams: {},
                queryParamsHandling: 'merge',
                replaceUrl: true
            // Refresh data to pick up new OAuth state
            this.loadAllData(true);
        } else if (params['oauth'] === 'error') {
            // OAuth failed
            const errorCode = params['error'] || 'unknown_error';
            const errorMessage = params['error_description'] || 'Authorization failed';
            console.error(`[MCPDashboard] OAuth authorization failed: ${errorCode} - ${errorMessage}`);
            // Show error message
            this.ErrorMessage = `OAuth authorization failed: ${errorMessage}`;
            // Clear the query params
     * Shows a success notification (temporary)
    private showSuccessNotification(message: string): void {
        // For now, use a simple console log and clear any error message
        console.log(`[MCPDashboard] Success: ${message}`);
        // TODO: Implement proper toast/notification system
     * Initiates OAuth authorization flow for a connection.
     * Stores the current URL and redirects directly to the OAuth provider.
     * Always uses the frontend callback URL so the OAuth redirect comes back to MJExplorer.
     * @param connectionId - The MCP connection ID requiring OAuth
    public async initiateOAuthFlow(connectionId: string): Promise<void> {
        // Store current URL for return after OAuth completion
        // Store both full URL and path-only version for cross-origin scenarios
        const currentUrl = window.location.href;
        const currentPath = window.location.pathname + window.location.search;
        // Store in both localStorage and sessionStorage for redundancy
        localStorage.setItem('oauth_return_url', currentUrl);
        localStorage.setItem('oauth_return_path', currentPath);
        sessionStorage.setItem('oauth_return_url', currentUrl);
        sessionStorage.setItem('oauth_return_path', currentPath);
        console.log('[MCPDashboard] Stored OAuth return URL:', currentUrl);
        console.log('[MCPDashboard] Stored OAuth return path:', currentPath);
        // Initiate OAuth via GraphQL to get the URL with frontend callback
        const result = await this.initiateMCPOAuth(connectionId);
        if (result.Success && result.AuthorizationUrl) {
            console.log('[MCPDashboard] Redirecting to OAuth provider:', result.AuthorizationUrl);
            window.location.href = result.AuthorizationUrl;
            this.ErrorMessage = result.ErrorMessage || 'Failed to initiate OAuth authorization';
     * Calls the InitiateMCPOAuth mutation with frontend callback URL
    private async initiateMCPOAuth(connectionId: string): Promise<{
        AuthorizationUrl?: string;
            const { GraphQLDataProvider, gql } = await import('@memberjunction/graphql-dataprovider');
            const mutation = gql`
                mutation InitiateMCPOAuth($input: InitiateMCPOAuthInput!) {
                    InitiateMCPOAuth(input: $input) {
                        AuthorizationUrl
                        StateParameter
            // Use the frontend callback URL so we handle the OAuth redirect
            const frontendCallbackUrl = `${window.location.origin}/oauth/callback`;
            const result = await GraphQLDataProvider.Instance.ExecuteGQL(mutation, {
                    ConnectionID: connectionId,
                    FrontendCallbackUrl: frontendCallbackUrl
            return result?.InitiateMCPOAuth || { Success: false, ErrorMessage: 'No result returned' };
            return { Success: false, ErrorMessage: message };
    // Utility Methods
            case 'Active':
                return 'status-active';
            case 'Inactive':
                return 'status-inactive';
            case 'Error':
                return 'status-error';
            case 'Deprecated':
                return 'status-deprecated';
                return 'status-unknown';
    public getTransportIcon(transportType: string): string {
                return 'fa-solid fa-globe';
                return 'fa-solid fa-terminal';
                return 'fa-solid fa-plug';
        return d.toLocaleString();
        if (ms === null) return '-';
    // Logs Sorting
     * Handles click on a sortable column header
    public onLogSortColumn(column: typeof this.LogsSortColumn): void {
        if (this.LogsSortColumn === column) {
            // Toggle sort direction
            this.LogsSortAscending = !this.LogsSortAscending;
            // New column, default to descending for dates/duration, ascending for text
            this.LogsSortColumn = column;
            this.LogsSortAscending = column !== 'started' && column !== 'duration';
     * Sorts the filtered logs based on current sort settings
    private sortFilteredLogs(): void {
        const direction = this.LogsSortAscending ? 1 : -1;
        this.filteredLogs.sort((a, b) => {
            let valA: string | number | Date | null;
            let valB: string | number | Date | null;
            switch (this.LogsSortColumn) {
                    valA = a.Status || '';
                    valB = b.Status || '';
                    valA = a.ServerName || '';
                    valB = b.ServerName || '';
                    valA = a.ToolName || '';
                    valB = b.ToolName || '';
                    valA = a.ConnectionName || '';
                    valB = b.ConnectionName || '';
                case 'started':
                    valA = a.StartedAt ? new Date(a.StartedAt).getTime() : 0;
                    valB = b.StartedAt ? new Date(b.StartedAt).getTime() : 0;
                case 'duration':
                    valA = a.DurationMs ?? -1;
                    valB = b.DurationMs ?? -1;
                    valA = a.ErrorMessage || '';
                    valB = b.ErrorMessage || '';
            if (typeof valA === 'string' && typeof valB === 'string') {
                return valA.localeCompare(valB) * direction;
            if (valA < valB) return -1 * direction;
            if (valA > valB) return 1 * direction;
     * Gets the sort indicator class for a column header
    public getLogSortClass(column: typeof this.LogsSortColumn): string {
        if (this.LogsSortColumn !== column) return '';
        return this.LogsSortAscending ? 'sorted-asc' : 'sorted-desc';
    // Log Detail Panel
     * Handles click on a log row to show detail panel
    public async onLogClick(log: MCPExecutionLogData): Promise<void> {
        // Load full log details if needed (InputArgs, Result)
        if (!log.InputArgs && !log.Result) {
                const result = await rv.RunView<MJMCPToolExecutionLogEntity>({
                    ExtraFilter: `ID='${log.ID}'`,
                    const fullLog = this.mapLogFromDatabase(result.Results[0]);
                    // Merge full log data (preserving already enriched fields)
                    log.InputArgs = fullLog.InputArgs;
                    log.Result = fullLog.Result;
                    log.ErrorMessage = fullLog.ErrorMessage || log.ErrorMessage;
                console.warn('[MCPDashboard] Failed to load full log details:', error);
        // Enrich with server name
        const connection = this.connections.find(c => c.ID === log.ConnectionID);
            log.ServerName = connection.ServerName;
        this.SelectedLog = log;
        this.ShowLogDetailPanel = true;
     * Closes the log detail panel
    public onLogDetailClose(): void {
        this.ShowLogDetailPanel = false;
        this.SelectedLog = null;
     * Handles run again from log detail panel
    public onRunAgainFromLog(event: { toolId: string; connectionId: string }): void {
        // Open test tool dialog with pre-selected tool and connection
        const tool = this.tools.find(t => t.ID === event.toolId);
        const connection = this.connections.find(c => c.ID === event.connectionId);
    // Expandable Server/Connection Cards
     * Toggles server card expansion
    public toggleServerExpand(server: MCPServerData): void {
        if (this.ExpandedServerID === server.ID) {
            this.ExpandedServerID = null;
            this.ExpandedServerID = server.ID;
            // Collapse any expanded connection
            this.ExpandedConnectionID = null;
     * Toggles connection card expansion
    public toggleConnectionExpand(conn: MCPConnectionData): void {
        if (this.ExpandedConnectionID === conn.ID) {
            this.ExpandedConnectionID = conn.ID;
            // Collapse any expanded server
     * Checks if a server card is expanded
    public isServerExpanded(server: MCPServerData): boolean {
        return this.ExpandedServerID === server.ID;
     * Checks if a connection card is expanded
    public isConnectionExpanded(conn: MCPConnectionData): boolean {
        return this.ExpandedConnectionID === conn.ID;
     * Gets tools for a specific server
    public getToolsForServer(serverId: string): MCPToolData[] {
        return this.tools.filter(t => t.MCPServerID === serverId);
     * Gets tools for a specific connection (via its server)
    public getToolsForConnection(connectionId: string): MCPToolData[] {
        const connection = this.connections.find(c => c.ID === connectionId);
        if (!connection) return [];
        return this.tools.filter(t => t.MCPServerID === connection.MCPServerID);
     * Opens test tool dialog with a specific tool from expanded card
    public runToolFromCard(tool: MCPToolData, connection?: MCPConnectionData): void {
        this.TestToolConnectionID = connection?.ID ?? null;
