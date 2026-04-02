 * {@link LinkEventFactoryTest} tests the {@link LinkEventFactory}.
public class LinkEventFactoryTest {
    private final LinkEventFactory factory = new LinkEventFactory();
    private static final ItemChannelLink LINK = new ItemChannelLink("item", new ChannelUID("a:b:c:d"));
    private static final ItemChannelLinkDTO LINK_DTO = new ItemChannelLinkDTO(LINK.getItemName(),
            LINK.getLinkedUID().toString(), LINK.getConfiguration().getProperties());
    private static final String LINK_EVENT_PAYLOAD = JSONCONVERTER.toJson(LINK_DTO);
    private static final String LINK_ADDED_EVENT_TOPIC = LinkEventFactory.LINK_ADDED_EVENT_TOPIC.replace("{linkID}",
            LINK.getItemName() + "-" + LINK.getLinkedUID().toString());
    private static final String LINK_REMOVED_EVENT_TOPIC = LinkEventFactory.LINK_REMOVED_EVENT_TOPIC.replace("{linkID}",
    public void testCreateItemChannelLinkAddedEvent() {
        ItemChannelLinkAddedEvent event = LinkEventFactory.createItemChannelLinkAddedEvent(LINK);
        assertEquals(ItemChannelLinkAddedEvent.TYPE, event.getType());
        assertEquals(LINK_ADDED_EVENT_TOPIC, event.getTopic());
        assertEquals(JsonParser.parseString(LINK_EVENT_PAYLOAD), JsonParser.parseString(event.getPayload()));
    public void testCreateEventItemChannelLinkAddedEvent() throws Exception {
        Event event = factory.createEvent(ItemChannelLinkAddedEvent.TYPE, LINK_ADDED_EVENT_TOPIC, LINK_EVENT_PAYLOAD,
        assertThat(event, is(instanceOf(ItemChannelLinkAddedEvent.class)));
        ItemChannelLinkAddedEvent triggeredEvent = (ItemChannelLinkAddedEvent) event;
        assertEquals(ItemChannelLinkAddedEvent.TYPE, triggeredEvent.getType());
        assertEquals(LINK_ADDED_EVENT_TOPIC, triggeredEvent.getTopic());
        assertEquals(LINK_EVENT_PAYLOAD, triggeredEvent.getPayload());
    public void testCreateItemChannelLinkRemovedEvent() {
        ItemChannelLinkRemovedEvent event = LinkEventFactory.createItemChannelLinkRemovedEvent(LINK);
        assertEquals(ItemChannelLinkRemovedEvent.TYPE, event.getType());
        assertEquals(LINK_REMOVED_EVENT_TOPIC, event.getTopic());
    public void testCreateEventItemChannelLinkRemovedEvent() throws Exception {
        Event event = factory.createEvent(ItemChannelLinkRemovedEvent.TYPE, LINK_REMOVED_EVENT_TOPIC,
                LINK_EVENT_PAYLOAD, null);
        assertThat(event, is(instanceOf(ItemChannelLinkRemovedEvent.class)));
        ItemChannelLinkRemovedEvent triggeredEvent = (ItemChannelLinkRemovedEvent) event;
        assertEquals(ItemChannelLinkRemovedEvent.TYPE, triggeredEvent.getType());
        assertEquals(LINK_REMOVED_EVENT_TOPIC, triggeredEvent.getTopic());
