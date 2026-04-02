// Import pre-built MJ class registrations manifest (covers all @memberjunction/* packages)
import '@memberjunction/server-bootstrap-lite/mj-class-registrations';
// Import supplemental manifest for user-defined classes (generated at prestart with --exclude-packages @memberjunction)
import './generated/class-registrations-manifest.js';
export * from './Server.js';
export * from './config.js';export { run } from '@oclif/core';
// Export services for potential programmatic use
export { AgentService } from './services/AgentService';
export { ActionService } from './services/ActionService';
export { ConversationService } from './services/ConversationService';
export { ValidationService } from './services/ValidationService';
export { PromptService } from './services/PromptService';
export { AgentAuditService } from './services/AgentAuditService';
// Export utilities
export { OutputFormatter } from './lib/output-formatter';
export { ExecutionLogger, createExecutionLogger } from './lib/execution-logger';
export { initializeMJProvider, getConnectionPool, closeMJProvider } from './lib/mj-provider';
export { loadAIConfig } from './config';
export { AuditAnalyzer } from './lib/audit-analyzer';
export { AuditFormatter } from './lib/audit-formatter';
// Export types for AgentAuditService
export type {
  RunSummary,
  StepDetail,
  ErrorAnalysis,
  ListRunsOptions,
  StepDetailOptions,
  RunSummaryOptions,
} from './services/AgentAuditService';
// Export all agent management actions
export * from './actions/base-agent-management.action';
export * from './types/agent-management.types';
 * @module @memberjunction/agent-manager-core
 * Core interfaces and types for the MemberJunction Agent Manager system
// Export all interfaces
export * from './old/agent-definition.interface';
export * from './agent-spec-sync';
// Export agent implementations
export * from './agents/architect-agent';
export * from './agents/builder-agent';
export * from './agents/planning-designer-agent';
 * @fileoverview Main export module for the MemberJunction AI Agent framework.
 * This module exports all public APIs for the AI Agent system, including
 * base classes, type definitions, and utility functions.
export * from './agent-types/base-agent-type';
export * from './agent-types/loop-agent-response-type';
export * from './agent-types/loop-agent-prompt-params';
export * from './base-agent';
export * from './agent-types';
export * from './AgentRunner';
export * from './PayloadManager';
export * from './PayloadChangeAnalyzer';
export * from './PayloadFeedbackManager';
export * from './types/payload-operations';
export * from './AgentDataPreloader';
export * from './agent-context-injector';
export * from './memory-manager-agent';
// Re-export from ai-reranker for backward compatibility
export {
    RerankerService,
    RerankerConfiguration,
    parseRerankerConfiguration,
    RerankServiceResult,
    RerankObservabilityOptions,
    LLMReranker
} from '@memberjunction/ai-reranker'; * @fileoverview Agent type implementations for the MemberJunction AI Agent framework.
 * This module exports all available agent type implementations that extend
 * the BaseAgentType class. Each agent type defines a specific pattern for
 * agent execution and decision-making.
 * @module @memberjunction/ai-agents/agent-types
export * from './base-agent-type';
export * from './loop-agent-type';
export * from './flow-agent-type';
export * from './BaseAIEngine';
export * from './PriceUnitTypes';
export * from './AIAgentPermissionHelper';
export * from './AICredentialBindingEntityExtended';
export * from './generic/baseModel'
export * from './generic/baseLLM';
export * from './generic/baseImage';
export * from './generic/baseReranker';
export * from './generic/chat.types';
export * from './generic/classify.types';
export * from './generic/summarize.types';
export * from './generic/reranker.types';
export * from './generic/apiKeyDictionary';
export * from './generic/embed.types';
export * from './generic/baseEmbeddings';
export * from './generic/baseAudio';
export * from './generic/baseVideo';
export * from './generic/errorTypes';
export * from './generic/errorAnalyzer';export * from './prompt.types';
export * from './agent-payload-change-request';
export * from './prompt.system-placeholders';
export * from './agent-spec';
export * from './response-forms';
export * from './ui-commands';
export * from './conversation-utility';
export * from './foreach-operation';
export * from './while-operation';
export * from './AIPromptExtended';
export * from './AIPromptCategoryExtended';
export * from './AIAgentExtended';
export * from './AIModelExtended';
export * from './AIAgentRunExtended';
export * from './AIAgentRunStepExtended';
export * from './AIPromptRunEntityExtended';
export * from './AIEngine';
export * from './services/AgentEmbeddingService';
export * from './services/ActionEmbeddingService';
export * from './services/ConversationAttachmentService';
export * from './types/AgentMatchResult';
export * from './types/ActionMatchResult';
export * from './types/NoteMatchResult';
export * from './types/ExampleMatchResult';  * @fileoverview MCP Client package for MemberJunction
 * This package provides a comprehensive MCP (Model Context Protocol) client
 * implementation for MemberJunction, enabling MJ agents, actions, and services
 * to consume tools from external MCP servers.
 * @module @memberjunction/ai-mcp-client
 * import { MCPClientManager } from '@memberjunction/ai-mcp-client';
 * // Get the singleton instance
 * // Initialize (once at startup)
 * await manager.initialize(contextUser);
 * // List available tools
 * const tools = await manager.listTools('connection-id', { contextUser });
 * // Sync tools to database
 * await manager.syncTools('connection-id', { contextUser });
// Main client manager
export { MCPClientManager } from './MCPClientManager.js';
// Rate limiting utilities
export { RateLimiter, RateLimiterRegistry } from './RateLimiter.js';
// Execution logging
    ExecutionLogger,
    type MCPExecutionLogSummary,
    type MCPExecutionStats,
    type MCPToolStats
} from './ExecutionLogger.js';
// Agent integration
    AgentToolAdapter,
    createAgentToolAdapter,
    type OpenAIFunctionDefinition,
    type AnthropicToolDefinition,
    type AgentToolDefinition,
    type AgentToolResult,
    type ToolDiscoveryOptions
} from './AgentToolAdapter.js';
// Type definitions
    // Transport and auth types
    MCPServerStatus,
    MCPConnectionStatus,
    MCPToolStatus,
    // Configuration types
    MCPToolAnnotations,
    // Operation options
    // Result types
    MCPProgressInfo,
    // Internal types
    MCPExecutionLogEntry,
    // Rate limiting types
    RateLimitConfig,
    RateLimitState,
    QueuedRequest,
    // Event types
    MCPClientEventListener
