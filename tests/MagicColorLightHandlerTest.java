 * Tests cases for {@link MagicColorLightHandler}. The tests provide mocks for supporting entities using Mockito.
public class MagicColorLightHandlerTest {
    private @NonNullByDefault({}) ThingHandler handler;
    private @Mock @NonNullByDefault({}) ThingHandlerCallback callbackMock;
        handler = new MagicColorLightHandler(thingMock);
        handler.setCallback(callbackMock);
    public void initializeShouldCallTheCallback() {
        handler.initialize();
        ArgumentCaptor<ThingStatusInfo> statusInfoCaptor = ArgumentCaptor.forClass(ThingStatusInfo.class);
        verify(callbackMock).statusUpdated(eq(thingMock), statusInfoCaptor.capture());
        ThingStatusInfo thingStatusInfo = statusInfoCaptor.getValue();
        assertThat(thingStatusInfo.getStatus(), is(equalTo(ThingStatus.ONLINE)));
