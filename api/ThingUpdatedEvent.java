 * A {@link ThingUpdatedEvent} notifies subscribers that a thing has been updated.
 * Thing updated events must be created with the {@link ThingEventFactory}.
public class ThingUpdatedEvent extends AbstractThingRegistryEvent {
     * The thing updated event type.
    public static final String TYPE = ThingUpdatedEvent.class.getSimpleName();
    private final ThingDTO oldThing;
     * Constructs a new thing updated event object.
     * @param oldThing the old thing data transfer object
    protected ThingUpdatedEvent(String topic, String payload, ThingDTO thing, ThingDTO oldThing) {
        this.oldThing = oldThing;
     * Gets the old thing.
     * @return the oldThing
    public ThingDTO getOldThing() {
        return "Thing '" + getThing().UID + "' has been updated.";
