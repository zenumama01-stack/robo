 * The {@link ThingStorageEntity} is an entity for Thing storage
public class ThingStorageEntity extends ThingDTO {
    ThingStorageEntity() {
        // do not remove, needed by GSON for deserialization
    public ThingStorageEntity(ThingDTO thingDTO, boolean isBridge) {
        super(thingDTO.thingTypeUID, thingDTO.UID, thingDTO.label, thingDTO.bridgeUID, thingDTO.channels,
                thingDTO.configuration, thingDTO.properties, thingDTO.location, thingDTO.semanticEquipmentTag);
        this.isBridge = isBridge;
