 * This is a data transfer object that is used to serialize options of a
 * parameter group.
@Schema(name = "ConfigDescriptionParameterGroup")
public class ConfigDescriptionParameterGroupDTO {
    public ConfigDescriptionParameterGroupDTO() {
    public ConfigDescriptionParameterGroupDTO(String name, String context, Boolean advanced, String label,
            String description) {
