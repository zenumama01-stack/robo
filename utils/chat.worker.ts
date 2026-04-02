import type {
  WorkerRequest,
  WorkerResponse,
  ModelConfig,
  ChatGenerateRequest,
} from './ai-messages';
// Disable local model check — always fetch from HF Hub
let tokenizer: PreTrainedTokenizer | null = null;
let model: PreTrainedModel | null = null;
function post(msg: WorkerResponse): void {
async function loadModel(config: ModelConfig): Promise<void> {
    const device = await resolveDevice(config.Device);
    const progressCallback = (progress: {
      status: string;
      file?: string;
      loaded?: number;
      total?: number;
      progress?: number;
    }): void => {
      if (progress.status === 'progress') {
          File: progress.file ?? '',
          Loaded: progress.loaded ?? 0,
          Total: progress.total ?? 0,
          Progress: progress.progress ?? 0,
    // Load tokenizer and model separately for streaming control
    tokenizer = await AutoTokenizer.from_pretrained(config.ModelId, {
      progress_callback: progressCallback,
    model = await AutoModelForCausalLM.from_pretrained(config.ModelId, {
      dtype: config.DType as 'q4f16' | 'q4' | 'fp16' | 'fp32',
      device,
    post({ Type: 'ready', ModelId: config.ModelId });
    // Handle both Error objects and numeric WebGPU error codes
    let message: string;
    if (err instanceof Error) {
      message = err.message;
    } else if (typeof err === 'number') {
      // WebGPU error codes
      message = `WebGPU error code ${err}. This may indicate:\n` +
                `- Insufficient GPU memory for this model\n` +
                `- WebGPU not properly initialized\n` +
                `- Browser/driver limitations\n` +
                `Try: (1) Refresh page, (2) Smaller model, or (3) Use WASM device`;
      message = String(err);
    console.error('Model loading failed:', err);
    post({ Type: 'error', Message: `Failed to load model: ${message}` });
async function generate(request: ChatGenerateRequest): Promise<void> {
  if (!tokenizer || !model || !stoppingCriteria) {
    post({ Type: 'error', Message: 'Model not loaded' });
    // Apply chat template to convert messages into model input
    const inputs = tokenizer.apply_chat_template(
      request.Messages.map((m) => ({ role: m.Role, content: m.Content })),
      { add_generation_prompt: true, return_dict: true }
    // Track performance metrics
    let startTime: number | null = null;
    let numTokens = 0;
    let tokensPerSecond = 0;
    const tokenCallback = (): void => {
      if (startTime == null) {
        startTime = performance.now();
      numTokens++;
      if (numTokens > 1 && startTime != null) {
        tokensPerSecond = ((numTokens - 1) / (performance.now() - startTime)) * 1000;
      post({ Type: 'token', Token: text });
    // TextStreamer is the correct streaming mechanism in Transformers.js v3
    const streamer = new TextStreamer(tokenizer, {
      token_callback_function: tokenCallback,
    const maxTokens = request.MaxTokens ?? 512;
    const output = await model.generate({
      max_new_tokens: maxTokens,
      top_p: 0.9,
      return_dict_in_generate: true,
    // Decode the full response for the completion message
    const outputTokenIds = output.sequences;
    const decoded = tokenizer.batch_decode(outputTokenIds, {
    // Extract only the assistant's reply (after the prompt)
    const inputLength = inputs.input_ids.dims?.[1] ?? 0;
    const outputIds = outputTokenIds.slice(null, [inputLength, null]);
    const assistantReply = tokenizer.batch_decode(outputIds, {
    })[0] ?? '';
      Type: 'complete',
      Text: assistantReply,
      TokensPerSecond: Math.round(tokensPerSecond * 10) / 10,
      TotalTokens: numTokens,
    if (err instanceof Error && err.name === 'AbortError') return;
    post({ Type: 'error', Message: `Generation failed: ${message}` });
async function resolveDevice(
  preference: 'webgpu' | 'wasm' | 'auto'
): Promise<'webgpu' | 'wasm'> {
  if (preference === 'wasm') return 'wasm';
  if (preference === 'webgpu') return 'webgpu';
  // Auto-detect
    if ('gpu' in navigator) {
      const gpu = (navigator as Navigator & { gpu: GPU }).gpu;
      const adapter = await gpu.requestAdapter();
      if (adapter != null) return 'webgpu';
    // Fall through to wasm
  return 'wasm';
// ── Message Handler ──────────────────────────────────
self.onmessage = async (event: MessageEvent<WorkerRequest>): Promise<void> => {
    case 'chat:load':
      await loadModel(msg.Config);
    case 'chat:generate':
      await generate(msg);
    case 'chat:abort':
