 * The {@link ChannelDTOMapper} is a utility class to map channels into channel data transfer objects (DTOs).
 * @author Kai Kreuzer - added DTO to channel mapping
public class ChannelDTOMapper {
     * Maps channel into channel DTO object.
     * @return the channel DTO object
    public static ChannelDTO map(Channel channel) {
        String channelTypeUIDValue = channelTypeUID != null ? channelTypeUID.getAsString() : null;
        return new ChannelDTO(channel.getUID(), channelTypeUIDValue, channel.getAcceptedItemType(), channel.getKind(),
                channel.getLabel(), channel.getDescription(), channel.getProperties(), channel.getConfiguration(),
                channel.getDefaultTags(), channel.getAutoUpdatePolicy());
     * Maps channel DTO into channel object.
     * @param channelDTO the channel DTO
     * @return the channel object
    public static Channel map(ChannelDTO channelDTO) {
        ChannelUID channelUID = new ChannelUID(channelDTO.uid);
        ChannelTypeUID channelTypeUID = channelDTO.channelTypeUID != null
                ? new ChannelTypeUID(channelDTO.channelTypeUID)
        return ChannelBuilder.create(channelUID, channelDTO.itemType)
                .withConfiguration(new Configuration(channelDTO.configuration)).withLabel(channelDTO.label)
                .withDescription(channelDTO.description).withProperties(channelDTO.properties).withType(channelTypeUID)
                .withDefaultTags(channelDTO.defaultTags).withKind(ChannelKind.parse(channelDTO.kind))
                .withAutoUpdatePolicy(AutoUpdatePolicy.parse(channelDTO.autoUpdatePolicy)).build();
