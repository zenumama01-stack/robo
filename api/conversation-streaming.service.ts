import { BehaviorSubject, Subject, Subscription, firstValueFrom } from 'rxjs';
import { ActiveTasksService } from './active-tasks.service';
import { DataCacheService } from './data-cache.service';
 * Completion event structure broadcast when an agent finishes
export interface CompletionEvent {
  agentRunId: string;
 * Metadata structure for message progress updates
export interface MessageProgressMetadata {
  /** Progress details (from RunAIAgentResolver) */
    stepCount?: number;
    agentType?: string;
  /** Agent run information (from RunAIAgentResolver) */
  agentRun?: {
    Agent?: string;
    ConversationDetailID?: string;
    ID?: string;
  /** Agent run ID (alternative to agentRun.ID) */
 * Message progress update structure
export interface MessageProgressUpdate {
  percentComplete?: number;
  taskName?: string;
  metadata?: MessageProgressMetadata;
  /** Identifies which backend resolver published this update */
  resolver?: 'TaskOrchestrator' | 'RunAIAgentResolver' | string;
 * Callback type for message progress updates
export type MessageProgressCallback = (progress: MessageProgressUpdate) => Promise<void> | void;
 * Connection status for streaming service
export type StreamingConnectionStatus = 'connected' | 'disconnected' | 'error' | 'reconnecting';
 * Global streaming service that manages PubSub subscriptions for all conversations.
 * This service maintains a single WebSocket connection and routes updates to all
 * registered message callbacks, regardless of which conversation is currently visible.
 * This ensures that messages update correctly even when users navigate away and return.
export class ConversationStreamingService implements OnDestroy {
  // Single PubSub subscription shared across the entire app
  private pushStatusSubscription?: Subscription;
  // Registry of callbacks per conversation detail ID
  // Multiple callbacks can be registered for the same message (e.g., different components)
  private callbackRegistry = new Map<string, MessageProgressCallback[]>();
  // Track recent completions for late-arriving components (e.g., after navigation)
  // Key: conversationDetailId, Value: completion info with timestamp
  private recentCompletions = new Map<string, {
  // Observable for components to subscribe to completion events in real-time
  public completionEvents$ = new Subject<CompletionEvent>();
  // Subject to emit connection status changes
  private connectionStatus$ = new BehaviorSubject<StreamingConnectionStatus>('disconnected');
  // Track if service has been initialized
  // Reconnection timeout
  private reconnectionTimeout?: any;
    private dataCache: DataCacheService
   * Initialize the global PubSub subscription.
   * Should be called once at app startup (e.g., in workspace component).
  public initialize(): void {
      const dataProvider = GraphQLDataProvider.Instance;
      const sessionId = (dataProvider as any).sessionId || (dataProvider as any)._sessionId;
      this.pushStatusSubscription = dataProvider.PushStatusUpdates().subscribe({
        next: (status: any) => {
          this.handlePushStatusUpdate(status);
          console.error('[ConversationStreamingService] PubSub connection error:', error);
          this.connectionStatus$.next('error');
          this.scheduleReconnection();
        complete: () => {
          this.connectionStatus$.next('disconnected');
      this.connectionStatus$.next('connected');
      console.error('[ConversationStreamingService] Failed to initialize:', error);
   * Get the current connection status as an observable
  public getConnectionStatus$() {
    return this.connectionStatus$.asObservable();
   * Get the current connection status value
  public getConnectionStatus(): StreamingConnectionStatus {
    return this.connectionStatus$.value;
   * Register a callback for a specific conversation detail (message).
   * The callback will be invoked whenever progress updates arrive for this message.
   * @param conversationDetailId - The ID of the conversation detail entity
   * @param callback - Function to call with progress updates
  public registerMessageCallback(
    callback: MessageProgressCallback
    const existing = this.callbackRegistry.get(conversationDetailId) || [];
    existing.push(callback);
    this.callbackRegistry.set(conversationDetailId, existing);
   * Unregister a callback for a specific conversation detail.
   * If no callback is provided, all callbacks for the message are removed.
   * @param callback - Optional specific callback to remove
  public unregisterMessageCallback(
    callback?: MessageProgressCallback
      // Remove specific callback
      const filtered = existing.filter(cb => cb !== callback);
      if (filtered.length > 0) {
        this.callbackRegistry.set(conversationDetailId, filtered);
        this.callbackRegistry.delete(conversationDetailId);
      // Remove all callbacks for this message
      const hadCallbacks = this.callbackRegistry.has(conversationDetailId);
      if (hadCallbacks) {
        // console.log(`[ConversationStreamingService] Unregistered all callbacks for message ${conversationDetailId}`);
   * Get the number of registered callbacks (for monitoring/debugging)
  public getRegisteredCallbackCount(): number {
    for (const callbacks of this.callbackRegistry.values()) {
      count += callbacks.length;
   * Get the number of messages being tracked (for monitoring/debugging)
  public getTrackedMessageCount(): number {
    return this.callbackRegistry.size;
   * Handle incoming PubSub status updates and route to registered callbacks
  private async handlePushStatusUpdate(status: any): Promise<void> {
    if (!status) {
      console.warn('PubSub status is null or undefined');
      // Parse the status if it's a JSON string, otherwise use as-is
      // GraphQL subscription emits JSON strings that need to be parsed
      const statusObj = typeof status === 'string' ? JSON.parse(status) : status;
      // Handle both resolver types for agent progress updates
      if (statusObj.resolver === 'TaskOrchestrator') {
        // Task graph execution (multi-step workflows)
        await this.routeTaskProgress(statusObj);
      } else if (statusObj.resolver === 'RunAIAgentResolver') {
        // Direct agent execution (Sage, Research Agent, etc.)
        await this.routeAgentProgress(statusObj);
      console.error('[ConversationStreamingService] Error processing push status update:', error);
   * Route task progress updates to registered callbacks.
   * Uses conversationDetailId from PubSub message for direct routing.
  private async routeTaskProgress(statusObj: any): Promise<void> {
      // Extract progress information from status object
      const { taskName, message, percentComplete, metadata, conversationDetailId } = statusObj.data || {};
      // console.log(`[ConversationStreamingService] 📥 Received progress update:`, {
      //   taskName,
      //   hasMessage: !!message,
      //   conversationDetailId,
      //   registeredCallbacks: Array.from(this.callbackRegistry.keys())
      // });
        console.warn('[ConversationStreamingService] ⚠️  No message content in progress update');
        return; // No message content to update
      if (!conversationDetailId) {
        console.warn('[ConversationStreamingService] ⚠️  Progress update missing conversationDetailId, cannot route', { taskName, message });
      // Direct lookup using conversationDetailId from backend
      const callbacks = this.callbackRegistry.get(conversationDetailId) || [];
      if (callbacks.length === 0) {
        // No callbacks registered - message might be in a hidden conversation
        // This is fine, callbacks will be registered when conversation becomes visible
        console.warn(`[ConversationStreamingService] ⚠️  No callbacks registered for message ${conversationDetailId}. Registered: [${Array.from(this.callbackRegistry.keys()).join(', ')}]`);
      // Create progress update object
      const progressUpdate: MessageProgressUpdate = {
        percentComplete,
        taskName,
        resolver: 'TaskOrchestrator'
      // Invoke all registered callbacks for this specific message
      for (const callback of callbacks) {
          await callback(progressUpdate);
          console.error(`[ConversationStreamingService] Error executing callback for message ${conversationDetailId}:`, error);
          // Continue with other callbacks even if one fails
      // console.log(`[ConversationStreamingService] ✅ Routed progress update to message ${conversationDetailId} (${callbacks.length} callback(s))`);
      console.error('[ConversationStreamingService] Error routing task progress:', error);
   * Route agent progress updates (from RunAIAgentResolver) to registered callbacks.
   * Uses conversationDetailID from agentRun data for direct routing.
   * Also handles completion messages to remove tasks from ActiveTasksService.
  private async routeAgentProgress(statusObj: any): Promise<void> {
      // Extract progress information from RunAIAgentResolver message
      const { agentRun, progress, type } = statusObj.data || {};
      // Handle completion messages - backend sends type: 'complete' when agent finishes
      // Now includes conversationDetailId directly from backend (no reverse lookup needed)
      if (type === 'complete') {
        const agentRunId = statusObj.data?.agentRunId;
        const conversationDetailId = statusObj.data?.conversationDetailId;
        // Remove from active tasks (clears spinner in conversation list)
          const removed = this.activeTasks.removeByAgentRunId(agentRunId);
            console.log(`[ConversationStreamingService] ✅ Agent run ${agentRunId} completed, removed from active tasks`);
        // Broadcast completion event if we have the conversationDetailId
        if (conversationDetailId) {
          // Store for late-arriving components (navigation scenario)
          this.recentCompletions.set(conversationDetailId, {
            agentRunId: agentRunId || '',
          // Broadcast to all subscribers
          this.completionEvents$.next({
            agentRunId: agentRunId || ''
          // Cleanup old completions to prevent memory leak
          this.cleanupOldCompletions();
          console.log(`[ConversationStreamingService] 📢 Completion broadcast for message ${conversationDetailId}`);
          console.warn(`[ConversationStreamingService] ⚠️ Completion received without conversationDetailId for agentRunId: ${agentRunId}`);
      const conversationDetailId = agentRun?.ConversationDetailID;
      const message = progress?.message;
      const percentComplete = progress?.percentage;
      const stepCount = progress?.stepCount;
        console.warn('[ConversationStreamingService] ⚠️  No message content in agent progress update');
        console.warn('[ConversationStreamingService] ⚠️  Agent progress update missing conversationDetailId', { agentName: agentRun?.Agent, message });
      // Direct lookup using conversationDetailID from backend
        taskName: agentRun?.Agent || 'Agent',
        metadata: { agentRun, progress } as MessageProgressMetadata,
        stepCount,
        resolver: 'RunAIAgentResolver'
      console.error('[ConversationStreamingService] Error routing agent progress:', error);
   * Schedule a reconnection attempt after a delay
  private scheduleReconnection(): void {
    if (this.reconnectionTimeout) {
      clearTimeout(this.reconnectionTimeout);
    this.connectionStatus$.next('reconnecting');
    this.reconnectionTimeout = setTimeout(() => {
      console.log('[ConversationStreamingService] Attempting to reconnect...');
      this.initialized = false; // Reset initialization flag
      this.initialize();
   * Get a recent completion event for a message (used when component initializes after completion)
   * This handles the navigation scenario: user navigates away, agent completes, user returns.
   * @param conversationDetailId - The message ID to check
   * @returns Completion info if found within the last 5 minutes, undefined otherwise
  public getRecentCompletion(conversationDetailId: string): { agentRunId: string } | undefined {
    const completion = this.recentCompletions.get(conversationDetailId);
    return completion ? { agentRunId: completion.agentRunId } : undefined;
   * Clear a recent completion after it has been handled
   * @param conversationDetailId - The message ID to clear
  public clearRecentCompletion(conversationDetailId: string): void {
    this.recentCompletions.delete(conversationDetailId);
   * Cleanup completions older than 5 minutes to prevent memory leak
  private cleanupOldCompletions(): void {
    const fiveMinutesAgo = Date.now() - 5 * 60 * 1000;
    for (const [id, data] of this.recentCompletions) {
      if (data.timestamp.getTime() < fiveMinutesAgo) {
        this.recentCompletions.delete(id);
   * Cleanup when service is destroyed
    if (this.pushStatusSubscription) {
      this.pushStatusSubscription.unsubscribe();
      this.pushStatusSubscription = undefined;
    this.callbackRegistry.clear();
    this.recentCompletions.clear();
    this.completionEvents$.complete();
    this.connectionStatus$.complete();
