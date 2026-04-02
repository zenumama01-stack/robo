 * A {@link ThingRemovedEvent} notifies subscribers that a thing has been removed.
 * Thing removed events must be created with the {@link ThingEventFactory}.
public class ThingRemovedEvent extends AbstractThingRegistryEvent {
     * The thing removed event type.
    public static final String TYPE = ThingRemovedEvent.class.getSimpleName();
     * Constructs a new thing removed event object.
    protected ThingRemovedEvent(String topic, String payload, ThingDTO thing) {
        return "Thing '" + getThing().UID + "' has been removed.";
