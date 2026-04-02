// Model registry for pluggable model selection
export interface BrowserModelDefinition {
  DType: string;
  DefaultTemperature: number;
  Category: 'chat' | 'speech' | 'embeddings';
export const BROWSER_CHAT_MODELS: BrowserModelDefinition[] = [
    Name: 'Phi-4 Mini Instruct',
    MaxNewTokens: 2048,
    DefaultTemperature: 0.7,
    Category: 'chat',
    Name: 'Phi-3.5 Mini Instruct',
    Name: 'SmolLM2 1.7B Instruct',
    MaxNewTokens: 1024,
    Name: 'SmolLM2 360M Instruct',
    MaxNewTokens: 512,
/** Select the best available chat model based on device capabilities. */
export async function SelectBestChatModel(): Promise<BrowserModelDefinition> {
  const hasWebGPU = await DetectWebGPU();
  if (hasWebGPU) {
    return BROWSER_CHAT_MODELS[0]; // Phi-4 Mini
  // Fall back to first model that doesn't require WebGPU
    BROWSER_CHAT_MODELS.find((m) => !m.RequiresWebGPU) ??
    BROWSER_CHAT_MODELS[BROWSER_CHAT_MODELS.length - 1]
export async function DetectWebGPU(): Promise<boolean> {
    if (!('gpu' in navigator)) return false;
    const adapter = await (navigator as Navigator & { gpu: GPU }).gpu.requestAdapter();
    return adapter != null;
