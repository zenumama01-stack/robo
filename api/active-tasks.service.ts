import { map } from 'rxjs/operators';
import { RunView, UserInfo } from '@memberjunction/core';
import { ConversationDataService } from './conversation-data.service';
 * Represents an active agent task that is currently running
export interface ActiveTask {
  agentId?: string; // The agent ID for looking up icon/metadata
  agentRunId?: string; // The AIAgentRun ID for finding task on completion
  relatedMessageId: string;
  conversationDetailId?: string;  // The ConversationDetail that tracks this task
  conversationId?: string; // The conversation this task belongs to
  conversationName?: string | null; // Display name of the conversation
 * Service for tracking active agent tasks across the application.
 * Maintains a live list of running agents and their status.
export class ActiveTasksService {
  private _tasks$ = new BehaviorSubject<Map<string, ActiveTask>>(new Map());
  private _conversationIdsWithTasks$ = new BehaviorSubject<Set<string>>(new Set());
  constructor(private conversationData: ConversationDataService) {}
   * Observable of all active tasks as an array
  public readonly tasks$: Observable<ActiveTask[]> = this._tasks$.pipe(
    map(taskMap => Array.from(taskMap.values()))
   * Observable of the count of active tasks
  public readonly taskCount$: Observable<number> = this.tasks$.pipe(
    map(tasks => tasks.length)
   * Observable of conversation IDs that have 1+ active tasks
   * Use this for quick lookups in conversation lists
  public readonly conversationIdsWithTasks$: Observable<Set<string>> = this._conversationIdsWithTasks$.asObservable();
   * Observable of tasks grouped by conversation ID
   * Returns Map<conversationId, ActiveTask[]>
  public readonly tasksByConversationId$: Observable<Map<string, ActiveTask[]>> = this.tasks$.pipe(
    map(tasks => {
      const grouped = new Map<string, ActiveTask[]>();
        if (task.conversationId) {
          const existing = grouped.get(task.conversationId) || [];
          existing.push(task);
          grouped.set(task.conversationId, existing);
   * Add a new active task
   * @param task Task details (without id and startTime)
   * @returns The generated task ID
  add(task: Omit<ActiveTask, 'id' | 'startTime'>): string {
    const id = `task-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`;
    const fullTask = {
      ...task,
      startTime: Date.now()
    const current = this._tasks$.value;
    current.set(id, fullTask);
    this._tasks$.next(new Map(current));
    // Update conversation IDs set
    if (fullTask.conversationId) {
      this.updateConversationIdsSet();
    console.log(`➕ Task added:`, {id, conversationId: fullTask.conversationId, agentName: fullTask.agentName});
    console.log(`📊 Total tasks:`, this._tasks$.value.size);
    console.log(`🗂️ Conversation IDs with tasks:`, Array.from(this._conversationIdsWithTasks$.value));
   * Remove an active task
   * @param id The task ID to remove
  remove(id: string): void {
    const task = current.get(id);
    current.delete(id);
    // Update conversation IDs set if this was the last task for a conversation
    if (task?.conversationId) {
    console.log(`➖ Task removed:`, {id, conversationId: task?.conversationId, agentName: task?.agentName});
    console.log(`📊 Total tasks remaining:`, this._tasks$.value.size);
   * Update the set of conversation IDs with active tasks
  private updateConversationIdsSet(): void {
    const conversationIds = new Set<string>();
    for (const task of this._tasks$.value.values()) {
        conversationIds.add(task.conversationId);
    this._conversationIdsWithTasks$.next(conversationIds);
   * Update the status of an active task
   * @param id The task ID
   * @param status The new status text
  updateStatus(id: string, status: string): void {
      task.status = status;
   * Get an active task by its conversation detail ID
   * @param conversationDetailId The conversation detail ID
   * @returns The task if found, undefined otherwise
  getByConversationDetailId(conversationDetailId: string): ActiveTask | undefined {
    const tasks = Array.from(this._tasks$.value.values());
    return tasks.find(task => task.conversationDetailId === conversationDetailId);
   * Update the status of a task by its conversation detail ID
   * @returns true if task was found and updated, false otherwise
  updateStatusByConversationDetailId(conversationDetailId: string, status: string): boolean {
    const task = this.getByConversationDetailId(conversationDetailId);
      this.updateStatus(task.id, status);
   * Get an active task by its agent run ID
   * @param agentRunId The AIAgentRun ID
  getByAgentRunId(agentRunId: string): ActiveTask | undefined {
    return tasks.find(task => task.agentRunId === agentRunId);
   * Remove a task by its agent run ID
   * @returns true if task was found and removed, false otherwise
  removeByAgentRunId(agentRunId: string): boolean {
    const task = this.getByAgentRunId(agentRunId);
      this.remove(task.id);
   * Clear all active tasks
    this._tasks$.next(new Map());
   * Restore active tasks from database by querying running agent runs.
   * Call this on app initialization to restore state after browser refresh.
   * @param currentUser The current user to filter agent runs by
  async restoreFromDatabase(currentUser: UserInfo): Promise<void> {
      // Query for running agent runs owned by this user
      // Only restore parent agents (those with ConversationDetailID) - child agents don't have one
      // This matches normal behavior where we only track parent agents, not child agents
        Fields: ["ID", "ConversationID", "AgentID", "Agent", "ConversationDetailID"], // narrow field scope to not pull back JSON blobs - much faster
        ExtraFilter: `Status='Running' AND UserID='${currentUser.ID}' AND ConversationDetailID IS NOT NULL`,
        ResultType: 'simple' // no need for entity-object here we aren't mutating
        // No running tasks or query failed - nothing to restore
      // Get conversation names from cached ConversationDataService
      const conversationNames = new Map<string, string>();
      for (const agentRun of result.Results) {
        if (agentRun.ConversationID) {
          const conv = this.conversationData.getConversationById(agentRun.ConversationID);
          if (conv?.Name) {
            conversationNames.set(agentRun.ConversationID, conv.Name);
      // Add each running agent to ActiveTasksService
      let restoredCount = 0;
        // Skip if already tracked (prevents duplicates)
        if (agentRun.ConversationDetailID &&
            this.getByConversationDetailId(agentRun.ConversationDetailID)) {
        this.add({
          agentName: agentRun.Agent || 'Unknown Agent',
          agentRunId: agentRun.ID, // For finding task on completion
          status: 'Reconnecting...',
          relatedMessageId: agentRun.ConversationDetailID || agentRun.ID,
          conversationDetailId: agentRun.ConversationDetailID || undefined,
          conversationId: agentRun.ConversationID || undefined,
          conversationName: agentRun.ConversationID
            ? conversationNames.get(agentRun.ConversationID) || null
        restoredCount++;
      if (restoredCount > 0) {
        console.log(`✅ Restored ${restoredCount} active task(s) from database`);
      console.error('Failed to restore active tasks from database:', error);
