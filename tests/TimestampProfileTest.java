 * Tests for {@link TimestampChangeProfile} and {@link TimestampUpdateProfile}.
public class TimestampProfileTest extends JavaTest {
    public void testTimestampOnUpdateStateUpdateFromHandler() {
        TimestampUpdateProfile timestampProfile = new TimestampUpdateProfile(callback);
        timestampProfile.onStateUpdateFromHandler(new DecimalType(23));
        Instant timestamp = updateResult.getInstant();
        long difference = ChronoUnit.MINUTES.between(now, timestamp);
        assertTrue(difference < 1);
    public void testTimestampOnChangeStateUpdateFromHandler() {
        TimestampChangeProfile timestampProfile = new TimestampChangeProfile(callback);
        // No existing previous state saved, the callback is first called
        DateTimeType changeResult = (DateTimeType) result;
        waitForAssert(() -> assertTrue(Instant.now().isAfter(changeResult.getInstant())));
        // The state is unchanged, no additional call to the callback
        // The state is changed, one additional call to the callback
        timestampProfile.onStateUpdateFromHandler(new DecimalType(24));
        verify(callback, times(2)).sendUpdate(capture.capture());
        result = capture.getValue();
        DateTimeType updatedResult = (DateTimeType) result;
        assertTrue(updatedResult.getInstant().isAfter(changeResult.getInstant()));
