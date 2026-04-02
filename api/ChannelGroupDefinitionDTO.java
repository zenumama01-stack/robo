 * This is a data transfer object that is used to serialize channel group definitions.
@Schema(name = "ChannelGroupDefinition")
public class ChannelGroupDefinitionDTO {
    public List<ChannelDefinitionDTO> channels;
    public ChannelGroupDefinitionDTO() {
        this("", null, null, List.of());
    public ChannelGroupDefinitionDTO(String id, @Nullable String label, @Nullable String description,
            List<ChannelDefinitionDTO> channels) {
