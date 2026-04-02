public class ScriptEngineContainer {
    private ScriptEngine scriptEngine;
    private ScriptEngineFactory factory;
    private String identifier;
    public ScriptEngineContainer(ScriptEngine scriptEngine, ScriptEngineFactory factory, String identifier) {
        this.scriptEngine = scriptEngine;
        this.factory = factory;
        this.identifier = identifier;
    public ScriptEngine getScriptEngine() {
    public ScriptEngineFactory getFactory() {
        return factory;
    public String getIdentifier() {
        return identifier;