// OAuth 2.1 module for MCP server authentication
    // Core classes
    OAuthManager,
    AuthServerDiscovery,
    ClientRegistration,
    TokenManager,
    PKCEGenerator,
    OAuthErrorMessages,
    OAuthAuditLogger,
    getOAuthAuditLogger,
    // Error classes
    OAuthAuthorizationRequiredError,
    OAuthReauthorizationRequiredError,
    // Type re-exports
    type AuthServerMetadata,
    type CachedAuthServerMetadata,
    type DCRRequest,
    type DCRResponse,
    type PKCEChallenge,
    type OAuthTokenResponse,
    type OAuthTokenSet,
    type OAuth2AuthCodeCredentialValues,
    type OAuthAuthorizationState,
    type OAuthAuthorizationStatus,
    type OAuthClientRegistration,
    type OAuthClientRegistrationStatus,
    type OAuthErrorResponse,
    type InitiateAuthorizationResult,
    type CompleteAuthorizationResult,
    type TokenRefreshResult,
    type OAuthConnectionStatus,
    type OAuthEventType,
    type OAuthEventData,
    type MCPServerOAuthConfig,
    // Audit logging types
    type AuthorizationInitiatedDetails,
    type AuthorizationCompletedDetails,
    type AuthorizationFailedDetails,
    type TokenRefreshDetails,
    type TokenRefreshFailedDetails,
    type CredentialsRevokedDetails
} from './oauth/index.js';
 * @fileoverview OAuth 2.1 module for MCP server authentication
 * Provides OAuth 2.1 Authorization Code flow with PKCE and Dynamic Client
 * Registration (DCR) for authenticating with OAuth-protected MCP servers.
 * - RFC 8414 Authorization Server Metadata discovery
 * - RFC 7591 Dynamic Client Registration
 * - PKCE (S256 method only) per OAuth 2.1 requirements
 * - Automatic token refresh with concurrent refresh protection
 * - Integration with MJ CredentialEngine for secure token storage
 * @module @memberjunction/ai-mcp-client/oauth
 * import { OAuthManager } from '@memberjunction/ai-mcp-client/oauth';
 * // Check for valid token or initiate flow
 * const result = await oauth.getAccessToken(connectionId, serverConfig, contextUser);
 * if (result.requiresAuthorization) {
 *     // Redirect user to result.authorizationUrl
 * } else {
 *     // Use result.accessToken for API calls
export { PKCEGenerator } from './PKCEGenerator.js';
export { AuthServerDiscovery } from './AuthServerDiscovery.js';
export { ClientRegistration } from './ClientRegistration.js';
export { TokenManager } from './TokenManager.js';
export { OAuthManager } from './OAuthManager.js';
export { OAuthErrorMessages } from './ErrorMessages.js';
export { OAuthAuditLogger, getOAuthAuditLogger } from './OAuthAuditLogger.js';
    // RFC 8414 metadata types
    CachedAuthServerMetadata,
    // RFC 7591 DCR types
    // PKCE types
    PKCEChallenge,
    // Token types
    OAuth2AuthCodeCredentialValues,
    // Authorization state types
    // Client registration types
    OAuthClientRegistrationStatus,
    // Error types
    OAuthErrorResponse,
    OAuthEventType,
    OAuthEventData,
    MCPServerOAuthConfig
    AuthorizationInitiatedDetails,
    AuthorizationCompletedDetails,
    AuthorizationFailedDetails,
    TokenRefreshDetails,
    TokenRefreshFailedDetails,
    CredentialsRevokedDetails
} from './OAuthAuditLogger.js';
 * @fileoverview CLI entry point for the MemberJunction MCP Server.
 * This module provides the command-line interface for starting and managing
 * the MCP server. It supports:
 * - Starting the server with optional tool filtering
 * - Listing available tools without starting the server
 * - Loading filter configuration from JSON files
 * ```bash
 * # Start server with all tools
 * npx @memberjunction/ai-mcp-server
 * # Start with filtered tools
 * npx @memberjunction/ai-mcp-server --include "Get_*,Run_Agent"
 * # List available tools
 * npx @memberjunction/ai-mcp-server --list-tools
 * @module @memberjunction/ai-mcp-server/cli
import yargs from 'yargs';
import { hideBin } from 'yargs/helpers';
import { initializeServer, listAvailableTools, ToolFilterOptions } from './Server.js';
/** CLI argument types */
interface CLIArguments {
    /** Comma-separated tool patterns to include */
    include?: string;
    /** Comma-separated tool patterns to exclude */
    exclude?: string;
    /** Path to JSON file with filter configuration */
    toolsFile?: string;
    /** If true, list tools and exit without starting server */
    listTools: boolean;
 * Main CLI entry point.
 * Parses command-line arguments and either lists tools or starts the server.
async function main(): Promise<void> {
    const argv = await yargs(hideBin(process.argv))
        .option('include', {
            alias: 'i',
            type: 'string',
            description: 'Comma-separated tool name patterns to include (supports wildcards: *, Get_*, *_Record)',
            coerce: (arg: string) => arg
        .option('exclude', {
            alias: 'e',
            description: 'Comma-separated tool name patterns to exclude (supports wildcards)',
        .option('tools-file', {
            alias: 'f',
            description: 'Path to a JSON file containing tool filter configuration',
        .option('list-tools', {
            alias: 'l',
            type: 'boolean',
            default: false,
            description: 'List all available tools and exit without starting the server'
        .example('$0', 'Start server with all configured tools')
        .example('$0 --include "Get_Users_Record,Run_Agent"', 'Start with only specific tools')
        .example('$0 --include "Get_*" --exclude "Get_AuditLogs_*"', 'Include pattern with exclusions')
        .example('$0 --list-tools', 'Show all available tool names')
        .example('$0 --tools-file ./my-tools.json', 'Load tool filter from file')
        .help()
        .alias('help', 'h')
        .version()
        .alias('version', 'v')
        .parse() as CLIArguments;
    // Build filter options from CLI arguments
    const filterOptions: ToolFilterOptions = {};
    // Handle --tools-file option
    if (argv.toolsFile) {
            const path = await import('path');
            const filePath = path.resolve(process.cwd(), argv.toolsFile);
            const fileContent = fs.readFileSync(filePath, 'utf-8');
            const fileConfig = JSON.parse(fileContent);
            if (fileConfig.include) {
                filterOptions.includePatterns = Array.isArray(fileConfig.include)
                    ? fileConfig.include
                    : [fileConfig.include];
            if (fileConfig.exclude) {
                filterOptions.excludePatterns = Array.isArray(fileConfig.exclude)
                    ? fileConfig.exclude
                    : [fileConfig.exclude];
            console.error(`Error reading tools file: ${error instanceof Error ? error.message : String(error)}`);
    // CLI arguments override file configuration
    if (argv.include) {
        filterOptions.includePatterns = argv.include.split(',').map(p => p.trim());
    if (argv.exclude) {
        filterOptions.excludePatterns = argv.exclude.split(',').map(p => p.trim());
    // Handle --list-tools option
    if (argv.listTools) {
        await listAvailableTools(filterOptions);
    // Start the server with filter options
    await initializeServer(filterOptions);
main().catch(error => {
    console.error('Failed to start MCP server:', error);
 * @fileoverview MCP Server OAuth Authentication Module
 * This module provides OAuth 2.1 authentication support for the MCP Server,
 * implementing RFC 9728 Protected Resource Metadata and MCP-canonical authorization.
 * - Multiple auth modes: apiKey (default), oauth, both, none
 * - Protected Resource Metadata endpoint (/.well-known/oauth-protected-resource)
 * - WWW-Authenticate headers per RFC 9728
 * - Bearer token validation using MJServer auth providers
 * - User mapping from OAuth claims to MemberJunction users
 * @module @memberjunction/ai-mcp-server/auth
// Types and interfaces
export * from './types.js';
// Configuration
export * from './OAuthConfig.js';
// Protocol helpers
export * from './WWWAuthenticate.js';
export * from './ProtectedResourceMetadata.js';
// Core authentication
export * from './TokenValidator.js';
export * from './AuthGate.js';
// OAuth Proxy - JWT Issuance
export * from './JWTIssuer.js';
// Scope-based authorization
export * from './ScopeService.js';
export * from './ScopeEvaluator.js';
// OAuth Proxy UI
export * from './ConsentPage.js';
export * from './LoginPage.js';
export * from './styles.js';
export * from './AIPromptRunner';
export * from './models/anthropic';export * from './models/azure';
export * from './models/azureEmbedding';
export * from './generic/azure.types';
export * from './config';export * from './models/bedrockLLM';
export * from './models/bedrockEmbedding';export * from './generic/BettyBot.types';
export * from './models/BettyBotLLM';
    ImageModelInfo,
 * Configuration for BFL API polling
interface BFLPollingConfig {
    /** Maximum time to wait for result in milliseconds (default: 120000 = 2 min) */
    maxWaitTime: number;
    /** Interval between status checks in milliseconds (default: 2000 = 2 sec) */
    pollInterval: number;
 * BFL API task response
interface BFLTaskResponse {
 * BFL API result response
interface BFLResultResponse {
    status: 'Ready' | 'Pending' | 'Error' | 'Request Moderated' | 'Content Moderated' | 'Task not found';
    result?: {
        sample: string; // URL to the generated image
 * Black Forest Labs FLUX Image Generator implementation.
 * Supports FLUX.2 Pro, FLUX 1.1 Pro, and other FLUX models via the BFL API.
 * BFL uses an async task-based API:
 * 1. Submit generation request -> get task ID
 * 2. Poll for result until ready
 * 3. Retrieve generated image URL
@RegisterClass(BaseImageGenerator, 'FLUXImageGenerator')
export class FLUXImageGenerator extends BaseImageGenerator {
    private _baseUrl = 'https://api.bfl.ai/v1';
    private _pollingConfig: BFLPollingConfig = {
        maxWaitTime: 120000, // 2 minutes
        pollInterval: 2000   // 2 seconds
    constructor(apiKey: string, baseUrl?: string) {
        if (baseUrl && baseUrl.length > 0) {
            this._baseUrl = baseUrl;
     * Configure polling behavior for async task completion
    public setPollingConfig(config: Partial<BFLPollingConfig>): void {
        this._pollingConfig = { ...this._pollingConfig, ...config };
     * Generate image(s) from a text prompt using FLUX models.
    public async GenerateImage(params: ImageGenerationParams): Promise<ImageGenerationResult> {
            const modelEndpoint = this.getModelEndpoint(params.model || 'flux-2-pro');
            const requestBody = this.buildGenerationRequest(params);
            // Submit generation task
            const taskResponse = await this.submitTask(modelEndpoint, requestBody);
            if (!taskResponse.id) {
                return this.createErrorResult(startTime, 'No task ID returned from BFL API');
            // Poll for result
            const resultResponse = await this.waitForResult(taskResponse.id);
            if (resultResponse.status === 'Error' || resultResponse.status === 'Request Moderated' ||
                resultResponse.status === 'Content Moderated') {
                return this.createErrorResult(startTime, `Generation failed: ${resultResponse.status}`);
            if (!resultResponse.result?.sample) {
                return this.createErrorResult(startTime, 'No image URL in result');
            // Download image and create result
            const generatedImage = await this.downloadImage(resultResponse.result.sample, 0);
            const result = this.createSuccessResult(startTime, [generatedImage]);
            // Include seed if available for reproducibility in metadata
            if (resultResponse.result.seed !== undefined) {
                result.metadata = { seed: resultResponse.result.seed };
            return this.handleError(error, startTime);
     * Edit an existing image using FLUX Fill model.
     * BFL offers FLUX.1 Fill [pro] for inpainting/outpainting operations.
    public async EditImage(params: ImageEditParams): Promise<ImageGenerationResult> {
            // Normalize input images
            const imageInput = await this.normalizeImageInput(params.image);
            const maskInput = params.mask ? await this.normalizeImageInput(params.mask) : undefined;
            // Use FLUX Fill for editing
            const endpoint = '/flux-pro-1.1-fill';
            const requestBody: Record<string, unknown> = {
                image: imageInput.base64,
                output_format: 'png'
            if (maskInput) {
                requestBody.mask = maskInput.base64;
            // Submit task
            const taskResponse = await this.submitTask(endpoint, requestBody);
            // Wait for result
            if (resultResponse.status !== 'Ready' || !resultResponse.result?.sample) {
                return this.createErrorResult(startTime, `Edit failed: ${resultResponse.status}`);
            return this.createSuccessResult(startTime, [generatedImage]);
     * Uses image-to-image generation with the original as a reference.
    public async CreateVariation(params: ImageVariationParams): Promise<ImageGenerationResult> {
            // For variations, we use the Kontext model or img2img endpoint
            const endpoint = '/flux-kontext-pro';
            const variationPrompt = params.prompt || 'Create a variation of this image with similar style and content';
                prompt: variationPrompt,
                return this.createErrorResult(startTime, `Variation failed: ${resultResponse.status}`);
     * Get available FLUX models.
    public async GetModels(): Promise<ImageModelInfo[]> {
                id: 'flux-2-pro',
                name: 'FLUX.2 Pro',
                description: 'Black Forest Labs\' most capable model. 32B parameter transformer delivering photorealistic 4MP images with exceptional detail accuracy.',
                supportedSizes: ['1024x1024', '1024x768', '768x1024', '1536x1024', '1024x1536', '2048x2048'],
                maxImages: 1, // BFL API generates one image per request
                supportsEditing: true,
                supportsVariations: true,
                supportsNegativePrompt: false,
                supportsSeed: true
                id: 'flux-1.1-pro',
                name: 'FLUX 1.1 Pro',
                description: 'Production-ready model with excellent quality and fast generation. Well-suited for high-volume workloads.',
                supportedSizes: ['1024x1024', '1024x768', '768x1024', '1536x1024', '1024x1536'],
                maxImages: 1,
                id: 'flux-dev',
                name: 'FLUX.1 [dev]',
                description: 'Open-weights 12B parameter model for development and testing.',
                supportedSizes: ['1024x1024', '1024x768', '768x1024'],
                supportsEditing: false,
                supportsVariations: false,
     * Get supported methods for this provider.
    public async GetSupportedMethods(): Promise<string[]> {
        return ['GenerateImage', 'EditImage', 'CreateVariation', 'GetModels'];
     * Get the API endpoint for a model.
    private getModelEndpoint(model: string): string {
        const modelEndpoints: Record<string, string> = {
            'flux-2-pro': '/flux-pro-2',
            'flux-1.1-pro': '/flux-pro-1.1',
            'flux-pro': '/flux-pro',
            'flux-dev': '/flux-dev',
            'flux-schnell': '/flux-schnell',
            'flux-ultra': '/flux-ultra'
        return modelEndpoints[model.toLowerCase()] || '/flux-pro-1.1';
     * Build the generation request body.
    private buildGenerationRequest(params: ImageGenerationParams): Record<string, unknown> {
        const request: Record<string, unknown> = {
        // Handle size/dimensions
        if (params.size) {
            const [width, height] = params.size.split('x').map(Number);
            if (!isNaN(width) && !isNaN(height)) {
                request.width = width;
                request.height = height;
            request.width = 1024;
            request.height = 1024;
        // Aspect ratio (if dimensions not set)
        if (params.aspectRatio && !params.size) {
            request.aspect_ratio = params.aspectRatio;
        // Seed for reproducibility
        if (params.seed !== undefined) {
            request.seed = params.seed;
        // Steps (if supported by model)
        if (params.providerOptions?.steps) {
            request.steps = params.providerOptions.steps;
        // Guidance scale
        if (params.providerOptions?.guidance) {
            request.guidance = params.providerOptions.guidance;
     * Submit a task to the BFL API.
    private async submitTask(endpoint: string, body: Record<string, unknown>): Promise<BFLTaskResponse> {
        const response = await fetch(`${this._baseUrl}${endpoint}`, {
                'X-Key': this.apiKey
            body: JSON.stringify(body)
            const errorText = await response.text();
            throw new Error(`BFL API error (${response.status}): ${errorText}`);
        return await response.json() as BFLTaskResponse;
     * Poll for task completion.
    private async waitForResult(taskId: string): Promise<BFLResultResponse> {
        while (Date.now() - startTime < this._pollingConfig.maxWaitTime) {
            const response = await fetch(`${this._baseUrl}/get_result?id=${taskId}`, {
                throw new Error(`BFL API error checking result: ${response.status}`);
            const result = await response.json() as BFLResultResponse;
            if (result.status === 'Ready' || result.status === 'Error' ||
                result.status === 'Request Moderated' || result.status === 'Content Moderated' ||
                result.status === 'Task not found') {
            // Wait before next poll
            await this.sleep(this._pollingConfig.pollInterval);
        throw new Error(`Timeout waiting for result after ${this._pollingConfig.maxWaitTime}ms`);
     * Download an image from URL and convert to GeneratedImage.
    private async downloadImage(url: string, index: number): Promise<GeneratedImage> {
        const response = await fetch(url);
            throw new Error(`Failed to download image: ${response.status}`);
        const buffer = Buffer.from(arrayBuffer);
        const base64 = buffer.toString('base64');
        const generatedImage = new GeneratedImage();
        generatedImage.data = buffer;
        generatedImage.base64 = base64;
        generatedImage.url = url;
        generatedImage.format = 'png';
        generatedImage.index = index;
        return generatedImage;
     * Sleep for specified milliseconds.
     * Handle errors and create error result.
    private handleError(error: unknown, startTime: Date): ImageGenerationResult {
        const errorInfo = ErrorAnalyzer.analyzeError(error, 'Black Forest Labs');
        console.error('FLUX Image Generation error:', errorMessage, errorInfo);
        const result = this.createErrorResult(startTime, errorMessage);
        result.errorInfo = errorInfo;
 * AI Provider Bundle
 * This package previously contained tree-shaking prevention functions for all
 * standard MemberJunction AI providers. Tree-shaking prevention is now handled
 * by the manifest system, making this package effectively a no-op.
 * The package is retained for backward compatibility with existing imports.
export * from './models/cerebras'; * @memberjunction/ai-cohere
 * Cohere AI provider for MemberJunction.
 * Currently provides reranking capabilities using Cohere's Rerank API.
 * Supported models:
 * - rerank-v3.5: Latest English reranker with best accuracy
 * - rerank-multilingual-v3.0: Supports 100+ languages
 * API Key:
 * Set via environment variable: AI_VENDOR_API_KEY__COHERELLM
 * Usage:
 * import { CohereReranker } from '@memberjunction/ai-cohere';
 * // Create instance via ClassFactory
 * const reranker = ClassFactory.CreateInstance<BaseReranker>(
 *     BaseReranker,
 *     'CohereLLM',
 *     apiKey,
 *     'rerank-v3.5'
 * const response = await reranker.Rerank({
 *     query: 'What is the capital of France?',
 *     documents: [
 *         { id: '1', text: 'Paris is the capital of France.' },
 *         { id: '2', text: 'London is in England.' }
 *     topK: 5
    CohereReranker,
    createCohereReranker
} from './models/CohereReranker';
import { BaseAudioGenerator, TextToSpeechParams, SpeechResult, SpeechToTextParams, VoiceInfo, AudioModel, PronounciationDictionary, ErrorAnalyzer } from "@memberjunction/ai";
import { ElevenLabsClient } from "@elevenlabs/elevenlabs-js";
@RegisterClass(BaseAudioGenerator, "ElevenLabsAudioGenerator")
export class ElevenLabsAudioGenerator extends BaseAudioGenerator {
    private _elevenLabs: ElevenLabsClient;
        this._elevenLabs = new ElevenLabsClient({apiKey: apiKey});
    public async CreateSpeech(params: TextToSpeechParams): Promise<SpeechResult> {
        const speechResult = new SpeechResult();
            // New API uses textToSpeech.convert instead of generate
            const audioStream = await this._elevenLabs.textToSpeech.convert(
                params.voice,
                    text: params.text,
                    modelId: params.model_id,
                    voiceSettings: params.voice_settings,
                    applyTextNormalization: params.apply_text_normalization,
                    pronunciationDictionaryLocators: params.pronunciation_dictionary_locators,
            // Convert ReadableStream to Buffer
            const chunks: Uint8Array[] = [];
            const reader = audioStream.getReader();
                while (true) {
                    const { done, value } = await reader.read();
                    if (done) break;
                    if (value) chunks.push(value);
                reader.releaseLock();
            const audioBuffer = Buffer.concat(chunks as Uint8Array[]);
            speechResult.data = audioBuffer;
            speechResult.content = audioBuffer.toString('base64'); // Convert to base64 string
            speechResult.success = true;
            const errorInfo = ErrorAnalyzer.analyzeError(error, 'ElevenLabs');
            speechResult.success = false;
            speechResult.errorMessage = error?.message || 'Unknown error occurred';
            console.error('ElevenLabs CreateSpeech error:', error, errorInfo);
        return speechResult;
    public async SpeechToText(params: SpeechToTextParams): Promise<SpeechResult> {
    public async GetVoices(): Promise<VoiceInfo[]> {
        const result: VoiceInfo[] = [];
            const voices = await this._elevenLabs.voices.getAll();
            for (const voice of voices.voices) {
                const voiceInfo = new VoiceInfo();
                voiceInfo.id = voice.voiceId;  // Changed from voice_id to voiceId (camelCase)
                voiceInfo.name = voice.name;
                voiceInfo.description = voice.labels?.description;
                voiceInfo.labels = [];
                if (voice.labels) {
                    for (const label in voice.labels) {
                        voiceInfo.labels.push({key: label, value: voice.labels[label]});
                voiceInfo.category = voice.category;
                voiceInfo.previewUrl = voice.previewUrl;  // Changed from preview_url to previewUrl (camelCase)
                result.push(voiceInfo);
            console.error('ElevenLabs GetVoices error:', errorInfo);
    public async GetModels(): Promise<AudioModel[]> {
        const result: AudioModel[] = [];
            // Changed from getAll() to list()
            const models = await this._elevenLabs.models.list();
                const audioModel = new AudioModel();
                // Handle both camelCase and snake_case property names
                audioModel.id = model.modelId || (model as any).model_id;
                audioModel.name = model.name;
                audioModel.supportsTextToSpeech = model.canDoTextToSpeech ?? (model as any).can_do_text_to_speech ?? false;
                audioModel.supportsVoiceConversion = model.canDoVoiceConversion ?? (model as any).can_do_voice_conversion ?? false;
                audioModel.supportsStyle = model.canUseStyle ?? (model as any).can_use_style ?? false;
                audioModel.supportsSpeakerBoost = model.canUseSpeakerBoost ?? (model as any).can_use_speaker_boost ?? false;
                audioModel.supportsFineTuning = model.canBeFinetuned ?? (model as any).can_be_finetuned ?? false;
                audioModel.languages = [];
                if (model.languages && Array.isArray(model.languages)) {
                    for (const language of model.languages) {
                        audioModel.languages.push({
                            id: (language as any).languageId || (language as any).language_id,
                            name: language.name
                result.push(audioModel);
            console.error('ElevenLabs GetModels error:', errorInfo);
     * Retrieves all pronunciation dictionaries from ElevenLabs API.
     * Implements automatic pagination to fetch all available dictionaries across multiple pages.
     * @returns Promise resolving to array of all pronunciation dictionaries
    public async GetPronounciationDictionaries(): Promise<PronounciationDictionary[]> {
        const result: PronounciationDictionary[] = [];
            let hasMore = true;
            let cursor: string | undefined = undefined;
            // Fetch all pages using pagination
            while (hasMore) {
                // Request with cursor for subsequent pages, max page size for efficiency
                const requestParams = cursor ? { cursor, pageSize: 100 } : { pageSize: 100 };
                const response = await this._elevenLabs.pronunciationDictionaries.list(requestParams);
                const dictionariesList = (response as any).pronunciationDictionaries ||
                                         (response as any).pronunciation_dictionaries ||
                                         [];
                // Convert API response to PronounciationDictionary objects
                for (const pronounciationDictionary of dictionariesList) {
                    const dictionary = new PronounciationDictionary();
                    dictionary.id = pronounciationDictionary.id;
                    dictionary.name = pronounciationDictionary.name;
                    dictionary.description = pronounciationDictionary.description;
                    // Handle both camelCase and snake_case
                    dictionary.latestVersionId = pronounciationDictionary.latestVersionId ||
                                               pronounciationDictionary.latest_version_id;
                    dictionary.createdBy = pronounciationDictionary.createdBy ||
                                          pronounciationDictionary.created_by;
                    dictionary.creationTimeStamp = pronounciationDictionary.creationTimeUnix ||
                                                  pronounciationDictionary.creation_time_unix;
                    result.push(dictionary);
                // Check if there are more pages to fetch
                hasMore = (response as any).hasMore ?? false;
                cursor = (response as any).nextCursor;
            console.error('ElevenLabs GetPronounciationDictionaries error:', errorInfo);
    public async GetSupportedMethods() {
        return ["CreateSpeech", "GetVoices", "GetModels", "GetPronounciationDictionaries"];
export * from './models/fireworks';
// Google Gemini Import
import { GoogleGenAI, Content, Part, Blob} from "@google/genai";
// MJ stuff
import { BaseLLM, ChatMessage, ChatParams, ChatResult, SummarizeParams, SummarizeResult, StreamingChatCallbacks, ChatMessageContent, ModelUsage, ErrorAnalyzer } from "@memberjunction/ai";
@RegisterClass(BaseLLM, "GeminiLLM")
export class GeminiLLM extends BaseLLM {
        // Note: We don't initialize the client here to allow subclasses to set up first
        // Initialization happens lazily on first use via ensureGeminiClient()
     * Factory method to create the GoogleGenAI client instance
     * Subclasses can override this to provide custom configuration
     * @returns Promise that resolves to a configured GoogleGenAI client
     * Ensure the Gemini client is initialized before use
     * This method should be called at the start of any method that uses the client
     * Read only getter method to get the Gemini client instance
     * Note: This is async now because the client may not be initialized yet
    public get GeminiClient(): GoogleGenAI {
        if (!this._gemini) {
            throw new Error('Gemini client not initialized. Ensure async initialization is complete before accessing GeminiClient.');
     * Convert MJ effort level (1-100) to Gemini thinkingBudget (0-24576)
     * Mapping strategy:
     * - 1-33 (low): 1024-4096 tokens
     * - 34-66 (medium): 4097-12288 tokens
     * - 67-100 (high): 12289-24576 tokens
     * - undefined: No thinkingConfig (Gemini default ~8192)
     * Model-specific behavior:
     * - Gemini 2.5 Flash/Flash-Lite: Can disable thinking with budget=0
     * - Gemini 2.5 Pro: Cannot disable thinking, minimum ~1024
     * - Gemini 3 models: Use thinkingLevel instead (future consideration)
     * @param effortLevel - MJ normalized effort level (1-100) as string or number
     * @param modelName - The Gemini model name to check capabilities
     * @returns thinkingBudget value or undefined for default behavior
    private getThinkingBudget(effortLevel: string | number | undefined, modelName: string): number | undefined {
        if (effortLevel === undefined || effortLevel === null || effortLevel === '') {
            return undefined; // Use Gemini's default behavior (auto, up to ~8192)
        // Parse string to number if needed
        const numericLevel = typeof effortLevel === 'string' ? parseInt(effortLevel, 10) : effortLevel;
        if (isNaN(numericLevel)) {
            return undefined; // Invalid effort level, use default
        // Clamp to valid range
        const level = Math.max(1, Math.min(100, numericLevel));
        // Check model capabilities for thinking
        const lowerModel = modelName.toLowerCase();
        const isFlashModel = lowerModel.includes('flash');
        const isProModel = lowerModel.includes('pro') && !isFlashModel;
        // Very low effort (1-5) - try to disable thinking on Flash models
        if (level <= 5 && isFlashModel) {
            return 0; // Disable thinking (only works on Flash/Flash-Lite)
        // For Pro models, minimum effective budget is ~1024
        // For Flash models with effort > 5, use normal scaling
        if (level <= 33) {
            // Low: linear scale from 1024 to 4096
            return Math.round(1024 + ((level - 1) / 32) * (4096 - 1024));
        } else if (level <= 66) {
            // Medium: linear scale from 4097 to 12288
            return Math.round(4097 + ((level - 34) / 32) * (12288 - 4097));
            // High: linear scale from 12289 to 24576
            return Math.round(12289 + ((level - 67) / 33) * (24576 - 12289));
     * Check if a model supports thinking configuration
     * Thinking is supported on Gemini 2.5+ models
    private supportsThinking(modelName: string): boolean {
        // Gemini 2.5 and later support thinking
        return lowerModel.includes('2.5') ||
               lowerModel.includes('gemini-3') ||
               lowerModel.includes('gemini-exp');
     * Gemini supports streaming
    protected geminiMessageSpacing(messages: Content[]): Content[] {
        // This method ensures messages alternate between user and model roles
        // by combining consecutive messages with the same role
        if (messages.length === 0) {
        const result: Content[] = [];
        let currentMessage: Content | null = null;
            if (currentMessage === null) {
                // First message - start accumulating
                currentMessage = { role: message.role, parts: [...message.parts] };
            } else if (currentMessage.role === message.role) {
                // Same role as current - combine the parts
                currentMessage.parts.push(...message.parts);
                // Different role - push current and start new
                result.push(currentMessage);
        // Push the last accumulated message
        if (currentMessage !== null) {
     * Implementation of non-streaming chat completion for Gemini
            // For text-only input, use the gemini-pro model
            const modelName = params.model || "gemini-pro";
            // Filter out system messages and extract system instruction content
            const noSystemMessages = params.messages.filter(m => m.role !== 'system');
            const sysPrompts = params.messages.filter(m => m.role === 'system');
            const systemInstructionText = sysPrompts.length > 0
                ? sysPrompts.map(m => typeof m.content === 'string' ? m.content : m.content.map(v => v.content).join('\n')).join('\n\n')
            // Convert all non-system messages and apply role alternation
            const convertedMessages = noSystemMessages.map(m => GeminiLLM.MapMJMessageToGeminiHistoryEntry(m));
            const tempMessages = this.geminiMessageSpacing(convertedMessages);
            // Split: all but last message go in history, last message gets system instructions prepended
            const history = tempMessages.slice(0, -1);
            const lastMessage = tempMessages.length > 0 ? tempMessages[tempMessages.length - 1] : null;
            // Prepare the final message with system instructions prepended to the last user message
            let finalMessageParts: Part[] = [];
            if (systemInstructionText) {
                finalMessageParts.push({ text: systemInstructionText });
            if (lastMessage) {
                finalMessageParts.push(...lastMessage.parts);
            // If no messages at all, send empty
            if (finalMessageParts.length === 0) {
                finalMessageParts = [{ text: '' }];
            // Create the model and then chat
            const modelOptions: Record<string, any> = {
                temperature: params.temperature || 0.5,
                responseType: params.responseFormat,
                modelOptions.top_p = params.topP;
                modelOptions.top_k = params.topK;
                modelOptions.stop_sequences = params.stopSequences;
                modelOptions.seed = params.seed;
            // Gemini doesn't support these parameters - warn if provided
                console.warn('Gemini provider does not support frequencyPenalty parameter, ignoring');
                console.warn('Gemini provider does not support presencePenalty parameter, ignoring');
                console.warn('Gemini provider does not support minP parameter, ignoring');
            // Build thinking configuration based on effort level and model capabilities
            const thinkingBudget = this.getThinkingBudget(params.effortLevel, modelName);
            const useThinking = this.supportsThinking(modelName) && thinkingBudget !== undefined;
            // Create chat config - only include thinkingConfig if model supports it and effortLevel is specified
            const chatConfig: Record<string, unknown> = {};
            this.setThinkingConfig(chatConfig, useThinking, params.effortLevel, modelName, thinkingBudget);
            // Ensure Gemini client is initialized
            // Create chat with history (all messages except the last)
            // Don't use systemInstruction parameter - we're bundling it with the user message
            const chat = client.chats.create({
                config: Object.keys(chatConfig).length > 0 ? chatConfig : undefined,
                history: history
            // Send the last message with system instructions prepended
            const result = await chat.sendMessage({
                message: finalMessageParts,
                config: modelOptions
            // Check for blocked response or empty candidates
            if (!result.candidates || result.candidates.length === 0) {
                // Response was blocked or failed - check promptFeedback for details
                const blockReason = (result as any).promptFeedback?.blockReason;
                const safetyRatings = (result as any).promptFeedback?.safetyRatings;
                let errorMessage = 'No output received from model';
                if (blockReason) {
                    errorMessage += `: Blocked (${blockReason})`;
                    if (safetyRatings && safetyRatings.length > 0) {
                        const blockedCategories = safetyRatings
                            .filter((r: any) => r.blocked)
                            .map((r: any) => r.category)
                        if (blockedCategories) {
                            errorMessage += ` - Categories: ${blockedCategories}`;
                } else if (result.usageMetadata?.candidatesTokenCount === 0) {
                    errorMessage += ': Model returned 0 completion tokens (possible timeout or internal error)';
            // Check finishReason for blocking or errors
            const candidate = result.candidates[0];
            const finishReason = (candidate as any).finishReason;
            // Detect problematic finishReasons that indicate the response should not be used
            if (finishReason && ['SAFETY', 'RECITATION', 'BLOCKLIST', 'PROHIBITED_CONTENT', 'MODEL_ARMOR'].includes(finishReason)) {
                const safetyRatings = (candidate as any).safetyRatings || [];
                    .filter((r: any) => r.blocked || (r.probability && ['HIGH', 'MEDIUM'].includes(r.probability)))
                    .map((r: any) => `${r.category}(${r.probability})`)
                throw new Error(`Content blocked by model: ${finishReason}${blockedCategories ? ` - ${blockedCategories}` : ''}`);
            const rawContent = candidate.content?.parts?.find(part => part.text && !part.thought)?.text || '';
            const thinking = candidate.content?.parts?.find(part => part.thought)?.text || '';
            // Check if we got empty content despite no blocking
            if (!rawContent && !thinking) {
                const usage = result.usageMetadata;
                    `No output received from model (finishReason: ${finishReason || 'none'}, ` +
                    `promptTokens: ${usage?.promptTokenCount || 0}, ` +
                    `completionTokens: ${usage?.candidatesTokenCount || 0})`
            // Extract thinking content if present
            let content: string = rawContent.trim();
                // extract thinking content
                // remove thinking content from main content
                content = content.substring(thinkEnd + '</think>'.length).trim();
                thinkingContent = thinking;
                        finish_reason: finishReason || "completed",
                    usage: new ModelUsage(
                        result.usageMetadata?.promptTokenCount || 0,
                        result.usageMetadata?.candidatesTokenCount || 0
                statusText: e && e.message ? e.message : "Error",
                timeElapsed: 0,
                    usage: new ModelUsage(0, 0) // Gemini doesn't provide detailed token usage
                errorMessage: e.message,
                exception: e,
                errorInfo: ErrorAnalyzer.analyzeError(e, 'Gemini')
    private setThinkingConfig(chatConfig: any, useThinking: boolean, effortLevel: string | undefined, modelName: string, thinkingBudget: number) {
        // this is a hack, need a cleaner way of doing this
        const gemini3AndAbove: boolean = modelName?.toLowerCase().includes("-3") ||
                                         modelName?.toLowerCase().includes("-4") ||
                                         modelName?.toLowerCase().includes("-5") ||
                                         modelName?.toLowerCase().includes("-6") ||
                                         modelName?.toLowerCase().includes("-7");
        if (useThinking) {
            if (gemini3AndAbove) {
                // no budget used, we map the effortLevel to 4 buckets
                let geminiLevel = undefined;
                let numericLevel = typeof effortLevel === 'string' ? parseInt(effortLevel, 10) : effortLevel;
                    numericLevel = 0;
                if (!numericLevel || numericLevel <= 1) {
                    geminiLevel = "MINIMAL" // if we don't have thinking setup and we're dealing with Gemini 3 series models, set thinking level to minimal
                else if(numericLevel <= 33) {
                    geminiLevel = "LOW" 
                else if (numericLevel <= 66) {
                    geminiLevel = "MEDIUM" 
                    geminiLevel = "HIGH" 
                chatConfig.thinkingConfig = {
                    thinkingLevel: geminiLevel
                    includeThoughts: true,
                    thinkingBudget: thinkingBudget
                    thinkingLevel: "MINIMAL" // if we don't have thinking setup and we're dealing with Gemini 3 series models, set thinking level to minimal
     * Create a streaming request for Gemini
        // Send the last message with system instructions prepended (streaming)
        const streamResult = await chat.sendMessageStream({
        // Return the stream for the for-await loop to work
        return streamResult;
     * Process a streaming chunk from Gemini
        // Extract text from the chunk with the new SDK
        if (chunk.candidates && 
            chunk.candidates[0] && 
            chunk.candidates[0].content && 
            chunk.candidates[0].content[0] && 
            chunk.candidates[0].content[0].parts) {
            // Find the text part
            const textPart = chunk.candidates[0].content[0].parts.find((part: any) => part.text);
            if (textPart?.text) {
                const rawContent = textPart.text;
        // Check for finish reason if available
        if (chunk.candidates && chunk.candidates[0] && chunk.candidates[0].finishReason) {
            finishReason = chunk.candidates[0].finishReason;
        // Extract usage from chunk if available (appears on final chunk)
        if (chunk.usageMetadata) {
                chunk.usageMetadata.promptTokenCount || 0,
                chunk.usageMetadata.candidatesTokenCount || 0
            const endIndex = state.pendingContent.indexOf('</think>');
                // Keep remaining content after </think> for output
                state.pendingContent = state.pendingContent.substring(endIndex + '</think>'.length);
            const startIndex = state.pendingContent.indexOf('<think>');
                    state.pendingContent = state.pendingContent.substring('<think>'.length);
                    // There's content before thinking block - this shouldn't happen
                    // with Gemini models, but handle it just in case
                    state.pendingContent.endsWith('<thin')) {
     * Create the final response from streaming results for Gemini
        if (lastChunk?.candidates && lastChunk.candidates.length > 0 && lastChunk.candidates[0].finishReason) {
            finishReason = lastChunk.candidates[0].finishReason;
        const hasContent = (accumulatedContent && accumulatedContent.trim().length > 0) || thinkingContent.length > 0;
        // Check for problematic finishReasons in streaming responses
            const safetyRatings = lastChunk?.candidates?.[0]?.safetyRatings || [];
            // Create error result for blocked content
            errorResult.statusText = 'Content blocked';
            errorResult.errorMessage = `Content blocked by model: ${finishReason}${blockedCategories ? ` - ${blockedCategories}` : ''}`;
            errorResult.exception = new Error(errorResult.errorMessage);
            errorResult.errorInfo = ErrorAnalyzer.analyzeError(errorResult.exception, 'Gemini');
        // Check for empty response (no content received)
        if (!hasContent) {
            const usageMetadata = usage || lastChunk?.usageMetadata;
            errorResult.statusText = 'No output received';
            errorResult.errorMessage = `No output received from model in streaming response (finishReason: ${finishReason || 'none'}, ` +
                `promptTokens: ${usageMetadata?.promptTokenCount || 0}, ` +
                `completionTokens: ${usageMetadata?.candidatesTokenCount || 0})`;
                    content: accumulatedContent ? accumulatedContent : '',
    SummarizeText(params: SummarizeParams): Promise<SummarizeResult> {
    ClassifyText(params: any): Promise<any> {
    public static MapMJContentToGeminiParts(content: ChatMessageContent): Array<Part> {
        const parts: Array<Part> = [];
            for (const part of content) {
                if (part.type === 'text') {
                    parts.push({text: part.content});
                    // use the inlineData property which expects a Blob property which consists of data and mimeType
                    const blob: Blob = {
                        data: part.content
                    switch (part.type) {
                        case 'image_url':
                            blob.mimeType = 'image/jpeg';
                        case 'audio_url':
                            blob.mimeType = 'audio/mpeg';
                        case 'video_url':
                            blob.mimeType = 'video/mp4';
                        case 'file_url':
                            blob.mimeType = 'application/octet-stream';
                    parts.push({inlineData: blob});
            // we know that message.content is a string
            parts.push({text: content});
    public static MapMJMessageToGeminiHistoryEntry(message: ChatMessage): Content {
            role: message.role === 'assistant' ? 'model' : 'user', // google calls all messages other than the replies from the model 'user' which would include the system prompt
            parts: GeminiLLM.MapMJContentToGeminiParts(message.content)
// Export image generation
export * from './geminiImage';export * from './models/groq'; import { AvatarInfo, AvatarVideoParams, BaseVideoGenerator, VideoResult, ErrorAnalyzer } from "@memberjunction/ai";
import axios from "axios";
@RegisterClass(BaseVideoGenerator, "HeyGenVideoGenerator")
export class HeyGenVideoGenerator extends BaseVideoGenerator {
    private _generateUrl: string = "https://api.heygen.com/v2/video/generate";
    private _avatarsUrl: string = "https://api.heygen.com/v2/avatars";
    public async CreateAvatarVideo(params: AvatarVideoParams): Promise<VideoResult> {
        const videoResult = new VideoResult();
            const response = await axios.post(
                this._generateUrl, {
                    video_inputs: [
                            character: {
                                type: 'avatar',
                                avatar_id: params.avatarId,
                                scale: params.scale,
                                avatar_style: params.avatarStyle,
                                offset: {x: params.offsetX, y: params.offsetY},
                            voice: {
                                type: 'audio',
                                audio_asset_id: params.audioAssetId,
                            background: {
                                image_asset_id: params.imageAssetId,
                    dimension: {
                        width: params.outputWidth,
                        height: params.outputHeight,
                    headers: { Accept: 'application/json', 'X-Api-Key': this.apiKey }
            videoResult.videoId = response.data.data.video_id;
            videoResult.success = true;
            const errorInfo = ErrorAnalyzer.analyzeError(error, 'HeyGen');
            videoResult.success = false;
            videoResult.errorMessage = error?.message || 'Unknown error occurred';
            console.error('HeyGen CreateAvatarVideo error:', error, errorInfo);
        return videoResult;
    public async CreateVideoTranslation(params: any): Promise<VideoResult> {
    public async GetAvatars(): Promise<AvatarInfo[]> {
        const result: AvatarInfo[] = [];
            const response = await axios.get(this._avatarsUrl, {
            for (const avatar of response.data.data.avatars) {
                const avatarInfo = new AvatarInfo();
                avatarInfo.id = avatar.avatar_id;
                avatarInfo.name = avatar.avatar_name;
                avatarInfo.gender = avatar.gender
                avatarInfo.previewImageUrl = avatar.preview_image_url;
                avatarInfo.previewVideoUrl = avatar.preview_video_url;
                result.push(avatarInfo);
            console.error('HeyGen GetAvatars error:', errorInfo);
        return ["CreateAvatarVideo", "CreateVideoTranslation", "GetAvatars"];
export * from './models/lm-studio'; export * from './models/localEmbedding';
export * from './models/mistral';
export * from './models/mistralEmbedding';export * from './models/ollama-llm';
export * from './models/ollama-embeddings';
export * from './models/openAI';
export * from './models/openAIEmbedding';
export * from './models/embeddingModels.types';
export * from './models/tts';
export * from './models/openAIImage';export * from './models/openRouter'; export * from './provider';export * from "./models/PineconeDatabase";export * from './models/vertexLLM';
// Note: vertexEmbedding.ts removed - it used deprecated @google-cloud/vertexai SDK
// and only returned mock embeddings. The new @google/genai SDK doesn't yet support
// embeddings. Will be re-implemented when embedding support is added to @google/genai.
export * from './models/zhipu';
export * from './models/x.ai'; export * from './Engine';
export * from './ProviderBase';
export * from './generic/types'; * @memberjunction/ai-reranker
 * AI Reranker Service for MemberJunction's two-stage retrieval system.
 * Provides centralized reranker management and LLM-based reranking capabilities.
 * Key Components:
 * - RerankerService: Singleton service for managing reranker instances
 * - LLMReranker: LLM-based reranker using AI Prompts system
 * - RerankerConfiguration: Configuration types for agent-level settings
 * import {
 *     RerankerService,
 *     RerankerConfiguration,
 *     parseRerankerConfiguration
 * } from '@memberjunction/ai-reranker';
 * // Parse configuration from agent
 * const config = parseRerankerConfiguration(agent.RerankerConfiguration);
 * // Rerank notes if enabled
 *     const result = await RerankerService.Instance.rerankNotes(
    parseRerankerConfiguration
} from './config.types';
// Service
    RerankObservabilityOptions
} from './RerankerService';
// LLM Reranker
    LLMReranker,
    createLLMReranker
} from './LLMReranker';
export * from './generic/IVectorDatabase';
export * from './generic/IVectorIndex';
export * from './generic/IEmbedding';
export * from './models/VectorBase';
export * from './generic/VectorCore.types'; export * from './generic/record';
export * from './generic/VectorDBBase';
export * from './generic/query.types';export * from './generic/vectorSyncBase';
export * from './models/entitySyncConfig';
export * from './duplicateRecordDetector'; * @module @memberjunction/ai-vectors-memory
 * @description In-memory vector similarity search service for MemberJunction
  SimpleVectorService,
  VectorEntry,
  VectorSearchResult,
  DistanceMetric,
  ClusterResult
} from './models/SimpleVectorService';export * from './generic/vectorSync.types';
export * from './generic/entitySyncConfig.types';
export * from './models/entityVectorSync';
export * from './generic/EntityDocumentTemplateParser';
 * MemberJunction API Keys Engine Base Package
 * This package provides the base engine for API Keys metadata caching.
 * It can be used on both client and server for cached access to:
 * - API Scopes
 * - API Applications
 * - API Application Scopes
 * - API Key Applications
 * - API Key Scopes
 * For server-side authorization features (key generation, validation, usage logging),
 * use the @memberjunction/api-keys package instead.
export { APIKeysEngineBase, APIScopeUIConfig, parseAPIScopeUIConfig } from './APIKeysEngineBase';
 * MemberJunction API Keys Authorization Package
 * This package provides the complete API Key management and authorization system:
 * - API key generation, creation, validation, and revocation
 * - Hierarchical scopes with pattern-based access control
 * - Application-level scope ceilings
 * **Architecture:**
 * - `APIKeysEngineBase` (from @memberjunction/api-keys-base) - Metadata caching, usable anywhere
 * - `APIKeyEngine` (this package) - Server-side operations, wraps Base engine
 * **Note**: This package requires Node.js and the crypto module. It is intended
 * for server-side use only. For client-side access to cached metadata, use
 * the @memberjunction/api-keys-base package directly.
// Re-export base engine for convenience
export { APIKeysEngineBase } from '@memberjunction/api-keys-base';
// Main engine and singleton
    APIKeyEngine,
    GetAPIKeyEngine,
    ResetAPIKeyEngine,
    KeyHashValidationResult
} from './APIKeyEngine';
// Scope evaluation
export { ScopeEvaluator } from './ScopeEvaluator';
// Pattern matching
export { PatternMatcher, PatternMatchResult } from './PatternMatcher';
// Usage logging
export { UsageLogger } from './UsageLogger';
// Interfaces - API Key Management
// Interfaces - Authorization
    ScopeRuleMatch,
    ApplicationScopeRule,
    KeyScopeRule
// Interfaces - Logging and Configuration
    UsageLogEntry,
    APIKeyEngineConfig
export * from './accounts';
export * from './contacts';
// ApolloEnrichContact this Action would use the www.apollo.io enrichment service and enrich a "contact" type of record. The parameters to this Action would be:
// EntityName - entity in question that contacts "contact" types of records
// EmailField - string - field name in the target entity that contains the email to be used for lookups
// FirstNameField - string 
// LastNameField - string
// AccountNameField - string
// EnrichmentFieldMapping - JSON - this JSON string would contain mappings for the wide array of enrichment outputs. The idea is that the outputs could include a wide array of things and this would map any of the scalar values that Apollo.io provides back to entity field names in the target entity
// EducationHistoryEntityName - string - optional - this would contain the name of the entity that will be used to stored the retrieved education history for each contact
// EmploymentHistoryEntityName - string - optional - would contain the name of the entity that will be used to store the retrieved employment history for each contact
// ApolloEnrichAccount - same idea and pattern as above, but would be for Account style entities
// Would appreciate feedback on (a) the approach to how we package this stuff and (b) the approach to params noted above.export * from './ActionEngine-Base';
export * from './ActionEntity-Extended';
export * from './EntityActionEngine-Base';
export * from './EntityActionEntity-Extended';// Export all accounting actions
export * from './base/base-accounting-action';
// QuickBooks base
export * from './providers/quickbooks/quickbooks-base.action';
// QuickBooks actions - export specific items to avoid conflicts
export { GetQuickBooksGLCodesAction, GLCode } from './providers/quickbooks/actions/get-gl-codes.action';
export { GetQuickBooksTransactionsAction, Transaction, TransactionLine } from './providers/quickbooks/actions/get-transactions.action';
export { GetQuickBooksAccountBalancesAction, AccountBalance } from './providers/quickbooks/actions/get-account-balances.action';
// export { GetQuickBooksInvoicesAction, Invoice, InvoiceLine, Address as InvoiceAddress } from './providers/quickbooks/actions/get-invoices.action';
// export { GetQuickBooksBillsAction, Bill, BillLine, Address as BillAddress } from './providers/quickbooks/actions/get-bills.action';
// export { GetQuickBooksCustomersAction, Customer, Address as CustomerAddress } from './providers/quickbooks/actions/get-customers.action';
// export { GetQuickBooksVendorsAction, Vendor, Address as VendorAddress } from './providers/quickbooks/actions/get-vendors.action';
export { CreateQuickBooksJournalEntryAction, JournalEntryLine } from './providers/quickbooks/actions/create-journal-entry.action';
// Business Central base
export * from './providers/business-central/business-central-base.action';
// Business Central actions
export { GetBusinessCentralGLAccountsAction, BCGLAccount } from './providers/business-central/actions/get-gl-accounts.action';
export { GetBusinessCentralGeneralLedgerEntriesAction, BCGeneralLedgerEntry, BCDimensionSetLine } from './providers/business-central/actions/get-general-ledger-entries.action';
export { GetBusinessCentralCustomersAction, BCCustomer, BCAddress } from './providers/business-central/actions/get-customers.action';
export { GetBusinessCentralSalesInvoicesAction, BCSalesInvoice, BCSalesInvoiceLine } from './providers/business-central/actions/get-sales-invoices.action';
// NetSuite actions
// export * from './providers/netsuite/actions/get-gl-codes.action';
// Sage Intacct actions
// export * from './providers/sage-intacct/actions/get-gl-codes.action';
// Common actions
// export * from './common/get-account-balances.action';
// export * from './common/validate-journal-entry.action'; * CRM Integration Actions
 * This module exports all CRM provider integrations and their actions
// Base classes
export * from './base/base-crm.action';
// HubSpot provider
export * from './providers/hubspot'; * HubSpot CRM Provider
 * This module exports all HubSpot CRM-related classes and actions
export * from './hubspot-base.action';
export * from './actions'; * HubSpot CRM Actions
 * This module exports all HubSpot CRM-related actions for contact and company management
// Contact Actions
export * from './create-contact.action';
export * from './update-contact.action';
export * from './get-contact.action';
export * from './search-contacts.action';
export * from './delete-contact.action';
export * from './merge-contacts.action';
// Company Actions
export * from './create-company.action';
export * from './update-company.action';
export * from './get-company.action';
export * from './search-companies.action';
export * from './associate-contact-to-company.action';
// Deal Actions
export * from './create-deal.action';
export * from './update-deal.action';
export * from './get-deal.action';
export * from './search-deals.action';
export * from './get-deals-by-contact.action';
export * from './get-deals-by-company.action';
// Activity Management Actions
export * from './log-activity.action';
export * from './create-task.action';
export * from './update-task.action';
export * from './get-activities-by-contact.action';
export * from './get-upcoming-tasks.action';// Base Classes
export * from './base';
// Typeform Provider
export * from './providers/typeform';
// JotForm Provider
export * from './providers/jotform';
// SurveyMonkey Provider
export * from './providers/surveymonkey';
// Google Forms Provider
export * from './providers/google-forms';export * from './base-form-builder.action';
// Base Google Forms Action
export * from './googleforms-base.action';
// All Google Forms Actions
// Google Forms Response Actions
export * from './get-single-response.action';
export * from './get-statistics.action';
export * from './export-csv.action';
// Google Forms Form Management Actions
export * from './get-form.action';
// Base JotForm Action
export * from './jotform-base.action';
// All JotForm Actions
// JotForm Form Actions
export * from './create-form.action';
export * from './update-form.action';
// JotForm Submission Actions
export * from './get-submissions.action';
export * from './get-single-submission.action';
export * from './watch-new-submissions.action';
// Base SurveyMonkey Action
export * from './surveymonkey-base.action';
// All SurveyMonkey Actions
// SurveyMonkey Response Actions
export * from './get-responses.action';
export * from './watch-new-responses.action';
// SurveyMonkey Survey Management Actions
export * from './get-survey.action';
export * from './create-survey.action';
export * from './update-survey.action';
// Base Typeform Action
export * from './typeform-base.action';
// All Typeform Actions
// Typeform Response Actions
// Typeform Form Management Actions
export * from './get-forms.action';
export * from './get-file-content.action';
// Export base LMS action class
export * from './base/base-lms.action';
// Export LearnWorlds provider
export * from './providers/learnworlds';// Export LearnWorlds base action
export * from './learnworlds-base.action';
// Export all LearnWorlds actions
export * from './get-users.action';
export * from './get-user-details.action';
export * from './get-user-progress.action';
export * from './get-courses.action';
export * from './get-course-details.action';
export * from './enroll-user.action';
export * from './get-user-enrollments.action';
export * from './get-course-analytics.action';
export * from './create-user.action';
export * from './update-user-progress.action';
export * from './get-certificates.action';
export * from './get-quiz-results.action';export * from './base/base-social.action';
// HootSuite
export * from './providers/hootsuite';
export * from './providers/buffer';
// LinkedIn
export * from './providers/linkedin';
// Twitter/X
export * from './providers/twitter';
// Facebook
export * from './providers/facebook';
// Instagram
export * from './providers/instagram';
// TikTok
export * from './providers/tiktok';
// YouTube
export * from './providers/youtube';export * from './buffer-base.action';
export * from './get-profiles.action';
export * from './create-post.action';
export * from './get-pending-posts.action';
export * from './get-sent-posts.action';
export * from './get-analytics.action';
export * from './reorder-queue.action';
export * from './delete-post.action';
export * from './search-posts.action';export * from './facebook-base.action';
export * from './get-page-posts.action';
export * from './get-post-insights.action';
export * from './schedule-post.action';
export * from './create-album.action';
export * from './get-page-insights.action';
export * from './respond-to-comments.action';
export * from './boost-post.action';
// Export base class
export * from './hootsuite-base.action';
// Export all HootSuite actions
export * from './actions/get-scheduled-posts.action';
export * from './actions/create-scheduled-post.action';
export * from './actions/get-analytics.action';
export * from './actions/get-social-profiles.action';
export * from './actions/update-scheduled-post.action';
export * from './actions/delete-scheduled-post.action';
export * from './actions/bulk-schedule-posts.action';
export * from './actions/search-posts.action';// Instagram Provider
export * from './instagram-base.action';
// Instagram Actions
export * from './get-business-posts.action';
export * from './get-account-insights.action';
export * from './get-comments.action';
export * from './create-story.action';
export * from './linkedin-base.action';
export * from './get-organization-posts.action';
export * from './get-personal-posts.action';
export * from './get-post-analytics.action';
export * from './get-followers.action';
export * from './create-article.action';
// TikTok provider exports
export * from './tiktok-base.action';
// TikTok action exports
export * from './get-user-videos.action';
export * from './get-video-analytics.action';
export * from './get-account-analytics.action';
export * from './create-video-post.action';
export * from './get-trending-hashtags.action';
export * from './search-videos.action'; * Twitter/X Provider for MemberJunction Social Media Actions
 * This module exports the base Twitter action class and all Twitter-specific actions.
export * from './twitter-base.action';
 * Twitter/X Actions for MemberJunction
 * This module exports all Twitter-specific actions for social media automation.
export * from './create-tweet.action';
export * from './create-thread.action';
export * from './get-timeline.action';
export * from './get-mentions.action';
export * from './schedule-tweet.action';
export * from './delete-tweet.action';
export * from './search-tweets.action';export * from './youtube-base.action';
export * from './upload-video.action';
export * from './get-channel-videos.action';
export * from './update-video-metadata.action';
export * from './create-playlist.action';
export * from './schedule-video.action';
export * from './get-channel-analytics.action';
 * @description Sandboxed code execution service for MemberJunction
export * from './CodeExecutionService';
export * from './WorkerPool';
export * from './types';
export * from './libraries';
 * Libraries are provided as either:
 * - Inline implementations (lodash, date-fns, uuid, validator) - simpler, hand-coded subsets
 * - Bundled source code (mathjs, papaparse, jstat) - full libraries with complex functionality
// Get __dirname equivalent in ESM
    'mathjs',
    'papaparse',
    'jstat',
        case 'mathjs':
            return getMathjsSource();
        case 'papaparse':
            return getPapaparseSource();
        case 'jstat':
            return getJstatSource();
        meanBy: function(array, iteratee) {
            const values = array.map(fn);
            return values.length ? _.sum(values) / values.length : 0;
 * mathjs implementation
 * Provides comprehensive mathematics and statistics functions
 * We bundle the actual mathjs library for full functionality
function getMathjsSource(): string {
    const libPath = path.join(__dirname, 'bundled-libs', 'mathjs.js');
    const source = fs.readFileSync(libPath, 'utf8');
    // Wrap in IIFE that returns the math object
    return `(function() {
        ${source}
        return math;
    })()`;
 * papaparse implementation
 * Provides CSV parsing and generation
 * We bundle the actual papaparse library
function getPapaparseSource(): string {
    const libPath = path.join(__dirname, 'bundled-libs', 'papaparse.js');
    // Wrap in IIFE that returns the Papa object
        return Papa;
 * jstat implementation
 * Provides statistical distributions, hypothesis testing, and regression
 * We bundle the actual jstat library
function getJstatSource(): string {
    const libPath = path.join(__dirname, 'bundled-libs', 'jstat.js');
    // Wrap in IIFE that returns the jStat object
        return jStat;
 * Parameters for code execution
export interface CodeExecutionParams {
     * The code to execute
     * Programming language (currently only 'javascript' supported)
    language: 'javascript';
     * Input data available to the code via 'input' variable
     * Maximum execution time in seconds (default: 30)
    timeoutSeconds?: number;
     * Memory limit in MB (default: 128)
     * Note: Memory limiting requires Node.js flags, enforced at process level
    memoryLimitMB?: number;
 * Result of code execution
export interface CodeExecutionResult {
     * Whether execution was successful
     * The output value set by the code (value of 'output' variable)
    output?: any;
     * Console logs captured during execution
    logs?: string[];
     * Type of error that occurred
    errorType?: 'TIMEOUT' | 'MEMORY_LIMIT' | 'SYNTAX_ERROR' | 'RUNTIME_ERROR' | 'SECURITY_ERROR';
     * Execution time in milliseconds
    executionTimeMs?: number;
 * Options for JavaScript sandbox execution
export interface JavaScriptExecutionOptions {
     * Timeout in seconds
    timeout: number;
     * Memory limit in MB
    memoryLimit: number;
     * List of allowed npm packages that can be required
    allowedLibraries?: string[];
 * Sandbox context provided to executed code
export interface SandboxContext {
     * Input data for the code
    input: any;
     * Output data set by the code
    output: any;
     * Console API for logging
        log: (...args: any[]) => void;
        error: (...args: any[]) => void;
        warn: (...args: any[]) => void;
        info: (...args: any[]) => void;
export * from './generic/content-autotag-and-vectorize.action';export * from './generated/action_subclasses';
// Communication Actions
export * from './custom/communication/send-single-message.action';
export * from './custom/communication/slack-webhook.action';
export * from './custom/communication/teams-webhook.action';
// CRUD Actions
export * from './custom/crud/create-record.action';
export * from './custom/crud/get-record.action';
export * from './custom/crud/update-record.action';
export * from './custom/crud/delete-record.action';
// Demo Actions
export * from './custom/demo/get-weather.action';
export * from './custom/demo/get-stock-price.action';
export * from './custom/demo/calculate-expression.action';
export * from './custom/demo/color-converter.action';
export * from './custom/demo/text-analyzer.action';
export * from './custom/demo/unit-converter.action';
// Utility Actions
export * from './custom/utilities/vectorize-entity.action';
export * from './custom/utilities/business-days-calculator.action';
export * from './custom/utilities/ip-geolocation.action';
export * from './custom/utilities/census-data-lookup.action';
export * from './custom/utilities/external-change-detection.action';
export * from './custom/utilities/qr-code.action';
// Web Actions
export * from './custom/web/web-search.action';
export * from './custom/web/web-page-content.action';
export * from './custom/web/url-link-validator.action';
export * from './custom/web/url-metadata-extractor.action';
export * from './custom/web/perplexity-search.action';
export * from './custom/web/google-custom-search.action';
// Data Transformation Actions
export * from './custom/data/csv-parser.action';
export * from './custom/data/json-transform.action';
export * from './custom/data/xml-parser.action';
export * from './custom/data/aggregate-data.action';
export * from './custom/data/data-mapper.action';
export * from './custom/data/explore-database-schema.action';
export * from './custom/data/execute-research-query.action';
// Code Execution Actions
export * from './custom/code-execution/execute-code.action';
// File Operation Actions
export * from './custom/files/pdf-generator.action';
export * from './custom/files/pdf-extractor.action';
export * from './custom/files/excel-reader.action';
export * from './custom/files/excel-writer.action';
export * from './custom/files/file-compress.action';
// File Storage Actions - Granular operations for cloud storage
export * from './custom/files';
export * from './custom/files/get-file-content.action';
// Integration Actions
export * from './custom/integration/http-request.action';
export * from './custom/integration/graphql-query.action';
export * from './custom/integration/oauth-flow.action';
export * from './custom/integration/api-rate-limiter.action';
export * from './custom/integration/gamma-generate-presentation.action';
// Security Actions
export * from './custom/security/password-strength.action';
// Workflow Control Actions
export * from './custom/workflow/conditional.action';
export * from './custom/workflow/loop.action';
export * from './custom/workflow/parallel-execute.action';
export * from './custom/workflow/retry.action';
export * from './custom/workflow/delay.action';
// AI Actions
export * from './custom/ai/execute-ai-prompt.action';
export * from './custom/ai/summarize-content.action';
export * from './custom/ai/find-candidate-agents.action';
export * from './custom/ai/find-candidate-actions.action';
export * from './custom/ai/load-agent-spec.action';
export * from './custom/ai/generate-image.action';
// User Management Actions
export * from './custom/user-management/check-user-permission.action';
export * from './custom/user-management/create-user.action';
export * from './custom/user-management/create-employee.action';
export * from './custom/user-management/assign-user-roles.action';
export * from './custom/user-management/validate-email-unique.action';
// List Management Actions
export * from './custom/lists';
// Visualization Actions
export * from './custom/visualization/create-svg-chart.action';
export * from './custom/visualization/create-svg-diagram.action';
export * from './custom/visualization/create-svg-word-cloud.action';
export * from './custom/visualization/create-svg-network.action';
export * from './custom/visualization/create-svg-infographic.action';
export * from './custom/visualization/create-svg-sketch-diagram.action';
export * from './custom/visualization/create-mermaid-diagram.action';
export * from './custom/visualization/shared/svg-types';
export * from './custom/visualization/shared/svg-utils';
export * from './custom/visualization/shared/svg-theming';
export * from './custom/visualization/shared/mermaid-types';
// MCP Actions
export * from './custom/mcp';
 * CRUD Actions
 * This module exports generic Create, Read, Update, and Delete (CRUD) actions
 * that can work with any entity in the MemberJunction framework.
 * These actions provide a flexible foundation for data operations and can be
 * extended through child actions for entity-specific functionality.
export * from './base-record-mutation.action';
export * from './create-record.action';
export * from './get-record.action';
export * from './get-records.action';
export * from './update-record.action';
export * from './delete-record.action'; * File Storage Actions
 * Granular actions for interacting with various cloud storage providers
 * (Azure Blob Storage, AWS S3, Google Cloud Storage, Google Drive, Dropbox, Box.com, SharePoint).
 * Each action performs a single, focused operation on a storage provider.
 * All actions extend BaseFileStorageAction for shared functionality.
export { BaseFileStorageAction } from './base-file-storage.action';
export { ListObjectsAction } from './list-objects.action';
export { GetMetadataAction } from './get-metadata.action';
export { GetObjectAction } from './get-object.action';
export { GetDownloadUrlAction } from './get-download-url.action';
export { GetUploadUrlAction } from './get-upload-url.action';
export { ObjectExistsAction } from './object-exists.action';
export { DirectoryExistsAction } from './directory-exists.action';
export { CopyObjectAction } from './copy-object.action';
export { MoveObjectAction } from './move-object.action';
export { DeleteObjectAction } from './delete-object.action';
export { CreateDirectoryAction } from './create-directory.action';
export { DeleteDirectoryAction } from './delete-directory.action';
export { SearchStorageFilesAction } from './search-storage-files.action';
export { ListStorageAccountsAction } from './list-storage-providers.action';
 * List Management Actions
 * These actions provide comprehensive list management capabilities for AI agents,
 * workflows, and other automation scenarios.
 * Available Actions:
 * - Add Records to List: Add one or more records to a list
 * - Remove Records from List: Remove records from a list
 * - Create List: Create a new list with optional initial records
 * - Get List Records: Retrieve records from a list with filtering
 * - Get Record List Membership: Find which lists contain a specific record
 * - Update List Item Status: Bulk update status on list items
export * from './add-records-to-list.action';
export * from './remove-records-from-list.action';
export * from './create-list.action';
export * from './get-list-records.action';
export * from './get-record-list-membership.action';
export * from './update-list-item-status.action';
 * @fileoverview MCP (Model Context Protocol) Actions for MemberJunction
 * This module provides Actions for interacting with external MCP servers,
 * enabling workflow and agent integration with MCP tool capabilities.
 * @module @memberjunction/actions/mcp
// Action exports
export { ExecuteMCPToolAction } from './execute-mcp-tool.action.js';
export { SyncMCPToolsAction } from './sync-mcp-tools.action.js';
export { TestMCPConnectionAction } from './test-mcp-connection.action.js';
export { ListMCPToolsAction } from './list-mcp-tools.action.js';
export { MCPToolAction } from './mcp-tool.action.js';
export * from './base-file-handler';
export * from './json-param-helper';export * from './generic/BaseAction';
export * from './generic/BaseActionFilter';
export * from './generic/BaseOAuthAction';
export * from './generic/ActionEngine';
export * from './generic/OAuth2Manager';
export * from './entity-actions/EntityActionEngine';
export * from './entity-actions/EntityActionInvocationTypes';export * from "./scheduler";import express from 'express';
import { LogError, LogStatus, Metadata } from "@memberjunction/core";
import { currentUserEmail, serverPort } from "./config";
import { handleServerInit } from './util';
import { ScheduledActionEngine } from '@memberjunction/scheduled-actions';
app.use(express.urlencoded({ extended: true }));
// these can be command line options when running this from command line or they
// can be HTTP query strig params, a comma delimited list passed into a GET request
// on the query string for the parameter RunOptions, for example
// http://localhost:8000/?RunOptions=all 
// in the future we might have other RunOptions
type runOption = {
    run: (initServer: boolean) => Promise<boolean>;
    currentRuns?: number;
const runOptions: runOption[] = [
        name: "all",
        description: "Run all processes",
        run: runAll,
        maxConcurrency: 1,
        name: "ScheduledActions",
        description: "Run all scheduled actions",
        run: runScheduledActions,
        name: "enrichaccounts",
        description: "Run Apollo Enrichment for Accounts",
        run: enrichAccounts,
        maxConcurrency: 1
        name: "enrichcontacts",
        description: "Run Apollo Enrichment for Contacts",
        run: enrichContacts,
    ,{
        name: "autoTagAndVectorize",
        description: "Run Autotag and Vectorize Content",
        run: autotagAndVectorize,
app.get('/', async (req: any, res: any) => {
    //Run all active integrations with conditions
    } = req.query;
    LogStatus(`Server Request Received: options === ${options}`);
    let typedOptions: string = options;    
    const optionsToRun: string[] = typedOptions.includes(',') ? typedOptions.split(',') : [typedOptions];
    if (await runWithOptions(optionsToRun)) {
        res.json({Status: "Success"});
        res.json({Status: "Error"});
app.listen(serverPort, () => {
        LogStatus(`Server listening on port ${serverPort}!`)
async function runWithOptions(options: string[]): Promise<boolean> {
        // first check for the all flag and if that is inluded, just run all and ignore everything else, or if NO params are passed, run all
        if (options.includes("all") || options.length === 0) {
            return await runAll();
        // next loop through the runOptions and run any that are included in the args, if we get here that means we don't have the all flag
        await handleServerInit(false); // init server here once
        for (const requestedOption of options) {
            // loop through the requested options from the caller and run each one
            const opt = runOptions.find(o => o.name.trim().toLowerCase() === requestedOption.trim().toLowerCase())
            if (!opt) {
                // if the requested option is not found, log a warning and skip it
                LogStatus(`Requested option ${requestedOption} not found, skipping`);
                // if the requested option is found, run it
                LogStatus(`Running option ${opt.name}`);
                bSuccess = bSuccess && await executeRunOption(opt,false) // pass in false as we don't need each option to init the server again
        LogError("An error occurred:", undefined, error);
export async function runAll(): Promise<boolean> {
    // loop through the runOptions and run each one, filter out the all option since that is what WE are doing here :)
    for (const option of runOptions.filter(o => o.name !== "all")) {
        bSuccess = bSuccess && await executeRunOption(option, false); // pass in false as we don't need each option to init the server again
    return bSuccess
async function executeRunOption(option: runOption, initServer: boolean): Promise<boolean> {
    if (option.currentRuns === undefined) {
        option.currentRuns = 0;
    if (option.maxConcurrency === undefined) {
        option.maxConcurrency = 1;
    if (option && option.maxConcurrency > option.currentRuns) {
        option.currentRuns++;
        let bResult: boolean = false;
            bResult = await option.run(initServer)
            LogStatus(e)
        finally {
            option.currentRuns--;
            return bResult;
        LogError(`Max concurrency of ${option.maxConcurrency} reached for ${option.name} option, skipping`);
export async function runScheduledActions(): Promise<boolean> {
        const user = UserCache.Instance.Users.find(u => u.Email === currentUserEmail)
        if (!user){
            throw new Error(`User ${currentUserEmail} not found in cache`);
        ScheduledActionEngine.Instance.Config(false, user);
        const actionResults = await ScheduledActionEngine.Instance.ExecuteScheduledActions(user);
        return actionResults.some(r => !r.Success) ? false : true;
export async function enrichAccounts(): Promise<boolean> {
    return await runScheduledAction("Apollo Enrichment - Accounts");
export async function enrichContacts(): Promise<boolean> {
    return await runScheduledAction("Apollo Enrichment - Contacts");
export async function autotagAndVectorize(): Promise<boolean> {
    return await runScheduledAction("Autotag And Vectorize Content");
export async function runScheduledAction(actionName: string): Promise<boolean> {
        const actionResults = await ScheduledActionEngine.Instance.ExecuteScheduledAction(actionName, user);
        return actionResults.Success;
export * from './services/ai-instrumentation.service';
// Main Components
export * from './components/execution-monitoring.component';
export * from './components/prompts/prompt-management.component';
export * from './components/agents/agent-configuration.component';
export * from './components/models/model-management.component';
export * from './components/system/system-configuration.component';
// Widget Components
export * from './components/widgets/kpi-card.component';
export * from './components/widgets/live-execution-widget.component';
export * from './components/charts/time-series-chart.component';
export * from './components/charts/performance-heatmap.component';export * from './api-keys-resource.component';
export * from './api-key-create-dialog.component';
export * from './api-key-edit-panel.component';
export * from './api-key-list.component';
export * from './api-applications-panel.component';
export * from './api-scopes-panel.component';
// Actions Resource Components (BaseResourceComponent-based)
export * from './components/actions-overview.component';
export * from './components/scheduled-actions.component';
export * from './components/code-management.component';
export * from './components/entity-integration.component';
export * from './components/security-permissions.component';
// List view components
export * from './components/actions-list-view.component';
export * from './components/executions-list-view.component';
export * from './components/categories-list-view.component';
// Action Explorer (Windows Explorer-style browser)
export * from './components/explorer';
export * from './services/action-explorer-state.service';
export * from './action-explorer.component';
export * from './action-tree-panel.component';
export * from './action-toolbar.component';
export * from './action-breadcrumb.component';
export * from './action-card.component';
export * from './action-list-item.component';
export * from './new-category-panel.component';
export * from './new-action-panel.component';
export * from './component-studio-dashboard.component';
export * from './components/artifact-selection-dialog.component';
export * from './components/artifact-load-dialog.component';
export * from './components/text-import-dialog.component';// Main dashboard component
export * from './credentials-dashboard.component';
// Resource components
export * from './components/credentials-overview-resource.component';
export * from './components/credentials-list-resource.component';
export * from './components/credentials-types-resource.component';
export * from './components/credentials-categories-resource.component';
export * from './components/credentials-audit-resource.component';
// Data Explorer Dashboard - Main exports
// Dashboard component
export { DataExplorerDashboardComponent } from './data-explorer-dashboard.component';
// Resource component (BaseResourceComponent-based wrapper)
export { DataExplorerResourceComponent } from './data-explorer-resource.component';
// Child components
export { NavigationPanelComponent } from './components/navigation-panel/navigation-panel.component';
export { ExplorerStateService } from './services/explorer-state.service';
// Models
export * from './models/explorer-state.interface';
export * from './components/lists-my-lists-resource.component';
export * from './components/lists-browse-resource.component';
export * from './components/lists-categories-resource.component';
export * from './components/lists-operations-resource.component';
export * from './components/venn-diagram/venn-diagram.component';
export * from './services/list-set-operations.service';
 * @fileoverview MCP Module Exports
 * Public API exports for the MCP management module.
export { MCPModule } from './mcp.module';
// Dashboard Component
    MCPServerData,
    MCPConnectionData,
    MCPToolData,
    MCPExecutionLogData,
    MCPDashboardFilters,
    MCPDashboardStats,
    MCPDashboardTab
} from './mcp-dashboard.component';
// Dialog Components
    MCPServerDialogComponent,
    ServerDialogResult,
    TRANSPORT_TYPES,
    AUTH_TYPES
} from './components/mcp-server-dialog.component';
    MCPConnectionDialogComponent,
    ConnectionDialogResult
} from './components/mcp-connection-dialog.component';
    MCPToolsService,
    MCPSyncResult,
    MCPSyncProgress,
    MCPSyncState
} from './services/mcp-tools.service';
// Export all Scheduling resource components
export * from './scheduling-overview-resource.component';
export * from './scheduling-jobs-resource.component';
export * from './scheduling-activity-resource.component';
// Export child components for internal use
export * from './scheduling-overview.component';
export * from './scheduling-jobs.component';
export * from './scheduling-activity.component';
export * from './job-slideout.component';
// System Diagnostics exports
export * from './system-diagnostics.component';
// Export all Testing resource components
export * from './testing-dashboard-tab-resource.component';
export * from './testing-runs-resource.component';
export * from './testing-analytics-resource.component';
export * from './testing-review-resource.component';
export * from './testing-explorer-resource.component';
// Export tab components for internal use
export * from './testing-dashboard-tab.component';
export * from './testing-runs.component';
export * from './testing-analytics.component';
export * from './testing-review.component';
export * from './components';export * from './labels-resource.component';
export * from './diff-resource.component';
export * from './restore-resource.component';
export * from './graph-resource.component';
export * from './highlight-search.pipe';
export * from './public-api';
export * from './command-palette.component';
export * from './command-palette.service';
export * from './user-menu.types';
export * from './base-user-menu';
// Utility dialogs only - config panels are used instead of part-specific config dialogs
export * from './confirm-dialog.component';
// Config Panels - Standalone form components without dialog chrome
// These can be embedded in the add-panel-dialog or wrapped in a dialog for editing
export * from './base-config-panel';
export * from './weburl-config-panel.component';
export * from './view-config-panel.component';
export * from './query-config-panel.component';
export * from './artifact-config-panel.component';
export * from './dashboard-browser.component';
// Dashboard Part Renderers - Base Class
export * from './base-dashboard-part';
// Runtime Part Components
export * from './weburl-part.component';
export * from './view-part.component';
export * from './query-part.component';
export * from './artifact-part.component';
 * @fileoverview Entry point for @memberjunction/ng-react package
 * @module @memberjunction/ng-react
 * @fileoverview Main entry point for the MemberJunction CodeGen Library.
 * This package provides comprehensive code generation capabilities for the MemberJunction platform,
 * including:
 * **Configuration Management:**
 * - Configuration file parsing and validation
 * - Database connection management
 * **Database Operations:**
 * - Schema introspection and metadata management
 * - SQL script generation (views, procedures, indexes)
 * - Database schema JSON export
 * **Code Generation:**
 * - TypeScript entity classes with Zod validation
 * - Angular components and forms
 * - GraphQL resolvers and schemas
 * - Action subclasses for business logic
 * **Utilities:**
 * - Status logging and error handling
 * - Command execution
 * - System integrity checks
 * import { RunCodeGenBase, initializeConfig } from '@memberjunction/codegen-lib';
 * // Initialize configuration
 * const config = initializeConfig(process.cwd());
 * // Run code generation
 * const codeGen = new RunCodeGenBase();
 * await codeGen.Run();
// Configuration exports
export { initializeConfig } from './Config/config'
export * from './Config/config'
export * from './Config/db-connection'
// Database exports
export * from './Database/dbSchema'
export * from './Database/manage-metadata'
export * from './Database/sql_codegen'
export * from './Database/sql'
// Code generation exports
export * from './Misc/entity_subclasses_codegen'
export * from './Misc/action_subclasses_codegen';
export * from './Misc/graphql_server_codegen'
// Angular exports
export * from './Angular/angular-codegen'
export * from './Angular/related-entity-components';
export * from './Angular/entity-data-grid-related-entity-component';
export * from './Angular/join-grid-related-entity-component';
export * from './Angular/timeline-related-entity-component';
// Utility exports
export * from './Misc/status_logging'
export * from './Misc/system_integrity';
export * from './Misc/runCommand'
export * from './Misc/util'
// Manifest generation
export * from './Manifest/GenerateClassRegistrationsManifest'
// Entity name scanning
export * from './EntityNameScanner/EntityNameScanner'
export * from './EntityNameScanner/MetadataNameScanner'
export * from './EntityNameScanner/HtmlEntityNameScanner'
// Main runner
export * from './runCodeGen'// PUBLIC API SURFACE AREA
export * from './BaseEngine';
export * from './BaseProvider';
export * from './CredentialUtils';export * from './client';export * from './entity-communications'; * @memberjunction/notifications
 * Unified notification engine for MemberJunction with multi-channel delivery support.
 * Extends BaseEngine for cached notification types and auto-refresh capabilities.
 * - Cached notification types (loaded once, auto-refreshed on changes)
 * - User delivery preferences (In-App, Email, SMS per channel)
 * - Template-based email/SMS formatting
 * - Integration with CommunicationEngine for external delivery
 * - Automatic in-app notification creation
 * import { NotificationEngine } from '@memberjunction/notifications';
 * // Initialize at server startup
 * await NotificationEngine.Instance.Config(false, contextUser);
 * // Send a notification
 * const result = await NotificationEngine.Instance.SendNotification({
 *   userId: contextUser.ID,
 *   typeNameOrId: 'Agent Completion',
 *   title: 'Task Complete',
 *   message: 'Your AI agent has finished processing',
 *   templateData: {
 *     agentName: 'My Agent',
 *     conversationUrl: 'https://...'
export { NotificationEngine } from './NotificationEngine';
export { SendNotificationParams, NotificationResult, DeliveryChannels } from './types';
export * from './MSGraphProvider';
export * from './auth';
export * from './generic/models';
// Re-export credential types for convenience
export type { MSGraphCredentials } from './MSGraphProvider';export * from './GmailProvider';
export type { GmailCredentials } from './GmailProvider';export * from './SendGridProvider';
export type { SendGridCredentials } from './SendGridProvider';export * from './TwilioProvider';
export type { TwilioCredentials } from './TwilioProvider'; * Entry point for the MemberJunction Component Registry Server
 * This module starts the Component Registry API Server when run directly.
 * The server can be configured via the mj.config.cjs file in your project root.
 * Configuration example in mj.config.cjs:
 * ```javascript
 * module.exports = {
 *   // ... other config ...
 *   componentRegistrySettings: {
 *     port: 3200,
 *     enableRegistry: true,
 *     registryId: 'your-registry-guid', // Optional
 *     requireAuth: false,
 *     corsOrigins: ['*']
import { startComponentRegistryServer } from './Server.js';
// Export all server components for library use
// Start the server if this file is run directly
  startComponentRegistryServer().catch(error => {
    console.error('Failed to start Component Registry Server:', error);
export * from './ComponentRegistryClient';
 * @memberjunction/config
 * Central configuration utilities for MemberJunction framework.
 * Provides utilities for loading and merging user overrides with package defaults.
 * Architecture:
 * - Each package (server, codegen-lib, etc.) exports its own DEFAULT_CONFIG
 * - This package provides utilities to discover, load, and merge configurations
 * - User's mj.config.cjs file overrides package defaults
 * - Environment variables override everything
  loadMJConfig,
  loadMJConfigSync,
  buildMJConfig,
  type LoadConfigOptions,
  type LoadConfigResult
} from './config-loader';
  mergeConfigs,
  validateConfigStructure,
  type MergeOptions
} from './config-merger';
  type MJConfig,
  isValidConfig
} from './config-types';
  parseBooleanEnv
} from './env-utils';
export * from './Core';
export * from './LocalFileSystem';
export * from './RSSFeed';
export * from './Websites';
export * from './CloudStorage'export * from './generic/CloudStorageBase'
export * from './providers/AutotagAzureBlob'export * from './generic/AutotagBase';export * from './generic/AutotagBaseEngine'
export * from './generic/content.types';
export * from './generic/process.types';
export * from './generic/AutotagEntity';export * from './generic/AutotagLocalFileSystem'export * from './generic/RSS.types'
export * from './generic/AutotagRSSFeed'export * from './generic/AutotagWebsite'// Core engine
export { CredentialEngine } from './CredentialEngine';
    CredentialAccessDetails,
    // Pre-defined credential value interfaces for type safety
    APIKeyCredentialValues,
    APIKeyWithEndpointCredentialValues,
    OAuth2ClientCredentialValues,
    BasicAuthCredentialValues,
    AzureServicePrincipalCredentialValues,
    AWSIAMCredentialValues,
    DatabaseConnectionCredentialValues,
    TwilioCredentialValues
