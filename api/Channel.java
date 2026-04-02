 * {@link Channel} is a part of a {@link Thing} that represents a functionality of it. Therefore {@link Item}s can be
 * linked a to a channel. The channel only accepts a specific item type which is specified by
 * {@link Channel#getAcceptedItemType()} methods. Use the {@link ChannelBuilder} for building channels.
 * @author Alex Tugarev - Extended about default tags
 * @author Benedikt Niehues - fix for Bug https://bugs.eclipse.org/bugs/show_bug.cgi?id=445137 considering default
 *         values
 * @author Chris Jackson - Added properties, label, description
 * @author Kai Kreuzer - Removed linked items from channel
public class Channel {
    private @Nullable String acceptedItemType;
    private final ChannelKind kind;
    // uid might not have been initialized by the default constructor.
    private @NonNullByDefault({}) ChannelUID uid;
    private @Nullable ChannelTypeUID channelTypeUID;
    private Map<String, String> properties;
    private Set<String> defaultTags = new LinkedHashSet<>();
    private @Nullable AutoUpdatePolicy autoUpdatePolicy;
    Channel() {
        this.kind = ChannelKind.STATE;
        this.configuration = new Configuration();
        this.properties = Map.of();
     * Use the {@link ChannelBuilder} for building channels.
    protected Channel(ChannelUID uid, @Nullable ChannelTypeUID channelTypeUID, @Nullable String acceptedItemType,
            ChannelKind kind, @Nullable Configuration configuration, Set<String> defaultTags,
            @Nullable Map<String, String> properties, @Nullable String label, @Nullable String description,
            @Nullable AutoUpdatePolicy autoUpdatePolicy) {
        this.channelTypeUID = channelTypeUID;
        this.acceptedItemType = acceptedItemType;
        this.kind = kind;
        this.autoUpdatePolicy = autoUpdatePolicy;
        this.defaultTags = Set.copyOf(defaultTags);
        this.configuration = configuration == null ? new Configuration() : configuration;
     * Returns the accepted item type.
     * @return accepted item type
    public @Nullable String getAcceptedItemType() {
        return acceptedItemType;
     * Returns the channel kind.
     * @return channel kind
        return kind;
     * Returns the unique id of the channel.
     * @return unique id of the channel
    public ChannelUID getUID() {
     * Returns the channel type UID
     * @return channel type UID or null if no channel type is specified
    public @Nullable ChannelTypeUID getChannelTypeUID() {
        return channelTypeUID;
     * Returns the label (if set).
     * If no label is set, getLabel will return null and the default label for the {@link Channel} is used.
     * @return the label for the channel. Can be null.
     * Returns the description (if set).
     * If no description is set, getDescription will return null and the default description for the {@link Channel} is
     * @return the description for the channel. Can be null.
     * Returns the channel configuration
     * @return channel configuration (not null)
     * Returns an immutable copy of the {@link Channel} properties.
     * @return an immutable copy of the {@link Channel} properties (not {@code null})
    public Map<String, String> getProperties() {
            return Map.copyOf(properties);
     * Returns default tags of this channel.
     * @return default tags of this channel.
    public Set<String> getDefaultTags() {
    public @Nullable AutoUpdatePolicy getAutoUpdatePolicy() {
        return autoUpdatePolicy;
        Channel channel = (Channel) o;
        return Objects.equals(acceptedItemType, channel.acceptedItemType) && kind == channel.kind
                && Objects.equals(uid, channel.uid) && Objects.equals(channelTypeUID, channel.channelTypeUID)
                && Objects.equals(label, channel.label) && Objects.equals(description, channel.description)
                && Objects.equals(configuration, channel.configuration)
                && Objects.equals(properties, channel.properties) && Objects.equals(defaultTags, channel.defaultTags)
                && autoUpdatePolicy == channel.autoUpdatePolicy;
        return Objects.hash(acceptedItemType, kind, uid, channelTypeUID, label, description, configuration, properties,
                defaultTags, autoUpdatePolicy);
