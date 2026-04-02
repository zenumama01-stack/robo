 * An {@link InboxUpdatedEvent} notifies subscribers that a discovery result has been updated in the inbox.
 * Inbox updated events must be created with the {@link InboxEventFactory}.
public class InboxUpdatedEvent extends AbstractInboxEvent {
     * The inbox updated event type.
    public static final String TYPE = InboxUpdatedEvent.class.getSimpleName();
     * Constructs a new inbox updated event object.
    protected InboxUpdatedEvent(String topic, String payload, DiscoveryResultDTO discoveryResult) {
        return "Discovery Result with UID '" + getDiscoveryResult().thingUID + "' has been updated.";
