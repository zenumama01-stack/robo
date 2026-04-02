 * Execute Code Action
 * Thin wrapper action that exposes code execution capabilities to AI agents and workflows.
 * Delegates all logic to CodeExecutionService following MJ's pattern of Actions as boundaries.
 * @module @memberjunction/core-actions
import { RunActionParams, ActionResultSimple } from '@memberjunction/actions-base';
    CodeExecutionService,
    CodeExecutionParams,
    CodeExecutionResult
} from '@memberjunction/code-execution';
 * Action for executing JavaScript code in a sandboxed environment
 * This is a THIN WRAPPER - all core logic lives in CodeExecutionService.
 * The action's job is simply to:
 * 1. Extract parameters from the action interface
 * 2. Delegate to CodeExecutionService
 * 3. Map results back to ActionResultSimple
 * Security is enforced entirely by CodeExecutionService.
 * @example Agent usage:
 *   "type": "Action",
 *   "action": {
 *     "name": "Execute Code",
 *     "params": {
 *       "code": "const sum = input.values.reduce((a,b) => a+b, 0); output = sum;",
 *       "language": "javascript",
 *       "inputData": "{\"values\": [1,2,3,4,5]}"
@RegisterClass(BaseAction, "__ExecuteCode")
export class ExecuteCodeAction extends BaseAction {
     * Execute the action by delegating to CodeExecutionService
     * @param params - Action parameters including code, language, inputData
     * @returns ActionResultSimple with execution results or errors
            // 1. Extract parameters (this is all the action does besides delegation)
            const code = this.getStringParam(params, "code");
            const language = this.getStringParam(params, "language", "javascript") as 'javascript';
            const inputDataStr = this.getStringParam(params, "inputData");
            const timeout = this.getNumericParam(params, "timeout", 30);
            const memoryLimit = this.getNumericParam(params, "memoryLimit", 128);
                    ResultCode: "MISSING_CODE",
                    Message: "Parameter 'code' is required"
            // Parse input data if provided
            let inputData: any = undefined;
            if (inputDataStr) {
                    inputData = JSON.parse(inputDataStr);
                        ResultCode: "INVALID_INPUT_DATA",
                        Message: `Failed to parse inputData as JSON: ${parseError instanceof Error ? parseError.message : String(parseError)}`
            // 2. Delegate to service (DIRECT IMPORT - not another action!)
            const executionService = new CodeExecutionService();
            const executionParams: CodeExecutionParams = {
                timeoutSeconds: timeout,
                memoryLimitMB: memoryLimit
            const result: CodeExecutionResult = await executionService.execute(executionParams);
            // 3. Map results to action format
                    ResultCode: result.errorType || "EXECUTION_FAILED",
                    Message: result.error || "Code execution failed"
            // Add output parameters for agents to use
            if (result.output !== undefined) {
                this.addOutputParam(params, "output", result.output);
                this.addOutputParam(params, "logs", result.logs);
            if (result.executionTimeMs !== undefined) {
                this.addOutputParam(params, "executionTimeMs", result.executionTimeMs);
                Message: JSON.stringify({
                    output: result.output,
                    logs: result.logs,
                    executionTimeMs: result.executionTimeMs
            LogError(`Error in ExecuteCodeAction: ${error}`);
                ResultCode: "UNEXPECTED_ERROR",
                Message: error instanceof Error ? error.message : String(error)
     * Helper to get string parameter from action params
    private getStringParam(params: RunActionParams, name: string, defaultValue?: string): string | undefined {
        const param = params.Params.find(p => p.Name.trim().toLowerCase() === name.toLowerCase());
     * Helper to get numeric parameter from action params
    private getNumericParam(params: RunActionParams, name: string, defaultValue: number): number {
     * Helper to add output parameter to action results
