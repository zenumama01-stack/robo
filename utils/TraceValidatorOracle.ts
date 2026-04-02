 * @fileoverview Trace validation oracle implementation
import { MJAIAgentRunEntity, MJAIAgentRunStepEntity } from '@memberjunction/core-entities';
 * Trace Validator Oracle.
 * Validates that agent execution trace has no errors.
 * Checks AgentRun status and all AgentRunStep records for error conditions.
 * - allowWarnings: Whether to allow warning-level issues (default: true)
 * - requiredSteps: Minimum number of steps expected (optional)
 * - maxSteps: Maximum number of steps allowed (optional)
 * const oracle = new TraceValidatorOracle();
 *     targetEntity: agentRun,
 *     allowWarnings: true,
 *     requiredSteps: 2
export class TraceValidatorOracle implements IOracle {
    readonly type = 'trace-no-errors';
     * Evaluate agent run trace for errors.
     * @param input - Oracle input with agent run entity
     * @returns Oracle result with pass/fail and trace analysis
            // Get agent run entity
            const agentRun = input.targetEntity as MJAIAgentRunEntity;
                    message: 'No agent run entity provided'
            // Check agent run status
            if (agentRun.Status === 'Failed' || agentRun.Status === 'Cancelled') {
                    message: `Agent run failed with status: ${agentRun.Status}`,
                        status: agentRun.Status
            // Load agent run steps
                ExtraFilter: `AgentRunID='${agentRun.ID}'`,
            }, input.contextUser);
                    message: `Failed to load agent run steps: ${stepsResult.ErrorMessage}`
            // Check step count constraints
            const requiredSteps = config.requiredSteps as number;
            const maxSteps = config.maxSteps as number;
            if (requiredSteps && steps.length < requiredSteps) {
                    message: `Expected at least ${requiredSteps} steps, got ${steps.length}`,
                    details: { stepCount: steps.length, requiredSteps }
            if (maxSteps && steps.length > maxSteps) {
                    message: `Expected at most ${maxSteps} steps, got ${steps.length}`,
                    details: { stepCount: steps.length, maxSteps }
            // Check each step for errors
            const allowWarnings = config.allowWarnings !== false; // Default to true
                if (step.Status === 'Failed') {
                    errors.push(`Step ${step.StepNumber}: ${step.StepName} - ${step.Status}`);
                // Note: Output field doesn't exist on MJAIAgentRunStepEntity
                // If we need to check output, we'd need to load it from another source
            // Determine result
                    message: `Trace contains ${errors.length} error(s)`,
                    details: { errors, warnings, stepCount: steps.length }
            if (!allowWarnings && warnings.length > 0) {
                    score: 0.5,
                    message: `Trace contains ${warnings.length} warning(s)`,
                    details: { warnings, stepCount: steps.length }
            // Success
                message: `Trace clean with ${steps.length} step(s)`,
                message: `Trace validation error: ${(error as Error).message}`
