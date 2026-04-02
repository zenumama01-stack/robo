import type { WorkerResponse } from './ai-messages';
  type BrowserModelDefinition,
  SelectBestChatModel,
} from './model-registry';
export interface ChatMessage {
  Role: 'system' | 'user' | 'assistant';
  Content: string;
export class ChatService implements OnDestroy {
  // State
  private _IsGenerating = new BehaviorSubject<boolean>(false);
  private _CurrentToken = new Subject<string>();
  private _GenerationComplete = new Subject<string>();
  private _ActiveModel = new BehaviorSubject<BrowserModelDefinition | null>(null);
  private _TokensPerSecond = new BehaviorSubject<number>(0);
  IsGenerating$: Observable<boolean> = this._IsGenerating.asObservable();
  CurrentToken$: Observable<string> = this._CurrentToken.asObservable();
  GenerationComplete$: Observable<string> = this._GenerationComplete.asObservable();
  ActiveModel$: Observable<BrowserModelDefinition | null> = this._ActiveModel.asObservable();
  TokensPerSecond$: Observable<number> = this._TokensPerSecond.asObservable();
  // Conversation history
  private Messages: ChatMessage[] = [
      Role: 'system',
      Content: 'You are a helpful assistant. Keep responses concise and clear.',
  async Initialize(modelDef?: BrowserModelDefinition, device: 'auto' | 'webgpu' | 'wasm' = 'auto'): Promise<void> {
    if (this.worker) return;
    const selectedModel = modelDef ?? (await SelectBestChatModel());
    this._ActiveModel.next(selectedModel);
      new URL('./chat.worker', import.meta.url),
    this.worker.onmessage = (event: MessageEvent<WorkerResponse>) => {
      Type: 'chat:load',
      Config: {
        ModelId: selectedModel.HuggingFaceId,
        Device: device,
        DType: selectedModel.DType,
        MaxNewTokens: selectedModel.MaxNewTokens,
  SendMessage(userMessage: string): void {
    if (!this.worker || !this._IsReady.value) return;
    this.Messages.push({ Role: 'user', Content: userMessage });
    this._IsGenerating.next(true);
      Type: 'chat:generate',
      Messages: [...this.Messages],
      MaxTokens: this._ActiveModel.value?.MaxNewTokens ?? 512,
    this.worker?.postMessage({ Type: 'chat:abort' });
    this._IsGenerating.next(false);
  ClearHistory(): void {
    this.Messages = [this.Messages[0]]; // Keep system prompt
  GetHistory(): ChatMessage[] {
    return [...this.Messages];
  private handleWorkerMessage(msg: WorkerResponse): void {
        this._LoadProgress.next(msg.Progress);
      case 'token':
        this._CurrentToken.next(msg.Token);
      case 'complete':
        this._GenerationComplete.next(msg.Text);
        this._TokensPerSecond.next(msg.TokensPerSecond);
        this.Messages.push({ Role: 'assistant', Content: msg.Text });
