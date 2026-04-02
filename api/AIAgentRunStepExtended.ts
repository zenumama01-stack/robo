import { MJActionExecutionLogEntity, MJAIAgentRunStepEntity, MJAIPromptRunEntity } from "@memberjunction/core-entities";
import { AIAgentRunEntityExtended } from "./AIAgentRunExtended";
@RegisterClass(BaseEntity, "MJ: AI Agent Run Steps")
export class AIAgentRunStepEntityExtended extends MJAIAgentRunStepEntity {
    private _actionExecutionLog?: MJActionExecutionLogEntity;
     * If StepType == 'Actions', this property can be used to stash the MJActionExecutionLogEntity
     * which contains the execution log of the action that was executed in this step.
     * This is useful for debugging and tracking the execution of actions within the agent run step.
     * NOTE: This property is only applicable when StepType is 'Actions'.
    public get ActionExecutionLog(): MJActionExecutionLogEntity | undefined {
        return this._actionExecutionLog;
    public set ActionExecutionLog(value: MJActionExecutionLogEntity | undefined) {
        this._actionExecutionLog = value;
    private _subAgentRun?: AIAgentRunEntityExtended;
     * If StepType == 'Sub-Agent', this property can be used to stash the AIAgentRunEntityExtended
     * which contains the sub-agent run details.
     * This is useful for tracking the execution of sub-agents within the agent run step.
     * NOTE: This property is only applicable when StepType is 'Sub-Agent'.
    public get SubAgentRun(): AIAgentRunEntityExtended | undefined {
        return this._subAgentRun;
    public set SubAgentRun(value: AIAgentRunEntityExtended | undefined) {
        this._subAgentRun = value;
    private _promptRun?: MJAIPromptRunEntity;
     * If StepType == 'Prompt', this property can be used to stash the MJAIPromptRunEntity
     * which contains the prompt run details.
     * This is useful for tracking the execution of prompts within the agent run step.
     * NOTE: This property is only applicable when StepType is 'Prompt'.
    public get PromptRun(): MJAIPromptRunEntity | undefined {
        return this._promptRun;
    public set PromptRun(value: MJAIPromptRunEntity | undefined) {
        this._promptRun = value;
    public override GetAll(): any {
        // Add related entities based on StepType with __ prefix
        const extended: any = { ...baseData };
        if (this.StepType === 'Prompt' && this._promptRun) {
            extended.__promptRun = this._promptRun.GetAll();
        } else if (this.StepType === 'Actions' && this._actionExecutionLog) {
            extended.__actionExecutionLog = this._actionExecutionLog.GetAll();
        } else if (this.StepType === 'Sub-Agent' && this._subAgentRun) {
            extended.__subAgentRun = this._subAgentRun.GetAll(); // Recursive!
        return extended;
    public override async LoadFromData(data: any, _replaceOldValues?: boolean): Promise<boolean> {
        const { __promptRun, __actionExecutionLog, __subAgentRun, ...baseData } = data;
        // Handle extended properties based on StepType
        if (this.StepType === 'Prompt' && __promptRun) {
            this._promptRun = await md.GetEntityObject<MJAIPromptRunEntity>('MJ: AI Prompt Runs', this.ContextCurrentUser);
            await this._promptRun.LoadFromData(__promptRun);
        } else if (this.StepType === 'Actions' && __actionExecutionLog) {
            this._actionExecutionLog = await md.GetEntityObject<MJActionExecutionLogEntity>('MJ: Action Execution Logs', this.ContextCurrentUser);
            await this._actionExecutionLog.LoadFromData(__actionExecutionLog);
        } else if (this.StepType === 'Sub-Agent' && __subAgentRun) {
            this._subAgentRun = await md.GetEntityObject<AIAgentRunEntityExtended>('MJ: AI Agent Runs', this.ContextCurrentUser);
            await this._subAgentRun.LoadFromData(__subAgentRun);
        } else if (this.ID?.length > 0) {
    public override async InnerLoad(CompositeKey: CompositeKey, EntityRelationshipsToLoad?: string[]): Promise<boolean> {
     * Based on the step type, for an existing record, it will load the related records automatically
            if (this.ID?.length > 0 && this.TargetLogID?.length > 0) {
                switch (this.StepType) {
                    case "Actions":
                        this._actionExecutionLog = await md.GetEntityObject<MJActionExecutionLogEntity>("MJ: Action Execution Logs", this.ContextCurrentUser);
                        await this._actionExecutionLog.Load(this.TargetLogID);
                    case "Sub-Agent":
                        this._subAgentRun = await md.GetEntityObject<AIAgentRunEntityExtended>("MJ: AI Agent Runs", this.ContextCurrentUser);
                        await this._subAgentRun.Load(this.TargetLogID);
                    case "Prompt":
                        this._promptRun = await md.GetEntityObject<MJAIPromptRunEntity>("MJ: AI Prompt Runs", this.ContextCurrentUser);
                        await this._promptRun.Load(this.TargetLogID);
                        // no related data to load for other step types
                message: `Error loading related data for AI Agent Run Step ID: ${this.ID}, Step Type: ${this.StepType}`,
