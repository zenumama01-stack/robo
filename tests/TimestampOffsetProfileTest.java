 * Tests for the system:timestamp-offset profile
public class TimestampOffsetProfileTest {
        public final long seconds;
        public ParameterSet(long seconds) {
            this.seconds = seconds;
                { new ParameterSet(0) }, //
                { new ParameterSet(30) }, //
                { new ParameterSet(-30) } });
    public void testUNDEFOnStateUpdateFromHandler() {
        TimestampOffsetProfile offsetProfile = createProfile(callback, Long.toString(60));
        State state = UnDefType.UNDEF;
        assertEquals(UnDefType.UNDEF, result);
    public void testOnCommandFromItem(ParameterSet parameterSet) {
        TimestampOffsetProfile offsetProfile = createProfile(callback, Long.toString(parameterSet.seconds));
        Command cmd = DateTimeType.valueOf("2021-03-30T10:58:47.033+0000");
        DateTimeType updateResult = (DateTimeType) result;
        DateTimeType expectedResult = new DateTimeType(
                ((DateTimeType) cmd).getInstant().minusSeconds(parameterSet.seconds));
        assertEquals(expectedResult.getInstant(), updateResult.getInstant());
        Command cmd = new DateTimeType("2021-03-30T10:58:47.033+0000");
                ((DateTimeType) cmd).getInstant().plusSeconds(parameterSet.seconds));
        State state = new DateTimeType("2021-03-30T10:58:47.033+0000");
                ((DateTimeType) state).getInstant().plusSeconds(parameterSet.seconds));
    private TimestampOffsetProfile createProfile(ProfileCallback callback, String offset) {
        properties.put(TimestampOffsetProfile.OFFSET_PARAM, offset);
        when(context.getConfiguration()).thenReturn(new Configuration(properties));