// Re-export entity types for convenience
    MJCredentialCategoryEntity
 * DBAutoDoc - AI-powered SQL Server database documentation generator
 * Main exports for programmatic use
// Programmatic API (primary entry point for library usage)
export * from './api/index.js';
// Core types
export * from './types/index.js';
// Database layer
export * from './database/index.js';
// Prompts
export * from './prompts/index.js';
export * from './state/index.js';
// Analysis engine
export * from './core/index.js';
export * from './core/AnalysisOrchestrator.js';
// Generators
export * from './generators/index.js';
export * from './utils/index.js';
 * Main entry point for library usage
export { DBAutoDocAPI } from './DBAutoDocAPI.js';
  DBAutoDocAPIConfig,
  AnalysisExecutionResult,
  AnalysisStatus,
  ProgressCallback
} from './DBAutoDocAPI.js';
 * Exports for core module
export { AnalysisEngine } from './AnalysisEngine.js';
export { BackpropagationEngine } from './BackpropagationEngine.js';
export { ConvergenceDetector } from './ConvergenceDetector.js';
 * Exports for database module
export { DatabaseConnection, Introspector, DataSampler, createDriver } from './Database.js';
export { TopologicalSorter } from './TopologicalSorter.js';
 * Relationship Discovery Module
 * Detects primary keys and foreign keys in databases with incomplete metadata
export * from './DiscoveryTriggerAnalyzer.js';
export * from './DiscoveryEngine.js';
export * from './PKDetector.js';
export * from './FKDetector.js';
 * Exports for generators module
export { SQLGenerator, SQLGeneratorOptions } from './SQLGenerator.js';
export { MarkdownGenerator } from './MarkdownGenerator.js';
export { ReportGenerator } from './ReportGenerator.js';
export { HTMLGenerator, HTMLGeneratorOptions } from './HTMLGenerator.js';
export { CSVGenerator, CSVGeneratorOptions, CSVExport } from './CSVGenerator.js';
export { MermaidGenerator, MermaidGeneratorOptions } from './MermaidGenerator.js';
 * Exports for prompts module
export { PromptFileLoader } from './PromptFileLoader.js';
export { PromptEngine } from './PromptEngine.js';
 * Exports for state module
export { StateManager } from './StateManager.js';
export { IterationTracker } from './IterationTracker.js';
export { StateValidator, ValidationResult } from './StateValidator.js';
 * Centralized exports for all types
export * from './state.js';
export * from './analysis.js';
export * from './prompts.js';
 * Exports for utils module
export { ConfigLoader } from './config-loader.js';
//PUBLIC API SURFACE AREA
 * @fileoverview MemberJunction Field-Level Encryption Package
 * This package provides field-level encryption capabilities for MemberJunction
 * entities. It enables encrypting sensitive data at rest in the database while
 * providing transparent decryption for authorized access.
 * - **AES-256-GCM/CBC encryption** - Industry-standard authenticated encryption
 * - **Pluggable key sources** - Environment variables, config files, or custom providers
 * - **Declarative configuration** - Enable encryption via EntityField metadata
 * - **Transparent operation** - Automatic encrypt on save, decrypt on load
 * - **Key rotation support** - Full re-encryption with transactional safety
 * ## Quick Start
 * const decrypted = await engine.Decrypt(encrypted, contextUser);
 * // Check if value is encrypted
 * if (engine.IsEncrypted(someValue)) {
 * ## Key Source Providers
 * Built-in providers:
 * - `EnvVarKeySource` - Reads keys from environment variables
 * - `ConfigFileKeySource` - Reads keys from mj.config.cjs
 * To create a custom provider, extend `EncryptionKeySourceBase`.
 * - Keys are never stored in the database
 * - Cached key material has configurable TTL
 * ## API Key Management
 * API key functionality has been moved to the `@memberjunction/api-keys` package.
 * Use that package for API key generation, validation, and scope-based authorization.
// Core interfaces and types
    EncryptionKeySourceConfig,
    KeyConfiguration,
    RotateKeyParams,
    RotateKeyResult,
    EnableFieldEncryptionParams,
    EnableFieldEncryptionResult
// Base class for key source providers
export { EncryptionKeySourceBase } from './EncryptionKeySourceBase';
// Core encryption engine
export { EncryptionEngine } from './EncryptionEngine';
// Built-in key source providers
export { EnvVarKeySource } from './providers/EnvVarKeySource';
export { ConfigFileKeySource } from './providers/ConfigFileKeySource';
// Cloud key source providers (require optional dependencies)
export { AWSKMSKeySource } from './providers/AWSKMSKeySource';
export { AzureKeyVaultKeySource } from './providers/AzureKeyVaultKeySource';
// Actions for key management and data migration
export { RotateEncryptionKeyAction } from './actions/RotateEncryptionKeyAction';
export { EnableFieldEncryptionAction } from './actions/EnableFieldEncryptionAction';
 * @fileoverview Encryption-related actions for key management and data migration.
 * These actions provide administrative capabilities for:
 * - Rotating encryption keys with full data re-encryption
 * - Enabling encryption on existing fields (initial data encryption)
export { RotateEncryptionKeyAction } from './RotateEncryptionKeyAction';
export { EnableFieldEncryptionAction } from './EnableFieldEncryptionAction';
export * from './ChangeDetector';export * from './generated/entity_subclasses';export { gql } from 'graphql-request';
export { setupGraphQLClient } from './config';
export { GraphQLDataProvider, GraphQLProviderConfigData } from './graphQLDataProvider';
export * from './graphQLTransactionGroup';
export { FieldMapper } from './FieldMapper';
export * from './rolesAndUsersType';
export * from './graphQLSystemUserClient';
export { GraphQLActionClient } from './graphQLActionClient';
export { GraphQLEncryptionClient } from './graphQLEncryptionClient';
export type { CreateAPIKeyParams, CreateAPIKeyResult, RevokeAPIKeyResult } from './graphQLEncryptionClient';
export { GraphQLAIClient } from './graphQLAIClient';
export { GraphQLTestingClient } from './graphQLTestingClient';
    RunTestParams,
    RunTestResult,
    RunTestSuiteParams,
    RunTestSuiteResult,
    TestExecutionProgress
} from './graphQLTestingClient';
export { GraphQLComponentRegistryClient } from './GraphQLComponentRegistryClient';
    GetRegistryComponentParams,
    SearchRegistryComponentsParams,
    RegistryComponentSearchResult,
    ComponentDependencyTree,
    ComponentSpecWithHash
} from './GraphQLComponentRegistryClient';
export { GraphQLVersionHistoryClient } from './graphQLVersionHistoryClient';
    CreateVersionLabelParams,
    CreateVersionLabelProgress,
    CreateVersionLabelResult
} from './graphQLVersionHistoryClient';
export * from './graphQLFileStorageClient';
export * from './storage-providers';export * from './data-requirements';
export * from './component-option';
export * from './runtime-types';
export * from './shared';
export * from './component-props-events';
export * from './component-constraints';
export * from './util';
export * from './component-spec';
export * from './component-spec-runtime';
export * from './library-dependency'; * MemberJunction API Server (MJ 3.0 Minimal Architecture)
 * All initialization logic is in @memberjunction/server-bootstrap
