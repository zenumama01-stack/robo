import { MJTaskEntity, MJTaskDependencyEntity, MJAIAgentRunEntity } from '@memberjunction/core-entities';
import { TaskComponent } from '@memberjunction/ng-tasks';
 * Full-page tasks view with task list and Gantt chart
 * Generic component that displays tasks based on provided filter
 * Supports drilling into individual tasks to see sub-tasks
  selector: 'mj-tasks-full-view',
  imports: [TaskComponent],
    <div class="tasks-full-view">
      @if (!selectedTask) {
        <!-- Task List View -->
        <mj-task
          [tasks]="filteredTasks"
          [title]="'Tasks'"
          [description]="getDescription()"
          [showHeader]="true"
          [showViewToggle]="false"
          [viewMode]="'simple'"
          (taskClicked)="onTaskClick($event)">
        </mj-task>
        <!-- Task Detail View with Sub-tasks -->
        <div class="task-detail-view" [class.swoosh-in]="showDetailAnimation">
            <button class="breadcrumb-back" (click)="backToTaskList()">
              <span>Back to Tasks</span>
            <div class="breadcrumb-divider">/</div>
            <span class="breadcrumb-current">{{ selectedTask.Name }}</span>
          <!-- Task Details & Sub-tasks with Gantt Toggle -->
            [tasks]="subTasks"
            [ganttTasks]="subTasksWithParent"
            [taskDependencies]="taskDependencies"
            [agentRunMap]="agentRunMap"
            [title]="selectedTask.Name"
            [description]="getTaskDetailDescription()"
            [showViewToggle]="true"
            [viewMode]="'gantt'"
            (taskClicked)="onSubTaskClick($event)"
    .tasks-full-view {
    .task-detail-view {
    .swoosh-in {
      animation: swooshIn 0.3s ease-out;
    @keyframes swooshIn {
        transform: translateX(50px);
    .breadcrumb-back {
    .breadcrumb-back:hover {
    .breadcrumb-back i {
    .breadcrumb-divider {
export class TasksFullViewComponent implements OnInit, OnChanges {
  @Input() baseFilter: string = '1=1'; // SQL filter for tasks (default: show all)
  @Input() activeTaskId?: string; // Task ID to auto-select and drill into
  @Output() openEntityRecord = new EventEmitter<{ entityName: string; recordId: string }>();
  @Output() taskSelected = new EventEmitter<string | null>(); // Emits task ID when drill-down occurs, null when returning to list
  public allTasks: MJTaskEntity[] = [];
  public filteredTasks: MJTaskEntity[] = [];
  public subTasks: MJTaskEntity[] = [];
  public subTasksWithParent: MJTaskEntity[] = []; // Includes parent for Gantt hierarchy
  public taskDependencies: MJTaskDependencyEntity[] = []; // Dependencies for Gantt links
  public agentRunMap = new Map<string, string>(); // Maps TaskID -> AgentRunID
  public selectedTask: MJTaskEntity | null = null;
  public showDetailAnimation: boolean = false;
  private aiEngineConfigured: boolean = false;
    this.loadTasks();
    // Reload tasks if baseFilter changes
    if (changes['baseFilter'] && !changes['baseFilter'].firstChange) {
    // Auto-drill into task if activeTaskId changes
    if (changes['activeTaskId'] && this.activeTaskId) {
      const task = this.allTasks.find(t => t.ID === this.activeTaskId);
        this.onTaskClick(task);
  public async loadTasks(): Promise<void> {
      // Configure AIEngineBase on first load (false = don't force refresh)
      if (!this.aiEngineConfigured) {
        this.aiEngineConfigured = true;
      console.log('📝 Tasks filter SQL:', this.baseFilter);
      // Load all tasks with the provided filter
      const tasksResult = await rv.RunView<MJTaskEntity>(
          ExtraFilter: this.baseFilter,
      console.log('📊 Tasks query result:', {
        success: tasksResult.Success,
        resultCount: tasksResult.Results?.length || 0,
        errorMessage: tasksResult.ErrorMessage
      if (tasksResult.Success) {
        this.allTasks = tasksResult.Results || [];
        this.filteredTasks = this.allTasks;
        console.log(`📋 Loaded ${this.allTasks.length} tasks`);
        if (this.allTasks.length === 0) {
          console.log('💡 No tasks found with current filter');
          console.log('✅ Sample task:', {
            id: this.allTasks[0].ID,
            name: this.allTasks[0].Name,
            status: this.allTasks[0].Status,
            conversationDetailID: this.allTasks[0].ConversationDetailID
        console.error('❌ Failed to load tasks:', tasksResult.ErrorMessage);
        this.allTasks = [];
        this.filteredTasks = [];
      console.error('Failed to load tasks:', error);
  public async onTaskClick(task: MJTaskEntity): Promise<void> {
    console.log('Task clicked:', task);
    this.selectedTask = task;
    this.showDetailAnimation = true;
    // Emit task selection event for URL tracking
    this.taskSelected.emit(task.ID);
    // Load all tasks in the hierarchy using RootParentID
    await this.loadTaskHierarchy(task);
  private async loadTaskHierarchy(task: MJTaskEntity): Promise<void> {
      // Use RootParentID to load all tasks in this hierarchy
      // If task has no RootParentID, it's the root itself, so use its ID
      const rootId = task.RootParentID || task.ID;
      // Load all tasks where RootParentID matches, or tasks that are the root itself
      const hierarchyResult = await rv.RunView<MJTaskEntity>(
          ExtraFilter: `RootParentID='${rootId}' OR ID='${rootId}'`,
      if (hierarchyResult.Success) {
        const allHierarchy = hierarchyResult.Results || [];
        // For list view: Filter out the clicked task itself - only show its children/descendants
        this.subTasks = allHierarchy.filter(t => t.ID !== task.ID);
        // For Gantt view: Include the parent task so hierarchy works correctly
        this.subTasksWithParent = allHierarchy;
        console.log(`📋 Loaded ${this.subTasks.length} tasks in hierarchy for root ${rootId}`);
        // Load task dependencies for this hierarchy
        await this.loadTaskDependencies(rootId);
        // Load agent runs for this hierarchy
        await this.loadAgentRuns(allHierarchy);
        console.error('❌ Failed to load task hierarchy:', hierarchyResult.ErrorMessage);
        this.subTasks = [];
        this.subTasksWithParent = [];
        this.taskDependencies = [];
      console.error('Failed to load task hierarchy:', error);
  private async loadTaskDependencies(rootId: string): Promise<void> {
      // Load task dependencies where either TaskID or DependsOnTaskID is in this hierarchy
      // Use subquery to find all tasks with this RootParentID
      // Note: Using __mj as the default schema - this is the standard MJ schema
      const schema = '__mj';
      const depsResult = await rv.RunView<MJTaskDependencyEntity>(
          EntityName: 'MJ: Task Dependencies',
            TaskID IN (SELECT ID FROM [${schema}].[vwTasks] WHERE RootParentID='${rootId}' OR ID='${rootId}')
            OR
            DependsOnTaskID IN (SELECT ID FROM [${schema}].[vwTasks] WHERE RootParentID='${rootId}' OR ID='${rootId}')
      if (depsResult.Success) {
        this.taskDependencies = depsResult.Results || [];
        console.log(`🔗 Loaded ${this.taskDependencies.length} task dependencies`);
        console.error('❌ Failed to load task dependencies:', depsResult.ErrorMessage);
      console.error('Failed to load task dependencies:', error);
  private async loadAgentRuns(tasks: MJTaskEntity[]): Promise<void> {
      // Clear existing map
      this.agentRunMap.clear();
      // Get all unique ConversationDetailIDs from tasks (filter out nulls)
      const conversationDetailIds = tasks
        .filter(t => t.ConversationDetailID != null)
        .map(t => t.ConversationDetailID!);
        console.log('💡 No tasks with ConversationDetailID');
      // Build filter to find agent runs for these conversation details
      // Use a subquery to avoid passing large ID lists
      const taskIds = tasks.map(t => `'${t.ID}'`).join(',');
      const agentRunsResult = await rv.RunView<MJAIAgentRunEntity>(
            ConversationDetailID IN (
              SELECT DISTINCT ConversationDetailID
              FROM [${schema}].[vwTasks]
              WHERE ID IN (${taskIds})
              AND ConversationDetailID IS NOT NULL
      if (agentRunsResult.Success) {
        const agentRuns = agentRunsResult.Results || [];
        console.log(`🤖 Loaded ${agentRuns.length} agent runs`, agentRuns);
        // Build map: ConversationDetailID -> AgentRunID
        const convoToRunMap = new Map<string, string>();
        agentRuns.forEach(run => {
          if (run.ConversationDetailID) {
            convoToRunMap.set(run.ConversationDetailID, run.ID);
            console.log(`📝 Mapping ConvoDetailID ${run.ConversationDetailID} -> RunID ${run.ID}`);
        // Map TaskID -> AgentRunID using ConversationDetailID as the link
        tasks.forEach(task => {
          console.log(`🔍 Task ${task.Name} - ConvoDetailID: ${task.ConversationDetailID}, AgentID: ${task.AgentID}`);
          if (task.ConversationDetailID) {
            const agentRunId = convoToRunMap.get(task.ConversationDetailID);
              this.agentRunMap.set(task.ID, agentRunId);
              console.log(`✅ Mapped Task ${task.ID} -> AgentRun ${agentRunId}`);
              console.log(`⚠️ No agent run found for ConvoDetailID ${task.ConversationDetailID}`);
        console.log(`🔗 Mapped ${this.agentRunMap.size} tasks to agent runs`, Array.from(this.agentRunMap.entries()));
        console.error('❌ Failed to load agent runs:', agentRunsResult.ErrorMessage);
      console.error('Failed to load agent runs:', error);
  public backToTaskList(): void {
    this.selectedTask = null;
    this.showDetailAnimation = false;
    // Emit null to indicate returning to task list (for URL tracking)
    this.taskSelected.emit(null);
  public onSubTaskClick(subTask: MJTaskEntity): void {
    console.log('Sub-task clicked:', subTask);
    // Could drill down further if needed
  public onOpenEntityRecord(event: { entityName: string; recordId: string }): void {
    // Bubble up the event to parent component
  public getDescription(): string {
    const activeCount = this.allTasks.filter(t => t.Status === 'Pending' || t.Status === 'In Progress').length;
    const completedCount = this.allTasks.filter(t => t.Status === 'Complete').length;
    return `${activeCount} active, ${completedCount} completed, ${this.allTasks.length} total`;
  public getTaskDetailDescription(): string {
    if (this.subTasks.length === 0) {
      return 'No sub-tasks';
    const activeCount = this.subTasks.filter(t => t.Status === 'Pending' || t.Status === 'In Progress').length;
    const completedCount = this.subTasks.filter(t => t.Status === 'Complete').length;
    return `${activeCount} active, ${completedCount} completed, ${this.subTasks.length} sub-tasks`;
