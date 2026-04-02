import com.google.gson.JsonParser;
 * {@link InboxEventFactoryTest} tests the {@link InboxEventFactory}.
public class InboxEventFactoryTest {
    private static final ThingTypeUID THING_TYPE_UID = new ThingTypeUID("binding", "type");
    private static final ThingUID THING_UID = new ThingUID(THING_TYPE_UID, "id");
    private static final DiscoveryResult DISCOVERY_RESULT = DiscoveryResultBuilder.create(THING_UID)
            .withThingType(THING_TYPE_UID).withTTL(60).build();
    private static final String INBOX_ADDED_EVENT_TYPE = InboxAddedEvent.TYPE;
    private static final String INBOX_ADDED_EVENT_TOPIC = InboxEventFactory.INBOX_ADDED_EVENT_TOPIC
            .replace("{thingUID}", THING_UID.getAsString());
    private static final String INBOX_ADDED_EVENT_PAYLOAD = new Gson()
            .toJson(DiscoveryResultDTOMapper.map(DISCOVERY_RESULT));
    private InboxEventFactory factory = new InboxEventFactory();
    public void inboxEventFactoryCreatesEventAsInboxAddedEventCorrectly() throws Exception {
        Event event = factory.createEvent(INBOX_ADDED_EVENT_TYPE, INBOX_ADDED_EVENT_TOPIC, INBOX_ADDED_EVENT_PAYLOAD,
        assertThat(event, is(instanceOf(InboxAddedEvent.class)));
        InboxAddedEvent inboxAddedEvent = (InboxAddedEvent) event;
        assertThat(inboxAddedEvent.getType(), is(INBOX_ADDED_EVENT_TYPE));
        assertThat(inboxAddedEvent.getTopic(), is(INBOX_ADDED_EVENT_TOPIC));
        assertThat(inboxAddedEvent.getPayload(), is(INBOX_ADDED_EVENT_PAYLOAD));
        assertThat(inboxAddedEvent.getDiscoveryResult(), not(nullValue()));
        assertThat(inboxAddedEvent.getDiscoveryResult().thingUID, is(THING_UID.getAsString()));
    public void inboxEventFactoryCreatesInboxAddedEventCorrectly() {
        InboxAddedEvent event = InboxEventFactory.createAddedEvent(DISCOVERY_RESULT);
        assertThat(event.getType(), is(INBOX_ADDED_EVENT_TYPE));
        assertThat(event.getTopic(), is(INBOX_ADDED_EVENT_TOPIC));
        assertThat(JsonParser.parseString(event.getPayload()), is(JsonParser.parseString(INBOX_ADDED_EVENT_PAYLOAD)));
        assertThat(event.getDiscoveryResult(), not(nullValue()));
        assertThat(event.getDiscoveryResult().thingUID, is(THING_UID.getAsString()));
