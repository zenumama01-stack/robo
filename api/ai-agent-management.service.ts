import { Injectable, ViewContainerRef } from '@angular/core';
import { DialogService, DialogRef, WindowService, WindowRef, WindowSettings } from '@progress/kendo-angular-dialog';
import { MJActionEntity, MJAIAgentPromptEntity,  } from '@memberjunction/core-entities';
import { AddActionDialogComponent } from './add-action-dialog.component';
import { PromptSelectorDialogComponent, PromptSelectorConfig, PromptSelectorResult } from './prompt-selector-dialog.component';
import { AgentPromptAdvancedSettingsDialogComponent, AgentPromptAdvancedSettingsFormData } from './agent-prompt-advanced-settings-dialog.component';
import { SubAgentAdvancedSettingsDialogComponent, SubAgentAdvancedSettingsFormData } from './sub-agent-advanced-settings-dialog.component';
import { SubAgentSelectorDialogComponent, SubAgentSelectorConfig, SubAgentSelectorResult } from './sub-agent-selector-dialog.component';
import { CreatePromptDialogComponent, CreatePromptConfig, CreatePromptResult } from './create-prompt-dialog.component';
import { CreateSubAgentDialogComponent, CreateSubAgentConfig, CreateSubAgentResult } from './create-sub-agent-dialog.component';
import { AIAgentEntityExtended, AIPromptEntityExtended } from '@memberjunction/ai-core-plus';
 * Consolidated service for managing AI Agent operations including:
 * - Adding/removing actions
 * - Advanced settings configuration  
 * - Agent creation (future)
 * - Agent editing operations
 * This service centralizes all agent-related dialog and management functionality
 * to provide a consistent interface for both editing existing agents and creating new ones.
