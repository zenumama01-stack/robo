 * The {@link AbstractTypedEventSubscriber} is an abstract implementation of the {@link EventSubscriber} interface which
 * helps to subscribe to a specific event type. To receive an event - casted to the specific event type - the
 * {@link #receiveTypedEvent(T)} method must be implemented. This implementation provides no event filter. To filter
 * events based on the topic or some content the {@link #getEventFilter()} method can be overridden.
 * @param <T> The specific event type this class subscribes to.
public abstract class AbstractTypedEventSubscriber<T extends Event> implements EventSubscriber {
    private final Set<String> subscribedEventTypes;
     * Constructs a new typed event subscriber. Must be called in the subclass.
    protected AbstractTypedEventSubscriber(String eventType) {
        this.subscribedEventTypes = Set.of(eventType);
        receiveTypedEvent((T) event);
     * Callback method for receiving typed events of type T.
     * @param event the received event
    protected abstract void receiveTypedEvent(T event);
