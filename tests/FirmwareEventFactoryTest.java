public class FirmwareEventFactoryTest extends JavaTest {
    private static final ThingTypeUID THING_TYPE_UID1 = new ThingTypeUID("binding", "simpleID");
    private final ThingUID thingUID = new ThingUID(THING_TYPE_UID1, "idSample");
    private @NonNullByDefault({}) FirmwareEventFactory eventFactory;
        eventFactory = new FirmwareEventFactory();
        assertThat(eventFactory.getSupportedEventTypes(), containsInAnyOrder(FirmwareStatusInfoEvent.TYPE,
                FirmwareUpdateProgressInfoEvent.TYPE, FirmwareUpdateResultInfoEvent.TYPE));
    public void testCreateEventForUnknownType() throws Exception {
                () -> eventFactory.createEventByType("unknownType", "topic", "somePayload", "Source"));
    public void testSerializationAndDeserializationFirmwareStatusInfo() throws Exception {
        FirmwareStatusInfo firmwareStatusInfo = FirmwareStatusInfo.createUpdateAvailableInfo(thingUID);
        // The info is serialized into the event payload
        FirmwareStatusInfoEvent firmwareStatusInfoEvent = FirmwareEventFactory
                .createFirmwareStatusInfoEvent(firmwareStatusInfo);
        // Deserialize
        Event event = eventFactory.createEventByType(FirmwareStatusInfoEvent.TYPE, "topic",
                firmwareStatusInfoEvent.getPayload(), null);
        assertThat(event, is(instanceOf(FirmwareStatusInfoEvent.class)));
        assertThat(event.getType(), is(FirmwareStatusInfoEvent.TYPE));
        FirmwareStatusInfo deserializedStatusInfo = ((FirmwareStatusInfoEvent) event).getFirmwareStatusInfo();
        assertThat(firmwareStatusInfo, is(deserializedStatusInfo));
    public void testSerializationAndDeserializationFirmwareUpdateProgressInfo() throws Exception {
        FirmwareUpdateProgressInfo firmwareUpdateProgressInfo = FirmwareUpdateProgressInfo
                .createFirmwareUpdateProgressInfo(thingUID, "1.2.3", ProgressStep.UPDATING,
                        List.of(ProgressStep.WAITING, ProgressStep.UPDATING), false, 10);
        FirmwareUpdateProgressInfoEvent progressInfoEvent = FirmwareEventFactory
                .createFirmwareUpdateProgressInfoEvent(firmwareUpdateProgressInfo);
        Event event = eventFactory.createEventByType(FirmwareUpdateProgressInfoEvent.TYPE, "topic",
                progressInfoEvent.getPayload(), null);
        assertThat(event, is(instanceOf(FirmwareUpdateProgressInfoEvent.class)));
        FirmwareUpdateProgressInfo deserializedStatusInfo = ((FirmwareUpdateProgressInfoEvent) event).getProgressInfo();
        assertThat(firmwareUpdateProgressInfo, is(deserializedStatusInfo));
    public void testSerializationAndDeserializationFirmwareUpdateResultInfo() throws Exception {
        FirmwareUpdateResultInfo firmwareUpdateResultInfo = FirmwareUpdateResultInfo
                .createFirmwareUpdateResultInfo(thingUID, FirmwareUpdateResult.ERROR, "error message");
        FirmwareUpdateResultInfoEvent firmwareUpdateResultInfoEvent = FirmwareEventFactory
                .createFirmwareUpdateResultInfoEvent(firmwareUpdateResultInfo);
        Event event = eventFactory.createEventByType(FirmwareUpdateResultInfoEvent.TYPE, "topic",
                firmwareUpdateResultInfoEvent.getPayload(), "Source");
        assertThat(event, is(instanceOf(FirmwareUpdateResultInfoEvent.class)));
        FirmwareUpdateResultInfo updateResultInfo = ((FirmwareUpdateResultInfoEvent) event)
                .getFirmwareUpdateResultInfo();
        assertThat(firmwareUpdateResultInfo, is(updateResultInfo));
