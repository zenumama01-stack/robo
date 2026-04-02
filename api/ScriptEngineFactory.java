import javax.script.ScriptEngineManager;
import org.openhab.core.automation.module.script.internal.provider.ScriptModuleTypeProvider;
 * This class is used by the ScriptEngineManager to load ScriptEngines. This is meant as a way to allow other OSGi
 * bundles to provide custom script-languages with special needs, e.g. Nashorn, Jython, Groovy, etc.
 * @author Scott Rushworth - added/changed methods and parameters when implementing {@link ScriptModuleTypeProvider}
 * @author Jonathan Gilbert - added context keys
public interface ScriptEngineFactory {
    ScriptEngineManager ENGINE_MANAGER = new ScriptEngineManager();
     * Key to access engine identifier in script context.
    String CONTEXT_KEY_ENGINE_IDENTIFIER = "oh.engine-identifier";
     * Key to access Extension Accessor {@link ScriptExtensionAccessor}
    String CONTEXT_KEY_EXTENSION_ACCESSOR = "oh.extension-accessor";
     * Key to access Dependency Listener {@link ScriptDependencyListener}
    String CONTEXT_KEY_DEPENDENCY_LISTENER = "oh.dependency-listener";
     * This method returns a list of file extensions and MimeTypes that are supported by the ScriptEngine, e.g. py,
     * application/python, js, application/javascript, etc.
     * @return List of supported script types
    List<String> getScriptTypes();
     * This method "scopes" new values into the given ScriptEngine.
     * @param scriptEngine
     * @param scopeValues
    void scopeValues(ScriptEngine scriptEngine, Map<String, Object> scopeValues);
     * This method creates a new ScriptEngine based on the supplied file extension or MimeType.
     * openHAB-core always passes as parameter one of the values, returned by getScriptTypes().
     * The parameter serves for a ScriptEngineFactory, which announces support for several
     * distinct languages, to create a ScriptEngine for the requested language.
     * @param scriptType a file extension (script) or MimeType (ScriptAction or ScriptCondition)
     * @return ScriptEngine or null
    ScriptEngine createScriptEngine(String scriptType);
     * This method returns a {@link ScriptDependencyTracker} if it is available
     * @return a {@link ScriptDependencyTracker} or <code>null</code> if dependency tracking is not supported for
     *         {@link ScriptEngine}s created by this factory
    default @Nullable ScriptDependencyTracker getDependencyTracker() {
