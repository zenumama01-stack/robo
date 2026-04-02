import { Component, Input, Output, EventEmitter, OnChanges, AfterViewInit, ElementRef, ViewChild, OnDestroy, HostListener } from '@angular/core';
import { MJTaskEntity, MJTaskDependencyEntity } from '@memberjunction/core-entities';
import { gantt } from 'dhtmlx-gantt';
import { TaskDetailPanelComponent } from './task-detail-panel.component';
 * Gantt chart view for tasks using DHTMLX Gantt
  selector: 'mj-gantt-task-viewer',
  imports: [TaskDetailPanelComponent],
    <div class="gantt-task-viewer">
      @if (!tasks || tasks.length === 0) {
          <i class="fas fa-chart-gantt"></i>
          <p>No tasks to display in Gantt view</p>
      @if (tasks && tasks.length > 0) {
        <div class="gantt-layout">
          <div #ganttContainer class="gantt-container"></div>
          @if (selectedTask) {
            <div class="gantt-resizer"
            <div class="task-detail-panel" [style.width.px]="detailPanelWidth">
              <mj-task-detail-panel
                [task]="selectedTask"
                [agentRunId]="getAgentRunId(selectedTask)"
                (closePanel)="closeDetailPanel()"
              </mj-task-detail-panel>
    .gantt-task-viewer {
    .gantt-layout {
    .gantt-container {
    .gantt-resizer {
    .gantt-resizer:hover {
    .task-detail-panel {
    /* Override DHTMLX Gantt default styles */
    :host ::ng-deep .gantt_container {
    :host ::ng-deep .gantt_grid_scale,
    :host ::ng-deep .gantt_task_scale {
    :host ::ng-deep .gantt_task .gantt_task_content {
export class GanttTaskViewerComponent implements OnChanges, AfterViewInit, OnDestroy {
  @Input() tasks: MJTaskEntity[] = [];
  @Input() taskDependencies: MJTaskDependencyEntity[] = [];
  @Input() agentRunMap?: Map<string, string>; // Maps TaskID -> AgentRunID
  @ViewChild('ganttContainer', { static: false }) ganttContainer!: ElementRef<HTMLDivElement>;
  public detailPanelWidth: number = 400;
  private ganttInitialized = false;
    console.log('🔧 ngAfterViewInit called', {
      taskCount: this.tasks?.length || 0,
      hasContainer: !!this.ganttContainer
    if (this.tasks && this.tasks.length > 0 && this.ganttContainer) {
      this.initGantt();
    console.log('🔄 ngOnChanges called', {
      initialized: this.ganttInitialized,
      hasContainer: !!this.ganttContainer,
      taskCount: this.tasks?.length || 0
    if (this.ganttInitialized && this.ganttContainer) {
      this.updateGanttData();
    } else if (!this.ganttInitialized && this.ganttContainer && this.tasks && this.tasks.length > 0) {
      // Initialize if we have container and tasks but haven't initialized yet
      console.log('🎨 Late initialization - gantt not initialized but container and tasks available');
    if (this.ganttInitialized) {
      gantt.clearAll();
      this.ganttInitialized = false;
  private initGantt(): void {
      console.log('🎨 Initializing DHTMLX Gantt');
      // IMPORTANT: Clear any previous configuration
      // Configure Gantt layout and appearance
      gantt.config.date_format = '%Y-%m-%d %H:%i';
      gantt.config.scale_unit = 'day';
      gantt.config.date_scale = '%d %M';
      gantt.config.subscales = [];
      gantt.config.show_progress = true;
      gantt.config.show_links = true;
      gantt.config.auto_types = true;
      gantt.config.readonly = true; // Read-only for now
      gantt.config.fit_tasks = true; // Auto-fit timeline to tasks
      // Disable auto-scheduling - we calculate dates ourselves based on dependencies
      gantt.config.auto_scheduling = false;
      gantt.config.grid_width = 350;
      gantt.config.row_height = 36;
      gantt.config.scale_height = 0; // Hide date scale - dates are just for positioning
      // Layout configuration - ensure timeline is visible
      gantt.config.layout = {
        css: "gantt_container",
        rows: [
            cols: [
              { view: "grid", group: "grids", scrollY: "scrollVer" },
              { resizer: true, width: 1 },
              { view: "timeline", scrollX: "scrollHor", scrollY: "scrollVer" },
              { view: "scrollbar", id: "scrollVer", group: "vertical" }
          { view: "scrollbar", id: "scrollHor", group: "horizontal" }
      // Column configuration - only show task names, hide dates
      gantt.config.columns = [
        { name: 'text', label: 'Task name', tree: true, width: '*' }
      // Initialize Gantt in the container
      gantt.init(this.ganttContainer.nativeElement);
      this.ganttInitialized = true;
      // Attach click event
      gantt.attachEvent('onTaskClick', (id: string) => {
        const originalTask = this.tasks.find(t => t.ID === id);
        if (originalTask) {
          this.selectedTask = originalTask;
          this.taskClicked.emit(originalTask);
      // Force resize after data load to ensure proper rendering
        gantt.setSizes();
      // Expand and select after render completes
        this.expandAllAndSelectRoot();
      console.log('✅ DHTMLX Gantt initialized successfully');
      console.error('❌ Error initializing Gantt:', error);
  private updateGanttData(): void {
    if (!this.ganttInitialized) return;
      console.log('📊 Updating Gantt data with', this.tasks.length, 'tasks');
      const ganttData = this.convertToGanttFormat(this.tasks);
      gantt.parse(ganttData);
      // Log final parsed data for debugging
      console.log('📋 Gantt data after parse:', {
        tasks: gantt.getTaskByTime(),
        links: gantt.getLinks()
      // Force resize after data update
      console.log('✅ Gantt data updated');
      console.error('❌ Error updating Gantt data:', error);
  private convertToGanttFormat(tasks: MJTaskEntity[]): { data: any[], links: any[] } {
    const data: any[] = [];
    console.log('🔍 Converting tasks:', tasks);
    console.log('🔗 Task dependencies:', this.taskDependencies);
    // Build a map of task ID to task for quick lookup
    const taskMap = new Map<string, MJTaskEntity>();
    tasks.forEach(t => taskMap.set(t.ID, t));
    // Build dependency map: taskId -> array of tasks it depends on
    const dependencyMap = new Map<string, string[]>();
    this.taskDependencies.forEach(dep => {
      if (!dependencyMap.has(dep.TaskID)) {
        dependencyMap.set(dep.TaskID, []);
      dependencyMap.get(dep.TaskID)!.push(dep.DependsOnTaskID);
    // Calculate start dates based on dependencies
    const taskStartDates = new Map<string, Date>();
    const baseDate = new Date();
    baseDate.setHours(0, 0, 0, 0);
    // Recursive function to calculate start date for a task
    const calculateStartDate = (taskId: string, visited = new Set<string>()): Date => {
      // Prevent circular dependencies
      if (visited.has(taskId)) {
        return new Date(baseDate);
      visited.add(taskId);
      // If already calculated, return it
      if (taskStartDates.has(taskId)) {
        return taskStartDates.get(taskId)!;
      const task = taskMap.get(taskId);
      if (!task) return new Date(baseDate);
      const dependencies = dependencyMap.get(taskId) || [];
      if (dependencies.length === 0) {
        // No dependencies - use base date or task's actual start date
        const startDate = task.StartedAt ? new Date(task.StartedAt) : new Date(baseDate);
        taskStartDates.set(taskId, startDate);
        return startDate;
      // Has dependencies - start after the latest dependency ends
      let latestEnd = new Date(baseDate);
      for (const depId of dependencies) {
        const depTask = taskMap.get(depId);
        if (depTask) {
          const depStart = calculateStartDate(depId, new Set(visited));
          const depDuration = this.calculateDuration(depTask);
          const depEnd = new Date(depStart);
          depEnd.setDate(depEnd.getDate() + depDuration);
          if (depEnd > latestEnd) {
            latestEnd = depEnd;
      taskStartDates.set(taskId, latestEnd);
      return latestEnd;
    // Calculate start dates for all tasks
    tasks.forEach(task => calculateStartDate(task.ID));
    // Calculate min/max dates for timeline display range
    let minDate: Date | null = null;
    let maxDate: Date | null = null;
      const startDate = taskStartDates.get(task.ID);
        const duration = this.calculateDuration(task);
        const endDate = new Date(startDate);
        endDate.setDate(endDate.getDate() + duration);
        if (!minDate || startDate < minDate) {
          minDate = startDate;
        if (!maxDate || endDate > maxDate) {
          maxDate = endDate;
    // Add 1 day padding before and after
    if (minDate) {
      const paddedStart = new Date(minDate);
      paddedStart.setDate(paddedStart.getDate() - 1);
      gantt.config.start_date = paddedStart;
    if (maxDate) {
      const paddedEnd = new Date(maxDate);
      paddedEnd.setDate(paddedEnd.getDate() + 1);
      gantt.config.end_date = paddedEnd;
    // Now create Gantt tasks with calculated dates
      console.log('📝 Processing task:', {
        ID: task.ID,
        Name: task.Name,
        ParentID: task.ParentID
      // Calculate progress (0-1 scale for DHTMLX)
      let progress = (task.PercentComplete || 0) / 100;
      if (task.Status === 'Complete') {
        progress = 1;
      const startDate = taskStartDates.get(task.ID) || new Date(baseDate);
      const ganttTask: any = {
        text: task.Name || 'Untitled Task',
        start_date: this.formatDateForDHTMLX(startDate),
        progress: progress
      // Add parent relationship for tree structure (not dependency)
      if (task.ParentID) {
        ganttTask.parent = task.ParentID;
        // Root tasks need parent: 0
        ganttTask.parent = 0;
      console.log('✅ Created Gantt task:', ganttTask);
      data.push(ganttTask);
    // Create links from MJTaskDependencyEntity records
    this.taskDependencies.forEach((dep, index) => {
        id: dep.ID || `link_${index}`,
        source: dep.DependsOnTaskID, // The task being depended on
        target: dep.TaskID,           // The task that depends on it
        type: '0' // finish-to-start (DHTMLX type 0)
      console.log(`🔗 Created link: ${dep.DependsOnTaskID} -> ${dep.TaskID}`);
    console.log('📊 Final DHTMLX format:', { data, links });
    return { data, links };
  private calculateDuration(task: MJTaskEntity): number {
    if (task.StartedAt && task.DueAt) {
      const startDate = new Date(task.StartedAt);
      const endDate = new Date(task.DueAt);
      return Math.max(1, Math.ceil((endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24)));
    return 1; // Default to 1 day
  private formatDateForDHTMLX(date: Date): string {
    return `${year}-${month}-${day} 00:00`;
  public getAgentRunId(task: MJTaskEntity): string | null {
    return this.agentRunMap?.get(task.ID) || null;
    this.resizeStartWidth = this.detailPanelWidth;
  handleResize(event: MouseEvent): void {
    const newWidth = this.resizeStartWidth + delta;
    // Constrain width between min and max
    this.detailPanelWidth = Math.max(300, Math.min(600, newWidth));
  stopResize(): void {
      // Resize gantt chart after panel resize completes
  private expandAllAndSelectRoot(): void {
      // Expand all tasks
      gantt.eachTask((task: any) => {
        gantt.open(task.id);
      // Find and select the root task (task with parent = 0)
      let rootTask: any = null;
        if (task.parent === 0 || task.parent === '0') {
          rootTask = task;
          return false; // Stop iteration
      if (rootTask) {
        gantt.selectTask(rootTask.id);
        // Trigger task click event to open detail panel
        const originalTask = this.tasks.find(t => t.ID === rootTask.id);
      console.log('✅ Expanded all tasks and selected root:', rootTask?.id);
      console.error('❌ Error expanding/selecting tasks:', error);
