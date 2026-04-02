import { Component, Input, Output, EventEmitter, OnInit, ViewContainerRef } from '@angular/core';
import { MJProjectEntity, MJConversationEntity } from '@memberjunction/core-entities';
import { DialogService as KendoDialogService } from '@progress/kendo-angular-dialog';
import { ProjectFormModalComponent } from './project-form-modal.component';
export interface ProjectWithStats extends MJProjectEntity {
  conversationCount?: number;
  selector: 'mj-project-selector',
    <div class="project-selector">
        [data]="projectsWithStats"
        [value]="selectedProject"
        (valueChange)="onProjectChange($event)">
          <div class="project-item">
            <i class="fa-solid {{ dataItem.Icon || 'fa-folder' }}"
               [style.color]="dataItem.Color || '#0076B6'"></i>
            <div class="project-info">
              <span class="project-name">{{ dataItem.Name }}</span>
              @if (showStats && dataItem.conversationCount != null) {
                <span class="project-stats">{{ dataItem.conversationCount }} conversations</span>
            <span>{{ dataItem.Name }}</span>
      <div class="project-actions">
        @if (selectedProject) {
            [icon]="'edit'"
            (click)="onEditProject()"
            title="Edit Project"
            class="btn-icon">
            [icon]="'trash'"
            (click)="onDeleteProject()"
            title="Delete Project"
            class="btn-icon btn-danger">
          [icon]="'plus'"
          (click)="onCreateProject()"
          title="Create New Project"
    .project-selector {
    .project-selector kendo-dropdownlist {
    .project-item {
    .project-info {
    .project-name {
    .project-stats {
    .project-actions {
      background-color: #F44336;
      border-color: #F44336;
export class ProjectSelectorComponent implements OnInit {
  @Input() selectedProjectId: string | null = null;
  @Input() showStats: boolean = true;
  @Output() projectSelected = new EventEmitter<MJProjectEntity | null>();
  @Output() projectCreated = new EventEmitter<MJProjectEntity>();
  @Output() projectUpdated = new EventEmitter<MJProjectEntity>();
  @Output() projectDeleted = new EventEmitter<string>();
  public projectsWithStats: ProjectWithStats[] = [];
  public selectedProject: MJProjectEntity | null = null;
    private kendoDialogService: KendoDialogService,
    this.loadProjects();
  private async loadProjects(): Promise<void> {
      // Load projects and conversation counts in parallel
      const [projectsResult, conversationsResult] = await rv.RunViews([
          EntityName: 'MJ: Projects',
          ExtraFilter: `EnvironmentID='${this.environmentId}' AND IsArchived=0`,
      ], this.currentUser);
      if (projectsResult.Success && conversationsResult.Success) {
        const projects = projectsResult.Results as MJProjectEntity[] || [];
        const conversations = conversationsResult.Results as MJConversationEntity[] || [];
        // Calculate conversation counts per project
        const conversationCounts = this.calculateConversationCounts(conversations);
        // Merge projects with stats
        this.projectsWithStats = projects.map(p => {
          const projectWithStats = p as ProjectWithStats;
          projectWithStats.conversationCount = conversationCounts.get(p.ID) || 0;
          return projectWithStats;
        if (this.selectedProjectId) {
          this.selectedProject = this.projectsWithStats.find(p => p.ID === this.selectedProjectId) || null;
      console.error('Error loading projects:', error);
  private calculateConversationCounts(conversations: MJConversationEntity[]): Map<string, number> {
      if (conv.ProjectID) {
        counts.set(conv.ProjectID, (counts.get(conv.ProjectID) || 0) + 1);
  onProjectChange(project: MJProjectEntity | null): void {
    this.selectedProject = project;
    this.projectSelected.emit(project);
  onCreateProject(): void {
    const dialogRef = this.kendoDialogService.open({
      content: ProjectFormModalComponent,
      minWidth: 400
    const modalInstance = dialogRef.content.instance as ProjectFormModalComponent;
    modalInstance.dialogRef = dialogRef;
    modalInstance.environmentId = this.environmentId;
    modalInstance.currentUser = this.currentUser;
    modalInstance.projectSaved.subscribe(async (project: MJProjectEntity) => {
      this.projectCreated.emit(project);
      await this.loadProjects();
  onEditProject(): void {
    if (!this.selectedProject) return;
    modalInstance.project = this.selectedProject;
      this.projectUpdated.emit(project);
  async onDeleteProject(): Promise<void> {
    const projectName = this.selectedProject.Name;
    const projectId = this.selectedProject.ID;
    const conversationCount = (this.selectedProject as ProjectWithStats).conversationCount || 0;
    let message = `Are you sure you want to delete the project "${projectName}"?`;
    if (conversationCount > 0) {
      message += `\n\nThis project has ${conversationCount} conversation(s). The conversations will not be deleted, but will be unassigned from this project.`;
      title: 'Delete Project',
      const project = await md.GetEntityObject<MJProjectEntity>('MJ: Projects', this.currentUser);
      await project.Load(projectId);
      const deleted = await project.Delete();
        this.projectDeleted.emit(projectId);
        this.selectedProject = null;
        this.projectSelected.emit(null);
        throw new Error('Delete operation returned false');
      console.error('Error deleting project:', error);
      await this.dialogService.alert('Error', 'Failed to delete project. Please try again.');
