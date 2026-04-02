package org.openhab.core.thing.internal.update;
import org.openhab.core.thing.internal.update.dto.XmlRemoveChannel;
 * The {@link RemoveChannelInstructionImpl} implements a {@link ThingUpdateInstruction} that removes a channel from a
 * thing.
public class RemoveChannelInstructionImpl implements ThingUpdateInstruction {
    private final int thingTypeVersion;
    private final List<String> groupIds;
    private final String channelId;
    RemoveChannelInstructionImpl(int thingTypeVersion, XmlRemoveChannel removeChannel) {
        this.thingTypeVersion = thingTypeVersion;
        String rawGroupIds = removeChannel.getGroupIds();
        this.groupIds = rawGroupIds != null ? Arrays.asList(rawGroupIds.split(",")) : List.of();
        this.channelId = removeChannel.getId();
    public int getThingTypeVersion() {
        return thingTypeVersion;
    public void perform(Thing thing, ThingBuilder thingBuilder) {
        if (groupIds.isEmpty()) {
            thingBuilder.withoutChannel(new ChannelUID(thing.getUID(), channelId));
            groupIds.forEach(groupId -> thingBuilder.withoutChannel(
                    new ChannelUID(thing.getUID(), groupId + ChannelUID.CHANNEL_GROUP_SEPARATOR + channelId)));
