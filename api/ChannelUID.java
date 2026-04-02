 * {@link ChannelUID} represents a unique identifier for channels.
 * @author Jochen Hiller - Bugfix 455434: added default constructor
 * @author Dennis Nobel - Added channel group id
 * @author Kai Kreuzer - Changed creation of channels to not require a thing type
 * @author Christoph Weitkamp - Changed pattern for validating last segment to contain either a single {@code #} or none
public class ChannelUID extends UID {
    public static final Pattern CHANNEL_SEGMENT_PATTERN = Pattern.compile("[\\w-]*(?:#[\\w-]*)?");
    public static final String CHANNEL_GROUP_SEPARATOR = "#";
    ChannelUID() {
     * Parses a {@link ChannelUID} for a given string. The UID must be in the format
     * @param channelUid uid in form a string
    public ChannelUID(String channelUid) {
        super(channelUid);
     * @param thingUID the unique identifier of the thing the channel belongs to
     * @param id the channel's id
    public ChannelUID(ThingUID thingUID, String id) {
        super(toSegments(thingUID, null, id));
     * @param channelGroupUID the unique identifier of the channel group the channel belongs to
    public ChannelUID(ChannelGroupUID channelGroupUID, String id) {
        super(toSegments(channelGroupUID.getThingUID(), channelGroupUID.getId(), id));
     * @param groupId the channel's group id
    public ChannelUID(ThingUID thingUID, String groupId, String id) {
        super(toSegments(thingUID, groupId, id));
            throw new IllegalArgumentException("ChannelUID contains an invalid ThingUID part: " + e.getMessage(), e);
    private static List<String> toSegments(ThingUID thingUID, @Nullable String groupId, String id) {
        ret.add(getChannelId(groupId, id));
    private static String getChannelId(@Nullable String groupId, String id) {
        return groupId != null ? groupId + CHANNEL_GROUP_SEPARATOR + id : id;
     * Returns the id without the group id.
     * @return id id without group id
    public String getIdWithoutGroup() {
        if (!isInGroup()) {
            return getId();
            return getId().split(CHANNEL_GROUP_SEPARATOR)[1];
    public boolean isInGroup() {
        return getId().contains(CHANNEL_GROUP_SEPARATOR);
     * Returns the group id.
     * @return group id or null if channel is not in a group
    public @Nullable String getGroupId() {
        return isInGroup() ? getId().split(CHANNEL_GROUP_SEPARATOR)[0] : null;
    protected void validateSegment(String segment, int index, int length) {
        if (index < length - 1) {
            super.validateSegment(segment, index, length);
            if (!CHANNEL_SEGMENT_PATTERN.matcher(segment).matches()) {
                        "UID segment '%s' contains invalid characters. The last segment of the channel UID must match the pattern '%s'.",
                        segment, CHANNEL_SEGMENT_PATTERN));
