import { BaseEngine, BaseEnginePropertyConfig, IMetadataProvider, LogError, Metadata, UserInfo } from "@memberjunction/core";
import { ScheduledActionEntityExtended, MJScheduledActionParamEntity } from "@memberjunction/core-entities";
import { ActionEntityExtended, ActionParam, ActionResult, RunActionParams } from "@memberjunction/actions-base";
import cronParser from 'cron-parser';
import { SafeJSONParse } from "@memberjunction/global";
 * ScheduledActionEngine handles metadata caching and execution of scheduled actions based on their defined CronExpressions
export class ScheduledActionEngine extends BaseEngine<ScheduledActionEngine> {
                EntityName: 'MJ: Scheduled Actions',
                PropertyName: '_scheduledActions',
                EntityName: 'MJ: Scheduled Action Params',
                PropertyName: '_scheduledActionParams',
        return await this.Load(configs, provider, forceRefresh, contextUser);
    protected override AdditionalLoading(contextUser?: UserInfo): Promise<void> {
        // associate params with actions
        this.ScheduledActions.forEach(scheduledAction => {
            scheduledAction.Params = this.ScheduledActionParams.filter(param => param.ScheduledActionID === scheduledAction.ID);
    private _scheduledActions: ScheduledActionEntityExtended[] = [];
    public get ScheduledActions(): ScheduledActionEntityExtended[] {
        return this._scheduledActions;
    private _scheduledActionParams: MJScheduledActionParamEntity[] = [];
    public get ScheduledActionParams(): MJScheduledActionParamEntity[] {
        return this._scheduledActionParams;
    public static get Instance(): ScheduledActionEngine {
        return super.getInstance<ScheduledActionEngine>();
     * This method executes all scheduled actions that are due to be executed based on their CronExpressions and returns
     * an array of zero to many ActionResult objects.
    public async ExecuteScheduledActions(contextUser: UserInfo): Promise<ActionResult[]> {
        await this.Config(false, contextUser);
        const results: ActionResult[] = [];
        for (const scheduledAction of this.ScheduledActions) {
            if (ScheduledActionEngine.IsActionDue(scheduledAction, now)) {
                const action: ActionEntityExtended = ActionEngineServer.Instance.Actions.find(a => a.ID === scheduledAction.ActionID);
                const params: ActionParam[] = await this.MapScheduledActionParamsToActionParams(scheduledAction);
                    Params: params
     public async ExecuteScheduledAction(actionName: string, contextUser: UserInfo): Promise<ActionResult> {
        const scheduledAction = this.ScheduledActions.find(sa => sa.Name === actionName);
        if(!scheduledAction) {
            throw new Error(`Scheduled action ${actionName} not found`);
        //since CronExpresssion is optional, only check if its populated
        const canRun: boolean = scheduledAction.CronExpression ? ScheduledActionEngine.IsActionDue(scheduledAction, now) : true;
        if (canRun) {
    protected async MapScheduledActionParamsToActionParams(scheduledAction: ScheduledActionEntityExtended): Promise<ActionParam[]> {
        const allParams = ActionEngineServer.Instance.ActionParams;
        for (const sap of scheduledAction.Params) {
            const param = allParams.find(p => p.ID === sap.ActionParamID);
            switch (sap.ValueType) {
                    const jsonValue = SafeJSONParse(sap.Value);
                        value = sap.Value;
                case 'SQL Statement':
                    value = await this.ExecuteSQL(sap.Value);
    protected async ExecuteSQL(sql: string): Promise<any> {
        // execute the SQL and return the result
            const sqlProvider = <SQLServerDataProvider>Metadata.Provider;
            const result = await sqlProvider.ExecuteSQL(sql);
            LogError('Error executing SQL: ' + sql);
    public static IsActionDue(scheduledAction: ScheduledActionEntityExtended, evalTime: Date): boolean {
        // get the cron expression from the scheduled action and evaluate it against the evalTime
        const cronExpression = scheduledAction.CronExpression;
        // evaluate the cron expression
            const interval = cronParser.parseExpression(cronExpression, { currentDate: evalTime });
            const nextExecution = interval.next().toDate();
            return nextExecution <= evalTime;
            console.error('Error parsing cron expression:', err);
