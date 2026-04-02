 * This is a data transfer object that is used with to serialize thing types.
@Schema(name = "ThingType")
public class ThingTypeDTO extends StrippedThingTypeDTO {
    public List<ChannelGroupDefinitionDTO> channelGroups;
    public List<ConfigDescriptionParameterDTO> configParameters;
    public List<String> extensibleChannelTypeIds;
    public ThingTypeDTO() {
        this("", "", null, null, true, List.of(), List.of(), List.of(), List.of(), Map.of(), false, List.of(),
                List.of(), null);
    public ThingTypeDTO(String uid, String label, @Nullable String description, @Nullable String category,
            boolean listed, List<ConfigDescriptionParameterDTO> configParameters, List<ChannelDefinitionDTO> channels,
            List<ChannelGroupDefinitionDTO> channelGroups, List<String> supportedBridgeTypeUIDs,
            Map<String, String> properties, boolean bridge, List<ConfigDescriptionParameterGroupDTO> parameterGroups,
            List<String> extensibleChannelTypeIds, @Nullable String semanticEquipmentTag) {
        this.configParameters = configParameters;
        this.channelGroups = channelGroups;
        this.extensibleChannelTypeIds = extensibleChannelTypeIds;
