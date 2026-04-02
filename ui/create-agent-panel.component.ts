 * Configuration for the CreateAgentPanel component.
export interface CreateAgentConfig {
    /** Optional parent agent ID for creating sub-agents */
    ParentAgentId?: string;
    /** Optional parent agent name for display */
    ParentAgentName?: string;
    /** Initial name for the agent */
    InitialName?: string;
    InitialTypeId?: string;
    /** Title for the panel header */
    Title?: string;
    /** Whether to show advanced model configuration fields */
    ShowAdvancedConfig?: boolean;
 * Result returned when an agent is created.
export interface CreateAgentResult {
    /** Created agent entity (not saved to database) */
    Agent: AIAgentEntityExtended;
    AgentPrompts?: MJAIAgentPromptEntity[];
    AgentActions?: MJAIAgentActionEntity[];
 * Base panel component for creating AI Agents.
 * This component contains all the logic and UI for agent creation.
 * Use CreateAgentDialogComponent or CreateAgentSlideInComponent as wrappers.
 * - Creates both top-level agents and sub-agents
 * - Agent type selection with loaded types from AIEngineBase
 * - Basic info: Name, Description, Status, Execution Mode
 * - Advanced model configuration (optional)
 * - Linked prompts and actions management
 * - Returns unsaved entities for parent to handle atomically
 * <mj-create-agent-panel
 *     (Cancelled)="onCancel()">
 * </mj-create-agent-panel>
    selector: 'mj-create-agent-panel',
    templateUrl: './create-agent-panel.component.html',
    styleUrls: ['./create-agent-panel.component.css'],
export class CreateAgentPanelComponent implements OnInit, OnDestroy {
    // Inputs & Outputs
    private _config: CreateAgentConfig = {};
    set Config(value: CreateAgentConfig) {
        this._config = value || {};
    get Config(): CreateAgentConfig {
    /** Emitted when agent is successfully created (returns unsaved entities) */
    /** Emitted when user cancels the creation */
    @Output() Cancelled = new EventEmitter<void>();
    // Public State
    public Form!: FormGroup;
    public IsSubmitting = false;
    public AgentTypes: MJAIAgentTypeEntity[] = [];
    public LinkedPrompts: AIPromptEntityExtended[] = [];
    public LinkedActions: MJActionEntity[] = [];
    public ShowAdvancedConfig = false;
    // Private State
    private agentEntity: AIAgentEntityExtended | null = null;
    private agentPromptLinks: MJAIAgentPromptEntity[] = [];
    private agentActionLinks: MJAIAgentActionEntity[] = [];
    private availablePrompts: AIPromptEntityExtended[] = [];
    private availableActions: MJActionEntity[] = [];
    // Initialization
        this.Form = this.fb.group({
            typeId: ['', Validators.required],
            status: ['Pending'],
            executionMode: ['Sequential'],
            purpose: [''],
            userMessage: [''],
            modelSelectionMode: ['Agent Type'],
            temperature: [0.1, [Validators.min(0), Validators.max(2)]],
            topP: [0.1, [Validators.min(0), Validators.max(1)]],
            topK: [40, [Validators.min(1), Validators.max(100)]],
            maxTokens: [4000, [Validators.min(1), Validators.max(32000)]],
            enableCaching: [false],
            cacheTTL: [3600, [Validators.min(60), Validators.max(86400)]]
        // Watch for form changes
        this.Form.valueChanges
            .subscribe(() => this.syncEntityFromForm());
    private applyConfig(): void {
        if (!this.Form) return;
        if (this._config.InitialName) {
            this.Form.patchValue({ name: this._config.InitialName });
        if (this._config.InitialTypeId) {
            this.Form.patchValue({ typeId: this._config.InitialTypeId });
        if (this._config.ShowAdvancedConfig !== undefined) {
            this.ShowAdvancedConfig = this._config.ShowAdvancedConfig;
            this.AgentTypes = engine.AgentTypes as MJAIAgentTypeEntity[];
            // Load available prompts and actions in parallel
            const [promptsResult, actionsResult] = await rv.RunViews([
            if (promptsResult.Success && promptsResult.Results) {
                this.availablePrompts = promptsResult.Results as AIPromptEntityExtended[];
                this.availableActions = actionsResult.Results as MJActionEntity[];
            // Set default type if not specified and types are available
            if (!this._config.InitialTypeId && this.AgentTypes.length > 0) {
                this.Form.patchValue({ typeId: this.AgentTypes[0].ID });
            // Create the agent entity
            await this.createAgentEntity();
            console.error('Error loading agent creation data:', error);
            this.ErrorMessage = 'Failed to load agent creation data. Please try again.';
    private async createAgentEntity(): Promise<void> {
        this.agentEntity = await md.GetEntityObject<AIAgentEntityExtended>('MJ: AI Agents');
        this.agentEntity.NewRecord();
        // Set defaults
        this.agentEntity.Status = 'Pending';
        this.agentEntity.ExecutionMode = 'Sequential';
        this.agentEntity.ExposeAsAction = false;
        this.agentEntity.ModelSelectionMode = 'Agent Type';
        // Set parent if creating sub-agent
        if (this._config.ParentAgentId) {
            this.agentEntity.ParentID = this._config.ParentAgentId;
        // Set model config defaults
        this.agentEntity.Set('Temperature', 0.1);
        this.agentEntity.Set('TopP', 0.1);
        this.agentEntity.Set('TopK', 40);
        this.agentEntity.Set('MaxTokensPerRun', 4000);
        this.agentEntity.Set('EnableCaching', false);
        this.agentEntity.Set('CacheTTLSeconds', 3600);
    private syncEntityFromForm(): void {
        if (!this.agentEntity) return;
        const v = this.Form.value;
        this.agentEntity.Name = v.name;
        this.agentEntity.Description = v.description || '';
        this.agentEntity.TypeID = v.typeId;
        this.agentEntity.Status = v.status;
        this.agentEntity.ExecutionMode = v.executionMode;
        this.agentEntity.Set('Purpose', v.purpose || '');
        this.agentEntity.Set('UserMessage', v.userMessage || '');
        this.agentEntity.ModelSelectionMode = v.modelSelectionMode;
        this.agentEntity.Set('Temperature', v.temperature);
        this.agentEntity.Set('TopP', v.topP);
        this.agentEntity.Set('TopK', v.topK);
        this.agentEntity.Set('MaxTokensPerRun', v.maxTokens);
        this.agentEntity.Set('EnableCaching', v.enableCaching);
        this.agentEntity.Set('CacheTTLSeconds', v.cacheTTL);
    public get IsSubAgent(): boolean {
        return !!this._config.ParentAgentId;
        if (this._config.Title) return this._config.Title;
        return this.IsSubAgent ? 'Create Sub-Agent' : 'Create New Agent';
    public get LinkedPromptCount(): number {
        return this.LinkedPrompts.length;
    public get LinkedActionCount(): number {
        return this.LinkedActions.length;
    public get AvailablePrompts(): AIPromptEntityExtended[] {
        return this.availablePrompts;
    public get AvailableActions(): MJActionEntity[] {
        return this.availableActions;
    public ToggleAdvancedConfig(): void {
        this.ShowAdvancedConfig = !this.ShowAdvancedConfig;
    public TrackById(_index: number, item: { ID: string }): string {
    // Prompt Management
    public ShowPromptSelector = false;
    public PromptSearchQuery = '';
    public FilteredPrompts: AIPromptEntityExtended[] = [];
    public OnOpenPromptSelector(): void {
        this.ShowPromptSelector = true;
        this.PromptSearchQuery = '';
        this.updateFilteredPrompts();
    public OnClosePromptSelector(): void {
        this.ShowPromptSelector = false;
    public OnPromptSearchChanged(): void {
    private updateFilteredPrompts(): void {
        const query = this.PromptSearchQuery.toLowerCase().trim();
        const linkedIds = new Set(this.LinkedPrompts.map(p => p.ID));
        let available = this.availablePrompts.filter(p => !linkedIds.has(p.ID));
            available = available.filter(p =>
                p.Name.toLowerCase().includes(query) ||
                (p.Description && p.Description.toLowerCase().includes(query))
        this.FilteredPrompts = available.slice(0, 20);
    public async OnSelectPrompt(prompt: AIPromptEntityExtended): Promise<void> {
        // Add to linked prompts
        this.LinkedPrompts.push(prompt);
        // Create link entity
        agentPrompt.AgentID = this.agentEntity.ID;
        this.OnClosePromptSelector();
    public RemovePrompt(prompt: AIPromptEntityExtended): void {
        const index = this.LinkedPrompts.findIndex(p => p.ID === prompt.ID);
            this.LinkedPrompts.splice(index, 1);
    // Action Management
    public ShowActionSelector = false;
    public ActionSearchQuery = '';
    public FilteredActions: MJActionEntity[] = [];
    public OnOpenActionSelector(): void {
        this.ShowActionSelector = true;
        this.ActionSearchQuery = '';
        this.updateFilteredActions();
    public OnCloseActionSelector(): void {
        this.ShowActionSelector = false;
    public OnActionSearchChanged(): void {
    private updateFilteredActions(): void {
        const query = this.ActionSearchQuery.toLowerCase().trim();
        const linkedIds = new Set(this.LinkedActions.map(a => a.ID));
        let available = this.availableActions.filter(a => !linkedIds.has(a.ID));
            available = available.filter(a =>
                a.Name.toLowerCase().includes(query) ||
                (a.Description && a.Description.toLowerCase().includes(query))
        this.FilteredActions = available.slice(0, 20);
    public async OnSelectAction(action: MJActionEntity): Promise<void> {
        // Add to linked actions
        this.LinkedActions.push(action);
        agentAction.AgentID = this.agentEntity.ID;
        this.OnCloseActionSelector();
    public RemoveAction(action: MJActionEntity): void {
        const index = this.LinkedActions.findIndex(a => a.ID === action.ID);
            this.LinkedActions.splice(index, 1);
    // Form Actions
    public OnSubmit(): void {
        if (this.Form.invalid || !this.agentEntity || this.IsSubmitting) {
            // Mark all fields as touched to show validation errors
            this.Form.markAllAsTouched();
            // Sync final form values to entity
            this.syncEntityFromForm();
            // Emit result with unsaved entities
            const result: CreateAgentResult = {
                Agent: this.agentEntity,
                AgentPrompts: this.agentPromptLinks.length > 0 ? this.agentPromptLinks : undefined,
                AgentActions: this.agentActionLinks.length > 0 ? this.agentActionLinks : undefined
            this.ErrorMessage = 'Failed to create agent. Please try again.';
        this.Cancelled.emit();
