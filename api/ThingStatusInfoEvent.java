 * {@link ThingStatusInfoEvent}s will be delivered through the openHAB event bus if the status of a thing has
 * been updated. Thing status info objects must be created with the {@link ThingEventFactory}.
public class ThingStatusInfoEvent extends AbstractEvent {
    public static final String TYPE = ThingStatusInfoEvent.class.getSimpleName();
    protected ThingStatusInfoEvent(String topic, String payload, ThingUID thingUID, ThingStatusInfo thingStatusInfo) {
        this.thingStatusInfo = thingStatusInfo;
        return String.format("Thing '%s' updated: %s", thingUID, thingStatusInfo);
        if (o == null || getClass() != o.getClass() || !super.equals(o)) {
        ThingStatusInfoEvent that = (ThingStatusInfoEvent) o;
        return thingUID.equals(that.thingUID) && thingStatusInfo.equals(that.thingStatusInfo);
        return Objects.hash(super.hashCode(), thingUID, thingStatusInfo);
