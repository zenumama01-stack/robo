 * {@link ChannelBuilder} is responsible for creating {@link Channel}s.
 * @author Chris Jackson - Added properties and label/description
public class ChannelBuilder {
    private class ChannelImpl extends Channel {
        ChannelImpl(ChannelUID uid, @Nullable ChannelTypeUID channelTypeUID, @Nullable String acceptedItemType,
            super(channelUID, channelTypeUID, acceptedItemType, kind, configuration, defaultTags, properties, label,
                    description, autoUpdatePolicy);
    private final Logger logger = LoggerFactory.getLogger(ChannelBuilder.class);
    private ChannelKind kind;
    private @Nullable Configuration configuration;
    private Set<String> defaultTags;
    private @Nullable Map<String, String> properties;
    private ChannelBuilder(ChannelUID channelUID, @Nullable String acceptedItemType, Set<String> defaultTags) {
        this.defaultTags = defaultTags;
     * Creates a {@link ChannelBuilder} for the given {@link ChannelUID}.
     * @param channelUID the {@link ChannelUID}
     * @return channel builder
    public static ChannelBuilder create(ChannelUID channelUID) {
        return new ChannelBuilder(channelUID, null, new HashSet<>());
     * Creates a {@link ChannelBuilder} for the given {@link ChannelUID} and item type.
     * @param acceptedItemType item type that is accepted by this channel
    public static ChannelBuilder create(ChannelUID channelUID, @Nullable String acceptedItemType) {
        return new ChannelBuilder(channelUID, acceptedItemType, new HashSet<>());
     * Creates a {@link ChannelBuilder} from the given {@link Channel}.
     * @param channel the channel to be changed
    public static ChannelBuilder create(Channel channel) {
        ChannelBuilder channelBuilder = create(channel.getUID(), channel.getAcceptedItemType())
                .withConfiguration(channel.getConfiguration()).withDefaultTags(channel.getDefaultTags())
                .withKind(channel.getKind()).withProperties(channel.getProperties())
                .withType(channel.getChannelTypeUID()).withAutoUpdatePolicy(channel.getAutoUpdatePolicy());
        String label = channel.getLabel();
            channelBuilder.withLabel(label);
        String description = channel.getDescription();
     * Appends the {@link ChannelType} given by its {@link ChannelTypeUID} to the {@link Channel} to be build
     * @param channelTypeUID the {@link ChannelTypeUID}
    public ChannelBuilder withType(@Nullable ChannelTypeUID channelTypeUID) {
     * Appends a {@link Configuration} to the {@link Channel} to be build.
     * @param configuration the {@link Configuration}
    public ChannelBuilder withConfiguration(Configuration configuration) {
     * Adds properties to the {@link Channel}.
     * @param properties properties to add
    public ChannelBuilder withProperties(Map<String, String> properties) {
        String propertiesWithNullKeyOrValue = properties.entrySet().stream()
                .filter(e -> e.getKey() == null || e.getValue() == null).map(e -> String.valueOf(e.getKey())).sorted()
                .collect(Collectors.joining(", "));
        if (!propertiesWithNullKeyOrValue.isEmpty()) {
                    "Unexpected properties ({}) with null key or value for channel {}; probably a bug in the related binding!",
                    propertiesWithNullKeyOrValue, channelUID);
            throw new IllegalArgumentException("Unexpected properties (%s) with null key or value for channel %s"
                    .formatted(propertiesWithNullKeyOrValue, channelUID.getAsString()));
     * Sets the channel label. This allows overriding of the default label set in the {@link ChannelType}.
     * @param label the channel label to override the label set in the {@link ChannelType}
    public ChannelBuilder withLabel(String label) {
     * Sets the channel description. This allows overriding of the default description set in the {@link ChannelType}.
     * @param description the channel label to override the description set in the {@link ChannelType}
    public ChannelBuilder withDescription(String description) {
     * Appends default tags to the {@link Channel} to be build.
     * @param defaultTags default tags
    public ChannelBuilder withDefaultTags(Set<String> defaultTags) {
     * Sets the {@link ChannelKind} of the {@link Channel} to be build.
     * @param kind the {@link ChannelKind}
    public ChannelBuilder withKind(ChannelKind kind) {
        if (kind == null) {
            throw new IllegalArgumentException("'ChannelKind' must not be null");
     * Sets the accepted item type of the {@link Channel} to be build. See
     * {@link CoreItemFactory#getSupportedItemTypes()} for a list of available item types.
    public ChannelBuilder withAcceptedItemType(@Nullable String acceptedItemType) {
     * Sets the {@link AutoUpdatePolicy} to the {@link Channel} to be build.
     * @param policy the {@link AutoUpdatePolicy} to be used
    public ChannelBuilder withAutoUpdatePolicy(@Nullable AutoUpdatePolicy policy) {
        this.autoUpdatePolicy = policy;
     * Builds and returns the {@link Channel}.
     * @return the {@link Channel}
    public Channel build() {
        return new ChannelImpl(channelUID, channelTypeUID, acceptedItemType, kind, configuration, defaultTags,
                properties, label, description, autoUpdatePolicy);
