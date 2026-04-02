import static org.hamcrest.collection.IsIterableContainingInAnyOrder.containsInAnyOrder;
import org.openhab.core.thing.events.ThingEventFactory.ChannelDescriptionChangedEventPayloadBean;
import org.openhab.core.thing.events.ThingEventFactory.ChannelDescriptionPatternPayloadBean;
import org.openhab.core.thing.events.ThingEventFactory.ChannelDescriptionStateOptionsPayloadBean;
import org.openhab.core.thing.events.ThingEventFactory.TriggerEventPayloadBean;
import com.google.gson.JsonNull;
 * {@link ThingEventFactoryTest} tests the {@link ThingEventFactory}.
public class ThingEventFactoryTest {
    private static final Gson JSONCONVERTER = new Gson();
    private static final ThingStatusInfo THING_STATUS_INFO = ThingStatusInfoBuilder
            .create(ThingStatus.OFFLINE, ThingStatusDetail.COMMUNICATION_ERROR).withDescription("Some description")
    private final ThingEventFactory factory = new ThingEventFactory();
    private static final Thing THING = ThingBuilder.create(THING_TYPE_UID, THING_UID).build();
    private static final String THING_STATUS_EVENT_TOPIC = ThingEventFactory.THING_STATUS_INFO_EVENT_TOPIC
    private static final String THING_ADDED_EVENT_TOPIC = ThingEventFactory.THING_ADDED_EVENT_TOPIC
    private static final String THING_STATUS_EVENT_PAYLOAD = JSONCONVERTER.toJson(THING_STATUS_INFO);
    private static final String THING_ADDED_EVENT_PAYLOAD = JSONCONVERTER.toJson(ThingDTOMapper.map(THING));
    private static final String CHANNEL_DESCRIPTION_CHANGED_EVENT_TOPIC = ThingEventFactory.CHANNEL_DESCRIPTION_CHANGED_TOPIC
            .replace("{channelUID}", CHANNEL_UID.getAsString());
    private static final String CHANNEL_DESCRIPTION_PATTERN_PAYLOAD = JSONCONVERTER
            .toJson(new ChannelDescriptionPatternPayloadBean("%s"));
    private static final String CHANNEL_DESCRIPTION_OLD_PATTERN_PAYLOAD = JSONCONVERTER
            .toJson(new ChannelDescriptionPatternPayloadBean("%unit%"));
    private static final String CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_ONLY_NEW_VALUE = JSONCONVERTER
            .toJson(new ChannelDescriptionChangedEventPayloadBean(CommonChannelDescriptionField.PATTERN,
                    CHANNEL_UID.getAsString(), Set.of("item1", "item2"), CHANNEL_DESCRIPTION_PATTERN_PAYLOAD, null));
    private static final String CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_NEW_AND_OLD_VALUE = JSONCONVERTER
                    CHANNEL_UID.getAsString(), Set.of("item1", "item2"), CHANNEL_DESCRIPTION_PATTERN_PAYLOAD,
                    CHANNEL_DESCRIPTION_OLD_PATTERN_PAYLOAD));
    private static final String CHANNEL_DESCRIPTION_STATE_OPTIONS_PAYLOAD = JSONCONVERTER
            .toJson(new ChannelDescriptionStateOptionsPayloadBean(List.of(new StateOption("offline", "Offline"))));
    private static final String CHANNEL_DESCRIPTION_OLD_STATE_OPTIONS_PAYLOAD = JSONCONVERTER
            .toJson(new ChannelDescriptionStateOptionsPayloadBean(List.of(new StateOption("online", "Online"))));
    private static final String CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_ONLY_NEW_OPTIONS = JSONCONVERTER
            .toJson(new ChannelDescriptionChangedEventPayloadBean(CommonChannelDescriptionField.STATE_OPTIONS,
                    CHANNEL_UID.getAsString(), Set.of("item1", "item2"), CHANNEL_DESCRIPTION_STATE_OPTIONS_PAYLOAD,
    private static final String CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_NEW_AND_OLD_OPTIONS = JSONCONVERTER
                    CHANNEL_DESCRIPTION_OLD_STATE_OPTIONS_PAYLOAD));
    private static final String CHANNEL_DESCRIPTION_STATE_DESCRIPTION_PAYLOAD = JSONCONVERTER
            .toJson(StateDescriptionFragmentBuilder.create() //
                    .withMinimum(BigDecimal.ZERO) //
                    .withMaximum(new BigDecimal(1000)) //
                    .withStep(new BigDecimal(100)) //
                    .withPattern("%.0f K") //
    private static final String CHANNEL_DESCRIPTION_OLD_STATE_DESCRIPTION_PAYLOAD = JSONCONVERTER
                    .withMaximum(new BigDecimal(6000)) //
    private static final String CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_NEW_AND_OLD_DESCRIPTION = JSONCONVERTER
            .toJson(new ChannelDescriptionChangedEventPayloadBean(CommonChannelDescriptionField.ALL,
                    CHANNEL_UID.getAsString(), Set.of("item1", "item2"), CHANNEL_DESCRIPTION_STATE_DESCRIPTION_PAYLOAD,
                    CHANNEL_DESCRIPTION_OLD_STATE_DESCRIPTION_PAYLOAD));
    private static final String CHANNEL_TRIGGERED_EVENT_TOPIC = ThingEventFactory.CHANNEL_TRIGGERED_EVENT_TOPIC
    private static final String CHANNEL_TRIGGERED_PRESSED_EVENT_PAYLOAD = new Gson()
            .toJson(new TriggerEventPayloadBean(CommonTriggerEvents.PRESSED, CHANNEL_UID.getAsString()));
    private static final String CHANNEL_TRIGGERED_EMPTY_EVENT_PAYLOAD = new Gson()
            .toJson(new TriggerEventPayloadBean("", CHANNEL_UID.getAsString()));
    public void testSupportedEventTypes() {
        assertThat(factory.getSupportedEventTypes(),
                containsInAnyOrder(ThingStatusInfoEvent.TYPE, ThingStatusInfoChangedEvent.TYPE, ThingAddedEvent.TYPE,
    public void testCreateEventThingStatusInfoEvent() throws Exception {
        Event event = factory.createEvent(ThingStatusInfoEvent.TYPE, THING_STATUS_EVENT_TOPIC,
                THING_STATUS_EVENT_PAYLOAD, null);
        assertThat(event, is(instanceOf(ThingStatusInfoEvent.class)));
        ThingStatusInfoEvent statusEvent = (ThingStatusInfoEvent) event;
        assertEquals(ThingStatusInfoEvent.TYPE, statusEvent.getType());
        assertEquals(THING_STATUS_EVENT_TOPIC, statusEvent.getTopic());
        assertEquals(THING_STATUS_EVENT_PAYLOAD, statusEvent.getPayload());
        assertEquals(THING_STATUS_INFO, statusEvent.getStatusInfo());
        assertEquals(THING_UID, statusEvent.getThingUID());
    public void testCreateStatusInfoEvent() {
        ThingStatusInfoEvent event = ThingEventFactory.createStatusInfoEvent(THING_UID, THING_STATUS_INFO);
        assertEquals(ThingStatusInfoEvent.TYPE, event.getType());
        assertEquals(THING_STATUS_EVENT_TOPIC, event.getTopic());
        assertEquals(JsonParser.parseString(THING_STATUS_EVENT_PAYLOAD), JsonParser.parseString(event.getPayload()));
        assertEquals(THING_STATUS_INFO, event.getStatusInfo());
        assertEquals(THING_UID, event.getThingUID());
    public void testCreateEventThingAddedEvent() throws Exception {
        Event event = factory.createEvent(ThingAddedEvent.TYPE, THING_ADDED_EVENT_TOPIC, THING_ADDED_EVENT_PAYLOAD,
        assertThat(event, is(instanceOf(ThingAddedEvent.class)));
        ThingAddedEvent addedEvent = (ThingAddedEvent) event;
        assertEquals(ThingAddedEvent.TYPE, addedEvent.getType());
        assertEquals(THING_ADDED_EVENT_TOPIC, addedEvent.getTopic());
        assertEquals(THING_ADDED_EVENT_PAYLOAD, addedEvent.getPayload());
        assertNotNull(addedEvent.getThing());
        assertEquals(THING_UID.getAsString(), addedEvent.getThing().UID);
    public void testCreateAddedEvent() {
        ThingAddedEvent event = ThingEventFactory.createAddedEvent(THING);
        assertEquals(ThingAddedEvent.TYPE, event.getType());
        assertEquals(THING_ADDED_EVENT_TOPIC, event.getTopic());
        assertEquals(JsonParser.parseString(THING_ADDED_EVENT_PAYLOAD), JsonParser.parseString(event.getPayload()));
        assertNotNull(event.getThing());
        assertEquals(THING_UID.getAsString(), event.getThing().UID);
    public void testCreateChannelDescriptionChangedEventOnlyNewValue() {
        ChannelDescriptionChangedEvent event = ThingEventFactory
                .createChannelDescriptionPatternChangedEvent(CHANNEL_UID, Set.of("item1", "item2"), "%s", null);
        assertEquals(ChannelDescriptionChangedEvent.TYPE, event.getType());
        assertEquals(CHANNEL_DESCRIPTION_CHANGED_EVENT_TOPIC, event.getTopic());
        assertEquals(JsonParser.parseString(CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_ONLY_NEW_VALUE),
                JsonParser.parseString(event.getPayload()));
        assertEquals(CommonChannelDescriptionField.PATTERN, event.getField());
        assertEquals(CHANNEL_UID, event.getChannelUID());
        assertThat(event.getLinkedItemNames(), hasSize(2));
        assertEquals(JsonParser.parseString(CHANNEL_DESCRIPTION_PATTERN_PAYLOAD),
                JsonParser.parseString(event.getValue()));
        assertNull(event.getOldValue());
    public void testCreateChannelDescriptionChangedEventNewAndOldValue() {
                .createChannelDescriptionPatternChangedEvent(CHANNEL_UID, Set.of("item1", "item2"), "%s", "%unit%");
        assertEquals(JsonParser.parseString(CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_NEW_AND_OLD_VALUE),
        assertEquals(CHANNEL_DESCRIPTION_PATTERN_PAYLOAD, event.getValue());
        assertEquals(CHANNEL_DESCRIPTION_OLD_PATTERN_PAYLOAD, event.getOldValue());
    public void testCreateEventChannelDescriptionChangedEventOnlyNewValue() throws Exception {
        Event event = factory.createEvent(ChannelDescriptionChangedEvent.TYPE, CHANNEL_DESCRIPTION_CHANGED_EVENT_TOPIC,
                CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_ONLY_NEW_VALUE, null);
        assertThat(event, is(instanceOf(ChannelDescriptionChangedEvent.class)));
        ChannelDescriptionChangedEvent triggeredEvent = (ChannelDescriptionChangedEvent) event;
        assertEquals(ChannelDescriptionChangedEvent.TYPE, triggeredEvent.getType());
        assertEquals(CHANNEL_DESCRIPTION_CHANGED_EVENT_TOPIC, triggeredEvent.getTopic());
        assertEquals(CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_ONLY_NEW_VALUE, triggeredEvent.getPayload());
        assertEquals(CommonChannelDescriptionField.PATTERN, triggeredEvent.getField());
        assertEquals(CHANNEL_UID, triggeredEvent.getChannelUID());
        assertThat(triggeredEvent.getLinkedItemNames(), hasSize(2));
        assertEquals(CHANNEL_DESCRIPTION_PATTERN_PAYLOAD, triggeredEvent.getValue());
        assertNull(triggeredEvent.getOldValue());
    public void testCreateEventChannelDescriptionChangedEventNewAndOldValue() throws Exception {
                CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_NEW_AND_OLD_VALUE, null);
        assertEquals(CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_NEW_AND_OLD_VALUE, triggeredEvent.getPayload());
        assertEquals(CHANNEL_DESCRIPTION_OLD_PATTERN_PAYLOAD, triggeredEvent.getOldValue());
    public void testCreateChannelDescriptionChangedEventOnlyNewOptions() {
        Set<String> itemNames = Set.of("item1", "item2");
        List<StateOption> options = List.of(new StateOption("offline", "Offline"));
                .createChannelDescriptionStateOptionsChangedEvent(CHANNEL_UID, itemNames, options, null);
        JsonObject expectedJson = JsonParser.parseString(CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_ONLY_NEW_OPTIONS)
                .getAsJsonObject();
        expectedJson.add("value", JsonParser.parseString(expectedJson.get("value").getAsString()));
        JsonObject actualJson = JsonParser.parseString(event.getPayload()).getAsJsonObject();
        actualJson.add("value", JsonParser.parseString(actualJson.get("value").getAsString()));
        assertEquals(expectedJson, actualJson);
        assertEquals(CommonChannelDescriptionField.STATE_OPTIONS, event.getField());
        assertEquals(itemNames, event.getLinkedItemNames());
        assertEquals(JsonParser.parseString(CHANNEL_DESCRIPTION_STATE_OPTIONS_PAYLOAD),
        assertEquals(options,
                JSONCONVERTER.fromJson(event.getValue(), ChannelDescriptionStateOptionsPayloadBean.class).options);
    public void testCreateEventChannelDescriptionChangedEventOnlyNewOptions() throws Exception {
                CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_ONLY_NEW_OPTIONS, null);
        assertEquals(CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_ONLY_NEW_OPTIONS, triggeredEvent.getPayload());
        assertEquals(CommonChannelDescriptionField.STATE_OPTIONS, triggeredEvent.getField());
        assertEquals(Set.of("item1", "item2"), triggeredEvent.getLinkedItemNames());
        assertEquals(CHANNEL_DESCRIPTION_STATE_OPTIONS_PAYLOAD, triggeredEvent.getValue());
        assertEquals(options, JSONCONVERTER.fromJson(triggeredEvent.getValue(),
                ChannelDescriptionStateOptionsPayloadBean.class).options);
    public void testCreateChannelDescriptionChangedEventOldAndNewOptions() {
        List<StateOption> oldOptions = List.of(new StateOption("online", "Online"));
                .createChannelDescriptionStateOptionsChangedEvent(CHANNEL_UID, itemNames, options, oldOptions);
        JsonObject expectedJson = JsonParser.parseString(CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_NEW_AND_OLD_OPTIONS)
        expectedJson.add("oldValue", JsonParser.parseString(expectedJson.get("oldValue").getAsString()));
        actualJson.add("oldValue", JsonParser.parseString(actualJson.get("oldValue").getAsString()));
        String oldValue = event.getOldValue();
        assertEquals(JsonParser.parseString(CHANNEL_DESCRIPTION_OLD_STATE_OPTIONS_PAYLOAD),
                oldValue == null ? JsonNull.INSTANCE : JsonParser.parseString(oldValue));
        assertEquals(oldOptions,
                JSONCONVERTER.fromJson(event.getOldValue(), ChannelDescriptionStateOptionsPayloadBean.class).options);
    public void testCreateEventChannelDescriptionChangedEventOldAndNewOptions() throws Exception {
                CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_NEW_AND_OLD_OPTIONS, null);
        assertEquals(CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_NEW_AND_OLD_OPTIONS, triggeredEvent.getPayload());
        assertEquals(CHANNEL_DESCRIPTION_OLD_STATE_OPTIONS_PAYLOAD, triggeredEvent.getOldValue());
        assertEquals(oldOptions, JSONCONVERTER.fromJson(triggeredEvent.getOldValue(),
    public void testCreateChannelDescriptionChangedEventOldAndNewStateDescription() {
        StateDescriptionFragment stateDescriptionFragment = StateDescriptionFragmentBuilder.create() //
        StateDescriptionFragment oldStateDescriptionFragment = StateDescriptionFragmentBuilder.create() //
        ChannelDescriptionChangedEvent event = ThingEventFactory.createChannelDescriptionChangedEvent(CHANNEL_UID,
                itemNames, stateDescriptionFragment, oldStateDescriptionFragment);
        JsonObject expectedJson = JsonParser
                .parseString(CHANNEL_DESCRIPTION_CHANGED_EVENT_PAYLOAD_NEW_AND_OLD_DESCRIPTION).getAsJsonObject();
        assertEquals(CommonChannelDescriptionField.ALL, event.getField());
        assertEquals(JsonParser.parseString(CHANNEL_DESCRIPTION_STATE_DESCRIPTION_PAYLOAD),
        assertEquals(stateDescriptionFragment,
                JSONCONVERTER.fromJson(event.getValue(), StateDescriptionFragmentImpl.class));
        assertEquals(JsonParser.parseString(CHANNEL_DESCRIPTION_OLD_STATE_DESCRIPTION_PAYLOAD),
        assertEquals(oldStateDescriptionFragment,
                JSONCONVERTER.fromJson(event.getOldValue(), StateDescriptionFragmentImpl.class));
    public void testCreateTriggerPressedEvent() {
        ChannelTriggeredEvent event = ThingEventFactory.createTriggerEvent(CommonTriggerEvents.PRESSED, CHANNEL_UID);
        assertEquals(ChannelTriggeredEvent.TYPE, event.getType());
        assertEquals(CHANNEL_TRIGGERED_EVENT_TOPIC, event.getTopic());
        assertEquals(JsonParser.parseString(CHANNEL_TRIGGERED_PRESSED_EVENT_PAYLOAD),
        assertNotNull(event.getEvent());
        assertEquals(CommonTriggerEvents.PRESSED, event.getEvent());
        assertEquals(CHANNEL_UID, event.getChannel());
    public void testCreateEventChannelTriggeredPressedEvent() throws Exception {
        Event event = factory.createEvent(ChannelTriggeredEvent.TYPE, CHANNEL_TRIGGERED_EVENT_TOPIC,
                CHANNEL_TRIGGERED_PRESSED_EVENT_PAYLOAD, null);
        assertThat(event, is(instanceOf(ChannelTriggeredEvent.class)));
        ChannelTriggeredEvent triggeredEvent = (ChannelTriggeredEvent) event;
        assertEquals(ChannelTriggeredEvent.TYPE, triggeredEvent.getType());
        assertEquals(CHANNEL_TRIGGERED_EVENT_TOPIC, triggeredEvent.getTopic());
        assertEquals(CHANNEL_TRIGGERED_PRESSED_EVENT_PAYLOAD, triggeredEvent.getPayload());
        assertNotNull(triggeredEvent.getEvent());
        assertEquals(CommonTriggerEvents.PRESSED, triggeredEvent.getEvent());
        assertEquals(CHANNEL_UID, triggeredEvent.getChannel());
    public void testCreateTriggerEmptyEvent() {
        ChannelTriggeredEvent event = ThingEventFactory.createTriggerEvent("", CHANNEL_UID);
        assertEquals(JsonParser.parseString(CHANNEL_TRIGGERED_EMPTY_EVENT_PAYLOAD),
        assertEquals("", event.getEvent());
    public void testCreateEventChannelTriggeredEmptyEvent() throws Exception {
                CHANNEL_TRIGGERED_EMPTY_EVENT_PAYLOAD, null);
        assertEquals(CHANNEL_TRIGGERED_EMPTY_EVENT_PAYLOAD, triggeredEvent.getPayload());
        assertEquals("", triggeredEvent.getEvent());
