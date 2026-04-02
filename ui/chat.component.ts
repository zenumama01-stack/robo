import { DecimalPipe } from '@angular/common';
import { ChatService, ChatMessage } from '../ai/chat.service';
import { BROWSER_CHAT_MODELS, type BrowserModelDefinition } from '../ai/model-registry';
  selector: 'app-chat',
  imports: [DecimalPipe, FormsModule],
    <!-- Loading state -->
          <p>Loading {{ ActiveModelName }}... {{ LoadProgress | number:'1.0-0' }}%</p>
            First load downloads the model (~{{ ActiveModelSizeMB }} MB).<br>
            It's cached locally for instant loads afterward.
    <!-- Chat interface -->
      <div class="chat-container">
          <h2>Local AI Chat</h2>
          <span class="model-badge">
            {{ ActiveModelName }} · {{ TokensPerSecond }} tok/s · Running in your browser
        <div class="messages" #messagesContainer>
          @for (msg of DisplayMessages; track $index) {
            <div class="message" [class.user]="msg.Role === 'user'"
                 [class.assistant]="msg.Role === 'assistant'">
              <div class="message-role">{{ msg.Role === 'user' ? 'You' : 'AI' }}</div>
              <div class="message-content">{{ msg.Content }}</div>
          <!-- Streaming indicator -->
          @if (IsGenerating) {
            <div class="message assistant">
              <div class="message-role">AI</div>
              <div class="message-content">
                {{ StreamingText }}<span class="cursor">&#x2588;</span>
        <div class="input-area">
            [(ngModel)]="UserInput"
            (keydown.enter)="OnEnter($event)"
            placeholder="Type a message... (Enter to send, Shift+Enter for newline)"
            [disabled]="IsGenerating"
            rows="2"
          ></textarea>
          @if (!IsGenerating) {
            <button (click)="Send()" [disabled]="!UserInput.trim()" class="send-btn">
              Send
            <button (click)="chatService.Abort()" class="abort-btn">
              Stop
    <!-- Init state (not loading and not ready) -->
    @if (!IsLoading && !IsReady) {
      <div class="init-prompt">
        <h2>Client-Side AI Chat</h2>
        <p>Run a language model entirely in your browser. No data leaves your device.</p>
          <label for="model-select">Model:</label>
          <select id="model-select" [(ngModel)]="SelectedModelId">
            @for (model of AvailableModels; track model.Id) {
        <div class="device-selector">
          <label for="device-select">Device:</label>
          <select id="device-select" [(ngModel)]="SelectedDevice">
            <option value="auto">Auto-detect</option>
            <option value="webgpu">WebGPU (GPU - Fastest)</option>
            <option value="wasm">WASM (CPU - Compatible)</option>
        <button (click)="LoadModel()" class="load-btn">Load Model</button>
          <p class="error">{{ ErrorMessage }}</p>
    :host { display: flex; flex-direction: column; height: 100vh; font-family: system-ui, sans-serif; }
      display: flex; align-items: center; justify-content: center;
      height: 100vh; background: #f8f9fa;
    .loading-content { text-align: center; max-width: 400px; }
      width: 40px; height: 40px; border: 3px solid #e0e0e0;
      border-top-color: #3b82f6; border-radius: 50%;
      animation: spin 0.8s linear infinite; margin: 0 auto 16px;
    @keyframes spin { to { transform: rotate(360deg); } }
      height: 6px; background: #e0e0e0; border-radius: 3px;
      overflow: hidden; margin: 12px 0;
    .progress-fill { height: 100%; background: #3b82f6; transition: width 0.3s; }
    .loading-hint { font-size: 13px; color: #888; margin-top: 12px; }
    .chat-container { display: flex; flex-direction: column; height: 100vh; }
      padding: 16px 20px; border-bottom: 1px solid #e0e0e0;
      display: flex; align-items: baseline; gap: 12px;
    .chat-header h2 { margin: 0; font-size: 18px; }
    .model-badge {
      font-size: 12px; color: #666; background: #f0f0f0;
      padding: 2px 8px; border-radius: 10px;
    .messages { flex: 1; overflow-y: auto; padding: 20px; }
    .message { margin-bottom: 16px; max-width: 80%; }
    .message.user { margin-left: auto; }
    .message.assistant { margin-right: auto; }
    .message-role { font-size: 12px; font-weight: 600; color: #888; margin-bottom: 4px; }
    .message.user .message-role { text-align: right; }
    .message-content {
      padding: 10px 14px; border-radius: 12px; line-height: 1.5;
      white-space: pre-wrap; word-break: break-word;
    .message.user .message-content { background: #3b82f6; color: white; }
    .message.assistant .message-content { background: #f0f0f0; color: #333; }
    .cursor { animation: blink 0.7s step-end infinite; }
    @keyframes blink { 50% { opacity: 0; } }
    .input-area {
      display: flex; gap: 8px; padding: 16px 20px;
      border-top: 1px solid #e0e0e0; background: #fafafa;
    textarea {
      flex: 1; padding: 10px 14px; border: 1px solid #d0d0d0;
      border-radius: 8px; resize: none; font-family: inherit; font-size: 14px;
    textarea:focus { border-color: #3b82f6; }
    .send-btn, .abort-btn, .load-btn {
      padding: 10px 20px; border: none; border-radius: 8px;
      font-size: 14px; cursor: pointer; font-weight: 500;
    .send-btn { background: #3b82f6; color: white; }
    .send-btn:disabled { opacity: 0.5; cursor: default; }
    .abort-btn { background: #ef4444; color: white; }
    .load-btn { background: #3b82f6; color: white; padding: 12px 32px; font-size: 16px; }
    .model-selector, .device-selector {
      display: flex; align-items: center; gap: 8px; margin-bottom: 16px;
    .model-selector select, .device-selector select {
      padding: 8px 12px; border: 1px solid #d0d0d0; border-radius: 6px;
    .init-prompt { text-align: center; padding: 80px 20px; }
    .init-prompt h2 { font-size: 24px; margin-bottom: 8px; }
    .init-prompt p { color: #666; margin-bottom: 24px; }
    .error { color: #ef4444; margin-top: 16px; }
  `],
export class ChatComponent implements OnInit {
  @ViewChild('messagesContainer') MessagesContainer!: ElementRef;
  protected readonly chatService = inject(ChatService);
  IsGenerating = false;
  UserInput = '';
  StreamingText = '';
  TokensPerSecond = 0;
  DisplayMessages: ChatMessage[] = [];
  ActiveModelName = '';
  ActiveModelSizeMB = 0;
  SelectedModelId = BROWSER_CHAT_MODELS[0].Id;
  SelectedDevice: 'auto' | 'webgpu' | 'wasm' = 'webgpu'; // Default to WebGPU for best performance
  AvailableModels = BROWSER_CHAT_MODELS;
    this.chatService.IsLoading$
      .subscribe((v) => (this.IsLoading = v));
    this.chatService.IsReady$
      .subscribe((v) => (this.IsReady = v));
    this.chatService.IsGenerating$
      .subscribe((v) => (this.IsGenerating = v));
    this.chatService.LoadProgress$
      .subscribe((v) => (this.LoadProgress = v));
    this.chatService.CurrentToken$
      .subscribe((token) => {
        // TextStreamer sends incremental text chunks, accumulate them
        this.StreamingText += token;
    this.chatService.GenerationComplete$
        // Clear streaming text and update display in a microtask
        // This ensures IsGenerating has been set to false first
          this.StreamingText = '';
          this.DisplayMessages = this.chatService
            .GetHistory()
            .filter((m) => m.Role !== 'system');
    this.chatService.TokensPerSecond$
      .subscribe((v) => (this.TokensPerSecond = v));
    this.chatService.ActiveModel$
      .subscribe((model) => {
        if (model) {
          this.ActiveModelName = model.Name;
          this.ActiveModelSizeMB = model.ApproxSizeMB;
    this.chatService.Error$
      .subscribe((err) => (this.ErrorMessage = err));
  LoadModel(): void {
    const selected = BROWSER_CHAT_MODELS.find((m) => m.Id === this.SelectedModelId);
    this.chatService.Initialize(selected, this.SelectedDevice);
  Send(): void {
    const text = this.UserInput.trim();
    if (!text) return;
    this.UserInput = '';
    this.DisplayMessages = [
      ...this.chatService.GetHistory().filter((m) => m.Role !== 'system'),
      { Role: 'user', Content: text },
    this.chatService.SendMessage(text);
  OnEnter(event: Event): void {
    const ke = event as KeyboardEvent;
    if (!ke.shiftKey) {
      ke.preventDefault();
      this.Send();
    // Use microtask instead of setTimeout for proper Angular timing
      const el = this.MessagesContainer?.nativeElement;
import { AfterViewInit, ChangeDetectorRef, Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { MarkdownService } from '@memberjunction/ng-markdown';
import { LogError } from '@memberjunction/core'
export class ChatWelcomeQuestion {
  public topLine: string="";
  public bottomLine: string="";
  public prompt: string="";
export class ChatMessage {
  public message!: string;
  public senderName!: string;
  public senderType: 'user' | 'ai' = 'user';
  public id?: any;
  constructor(message: string, senderName: string, senderType: 'user' | 'ai', id: any = null) {
    this.message = message;
    this.senderName = senderName;
    this.senderType = senderType;
    this.id = id;
  selector: 'mj-chat',
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.css'
export class ChatComponent implements AfterViewInit {
  @Input() InitialMessage: string = '';
  @Input() Messages: ChatMessage[] = [];
   * Optional, provide this to show an image for the AI. If not provided, a default robot icon will be shown.
  @Input() AIImageURL: string = '';
  @Input() AILargeImageURL: string = '';
   * Optional, provide up to 4 welcome questions with example prompts. 
   * These will be shown to the user when the chat is first opened and there are no messages.
  @Input() WelcomeQuestions: ChatWelcomeQuestion[] = [];
   * Optional, provide a prompt for the user when they click the clear all messages button.
  @Input() ClearAllMessagesPrompt: string = 'Are you sure you want to clear all messages?';
   * Set this to enable/disable sending of a message. Whenever the input is empty, this field will be
   * ignored and the send button will be disabled.
  @Input() AllowSend: boolean = true;
  public InternalAllowSend: boolean = true;
   * The placeholder text for the input field
  @Input() public Placeholder: string = 'Type a message...';
  private _ShowWaitingIndicator: boolean = false;
  @Input() public get ShowWaitingIndicator(): boolean {
    return this._ShowWaitingIndicator;
  public set ShowWaitingIndicator(value: boolean) {
    this._ShowWaitingIndicator = value;
    this.cd?.detectChanges(); // Manually trigger change detection
    if (!value)  {
      this.FocusTextArea();
  @Output() MessageAdded = new EventEmitter<ChatMessage>();
  @Output() ClearChatRequested = new EventEmitter<void>();
  @ViewChild('messagesContainer', { static: true }) private messagesContainer!: ElementRef;
  @ViewChild('theInput') theInput: ElementRef | undefined;
  public currentMessage: string = '';
  public showingClearAllDialog: boolean = false;
  constructor(private markdownService: MarkdownService, private cd: ChangeDetectorRef) {}
  public SendCurrentMessage(): void {
    if (this.currentMessage.trim() !== '') {
      this.SendMessage(this.currentMessage, 'User', 'user', null);
      this.currentMessage = ''; // Clear the input field
  public handleInputChange(event: any) {
    const val = this.theInput?.nativeElement.value;
    this.InternalAllowSend = this.AllowSend && (val ? val.length > 0 : false);
    this.resizeTextInput();
  protected resizeTextInput() {
      const textarea = this.theInput?.nativeElement;
      if (textarea) {
        textarea.style.height = 'auto'; // Reset height to recalculate
        textarea.style.height = `${textarea.scrollHeight}px`; // Set to scrollHeight    
  public SendMessage(message: string, senderName: string, senderType: 'user' | 'ai', id: any, fireEvent: boolean = true): void {
    const newMessage = new ChatMessage(message, senderName, senderType, id);
    this.AppendMessage(newMessage, fireEvent);  
  public SendUserMessage(message: string) {
    this.SendMessage(message, 'User', 'user', null);
  public HandleClearChat() {
    this.ClearChatRequested.emit();
  public ClearAllMessages() {
    this.Messages = [];
    this.messagesContainer.nativeElement.innerHTML = `<span>${this.InitialMessage}</span>`;
    this.ScrollMessagesToBottom();
    this.cd.detectChanges(); // Manually trigger change detection
    this.showingClearAllDialog = false;
  protected FocusTextArea() {
    setTimeout(() => this.theInput?.nativeElement.focus(), 0); // use a timeout to ensure that angular has updated the DOM
  protected async AppendMessage(message: ChatMessage, fireEvent: boolean = true) {
    const messageWrapElement = document.createElement('div');
    messageWrapElement.className = "chat-message-wrap";
    const imageElement = document.createElement('span');
    if (message.senderType === 'ai') {
      if (this.AIImageURL) {
        const img = document.createElement('img');
        img.src = this.AIImageURL;
        img.style.maxWidth = '24px';
        imageElement.appendChild(img);
        imageElement.classList.add('fa-solid', 'fa-robot');
      imageElement.classList.add('fa-solid', 'fa-user');
    imageElement.classList.add("chat-message-image");
    messageWrapElement.appendChild(imageElement);
    const messageElement = document.createElement('div');
    messageElement.innerHTML = await this.markdownService.parse(message.message);
    messageElement.className = "chat-message";  
      messageElement.classList.add('chat-message-ai');
    messageWrapElement.appendChild(messageElement);
    this.Messages.push(message);
    if (this.Messages.length === 1) {
      // clear out the default message
      this.messagesContainer.nativeElement.innerHTML = '';
    this.messagesContainer.nativeElement.appendChild(messageWrapElement);       
    if (fireEvent)
      this.MessageAdded.emit(message); 
    this.ScrollMessagesToBottom(false);
  protected ScrollMessagesToBottom(animate: boolean = true): void {
      if (animate) {
        const element = this.messagesContainer.nativeElement;
          behavior: 'smooth'  // This enables the smooth scrolling
    } catch(err) {}
  public ShowScrollToBottomButton: boolean = false;
  handleCheckScroll(): void {
    if (element.scrollHeight - element.scrollTop > element.clientHeight) {
      this.ShowScrollToBottomButton = true;
      this.ShowScrollToBottomButton = false;
import { ConversationsApp } from '../conversations.app';
        <h2>Chat</h2>
        <button class="new-tab-btn" (click)="OpenInNewTab()">
          Open Thread in New Tab
      <div class="chat-content">
        <div class="message-list">
          <div class="message" *ngFor="let msg of messages">
              <strong>{{ msg.sender }}</strong>
              <span class="timestamp">{{ msg.time }}</span>
            <p>{{ msg.text }}</p>
    .chat-container {
    .new-tab-btn {
    .chat-content {
    .message-list {
export class ChatComponent {
    { sender: 'Alice', time: '10:30 AM', text: 'Hey, how is the project going?' },
    { sender: 'You', time: '10:32 AM', text: 'Making good progress! Just finishing up the prototype.' },
    { sender: 'Alice', time: '10:33 AM', text: 'Great! Can you show me a demo later?' }
  constructor(private conversationsApp: ConversationsApp) {}
  OpenInNewTab(): void {
    this.conversationsApp.RequestNewTab(
      'Chat Thread: Project Discussion',
      '/conversations/chat/thread-123',
      { threadId: '123' }
