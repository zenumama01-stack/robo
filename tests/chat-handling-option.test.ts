 * Unit tests for ChatHandlingOption functionality in sub-agent execution.
 * These tests verify that when a sub-agent returns FinalStep='Chat',
 * the parent agent's ChatHandlingOption is respected and the Chat step
 * is remapped to Success/Failed/Retry as configured.
 * @since 3.1.1
// Mock Types and Helpers
    ChatHandlingOption: 'Success' | 'Failed' | 'Retry' | null;
 * Mock next step structure
interface MockNextStep {
    step: 'Success' | 'Failed' | 'Chat' | 'Retry';
    terminate?: boolean;
    message?: string | null;
    previousPayload?: Record<string, unknown>;
    newPayload?: Record<string, unknown>;
    responseForm?: Record<string, unknown>;
    actionableCommands?: Record<string, unknown>[];
    automaticCommands?: Record<string, unknown>[];
 * Mock params structure
interface MockParams {
    agent: MockAgent;
// Standalone Implementation of validateChatNextStep for Testing
// This mirrors the BaseAgent.validateChatNextStep() implementation
 * Implements ChatHandlingOption remapping logic.
function validateChatNextStep(
    params: MockParams,
    nextStep: MockNextStep
): MockNextStep {
                console.error(`Invalid ChatHandlingOption value: ${chatHandlingOption}`);
        const remappedStep: MockNextStep = {
            console.log(`  [DEBUG] Remapping Chat step to ${chatHandlingOption}`);
        return remappedStep;
// Tests for ChatHandlingOption with Sub-Agent Execution
describe('ChatHandlingOption', () => {
    it('null (default behavior) - Chat step is not remapped', () => {
        const parentAgent: MockAgent = { ID: 'parent-1', Name: 'Parent Agent', ChatHandlingOption: null };
        const params: MockParams = { agent: parentAgent };
        const chatStep: MockNextStep = { step: 'Chat', terminate: true, message: 'Need user input', newPayload: { someData: 'test' } };
        const result = validateChatNextStep(params, chatStep);
        expect(result.step).toBe('Chat');
        expect(result.message).toBe('Need user input');
        expect(result.terminate).toBe(true);
    it('"Success" - remap Chat to Success', () => {
        const parentAgent: MockAgent = { ID: 'parent-2', Name: 'Parent Agent', ChatHandlingOption: 'Success' };
        const chatStep: MockNextStep = {
            step: 'Chat', terminate: true, message: 'Need user input',
            newPayload: { someData: 'test' }, responseForm: { type: 'form' },
            actionableCommands: [{ command: 'approve' }]
        expect(result.step).toBe('Success');
        expect(result.newPayload).toEqual({ someData: 'test' });
        expect(result.responseForm).toEqual({ type: 'form' });
    it('"Failed" - remap Chat to Failed', () => {
        const parentAgent: MockAgent = { ID: 'parent-3', Name: 'Parent Agent', ChatHandlingOption: 'Failed' };
        const chatStep: MockNextStep = { step: 'Chat', terminate: true, message: 'Cannot proceed without user input', newPayload: { someData: 'test' } };
        expect(result.step).toBe('Failed');
        expect(result.message).toBe('Cannot proceed without user input');
    it('"Retry" - remap Chat to Retry', () => {
        const parentAgent: MockAgent = { ID: 'parent-4', Name: 'Parent Agent', ChatHandlingOption: 'Retry' };
        const chatStep: MockNextStep = { step: 'Chat', terminate: true, message: 'Need clarification', newPayload: { someData: 'test' } };
        expect(result.step).toBe('Retry');
        expect(result.message).toBe('Need clarification');
describe('Sub-Agent Chat Bubbling (Real-World Scenario)', () => {
    it('Parent remaps child Chat to Success', () => {
        const parentAgent: MockAgent = { ID: 'parent-5', Name: 'Technical Product Manager', ChatHandlingOption: 'Success' };
        const childFinalStep: MockNextStep = {
            step: 'Chat', terminate: true, message: 'Need design approval',
            newPayload: { technicalDesign: 'Component architecture' }
        const parentParams: MockParams = { agent: parentAgent };
        const parentResult = validateChatNextStep(parentParams, childFinalStep);
        expect(parentResult.step).toBe('Success');
        expect(parentResult.message).toBe('Need design approval');
        expect(parentResult.newPayload).toEqual({ technicalDesign: 'Component architecture' });
describe('Verbose Logging', () => {
    it('Chat remapped with verbose logging', () => {
        const parentAgent: MockAgent = { ID: 'parent-6', Name: 'Verbose Agent', ChatHandlingOption: 'Success' };
        const params: MockParams = { agent: parentAgent, verbose: true };
        const chatStep: MockNextStep = { step: 'Chat', terminate: true, message: 'Test' };
