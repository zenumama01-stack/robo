import { BaseAgent } from '@memberjunction/ai-agents';
import { ExecuteAgentParams, AgentConfiguration, BaseAgentNextStep, AgentSpec } from '@memberjunction/ai-core-plus';
import { Metadata } from '@memberjunction/core';
import { AIAgentRunStepEntityExtended } from '@memberjunction/ai-core-plus';
import { TemplateEngineServer } from '@memberjunction/templates';
import { AIEngineBase } from '@memberjunction/ai-engine-base';
 * Builder Agent - Persists validated AgentSpec to database via AgentSpecSync
 * This is a code-based agent that overrides executeAgentInternal to bypass
 * the normal chat loop and directly execute persistence logic. It receives
 * a validated AgentSpec from the Architect Agent and saves it to the database
 * using AgentSpecSync.
 * - Receive validated AgentSpec from payload
 * - Create AgentSpecSync instance
 * - Persist to database
 * - Track mutations in AI Agent Run Steps table
 * - Return success with created agent ID, or failure with error details
 * This agent does NOT iterate - it runs once and returns Success or Failed.
@RegisterClass(BaseAgent, 'AgentBuilderAgent')
export class AgentBuilderAgent extends BaseAgent {
     * Override executeAgentInternal to run code instead of chat loop.
     * Directly creates the AI Agent Run Step with mutations from AgentSpecSync.
    protected override async executeAgentInternal<P = any>(
        params: ExecuteAgentParams<P>,
        _config: AgentConfiguration
    ): Promise<{ finalStep: BaseAgentNextStep<P>; stepCount: number }> {
        console.log('🔨 Builder Agent: Starting agent persistence...');
            // Validate payload
            const agentSpec = params.payload as AgentSpec;
            if (!agentSpec) {
                throw new Error('No AgentSpec found in payload - ensure Architect Agent provided valid AgentSpec');
            if (!agentSpec.Name) {
                throw new Error('AgentSpec is missing required Name field');
            console.log(`🔨 Builder Agent: Creating agent "${agentSpec.Name}"...`);
            // Create AgentSpecSync and save to database
            const specSync = new AgentSpecSync(agentSpec, params.contextUser);
            specSync.markDirty();
            // If updating existing agent (ID exists), mark as loaded so delete logic runs
            if (agentSpec.ID && agentSpec.ID !== '') {
                specSync.markLoaded();
            const result = await specSync.SaveToDatabase();
                throw new Error('AgentSpecSync.SaveToDatabase() returned success=false');
            console.log(`✅ Builder Agent: Successfully created agent with ID: ${result.agentId}`);
            // Refresh metadata and template caches
            console.log('🔄 Builder Agent: Refreshing metadata and template caches...');
            await md.Refresh();
            const templateEngine = new TemplateEngineServer();
            await templateEngine.Config(true, params.contextUser);
            console.log('✅ Builder Agent: Metadata and template caches refreshed');
            // Refresh AIEngine cache so newly created agents are immediately available
            const aiEngine = AIEngineBase.Instance;
            await aiEngine.Config(true, params.contextUser);
            console.log('✅ Builder Agent: AIEngine cache refreshed');
            // Create AI Agent Run Step record directly
            const agentRunId = params.parentRun?.ID || 'unknown';
            const stepEntity = await md.GetEntityObject<AIAgentRunStepEntityExtended>(
                'MJ: AI Agent Run Steps',
                params.contextUser
            stepEntity.AgentRunID = agentRunId;
            stepEntity.StepNumber = 2; // Validation is step 1, this is step 2
            stepEntity.StepType = 'Decision';
            stepEntity.StepName = 'Sync Agent Spec';
            stepEntity.Status = 'Completed';
            stepEntity.Success = true;
            stepEntity.StartedAt = new Date();
            stepEntity.CompletedAt = new Date();
            // InputData = Full AgentSpec
            stepEntity.InputData = JSON.stringify(agentSpec, null, 2);
            stepEntity.PayloadAtStart = JSON.stringify(params.payload);
            // OutputData = Mutations array
            stepEntity.OutputData = JSON.stringify(result.mutations, null, 2);
            stepEntity.PayloadAtEnd = stepEntity.PayloadAtStart;
            await stepEntity.Save();
            console.log(`✅ Builder Agent: Created AI Agent Run Step with ID: ${stepEntity.ID}`);
            // Return success
            const updatedSpec = { ...agentSpec, ID: result.agentId };
                finalStep: {
                    terminate: true,
                    step: 'Success',
                    reasoning: `Successfully created agent "${agentSpec.Name}" with ID: ${result.agentId}`,
                    newPayload: updatedSpec as P
                stepCount: 2
            console.error('❌ Builder Agent: Failed to create agent:', error);
            // Create failed AI Agent Run Step
            stepEntity.StepNumber = 2;
            stepEntity.Status = 'Failed';
            stepEntity.Success = false;
            stepEntity.ErrorMessage = error?.message || String(error);
            stepEntity.InputData = JSON.stringify(params.payload, null, 2);
            stepEntity.OutputData = JSON.stringify({ error: error?.message || String(error) });
                    step: 'Failed',
                    reasoning: `Failed to create agent: ${error?.message || String(error)}`
