export interface SubAgentAdvancedSettingsFormData {
  executionMode: 'Sequential' | 'Parallel';
  status: 'Active' | 'Disabled' | 'Pending';
  typeID: string | null;
  exposeAsAction: boolean;
 * Advanced Settings dialog for Sub-Agents.
 * Manages execution order, execution mode, and other advanced sub-agent configurations.
  selector: 'mj-sub-agent-advanced-settings-dialog',
  templateUrl: './sub-agent-advanced-settings-dialog.component.html',
  styleUrls: ['./sub-agent-advanced-settings-dialog.component.css']
export class SubAgentAdvancedSettingsDialogComponent implements OnInit, OnDestroy {
  subAgent!: AIAgentEntityExtended;
  allSubAgents: AIAgentEntityExtended[] = []; // For execution order validation
  public result = new Subject<SubAgentAdvancedSettingsFormData | null>();
  executionModeOptions = [
      text: 'Sequential', 
      value: 'Sequential', 
      description: 'Child agents execute one after another in order',
      icon: 'fa-list-ol'
      text: 'Parallel', 
      value: 'Parallel', 
      description: 'Child agents execute simultaneously for faster processing',
      icon: 'fa-layer-group'
    { text: 'Disabled', value: 'Disabled' },
    { text: 'Pending', value: 'Pending' }
      executionOrder: [this.subAgent.ExecutionOrder || 0, [Validators.required, Validators.min(0)]],
      executionMode: [this.subAgent.ExecutionMode || 'Sequential', [Validators.required]],
      status: [this.subAgent.Status || 'Active', [Validators.required]],
      typeID: [this.subAgent.TypeID],
      exposeAsAction: [this.subAgent.ExposeAsAction || false]
    // ExposeAsAction validation (sub-agents cannot be exposed as actions)
    const exposeAsActionControl = this.advancedForm.get('exposeAsAction');
    exposeAsActionControl?.valueChanges.pipe(
    ).subscribe(expose => {
      if (expose && this.subAgent.ParentID) {
        // Sub-agents cannot be exposed as actions
        exposeAsActionControl.setValue(false);
          'Sub-agents cannot be exposed as actions. Only root agents can be exposed.',
    // Check for conflicts with other sub-agents under the same parent (excluding current one)
    const conflictingAgent = this.allSubAgents.find(agent => 
      agent.ID !== this.subAgent.ID && 
      agent.ParentID === this.subAgent.ParentID &&
      agent.ExecutionOrder === order
    if (conflictingAgent) {
      this.executionOrderError = `Execution order ${order} is already used by "${conflictingAgent.Name}". Please choose a different order.`;
      // Load AI Agent Types
      const agentTypesResult = await rv.RunView<MJAIAgentTypeEntity>({
      if (agentTypesResult.Success) {
        this.agentTypes$.next(agentTypesResult.Results || []);
  // === Execution Mode Helpers ===
  getExecutionModeIcon(mode: string): string {
    const option = this.executionModeOptions.find(opt => opt.value === mode);
    return option?.icon || 'fa-robot';
  getExecutionModeDescription(mode: string): string {
      const formData: SubAgentAdvancedSettingsFormData = {
        executionMode: this.advancedForm.get('executionMode')?.value,
        status: this.advancedForm.get('status')?.value,
        typeID: this.advancedForm.get('typeID')?.value || null,
        exposeAsAction: false // Sub-agents cannot be exposed as actions
