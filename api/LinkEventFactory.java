 * This is an {@link EventFactory} for creating link events. The following event types are supported by this factory:
 * <li>{@link ItemChannelLinkAddedEvent#TYPE}</li>
 * <li>{@link ItemChannelLinkRemovedEvent#TYPE}</li>
 * @author Kai Kreuzer - Removed Thing link events
public class LinkEventFactory extends AbstractEventFactory {
    static final String LINK_ADDED_EVENT_TOPIC = "openhab/links/{linkID}/added";
    static final String LINK_REMOVED_EVENT_TOPIC = "openhab/links/{linkID}/removed";
     * Constructs a new LinkEventFactory.
    public LinkEventFactory() {
        super(Set.of(ItemChannelLinkAddedEvent.TYPE, ItemChannelLinkRemovedEvent.TYPE));
        if (ItemChannelLinkAddedEvent.TYPE.equals(eventType)) {
            return createItemChannelLinkAddedEvent(topic, payload);
        } else if (ItemChannelLinkRemovedEvent.TYPE.equals(eventType)) {
            return createItemChannelLinkRemovedEvent(topic, payload);
    private Event createItemChannelLinkAddedEvent(String topic, String payload) throws Exception {
        ItemChannelLinkDTO link = deserializePayload(payload, ItemChannelLinkDTO.class);
        return new ItemChannelLinkAddedEvent(topic, payload, link);
    private Event createItemChannelLinkRemovedEvent(String topic, String payload) throws Exception {
        return new ItemChannelLinkRemovedEvent(topic, payload, link);
     * Creates an item channel link added event.
     * @param itemChannelLink item channel link
     * @return the created item channel link added event
     * @throws IllegalArgumentException if item channel link is null
    public static ItemChannelLinkAddedEvent createItemChannelLinkAddedEvent(ItemChannelLink itemChannelLink) {
        checkNotNull(itemChannelLink, "itemChannelLink");
        String topic = buildTopic(LINK_ADDED_EVENT_TOPIC, itemChannelLink);
        ItemChannelLinkDTO itemChannelLinkDTO = map(itemChannelLink);
        String payload = serializePayload(itemChannelLinkDTO);
        return new ItemChannelLinkAddedEvent(topic, payload, itemChannelLinkDTO);
     * Creates an item channel link removed event.
     * @return the created item channel link removed event
    public static ItemChannelLinkRemovedEvent createItemChannelLinkRemovedEvent(ItemChannelLink itemChannelLink) {
        String topic = buildTopic(LINK_REMOVED_EVENT_TOPIC, itemChannelLink);
        return new ItemChannelLinkRemovedEvent(topic, payload, itemChannelLinkDTO);
    private static String buildTopic(String topic, AbstractLink itemChannelLink) {
        String targetEntity = itemChannelLink.getItemName() + "-" + itemChannelLink.getLinkedUID().toString();
        return topic.replace("{linkID}", targetEntity);
    private static ItemChannelLinkDTO map(ItemChannelLink itemChannelLink) {
        return new ItemChannelLinkDTO(itemChannelLink.getItemName(), itemChannelLink.getLinkedUID().toString(),
                itemChannelLink.getConfiguration().getProperties());
