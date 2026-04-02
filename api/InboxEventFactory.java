import org.openhab.core.config.discovery.dto.DiscoveryResultDTOMapper;
 * An {@link InboxEventFactory} is responsible for creating inbox event instances.
public class InboxEventFactory extends AbstractEventFactory {
    static final String INBOX_ADDED_EVENT_TOPIC = "openhab/inbox/{thingUID}/added";
    static final String INBOX_REMOVED_EVENT_TOPIC = "openhab/inbox/{thingUID}/removed";
    static final String INBOX_UPDATED_EVENT_TOPIC = "openhab/inbox/{thingUID}/updated";
     * Constructs a new InboxEventFactory.
    public InboxEventFactory() {
        super(Set.of(InboxAddedEvent.TYPE, InboxUpdatedEvent.TYPE, InboxRemovedEvent.TYPE));
        if (InboxAddedEvent.TYPE.equals(eventType)) {
            return createAddedEvent(topic, payload);
        } else if (InboxRemovedEvent.TYPE.equals(eventType)) {
            return createRemovedEvent(topic, payload);
        } else if (InboxUpdatedEvent.TYPE.equals(eventType)) {
            return createUpdatedEvent(topic, payload);
    private Event createAddedEvent(String topic, String payload) {
        DiscoveryResultDTO resultDTO = deserializePayload(payload, DiscoveryResultDTO.class);
        return new InboxAddedEvent(topic, payload, resultDTO);
    private Event createRemovedEvent(String topic, String payload) {
        return new InboxRemovedEvent(topic, payload, resultDTO);
    private Event createUpdatedEvent(String topic, String payload) {
        return new InboxUpdatedEvent(topic, payload, resultDTO);
     * Creates an inbox added event.
     * @return the created inbox added event
     * @throws IllegalArgumentException if discoveryResult is null
    public static InboxAddedEvent createAddedEvent(DiscoveryResult discoveryResult) {
        assertValidArgument(discoveryResult);
        String topic = buildTopic(INBOX_ADDED_EVENT_TOPIC, discoveryResult.getThingUID().getAsString());
        DiscoveryResultDTO resultDTO = map(discoveryResult);
        String payload = serializePayload(resultDTO);
     * Creates an inbox removed event.
     * @return the created inbox removed event
    public static InboxRemovedEvent createRemovedEvent(DiscoveryResult discoveryResult) {
        String topic = buildTopic(INBOX_REMOVED_EVENT_TOPIC, discoveryResult.getThingUID().getAsString());
     * Creates an inbox updated event.
     * @return the created inbox updated event
    public static InboxUpdatedEvent createUpdatedEvent(DiscoveryResult discoveryResult) {
        String topic = buildTopic(INBOX_UPDATED_EVENT_TOPIC, discoveryResult.getThingUID().getAsString());
    private static void assertValidArgument(DiscoveryResult discoveryResult) {
        checkNotNull(discoveryResult, "discoveryResult");
    private static String buildTopic(String topic, String thingUID) {
        return topic.replace("{thingUID}", thingUID);
    private static DiscoveryResultDTO map(DiscoveryResult discoveryResult) {
        return DiscoveryResultDTOMapper.map(discoveryResult);
