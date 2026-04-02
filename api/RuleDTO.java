 * This is a data transfer object that is used to serialize rules.
@Schema(name = "Rule")
public class RuleDTO {
    public List<@NonNull TriggerDTO> triggers;
    public List<@NonNull ConditionDTO> conditions;
    public List<@NonNull ActionDTO> actions;
    public Map<@NonNull String, @NonNull Object> configuration;
    public List<@NonNull ConfigDescriptionParameterDTO> configDescriptions;
    public String templateUID;
    public String templateState;
    public Set<@NonNull String> tags;
