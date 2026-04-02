import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { Subject, BehaviorSubject, takeUntil } from 'rxjs';
import { MJAIAgentPromptEntity, MJAIConfigurationEntity } from '@memberjunction/core-entities';
import { MJNotificationService } from '@memberjunction/ng-notifications';
export interface AgentPromptAdvancedSettingsFormData {
  executionOrder: number;
  purpose: string | null;
  configurationID: string | null;
  contextBehavior: 'Complete' | 'Smart' | 'None' | 'RecentMessages' | 'InitialMessages' | 'Custom';
  contextMessageCount: number | null;
  status: 'Active' | 'Inactive' | 'Deprecated' | 'Preview';
 * Advanced Settings dialog for AI Agent Prompts.
 * Manages execution order, context behavior, and other advanced prompt configurations.
  selector: 'mj-agent-prompt-advanced-settings-dialog',
  templateUrl: './agent-prompt-advanced-settings-dialog.component.html',
  styleUrls: ['./agent-prompt-advanced-settings-dialog.component.css']
export class AgentPromptAdvancedSettingsDialogComponent implements OnInit, OnDestroy {
  agentPrompt!: MJAIAgentPromptEntity;
  allAgentPrompts: MJAIAgentPromptEntity[] = []; // For execution order validation
  public result = new Subject<AgentPromptAdvancedSettingsFormData | null>();
  // Form and data
  advancedForm!: FormGroup;
  isSaving$ = new BehaviorSubject<boolean>(false);
  // Dropdown data
  configurations$ = new BehaviorSubject<MJAIConfigurationEntity[]>([]);
  // Available options
  contextBehaviorOptions = [
    { text: 'Complete Context', value: 'Complete', description: 'Include entire conversation context' },
    { text: 'Smart Context', value: 'Smart', description: 'AI determines relevant context automatically' },
    { text: 'No Context', value: 'None', description: 'No conversation context included' },
    { text: 'Recent Messages', value: 'RecentMessages', description: 'Include only recent messages' },
    { text: 'Initial Messages', value: 'InitialMessages', description: 'Include only conversation start' },
    { text: 'Custom Context', value: 'Custom', description: 'Custom context filtering logic' }
  statusOptions = [
    { text: 'Active', value: 'Active' },
    { text: 'Inactive', value: 'Inactive' },
    { text: 'Deprecated', value: 'Deprecated' },
    { text: 'Preview', value: 'Preview' }
  // Execution order validation
  executionOrderError: string | null = null;
    private fb: FormBuilder,
    this.initializeForm();
    this.loadDropdownData();
  private initializeForm() {
    this.advancedForm = this.fb.group({
      executionOrder: [this.agentPrompt.ExecutionOrder || 0, [Validators.required, Validators.min(0)]],
      purpose: [this.agentPrompt.Purpose],
      configurationID: [this.agentPrompt.ConfigurationID],
      contextBehavior: [this.agentPrompt.ContextBehavior || 'Complete', [Validators.required]],
      contextMessageCount: [this.agentPrompt.ContextMessageCount],
      status: [this.agentPrompt.Status || 'Active', [Validators.required]]
    this.setupValidationLogic();
  private setupValidationLogic() {
    // Context behavior validation
    const contextBehaviorControl = this.advancedForm.get('contextBehavior');
    const contextMessageCountControl = this.advancedForm.get('contextMessageCount');
    contextBehaviorControl?.valueChanges.pipe(
    ).subscribe(behavior => {
      if (behavior === 'RecentMessages' || behavior === 'InitialMessages') {
        contextMessageCountControl?.setValidators([Validators.required, Validators.min(1)]);
        contextMessageCountControl?.clearValidators();
        if (behavior !== 'Custom') {
          contextMessageCountControl?.setValue(null);
      contextMessageCountControl?.updateValueAndValidity();
    const executionOrderControl = this.advancedForm.get('executionOrder');
    executionOrderControl?.valueChanges.pipe(
    ).subscribe(order => {
      this.validateExecutionOrder(order);
  private validateExecutionOrder(order: number) {
    if (order == null) {
      this.executionOrderError = null;
    // Check for conflicts with other prompts (excluding current one)
    const conflictingPrompt = this.allAgentPrompts.find(p => 
      p.ID !== this.agentPrompt.ID && 
      p.ExecutionOrder === order
    if (conflictingPrompt) {
      this.executionOrderError = `Execution order ${order} is already used by another prompt. Please choose a different order.`;
  private async loadDropdownData() {
      // Load AI Configurations
      const configurationsResult = await rv.RunView<MJAIConfigurationEntity>({
        ExtraFilter: "Status = 'Active'",
      if (configurationsResult.Success) {
        this.configurations$.next(configurationsResult.Results || []);
      console.error('Error loading dropdown data:', error);
      MJNotificationService.Instance.CreateSimpleNotification(
        'Error loading form data. Please try again.',
        'error',
        3000
  // === Validation Helpers ===
  isFieldInvalid(fieldName: string): boolean {
    const field = this.advancedForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  getFieldError(fieldName: string): string {
    if (field?.errors) {
      if (field.errors['required']) return `${fieldName} is required`;
      if (field.errors['min']) return `${fieldName} must be greater than or equal to ${field.errors['min'].min}`;
  hasExecutionOrderError(): boolean {
    return !!this.executionOrderError;
  // === Context Behavior Helpers ===
  requiresMessageCount(): boolean {
    const behavior = this.advancedForm.get('contextBehavior')?.value;
    return behavior === 'RecentMessages' || behavior === 'InitialMessages';
  getContextBehaviorDescription(value: string): string {
    const option = this.contextBehaviorOptions.find(opt => opt.value === value);
    return option?.description || '';
    this.result.next(null);
  async save() {
    if (this.advancedForm.invalid || this.hasExecutionOrderError()) {
      this.advancedForm.markAllAsTouched();
        'Please fix validation errors before saving',
    this.isSaving$.next(true);
      const formData: AgentPromptAdvancedSettingsFormData = {
        executionOrder: this.advancedForm.get('executionOrder')?.value,
        purpose: this.advancedForm.get('purpose')?.value || null,
        configurationID: this.advancedForm.get('configurationID')?.value || null,
        contextBehavior: this.advancedForm.get('contextBehavior')?.value,
        contextMessageCount: this.advancedForm.get('contextMessageCount')?.value || null,
        status: this.advancedForm.get('status')?.value
      this.result.next(formData);
      console.error('Error saving advanced settings:', error);
        'Error saving settings. Please try again.',
      this.isSaving$.next(false);
