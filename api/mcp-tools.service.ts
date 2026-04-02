 * @fileoverview MCP Tools Service
 * Service for managing MCP tool operations including synchronization
 * with real-time progress streaming via GraphQL subscriptions.
import { Subject, BehaviorSubject, Observable } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';
 * Result of an MCP tool sync operation
export interface MCPSyncResult {
    Added: number;
    Updated: number;
    Deprecated: number;
    /** Whether OAuth authorization is required before connecting */
    RequiresOAuth?: boolean;
    /** OAuth authorization URL if authorization is required */
    /** OAuth state parameter for tracking the authorization flow */
    StateParameter?: string;
    /** Whether OAuth re-authorization is required */
    RequiresReauthorization?: boolean;
    /** Reason for re-authorization if required */
    ReauthorizationReason?: string;
 * Progress message received during sync
export interface MCPSyncProgress {
    resolver: string;
    status: 'ok' | 'error';
    phase: 'connecting' | 'listing' | 'syncing' | 'complete' | 'error';
    progress?: {
 * Sync state for a connection
export interface MCPSyncState {
    isSyncing: boolean;
    progress: MCPSyncProgress | null;
    lastResult: MCPSyncResult | null;
    error: string | null;
 * GraphQL mutation for syncing MCP tools
const SyncMCPToolsMutation = gql`
    mutation SyncMCPTools($input: SyncMCPToolsInput!) {
        SyncMCPTools(input: $input) {
            Deprecated
            ServerName
            ConnectionName
            RequiresOAuth
            RequiresReauthorization
            ReauthorizationReason
 * Service for MCP tool synchronization operations
export class MCPToolsService implements OnDestroy {
    /** Map of connection ID to sync state */
    private syncStates = new Map<string, BehaviorSubject<MCPSyncState>>();
    /** Provider instance for GraphQL operations */
        this.setupProgressListener();
        this.syncStates.forEach(state => state.complete());
        this.syncStates.clear();
     * Gets the sync state observable for a connection
    public getSyncState(connectionId: string): Observable<MCPSyncState> {
        return this.getOrCreateSyncState(connectionId).asObservable();
     * Gets the current sync state value for a connection
    public getSyncStateValue(connectionId: string): MCPSyncState {
        return this.getOrCreateSyncState(connectionId).getValue();
        const state = this.syncStates.get(connectionId);
        return state ? state.getValue().isSyncing : false;
     * Syncs tools for a specific MCP connection
     * @param connectionId - The ID of the MCP connection to sync
     * @param forceSync - Optional flag to force sync even if recently synced
     * @returns Promise resolving to the sync result
    public async syncTools(connectionId: string, forceSync = false): Promise<MCPSyncResult> {
        const state$ = this.getOrCreateSyncState(connectionId);
        // Check if already syncing
        if (state$.getValue().isSyncing) {
            throw new Error('Sync already in progress for this connection');
        // Update state to syncing
        state$.next({
            ...state$.getValue(),
            isSyncing: true,
                resolver: 'MCPResolver',
                type: 'MCPToolSyncProgress',
                phase: 'connecting',
                message: 'Starting sync...'
            // Execute the mutation
            const result = await this.gqlProvider.ExecuteGQL(SyncMCPToolsMutation, {
                    ForceSync: forceSync
            const syncResult: MCPSyncResult = result?.SyncMCPTools || {
                ErrorMessage: 'No result returned from server',
                Added: 0,
                Updated: 0,
                Deprecated: 0,
                Total: 0
            // Update state with result
                isSyncing: false,
                progress: null,
                lastResult: syncResult,
                error: syncResult.Success ? null : (syncResult.ErrorMessage || 'Sync failed')
            return syncResult;
            // Update state with error
                lastResult: null,
                error: errorMsg
                ErrorMessage: errorMsg,
     * Clears the sync state for a connection
    public clearSyncState(connectionId: string): void {
        const state$ = this.syncStates.get(connectionId);
        if (state$) {
     * Gets or creates a sync state subject for a connection
    private getOrCreateSyncState(connectionId: string): BehaviorSubject<MCPSyncState> {
        let state$ = this.syncStates.get(connectionId);
        if (!state$) {
            state$ = new BehaviorSubject<MCPSyncState>({
            this.syncStates.set(connectionId, state$);
        return state$;
     * Sets up listener for progress updates via the GraphQL subscription
     * The statusUpdates subscription receives progress messages during sync
    private setupProgressListener(): void {
        // Listen for status updates from the GraphQL provider
        // These come through the existing PUSH_STATUS_UPDATES subscription
        // PushStatusUpdates returns Observable<string> with the message content
        this.gqlProvider.PushStatusUpdates()
                filter((message: string) => {
                        const parsed = JSON.parse(message || '{}');
                        return parsed.type === 'MCPToolSyncProgress';
            .subscribe((message: string) => {
                    const progress: MCPSyncProgress = JSON.parse(message || '{}');
                    this.handleProgressUpdate(progress);
                    console.error('Failed to parse MCP sync progress:', error);
     * Handles a progress update from the server
    private handleProgressUpdate(progress: MCPSyncProgress): void {
        const state$ = this.syncStates.get(progress.connectionId);
        if (state$ && state$.getValue().isSyncing) {
                progress
