 * {@link ThingTypeUID} represents a unique identifier for thing types.
public class ThingTypeUID extends UID {
    ThingTypeUID() {
    public ThingTypeUID(String uid) {
        super(uid);
    public ThingTypeUID(String bindingId, String thingTypeId) {
        super(bindingId, thingTypeId);
        return getSegment(1);
