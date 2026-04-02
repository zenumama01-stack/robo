 * This interface is implemented by openHAB scripts.
@SuppressWarnings("restriction")
public interface Script {
    String SCRIPT_FILEEXT = "script";
     * Executes the script instance and returns the execution result
     * @return the execution result or <code>null</code>, if the script does not have a return value
    Object execute() throws ScriptExecutionException;
     * Executes the script instance with a given evaluation context and returns the execution result
     * @param evaluationContext the evaluation context is a map of variables (name, object)
     *            that should be available during the script execution
    Object execute(IEvaluationContext evaluationContext) throws ScriptExecutionException;
