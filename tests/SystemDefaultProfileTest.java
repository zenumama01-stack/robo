public class SystemDefaultProfileTest {
    public void testOnCommand() {
        SystemDefaultProfile profile = new SystemDefaultProfile(callbackMock);
        profile.onCommandFromItem(OnOffType.ON);
        verify(callbackMock).handleCommand(eq(OnOffType.ON));
    public void testStateUpdated() {
        profile.onStateUpdateFromHandler(OnOffType.ON);
        verify(callbackMock).sendUpdate(eq(OnOffType.ON));
    public void testPostCommand() {
        profile.onCommandFromHandler(OnOffType.ON);
        verify(callbackMock).sendCommand(eq(OnOffType.ON));
    public void testSendTimeSeries() {
        profile.onTimeSeriesFromHandler(timeSeries);
        verify(callbackMock).sendTimeSeries(timeSeries);
