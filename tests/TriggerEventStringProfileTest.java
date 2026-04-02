 * Tests for the system:trigger-event-string profile
public class TriggerEventStringProfileTest {
    public void testEventStringItem() {
        TriggerProfile profile = new TriggerEventStringProfile(callbackMock);
        verify(callbackMock, times(1)).sendCommand(eq(new StringType(CommonTriggerEvents.PRESSED)));
