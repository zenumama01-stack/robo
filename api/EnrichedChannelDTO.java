package org.openhab.core.io.rest.core.thing;
 * This is a data transfer object that is used to serialize channels with dynamic data like linked items.
@Schema(name = "EnrichedChannel")
public class EnrichedChannelDTO extends ChannelDTO {
    public final Set<String> linkedItems;
    public EnrichedChannelDTO(ChannelDTO channelDTO, Set<String> linkedItems) {
        this.uid = channelDTO.uid;
        this.id = channelDTO.id;
        this.channelTypeUID = channelDTO.channelTypeUID;
        this.itemType = channelDTO.itemType;
        this.kind = channelDTO.kind;
        this.label = channelDTO.label;
        this.description = channelDTO.description;
        this.properties = channelDTO.properties;
        this.configuration = channelDTO.configuration;
        this.defaultTags = channelDTO.defaultTags;
        this.linkedItems = linkedItems != null ? new HashSet<>(linkedItems) : new HashSet<>();
