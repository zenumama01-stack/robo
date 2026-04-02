import org.mockito.Captor;
 * The {@link SystemTriggerHandlerTest} contains tests for the {@link SystemTriggerHandler}
public class SystemTriggerHandlerTest {
    private static final int CFG_STARTLEVEL = 80;
    private @Mock @NonNullByDefault({}) BundleContext bundleContextMock;
    private @Mock @NonNullByDefault({}) TriggerHandlerCallback callbackMock;
    private @Mock @NonNullByDefault({}) Trigger triggerMock;
    private @Captor @NonNullByDefault({}) ArgumentCaptor<Map<String, Object>> captor;
        when(triggerMock.getConfiguration())
                .thenReturn(new Configuration(Map.of(SystemTriggerHandler.CFG_STARTLEVEL, CFG_STARTLEVEL)));
        when(triggerMock.getTypeUID()).thenReturn(SystemTriggerHandler.STARTLEVEL_MODULE_TYPE_ID);
    public void testDoesNotTriggerIfStartLevelTooLow() {
        when(startLevelServiceMock.getStartLevel()).thenReturn(0);
        SystemTriggerHandler triggerHandler = new SystemTriggerHandler(triggerMock, bundleContextMock);
        triggerHandler.setCallback(callbackMock);
        verifyNoInteractions(callbackMock);
    public void testDoesNotTriggerIfStartLevelEventLower() {
        Event event = SystemEventFactory.createStartlevelEvent(70);
        triggerHandler.receive(event);
    public void testDoesTriggerIfStartLevelEventHigher() {
        Event event = SystemEventFactory.createStartlevelEvent(100);
        verify(callbackMock).triggered(eq(triggerMock), captor.capture());
        Map<String, Object> configuration = captor.getValue();
        assertThat(configuration.get(SystemTriggerHandler.OUT_STARTLEVEL), is(CFG_STARTLEVEL));
    public void testDoesNotTriggerAfterEventTrigger() {
        verifyNoMoreInteractions(callbackMock);
