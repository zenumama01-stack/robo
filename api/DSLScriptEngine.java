package org.openhab.core.model.script.runtime.internal.engine;
import javax.script.Bindings;
import javax.script.ScriptEngineFactory;
import org.eclipse.xtext.xbase.interpreter.impl.DefaultEvaluationContext;
import org.openhab.core.model.script.engine.Script;
import org.openhab.core.model.script.engine.ScriptParsingException;
import org.openhab.core.model.script.jvmmodel.ScriptJvmModelInferrer;
 * A basic implementation of the {@link javax.script.ScriptEngine} interface for using DSL scripts
 * within a jsr223 scripting context in Java.
 * Most methods are left empty, because they aren't used in our rule engine.
 * The most important methods are the ones that return metadata about the script engine factory.
 * Note: This class is not marked as NonNullByDefault as almost all parameters of all methods are
 * nullable as the interface is declared without null annotations.
public class DSLScriptEngine implements javax.script.ScriptEngine {
    private static final String OUTPUT_EVENT = "event";
    public static final String MIMETYPE_OPENHAB_DSL_RULE = "application/vnd.openhab.dsl.rule";
    private static final Map<String, String> IMPLICIT_VARS = Map.of( //
            "command", ScriptJvmModelInferrer.VAR_RECEIVED_COMMAND, //
            "state", ScriptJvmModelInferrer.VAR_NEW_STATE, //
            "newState", ScriptJvmModelInferrer.VAR_NEW_STATE, //
            "oldState", ScriptJvmModelInferrer.VAR_PREVIOUS_STATE, //
            "lastStateUpdate", ScriptJvmModelInferrer.VAR_LAST_STATE_UPDATE, //
            "lastStateChange", ScriptJvmModelInferrer.VAR_LAST_STATE_CHANGE, //
            "triggeringItem", ScriptJvmModelInferrer.VAR_TRIGGERING_ITEM, //
            "triggeringGroup", ScriptJvmModelInferrer.VAR_TRIGGERING_GROUP, //
            "input", ScriptJvmModelInferrer.VAR_INPUT);
    private final Logger logger = LoggerFactory.getLogger(DSLScriptEngine.class);
    private final org.openhab.core.model.script.engine.ScriptEngine scriptEngine;
    private final @Nullable DSLScriptContextProvider contextProvider;
    private final ScriptContext context = new SimpleScriptContext();
    private final ScriptExtensionAccessor scriptExtensionAccessor;
    private @Nullable Script parsedScript;
    public DSLScriptEngine(org.openhab.core.model.script.engine.ScriptEngine scriptEngine,
            @Nullable DSLScriptContextProvider contextProvider, ScriptExtensionAccessor scriptExtensionAccessor) {
        this.contextProvider = contextProvider;
        this.scriptExtensionAccessor = scriptExtensionAccessor;
    public Object eval(String script, ScriptContext context) throws ScriptException {
    public Object eval(Reader reader, ScriptContext context) throws ScriptException {
    public Object eval(String script) throws ScriptException {
            IEvaluationContext specificContext = null;
            org.openhab.core.model.script.engine.Script s = null;
            if (script.stripLeading().startsWith(DSLScriptContextProvider.CONTEXT_IDENTIFIER)) {
                String contextString = script.stripLeading().substring(
                        DSLScriptContextProvider.CONTEXT_IDENTIFIER.length(), script.stripLeading().indexOf('\n'));
                if (contextString.contains("-")) {
                    int indexLastDash = contextString.lastIndexOf('-');
                    modelName = contextString.substring(0, indexLastDash);
                    String ruleIndex = contextString.substring(indexLastDash + 1);
                    if (contextProvider != null) {
                        DSLScriptContextProvider cp = contextProvider;
                        logger.debug("Script uses context '{}'.", contextString);
                        specificContext = cp.getContext(modelName);
                        XExpression xExpression = cp.getParsedScript(modelName, ruleIndex);
                        if (xExpression != null) {
                            s = scriptEngine.newScriptFromXExpression(xExpression);
                            logger.warn("No pre-parsed script found for {}-{}.", modelName, ruleIndex);
                        logger.error("Script references context '{}', but no context provider is registered!",
                                contextString);
                    logger.error("Script has an invalid context reference '{}'!", contextString);
                s = parsedScript;
                if (s == null) {
                    s = scriptEngine.newScriptFromString(script);
                    parsedScript = s;
            IEvaluationContext evalContext = createEvaluationContext(s, specificContext);
            return s.execute(evalContext);
        } catch (ScriptExecutionException | ScriptParsingException e) {
            // in case of error, drop the cached script to make sure, it is re-resolved.
            parsedScript = null;
            throw new ScriptException(e.getMessage(), modelName, -1);
    private DefaultEvaluationContext createEvaluationContext(Script script, IEvaluationContext specificContext) {
        IEvaluationContext parentContext = specificContext;
        if (specificContext == null && script instanceof ScriptImpl impl) {
            XExpression xExpression = impl.getXExpression();
                Resource resource = xExpression.eResource();
                if (resource instanceof XtextResource xtextResource) {
                    IResourceServiceProvider provider = xtextResource.getResourceServiceProvider();
                    parentContext = provider.get(IEvaluationContext.class);
        DefaultEvaluationContext evalContext = new DefaultEvaluationContext(parentContext);
        for (Map.Entry<String, String> entry : IMPLICIT_VARS.entrySet()) {
            Object value = context.getAttribute(entry.getKey());
                QualifiedName qn = QualifiedName.create(entry.getValue());
                if (evalContext.getValue(qn) == null) {
                    evalContext.newValue(qn, value);
                    evalContext.assignValue(qn, value);
        Map<String, Object> cachePreset = scriptExtensionAccessor.findPreset("cache",
                (String) context.getAttribute("oh.engine-identifier", ScriptContext.ENGINE_SCOPE));
        evalContext.newValue(QualifiedName.create(ScriptJvmModelInferrer.VAR_SHARED_CACHE),
                cachePreset.get("sharedCache"));
        evalContext.newValue(QualifiedName.create(ScriptJvmModelInferrer.VAR_PRIVATE_CACHE),
                cachePreset.get("privateCache"));
        // now add specific implicit vars, where we have to map the right content
        Object value = context.getAttribute(OUTPUT_EVENT);
        if (value instanceof ChannelTriggeredEvent event) {
            evalContext.newValue(QualifiedName.create(ScriptJvmModelInferrer.VAR_RECEIVED_EVENT), event.getEvent());
            evalContext.newValue(QualifiedName.create(ScriptJvmModelInferrer.VAR_TRIGGERING_CHANNEL),
                    event.getChannel().getAsString());
        if (value instanceof ItemEvent event) {
            evalContext.newValue(QualifiedName.create(ScriptJvmModelInferrer.VAR_TRIGGERING_ITEM_NAME),
                    event.getItemName());
            Object group = context.getAttribute(ScriptJvmModelInferrer.VAR_TRIGGERING_GROUP);
            if (group instanceof Item groupItem) {
                evalContext.newValue(QualifiedName.create(ScriptJvmModelInferrer.VAR_TRIGGERING_GROUP_NAME),
                        groupItem.getName());
        if (value instanceof ThingStatusInfoChangedEvent event) {
            evalContext.newValue(QualifiedName.create(ScriptJvmModelInferrer.VAR_TRIGGERING_THING),
                    event.getThingUID().toString());
            evalContext.newValue(QualifiedName.create(ScriptJvmModelInferrer.VAR_PREVIOUS_STATUS),
                    event.getOldStatusInfo().getStatus().toString());
            evalContext.newValue(QualifiedName.create(ScriptJvmModelInferrer.VAR_NEW_STATUS),
                    event.getStatusInfo().getStatus().toString());
        return evalContext;
    public Object eval(Reader reader) throws ScriptException {
    public Object eval(String script, Bindings n) throws ScriptException {
    public Object eval(Reader reader, Bindings n) throws ScriptException {
    public void put(String key, Object value) {
    public Bindings getBindings(int scope) {
    public void setBindings(Bindings bindings, int scope) {
    public Bindings createBindings() {
    public ScriptContext getContext() {
    public void setContext(ScriptContext context) {
        return new ScriptEngineFactory() {
            public String getProgram(String... statements) {
            public Object getParameter(String key) {
            public String getOutputStatement(String toDisplay) {
            public List<String> getNames() {
            public List<String> getMimeTypes() {
                return List.of(MIMETYPE_OPENHAB_DSL_RULE);
            public String getMethodCallSyntax(String obj, String m, String... args) {
            public String getLanguageVersion() {
                return "v1";
            public String getLanguageName() {
                return "Rule DSL";
            public List<String> getExtensions() {
            public String getEngineVersion() {
            public String getEngineName() {
