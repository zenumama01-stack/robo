package org.openhab.core.automation.module.script.internal.handler;
import org.openhab.core.automation.handler.BaseModuleHandler;
 * This is an abstract class that can be used when implementing any module handler that handles scripts.
 * Remember to implement multi-thread synchronization in the concrete handler if the script engine is not thread-safe!
 * @author Florian Hotze - Add support for script pre-compilation
 * @param <T> the type of module the concrete handler can handle
public abstract class AbstractScriptModuleHandler<T extends Module> extends BaseModuleHandler<T> {
    private final Logger logger = LoggerFactory.getLogger(AbstractScriptModuleHandler.class);
    /** Constant defining the configuration parameter of modules that specifies the mime type of a script */
    public static final String CONFIG_SCRIPT_TYPE = "type";
    /** Constant defining the configuration parameter of modules that specifies the script itself */
    public static final String CONFIG_SCRIPT = "script";
     * Constant defining the context key of the module type id.
    public static final String CONTEXT_KEY_MODULE_TYPE_ID = "oh.module-type-id";
    protected final ScriptEngineManager scriptEngineManager;
    private final String engineIdentifier;
    private @Nullable ScriptEngine scriptEngine = null;
    private @Nullable CompiledScript compiledScript = null;
    protected final String script;
    protected final String ruleUID;
    protected AbstractScriptModuleHandler(T module, String ruleUID, ScriptEngineManager scriptEngineManager) {
        this.ruleUID = ruleUID;
        this.engineIdentifier = UUID.randomUUID().toString();
        this.type = getValidConfigParameter(CONFIG_SCRIPT_TYPE, module.getConfiguration(), module.getId(), false);
        this.script = getValidConfigParameter(CONFIG_SCRIPT, module.getConfiguration(), module.getId(), true);
    private static String getValidConfigParameter(String parameter, Configuration config, String moduleId,
            boolean emptyAllowed) {
        Object value = config.get(parameter);
        if (value instanceof String string && (emptyAllowed || !string.trim().isEmpty())) {
            return string;
            throw new IllegalStateException(String.format(
                    "Config parameter '%s' is missing in the configuration of module '%s'.", parameter, moduleId));
     * Creates the {@link ScriptEngine} and compiles the script if the {@link ScriptEngine} implements
     * {@link Compilable}.
    protected void compileScript() throws ScriptException {
        if (compiledScript != null || script.isEmpty()) {
        if (!scriptEngineManager.isSupported(type)) {
                    "ScriptEngine for language '{}' could not be found, skipping compilation of script for identifier: {}",
                    type, engineIdentifier);
        ScriptEngine engine = getScriptEngine();
            if (engine instanceof Compilable compilable) {
                logger.debug("Pre-compiling script of rule with UID '{}'", ruleUID);
                compiledScript = compilable.compile(script);
        scriptEngineManager.removeEngine(engineIdentifier);
     * Reset the script engine to force a script reload
    public synchronized void resetScriptEngine() {
        scriptEngine = null;
        compiledScript = null;
     * Gets the unique identifier of the rule this module handler is used for.
     * @return the UID of the rule
    public String getRuleUID() {
        return ruleUID;
     * Gets the type identifier of this module handler
     * @return the type identifier
    public abstract String getTypeId();
     * Gets the script engine identifier for this module
     * @return the engine identifier string
    public String getEngineIdentifier() {
        return engineIdentifier;
     * Get the script engine instance used by this module handler.
     * @return the script engine instance if available, otherwise null
    protected @Nullable ScriptEngine getScriptEngine() {
        return scriptEngine != null ? scriptEngine : createScriptEngine();
     * Creates a new script engine for the type defined in the module configuration.
     * @return the script engine if available, otherwise null
    private @Nullable ScriptEngine createScriptEngine() {
        ScriptEngineContainer container = scriptEngineManager.createScriptEngine(type, engineIdentifier);
            scriptEngine = container.getScriptEngine();
            // Inject the module type id into the script context early, so engines can access it before script
            // invocation.
            ScriptContext scriptContext = container.getScriptEngine().getContext();
                        "Script context is null for script engine '{}' of rule with UID '{}'. Please report this bug.",
                        engineIdentifier, ruleUID);
                scriptContext.setAttribute(CONTEXT_KEY_MODULE_TYPE_ID, getTypeId(), ScriptContext.ENGINE_SCOPE);
            logger.debug("No engine available for script type '{}' in action '{}'.", type, module.getId());
     * Adds the passed context variables of the rule engine to the context scope of the ScriptEngine
     * this should be done each time the module is executed to prevent leaking context to later executions
     * @param engine the script engine that is used
     * @param context the variables and types to remove from the execution context
    protected void setExecutionContext(ScriptEngine engine, Map<String, ?> context) {
        // Add the rule's UID to the context and make it available as "ctx".
        // Note: We don't use "context" here as it doesn't work on all JVM versions!
        final Map<String, Object> contextNew = new HashMap<>();
        // add the single context entries without their prefix to contextNew
        for (Entry<String, ?> entry : context.entrySet()) {
            Object value = entry.getValue();
            String key = entry.getKey();
                key = key.substring(dotIndex + 1);
            contextNew.put(key, value);
        contextNew.put("ruleUID", this.ruleUID);
        executionContext.setAttribute("ctx", contextNew, ScriptContext.ENGINE_SCOPE);
        // add the single contextNew entries to the scope
        for (Entry<String, ?> entry : contextNew.entrySet()) {
            executionContext.setAttribute(key, value, ScriptContext.ENGINE_SCOPE);
     * Removes passed context variables of the rule engine from the context scope of the ScriptEngine, this should be
     * updated each time the module is executed
     * @param context the variables and types to put into the execution context
    protected void resetExecutionContext(ScriptEngine engine, Map<String, ?> context) {
            executionContext.removeAttribute(key, ScriptContext.ENGINE_SCOPE);
     * Evaluates the script with the given script engine.
     * @return the value returned from the execution of the script
    protected @Nullable Object eval(ScriptEngine engine) {
        if (script.isEmpty()) {
            if (compiledScript != null) {
                logger.debug("Executing pre-compiled script of rule with UID '{}'", ruleUID);
                return compiledScript.eval(engine.getContext());
            logger.debug("Executing script of rule with UID '{}'", ruleUID);
            return engine.eval(script);
            logger.error("Script execution of rule with UID '{}' failed: {}", ruleUID, e.getMessage(),
                    logger.isDebugEnabled() ? e : null);
