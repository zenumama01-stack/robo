 * An {@link InboxAddedEvent} notifies subscribers that a discovery result has been added to the inbox.
 * Inbox added events must be created with the {@link InboxEventFactory}.
public class InboxAddedEvent extends AbstractInboxEvent {
     * The inbox added event type.
    public static final String TYPE = InboxAddedEvent.class.getSimpleName();
     * Constructs a new inbox added event object.
     * @param discoveryResult the discovery result data transfer object
    InboxAddedEvent(String topic, String payload, DiscoveryResultDTO discoveryResult) {
        super(topic, payload, discoveryResult);
        return "Discovery Result with UID '" + getDiscoveryResult().thingUID + "' has been added.";
