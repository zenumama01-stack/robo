 * The {@link YamlRuleTemplateDTO} is a data transfer object used to serialize a rule template in a YAML configuration
 * file.
@YamlElementName("ruleTemplates")
public class YamlRuleTemplateDTO
        implements ModularDTO<YamlRuleTemplateDTO, ObjectMapper, JsonNode>, YamlElement, Cloneable {
    public YamlRuleTemplateDTO() {
     * Creates a new instance based on the specified {@link RuleTemplate}.
     * @param template the {@link RuleTemplate}.
    public YamlRuleTemplateDTO(@NonNull RuleTemplate template) {
        this.uid = template.getUID();
        this.label = template.getLabel();
        this.tags = template.getTags();
        this.description = template.getDescription();
        this.visibility = template.getVisibility();
        List<@NonNull ConfigDescriptionParameter> configDescriptions = template.getConfigurationDescriptions();
        List<@NonNull Action> actions = template.getActions();
        List<@NonNull Condition> conditions = template.getConditions();
        List<@NonNull Trigger> triggers = template.getTriggers();
    public @NonNull YamlRuleTemplateDTO toDto(@NonNull JsonNode node, @NonNull ObjectMapper mapper)
        YamlPartialRuleTemplateDTO partial;
        YamlRuleTemplateDTO result = new YamlRuleTemplateDTO();
            partial = mapper.treeToValue(node, YamlPartialRuleTemplateDTO.class);
    public YamlRuleTemplateDTO cloneWithoutId() {
        YamlRuleTemplateDTO copy;
            copy = (YamlRuleTemplateDTO) super.clone();
            return new YamlRuleTemplateDTO();
            addToList(errors, "invalid rule template: uid is missing while mandatory");
                        "invalid rule template \"%s\": segment \"%s\" in the uid doesn't match the expected syntax %s"
        // Check that label is present
            addToList(errors, "invalid rule template \"%s\": label is missing while mandatory".formatted(uid));
        // Check that the rule template has at least one module
        if ((triggers == null || triggers.isEmpty()) && (conditions == null || conditions.isEmpty())
                && (actions == null || actions.isEmpty())) {
            addToList(errors, "invalid rule template \"%s\": the template is empty".formatted(uid));
                        "invalid rule template \"%s\": Illegal %s ID \"%s\" - IDs must be unique across all modules in the rule template"
        return Objects.hash(actions, conditions, configDescriptions, description, label, tags, triggers, uid,
                visibility);
        if (!(obj instanceof YamlRuleTemplateDTO)) {
        YamlRuleTemplateDTO other = (YamlRuleTemplateDTO) obj;
                && Objects.equals(configDescriptions, other.configDescriptions)
                && Objects.equals(tags, other.tags) && Objects.equals(triggers, other.triggers)
     * A data transfer object for partial deserialization of a template.
    protected static class YamlPartialRuleTemplateDTO {
