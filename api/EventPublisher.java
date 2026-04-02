 * The {@link EventPublisher} posts {@link Event}s through the openHAB event bus in an asynchronous way.
 * Posted events can be received by implementing the {@link EventSubscriber} callback interface.
public interface EventPublisher {
     * Posts an event through the event bus in an asynchronous way.
     * @param event the event posted through the event bus
     * @throws IllegalArgumentException if the event is null
     * @throws IllegalArgumentException if one of the event properties type, payload or topic is null
     * @throws IllegalStateException if the underlying event bus module is not available
    void post(Event event) throws IllegalArgumentException, IllegalStateException;
