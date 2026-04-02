package org.openhab.core.automation.module.script;
 * This is an abstract class for implementing {@link ScriptEngineFactory}s.
 * @author Scott Rushworth - Initial contribution
public abstract class AbstractScriptEngineFactory implements ScriptEngineFactory {
    protected final Logger logger = LoggerFactory.getLogger(getClass());
    public List<String> getScriptTypes() {
        List<String> scriptTypes = new ArrayList<>();
        for (javax.script.ScriptEngineFactory f : ENGINE_MANAGER.getEngineFactories()) {
            scriptTypes.addAll(f.getExtensions());
            scriptTypes.addAll(f.getMimeTypes());
        return Collections.unmodifiableList(scriptTypes);
    public void scopeValues(ScriptEngine scriptEngine, Map<String, Object> scopeValues) {
        for (Entry<String, Object> entry : scopeValues.entrySet()) {
            scriptEngine.put(entry.getKey(), entry.getValue());
    public @Nullable ScriptEngine createScriptEngine(String scriptType) {
        ScriptEngine scriptEngine = ENGINE_MANAGER.getEngineByExtension(scriptType);
        if (scriptEngine == null) {
            scriptEngine = ENGINE_MANAGER.getEngineByMimeType(scriptType);
            scriptEngine = ENGINE_MANAGER.getEngineByName(scriptType);
        return scriptEngine;
