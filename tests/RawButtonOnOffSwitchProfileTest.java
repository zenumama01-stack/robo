 * Tests for the system:rawbutton-on-off-switch profile
public class RawButtonOnOffSwitchProfileTest {
    private @Mock @NonNullByDefault({}) ProfileCallback callbackMock;
    public void testOnOffSwitchItem() {
        TriggerProfile profile = new RawButtonOnOffSwitchProfile(callbackMock);
        verifyAction(profile, CommonTriggerEvents.PRESSED, OnOffType.ON);
        verifyAction(profile, CommonTriggerEvents.RELEASED, OnOffType.OFF);
    private void verifyAction(TriggerProfile profile, String trigger, Command expectation) {
        reset(callbackMock);
        profile.onTriggerFromHandler(trigger);
        verify(callbackMock, times(1)).sendCommand(eq(expectation));