export class AIAgentManagementService {
    private dialogService: DialogService,
    private windowService: WindowService
  // === Action Management ===
   * Opens the Add Action dialog for selecting actions to link to an agent
   * @param config Configuration for the action selection dialog
   * @returns Observable that emits the selected actions when dialog is closed
  openAddActionDialog(config: {
    existingActionIds: string[];
    viewContainerRef?: ViewContainerRef;
  }): Observable<MJActionEntity[]> {
    const windowSettings: WindowSettings = {
      title: `Add Actions to ${config.agentName}`,
      content: AddActionDialogComponent,
      width: 1000,
      height: 700,
      minWidth: 800,
      minHeight: 600,
      draggable: true,
      resizable: true,
      state: 'default'
    if (config.viewContainerRef) {
      windowSettings.appendTo = config.viewContainerRef;
    const windowRef: WindowRef = this.windowService.open(windowSettings);
    // Pass configuration to the dialog component
    const componentInstance = windowRef.content.instance as AddActionDialogComponent;
    componentInstance.agentId = config.agentId;
    componentInstance.agentName = config.agentName;
    componentInstance.existingActionIds = [...config.existingActionIds];
    return componentInstance.result.asObservable();
  // === Prompt Management ===
   * Opens the prompt selector dialog for selecting general prompts to add to an agent
   * @param config Configuration for the prompt selection dialog
   * @returns Observable that emits the selected prompts when dialog is closed
  openPromptSelectorDialog(config: {
    multiSelect?: boolean;
    selectedPromptIds?: string[];
    showCreateNew?: boolean;
    linkedPromptIds?: string[];
  }): Observable<PromptSelectorResult | null> {
    const selectorConfig: PromptSelectorConfig = {
      title: config.title || 'Select Prompts',
      multiSelect: config.multiSelect ?? true,
      selectedPromptIds: config.selectedPromptIds || [],
      showCreateNew: config.showCreateNew ?? true,
      extraFilter: config.extraFilter,
      linkedPromptIds: config.linkedPromptIds || []
      title: selectorConfig.title,
      width: 900,
      height: 600,
      minWidth: 600,
      minHeight: 400,
    const componentInstance = windowRef.content.instance as PromptSelectorDialogComponent;
    componentInstance.config = selectorConfig;
   * Opens the prompt selector for selecting a context compression prompt (single select)
   * @param config Configuration for the context compression prompt selection
   * @returns Observable that emits the selected prompt when dialog is closed
  openContextCompressionPromptSelector(config: {
    currentPromptId?: string;
  }): Observable<AIPromptEntityExtended | null> {
      this.openPromptSelectorDialog({
        selectedPromptIds: config.currentPromptId ? [config.currentPromptId] : [],
        showCreateNew: false,
        extraFilter: undefined, // Show all active prompts
        viewContainerRef: config.viewContainerRef
      }).subscribe(result => {
          observer.next(result.selectedPrompts[0]);
          observer.next(null);
  // === Sub-Agent Management ===
   * Opens the sub-agent selector dialog for selecting agents to convert to sub-agents
   * @param config Configuration for the sub-agent selection dialog
   * @returns Observable that emits the selected agents when dialog is closed
  openSubAgentSelectorDialog(config: {
    selectedAgentIds?: string[];
    parentAgentId: string;
  }): Observable<SubAgentSelectorResult | null> {
    const selectorConfig: SubAgentSelectorConfig = {
      title: config.title || 'Add Sub-Agents',
      selectedAgentIds: config.selectedAgentIds || [],
      parentAgentId: config.parentAgentId
      content: SubAgentSelectorDialogComponent,
    const componentInstance = windowRef.content.instance as SubAgentSelectorDialogComponent;
  // === Advanced Settings for Related Entities ===
   * Opens the advanced settings dialog for an AI Agent Prompt
   * @param config Configuration for the agent prompt advanced settings dialog
   * @returns Observable that emits the form data when dialog is closed, or null if cancelled
  openAgentPromptAdvancedSettingsDialog(config: {
    agentPrompt: MJAIAgentPromptEntity;
    allAgentPrompts: MJAIAgentPromptEntity[];
  }): Observable<AgentPromptAdvancedSettingsFormData | null> {
      title: `Advanced Settings - Prompt Configuration`,
      content: AgentPromptAdvancedSettingsDialogComponent,
      width: 700,
      minWidth: 500,
    const componentInstance = windowRef.content.instance as AgentPromptAdvancedSettingsDialogComponent;
    componentInstance.agentPrompt = config.agentPrompt;
    componentInstance.allAgentPrompts = config.allAgentPrompts;
   * Opens the advanced settings dialog for a Sub-Agent
   * @param config Configuration for the sub-agent advanced settings dialog
  openSubAgentAdvancedSettingsDialog(config: {
    subAgent: AIAgentEntityExtended;
    allSubAgents: AIAgentEntityExtended[];
  }): Observable<SubAgentAdvancedSettingsFormData | null> {
      title: `Advanced Settings - ${config.subAgent.Name || 'Sub-Agent'}`,
      content: SubAgentAdvancedSettingsDialogComponent,
    const componentInstance = windowRef.content.instance as SubAgentAdvancedSettingsDialogComponent;
    componentInstance.subAgent = config.subAgent;
    componentInstance.allSubAgents = config.allSubAgents;
  // === Prompt Creation ===
   * Opens the create prompt dialog for creating new prompts from within the AI Agent form
   * Returns the created entities (not saved to database) for parent to add to PendingRecords
   * @param config Configuration for prompt creation
   * @returns Observable that emits the created prompt and related entities when dialog is closed
  openCreatePromptDialog(config: {
    initialName?: string;
    initialTypeID?: string;
  }): Observable<CreatePromptResult | null> {
    const createConfig: CreatePromptConfig = {
      title: config.title || 'Create New Prompt',
      initialName: config.initialName,
      initialTypeID: config.initialTypeID
      title: createConfig.title,
      content: CreatePromptDialogComponent,
      minWidth: 700,
      minHeight: 500,
    const componentInstance = windowRef.content.instance as CreatePromptDialogComponent;
    componentInstance.config = createConfig;
  // === Sub-Agent Creation ===
   * Opens the create sub-agent dialog for creating new sub-agents from within the AI Agent form
   * @param config Configuration for sub-agent creation
   * @returns Observable that emits the created sub-agent and related entities when dialog is closed
  openCreateSubAgentDialog(config: {
    parentAgentName?: string;
  }): Observable<CreateSubAgentResult | null> {
    const createConfig: CreateSubAgentConfig = {
      title: config.title || 'Create New Sub-Agent',
      initialTypeID: config.initialTypeID,
      parentAgentId: config.parentAgentId,
      parentAgentName: config.parentAgentName
      content: CreateSubAgentDialogComponent,
      height: 800,
    const componentInstance = windowRef.content.instance as CreateSubAgentDialogComponent;
  // === Future: Agent Creation ===
   * Opens the agent creation dialog (to be implemented)
   * This will reuse existing components and provide a streamlined agent creation experience
   * @param config Configuration for agent creation
   * @returns Observable that emits the created agent when dialog is closed
  openCreateAgentDialog(config: {
    parentAgentId?: string;
    initialData?: Partial<AIAgentEntityExtended>;
  }): Observable<AIAgentEntityExtended | null> {
    // TODO: Implement agent creation dialog
    // This will reuse the same form components and advanced settings
    // but in a creation context rather than editing context
    throw new Error('Agent creation dialog not yet implemented');
  // === Utility Methods ===
   * Validates agent configuration and relationships
   * Used by both creation and editing workflows
  validateAgentConfiguration(agent: AIAgentEntityExtended): { isValid: boolean; errors: string[] } {
    // ParentID vs ExposeAsAction validation
    if (agent.ParentID && agent.ExposeAsAction) {
      errors.push('Agents with a parent cannot be exposed as actions');
    // Context compression validation
    if (agent.EnableContextCompression) {
      if (!agent.ContextCompressionMessageThreshold) {
        errors.push('Context compression requires a message threshold');
      if (!agent.ContextCompressionPromptID) {
        errors.push('Context compression requires a compression prompt');
      if (!agent.ContextCompressionMessageRetentionCount) {
        errors.push('Context compression requires a message retention count');
      isValid: errors.length === 0,
      errors
   * Future method for managing sub-agent relationships
   * This will handle ParentID, ExecutionOrder, and ExecutionMode
   * in the context of the parent agent's sub-agents section
  openSubAgentManagementDialog(config: {
    parentAgent: AIAgentEntityExtended;
    subAgent?: AIAgentEntityExtended; // For editing existing sub-agent relationship
  }): Observable<any> {
    // TODO: Implement sub-agent management dialog
    // This will handle the hierarchy settings that were removed from Advanced Settings
    throw new Error('Sub-agent management dialog not yet implemented');
