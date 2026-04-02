 * A {@link ThingAddedEvent} notifies subscribers that a thing has been added.
 * Thing added events must be created with the {@link ThingEventFactory}.
public class ThingAddedEvent extends AbstractThingRegistryEvent {
     * The thing added event type.
    public static final String TYPE = ThingAddedEvent.class.getSimpleName();
     * Constructs a new thing added event object.
    protected ThingAddedEvent(String topic, String payload, ThingDTO thing) {
        super(topic, payload, null, thing);
        return "Thing '" + getThing().UID + "' has been added.";
