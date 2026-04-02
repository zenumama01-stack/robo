 * This class registers an OSGi service for the ScriptExecution action.
public class ScriptExecutionActionService implements ActionService {
    private static @Nullable ScriptExecution scriptExecution;
    public ScriptExecutionActionService(final @Reference ScriptExecution scriptExecution) {
        ScriptExecutionActionService.scriptExecution = scriptExecution;
        return ScriptExecution.class;
    public static ScriptExecution getScriptExecution() {
        return Objects.requireNonNull(scriptExecution);
