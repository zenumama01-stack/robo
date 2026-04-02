import { Resolver, Mutation, Arg, Ctx, ObjectType, Field, PubSub, PubSubEngine } from 'type-graphql';
import { TaskOrchestrator, TaskGraphResponse, TaskExecutionResult } from '../services/TaskOrchestrator.js';
export class TaskExecutionResultType {
export class ExecuteTaskGraphResult {
    @Field(() => [TaskExecutionResultType])
    results: TaskExecutionResultType[];
 * TaskOrchestrationResolver handles multi-step task orchestration.
 * This resolver is called when the Conversation Manager returns a task graph
 * for complex workflows that require multiple agents working in sequence or parallel.
export class TaskOrchestrationResolver extends ResolverBase {
     * Execute a task graph from the Conversation Manager.
     * This creates tasks in the database, manages dependencies, and executes them in proper order.
     * @param taskGraphJson - JSON string containing the task graph from Conversation Manager
     * @param conversationDetailId - ID of the conversation detail that triggered this workflow
     * @param environmentId - Environment ID for the tasks
    @Mutation(() => ExecuteTaskGraphResult)
    async ExecuteTaskGraph(
        @Arg('taskGraphJson') taskGraphJson: string,
        @Arg('environmentId') environmentId: string,
        @Arg('createNotifications', { nullable: true }) createNotifications?: boolean
    ): Promise<ExecuteTaskGraphResult> {
        // Check API key scope authorization for task execution
        await this.CheckAPIKeyScopeAuthorization('task:execute', '*', userPayload);
            LogStatus(`=== EXECUTING TASK GRAPH FOR CONVERSATION: ${conversationDetailId} ===`);
            // Parse task graph
            const taskGraph: TaskGraphResponse = JSON.parse(taskGraphJson);
            // Validate task graph
            if (!taskGraph.workflowName || !taskGraph.tasks || taskGraph.tasks.length === 0) {
                throw new Error('Invalid task graph: must have workflowName and at least one task');
            LogStatus(`Workflow: ${taskGraph.workflowName} (${taskGraph.tasks.length} tasks)`);
            if (taskGraph.reasoning) {
                LogStatus(`Reasoning: ${taskGraph.reasoning}`);
            // Create task orchestrator with PubSub for progress updates
            const orchestrator = new TaskOrchestrator(currentUser, pubSub, sessionId, userPayload, createNotifications || false, conversationDetailId);
            // Create parent task and child tasks with dependencies
            const { parentTaskId, taskIdMap } = await orchestrator.createTasksFromGraph(
                taskGraph,
                environmentId
            LogStatus(`Created parent task ${parentTaskId} with ${taskIdMap.size} child tasks`);
            // Execute tasks in proper order
            const results = await orchestrator.executeTasksForParent(
                parentTaskId
            // Log results
            const successCount = results.filter(r => r.success).length;
            LogStatus(`Completed ${successCount} of ${results.length} tasks successfully`);
                    LogStatus(`✅ Task ${result.taskId} completed successfully`);
                    LogError(`❌ Task ${result.taskId} failed: ${result.error}`);
            // Convert results to GraphQL types
            const graphqlResults: TaskExecutionResultType[] = results.map(r => ({
                taskId: r.taskId,
                success: r.success,
                output: r.output ? JSON.stringify(r.output) : undefined,
                error: r.error
            LogStatus(`=== TASK GRAPH EXECUTION COMPLETE ===`);
                results: graphqlResults
            LogStatus(`Returning ExecuteTaskGraph result: ${JSON.stringify({
                resultsCount: result.results.length,
                firstResult: result.results[0]
            })}`);
            LogError(`Task graph execution failed:`, undefined, error);
                results: []
