import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DialogRef } from '@progress/kendo-angular-dialog';
import { NotificationService } from '@progress/kendo-angular-notification';
import { AIAgentEntityExtended, AIModelEntityExtended } from "@memberjunction/ai-core-plus";
import { NavigationService } from '@memberjunction/ng-shared';
import { BehaviorSubject } from 'rxjs';
export interface NewAgentConfig {
  redirectToForm?: boolean;
  selector: 'mj-new-agent-dialog',
  templateUrl: './new-agent-dialog.component.html',
  styleUrls: ['./new-agent-dialog.component.css']
export class NewAgentDialogComponent implements OnInit {
  @Input() config: NewAgentConfig = {
    redirectToForm: true
  form!: FormGroup;
  models$ = new BehaviorSubject<AIModelEntityExtended[]>([]);
  agentTypes$ = new BehaviorSubject<MJAIAgentTypeEntity[]>([]);
  isSubmitting = false;
    private dialog: DialogRef,
    private navigationService: NavigationService,
    private notificationService: NotificationService
    this.loadData();
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(255)]],
      description: [''],
      modelId: ['', Validators.required],
      agentType: ['standard', Validators.required],
      systemPrompt: ['You are a helpful AI assistant.'],
      enableStreaming: [true],
      temperature: [0.7, [Validators.min(0), Validators.max(2)]],
      maxTokens: [2000, [Validators.min(1), Validators.max(8000)]]
  private async loadData() {
      await engine.Config(false);
      const models = engine.Models;
      models.sort ((a, b) => { 
      this.models$.next(models || []);
      // Pre-select first model if available
      if (models && models.length > 0) {
        this.form.patchValue({ modelId: models[0].ID });
      const agentTypes = engine.AgentTypes;
      this.agentTypes$.next(agentTypes as MJAIAgentTypeEntity[] || []);
      console.error('Error loading data:', error);
      this.showError('Failed to load required data');
  async onSubmit() {
    if (this.form.invalid || this.isSubmitting) {
    this.isSubmitting = true;
      const agent = await md.GetEntityObject<AIAgentEntityExtended>('MJ: AI Agents');
        throw new Error('Failed to create agent entity');
      // Set agent properties
      agent.Name = this.form.value.name;
      agent.Description = this.form.value.description;
      // Set parent agent if provided
      if (this.config.parentAgentId) {
        agent.ParentID = this.config.parentAgentId;
      // Set execution mode
      agent.ExecutionMode = 'Sequential';
      agent.ExecutionOrder = 0;
      agent.ExposeAsAction = false;
      // Save the agent
      const saveResult = await agent.Save();
        this.showSuccess('Agent created successfully!');
        // Close dialog with the new agent
        this.dialog.close({ agent, action: 'created' });
        // Redirect to form if configured
        if (this.config.redirectToForm && !this.config.parentAgentId) {
          // Only redirect for top-level agents - use NavigationService to open the record
            this.navigationService.OpenEntityRecord('MJ: AI Agents', agent.PrimaryKey);
        throw new Error('Failed to save agent');
      console.error('Error creating agent:', error);
      this.showError('Failed to create agent: ' + (error.message || 'Unknown error'));
      this.isSubmitting = false;
  onCancel() {
    this.dialog.close({ action: 'cancelled' });
  private showSuccess(message: string) {
    this.notificationService.show({
      content: message,
      type: { style: 'success', icon: true },
      position: { horizontal: 'right', vertical: 'top' },
      animation: { type: 'slide', duration: 300 },
      hideAfter: 3000
  private showError(message: string) {
      type: { style: 'error', icon: true },
      hideAfter: 5000
