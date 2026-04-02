 * {@link Event} objects are delivered by the {@link EventPublisher} through the openHAB event bus.
 * The callback interface {@link EventSubscriber} can be implemented in order to receive such events.
public interface Event {
     * Gets the event type.
    String getType();
     * Gets the topic of an event.
     * @return the event topic
    String getTopic();
     * Gets the payload as a serialized string.
     * @return the serialized event
    String getPayload();
     * Gets the name of the source identifying the sender.
     * @return the name of the source
    String getSource();
