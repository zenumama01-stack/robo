 * {@link ThingStatusInfoChangedEvent}s will be delivered through the openHAB event bus if the status of a
 * thing has changed. Thing status info objects must be created with the {@link ThingEventFactory}.
public class ThingStatusInfoChangedEvent extends AbstractEvent {
     * The thing status event type.
    public static final String TYPE = ThingStatusInfoChangedEvent.class.getSimpleName();
    private final ThingStatusInfo thingStatusInfo;
    private final ThingStatusInfo oldStatusInfo;
     * Creates a new thing status event object.
     * @param newThingStatusInfo the thing status info object
    protected ThingStatusInfoChangedEvent(String topic, String payload, ThingUID thingUID,
            ThingStatusInfo newThingStatusInfo, ThingStatusInfo oldThingStatusInfo) {
        this.thingStatusInfo = newThingStatusInfo;
        this.oldStatusInfo = oldThingStatusInfo;
     * Gets the thing UID.
     * Gets the thing status info.
     * @return the thing status info
    public ThingStatusInfo getStatusInfo() {
        return thingStatusInfo;
     * Gets the old thing status info.
     * @return the old thing status info
    public ThingStatusInfo getOldStatusInfo() {
        return oldStatusInfo;
        return String.format("Thing '%s' changed from %s to %s", thingUID, oldStatusInfo, thingStatusInfo);
