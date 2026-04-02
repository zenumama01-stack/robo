 * The {@link DiscoveryResultDTOMapper} is a utility class to map discovery results into discovery result transfer
 * @author Stefan Bussweiler - Initial contribution
public class DiscoveryResultDTOMapper {
     * Maps discovery result into discovery result data transfer object.
     * @param discoveryResult the discovery result
     * @return the discovery result data transfer object
    public static DiscoveryResultDTO map(DiscoveryResult discoveryResult) {
        ThingUID thingUID = discoveryResult.getThingUID();
        ThingUID bridgeUID = discoveryResult.getBridgeUID();
        return new DiscoveryResultDTO(thingUID.toString(), bridgeUID != null ? bridgeUID.toString() : null,
                discoveryResult.getThingTypeUID().toString(), discoveryResult.getLabel(), discoveryResult.getFlag(),
                discoveryResult.getProperties(), discoveryResult.getRepresentationProperty());
     * Maps discovery result data transfer object into discovery result.
     * @param discoveryResultDTO the discovery result data transfer object
     * @return the discovery result
    public static DiscoveryResult map(DiscoveryResultDTO discoveryResultDTO) {
        final ThingUID thingUID = new ThingUID(discoveryResultDTO.thingUID);
        final String dtoThingTypeUID = discoveryResultDTO.thingTypeUID;
        final ThingTypeUID thingTypeUID = dtoThingTypeUID != null ? new ThingTypeUID(dtoThingTypeUID) : null;
        final String dtoBridgeUID = discoveryResultDTO.bridgeUID;
        final ThingUID bridgeUID = dtoBridgeUID != null ? new ThingUID(dtoBridgeUID) : null;
        return DiscoveryResultBuilder.create(thingUID).withThingType(thingTypeUID).withBridge(bridgeUID)
                .withLabel(discoveryResultDTO.label)
                .withRepresentationProperty(discoveryResultDTO.representationProperty)
                .withProperties(discoveryResultDTO.properties).build();
