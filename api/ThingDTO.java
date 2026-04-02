@Schema(name = "Thing")
public class ThingDTO extends AbstractThingDTO {
    public List<ChannelDTO> channels;
    public ThingDTO() {
    protected ThingDTO(String thingTypeUID, String uid, String label, String bridgeUID, List<ChannelDTO> channels,
        super(thingTypeUID, uid, label, bridgeUID, configuration, properties, location, semanticEquipmentTag);
