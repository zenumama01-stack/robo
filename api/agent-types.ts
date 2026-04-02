 * @fileoverview Type definitions for AI Agent execution results.
 * This module contains type definitions for agent execution results that are
 * shared between server and client code. These types provide strongly-typed
 * interfaces for agent execution results and the complete execution history.
 * @module @memberjunction/aiengine
 * @since 2.50.0
import { MJAIAgentTypeEntity,  } from '@memberjunction/core-entities';
import {  } from '@memberjunction/core-entities';
import { UserInfo } from '@memberjunction/core';
import { AgentPayloadChangeRequest } from './agent-payload-change-request';
import { AIAPIKey } from '@memberjunction/ai';
import { AgentResponseForm } from './response-forms';
import { ActionableCommand, AutomaticCommand } from './ui-commands';
import { AIAgentRunEntityExtended } from './AIAgentRunExtended';
import { AIAgentEntityExtended } from './AIAgentExtended';
import { AIPromptEntityExtended } from './AIPromptExtended';
import { MediaModality } from './prompt.types';
 * Value type for secondary scope dimensions.
 * Supports strings, numbers, booleans, and string arrays (for multi-valued dimensions).
export type SecondaryScopeValue = string | number | boolean | string[];
 * Configuration for secondary scope dimensions on an AI Agent.
 * Defines what secondary scope dimensions are valid for an agent and how they
 * should behave for memory retrieval and storage. This configuration is stored
 * in the `SecondaryScopeConfig` JSON field on the AIAgent entity.
 * @example Customer Service App (entity-backed dimensions)
 * ```json
 *     "dimensions": [
 *         {"name": "ContactID", "entityId": "uuid-for-contacts", "inheritanceMode": "cascading"},
 *         {"name": "TeamID", "entityId": "uuid-for-teams", "inheritanceMode": "strict"}
 *     ],
 *     "allowSecondaryOnly": false
 * @example Analytics App (arbitrary value dimensions)
 *         {"name": "Region", "inheritanceMode": "cascading"},
 *         {"name": "DealStage", "inheritanceMode": "strict"}
 *     "allowSecondaryOnly": true
export interface SecondaryScopeConfig {
     * Array of dimension definitions.
     * Each dimension defines a scope key that can be provided at runtime via
     * `ExecuteAgentParams.SecondaryScopes`.
    dimensions: SecondaryDimension[];
     * Default inheritance mode for dimensions that don't specify one.
     * - 'cascading': Notes without a dimension match queries with that dimension (broader retrieval)
     * - 'strict': Notes must exactly match the dimension value or be absent
     * @default 'cascading'
    defaultInheritanceMode?: 'cascading' | 'strict';
     * Whether to allow secondary-only scoping (no primary scope required).
     * When true, the agent can function with only secondary dimensions provided
     * in `ExecuteAgentParams.SecondaryScopes` without requiring `PrimaryScopeRecordID`.
     * @default false
    allowSecondaryOnly?: boolean;
     * Whether to validate runtime scope values against this config.
     * When true, extra dimensions not defined in `dimensions` array will cause validation errors.
     * When false, extra dimensions are accepted and stored but may not be used in filtering.
    strictValidation?: boolean;
 * Definition of a single secondary scope dimension.
 * Secondary dimensions allow fine-grained scoping beyond the primary scope level.
 * Each dimension can be configured for validation, inheritance behavior, and defaults.
export interface SecondaryDimension {
     * Dimension name/key (e.g., "ContactID", "TeamName", "Region").
     * This is the key used in `ExecuteAgentParams.SecondaryScopes` and stored
     * in the `SecondaryScopes` JSON field on notes/examples/runs.
     * Optional MemberJunction Entity ID for validation.
     * When provided, runtime values can be validated as existing records in that entity.
     * When null/omitted, the dimension accepts any string value (useful for non-entity
     * dimensions like "Region", "DealStage", "ProductLine", etc.).
    entityId?: string | null;
     * Whether this dimension is required at runtime.
     * When true, `ExecuteAgentParams.SecondaryScopes` must include this dimension
     * or have a `defaultValue` defined.
    required?: boolean;
     * Inheritance mode for this specific dimension, overrides `defaultInheritanceMode`.
     * - 'cascading': Notes without this dimension match queries with it (broader retrieval).
     *   For example, if querying with ContactID=123, notes without any ContactID will match.
     * - 'strict': Notes must exactly match the provided dimension value.
     *   Notes without the dimension do NOT match queries that include it.
    inheritanceMode?: 'cascading' | 'strict';
     * Default value if not provided at runtime.
     * Only used when `required=false`. If the dimension is not in the runtime scope
     * and a defaultValue is set, this value will be used.
    defaultValue?: string | null;
     * Human-readable description of this dimension for documentation.
// Import loop operation types from their dedicated modules
// These are in separate files so they can be @include'd in prompt templates
// Exported directly from index.ts, not re-exported here
import type { ForEachOperation } from './foreach-operation';
import type { WhileOperation } from './while-operation';
 * Represents a media output that an agent has explicitly promoted to its outputs.
 * This is the interface used in ExecuteAgentResult.mediaOutputs.
 * Media can come from two sources:
 * 1. Promoted from a prompt run (has promptRunMediaId)
 * 2. Generated directly by agent code (has data or url)
export interface MediaOutput {
    /** Reference to source AIPromptRunMedia (if promoted from prompt execution) */
    promptRunMediaId?: string;
    /** The modality type */
    modality: MediaModality;
    /** MIME type of the media (e.g., 'image/png', 'audio/mp3') */
    /** Base64 encoded data (only if NOT from prompt run) */
    data?: string;
    /** URL if available (some providers return URLs) */
    /** Width in pixels (for images/video) */
    /** Height in pixels (for images/video) */
    /** Duration in seconds (for audio/video) */
    durationSeconds?: number;
    /** Agent-provided label for UI display */
    label?: string;
    /** Provider-specific metadata */
     * Placeholder reference ID for the ${media:xxx} pattern.
     * Used to look up media when resolving placeholders in agent output.
    refId?: string;
     * Controls whether this media should be persisted to the database.
     * Default behavior (undefined or true): media is persisted to AIAgentRunMedia and ConversationDetailAttachment.
     * Set to false for intercepted/working media that shouldn't be saved (e.g., generated but not used in output).
    persist?: boolean;
     * Agent notes describing what this media represents.
     * Used for internal tracking, debugging, and can be persisted for audit purposes.
 * Represents a single action to be executed.
export type AgentAction = {
    /** Name of the action */
    /** Parameters to pass to the action */
    /** Mapping of action outputs to payload fields */
    outputMapping?: string;   
 * Represents a sub-agent invocation request.
 * @template TContext - Type of the context object passed to the sub-agent.
 *                      This allows for type-safe context propagation from parent to sub-agent.
 *                      Defaults to any for backward compatibility.
 * // Define a typed context
 *   apiEndpoint: string;
 *   authToken: string;
 * // Use in sub-agent request
 * const subAgentRequest: AgentSubAgentRequest<MyContext> = {
 *   id: 'sub-agent-uuid',
 *   name: 'DataProcessorAgent',
 *   message: 'Process the uploaded data',
 *   terminateAfter: false,
 *     apiEndpoint: 'https://api.example.com',
 *     authToken: 'bearer-token'
export type AgentSubAgentRequest<TContext = any> = {
    /** Name of the sub-agent */
    /** Context and instructions for the sub-agent */
    /** Whether to terminate the parent agent after sub-agent completes */
    /** Optional template parameters for sub-agent invocation */
    templateParameters?: Record<string, string>;
     * Context data passed to the sub-agent by the parent agent.
     * This context flows through the entire execution hierarchy,
     * allowing sub-agents to access runtime-specific configuration,
     * environment settings, and shared state from their parent agents.
     * Optional because AI determines sub-agent invocation, context comes from execution params.
    context?: TContext;
 * Represents the next step determination from an agent type.
 * Agent types analyze the output of prompt execution and determine what should
 * happen next in the agent workflow. This type encapsulates that decision along
 * with any necessary context for the determined action.
 * @template P - Type of the payload value, allowing flexibility in the type of data returned
 * @template TContext - Type of the context object used for sub-agent requests.
 *                      This ensures type safety when passing context to sub-agents.
export type BaseAgentNextStep<P = any, TContext = any> = {
    /** Whether to terminate the agent execution after this step */
    terminate: boolean;
     * The determined next step:
     * - 'success': The agent has completed its task successfully
     * - 'failed': The agent has failed to complete its task
     * - 'retry': The agent should re-run to either:
     *   a) Process new results from completed actions or sub-agents
     *   b) Retry after a failure condition
     *   c) Continue processing with updated context
     *   d) Process after expanding a compacted message (when messageIndex is set)
     * - 'sub-agent': The agent should spawn a sub-agent to handle a specific task
     * - 'actions': The agent should perform one or more actions using the Actions framework
     * - 'chat': The agent needs to communicate with the user before proceeding
     * Note: To expand a compacted message, set step to 'Retry', set messageIndex to the message to expand,
     * and optionally set expandReason to explain why expansion is needed. The framework will expand the message
     * and then continue with the retry.
    step: AIAgentRunEntityExtended['FinalStep']
    /** Result from the prior step, useful for retry or sub-agent context */
    priorStepResult?: any;
     * Payload change request from the agent.
     * Framework will apply these changes to the previous payload to create the new state.
     * This approach ensures safe state mutations and prevents data loss from LLM truncation.
     * The payload that was passed into this step execution.
     * Useful for debugging and understanding what state the agent was working with.
     * @deprecated Use payloadChangeRequest instead for state mutations
    previousPayload?: P;
     * This represents the new payload after the step was executed after the payloadChangeRequest is applied.
    newPayload?: P;
    /** Error message when step is 'failed' */
    /** Reason for retry when step is 'retry' (e.g., "Processing action results", "Handling error condition") */
    retryReason?: string;
    /** Instructions for the retry attempt, including any new context or results */
    retryInstructions?: string;
    /** Sub-agent details when step is 'sub-agent' */
    subAgent?: AgentSubAgentRequest<TContext>;
    /** Array of actions to execute when step is 'actions' */
    actions?: AgentAction[];
    /** Message to send to user when step is 'chat' */
     * When present, the UI will render appropriate input controls based on question types.
     * Use for collecting information from users during agent execution.
     * Typically used after completing work to provide easy navigation to created/modified resources.
    /** Index of the message to expand when step is 'expand-message' */
    messageIndex?: number;
    /** Reason for expanding the message when step is 'expand-message' */
    expandReason?: string;
    /** Optional, reasoning information from the agent */
    /** Optional confidence level in the decision (0.0 to 1.0) */
    /** ForEach operation details when step is 'ForEach' (v2.112+) */
    /** While operation details when step is 'While' (v2.112+) */
     * Media outputs to promote to the agent's final outputs.
     * When set, these media items will be added to the agent's mediaOutputs collection
     * and stored in AIAgentRunMedia.
    promoteMediaOutputs?: MediaOutput[];
 * Result returned from executing an AI Agent with comprehensive execution history.
 * This result structure provides complete visibility into the agent's execution flow,
 * including all prompts executed, actions taken, sub-agents invoked, and decisions made.
 * The agentRun property contains the full execution history with all steps.
 * @template P - Generic type parameter for payload value, allowing flexibility in the type of data returned by the agent
export type ExecuteAgentResult<P = any> = {
    /** Whether the agent execution was successful */
    /** Optional payload returned by the agent */
     * The agent run entity with full execution history.
     * - Use agentRun.ErrorMessage for error details
     * - Use agentRun.Status === 'Cancelled' to check if cancelled
     * - Use agentRun.CancellationReason for cancellation reason
     * - Use agentRun.Steps for the execution step history
    agentRun: AIAgentRunEntityExtended;
     * The artifact type ID for the returned payload.
     * This identifies what type of artifact the payload represents (e.g., JSON, Markdown, HTML).
     * Used by UI to determine how to render the payload and which extract rules to apply.
     * If not specified, falls back to the agent's default artifact type configuration.
    payloadArtifactTypeID?: string;
     * Populated from the agent's final step.
     * Optional memory context that was injected into the agent execution.
     * Includes the notes and examples that were retrieved and used for context.
    memoryContext?: {
        notes: any[]; // MJAIAgentNoteEntity[] - using any to avoid circular dependency
        examples: any[]; // MJAIAgentExampleEntity[] - using any to avoid circular dependency
     * Multi-modal outputs generated by the agent.
     * Contains media that the agent explicitly promoted to its outputs.
     * This flows to ConversationDetailAttachment for UI display.
     * Media items with `refId` are used for placeholder resolution (${media:xxx}).
     * Media items with `persist: false` are excluded from database persistence.
     * Sub-agents return their mediaOutputs to parents for bubbling up.
    mediaOutputs?: MediaOutput[];
 * The decision made about what to do next after a step completes.
 * This captures not just what was decided, but why it was decided and what
 * will happen next, providing transparency into the agent's decision-making.
export type NextStepDecision = {
    /** The next step type that was decided */
    decision: BaseAgentNextStep['step'];
    /** Human-readable explanation of why this decision was made */
    /** Details about what will be executed next (if not terminating) */
    nextStepDetails?: NextStepDetails;
 * Details about what will be executed in the next step.
 * Union type that provides specific information based on the type of next step,
 * enabling proper preparation and execution of the subsequent operation.
export type NextStepDetails <P = any> = 
    | { type: 'Prompt'; promptId: string; promptName: string; payload?: P }
    | { type: 'Actions'; actions: AgentAction[]; payload?: P }
    | { type: 'Sub-Agent'; subAgent: AgentSubAgentRequest; payload?: P }
    | { type: 'Retry'; retryReason: string; retryInstructions: string; payload?: P }
    | { type: 'Chat'; message: string; payload?: P }
    | { type: 'Complete'; payload?: P };
 * Callback function type for agent execution progress updates
export type AgentExecutionProgressCallback = (progress: {
    /** Current step in the agent execution process */
    step: 'initialization' | 'validation' | 'prompt_execution' | 'action_execution' | 'subagent_execution' | 'decision_processing' | 'finalization';
    /** @deprecated Progress percentage (0-100) - Use metadata.stepCount instead for actual progress tracking */
    percentage?: number;
    /** Human-readable status message */
    /** Additional metadata about the current step. Use metadata.stepCount for accurate step tracking. */
    /** When this progress message should be displayed */
    displayMode?: 'live' | 'historical' | 'both';
}) => void;
 * Callback function type for streaming content updates during agent execution
export type AgentExecutionStreamingCallback = (chunk: {
    /** The content chunk received */
    /** Whether this is the final chunk */
    isComplete: boolean;
    /** Which step is producing this content */
    stepType?: 'prompt' | 'action' | 'subagent' | 'chat';
    /** Specific step entity ID producing this content */
    stepEntityId?: string;
    /** Model name producing this content (for prompt steps) */
 * Parameters required to execute an AI Agent.
 * @template TContext - Type of the context object passed through agent and action execution.
 *                      This allows for type-safe context propagation throughout the execution hierarchy.
 * @template P - Type of the payload passed to the agent execution
 * @template TAgentTypeParams - Type of agent-type-specific execution parameters.
 *                              Flow agents use FlowAgentExecuteParams, Loop agents could define their own.
 *                              Defaults to unknown for backward compatibility.
 * interface MyAgentContext {
 *   userPreferences: { language: string; timezone: string };
 *   sessionId: string;
 * // Use with type safety
 * const params: ExecuteAgentParams<MyAgentContext> = {
 *     userPreferences: { language: 'en', timezone: 'UTC' },
 *     sessionId: 'abc123'
 * // Flow agent with type-specific params
 * const flowParams: ExecuteAgentParams<any, any, FlowAgentExecuteParams> = {
 *     startAtStep: someStepEntity,  // Start at a specific step
 *     skipSteps: [stepToSkip]       // Skip certain steps
export type ExecuteAgentParams<TContext = any, P = any, TAgentTypeParams = unknown> = {
    /** The agent entity to execute, containing all metadata and configuration */
    agent: AIAgentEntityExtended;
    /** Array of chat messages representing the conversation history */
    conversationMessages: ChatMessage[];
    /** Optional user context for permission checking and personalization */
    contextUser?: UserInfo;
    /** Optional user ID for scoping context memory (notes/examples). If not provided, uses contextUser.ID */
    /** Optional company ID for scoping context memory (notes/examples) */
     * Primary scope entity name (e.g., 'Organizations', 'Skip Tenants').
     * Resolved to PrimaryScopeEntityID on the AIAgentRun record.
     * Used by external applications for multi-tenant memory scoping.
     * Not used by MJ's own chat infrastructure.
    PrimaryScopeEntityName?: string;
     * Primary scope record ID — the actual record ID within the primary entity.
     * Stored as an indexed column on AIAgentRun/AIAgentNote for fast filtering.
    PrimaryScopeRecordID?: string;
     * Arbitrary key/value dimensions for external-app scoping.
     * Stored as JSON in the SecondaryScopes column on AIAgentRun/AIAgentNote.
     * Used by external applications (Skip, Izzy, etc.) to segment agent memory
     * by custom dimensions. MJ's own chat infrastructure does not use this.
     * params.SecondaryScopes = {
     *     ContactID: 'contact-456',
     *     TeamID: 'team-alpha',
     *     Region: 'EMEA'
    SecondaryScopes?: Record<string, SecondaryScopeValue>;
    /** Optional cancellation token to abort the agent execution */
    /** Optional callback for receiving execution progress updates */
    onProgress?: AgentExecutionProgressCallback;
    /** Optional callback for receiving streaming content updates */
    onStreaming?: AgentExecutionStreamingCallback;
    /** Optional parent agent hierarchy for sub-agent execution */
    parentAgentHierarchy?: string[];
    /** Optional parent depth for sub-agent execution */
    parentDepth?: number;
     * Optional parent step counts from root to immediate parent agent.
     * @internal - Managed automatically by agent execution framework
    parentStepCounts?: number[];
    /** Optional parent agent run entity for nested sub-agent execution */
    parentRun?: AIAgentRunEntityExtended;
    /** Optional data for template rendering and prompt execution, passed to the agent's prompt as well as all sub-agents */
    data?: Record<string, any>;
    /** Optional payload to pass to the agent execution, type depends on agent implementation. Payload is the ongoing dynamic state of the agent run. */
     * Optional additional context data to pass to the agent execution.
     * This context is propagated to all sub-agents and actions throughout 
     * the execution hierarchy. Use this for runtime-specific data such as:
     * - Environment-specific configuration (API endpoints, feature flags)
     * - User-specific settings or preferences
     * - Session-specific data (request IDs, correlation IDs)
     * - External service credentials or connection information
     * Note: Avoid including sensitive data like passwords or API keys 
     * unless absolutely necessary, as context may be passed to multiple 
     * agents and actions.
     * Optional runtime override for agent execution.
     * When specified, these values take precedence over all other model selection methods.
     * Currently supports model and vendor overrides, but can be extended in the future.
     * Model selection precedence (highest to lowest):
     * 1. Runtime override (this parameter)
     * 2. Agent's ModelSelectionMode configuration:
     *    - If "Agent": Uses the agent's specific prompt model configuration
     *    - If "Agent Type" (default): Uses the agent type's system prompt model configuration
     * 3. Default model selection based on prompt configuration
     * This override is passed to all prompt executions within the agent, allowing
     * consistent model usage throughout the agent's execution hierarchy.
    override?: {
        modelId?: string;
        vendorId?: string;
     * Optional flag to enable verbose logging during agent execution.
     * When true, detailed information about agent decision-making, action selection,
     * sub-agent invocations, and execution flow will be logged.
     * Can also be controlled via MJ_VERBOSE environment variable.
     * Optional array of API keys to use for AI provider access during agent execution.
     * When provided, these keys will be used instead of the default keys configured
     * in the system. This allows for runtime-specific API key usage, useful for:
     * - Multi-tenant scenarios where different users have different API keys
     * - Testing with different API key configurations
     * - Isolating API usage by application or user
     * Each key should specify the driverClass (e.g., 'OpenAILLM', 'AnthropicLLM')
     * and the corresponding apiKey value.
    apiKeys?: AIAPIKey[];
     * Optional ID of the last run in a run chain.
     * When provided, this links the new run to a previous run, allowing
     * agents to maintain context across multiple interactions.
     * Different from parentRun which is for sub-agent hierarchy.
    lastRunId?: string;
     * Optional conversation detail ID to associate with this agent execution.
     * When provided, this value is stored in the ConversationDetailID column within
     * the to be created AIAgentRun record. This allows for linking the agent run 
     * to a specific conversation detail for tracking and reporting purposes.
     * Optional flag to automatically populate the payload from the last run.
     * When true and lastRunId is provided, the framework will:
     * 1. Load the last run's FinalPayload
     * 2. Set it as the StartingPayload for the new run
     * 3. Use it as the initial payload if no payload is explicitly provided
     * This helps maintain state across run chains and reduces
     * bandwidth by avoiding passing large payloads back and forth.
    autoPopulateLastRunPayload?: boolean;
     * Optional AI Configuration ID to use for this agent execution.
     * When provided, this configuration will be passed to all prompts executed
     * by this agent and its sub-agents, enabling environment-specific model
     * selection (e.g., Prod vs Dev configurations).
     * The configuration ID filters which AI models are available for prompt
     * execution and can provide configuration parameters for dynamic behavior.
     * Optional callback fired immediately after the AgentRun record is created and saved.
     * Provides the AgentRun ID for immediate tracking/monitoring purposes.
     * This callback is useful for:
     * - Linking the AgentRun to parent records (e.g., AIAgentRunStep.TargetLogID for sub-agents)
     * - Real-time monitoring and tracking
     * - Early logging and debugging
     * The callback is invoked after the AgentRun is successfully saved but before
     * the actual agent execution begins. If the callback throws an error, it will
     * be logged but won't fail the agent execution.
     * @param agentRunId - The ID of the newly created AIAgentRun record
     * const params: ExecuteAgentParams = {
     *   onAgentRunCreated: async (agentRunId) => {
     *     console.log(`Agent run started: ${agentRunId}`);
     *     // Update parent records, send monitoring events, etc.
    onAgentRunCreated?: (agentRunId: string) => void | Promise<void>;
     * Optional effort level for all prompt executions in this agent run (1-100).
     * Higher values request more thorough reasoning and analysis from AI models.
     * This effort level takes precedence over the agent's DefaultPromptEffortLevel
     * and individual prompt EffortLevel settings for all prompts executed during
     * this agent run.
     * Each provider maps the 1-100 scale to their specific effort parameters:
     * - OpenAI: Maps to reasoning_effort (1-33=low, 34-66=medium, 67-100=high)
     * - Anthropic: Maps to thinking mode with token budgets
     * - Groq: Maps to reasoning_effort parameter (experimental)
     * - Gemini: Controls reasoning mode intensity
     * This setting is inherited by all sub-agents unless they explicitly override it.
     * Precedence hierarchy (highest to lowest priority):
     * 1. This effortLevel parameter (runtime override - highest priority)
     * 2. Agent's DefaultPromptEffortLevel (agent default)
     * 3. Prompt's EffortLevel property (prompt default)
     * 4. No effort level (provider default behavior - lowest priority)
     *   effortLevel: 85, // High effort for thorough analysis across all prompts
     *   contextUser: user
    effortLevel?: number;
     * Optional runtime override for message expiration behavior.
     * When specified, these values take precedence over the AIAgentAction configuration
     * for all action results in this agent run. Useful for testing, debugging, or
     * implementing custom expiration strategies.
     *   messageExpirationOverride: {
     *     expirationTurns: 2,
     *     expirationMode: 'Compact',
     *     compactMode: 'First N Chars',
     *     compactLength: 500,
     *     preserveOriginalContent: true
    messageExpirationOverride?: MessageExpirationOverride;
     * Optional callback for message lifecycle events.
     * Called when messages are expired, compacted, removed, or expanded during agent execution.
     * Useful for monitoring, debugging, and tracking token savings.
     *   onMessageLifecycle: (event) => {
     *     console.log(`[Turn ${event.turn}] ${event.type}: ${event.reason}`);
     *     if (event.tokensSaved) {
     *       console.log(`  Tokens saved: ${event.tokensSaved}`);
    onMessageLifecycle?: MessageLifecycleCallback;
     * Optional flag to disable data preloading from AIAgentDataSource metadata.
     * When true, the agent will not automatically preload data sources even if
     * they are configured in the database. This is useful for:
     * - Performance optimization when preloaded data is not needed
     * - Testing scenarios where you want to control data explicitly
     * - Cases where the caller provides all necessary data manually
     * Default: false (data preloading is enabled)
     * Note: Caller-provided data in the data parameter always takes precedence
     * over preloaded data, even when preloading is enabled.
     *   disableDataPreloading: true,  // Skip automatic data preloading
     *   data: { CUSTOM_DATA: myData }  // Use only caller-provided data
    disableDataPreloading?: boolean;
     * Optional absolute maximum number of iterations (steps) for the agent run.
     * This provides a hard limit safety net to prevent infinite loops in case of
     * configuration errors or unexpected agent behavior. This value overrides any
     * agent-level MaxIterationsPerRun setting if it is lower.
     * If not specified, defaults to 5000 iterations as a safety measure.
     * Use a higher value only if you have a legitimate need for very long-running agents.
     * This is different from MaxIterationsPerRun (agent metadata guardrail):
     * - MaxIterationsPerRun: Configurable per-agent business rule
     * - absoluteMaxIterations: System-wide safety limit (default: 5000)
     * @default 5000
     *   absoluteMaxIterations: 10000,  // Allow more iterations for this specific run
    absoluteMaxIterations?: number;
     * Optional test run ID to associate with this agent execution.
     * When provided, this value is stored in the TestRunID column within
     * the AIAgentRun record, linking the agent run to a specific test run
     * from the MemberJunction testing framework for tracking and reporting purposes.
     *   testRunId: '12345678-1234-1234-1234-123456789012',  // Link to test run
     * Optional flag to convert UI markup (@{...} syntax) in user messages to plain text
     * before passing to the agent.
     * When true (default), special UI syntax like mentions (@{_mode:"mention",...}) and
     * form responses (@{_mode:"form",...}) are converted to human-readable plain text:
     * - Mentions: "@Agent Name" or "@User Name"
     * - Forms: "Field1: Value1, Field2: Value2"
     * When false, the raw @{...} JSON is preserved in conversation history.
     * This prevents agents from:
     * - Getting confused by UI-specific JSON syntax
     * - Wasting tokens on markup that doesn't provide useful context
     * - Trying to replicate or interpret UI-specific formatting
     * // Default behavior - convert to plain text
     *   conversationMessages: messages,  // "@{_mode:"mention",...}" becomes "@Agent Name"
     * // Preserve raw markup (not recommended)
     *   convertUIMarkupToPlainText: false,  // Keep raw @{...} syntax
    convertUIMarkupToPlainText?: boolean;
     * Optional runtime modifications to the agent's available actions.
     * Action changes allow dynamic customization of which actions are available
     * to agents at runtime, without modifying database configuration. This is
     * particularly useful for:
     * - Multi-tenant scenarios where different executions need different integrations
     * - Security restrictions where sub-agents should have limited action access
     * - Testing scenarios with controlled action availability
     * Changes are applied in order. For each agent in the hierarchy:
     * 1. Start with the agent's configured actions (from AIAgentAction table)
     * 2. Apply each ActionChange that matches the agent's scope
     * 3. The resulting action set is what the agent sees and can invoke
     * Changes are propagated to sub-agents based on scope:
     * - 'root': Not propagated (only applies to root)
     * - 'all-subagents': Propagated as 'global' to sub-agents
     * - 'specific': Propagated as-is, each agent checks if it's in agentIds
     * // Add LMS and CRM integrations for a specific tenant
     *   actionChanges: [
     *     {
     *       scope: 'global',
     *       mode: 'add',
     *       actionIds: ['lms-query-action-id', 'crm-search-action-id']
     * // Remove dangerous actions from sub-agents
     *       scope: 'all-subagents',
     *       mode: 'remove',
     *       actionIds: ['delete-record-action-id', 'execute-sql-action-id']
     * // Different actions for specific sub-agent
     *     { scope: 'global', mode: 'add', actionIds: ['common-action-id'] },
     *       scope: 'specific',
     *       actionIds: ['special-data-action-id'],
     *       agentIds: ['data-gatherer-sub-agent-id']
    actionChanges?: ActionChange[];
     * Optional agent-type-specific execution parameters.
     * Different agent types can define their own parameter interfaces for
     * type-specific configuration that doesn't belong in the general ExecuteAgentParams.
     * - Flow agents: FlowAgentExecuteParams with startAtStep, skipSteps
     * - Loop agents: Could define LoopAgentExecuteParams with custom iteration controls
     * The type is determined by the TAgentTypeParams generic parameter.
     * When using a specific agent type, import and use its params interface for type safety.
     *     startAtStep: AIEngine.Instance.GetAgentSteps(agentId).find(s => s.Name === 'Approval'),
     *     skipSteps: [debugStep, testStep]
    agentTypeParams?: TAgentTypeParams;
 * Context data provided to agent prompts during execution.
 * This data structure is passed to the prompt templates and contains information
 * about the agent, its sub-agents, and available actions. The sub-agent and action
 * details are JSON stringified to work with the template engine.
export type AgentContextData = {
    /** The name of the agent being executed */
    agentName: string | null;
    /** Description of the agent's purpose and capabilities */
    agentDescription: string | null;
    /** Optional parent agent name for sub-agent context */
    parentAgentName?: string | null;
    /** Number of sub-agents available to this agent */
    subAgentCount: number;
    /** JSON stringified array of AIAgentEntityExtended objects representing sub-agents */
    subAgentDetails: string;
    /** Number of actions available to this agent */
    actionCount: number;
    /** JSON stringified array of MJActionEntity objects representing available actions */
    actionDetails: string;
 * Configuration loaded for agent execution.
export type AgentConfiguration = {
    /** Whether configuration was loaded successfully */
    /** Error message if configuration failed */
    /** The loaded agent type entity */
    /** The loaded system prompt entity */
    systemPrompt?: AIPromptEntityExtended;
    /** The loaded child prompt entity */
    childPrompt?: AIPromptEntityExtended;
 * Typed metadata for agent conversation messages.
 * Extends ChatMessage<M> to provide agent-specific metadata for message lifecycle management.
export type AgentChatMessageMetadata = {
    /** Turn number when this message was added to the conversation */
    turnAdded?: number;
    /** Number of turns after which this message expires */
    expirationTurns?: number;
    /** Mode for handling expired messages */
    expirationMode?: 'None' | 'Remove' | 'Compact';
    /** Mode for compacting expired messages */
    compactMode?: 'First N Chars' | 'AI Summary';
    /** Number of characters to keep when using 'First N Chars' mode */
    compactLength?: number;
    /** Prompt ID to use for AI Summary compaction */
    compactPromptId?: string;
    /** Whether this message has been compacted */
    wasCompacted?: boolean;
    /** Original content before compaction (for expansion) */
    originalContent?: ChatMessage['content'];
    /** Original length in characters before compaction */
    originalLength?: number;
    /** Number of tokens saved by compaction */
    tokensSaved?: number;
    /** Whether this message can be expanded back to original */
    canExpand?: boolean;
    /** Whether this message has expired */
    isExpired?: boolean;
    /** Type of message (for logging/debugging) */
    messageType?: 'action-result' | 'sub-agent-result' | 'chat' | 'system' | 'user';
 * Agent conversation message with typed metadata.
export type AgentChatMessage = ChatMessage<AgentChatMessageMetadata>;
 * Event types for message lifecycle callbacks.
export type MessageLifecycleEventType = 'message-expired' | 'message-compacted' | 'message-removed' | 'message-expanded';
 * Event data for message lifecycle callbacks.
export type MessageLifecycleEvent = {
    /** Type of lifecycle event */
    type: MessageLifecycleEventType;
    /** Turn number when the event occurred */
    turn: number;
    /** Index of the message in the conversation array */
    messageIndex: number;
    /** The message that was affected */
    /** Human-readable reason for the event */
    /** Number of tokens saved (for compaction events) */
 * Callback function type for message lifecycle events.
export type MessageLifecycleCallback = (event: MessageLifecycleEvent) => void;
 * Runtime override for message expiration behavior.
 * When specified in ExecuteAgentParams, these values take precedence over
 * the AIAgentAction configuration for all action results in this agent run.
export type MessageExpirationOverride = {
    /** Number of turns before expiration (overrides AIAgentAction.ResultExpirationTurns) */
    /** Mode for handling expired messages (overrides AIAgentAction.ResultExpirationMode) */
    /** Mode for compacting expired messages (overrides AIAgentAction.CompactMode) */
    /** Number of characters to keep when using 'First N Chars' mode (overrides AIAgentAction.CompactLength) */
    /** Prompt ID to use for AI Summary compaction (overrides AIAgentAction.CompactPromptID) */
    /** Whether to preserve original content for expansion (default: true) */
    preserveOriginalContent?: boolean;
 * Request to expand a compacted message to its original content.
export interface ExpandMessageRequest {
    /** Step type identifier */
    step: 'expand-message';
    /** Index of the message to expand in the conversation array */
    /** Optional reason for expanding the message */
 * Scope options for runtime action changes.
 * Determines which agents in the execution hierarchy the change applies to.
export type ActionChangeScope =
    /** Applies to all agents in the hierarchy (root + all sub-agents) */
    | 'global'
    /** Applies only to the root agent */
    | 'root'
    /** Applies to all sub-agents but NOT the root agent */
    | 'all-subagents'
    /** Applies only to specific agents identified by agentIds */
    | 'specific';
 * Mode options for runtime action changes.
 * Determines how the action change is applied.
export type ActionChangeMode =
    /** Add actions to the existing set */
    | 'add'
    /** Remove actions from the existing set */
    | 'remove';
 * Represents a runtime modification to an agent's available actions.
 * Action changes allow callers to dynamically customize which actions are available
 * to agents at runtime, without modifying the agent's database configuration.
 * This is particularly useful for multi-tenant scenarios where different executions
 * of the same agent need access to different integrations.
 * // Add CRM and LMS actions to all agents in the hierarchy
 * const change: ActionChange = {
 *   scope: 'global',
 *   mode: 'add',
 *   actionIds: ['crm-search-action-id', 'lms-query-action-id']
 * // Remove dangerous actions from sub-agents only
 * const restrictChange: ActionChange = {
 *   scope: 'all-subagents',
 *   mode: 'remove',
 *   actionIds: ['delete-record-action-id', 'execute-sql-action-id']
 * // Add special actions to a specific sub-agent
 * const specificChange: ActionChange = {
 *   scope: 'specific',
 *   actionIds: ['special-data-action-id'],
 *   agentIds: ['data-gatherer-sub-agent-id']
export interface ActionChange {
     * Scope of the action change - determines which agents it applies to.
     * - 'global': All agents in the hierarchy
     * - 'root': Only the root agent
     * - 'all-subagents': All sub-agents but not the root
     * - 'specific': Only agents listed in agentIds
    scope: ActionChangeScope;
     * Mode of the action change.
     * - 'add': Add actions to the agent's available actions
     * - 'remove': Remove actions from the agent's available actions
    mode: ActionChangeMode;
     * Array of Action entity IDs to add or remove.
     * These must be valid Action IDs from the Actions table.
    actionIds: string[];
     * Array of Agent IDs that this change applies to.
     * Required when scope is 'specific', ignored otherwise.
    agentIds?: string[];
     * Optional execution limits for actions being added.
     * Maps action IDs to their maximum executions per agent run.
     * Only applies when mode is 'add'. Ignored for 'remove' mode.
     *   actionIds: ['search-action-id', 'email-action-id'],
     *   actionLimits: {
     *     'search-action-id': 10,    // Max 10 searches per run
     *     'email-action-id': 5       // Max 5 emails per run
    actionLimits?: Record<string, number>;
 * @fileoverview AI Agent and learning cycle types for Skip API
 * This file contains types related to AI agent functionality, learning cycles, and
 * human-in-the-loop interactions within the Skip API system. These types define the structure for:
 * - Agent notes and note types (SkipAPIAgentNote, SkipAPIAgentNoteType)
 * - Agent requests for human approval/feedback (SkipAPIAgentRequest)
 * - Learning cycle processes (SkipAPILearningCycleRequest, SkipAPILearningCycleResponse)
 * - Change tracking for learning cycles (various SkipLearningCycle*Change types)
 * The learning cycle functionality allows Skip to analyze conversation history and improve
 * its performance over time by generating notes, updating queries, and creating agent
 * requests based on patterns it discovers in user interactions.
 * Agent requests enable human-in-the-loop workflows where Skip can ask for approval or
 * guidance on specific actions, ensuring that AI decisions align with organizational
 * policies and user preferences.
 * Notes provide a way for Skip to store and retrieve organizational knowledge, user
 * preferences, and contextual information that improves future interactions.
import type { SkipConversation } from './conversation-types';
import type { SkipEntityInfo } from './entity-metadata-types';
import type { SkipQueryInfo, SkipLearningCycleQueryChange } from './query-types';
import type { SkipAPIRequestAPIKey } from './auth-types';
 * Type that defines a possible note type from the source system that invoked Skip
export class SkipAPIAgentNoteType {
 * Defines the shape of an individual Agent note that is stored in MJ that can be passed to Skip for additional context.
export class SkipAPIAgentNote {
     * Unique identifier for the note
     * Unique type id (UUID) for the note type, maps to a SkipAPIAgentNoteType that was passed in the SkipAPIRequest
    agentNoteTypeId: string;
     * Text name for the note type
    agentNoteType: string;
     * Date/Time the note was initially created
     * Date/Time the note was last updated
     * The text of the note
    note: string; 
     * This type field contains the scope of the note, either Global or User
    type: 'User' | 'Global';
     * The unique identifier for the user that the note is associated with, only populated if type === 'User'
     * The name of the user that the note is associated with, only populated if type === 'User'
    user: string | null;
 * Whenever an agent is interested in getting human-in-the-loop style feedback/approval, this type is used
export class SkipAPIAgentRequest {
     * The unique identifier for the request
     * The unique identifier for the agent that made the request
     * The name of the agent that made the request
     * The date and time the request was made
     * Optional, the unique identifier for the user that the request was made for by the Agent
    requestForUserId?: string;
     * Only populated if the request was made for a user, the name of the user that the request was made for
    requestForUser?: string;
     * Status of the request: 'Requested' | 'Approved' | 'Rejected' | 'Canceled'
    status: 'Requested' | 'Approved' | 'Rejected' | 'Canceled';
     * Text body of the request the AI Agent is making
    request: string;
     * Text body of the response that is being sent back to the AI Agent
     * The unique identifier for the user that responded to the request
    responseByUserId: string;
     * The name of the user that responded to the request
    responseByUser: string;
     * The date and time the user responded to the request
    respondedAt: Date;
     * Internal comments that are not intended to be shared with the AI Agent
     * The date and time the request record was created in the database
     * The date and time the request record was last updated in the database
 * Represents a change to agent notes during the learning cycle process, allowing Skip
 * to add new notes, update existing ones, or mark notes for deletion based on
 * its analysis of conversation patterns and organizational learning.
export class SkipLearningCycleNoteChange {
    note: SkipAPIAgentNote;
    changeType: 'add' | 'update' | 'delete';
 * Represents a change to agent requests during the learning cycle process, allowing Skip
 * to add new requests, update existing ones, or mark requests for deletion based on
 * its analysis of conversation patterns and user feedback.
export class SkipLearningCycleRequestChange {
    request: SkipAPIAgentRequest;
 * API Request shape to ask the /learn end point to learn from conversation history and pass back "notes" that can be stored in the database for future requests
export class SkipAPILearningCycleRequest {
     * OrganizationID for Skip to identify the organization
    organizationId: string
     * This is an optional string parameter where you can tell Skip anything you'd like to share about your organization, structure, database schema, and anything else
     * that might be helpful for him to be aware of. Keep in mind that this organizationInfo will be incorprorated into every request Skip makes to the underlying AI
     * services which can add cost and processing time to your requests. Including this information is extremely helpful as a very simple method of 
     * contextualizing Skip for your organization. In the Pro and above Skip plans, there are far more granular and effect methods of training Skip beyond this organizationInfo parameter, contact
     * the team at MemberJunction.com for more information if you're interested.
     * Learning Cycle ID is a unique identifier from the MJ AI Agent Learning Cycles table that will track the details of the API calls and the results for logging purposes and 
     * also to track the timestamps for each run to batch the conversations that are being sent
    learningCycleId: string;
     * An array of conversations that have taken place since the last learning cycle
    newConversations: SkipConversation[];
     * Summary entity metadata that is passed into the Skip Server so that Skip has knowledge of the schema of the calling MJAPI environment
    entities: SkipEntityInfo[];
     * Stored queries in the MJ metadata that Skip can use and learn from
    queries: SkipQueryInfo[];
     * An array of notes that have been generated by the Skip API server during the learning cycle process in the past
    notes: SkipAPIAgentNote[];
     * An array of the possible note types that can be stored in the source MJ system
    noteTypes: SkipAPIAgentNoteType[];
     * An array of the requests that Skip has previously made. Full history provided including requests of all status conditions.
    requests: SkipAPIAgentRequest[];
     * Optional, the date/time of the last learning cycle performed on this dataset
    lastLearningCycleDate: Date;
     * One or more API keys that are used for AI systems that Skip will access on behalf of the API caller
     * NOTE: This is not where you put in the bearer token for the Skip API server itself, that goes in the header of the request
    apiKeys: SkipAPIRequestAPIKey[];
 * API Response shape to ask the /learn end point to learn from conversation history and pass back "notes", an array of notes are provided that should be stored in the database
 * to then be passed into future Skip API requests for analysis/etc.
export class SkipAPILearningCycleResponse {
     * Indicates if the learning cycle was successful or not
     * If a learning cycle is skipped because there is no new conversation data to learn from, this property will be set to true
    learningCycleSkipped?: boolean;
     * If the learning cycle was not successful, this property will contain an error message that describes the reason for the failure
     * The number of milliseconds that have elapsed since the learning cycle process started
     * The notes that were generated by the Skip API server during the learning cycle process
    noteChanges: SkipLearningCycleNoteChange[];
     * This provides an array of changes requested by Skip to the MJ database for queries, adding, updating and/or deleting.
    queryChanges: SkipLearningCycleQueryChange[];
     * This array should be populated by the agent with any changes to requests - deleting existing requests that have not been responded to yet and for whatever reason are not relevant anymore, updating existing requests that haven't yet been responded to, and adding new requests to help the agent learn.
    requestChanges: SkipLearningCycleRequestChange[];
