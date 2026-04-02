 * This handler can evaluate a condition based on a script.
public class ScriptConditionHandler extends AbstractScriptModuleHandler<Condition> implements ConditionHandler {
    public static final String TYPE_ID = "script.ScriptCondition";
    private final Logger logger = LoggerFactory.getLogger(ScriptConditionHandler.class);
    public ScriptConditionHandler(Condition module, String ruleUID, ScriptEngineManager scriptEngineManager) {
    public boolean isSatisfied(final Map<String, Object> context) {
        boolean result = false;
                Object returnVal = eval(scriptEngine);
                if (returnVal instanceof Boolean boolean1) {
                    result = boolean1;
                    logger.error("Script of rule with UID '{}' did not return a boolean value, but '{}'", ruleUID,
                            returnVal);
