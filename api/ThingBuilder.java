 * This class allows the easy construction of a {@link Thing} instance using the builder pattern.
 * @author Kai Kreuzer - Refactoring to make BridgeBuilder a subclass
public class ThingBuilder {
    protected final ThingUID thingUID;
    protected final ThingTypeUID thingTypeUID;
    private final List<Channel> channels = new ArrayList<>();
    private @Nullable String location;
    private @Nullable String semanticEquipmentTag;
    protected ThingBuilder(ThingTypeUID thingTypeUID, ThingUID thingUID) {
     * Create a new {@link ThingBuilder}
     * @param thingTypeUID the {@link ThingTypeUID} of the new thing
     * @param thingId the id part of the {@link ThingUID} of the new thing
     * @return the created {@link ThingBuilder}
    public static ThingBuilder create(ThingTypeUID thingTypeUID, String thingId) {
        return new ThingBuilder(thingTypeUID, new ThingUID(thingTypeUID, thingId));
     * @param thingUID the {@link ThingUID} of the new thing
    public static ThingBuilder create(ThingTypeUID thingTypeUID, ThingUID thingUID) {
        return new ThingBuilder(thingTypeUID, thingUID);
     * Create a new thing {@link ThingBuilder} for a copy of the given thing
     * @param thing the {@link Thing} to create this builder from
    public static ThingBuilder create(Thing thing) {
        return ThingBuilder.create(thing.getThingTypeUID(), thing.getUID()).withBridge(thing.getBridgeUID())
     * Build the thing
     * @return the {@link Thing}
    public Thing build() {
        final ThingImpl thing = new ThingImpl(thingTypeUID, thingUID);
        return populate(thing);
     * Sets the <code>label</code> for the thing
     * @param label a string containing the label
     * @return the {@link ThingBuilder} itself
    public ThingBuilder withLabel(@Nullable String label) {
     * Adds the given channel to the thing
     * @param channel the {@link Channel}
     * @throws IllegalArgumentException if a channel with the same UID is already present or the {@link ChannelUID} is
     *             not valid
    public ThingBuilder withChannel(Channel channel) {
        validateChannelUIDs(List.of(channel));
        ThingHelper.ensureUniqueChannels(channels, channel);
     * Replaces all channels of this thing with the given channels
     * @param channels one or more {@link Channel}s
    public ThingBuilder withChannels(Channel... channels) {
        return withChannels(Arrays.asList(channels));
     * @param channels a {@link List} of {@link Channel}s
    public ThingBuilder withChannels(List<Channel> channels) {
        validateChannelUIDs(channels);
        ThingHelper.ensureUniqueChannels(channels);
        this.channels.clear();
        this.channels.addAll(channels);
     * Removes the channel with the given UID from the thing
    public ThingBuilder withoutChannel(ChannelUID channelUID) {
        Iterator<Channel> iterator = channels.iterator();
            if (iterator.next().getUID().equals(channelUID)) {
     * Removes the given channels from the thing
    public ThingBuilder withoutChannels(Channel... channels) {
        return withoutChannels(Arrays.asList(channels));
    public ThingBuilder withoutChannels(List<Channel> channels) {
            withoutChannel(channel.getUID());
     * Set (or replace) the configuration of the thing
     * @param configuration a {@link Configuration} for this thing
    public ThingBuilder withConfiguration(Configuration configuration) {
     * Set the bridge for this thing
     * @param bridgeUID the {@link ThingUID} of the bridge for the thing
    public ThingBuilder withBridge(@Nullable ThingUID bridgeUID) {
     * Set / replace a single property for this thing
     * @param key the key / name of the property
    public ThingBuilder withProperty(String key, String value) {
        Map<String, String> oldProperties = Objects.requireNonNullElse(this.properties, Map.of());
        Map<String, String> newProperties = new HashMap<>(oldProperties);
        newProperties.put(key, value);
        return withProperties(newProperties);
     * Set/replace the properties for this thing
     * @param properties a {@link Map<String, String>} containing the properties
    public ThingBuilder withProperties(Map<String, String> properties) {
     * Set the location for this thing
     * @param location a string wih the location of the thing
    public ThingBuilder withLocation(@Nullable String location) {
        this.location = location;
     * Set the semantic (equipment) tag for this thing
     * @param semanticEquipmentTag a string with semantic (equipment) tag for this thing
    public ThingBuilder withSemanticEquipmentTag(@Nullable String semanticEquipmentTag) {
        this.semanticEquipmentTag = semanticEquipmentTag;
     * @param semanticEquipmentTag semantic (equipment) tag for this thing
    public ThingBuilder withSemanticEquipmentTag(SemanticTag semanticEquipmentTag) {
    protected Thing populate(ThingImpl thing) {
        thing.setLabel(label);
        thing.setChannels(channels);
        thing.setConfiguration(configuration);
        thing.setBridgeUID(bridgeUID);
            for (Map.Entry<String, String> entry : properties.entrySet()) {
        thing.setLocation(location);
        thing.setSemanticEquipmentTag(semanticEquipmentTag);
    private void validateChannelUIDs(List<Channel> channels) {
            if (!thingUID.equals(channel.getUID().getThingUID())) {
                        "Channel UID '" + channel.getUID() + "' does not match thing UID '" + thingUID + "'");
