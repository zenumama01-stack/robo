 * The {@link ScriptExecution} allows creating timers for asynchronous script execution
public interface ScriptExecution {
     * Schedules a block of code for later execution.
     * @param zonedDateTime the point in time when the code should be executed
     * @param closure the code block to execute
     * @return a handle to the created timer, so that it can be canceled or rescheduled
    Timer createTimer(ZonedDateTime zonedDateTime, Runnable closure);
     * @param identifier an optional identifier
    Timer createTimer(@Nullable String identifier, ZonedDateTime zonedDateTime, Runnable closure);
     * Schedules a block of code (with argument) for later execution
     * @param arg1 the argument to pass to the code block
    Timer createTimerWithArgument(ZonedDateTime zonedDateTime, Object arg1, Consumer<Object> closure);
    Timer createTimerWithArgument(@Nullable String identifier, ZonedDateTime zonedDateTime, Object arg1,
            Consumer<Object> closure);
import org.eclipse.xtext.xbase.lib.Procedures;
import org.openhab.core.model.script.internal.engine.action.ScriptExecutionActionService;
 * The {@link ScriptExecution} is a wrapper for the ScriptExecution actions
public class ScriptExecution {
    private static final Logger logger = LoggerFactory.getLogger(ScriptExecution.class);
     * Calls a script which must be located in the configurations/scripts folder.
     * @param scriptName the name of the script (if the name does not end with
     *            the .script file extension it is added)
     * @return the return value of the script
     * @throws ScriptExecutionException if an error occurs during the execution
    @ActionDoc(text = "call a script file")
    public static Object callScript(String scriptName) throws ScriptExecutionException {
        ModelRepository repo = ScriptServiceUtil.getModelRepository();
        if (repo != null) {
            String scriptNameWithExt = scriptName;
            if (!scriptName.endsWith(Script.SCRIPT_FILEEXT)) {
                scriptNameWithExt = scriptName + "." + Script.SCRIPT_FILEEXT;
            XExpression expr = (XExpression) repo.getModel(scriptNameWithExt);
            if (expr != null) {
                ScriptEngine scriptEngine = ScriptServiceUtil.getScriptEngine();
                    Script script = scriptEngine.newScriptFromXExpression(expr);
                    return script.execute();
                    throw new ScriptExecutionException("Script engine is not available.");
                throw new ScriptExecutionException("Script '" + scriptName + "' cannot be found.");
            throw new ScriptExecutionException("Model repository is not available.");
    @ActionDoc(text = "create a timer")
    public static Timer createTimer(ZonedDateTime zonedDateTime, Procedures.Procedure0 closure) {
        return new Timer(ScriptExecutionActionService.getScriptExecution().createTimer(zonedDateTime, closure::apply));
    @ActionDoc(text = "create an identifiable timer ")
    public static Timer createTimer(@Nullable String identifier, ZonedDateTime zonedDateTime,
            Procedures.Procedure0 closure) {
        return new Timer(ScriptExecutionActionService.getScriptExecution().createTimer(identifier, zonedDateTime,
                closure::apply));
    @ActionDoc(text = "create a timer with argument")
    public static Timer createTimerWithArgument(ZonedDateTime zonedDateTime, Object arg1,
            Procedures.Procedure1 closure) {
        return new Timer(ScriptExecutionActionService.getScriptExecution().createTimerWithArgument(zonedDateTime, arg1,
    @ActionDoc(text = "create an identifiable timer with argument")
    public static Timer createTimerWithArgument(@Nullable String identifier, ZonedDateTime zonedDateTime, Object arg1,
        return new Timer(ScriptExecutionActionService.getScriptExecution().createTimerWithArgument(identifier,
                zonedDateTime, arg1, closure::apply));
