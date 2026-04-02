 * @fileoverview Driver for executing scheduled Action jobs
import { ValidationResult, UserInfo, Metadata, ValidationErrorInfo, ValidationErrorType } from '@memberjunction/core';
    ScheduledJobResult,
    NotificationContent,
    ActionJobConfiguration
} from '@memberjunction/scheduling-base-types';
 * Driver for executing scheduled Action jobs
 * Configuration schema (stored in ScheduledJob.Configuration):
 *   ActionID: string,
 *   Params?: Array<{
 *     ActionParamID: string,
 *     ValueType: 'Static' | 'SQL Statement',
 *     Value: string
 *   }>
 * Execution result details (stored in ScheduledJobRun.Details):
 *   ResultCode: string,
 *   IsSuccess: boolean,
 *   OutputParams?: any
@RegisterClass(BaseScheduledJob, 'ActionScheduledJobDriver')
export class ActionScheduledJobDriver extends BaseScheduledJob {
    public async Execute(context: ScheduledJobExecutionContext): Promise<ScheduledJobResult> {
        const config = this.parseConfiguration<ActionJobConfiguration>(context.Schedule);
        // Load the action
        await ActionEngineServer.Instance.Config(false, context.ContextUser);
        const action = ActionEngineServer.Instance.Actions.find(a => a.ID === config.ActionID);
            throw new Error(`Action with ID ${config.ActionID} not found`);
        this.log(`Executing action: ${action.Name}`);
        // Process parameters (static values or SQL queries)
        const params = await this.processParams(config.Params || [], context.ContextUser);
        const actionResult = await ActionEngineServer.Instance.RunAction({
            ContextUser: context.ContextUser,
            Success: actionResult.Success,
            ErrorMessage: actionResult.Message || undefined,
                ResultCode: actionResult.Result?.ResultCode,
                IsSuccess: actionResult.Success,
                OutputParams: actionResult.Params
    public ValidateConfiguration(schedule: any): ValidationResult {
            const config = this.parseConfiguration<ActionJobConfiguration>(schedule);
            if (!config.ActionID) {
                    'Configuration.ActionID',
                    'ActionID is required',
                    config.ActionID,
            // Validate params structure
            if (config.Params) {
                if (!Array.isArray(config.Params)) {
                        'Configuration.Params',
                        'Params must be an array',
                        config.Params,
                    for (let i = 0; i < config.Params.length; i++) {
                        const param = config.Params[i];
                        if (!param.ActionParamID) {
                                `Configuration.Params[${i}].ActionParamID`,
                                'ActionParamID is required',
                                param.ActionParamID,
                        if (!param.ValueType || !['Static', 'SQL Statement'].includes(param.ValueType)) {
                                `Configuration.Params[${i}].ValueType`,
                                'ValueType must be "Static" or "SQL Statement"',
                                param.ValueType,
            const errorMessage = error instanceof Error ? error.message : 'Invalid configuration';
                'Configuration',
                schedule.Configuration,
        result.Success = result.Errors.length === 0;
    public FormatNotification(
    ): NotificationContent {
        const details = result.Details as any;
        const subject = result.Success
            ? `Scheduled Action Completed: ${context.Schedule.Name}`
            : `Scheduled Action Failed: ${context.Schedule.Name}`;
        const body = result.Success
            ? `The scheduled action "${context.Schedule.Name}" completed successfully.\n\n` +
              `Result Code: ${details?.ResultCode || 'N/A'}`
            : `The scheduled action "${context.Schedule.Name}" failed.\n\n` +
              `Error: ${result.ErrorMessage}`;
            Subject: subject,
            Priority: result.Success ? 'Normal' : 'High',
                ScheduleID: context.Schedule.ID,
                JobType: 'Action',
                ResultCode: details?.ResultCode
    private async processParams(
        params: ActionJobConfiguration['Params'],
    ): Promise<ActionParam[]> {
        const allActionParams = ActionEngineServer.Instance.ActionParams;
        const result: ActionParam[] = [];
            const actionParam = allActionParams.find(p => p.ID === param.ActionParamID);
            if (!actionParam) {
                this.logError(`Action param ${param.ActionParamID} not found`);
                    // Value could be scalar or JSON
                    const jsonValue = SafeJSONParse(param.Value);
                    value = jsonValue !== null ? jsonValue : param.Value;
                    value = await this.executeSQL(param.Value);
                Name: actionParam.Name,
                Type: actionParam.Type
    private async executeSQL(sql: string): Promise<any> {
            const sqlProvider = Metadata.Provider as SQLServerDataProvider;
            this.logError(`Error executing SQL: ${sql}`, error);
