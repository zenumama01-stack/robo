import static org.hamcrest.number.IsCloseTo.closeTo;
 * Tests for the system:offset profile
public class SystemOffsetProfileTest {
        // initialize parser with ImperialUnits, otherwise units like °F are unknown
        Unit<Temperature> fahrenheit = ImperialUnits.FAHRENHEIT;
    public void testDecimalTypeOnCommandFromItem() {
        ProfileCallback callback = mock(ProfileCallback.class);
        SystemOffsetProfile offsetProfile = createProfile(callback, "3");
        Command cmd = new DecimalType(23);
        offsetProfile.onCommandFromItem(cmd);
        ArgumentCaptor<Command> capture = ArgumentCaptor.forClass(Command.class);
        verify(callback, times(1)).handleCommand(capture.capture());
        Command result = capture.getValue();
        DecimalType decResult = (DecimalType) result;
        assertEquals(20, decResult.intValue());
    public void testQuantityTypeOnCommandFromItem() {
        SystemOffsetProfile offsetProfile = createProfile(callback, "3°C");
        Command cmd = new QuantityType<>("23°C");
        QuantityType<?> decResult = (QuantityType<?>) result;
        assertEquals(SIUnits.CELSIUS, decResult.getUnit());
    public void testDecimalTypeOnCommandFromHandler() {
        offsetProfile.onCommandFromHandler(cmd);
        verify(callback, times(1)).sendCommand(capture.capture());
        assertEquals(26, decResult.intValue());
    public void testDecimalTypeOnStateUpdateFromHandler() {
        State state = new DecimalType(23);
        offsetProfile.onStateUpdateFromHandler(state);
        ArgumentCaptor<State> capture = ArgumentCaptor.forClass(State.class);
        verify(callback, times(1)).sendUpdate(capture.capture());
        State result = capture.getValue();
    public void testQuantityTypeOnCommandFromHandler() {
    public void testQuantityTypeOnStateUpdateFromHandler() {
        State state = new QuantityType<>("23°C");
    public void testQuantityTypeOnStateUpdateFromHandlerFahrenheitOffset() {
        SystemOffsetProfile offsetProfile = createProfile(callback, "3 °F");
        State state = new QuantityType<>("23 °C");
        assertThat(decResult.doubleValue(), is(closeTo(24.6666666666666666666666666666667d, 0.0000000000000001d)));
    public void testQuantityTypeWithUnitCelsiusOnStateUpdateFromHandlerDecimalOffset() {
    public void testQuantityTypeWithUnitOneOnStateUpdateFromHandlerDecimalOffset() {
        State state = new QuantityType<>();
        assertEquals(3, decResult.intValue());
        assertEquals(Units.ONE, decResult.getUnit());
    public void testTimeSeriesFromHandler() {
        TimeSeries ts = new TimeSeries(Policy.ADD);
        ts.add(now, new DecimalType(23));
        offsetProfile.onTimeSeriesFromHandler(ts);
        ArgumentCaptor<TimeSeries> capture = ArgumentCaptor.forClass(TimeSeries.class);
        verify(callback, times(1)).sendTimeSeries(capture.capture());
        TimeSeries result = capture.getValue();
        assertEquals(ts.getStates().count(), result.getStates().count());
        Entry entry = result.getStates().findFirst().get();
        assertNotNull(entry);
        assertEquals(now, entry.timestamp());
        DecimalType decResult = (DecimalType) entry.state();
    private SystemOffsetProfile createProfile(ProfileCallback callback, String offset) {
        ProfileContext context = mock(ProfileContext.class);
        Configuration config = new Configuration();
        config.put(SystemOffsetProfile.OFFSET_PARAM, offset);
        when(context.getConfiguration()).thenReturn(config);
