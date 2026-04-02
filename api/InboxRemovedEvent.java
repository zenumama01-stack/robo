 * An {@link InboxRemovedEvent} notifies subscribers that a discovery result has been removed from the inbox.
 * Inbox removed events must be created with the {@link InboxEventFactory}.
public class InboxRemovedEvent extends AbstractInboxEvent {
     * The inbox removed event type.
    public static final String TYPE = InboxRemovedEvent.class.getSimpleName();
     * Constructs a new inbox removed event object.
    InboxRemovedEvent(String topic, String payload, DiscoveryResultDTO discoveryResult) {
        return "Discovery Result with UID '" + getDiscoveryResult().thingUID + "' has been removed.";
