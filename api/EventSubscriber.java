 * The {@link EventSubscriber} defines the callback interface for receiving events from
 * the openHAB event bus.
public interface EventSubscriber {
     * The constant {@link #ALL_EVENT_TYPES} must be returned by the {@link #getSubscribedEventTypes()} method, if the
     * event subscriber should subscribe to all event types.
    String ALL_EVENT_TYPES = "ALL";
     * Gets the event types to which the event subscriber is subscribed to.
     * @return subscribed event types (not null)
    Set<String> getSubscribedEventTypes();
     * Gets an {@link EventFilter} in order to receive specific events if the filter applies. If there is no
     * filter all subscribed event types are received.
     * @return the event filter, or null
    default @Nullable EventFilter getEventFilter() {
     * Callback method for receiving {@link Event}s from the openHAB event bus. This method is called for
     * every event where the event subscriber is subscribed to and the event filter applies.
     * @param event the received event (not null)
    void receive(Event event);
