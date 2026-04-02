 * The {@link EnrichedThingDTOMapper} is a utility class to map things into enriched thing data transfer objects
public class EnrichedThingDTOMapper extends ThingDTOMapper {
     * Maps thing into enriched thing data transfer object.
     * @param thing the thing
     * @param thingStatusInfo the thing status information to be used for the enriched object
     * @param firmwareStatus the firmwareStatus to be used for the enriched object
     * @param linkedItemsMap the map of linked items to be injected into the enriched object
     * @return the enriched thing DTO object
    public static EnrichedThingDTO map(Thing thing, ThingStatusInfo thingStatusInfo,
            @Nullable FirmwareStatusDTO firmwareStatus, @Nullable Map<String, Set<String>> linkedItemsMap,
        ThingDTO thingDTO = ThingDTOMapper.map(thing);
        List<EnrichedChannelDTO> channels = new ArrayList<>();
        for (ChannelDTO channel : thingDTO.channels) {
            Set<String> linkedItems = linkedItemsMap != null ? linkedItemsMap.get(channel.id) : Set.of();
            channels.add(new EnrichedChannelDTO(channel, linkedItems));
        return new EnrichedThingDTO(thingDTO, channels, thingStatusInfo, firmwareStatus, editable);
