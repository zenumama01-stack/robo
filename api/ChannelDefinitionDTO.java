 * This is a data transfer object that is used to serialize channel definitions.
 * @author Chris Jackson - Added properties
@Schema(name = "ChannelDefinition")
public class ChannelDefinitionDTO {
    public @Nullable StateDescription stateDescription;
    public String typeUID;
    public ChannelDefinitionDTO() {
        this("", "", "", null, Set.of(), null, null, false, Map.of());
    public ChannelDefinitionDTO(String id, String typeUID, String label, @Nullable String description, Set<String> tags,
            @Nullable String category, @Nullable StateDescription stateDescription, boolean advanced,
            Map<String, String> properties) {
