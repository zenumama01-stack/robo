import { Component, OnInit, OnDestroy, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { ComponentStudioStateService, ComponentError } from '../../services/component-studio-state.service';
 * Represents a single message in the AI assistant chat thread
  Role: 'user' | 'assistant' | 'system';
 * Represents a quick action button in the AI assistant panel
interface QuickAction {
  Icon: string;
  Prompt: string;
  RequiresError: boolean;
 * Represents an AI model option in the model selector dropdown
interface AIModelOption {
  Vendor: string | null;
  DisplayLabel: string;
  selector: 'mj-ai-assistant-panel',
  templateUrl: './ai-assistant-panel.component.html',
  styleUrls: ['./ai-assistant-panel.component.css']
export class AIAssistantPanelComponent implements OnInit, OnDestroy {
  @ViewChild('chatThread') chatThreadEl!: ElementRef<HTMLDivElement>;
  @ViewChild('chatInput') chatInputEl!: ElementRef<HTMLTextAreaElement>;
  // --- Chat State ---
  Messages: ChatMessage[] = [];
  InputText = '';
  IsWaitingForResponse = false;
  // --- Model Selector ---
  AvailableModels: AIModelOption[] = [];
  SelectedModelID: string | null = null;
  IsLoadingModels = false;
  // --- Quick Actions ---
  QuickActions: QuickAction[] = [
    { Label: 'Fix Errors', Icon: 'fa-bug', Prompt: 'Fix this error: ', RequiresError: true },
    { Label: 'Improve Code', Icon: 'fa-magic', Prompt: 'Review and improve the current component code. Suggest optimizations, better patterns, and cleaner structure.', RequiresError: false },
    { Label: 'Generate Code', Icon: 'fa-code', Prompt: 'Generate code for the current component based on its specification.', RequiresError: false },
    { Label: 'Explain', Icon: 'fa-question-circle', Prompt: 'Explain what the current component does, including its structure, data flow, and key behaviors.', RequiresError: false }
    public State: ComponentStudioStateService,
    this.subscribeToErrorEvents();
    await this.LoadModels();
    this.addWelcomeMessage();
  // MODEL LOADING
  async LoadModels(): Promise<void> {
    this.IsLoadingModels = true;
      const result = await rv.RunView<MJAIModelEntity>({
        ExtraFilter: `IsActive = 1 AND AIModelTypeID IN (SELECT ID FROM __mj.vwAIModelTypes WHERE Name = 'LLM')`,
        OrderBy: 'PowerRank DESC, Name ASC',
        this.AvailableModels = result.Results.map(model => ({
          ID: model.ID,
          Name: model.Name,
          Vendor: model.Vendor,
          DisplayLabel: model.Vendor ? `${model.Name} (${model.Vendor})` : model.Name
        if (this.AvailableModels.length > 0) {
          this.SelectedModelID = this.AvailableModels[0].ID;
      console.error('Error loading AI models:', error);
      this.IsLoadingModels = false;
  // ERROR SUBSCRIPTION
  private subscribeToErrorEvents(): void {
    this.State.SendErrorToAI
      .subscribe((error: ComponentError) => {
        this.handleIncomingError(error);
  private handleIncomingError(error: ComponentError): void {
    const errorDetails = error.technicalDetails
      ? (typeof error.technicalDetails === 'string' ? error.technicalDetails : JSON.stringify(error.technicalDetails, null, 2))
    const systemContent = `Error detected [${error.type}]: ${error.message}${errorDetails ? '\n\nDetails:\n' + errorDetails : ''}`;
    this.addMessage('system', systemContent);
    this.InputText = `Fix this error: ${error.type} - ${error.message}`;
    this.focusInput();
  // WELCOME MESSAGE
  private addWelcomeMessage(): void {
    this.addMessage(
      'assistant',
      'Welcome to the Component Studio AI Assistant. I can help you fix errors, improve code, generate components, and explain how things work. Select a component and ask me anything!'
  // CHAT ACTIONS
  OnSendMessage(): void {
    const text = this.InputText.trim();
    if (!text || this.IsWaitingForResponse) return;
    this.addMessage('user', text);
    this.InputText = '';
    this.resetInputHeight();
    this.simulateResponse(text);
  OnQuickAction(action: QuickAction): void {
    if (action.RequiresError && !this.State.CurrentError) return;
    if (action.RequiresError && this.State.CurrentError) {
      const errorContext = `${this.State.CurrentError.type} - ${this.State.CurrentError.message}`;
      this.InputText = `${action.Prompt}${errorContext}`;
      this.InputText = action.Prompt;
  OnInputKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      this.OnSendMessage();
  OnInputChange(): void {
    this.autoGrowTextarea();
  OnCollapsePanel(): void {
    this.State.IsAIPanelCollapsed = true;
    this.State.StateChanged.emit();
  IsQuickActionEnabled(action: QuickAction): boolean {
    if (action.RequiresError) {
      return this.State.CurrentError != null;
  // MESSAGE MANAGEMENT
  private addMessage(role: 'user' | 'assistant' | 'system', content: string): void {
    this.Messages.push({
      Role: role,
      Content: content,
      Timestamp: new Date()
    this.scrollToBottom();
  private simulateResponse(userMessage: string): void {
    this.IsWaitingForResponse = true;
        'AI assistant coming soon \u2014 agent integration in progress. Your message has been received and will be processed once the AI backend is connected.'
      this.IsWaitingForResponse = false;
  // UI HELPERS
  private scrollToBottom(): void {
      if (this.chatThreadEl) {
        const el = this.chatThreadEl.nativeElement;
        el.scrollTop = el.scrollHeight;
  private focusInput(): void {
      if (this.chatInputEl) {
        this.chatInputEl.nativeElement.focus();
  private autoGrowTextarea(): void {
    if (!this.chatInputEl) return;
    const textarea = this.chatInputEl.nativeElement;
    textarea.style.height = 'auto';
    const lineHeight = 20;
    const maxLines = 4;
    const maxHeight = lineHeight * maxLines;
    textarea.style.height = Math.min(textarea.scrollHeight, maxHeight) + 'px';
  private resetInputHeight(): void {
    this.chatInputEl.nativeElement.style.height = 'auto';
  FormatTimestamp(date: Date): string {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  TrackByTimestamp(index: number, message: ChatMessage): number {
    return message.Timestamp.getTime();
