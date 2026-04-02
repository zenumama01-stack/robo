import static org.openhab.core.automation.module.script.ScriptEngineFactory.CONTEXT_KEY_DEPENDENCY_LISTENER;
 * The {@link ScriptEngineManagerImplTest} is a test class for the {@link ScriptEngineManagerImpl}
public class ScriptEngineManagerImplTest {
    private static final String SUPPORTED_SCRIPT_TYPE = "supported";
    private @Mock @NonNullByDefault({}) ScriptExtensionManager scriptExtensionManagerMock;
    private @Mock @NonNullByDefault({}) ScriptEngineFactory scriptEngineFactoryMock;
    private @Mock @NonNullByDefault({}) ScriptEngine scriptEngineMock;
    private @Mock @NonNullByDefault({}) javax.script.ScriptEngineFactory internalScriptEngineFactoryMock;
    private @Mock @NonNullByDefault({}) ScriptContext scriptContextMock;
    private @Mock @NonNullByDefault({}) Consumer<String> dependencyListenerMock;
    private @NonNullByDefault({}) ScriptEngineManagerImpl scriptEngineManager;
        when(scriptEngineMock.getFactory()).thenReturn(internalScriptEngineFactoryMock);
        when(scriptEngineMock.getContext()).thenReturn(scriptContextMock);
        when(scriptEngineFactoryMock.getScriptTypes()).thenReturn(List.of(SUPPORTED_SCRIPT_TYPE));
        when(scriptEngineFactoryMock.createScriptEngine(SUPPORTED_SCRIPT_TYPE)).thenReturn(scriptEngineMock);
        when(scriptDependencyTrackerMock.getTracker(any())).thenReturn(dependencyListenerMock);
        scriptEngineManager = new ScriptEngineManagerImpl(scriptExtensionManagerMock);
        scriptEngineManager.addScriptEngineFactory(scriptEngineFactoryMock);
    public void testDependencyListenerIsProperlyHandled() {
        String engineIdentifier = "testIdentifier";
        String scriptContent = "testContent";
        InputStreamReader scriptContentReader = new InputStreamReader(
                new ByteArrayInputStream(scriptContent.getBytes(StandardCharsets.UTF_8)));
        scriptEngineManager.createScriptEngine(SUPPORTED_SCRIPT_TYPE, engineIdentifier);
        scriptEngineManager.loadScript(engineIdentifier, scriptContentReader);
        // verify the dependency tracker is requested
        verify(scriptDependencyTrackerMock).getTracker(eq(engineIdentifier));
        // verify dependency tracker is set in the context
        ArgumentCaptor<Object> captor = ArgumentCaptor.forClass(Object.class);
        verify(scriptContextMock).setAttribute(eq(CONTEXT_KEY_DEPENDENCY_LISTENER), captor.capture(),
                eq(ScriptContext.ENGINE_SCOPE));
        Object captured = captor.getValue();
        assertThat(captured, is(dependencyListenerMock));
        // verify tracking is stopped when script engine is removed
        verify(scriptDependencyTrackerMock).removeTracking(eq(engineIdentifier));
