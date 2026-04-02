 * The {@link ScriptTransformationServiceTest} holds tests for the {@link ScriptTransformationService}
public class ScriptTransformationServiceTest extends JavaTest {
    private static final String SCRIPT_LANGUAGE = "customDsl";
    private static final String SCRIPT_UID = "scriptUid." + SCRIPT_LANGUAGE;
    private static final String INVALID_SCRIPT_UID = "invalidScriptUid";
    private static final String INLINE_SCRIPT = "|inlineScript";
    private static final String SCRIPT = "script";
    private static final String SCRIPT_OUTPUT = "output";
    private static final Transformation TRANSFORMATION_CONFIGURATION = new Transformation(SCRIPT_UID, "label",
            SCRIPT_LANGUAGE, Map.of(Transformation.FUNCTION, SCRIPT));
    private static final Transformation INVALID_TRANSFORMATION_CONFIGURATION = new Transformation(INVALID_SCRIPT_UID,
            "label", "invalid", Map.of(Transformation.FUNCTION, SCRIPT));
    private @Mock @NonNullByDefault({}) TransformationRegistry transformationRegistry;
    private @Mock @NonNullByDefault({}) ScriptEngineManager scriptEngineManager;
    private @Mock @NonNullByDefault({}) ScriptEngineContainer scriptEngineContainer;
    private @Mock @NonNullByDefault({}) ScriptEngine scriptEngine;
    private @Mock @NonNullByDefault({}) ScriptContext scriptContext;
    private @NonNullByDefault({}) ScriptTransformationService service;
    public void setUp() throws ScriptException {
        Map<String, Object> properties = Map.of(ScriptTransformationService.SCRIPT_TYPE_PROPERTY_NAME, SCRIPT_LANGUAGE);
        service = new ScriptTransformationService(transformationRegistry, mock(ConfigDescriptionRegistry.class),
                scriptEngineManager, properties);
        when(scriptEngineManager.createScriptEngine(eq(SCRIPT_LANGUAGE), any())).thenReturn(scriptEngineContainer);
        when(scriptEngineManager.isSupported(anyString()))
                .thenAnswer(arguments -> SCRIPT_LANGUAGE.equals(arguments.getArgument(0)));
        when(scriptEngineContainer.getScriptEngine()).thenReturn(scriptEngine);
        when(scriptEngine.eval(SCRIPT)).thenReturn("output");
        when(scriptEngine.getContext()).thenReturn(scriptContext);
        when(transformationRegistry.get(anyString())).thenAnswer(arguments -> {
            String scriptUid = arguments.getArgument(0);
            if (SCRIPT_UID.equals(scriptUid)) {
                return TRANSFORMATION_CONFIGURATION;
            } else if (INVALID_SCRIPT_UID.equals(scriptUid)) {
                return INVALID_TRANSFORMATION_CONFIGURATION;
    public void success() throws TransformationException {
        String returnValue = Objects.requireNonNull(service.transform(SCRIPT_UID, "input"));
        assertThat(returnValue, is(SCRIPT_OUTPUT));
    public void scriptExecutionParametersAreInjectedIntoEngineContext() throws TransformationException {
        service.transform(SCRIPT_UID + "?param1=value1&param2=value2", "input");
        verify(scriptContext).setAttribute(eq("input"), eq("input"), eq(ScriptContext.ENGINE_SCOPE));
        verify(scriptContext).setAttribute(eq("param1"), eq("value1"), eq(ScriptContext.ENGINE_SCOPE));
        verify(scriptContext).setAttribute(eq("param2"), eq("value2"), eq(ScriptContext.ENGINE_SCOPE));
    public void scriptExecutionParametersAreDecoded() throws TransformationException {
        service.transform(SCRIPT_UID + "?param1=%26amp;&param2=%3dvalue", "input");
        verify(scriptContext).setAttribute(eq("param1"), eq("&amp;"), eq(ScriptContext.ENGINE_SCOPE));
        verify(scriptContext).setAttribute(eq("param2"), eq("=value"), eq(ScriptContext.ENGINE_SCOPE));
    public void scriptSetAttributesBeforeCompiling() throws TransformationException, ScriptException {
        abstract class CompilableScriptEngine implements ScriptEngine, Compilable {
        scriptEngine = mock(CompilableScriptEngine.class);
        InOrder inOrder = inOrder(scriptContext, scriptEngine);
        service.transform(SCRIPT_UID + "?param1=value1", "input");
        inOrder.verify(scriptContext, times(2)).setAttribute(anyString(), anyString(), eq(ScriptContext.ENGINE_SCOPE));
        inOrder.verify((Compilable) scriptEngine).compile(SCRIPT);
        inOrder.verify(scriptEngine).eval(SCRIPT);
    public void scriptAttributesRemovedAfterExecution() throws TransformationException, ScriptException {
        inOrder.verify(scriptContext).removeAttribute(eq("param1"), eq(ScriptContext.ENGINE_SCOPE));
    public void invalidScriptExecutionParametersAreDiscarded() throws TransformationException {
        service.transform(SCRIPT_UID + "?param1=value1&invalid", "input");
        verify(scriptContext, times(0)).setAttribute(eq("invalid"), any(), eq(ScriptContext.ENGINE_SCOPE));
    public void scriptsAreCached() throws TransformationException {
        service.transform(SCRIPT_UID, "input");
        verify(transformationRegistry).get(SCRIPT_UID);
    public void scriptCacheInvalidatedAfterChange() throws TransformationException {
        service.updated(TRANSFORMATION_CONFIGURATION, TRANSFORMATION_CONFIGURATION);
        verify(transformationRegistry, times(2)).get(SCRIPT_UID);
    public void unknownScriptUidThrowsException() {
        TransformationException e = assertThrows(TransformationException.class,
                () -> service.transform("foo", "input"));
        assertThat(e.getMessage(), is("Could not get script for UID 'foo'."));
    public void scriptExceptionResultsInTransformationException() throws ScriptException {
        when(scriptEngine.eval(SCRIPT)).thenThrow(new ScriptException("exception"));
                () -> service.transform(SCRIPT_UID, "input"));
        assertThat(e.getMessage(), is("Failed to execute script."));
        assertThat(e.getCause(), instanceOf(ScriptException.class));
        assertThat(e.getCause().getMessage(), is("exception"));
    public void recoversFromClosedScriptContext() throws ScriptException, TransformationException {
        when(scriptEngine.eval(SCRIPT)).thenThrow(new IllegalStateException("The Context is already closed."))
                .thenReturn(SCRIPT_OUTPUT);
        setupInterceptedLogger(ScriptTransformationService.class, LogLevel.WARN);
        stopInterceptedLogger(ScriptTransformationService.class);
        assertLogMessage(ScriptTransformationService.class, LogLevel.WARN, "Script engine context " + SCRIPT_UID
                + " is already closed, this should not happen. Recreating script engine.");
    public void inlineScriptProperlyProcessed() throws TransformationException, ScriptException {
        service.transform(INLINE_SCRIPT, "input");
        verify(scriptEngine).eval(INLINE_SCRIPT.substring(1));
