 * Action that executes different actions based on a condition
 * // Simple condition with action execution
 *   ActionName: 'Conditional',
 *     Name: 'Condition',
 *     Value: 'value > 100'
 *     Name: 'Context',
 *     Value: { value: 150 }
 *     Name: 'TrueAction',
 *       ActionName: 'Send Single Message',
 *       Params: {
 *         MessageTypeID: 'alert-id',
 *         To: 'admin@example.com',
 *         Subject: 'High Value Alert'
 *     Name: 'FalseAction',
 *       ActionName: 'Calculate Expression',
 *       Params: { Expression: 'value * 0.9', value: 150 }
 * // With passthrough context
 *     Value: 'user.role === "admin"'
 *     Value: { user: { role: 'admin', id: 123 } }
 *     Name: 'PassthroughContext',
 *       ActionName: 'Get Record',
 *       Params: { EntityName: 'Admin Settings' }
@RegisterClass(BaseAction, "Conditional")
export class ConditionalAction extends BaseAction {
     * Executes actions based on a condition
     *   - Condition: JavaScript expression string to evaluate (required)
     *   - Context: Object with variables for condition evaluation (optional)
     *   - TrueAction: Action configuration to run if condition is true (optional)
     *   - FalseAction: Action configuration to run if condition is false (optional)
     *   - PassthroughContext: Boolean - pass context to child actions (default: false)
     *   - StrictMode: Boolean - use strict mode for evaluation (default: true)
     * @returns Result from executed action or condition result
            const condition = this.getParamValue(params, 'condition');
            const context = JSONParamHelper.getJSONParam(params, 'context') || {};
            const trueAction = JSONParamHelper.getJSONParam(params, 'trueaction');
            const falseAction = JSONParamHelper.getJSONParam(params, 'falseaction');
            const passthroughContext = this.getBooleanParam(params, 'passthroughcontext', false);
            const strictMode = this.getBooleanParam(params, 'strictmode', true);
            // Validate condition
            if (!condition) {
                    Message: "Condition parameter is required",
                    ResultCode: "MISSING_CONDITION"
            // Evaluate condition
            let conditionResult: boolean;
                conditionResult = this.evaluateCondition(condition, context, strictMode);
                    Message: `Failed to evaluate condition: ${error instanceof Error ? error.message : String(error)}`,
                    ResultCode: "CONDITION_ERROR"
            // Add output parameter for condition result
                Name: 'ConditionResult',
                Value: conditionResult
            // Determine which action to run
            const actionToRun = conditionResult ? trueAction : falseAction;
            if (!actionToRun) {
                // No action to run, just return condition result
                    ResultCode: conditionResult ? "CONDITION_TRUE" : "CONDITION_FALSE",
                        message: `Condition evaluated to ${conditionResult}`,
                        condition: condition,
                        result: conditionResult,
                        context: context
            // Prepare action parameters
            const actionConfig = this.prepareActionConfig(actionToRun, context, passthroughContext);
                const actionResult = await engine.RunAction(actionConfig);
                // Add output parameters from child action
                    Name: 'ChildActionResult',
                    Value: actionResult
                if (actionResult.Success) {
                        ResultCode: conditionResult ? "TRUE_ACTION_SUCCESS" : "FALSE_ACTION_SUCCESS",
                            message: `${conditionResult ? 'True' : 'False'} action executed successfully`,
                            conditionResult: conditionResult,
                            actionName: actionConfig.ActionName,
                            actionResult: actionResult
                        ResultCode: conditionResult ? "TRUE_ACTION_FAILED" : "FALSE_ACTION_FAILED",
                            message: `${conditionResult ? 'True' : 'False'} action failed`,
                            actionError: actionResult.Message
                    Message: `Failed to execute action: ${error instanceof Error ? error.message : String(error)}`,
                    ResultCode: "ACTION_EXECUTION_ERROR"
                Message: `Conditional action failed: ${error instanceof Error ? error.message : String(error)}`,
                ResultCode: "CONDITIONAL_FAILED"
     * Evaluate condition safely
    private evaluateCondition(condition: string, context: any, strictMode: boolean): boolean {
        // Create a safe evaluation context
        const safeContext = { ...context };
        // Build the evaluation function
        const functionBody = strictMode 
            ? `"use strict"; return (${condition});`
            : `return (${condition});`;
        // Create function with context variables
        const contextKeys = Object.keys(safeContext);
        const contextValues = contextKeys.map(key => safeContext[key]);
            const evaluator = new Function(...contextKeys, functionBody);
            const result = evaluator(...contextValues);
            // Ensure boolean result
            return Boolean(result);
            throw new Error(`Invalid condition: ${error instanceof Error ? error.message : String(error)}`);
     * Prepare action configuration
    private prepareActionConfig(actionConfig: any, context: any, passthroughContext: boolean): any {
        // Validate action config
        if (!actionConfig || typeof actionConfig !== 'object') {
            throw new Error('Invalid action configuration');
        if (!actionConfig.ActionName) {
            throw new Error('Action configuration must include ActionName');
        // Prepare parameters
        let preparedConfig = { ...actionConfig };
        if (passthroughContext && context) {
            // Merge context into action parameters
            if (!preparedConfig.Params) {
                preparedConfig.Params = {};
            // If Params is an array, convert to object
            if (Array.isArray(preparedConfig.Params)) {
                const paramsObj: any = {};
                preparedConfig.Params.forEach((param: any) => {
                    if (param.Name) {
                        paramsObj[param.Name] = param.Value;
                preparedConfig.Params = paramsObj;
            // Merge context (context takes precedence over existing params)
            preparedConfig.Params = { ...preparedConfig.Params, ...context };
        return preparedConfig;
