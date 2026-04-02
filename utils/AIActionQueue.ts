import { QueueBase, TaskBase, TaskResult } from "../generic/QueueBase";
@RegisterClass(QueueBase, 'AI Action')
export class AIActionQueue extends QueueBase {
        return this.ProcessGeneric(task, false)
    protected async ProcessGeneric(task: TaskBase, entityAIAction: boolean): Promise<TaskResult> {
            await AIEngine.Instance.Config(false, this._contextUser);
            if (entityAIAction)
                result = await AIEngine.Instance.ExecuteEntityAIAction(task.Data);
                result = await AIEngine.Instance.ExecuteAIAction(task.Data);
                success: result ? result.success : false,
                output: result ? (result.success ? null : result.errorMessage) : null,
                userMessage: result ? result.errorMessage : null,
                userMessage: 'Execution Error: ' + e.message,
                exception: e
@RegisterClass(QueueBase, 'Entity AI Action', 1)
export class EntityAIActionQueue extends AIActionQueue {
        return this.ProcessGeneric(task, true)
