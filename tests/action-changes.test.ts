 * Unit tests for Runtime Action Changes functionality.
 * These tests verify the core logic for action change processing:
 * - doesChangeScopeApply(): Determines if a change applies to an agent
 * - applyActionChanges(): Applies changes to a base action set
 * - filterActionChangesForSubAgent(): Filters changes for sub-agent propagation
import { describe, it, expect } from 'vitest';
import { ActionChange, ActionChangeScope } from '@memberjunction/ai-core-plus';
// ============================================================================
// Test Helpers - Standalone implementations of the methods for testing
// These mirror the implementations in BaseAgent but without class dependencies
function doesChangeScopeApply(
 * Mock action type for testing
interface MockAction {
    Status: string;
 * Mock action registry for testing applyActionChanges
const mockActionRegistry: MockAction[] = [
    { ID: 'action-1', Name: 'Send Email', Status: 'Active' },
    { ID: 'action-2', Name: 'Create Record', Status: 'Active' },
    { ID: 'action-3', Name: 'Delete Record', Status: 'Active' },
    { ID: 'action-4', Name: 'Execute SQL', Status: 'Active' },
    { ID: 'action-5', Name: 'LMS Query', Status: 'Active' },
    { ID: 'action-6', Name: 'CRM Search', Status: 'Active' },
 * Applies runtime action changes to a base set of actions.
 * Also returns any dynamic action limits that were specified.
function applyActionChanges(
    baseActions: MockAction[],
): { actions: MockAction[], dynamicLimits: Record<string, number> } {
        if (!doesChangeScopeApply(change.scope, agentId, isRoot, change.agentIds)) {
                    const actionToAdd = mockActionRegistry.find(a => a.ID === actionId);
function filterActionChangesForSubAgent(
// Tests for doesChangeScopeApply
describe('doesChangeScopeApply', () => {
    describe('global scope', () => {
        it('applies to root agent', () => {
            expect(doesChangeScopeApply('global', 'agent-1', true)).toBe(true);
        it('applies to sub-agent', () => {
            expect(doesChangeScopeApply('global', 'agent-2', false)).toBe(true);
    describe('root scope', () => {
            expect(doesChangeScopeApply('root', 'agent-1', true)).toBe(true);
        it('does NOT apply to sub-agent', () => {
            expect(doesChangeScopeApply('root', 'agent-2', false)).toBe(false);
    describe('all-subagents scope', () => {
        it('does NOT apply to root agent', () => {
            expect(doesChangeScopeApply('all-subagents', 'agent-1', true)).toBe(false);
            expect(doesChangeScopeApply('all-subagents', 'agent-2', false)).toBe(true);
    describe('specific scope', () => {
        const specificAgents = ['agent-2', 'agent-3'];
        it('does NOT apply to agent not in list (root)', () => {
            expect(doesChangeScopeApply('specific', 'agent-1', true, specificAgents)).toBe(false);
        it('applies to agent in list', () => {
            expect(doesChangeScopeApply('specific', 'agent-2', false, specificAgents)).toBe(true);
        it('does NOT apply to agent not in list (sub-agent)', () => {
            expect(doesChangeScopeApply('specific', 'agent-4', false, specificAgents)).toBe(false);
        it('returns false with undefined agentIds', () => {
            expect(doesChangeScopeApply('specific', 'agent-1', true, undefined)).toBe(false);
        it('returns false with empty agentIds', () => {
            expect(doesChangeScopeApply('specific', 'agent-1', true, [])).toBe(false);
// Tests for applyActionChanges
describe('applyActionChanges', () => {
    it('add mode with global scope', () => {
        const baseActions: MockAction[] = [
        const changes: ActionChange[] = [
            { scope: 'global', mode: 'add', actionIds: ['action-5', 'action-6'] }
        const { actions: result } = applyActionChanges(baseActions, changes, 'agent-1', true);
        expect(result.length).toBe(3);
        expect(result.some(a => a.ID === 'action-5')).toBe(true);
        expect(result.some(a => a.ID === 'action-6')).toBe(true);
    it('add mode does not duplicate', () => {
        expect(result.filter(a => a.ID === 'action-5').length).toBe(1);
    it('remove mode', () => {
            { scope: 'global', mode: 'remove', actionIds: ['action-3', 'action-4'] }
        expect(result.length).toBe(1);
        expect(result[0].ID).toBe('action-1');
    it('scope filtering (root only)', () => {
            { scope: 'root', mode: 'add', actionIds: ['action-5'] }
        const { actions: rootResult } = applyActionChanges(baseActions, changes, 'root-agent', true);
        const { actions: subAgentResult } = applyActionChanges(baseActions, changes, 'sub-agent', false);
        expect(rootResult.length).toBe(2);
        expect(subAgentResult.length).toBe(1);
    it('scope filtering (all-subagents)', () => {
            { scope: 'all-subagents', mode: 'remove', actionIds: ['action-3'] }
    it('scope filtering (specific)', () => {
                scope: 'specific',
                mode: 'add',
                actionIds: ['action-5'],
                agentIds: ['special-agent']
        const { actions: normalResult } = applyActionChanges(baseActions, changes, 'normal-agent', false);
        const { actions: specialResult } = applyActionChanges(baseActions, changes, 'special-agent', false);
        expect(normalResult.length).toBe(1);
        expect(specialResult.length).toBe(2);
    it('multiple changes applied in order', () => {
            { scope: 'global', mode: 'remove', actionIds: ['action-2'] },
            { scope: 'global', mode: 'add', actionIds: ['action-5'] }
        expect(result.length).toBe(2);
        expect(result.some(a => a.ID === 'action-2')).toBe(false);
    it('actionLimits stored for added actions', () => {
                scope: 'global',
                actionIds: ['action-5', 'action-6'],
                actionLimits: {
                    'action-5': 10,
                    'action-6': 5
        const { actions, dynamicLimits } = applyActionChanges(baseActions, changes, 'agent-1', true);
        expect(actions.length).toBe(3);
        expect(dynamicLimits['action-5']).toBe(10);
        expect(dynamicLimits['action-6']).toBe(5);
        expect(dynamicLimits['action-1']).toBeUndefined();
    it('actionLimits partial (some actions without limits)', () => {
        const baseActions: MockAction[] = [];
                    'action-5': 3  // Only action-5 has a limit
        expect(actions.length).toBe(2);
        expect(dynamicLimits['action-5']).toBe(3);
        expect(dynamicLimits['action-6']).toBeUndefined();
    it('actionLimits ignored for remove mode', () => {
                mode: 'remove',
                    'action-5': 10  // Should be ignored for remove mode
        expect(actions.length).toBe(1);
        expect(Object.keys(dynamicLimits).length).toBe(0);
// Tests for filterActionChangesForSubAgent
describe('filterActionChangesForSubAgent', () => {
    it('empty/undefined input returns undefined', () => {
        expect(filterActionChangesForSubAgent(undefined)).toBeUndefined();
        expect(filterActionChangesForSubAgent([])).toBeUndefined();
    it('global scope propagated as-is', () => {
            { scope: 'global', mode: 'add', actionIds: ['action-1'] }
        const result = filterActionChangesForSubAgent(changes);
        expect(result).toBeDefined();
        expect(result!.length).toBe(1);
        expect(result![0].scope).toBe('global');
        expect(result![0].mode).toBe('add');
        expect(result![0].actionIds).toEqual(['action-1']);
    it('root scope NOT propagated', () => {
            { scope: 'root', mode: 'add', actionIds: ['action-1'] }
        expect(result).toBeUndefined();
    it('all-subagents becomes global', () => {
        expect(result![0].mode).toBe('remove');
    it('specific scope propagated as-is', () => {
            { scope: 'specific', mode: 'add', actionIds: ['action-5'], agentIds: ['agent-x', 'agent-y'] }
        expect(result![0].scope).toBe('specific');
        expect(result![0].agentIds).toEqual(['agent-x', 'agent-y']);
    it('mixed scopes', () => {
            { scope: 'global', mode: 'add', actionIds: ['action-1'] },
            { scope: 'root', mode: 'add', actionIds: ['action-2'] },
            { scope: 'all-subagents', mode: 'remove', actionIds: ['action-3'] },
            { scope: 'specific', mode: 'add', actionIds: ['action-4'], agentIds: ['agent-x'] }
        expect(result!.length).toBe(3);
        const globalChange = result!.find(c => c.actionIds.includes('action-1'));
        const transformedChange = result!.find(c => c.actionIds.includes('action-3'));
        const specificChange = result!.find(c => c.actionIds.includes('action-4'));
        expect(globalChange).toBeDefined();
        expect(globalChange!.scope).toBe('global');
        expect(transformedChange).toBeDefined();
        expect(transformedChange!.scope).toBe('global');
        expect(specificChange).toBeDefined();
        expect(specificChange!.scope).toBe('specific');
