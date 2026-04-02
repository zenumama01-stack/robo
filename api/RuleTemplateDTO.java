 * This is a data transfer object that is used to serialize the rule templates.
public class RuleTemplateDTO {
    public List<TriggerDTO> triggers;
    public List<ConditionDTO> conditions;
    public List<ActionDTO> actions;
