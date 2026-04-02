 * {@link ChannelGroupUID} represents a unique identifier for channel groups.
public class ChannelGroupUID extends UID {
     * Default constructor in package scope only. Will allow to instantiate this
     * class by reflection. Not intended to be used for normal instantiation.
    ChannelGroupUID() {
     * Parses a {@link ChannelGroupUID} for a given string. The UID must be in the format
     * 'bindingId:segment:segment:...'.
     * @param channelGroupUid uid in form a string (must not be null)
    public ChannelGroupUID(String channelGroupUid) {
        super(channelGroupUid);
        validateThingUID();
     * @param thingUID the unique identifier of the thing the channel group belongs to
     * @param id the channel group's id
    public ChannelGroupUID(ThingUID thingUID, String id) {
        super(toSegments(thingUID, id));
    void validateThingUID() {
            getThingUID();
            throw new IllegalArgumentException("ChannelGroupUID contains an invalid ThingUID part: " + e.getMessage(),
    private static List<String> toSegments(ThingUID thingUID, String id) {
        List<String> ret = new ArrayList<>(thingUID.getAllSegments());
        ret.add(id);
     * Returns the id.
     * @return id
        List<String> segments = getAllSegments();
        return segments.getLast();
    protected int getMinimalNumberOfSegments() {
     * Returns the thing UID
     * @return the thing UID
        List<String> allSegments = getAllSegments();
        return new ThingUID(allSegments.subList(0, allSegments.size() - 1).toArray(new String[allSegments.size() - 1]));
