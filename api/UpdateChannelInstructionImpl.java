 * The {@link UpdateChannelInstructionImpl} implements a {@link ThingUpdateInstruction} that updates a channel of a
public class UpdateChannelInstructionImpl implements ThingUpdateInstruction {
    private final Logger logger = LoggerFactory.getLogger(UpdateChannelInstructionImpl.class);
    private final boolean removeOldChannel;
    private final boolean preserveConfig;
    private final ChannelTypeUID channelTypeUid;
    private final @Nullable List<String> tags;
    UpdateChannelInstructionImpl(int thingTypeVersion, XmlUpdateChannel updateChannel,
            ChannelTypeRegistry channelTypeRegistry, ConfigDescriptionRegistry configDescriptionRegistry) {
        this.removeOldChannel = true;
        this.channelId = updateChannel.getId();
        this.channelTypeUid = new ChannelTypeUID(updateChannel.getType());
        String rawGroupIds = updateChannel.getGroupIds();
        this.label = updateChannel.getLabel();
        this.description = updateChannel.getDescription();
        this.tags = updateChannel.getTags();
        this.preserveConfig = updateChannel.isPreserveConfiguration();
    UpdateChannelInstructionImpl(int thingTypeVersion, XmlAddChannel addChannel,
        this.removeOldChannel = false;
        this.channelId = addChannel.getId();
        this.channelTypeUid = new ChannelTypeUID(addChannel.getType());
        String rawGroupIds = addChannel.getGroupIds();
        this.label = addChannel.getLabel();
        this.description = addChannel.getDescription();
        this.tags = addChannel.getTags();
        this.preserveConfig = false;
        logger.debug("Applying {} to thing '{}'", this, thing.getUID());
            doChannel(thing, thingBuilder, new ChannelUID(thing.getUID(), channelId));
            groupIds.forEach(groupId -> doChannel(thing, thingBuilder,
    private void doChannel(Thing thing, ThingBuilder thingBuilder, ChannelUID affectedChannelUid) {
        if (removeOldChannel) {
            thingBuilder.withoutChannel(affectedChannelUid);
        ChannelType channelType = channelTypeRegistry.getChannelType(channelTypeUid);
            logger.error("Failed to create channel '{}' because channel type '{}' could not be found.",
                    affectedChannelUid, channelTypeUid);
        ChannelBuilder channelBuilder = ThingFactoryHelper.createChannelBuilder(affectedChannelUid, channelType,
        if (preserveConfig) {
            Channel oldChannel = thing.getChannel(affectedChannelUid);
            if (oldChannel != null) {
                channelBuilder.withConfiguration(oldChannel.getConfiguration());
                channelBuilder.withDefaultTags(oldChannel.getDefaultTags());
                channelBuilder.withProperties(oldChannel.getProperties());
            channelBuilder.withLabel(Objects.requireNonNull(label));
            channelBuilder.withDescription(Objects.requireNonNull(description));
            channelBuilder.withDefaultTags(Set.copyOf(Objects.requireNonNull(tags)));
        thingBuilder.withChannel(channelBuilder.build());
        return "UpdateChannelInstructionImpl{" + "removeOldChannel=" + removeOldChannel + ", thingTypeVersion="
                + thingTypeVersion + ", preserveConfig=" + preserveConfig + ", groupIds=" + groupIds + ", channelId='"
                + channelId + "'" + ", channelTypeUid=" + channelTypeUid + ", label='" + label + "'" + ", description='"
                + description + "'" + ", tags=" + tags + "}";
