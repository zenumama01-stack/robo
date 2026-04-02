 * An {@link EventFactory} is responsible for creating {@link Event} instances of specific event types. Event Factories
 * are used to create new Events ({@link #createEvent(String, String, String, String)}) based on the event type, the
 * topic, the payload and the source if an event type is supported ({@link #getSupportedEventTypes()}).
public interface EventFactory {
     * Create a new event instance of a specific event type.
     * @param source the source (can be null)
     * @return the created event instance (not null)
     * @throws IllegalArgumentException if eventType, topic or payload is null or empty
     * @throws IllegalArgumentException if the eventType is not supported
     * @throws Exception if the creation of the event has failed
    Event createEvent(String eventType, String topic, String payload, @Nullable String source) throws Exception;
     * Returns a list of all supported event types of this factory.
     * @return the supported event types (not null)
    Set<String> getSupportedEventTypes();
