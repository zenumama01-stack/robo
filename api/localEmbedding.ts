import { EmbedTextParams, EmbedTextsParams, EmbedTextResult, EmbedTextsResult, BaseEmbeddings, ModelUsage, ErrorAnalyzer } from "@memberjunction/ai";
// Dynamic import for ESM-only package
// TypeScript types for @xenova/transformers (since we can't import them directly)
interface TransformersEnv {
    allowLocalModels: boolean;
    cacheDir: string;
    localURL?: string;
interface TransformersModule {
    pipeline: (task: string, model: string, options?: Record<string, unknown>) => Promise<unknown>;
    env: TransformersEnv;
let pipeline: TransformersModule['pipeline'] | null = null;
let env: TransformersEnv | null = null;
let transformersLoaded = false;
let transformersLoadingPromise: Promise<void> | null = null;
let pendingSettings: Record<string, unknown> = {};
// Retry configuration for model downloads
const DEFAULT_MAX_RETRIES = 5;
const DEFAULT_INITIAL_DELAY_MS = 2000;
const DEFAULT_MAX_DELAY_MS = 30000;
 * Delays execution for the specified number of milliseconds
function delay(ms: number): Promise<void> {
 * Calculates exponential backoff delay with jitter
function calculateBackoffDelay(attempt: number, initialDelay: number, maxDelay: number): number {
    // Exponential backoff: initialDelay * 2^attempt
    const exponentialDelay = initialDelay * Math.pow(2, attempt);
    // Add jitter (±25%) to prevent thundering herd
    const jitter = exponentialDelay * 0.25 * (Math.random() * 2 - 1);
    const delayWithJitter = exponentialDelay + jitter;
    // Cap at max delay
    return Math.min(delayWithJitter, maxDelay);
 * Loads the @xenova/transformers ESM module with proper synchronization.
 * Uses a loading promise to prevent multiple concurrent imports when
 * parallel embedding requests occur during startup.
async function loadTransformers(): Promise<void> {
    // Fast path: already loaded
    if (transformersLoaded) {
    // If currently loading, wait for the same promise (prevents duplicate imports)
    if (transformersLoadingPromise) {
        return transformersLoadingPromise;
    // Start loading and store the promise for concurrent callers to reuse
    transformersLoadingPromise = (async () => {
        // Use dynamic import to load ESM module in CommonJS context
        // This approach is cleaner than Function constructor and TypeScript-friendly
        const transformers = await (eval('import("@xenova/transformers")') as Promise<TransformersModule>);
        pipeline = transformers.pipeline;
        env = transformers.env;
        // Configure Transformers.js settings
        env.allowLocalModels = true;
        // Note: localURL might not be available in all versions of transformers.js
        if ('localURL' in env) {
            env.localURL = process.env.TRANSFORMERS_LOCAL_URL || '';
        env.cacheDir = process.env.TRANSFORMERS_CACHE_DIR || './.cache/transformers';
        // Apply any pending settings that were set before transformers was loaded
        if (pendingSettings.cacheDir) {
            env.cacheDir = pendingSettings.cacheDir as string;
        if (pendingSettings.localURL && 'localURL' in env) {
            env.localURL = pendingSettings.localURL as string;
        transformersLoaded = true;
@RegisterClass(BaseEmbeddings, 'LocalEmbedding')
export class LocalEmbedding extends BaseEmbeddings {
    private static pipelines: Map<string, any> = new Map();
    private static loadingPromises: Map<string, Promise<any>> = new Map();
        // Local embeddings don't require an API key
        super(apiKey || 'local');
     * Get or create a pipeline for the specified model
    private async getPipeline(modelName: string): Promise<any> {
        // Ensure transformers is loaded
        await loadTransformers();
        // Check if pipeline already exists
        if (LocalEmbedding.pipelines.has(modelName)) {
            return LocalEmbedding.pipelines.get(modelName)!;
        // Check if pipeline is currently being loaded
        if (LocalEmbedding.loadingPromises.has(modelName)) {
            return LocalEmbedding.loadingPromises.get(modelName)!;
        // Create loading promise
        // The modelName should be the Hugging Face model ID (e.g., 'Xenova/all-MiniLM-L6-v2')
        const loadingPromise = this.loadPipeline(modelName);
        LocalEmbedding.loadingPromises.set(modelName, loadingPromise);
            const pipeline = await loadingPromise;
            LocalEmbedding.pipelines.set(modelName, pipeline);
            LocalEmbedding.loadingPromises.delete(modelName);
     * Load a pipeline for the specified model with retry logic.
     * Uses exponential backoff to handle transient network failures when
     * downloading models from HuggingFace Hub on first startup.
    private async loadPipeline(modelId: string): Promise<unknown> {
        if (!pipeline) {
            throw new Error('Transformers module not loaded');
        const maxRetries = DEFAULT_MAX_RETRIES;
        let lastError: unknown = null;
        for (let attempt = 0; attempt < maxRetries; attempt++) {
                if (attempt === 0) {
                    // First attempt - let user know download may take time
                    console.log(`Loading embedding model ${modelId} from HuggingFace (first-time download may take a moment)...`);
                    const backoffDelay = calculateBackoffDelay(attempt - 1, DEFAULT_INITIAL_DELAY_MS, DEFAULT_MAX_DELAY_MS);
                    console.log(`  Retrying download (attempt ${attempt + 1}/${maxRetries})...`);
                    await delay(backoffDelay);
                // Create feature extraction pipeline
                const pipe = await pipeline('feature-extraction', modelId, {
                    quantized: true, // Use quantized models for better performance
                    console.log(`Successfully loaded model ${modelId} on attempt ${attempt + 1}`);
                    console.log(`Model ${modelId} loaded successfully`);
                // Check if this is a transient network error worth retrying
                const isTransientError = this.isTransientNetworkError(error);
                if (isTransientError && attempt < maxRetries - 1) {
                    // Brief message for transient errors - full details only on final failure
                    const briefReason = this.getBriefErrorReason(error);
                    console.warn(`  Connection issue: ${briefReason}`);
                    // Continue to next retry attempt
                    // Final attempt or non-transient error - log full details
                    const errorInfo = ErrorAnalyzer.analyzeError(error, 'LocalEmbedding');
                    console.error(`Failed to load model ${modelId} after ${attempt + 1} attempt(s):`, errorInfo);
        // All retries exhausted
        const errorInfo = ErrorAnalyzer.analyzeError(lastError, 'LocalEmbedding');
        const errorMessage = typeof errorInfo === 'string'
            ? errorInfo
            : (errorInfo as { message?: string }).message || 'Unknown error';
        throw new Error(`Failed to load local embedding model ${modelId}: ${errorMessage}`);
     * Determines if an error is a transient network error that should be retried.
    private isTransientNetworkError(error: unknown): boolean {
        if (!error) return false;
        const errorObj = error as { cause?: { code?: string }; code?: string; message?: string };
        // Check for common transient error codes
        const transientCodes = [
            'UND_ERR_CONNECT_TIMEOUT',
            'ECONNRESET',
            'ECONNREFUSED',
            'ETIMEDOUT',
            'ENOTFOUND',
            'EAI_AGAIN',
            'EPIPE',
            'EHOSTUNREACH',
            'ENETUNREACH'
        const errorCode = errorObj.cause?.code || errorObj.code;
        if (errorCode && transientCodes.includes(errorCode)) {
        // Check error message for network-related keywords
        const errorMessage = errorObj.message?.toLowerCase() || '';
        const transientKeywords = ['timeout', 'network', 'connection', 'fetch failed', 'socket hang up'];
        if (transientKeywords.some(keyword => errorMessage.includes(keyword))) {
     * Returns a brief, user-friendly description of the error for retry logging.
    private getBriefErrorReason(error: unknown): string {
        if (!error) return 'unknown error';
        const errorObj = error as { cause?: { code?: string; message?: string }; code?: string; message?: string };
        // Map error codes to friendly messages
        const codeMessages: Record<string, string> = {
            'UND_ERR_CONNECT_TIMEOUT': 'connection timeout',
            'ECONNRESET': 'connection reset',
            'ECONNREFUSED': 'connection refused',
            'ETIMEDOUT': 'request timeout',
            'ENOTFOUND': 'host not found',
            'EAI_AGAIN': 'DNS lookup timeout',
            'EHOSTUNREACH': 'host unreachable',
            'ENETUNREACH': 'network unreachable'
        if (errorCode && codeMessages[errorCode]) {
            return codeMessages[errorCode];
        // Fall back to error message
        if (errorObj.message) {
            return errorObj.message.substring(0, 50);
        return 'network error';
     * Embed a single text string
        if (!params.model) {
            throw new Error('Model name is required for LocalEmbedding provider');
        const modelName = params.model;
            // Get or load the pipeline
            const pipe = await this.getPipeline(modelName);
            // Generate embeddings
            const output = await pipe(params.text, {
                pooling: 'mean',
                normalize: true
            // Convert tensor to array
            const embedding = Array.from(output.data as Float32Array);
            // Calculate approximate token count (rough estimation)
            const tokenCount = Math.ceil(params.text.length / 4);
            const endTime = Date.now();
            // Only log if it took more than 1 second (indicating potential performance issues)
            if (endTime - startTime > 1000) {
                console.log(`Embedded text in ${endTime - startTime}ms using ${modelName}`);
                ModelUsage: new ModelUsage(tokenCount, 0),
            console.error('Local embedding error:', errorInfo);
     * Embed multiple text strings
            // Process texts in batches for efficiency
            const batchSize = 32;
            const vectors: number[][] = [];
            for (let i = 0; i < params.texts.length; i += batchSize) {
                const batch = params.texts.slice(i, i + batchSize);
                // Generate embeddings for batch
                const outputs = await Promise.all(
                    batch.map(text => pipe(text, {
                // Convert tensors to arrays
                for (const output of outputs) {
                    vectors.push(embedding);
                // Estimate token count
                totalTokens += batch.reduce((sum, text) => sum + Math.ceil(text.length / 4), 0);
            // Only log if it took more than 1 second per text on average
            if ((endTime - startTime) / params.texts.length > 1000) {
                console.log(`Embedded ${params.texts.length} texts in ${endTime - startTime}ms using ${modelName}`);
                vectors: vectors
     * Note: In production, this would be fetched from the database
     * through MemberJunction's AI framework
        // This method is typically not called directly when using MemberJunction
        // as the models are configured in the database
     * Override SetAdditionalSettings to handle local embedding specific settings
        // Store settings in pendingSettings to be applied when transformers loads
        if (settings.cacheDir) {
            pendingSettings.cacheDir = settings.cacheDir;
        if (settings.localURL) {
            pendingSettings.localURL = settings.localURL;
        // Handle quantized model preference
        if (settings.useQuantized !== undefined) {
            // Store for use when loading models
            this._additionalSettings.useQuantized = settings.useQuantized;
     * Clear loaded pipelines to free memory
    public clearCache(): void {
        LocalEmbedding.pipelines.clear();
        LocalEmbedding.loadingPromises.clear();
        console.log('Cleared local embedding model cache');
     * Static method to clear the shared cache
    public static clearSharedCache(): void {
        console.log('Cleared shared local embedding model cache');
     * Preload a specific model for faster first inference
    public async preloadModel(modelName: string): Promise<void> {
        await this.getPipeline(modelName);
