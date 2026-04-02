 * {@link ChannelTriggeredEvent}s can be used to deliver triggers through the openHAB event bus.
 * Trigger events must be created with the {@link ThingEventFactory}.
public class ChannelTriggeredEvent extends AbstractEvent {
     * The thing trigger event type.
    public static final String TYPE = ChannelTriggeredEvent.class.getSimpleName();
    private final ChannelUID channel;
     * The event.
    private final String event;
     * Constructs a new thing trigger event.
     * @param topic the topic. The topic includes the thing UID, see
     *            {@link ThingEventFactory#CHANNEL_TRIGGERED_EVENT_TOPIC}
     * @param payload the payload. Contains a serialized {@link ThingEventFactory.TriggerEventPayloadBean}.
     * @param channel the channel which triggered the event
    protected ChannelTriggeredEvent(String topic, String payload, @Nullable String source, String event,
            ChannelUID channel) {
     * Returns the event.
     * @return the event
    public String getEvent() {
     * @return the channel which triggered the event
    public ChannelUID getChannel() {
        return channel;
        return String.format("%s triggered %s", channel, event);
