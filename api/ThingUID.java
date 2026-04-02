 * {@link ThingUID} represents a unique identifier for things.
public class ThingUID extends UID {
    private static final String NO_THING_TYPE = "";
    ThingUID() {
     * Instantiates a new thing UID.
     * @param thingTypeUID the thing type
     * @param id the id
    public ThingUID(ThingTypeUID thingTypeUID, String id) {
        super(thingTypeUID.getBindingId(), thingTypeUID.getId(), id);
     * @param bridgeUID the bridge UID through which the thing is accessed
     * @param id the id of the thing
    public ThingUID(ThingTypeUID thingTypeUID, ThingUID bridgeUID, String id) {
        super(getArray(thingTypeUID.getBindingId(), thingTypeUID.getId(), id, bridgeUID.getBridgeIds(),
                bridgeUID.getId()));
    public ThingUID(ThingTypeUID thingTypeUID, String id, String... bridgeIds) {
        super(getArray(thingTypeUID.getBindingId(), thingTypeUID.getId(), id, bridgeIds));
     * @param bindingId the binding id
    public ThingUID(String bindingId, String id) {
        super(bindingId, NO_THING_TYPE, id);
    public ThingUID(String bindingId, ThingUID bridgeUID, String id) {
        super(getArray(bindingId, NO_THING_TYPE, id, bridgeUID.getBridgeIds(), bridgeUID.getId()));
    private static String[] getArray(String bindingId, String thingTypeId, String id, @Nullable String... bridgeIds) {
        if (bridgeIds.length == 0) {
            return new String[] { bindingId, thingTypeId, id };
        String[] result = new String[3 + bridgeIds.length];
        result[0] = bindingId;
        result[1] = thingTypeId;
        System.arraycopy(bridgeIds, 0, result, 2, bridgeIds.length);
        result[result.length - 1] = id;
    private static String[] getArray(String bindingId, String thingTypeId, String id, List<String> bridgeIds,
            String bridgeId) {
        List<String> allBridgeIds = new ArrayList<>(bridgeIds);
        allBridgeIds.add(bridgeId);
        return getArray(bindingId, thingTypeId, id, allBridgeIds.toArray(new String[0]));
     * @param thingTypeId the thing type id
    public ThingUID(String bindingId, String thingTypeId, String id) {
        super(bindingId, thingTypeId, id);
     * @param thingUID the thing UID
    public ThingUID(String thingUID) {
        super(thingUID);
     * @param segments segments
    public ThingUID(String... segments) {
        super(segments);
     * Returns the bridge ids.
     * @return list of bridge ids
    public List<String> getBridgeIds() {
        return allSegments.subList(2, allSegments.size() - 1);
     * @return id the id
