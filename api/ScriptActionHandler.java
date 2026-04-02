 * This handler can execute script actions.
 * @author Florian Hotze - Add support for script pre-compilation, Synchronize script context access if the ScriptEngine
 *         implements locking
public class ScriptActionHandler extends AbstractScriptModuleHandler<Action> implements ActionHandler {
    public static final String TYPE_ID = "script.ScriptAction";
    private final Logger logger = LoggerFactory.getLogger(ScriptActionHandler.class);
    private final Consumer<ScriptActionHandler> onRemoval;
     * constructs a new ScriptActionHandler
     * @param module the module
     * @param ruleUID the UID of the rule this handler is used for
     * @param onRemoval called on removal of this script
    public ScriptActionHandler(Action module, String ruleUID, ScriptEngineManager scriptEngineManager,
            Consumer<ScriptActionHandler> onRemoval) {
        super(module, ruleUID, scriptEngineManager);
        this.onRemoval = onRemoval;
        onRemoval.accept(this);
        super.dispose();
    public String getTypeId() {
        return TYPE_ID;
    public void compile() throws ScriptException {
        super.compileScript();
    public @Nullable Map<String, @Nullable Object> execute(final Map<String, Object> context) {
        ScriptEngine scriptEngine = getScriptEngine();
                if (scriptEngine instanceof Lock lock && !lock.tryLock(1, TimeUnit.MINUTES)) {
                            "Failed to acquire lock within one minute for script module '{}' of rule with UID '{}'",
                            module.getId(), ruleUID);
                throw new RuntimeException(e);
                setExecutionContext(scriptEngine, context);
                Object result = eval(scriptEngine);
                resetExecutionContext(scriptEngine, context);
            } finally { // Make sure that Lock is unlocked regardless of an exception being thrown or not to avoid
                        // deadlocks
                if (scriptEngine instanceof Lock lock) {
