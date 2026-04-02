 * The {@link ThingDTOMapper} is a utility class to map things into data transfer objects (DTO).
 * @author Kai Kreuzer - Added DTO to Thing mapping
public class ThingDTOMapper {
     * Maps thing into thing data transfer object (DTO).
     * @return the thing DTO object
    public static ThingDTO map(Thing thing) {
        List<ChannelDTO> channelDTOs = new ArrayList<>();
            ChannelDTO channelDTO = ChannelDTOMapper.map(channel);
            channelDTOs.add(channelDTO);
        String thingTypeUID = thing.getThingTypeUID().getAsString();
        String thingUID = thing.getUID().toString();
        final ThingUID bridgeUID = thing.getBridgeUID();
        return new ThingDTO(thingTypeUID, thingUID, thing.getLabel(), bridgeUID != null ? bridgeUID.toString() : null,
                channelDTOs, toMap(thing.getConfiguration()), thing.getProperties(), thing.getLocation(),
                thing.getSemanticEquipmentTag());
     * Maps thing DTO into thing
     * @param thingDTO the thingDTO
     * @param isBridge flag if the thing DTO identifies a bridge
     * @return the corresponding thing
    public static Thing map(ThingDTO thingDTO, boolean isBridge) {
        ThingUID thingUID = new ThingUID(thingDTO.UID);
        ThingTypeUID thingTypeUID = thingDTO.thingTypeUID == null ? new ThingTypeUID("")
                : new ThingTypeUID(thingDTO.thingTypeUID);
        final Thing thing;
        if (isBridge) {
            thing = BridgeBuilder.create(thingTypeUID, thingUID).build();
            thing = ThingBuilder.create(thingTypeUID, thingUID).build();
        return ThingHelper.merge(thing, thingDTO);
    private static Map<String, Object> toMap(Configuration configuration) {
