import org.openhab.core.automation.module.script.internal.ScriptEngineFactoryHelper;
import org.osgi.service.component.ComponentFactory;
import org.osgi.service.component.ComponentInstance;
 * The {@link ScriptTransformationServiceFactory} registers a {@link ScriptTransformationService}
 * for each newly added script engine.
 * @author Jimmy Tanagra - Initial contribution
@Component(immediate = true, service = { ScriptTransformationServiceFactory.class })
public class ScriptTransformationServiceFactory {
    private final ComponentFactory<ScriptTransformationService> scriptTransformationFactory;
    private final Map<ScriptEngineFactory, ComponentInstance<ScriptTransformationService>> scriptTransformations = new ConcurrentHashMap<>();
    public ScriptTransformationServiceFactory(
            @Reference(target = "(component.factory=org.openhab.core.automation.module.script.transformation.factory)") ComponentFactory<ScriptTransformationService> factory) {
        this.scriptTransformationFactory = factory;
        scriptTransformations.values().forEach(this::unregisterService);
        scriptTransformations.clear();
     * As {@link ScriptEngineFactory}s are added/removed, this method will cache all available script types
     * and registers a transformation service for the script engine.
    public void setScriptEngineFactory(ScriptEngineFactory engineFactory) {
        Optional<String> scriptType = ScriptEngineFactoryHelper.getPreferredExtension(engineFactory);
        if (scriptType.isEmpty()) {
        scriptTransformations.computeIfAbsent(engineFactory, factory -> {
            ScriptEngine scriptEngine = engineFactory.createScriptEngine(scriptType.get());
            String languageName = ScriptEngineFactoryHelper.getLanguageName(scriptEngine.getFactory());
            Dictionary<String, Object> properties = new Hashtable<>();
            properties.put(TransformationService.SERVICE_PROPERTY_NAME, scriptType.get().toUpperCase());
            properties.put(TransformationService.SERVICE_PROPERTY_LABEL, "SCRIPT " + languageName);
            properties.put(ScriptTransformationService.SCRIPT_TYPE_PROPERTY_NAME, scriptType.get());
            return scriptTransformationFactory.newInstance(properties);
    public void unsetScriptEngineFactory(ScriptEngineFactory engineFactory) {
        ComponentInstance<ScriptTransformationService> toBeUnregistered = scriptTransformations.remove(engineFactory);
        if (toBeUnregistered != null) {
            unregisterService(toBeUnregistered);
    private void unregisterService(ComponentInstance<ScriptTransformationService> instance) {
        instance.getInstance().deactivate();
        instance.dispose();
