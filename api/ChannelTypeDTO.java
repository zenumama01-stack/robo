 * This is a data transfer object that is used with to serialize channel types.
@Schema(name = "ChannelType")
public class ChannelTypeDTO {
    @Schema(requiredMode = Schema.RequiredMode.REQUIRED, allowableValues = { "STATE", "TRIGGER" })
    public ChannelTypeDTO() {
        this("", "", null, null, null, null, ChannelKind.STATE, List.of(), List.of(), null, Set.of(), false, null);
    public ChannelTypeDTO(String uid, String label, @Nullable String description, @Nullable String category,
            @Nullable String itemType, @Nullable String unitHint, ChannelKind kind,
            List<ConfigDescriptionParameterDTO> parameters, List<ConfigDescriptionParameterGroupDTO> parameterGroups,
            @Nullable StateDescription stateDescription, Set<String> tags, boolean advanced,
            @Nullable CommandDescription commandDescription) {
        this.unitHint = unitHint;
