import { Component, OnInit, OnDestroy, ChangeDetectorRef, ViewContainerRef } from '@angular/core';
import { MJAIAgentTypeEntity, MJAIAgentPromptEntity, MJAIAgentActionEntity, MJActionEntity } from '@memberjunction/core-entities';
export interface CreateSubAgentConfig {
  /** Initial name for the sub-agent */
  /** Pre-selected agent type ID */
  /** Parent agent ID for relationship */
  /** Parent agent name for display */
export interface CreateSubAgentResult {
  /** Created sub-agent entity (not saved to database) */
  /** Agent prompt link entities (not saved to database) */
  agentPrompts?: MJAIAgentPromptEntity[];
  /** Agent action link entities (not saved to database) */
  agentActions?: MJAIAgentActionEntity[];
  /** Any new prompts created within the dialog */
  newPrompts?: AIPromptEntityExtended[];
  /** Any new prompt templates created within the dialog */
  newPromptTemplates?: any[];
  /** Any new template contents created within the dialog */
  newTemplateContents?: any[];
 * Dialog for creating new AI Sub-Agents with essential fields, actions, and prompts management.
  selector: 'mj-create-sub-agent-dialog',
  templateUrl: './create-sub-agent-dialog.component.html',
  styleUrls: ['./create-sub-agent-dialog.component.css']
export class CreateSubAgentDialogComponent implements OnInit, OnDestroy {
  config: CreateSubAgentConfig = {} as CreateSubAgentConfig;
  public result = new Subject<CreateSubAgentResult | null>();
  subAgentForm: FormGroup;
  availableAgentTypes$ = new BehaviorSubject<MJAIAgentTypeEntity[]>([]);
  availablePrompts$ = new BehaviorSubject<AIPromptEntityExtended[]>([]);
  availableActions$ = new BehaviorSubject<MJActionEntity[]>([]);
  subAgentEntity: AIAgentEntityExtended | null = null;
  linkedPrompts: AIPromptEntityExtended[] = [];
  linkedActions: MJActionEntity[] = [];
  // Link entities for database relationships
  agentPromptLinks: MJAIAgentPromptEntity[] = [];
  agentActionLinks: MJAIAgentActionEntity[] = [];
  // Storage for new entities created within dialog
  newlyCreatedPrompts: AIPromptEntityExtended[] = [];
  newlyCreatedPromptTemplates: any[] = [];
  newlyCreatedTemplateContents: any[] = [];
    private agentManagementService: AIAgentManagementService,
    private viewContainerRef: ViewContainerRef
    this.subAgentForm = this.createForm();
      executionMode: new FormControl('Sequential'),
      purpose: new FormControl(''),
      userMessage: new FormControl(''),
      // systemMessage: new FormControl(''), // SystemMessage does not exist on MJAIAgentEntity
      modelSelectionMode: new FormControl('Agent Type'),
      temperature: new FormControl(0.1),
      topP: new FormControl(0.1),
      topK: new FormControl(40),
      maxTokens: new FormControl(4000),
      enableCaching: new FormControl(false),
      cacheTTL: new FormControl(3600)
    // Watch for form changes to update entity
    this.subAgentForm.valueChanges
      .subscribe(formValue => {
        this.updateSubAgentEntity(formValue);
      // Load all data in a single batch for better performance
        // Agent types (index 0)
        // Available prompts (index 1)
        // Available actions (index 2)
      // Process agent types (index 0)
      if (results[0].Success && results[0].Results) {
        this.availableAgentTypes$.next(results[0].Results as MJAIAgentTypeEntity[]);
        if (!this.config.initialTypeID && results[0].Results.length > 0) {
          this.subAgentForm.patchValue({ typeID: results[0].Results[0].ID });
      // Process available prompts (index 1)
      if (results[1].Success && results[1].Results) {
        this.availablePrompts$.next(results[1].Results as AIPromptEntityExtended[]);
      // Process available actions (index 2)
      const actionsResult = results[2];
      if (actionsResult.Success && actionsResult.Results) {
        this.availableActions$.next(actionsResult.Results);
      // Create the sub-agent entity
      this.subAgentEntity = await md.GetEntityObject<AIAgentEntityExtended>('MJ: AI Agents');
      this.subAgentEntity.NewRecord();
      this.subAgentEntity.Status = 'Pending';
      this.subAgentEntity.ExecutionMode = 'Sequential';
      this.subAgentEntity.ExposeAsAction = false; // Database constraint for sub-agents
      this.subAgentEntity.ParentID = this.config.parentAgentId;
      this.subAgentEntity.ModelSelectionMode = 'Agent Type';
      this.subAgentEntity.Set('Temperature', 0.1);
      this.subAgentEntity.Set('TopP', 0.1);
      this.subAgentEntity.Set('TopK', 40);
      this.subAgentEntity.Set('MaxTokensPerRun', 4000);
      this.subAgentEntity.Set('EnableCaching', false);
      this.subAgentEntity.Set('CacheTTLSeconds', 3600);
      // Update form with initial values
      this.updateSubAgentEntity(this.subAgentForm.value);
      console.error('Error loading sub-agent creation data:', error);
        'Error loading data for sub-agent creation',
  private updateSubAgentEntity(formValue: any) {
    if (!this.subAgentEntity) return;
    // Update entity with form values
    this.subAgentEntity.Name = formValue.name;
    this.subAgentEntity.Description = formValue.description || '';
    this.subAgentEntity.TypeID = formValue.typeID;
    this.subAgentEntity.Status = formValue.status;
    this.subAgentEntity.ExecutionMode = formValue.executionMode;
    this.subAgentEntity.Set('Purpose', formValue.purpose || '');
    this.subAgentEntity.Set('UserMessage', formValue.userMessage || '');
    // Note: SystemMessage does not exist on AIAgentEntityExtended, removing this line
    this.subAgentEntity.ModelSelectionMode = formValue.modelSelectionMode;
    this.subAgentEntity.Set('Temperature', formValue.temperature);
    this.subAgentEntity.Set('TopP', formValue.topP);
    this.subAgentEntity.Set('TopK', formValue.topK);
    this.subAgentEntity.Set('MaxTokensPerRun', formValue.maxTokens);
    this.subAgentEntity.Set('EnableCaching', formValue.enableCaching);
    this.subAgentEntity.Set('CacheTTLSeconds', formValue.cacheTTL);
    // Get currently linked prompt IDs
    const linkedPromptIds = this.linkedPrompts.map(p => p.ID);
        title: 'Add Prompts to Sub-Agent',
        linkedPromptIds: linkedPromptIds,
            // Filter out already linked prompts
              !linkedPromptIds.includes(prompt.ID)
            if (newPrompts.length > 0) {
              // Add to UI
              this.linkedPrompts.push(...newPrompts);
              // Create agent prompt link entities
                agentPrompt.AgentID = this.subAgentEntity!.ID;
                agentPrompt.ExecutionOrder = this.agentPromptLinks.length + 1;
                this.agentPromptLinks.push(agentPrompt);
              // Trigger change detection
                `${newPrompts.length} prompt${newPrompts.length === 1 ? '' : 's'} added to sub-agent`,
  public async createNewPrompt() {
        title: `Create New Prompt for ${this.subAgentEntity?.Name || 'Sub-Agent'}`,
              // Store the newly created entities
              this.newlyCreatedPrompts.push(result.prompt);
                this.newlyCreatedPromptTemplates.push(result.template);
                this.newlyCreatedTemplateContents.push(...result.templateContents);
              this.linkedPrompts.push(result.prompt);
              // Create agent prompt link entity
                `New prompt "${result.prompt.Name}" created and linked to sub-agent`,
  public async addAction() {
    // Get currently linked action IDs
    const linkedActionIds = this.linkedActions.map(a => a.ID);
        agentId: this.subAgentEntity?.ID || '',
        agentName: this.subAgentEntity?.Name || 'Sub-Agent',
        existingActionIds: linkedActionIds,
            // Filter out already linked actions
              !linkedActionIds.includes(action.ID)
            if (newActions.length > 0) {
              this.linkedActions.push(...newActions);
              // Create agent action link entities
                agentAction.AgentID = this.subAgentEntity!.ID;
                this.agentActionLinks.push(agentAction);
                `${newActions.length} action${newActions.length === 1 ? '' : 's'} added to sub-agent`,
      console.error('Error in addAction:', error);
        'Error adding actions. Please try again.',
  public removePrompt(prompt: AIPromptEntityExtended) {
    // Remove from UI
    const promptIndex = this.linkedPrompts.findIndex(p => p.ID === prompt.ID);
      this.linkedPrompts.splice(promptIndex, 1);
    // Remove from link entities
    const linkIndex = this.agentPromptLinks.findIndex(ap => ap.PromptID === prompt.ID);
    if (linkIndex >= 0) {
      this.agentPromptLinks.splice(linkIndex, 1);
    // Remove from newly created prompts if it was created in this dialog
    const newPromptIndex = this.newlyCreatedPrompts.findIndex(p => p.ID === prompt.ID);
    if (newPromptIndex >= 0) {
      this.newlyCreatedPrompts.splice(newPromptIndex, 1);
      `Prompt "${prompt.Name}" removed from sub-agent`,
  public removeAction(action: MJActionEntity) {
    const actionIndex = this.linkedActions.findIndex(a => a.ID === action.ID);
      this.linkedActions.splice(actionIndex, 1);
    const linkIndex = this.agentActionLinks.findIndex(aa => aa.ActionID === action.ID);
      this.agentActionLinks.splice(linkIndex, 1);
      `Action "${action.Name}" removed from sub-agent`,
    if (!this.subAgentForm.valid || !this.subAgentEntity) {
      // Update entity with final form values
      const result: CreateSubAgentResult = {
        subAgent: this.subAgentEntity,
        agentPrompts: this.agentPromptLinks,
        agentActions: this.agentActionLinks,
        newPrompts: this.newlyCreatedPrompts.length > 0 ? this.newlyCreatedPrompts : undefined,
        newPromptTemplates: this.newlyCreatedPromptTemplates.length > 0 ? this.newlyCreatedPromptTemplates : undefined,
        newTemplateContents: this.newlyCreatedTemplateContents.length > 0 ? this.newlyCreatedTemplateContents : undefined
      console.error('Error preparing sub-agent for creation:', error);
        'Error preparing sub-agent for creation',
  // Helper methods for UI
    return this.subAgentEntity?.IconClass || 'fa-solid fa-robot';
  public getPromptIcon(): string {
    return 'fa-solid fa-comments';
  public getActionIcon(): string {
    return 'fa-solid fa-bolt';
  public get linkedPromptCount(): number {
    return this.linkedPrompts.length;
  public get linkedActionCount(): number {
    return this.linkedActions.length;
