import static org.openhab.core.automation.module.script.ScriptEngineFactory.*;
import javax.script.Invocable;
import javax.script.SimpleScriptContext;
import org.openhab.core.automation.module.script.ScriptExtensionManagerWrapper;
 * @author Scott Rushworth - replaced GenericScriptEngineFactory with a service and cleaned up logging
 * @author Jonathan Gilbert - included passing of context to script engines
@Component(service = ScriptEngineManager.class)
public class ScriptEngineManagerImpl implements ScriptEngineManager {
    private final Logger logger = LoggerFactory.getLogger(ScriptEngineManagerImpl.class);
    private final Map<String, ScriptEngineContainer> loadedScriptEngineInstances = new HashMap<>();
    private final Map<String, ScriptEngineFactory> factories = new HashMap<>();
    private final ScriptExtensionManager scriptExtensionManager;
    private final Set<FactoryChangeListener> listeners = new HashSet<>();
    public ScriptEngineManagerImpl(final @Reference ScriptExtensionManager scriptExtensionManager) {
        this.scriptExtensionManager = scriptExtensionManager;
    public void addScriptEngineFactory(ScriptEngineFactory engineFactory) {
        logger.trace("{}.getScriptTypes(): {}", engineFactory.getClass().getSimpleName(), scriptTypes);
        for (String scriptType : scriptTypes) {
            factories.put(scriptType, engineFactory);
            listeners.forEach(listener -> listener.factoryAdded(scriptType));
                    javax.script.ScriptEngineFactory factory = scriptEngine.getFactory();
                            "Initialized a ScriptEngineFactory for {} ({}): supports {} ({}) with file extensions {}, names {}, and mimetypes {}",
                            factory.getEngineName(), factory.getEngineVersion(), factory.getLanguageName(),
                            factory.getLanguageVersion(), factory.getExtensions(), factory.getNames(),
                            factory.getMimeTypes());
                    logger.trace("addScriptEngineFactory: engine was null");
                logger.trace("addScriptEngineFactory: scriptTypes was empty");
    public void removeScriptEngineFactory(ScriptEngineFactory engineFactory) {
            factories.remove(scriptType, engineFactory);
            listeners.forEach(listener -> listener.factoryRemoved(scriptType));
        logger.debug("Removed {}", engineFactory.getClass().getSimpleName());
    public @Nullable ScriptEngineContainer createScriptEngine(String scriptType, String engineIdentifier) {
        ScriptEngineContainer result = null;
        ScriptEngineFactory engineFactory = findEngineFactory(scriptType);
        if (loadedScriptEngineInstances.containsKey(engineIdentifier)) {
            removeEngine(engineIdentifier);
        if (engineFactory == null) {
            logger.error("ScriptEngine for language '{}' could not be found for identifier: {}", scriptType,
                    engineIdentifier);
                ScriptEngine engine = engineFactory.createScriptEngine(scriptType);
                if (engine != null) {
                    Map<String, Object> scriptExManager = new HashMap<>();
                    result = new ScriptEngineContainer(engine, engineFactory, engineIdentifier);
                    ScriptExtensionManagerWrapper wrapper = new ScriptExtensionManagerWrapperImpl(
                            scriptExtensionManager, result);
                    scriptExManager.put("scriptExtension", wrapper);
                    scriptExManager.put("se", wrapper);
                    engineFactory.scopeValues(engine, scriptExManager);
                    scriptExtensionManager.importDefaultPresets(engineFactory, engine, engineIdentifier);
                    loadedScriptEngineInstances.put(engineIdentifier, result);
                    logger.debug("Added ScriptEngine for language '{}' with identifier: {}", scriptType,
                    addAttributeToScriptContext(engine, CONTEXT_KEY_ENGINE_IDENTIFIER, engineIdentifier);
                    addAttributeToScriptContext(engine, CONTEXT_KEY_EXTENSION_ACCESSOR, scriptExtensionManager);
                    ScriptDependencyTracker tracker = engineFactory.getDependencyTracker();
                        addAttributeToScriptContext(engine, CONTEXT_KEY_DEPENDENCY_LISTENER,
                                tracker.getTracker(engineIdentifier));
                    logger.error("ScriptEngine for language '{}' could not be created for identifier: {}", scriptType,
                logger.error("Error while creating ScriptEngine", ex);
                removeScriptExtensions(engineIdentifier);
    public boolean loadScript(String engineIdentifier, InputStreamReader scriptData) {
        ScriptEngineContainer container = loadedScriptEngineInstances.get(engineIdentifier);
        if (container == null) {
            logger.error("Could not load script, as no ScriptEngine has been created");
            ScriptEngine engine = container.getScriptEngine();
                engine.eval(scriptData);
                if (engine instanceof Invocable inv) {
                        inv.invokeFunction("scriptLoaded", engineIdentifier);
                    } catch (NoSuchMethodException e) {
                        logger.trace("scriptLoaded() is not defined in the script: {}", engineIdentifier);
                    logger.trace("ScriptEngine does not support Invocable interface");
                logger.error("Error during evaluation of script '{}': {}", engineIdentifier, ex.getMessage());
                // Only call logger if debug level is actually enabled, because OPS4J Pax Logging holds (at least for
                // some time) a reference to the exception and its cause, which may hold a reference to the script
                // engine.
                // This prevents garbage collection (at least for some time) to remove the script engine from heap.
                    logger.debug("", ex);
    public void removeEngine(String engineIdentifier) {
        ScriptEngineContainer container = loadedScriptEngineInstances.remove(engineIdentifier);
            ScriptDependencyTracker tracker = container.getFactory().getDependencyTracker();
                tracker.removeTracking(engineIdentifier);
            ScriptEngine scriptEngine = container.getScriptEngine();
            if (scriptEngine instanceof Invocable inv) {
                    inv.invokeFunction("scriptUnloaded");
                    logger.trace("scriptUnloaded() is not defined in the script");
                } catch (ScriptException ex) {
                    logger.error("Error while executing script", ex);
            if (scriptEngine instanceof AutoCloseable closeable) {
                // we cannot not use ScheduledExecutorService.execute here as it might execute the task in the calling
                // thread (calling ScriptEngine.close in the same thread may result in a deadlock if the ScriptEngine
                // tries to Thread.join)
                scheduler.schedule(() -> {
                        closeable.close();
                        logger.error("Error while closing script engine", e);
                }, 0, TimeUnit.SECONDS);
                logger.trace("ScriptEngine does not support AutoCloseable interface");
    private void removeScriptExtensions(String pathIdentifier) {
            scriptExtensionManager.dispose(pathIdentifier);
            logger.error("Error removing ScriptEngine", ex);
     * This method will find and return a {@link ScriptEngineFactory} capable of executing a script of the given type,
     * if one exists. Custom ScriptEngineFactories are preferred over generic.
     * @return {@link ScriptEngineFactory} or null
    private @Nullable ScriptEngineFactory findEngineFactory(String scriptType) {
        return factories.get(scriptType);
    public boolean isSupported(String scriptType) {
        return findEngineFactory(scriptType) != null;
    private void addAttributeToScriptContext(ScriptEngine engine, String name, Object value) {
        ScriptContext scriptContext = engine.getContext();
        if (scriptContext == null) {
            scriptContext = new SimpleScriptContext();
            engine.setContext(scriptContext);
        scriptContext.setAttribute(name, value, ScriptContext.ENGINE_SCOPE);
    public void addFactoryChangeListener(FactoryChangeListener listener) {
    public void removeFactoryChangeListener(FactoryChangeListener listener) {
        listeners.remove(listener);
