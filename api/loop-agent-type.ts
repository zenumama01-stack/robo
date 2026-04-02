 * @fileoverview Implementation of the Loop Agent Type for iterative task execution.
 * The LoopAgentType enables agents to operate in a continuous loop, making decisions
 * about task completion and next steps based on the current state. It parses structured
 * JSON responses from the AI model and translates them into BaseAgentNextStep directives.
import { AIPromptRunResult, BaseAgentNextStep, AIPromptParams, ExecuteAgentParams, AgentConfiguration, AgentAction } from '@memberjunction/ai-core-plus';
import { LogError, LogStatusEx } from '@memberjunction/core';
import { AIPromptEntityExtended } from '@memberjunction/ai-core-plus';
import { LoopAgentResponse } from './loop-agent-response-type';
 * Implementation of the Loop Agent Type pattern.
 * This agent type enables iterative execution where the agent continues
 * processing until a task is marked complete. It supports:
 * - Task completion detection
 * - Sub-agent delegation
 * - Action execution
 * - Progress tracking
 * - Error handling
 * - Result accumulation across iterations
 * @class LoopAgentType
 * // The agent will execute in a loop until taskComplete is true
 * const loopAgent = new LoopAgentType();
 * const nextStep = await loopAgent.DetermineNextStep(promptResult);
 * switch (nextStep.step) {
 *   case 'success':
 *     console.log('Task completed:', nextStep.payload);
 *     break;
 *   case 'sub-agent':
 *     console.log('Delegating to:', nextStep.subAgentName);
 *   case 'action':
 *     console.log('Executing action:', nextStep.actionName);
@RegisterClass(BaseAgentType, "LoopAgentType")
export class LoopAgentType extends BaseAgentType {
        // Loop agents do not require agent-type specific state initialization
        // but can be extended in the future if needed
        return {} as ATS; // Return an empty object for now
     * Determines the next step based on the structured response from the AI model.
     * This method parses the JSON response from the loop agent prompt and translates
     * it into appropriate BaseAgentNextStep directives. It handles task completion,
     * sub-agent delegation, action execution, and error conditions.
     * @param {AIPromptRunResult} promptResult - The result from executing the agent's prompt
     * @returns {Promise<BaseAgentNextStep>} The next step to take in the agent execution
     * @throws {Error} Implicitly through failed parsing, but returns failed step instead
            // Ensure we have a successful result
                    errorMessage: promptResult.errorMessage || 'Prompt execution failed'
            // Parse the response using the base class utility
            const response = this.parseJSONResponse<LoopAgentResponse>(promptResult);
            if (!response) {
                return this.createRetryStep('Failed to parse JSON response');
            // Validate the response structure
            const validationResult = this.isValidLoopResponse(response);
            if (!validationResult.success) {
                return this.createRetryStep(validationResult.message);
            // Check for Chat nextStep BEFORE checking taskComplete
            // This allows agents to ask for user clarification even when taskComplete=true
            if (response.nextStep?.type === 'Chat') {
                if (!response.message) {
                    return this.createRetryStep('Chat type specified but no user message provided');
                    message: '💬 Loop Agent: Requesting user interaction. Message: ' + response.message,
                    message: response.message,
                    terminate: true, // Chat always terminates to return to user
                    payloadChangeRequest: response.payloadChangeRequest,
                    responseForm: response.responseForm,
                    actionableCommands: response.actionableCommands,
                    automaticCommands: response.automaticCommands,
                    reasoning: response.reasoning,
                    confidence: response.confidence
            // Check if task is complete
            if (response.taskComplete) {
                    message: '✅ Loop Agent: Task completed successfully. Message: ' + response.message,
                    confidence: response.confidence,
                    automaticCommands: response.automaticCommands
            // Handle when nextStep is not provided but task is not complete
            if (!response.nextStep) {
                return this.createRetryStep('Task not complete but no next step provided');
            // Determine next step based on type
            const retVal: Partial<BaseAgentNextStep<P>> = {
                terminate: response.taskComplete,
            switch (response.nextStep.type) {
                    if (!response.nextStep.subAgent) {
                        retVal.step = 'Retry';
                        retVal.message = 'When nextStep.type == "Sub-Agent", subAgent details must be specified';
                        retVal.errorMessage = 'Sub-agent details not specified';
                        retVal.step = 'Sub-Agent';
                        retVal.subAgent = {
                            name: response.nextStep.subAgent.name,
                            message: response.nextStep.subAgent.message,
                            terminateAfter: response.nextStep.subAgent.terminateAfter,
                            templateParameters: response.nextStep.subAgent.templateParameters || {}
                    if (!response.nextStep.actions || response.nextStep.actions.length === 0) {
                        retVal.message = 'When nextStep.type == "Actions", 1 or more actions must be specified in the nextStep.actions array';
                        retVal.errorMessage = 'Actions not specified for action type';
                        retVal.step = 'Actions',
                        retVal.actions = response.nextStep.actions.map(action => ({
                            name: action.name,
                            params: action.params
                    if (!response.nextStep.forEach) {
                        retVal.errorMessage = 'ForEach type specified but forEach details missing';
                        // Return ForEach decision for BaseAgent to execute in next iteration
                        retVal.step = 'ForEach';
                        retVal.forEach = response.nextStep.forEach;
                    if (!response.nextStep.while) {
                        retVal.errorMessage = 'While type specified but while details missing';
                        // Return While decision for BaseAgent to execute in next iteration
                        retVal.step = 'While';
                        retVal.while = response.nextStep.while;
                    retVal.errorMessage = `Unknown next step type: ${response.nextStep.type}`;
            return retVal as BaseAgentNextStep<P>;
            LogError(`Error in LoopAgentType.DetermineNextStep: ${error.message}`);
            return this.createRetryStep(`Failed to parse loop agent response: ${error.message}`);
     * Validates that the response conforms to the expected LoopAgentResponse structure.
     * @param {any} response - The response to validate
     * @returns {boolean} True if the response is valid, false otherwise
    protected isValidLoopResponse(simpleResponse: unknown): {success: boolean, message?: string} {
        // Check required fields, first cast the simpleResponse to LoopAgentResponse
        // so we get the benefit of TypeScript's type checking below in this method
        const response = simpleResponse as LoopAgentResponse;
        if (typeof response !== 'object' || response === null) {
            return {success: false, message: 'Invalid response format'};
        if (!response.taskComplete) {
            // if not provided, default to false to make processing work
            response.taskComplete = false;
        if (typeof response.taskComplete !== 'boolean') {
            LogError('LoopAgentResponse missing required field: taskComplete');
            return {success: false, message: 'Missing required field: taskComplete'};
        // nextStep is optional when taskComplete is true
        if (!response.taskComplete && !response.nextStep) {
            if (response.message?.trim().length > 0 || 
                    typeof response.reasoning === 'string' && 
                    response.reasoning?.trim().length > 0
                // in this situation we have a message or reasoning coming back but a malformed response
                // so we can consider it a chat response becuase it is trying to communicate
                // something back, better to provide that back than discarding it entirely
                // possibly we will make this configurable in the future
                response.nextStep = {
                    type: 'Chat'
                if (response.message?.trim().length === 0) {
                    // this means reasoning was provided but no message, copy reasoning to message 
                    // as you shouldn't ever have that but we can handle it gracefully
                    response.message = response.reasoning;
            else if (response.payloadChangeRequest?.newElements ||
                    response.payloadChangeRequest?.updateElements ||
                    response.payloadChangeRequest?.removeElements) {
                // the AI forgot to mark taskComplete as true but provided a payloadChangeRequest
                // with changes to the elements, so we can consider this a valid response
                // and we can consider it attempting to have a taskComplete = true
                // and validation will catch if it didn't actually complete and drive a retry
                response.message = 'taskComplete set to true automatically due to payloadChangeRequest and no nextStep provided by AI';
                response.taskComplete = true;
                LogError('LoopAgentResponse requires nextStep when taskComplete is false');
                        message: `Missing nextStep is a required field when taskComplete is false
                                  Per the LoopAgentResponse structure, nextStep is an object with a key called type that can be: 
                                  - "Actions", 
                                  - "Sub-Agent"
                                  - "Chat"
                                  If taskComplete is true, nextStep is optional.
                                  If taskComplete is false, nextStep is required and fill in type with the appropriate value!`};
        // Validate nextStep structure if present
        if (response.nextStep) {
            const validStepTypes = ['actions', 'sub-agent', 'chat', 'foreach', 'while'];
            let lcaseType = response.nextStep.type?.toLowerCase().trim();
            // allow the AI to mess up the case, but we need to validate it
            // be smart/lenient about missing types. if type is missing but we have a nextStep.subAgent, default to sub-agent and if type is missing and we have nextStep.actions, default to actions
            if (!lcaseType && response.nextStep.subAgent) {
                response.nextStep.type = 'Sub-Agent'; // update the data structure to have the correct type
                lcaseType = 'sub-agent';
            } else if (!lcaseType && response.nextStep.actions && response.nextStep.actions.length > 0) {
                response.nextStep.type = 'Actions'; // update the data structure to have the correct type
                lcaseType = 'actions';
            } else if (!lcaseType && response.nextStep.forEach) {
                response.nextStep.type = 'ForEach'; // update the data structure to have the correct type
                lcaseType = 'foreach';
            } else if (!lcaseType && response.nextStep.while) {
                response.nextStep.type = 'While'; // update the data structure to have the correct type
                lcaseType = 'while';
            if (!validStepTypes.includes(lcaseType)) {
                const message = `LoopAgentResponse has invalid nextStep.type: ${response.nextStep.type}`;
                LogError(message);
                return {success: false, message};
            // Validate specific fields based on type
            if (lcaseType === 'actions' && !response.nextStep.actions) {
                const message = 'LoopAgentResponse requires actions array for action type';
            if (lcaseType === 'sub-agent' && !response.nextStep.subAgent) {
                const message = 'LoopAgentResponse requires subAgent object for sub-agent type';
            if (lcaseType === 'chat' && !response.message) {
                // check to see if we have reasoning, if so, use that, otherwise we have to fail
                if (!response.reasoning || response.reasoning.trim().length === 0) {
                    const message = 'LoopAgentResponse requires message for chat type';
                    // if we have reasoning, use that as the message
            if (lcaseType === 'foreach' && !response.nextStep.forEach) {
                const message = 'LoopAgentResponse requires forEach object for ForEach type';
            if (lcaseType === 'while' && !response.nextStep.while) {
                const message = 'LoopAgentResponse requires while object for While type';
        return {success: true};
     * Injects a payload into the prompt parameters.
     * For LoopAgentType, this could be used to inject previous loop results or context.
     * @param {T} payload - The payload to inject
     * @param {object} agentInfo - Agent identification info (unused by LoopAgentType)
    public async InjectPayload<T = any, ATS = any>(
        payload: T, 
        if (!prompt)
        if (!prompt.data )
     * Determines the initial step for loop agent types.
     * Loop agents always start with a prompt execution to determine the initial actions.
     * @returns {Promise<BaseAgentNextStep<P> | null>} Always returns null to use default behavior
    public async DetermineInitialStep<P = any>(params: ExecuteAgentParams<P>): Promise<BaseAgentNextStep<P> | null> {
        // Loop agents always start with a prompt execution
     * Pre-processes steps for loop agent types.
     * Loop agents use the default next step behavior which executes the prompt again.
     * @param {BaseAgentNextStep} step - The step that needs to be preprocessed
        // Loop agents use default next step behavior (execute prompt)
     * Loop agents always use the default prompt from configuration.
     * @param {ExecuteAgentParams} params - The execution parameters (unused)
     * @param {BaseAgentNextStep | null} previousDecision - The previous step decision (unused)
     * @returns {Promise<AIPromptEntityExtended | null>} Returns config.childPrompt
        // Loop agents always use the default prompt from configuration
     * Pre-processes action parameters to resolve conversation references.
     * Loop agents get action parameters directly from the LLM's JSON response.
     * This method resolves any "conversation.*" references in those parameters
     * before the actions are executed.
     * @param {ATS} agentTypeState - The agent type state
     * @param {ExecuteAgentParams<P>} params - The execution parameters with conversation messages
     * @since 2.120.0
        // Resolve conversation references in action parameters
            if (action.params) {
                action.params = this.resolveConversationReferences(action.params, params) as Record<string, unknown>;
     * Recursively resolves conversation references in action parameters.
     * Handles nested objects and arrays, resolving any string values that start
     * with "conversation." using the ConversationMessageResolver.
     * @param {unknown} value - The value to resolve (can be object, array, string, or primitive)
     * @param {ExecuteAgentParams} params - The execution parameters with conversation messages
     * @returns {unknown} The resolved value
    private resolveConversationReferences(value: unknown, params?: ExecuteAgentParams): unknown {
            // Recursively resolve array elements
            return value.map(item => this.resolveConversationReferences(item, params));
            // Recursively resolve object properties
                resolvedObj[key] = this.resolveConversationReferences(val, params);
            // Primitives (numbers, booleans, null, undefined) pass through