import { createMJServer } from '@memberjunction/server-bootstrap';
import { resolve } from 'node:path';
// Import generated packages to trigger class registration
import 'mj_generatedentities';
import 'mj_generatedactions';
import '@memberjunction/server-bootstrap/mj-class-registrations';
// Optional: Import communication providers if needed
// import '@memberjunction/communication-sendgrid';
// import '@memberjunction/communication-teams';
// Optional: Import custom auth/user creation logic
// See: /docs/examples/custom-user-creation/README.md
// import './custom/customUserCreation';
// Resolve resolver paths relative to this file
const __dirname = fileURLToPath(new URL('.', import.meta.url));
const resolverPaths = [resolve(__dirname, 'generated/generated.{js,ts}')];
createMJServer({ resolverPaths }).catch(console.error);export default class AI extends Command {
  static description = 'Execute AI agents and actions';
  static hidden = false;
    // This command just displays help for the ai topic
    await this.config.runCommand('help', ['ai']);
export default class Audit extends Command {
  static description = 'Analyze and audit AI agent runs, prompts, and actions for debugging and performance analysis';
    // This command just displays help for the audit topic
    await this.config.runCommand('help', ['ai', 'audit']);
import { readFileSync, writeFileSync } from 'node:fs';
import fg from 'fast-glob';
import { dirname } from 'node:path';
// https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string
const semverRegex =
  /^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$/;
const tagSchema = z
  .string()
  .optional()
  .transform((tag) => tag?.replace(/^v/, ''))
  .refine((tag) => !tag || semverRegex.test(tag));
 * No-op used to suppress logging output
 * @returns No return value
const suppressLogging = () => null;
export default class Bump extends Command {
  static description = 'Bumps MemberJunction dependency versions';
      command: '<%= config.bin %> <%= command.id %>',
      description: "Bump all @memberjunction/* dependencies in the current directory's package.json to the CLI version",
      command: '<%= config.bin %> <%= command.id %> -rdv',
      description: 'Preview all recursive packages bumps without writing any changes.',
      command: '<%= config.bin %> <%= command.id %> -rqt v2.10.0 | xargs -n1 -I{} npm install --prefix {}',
        'Recursively bump all @memberjunction/* dependencies in all packages to version v2.10.0 and output only the paths containing the updated package.json files. Pipe the output to xargs to run npm install in each directory and update the package-lock.json files as well.',
    verbose: Flags.boolean({ char: 'v', description: 'Enable additional logging' }),
    recursive: Flags.boolean({ char: 'r', description: 'Bump version in current directory and all subdirectories' }),
    tag: Flags.string({ char: 't', description: 'Version tag to bump target for bump (e.g. v2.10.0), defaults to the CLI version' }),
    quiet: Flags.boolean({ char: 'q', description: 'Only output paths for updated packages' }),
    dry: Flags.boolean({ char: 'd', description: 'Dry run, do not write changes to package.json files' }),
    const { flags } = await this.parse(Bump);
    const verboseLogger = flags.verbose && !flags.quiet ? this.log.bind(this) : suppressLogging;
    const normalLogger = flags.quiet ? suppressLogging : this.log.bind(this);
    const quietLogger = flags.quiet ? this.log.bind(this) : suppressLogging;
    // Get the target version from the tag flag or the CLI version
    const tagArgument = tagSchema.safeParse(flags.tag);
    if (flags.tag && tagArgument.success === false) {
      this.error(`Invalid tag argument: '${flags.tag}'; must be a valid semver version string (e.g. v2.10.0)`);
    const targetVersion = flags.tag && tagArgument.success ? tagArgument.data : this.config.pjson.version;
    normalLogger(`Bumping all @memberjunction/* dependencies to version ${targetVersion}`);
    flags.recursive && normalLogger('Recursively updating all package.json files');
    // get list of package.json files to edit
    const packageJsonFiles = fg.sync(`${flags.recursive ? '**/' : ''}package.json`, { ignore: ['node_modules/**'] });
    if (packageJsonFiles.length === 0) {
      this.error('No package.json files found');
    if (flags.dry) {
      normalLogger('Dry run, no changes will be made to package.json files');
    const skipped = [];
    const mjRegx = /"@memberjunction\/([^"]+)":(\s*)("[^"]+")/g;
    const banner = 'Bumping packages... ';
    const spinner = ora(banner);
    normalLogger('');
      for (let i = 0; i < packageJsonFiles.length; i++) {
        const packageJson = `./${packageJsonFiles[i]}`;
        const packageJsonContents = readFileSync(packageJson).toString();
        if (!mjRegx.test(packageJsonContents)) {
          skipped.push(packageJson);
        verboseLogger(`\tBumping ${dirname(packageJson)}`);
        spinner.text = `${banner} ${i + 1 - skipped.length}/${packageJsonFiles.length - skipped.length}`;
        const bumpedPackageJson = packageJsonContents.replaceAll(mjRegx, `"@memberjunction/$1":$2"${targetVersion}"`);
        if (!flags.dry) {
          writeFileSync(packageJson, bumpedPackageJson);
        // In quiet mode, we only output the paths of the updated package.json files so they can be piped to another command
        quietLogger(dirname(packageJson));
      spinner.succeed(`Bumped ${packageJsonFiles.length - skipped.length}/${packageJsonFiles.length} packages`);
      this.error(error instanceof Error ? error : 'Command failed');
import { Skyway } from '@memberjunction/skyway-core';
import { getValidatedConfig, getSkywayConfig } from '../../config';
export default class Clean extends Command {
  static description = 'Resets the MemberJunction database to a pre-installation state';
    `<%= config.bin %> <%= command.id %>
    const { flags } = await this.parse(Clean);
    const config = getValidatedConfig();
    if (config.cleanDisabled !== false) {
      this.error('Clean is disabled. Set cleanDisabled: false in mj.config.cjs to enable.');
    const skywayConfig = await getSkywayConfig(config);
    const skyway = new Skyway(skywayConfig);
    this.log('Resetting MJ database to pre-installation state');
    this.log('Note that users and roles have not been dropped');
    const spinner = ora('Cleaning up...');
      const result = await skyway.Clean();
        spinner.succeed();
        this.log(`The database has been reset. ${result.ObjectsDropped} objects dropped.`);
        if (result.DroppedObjects.length > 0) {
          this.log(`Objects dropped:\n\t- ${result.DroppedObjects.join('\n\t- ')}`);
        this.logToStderr(`\nClean failed: ${result.ErrorMessage ?? 'unknown error'}`);
        if (flags.verbose && result.DroppedObjects.length > 0) {
          this.logToStderr(`Partial cleanup:\n\t- ${result.DroppedObjects.join('\n\t- ')}`);
        this.error('Command failed');
      this.error(`Clean error: ${message}`);
      await skyway.Close();
import type { ParserOutput } from '@oclif/core/lib/interfaces/parser';
import { updatedConfig } from '../../config';
export default class CodeGen extends Command {
  static description = `Run the full MemberJunction code generation pipeline.
Analyzes your SQL Server database schema, updates MemberJunction metadata, and
generates synchronized code across the entire stack:
  - SQL views, stored procedures, indexes, and permissions
  - TypeScript entity classes with Zod validation schemas
  - Angular form components with AI-driven layouts
  - GraphQL resolvers and type definitions
  - Action subclasses and DB schema JSON
Configuration is loaded from mj.config.cjs in the current directory (or parent
directories). Database connection can also be set via environment variables:
DB_HOST, DB_DATABASE, CODEGEN_DB_USERNAME, CODEGEN_DB_PASSWORD.
Use --skipdb to skip all database operations (metadata sync, SQL object
generation) and only regenerate TypeScript, Angular, and GraphQL output from
existing metadata.`;
      description: 'Run the full code generation pipeline',
      command: '<%= config.bin %> <%= command.id %> --skipdb',
      description: 'Regenerate code files without touching the database',
    skipdb: Flags.boolean({
      description: 'Skip database operations (metadata sync, SQL generation). Only regenerate TypeScript entities, Angular components, and GraphQL resolvers from existing metadata.',
  flags: ParserOutput<CodeGen>['flags'];
    const { runMemberJunctionCodeGeneration, initializeConfig } = await import('@memberjunction/codegen-lib');
    const parsed = await this.parse(CodeGen);
    this.flags = parsed.flags;
    const config = updatedConfig();
      this.error('No configuration found. Ensure mj.config.cjs exists in the current directory or a parent directory.');
    // Initialize configuration
    initializeConfig(process.cwd());
    // Call the function with the determined argument
    runMemberJunctionCodeGeneration(this.flags.skipDb);
export default class DBDoc extends Command {
  static description = 'AI-powered database documentation generator';
    // This command just displays help for the dbdoc topic
    await this.config.runCommand('help', ['dbdoc']);
import { confirm, input, select } from '@inquirer/prompts';
import recast from 'recast';
import fs from 'fs-extra';
import { execSync } from 'node:child_process';
import os from 'node:os';
import { ZodError, z } from 'zod';
// Directories are relative to execution cwd
const GENERATED_ENTITIES_DIR = 'GeneratedEntities';
const SQL_SCRIPTS_DIR = 'SQL Scripts';
const GENERATED_DIR = 'generated';
const MJ_BASE_DIR = 'MJ_BASE';
const MJAPI_DIR = 'MJAPI';
const MJEXPLORER_DIR = 'MJExplorer';
type Config = z.infer<typeof configSchema>;
const configSchema = z.object({
  dbUrl: z.string().min(1),
  dbInstance: z.string(),
  dbDatabase: z.string().min(1),
  dbPort: z.number({ coerce: true }).int().positive(),
  codeGenPwD: z.string(),
  mjAPILogin: z.string(),
  mjAPIPwD: z.string(),
  graphQLPort: z.number({ coerce: true }).int().positive().optional(),
  authType: z.enum(['MSAL', 'AUTH0', 'BOTH']),
  msalWebClientId: z.string().optional(),
  msalTenantId: z.string().optional(),
  auth0ClientId: z.string().optional(),
  auth0ClientSecret: z.string().optional(),
  auth0Domain: z.string().optional(),
  createNewUser: z.coerce.boolean().optional(),
  userEmail: z.string().email().or(z.literal('')).optional().default(''),
  userFirstName: z.string().optional(),
  userLastName: z.string().optional(),
  userName: z.string().optional(),
  openAIAPIKey: z.string().optional(),
  anthropicAPIKey: z.string().optional(),
  mistralAPIKey: z.string().optional(),
export default class Install extends Command {
  static description = 'Install MemberJunction';
  flags: ParserOutput<Install>['flags'];
  userConfig!: Config;
    const parsed = await this.parse(Install);
    this.checkNodeVersion();
    this.checkAvailableDiskSpace(2);
    this.verifyDirs(GENERATED_ENTITIES_DIR, SQL_SCRIPTS_DIR, MJAPI_DIR, MJEXPLORER_DIR);
    this.userConfig = await this.getUserConfiguration();
    this.log('Setting up MemberJunction Distribution...');
    if (this.flags.verbose) {
      this.logJson({ userConfig: this.userConfig, flags: this.flags });
    this.log('\nBootstrapping GeneratedEntities...');
    this.log('Running npm install...');
    execSync('npm install', { stdio: 'inherit', cwd: GENERATED_ENTITIES_DIR });
    // Process Config
    this.log('\nProcessing Config...');
    this.log('   Updating ');
    this.log('   Setting up .env and mj.config.cjs...');
    const dotenvContent = `#Database Setup
DB_HOST='${this.userConfig.dbUrl}'
DB_PORT=${this.userConfig.dbPort}
CODEGEN_DB_USERNAME='${this.userConfig.codeGenLogin}'
CODEGEN_DB_PASSWORD='${this.userConfig.codeGenPwD}'
DB_USERNAME='${this.userConfig.mjAPILogin}'
DB_PASSWORD='${this.userConfig.mjAPIPwD}'
DB_DATABASE='${this.userConfig.dbDatabase}'
${this.userConfig.dbInstance ? "DB_INSTANCE_NAME='" + this.userConfig.dbInstance + "'" : ''}
${this.userConfig.dbTrustServerCertificate === 'Y' ? 'DB_TRUST_SERVER_CERTIFICATE=1' : ''}
OUTPUT_CODE='${this.userConfig.dbDatabase}'
# If using Advanced Generation or the MJAI library, populate this with the API key for the AI vendor you are using
# Also, you need to configure the settings under advancedGeneration in the mj.config.cjs file, including choosing the vendor.
AI_VENDOR_API_KEY__OpenAILLM='${this.userConfig.openAIAPIKey}'
AI_VENDOR_API_KEY__MistralLLM='${this.userConfig.mistralAPIKey}'
AI_VENDOR_API_KEY__AnthropicLLM='${this.userConfig.anthropicAPIKey}'
PORT=${this.userConfig.graphQLPort}
WEB_CLIENT_ID=${this.userConfig.msalWebClientId}
TENANT_ID=${this.userConfig.msalTenantId}
AUTH0_CLIENT_ID=${this.userConfig.auth0ClientId}
AUTH0_CLIENT_SECRET=${this.userConfig.auth0ClientSecret}
AUTH0_DOMAIN=${this.userConfig.auth0Domain}
    fs.writeFileSync('.env', dotenvContent);
    this.log('\n\nBootstrapping MJAPI...');
    this.log('   Running npm link for generated code...');
    execSync('npm link ../GeneratedEntities ../GeneratedActions', { stdio: 'inherit', cwd: MJAPI_DIR });
    this.log('Running CodeGen...');
    this.renameFolderToMJ_BASE(this.userConfig.dbDatabase);
    // We do not manually run the compilation for GeneratedEntities because CodeGen handles that, but notice above that we did npm install for GeneratedEntities otherwise when CodeGen attempts to compile it, it will fail.
    this.config.runCommand('codegen');
    this.log('\nProcessing MJExplorer...');
    this.log('\n   Updating environment files...');
      CLIENT_ID: this.userConfig.msalWebClientId,
      TENANT_ID: this.userConfig.msalTenantId,
      CLIENT_AUTHORITY: this.userConfig.msalTenantId ? `https://login.microsoftonline.com/${this.userConfig.msalTenantId}` : '',
      AUTH_TYPE: this.userConfig.authType === 'AUTH0' ? 'auth0' : this.userConfig.authType.toLowerCase(),
      AUTH0_DOMAIN: this.userConfig.auth0Domain,
      AUTH0_CLIENTID: this.userConfig.auth0ClientId,
    await this.updateEnvironmentFiles(path.join(MJEXPLORER_DIR, 'src', 'environments'), config);
    this.log('   Running npm link for GeneratedEntities...');
    execSync('npm link ../GeneratedEntities', { stdio: 'inherit', cwd: MJEXPLORER_DIR });
    this.log('Installation complete!');
  async getUserConfiguration() {
    let userConfig: Config | undefined;
      const configObject = await fs.readJSON('install.config.json');
      userConfig = configSchema.parse(configObject);
      if (e instanceof ZodError) {
        this.log(
          `Invalid config file found at '${path.join(fs.realpathSync('.'), 'install.config.json')}'${
            this.flags.verbose ? '' : ', retry with --verbose for details'
          console.table(e.issues);
        this.log(`No config file found at '${path.join(fs.realpathSync('.'), 'install.config.json')}'`);
    if (!userConfig) {
        '\n>>> Please answer the following questions to setup the .env files for CodeGen. After this process you can manually edit the .env file as desired.'
      const dbUrl = await input({
        message: 'Enter the database server hostname:',
        validate: (v) => configSchema.shape.dbDatabase.safeParse(v).success,
      const dbInstance = await input({
        message: 'If you are using a named instance on that server, if so, enter the name here, if not leave blank:',
      const dbTrustServerCertificate = (await confirm({
        message: 'Does the database server use a self-signed certificate? If you are using a local instance, enter Y:',
        ? 'Y'
        : 'N';
      const dbDatabase = await input({
        message: 'Enter the database name on that server:',
      const dbPort = await input({
        message: 'Enter the port the database server listens on',
        validate: (v) => configSchema.shape.dbPort.safeParse(v).success,
        default: '1433',
      const codeGenLogin = await input({ message: 'Enter the database login for CodeGen:' });
      const codeGenPwD = await input({ message: 'Enter the database password for CodeGen:' });
        '\n>>> Please answer the following questions to setup the .env files for MJAPI. After this process you can manually edit the .env file in CodeGen as desired.'
      const mjAPILogin = await input({ message: 'Enter the database login for MJAPI:' });
      const mjAPIPwD = await input({ message: 'Enter the database password for MJAPI:' });
      const graphQLPort = await input({
        message: 'Enter the port to use for the GraphQL API',
        validate: (v) => configSchema.shape.graphQLPort.safeParse(v).success,
        default: '4000',
      const authType = await select({
        message: 'Will you be using Microsoft Entra (formerly Azure AD), Auth0, or both for authentication services for MJAPI:',
          { name: 'Microsoft Entra (MSAL)', value: 'MSAL' },
          { name: 'Auth0', value: 'AUTH0' },
          { name: 'Both', value: 'BOTH' },
      const msalTenantId = ['BOTH', 'MSAL'].includes(authType) ? await input({ message: 'Enter the web client ID for Entra:' }) : '';
      const msalWebClientId = ['BOTH', 'MSAL'].includes(authType) ? await input({ message: 'Enter the tenant ID for Entra:' }) : '';
      const auth0ClientId = ['BOTH', 'AUTH0'].includes(authType) ? await input({ message: 'Enter the client ID for Auth0:' }) : '';
      const auth0ClientSecret = ['BOTH', 'AUTH0'].includes(authType) ? await input({ message: 'Enter the client secret for Auth0:' }) : '';
      const auth0Domain = ['BOTH', 'AUTH0'].includes(authType) ? await input({ message: 'Enter the domain for Auth0:' }) : '';
      const createNewUser = await confirm({ message: 'Do you want to create a new user in the database? (Y/N):' });
      const userEmail = createNewUser
        ? await input({
            message: 'Enter the new user email',
            validate: (v) => configSchema.shape.userEmail.safeParse(v).success,
      const userFirstName = createNewUser ? await input({ message: 'Enter the new user first name:' }) : '';
      const userLastName = createNewUser ? await input({ message: 'Enter the new user last name::' }) : '';
      const userName = createNewUser
        ? await input({ message: 'Enter the new user name (leave blank to use email):', default: userEmail })
      const openAIAPIKey = await input({ message: 'Enter the OpenAI API Key (leave blank if not using):' });
      const anthropicAPIKey = await input({ message: 'Enter the Anthropic API Key (leave blank if not using):' });
      const mistralAPIKey = await input({ message: 'Enter the Mistral API Key (leave blank if not using):' });
      userConfig = configSchema.parse({
        createNewUser: createNewUser ? 'Y' : 'N',
        mistralAPIKey,
    return userConfig;
   * Verifies that the specified directories exist.
   * @param {...string} dirs - The directories to check.
  verifyDirs(...dirs: string[]) {
    dirs.forEach((dir) => {
      if (!fs.existsSync(dir)) {
        this.error(`Unable to locate required package at '${path.join(fs.realpathSync('.'), dir)}'`, {
          exit: 1,
          suggestions: ['Run the install from the same directory as the extracted MemberJunction distribution'],
  checkNodeVersion() {
    const validNodeVersion = Number(process.version.replace(/^v(\d+).*/, '$1')) >= 20;
    if (!validNodeVersion) {
      this.error('MemberJunction requires Node.js version 20 or higher.', { exit: 1 });
   * Checks if there is at least `numGB` GB of free disk space.
   * @param {number} numGB - The number of GB to check for.
   * @returns {boolean} True if there is enough free disk space, false otherwise.
  checkAvailableDiskSpace(numGB = 2) {
      this.log(`Checking for at least ${numGB}GB of free disk space...`);
        // For POSIX systems, check the root directory
        const command = `df -k / | tail -1 | awk '{ print $4; }'`;
        this.log(`   Sufficient disk space available: ${Math.round(freeSpace / GBToBytes)} GB`);
        this.error(`Insufficient disk space. Required: ${requiredSpace} bytes, Available: ${Math.round(freeSpace / GBToBytes)} GB`, {
      this.logToStderr(this.toErrorJson(error));
      this.error('Error checking disk space', { exit: 1 });
  async updateEnvironmentFiles(dirPath: string, config: Record<string, string | undefined>) {
      const files = await fs.readdir(dirPath);
      const envFiles = files.filter((file) => envFilePattern.test(file));
          this.log(`Updating ${file}`);
        const data = await fs.readFile(filePath, 'utf8');
        Object.entries(config).forEach(([key, value = '']) => {
          const regex = new RegExp(`(["\']?${key}["\']?:\\s*["\'])([^"\']*)(["\'])`, 'g');
          const escapedValue = value.replaceAll('$', () => '$$');
          updatedData = updatedData.replace(regex, `$1${escapedValue}$3`);
        await fs.writeFile(filePath, updatedData, 'utf8');
        this.log(`Updated ${file}`);
  renameFolderToMJ_BASE(dbDatabase: string) {
    const oldFolderPath = path.join(SQL_SCRIPTS_DIR, GENERATED_DIR, MJ_BASE_DIR);
    const newFolderPath = path.join(SQL_SCRIPTS_DIR, GENERATED_DIR, dbDatabase); // Assuming dbDatabase holds the new name
    if (!fs.existsSync(oldFolderPath)) {
      this.warn(`SQL scripts not found at '${oldFolderPath}', skipping rename`);
      fs.moveSync(oldFolderPath, newFolderPath);
      this.log(`Renamed ${oldFolderPath} to ${newFolderPath} successfully.`);
      this.logToStderr(`An error occurred while renaming the '${oldFolderPath}' folder:`, err);
   * Updates newUserSetup in the mj.config.cjs file.
  async updateConfigNewUserSetup(userName?: string, firstName?: string, lastName?: string, email?: string) {
      // Read the mj.config.cjs file
      const configFileContent = await fs.readFile('mj.config.cjs', 'utf8');
      // Parse the content into an AST
      const ast = recast.parse(configFileContent);
      // Modify the AST
      const n = recast.types.namedTypes;
      const b = recast.types.builders;
      recast.types.visit(ast, {
        visitObjectExpression(path) {
          const properties = path.node.properties;
          // Check if newUserSetup key exists
          const newUserSetupProperty = properties.find(
            (prop) => n.Property.check(prop) && n.Identifier.check(prop.key) && prop.key.name === 'newUserSetup'
          const newUserSetupValue = b.objectExpression([
            b.property('init', b.identifier('userName'), b.literal(userName || '')),
            b.property('init', b.identifier('firstName'), b.literal(firstName || '')),
            b.property('init', b.identifier('lastName'), b.literal(lastName || '')),
            b.property('init', b.identifier('email'), b.literal(email || '')),
          if (newUserSetupProperty && newUserSetupProperty.type === 'Property') {
            // Overwrite the existing newUserSetup key with an object
            newUserSetupProperty.value = newUserSetupValue;
            // Add a new newUserSetup key
            properties.push(b.property('init', b.identifier('newUserSetup'), newUserSetupValue));
          return false; // Stop traversing this path
      // Serialize the AST back to a string
      const updatedConfigFileContent = recast.prettyPrint(ast).code;
      // Write the updated content back to the file
      await fs.writeFile('mj.config.cjs', updatedConfigFileContent);
      this.log(`      Updated mj.config.cjs`);
      this.logToStderr('Error:', err);
import type { MigrateResult, MigrationExecutionResult, ResolvedMigration } from '@memberjunction/skyway-core';
export default class Migrate extends Command {
  static description = 'Migrate MemberJunction database to latest version';
    `<%= config.bin %> <%= command.id %> --schema __BCSaaS --dir ./migrations/v1
    `<%= config.bin %> <%= command.id %> --schema __BCSaaS --tag v1.0.0
    tag: Flags.string({ char: 't', description: 'Version tag to use for running remote migrations' }),
    schema: Flags.string({ char: 's', description: 'Target schema (overrides coreSchema from config)' }),
    dir: Flags.string({ description: 'Migration source directory (overrides migrationsLocation from config)' }),
    const { flags } = await this.parse(Migrate);
    const targetSchema = flags.schema || config.coreSchema;
    const skywayConfig = await getSkywayConfig(config, flags.tag, flags.schema, flags.dir);
    // Always capture progress for error diagnostics; verbose mode prints it live
    const errorLog: string[] = [];
    const failedMigrations: MigrationExecutionResult[] = [];
    let lastMigrationStarted: ResolvedMigration | undefined;
    skyway.OnProgress({
      OnLog: (msg) => {
        errorLog.push(msg);
        if (flags.verbose) this.log(`  ${msg}`);
      OnMigrationStart: (m) => {
        lastMigrationStarted = m;
        if (flags.verbose) this.log(`  Applying: ${m.Version ?? '(repeatable)'} — ${m.Description}`);
      OnMigrationEnd: (r) => {
        if (!r.Success) failedMigrations.push(r);
        if (flags.verbose) this.log(`  ${r.Success ? 'OK' : 'FAIL'}: ${r.Migration.Description} (${r.ExecutionTimeMS}ms)`);
      this.log(`Database Connection: ${config.dbHost}:${config.dbPort}, ${config.dbDatabase}, User: ${config.codeGenLogin}`);
      this.log(`Migrating ${targetSchema} schema using migrations from:\n\t- ${skywayConfig.Migrations.Locations.join('\n\t- ')}\n`);
      this.log(`Skyway config: baselineVersion: ${config.baselineVersion ?? '(auto-detect)'}, baselineOnMigrate: ${config.baselineOnMigrate}\n`);
    if (flags.tag) {
      this.log(`Migrating to ${flags.tag}`);
    const spinner = ora('Running migrations...');
    let result: MigrateResult;
      result = await skyway.Migrate();
      this.logToStderr(`\nMigration error: ${message}\n`);
      this.printCallbackErrors(failedMigrations, lastMigrationStarted, errorLog);
      this.error('Migrations failed');
      this.log(`Migrations complete in ${(result.TotalExecutionTimeMS / 1000).toFixed(1)}s — ${result.MigrationsApplied} applied`);
      if (result.CurrentVersion && flags.verbose) {
        this.log(`\tCurrent version: ${result.CurrentVersion}`);
      if (flags.verbose && result.Details.length > 0) {
        for (const detail of result.Details) {
          this.log(`\t${detail.Migration.Version ?? '(R)'} ${detail.Migration.Description} — ${detail.ExecutionTimeMS}ms`);
      this.logToStderr(`\nMigration failed: ${result.ErrorMessage ?? 'unknown error'}\n`);
      if (result.Details.length > 0) {
        // We have per-migration details — show them
        const succeeded = result.Details.filter(d => d.Success);
        if (succeeded.length > 0) {
          this.logToStderr(`  Applied ${succeeded.length} migration(s) before failure:`);
          for (const detail of succeeded) {
            this.logToStderr(`    OK: ${detail.Migration.Filename} (${detail.ExecutionTimeMS}ms)`);
          this.logToStderr('');
        const failed = result.Details.filter(d => !d.Success);
        for (const detail of failed) {
          this.logToStderr(`  FAILED: ${detail.Migration.Filename}`);
          this.logToStderr(`    Script: ${detail.Migration.FilePath}`);
          this.logToStderr(`    Version: ${detail.Migration.Version ?? '(repeatable)'}`);
          this.logToStderr(`    Description: ${detail.Migration.Description}`);
          if (detail.Error) {
            this.logToStderr(`    Error: ${detail.Error.message}`);
        // Details is empty — error was caught at the transaction/connection level.
        // Fall back to errors captured by OnProgress callbacks.
   * Prints error details captured by OnProgress callbacks.
   * Used when Skyway's result.Details is empty (transaction-level errors).
  private printCallbackErrors(
    failedMigrations: MigrationExecutionResult[],
    lastMigrationStarted: ResolvedMigration | undefined,
    errorLog: string[]
    // Show any migration failures captured by OnMigrationEnd
    if (failedMigrations.length > 0) {
      for (const detail of failedMigrations) {
    } else if (lastMigrationStarted) {
      // OnMigrationEnd never fired, but we know which migration was running
      this.logToStderr(`  Failed while executing: ${lastMigrationStarted.Filename}`);
      this.logToStderr(`    Script: ${lastMigrationStarted.FilePath}`);
      this.logToStderr(`    Version: ${lastMigrationStarted.Version ?? '(repeatable)'}`);
    // Show relevant log messages from Skyway (error/failure lines)
    const relevantLogs = errorLog.filter(
      msg => msg.toLowerCase().includes('fail') || msg.toLowerCase().includes('error') || msg.toLowerCase().includes('rolled back')
    if (relevantLogs.length > 0) {
      this.logToStderr('  Skyway log:');
      for (const msg of relevantLogs) {
        this.logToStderr(`    ${msg}`);
export default class Test extends Command {
  static description = 'MemberJunction Testing Framework - Execute and manage tests';
    '<%= config.bin %> <%= command.id %> run <test-id>',
    '<%= config.bin %> <%= command.id %> run --name="Active Members Count"',
    '<%= config.bin %> <%= command.id %> suite <suite-id>',
    '<%= config.bin %> <%= command.id %> list',
    '<%= config.bin %> <%= command.id %> list --suites',
    '<%= config.bin %> <%= command.id %> validate --all',
    this.log('MemberJunction Testing Framework\n');
    this.log('Available commands:');
    this.log('  mj test run <test-id>           - Execute a single test');
    this.log('  mj test suite <suite-id>        - Execute a test suite');
    this.log('  mj test list                    - List available tests');
    this.log('  mj test validate                - Validate test definitions');
    this.log('  mj test history                 - View test execution history');
    this.log('  mj test compare                 - Compare test runs for regressions');
    this.log('');
    this.log('Legacy commands (file-based evals):');
    this.log('  mj test eval <eval-id>          - Run file-based AI evaluation');
    this.log('  mj test report --type=eval      - Generate eval report');
    this.log('Run "mj test COMMAND --help" for more information on a command.');
import { from, tap } from 'rxjs';
import { ___serverPort } from "./config";
import { ___runObject, handleServerInit } from './util';
import { RunCodeGenBase, SQLCodeGenBase } from '@memberjunction/codegen-lib';
import AppDataSource from '@memberjunction/codegen-lib/dist/Config/db-connection';
// get the server up and running
app.listen(___serverPort, () => console.log('Server starting up...'));
// start the initialization process
const serverInit$ = from(handleServerInit()).pipe(
  tap(() => console.log(`🚀 Server listening on port ${___serverPort}!\n`)) // Use tap for side effects
serverInit$.subscribe({
    // Start listening for requests only after initialization is complete
//    app.post ('/api/entity-permissions', handleEntityPermissions);
    app.post ('/api/run', handleRunCodeGen);
  error: (err) => console.error(`Initialization failed: ${err}`),
let runningPromise: Promise<void> | null = null;
async function handleRunCodeGen() {
    if (runningPromise) {
      return runningPromise;
      runningPromise = new Promise(async () => {
        // We now need to process the entity permissions for the entities specified
        const codeGen = MJGlobal.Instance.ClassFactory.CreateInstance<RunCodeGenBase>(RunCodeGenBase);
        if (!codeGen) {
          throw new Error("Couldn't get CodeGenRun object!!!")
        await codeGen?.Run();
// async function handleEntityPermissions(req: any, res: any) {
//   if (!req || !req.body) 
//     res.status(400).send('Invalid request');
//   else {
//     const params = req.body;
//     // params should have a single property in it, entityIDArray, which is an array of entity IDs to update
//     const entityIDArray = params.entityIDArray;
//     if (!entityIDArray || !Array.isArray(entityIDArray)) {
//       res.status(400).send('Invalid request');
//     else {
//       // we now need to process the entity permissions for the entities specified
//       const sqlCodeGenObject = MJGlobal.Instance.ClassFactory.CreateInstance<SQLCodeGenBase>(SQLCodeGenBase);
//       if (!sqlCodeGenObject) {
//         res.status(500).send('Failed to create SQLCodeGenBase instance');
//         return;
//         const md = new Metadata();
//         // force a metadata refresh because the permissions might be out of date
//         console.log('Request received to update entity permissions for entities: ', entityIDArray.map(e => e.toString()).join(', '));
//         console.log('Refreshing metadata...');
//         await md.Refresh()
//         const entities = md.Entities.filter(e => entityIDArray.includes(e.ID));
//         await sqlCodeGenObject.generateAndExecuteEntitySQLToSeparateFiles({
//           ds: AppDataSource, 
//           entities: entities, 
//           directory: '', 
//           onlyPermissions: true, 
//           writeFiles: false,
//           skipExecution: false
//         })
//         res.status(200).send({ status: 'ok' });
//         console.log('Entity permissions updated successfully');
//       } catch (err: any) {
//         res.status(500).send({ status: 'error', errorMessage: err.message });
import { BaseEntity } from "./generic/baseEntity";
import { Metadata } from "./generic/metadata";
import { RunQuery } from "./generic/runQuery";
import { RunReport } from "./generic/runReport";
import { RunView } from "./views/runView";
export * from "./generic/metadata";
export * from "./generic/baseInfo";
export * from "./generic/baseEngine";
export * from "./views/runView";
export * from "./generic/runReport";
export * from "./generic/runQuery";
export * from "./generic/interfaces";
export * from "./generic/baseEntity";
export * from "./generic/applicationInfo";
export * from "./generic/providerBase";
export * from "./generic/entityInfo";
export * from "./generic/securityInfo";
export * from "./generic/transactionGroup";
export * from "./generic/util";
export * from "./generic/logging";
export * from "./generic/queryInfo";
export * from "./generic/queryInfoInterfaces";
export * from "./generic/querySQLFilters";
export * from "./generic/runQuerySQLFilterImplementations";
export * from "./generic/libraryInfo";
export * from "./generic/QueryCacheConfig";
export * from "./generic/QueryCache";
export * from "./generic/explorerNavigationItem";
export * from "./generic/compositeKey";
export * from "./generic/authEvaluator";
export * from "./generic/metadataUtil";
export * from "./generic/authTypes";
export * from "./generic/graphqlTypeNames";
export * from "./generic/databaseProviderBase";
export * from "./generic/baseEngineRegistry";
export * from "./generic/localCacheManager";
export * from "./generic/RegisterForStartup";
export * from "./generic/telemetryManager";
export * from "./generic/InMemoryLocalStorageProvider";
export function SetProvider(provider) {
    Metadata.Provider = provider;
    BaseEntity.Provider = provider;
    RunView.Provider = provider;
    RunReport.Provider = provider;
    RunQuery.Provider = provider;
export * from './generated/entity_subclasses'
export * from "./custom/UserViewEntity";
export * from './custom/DashboardEntityExtended';
export * from './custom/ListDetailEntityExtended';
export * from './custom/ScheduledActionExtended';
export * from './custom/EntityEntityExtended';
export * from './custom/EntityFieldEntityExtended';
export * from './custom/ComponentEntityExtended';
export * from './custom/EnvironmentEntityExtended';
export * from './custom/TemplateEntityExtended';
export * from './custom/ResourcePermissions/ResourcePermissionEngine';
export * from './custom/ResourcePermissions/ResourcePermissionSubclass';
export * from './custom/ResourcePermissions/ResourceData';
export * from './engines/component-metadata';
export * from './engines/TypeTablesCache';
export * from './engines/artifacts';
export * from './engines/dashboards';
export * from './engines/EncryptionEngineBase';
export * from './engines/UserInfoEngine';
export * from './engines/UserViewEngine';
export * from './engines/FileStorageEngine';
export * from './engines/MCPEngine';
export * from './artifact-extraction/artifact-extract-rules';
export * from './artifact-extraction/artifact-extractor';export * from './custom/AIPromptEntityExtended.server';
export * from './custom/AIPromptRunEntity.server';
export * from './custom/ConversationDetailEntity.server';
export * from './custom/DuplicateRunEntity.server';
export * from './custom/QueryEntity.server';
export * from './custom/reportEntity.server';
export * from './custom/TemplateContentEntity.server';
export * from './custom/userViewEntity.server';
export * from './custom/ActionEntity.server';
export * from './custom/ApplicationEntity.server';
export * from './custom/ComponentEntity.server';
export * from './custom/ArtifactVersionExtended.server';
export * from './custom/AIAgentNoteEntity.server';
export * from './custom/AIAgentExampleEntity.server';
export * from './custom/util';
export * from './custom/sql-parser';export * from './types'
import { DataContextItem } from "@memberjunction/data-context";
@RegisterClass(DataContextItem, undefined, undefined, true) 
export class DataContextItemServer extends DataContextItem {
     * Server-Side only method to load the data context item from a SQL statement
     * @param dataSource - The data source to load the data context item from (this implementation uses mssql ConnectionPool object)
     * @param contextUser - The user that is requesting the data context 
    protected override async LoadFromSQL(dataSource: any, contextUser?: UserInfo): Promise<boolean> {
            const pool = dataSource as sql.ConnectionPool;
            const request = new sql.Request(pool);
            const result = await request.query(this.SQL);
            this.Data = result.recordset;
            return true; // if we get here the above query didn't fail by throwing an exception which would get caught below
            this.DataLoadingError = `Error loading data context item from SQL: ${e && e.message ? e.message : e}`
// Base class
export { BaseExporter } from './base-exporter';
// Exporters
export { ExcelExporter } from './excel-exporter';
export { CSVExporter } from './csv-exporter';
export { JSONExporter } from './json-exporter';
// Main engine
export { ExportEngine } from './export-engine';
// Export all types and utilities
export { ClassFactory, ClassRegistration } from './ClassFactory'
export * from './interface'
export * from './util'
export * from './ObjectCache'
export * from './BaseSingleton'
export * from './DeepDiff'
export * from './ClassUtils'
export * from './util/PatternUtils';
export * from './ValidationTypes'
export * from './JSONValidator'
export * from './SafeExpressionEvaluator'
export * from './SQLExpressionValidator'
export * from './warningManager'
export * from './EncryptionUtils'
// NOTE: TelemetryManager has moved to @memberjunction/core
// Import from there instead of here
// Export the main classes
export * from './Global'
export * from './RegisterClass'
// NOTE: RegisterForStartup has moved to @memberjunction/core
export * from './generic/QueueBase';
export * from './generic/QueueManager';
export * from './drivers/AIActionQueue';import { expressMiddleware } from '@apollo/server/express4';
import { mergeSchemas } from '@graphql-tools/schema';
import { setupSQLServerClient, SQLServerProviderConfigData, UserCache } from '@memberjunction/sqlserver-dataprovider';
import { extendConnectionPoolWithQuery } from './util.js';
import { default as BodyParser } from 'body-parser';
import compression from 'compression'; // Add compression middleware
import express, { Application } from 'express';
import { default as fg } from 'fast-glob';
import { useServer } from 'graphql-ws/lib/use/ws';
import { createServer } from 'node:http';
import { sep } from 'node:path';
import { ReplaySubject } from 'rxjs';
import { BuildSchemaOptions, buildSchemaSync, GraphQLTimestamp } from 'type-graphql';
import { WebSocketServer } from 'ws';
import buildApolloServer from './apolloServer/index.js';
import { configInfo, dbDatabase, dbHost, dbPort, dbUsername, graphqlPort, graphqlRootPath, mj_core_schema, websiteRunFromPackage, RESTApiOptions } from './config.js';
import { contextFunction, getUserPayload } from './context.js';
import { requireSystemUserDirective, publicDirective } from './directives/index.js';
import { setupRESTEndpoints } from './rest/setupRESTEndpoints.js';
import { createOAuthCallbackHandler } from './rest/OAuthCallbackHandler.js';
import { DataSourceInfo, raiseEvent } from './types.js';
import { ExternalChangeDetectorEngine } from '@memberjunction/external-change-detection';
import { ScheduledJobsService } from './services/ScheduledJobsService.js';
import { LocalCacheManager, StartupManager, TelemetryManager, TelemetryLevel } from '@memberjunction/core';
import { getSystemUser } from './auth/index.js';
const cacheRefreshInterval = configInfo.databaseSettings.metadataCacheRefreshInterval;
export { MaxLength } from 'class-validator';
export * from 'type-graphql';
export { NewUserBase } from './auth/newUsers.js';
export { configInfo, DEFAULT_SERVER_CONFIG } from './config.js';
export * from './directives/index.js';
export * from './entitySubclasses/entityPermissions.server.js';
    TokenExpiredError,
    getSystemUser,
    getSigningKeys,
    extractUserInfoFromPayload,
    verifyUserRecord,
    AuthProviderFactory,
    IAuthProvider,
} from './auth/index.js';
export * from './auth/APIKeyScopeAuth.js';
export * from './generic/PushStatusResolver.js';
export * from './generic/ResolverBase.js';
export * from './generic/RunViewResolver.js';
export * from './resolvers/RunTemplateResolver.js';
export * from './resolvers/RunAIPromptResolver.js';
export * from './resolvers/RunAIAgentResolver.js';
export * from './resolvers/TaskResolver.js';
export * from './generic/KeyValuePairInput.js';
export * from './generic/KeyInputOutputTypes.js';
export * from './generic/DeleteOptionsInput.js';
export * from './agents/skip-agent.js';
export * from './agents/skip-sdk.js';
export * from './resolvers/ColorResolver.js';
export * from './resolvers/ComponentRegistryResolver.js';
export * from './resolvers/DatasetResolver.js';
export * from './resolvers/EntityRecordNameResolver.js';
export * from './resolvers/MergeRecordsResolver.js';
export * from './resolvers/ReportResolver.js';
export * from './resolvers/QueryResolver.js';
export * from './resolvers/SqlLoggingConfigResolver.js';
export * from './resolvers/SyncRolesUsersResolver.js';
export * from './resolvers/SyncDataResolver.js';
export * from './resolvers/GetDataResolver.js';
export * from './resolvers/GetDataContextDataResolver.js';
export * from './resolvers/TransactionGroupResolver.js';
export * from './resolvers/CreateQueryResolver.js';
export * from './resolvers/TelemetryResolver.js';
export * from './resolvers/APIKeyResolver.js';
export * from './resolvers/MCPResolver.js';
export * from './resolvers/ActionResolver.js';
export * from './resolvers/EntityCommunicationsResolver.js';
export * from './resolvers/EntityResolver.js';
export * from './resolvers/ISAEntityResolver.js';
export * from './resolvers/FileCategoryResolver.js';
export * from './resolvers/FileResolver.js';
export * from './resolvers/InfoResolver.js';
export * from './resolvers/PotentialDuplicateRecordResolver.js';
export * from './resolvers/RunTestResolver.js';
export * from './resolvers/UserFavoriteResolver.js';
export * from './resolvers/UserResolver.js';
export * from './resolvers/UserViewResolver.js';
export * from './resolvers/VersionHistoryResolver.js';
export { GetReadOnlyDataSource, GetReadWriteDataSource, GetReadWriteProvider, GetReadOnlyProvider } from './util.js';
export * from './generated/generated.js';
export type MJServerOptions = {
  onBeforeServe?: () => void | Promise<void>;
  restApiOptions?: Partial<RESTApiOptions>; // Options for REST API configuration
const localPath = (p: string) => {
  // Convert import.meta.url to a local directory path
  const dirname = fileURLToPath(new URL('.', import.meta.url));
  // Resolve the provided path relative to the derived directory path
  const resolvedPath = resolve(dirname, p);
export const createApp = (): Application => express();
export const serve = async (resolverPaths: Array<string>, app: Application = createApp(), options?: MJServerOptions): Promise<void> => {
  const localResolverPaths = ['resolvers/**/*Resolver.{js,ts}', 'generic/*Resolver.{js,ts}', 'generated/generated.{js,ts}'].map(localPath);
  const combinedResolverPaths = [...resolverPaths, ...localResolverPaths];
  const isWindows = sep === '\\';
  const globs = combinedResolverPaths.flatMap((path) => (isWindows ? path.replace(/\\/g, '/') : path));
  const paths = fg.globSync(globs);
    console.warn(`No resolvers found in ${combinedResolverPaths.join(', ')}`);
    console.log({ combinedResolverPaths, paths, cwd: process.cwd() });
  const pool = new sql.ConnectionPool(createMSSQLConfig());
  const setupComplete$ = new ReplaySubject(1);
  const dataSources = [new DataSourceInfo({dataSource: pool, type: 'Read-Write', host: dbHost, port: dbPort, database: dbDatabase, userName: dbUsername})];
  // Establish a second read-only connection to the database if dbReadOnlyUsername and dbReadOnlyPassword exist
  let readOnlyPool: sql.ConnectionPool | null = null;
  if (configInfo.dbReadOnlyUsername && configInfo.dbReadOnlyPassword) {
      user: configInfo.dbReadOnlyUsername,
      password: configInfo.dbReadOnlyPassword,
    readOnlyPool = new sql.ConnectionPool(readOnlyConfig);
    await readOnlyPool.connect();
    // since we created a read-only pool, add it to the list of data sources
    dataSources.push(new DataSourceInfo({dataSource: readOnlyPool, type: 'Read-Only', host: dbHost, port: dbPort, database: dbDatabase, userName: configInfo.dbReadOnlyUsername}));
    console.log('Read-only Connection Pool has been initialized.');
  const config = new SQLServerProviderConfigData(pool, mj_core_schema, cacheRefreshInterval);
  await setupSQLServerClient(config); // datasource is already initialized, so we can setup the client right away
  console.log(`Data Source has been initialized. ${md?.Entities ? md.Entities.length : 0} entities loaded.`);
  // Initialize server telemetry based on config
  if (configInfo.telemetry?.enabled) {
    tm.SetEnabled(true);
    if (configInfo.telemetry?.level) {
      tm.UpdateSettings({ level: configInfo.telemetry.level as TelemetryLevel });
    console.log(`Server telemetry enabled with level: ${configInfo.telemetry.level || 'standard'}`);
    tm.SetEnabled(false);
    console.log('Server telemetry disabled');
  // Initialize LocalCacheManager with the server-side storage provider (in-memory)
  await LocalCacheManager.Instance.Initialize(Metadata.Provider.LocalStorageProvider);
  console.log('LocalCacheManager initialized');
  setupComplete$.next(true);
  raiseEvent('setupComplete', dataSources, null,  this);
  /******TEST HARNESS FOR CHANGE DETECTION */
  // const cd = ExternalChangeDetectorEngine.Instance;
  // await cd.Config(false, UserCache.Users[0]);
  // // don't wait for this, just run it and show in console whenever done.
  // cd.DetectChangesForAllEligibleEntities().then(result => {
  //   console.log(result)
  //   cd.ReplayChanges(result.Changes).then(replayResult => {
  //     console.log(replayResult)
  const dynamicModules = await Promise.all(
    paths.map((modulePath) => {
        const module = import(isWindows ? `file://${modulePath}` : modulePath);
        console.error(`Error loading dynamic module at '${modulePath}'`, e);
  const resolvers = dynamicModules.flatMap((module) =>
    Object.values(module).filter((value) => typeof value === 'function')
  ) as BuildSchemaOptions['resolvers'];
  let schema = mergeSchemas({
      buildSchemaSync({
        resolvers,
        validate: false,
        scalarsMap: [{ type: Date, scalar: GraphQLTimestamp }],
        emitSchemaFile: websiteRunFromPackage !== 1,
    typeDefs: [requireSystemUserDirective.typeDefs, publicDirective.typeDefs],
  schema = requireSystemUserDirective.transformer(schema);
  schema = publicDirective.transformer(schema);
  const httpServer = createServer(app);
  const webSocketServer = new WebSocketServer({ server: httpServer, path: graphqlRootPath });
  const serverCleanup = useServer(
      context: async ({ connectionParams }) => {
        const userPayload = await getUserPayload(String(connectionParams?.Authorization), undefined, dataSources);
        return { userPayload };
      onError: (ctx, message, errors) => {
        // Check if error is token expiration (expected behavior)
        const isTokenExpired = errors.some(err =>
          err.extensions?.code === 'JWT_EXPIRED' ||
          err.message?.includes('token has expired')
          // Log at warn level - this is expected from long-lived browser sessions
          console.warn('WebSocket connection token expired - client should reconnect with refreshed token');
          // Log actual errors at error level
          console.error('WebSocket error:', errors);
    webSocketServer
  const apolloServer = buildApolloServer({ schema }, { httpServer, serverCleanup });
  await apolloServer.start();
  // Fix #8: Add compression for better throughput performance
  app.use(compression({
    // Don't compress responses smaller than 1KB
    threshold: 1024,
    // Skip compression for images, videos, and other binary files
    filter: (req, res) => {
      if (req.headers['content-type']) {
        const contentType = req.headers['content-type'];
        if (contentType.includes('image/') || 
            contentType.includes('video/') ||
            contentType.includes('audio/') ||
            contentType.includes('application/octet-stream')) {
      return compression.filter(req, res);
    // High compression level (good balance between CPU and compression ratio)
    level: 6
  // Setup REST API endpoints BEFORE GraphQL (since graphqlRootPath may be '/' which catches all routes)
  const authMiddleware = async (req, res, next) => {
      const requestDomain = new URL(req.headers.origin || '').hostname;
      const apiKey = String(req.headers['x-mj-api-key']);
      const userPayload = await getUserPayload(bearerToken, sessionId, dataSources, requestDomain, apiKey);
      if (!userPayload) {
        return res.status(401).json({ error: 'Invalid token' });
      // Set both req.user (standard Express convention) and req['mjUser'] (MJ REST convention)
      // Note: userPayload contains { userRecord: UserInfo, email, sessionId }
      // The mjUser property expects the UserInfo directly (userRecord)
      req.user = userPayload;
      req['mjUser'] = userPayload.userRecord;
      console.error('Auth error:', error);
      return res.status(401).json({ error: 'Authentication failed' });
  // Build public URL for OAuth callbacks
  const oauthPublicUrl = configInfo.publicUrl || `${configInfo.baseUrl}:${configInfo.graphqlPort}${configInfo.graphqlRootPath || ''}`;
  console.log(`[OAuth] publicUrl: ${oauthPublicUrl}`);
  // Set up OAuth callback routes at /oauth (independent of REST API)
  // These must be registered BEFORE GraphQL middleware since graphqlRootPath may be '/'
  if (oauthPublicUrl) {
    const { callbackRouter, authenticatedRouter } = createOAuthCallbackHandler({
      publicUrl: oauthPublicUrl,
      // TODO: These should be configurable to point to the MJ Explorer UI
      successRedirectUrl: `${oauthPublicUrl}/oauth/success`,
      errorRedirectUrl: `${oauthPublicUrl}/oauth/error`
    // Create CORS middleware for OAuth routes (needed for cross-origin requests from frontend)
    const oauthCors = cors<cors.CorsRequest>();
    // OAuth callback is unauthenticated (called by external auth server)
    app.use('/oauth', oauthCors, callbackRouter);
    console.log('[OAuth] Callback route registered at /oauth/callback');
    // OAuth status, initiate, and exchange endpoints require authentication
    // Must also have CORS for frontend requests and JSON body parsing
    app.use('/oauth', oauthCors, BodyParser.json(), authMiddleware, authenticatedRouter);
    console.log('[OAuth] Authenticated routes registered at /oauth/status, /oauth/initiate, and /oauth/exchange');
  // Get REST API configuration
  const restApiConfig = {
    enabled: configInfo.restApiOptions?.enabled ?? false,
    includeEntities: configInfo.restApiOptions?.includeEntities,
    excludeEntities: configInfo.restApiOptions?.excludeEntities,
  // Apply options from server options if provided (these override the config file)
  if (options?.restApiOptions) {
    Object.assign(restApiConfig, options.restApiOptions);
  // Get REST API configuration from environment variables if present (env vars override everything)
  if (process.env.MJ_REST_API_ENABLED !== undefined) {
    restApiConfig.enabled = process.env.MJ_REST_API_ENABLED === 'true';
    if (restApiConfig.enabled) {
      console.log('REST API is enabled via environment variable');
  if (process.env.MJ_REST_API_INCLUDE_ENTITIES) {
    restApiConfig.includeEntities = process.env.MJ_REST_API_INCLUDE_ENTITIES.split(',').map(e => e.trim());
  if (process.env.MJ_REST_API_EXCLUDE_ENTITIES) {
    restApiConfig.excludeEntities = process.env.MJ_REST_API_EXCLUDE_ENTITIES.split(',').map(e => e.trim());
  // Set up REST endpoints with the configured options and auth middleware
  setupRESTEndpoints(app, restApiConfig, authMiddleware);
  // GraphQL middleware (after REST so /api/v1/* routes are handled first)
  // Note: Type assertion needed due to @apollo/server bundling older @types/express types
  // that are incompatible with Express 5.x types (missing 'param' property)
  app.use(
    cors<cors.CorsRequest>(),
    BodyParser.json({ limit: '50mb' }),
    expressMiddleware(apolloServer, {
      context: contextFunction({
                                 setupComplete$,
                                 dataSource: extendConnectionPoolWithQuery(pool), // default read-write data source
                                 dataSources // all data source
    }) as unknown as express.RequestHandler
  // Initialize and start scheduled jobs service if enabled
  let scheduledJobsService: ScheduledJobsService | null = null;
  if (configInfo.scheduledJobs?.enabled) {
      scheduledJobsService = new ScheduledJobsService(configInfo.scheduledJobs);
      await scheduledJobsService.Initialize();
      await scheduledJobsService.Start();
      console.error('❌ Failed to start scheduled jobs service:', error);
      // Don't throw - allow server to start even if scheduled jobs fail
  if (options?.onBeforeServe) {
    await Promise.resolve(options.onBeforeServe());
  await new Promise<void>((resolve) => httpServer.listen({ port: graphqlPort }, resolve));
  console.log(`📦 Connected to database: ${dbHost}:${dbPort}/${dbDatabase}`);
  console.log(`🚀 Server ready at http://localhost:${graphqlPort}/`);
  // Set up graceful shutdown handlers
  const gracefulShutdown = async (signal: string) => {
    console.log(`\n${signal} received, shutting down gracefully...`);
    // Stop scheduled jobs service
    if (scheduledJobsService?.IsRunning) {
        await scheduledJobsService.Stop();
        console.log('✅ Scheduled jobs service stopped');
        console.error('❌ Error stopping scheduled jobs service:', error);
    // Close server
    httpServer.close(() => {
      console.log('✅ HTTP server closed');
    // Force close after 10 seconds
      console.error('⚠️  Forced shutdown after timeout');
    }, 10000);
  process.on('SIGTERM', () => gracefulShutdown('SIGTERM'));
  process.on('SIGINT', () => gracefulShutdown('SIGINT'));
  // Handle unhandled promise rejections to prevent server crashes
  process.on('unhandledRejection', (reason, promise) => {
    console.error('❌ Unhandled Promise Rejection:', reason);
    console.error('   Promise:', promise);
    // Log the error but DO NOT crash the server
    // This is critical for server stability when downstream dependencies fail
import { ApolloServer, ApolloServerOptions } from '@apollo/server';
import { ApolloServerPluginDrainHttpServer } from '@apollo/server/plugin/drainHttpServer';
import { Disposable } from 'graphql-ws';
import { Server } from 'http';
import { enableIntrospection } from '../config.js';
import { AppContext } from '../types.js';
const buildApolloServer = (
  configOverride: ApolloServerOptions<AppContext>,
  { httpServer, serverCleanup }: { httpServer: Server; serverCleanup: Disposable }
) =>
  new ApolloServer({
    csrfPrevention: true,
    cache: 'bounded',
    plugins: [
      ApolloServerPluginDrainHttpServer({ httpServer }),
        async serverWillStart() {
            async drainServer() {
              await serverCleanup.dispose();
    introspection: enableIntrospection,
    ...configOverride,
export default buildApolloServer;
import { JwtHeader, SigningKeyCallback, JwtPayload } from 'jsonwebtoken';
import { Metadata, RoleInfo, UserInfo } from '@memberjunction/core';
import { MJUserEntity, MJUserEntityType } from '@memberjunction/core-entities';
import { AuthProviderFactory } from './AuthProviderFactory.js';
import { initializeAuthProviders } from './initializeProviders.js';
export { TokenExpiredError } from './tokenExpiredError.js';
export { IAuthProvider } from './IAuthProvider.js';
export { AuthProviderFactory } from './AuthProviderFactory.js';
export * from './APIKeyScopeAuth.js';
// This is a hard-coded forever constant due to internal migrations
class MissingAuthError extends Error {
    super('No authentication providers configured. Please configure at least one auth provider in mj.config.cjs');
    this.name = 'MissingAuthError';
const refreshUserCache = async (dataSource?: sql.ConnectionPool) => {
  const startTime: number = Date.now();
  await UserCache.Instance.Refresh(dataSource);
  const endTime: number = Date.now();
  const elapsed: number = endTime - startTime;
  // if elapsed time is less than the delay setting, wait for the additional time to achieve the full delay
  // the below also makes sure we never go more than a 30 second total delay
  const delay = configInfo.userHandling.updateCacheWhenNotFoundDelay
    ? configInfo.userHandling.updateCacheWhenNotFoundDelay < 30000
      ? configInfo.userHandling.updateCacheWhenNotFoundDelay
      : 30000
  if (elapsed < delay) await new Promise((resolve) => setTimeout(resolve, delay - elapsed));
  const finalTime: number = Date.now();
  const finalElapsed: number = finalTime - startTime;
    `   UserCache updated in ${elapsed}ms, total elapsed time of ${finalElapsed}ms including delay of ${delay}ms (if needed). Attempting to find the user again via recursive call`
 * Gets validation options for a specific issuer
 * This maintains backward compatibility with the old structure
export const getValidationOptions = (issuer: string): { audience: string; jwksUri: string } | undefined => {
  const provider = factory.getByIssuer(issuer);
    audience: provider.audience,
    jwksUri: provider.jwksUri
 * Backward compatible validationOptions object
 * @deprecated Use getValidationOptions() or AuthProviderRegistry instead
export const validationOptions: Record<string, { audience: string; jwksUri: string }> = new Proxy({}, {
  get: (target, prop: string) => {
    return getValidationOptions(prop);
  has: (target, prop: string) => {
    return getValidationOptions(prop) !== undefined;
  ownKeys: () => {
    return factory.getAllProviders().map(p => p.issuer);
export class UserPayload {
  aio?: string;
  aud?: string;
  oid?: string;
  rh?: string;
  tid?: string;
  uti?: string;
  ver?: string;
  [key: string]: unknown; // Allow additional claims
 * Gets signing keys for JWT validation
export const getSigningKeys = (issuer: string) => (header: JwtHeader, cb: SigningKeyCallback) => {
  // Initialize providers if not already done
  if (!factory.hasProviders()) {
    initializeAuthProviders();
    // Check if we have any providers at all
      throw new MissingAuthError();
    throw new Error(`No authentication provider found for issuer: ${issuer}`);
  provider.getSigningKey(header, cb);
 * Extracts user information from JWT payload using the appropriate provider
export const extractUserInfoFromPayload = (payload: JwtPayload): {
} => {
    // Fallback to default extraction
    const preferredUsername = payload.preferred_username as string | undefined;
      email: payload.email as string | undefined || preferredUsername,
      firstName: payload.given_name as string | undefined,
      lastName: payload.family_name as string | undefined,
      fullName: payload.name as string | undefined,
      preferredUsername
    const fullName = payload.name as string | undefined;
      firstName: payload.given_name as string | undefined || fullName?.split(' ')[0],
      lastName: payload.family_name as string | undefined || fullName?.split(' ')[1] || fullName?.split(' ')[0],
  return provider.extractUserInfo(payload);
export const getSystemUser = async (dataSource?: sql.ConnectionPool, attemptCacheUpdateIfNeeded: boolean = true): Promise<UserInfo> => {
    if (dataSource && attemptCacheUpdateIfNeeded) {
      console.warn(`System user not found in cache. Updating cache in attempt to find the user...`);
      await refreshUserCache(dataSource);
      return getSystemUser(dataSource, false); // try one more time but do not update cache next time if not found
    throw new Error(`System user ID '${UserCache.Instance.SYSTEM_USER_ID}' not found in database`);
  return systemUser;
export const verifyUserRecord = async (
  dataSource?: sql.ConnectionPool,
  attemptCacheUpdateIfNeeded: boolean = true
): Promise<UserInfo | undefined> => {
  if (!email) return undefined;
  let user = UserCache.Instance.Users.find((u) => {
    if (!u.Email || u.Email.trim() === '') {
      // this condition should never occur. If it doesn throw a console error including the user id
      // DB requires non-null but this is just an extra check and we could in theory have a blank string in the DB
      console.error(`SYSTEM METADATA ISSUE: User ${u.ID} has no email address`);
    } else return u.Email.toLowerCase().trim() === email.toLowerCase().trim();
      configInfo.userHandling.autoCreateNewUsers &&
      firstName &&
      lastName &&
      (requestDomain || configInfo.userHandling.newUserLimitedToAuthorizedDomains === false)
      // check to see if the domain that we have a request coming in from matches one of the domains in the autoCreateNewUsersDomains setting
      let passesDomainCheck: boolean =
        configInfo.userHandling.newUserLimitedToAuthorizedDomains ===
        false; /*in this first condition, we are set up to NOT care about domain */
      if (!passesDomainCheck && requestDomain) {
        /*in this second condition, we check the domain against authorized domains*/
        passesDomainCheck = configInfo.userHandling.newUserAuthorizedDomains.some((pattern) => {
          // Convert wildcard domain patterns to regular expressions
          const regex = new RegExp('^' + pattern.toLowerCase().trim().replace(/\./g, '\\.').replace(/\*/g, '.*') + '$');
          return regex.test(requestDomain?.toLowerCase().trim());
      if (passesDomainCheck) {
        // we have a domain from the request that matches one of the domains provided by the configuration, so we will create a new user
        console.warn(`User ${email} not found in cache. Attempting to create a new user...`);
        const newUserCreator: NewUserBase = MJGlobal.Instance.ClassFactory.CreateInstance<NewUserBase>(NewUserBase); // this will create the object that handles creating the new user for us
        const newUser: MJUserEntity | null = await newUserCreator.createNewUser(firstName, lastName, email);
        if (newUser) {
          // new user worked! we already have the stuff we need for the cache, so no need to go to the DB now, just create a new UserInfo object and use the return value from the createNewUser method
          // to init it, including passing in the role list for the user.
          const initData: MJUserEntityType & { UserRoles: { UserID: string; RoleName: string; RoleID: string }[] } = newUser.GetAll();
          initData.UserRoles = configInfo.userHandling.newUserRoles.map((role) => {
            const roleInfo: RoleInfo | undefined = md.Roles.find((r) => r.Name === role);
            const roleID: string = roleInfo ? roleInfo.ID : '';
            return { UserID: initData.ID, RoleName: role, RoleID: roleID };
          user = new UserInfo(Metadata.Provider, initData);
          UserCache.Instance.Users.push(user);
          console.warn(`   >>> New user ${email} created successfully!`);
          `User ${email} not found in cache. Request domain '${requestDomain}' does not match any of the domains in the newUserAuthorizedDomains setting. To ignore domain, make sure you set the newUserLimitedToAuthorizedDomains setting to false. In this case we are NOT creating a new user.`
    if (!user && configInfo.userHandling.updateCacheWhenNotFound && dataSource && attemptCacheUpdateIfNeeded) {
      // if we get here that means in the above, if we were attempting to create a new user, it did not work, or it wasn't attempted and we have a config that asks us to auto update the cache
      console.warn(`User ${email} not found in cache. Updating cache in attempt to find the user...`);
      return verifyUserRecord(email, firstName, lastName, requestDomain, dataSource, false); // try one more time but do not update cache next time if not found
// Initialize providers on module load
export * from './Public.js';
export * from './RequireSystemUser.js';
export * from './RESTEndpointHandler.js';
export * from './EntityCRUDHandler.js';
export * from './ViewOperationsHandler.js';
export * from './setupRESTEndpoints.js';
export * from './OAuthCallbackHandler.js';export * from './drivers/AWSFileStorage';
export * from './drivers/AzureFileStorage';
export * from './drivers/GoogleFileStorage';
export * from './drivers/GoogleDriveFileStorage';
export * from './drivers/SharePointFileStorage';
export * from './drivers/DropboxFileStorage';
export * from './drivers/BoxFileStorage';
export * from './generic/FileStorageBase';
// Core library exports
export { FileBackupManager } from './lib/file-backup-manager';
export { SyncEngine, DeferrableLookupError } from './lib/sync-engine';
export type { RecordData } from './lib/sync-engine';
export { ConfigManager, configManager } from './lib/config-manager';
export { getSyncEngine, resetSyncEngine } from './lib/singleton-manager';
export { SQLLogger } from './lib/sql-logger';
export { TransactionManager } from './lib/transaction-manager';
export { JsonWriteHelper } from './lib/json-write-helper';
export { FileWriteBatch } from './lib/file-write-batch';
export { JsonPreprocessor } from './lib/json-preprocessor';
export type { IncludeDirective } from './lib/json-preprocessor';
// Deletion audit exports
export { RecordDependencyAnalyzer } from './lib/record-dependency-analyzer';
export type { FlattenedRecord, ReverseDependency, DependencyAnalysisResult } from './lib/record-dependency-analyzer';
export { EntityForeignKeyHelper } from './lib/entity-foreign-key-helper';
export type { ReverseFKInfo } from './lib/entity-foreign-key-helper';
export { DatabaseReferenceScanner } from './lib/database-reference-scanner';
export type { DatabaseReference } from './lib/database-reference-scanner';
export { DeletionAuditor } from './lib/deletion-auditor';
export type { DeletionAudit } from './lib/deletion-auditor';
export { DeletionReportGenerator } from './lib/deletion-report-generator';
// Service exports
export { InitService } from './services/InitService';
export type { InitOptions, InitCallbacks } from './services/InitService';
export { PullService } from './services/PullService';
export type { PullOptions, PullCallbacks, PullResult } from './services/PullService';
export { PushService } from './services/PushService';
export type { PushOptions, PushCallbacks, PushResult } from './services/PushService';
export { StatusService } from './services/StatusService';
export type { StatusOptions, StatusCallbacks, StatusResult } from './services/StatusService';
export { FileResetService } from './services/FileResetService';
export type { FileResetOptions, FileResetCallbacks, FileResetResult } from './services/FileResetService';
export { WatchService } from './services/WatchService';
export type { WatchOptions, WatchCallbacks, WatchResult } from './services/WatchService';
export { FormattingService } from './services/FormattingService';
  loadSyncConfig,
  loadEntityConfig,
  loadFolderConfig,
  type EntityConfig,
  type FolderConfig,
  type RelatedEntityConfig
} from './config';
// Provider utilities
  initializeProvider,
  findEntityDirectories,
  getDataProvider
} from './lib/provider-utils';
// Validation types
  ValidationResult,
  ValidationError,
  ValidationWarning,
  EntityDependency,
  FileValidationResult,
  ValidationOptions,
  ReferenceType,
  ParsedReference
} from './types/validation'; * @fileoverview Service exports for MetadataSync library usage
 * @module services
 * This module exports all the service classes that can be used programmatically
 * without the CLI interface. These services provide the core functionality
 * for metadata synchronization operations.
export { InitService, InitOptions, InitCallbacks } from './InitService';
export { PullService, PullOptions, PullCallbacks, PullResult } from './PullService';
export { PushService, PushOptions, PushCallbacks, PushResult, EntityPushResult } from './PushService';
export { ValidationService } from './ValidationService';
export { FormattingService } from './FormattingService';
export { StatusService, StatusOptions, StatusCallbacks, StatusResult, EntityStatusResult } from './StatusService';
export { FileResetService, FileResetOptions, FileResetCallbacks, FileResetResult } from './FileResetService';
export { WatchService, WatchOptions, WatchCallbacks, WatchResult } from './WatchService'; * QueryGen package main entry point
 * @memberjunction/query-gen
 * AI-powered generation of domain-specific SQL query templates with
 * automatic testing, refinement, and metadata export.
// Export core classes
export { EntityGrouper } from './core/EntityGrouper';
export { QuestionGenerator } from './core/QuestionGenerator';
export { QueryWriter } from './core/QueryWriter';
export { QueryTester } from './core/QueryTester';
export { QueryFixer } from './core/QueryFixer';
export { QueryRefiner } from './core/QueryRefiner';
export { MetadataExporter } from './core/MetadataExporter';
export { QueryDatabaseWriter } from './core/QueryDatabaseWriter';
// Export utility classes
export { SimilaritySearch } from './vectors/SimilaritySearch';
export { EmbeddingService } from './vectors/EmbeddingService';
// Export types
export * from './data/schema';
// Export prompt names
export * from './prompts/PromptNames';
export { QueryGenConfig, loadConfig } from './cli/config';
// Export CLI commands
export { generateCommand } from './cli/commands/generate';
export { validateCommand } from './cli/commands/validate';
export { exportCommand } from './cli/commands/export';
export { extractErrorMessage, requireValue, getPropertyOrDefault } from './utils/error-handlers';
  formatEntityGroupForPrompt,
  findEntityById,
  getPrimaryKeyFields,
  getForeignKeyFields,
  hasRelationships,
  getRelationshipCount
} from './utils/entity-helpers';
 * CLI entry point for QueryGen package
 * Usage: mj-querygen <command> [options]
 * Commands:
 *   generate  - Generate queries for entities
 *   validate  - Validate existing query templates
 *   export    - Export queries from database to metadata files
import { Command } from 'commander';
import { generateCommand } from './commands/generate';
import { validateCommand } from './commands/validate';
import { exportCommand } from './commands/export';
// Use createRequire to import JSON (compatible with ESM)
const program = new Command();
program
  .name('mj-querygen')
  .description('AI-powered SQL query template generation for MemberJunction')
  .version(packageJson.version);
  .command('generate')
  .description('Generate queries for entities')
  .option('-e, --entities <names...>', 'Specific entities to generate queries for')
  .option('-x, --exclude-entities <names...>', 'Entities to exclude')
  .option('-s, --exclude-schemas <names...>', 'Schemas to exclude')
  .option('-m, --max-entities <number>', 'Max entities per group', '3')
  .option('-r, --max-refinements <number>', 'Max refinement iterations', '3')
  .option('-f, --max-fixes <number>', 'Max error-fixing attempts', '5')
  .option('--model <name>', 'Preferred AI model')
  .option('--vendor <name>', 'Preferred AI vendor')
  .option('-o, --output <path>', 'Output directory')
  .option('--mode <mode>', 'Output mode: metadata|database|both')
  .option('-v, --verbose', 'Verbose output')
  .action(generateCommand);
  .command('validate')
  .description('Validate existing query templates')
  .option('-p, --path <path>', 'Path to queries metadata file', './metadata/queries')
  .action(validateCommand);
  .command('export')
  .description('Export queries from database to metadata files')
  .action(exportCommand);
// Parse command line arguments
program.parse();
// Show help if no command provided
if (!process.argv.slice(2).length) {
  program.outputHelp();
 * @fileoverview Main entry point for the MemberJunction React Runtime.
 * Exports all public APIs for platform-agnostic React component compilation and execution.
 * @module @memberjunction/react-runtime
// Import necessary classes for createReactRuntime function
import { ComponentCompiler } from './compiler';
import { ComponentRegistry } from './registry';
import { ComponentResolver } from './registry';
import { ComponentManager } from './component-manager';
// Export all types
// Export compiler APIs
export { ComponentCompiler } from './compiler';
  DEFAULT_PRESETS,
  DEFAULT_PLUGINS,
  PRODUCTION_CONFIG,
  DEVELOPMENT_CONFIG,
  getBabelConfig,
  validateBabelPresets,
  getJSXConfig
} from './compiler';
// Export registry APIs
export { ComponentRegistry } from './registry';
  ComponentRegistryService,
  IComponentRegistryClient
} from './registry';
// Export unified ComponentManager
export { ComponentManager } from './component-manager';
  LoadOptions,
  LoadResult,
  HierarchyResult,
  ComponentManagerConfig
} from './component-manager';
// Export runtime APIs
  withErrorBoundary,
  formatComponentError,
  createErrorLogger
} from './runtime';
  buildComponentProps,
  normalizeCallbacks,
  normalizeStyles,
  validateComponentProps,
  mergeProps,
  createPropsTransformer,
  wrapCallbacksWithLogging,
  extractPropPaths,
  PropBuilderOptions
  registerComponentHierarchy,
  validateComponentSpec,
  flattenComponentHierarchy,
  countComponentsInHierarchy,
  HierarchyRegistrationResult,
  ComponentRegistrationError,
  HierarchyRegistrationOptions
  ReactRootManager,
  ManagedReactRoot
  createDefaultComponentStyles 
} from './utilities/component-styles';
  StandardLibraries,
  StandardLibraryManager,
  createStandardLibraries
} from './utilities/standard-libraries';
  LibraryLoadOptions,
  LibraryLoadResult
} from './utilities/library-loader';
  getCoreRuntimeLibraries,
  isCoreRuntimeLibrary
} from './utilities/core-libraries';
  LibraryRegistry,
  LibraryDefinition
} from './utilities/library-registry';
  ComponentErrorAnalyzer,
  FailedComponentInfo
} from './utilities/component-error-analyzer';
  ResourceManager,
  ManagedResource
} from './utilities/resource-manager';
  CacheManager,
  CacheEntry,
  CacheOptions
} from './utilities/cache-manager';
  unwrapLibraryComponent,
  unwrapLibraryComponents,
  unwrapAllLibraryComponents,
  // Legacy exports for backward compatibility
  unwrapComponent,
  unwrapComponents,
  unwrapAllComponents
} from './utilities/component-unwrapper';
// Version information
export const VERSION = '2.69.1';
// Default configurations
export const DEFAULT_CONFIGS = {
    babel: {
      plugins: []
    minify: false,
    sourceMaps: false,
    maxCacheSize: 100
    enableNamespaces: true
 * Creates a complete React runtime instance with all necessary components
 * @param babelInstance - Babel standalone instance for compilation
 * @param runtimeContext - Optional runtime context for registry-based components
 * @param debug - Enable debug logging (defaults to false)
 * @returns Object containing compiler, registry, and resolver instances
export function createReactRuntime(
  babelInstance: any,
  config?: {
    compiler?: Partial<import('./types').CompilerConfig>;
    registry?: Partial<import('./types').RegistryConfig>;
    manager?: Partial<import('./component-manager').ComponentManagerConfig>;
  runtimeContext?: import('./types').RuntimeContext,
  debug: boolean = false
  // Merge debug flag into configs
  const compilerConfig = {
    ...config?.compiler,
    debug: config?.compiler?.debug ?? debug
  const registryConfig = {
    ...config?.registry,
    debug: config?.registry?.debug ?? debug
  const managerConfig = {
    ...config?.manager,
    debug: config?.manager?.debug ?? debug
  const compiler = new ComponentCompiler(compilerConfig);
  compiler.setBabelInstance(babelInstance);
  const registry = new ComponentRegistry(registryConfig);
  const resolver = new ComponentResolver(registry, compiler, runtimeContext);
  // Create the unified ComponentManager
  const manager = new ComponentManager(
    registry,
    runtimeContext || { React: null, ReactDOM: null },
    managerConfig
    resolver,
    manager,  // New unified manager
    version: VERSION,
    debug
 * @fileoverview Compiler module exports
export { ComponentCompiler } from './component-compiler';
} from './babel-config'; * @fileoverview Unified Component Manager for MemberJunction React Runtime
 * Consolidates component fetching, compilation, registration, and usage tracking
 * into a single, efficient manager that eliminates duplicate operations.
 * @fileoverview Registry module exports
export { ComponentRegistry } from './component-registry';
export { ComponentResolver, ResolvedComponents } from './component-resolver';
export { ComponentRegistryService, IComponentRegistryClient } from './component-registry-service';
  RegistryComponentMetadata,
  RegistrySearchFilters,
  DependencyTree
export { ComponentSpec } from '@memberjunction/interactive-component-types'; * @fileoverview Runtime module exports
} from './error-boundary';
} from './prop-builder';
} from './component-hierarchy';
} from './react-root-manager'; * @fileoverview Core type definitions for the MemberJunction React Runtime.
 * These types are platform-agnostic and can be used in any JavaScript environment.
import { ComponentLibraryDependency, ComponentStyles, ComponentObject } from '@memberjunction/interactive-component-types';
 * Represents a compiled React component with its metadata
export interface CompiledComponent {
  /** Factory function that creates a ComponentObject when called with context */
  factory: (context: RuntimeContext, styles?: ComponentStyles, components?: Record<string, any>) => ComponentObject;
  /** Unique identifier for the component */
  /** Original component name */
  /** Compilation timestamp */
  /** Any compilation warnings */
 * Options for compiling a React component
export interface CompileOptions {
  /** Component name for identification */
  /** Raw component code to compile */
  componentCode: string;
  /** Optional styles to inject */
  /** Whether to use production mode optimizations */
  production?: boolean;
  /** Custom Babel plugins to use */
  babelPlugins?: string[];
  /** Custom Babel presets to use */
  babelPresets?: string[];
  /** Library dependencies that the component requires */
  /** Child component dependencies that the component requires */
  dependencies?: Array<{ name: string; code?: string }>;
 * Registry entry for a compiled component
export interface RegistryEntry {
  /** The compiled component object with all methods */
  component: ComponentObject;
  /** Component metadata */
  metadata: ComponentMetadata;
  /** Last access time for LRU cache */
  /** Reference count for cleanup */
  refCount: number;
 * Metadata about a registered component
export interface ComponentMetadata {
  /** Unique component identifier */
  /** Component name */
  /** Component version */
  /** Namespace for organization */
  /** Registration timestamp */
  /** Optional tags for categorization */
 * Error information from component execution
  /** Error stack trace */
  stack?: string;
  /** Component name where error occurred */
  /** Error phase (compilation, render, etc.) */
  phase: 'compilation' | 'registration' | 'render' | 'runtime';
  /** Additional error details */
 * Props passed to React components
export interface ComponentProps {
  /** Data object for the component */
  /** User-managed state */
  userState: any;
  /** Utility functions available to the component */
  utilities: any;
  /** Callback functions */
  callbacks: any;
  /** Child components available for use */
  components?: Record<string, any>;
  /** Component styles */
  /** Standard state change handler for controlled components */
  onStateChanged?: (stateUpdate: Record<string, any>) => void;
 * Configuration for the component compiler
export interface CompilerConfig {
  /** Babel configuration */
    /** Presets to use */
    presets: string[];
    /** Plugins to use */
    plugins: string[];
  /** Whether to minify output */
  minify: boolean;
  /** Source map generation */
  sourceMaps: boolean;
  /** Cache compiled components */
  cache: boolean;
  /** Maximum cache size */
  maxCacheSize: number;
  /** Enable debug logging */
 * Configuration for the component registry
export interface RegistryConfig {
  /** Maximum number of components to keep in memory */
  maxComponents: number;
  /** Time in ms before removing unused components */
  cleanupInterval: number;
  /** Whether to use LRU eviction */
  useLRU: boolean;
  /** Namespace isolation */
  enableNamespaces: boolean;
 * Result of a compilation operation
export interface CompilationResult {
  /** Whether compilation succeeded */
  /** The compiled component if successful */
  component?: CompiledComponent;
  /** Error information if failed */
  error?: ComponentError;
  /** Compilation duration in ms */
  /** Size of compiled code in bytes */
  /** Libraries loaded during compilation */
  loadedLibraries?: Map<string, any>;
 * Runtime context for component execution
export interface RuntimeContext {
  /** React library reference */
  /** ReactDOM library reference */
  ReactDOM?: any;
  /** Additional libraries available */
  libraries?: Record<string, any>;
  /** Global utilities */
  utilities?: Record<string, any>;
 * Component lifecycle events
export interface ComponentLifecycle {
  /** Called before component mounts */
  beforeMount?: () => void;
  /** Called after component mounts */
  afterMount?: () => void;
  /** Called before component updates */
  beforeUpdate?: (prevProps: any, nextProps: any) => void;
  /** Called after component updates */
  afterUpdate?: (prevProps: any, currentProps: any) => void;
  /** Called before component unmounts */
  beforeUnmount?: () => void;
 * Options for creating an error boundary
export interface ErrorBoundaryOptions {
  /** Custom error handler */
  onError?: (error: Error, errorInfo: any) => void;
  /** Fallback UI to render on error */
  fallback?: any;
  /** Whether to log errors */
  logErrors?: boolean;
  /** Error recovery strategy */
  recovery?: 'retry' | 'reset' | 'none';
// Export library configuration types
export * from './library-config';
// Export dependency types
export * from './dependency-types';
// Re-export ComponentObject for convenience
export { ComponentObject } from '@memberjunction/interactive-component-types'; * @fileoverview Utilities module exports
export * from './component-styles';
export * from './standard-libraries';
export * from './library-loader';
export * from './library-registry';
export * from './library-dependency-resolver';
export * from './component-error-analyzer';
export * from './resource-manager';
export * from './cache-manager';
export * from './component-unwrapper';export { ReactTestHarness, TestHarnessOptions } from './lib/test-harness';
export { BrowserManager, BrowserContextOptions } from './lib/browser-context';
// Export the component runner that uses the real React runtime UMD bundle
export { ComponentRunner, ComponentExecutionOptions, ComponentExecutionResult } from './lib/component-runner';
export { AssertionHelpers } from './lib/assertion-helpers';
export { ComponentLinter, LintResult, Violation } from './lib/component-linter';
export { LibraryLintCache, CompiledLibraryRules, CompiledValidator } from './lib/library-lint-cache';
import { ReactTestHarness } from '../lib/test-harness';
import { Violation } from '../lib/component-linter';
  .name('mj-react-test')
  .description('React component test harness for MemberJunction')
  .version('2.69.1');
  .command('run <componentFile>')
  .description('Run a React component test')
  .option('-p, --props <json>', 'Component props as JSON string')
  .option('-s, --selector <selector>', 'Wait for selector before capturing')
  .option('-t, --timeout <ms>', 'Timeout in milliseconds', '30000')
  .option('--headless', 'Run in headless mode (default)', true)
  .option('--headed', 'Run in headed mode (visible browser)')
  .option('--screenshot <path>', 'Save screenshot to path')
  .option('--debug', 'Enable debug output')
  .action(async (componentFile, options) => {
    const spinner = ora('Initializing test harness...').start();
      // Parse props if provided
      let props = {};
      if (options.props) {
          props = JSON.parse(options.props);
          spinner.fail(chalk.red('Invalid JSON in props parameter'));
      // Create test harness
        headless: !options.headed,
        debug: options.debug,
        screenshotPath: options.screenshot
      spinner.text = 'Starting browser...';
      spinner.text = 'Loading component...';
        componentFile,
        props,
          waitForSelector: options.selector,
          timeout: parseInt(options.timeout),
          contextUser: undefined! 
        console.log(chalk.green('✓ Component rendered successfully'));
          console.log('\n' + chalk.bold('Console Output:'));
          result.console.forEach((log: { type: string; text: string }) => {
            const color = log.type === 'error' ? chalk.red : 
                         log.type === 'warning' ? chalk.yellow : 
                         chalk.gray;
            console.log(color(`[${log.type}] ${log.text}`));
        if (options.screenshot && result.screenshot) {
          fs.writeFileSync(options.screenshot, result.screenshot as any);
          console.log(chalk.blue(`Screenshot saved to: ${options.screenshot}`));
        console.log(chalk.red('✗ Component rendering failed'));
        console.log(chalk.red('\nErrors:'));
        result.errors.forEach((error: Violation) => {
          switch (error.severity) {
            case 'critical':
              console.log(chalk.red(`  - ${error.message} [${error.severity}]`));
            case 'high':
              console.log(chalk.yellow(`  - ${error.message} [${error.severity}]`));
              console.log(chalk.blue(`  - ${error.message} [${error.severity}]`));
            case 'low':
              console.log(chalk.gray(`  - ${error.message} [${error.severity}]`));
      spinner.fail(chalk.red('Test execution failed'));
  .command('test <testFile>')
  .description('Run a test file with multiple test cases')
  .action(async (testFile, options) => {
    const spinner = ora('Loading test file...').start();
      const testPath = path.resolve(testFile);
      if (!fs.existsSync(testPath)) {
        spinner.fail(chalk.red(`Test file not found: ${testPath}`));
      // Import the test file
      const testModule = await import(testPath);
      if (!testModule.default || typeof testModule.default !== 'function') {
        spinner.fail(chalk.red('Test file must export a default function'));
      spinner.text = 'Running tests...';
      // Create harness with options
        debug: options.debug
      // Run the test function - add AssertionHelpers to harness for test functions
      const harnessWithHelpers = Object.assign(harness, {
        AssertionHelpers: harness.getAssertionHelpers()
      await testModule.default(harnessWithHelpers);
      spinner.succeed(chalk.green('All tests completed'));
  .command('create-example')
  .description('Create an example component and test file')
  .option('-d, --dir <directory>', 'Directory to create files in', './react-test-example')
  .action(async (options) => {
    const dir = path.resolve(options.dir);
    console.log(chalk.blue(`Creating example files in: ${dir}`));
    // Create directory if it doesn't exist
      fs.mkdirSync(dir, { recursive: true });
    // Create example component
    const componentContent = `// Example React Component
const Component = ({ name, count, showDetails }) => {
  const [clickCount, setClickCount] = React.useState(0);
    setClickCount(clickCount + 1);
    <div className="example-component">
      <h1>Hello, {name || 'World'}!</h1>
      <p>Count: {count || 0}</p>
      <button onClick={handleClick}>
        Clicked {clickCount} times
      {showDetails && (
        <div className="details">
          <p>This is additional detail content.</p>
};`;
    fs.writeFileSync(path.join(dir, 'ExampleComponent.jsx'), componentContent);
    console.log(chalk.green('✓ Created ExampleComponent.jsx'));
    // Create example test
    const testContent = `// Example test using ReactTestHarness
  console.log(\`\\nTest Summary: \${summary.passed}/\${summary.total} passed\`);
    fs.writeFileSync(path.join(dir, 'example.test.js'), testContent);
    console.log(chalk.green('✓ Created example.test.js'));
    console.log(chalk.blue('\nTo run the example:'));
    console.log(chalk.gray(`  cd ${options.dir}`));
    console.log(chalk.gray('  mj-react-test run ExampleComponent.jsx'));
    console.log(chalk.gray('  mj-react-test run ExampleComponent.jsx --props \'{"name":"Test","count":10}\''));
    console.log(chalk.gray('  mj-react-test test example.test.js'));
 * Constraint Validators Module
 * This module provides the infrastructure for validating component props
 * against business rules defined in component specifications.
 * Validators use MemberJunction's @RegisterClass decorator for automatic discovery.
 * Each validator is registered with BaseConstraintValidator and its constraint type as the key.
 * Key exports:
 * - BaseConstraintValidator: Abstract base class for all validators
 * - ValidationContext: Context passed to validators
 * - Entity/Query metadata types
 * - Concrete validator implementations (auto-registered via @RegisterClass)
export * from './base-constraint-validator';
export * from './validation-context';
export * from './subset-of-entity-fields-validator';
export * from './sql-where-clause-validator';
export * from './required-when-validator';
export { setupSQLServerClient } from "./config";
export { SQLServerDataProvider } from "./SQLServerDataProvider";
  SqlLoggingSession
export { SqlLoggingSessionImpl } from "./SqlLogger";
export { UserCache } from "./UserCache";
export { QueryParameterProcessor } from "./queryParameterProcessor";
export { NodeFileSystemProvider } from "./NodeFileSystemProvider"; * @memberjunction/scheduling-actions
 * Actions for managing scheduled jobs in MemberJunction.
 * These actions enable AI agents and workflows to query, create, update,
 * delete, and execute scheduled jobs, as well as retrieve execution statistics.
export * from './BaseJobAction';
// Export all action classes
export * from './QueryJobsAction';
export * from './CreateJobAction';
export * from './UpdateJobAction';
export * from './DeleteJobAction';
export * from './ExecuteJobNowAction';
export * from './GetJobStatisticsAction';
 * @fileoverview Main export for Scheduling Engine Base
export * from './SchedulingEngineBase';
export * from './ScheduledJobEntityExtended';
 * @fileoverview Main export for Scheduling Base Types
 * @module @memberjunction/scheduling-base-types
 * @fileoverview Main export for Scheduling Engine
export * from './BaseScheduledJob';
export * from './ScheduledJobEngine';
export * from './CronExpressionHelper';
export * from './NotificationManager';
export * from './drivers';
 * @fileoverview Export all scheduled job drivers
export * from './AgentScheduledJobDriver';
export * from './ActionScheduledJobDriver';
 * MemberJunction Server Bootstrap
 * Encapsulates all server initialization logic so MJAPI applications become minimal bootstrapping files.
 * This package provides a single `createMJServer` function that handles:
 * - Configuration loading
 * - Database connection setup
 * - GraphQL schema building
 * - Resolver discovery and registration
 * - Generated package auto-loading
 * - Server startup with proper lifecycle hooks
import { serve, MJServerOptions, configInfo } from '@memberjunction/server';
 * Configuration options for creating an MJ Server
export interface MJServerConfig {
   * Path to mj.config.cjs or other config file (optional - will auto-discover if not provided)
  configPath?: string;
   * Additional resolver paths to include beyond the defaults
   * @example ['./custom-resolvers/**\/*Resolver.{js,ts}']
  resolverPaths?: string[];
   * Hook that runs before the server starts
  beforeStart?: () => void | Promise<void>;
   * Hook that runs after the server starts
  afterStart?: () => void | Promise<void>;
   * Options for REST API configuration
  restApiOptions?: MJServerOptions['restApiOptions'];
 * Discovers and loads generated packages from the workspace based on configuration.
 * Generated packages (entities, actions, resolvers) register themselves via side effects when imported.
 * This function uses the mj.config.cjs to determine which packages to load.
 * @param config - The loaded MemberJunction configuration
async function discoverAndLoadGeneratedPackages(configResult: any): Promise<void> {
  if (!configResult?.config?.codeGeneration?.packages) {
    console.warn('No codeGeneration.packages configuration found - skipping auto-import of generated packages');
  const packages = configResult.config.codeGeneration.packages;
  // Attempt to import each configured generated package
  // These imports trigger class registration via @RegisterClass decorators
  const packageTypes = ['entities', 'actions', 'angularForms', 'graphqlResolvers'];
  for (const pkgType of packageTypes) {
    if (packages[pkgType]?.name) {
      const pkgName = packages[pkgType].name;
        // Dynamic import to trigger side effects (class registration)
        await import(pkgName);
        console.log(`✓ Loaded generated package: ${pkgName}`);
        // Not finding a package is expected in some cases (e.g., no forms generated yet)
        if (error.code === 'ERR_MODULE_NOT_FOUND') {
          console.log(`ℹ Generated package not found (may not exist yet): ${pkgName}`);
          console.warn(`⚠ Error loading generated package ${pkgName}:`, error);
 * Creates and starts a MemberJunction API server with minimal configuration.
 * This is the primary entry point for MJ 3.0 applications. It:
 * 1. Loads configuration from mj.config.cjs (or specified path)
 * 2. Auto-discovers and imports generated packages
 * 3. Builds the GraphQL schema with all registered resolvers
 * 4. Starts the server with proper lifecycle hooks
 * @param options - Configuration options for the server
 * // Minimal MJAPI 3.0 application (packages/api/src/index.ts):
 * import { createMJServer } from '@memberjunction/server-bootstrap';
 * // Import generated packages to trigger registration
 * import '@mycompany/generated-entities';
 * import '@mycompany/generated-actions';
 * import '@mycompany/generated-resolvers';
 * createMJServer().catch(console.error);
 * // With custom configuration:
 * createMJServer({
 *   resolverPaths: ['./custom-resolvers/**\/*Resolver.{js,ts}'],
 *   beforeStart: async () => {
 *     console.log('Running custom pre-start logic...');
 *   afterStart: async () => {
 *     console.log('Server ready for custom operations');
 * }).catch(console.error);
export async function createMJServer(options: MJServerConfig = {}): Promise<void> {
  console.log('🚀 MemberJunction Server Bootstrap');
  console.log('=====================================\n');
  // Configuration has already been loaded and merged by MJServer's config.ts at module init time
  // We just need to load the raw user config to access codeGeneration.packages setting
  const configSearchResult = explorer.search(options.configPath || process.cwd());
  // Create a result object for backward compatibility with discoverAndLoadGeneratedPackages
  const configResult = {
    config: configSearchResult?.config || {},
    hasUserConfig: configSearchResult && !configSearchResult.isEmpty,
    configFilePath: configSearchResult?.filepath
  // Discover and load generated packages automatically
  // This triggers their @RegisterClass decorators to register entities, actions, etc.
  console.log('Loading generated packages...');
  await discoverAndLoadGeneratedPackages(configResult);
  // Build resolver paths - auto-discover standard locations if not provided
  // This enables truly minimal MJAPI files without needing to specify paths
  const resolverPaths = options.resolverPaths || [
    // Standard locations where generated resolvers may exist
    './src/generated/generated.{js,ts}',
    './dist/generated/generated.{js,ts}',
    './generated/generated.{js,ts}',
  // Optional pre-start hook
  if (options.beforeStart) {
    console.log('Running pre-start hook...');
    await Promise.resolve(options.beforeStart());
  // Build server options
  const serverOptions: MJServerOptions = {
    onBeforeServe: options.beforeStart,
    restApiOptions: options.restApiOptions
  // Start the MJ Server
  // The serve() function from @memberjunction/server handles:
  // - Database connection pooling
  // - GraphQL schema building from resolvers
  // - WebSocket setup for subscriptions
  // - REST API endpoint registration
  // - Graceful shutdown handling
  console.log('Starting MemberJunction Server...\n');
  await serve(resolverPaths, undefined, serverOptions);
  // Optional post-start hook
  if (options.afterStart) {
    await Promise.resolve(options.afterStart());
// Re-export types from @memberjunction/server for convenience
export type { MJServerOptions } from '@memberjunction/server';
 * MemberJunction Server Bootstrap (Lite)
 * A lightweight class registrations manifest that excludes heavy and ESM-incompatible
 * dependencies (communication providers, storage, bizapps actions, etc.).
 * Use this package instead of @memberjunction/server-bootstrap for:
 * - CLI tools (MJCLI)
 * - CodeGen (CodeGenLib, MJCodeGenAPI)
 * - MCP Server
 * - A2A Server
 * - Any server-side app that doesn't need communication/storage/bizapps
 * For the full manifest with all packages, use @memberjunction/server-bootstrap.
export { CLASS_REGISTRATIONS, CLASS_REGISTRATIONS_MANIFEST_LOADED, CLASS_REGISTRATIONS_COUNT, CLASS_REGISTRATIONS_PACKAGES } from './mj-class-registrations.js';
// Re-export all types from organized modules
export * from './api-types';
export * from './conversation-types';
export * from './response-types';
export * from './entity-metadata-types';
export * from './query-types';
export * from './artifact-types';
export * from './auth-types';
export * from './TemplateEngineBase';
export * from './TemplateEngine';
export * from './extensions/AIPrompt.extension';
export * from './extensions/TemplateEmbed.extension';
export * from './extensions/TemplateExtensionBase'; * MemberJunction Testing CLI
 * Command-line interface for the Testing Framework.
 * Provides thin wrappers around Testing Engine for CLI execution.
// Command implementations
export { RunCommand } from './commands/run';
export { SuiteCommand } from './commands/suite';
export { ListCommand } from './commands/list';
export { ValidateCommand } from './commands/validate';
export { ReportCommand } from './commands/report';
export { HistoryCommand } from './commands/history';
export { CompareCommand } from './commands/compare';
export { OutputFormatter } from './utils/output-formatter';
export { loadCLIConfig } from './utils/config-loader';
export { SpinnerManager } from './utils/spinner-manager';
export { getContextUser } from './lib/mj-provider';
 * MemberJunction Testing Engine
 * Core execution engine for the MemberJunction Testing Framework.
 * Provides test drivers, oracles, and execution orchestration.
export * from './engine/TestEngine';
export * from './drivers/BaseTestDriver';
// Concrete drivers
export * from './drivers/AgentEvalDriver';
// Oracle interface and implementations
export * from './oracles/IOracle';
export * from './oracles/SchemaValidatorOracle';
export * from './oracles/TraceValidatorOracle';
export * from './oracles/LLMJudgeOracle';
export * from './oracles/ExactMatchOracle';
export * from './oracles/SQLValidatorOracle';
export * from './utils/scoring';
export * from './utils/cost-calculator';
export * from './utils/result-formatter';
export * from './utils/execution-context';
export * from './utils/variable-resolver';
 * Test driver implementations
export * from './BaseTestDriver';
export * from './AgentEvalDriver';
 * Oracle implementations for test evaluation
export * from './IOracle';
export * from './SchemaValidatorOracle';
export * from './TraceValidatorOracle';
export * from './LLMJudgeOracle';
export * from './ExactMatchOracle';
export * from './SQLValidatorOracle';
 * MemberJunction Testing Engine Base
 * Metadata cache for test framework (UI-safe, no execution logic)
export * from './TestEngineBase';
export { resetMJSingletons, resetClassFactory, resetObjectCache } from './singleton-reset';
export { createMockEntity, type MockEntityOptions } from './mock-entity';
export { mockRunView, mockRunViews, resetRunViewMocks } from './mock-run-view';
export { installCustomMatchers } from './custom-matchers';
export type {} from './vitest.d';
export * from './constants';
export * from './DependencyGraphWalker';
export * from './LabelManager';
export * from './SnapshotBuilder';
export * from './DiffEngine';
export * from './RestoreEngine';
export * from './VersionHistoryEngine';
import { valid, compare, gte, lte } from 'semver';
import { basename } from 'node:path';
import { get, render } from './notes';
import { ELECTRON_DIR } from '../../lib/utils';
import { ELECTRON_ORG, ELECTRON_REPO } from '../types';
const semverify = (version: string) => version.replace(/^origin\//, '').replace(/[xy]/g, '0').replace(/-/g, '.');
const runGit = async (args: string[]) => {
  console.info(`Running: git ${args.join(' ')}`);
  const response = spawnSync('git', args, {
  if (response.status !== 0) {
    throw new Error(response.stderr.trim());
  return response.stdout.trim();
const tagIsSupported = (tag: string) => !!tag && !tag.includes('nightly') && !tag.includes('unsupported');
const tagIsAlpha = (tag: string) => !!tag && tag.includes('alpha');
const tagIsBeta = (tag: string) => !!tag && tag.includes('beta');
const tagIsStable = (tag: string) => tagIsSupported(tag) && !tagIsBeta(tag) && !tagIsAlpha(tag);
const getTagsOf = async (point: string) => {
    const tags = await runGit(['tag', '--merged', point]);
    return tags.split('\n')
      .map(tag => tag.trim())
      .filter(tag => valid(tag))
      .sort(compare);
    console.error(`Failed to fetch tags for point ${point}`);
const getTagsOnBranch = async (point: string) => {
  const { data: { default_branch: defaultBranch } } = await octokit.repos.get({
    repo: ELECTRON_REPO
  const mainTags = await getTagsOf(defaultBranch);
  if (point === defaultBranch) {
    return mainTags;
  const mainTagsSet = new Set(mainTags);
  return (await getTagsOf(point)).filter(tag => !mainTagsSet.has(tag));
const getBranchOf = async (point: string) => {
    const branches = (await runGit(['branch', '-a', '--contains', point]))
      .map(branch => branch.trim())
      .filter(branch => !!branch);
    const current = branches.find(branch => branch.startsWith('* '));
    return current ? current.slice(2) : branches.shift();
    console.error(`Failed to fetch branch for ${point}: `, err);
const getAllBranches = async () => {
    const branches = await runGit(['branch', '--remote']);
    return branches.split('\n')
      .filter(branch => !!branch)
      .filter(branch => branch !== 'origin/HEAD -> origin/main')
    console.error('Failed to fetch all branches');
const getStabilizationBranches = async () => {
  return (await getAllBranches()).filter(branch => /^origin\/\d+-x-y$/.test(branch));
const getPreviousStabilizationBranch = async (current: string) => {
  const stabilizationBranches = (await getStabilizationBranches())
    .filter(branch => branch !== current && branch !== `origin/${current}`);
  if (!valid(current)) {
    // since we don't seem to be on a stabilization branch right now,
    // pick a placeholder name that will yield the newest branch
    // as a comparison point.
    current = 'v999.999.999';
  let newestMatch = null;
  for (const branch of stabilizationBranches) {
    if (gte(semverify(branch), semverify(current))) {
    if (newestMatch && lte(semverify(branch), semverify(newestMatch))) {
    newestMatch = branch;
  return newestMatch!;
const getPreviousPoint = async (point: string) => {
  const currentBranch = await getBranchOf(point);
  const currentTag = (await getTagsOf(point)).filter(tag => tagIsSupported(tag)).pop()!;
  const currentIsStable = tagIsStable(currentTag);
    // First see if there's an earlier tag on the same branch
    // that can serve as a reference point.
    let tags = (await getTagsOnBranch(`${point}^`)).filter(tag => tagIsSupported(tag));
    if (currentIsStable) {
      tags = tags.filter(tag => tagIsStable(tag));
    if (tags.length) {
      return tags.pop();
    console.log('error', error);
  // Otherwise, use the newest stable release that precedes this branch.
  // To reach that you may have to walk past >1 branch, e.g. to get past
  // 2-1-x which never had a stable release.
  let branch = currentBranch;
  while (branch) {
    const prevBranch = await getPreviousStabilizationBranch(branch);
    const tags = (await getTagsOnBranch(prevBranch)).filter(tag => tagIsStable(tag));
    branch = prevBranch;
async function getReleaseNotes (range: string, newVersion?: string, unique?: boolean) {
  const rangeList = range.split('..') || ['HEAD'];
  const to = rangeList.pop()!;
  const from = rangeList.pop() || (await getPreviousPoint(to))!;
    newVersion = to;
  const notes = await get(from, to, newVersion);
  const ret: { text: string; warning?: string; } = {
    text: render(notes, unique)
  if (notes.unknown.length) {
    ret.warning = `You have ${notes.unknown.length} unknown release notes. Please fix them before releasing.`;
  const { values: { help, unique, version }, positionals } = parseArgs({
      unique: {
      version: {
  const range = positionals.shift();
  if (help || !range) {
    const name = basename(process.argv[1]);
easy usage: ${name} version
full usage: ${name} [begin..]end [--version version] [--unique]
 * 'begin' and 'end' are two git references -- tags, branches, etc --
   from which the release notes are generated.
 * if omitted, 'begin' defaults to the previous tag in end's branch.
 * if omitted, 'version' defaults to 'end'. Specifying a version is
   useful if you're making notes on a new version that isn't tagged yet.
 * '--unique' omits changes that also landed in other branches.
For example, these invocations are equivalent:
  ${process.argv[1]} v4.0.1
  ${process.argv[1]} v4.0.0..v4.0.1 --version v4.0.1
  const notes = await getReleaseNotes(range, version, unique);
  console.log(notes.text);
  if (notes.warning) {
    throw new Error(notes.warning);
    console.error('Error Occurred:', err);
export default getReleaseNotes;
import '@jupyter-notebook/application';
import '@jupyter-notebook/application-extension';
import '@jupyter-notebook/console-extension';
import '@jupyter-notebook/docmanager-extension';
import '@jupyter-notebook/documentsearch-extension';
import '@jupyter-notebook/help-extension';
import '@jupyter-notebook/lab-extension';
import '@jupyter-notebook/notebook-extension';
import '@jupyter-notebook/terminal-extension';
import '@jupyter-notebook/tree';
import '@jupyter-notebook/tree-extension';
import '@jupyter-notebook/ui-components';
  ILabStatus,
  IRouter,
  ITreePathUpdater,
  JupyterFrontEnd,
  JupyterFrontEndPlugin,
  JupyterLab,
} from '@jupyterlab/application';
  DOMUtils,
  ICommandPalette,
  ISanitizer,
  ISplashScreen,
  IToolbarWidgetRegistry,
  showErrorMessage,
} from '@jupyterlab/apputils';
import { ConsolePanel } from '@jupyterlab/console';
import { PageConfig, PathExt, URLExt } from '@jupyterlab/coreutils';
import { IDocumentManager, renameDialog } from '@jupyterlab/docmanager';
import { DocumentWidget } from '@jupyterlab/docregistry';
import { IMainMenu } from '@jupyterlab/mainmenu';
  ILatexTypesetter,
  IMarkdownParser,
  IRenderMime,
  IRenderMimeRegistry,
  RenderMimeRegistry,
  standardRendererFactories,
} from '@jupyterlab/rendermime';
import { ISettingRegistry } from '@jupyterlab/settingregistry';
import { ITranslator, nullTranslator } from '@jupyterlab/translation';
  NotebookApp,
  NotebookShell,
  INotebookShell,
  SidePanel,
  SidePanelHandler,
  SidePanelPalette,
  INotebookPathOpener,
  defaultNotebookPathOpener,
} from '@jupyter-notebook/application';
import { jupyterIcon } from '@jupyter-notebook/ui-components';
import { PromiseDelegate } from '@lumino/coreutils';
  DisposableDelegate,
  DisposableSet,
} from '@lumino/disposable';
import { Menu, Widget } from '@lumino/widgets';
 * A regular expression to match path to notebooks and documents
const TREE_PATTERN = new RegExp('/(notebooks|edit)/(.*)');
 * A regular expression to suppress the file extension from display for .ipynb files.
const STRIP_IPYNB = /\.ipynb$/;
 * The JupyterLab document manager plugin id.
const JUPYTERLAB_DOCMANAGER_PLUGIN_ID =
  '@jupyterlab/docmanager-extension:plugin';
 * The command IDs used by the application plugin.
namespace CommandIDs {
   * Duplicate the current document and open the new document
  export const duplicate = 'application:duplicate';
   * Handle local links
  export const handleLink = 'application:handle-local-link';
   * Toggle Top Bar visibility
  export const toggleTop = 'application:toggle-top';
   * Toggle side panel visibility
  export const togglePanel = 'application:toggle-panel';
   * Toggle the Zen mode
  export const toggleZen = 'application:toggle-zen';
   * Open JupyterLab
  export const openLab = 'application:open-lab';
   * Open the tree page.
  export const openTree = 'application:open-tree';
   * Rename the current document
  export const rename = 'application:rename';
   * Resolve tree path
  export const resolveTree = 'application:resolve-tree';
 * Check if the application is dirty before closing the browser tab.
const dirty: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/application-extension:dirty',
    'Check if the application is dirty before closing the browser tab.',
  autoStart: true,
  requires: [ILabStatus, ITranslator],
  activate: (
    app: JupyterFrontEnd,
    status: ILabStatus,
    translator: ITranslator
  ): void => {
    if (!(app instanceof NotebookApp)) {
      throw new Error(`${dirty.id} must be activated in Jupyter Notebook.`);
    const trans = translator.load('notebook');
    const message = trans.__(
      'Are you sure you want to exit Jupyter Notebook?\n\nAny unsaved changes will be lost.'
    window.addEventListener('beforeunload', (event) => {
      if (app.status.isDirty) {
        return ((event as any).returnValue = message);
 * The application info.
const info: JupyterFrontEndPlugin<JupyterLab.IInfo> = {
  id: '@jupyter-notebook/application-extension:info',
  provides: JupyterLab.IInfo,
  activate: (app: JupyterFrontEnd): JupyterLab.IInfo => {
      throw new Error(`${info.id} must be activated in Jupyter Notebook.`);
    return app.info;
 * The logo plugin.
const logo: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/application-extension:logo',
  description: 'The logo plugin.',
  activate: (app: JupyterFrontEnd) => {
    const baseUrl = PageConfig.getBaseUrl();
    const node = document.createElement('a');
    node.href = `${baseUrl}tree`;
    node.target = '_blank';
    node.rel = 'noopener noreferrer';
    const logo = new Widget({ node });
    jupyterIcon.element({
      container: node,
      elementPosition: 'center',
      padding: '2px 2px 2px 8px',
      height: '28px',
      margin: 'auto',
    logo.id = 'jp-NotebookLogo';
    app.shell.add(logo, 'top', { rank: 0 });
 * A plugin to open documents in the main area.
const opener: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/application-extension:opener',
  description: 'A plugin to open documents in the main area.',
  requires: [IRouter, IDocumentManager],
  optional: [ISettingRegistry],
    router: IRouter,
    docManager: IDocumentManager,
    settingRegistry: ISettingRegistry | null
    const { commands, docRegistry } = app;
    const command = 'router:tree';
    commands.addCommand(command, {
      execute: (args: any) => {
        const parsed = args as IRouter.ILocation;
        const matches = parsed.path.match(TREE_PATTERN) ?? [];
        const [, , path] = matches;
        if (!path) {
        app.started.then(async () => {
          const file = decodeURIComponent(path);
          const urlParams = new URLSearchParams(parsed.search);
          let defaultFactory = docRegistry.defaultWidgetFactory(path).name;
          // Explicitly get the default viewers from the settings because
          // JupyterLab might not have had the time to load the settings yet (race condition)
          // Relevant code: https://github.com/jupyterlab/jupyterlab/blob/d56ff811f39b3c10c6d8b6eb27a94624b753eb53/packages/docmanager-extension/src/index.tsx#L265-L293
          if (settingRegistry) {
            const settings = await settingRegistry.load(
              JUPYTERLAB_DOCMANAGER_PLUGIN_ID
            const defaultViewers = settings.get('defaultViewers').composite as {
              [ft: string]: string;
            // get the file types for the path
            const types = docRegistry.getFileTypesForPath(path);
            // for each file type, check if there is a default viewer and if it
            // is available in the docRegistry. If it is the case, use it as the
            // default factory
            types.forEach((ft) => {
                defaultViewers[ft.name] !== undefined &&
                docRegistry.getWidgetFactory(defaultViewers[ft.name])
                defaultFactory = defaultViewers[ft.name];
          const factory = urlParams.get('factory') ?? defaultFactory;
          docManager.open(file, factory, undefined, {
            ref: '_noref',
    router.register({ command, pattern: TREE_PATTERN });
 * A plugin to customize menus
const menus: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/application-extension:menus',
  description: 'A plugin to customize menus.',
  requires: [IMainMenu],
  activate: (app: JupyterFrontEnd, menu: IMainMenu) => {
    // always disable the Tabs menu
    menu.tabsMenu.dispose();
    const page = PageConfig.getOption('notebookPage');
      case 'consoles':
      case 'terminals':
        menu.editMenu.dispose();
        menu.kernelMenu.dispose();
        menu.runMenu.dispose();
 * A plugin to provide a spacer at rank 900 in the menu area
const menuSpacer: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/application-extension:menu-spacer',
  description: 'A plugin to provide a spacer at rank 900 in the menu area.',
    const menu = new Widget();
    menu.id = DOMUtils.createDomID();
    menu.addClass('jp-NotebookSpacer');
    app.shell.add(menu, 'menu', { rank: 900 });
 * Add commands to open the tree and running pages.
const pages: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/application-extension:pages',
  description: 'Add commands to open the tree and running pages.',
  requires: [ITranslator],
  optional: [ICommandPalette],
    translator: ITranslator,
    palette: ICommandPalette | null
    app.commands.addCommand(CommandIDs.openLab, {
      label: trans.__('Open JupyterLab'),
      execute: () => {
        window.open(URLExt.join(baseUrl, 'lab'));
    app.commands.addCommand(CommandIDs.openTree, {
      label: trans.__('File Browser'),
        if (page === 'tree') {
          app.commands.execute('filebrowser:activate');
          window.open(URLExt.join(baseUrl, 'tree'));
    if (palette) {
      palette.addItem({ command: CommandIDs.openLab, category: 'View' });
      palette.addItem({ command: CommandIDs.openTree, category: 'View' });
 * A plugin to open paths in new browser tabs.
const pathOpener: JupyterFrontEndPlugin<INotebookPathOpener> = {
  id: '@jupyter-notebook/application-extension:path-opener',
  description: 'A plugin to open paths in new browser tabs.',
  provides: INotebookPathOpener,
  activate: (app: JupyterFrontEnd): INotebookPathOpener => {
    return defaultNotebookPathOpener;
 * The default paths for a Jupyter Notebook app.
const paths: JupyterFrontEndPlugin<JupyterFrontEnd.IPaths> = {
  id: '@jupyter-notebook/application-extension:paths',
  description: 'The default paths for a Jupyter Notebook app.',
  provides: JupyterFrontEnd.IPaths,
  activate: (app: JupyterFrontEnd): JupyterFrontEnd.IPaths => {
      throw new Error(`${paths.id} must be activated in Jupyter Notebook.`);
    return app.paths;
 * A plugin providing a rendermime registry.
const rendermime: JupyterFrontEndPlugin<IRenderMimeRegistry> = {
  id: '@jupyter-notebook/application-extension:rendermime',
  description: 'A plugin providing a rendermime registry.',
  provides: IRenderMimeRegistry,
  optional: [
    IDocumentManager,
    ITranslator,
    docManager: IDocumentManager | null,
    latexTypesetter: ILatexTypesetter | null,
    sanitizer: IRenderMime.ISanitizer | null,
    markdownParser: IMarkdownParser | null,
    translator: ITranslator | null,
    notebookPathOpener: INotebookPathOpener | null
    const trans = (translator ?? nullTranslator).load('jupyterlab');
    const opener = notebookPathOpener ?? defaultNotebookPathOpener;
    if (docManager) {
      app.commands.addCommand(CommandIDs.handleLink, {
        label: trans.__('Handle Local Link'),
        execute: (args) => {
          const path = args['path'] as string | undefined | null;
          if (path === undefined || path === null) {
          return docManager.services.contents
            .get(path, { content: false })
            .then((model) => {
              opener.open({
                prefix: URLExt.join(baseUrl, 'tree'),
                path: model.path,
                target: '_blank',
    return new RenderMimeRegistry({
      initialFactories: standardRendererFactories,
      linkHandler: !docManager
            handleLink: (node: HTMLElement, path: string, id?: string) => {
              // If node has the download attribute explicitly set, use the
              // default browser downloading behavior.
              if (node.tagName === 'A' && node.hasAttribute('download')) {
              app.commandLinker.connectNode(node, CommandIDs.handleLink, {
      latexTypesetter: latexTypesetter ?? undefined,
      markdownParser: markdownParser ?? undefined,
      translator: translator ?? undefined,
      sanitizer: sanitizer ?? undefined,
 * The default Jupyter Notebook application shell.
const shell: JupyterFrontEndPlugin<INotebookShell> = {
  id: '@jupyter-notebook/application-extension:shell',
  description: 'The default Jupyter Notebook application shell.',
  provides: INotebookShell,
    if (!(app.shell instanceof NotebookShell)) {
      throw new Error(`${shell.id} did not find a NotebookShell instance.`);
    const notebookShell = app.shell;
      settingRegistry
        .load(shell.id)
        .then((settings) => {
          // Add a layer of customization to support app shell mode
          const customLayout = settings.composite['layout'] as any;
          // Restore the layout.
          void notebookShell.restoreLayout(customLayout);
        .catch((reason) => {
          console.error('Fail to load settings for the layout restorer.');
    return notebookShell;
 * The default splash screen provider.
const splash: JupyterFrontEndPlugin<ISplashScreen> = {
  id: '@jupyter-notebook/application-extension:splash',
  description: 'Provides an empty splash screen.',
  provides: ISplashScreen,
    const { restored } = app;
    const splash = document.createElement('div');
    splash.style.position = 'absolute';
    splash.style.width = '100%';
    splash.style.height = '100%';
    splash.style.zIndex = '10';
      show: (light = true) => {
        splash.style.backgroundColor = light ? 'white' : '#111111';
        document.body.appendChild(splash);
        return new DisposableDelegate(async () => {
          document.body.removeChild(splash);
 * The default JupyterLab application status provider.
const status: JupyterFrontEndPlugin<ILabStatus> = {
  id: '@jupyter-notebook/application-extension:status',
  description: 'The default JupyterLab application status provider.',
  provides: ILabStatus,
      throw new Error(`${status.id} must be activated in Jupyter Notebook.`);
    return app.status;
 * A plugin to display the document title in the browser tab title
const tabTitle: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/application-extension:tab-title',
    'A plugin to display the document title in the browser tab title.',
  requires: [INotebookShell],
  activate: (app: JupyterFrontEnd, shell: INotebookShell) => {
    const setTabTitle = () => {
      const current = shell.currentWidget;
      if (current instanceof ConsolePanel) {
        const update = () => {
          const title =
            current.sessionContext.path || current.sessionContext.name;
          const basename = PathExt.basename(title);
          // Strip the ".ipynb" suffix from filenames for display in tab titles.
          document.title = basename.replace(STRIP_IPYNB, '');
        current.sessionContext.sessionChanged.connect(update);
        update();
      } else if (current instanceof DocumentWidget) {
          const basename = PathExt.basename(current.context.path);
        current.context.pathChanged.connect(update);
    shell.currentChanged.connect(setTabTitle);
    setTabTitle();
 * A plugin to display and rename the title of a file
const title: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/application-extension:title',
  description: 'A plugin to display and rename the title of a file.',
  requires: [INotebookShell, ITranslator],
  optional: [IDocumentManager, IRouter, IToolbarWidgetRegistry],
    shell: INotebookShell,
    router: IRouter | null,
    toolbarRegistry: IToolbarWidgetRegistry | null
    const { commands } = app;
    const node = document.createElement('div');
    if (toolbarRegistry) {
      toolbarRegistry.addFactory('TopBar', 'widgetTitle', (toolbar) => {
        const widget = new Widget({ node });
        widget.id = 'jp-title';
        return widget;
    const addTitle = async (): Promise<void> => {
      if (!current || !(current instanceof DocumentWidget)) {
      const h = document.createElement('h1');
      h.textContent = current.title.label.replace(STRIP_IPYNB, '');
      node.appendChild(h);
      node.style.marginLeft = '10px';
      if (!docManager) {
      const isEnabled = () => {
        const { currentWidget } = shell;
        return !!(currentWidget && docManager.contextForWidget(currentWidget));
      commands.addCommand(CommandIDs.duplicate, {
        label: () => trans.__('Duplicate'),
        isEnabled,
        execute: async () => {
          if (!isEnabled()) {
          // Duplicate the file, and open the new file.
          const result = await docManager.duplicate(current.context.path);
          await commands.execute('docmanager:open', { path: result.path });
      commands.addCommand(CommandIDs.rename, {
        label: () => trans.__('Rename…'),
            const result = await renameDialog(docManager, current.context);
            // activate the current widget to bring the focus
              current.activate();
            showErrorMessage(
              trans.__('Rename Error'),
              (error as Error).message ||
                trans.__('An error occurred while renaming the file.')
          const newPath = current.context.path;
          const basename = PathExt.basename(newPath);
          h.textContent = basename.replace(STRIP_IPYNB, '');
          if (!router) {
          const matches = router.current.path.match(TREE_PATTERN) ?? [];
          const [, route, path] = matches;
          if (!route || !path) {
          const encoded = encodeURIComponent(newPath);
          router.navigate(`/${route}/${encoded}`, {
            skipRouting: true,
      node.onclick = async () => {
        void commands.execute(CommandIDs.rename);
    shell.currentChanged.connect(addTitle);
    void addTitle();
 * Plugin to toggle the top header visibility.
const topVisibility: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/application-extension:top',
  description: 'Plugin to toggle the top header visibility.',
  optional: [ISettingRegistry, ICommandPalette],
    app: JupyterFrontEnd<JupyterFrontEnd.IShell>,
    notebookShell: INotebookShell,
    settingRegistry: ISettingRegistry | null,
    const top = notebookShell.top;
    const pluginId = topVisibility.id;
    app.commands.addCommand(CommandIDs.toggleTop, {
      label: trans.__('Show Header'),
        top.setHidden(top.isVisible);
          void settingRegistry.set(
            pluginId,
            'visible',
            top.isVisible ? 'yes' : 'no'
      isToggled: () => top.isVisible,
    let adjustToScreen = false;
      const loadSettings = settingRegistry.load(pluginId);
      const updateSettings = (settings: ISettingRegistry.ISettings): void => {
        // 'visible' property from user preferences or default settings
        let visible = settings.get('visible').composite;
        if (settings.user.visible !== undefined) {
          visible = settings.user.visible;
        top.setHidden(visible === 'no');
        // adjust to screen from user preferences or default settings
        adjustToScreen = visible === 'automatic';
      Promise.all([loadSettings, app.restored])
        .then(([settings]) => {
          updateSettings(settings);
          settings.changed.connect((settings) => {
        .catch((reason: Error) => {
          console.error(reason.message);
      palette.addItem({ command: CommandIDs.toggleTop, category: 'View' });
    const onChanged = (): void => {
      if (!adjustToScreen) {
      if (app.format === 'desktop') {
        notebookShell.expandTop();
        notebookShell.collapseTop();
    // listen on format change (mobile and desktop) to make the view more compact
    app.formatChanged.connect(onChanged);
 * Plugin to toggle the left or right side panel's visibility.
const sidePanelVisibility: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/application-extension:sidepanel',
  description: 'Plugin to toggle the visibility of left or right side panel.',
  optional: [IMainMenu, ICommandPalette],
    menu: IMainMenu | null,
    /* Arguments for togglePanel command:
     * side, left or right area
     * title, widget title to show in the menu
     * id, widget ID to activate in the side panel
    app.commands.addCommand(CommandIDs.togglePanel, {
      label: (args) => args['title'] as string,
      caption: (args) => {
        // We do not substitute the parameter into the string because the parameter is not
        // localized (e.g., it is always 'left') even though the string is localized.
        if (args['side'] === 'left') {
          return trans.__(
            'Show %1 in the left sidebar',
            args['title'] as string
        } else if (args['side'] === 'right') {
            'Show %1 in the right sidebar',
        return trans.__('Show %1 in the sidebar', args['title'] as string);
        switch (args['side'] as string) {
          case 'left':
            if (notebookShell.leftCollapsed) {
              notebookShell.expandLeft(args.id as string);
              notebookShell.leftHandler.currentWidget?.id !== args.id
              notebookShell.collapseLeft();
              if (notebookShell.currentWidget) {
                notebookShell.activateById(notebookShell.currentWidget.id);
          case 'right':
            if (notebookShell.rightCollapsed) {
              notebookShell.expandRight(args.id as string);
              notebookShell.rightHandler.currentWidget?.id !== args.id
              notebookShell.collapseRight();
      isToggled: (args) => {
          case 'left': {
            const currentWidget = notebookShell.leftHandler.currentWidget;
            if (!currentWidget) {
            return currentWidget.id === (args['id'] as string);
          case 'right': {
            const currentWidget = notebookShell.rightHandler.currentWidget;
    const sidePanelMenu: { [area in SidePanel.Area]: IDisposable | null } = {
      left: null,
      right: null,
     * The function which adds entries to the View menu for each widget of a side panel.
     * @param area - 'left' or 'right', the area of the side panel.
     * @param entryLabel - the name of the main entry in the View menu for that side panel.
     * @returns - The disposable menu added to the View menu or null.
    const updateMenu = (area: SidePanel.Area, entryLabel: string) => {
      if (menu === null) {
      // Remove the previous menu entry for this side panel.
      sidePanelMenu[area]?.dispose();
      // Creates a new menu entry and populates it with side panel widgets.
      const newMenu = new Menu({ commands: app.commands });
      newMenu.title.label = entryLabel;
      const widgets = notebookShell.widgets(area);
      let menuToAdd = false;
      for (const widget of widgets) {
        newMenu.addItem({
          command: CommandIDs.togglePanel,
            side: area,
            title: `Show ${widget.title.caption}`,
            id: widget.id,
        menuToAdd = true;
      // If there are widgets, add the menu to the main menu entry.
      if (menuToAdd) {
        sidePanelMenu[area] = menu.viewMenu.addItem({
          submenu: newMenu,
    app.restored.then(() => {
      // Create menu entries for the left and right panel.
        const getSidePanelLabel = (area: SidePanel.Area): string => {
          if (area === 'left') {
            return trans.__('Left Sidebar');
            return trans.__('Right Sidebar');
        const leftArea = notebookShell.leftHandler.area;
        const leftLabel = getSidePanelLabel(leftArea);
        updateMenu(leftArea, leftLabel);
        const rightArea = notebookShell.rightHandler.area;
        const rightLabel = getSidePanelLabel(rightArea);
        updateMenu(rightArea, rightLabel);
        const handleSidePanelChange = (
          sidePanel: SidePanelHandler,
          widget: Widget
          const label = getSidePanelLabel(sidePanel.area);
          updateMenu(sidePanel.area, label);
        notebookShell.leftHandler.widgetAdded.connect(handleSidePanelChange);
        notebookShell.leftHandler.widgetRemoved.connect(handleSidePanelChange);
        notebookShell.rightHandler.widgetAdded.connect(handleSidePanelChange);
        notebookShell.rightHandler.widgetRemoved.connect(handleSidePanelChange);
      // Add palette entries for side panels.
        const sidePanelPalette = new SidePanelPalette({
          commandPalette: palette as ICommandPalette,
        notebookShell.leftHandler.widgets.forEach((widget) => {
          sidePanelPalette.addItem(widget, notebookShell.leftHandler.area);
        notebookShell.rightHandler.widgets.forEach((widget) => {
          sidePanelPalette.addItem(widget, notebookShell.rightHandler.area);
        // Update menu and palette when widgets are added or removed from side panels.
        notebookShell.leftHandler.widgetAdded.connect((sidePanel, widget) => {
          sidePanelPalette.addItem(widget, sidePanel.area);
        notebookShell.leftHandler.widgetRemoved.connect((sidePanel, widget) => {
          sidePanelPalette.removeItem(widget, sidePanel.area);
        notebookShell.rightHandler.widgetAdded.connect((sidePanel, widget) => {
        notebookShell.rightHandler.widgetRemoved.connect(
          (sidePanel, widget) => {
 * A plugin for defining keyboard shortcuts specific to the notebook application.
const shortcuts: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/application-extension:shortcuts',
    'A plugin for defining keyboard shortcuts specific to the notebook application.',
    // for now this plugin is mostly useful for defining keyboard shortcuts
    // specific to the notebook application
 * The default tree route resolver plugin.
const tree: JupyterFrontEndPlugin<JupyterFrontEnd.ITreeResolver> = {
  id: '@jupyter-notebook/application-extension:tree-resolver',
  description: 'The default tree route resolver plugin.',
  requires: [IRouter],
  provides: JupyterFrontEnd.ITreeResolver,
    router: IRouter
  ): JupyterFrontEnd.ITreeResolver => {
    const set = new DisposableSet();
    const delegate = new PromiseDelegate<JupyterFrontEnd.ITreeResolver.Paths>();
    const treePattern = new RegExp('/(/tree/.*)?');
    set.add(
      commands.addCommand(CommandIDs.resolveTree, {
        execute: (async (args: IRouter.ILocation) => {
          if (set.isDisposed) {
          const query = URLExt.queryStringToObject(args.search ?? '');
          const browser = query['file-browser-path'] || '';
          // Remove the file browser path from the query string.
          delete query['file-browser-path'];
          // Clean up artifacts immediately upon routing.
          set.dispose();
          delegate.resolve({ browser, file: PageConfig.getOption('treePath') });
        }) as (args: any) => Promise<void>,
      router.register({ command: CommandIDs.resolveTree, pattern: treePattern })
    // If a route is handled by the router without the tree command being
    // invoked, resolve to `null` and clean up artifacts.
      delegate.resolve(null);
    router.routed.connect(listener);
      new DisposableDelegate(() => {
        router.routed.disconnect(listener);
    return { paths: delegate.promise };
 * Plugin to update tree path.
const treePathUpdater: JupyterFrontEndPlugin<ITreePathUpdater> = {
  id: '@jupyter-notebook/application-extension:tree-updater',
  description: 'Plugin to update tree path.',
  provides: ITreePathUpdater,
  activate: (app: JupyterFrontEnd, router: IRouter) => {
    function updateTreePath(treePath: string) {
      if (treePath !== PageConfig.getOption('treePath')) {
        const path = URLExt.join(
          PageConfig.getOption('baseUrl') || '/',
          'tree',
          URLExt.encodeParts(treePath)
        router.navigate(path, { skipRouting: true });
        // Persist the new tree path to PageConfig as it is used elsewhere at runtime.
        PageConfig.setOption('treePath', treePath);
    return updateTreePath;
 * Translator plugin
const translator: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/application-extension:translator',
  description: 'Translator plugin',
    notebookShell.translator = translator;
 * Zen mode plugin
const zen: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/application-extension:zen',
  description: 'Zen mode plugin.',
  optional: [ICommandPalette, INotebookShell],
    palette: ICommandPalette | null,
    notebookShell: INotebookShell | null
    const elem = document.documentElement;
    const toggleOn = () => {
      notebookShell?.collapseTop();
      notebookShell?.menu.setHidden(true);
      zenModeEnabled = true;
    const toggleOff = () => {
      notebookShell?.expandTop();
      notebookShell?.menu.setHidden(false);
      zenModeEnabled = false;
    let zenModeEnabled = false;
    commands.addCommand(CommandIDs.toggleZen, {
      label: trans.__('Toggle Zen Mode'),
        if (!zenModeEnabled) {
          elem.requestFullscreen();
          toggleOn();
          document.exitFullscreen();
          toggleOff();
      if (!document.fullscreenElement) {
      palette.addItem({ command: CommandIDs.toggleZen, category: 'Mode' });
 * Export the plugins as default.
const plugins: JupyterFrontEndPlugin<any>[] = [
  dirty,
  info,
  logo,
  menus,
  menuSpacer,
  opener,
  pages,
  pathOpener,
  paths,
  rendermime,
  shell,
  sidePanelVisibility,
  shortcuts,
  splash,
  tabTitle,
  topVisibility,
  tree,
  treePathUpdater,
  translator,
  zen,
export default plugins;
export * from './app';
export * from './shell';
export * from './panelhandler';
export * from './pathopener';
export * from './tokens';
import { IConsoleTracker } from '@jupyterlab/console';
import { INotebookTracker } from '@jupyterlab/notebook';
import { consoleIcon } from '@jupyterlab/ui-components';
import { ReadonlyJSONObject } from '@lumino/coreutils';
 * A plugin to open consoles in a new tab
  id: '@jupyter-notebook/console-extension:opener',
  description: 'A plugin to open consoles in a new tab',
    const consolePattern = new RegExp('/consoles/(.*)');
    const command = 'router:console';
        const matches = parsed.path.match(consolePattern);
        const [, match] = matches;
        const path = decodeURIComponent(match);
        commands.execute('console:create', { path });
    router.register({ command, pattern: consolePattern });
 * Open consoles in a new tab or in the side panel (scratchpad like).
const redirect: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/console-extension:redirect',
  requires: [IConsoleTracker],
  optional: [INotebookPathOpener, INotebookShell, INotebookTracker],
  description: 'Open consoles in a new tab',
    tracker: IConsoleTracker,
    notebookPathOpener: INotebookPathOpener | null,
    notebookShell: INotebookShell | null,
    notebookTracker: INotebookTracker | null
    tracker.widgetAdded.connect(async (send, console) => {
      // Check if we should open the console in side panel:
      //  - this is a notebook view
      //  - the notebook and the console share the same kernel
      // Otherwise, the console opens in a new tab.
      if (notebookShell && notebookTracker) {
        const notebook = notebookTracker.currentWidget;
        // Wait for the notebook and console to be ready.
          notebook?.sessionContext.ready,
          console.sessionContext.ready,
        const notebookKernelId = notebook?.sessionContext.session?.kernel?.id;
        const consoleKernelId = console.sessionContext.session?.kernel?.id;
        if (notebookKernelId === consoleKernelId) {
          notebookShell.add(console, 'right');
          notebookShell.expandRight(console.id);
      const { sessionContext } = console;
      await sessionContext.ready;
        app.shell.widgets('main'),
        (w) => w.id === console.id
        // bail if the console is already added to the main area
        prefix: URLExt.join(baseUrl, 'consoles'),
        path: sessionContext.path,
      // the widget is not needed anymore
      console.dispose();
 * Open consoles in the side panel.
const scratchpadConsole: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/console-extension:scratchpad-console',
  requires: [INotebookTracker],
  optional: [INotebookShell, ICommandPalette, ITranslator],
  description: 'Open scratchpad console in side panel',
    tracker: INotebookTracker,
    translator: ITranslator | null
    const manager = app.serviceManager;
    const trans = (translator ?? nullTranslator).load('notebook');
    const command = 'scratchpad-console:open';
      label: (args) =>
        args['isPalette']
          ? trans.__('Open a scratchpad console')
          : trans.__('Scratchpad console'),
      isVisible: () => !!tracker.currentWidget,
      icon: (args) => (args['isPalette'] ? undefined : consoleIcon),
      execute: async (args) => {
        if (!notebookShell) {
        const consoleId = scratchpadConsole.id;
        const sidebar = notebookShell.rightHandler;
        // Close the console if it is already opened (shortcut only).
        if (sidebar.isVisible && sidebar.currentWidget?.id === consoleId) {
          if (!args.isPalette && !args.isMenu) {
            notebookShell.currentWidget?.activate();
        let panel: Widget | undefined = sidebar.widgets.find(
          (w) => w.id === consoleId
        // Create the widget if it is not already in the right area.
        if (!panel) {
          const notebook = tracker.currentWidget;
          if (!notebook) {
          const notebookSessionContext = notebook.sessionContext;
          await Promise.all([notebookSessionContext.ready, manager.ready]);
          const id = notebookSessionContext.session?.kernel?.id;
          const kernelPref = notebookSessionContext.kernelPreference;
          panel = await commands.execute('console:create', {
            kernelPreference: { ...kernelPref, id } as ReadonlyJSONObject,
            preventTitleUpdate: true,
              'An error occurred during scratchpad console creation'
          panel.title.caption = trans.__('Console');
          panel.id = consoleId;
          notebookShell.expandRight(consoleId);
      describedBy: {
            isPalette: {
              description: trans.__(
                'Whether the command is executed from the palette'
            isMenu: {
                'Whether the command is executed from the menu'
      palette.addItem({
        category: 'Notebook Console',
        args: { isPalette: true },
  redirect,
  scratchpadConsole,
import { IDocumentWidgetOpener } from '@jupyterlab/docmanager';
import { IDocumentWidget, DocumentRegistry } from '@jupyterlab/docregistry';
import { Signal } from '@lumino/signaling';
 * A plugin to open documents in a new browser tab.
const opener: JupyterFrontEndPlugin<IDocumentWidgetOpener> = {
  id: '@jupyter-notebook/docmanager-extension:opener',
  optional: [INotebookPathOpener, INotebookShell],
  provides: IDocumentWidgetOpener,
  description: 'Open documents in a new browser tab',
    notebookPathOpener: INotebookPathOpener,
    const docRegistry = app.docRegistry;
    const pathOpener = notebookPathOpener ?? defaultNotebookPathOpener;
    let id = 0;
    return new (class {
      async open(
        widget: IDocumentWidget,
        const widgetName = options?.type ?? '';
        const ref = options?.ref;
        // check if there is an setting override and if it would add the widget in the main area
        const userLayoutArea = notebookShell?.userLayout?.[widgetName]?.area;
        if (ref !== '_noref' && userLayoutArea === undefined) {
          const path = widget.context.path;
          const ext = PathExt.extname(path);
          let route = 'edit';
            (widgetName === 'default' && ext === '.ipynb') ||
            widgetName.includes('Notebook')
            // make sure to save the notebook before opening it in a new tab
            // so the kernel info is saved (if created from the New dropdown)
            if (widget.context.sessionContext.kernelPreference.name) {
              await widget.context.save();
            route = 'notebooks';
          // append ?factory only if it's not the default
          const defaultFactory = docRegistry.defaultWidgetFactory(path);
          let searchParams = undefined;
          if (widgetName !== defaultFactory.name) {
            searchParams = new URLSearchParams({
              factory: widgetName,
          pathOpener.open({
            prefix: URLExt.join(baseUrl, route),
          // dispose the widget since it is not used on this page
          widget.dispose();
        // otherwise open the document on the current page
        if (!widget.id) {
          widget.id = `document-manager-${++id}`;
        widget.title.dataset = {
          type: 'document-title',
          ...widget.title.dataset,
        if (!widget.isAttached) {
          app.shell.add(widget, 'main', options || {});
        app.shell.activateById(widget.id);
        this._opened.emit(widget);
      get opened() {
        return this._opened;
      private _opened = new Signal<this, IDocumentWidget>(this);
const plugins: JupyterFrontEndPlugin<any>[] = [opener];
import { ISearchProviderRegistry } from '@jupyterlab/documentsearch';
import { INotebookShell } from '@jupyter-notebook/application';
const SEARCHABLE_CLASS = 'jp-mod-searchable';
 * A plugin to add document search functionalities.
const notebookShellWidgetListener: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/documentsearch-extension:notebookShellWidgetListener',
  requires: [INotebookShell, ISearchProviderRegistry],
  description: 'A plugin to add document search functionalities',
    registry: ISearchProviderRegistry
    // If a given widget is searchable, apply the searchable class.
    // If it's not searchable, remove the class.
    const transformWidgetSearchability = (widget: Widget | null) => {
      if (!widget) {
      if (registry.hasProvider(widget)) {
        widget.addClass(SEARCHABLE_CLASS);
        widget.removeClass(SEARCHABLE_CLASS);
    // Update searchability of the active widget when the registry
    // changes, in case a provider for the current widget was added
    // or removed
    registry.changed.connect(() =>
      transformWidgetSearchability(notebookShell.currentWidget)
    // Apply the searchable class only to the active widget if it is actually
    // searchable. Remove the searchable class from a widget when it's
    // no longer active.
    notebookShell.currentChanged.connect((_, args) => {
        transformWidgetSearchability(notebookShell.currentWidget);
const plugins: JupyterFrontEndPlugin<any>[] = [notebookShellWidgetListener];
  ILabShell,
import { ICommandPalette, IToolbarWidgetRegistry } from '@jupyterlab/apputils';
import { INotebookTracker, NotebookPanel } from '@jupyterlab/notebook';
import { ITranslator } from '@jupyterlab/translation';
import { Menu, MenuBar, Widget } from '@lumino/widgets';
  caretDownIcon,
  CommandToolbarButton,
  launchIcon,
} from '@jupyterlab/ui-components';
   * Launch Jupyter Notebook Tree
  export const launchNotebookTree = 'jupyter-notebook:launch-tree';
   * Open Jupyter Notebook
  export const openNotebook = 'jupyter-notebook:open-notebook';
   * Open in JupyterLab
  export const openLab = 'jupyter-notebook:open-lab';
   * Open in NbClassic
  export const openNbClassic = 'jupyter-notebook:open-nbclassic';
interface ISwitcherChoice {
  commandLabel: string;
  commandDescription: string;
  buttonLabel: string;
  urlPrefix: string;
 * A plugin to add custom toolbar items to the notebook page
const interfaceSwitcher: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/lab-extension:interface-switcher',
  description: 'A plugin to add custom toolbar items to the notebook page.',
    INotebookTracker,
    notebookTracker: INotebookTracker | null,
    labShell: ILabShell | null,
    if (!notebookTracker) {
      // bail if trying to use this plugin without a notebook tracker
    const { commands, shell } = app;
    const nbClassicEnabled =
      PageConfig.getOption('nbclassic_enabled') === 'true';
    const switcher = new Menu({ commands });
    const switcherOptions: ISwitcherChoice[] = [];
      switcherOptions.push({
        command: CommandIDs.openNotebook,
        commandLabel: trans.__('Notebook'),
        commandDescription: trans.__('Open in %1', 'Jupyter Notebook'),
        buttonLabel: 'openNotebook',
        urlPrefix: `${baseUrl}tree`,
    if (!labShell) {
        command: CommandIDs.openLab,
        commandLabel: trans.__('JupyterLab'),
        commandDescription: trans.__('Open in %1', 'JupyterLab'),
        buttonLabel: 'openLab',
        urlPrefix: `${baseUrl}doc/tree`,
    if (nbClassicEnabled) {
        command: CommandIDs.openNbClassic,
        commandLabel: trans.__('NbClassic'),
        commandDescription: trans.__('Open in %1', 'NbClassic'),
        buttonLabel: 'openNbClassic',
        urlPrefix: `${baseUrl}nbclassic/notebooks`,
        notebookTracker.currentWidget !== null &&
        notebookTracker.currentWidget === shell.currentWidget
    const addSwitcherCommand = (option: ISwitcherChoice) => {
      const { command, commandLabel, commandDescription, urlPrefix } = option;
      const execute = () => {
        const current = notebookTracker.currentWidget;
          prefix: urlPrefix,
          path: current.context.path,
        label: (args) => {
          if (args.noLabel) {
          if (args.isMenu || args.isPalette) {
            return commandDescription;
          return commandLabel;
        caption: commandLabel,
        execute,
          category: 'Other',
    switcherOptions.forEach((option) => {
      const { command } = option;
      addSwitcherCommand(option);
      switcher.addItem({ command });
    let toolbarFactory: (panel: NotebookPanel) => Widget;
    if (switcherOptions.length === 1) {
      toolbarFactory = (panel: NotebookPanel) => {
        const toolbarButton = new CommandToolbarButton({
          id: switcherOptions[0].command,
          label: switcherOptions[0].commandLabel,
          icon: launchIcon,
        toolbarButton.addClass('jp-nb-interface-switcher-button');
        return toolbarButton;
      const overflowOptions = {
        overflowMenuOptions: { isVisible: false },
      const menubar = new MenuBar(overflowOptions);
      switcher.title.label = trans.__('Open in...');
      switcher.title.icon = caretDownIcon;
      menubar.addMenu(switcher);
        menubar.addClass('jp-InterfaceSwitcher');
        return menubar;
      toolbarRegistry.addFactory<NotebookPanel>(
        'Notebook',
        'interfaceSwitcher',
        toolbarFactory
 * A plugin to add a command to open the Jupyter Notebook Tree.
const launchNotebookTree: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/lab-extension:launch-tree',
  description: 'A plugin to add a command to open the Jupyter Notebook Tree.',
    const category = trans.__('Help');
    commands.addCommand(CommandIDs.launchNotebookTree, {
      label: trans.__('Launch Jupyter Notebook File Browser'),
        const url = URLExt.join(PageConfig.getBaseUrl(), 'tree');
        window.open(url);
      palette.addItem({ command: CommandIDs.launchNotebookTree, category });
  launchNotebookTree,
  interfaceSwitcher,
  ISessionContext,
import { Cell, CodeCell } from '@jupyterlab/cells';
import { PageConfig, Text, Time, URLExt } from '@jupyterlab/coreutils';
import { IDebugger, IDebuggerSidebar } from '@jupyterlab/debugger';
import { IDocumentManager } from '@jupyterlab/docmanager';
  NotebookPanel,
  INotebookTools,
} from '@jupyterlab/notebook';
import { ITableOfContentsTracker } from '@jupyterlab/toc';
import { Poll } from '@lumino/polling';
import { TrustedComponent } from './trusted';
 * The class for kernel status errors.
const KERNEL_STATUS_ERROR_CLASS = 'jp-NotebookKernelStatus-error';
 * The class for kernel status warnings.
const KERNEL_STATUS_WARN_CLASS = 'jp-NotebookKernelStatus-warn';
 * The class for kernel status infos.
const KERNEL_STATUS_INFO_CLASS = 'jp-NotebookKernelStatus-info';
 * The class to fade out the kernel status.
const KERNEL_STATUS_FADE_OUT_CLASS = 'jp-NotebookKernelStatus-fade';
 * The class for scrolled outputs
const SCROLLED_OUTPUTS_CLASS = 'jp-mod-outputsScrolled';
 * The class for the full width notebook
const FULL_WIDTH_NOTEBOOK_CLASS = 'jp-mod-fullwidth';
 * The command IDs used by the notebook plugins.
   * A command to open right sidebar for Editing Notebook Metadata
  export const openEditNotebookMetadata = 'notebook:edit-metadata';
   * A command to toggle full width of the notebook
  export const toggleFullWidth = 'notebook:toggle-full-width';
 * A plugin for the checkpoint indicator
const checkpoints: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/notebook-extension:checkpoints',
  description: 'A plugin for the checkpoint indicator.',
  requires: [IDocumentManager, ITranslator],
  optional: [INotebookShell, IToolbarWidgetRegistry, ISettingRegistry],
    toolbarRegistry: IToolbarWidgetRegistry | null,
    const { shell } = app;
      toolbarRegistry.addFactory('TopBar', 'checkpoint', (toolbar) => {
        widget.id = DOMUtils.createDomID();
        widget.addClass('jp-NotebookCheckpoint');
    const getCurrent = () => {
      const context = docManager.contextForWidget(current);
    const updateCheckpointDisplay = async () => {
      const current = getCurrent();
      const checkpoints = await current.listCheckpoints();
      if (!checkpoints || !checkpoints.length) {
        node.textContent = '';
      const checkpoint = checkpoints[checkpoints.length - 1];
      node.textContent = trans.__(
        'Last Checkpoint: %1',
        Time.formatHuman(new Date(checkpoint.last_modified))
    const onSaveState = async (
      sender: DocumentRegistry.IContext<DocumentRegistry.IModel>,
      state: DocumentRegistry.SaveState
      if (state !== 'completed') {
      // Add a small artificial delay so that the UI can pick up the newly created checkpoint.
      // Since the save state signal is emitted after a file save, but not after a checkpoint has been created.
        void updateCheckpointDisplay();
    const onChange = async () => {
      const context = getCurrent();
      context.saveState.disconnect(onSaveState);
      context.saveState.connect(onSaveState);
      await updateCheckpointDisplay();
    if (notebookShell) {
      notebookShell.currentChanged.connect(onChange);
    let checkpointPollingInterval = 30; // Default 30 seconds
    let poll: Poll | null = null;
    const createPoll = () => {
      if (poll) {
        poll.dispose();
      if (checkpointPollingInterval > 0) {
        poll = new Poll({
          auto: true,
          factory: () => updateCheckpointDisplay(),
          frequency: {
            interval: checkpointPollingInterval * 1000,
            backoff: false,
          standby: 'when-hidden',
      checkpointPollingInterval = settings.get('checkpointPollingInterval')
        .composite as number;
      createPoll();
      const loadSettings = settingRegistry.load(checkpoints.id);
          settings.changed.connect(updateSettings);
            `Failed to load settings for ${checkpoints.id}: ${reason.message}`
          // Fall back to creating poll with default settings
      // Create poll with default settings
 * Add a command to close the browser tab when clicking on "Close and Shut Down"
const closeTab: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/notebook-extension:close-tab',
    'Add a command to close the browser tab when clicking on "Close and Shut Down".',
  optional: [ITranslator],
    menu: IMainMenu,
    translator = translator ?? nullTranslator;
    const id = 'notebook:close-and-halt';
    commands.addCommand(id, {
      label: trans.__('Close and Shut Down Notebook'),
        // Shut the kernel down, without confirmation
        await commands.execute('notebook:shutdown-kernel', { activate: false });
    menu.fileMenu.closeAndCleaners.add({
      // use a small rank to it takes precedence over the default
      // shut down action for the notebook
      rank: 0,
 * Add a command to open the tree view from the notebook view
const openTreeTab: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/notebook-extension:open-tree-tab',
    'Add a command to open a browser tab on the tree view when clicking "Open...".',
  activate: (app: JupyterFrontEnd, translator: ITranslator | null) => {
    const id = 'notebook:open-tree-tab';
      label: trans.__('Open…'),
 * A plugin to set the notebook to full width.
const fullWidthNotebook: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/notebook-extension:full-width-notebook',
  description: 'A plugin to set the notebook to full width.',
  optional: [ICommandPalette, ISettingRegistry, ITranslator],
    let fullWidth = false;
    const toggleFullWidth = () => {
      const current = tracker.currentWidget;
      fullWidth = !fullWidth;
      const content = current;
      content.toggleClass(FULL_WIDTH_NOTEBOOK_CLASS, fullWidth);
    let notebookSettings: ISettingRegistry.ISettings;
      const loadSettings = settingRegistry.load(fullWidthNotebook.id);
        const newFullWidth = settings.get('fullWidthNotebook')
          .composite as boolean;
        if (newFullWidth !== fullWidth) {
          toggleFullWidth();
          notebookSettings = settings;
    app.commands.addCommand(CommandIDs.toggleFullWidth, {
      label: trans.__('Enable Full Width Notebook'),
        if (notebookSettings) {
          notebookSettings.set('fullWidthNotebook', fullWidth);
      isEnabled: () => tracker.currentWidget !== null,
      isToggled: () => fullWidth,
        command: CommandIDs.toggleFullWidth,
        category: 'Notebook Operations',
 * The kernel logo plugin.
const kernelLogo: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/notebook-extension:kernel-logo',
  description: 'The kernel logo plugin.',
  optional: [IToolbarWidgetRegistry],
    const { serviceManager } = app;
      if (!(current instanceof NotebookPanel)) {
      if (!node.hasChildNodes()) {
        node.appendChild(img);
      await current.sessionContext.ready;
      current.sessionContext.kernelChanged.disconnect(onChange);
      current.sessionContext.kernelChanged.connect(onChange);
      const name = current.sessionContext.session?.kernel?.name ?? '';
      const spec = serviceManager.kernelspecs?.specs?.kernelspecs[name];
        node.childNodes[0].remove();
      const kernelIconUrl = spec.resources['logo-64x64'];
      if (!kernelIconUrl) {
      img.src = kernelIconUrl;
      img.title = spec.display_name;
      toolbarRegistry.addFactory('TopBar', 'kernelLogo', (toolbar) => {
        widget.addClass('jp-NotebookKernelLogo');
    app.started.then(() => {
      shell.currentChanged.connect(onChange);
 * A plugin to display the kernel status;
const kernelStatus: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/notebook-extension:kernel-status',
  description: 'A plugin to display the kernel status.',
    widget.addClass('jp-NotebookKernelStatus');
    app.shell.add(widget, 'menu', { rank: 10_010 });
    const removeClasses = () => {
      widget.removeClass(KERNEL_STATUS_ERROR_CLASS);
      widget.removeClass(KERNEL_STATUS_WARN_CLASS);
      widget.removeClass(KERNEL_STATUS_INFO_CLASS);
      widget.removeClass(KERNEL_STATUS_FADE_OUT_CLASS);
    const onStatusChanged = (sessionContext: ISessionContext) => {
      const status = sessionContext.kernelDisplayStatus;
      let text = `Kernel ${Text.titleCase(status)}`;
      removeClasses();
        case 'busy':
        case 'idle':
          text = '';
          widget.addClass(KERNEL_STATUS_FADE_OUT_CLASS);
        case 'dead':
        case 'terminating':
          widget.addClass(KERNEL_STATUS_ERROR_CLASS);
        case 'unknown':
          widget.addClass(KERNEL_STATUS_WARN_CLASS);
          widget.addClass(KERNEL_STATUS_INFO_CLASS);
      widget.node.textContent = trans.__(text);
      const sessionContext = current.sessionContext;
      sessionContext.statusChanged.connect(onStatusChanged);
 * A plugin to enable scrolling for outputs by default.
 * Mimic the logic from the classic notebook, as found here:
 * https://github.com/jupyter/notebook/blob/a9a31c096eeffe1bff4e9164c6a0442e0e13cdb3/notebook/static/notebook/js/outputarea.js#L96-L120
const scrollOutput: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/notebook-extension:scroll-output',
  description: 'A plugin to enable scrolling for outputs by default.',
  activate: async (
    const autoScrollThreshold = 100;
    let autoScrollOutputs = true;
    // decide whether to scroll the output of the cell based on some heuristics
    const autoScroll = (cell: CodeCell) => {
      if (!autoScrollOutputs) {
        // bail if disabled via the settings
        cell.removeClass(SCROLLED_OUTPUTS_CLASS);
      const { outputArea } = cell;
      // respect cells with an explicit scrolled state
      const scrolled = cell.model.getMetadata('scrolled');
      if (scrolled !== undefined) {
      const { node } = outputArea;
      const height = node.scrollHeight;
      const fontSize = parseFloat(node.style.fontSize.replace('px', ''));
      const lineHeight = (fontSize || 14) * 1.3;
      // do not set via cell.outputScrolled = true, as this would
      // otherwise synchronize the scrolled state to the notebook metadata
      const scroll = height > lineHeight * autoScrollThreshold;
      cell.toggleClass(SCROLLED_OUTPUTS_CLASS, scroll);
    const handlers: { [id: string]: () => void } = {};
    const setAutoScroll = (cell: Cell) => {
      if (cell.model.type === 'code') {
        const codeCell = cell as CodeCell;
        const id = codeCell.model.id;
        autoScroll(codeCell);
        if (handlers[id]) {
          codeCell.outputArea.model.changed.disconnect(handlers[id]);
        handlers[id] = () => autoScroll(codeCell);
        codeCell.outputArea.model.changed.connect(handlers[id]);
    tracker.widgetAdded.connect((sender, notebook) => {
      // when the notebook widget is created, process all the cells
      notebook.sessionContext.ready.then(() => {
        notebook.content.widgets.forEach(setAutoScroll);
      notebook.model?.cells.changed.connect((sender, args) => {
      const loadSettings = settingRegistry.load(scrollOutput.id);
        autoScrollOutputs = settings.get('autoScrollOutputs')
 * A plugin to add the NotebookTools to the side panel;
const notebookToolsWidget: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/notebook-extension:notebook-tools',
  description: 'A plugin to add the NotebookTools to the side panel.',
  optional: [INotebookTools],
    notebookTools: INotebookTools | null
      // Add the notebook tools in right area.
      if (notebookTools) {
        shell.add(notebookTools, 'right', { type: 'Property Inspector' });
 * A plugin to update the tab icon based on the kernel status.
const tabIcon: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/notebook-extension:tab-icon',
  description: 'A plugin to update the tab icon based on the kernel status.',
  activate: (app: JupyterFrontEnd, tracker: INotebookTracker) => {
    // the favicons are provided by Jupyter Server
    const baseURL = PageConfig.getBaseUrl();
    const notebookIcon = URLExt.join(
      'static/favicons/favicon-notebook.ico'
    const busyIcon = URLExt.join(baseURL, 'static/favicons/favicon-busy-1.ico');
    const updateBrowserFavicon = (
      status: ISessionContext.KernelDisplayStatus
      const link = document.querySelector(
        "link[rel*='icon']"
      ) as HTMLLinkElement;
          link.href = busyIcon;
          link.href = notebookIcon;
      const sessionContext = current?.sessionContext;
      if (!sessionContext) {
      sessionContext.statusChanged.connect(() => {
        updateBrowserFavicon(status);
    tracker.currentChanged.connect(onChange);
 * A plugin that adds a Trusted indicator to the menu area
const trusted: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/notebook-extension:trusted',
  description: 'A plugin that adds a Trusted indicator to the menu area.',
      const current = notebookShell.currentWidget;
      const notebook = current.content;
      await current.context.ready;
      const widget = TrustedComponent.create({ notebook, translator });
      notebookShell.add(widget, 'menu', {
        rank: 11_000,
 * Add a command to open right sidebar for Editing Notebook Metadata when clicking on "Edit Notebook Metadata" under Edit menu
const editNotebookMetadata: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/notebook-extension:edit-notebook-metadata',
    'Add a command to open right sidebar for Editing Notebook Metadata when clicking on "Edit Notebook Metadata" under Edit menu',
  optional: [ICommandPalette, ITranslator, INotebookTools],
    commands.addCommand(CommandIDs.openEditNotebookMetadata, {
      label: trans.__('Edit Notebook Metadata'),
        const command = 'application:toggle-panel';
          side: 'right',
          title: 'Show Notebook Tools',
          id: 'notebook-tools',
        // Check if Show Notebook Tools (Right Sidebar) is open (expanded)
        if (!commands.isToggled(command, args)) {
          await commands.execute(command, args).then((_) => {
            // For expanding the 'Advanced Tools' section (default: collapsed)
              const tools = (notebookTools?.layout as any).widgets;
              tools.forEach((tool: any) => {
                  tool.widget.title.label === trans.__('Advanced Tools') &&
                  tool.collapsed
                  tool.toggle();
      isVisible: () =>
        shell.currentWidget !== null &&
        shell.currentWidget instanceof NotebookPanel,
        command: CommandIDs.openEditNotebookMetadata,
 * A plugin to replace the menu item activating the TOC panel, to allow toggling it.
const overrideMenuItems: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/notebook-extension:menu-override',
  description: 'A plugin to override some menu items',
    IDebuggerSidebar,
    IMainMenu,
    ITableOfContentsTracker,
    debuggerSidebar: IDebugger.ISidebar | null,
    mainMenu: IMainMenu | null,
    shell: INotebookShell | null,
    tocTracker: ITableOfContentsTracker | null,
    if (!mainMenu || !shell) {
    if (tocTracker) {
      const TOC_PANEL_ID = 'table-of-contents';
      commands.addCommand('toc:toggle-panel', {
        label: trans.__('Table of Contents'),
        isToggleable: true,
        isToggled: () => {
          const area = shell.getWidgetArea(TOC_PANEL_ID);
          if (!area) {
            shell.widgets(area as INotebookShell.Area),
            (w) => w.id === TOC_PANEL_ID
          return shell.isSidePanelVisible(area) && widget.isVisible;
          if (shell.isSidePanelVisible(area) && widget?.isVisible) {
            shell.collapse(area);
            shell.activateById(TOC_PANEL_ID);
    if (debuggerSidebar) {
      const DEBUGGER_PANEL_ID = 'jp-debugger-sidebar';
      commands.addCommand('debugger:toggle-panel', {
        label: trans.__('Debugger Panel'),
          const area = shell.getWidgetArea(DEBUGGER_PANEL_ID);
            (w) => w.id === DEBUGGER_PANEL_ID
            shell.activateById(DEBUGGER_PANEL_ID);
  checkpoints,
  closeTab,
  openTreeTab,
  editNotebookMetadata,
  fullWidthNotebook,
  kernelLogo,
  kernelStatus,
  notebookToolsWidget,
  overrideMenuItems,
  scrollOutput,
  tabIcon,
  trusted,
import { ITerminalTracker } from '@jupyterlab/terminal';
 * A plugin to open terminals in a new tab
  id: '@jupyter-notebook/terminal-extension:opener',
  description: 'A plugin to open terminals in a new tab.',
  requires: [IRouter, ITerminalTracker],
    tracker: ITerminalTracker
    const terminalPattern = new RegExp('/terminals/(.*)');
    const command = 'router:terminal';
        const matches = parsed.path.match(terminalPattern);
        const [, name] = matches;
        tracker.widgetAdded.connect((send, terminal) => {
          terminal.content.setOption('closeOnExit', false);
        commands.execute('terminal:open', { name });
    router.register({ command, pattern: terminalPattern });
 * Open terminals in a new tab.
  id: '@jupyter-notebook/terminal-extension:redirect',
  description: 'Open terminals in a new tab.',
  requires: [ITerminalTracker],
  optional: [INotebookPathOpener],
    tracker: ITerminalTracker,
        (w) => w.id === terminal.id
        // bail if the terminal is already added to the main area
      const name = terminal.content.session.name;
        prefix: URLExt.join(baseUrl, 'terminals'),
      terminal.dispose();
const plugins: JupyterFrontEndPlugin<any>[] = [opener, redirect];
  createToolbarFactory,
  setToolbar,
  FileBrowser,
  Uploader,
  IDefaultFileBrowser,
  IFileBrowserFactory,
} from '@jupyterlab/filebrowser';
import { IRunningSessionManagers, RunningSessions } from '@jupyterlab/running';
  IJSONSettingEditorTracker,
  ISettingEditorTracker,
} from '@jupyterlab/settingeditor';
  folderIcon,
  runningIcon,
import { Menu, MenuBar } from '@lumino/widgets';
import { NotebookTreeWidget, INotebookTree } from '@jupyter-notebook/tree';
import { FilesActionButtons } from './fileactions';
 * The file browser factory.
const FILE_BROWSER_FACTORY = 'FileBrowser';
 * The file browser plugin id.
const FILE_BROWSER_PLUGIN_ID = '@jupyterlab/filebrowser-extension:browser';
 * The namespace for command IDs.
  // The command to activate the filebrowser widget in tree view.
  export const activate = 'filebrowser:activate';
  // Activate the file filter in the file browser
  export const toggleFileFilter = 'filebrowser:toggle-file-filter';
 * Plugin to add extra commands to the file browser to create
 * new notebooks, files, consoles and terminals
const createNew: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/tree-extension:new',
    'Plugin to add extra commands to the file browser to create new notebooks, files, consoles and terminals.',
    const { commands, serviceManager } = app;
    const newMenu = new Menu({ commands });
    newMenu.title.label = trans.__('New');
    newMenu.title.icon = caretDownIcon;
    menubar.addMenu(newMenu);
    const populateNewMenu = () => {
      // create an entry per kernel spec for creating a new notebook
      const specs = serviceManager.kernelspecs?.specs?.kernelspecs;
      for (const name in specs) {
          args: { kernelName: name, isLauncher: true },
          command: 'notebook:create-new',
      const baseCommands = [
        'terminal:create-new',
        'console:create',
        'filebrowser:create-new-file',
        'filebrowser:create-new-directory',
      baseCommands.forEach((command) => {
        newMenu.addItem({ command });
    serviceManager.kernelspecs?.specsChanged.connect(() => {
      newMenu.clearItems();
      populateNewMenu();
      toolbarRegistry.addFactory(
        FILE_BROWSER_FACTORY,
        'new-dropdown',
        (browser: FileBrowser) => {
          menubar.addClass('jp-DropdownMenu');
 * A plugin to add file browser actions to the file browser toolbar.
const fileActions: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/tree-extension:file-actions',
    'A plugin to add file browser actions to the file browser toolbar.',
  requires: [IDefaultFileBrowser, IToolbarWidgetRegistry, ITranslator],
    browser: IDefaultFileBrowser,
    toolbarRegistry: IToolbarWidgetRegistry,
    // Create a toolbar item that adds buttons to the file browser toolbar
    // to perform actions on the files
    const { selectionChanged } = browser;
    const fileActions = new FilesActionButtons({
      browser,
      selectionChanged,
    for (const widget of fileActions.widgets) {
      toolbarRegistry.addFactory(FILE_BROWSER_FACTORY, widget.id, () => widget);
 * A plugin to set the default file browser settings.
const fileBrowserSettings: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/tree-extension:settings',
  description: 'Set up the default file browser settings',
  requires: [IDefaultFileBrowser],
    // Default config for notebook.
    // This is a different set of defaults than JupyterLab.
    const defaultFileBrowserConfig = {
      navigateToCurrentDirectory: false,
      singleClickNavigation: true,
      showLastModifiedColumn: true,
      showFileSizeColumn: true,
      showHiddenFiles: false,
      showFileCheckboxes: true,
      sortNotebooksFirst: true,
      showFullPath: false,
    // Apply defaults on plugin activation
    let key: keyof typeof defaultFileBrowserConfig;
    for (key in defaultFileBrowserConfig) {
      browser[key] = defaultFileBrowserConfig[key];
      void settingRegistry.load(FILE_BROWSER_PLUGIN_ID).then((settings) => {
        function onSettingsChanged(settings: ISettingRegistry.ISettings): void {
            const value = settings.get(key).user as boolean;
            // only set the setting if it is defined by the user
              browser[key] = value;
        settings.changed.connect(onSettingsChanged);
        onSettingsChanged(settings);
 * A plugin to add the file filter toggle command to the palette
const fileFilterCommand: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/tree-extension:file-filter-command',
  description: 'A plugin to add file filter command to the palette.',
  activate: (app: JupyterFrontEnd, palette: ICommandPalette | null) => {
        command: CommandIDs.toggleFileFilter,
        category: 'File Browser',
 * Plugin to load the default plugins that are loaded on all the Notebook pages
 * (tree, edit, view, etc.) so they are visible in the settings editor.
const loadPlugins: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/tree-extension:load-plugins',
    'Plugin to load the default plugins that are loaded on all the Notebook pages (tree, edit, view, etc.) so they are visible in the settings editor.',
  requires: [ISettingRegistry],
  activate(app: JupyterFrontEnd, settingRegistry: ISettingRegistry) {
    const { isDisabled } = PageConfig.Extension;
    const connector = settingRegistry.connector;
    const allPluginsOption = PageConfig.getOption('allPlugins');
    if (!allPluginsOption) {
    // build the list of plugins shipped by default on the all the notebook pages
    // this avoid explicitly loading `'all'` plugins such as the ones used
    // in JupyterLab only
    const allPlugins = JSON.parse(allPluginsOption);
    const pluginsSet = new Set<string>();
    Object.keys(allPlugins).forEach((key: string) => {
      const extensionsAndPlugins: { [key: string]: boolean | [string] } =
        allPlugins[key];
      Object.keys(extensionsAndPlugins).forEach((plugin) => {
        const value = extensionsAndPlugins[plugin];
        if (typeof value === 'boolean' && value) {
          pluginsSet.add(plugin);
          value.forEach((v: string) => {
            pluginsSet.add(v);
    app.restored.then(async () => {
      const plugins = await connector.list('all');
        plugins.ids.map(async (id: string) => {
          const [extension] = id.split(':');
          // load the plugin if it is built-in the notebook application explicitly
          // either included as an extension or as a plugin directly
          const hasPlugin = pluginsSet.has(extension) || pluginsSet.has(id);
          if (!hasPlugin || isDisabled(id) || id in settingRegistry.plugins) {
            await settingRegistry.load(id);
            console.warn(`Settings failed to load for (${id})`, error);
 * A plugin to add file browser commands for the tree view.
const openFileBrowser: JupyterFrontEndPlugin<void> = {
  id: '@jupyter-notebook/tree-extension:open-file-browser',
  description: 'A plugin to add file browser commands for the tree view.',
  requires: [INotebookTree, IDefaultFileBrowser],
    notebookTree: INotebookTree,
    browser: IDefaultFileBrowser
    commands.addCommand(CommandIDs.activate, {
        notebookTree.currentWidget = browser;
 * A plugin to add the file browser widget to an INotebookShell
const notebookTreeWidget: JupyterFrontEndPlugin<INotebookTree> = {
  id: '@jupyter-notebook/tree-extension:widget',
  description: 'A plugin to add the file browser widget to an INotebookShell.',
  requires: [
    ISettingRegistry,
    IRunningSessionManagers,
  provides: INotebookTree,
    settingRegistry: ISettingRegistry,
    factory: IFileBrowserFactory,
    manager: IRunningSessionManagers | null,
    settingEditorTracker: ISettingEditorTracker | null,
    jsonSettingEditorTracker: IJSONSettingEditorTracker | null
  ): INotebookTree => {
    const nbTreeWidget = new NotebookTreeWidget();
    browser.title.label = trans.__('Files');
    browser.node.setAttribute('role', 'region');
    browser.node.setAttribute('aria-label', trans.__('File Browser Section'));
    browser.title.icon = folderIcon;
    nbTreeWidget.addWidget(browser);
    nbTreeWidget.tabBar.addTab(browser.title);
    nbTreeWidget.tabsMovable = false;
      'uploader',
      (browser: FileBrowser) =>
        new Uploader({
          model: browser.model,
          label: trans.__('Upload'),
    setToolbar(
      createToolbarFactory(
        toolbarRegistry,
        settingRegistry,
        notebookTreeWidget.id,
        translator
    if (manager) {
      const running = new RunningSessions(manager, translator);
      running.id = 'jp-running-sessions-tree';
      running.title.label = trans.__('Running');
      running.title.icon = runningIcon;
      nbTreeWidget.addWidget(running);
      nbTreeWidget.tabBar.addTab(running.title);
    app.shell.add(nbTreeWidget, 'main', { rank: 100 });
    // add a separate tab for each setting editor
    [settingEditorTracker, jsonSettingEditorTracker].forEach(
      (editorTracker) => {
        if (editorTracker) {
          editorTracker.widgetAdded.connect((_, editor) => {
            nbTreeWidget.addWidget(editor);
            nbTreeWidget.tabBar.addTab(editor.title);
            nbTreeWidget.currentWidget = editor;
    const { tracker } = factory;
    // TODO: remove
    // Workaround to force the focus on the default file browser
    // See https://github.com/jupyterlab/jupyterlab/issues/15629 for more info
    const setCurrentToDefaultBrower = () => {
      tracker['_pool'].current = browser;
    tracker.widgetAdded.connect((sender, widget) => {
      setCurrentToDefaultBrower();
    return nbTreeWidget;
  createNew,
  fileActions,
  fileBrowserSettings,
  fileFilterCommand,
  loadPlugins,
  openFileBrowser,
  notebookTreeWidget,
export * from './notebook-tree';
export * from './token';
export * from './icon';
export * from './iconimports';
export * from './config.js';export { run } from '@oclif/core';
} from '@memberjunction/ai-reranker'; * @fileoverview Agent type implementations for the MemberJunction AI Agent framework.
export * from './generic/errorAnalyzer';export * from './prompt.types';
export * from './types/ExampleMatchResult';  * @fileoverview MCP Client package for MemberJunction
export * from './models/anthropic';export * from './models/azure';
export * from './config';export * from './models/bedrockLLM';
export * from './models/bedrockEmbedding';export * from './generic/BettyBot.types';
export * from './models/cerebras'; * @memberjunction/ai-cohere
export * from './geminiImage';export * from './models/groq'; import { AvatarInfo, AvatarVideoParams, BaseVideoGenerator, VideoResult, ErrorAnalyzer } from "@memberjunction/ai";
export * from './models/lm-studio'; export * from './models/localEmbedding';
export * from './models/mistralEmbedding';export * from './models/ollama-llm';
export * from './models/openAIImage';export * from './models/openRouter'; export * from './provider';export * from "./models/PineconeDatabase";export * from './models/vertexLLM';
export * from './models/x.ai'; export * from './Engine';
export * from './generic/types'; * @memberjunction/ai-reranker
export * from './generic/VectorCore.types'; export * from './generic/record';
export * from './generic/query.types';export * from './generic/vectorSyncBase';
export * from './duplicateRecordDetector'; * @module @memberjunction/ai-vectors-memory
} from './models/SimpleVectorService';export * from './generic/vectorSync.types';
// Would appreciate feedback on (a) the approach to how we package this stuff and (b) the approach to params noted above.export * from './ActionEngine-Base';
export * from './EntityActionEntity-Extended';// Export all accounting actions
// export * from './common/validate-journal-entry.action'; * CRM Integration Actions
export * from './providers/hubspot'; * HubSpot CRM Provider
export * from './actions'; * HubSpot CRM Actions
export * from './get-upcoming-tasks.action';// Base Classes
export * from './providers/google-forms';export * from './base-form-builder.action';
export * from './providers/learnworlds';// Export LearnWorlds base action
export * from './get-quiz-results.action';export * from './base/base-social.action';
export * from './providers/youtube';export * from './buffer-base.action';
export * from './search-posts.action';export * from './facebook-base.action';
export * from './actions/search-posts.action';// Instagram Provider
export * from './search-videos.action'; * Twitter/X Provider for MemberJunction Social Media Actions
export * from './search-tweets.action';export * from './youtube-base.action';
export * from './generic/content-autotag-and-vectorize.action';export * from './generated/action_subclasses';
export * from './delete-record.action'; * File Storage Actions
export * from './json-param-helper';export * from './generic/BaseAction';
export * from './entity-actions/EntityActionInvocationTypes';export * from "./scheduler";import express from 'express';
export * from './components/charts/performance-heatmap.component';export * from './api-keys-resource.component';
export * from './components/text-import-dialog.component';// Main dashboard component
export * from './components';export * from './labels-resource.component';
export * from './runCodeGen'// PUBLIC API SURFACE AREA
export * from './CredentialUtils';export * from './client';export * from './entity-communications'; * @memberjunction/notifications
export type { MSGraphCredentials } from './MSGraphProvider';export * from './GmailProvider';
export type { GmailCredentials } from './GmailProvider';export * from './SendGridProvider';
export type { SendGridCredentials } from './SendGridProvider';export * from './TwilioProvider';
export type { TwilioCredentials } from './TwilioProvider'; * Entry point for the MemberJunction Component Registry Server
export * from './CloudStorage'export * from './generic/CloudStorageBase'
export * from './providers/AutotagAzureBlob'export * from './generic/AutotagBase';export * from './generic/AutotagBaseEngine'
export * from './generic/AutotagEntity';export * from './generic/AutotagLocalFileSystem'export * from './generic/RSS.types'
export * from './generic/AutotagRSSFeed'export * from './generic/AutotagWebsite'// Core engine
export * from './ChangeDetector';export * from './generated/entity_subclasses';export { gql } from 'graphql-request';
export * from './storage-providers';export * from './data-requirements';
export * from './library-dependency'; * MemberJunction API Server (MJ 3.0 Minimal Architecture)
createMJServer({ resolverPaths }).catch(console.error);export default class AI extends Command {
export * from './artifact-extraction/artifact-extractor';export * from './custom/AIPromptEntityExtended.server';
export * from './custom/sql-parser';export * from './types'
export * from './drivers/AIActionQueue';import { expressMiddleware } from '@apollo/server/express4';
export * from './OAuthCallbackHandler.js';export * from './drivers/AWSFileStorage';
} from './types/validation'; * @fileoverview Service exports for MetadataSync library usage
export { WatchService, WatchOptions, WatchCallbacks, WatchResult } from './WatchService'; * QueryGen package main entry point
} from './babel-config'; * @fileoverview Unified Component Manager for MemberJunction React Runtime
export { ComponentSpec } from '@memberjunction/interactive-component-types'; * @fileoverview Runtime module exports
} from './react-root-manager'; * @fileoverview Core type definitions for the MemberJunction React Runtime.
export { ComponentObject } from '@memberjunction/interactive-component-types'; * @fileoverview Utilities module exports
export * from './component-unwrapper';export { ReactTestHarness, TestHarnessOptions } from './lib/test-harness';
export { NodeFileSystemProvider } from "./NodeFileSystemProvider"; * @memberjunction/scheduling-actions
export * from './extensions/TemplateExtensionBase'; * MemberJunction Testing CLI
import { InvalidConfigurationError, executeFinally, log } from "builder-util"
import { asArray } from "builder-util-runtime"
import { PublishOptions } from "electron-publish"
import { Packager } from "./packager"
import { PackagerOptions } from "./packagerApi"
import { PublishManager } from "./publish/PublishManager"
import { resolveFunction } from "./util/resolve"
export { Arch, archFromString, getArchSuffix } from "builder-util"
export { AppInfo } from "./appInfo"
  AfterExtractContext,
  AfterPackContext,
  BeforePackContext,
  CommonConfiguration,
  Configuration,
  FuseOptionsV1,
  Hook,
  Hooks,
  MetadataDirectories,
  PackContext,
  ToolsetConfig,
} from "./configuration"
  BeforeBuildContext,
  CompressionLevel,
  DEFAULT_TARGET,
  DIR_TARGET,
  Platform,
  SourceRepositoryInfo,
  Target,
  TargetConfigType,
  TargetConfiguration,
  TargetSpecificOptions,
} from "./core"
export { ElectronBrandingOptions, ElectronDownloadOptions, ElectronPlatformName } from "./electron/ElectronFramework"
export { AppXOptions } from "./options/AppXOptions"
export { CommonWindowsInstallerConfiguration } from "./options/CommonWindowsInstallerConfiguration"
export { FileAssociation } from "./options/FileAssociation"
export { AppImageOptions, CommonLinuxOptions, DebOptions, FlatpakOptions, LinuxConfiguration, LinuxDesktopFile, LinuxTargetSpecificOptions } from "./options/linuxOptions"
export { DmgContent, DmgOptions, DmgWindow, MacConfiguration, MacOsTargetName, MasConfiguration } from "./options/macOptions"
export { AuthorMetadata, Metadata, RepositoryInfo } from "./options/metadata"
export { MsiOptions } from "./options/MsiOptions"
export { MsiWrappedOptions } from "./options/MsiWrappedOptions"
export { BackgroundAlignment, BackgroundScaling, PkgBackgroundOptions, PkgOptions } from "./options/pkgOptions"
export { AsarOptions, FileSet, FilesBuildOptions, PlatformSpecificBuildOptions, Protocol, ReleaseInfo } from "./options/PlatformSpecificBuildOptions"
export { PlugDescriptor, SlotDescriptor, SnapOptions } from "./options/SnapOptions"
export { SquirrelWindowsOptions } from "./options/SquirrelWindowsOptions"
export { WindowsAzureSigningConfiguration, WindowsConfiguration, WindowsSigntoolConfiguration } from "./options/winOptions"
export { BuildResult, Packager } from "./packager"
export { ArtifactBuildStarted, ArtifactCreated, PackagerOptions } from "./packagerApi"
export { CommonNsisOptions, CustomNsisBinary, NsisOptions, NsisWebOptions, PortableOptions } from "./targets/nsis/nsisOptions"
export { CancellationToken, ProgressInfo } from "builder-util-runtime"
export { PublishOptions, UploadTask } from "electron-publish"
export { WindowsSignOptions } from "./codeSign/windowsCodeSign"
  CertificateFromStoreInfo,
  CustomWindowsSign,
  CustomWindowsSignTaskConfiguration,
  FileCodeSigningInfo,
  WindowsSignTaskConfiguration,
} from "./codeSign/windowsSignToolManager"
export { ForgeOptions, buildForge } from "./forge-maker"
export { Framework, PrepareApplicationStageDirectoryOptions } from "./Framework"
export { LinuxPackager } from "./linuxPackager"
export { CustomMacSign, CustomMacSignOptions, MacPackager } from "./macPackager"
export { PlatformPackager } from "./platformPackager"
export { PublishManager } from "./publish/PublishManager"
export { WinPackager } from "./winPackager"
const expectedOptions = new Set(["publish", "targets", "mac", "win", "linux", "projectDir", "platformPackagerFactory", "config", "effectiveOptionComputed", "prepackaged"])
export function checkBuildRequestOptions(options: PackagerOptions & PublishOptions) {
  for (const optionName of Object.keys(options)) {
    if (!expectedOptions.has(optionName) && (options as any)[optionName] !== undefined) {
      throw new InvalidConfigurationError(`Unknown option "${optionName}"`)
export function build(options: PackagerOptions & PublishOptions, packager: Packager = new Packager(options)): Promise<Array<string>> {
  checkBuildRequestOptions(options)
  const publishManager = new PublishManager(packager, options)
  const sigIntHandler = () => {
    log.warn("cancelled by SIGINT")
    packager.cancellationToken.cancel()
    publishManager.cancelTasks()
  process.once("SIGINT", sigIntHandler)
  const promise = packager.build().then(async buildResult => {
    const afterAllArtifactBuild = await resolveFunction(packager.appInfo.type, buildResult.configuration.afterAllArtifactBuild, "afterAllArtifactBuild")
    if (afterAllArtifactBuild != null) {
      const newArtifacts = asArray(await Promise.resolve(afterAllArtifactBuild(buildResult)))
      if (newArtifacts.length === 0 || !publishManager.isPublish) {
        return buildResult.artifactPaths
      const publishConfigurations = await publishManager.getGlobalPublishConfigurations()
      if (publishConfigurations == null || publishConfigurations.length === 0) {
      for (const newArtifact of newArtifacts) {
        if (buildResult.artifactPaths.includes(newArtifact)) {
          log.warn({ newArtifact }, "skipping publish of artifact, already published")
        buildResult.artifactPaths.push(newArtifact)
        for (const publishConfiguration of publishConfigurations) {
          await publishManager.scheduleUpload(
            publishConfiguration,
              file: newArtifact,
              arch: null,
            packager.appInfo
  return executeFinally(promise, isErrorOccurred => {
    let promise: Promise<any>
    if (isErrorOccurred) {
      promise = Promise.resolve(null)
      promise = publishManager.awaitTasks()
    return promise.then(() => {
      packager.clearPackagerEventListeners()
      process.removeListener("SIGINT", sigIntHandler)
import { Nullish } from "builder-util-runtime"
import { TmpDir } from "temp-file"
import { NpmNodeModulesCollector } from "./npmNodeModulesCollector"
import { detectPackageManager, getPackageManagerCommand, PM } from "./packageManager"
import { PnpmNodeModulesCollector } from "./pnpmNodeModulesCollector"
import { YarnBerryNodeModulesCollector } from "./yarnBerryNodeModulesCollector"
import { YarnNodeModulesCollector } from "./yarnNodeModulesCollector"
import { BunNodeModulesCollector } from "./bunNodeModulesCollector"
import { Lazy } from "lazy-val"
import { spawn, log, exists, isEmptyOrSpaces } from "builder-util"
import * as fs from "fs-extra"
import * as path from "path"
import { TraversalNodeModulesCollector } from "./traversalNodeModulesCollector"
export { getPackageManagerCommand, PM }
export function getCollectorByPackageManager(pm: PM, rootDir: string, tempDirManager: TmpDir) {
  switch (pm) {
    case PM.PNPM:
      return new PnpmNodeModulesCollector(rootDir, tempDirManager)
    case PM.YARN:
      return new YarnNodeModulesCollector(rootDir, tempDirManager)
    case PM.YARN_BERRY:
      return new YarnBerryNodeModulesCollector(rootDir, tempDirManager)
    case PM.BUN:
      return new BunNodeModulesCollector(rootDir, tempDirManager)
    case PM.NPM:
      return new NpmNodeModulesCollector(rootDir, tempDirManager)
    case PM.TRAVERSAL:
      return new TraversalNodeModulesCollector(rootDir, tempDirManager)
export const determinePackageManagerEnv = ({ projectDir, appDir, workspaceRoot }: { projectDir: string; appDir: string; workspaceRoot: string | Nullish }) =>
  new Lazy(async () => {
    const availableDirs = [workspaceRoot, projectDir, appDir].filter((it): it is string => !isEmptyOrSpaces(it))
    const pm = await detectPackageManager(availableDirs)
    const root = await findWorkspaceRoot(pm.pm, projectDir)
    if (root != null) {
      // re-detect package manager from workspace root, this seems particularly necessary for pnpm workspaces
      const actualPm = await detectPackageManager([root])
      log.info(
        { pm: actualPm.pm, config: actualPm.corepackConfig, resolved: actualPm.resolvedDirectory, projectDir },
        `detected workspace root for project using ${actualPm.detectionMethod}`
        pm: actualPm.pm,
        workspaceRoot: Promise.resolve(actualPm.resolvedDirectory),
      pm: pm.pm,
      workspaceRoot: Promise.resolve(pm.resolvedDirectory),
async function findWorkspaceRoot(pm: PM, cwd: string): Promise<string | undefined> {
  let command: { command: string; args: string[] }
      command = { command: "pnpm", args: ["--workspace-root", "exec", "pwd"] }
      command = { command: "yarn", args: ["workspaces", "list", "--json"] }
    case PM.YARN: {
      command = { command: "yarn", args: ["workspaces", "info", "--silent"] }
      command = { command: "bun", args: ["pm", "ls", "--json"] }
      command = { command: "npm", args: ["prefix", "-w"] }
  const output = await spawn(command.command, command.args, { cwd, stdio: ["ignore", "pipe", "ignore"] })
    .then(async it => {
      const out: string | undefined = it?.trim()
      if (!out) {
        return undefined
      if (pm === PM.YARN) {
        JSON.parse(out) // if JSON valid, workspace detected
        return findNearestPackageJsonWithWorkspacesField(cwd)
      } else if (pm === PM.BUN) {
        const json = JSON.parse(out)
        if (Array.isArray(json) && json.length > 0) {
      } else if (pm === PM.YARN_BERRY) {
        const lines = out
          .split("\n")
          .map(l => l.trim())
          const parsed = JSON.parse(line)
          if (parsed.location != null) {
            const potential = path.resolve(cwd, parsed.location)
            return (await exists(potential)) ? findNearestPackageJsonWithWorkspacesField(potential) : undefined
      return out.length === 0 || out === "undefined" ? undefined : out
    .catch(() => findNearestPackageJsonWithWorkspacesField(cwd))
async function findNearestPackageJsonWithWorkspacesField(dir: string): Promise<string | undefined> {
  let current = dir
    const pkgPath = path.join(current, "package.json")
      const pkg = JSON.parse(await fs.readFile(pkgPath, "utf8"))
      if (pkg.workspaces) {
        log.debug({ path: current }, "identified workspace root")
        return current
    const parent = path.dirname(current)
    if (parent === current) {
    current = parent
export { BlockMap } from "./blockMapApi"
export { CancellationError, CancellationToken } from "./CancellationToken"
export { newError } from "./error"
  configureRequestOptions,
  configureRequestOptionsFromUrl,
  configureRequestUrl,
  createHttpError,
  DigestTransform,
  DownloadOptions,
  HttpExecutor,
  parseJson,
  RequestHeaders,
  safeGetHeader,
  safeStringifyJson,
} from "./httpExecutor"
export { MemoLazy } from "./MemoLazy"
export { ProgressCallbackTransform, ProgressInfo } from "./ProgressCallbackTransform"
  AllPublishOptions,
  BaseS3Options,
  BitbucketOptions,
  CustomPublishOptions,
  GenericServerOptions,
  getS3LikeProviderBaseUrl,
  GithubOptions,
  githubUrl,
  githubTagPrefix,
  GitlabOptions,
  KeygenOptions,
  PublishConfiguration,
  PublishProvider,
  S3Options,
  SnapStoreOptions,
  SpacesOptions,
  GitlabReleaseInfo,
  GitlabReleaseAsset,
} from "./publishOptions"
export { retry } from "./retry"
export { parseDn } from "./rfc2253Parser"
export { BlockMapDataHolder, PackageFileInfo, ReleaseNoteInfo, UpdateFileInfo, UpdateInfo, WindowsUpdateInfo } from "./updateInfo"
export { UUID } from "./uuid"
export { parseXml, XElement } from "./xml"
// nsis
export const CURRENT_APP_INSTALLER_FILE_NAME = "installer.exe"
// nsis-web
export const CURRENT_APP_PACKAGE_FILE_NAME = "package.7z"
export function asArray<T>(v: Nullish | T | Array<T>): Array<T> {
  } else if (Array.isArray(v)) {
    return [v]
export type Nullish = null | undefined
export { getArchSuffix, Arch, archFromString, log } from "builder-util"
export { build, CliOptions, createTargets } from "./builder"
export { publish, publishArtifactsWithOptions } from "./publish"
  MacConfiguration,
  DmgOptions,
  MasConfiguration,
  MacOsTargetName,
  PkgOptions,
  DmgContent,
  DmgWindow,
  PlatformSpecificBuildOptions,
  AsarOptions,
  FileSet,
  LinuxConfiguration,
  DebOptions,
  CommonLinuxOptions,
  LinuxTargetSpecificOptions,
  AppImageOptions,
  ReleaseInfo,
  ElectronBrandingOptions,
  ElectronDownloadOptions,
  SnapOptions,
  CommonWindowsInstallerConfiguration,
  FileAssociation,
  MsiOptions,
  AppXOptions,
  WindowsConfiguration,
  Packager,
  BuildResult,
  PackagerOptions,
  ArtifactCreated,
  ArtifactBuildStarted,
  NsisOptions,
  NsisWebOptions,
  PortableOptions,
  CommonNsisOptions,
  SquirrelWindowsOptions,
  WindowsSignOptions,
  AuthorMetadata,
  RepositoryInfo,
  AppInfo,
  UploadTask,
  PublishManager,
  PublishOptions,
  ProgressInfo,
  MacPackager,
  WinPackager,
  LinuxPackager,
} from "app-builder-lib"
export { buildForge, ForgeOptions } from "app-builder-lib"
export { CancellationToken } from "builder-util-runtime"
import { Arch } from "builder-util"
import { CancellationToken } from "builder-util-runtime"
import { MultiProgress } from "./multiProgress"
export { BitbucketPublisher } from "./bitbucketPublisher"
export { GitHubPublisher } from "./gitHubPublisher"
export { GitlabPublisher } from "./gitlabPublisher"
export { KeygenPublisher } from "./keygenPublisher"
export { S3Publisher } from "./s3/s3Publisher"
export { SpacesPublisher } from "./s3/spacesPublisher"
export { SnapStorePublisher } from "./snapStorePublisher"
export type PublishPolicy = "onTag" | "onTagOrDraft" | "always" | "never"
export { ProgressCallback } from "./progress"
export interface PublishOptions {
  publish?: PublishPolicy | null
export { HttpPublisher } from "./httpPublisher"
export { getCiTag, Publisher } from "./publisher"
export interface PublishContext {
  readonly cancellationToken: CancellationToken
  readonly progress: MultiProgress | null
export interface UploadTask {
  file: string
  fileContent?: Buffer | null
  arch: Arch | null
  safeArtifactName?: string | null
  timeout?: number | null
export { run } from '@oclif/core';
} from '@memberjunction/ai-reranker'; * @fileoverview Agent type implementations for the MemberJunction AI Agent framework.
export * from './models/localEmbedding';
export * from './models/anthropic';export * from './models/azure';
export * from './config';export * from './models/bedrockLLM';
export * from './models/bedrockEmbedding';export * from './generic/BettyBot.types';
export * from './models/cerebras'; * @memberjunction/ai-cohere
export * from './geminiImage';export * from './models/groq'; import { AvatarInfo, AvatarVideoParams, BaseVideoGenerator, VideoResult, ErrorAnalyzer } from "@memberjunction/ai";
export * from './models/lm-studio'; export * from './models/mistralEmbedding';export * from './models/ollama-llm';
export * from './models/openAIImage';export * from './models/openRouter'; export * from './provider';export * from "./models/PineconeDatabase";export * from './models/vertexLLM';
export * from './models/x.ai';  * @module @memberjunction/ai-vectors-memory
} from './models/SimpleVectorService';export * from './generic/VectorCore.types'; export * from './generic/record';
export * from './generic/query.types';export * from './generic/vectorSyncBase';
export * from './duplicateRecordDetector';export * from './generic/vectorSync.types';
export * from './config.js';export * from './generic/errorAnalyzer';export * from './prompt.types';
export * from './types/ExampleMatchResult';  * @fileoverview MCP Client package for MemberJunction
export * from './Engine';
export * from './generic/types'; * @memberjunction/ai-reranker
export * from './generated/action_subclasses';
export * from './delete-record.action'; * File Storage Actions
export * from './json-param-helper';// Would appreciate feedback on (a) the approach to how we package this stuff and (b) the approach to params noted above.export * from './ActionEngine-Base';
export * from './EntityActionEntity-Extended';// Export all accounting actions
// export * from './common/validate-journal-entry.action'; * CRM Integration Actions
export * from './providers/hubspot'; * HubSpot CRM Provider
export * from './actions'; * HubSpot CRM Actions
export * from './get-upcoming-tasks.action';// Base Classes
export * from './providers/google-forms';export * from './base-form-builder.action';
export * from './providers/learnworlds';// Export LearnWorlds base action
export * from './get-quiz-results.action';export * from './base/base-social.action';
export * from './providers/youtube';export * from './buffer-base.action';
export * from './search-posts.action';export * from './facebook-base.action';
export * from './actions/search-posts.action';// Instagram Provider
export * from './search-videos.action'; * Twitter/X Provider for MemberJunction Social Media Actions
export * from './search-tweets.action';export * from './youtube-base.action';
export * from './generic/content-autotag-and-vectorize.action';export * from './generic/BaseAction';
export * from './entity-actions/EntityActionInvocationTypes';export * from "./scheduler";import express from 'express';
export * from './components/charts/performance-heatmap.component';export * from './api-keys-resource.component';
export * from './components/text-import-dialog.component';// Main dashboard component
export * from './components';export * from './labels-resource.component';
 * Entry point for the MemberJunction Component Registry Server
export { gql } from 'graphql-request';
export * from './storage-providers'; * MemberJunction API Server (MJ 3.0 Minimal Architecture)
createMJServer({ resolverPaths }).catch(console.error);export default class AI extends Command {
 * QueryGen package main entry point
} from './babel-config'; * @fileoverview Unified Component Manager for MemberJunction React Runtime
export { ComponentSpec } from '@memberjunction/interactive-component-types'; * @fileoverview Runtime module exports
} from './react-root-manager'; * @fileoverview Core type definitions for the MemberJunction React Runtime.
export { ComponentObject } from '@memberjunction/interactive-component-types'; * @fileoverview Utilities module exports
export * from './component-unwrapper';export { ReactTestHarness, TestHarnessOptions } from './lib/test-harness';
export * from './runCodeGen'// PUBLIC API SURFACE AREA
export * from './CredentialUtils';export * from './client';export * from './entity-communications'; * @memberjunction/notifications
export type { MSGraphCredentials } from './MSGraphProvider';export * from './GmailProvider';
export type { GmailCredentials } from './GmailProvider';export * from './SendGridProvider';
export type { SendGridCredentials } from './SendGridProvider';export * from './TwilioProvider';
export type { TwilioCredentials } from './TwilioProvider';export * from './CloudStorage'export * from './generic/CloudStorageBase'
export * from './providers/AutotagAzureBlob'export * from './generic/AutotagBase';export * from './generic/AutotagBaseEngine'
export * from './generic/AutotagEntity';export * from './generic/AutotagLocalFileSystem'export * from './generic/RSS.types'
export * from './generic/AutotagRSSFeed'export * from './generic/AutotagWebsite'// Core engine
export * from './ChangeDetector';export * from './generated/entity_subclasses';export * from './data-requirements';
export * from './library-dependency';export * from './artifact-extraction/artifact-extractor';export * from './custom/AIPromptEntityExtended.server';
export * from './custom/sql-parser';export * from './types'
export * from './drivers/AIActionQueue';import { expressMiddleware } from '@apollo/server/express4';
export * from './OAuthCallbackHandler.js';export * from './drivers/AWSFileStorage';
} from './types/validation'; * @fileoverview Service exports for MetadataSync library usage
export { WatchService, WatchOptions, WatchCallbacks, WatchResult } from './WatchService';export { NodeFileSystemProvider } from "./NodeFileSystemProvider"; * @memberjunction/scheduling-actions
export * from './extensions/TemplateExtensionBase'; * MemberJunction Testing CLI
