public class TimestampTriggerProfileTest {
    public void testTimestampOnTrigger() {
        TriggerProfile profile = new TimestampTriggerProfile(callback);
        profile.onTriggerFromHandler(CommonTriggerEvents.PRESSED);
