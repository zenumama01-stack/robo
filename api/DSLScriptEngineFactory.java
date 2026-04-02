 * An implementation of {@link ScriptEngineFactory} for DSL scripts.
@Component(service = ScriptEngineFactory.class)
public class DSLScriptEngineFactory implements ScriptEngineFactory {
    private static final String SCRIPT_TYPE = "dsl";
    @Reference(cardinality = ReferenceCardinality.OPTIONAL, policyOption = ReferencePolicyOption.GREEDY)
    protected @Nullable DSLScriptContextProvider contextProvider;
    public DSLScriptEngineFactory(@Reference ScriptEngine scriptEngine,
            @Reference ScriptExtensionAccessor scriptExtensionAccessor) {
        return List.of(SCRIPT_TYPE, DSLScriptEngine.MIMETYPE_OPENHAB_DSL_RULE);
    public void scopeValues(javax.script.ScriptEngine scriptEngine, Map<String, Object> scopeValues) {
    public javax.script.@Nullable ScriptEngine createScriptEngine(String scriptType) {
        return new DSLScriptEngine(scriptEngine, contextProvider, scriptExtensionAccessor);
