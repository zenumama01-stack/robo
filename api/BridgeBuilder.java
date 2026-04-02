package org.openhab.core.thing.binding.builder;
 * This class allows the easy construction of a {@link Bridge} instance using the builder pattern.
 * @author Kai Kreuzer - Refactoring to make BridgeBuilder a subclass of ThingBuilder
 * @author Markus Rathgeb - Override methods to return BridgeBuidler instead of ThingBuidler
public class BridgeBuilder extends ThingBuilder {
    private BridgeBuilder(ThingTypeUID thingTypeUID, ThingUID thingUID) {
        super(thingTypeUID, thingUID);
    public static BridgeBuilder create(ThingTypeUID thingTypeUID, String bridgeId) {
        return new BridgeBuilder(thingTypeUID,
                new ThingUID(thingTypeUID.getBindingId(), thingTypeUID.getId(), bridgeId));
    public static BridgeBuilder create(ThingTypeUID thingTypeUID, ThingUID thingUID) {
        return new BridgeBuilder(thingTypeUID, thingUID);
     * Create a new bridge {@link BridgeBuilder} for a copy of the given bridge
     * @param bridge the {@link Bridge} to create this builder from
     * @return the created {@link BridgeBuilder}
    public static BridgeBuilder create(Bridge bridge) {
        return BridgeBuilder.create(bridge.getThingTypeUID(), bridge.getUID()).withBridge(bridge.getBridgeUID())
                .withChannels(bridge.getChannels()).withConfiguration(bridge.getConfiguration())
                .withLabel(bridge.getLabel()).withLocation(bridge.getLocation()).withProperties(bridge.getProperties())
                .withSemanticEquipmentTag(bridge.getSemanticEquipmentTag());
    public Bridge build() {
        final BridgeImpl bridge = new BridgeImpl(thingTypeUID, thingUID);
        return (Bridge) super.populate(bridge);
    public BridgeBuilder withLabel(@Nullable String label) {
        return (BridgeBuilder) super.withLabel(label);
    public BridgeBuilder withChannel(Channel channel) {
        return (BridgeBuilder) super.withChannel(channel);
    public BridgeBuilder withChannels(Channel... channels) {
        return (BridgeBuilder) super.withChannels(channels);
    public BridgeBuilder withChannels(List<Channel> channels) {
    public BridgeBuilder withoutChannel(ChannelUID channelUID) {
        return (BridgeBuilder) super.withoutChannel(channelUID);
    public BridgeBuilder withoutChannels(Channel... channels) {
        return (BridgeBuilder) super.withoutChannels(channels);
    public BridgeBuilder withoutChannels(List<Channel> channels) {
    public BridgeBuilder withConfiguration(Configuration thingConfiguration) {
        return (BridgeBuilder) super.withConfiguration(thingConfiguration);
    public BridgeBuilder withBridge(@Nullable ThingUID bridgeUID) {
        return (BridgeBuilder) super.withBridge(bridgeUID);
    public BridgeBuilder withProperties(Map<String, String> properties) {
        return (BridgeBuilder) super.withProperties(properties);
    public BridgeBuilder withLocation(@Nullable String location) {
        return (BridgeBuilder) super.withLocation(location);
    public BridgeBuilder withSemanticEquipmentTag(@Nullable String semanticEquipmentTag) {
        return (BridgeBuilder) super.withSemanticEquipmentTag(semanticEquipmentTag);
    public BridgeBuilder withSemanticEquipmentTag(SemanticTag semanticEquipmentTag) {
        return withSemanticEquipmentTag(semanticEquipmentTag.getName());
