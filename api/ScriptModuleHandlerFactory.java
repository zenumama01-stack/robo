package org.openhab.core.automation.module.script.internal.factory;
import org.openhab.core.automation.module.script.internal.handler.ScriptActionHandler;
import org.openhab.core.automation.module.script.internal.handler.ScriptConditionHandler;
 * This HandlerFactory creates ModuleHandlers for scripts.
@Component(service = { ModuleHandlerFactory.class, ScriptDependencyTracker.Listener.class })
public class ScriptModuleHandlerFactory extends BaseModuleHandlerFactory implements ScriptDependencyTracker.Listener {
    private final Logger logger = LoggerFactory.getLogger(ScriptModuleHandlerFactory.class);
    private static final Collection<String> TYPES = List.of(ScriptActionHandler.TYPE_ID,
            ScriptConditionHandler.TYPE_ID);
    private @NonNullByDefault({}) ScriptEngineManager scriptEngineManager;
    private Map<String, ScriptActionHandler> trackedHandlers = new ConcurrentHashMap<>();
        logger.trace("create {} -> {}", module.getId(), module.getTypeUID());
        String moduleTypeUID = module.getTypeUID();
        if (ScriptConditionHandler.TYPE_ID.equals(moduleTypeUID) && module instanceof Condition condition) {
            return new ScriptConditionHandler(condition, ruleUID, scriptEngineManager);
        } else if (ScriptActionHandler.TYPE_ID.equals(moduleTypeUID) && module instanceof Action action) {
            ScriptActionHandler handler = new ScriptActionHandler(action, ruleUID, scriptEngineManager,
                    this::onHandlerRemoval);
            trackedHandlers.put(handler.getEngineIdentifier(), handler);
            return handler;
            logger.error("The ModuleHandler is not supported: {}", moduleTypeUID);
    @Reference(policy = ReferencePolicy.DYNAMIC)
    public void setScriptEngineManager(ScriptEngineManager scriptEngineManager) {
    public void unsetScriptEngineManager(ScriptEngineManager scriptEngineManager) {
        this.scriptEngineManager = null;
    private void onHandlerRemoval(ScriptActionHandler handler) {
        trackedHandlers.values().remove(handler);
    public void onDependencyChange(String engineIdentifier) {
        ScriptActionHandler handler = trackedHandlers.get(engineIdentifier);
            logger.debug("Resetting script engine for script {}", engineIdentifier);
            handler.resetScriptEngine();
                handler.compile();
                logger.error("Failed to recompile script for rule {}: {}", handler.getRuleUID(), e.getMessage());
