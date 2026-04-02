package org.openhab.core.model.yaml.internal.things.fileconverter;
import org.openhab.core.model.yaml.internal.things.YamlChannelDTO;
import org.openhab.core.model.yaml.internal.things.YamlThingProvider;
 * {@link YamlThingConverter} is the YAML converter for {@link Thing} objects.
public class YamlThingConverter extends AbstractThingSerializer implements ThingParser {
    private final YamlThingProvider thingProvider;
    public YamlThingConverter(final @Reference YamlModelRepository modelRepository,
            final @Reference YamlThingProvider thingProvider,
            elements.add(buildThingDTO(thing, hideDefaultChannels, hideDefaultParameters));
    private YamlThingDTO buildThingDTO(Thing thing, boolean hideDefaultChannels, boolean hideDefaultParameters) {
        YamlThingDTO dto = new YamlThingDTO();
        dto.uid = thing.getUID().getAsString();
        dto.isBridge = thing instanceof Bridge ? true : null;
        dto.bridge = bridgeUID == null ? null : bridgeUID.getAsString();
        dto.label = thingType != null && thingType.getLabel().equals(thing.getLabel()) ? null : thing.getLabel();
        dto.location = thing.getLocation();
        Map<String, Object> config = new LinkedHashMap<>();
        getConfigurationParameters(thing, hideDefaultParameters).forEach(param -> {
            if (param.value() instanceof List<?> list) {
                    config.put(param.name(), param.value());
        dto.config = config.isEmpty() ? null : config;
        Map<String, YamlChannelDTO> channels = new LinkedHashMap<>();
        List<Channel> channelList = hideDefaultChannels ? getNonDefaultChannels(thing) : thing.getChannels();
        channelList.forEach(channel -> {
            channels.put(channel.getUID().getId(), buildChannelDTO(channel, hideDefaultParameters));
        dto.channels = channels.isEmpty() ? null : channels;
    private YamlChannelDTO buildChannelDTO(Channel channel, boolean hideDefaultParameters) {
        YamlChannelDTO dto = new YamlChannelDTO();
            dto.type = channelTypeUID.getId();
            dto.label = channelType != null && channelType.getLabel().equals(channel.getLabel()) ? null
                    : channel.getLabel();
            String descr = channelType != null ? channelType.getDescription() : null;
            dto.description = descr != null && descr.equals(channel.getDescription()) ? null : channel.getDescription();
            dto.kind = channel.getKind() == ChannelKind.STATE ? null : "trigger";
            String itemType = channel.getAcceptedItemType();
            dto.itemType = itemType != null ? ItemUtil.getMainItemType(itemType) : null;
            dto.itemDimension = ItemUtil.getItemTypeExtension(itemType);
            dto.label = channel.getLabel();
            dto.description = channel.getDescription();
        getConfigurationParameters(channel, hideDefaultParameters).forEach(param -> {
        return itemChannelLinkProvider.getAllFromModel(modelName);
