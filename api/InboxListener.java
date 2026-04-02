 * The {@link InboxListener} interface for receiving {@link Inbox} events.
 * A class that is interested in processing {@link Inbox} events fired synchronously by the {@link Inbox} service has to
 * @see Inbox
public interface InboxListener {
     * Invoked synchronously when a <i>NEW</i> {@link DiscoveryResult} has been added
     * to the {@link Inbox}.
     * @param source the inbox which is the source of this event (not null)
     * @param result the discovery result which has been added to the inbox (not null)
    void thingAdded(Inbox source, DiscoveryResult result);
     * Invoked synchronously when an <i>EXISTING</i> {@link DiscoveryResult} has been
     * updated in the {@link Inbox}.
     * @param result the discovery result which has been updated in the inbox (not null)
    void thingUpdated(Inbox source, DiscoveryResult result);
     * removed from the {@link Inbox}.
     * @param result the discovery result which has been removed from the inbox (not null)
    void thingRemoved(Inbox source, DiscoveryResult result);
