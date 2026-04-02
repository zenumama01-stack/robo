@Schema(name = "ModuleType")
public class ModuleTypeDTO {
    public String uid;
    public Visibility visibility;
    public Set<String> tags;
    public List<ConfigDescriptionParameterDTO> configDescriptions;
