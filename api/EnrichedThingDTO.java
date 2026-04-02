import org.openhab.core.thing.dto.AbstractThingDTO;
 * This is a data transfer object that is used to serialize things with dynamic data like the status.
 * @author Kai Kreuzer - Removed links and items
 * @author Chris Jackson - Added 'editable' flag
 * @author Wouter Born - Let (Enriched)ThingDTO extend AbstractThingDTO so both can define their own "channels" type
@Schema(name = "EnrichedThing")
public class EnrichedThingDTO extends AbstractThingDTO {
    public List<EnrichedChannelDTO> channels;
    public ThingStatusInfo statusInfo;
    public @Nullable FirmwareStatusDTO firmwareStatus;
     * Creates an enriched thing data transfer object.
     * @param thingDTO the base {@link ThingDTO}
     * @param channels the list of {@link EnrichedChannelDTO} for this thing
     * @param statusInfo {@link ThingStatusInfo} for this thing
     * @param firmwareStatus {@link FirmwareStatusDTO} for this thing
     * @param editable true if this thing can be edited
    EnrichedThingDTO(ThingDTO thingDTO, List<EnrichedChannelDTO> channels, ThingStatusInfo statusInfo,
            @Nullable FirmwareStatusDTO firmwareStatus, boolean editable) {
        super(thingDTO.thingTypeUID, thingDTO.UID, thingDTO.label, thingDTO.bridgeUID, thingDTO.configuration,
                thingDTO.properties, thingDTO.location, thingDTO.semanticEquipmentTag);
        this.firmwareStatus = firmwareStatus;
